using Akka;
using Akka.Actor;
using Akka.Event;
using Akka.Persistence.Query;
using Akka.Persistence.Query.Sql;
using Akka.Streams;
using Akka.Streams.Dsl;
using Akka.Util;

namespace CrushTest;

public class QueryActor : ReceiveActor
{
    private readonly ILoggingAdapter _log = Context.GetLogger();
    private readonly IActorRef _recoveryTracker;
    private readonly string _targetPersistentId;
    private readonly ActorMaterializer _materializer = Context.Materializer();
    private int _total = 0;
    private long _highSeqNo = 0;
    
    public QueryActor(string targetPersistentId, IActorRef recoveryTracker)
    {
        _targetPersistentId = targetPersistentId;
        _recoveryTracker = recoveryTracker;

        Receive<EventEnvelope>(e =>
        {
            _highSeqNo = e.SequenceNr;
            _total++;
            if (_total == 100)
            {
                _log.Info("Completed v1.5 recovery for entity {0}", _targetPersistentId);
                _recoveryTracker.Tell(RecoveryTracker.RecoveryComplete.Instance);
            }
        });

        ReceiveAsync<Status.Failure>(async failure =>
        {
            var retryTime = TimeSpan.FromSeconds(ThreadLocalRandom.Current.Next(1, 15));
            _log.Error(failure.Cause, "Query to recover entity {0} failed. Retrying in {1}", _targetPersistentId, retryTime);
            await Task.Delay(retryTime);
            StartQuery(_highSeqNo);
        });
    }

    protected override void PreStart()
    {
        StartQuery(0);
    }

    private void StartQuery(long startSeqNo)
    {
        var query = Context.System.ReadJournalFor<SqlReadJournal>(SqlReadJournal.Identifier);
        query.EventsByPersistenceId(_targetPersistentId, startSeqNo, long.MaxValue)
            .To(Sink.ActorRef<EventEnvelope>(Self, Done.Instance, exception => new Status.Failure(exception)))
            .Run(_materializer);
    }
}