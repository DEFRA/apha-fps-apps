using Apha.BatchJobs.Infrastructure.Data;

namespace Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;

internal sealed class LogRecreateSummariesStep : RecreateSummariesExecutionStepBase
{
    private readonly int _month;
    private readonly string _triggeredBy;

    public LogRecreateSummariesStep(int month, string triggeredBy)
    {
        _month = month;
        _triggeredBy = triggeredBy;
    }

    public override string StepName => "LogRecreateSummaries";

    protected override async Task<int> ExecuteCoreAsync(RecreateSummariesExecutionContext context, CancellationToken cancellationToken)
    {
        await context.DbContext.RsRecreateSummariesLog.AddAsync(new RsRecreateSummariesLogTable
        {
            UserId = _triggeredBy,
            Period = _month,
            DateDone = DateTime.UtcNow
        }, cancellationToken);

        return await context.DbContext.SaveChangesAsync(cancellationToken);
    }
}
