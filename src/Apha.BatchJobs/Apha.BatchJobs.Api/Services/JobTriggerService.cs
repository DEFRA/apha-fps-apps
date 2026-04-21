using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Domain.Enums;

namespace Apha.BatchJobs.Api.Services;

/// <summary>
/// Temporary in-process trigger implementation.
/// In production FPS, this start call will be replaced by container/task orchestration.
/// </summary>
public sealed class JobTriggerService : IJobTriggerService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<JobTriggerService> _logger;

    public JobTriggerService(IServiceScopeFactory scopeFactory, ILogger<JobTriggerService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public Task<TriggerResult> TriggerAsync(string jobName, CancellationToken cancellationToken = default)
    {
        var operationId = Guid.NewGuid().ToString("N");
        var acceptedAtUtc = DateTime.UtcNow;

        _ = Task.Run(async () =>
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var orchestrator = scope.ServiceProvider.GetRequiredService<IJobOrchestrator>();

                _logger.LogInformation(
                    "Accepted trigger started execution | OperationId={OperationId} | JobName={JobName}",
                    operationId,
                    jobName);

                var result = await orchestrator.RunAsync(jobName, RunMode.Manual, CancellationToken.None);

                _logger.LogInformation(
                    "Accepted trigger finished | OperationId={OperationId} | JobName={JobName} | Status={Status} | RunId={RunId}",
                    operationId,
                    jobName,
                    result.Status,
                    result.RunId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Accepted trigger failed to execute | OperationId={OperationId} | JobName={JobName}",
                    operationId,
                    jobName);
            }
        }, CancellationToken.None);

        return Task.FromResult(new TriggerResult(operationId, acceptedAtUtc));
    }
}
