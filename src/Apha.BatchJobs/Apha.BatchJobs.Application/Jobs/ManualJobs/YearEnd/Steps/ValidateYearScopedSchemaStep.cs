using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging;

using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Execution;
namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Steps;

/// <summary>
/// Validates that fps.tblyearmaster has the current year, and every year-scoped table in the matrix
/// has a routing destination for the target year.
/// </summary>
public sealed class ValidateYearScopedSchemaStep : IYearEndDataSetupStep
{
    private readonly IYearEndDataSetupRepository _repository;
    private readonly ILogger<ValidateYearScopedSchemaStep> _logger;

    public ValidateYearScopedSchemaStep(
        IYearEndDataSetupRepository repository,
        ILogger<ValidateYearScopedSchemaStep> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "ValidateYearScopedSchemaStep";

    public async Task ExecuteAsync(YearEndExecutionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.CurrentFpsYear.HasValue || !context.TargetFpsYear.HasValue)
        {
            throw new InvalidOperationException("Year End context must include currentFpsYear and targetFpsYear before schema validation.");
        }

        if (!await _repository.TableExistsAsync("fps", "tblyearmaster", cancellationToken))
        {
            throw new InvalidOperationException("Required table fps.tblyearmaster was not found. Year End cannot continue.");
        }

        var requiredColumns = new[] { "fpsyear", "fpsyearcode", "yearstatus", "active" };
        foreach (var columnName in requiredColumns)
        {
            if (!await _repository.ColumnExistsAsync("fps", "tblyearmaster", columnName, cancellationToken))
            {
                throw new InvalidOperationException($"Required column fps.tblyearmaster.{columnName} was not found. Year End cannot continue.");
            }
        }

        var currentYearExists = await _repository.YearRowExistsAsync(
            context.CurrentFpsYear.Value,
            cancellationToken);

        if (!currentYearExists)
        {
            throw new InvalidOperationException(
                $"Current year {context.CurrentFpsYear.Value} does not exist in fps.tblyearmaster. Year End cannot continue.");
        }

        await ValidateTargetYearPartitionsAsync(context.TargetFpsYear.Value, cancellationToken);

        _logger.LogInformation(
            "YearEnd schema validation succeeded | CorrelationId={CorrelationId} | CurrentFpsYear={CurrentFpsYear} | TargetFpsYear={TargetFpsYear}",
            context.CorrelationId,
            context.CurrentFpsYear,
            context.TargetFpsYear);
    }

    /// <summary>
    /// Checks every year-scoped table has a routing destination for the target year — an explicit
    /// partition or an attached DEFAULT partition. Read-only: Year End never creates partitions
    /// itself, that's a DB/DBA prerequisite.
    /// </summary>
    private async Task ValidateTargetYearPartitionsAsync(int targetYear, CancellationToken cancellationToken)
    {
        var notPartitioned = new List<string>();
        var unroutable = new List<string>();

        foreach (var entry in YearEndTableRuleMatrix.Entries)
        {
            if (entry.Role is not (YearEndTableRole.YearScopedBusinessParticipant
                or YearEndTableRole.YearScopedConfigurationDependency
                or YearEndTableRole.YearScopedTargetMustBeEmpty))
            {
                continue;
            }

            cancellationToken.ThrowIfCancellationRequested();

            var qualifiedName = $"{entry.Schema}.{entry.TableName}";

            if (!await _repository.IsPartitionedTableAsync(entry.Schema, entry.TableName, cancellationToken))
            {
                notPartitioned.Add(qualifiedName);
                continue;
            }

            var hasExplicitPartition = await _repository.IsPartitionAttachedForYearAsync(entry.Schema, entry.TableName, targetYear, cancellationToken);
            var hasDefaultPartition = !hasExplicitPartition
                && await _repository.IsDefaultPartitionAttachedAsync(entry.Schema, entry.TableName, cancellationToken);

            if (!hasExplicitPartition && !hasDefaultPartition)
            {
                unroutable.Add(qualifiedName);
            }
        }

        if (notPartitioned.Count == 0 && unroutable.Count == 0)
        {
            return;
        }

        var sections = new List<string>();
        if (notPartitioned.Count > 0)
        {
            sections.Add($"Not partitioned as expected: {string.Join(", ", notPartitioned)}.");
        }

        if (unroutable.Count > 0)
        {
            sections.Add($"No routing destination (explicit or DEFAULT partition) for target year ({targetYear}): {string.Join(", ", unroutable)}.");
        }

        throw new InvalidOperationException(
            $"Year End target-year partition validation failed. {string.Join(" ", sections)} " +
            "Partition creation is an external DB/DBA prerequisite; Year End performs no DDL.");
    }
}
