using Apha.BatchJobs.Pact.Api.Models;

namespace Apha.BatchJobs.Pact.Api.Services;

public interface IEventPublisher
{
    Task<string> PublishAsync(BatchTriggerEventDetail detail, CancellationToken cancellationToken = default);
}
