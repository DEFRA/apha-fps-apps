using Apha.BatchJobs.Pact.Api.Models;

namespace Apha.BatchJobs.Pact.Api.Services;

public sealed class EventBridgeTriggerDispatcher : ITriggerDispatcher
{
    private readonly IEventPublisher _eventPublisher;

    public EventBridgeTriggerDispatcher(IEventPublisher eventPublisher)
    {
        _eventPublisher = eventPublisher;
    }

    public Task<string> DispatchAsync(BatchTriggerEventDetail detail, CancellationToken cancellationToken = default)
        => _eventPublisher.PublishAsync(detail, cancellationToken);
}
