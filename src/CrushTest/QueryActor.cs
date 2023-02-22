using Akka;
using Akka.Actor;
using Akka.Event;
using Akka.Persistence.Query;
using Akka.Persistence.Query.Sql;
using Akka.Streams;
using Akka.Streams.Dsl;

namespace CrushTest;

public class QueryActor : ReceiveActor
{
    private readonly ILoggingAdapter _log = Context.GetLogger();
    private readonly IActorRef _recoveryTracker;
    private readonly string _targetPersistentId;
    private int _total = 0;

    public QueryActor(string targetPersistentId, IActorRef recoveryTracker)
    {
        _targetPersistentId = targetPersistentId;
        _recoveryTracker = recoveryTracker;

        Receive<EventEnvelope>(e =>
        {
            _log.Info("RECOVERY: {0}", e.Event);
            _total++;
            if (_total == 10)
            {
                _log.Info("Completed recovery for entity {0}", _targetPersistentId);
                _recoveryTracker.Tell(RecoveryTracker.RecoveryComplete.Instance);
            }
        });
    }

    protected override void PreStart()
    {
        var query = Context.System.ReadJournalFor<SqlReadJournal>(SqlReadJournal.Identifier);
        query.EventsByPersistenceId(_targetPersistentId, 0, long.MaxValue)
            .To(Sink.ActorRef<EventEnvelope>(Self, Done.Instance, e => new Status.Failure(e)))
            .Run(Context.Materializer());
    }
}