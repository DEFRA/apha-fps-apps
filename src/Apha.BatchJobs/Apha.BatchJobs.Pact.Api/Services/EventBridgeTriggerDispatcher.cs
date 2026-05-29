using Apha.BatchJobs.Triggering.Models;
using Apha.BatchJobs.Triggering.Services;

namespace Apha.BatchJobs.Pact.Api.Services;

public sealed class EventBridgeTriggerDispatcher : ITriggerDispatcher
{
    private readonly IEventBridgePublisher _eventBridgePublisher;

    public EventBridgeTriggerDispatcher(IEventBridgePublisher eventBridgePublisher)
    {
        _eventBridgePublisher = eventBridgePublisher;
    }

    public Task<string> DispatchAsync(BatchTriggerEventDetail detail, CancellationToken cancellationToken = default)
        => _eventBridgePublisher.PublishAsync(detail, cancellationToken);
}
