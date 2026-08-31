using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging;

using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Execution;
namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Steps;

/// <summary>
/// Removes target-year tblwgemployee rows (and their tblstaffjob rows) for inactive employees,
/// unless they match the General Staff exemption:
/// <list type="bullet">
/// <item>Inactive: <c>personstatus = 'I'</c> AND <c>enddate IS NULL</c> on the target year's row.</item>
/// <item>Exempt: <c>spnumber LIKE 'G%'</c> AND <c>firstname = 'GENERAL'</c> (both required).</item>
/// <item>Any other <c>personstatus</c> value fails validation before any deletion happens.</item>
/// </list>
/// FPS-only — never touches mabarchive tables.
/// </summary>
public sealed class InactiveEmployeeCleanupStep : IYearEndDataSetupStep
{
    private readonly IYearEndDataSetupRepository _repository;
    private readonly ILogger<InactiveEmployeeCleanupStep> _logger;

    public InactiveEmployeeCleanupStep(
        IYearEndDataSetupRepository repository,
        ILogger<InactiveEmployeeCleanupStep> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "InactiveEmployeeCleanupStep";

    public async Task ExecuteAsync(YearEndExecutionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.TargetFpsYear.HasValue)
        {
            throw new InvalidOperationException("Year End context must include targetFpsYear before inactive employee cleanup.");
        }

        var deleted = await _repository.DeleteInactiveEmployeesForYearEndAsync(context.TargetFpsYear.Value, cancellationToken);

        _logger.LogInformation(
            "YearEnd inactive employee cleanup completed | CorrelationId={CorrelationId} | TargetYear={TargetYear} | RowsDeleted={RowsDeleted}",
            context.CorrelationId,
            context.TargetFpsYear,
            deleted);
    }
}
