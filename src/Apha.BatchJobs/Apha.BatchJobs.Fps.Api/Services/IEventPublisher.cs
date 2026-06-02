using Apha.BatchJobs.Fps.Api.Models;

namespace Apha.BatchJobs.Fps.Api.Services;

public interface IEventPublisher
{
    Task<string> PublishAsync(BatchTriggerEventDetail detail, CancellationToken cancellationToken = default);
}
