using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging;

using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Execution;
namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Steps;

/// <summary>
/// Validates that Year End year-control metadata exists and contains the current year row, and that
/// every year-scoped table in <see cref="YearEndTableRuleMatrix"/> has a routing destination for the
/// target year.
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
    /// Year End performs no DDL. <c>fpsyear</c> is the authoritative business-year discriminator —
    /// the physical partition a row lands in is a storage implementation detail, not a business
    /// concept Year End enforces. Every year-scoped table — the 38 Table 23 business participants,
    /// the 3 year-scoped configuration dependencies, and the 21
    /// <see cref="YearEndTableRole.YearScopedTargetMustBeEmpty"/> tables (62 total) — must already
    /// have a routing destination for the target year before any business-data mutation begins: an
    /// explicit <c>FOR VALUES IN (targetYear)</c> partition, or an attached <c>DEFAULT</c>
    /// partition. Either is a legitimate, DDL-free destination; this only validates routability,
    /// never creates partitions.
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
