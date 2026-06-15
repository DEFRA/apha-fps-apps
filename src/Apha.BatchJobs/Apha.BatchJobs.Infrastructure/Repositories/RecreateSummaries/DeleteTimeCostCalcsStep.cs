using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;

internal sealed class DeleteTimeCostCalcsStep : RecreateSummariesExecutionStepBase
{
    public override string StepName => "DeleteTimeCostCalcs";

    protected override Task<int> ExecuteCoreAsync(RecreateSummariesExecutionContext context, CancellationToken cancellationToken)
        => context.DbContext.RsTimeCostCalcs.ExecuteDeleteAsync(cancellationToken);
}
