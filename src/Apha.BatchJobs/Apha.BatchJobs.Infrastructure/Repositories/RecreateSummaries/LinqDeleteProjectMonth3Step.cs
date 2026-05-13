using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;

internal sealed class LinqDeleteProjectMonth3Step : LinqRecreateSummariesExecutionStepBase
{
    public override string StepName => "DeleteProjectMonth3";

    protected override Task<int> ExecuteCoreAsync(RecreateSummariesExecutionContext context, CancellationToken cancellationToken)
        => context.DbContext.RsProjectMonth3.ExecuteDeleteAsync(cancellationToken);
}
