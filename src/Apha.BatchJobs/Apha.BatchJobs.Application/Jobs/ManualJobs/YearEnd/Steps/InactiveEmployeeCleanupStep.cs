using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging;

using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Execution;
namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Steps;

/// <summary>
/// Removes target-year <c>fps.tblwgemployee</c> rows (and their dependent <c>fps.tblstaffjob</c>
/// rows) for employees who were inactive in the target year and are not the General Staff exemption,
/// per the legacy <c>Annual_WGEmployeeList.sql</c> Year End rule:
///
/// <list type="bullet">
/// <item>Inactive candidate: <c>personstatus = 'I'</c> (case-insensitive) AND
/// <c>enddate IS NULL</c>, evaluated against the target year's own row only.</item>
/// <item>General Staff exemption (retained even if inactive):
/// <c>spnumber LIKE 'G%'</c> (case-sensitive) AND <c>UPPER(firstname) = 'GENERAL'</c>. Both
/// conditions required — this AND reading is not equivalent to OR against live data.</item>
/// <item>Any <c>personstatus</c> value other than <c>A</c>/<c>a</c>/<c>I</c>/<c>i</c> is a
/// data-quality error, surfaced before any deletion — never silently treated as active or
/// inactive.</item>
/// </list>
///
/// FPS-only: no <c>mabarchive</c> table is referenced. MABArchive participation in Year End is
/// gated exclusively through the dedicated MABArchive setup step — this step must not reach into
/// MABArchive outside that gate.
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
