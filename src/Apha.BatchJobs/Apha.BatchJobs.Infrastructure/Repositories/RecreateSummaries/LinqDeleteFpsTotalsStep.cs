using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;

internal sealed class LinqDeleteFpsTotalsStep : LinqRecreateSummariesExecutionStepBase
{
    public override string StepName => "DeleteFpsTotals";

    protected override Task<int> ExecuteCoreAsync(RecreateSummariesExecutionContext context, CancellationToken cancellationToken)
        => context.DbContext.RsFpsYearTotals.ExecuteDeleteAsync(cancellationToken);
}
