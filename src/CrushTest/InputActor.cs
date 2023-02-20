using Akka.Event;
using Akka.Persistence;

namespace CrushTest;

public class InputActor : ReceivePersistentActor
{
    private readonly ILoggingAdapter _log = Context.GetLogger();
    
    public InputActor(string persistenceId)
    {
        PersistenceId = persistenceId;

        Command<int>(str =>
        {
            Persist(str, s =>
            {
                _log.Info("Received {0}", s);
            });
        });
    }

    public override string PersistenceId { get; }
}