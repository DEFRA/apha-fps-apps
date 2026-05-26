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
            UserId = NormalizeTriggeredBy(_triggeredBy),
            Period = _month,
            DateDone = DateTime.UtcNow
        }, cancellationToken);

        return await context.DbContext.SaveChangesAsync(cancellationToken);
    }

    private static string NormalizeTriggeredBy(string triggeredBy)
    {
        var trimmed = triggeredBy?.Trim() ?? string.Empty;
        var slashIndex = trimmed.IndexOf('\\');

        if (slashIndex >= 0 && slashIndex < trimmed.Length - 1)
        {
            return trimmed[(slashIndex + 1)..];
        }

        return trimmed;
    }
}
