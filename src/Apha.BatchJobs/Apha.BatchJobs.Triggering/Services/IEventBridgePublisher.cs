using Apha.BatchJobs.Triggering.Models;

namespace Apha.BatchJobs.Triggering.Services;

public interface IEventBridgePublisher
{
    Task<string> PublishAsync(BatchTriggerEventDetail detail, CancellationToken cancellationToken = default);
}