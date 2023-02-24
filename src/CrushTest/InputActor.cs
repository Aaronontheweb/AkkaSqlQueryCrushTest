using Akka.Actor;
using Akka.Event;
using Akka.Persistence;

namespace CrushTest;

public class InputActor : ReceivePersistentActor
{
    private readonly ILoggingAdapter _log = Context.GetLogger();
    
    private int _count = 0;
    
    public InputActor(string persistenceId)
    {
        PersistenceId = persistenceId;
        
        Recover<int>(i =>
        {
            _count += 1;
        });

        Recover<RecoveryCompleted>(_ =>
        {
            if (_count == 100)
            {
                Context.Stop(Self);
                _log.Info("Recovered all events for {0} - shutting down", PersistenceId);
            }
            else if (_count > 100)
            {
                _log.Info("Discovered excess events [{1}] for {0} - shutting down", PersistenceId, _count);
                DeleteMessages(LastSequenceNr);
            }
            else
            {
                _log.Info("Only recovered {0} events out of 100 for {1} - backfilling", _count, PersistenceId);
                var remainer = Enumerable.Range(0, 100 - _count).Chunk(10);
                foreach(var c in remainer)
                    Self.Tell(c);
            }
                
        });
        
        Command<IEnumerable<int>>(i =>
        {
            var items = i.ToArray();
            _log.Info("Writing {0} items", items.Length);
            
            PersistAll(items, s =>
            {
            });
        });

        Command<string>(str =>
        {
            var items = Enumerable.Range(0, 10).ToArray();
            _log.Info("Writing {0} items", items.Length);
            
            PersistAll(items, s =>
            {
            });
        });
    }

    //public override Recovery Recovery => Recovery.None;

    protected override void PreStart()
    {
       
    }

    public override string PersistenceId { get; }
}