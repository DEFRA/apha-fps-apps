using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>
/// Service-layer entry point for Year End Cutover.
/// Closes the current FPS year and activates the target FPS year in a single transaction.
/// </summary>
public sealed class YearEndCutoverService : IYearEndCutoverService
{
    private const string PlannedStatus = "Planned";
    private const string OpenStatus = "Open";
    private const string ClosedStatus = "Closed";

    private readonly IDbContextFactory<BatchJobsDbContext> _dbContextFactory;
    private readonly IJobExecutionRepository _executionRepository;
    private readonly ILogger<YearEndCutoverService> _logger;

    public YearEndCutoverService(
        IDbContextFactory<BatchJobsDbContext> dbContextFactory,
        IJobExecutionRepository executionRepository,
        ILogger<YearEndCutoverService> logger)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _executionRepository = executionRepository ?? throw new ArgumentNullException(nameof(executionRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ExecuteAsync(YearEndExecutionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.CurrentFpsYear.HasValue)
        {
            throw new InvalidOperationException("Year End Cutover requires currentFpsYear in BATCH_JOB_PARAMETERS_JSON.");
        }

        if (!context.TargetFpsYear.HasValue)
        {
            throw new InvalidOperationException("Year End Cutover requires targetFpsYear in BATCH_JOB_PARAMETERS_JSON.");
        }

        var currentYear = context.CurrentFpsYear.Value;
        var targetYear = context.TargetFpsYear.Value;

        if (targetYear <= currentYear)
        {
            throw new InvalidOperationException("targetFpsYear must be greater than currentFpsYear for Year End Cutover.");
        }

        _logger.LogInformation(
            "YearEndCutover service started | CorrelationId={CorrelationId} | TargetFpsYear={TargetFpsYear} | CurrentFpsYear={CurrentFpsYear}",
            context.CorrelationId,
            targetYear,
            currentYear);

        var latestDataSetupExecution = await _executionRepository.GetLastExecutionByFpsYearAsync(
            BatchJobNames.YearEndDataSetup,
            targetYear,
            cancellationToken);

        if (latestDataSetupExecution is null || latestDataSetupExecution.Status != JobStatus.Completed)
        {
            var actualStatus = latestDataSetupExecution?.Status.ToString() ?? "None";
            throw new InvalidOperationException(
                $"Year End Cutover requires the latest {BatchJobNames.YearEndDataSetup} execution for target year {targetYear} " +
                $"to be Completed, but found '{actualStatus}'.");
        }

        await using var dbContext = _dbContextFactory.CreateDbContext();
        await dbContext.Database.OpenConnectionAsync(cancellationToken);

        var executionStrategy = dbContext.Database.CreateExecutionStrategy();

        await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            var connection = dbContext.Database.GetDbConnection();
            var dbTransaction = transaction.GetDbTransaction();

            try
            {
                var targetState = await GetYearStateForUpdateAsync(connection, dbTransaction, targetYear, cancellationToken);
                if (targetState is null)
                {
                    throw new InvalidOperationException(
                        $"Target year {targetYear} does not exist in fps.tblyearmaster. Year End Data Setup must complete before cutover.");
                }

                if (!string.Equals(targetState.YearStatus, PlannedStatus, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        $"Target year {targetYear} is in status '{targetState.YearStatus}', expected '{PlannedStatus}' before cutover.");
                }

                if (!targetState.Active)
                {
                    throw new InvalidOperationException($"Target year {targetYear} is inactive in fps.tblyearmaster.");
                }

                var currentState = await GetYearStateForUpdateAsync(connection, dbTransaction, currentYear, cancellationToken);
                if (currentState is null)
                {
                    throw new InvalidOperationException($"Current year {currentYear} does not exist in fps.tblyearmaster.");
                }

                if (!string.Equals(currentState.YearStatus, OpenStatus, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        $"Current year {currentYear} is in status '{currentState.YearStatus}', expected '{OpenStatus}' before cutover.");
                }

                if (!currentState.Active)
                {
                    throw new InvalidOperationException($"Current year {currentYear} is inactive in fps.tblyearmaster.");
                }

                await UpdateYearStatusAsync(connection, dbTransaction, currentYear, ClosedStatus, cancellationToken);
                await UpdateYearStatusAsync(connection, dbTransaction, targetYear, OpenStatus, cancellationToken);

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(CancellationToken.None);
                throw;
            }
        });

        _logger.LogInformation(
            "YearEndCutover completed | CorrelationId={CorrelationId} | ClosedYear={ClosedYear} | ActivatedYear={ActivatedYear}",
            context.CorrelationId,
            currentYear,
            targetYear);
    }

    private static async Task<YearState?> GetYearStateForUpdateAsync(
        DbConnection connection,
        DbTransaction transaction,
        int fpsYear,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = @"
            SELECT ym.yearstatus, ym.active
            FROM fps.tblyearmaster ym
            WHERE ym.fpsyear = @fpsyear
            FOR UPDATE;";

        AddParameter(command, "fpsyear", fpsYear);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        var yearStatus = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
        var active = !reader.IsDBNull(1) && reader.GetBoolean(1);

        return new YearState(yearStatus, active);
    }

    private static async Task UpdateYearStatusAsync(
        DbConnection connection,
        DbTransaction transaction,
        int fpsYear,
        string newStatus,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = @"
            UPDATE fps.tblyearmaster
            SET yearstatus = @yearstatus
            WHERE fpsyear = @fpsyear;";

        AddParameter(command, "yearstatus", newStatus);
        AddParameter(command, "fpsyear", fpsYear);

        var updated = await command.ExecuteNonQueryAsync(cancellationToken);
        if (updated != 1)
        {
            throw new InvalidOperationException(
                $"Expected to update exactly one row for fpsyear {fpsYear}, but updated {updated}.");
        }
    }

    private static void AddParameter(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }

    private sealed record YearState(string YearStatus, bool Active);
}
