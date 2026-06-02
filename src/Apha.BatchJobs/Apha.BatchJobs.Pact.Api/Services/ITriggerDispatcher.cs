using Apha.BatchJobs.Pact.Api.Models;

namespace Apha.BatchJobs.Pact.Api.Services;

public interface ITriggerDispatcher
{
    Task<string> DispatchAsync(BatchTriggerEventDetail detail, CancellationToken cancellationToken = default);
}
