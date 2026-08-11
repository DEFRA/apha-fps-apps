using Apha.BatchJobs.Application.Jobs.ManualJobs.BulkRates;
using Apha.BatchJobs.Application.Jobs.ManualJobs.BulkRates.Services;
using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Entities.BulkRates;
using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.Repositories.BulkRates;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using System.Text.Json;

namespace Apha.BatchJobs.Infrastructure.Services.BulkRates;

/// <summary>
/// Infrastructure implementation of <see cref="IBulkAnimalRatesService"/>.
/// Applies Animal annual rate changes (DailyRate, DefraDailyRate, PlanByWeek, Species, SecurityLevel)
/// inside a single database transaction, writes permanent history, and
/// clears request-scoped staging rows on success.
/// Drift detection and revalidation are the responsibility of the FPS approval flow;
/// the worker applies frozen effective values directly.
/// </summary>
public sealed class BulkAnimalRatesService : IBulkAnimalRatesService
{
    private readonly IDbContextFactory<BatchJobsDbContext> _dbContextFactory;
    private readonly IBulkRatesRepository _repository;
    private readonly ILogger<BulkAnimalRatesService> _logger;

    public BulkAnimalRatesService(
        IDbContextFactory<BatchJobsDbContext> dbContextFactory,
        IBulkRatesRepository repository,
        ILogger<BulkAnimalRatesService> logger)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ExecuteAsync(BulkRatesExecutionContext context, CancellationToken cancellationToken = default)
    {
        // ── 1. Load Running, previously approved request ──────────────────
        var entry = await _repository.GetRunningRequestAsync(context.JobExecutionId, cancellationToken)
            ?? throw new InvalidOperationException(
                $"BulkAnimalRatesUpdate: no job_queue row found for JobExecutionId={context.JobExecutionId:D}.");

        ValidatePreconditions(entry, context);

        var jobQueueId = entry.JobQueueId;
        var fpsYear    = entry.FpsYear;
        var appliedAt  = DateTime.UtcNow;
        // ── US-XC-02: Log execution start ─────────────────────────────
        await _repository.WriteJobQueueLogAsync(
            jobQueueId,
            $"Worker execution starting (FPS year {fpsYear}).",
            entry.ApprovedBy, cancellationToken);
        _logger.LogInformation(
            "[BulkRates.ExecutionStarted] JobQueueId={JobQueueId} | JobName={JobName} | FpsYear={FpsYear}",
            jobQueueId, entry.JobName, fpsYear);
        // ── 2. Load staging (including the frozen source_*/effective_*/
        // calculated_action/validation_version columns the release-time freeze wrote) ──────
        var stagingRows = await _repository.GetAnimalStagingRowsAsync(jobQueueId, cancellationToken);

        if (stagingRows.Count == 0)
        {
            throw new InvalidOperationException(
                $"BulkAnimalRatesUpdate: no staging rows found for JobQueueId={jobQueueId:D}.");
        }

        _logger.LogInformation(
            "BulkAnimalRatesUpdate staging loaded | JobQueueId={JobQueueId} | Rows={Rows}",
            jobQueueId, stagingRows.Count);

        // ── 3. Execute all mutations in one transaction ───────────────────
        int updated = 0, unchanged = 0;

        await using var dbContext = _dbContextFactory.CreateDbContext();
        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        var conn = (NpgsqlConnection)dbContext.Database.GetDbConnection();
        await using (var tx = await conn.BeginTransactionAsync(cancellationToken))
        {
            // Lock the targeted live rows for write (deterministic order reduces deadlock risk)
            // and read their current state for history before applying changes.
            var animalTypes = stagingRows
                .Select(r => r.AnimalType)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var liveLookup = await GetAnimalRowsForUpdateAsync(conn, tx, animalTypes, fpsYear, cancellationToken);

            foreach (var row in stagingRows)
            {
                if (row.CalculatedAction == "NoChange")
                {
                    unchanged++;
                    continue;
                }

                liveLookup.TryGetValue(row.AnimalType.ToUpperInvariant(), out var liveBefore);
                var rowsAffected = await UpdateAnimalRowAsync(conn, tx, row, fpsYear, cancellationToken);
                if (rowsAffected == 0)
                    throw new InvalidOperationException(
                        $"BulkAnimalRatesUpdate: UPDATE matched 0 rows for AnimalType='{row.AnimalType}' in JobQueueId={jobQueueId:D}.");
                foreach (var historyRow in BuildHistory(row, liveBefore, entry, appliedAt))
                    await BulkRatesRepository.InsertHistoryRowAsync(conn, tx, historyRow, cancellationToken);
                updated++;
            }

            await tx.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "BulkAnimalRatesUpdate committed | JobQueueId={JobQueueId} | Updated={Updated} | Unchanged={Unchanged}",
                jobQueueId, updated, unchanged);

            // ── US-XC-02: Log commit summary ──────────────────────────────
            await _repository.WriteJobQueueLogAsync(
                jobQueueId,
                $"Rate changes committed: Animal updated={updated}, unchanged={unchanged}.",
                entry.ApprovedBy, cancellationToken);
        }

        // ── 4. Delete staging post-commit ─────────────────────────────────
        // Best-effort cleanup: the rate change is already committed, so a failure here must not
        // fail the job or trigger a whole-job retry. Log and move on.
        try
        {
            await _repository.DeleteAnimalStagingRowsAsync(jobQueueId, cancellationToken);

            _logger.LogInformation(
                "BulkAnimalRatesUpdate staging cleared | JobQueueId={JobQueueId}",
                jobQueueId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "BulkAnimalRatesUpdate staging cleanup failed after commit; rate changes were already applied — staging rows may require manual cleanup | JobQueueId={JobQueueId}",
                jobQueueId);
        }
    }

    private static void ValidatePreconditions(BulkRatesJobQueueEntry entry, BulkRatesExecutionContext context)
    {
        // The orchestrator transitions Approved -> Running before invoking ExecuteAsync
        // (see JobOrchestrator.RunAsync), so by the time this runs the persisted status
        // is always 'Running' — checking for 'Approved' here would always fail.
        if (!string.Equals(entry.Status, "Running", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                $"BulkAnimalRatesUpdate: request {entry.JobQueueId:D} is in status '{entry.Status}', expected 'Running'.");

        if (!string.Equals(entry.JobName, BatchJobNames.BulkAnimalRatesUpdate, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                $"BulkAnimalRatesUpdate: JobExecutionId {context.JobExecutionId:D} belongs to job '{entry.JobName}'.");

        if (entry.FpsYear <= 0)
            throw new InvalidOperationException(
                $"BulkAnimalRatesUpdate: request {entry.JobQueueId:D} has no valid fpsyear.");

        if (context.TriggerYear.HasValue && context.TriggerYear.Value != entry.FpsYear)
            throw new InvalidOperationException(
                $"BulkAnimalRatesUpdate: trigger year {context.TriggerYear.Value} does not match persisted fpsyear {entry.FpsYear}.");

        if (string.IsNullOrWhiteSpace(entry.ApprovedBy) || !entry.ApprovedAtUtc.HasValue)
            throw new InvalidOperationException(
                $"BulkAnimalRatesUpdate: request {entry.JobQueueId:D} is missing approval metadata.");
    }

    // ── Live row read ─────────────────────────────────────────────────

    private static async Task<Dictionary<string, (decimal? DailyRate, decimal? DefraDailyRate, bool PlanByWeek, string? Species, string? SecurityLevel)>> GetAnimalRowsForUpdateAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        IReadOnlyCollection<string> animalTypes, int fpsYear,
        CancellationToken ct)
    {
        var result = new Dictionary<string, (decimal? DailyRate, decimal? DefraDailyRate, bool PlanByWeek, string? Species, string? SecurityLevel)>(StringComparer.OrdinalIgnoreCase);
        if (animalTypes.Count == 0)
            return result;

        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"
            SELECT animaltype, species, security_level, dailyrate::numeric, defradailyrate::numeric, planbyweek
            FROM fps.tblanimals
            WHERE fpsyear = @fpsyear AND animaltype = ANY(@types)
            ORDER BY animaltype
            FOR UPDATE;";
        cmd.Parameters.AddWithValue("fpsyear", fpsYear);
        cmd.Parameters.Add(new NpgsqlParameter("types", NpgsqlDbType.Array | NpgsqlDbType.Text)
        {
            Value = animalTypes.ToArray()
        });

        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
        {
            var animalType = r.GetString(0);
            result[animalType.ToUpperInvariant()] = (
                r.IsDBNull(3) ? null : r.GetDecimal(3),
                r.IsDBNull(4) ? null : r.GetDecimal(4),
                !r.IsDBNull(5) && r.GetBoolean(5),
                r.IsDBNull(1) ? null : r.GetString(1),
                r.IsDBNull(2) ? null : r.GetString(2)
            );
        }
        return result;
    }

    private static async Task<int> UpdateAnimalRowAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        AnimalStagingRow row, int fpsYear,
        CancellationToken ct)
    {
        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"
            UPDATE fps.tblanimals
            SET dailyrate      = @dailyrate::money,
                defradailyrate = @defradailyrate::money,
                planbyweek     = @planbyweek,
                species        = @species,
                security_level = @security_level
            WHERE animaltype = @animaltype AND fpsyear = @fpsyear;";
        cmd.Parameters.AddWithValue("dailyrate",      row.EffectiveDailyRate ?? 0m);
        cmd.Parameters.AddWithValue("defradailyrate", row.EffectiveDefraDailyRate ?? 0m);
        cmd.Parameters.AddWithValue("planbyweek",     row.EffectivePlanByWeek ?? false);
        var species = string.IsNullOrWhiteSpace(row.EffectiveSpecies) ? null : row.EffectiveSpecies.Trim();
        var secLevel = string.IsNullOrWhiteSpace(row.EffectiveSecurityLevel) ? null : row.EffectiveSecurityLevel.Trim();
        cmd.Parameters.AddWithValue("species",        (object?)species ?? DBNull.Value);
        cmd.Parameters.AddWithValue("security_level", (object?)secLevel ?? DBNull.Value);
        cmd.Parameters.AddWithValue("animaltype",     row.AnimalType);
        cmd.Parameters.AddWithValue("fpsyear",        fpsYear);
        return await cmd.ExecuteNonQueryAsync(ct);
    }

    private static RateChangeHistoryRow[] BuildHistory(
        AnimalStagingRow row,
        (decimal? DailyRate, decimal? DefraDailyRate, bool PlanByWeek, string? Species, string? SecurityLevel)? before,
        BulkRatesJobQueueEntry entry, DateTime appliedAt)
    {
        var key = JsonSerializer.Serialize(new { animalType = row.AnimalType });
        var c = (entry.JobQueueId, entry.JobExecutionId, entry.JobId, entry.FpsYear,
                 "Animal", key, entry.RequestedBy, entry.ApprovedBy, appliedAt);

        var beforeDailyRate      = before?.DailyRate ?? 0m;
        var beforeDefraDailyRate = before?.DefraDailyRate ?? 0m;
        var beforePlanByWeek     = before?.PlanByWeek ?? false;
        var beforeSpecies        = string.IsNullOrWhiteSpace(before?.Species) ? null : before.Value.Species!.Trim();
        var beforeSecurityLevel  = string.IsNullOrWhiteSpace(before?.SecurityLevel) ? null : before.Value.SecurityLevel!.Trim();

        var afterDailyRate      = row.EffectiveDailyRate ?? 0m;
        var afterDefraDailyRate = row.EffectiveDefraDailyRate ?? 0m;
        var afterPlanByWeek     = row.EffectivePlanByWeek ?? false;
        var afterSpecies        = string.IsNullOrWhiteSpace(row.EffectiveSpecies) ? null : row.EffectiveSpecies.Trim();
        var afterSecurityLevel  = string.IsNullOrWhiteSpace(row.EffectiveSecurityLevel) ? null : row.EffectiveSecurityLevel.Trim();

        var rows = new List<RateChangeHistoryRow>();
        if (beforeDailyRate != afterDailyRate)
            rows.Add(MakeRow(c, "dailyrate", beforeDailyRate.ToString(), afterDailyRate.ToString(), "Update"));
        if (beforeDefraDailyRate != afterDefraDailyRate)
            rows.Add(MakeRow(c, "defradailyrate", beforeDefraDailyRate.ToString(), afterDefraDailyRate.ToString(), "Update"));
        if (beforePlanByWeek != afterPlanByWeek)
            rows.Add(MakeRow(c, "planbyweek", beforePlanByWeek.ToString(), afterPlanByWeek.ToString(), "Update"));
        if (!string.Equals(beforeSpecies, afterSpecies, StringComparison.OrdinalIgnoreCase))
            rows.Add(MakeRow(c, "species", beforeSpecies, afterSpecies, "Update"));
        if (!string.Equals(beforeSecurityLevel, afterSecurityLevel, StringComparison.OrdinalIgnoreCase))
            rows.Add(MakeRow(c, "security_level", beforeSecurityLevel, afterSecurityLevel, "Update"));
        return [.. rows];
    }

    private static RateChangeHistoryRow MakeRow(
        (Guid JobQueueId, Guid JobExecutionId, int JobId, int FpsYear,
         string RateCategory, string BusinessKeyJson,
         string? RequestedBy, string? ApprovedBy, DateTime AppliedAt) c,
        string field, string? oldVal, string? newVal, string changeType)
        => new(c.JobQueueId, c.JobExecutionId, c.JobId, c.FpsYear,
               c.RateCategory, c.BusinessKeyJson, field,
               oldVal, newVal, changeType, c.RequestedBy, c.ApprovedBy, c.AppliedAt);
}
