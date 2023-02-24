// See https://aka.ms/new-console-template for more information

using Akka.Actor;
using Akka.Actor.Dsl;
using Akka.Hosting;
using Akka.Persistence.SqlServer.Hosting;
using CrushTest;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var builder = new HostBuilder()
    .ConfigureAppConfiguration(c => c.AddEnvironmentVariables()
        .AddJsonFile("appsettings.json"))
    .ConfigureServices((context, services) =>
    {
        // maps to environment variable ConnectionStrings__AkkaSqlConnection
        var connectionString = context.Configuration.GetConnectionString("AkkaSqlConnection");
        services.AddAkka("SqlSharding", (configurationBuilder, provider) =>
        {
            configurationBuilder
                .WithSqlServerPersistence(connectionString)
                // dial up pressure and force each query to run 10 times
                .AddHocon(@"akka.persistence.query.journal.sql.max-buffer-size = 10
                akka.persistence.query.journal.sql.refresh-interval = 1s", HoconAddMode.Prepend)
                .WithActors((system, registry) =>
                {
                    var recoveryTracker =
                        system.ActorOf(Props.Create(() => new RecoveryTracker(EntityIds.AllEntityIds.Length)),
                            "recovery-tracker");
                    registry.Register<RecoveryTracker>(recoveryTracker);
                })
                .AddStartup(async (system, registry) =>
                {
                    var i = 0;
                    foreach (var id in EntityIds.AllEntityIds)
                    {
                        var actorRef = system.ActorOf(Props.Create(() => new InputActor(id)), id);
                        if (++i % 100 == 0)
                        {
                            //await Task.Delay(TimeSpan.FromSeconds(10));
                        }
                    }

                    await Task.Delay(TimeSpan.FromSeconds(5));
                });
            // .AddStartup((system, registry) =>
            // {
            //     var recoveryTracker = registry.Get<RecoveryTracker>();
            //     
            //     foreach (var id in EntityIds.AllEntityIds)
            //     {
            //         var actorRef = system.ActorOf(Props.Create(() => new QueryActor(id, recoveryTracker)), $"projector-{id}");
            //     }
            // });
        });
    })
    .Build();

await builder.RunAsync();