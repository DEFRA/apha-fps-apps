using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;

internal sealed class DeleteProjectMonth2Step : RecreateSummariesExecutionStepBase
{
    public override string StepName => "DeleteProjectMonth2";

    protected override Task<int> ExecuteCoreAsync(RecreateSummariesExecutionContext context, CancellationToken cancellationToken)
        => context.DbContext.RsProjectMonth2.ExecuteDeleteAsync(cancellationToken);
}
