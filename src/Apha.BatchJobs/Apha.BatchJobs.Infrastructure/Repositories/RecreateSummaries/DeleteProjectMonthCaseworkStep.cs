using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;

internal sealed class DeleteProjectMonthCaseworkStep : RecreateSummariesExecutionStepBase
{
    public override string StepName => "DeleteProjectMonthCasework";

    protected override Task<int> ExecuteCoreAsync(RecreateSummariesExecutionContext context, CancellationToken cancellationToken)
        => context.DbContext.RsProjectMonthCasework.ExecuteDeleteAsync(cancellationToken);
}
