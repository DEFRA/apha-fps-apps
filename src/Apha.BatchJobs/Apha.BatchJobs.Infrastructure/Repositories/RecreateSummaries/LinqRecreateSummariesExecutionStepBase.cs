using Apha.BatchJobs.Application.Jobs.ScheduledJobs.RecreateSummaries;
using Apha.BatchJobs.Domain.Enums;

namespace Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;

/// <summary>
/// Base class for LINQ-based RecreateSummaries steps.
/// </summary>
internal abstract class LinqRecreateSummariesExecutionStepBase : IRecreateSummariesExecutionStep
{
    public abstract string StepName { get; }

    public async Task<StepResult> ExecuteAsync(RecreateSummariesExecutionContext context, CancellationToken cancellationToken = default)
    {
        var start = DateTime.UtcNow;

        try
        {
            var rowsAffected = await ExecuteCoreAsync(context, cancellationToken);
            return new StepResult(StepName, rowsAffected, start, DateTime.UtcNow, StepStatus.Success);
        }
        catch (Exception ex)
        {
            return new StepResult(StepName, 0, start, DateTime.UtcNow, StepStatus.Failed, ex.Message);
        }
    }

    protected abstract Task<int> ExecuteCoreAsync(RecreateSummariesExecutionContext context, CancellationToken cancellationToken);
}
