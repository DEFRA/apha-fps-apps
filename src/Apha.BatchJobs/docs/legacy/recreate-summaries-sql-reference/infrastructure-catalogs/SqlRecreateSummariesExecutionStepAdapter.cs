using Apha.BatchJobs.Application.Jobs.ScheduledJobs.RecreateSummaries;

namespace Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;

/// <summary>
/// Adapter that allows existing SQL-backed <see cref="IRecreateSummariesStep"/>
/// implementations to run through <see cref="IRecreateSummariesExecutionStep"/>.
/// </summary>
internal sealed class SqlRecreateSummariesExecutionStepAdapter : IRecreateSummariesExecutionStep
{
    private readonly IRecreateSummariesStep _inner;

    public SqlRecreateSummariesExecutionStepAdapter(IRecreateSummariesStep inner)
        => _inner = inner ?? throw new ArgumentNullException(nameof(inner));

    public string StepName
    {
        get
        {
            var name = _inner.StepName;
            return name.StartsWith("sql", StringComparison.OrdinalIgnoreCase)
                ? name
                : $"sql{name}";
        }
    }

    public Task<StepResult> ExecuteAsync(RecreateSummariesExecutionContext context, CancellationToken cancellationToken = default)
        => _inner.ExecuteAsync(context.Connection, cancellationToken);
}
