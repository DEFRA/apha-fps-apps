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
/// Infrastructure implementation of <see cref="IBulkStaffRatesService"/>.
/// Applies Staff profit-centre grade annual rate changes (PayRate, NPR, OHR)
/// inside a single database transaction, writes permanent history, and
/// clears request-scoped staging rows on success.
/// Drift detection and revalidation are the responsibility of the FPS approval flow;
/// the worker applies frozen effective values directly.
/// </summary>
public sealed class BulkStaffRatesService : IBulkStaffRatesService
{
    private readonly IDbContextFactory<BatchJobsDbContext> _dbContextFactory;
    private readonly IBulkRatesRepository _repository;
    private readonly ILogger<BulkStaffRatesService> _logger;

    public BulkStaffRatesService(
        IDbContextFactory<BatchJobsDbContext> dbContextFactory,
        IBulkRatesRepository repository,
        ILogger<BulkStaffRatesService> logger)
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
                $"BulkStaffRatesUpdate: no job_queue row found for JobExecutionId={context.JobExecutionId:D}.");

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
        var stagingRows = await _repository.GetStaffStagingRowsAsync(jobQueueId, cancellationToken);

        if (stagingRows.Count == 0)
        {
            throw new InvalidOperationException(
                $"BulkStaffRatesUpdate: no staging rows found for JobQueueId={jobQueueId:D}.");
        }

        _logger.LogInformation(
            "BulkStaffRatesUpdate staging loaded | JobQueueId={JobQueueId} | Rows={Rows}",
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
            var pcGrades = stagingRows
                .Select(r => r.PcGrade)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var liveLookup = await GetStaffRowsForUpdateAsync(conn, tx, pcGrades, fpsYear, cancellationToken);

            foreach (var row in stagingRows)
            {
                switch (row.CalculatedAction)
                {
                    case "NoChange":
                        unchanged++;
                        break;

                    case "Update":
                        liveLookup.TryGetValue(row.PcGrade.ToUpperInvariant(), out var liveBefore);
                        var rowsAffected = await UpdateStaffRowAsync(conn, tx, row, fpsYear, cancellationToken);
                        if (rowsAffected == 0)
                            throw new InvalidOperationException(
                                $"BulkStaffRatesUpdate: UPDATE matched 0 rows for PcGrade='{row.PcGrade}' in JobQueueId={jobQueueId:D}.");
                        foreach (var historyRow in BuildHistory(row, liveBefore, entry, appliedAt))
                            await BulkRatesRepository.InsertHistoryRowAsync(conn, tx, historyRow, cancellationToken);
                        updated++;
                        break;

                    case "Insert":
                        throw new InvalidOperationException(
                            $"BulkStaffRatesUpdate: Staff Insert is not supported. " +
                            $"PcGrade='{row.PcGrade}' in JobQueueId={jobQueueId:D} " +
                            "has CalculatedAction=Insert, which indicates an upstream defect.");

                    default:
                        throw new InvalidOperationException(
                            $"BulkStaffRatesUpdate: unexpected CalculatedAction '{row.CalculatedAction}' " +
                            $"for PcGrade='{row.PcGrade}' in JobQueueId={jobQueueId:D}.");
                }
            }

            await tx.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "BulkStaffRatesUpdate committed | JobQueueId={JobQueueId} | Updated={Updated} | Unchanged={Unchanged}",
                jobQueueId, updated, unchanged);

            // ── US-XC-02: Log commit summary ──────────────────────────────
            await _repository.WriteJobQueueLogAsync(
                jobQueueId,
                $"Rate changes committed: Staff updated={updated}, unchanged={unchanged}.",
                entry.ApprovedBy, cancellationToken);
        }

        // ── 4. Delete staging post-commit ─────────────────────────────────
        // Best-effort cleanup: the rate change is already committed, so a failure here must not
        // fail the job or trigger a whole-job retry. Log and move on.
        try
        {
            await _repository.DeleteStaffStagingRowsAsync(jobQueueId, cancellationToken);

            _logger.LogInformation(
                "BulkStaffRatesUpdate staging cleared | JobQueueId={JobQueueId}",
                jobQueueId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "BulkStaffRatesUpdate staging cleanup failed after commit; rate changes were already applied — staging rows may require manual cleanup | JobQueueId={JobQueueId}",
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
                $"BulkStaffRatesUpdate: request {entry.JobQueueId:D} is in status '{entry.Status}', expected 'Running'.");

        if (!string.Equals(entry.JobName, BatchJobNames.BulkStaffRatesUpdate, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                $"BulkStaffRatesUpdate: JobExecutionId {context.JobExecutionId:D} belongs to job '{entry.JobName}'.");

        if (entry.FpsYear <= 0)
            throw new InvalidOperationException(
                $"BulkStaffRatesUpdate: request {entry.JobQueueId:D} has no valid fpsyear.");

        if (context.TriggerYear.HasValue && context.TriggerYear.Value != entry.FpsYear)
            throw new InvalidOperationException(
                $"BulkStaffRatesUpdate: trigger year {context.TriggerYear.Value} does not match persisted fpsyear {entry.FpsYear}.");

        if (string.IsNullOrWhiteSpace(entry.ApprovedBy) || !entry.ApprovedAtUtc.HasValue)
            throw new InvalidOperationException(
                $"BulkStaffRatesUpdate: request {entry.JobQueueId:D} is missing approval metadata.");
    }

    // ── Drift check ──────────────────────────────────────────────────

    /// <summary>
    /// Locks and reads the live fps.profitcentregrade rows this upload targets for history.
    /// Ordered by business key ascending before FOR UPDATE — deterministic lock order
    /// reduces deadlock risk when concurrent requests target overlapping grades.
    /// </summary>
    private static async Task<Dictionary<string, (decimal? PayRate, decimal? Npr, decimal? Ohr)>> GetStaffRowsForUpdateAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        IReadOnlyCollection<string> pcGrades, int fpsYear,
        CancellationToken ct)
    {
        var result = new Dictionary<string, (decimal? PayRate, decimal? Npr, decimal? Ohr)>(StringComparer.OrdinalIgnoreCase);
        if (pcGrades.Count == 0)
            return result;

        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"
            SELECT pcgrade, payrate::numeric, npr::numeric, ohr::numeric
            FROM fps.profitcentregrade
            WHERE fpsyear = @fpsyear AND pcgrade = ANY(@grades)
            ORDER BY pcgrade
            FOR UPDATE;";
        cmd.Parameters.AddWithValue("fpsyear", fpsYear);
        cmd.Parameters.Add(new NpgsqlParameter("grades", NpgsqlDbType.Array | NpgsqlDbType.Text)
        {
            Value = pcGrades.ToArray()
        });

        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
        {
            var pcGrade = r.GetString(0);
            result[pcGrade.ToUpperInvariant()] = (
                r.IsDBNull(1) ? null : r.GetDecimal(1),
                r.IsDBNull(2) ? null : r.GetDecimal(2),
                r.IsDBNull(3) ? null : r.GetDecimal(3)
            );
        }
        return result;
    }

    private static async Task<int> UpdateStaffRowAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        StaffStagingRow row, int fpsYear,
        CancellationToken ct)
    {
        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"
            UPDATE fps.profitcentregrade
            SET payrate = @payrate::money,
                npr     = @npr::money,
                ohr     = @ohr::money
            WHERE pcgrade = @pcgrade AND fpsyear = @fpsyear;";
        cmd.Parameters.AddWithValue("payrate", row.EffectivePayRate ?? 0m);
        cmd.Parameters.AddWithValue("npr",     row.EffectiveNpr ?? 0m);
        cmd.Parameters.AddWithValue("ohr",     row.EffectiveOhr ?? 0m);
        cmd.Parameters.AddWithValue("pcgrade", row.PcGrade);
        cmd.Parameters.AddWithValue("fpsyear", fpsYear);
        return await cmd.ExecuteNonQueryAsync(ct);
    }

    private static RateChangeHistoryRow[] BuildHistory(
        StaffStagingRow row, (decimal? PayRate, decimal? Npr, decimal? Ohr)? before,
        BulkRatesJobQueueEntry entry, DateTime appliedAt)
    {
        var key = JsonSerializer.Serialize(new { pcGrade = row.PcGrade });
        var c = (entry.JobQueueId, entry.JobExecutionId, entry.JobId, entry.FpsYear,
                 "Staff", key, entry.RequestedBy, entry.ApprovedBy, appliedAt);

        var beforePayRate = before?.PayRate ?? 0m;
        var beforeNpr     = before?.Npr ?? 0m;
        var beforeOhr     = before?.Ohr ?? 0m;
        var afterPayRate  = row.EffectivePayRate ?? 0m;
        var afterNpr      = row.EffectiveNpr ?? 0m;
        var afterOhr      = row.EffectiveOhr ?? 0m;

        var rows = new List<RateChangeHistoryRow>();
        if (beforePayRate != afterPayRate)
            rows.Add(MakeRow(c, "payrate", beforePayRate.ToString(), afterPayRate.ToString(), "Update"));
        if (beforeNpr != afterNpr)
            rows.Add(MakeRow(c, "npr", beforeNpr.ToString(), afterNpr.ToString(), "Update"));
        if (beforeOhr != afterOhr)
            rows.Add(MakeRow(c, "ohr", beforeOhr.ToString(), afterOhr.ToString(), "Update"));
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
