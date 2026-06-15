using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;

internal sealed class LogRecreateSummariesStep : RecreateSummariesExecutionStepBase
{
    private readonly int _month;
    private readonly int _year;
    private readonly string _triggeredBy;

    public LogRecreateSummariesStep(int month, int year, string triggeredBy)
    {
        _month = month;
        _year = year;
        _triggeredBy = triggeredBy;
    }

    public override string StepName => "LogRecreateSummaries";

    protected override async Task<int> ExecuteCoreAsync(RecreateSummariesExecutionContext context, CancellationToken cancellationToken)
    {
        var db = context.DbContext;

        var fpsYear = await db.RsTblPeriod
            .AsNoTracking()
            .Where(p => p.EndPeriod == _month && p.FpsYear == _year)
            .Select(p => p.FpsYear)
            .FirstOrDefaultAsync(cancellationToken);

        await db.RsRecreateSummariesLog.AddAsync(new RsRecreateSummariesLogTable
        {
            UserId = NormalizeTriggeredBy(_triggeredBy),
            Period = _month,
            DateDone = DateTime.UtcNow,
            FpsYear = fpsYear
        }, cancellationToken);

        return await db.SaveChangesAsync(cancellationToken);
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
