using Akka;
using Akka.Actor;
using Akka.Persistence.Query;
using Akka.Persistence.Query.Sql;
using Akka.Streams;
using Akka.Streams.Dsl;

namespace CrushTest;

public class QueryActor : ReceiveActor
{
    private readonly string _targetPersistentId;
    private int _total = 0;

    public QueryActor(string targetPersistentId)
    {
        _targetPersistentId = targetPersistentId;
        
        Receive<EventEnvelope>(e =>
        {
            _total++;
            if (_total == 10)
            {
                Context.System.Log.Info("Completed recovery for entity {0}", _targetPersistentId);
                Context.Stop(Self);
            }
        });
    }

    protected override void PreStart()
    {
        var query = Context.System.ReadJournalFor<SqlReadJournal>(SqlReadJournal.Identifier);
        query.EventsByPersistenceId(_targetPersistentId, 0, long.MaxValue)
            .To(Sink.ActorRef<EventEnvelope>(Self, Done.Instance))
            .Run(Context.Materializer());
    }
}