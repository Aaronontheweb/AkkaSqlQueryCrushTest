using Akka.Actor;
using Akka.Event;

namespace CrushTest;

public class RecoveryTracker : ReceiveActor
{
    private readonly int _maxRecoveries;
    private int _recoveries = 0;

    public sealed class RecoveryComplete
    {
        private RecoveryComplete()
        {
        }
        
        public static readonly RecoveryComplete Instance = new();
    }
    
    private readonly ILoggingAdapter _log = Context.GetLogger();
    private DateTime _startTime;

    public RecoveryTracker(int maxRecoveries)
    {
        _maxRecoveries = maxRecoveries;

        Receive<RecoveryComplete>(s =>
        {
            if (++_recoveries == _maxRecoveries)
            {
                _log.Info("Recovery complete - took {0}ms", (DateTime.Now - _startTime).TotalMilliseconds);
                Context.System.Terminate();
            }
        });
    }
    
    protected override void PreStart()
    {
        _startTime = DateTime.Now;
    }
}