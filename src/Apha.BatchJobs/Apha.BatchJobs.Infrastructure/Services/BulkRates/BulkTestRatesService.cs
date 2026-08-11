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
/// Infrastructure implementation of <see cref="IBulkTestRatesService"/>.
/// Applies FEC Test/Product (FEC before AGRUP, spec §15.2) annual rate changes
/// inside a single database transaction, then writes permanent history and
/// clears request-scoped staging rows on success.
/// Drift detection and revalidation are the responsibility of the FPS approval flow;
/// the worker applies frozen effective values directly.
/// </summary>
public sealed class BulkTestRatesService : IBulkTestRatesService
{
    private readonly IDbContextFactory<BatchJobsDbContext> _dbContextFactory;
    private readonly IBulkRatesRepository _repository;
    private readonly IJobExecutionRepository _executionRepository;
    private readonly ILogger<BulkTestRatesService> _logger;

    public BulkTestRatesService(
        IDbContextFactory<BatchJobsDbContext> dbContextFactory,
        IBulkRatesRepository repository,
        IJobExecutionRepository executionRepository,
        ILogger<BulkTestRatesService> logger)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _executionRepository = executionRepository ?? throw new ArgumentNullException(nameof(executionRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ExecuteAsync(BulkRatesExecutionContext context, CancellationToken cancellationToken = default)
    {
        // ── 1. Load Running, previously approved request ──────────────────
        // Re-reading job_queue/staging here (rather than inside the write transaction below)
        // is safe: this request's own status/staging identity cannot change concurrently —
        // JobOrchestrator's job-name lock guarantees only one worker execution of this
        // JobQueueId runs at a time, and staging rows are frozen (no re-upload permitted)
        // once a request reaches Approved. The remaining race is on the
        // *live* fps.testorproduct/fps.tlkptestreqmt rows, which is why those specifically
        // move inside the transaction under FOR UPDATE below.
        var entry = await _repository.GetRunningRequestAsync(context.JobExecutionId, cancellationToken)
            ?? throw new InvalidOperationException(
                $"BulkTestRatesUpdate: no job_queue row found for JobExecutionId={context.JobExecutionId:D}.");

        ValidatePreconditions(entry, context);

        var jobQueueId    = entry.JobQueueId;
        var jobId         = entry.JobId;
        var fpsYear       = entry.FpsYear;
        var requestedBy   = entry.RequestedBy;
        var approvedBy    = entry.ApprovedBy;
        var appliedAt     = DateTime.UtcNow;

        // ── US-XC-02: Log execution start ─────────────────────────────
        await _repository.WriteJobQueueLogAsync(
            jobQueueId,
            $"Worker execution starting (FPS year {fpsYear}).",
            approvedBy, cancellationToken);
        _logger.LogInformation(
            "[BulkRates.ExecutionStarted] JobQueueId={JobQueueId} | JobName={JobName} | FpsYear={FpsYear}",
            jobQueueId, entry.JobName, fpsYear);

        // ── 2. Load approved staging rows ──────────────────────────────────
        var fecRows   = await _repository.GetFecStagingRowsAsync(jobQueueId, cancellationToken);
        var agrupRows = await _repository.GetAgrupStagingRowsAsync(jobQueueId, cancellationToken);

        if (fecRows.Count == 0 && agrupRows.Count == 0)
        {
            throw new InvalidOperationException(
                $"BulkTestRatesUpdate: no staging rows found for JobQueueId={jobQueueId:D}. " +
                "Request cannot be executed without approved data.");
        }

        _logger.LogInformation(
            "BulkTestRatesUpdate staging loaded | JobQueueId={JobQueueId} | FecRows={FecRows} | AgrupRows={AgrupRows}",
            jobQueueId, fecRows.Count, agrupRows.Count);

        // ── 3. Execute all mutations in one transaction ───────────────────
        var historyRows = new List<RateChangeHistoryRow>();

        await using var dbContext = _dbContextFactory.CreateDbContext();
        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        var conn = (NpgsqlConnection)dbContext.Database.GetDbConnection();
        await using (var tx = await conn.BeginTransactionAsync(cancellationToken))
        {
            // Lock and read live rows for history (deterministic test-code order reduces deadlock risk).
            var testCodes = fecRows.Select(r => r.TestCode)
                .Concat(agrupRows.Select(r => r.TestCode))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var liveFecLookup   = await GetFecRowsForUpdateAsync(conn, tx, testCodes, fpsYear, cancellationToken);
            var liveAgrupLookup = await GetAgrupRowsForUpdateAsync(conn, tx, testCodes, fpsYear, cancellationToken);

            // FEC Test/Product — inserts first, then updates (spec §15.2)
            int fecInserted = 0, fecUpdated = 0, fecUnchanged = 0;
            foreach (var row in fecRows)
            {
                var effectiveRate = row.EffectiveNewRate ?? 0m;
                liveFecLookup.TryGetValue(row.TestCode.ToUpperInvariant(), out var live);

                switch (row.CalculatedAction)
                {
                    case "Insert":
                        await InsertFecRowAsync(conn, tx, row, fpsYear, effectiveRate, cancellationToken);
                        historyRows.AddRange(BuildFecInsertHistory(row, entry, appliedAt, effectiveRate));
                        fecInserted++;
                        break;

                    case "Update":
                    case "ZeroRateWithdrawal":
                        await UpdateFecRowAsync(conn, tx, row.TestCode, fpsYear, effectiveRate, cancellationToken);
                        historyRows.AddRange(BuildFecUpdateHistory(
                            row, (live.UnitPriceVla ?? 0m, live.DefraUnitPrice ?? 0m),
                            entry, appliedAt, effectiveRate, row.CalculatedAction!));
                        fecUpdated++;
                        break;

                    default:
                        fecUnchanged++;
                        break;
                }
            }

            // AGRUP — after FEC (spec §2.4 sequencing rule)
            int agrupInserted = 0, agrupUpdated = 0, agrupUnchanged = 0;
            foreach (var row in agrupRows)
            {
                var agrupKey = (row.TestCode.ToUpperInvariant(), row.Buyer.ToUpperInvariant());
                var effectiveRate = row.EffectiveNewRate ?? 0m;
                liveAgrupLookup.TryGetValue(agrupKey, out var live);

                switch (row.CalculatedAction)
                {
                    case "Insert":
                        await InsertAgrupRowAsync(conn, tx, row, fpsYear, effectiveRate, appliedAt, cancellationToken);
                        await WriteTestreqLogAsync(conn, tx,
                            row.TestCode, row.Buyer, fpsYear, effectiveRate,
                            row.NoRequired, row.ProjectBuyerCode, row.TestBuyerCode, active: 1,
                            appliedAt, approvedBy, "I", cancellationToken);
                        historyRows.AddRange(BuildAgrupInsertHistory(row, entry, appliedAt, effectiveRate));
                        agrupInserted++;
                        break;

                    case "Update":
                    case "ZeroRateWithdrawal":
                        await UpdateAgrupRowAsync(conn, tx, row.TestCode, row.Buyer, fpsYear, effectiveRate, cancellationToken);
                        await WriteTestreqLogAsync(conn, tx,
                            row.TestCode, row.Buyer, fpsYear, effectiveRate,
                            live.NoRequired, live.ProjectBuyerCode, live.TestBuyerCode, live.Active,
                            appliedAt, approvedBy, "I", cancellationToken);
                        historyRows.AddRange(BuildAgrupUpdateHistory(
                            row, live.UnitPrice, entry, appliedAt, effectiveRate, row.CalculatedAction!));
                        agrupUpdated++;
                        break;

                    default:
                        agrupUnchanged++;
                        break;
                }
            }

            // ── 4. Write permanent history inside the transaction ─────────
            await WriteHistoryInsideTransactionAsync(conn, tx, historyRows, cancellationToken);

            await tx.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "BulkTestRatesUpdate committed | JobQueueId={JobQueueId} | FecInserted={FI} | FecUpdated={FU} | FecUnchanged={FC} | AgrupInserted={AI} | AgrupUpdated={AU} | AgrupUnchanged={AC}",
                jobQueueId, fecInserted, fecUpdated, fecUnchanged, agrupInserted, agrupUpdated, agrupUnchanged);

            // ── US-XC-02: Log commit summary ──────────────────────────────
            await _repository.WriteJobQueueLogAsync(
                jobQueueId,
                $"Rate changes committed: FEC inserted={fecInserted}, updated={fecUpdated}, unchanged={fecUnchanged}; AGRUP inserted={agrupInserted}, updated={agrupUpdated}, unchanged={agrupUnchanged}.",
                approvedBy, cancellationToken);
        }

        // ── 5. Delete staging rows AFTER successful commit (spec §10.6) ──
        // Best-effort cleanup: the rate change is already committed, so a failure here must not
        // fail the job or trigger a whole-job retry — that would re-run an already-applied change
        // against staging rows that (mostly) still need clearing. Log and move on.
        try
        {
            await _repository.DeleteFecStagingRowsAsync(jobQueueId, cancellationToken);

            _logger.LogInformation(
                "BulkTestRatesUpdate staging cleared | JobQueueId={JobQueueId}",
                jobQueueId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "BulkTestRatesUpdate staging cleanup failed after commit; rate changes were already applied — staging rows may require manual cleanup | JobQueueId={JobQueueId}",
                jobQueueId);
        }
    }

    // ── Precondition validation ─────────────────────────────────────────────

    private static void ValidatePreconditions(BulkRatesJobQueueEntry entry, BulkRatesExecutionContext context)
    {
        // The orchestrator transitions Approved -> Running before invoking ExecuteAsync
        // (see JobOrchestrator.RunAsync), so by the time this runs the persisted status
        // is always 'Running' — checking for 'Approved' here would always fail.
        if (!string.Equals(entry.Status, "Running", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"BulkTestRatesUpdate: request {entry.JobQueueId:D} is in status '{entry.Status}', expected 'Running'.");
        }

        if (!string.Equals(entry.JobName, BatchJobNames.BulkTestRatesUpdate, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"BulkTestRatesUpdate: JobExecutionId {context.JobExecutionId:D} belongs to job '{entry.JobName}', not '{BatchJobNames.BulkTestRatesUpdate}'.");
        }

        if (entry.FpsYear <= 0)
        {
            throw new InvalidOperationException(
                $"BulkTestRatesUpdate: request {entry.JobQueueId:D} has no valid fpsyear.");
        }

        if (context.TriggerYear.HasValue && context.TriggerYear.Value != entry.FpsYear)
        {
            throw new InvalidOperationException(
                $"BulkTestRatesUpdate: trigger year {context.TriggerYear.Value} does not match persisted fpsyear {entry.FpsYear}.");
        }

        if (string.IsNullOrWhiteSpace(entry.ApprovedBy) || !entry.ApprovedAtUtc.HasValue)
        {
            throw new InvalidOperationException(
                $"BulkTestRatesUpdate: request {entry.JobQueueId:D} is missing approval metadata (approved_by/approved_at_utc).");
        }
    }

    // ── FEC helpers ─────────────────────────────────────────────────────────

    private static async Task<Dictionary<string, (decimal? UnitPriceVla, decimal? DefraUnitPrice)>> GetFecRowsForUpdateAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        IReadOnlyCollection<string> testCodes, int fpsYear,
        CancellationToken ct)
    {
        var result = new Dictionary<string, (decimal? UnitPriceVla, decimal? DefraUnitPrice)>(StringComparer.OrdinalIgnoreCase);
        if (testCodes.Count == 0)
            return result;

        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"
            SELECT itemcode, unitpricevla::numeric, defraunitprice::numeric
            FROM fps.testorproduct
            WHERE fpsyear = @fpsyear AND itemcode = ANY(@codes)
            FOR UPDATE;";
        cmd.Parameters.AddWithValue("fpsyear", fpsYear);
        cmd.Parameters.Add(new NpgsqlParameter("codes", NpgsqlDbType.Array | NpgsqlDbType.Text)
        {
            Value = testCodes.ToArray()
        });

        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
        {
            var testCode = r.GetString(0);
            result[testCode.ToUpperInvariant()] = (
                r.IsDBNull(1) ? null : r.GetDecimal(1),
                r.IsDBNull(2) ? null : r.GetDecimal(2)
            );
        }
        return result;
    }

    private static async Task InsertFecRowAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        FecStagingRow row, int fpsYear, decimal rate,
        CancellationToken ct)
    {
        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"
            INSERT INTO fps.testorproduct
                (itemcode, itemdescription, unitpricevla, owner, shortdescription, defraunitprice, fpsyear)
            VALUES
                (@itemcode, @itemdescription, @unitpricevla, @owner, @shortdescription, @defraunitprice, @fpsyear);";
        cmd.Parameters.AddWithValue("itemcode",         row.TestCode);
        cmd.Parameters.AddWithValue("itemdescription",  (object?)row.ItemDescription ?? DBNull.Value);
        cmd.Parameters.AddWithValue("unitpricevla",     rate);
        cmd.Parameters.AddWithValue("owner",            (object?)row.Owner ?? DBNull.Value);
        cmd.Parameters.AddWithValue("shortdescription", (object?)row.ShortDescription ?? DBNull.Value);
        cmd.Parameters.AddWithValue("defraunitprice",   rate);
        cmd.Parameters.AddWithValue("fpsyear",          fpsYear);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static async Task UpdateFecRowAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        string testCode, int fpsYear, decimal newRate,
        CancellationToken ct)
    {
        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"
            UPDATE fps.testorproduct
            SET unitpricevla = @rate, defraunitprice = @rate
            WHERE itemcode = @itemcode AND fpsyear = @fpsyear;";
        cmd.Parameters.AddWithValue("rate",    newRate);
        cmd.Parameters.AddWithValue("itemcode", testCode);
        cmd.Parameters.AddWithValue("fpsyear", fpsYear);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    // ── AGRUP helpers ────────────────────────────────────────────────────────

    /// <summary>
    /// Locks and reads every live fps.tlkptestreqmt row under any TestCode this upload
    /// targets — by TestCode, not by the exact staged (TestCode,Buyer) keys.
    /// </summary>
    private static async Task<Dictionary<(string TestCode, string Buyer), (decimal? UnitPrice, double? NoRequired, string? ProjectBuyerCode, string? TestBuyerCode, short? Active)>> GetAgrupRowsForUpdateAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        IReadOnlyCollection<string> testCodes, int fpsYear,
        CancellationToken ct)
    {
        var result = new Dictionary<(string, string), (decimal? UnitPrice, double? NoRequired, string? ProjectBuyerCode, string? TestBuyerCode, short? Active)>();
        if (testCodes.Count == 0)
            return result;

        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"
            SELECT testcode, buyer, unitprice::numeric, projectbuyercode, testbuyercode,
                   norequired, active
            FROM fps.tlkptestreqmt
            WHERE fpsyear = @fpsyear AND testcode = ANY(@codes)
            FOR UPDATE;";
        cmd.Parameters.AddWithValue("fpsyear", fpsYear);
        cmd.Parameters.Add(new NpgsqlParameter("codes", NpgsqlDbType.Array | NpgsqlDbType.Text)
        {
            Value = testCodes.ToArray()
        });

        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
        {
            var testCode = r.GetString(0);
            var buyer    = r.GetString(1);
            result[(testCode.ToUpperInvariant(), buyer.ToUpperInvariant())] = (
                r.IsDBNull(2) ? null : r.GetDecimal(2),
                r.IsDBNull(5) ? null : r.GetDouble(5),
                r.IsDBNull(3) ? null : r.GetString(3),
                r.IsDBNull(4) ? null : r.GetString(4),
                r.IsDBNull(6) ? null : r.GetInt16(6)
            );
        }
        return result;
    }

    private static async Task InsertAgrupRowAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        AgrupStagingRow row, int fpsYear, decimal rate, DateTime executionTimestamp,
        CancellationToken ct)
    {
        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        // Use the staged routing fields instead of the old hardcoded
        // ProjectBuyerCode = Buyer / omitted TestBuyerCode — supersedes tracker A-14
        // (reconciliation §2.4, superseded-not-edited per that doc's own ground rule).
        cmd.CommandText = @"
            INSERT INTO fps.tlkptestreqmt
                (testcode, buyer, unitprice, norequired, projectbuyercode, testbuyercode, datecreated, active, fpsyear)
            VALUES
                (@testcode, @buyer, @unitprice, @norequired, @projectbuyercode, @testbuyercode, @datecreated, 1, @fpsyear);";
        cmd.Parameters.AddWithValue("testcode",   row.TestCode);
        cmd.Parameters.AddWithValue("buyer",      row.Buyer);
        cmd.Parameters.AddWithValue("unitprice",  rate);
        cmd.Parameters.AddWithValue("norequired", (object?)row.NoRequired ?? DBNull.Value);
        cmd.Parameters.AddWithValue("projectbuyercode", (object?)row.ProjectBuyerCode ?? DBNull.Value);
        cmd.Parameters.AddWithValue("testbuyercode",    (object?)row.TestBuyerCode ?? DBNull.Value);
        cmd.Parameters.AddWithValue("datecreated", executionTimestamp);
        cmd.Parameters.AddWithValue("fpsyear",    fpsYear);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static async Task UpdateAgrupRowAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        string testCode, string buyer, int fpsYear, decimal newRate,
        CancellationToken ct)
    {
        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        // Spec §2.3: Update UnitPrice only; do not touch NoRequired, DateCreated, Active,
        // ProjectBuyerCode/TestBuyerCode (existing-row routing immutability).
        cmd.CommandText = @"
            UPDATE fps.tlkptestreqmt
            SET unitprice = @unitprice
            WHERE testcode = @testcode AND buyer = @buyer AND fpsyear = @fpsyear;";
        cmd.Parameters.AddWithValue("unitprice", newRate);
        cmd.Parameters.AddWithValue("testcode",  testCode);
        cmd.Parameters.AddWithValue("buyer",     buyer);
        cmd.Parameters.AddWithValue("fpsyear",   fpsYear);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    // ── History builders ─────────────────────────────────────────────────────

    private static IEnumerable<RateChangeHistoryRow> BuildFecInsertHistory(
        FecStagingRow row, BulkRatesJobQueueEntry entry, DateTime appliedAt, decimal newRate)
    {
        var key = JsonSerializer.Serialize(new { testCode = row.TestCode });
        var common = (entry.JobQueueId, entry.JobExecutionId, entry.JobId, entry.FpsYear,
                      "FEC", key, entry.RequestedBy, entry.ApprovedBy, appliedAt);

        yield return MakeHistoryRow(common, "unitpricevla",  null, newRate.ToString(), "Insert");
        yield return MakeHistoryRow(common, "defraunitprice", null, newRate.ToString(), "Insert");
    }

    private static IEnumerable<RateChangeHistoryRow> BuildFecUpdateHistory(
        FecStagingRow row,
        (decimal UnitPriceVla, decimal DefraUnitPrice) before,
        BulkRatesJobQueueEntry entry, DateTime appliedAt, decimal newRate, string changeType)
    {
        var key = JsonSerializer.Serialize(new { testCode = row.TestCode });
        var common = (entry.JobQueueId, entry.JobExecutionId, entry.JobId, entry.FpsYear,
                      "FEC", key, entry.RequestedBy, entry.ApprovedBy, appliedAt);

        yield return MakeHistoryRow(common, "unitpricevla",   before.UnitPriceVla.ToString(), newRate.ToString(), changeType);
        yield return MakeHistoryRow(common, "defraunitprice",  before.DefraUnitPrice.ToString(), newRate.ToString(), changeType);
    }

    private static IEnumerable<RateChangeHistoryRow> BuildAgrupInsertHistory(
        AgrupStagingRow row, BulkRatesJobQueueEntry entry, DateTime appliedAt, decimal newRate)
    {
        var key = JsonSerializer.Serialize(new { testCode = row.TestCode, buyer = row.Buyer });
        var common = (entry.JobQueueId, entry.JobExecutionId, entry.JobId, entry.FpsYear,
                      "AGRUP", key, entry.RequestedBy, entry.ApprovedBy, appliedAt);

        yield return MakeHistoryRow(common, "unitprice", null, newRate.ToString(), "Insert");
    }

    private static IEnumerable<RateChangeHistoryRow> BuildAgrupUpdateHistory(
        AgrupStagingRow row, decimal? currentUnitPrice, BulkRatesJobQueueEntry entry, DateTime appliedAt,
        decimal newRate, string changeType)
    {
        var key = JsonSerializer.Serialize(new { testCode = row.TestCode, buyer = row.Buyer });
        var common = (entry.JobQueueId, entry.JobExecutionId, entry.JobId, entry.FpsYear,
                      "AGRUP", key, entry.RequestedBy, entry.ApprovedBy, appliedAt);

        yield return MakeHistoryRow(common, "unitprice", currentUnitPrice?.ToString(), newRate.ToString(), changeType);
    }

    private static RateChangeHistoryRow MakeHistoryRow(
        (Guid JobQueueId, Guid JobExecutionId, int JobId, int FpsYear,
         string RateCategory, string BusinessKeyJson,
         string? RequestedBy, string? ApprovedBy, DateTime AppliedAt) c,
        string fieldName, string? oldValue, string? newValue, string changeType)
        => new(c.JobQueueId, c.JobExecutionId, c.JobId, c.FpsYear,
               c.RateCategory, c.BusinessKeyJson, fieldName,
               oldValue, newValue, changeType,
               c.RequestedBy, c.ApprovedBy, c.AppliedAt);

    // ── testreq_log write ───────────────────────────────────
    // Restores the legacy trigger-equivalent row snapshot in fps.testreq_log.
    // Always insert_delete='I' for Insert, Update, and ZeroRateWithdrawal (the final
    // live row image after the change is applied). 'D' is reserved for physical Delete
    // paths if one is ever introduced. Same transaction as the live-table mutation and
    // rate_change_history insert — rolls back together on failure.

    private static async Task WriteTestreqLogAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        string testCode, string buyer, int fpsYear, decimal unitPrice,
        double? noRequired, string? projectBuyerCode, string? testBuyerCode, short? active,
        DateTime executionTimestamp, string? userId, string insertDelete,
        CancellationToken ct)
    {
        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"
            INSERT INTO fps.testreq_log
                (testcode, buyer, unitprice, norequired, projectbuyercode, testbuyercode,
                 active, date_time, user_id, insert_delete, jobcode, fpsyear)
            VALUES
                (@testcode, @buyer, @unitprice, @norequired, @projectbuyercode, @testbuyercode,
                 @active, @date_time, @user_id, @insert_delete, @jobcode, @fpsyear);";
        // Truncate to column sizes: testcode/buyer varchar(20), user_id varchar(20)
        cmd.Parameters.AddWithValue("testcode",         testCode.Length <= 20 ? testCode : testCode[..20]);
        cmd.Parameters.AddWithValue("buyer",            buyer.Length <= 20 ? buyer : buyer[..20]);
        cmd.Parameters.AddWithValue("unitprice",        (double)unitPrice);
        cmd.Parameters.AddWithValue("norequired",       noRequired.HasValue ? (object)(int)noRequired.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("projectbuyercode", (object?)projectBuyerCode ?? DBNull.Value);
        cmd.Parameters.AddWithValue("testbuyercode",    (object?)testBuyerCode ?? DBNull.Value);
        cmd.Parameters.AddWithValue("active",           active.HasValue ? (object)active.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("date_time",        executionTimestamp);
        cmd.Parameters.AddWithValue("user_id",
            userId is null ? DBNull.Value
            : userId.Length <= 20 ? (object)userId : userId[..20]);
        cmd.Parameters.AddWithValue("insert_delete",    insertDelete);
        // jobcode mirrors projectbuyercode per schema comment
        cmd.Parameters.AddWithValue("jobcode",          (object?)projectBuyerCode ?? DBNull.Value);
        cmd.Parameters.AddWithValue("fpsyear",          fpsYear);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    // ── Write history inside an existing open transaction ───────────────────
    // We use the same connection/transaction as the mutations so history is
    // included in the same commit (spec §17.2).

    private static async Task WriteHistoryInsideTransactionAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        IReadOnlyList<RateChangeHistoryRow> rows,
        CancellationToken ct)
    {
        foreach (var row in rows)
        {
            await using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = @"
                INSERT INTO fps.rate_change_history
                    (jobqueueid, jobexecutionid, jobid, fpsyear, ratecategory,
                     businesskey, fieldname, oldvalue, newvalue, changetype,
                     requestedby, approvedby, appliedatutc)
                VALUES
                    (@jobqueueid, @jobexecutionid, @jobid, @fpsyear, @ratecategory,
                     @businesskey::jsonb, @fieldname, @oldvalue, @newvalue, @changetype,
                     @requestedby, @approvedby, @appliedatutc);";
            cmd.Parameters.AddWithValue("jobqueueid",    row.JobQueueId);
            cmd.Parameters.AddWithValue("jobexecutionid", row.JobExecutionId);
            cmd.Parameters.AddWithValue("jobid",         row.JobId);
            cmd.Parameters.AddWithValue("fpsyear",       row.FpsYear);
            cmd.Parameters.AddWithValue("ratecategory",  row.RateCategory);
            cmd.Parameters.AddWithValue("businesskey",   row.BusinessKeyJson);
            cmd.Parameters.AddWithValue("fieldname",     row.FieldName);
            cmd.Parameters.AddWithValue("oldvalue",      (object?)row.OldValue ?? DBNull.Value);
            cmd.Parameters.AddWithValue("newvalue",      (object?)row.NewValue ?? DBNull.Value);
            cmd.Parameters.AddWithValue("changetype",    row.ChangeType);
            cmd.Parameters.AddWithValue("requestedby",   (object?)row.RequestedBy ?? DBNull.Value);
            cmd.Parameters.AddWithValue("approvedby",    (object?)row.ApprovedBy ?? DBNull.Value);
            cmd.Parameters.AddWithValue("appliedatutc",  row.AppliedAtUtc);
            await cmd.ExecuteNonQueryAsync(ct);
        }
    }
}
