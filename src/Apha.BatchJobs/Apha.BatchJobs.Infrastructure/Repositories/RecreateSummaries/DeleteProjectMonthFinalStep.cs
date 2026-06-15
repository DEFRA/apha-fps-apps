using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;

internal sealed class DeleteProjectMonthFinalStep : RecreateSummariesExecutionStepBase
{
    public override string StepName => "DeleteProjectMonthFinal";

    protected override Task<int> ExecuteCoreAsync(RecreateSummariesExecutionContext context, CancellationToken cancellationToken)
        => context.DbContext.RsProjectMonthFinal.ExecuteDeleteAsync(cancellationToken);
}
