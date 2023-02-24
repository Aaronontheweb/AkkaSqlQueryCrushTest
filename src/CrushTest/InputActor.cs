using Akka.Actor;
using Akka.Event;
using Akka.Persistence;

namespace CrushTest;

public class InputActor : ReceivePersistentActor
{
    private readonly ILoggingAdapter _log = Context.GetLogger();
    
    public InputActor(string persistenceId)
    {
        PersistenceId = persistenceId;

        Command<string>(str =>
        {
            var items = Enumerable.Range(0, 10).ToArray();
            _log.Info("Writing {0} items", items.Length);
            
            PersistAll(items, s =>
            {
            });
        });
    }

    public override Recovery Recovery => Recovery.None;

    protected override void PreStart()
    {
        Self.Tell("write");
    }

    public override string PersistenceId { get; }
}