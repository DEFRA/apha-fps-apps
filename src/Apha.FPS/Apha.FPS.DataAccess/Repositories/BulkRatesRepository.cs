using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Apha.FPS.DataAccess.Repositories
{
    /// <summary>
    /// Hybrid EF/raw-Npgsql implementation of <see cref="IBulkRatesRepository"/> for the FPS API.
    /// The core job_queue lifecycle (lookup, CRUD, status transitions, audit log) uses
    /// EF/LINQ via the widened <see cref="BatchJobQueue"/> entity and the existing BatchJobs entities —
    /// see <see cref="QueueRowsQuery"/>. Staging replace/read, validation errors, download
    /// snapshots, and live-table exports remain raw Npgsql: those tables have no existing EF
    /// mapping, and several (paired-array lookups, the concurrency-guarded download-activation
    /// UPDATE) have PostgreSQL-specific or atomicity semantics that are the actual correctness
    /// requirement, not just an implementation detail LINQ would express just as well. Each
    /// method below carries a short comment where it stays raw explaining why.
    /// </summary>
    public class BulkRatesRepository : IBulkRatesRepository
    {
        private readonly FpsDbContext _dbContext;
        private readonly ILogger<BulkRatesRepository> _logger;

        public BulkRatesRepository(FpsDbContext dbContext, ILogger<BulkRatesRepository> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ── Connection helper ────────────────────────────────────────────────────

        private async Task<NpgsqlConnection> OpenAsync(CancellationToken ct)
        {
            await _dbContext.Database.OpenConnectionAsync(ct);
            return (NpgsqlConnection)_dbContext.Database.GetDbConnection();
        }

        // ── Job master / status lookup ───────────────────────────────────────────
        // Ordinary single-value lookups on tables already EF-mapped (BatchJobMaster/
        // BatchJobStatus, shared with the YearEnd/BatchJobs feature) — natural LINQ.

        public Task<int?> GetJobIdByNameAsync(string jobName, CancellationToken ct = default) =>
            _dbContext.BatchJobs
                .Where(j => j.JobName == jobName)
                .Select(j => (int?)j.JobId)
                .FirstOrDefaultAsync(ct);

        public Task<int?> GetStatusIdByNameAsync(int jobId, string statusName, CancellationToken ct = default) =>
            _dbContext.BatchJobStatuses
                .Where(s => s.JobId == jobId && s.Status == statusName)
                .Select(s => (int?)s.StatusId)
                .FirstOrDefaultAsync(ct);

        // ── Queue entry CRUD ─────────────────────────────────────────────────────
        // fps.job_queue + job_master + job_status, via the existing BatchJobQueue entity
        // (widened with the Bulk-Rates workflow columns — see BatchJobQueue's doc comment)
        // joined against the existing, shared BatchJobMaster/BatchJobStatus entities.
        // BatchJobQueue carries a FpsYear query filter tied to the ambient
        // IFpsRequestContext year (for YearEnd's own use) — every query against it here
        // calls .IgnoreQueryFilters() since Bulk Rates filters by an explicit, caller-supplied
        // year rather than "whatever year the UI currently has selected".

        /// <summary>
        /// The job_queue + job_master + job_status join, projected into <see cref="BulkRatesQueueRow"/>.
        /// Composable: callers add their own Where/OrderBy/paging before executing.
        /// </summary>
        private IQueryable<BulkRatesQueueRow> QueueRowsQuery() =>
            from q in _dbContext.BatchJobQueues.IgnoreQueryFilters()
            join m in _dbContext.BatchJobs on q.JobId equals m.JobId
            join s in _dbContext.BatchJobStatuses on new { q.StatusId, q.JobId } equals new { s.StatusId, s.JobId }
            select new BulkRatesQueueRow
            {
                JobQueueId = q.JobqueueId,
                JobId = q.JobId,
                JobName = m.JobName,
                StatusId = q.StatusId,
                Status = s.Status,
                JobExecutionId = q.JobExecutionId,
                RequestedBy = q.RequestedBy,
                RequestedAtUtc = q.RequestedAtUtc ?? default,
                FpsYear = q.FpsYear,
                UploadFilename = q.UploadFilename,
                UploadChecksumSha256 = q.UploadChecksumSha256,
                UploadVersion = q.UploadVersion,
                UploadValidatedAtUtc = q.UploadValidatedAtUtc,
                UploadRowCountsJson = q.UploadRowCountsJson,
                ApprovedBy = q.ApprovedBy,
                ApprovedAtUtc = q.ApprovedAtUtc,
                RejectedBy = q.RejectedBy,
                RejectedAtUtc = q.RejectedAtUtc,
                RejectionReason = q.RejectionReason,
                CancelledBy = q.CancelledBy,
                CancelledAtUtc = q.CancelledAtUtc,
                CancellationReason = q.CancellationReason,
                TriggeredBy = q.TriggeredBy,
                TriggeredAtUtc = q.TriggeredAtUtc,
                StartDateTime = q.StartDateTime,
                EndDateTime = q.EndDateTime,
                // Physical column "errormessage" surfaces as FailureReason, not ErrorMessage —
                // matches the pre-existing raw-SQL mapping exactly.
                FailureReason = q.ErrorMessage,
                ActiveDownloadVersion = q.ActiveDownloadVersion,
                S3ObjectKey = q.S3ObjectKey
            };

        public async Task<BulkRatesQueueRow> CreateRequestAsync(
            Guid jobQueueId, Guid jobExecutionId, int jobId, int initiatedStatusId,
            string requestedBy, DateTime requestedAtUtc, int fpsYear,
            CancellationToken ct = default)
        {
            _dbContext.BatchJobQueues.Add(new BatchJobQueue
            {
                JobqueueId = jobQueueId,
                JobExecutionId = jobExecutionId,
                JobId = jobId,
                StatusId = initiatedStatusId,
                RequestedBy = requestedBy,
                RequestedAtUtc = requestedAtUtc,
                FpsYear = fpsYear
                // StartDateTime is left null — it's the Batch Worker's own "execution actually
                // began" marker (set by JobExecutionRepository's Running transition), not a
                // request/trigger timestamp. TriggeredAtUtc (set in SetApprovalAsync) is what
                // Bulk Rates uses to tell "released" from "dispatch to the worker succeeded".
            });
            await _dbContext.SaveChangesAsync(ct);

            _logger.LogInformation("Created job_queue row | JobQueueId={JobQueueId} | JobId={JobId} | FpsYear={FpsYear}",
                jobQueueId, jobId, fpsYear);

            return await GetRequestAsync(jobExecutionId, ct)
                ?? throw new InvalidOperationException($"Row just inserted (jobExecutionId={jobExecutionId}) could not be read back.");
        }

        public Task<BulkRatesQueueRow?> GetRequestAsync(Guid jobExecutionId, CancellationToken ct = default) =>
            QueueRowsQuery()
                .Where(r => r.JobExecutionId == jobExecutionId)
                .FirstOrDefaultAsync(ct);

        public async Task<PagedData<BulkRatesQueueRow>> GetRequestsAsync(
            string? jobName, int? fpsYear, string? status,
            int page, int pageSize, string? sortBy, bool descending,
            CancellationToken ct = default)
        {
            var query = QueueRowsQuery();
            if (jobName != null) query = query.Where(r => r.JobName == jobName);
            if (fpsYear.HasValue) query = query.Where(r => r.FpsYear == fpsYear.Value);
            if (status != null) query = query.Where(r => r.Status == status);

            var totalRecords = await query.CountAsync(ct);

            // sortBy is user input (via the DataGrid's sortable-header clicks) — the switch
            // below is the whitelist; anything unrecognised falls back to RequestedAtUtc rather
            // than being interpolated into anything.
            query = ApplySortOrder(query, sortBy, descending);

            var results = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PagedData<BulkRatesQueueRow>(results, new PaginationData
            {
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = pageSize > 0 ? (int)Math.Ceiling(totalRecords / (double)pageSize) : 0,
                TotalRecords = totalRecords
            });
        }

        public async Task<bool> CanInitiateRequestAsync(string jobName, CancellationToken ct = default)
        {
            // Built on QueueRowsQuery()'s job/status join and IgnoreQueryFilters() — see the class
            // doc comment. Blocking-status list matches the pre-existing SQL's IN-list exactly
            // (Initiated/ReleasedForApproval/Approved/Running); no new exclusion-based predicate.
            var blockingStatuses = new[] { "Initiated", "ReleasedForApproval", "Approved", "Running" };
            var hasBlockingRequest = await QueueRowsQuery()
                .Where(r => r.JobName == jobName && blockingStatuses.Contains(r.Status))
                .AnyAsync(ct);
            return !hasBlockingRequest;
        }

        // ── Status transitions ───────────────────────────────────────────────────

        // EF atomic operation: a guarded UPDATE (only applies if the row is still in the
        // expected status) must stay a single UPDATE...WHERE statement for its optimistic-
        // concurrency check to mean anything — ExecuteUpdateAsync compiles to exactly that.
        public async Task<bool> TransitionStatusAsync(
            Guid jobQueueId, int expectedStatusId, int newStatusId,
            CancellationToken ct = default)
        {
            var affected = await _dbContext.BatchJobQueues.IgnoreQueryFilters()
                .Where(q => q.JobqueueId == jobQueueId && q.StatusId == expectedStatusId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(q => q.StatusId, newStatusId)
                    .SetProperty(q => q.UpdatedAt, DateTime.UtcNow), ct);
            return affected > 0;
        }

        public Task SetApprovalAsync(
            Guid jobQueueId, Guid jobExecutionId,
            string approvedBy, DateTime approvedAtUtc,
            string triggeredBy, DateTime triggeredAtUtc,
            int approvedStatusId,
            CancellationToken ct = default) =>
            _dbContext.BatchJobQueues.IgnoreQueryFilters()
                .Where(q => q.JobqueueId == jobQueueId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(q => q.StatusId, approvedStatusId)
                    .SetProperty(q => q.ApprovedBy, approvedBy)
                    .SetProperty(q => q.ApprovedAtUtc, approvedAtUtc)
                    .SetProperty(q => q.TriggeredBy, triggeredBy)
                    .SetProperty(q => q.TriggeredAtUtc, triggeredAtUtc)
                    .SetProperty(q => q.UpdatedAt, DateTime.UtcNow), ct);

        public Task SetRejectionAsync(
            Guid jobQueueId, string rejectedBy, DateTime rejectedAtUtc,
            string reason, int rejectedStatusId,
            CancellationToken ct = default) =>
            _dbContext.BatchJobQueues.IgnoreQueryFilters()
                .Where(q => q.JobqueueId == jobQueueId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(q => q.StatusId, rejectedStatusId)
                    .SetProperty(q => q.RejectedBy, rejectedBy)
                    .SetProperty(q => q.RejectedAtUtc, rejectedAtUtc)
                    .SetProperty(q => q.RejectionReason, reason)
                    .SetProperty(q => q.UpdatedAt, DateTime.UtcNow), ct);

        public Task SetCancellationAsync(
            Guid jobQueueId, string cancelledBy, DateTime cancelledAtUtc,
            string? reason, int cancelledStatusId,
            CancellationToken ct = default) =>
            _dbContext.BatchJobQueues.IgnoreQueryFilters()
                .Where(q => q.JobqueueId == jobQueueId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(q => q.StatusId, cancelledStatusId)
                    .SetProperty(q => q.CancelledBy, cancelledBy)
                    .SetProperty(q => q.CancelledAtUtc, cancelledAtUtc)
                    .SetProperty(q => q.CancellationReason, reason)
                    .SetProperty(q => q.UpdatedAt, DateTime.UtcNow), ct);

        // ── Upload metadata ──────────────────────────────────────────────────────

        public Task UpdateUploadMetadataAsync(
            Guid jobQueueId, string filename, string checksumSha256, int uploadVersion,
            DateTime validatedAtUtc, string rowCountsJson, CancellationToken ct = default) =>
            _dbContext.BatchJobQueues.IgnoreQueryFilters()
                .Where(q => q.JobqueueId == jobQueueId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(q => q.UploadFilename, filename)
                    .SetProperty(q => q.UploadChecksumSha256, checksumSha256)
                    .SetProperty(q => q.UploadVersion, uploadVersion)
                    .SetProperty(q => q.UploadValidatedAtUtc, validatedAtUtc)
                    .SetProperty(q => q.UploadRowCountsJson, rowCountsJson)
                    .SetProperty(q => q.UpdatedAt, DateTime.UtcNow), ct);

        public Task UpdateS3ObjectKeyAsync(
            Guid jobQueueId, string s3ObjectKey, CancellationToken ct = default) =>
            _dbContext.BatchJobQueues.IgnoreQueryFilters()
                .Where(q => q.JobqueueId == jobQueueId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(q => q.S3ObjectKey, s3ObjectKey)
                    .SetProperty(q => q.UpdatedAt, DateTime.UtcNow), ct);

        // ── Audit log ────────────────────────────────────────────────────────────
        // fps.job_queue_log via the existing BatchJobQueueLog entity (already reused as the
        // return type here even before this conversion — see GetJobQueueLogsAsync's original
        // comment, kept below).

        public async Task WriteJobQueueLogAsync(
            Guid jobQueueId, string note, string? actor, CancellationToken ct = default)
        {
            // Resolve current statusid (required by fps.job_queue_log FK constraint)
            var statusId = await _dbContext.BatchJobQueues.IgnoreQueryFilters()
                .Where(q => q.JobqueueId == jobQueueId)
                .Select(q => (int?)q.StatusId)
                .FirstOrDefaultAsync(ct);

            if (statusId is null)
            {
                _logger.LogWarning("WriteJobQueueLogAsync: jobqueueid {JobQueueId} not found; log entry skipped.", jobQueueId);
                return;
            }

            _dbContext.BatchJobQueueLogs.Add(new BatchJobQueueLog
            {
                JobqueueId = jobQueueId,
                StatusId = statusId.Value,
                // BatchJobQueueLog.PerformedBy is modeled non-nullable (shared with YearEnd) —
                // every real caller always passes a real actor (see BulkRatesRequestService),
                // so this only matters in the never-hit null case; empty string rather than a
                // literal DB NULL to keep the existing entity's own nullability contract intact.
                PerformedBy = actor ?? string.Empty,
                Note = note
            });
            await _dbContext.SaveChangesAsync(ct);
        }

        public async Task<IReadOnlyList<BatchJobQueueLog>> GetJobQueueLogsAsync(
            Guid jobQueueId, CancellationToken ct = default) =>
            await _dbContext.BatchJobQueueLogs
                .Where(l => l.JobqueueId == jobQueueId)
                .OrderBy(l => l.LogTime)
                .ToListAsync(ct);

        // ── Staging — replace semantics ──────────────────────────────────────────
        // fps.tblstaging* tables have no existing EF mapping. Kept raw rather than adding four
        // more entities for this pass — delete-then-bulk-insert within an explicit transaction,
        // with FK-ordering between the two FEC/AGRUP staging tables, is clearer as SQL than as
        // AddRange+SaveChanges across a mapping that doesn't exist yet anywhere else.

        public async Task ReplaceStagingFecAsync(
            Guid jobQueueId,
            IReadOnlyList<TestOrProductStagingRow> fecRows,
            IReadOnlyList<TestRequirementStagingRow> agrupRows,
            CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var tx = await conn.BeginTransactionAsync(ct);

            // Delete AGRUP first (child FK to FEC staging)
            await using (var del = conn.CreateCommand())
            {
                del.Transaction = tx;
                del.CommandText = "DELETE FROM fps.tblstagingtlkptestreqmt WHERE jobqueueid = @jqid;";
                del.Parameters.AddWithValue("jqid", jobQueueId);
                await del.ExecuteNonQueryAsync(ct);
            }
            await using (var del = conn.CreateCommand())
            {
                del.Transaction = tx;
                del.CommandText = "DELETE FROM fps.tblstagingtestorproduct WHERE jobqueueid = @jqid;";
                del.Parameters.AddWithValue("jqid", jobQueueId);
                await del.ExecuteNonQueryAsync(ct);
            }

            // Insert FEC rows
            foreach (var row in fecRows)
            {
                await using var ins = conn.CreateCommand();
                ins.Transaction = tx;
                ins.CommandText = @"
                    INSERT INTO fps.tblstagingtestorproduct
                        (jobqueueid, testcode, unitpricevla, defraunitprice, fecnewrate,
                         change, itemdescription, shortdescription, owner, comments)
                    VALUES
                        (@jobqueueid, @testcode, @unitpricevla, @defraunitprice, @fecnewrate,
                         @change, @itemdescription, @shortdescription, @owner, @comments);";
                ins.Parameters.AddWithValue("jobqueueid",       jobQueueId);
                ins.Parameters.AddWithValue("testcode",         row.TestCode);
                ins.Parameters.AddWithValue("unitpricevla",     (object?)row.UnitPriceVla ?? DBNull.Value);
                ins.Parameters.AddWithValue("defraunitprice",   (object?)row.DefraUnitPrice ?? DBNull.Value);
                ins.Parameters.AddWithValue("fecnewrate",       (object?)row.FecNewRate ?? DBNull.Value);
                ins.Parameters.AddWithValue("change",           (object?)row.Change ?? DBNull.Value);
                ins.Parameters.AddWithValue("itemdescription",  (object?)row.ItemDescription ?? DBNull.Value);
                ins.Parameters.AddWithValue("shortdescription", (object?)row.ShortDescription ?? DBNull.Value);
                ins.Parameters.AddWithValue("owner",            (object?)row.Owner ?? DBNull.Value);
                ins.Parameters.AddWithValue("comments",         (object?)row.Comments ?? DBNull.Value);
                await ins.ExecuteNonQueryAsync(ct);
            }

            // Insert AGRUP rows
            foreach (var row in agrupRows)
            {
                await using var ins = conn.CreateCommand();
                ins.Transaction = tx;
                ins.CommandText = @"
                    INSERT INTO fps.tblstagingtlkptestreqmt
                        (jobqueueid, testcode, buyer, agrup, agrupnew, change,
                         norequired, datecreated, active, comments,
                         projectbuyercode, testbuyercode, testbuyerworkgroup)
                    VALUES
                        (@jobqueueid, @testcode, @buyer, @agrup, @agrupnew, @change,
                         @norequired, @datecreated, @active, @comments,
                         @projectbuyercode, @testbuyercode, @testbuyerworkgroup);";
                ins.Parameters.AddWithValue("jobqueueid",  jobQueueId);
                ins.Parameters.AddWithValue("testcode",    row.TestCode);
                ins.Parameters.AddWithValue("buyer",       row.Buyer);
                ins.Parameters.AddWithValue("agrup",       (object?)row.Agrup ?? DBNull.Value);
                ins.Parameters.AddWithValue("agrupnew",    (object?)row.AgrupNew ?? DBNull.Value);
                ins.Parameters.AddWithValue("change",      (object?)row.Change ?? DBNull.Value);
                ins.Parameters.AddWithValue("norequired",  (object?)row.NoRequired ?? DBNull.Value);
                ins.Parameters.AddWithValue("datecreated", (object?)row.DateCreated ?? DBNull.Value);
                ins.Parameters.AddWithValue("active",      (object?)row.Active ?? DBNull.Value);
                ins.Parameters.AddWithValue("comments",    (object?)row.Comments ?? DBNull.Value);
                ins.Parameters.AddWithValue("projectbuyercode",   (object?)row.ProjectBuyerCode ?? DBNull.Value);
                ins.Parameters.AddWithValue("testbuyercode",      (object?)row.TestBuyerCode ?? DBNull.Value);
                ins.Parameters.AddWithValue("testbuyerworkgroup", (object?)row.TestBuyerWorkGroup ?? DBNull.Value);
                await ins.ExecuteNonQueryAsync(ct);
            }

            await tx.CommitAsync(ct);

            _logger.LogInformation(
                "ReplaceStagingFec | JobQueueId={JobQueueId} | FecRows={FecRows} | AgrupRows={AgrupRows}",
                jobQueueId, fecRows.Count, agrupRows.Count);
        }

        public async Task ReplaceStagingStaffAsync(
            Guid jobQueueId,
            IReadOnlyList<ProfitCentreGradeStagingRow> rows,
            CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var tx = await conn.BeginTransactionAsync(ct);

            await using (var del = conn.CreateCommand())
            {
                del.Transaction = tx;
                del.CommandText = "DELETE FROM fps.tblstagingprofitcentregrade WHERE jobqueueid = @jqid;";
                del.Parameters.AddWithValue("jqid", jobQueueId);
                await del.ExecuteNonQueryAsync(ct);
            }

            foreach (var row in rows)
            {
                await using var ins = conn.CreateCommand();
                ins.Transaction = tx;
                ins.CommandText = @"
                    INSERT INTO fps.tblstagingprofitcentregrade
                        (jobqueueid, pcgrade, payrate, npr, ohr)
                    VALUES
                        (@jobqueueid, @pcgrade, @payrate, @npr, @ohr);";
                ins.Parameters.AddWithValue("jobqueueid", jobQueueId);
                ins.Parameters.AddWithValue("pcgrade",    row.PcGrade);
                ins.Parameters.AddWithValue("payrate",    (object?)row.PayRate ?? DBNull.Value);
                ins.Parameters.AddWithValue("npr",        (object?)row.Npr ?? DBNull.Value);
                ins.Parameters.AddWithValue("ohr",        (object?)row.Ohr ?? DBNull.Value);
                await ins.ExecuteNonQueryAsync(ct);
            }

            await tx.CommitAsync(ct);

            _logger.LogInformation(
                "ReplaceStagingStaff | JobQueueId={JobQueueId} | Rows={Rows}",
                jobQueueId, rows.Count);
        }

        public async Task ReplaceStagingAnimalAsync(
            Guid jobQueueId,
            IReadOnlyList<AnimalStagingRow> rows,
            CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var tx = await conn.BeginTransactionAsync(ct);

            await using (var del = conn.CreateCommand())
            {
                del.Transaction = tx;
                del.CommandText = "DELETE FROM fps.tblstaginganimals WHERE jobqueueid = @jqid;";
                del.Parameters.AddWithValue("jqid", jobQueueId);
                await del.ExecuteNonQueryAsync(ct);
            }

            foreach (var row in rows)
            {
                await using var ins = conn.CreateCommand();
                ins.Transaction = tx;
                ins.CommandText = @"
                    INSERT INTO fps.tblstaginganimals
                        (jobqueueid, animaltype, species, security_level,
                         dailyrate, defradailyrate, planbyweek)
                    VALUES
                        (@jobqueueid, @animaltype, @species, @security_level,
                         @dailyrate, @defradailyrate, @planbyweek);";
                ins.Parameters.AddWithValue("jobqueueid",    jobQueueId);
                ins.Parameters.AddWithValue("animaltype",    row.AnimalType);
                ins.Parameters.AddWithValue("species",       (object?)row.Species ?? DBNull.Value);
                ins.Parameters.AddWithValue("security_level",(object?)row.SecurityLevel ?? DBNull.Value);
                ins.Parameters.AddWithValue("dailyrate",     (object?)row.DailyRate ?? DBNull.Value);
                ins.Parameters.AddWithValue("defradailyrate",(object?)row.DefraDailyRate ?? DBNull.Value);
                ins.Parameters.AddWithValue("planbyweek",    (object?)row.PlanByWeek ?? DBNull.Value);
                await ins.ExecuteNonQueryAsync(ct);
            }

            await tx.CommitAsync(ct);

            _logger.LogInformation(
                "ReplaceStagingAnimal | JobQueueId={JobQueueId} | Rows={Rows}",
                jobQueueId, rows.Count);
        }

        public async Task ClearStagingByJobQueueIdAsync(
            Guid jobQueueId, string jobName, CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var tx = await conn.BeginTransactionAsync(ct);

            // FEC/AGRUP: AGRUP first (child FK), then FEC
            await DeleteFromAsync(conn, tx, "fps.tblstagingtlkptestreqmt", jobQueueId, ct);
            await DeleteFromAsync(conn, tx, "fps.tblstagingtestorproduct", jobQueueId, ct);
            await DeleteFromAsync(conn, tx, "fps.tblstagingprofitcentregrade", jobQueueId, ct);
            await DeleteFromAsync(conn, tx, "fps.tblstaginganimals", jobQueueId, ct);

            await tx.CommitAsync(ct);

            _logger.LogInformation("ClearStagingByJobQueueId | JobQueueId={JobQueueId} | JobName={JobName}",
                jobQueueId, jobName);
        }

        // ── Staging read ─────────────────────────────────────────────────────────

        public async Task<IReadOnlyList<TestOrProductStagingRow>> GetTestOrProductStagingRowsAsync(
            Guid jobQueueId, CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT jobqueueid, testcode, unitpricevla::numeric, defraunitprice::numeric,
                       fecnewrate::numeric, change::numeric,
                       itemdescription, shortdescription, owner, comments,
                       calculated_action, effective_new_rate::numeric, source_current_rate::numeric,
                       validation_version
                FROM fps.tblstagingtestorproduct
                WHERE jobqueueid = @jobqueueid
                ORDER BY testcode;";
            cmd.Parameters.AddWithValue("jobqueueid", jobQueueId);

            var rows = new List<TestOrProductStagingRow>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
                rows.Add(MapFecStagingRow(reader));
            return rows;
        }

        public async Task<IReadOnlyList<TestRequirementStagingRow>> GetTestRequirementStagingRowsAsync(
            Guid jobQueueId, CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT jobqueueid, testcode, buyer,
                       agrup::numeric, agrupnew::numeric, change::numeric,
                       norequired, datecreated, active, comments,
                       projectbuyercode, testbuyercode, testbuyerworkgroup,
                       calculated_action, effective_new_rate::numeric, source_current_rate::numeric,
                       validation_version
                FROM fps.tblstagingtlkptestreqmt
                WHERE jobqueueid = @jobqueueid
                ORDER BY testcode, buyer;";
            cmd.Parameters.AddWithValue("jobqueueid", jobQueueId);

            var rows = new List<TestRequirementStagingRow>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
                rows.Add(MapAgrupStagingRow(reader));
            return rows;
        }

        public async Task<IReadOnlyList<ProfitCentreGradeStagingRow>> GetProfitCentreGradeStagingRowsAsync(
            Guid jobQueueId, CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT jobqueueid, pcgrade,
                       payrate::numeric, npr::numeric, ohr::numeric
                FROM fps.tblstagingprofitcentregrade
                WHERE jobqueueid = @jobqueueid;";
            cmd.Parameters.AddWithValue("jobqueueid", jobQueueId);

            var rows = new List<ProfitCentreGradeStagingRow>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                rows.Add(new ProfitCentreGradeStagingRow
                {
                    JobQueueId = reader.GetGuid(0),
                    PcGrade    = reader.GetString(1),
                    PayRate    = reader.IsDBNull(2) ? null : reader.GetDecimal(2),
                    Npr        = reader.IsDBNull(3) ? null : reader.GetDecimal(3),
                    Ohr        = reader.IsDBNull(4) ? null : reader.GetDecimal(4)
                });
            }
            return rows;
        }

        public async Task<IReadOnlyList<AnimalStagingRow>> GetAnimalStagingRowsAsync(
            Guid jobQueueId, CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT jobqueueid, animaltype, species, security_level,
                       dailyrate::numeric, defradailyrate::numeric, planbyweek
                FROM fps.tblstaginganimals
                WHERE jobqueueid = @jobqueueid;";
            cmd.Parameters.AddWithValue("jobqueueid", jobQueueId);

            var rows = new List<AnimalStagingRow>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                rows.Add(new AnimalStagingRow
                {
                    JobQueueId    = reader.GetGuid(0),
                    AnimalType    = reader.GetString(1),
                    Species       = reader.IsDBNull(2) ? null : reader.GetString(2),
                    SecurityLevel = reader.IsDBNull(3) ? null : reader.GetString(3),
                    DailyRate     = reader.IsDBNull(4) ? null : reader.GetDecimal(4),
                    DefraDailyRate = reader.IsDBNull(5) ? null : reader.GetDecimal(5),
                    PlanByWeek    = reader.IsDBNull(6) ? null : reader.GetBoolean(6)
                });
            }
            return rows;
        }

        // ── Validation errors ────────────────────────────────────────────────────
        // fps.staging_validation_error has no existing EF mapping — same delete-then-bulk-
        // insert rationale as the staging block above.

        public async Task ReplaceValidationErrorsAsync(
            Guid jobQueueId,
            IReadOnlyList<StagingValidationError> errors,
            CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var tx = await conn.BeginTransactionAsync(ct);

            await using (var del = conn.CreateCommand())
            {
                del.Transaction = tx;
                del.CommandText = "DELETE FROM fps.staging_validation_error WHERE jobqueueid = @jqid;";
                del.Parameters.AddWithValue("jqid", jobQueueId);
                await del.ExecuteNonQueryAsync(ct);
            }

            foreach (var err in errors)
            {
                await using var ins = conn.CreateCommand();
                ins.Transaction = tx;
                ins.CommandText = @"
                    INSERT INTO fps.staging_validation_error
                        (jobqueueid, upload_version, sourcerownumber, fieldname,
                         validationcode, severity, validationmessage,
                         sheetname, testcode, buyer, currentvalue, expectedvalue, is_request_level)
                    VALUES
                        (@jobqueueid, @uploadversion, @sourcerownumber, @fieldname,
                         @validationcode, @severity, @validationmessage,
                         @sheetname, @testcode, @buyer, @currentvalue, @expectedvalue, @isrequestlevel);";
                ins.Parameters.AddWithValue("jobqueueid",       jobQueueId);
                ins.Parameters.AddWithValue("uploadversion",    err.UploadVersion);
                ins.Parameters.AddWithValue("sourcerownumber",  err.SourceRowNumber);
                ins.Parameters.AddWithValue("fieldname",        (object?)err.FieldName ?? DBNull.Value);
                ins.Parameters.AddWithValue("validationcode",   (object?)err.ValidationCode ?? DBNull.Value);
                ins.Parameters.AddWithValue("severity",         err.Severity);
                ins.Parameters.AddWithValue("validationmessage",err.ValidationMessage);
                ins.Parameters.AddWithValue("sheetname",        (object?)err.SheetName ?? DBNull.Value);
                ins.Parameters.AddWithValue("testcode",         (object?)err.TestCode ?? DBNull.Value);
                ins.Parameters.AddWithValue("buyer",            (object?)err.Buyer ?? DBNull.Value);
                ins.Parameters.AddWithValue("currentvalue",     (object?)err.CurrentValue ?? DBNull.Value);
                ins.Parameters.AddWithValue("expectedvalue",    (object?)err.ExpectedValue ?? DBNull.Value);
                ins.Parameters.AddWithValue("isrequestlevel",   err.IsRequestLevel);
                await ins.ExecuteNonQueryAsync(ct);
            }

            await tx.CommitAsync(ct);

            _logger.LogInformation(
                "ReplaceValidationErrors | JobQueueId={JobQueueId} | Errors={Errors}",
                jobQueueId, errors.Count);
        }

        public async Task<IReadOnlyList<StagingValidationError>> GetValidationErrorsAsync(
            Guid jobQueueId, CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT validationerrorid, jobqueueid, upload_version, sourcerownumber, fieldname,
                       validationcode, severity, validationmessage,
                       sheetname, testcode, buyer, currentvalue, expectedvalue, is_request_level
                FROM fps.staging_validation_error
                WHERE jobqueueid = @jobqueueid
                ORDER BY sourcerownumber, validationerrorid;";
            cmd.Parameters.AddWithValue("jobqueueid", jobQueueId);

            var results = new List<StagingValidationError>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                results.Add(new StagingValidationError
                {
                    Id                = reader.GetInt64(0),
                    JobQueueId        = reader.GetGuid(1),
                    UploadVersion     = reader.GetInt32(2),
                    SourceRowNumber   = reader.GetInt32(3),
                    FieldName         = reader.IsDBNull(4) ? null : reader.GetString(4),
                    ValidationCode    = reader.IsDBNull(5) ? null : reader.GetString(5),
                    Severity          = reader.GetString(6),
                    ValidationMessage = reader.GetString(7),
                    SheetName         = reader.IsDBNull(8) ? null : reader.GetString(8),
                    TestCode          = reader.IsDBNull(9) ? null : reader.GetString(9),
                    Buyer             = reader.IsDBNull(10) ? null : reader.GetString(10),
                    CurrentValue      = reader.IsDBNull(11) ? null : reader.GetString(11),
                    ExpectedValue     = reader.IsDBNull(12) ? null : reader.GetString(12),
                    IsRequestLevel    = reader.GetBoolean(13)
                });
            }
            return results;
        }

        // ── Cancel + clear staging (atomic) ──────────────────────────────────────
        // Kept fully raw for the same reason as MarkDownloadReadyAsync: this transaction spans
        // job_queue (mapped) and four unmapped staging/validation tables in one atomic unit —
        // splitting just the job_queue update into ExecuteUpdateAsync would mix styles within
        // a single method for no real benefit, since it isn't a guarded/concurrency-sensitive
        // update the way TransitionStatusAsync/MarkDownloadReadyAsync are.

        public async Task CancelAndClearStagingAsync(
            Guid jobQueueId, string jobName,
            string cancelledBy, DateTime cancelledAtUtc,
            string? reason, int cancelledStatusId,
            CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var tx = await conn.BeginTransactionAsync(ct);

            // Update job_queue with cancellation metadata
            await using (var upd = conn.CreateCommand())
            {
                upd.Transaction = tx;
                upd.CommandText = @"
                    UPDATE fps.job_queue
                    SET statusid            = @statusid,
                        cancelled_by        = @cancelled_by,
                        cancelled_at_utc    = @cancelled_at_utc,
                        cancellation_reason = @cancellation_reason,
                        updated_at          = NOW()
                    WHERE jobqueueid = @jobqueueid;";
                upd.Parameters.AddWithValue("statusid",            cancelledStatusId);
                upd.Parameters.AddWithValue("cancelled_by",        cancelledBy);
                upd.Parameters.AddWithValue("cancelled_at_utc",    cancelledAtUtc);
                upd.Parameters.AddWithValue("cancellation_reason", (object?)reason ?? DBNull.Value);
                upd.Parameters.AddWithValue("jobqueueid",          jobQueueId);
                await upd.ExecuteNonQueryAsync(ct);
            }

            // Clear all staging rows within the same transaction
            await DeleteFromAsync(conn, tx, "fps.tblstagingtlkptestreqmt",   jobQueueId, ct);
            await DeleteFromAsync(conn, tx, "fps.tblstagingtestorproduct",    jobQueueId, ct);
            await DeleteFromAsync(conn, tx, "fps.tblstagingprofitcentregrade", jobQueueId, ct);
            await DeleteFromAsync(conn, tx, "fps.tblstaginganimals",          jobQueueId, ct);

            // Clear validation errors within the same transaction
            await using (var del = conn.CreateCommand())
            {
                del.Transaction = tx;
                del.CommandText = "DELETE FROM fps.staging_validation_error WHERE jobqueueid = @jqid;";
                del.Parameters.AddWithValue("jqid", jobQueueId);
                await del.ExecuteNonQueryAsync(ct);
            }

            await tx.CommitAsync(ct);

            _logger.LogInformation(
                "CancelAndClearStaging committed | JobQueueId={JobQueueId} | CancelledBy={CancelledBy}",
                jobQueueId, cancelledBy);
        }

        // ── Reference checks ─────────────────────────────────────────────────────

        // fps.tblyearmaster is already EF-mapped (YearMaster/YearMasters, no query filter) —
        // an ordinary single-value lookup, natural LINQ.
        public Task<string?> GetFpsYearStatusAsync(int fpsYear, CancellationToken ct = default) =>
            _dbContext.YearMasters
                .Where(y => y.FpsYear == fpsYear && y.Active)
                .Select(y => (string?)y.YearStatus)
                .FirstOrDefaultAsync(ct);

        // Raw SQL retained: fps.tlkpproject is already EF-mapped (Project/Projects), but with a
        // FpsYear query filter tied to the ambient IFpsRequestContext year
        // (HasQueryFilter(e => e.FpsYear == FilterFpsYear)) — this method takes fpsYear as an
        // explicit parameter that is not guaranteed to equal whatever year the request context
        // currently holds, so reusing that DbSet would risk silently mis-scoping results
        // whenever the two diverge. Not worth an .IgnoreQueryFilters() escape hatch on a
        // shared, actively-filtered entity for a bulk existence check this simple.
        public async Task<IReadOnlySet<string>> GetExistingProjectCodesAsync(
            IEnumerable<string> parentProjectCodes, int fpsYear, CancellationToken ct = default)
        {
            var codeList = parentProjectCodes.ToList();
            if (codeList.Count == 0)
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT parentproject FROM fps.tlkpproject
                WHERE fpsyear = @fpsyear AND parentproject = ANY(@codes);";
            cmd.Parameters.AddWithValue("fpsyear", fpsYear);
            cmd.Parameters.AddWithValue("codes", codeList.ToArray());

            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
                result.Add(reader.GetString(0));

            return result;
        }

        // Raw SQL retained: a paired-array UNNEST join (testcode+workgroup as a matched pair,
        // not two independent IN-lists) has no natural LINQ/EF translation — this is exactly
        // the PostgreSQL-specific-semantics case, not a style choice.
        public async Task<IReadOnlySet<(string TestCode, string WorkGroup)>> GetExistingCapabilityPairsAsync(
            IEnumerable<(string TestCode, string WorkGroup)> pairs, int fpsYear, CancellationToken ct = default)
        {
            var pairList = pairs.ToList();
            if (pairList.Count == 0)
                return new HashSet<(string, string)>();

            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();

            // Two real columns (testcode, workgroup) — never a concatenated string.
            cmd.CommandText = @"
                SELECT c.testcode, c.workgroup
                FROM fps.tlkptestcapability c
                JOIN unnest(@testcodes::text[], @workgroups::text[]) AS v(tc, wg)
                  ON c.testcode = v.tc AND c.workgroup = v.wg
                WHERE c.fpsyear = @fpsyear;";
            cmd.Parameters.AddWithValue("fpsyear", fpsYear);
            cmd.Parameters.AddWithValue("testcodes", pairList.Select(p => p.TestCode).ToArray());
            cmd.Parameters.AddWithValue("workgroups", pairList.Select(p => p.WorkGroup).ToArray());

            var result = new HashSet<(string, string)>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
                result.Add((reader.GetString(0), reader.GetString(1)));

            return result;
        }

        // ── Download snapshot ─────────────────────────────────────────────────────
        // fps.bulk_rates_download / bulk_rates_downloaded_key / *_download_detail have no
        // existing EF mapping; kept raw rather than introducing four more entities for this
        // pass (see tracker for scope notes).

        public async Task<int> GetNextDownloadVersionAsync(Guid jobQueueId, CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT COALESCE(MAX(download_version), 0) + 1
                FROM fps.bulk_rates_download
                WHERE jobqueueid = @jobqueueid;";
            cmd.Parameters.AddWithValue("jobqueueid", jobQueueId);
            return (int)(await cmd.ExecuteScalarAsync(ct))!;
        }

        public async Task CreateDownloadSnapshotAsync(
            Guid jobQueueId, int downloadVersion,
            IReadOnlyList<TestOrProductStagingRow> fecRows, IReadOnlyList<TestRequirementStagingRow> agrupRows,
            CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var tx = await conn.BeginTransactionAsync(ct);

            await using (var ins = conn.CreateCommand())
            {
                ins.Transaction = tx;
                ins.CommandText = @"
                    INSERT INTO fps.bulk_rates_download (jobqueueid, download_version, status)
                    VALUES (@jobqueueid, @downloadversion, 'Generating');";
                ins.Parameters.AddWithValue("jobqueueid", jobQueueId);
                ins.Parameters.AddWithValue("downloadversion", downloadVersion);
                await ins.ExecuteNonQueryAsync(ct);
            }

            // source_rate carries defraunitprice for FEC rows, unitprice for AGRUP rows —
            // the single "current rate" value ValidationContext.FrozenSnapshot
            // reads (reconciliation §2.6). unitpricevla/norequired/datecreated/
            // active/itemdescription/shortdescription/owner exist only to let the
            // workbook be regenerated from the snapshot without a second live query.
            foreach (var row in fecRows)
            {
                await using var ins = conn.CreateCommand();
                ins.Transaction = tx;
                ins.CommandText = @"
                    INSERT INTO fps.bulk_rates_downloaded_key
                        (jobqueueid, download_version, sheetname, testcode, source_rate,
                         unitpricevla, itemdescription, shortdescription, owner)
                    VALUES
                        (@jobqueueid, @downloadversion, 'FEC', @testcode, @sourcerate,
                         @unitpricevla, @itemdescription, @shortdescription, @owner);";
                ins.Parameters.AddWithValue("jobqueueid", jobQueueId);
                ins.Parameters.AddWithValue("downloadversion", downloadVersion);
                ins.Parameters.AddWithValue("testcode", row.TestCode);
                ins.Parameters.AddWithValue("sourcerate", (object?)row.DefraUnitPrice ?? DBNull.Value);
                ins.Parameters.AddWithValue("unitpricevla", (object?)row.UnitPriceVla ?? DBNull.Value);
                ins.Parameters.AddWithValue("itemdescription", (object?)row.ItemDescription ?? DBNull.Value);
                ins.Parameters.AddWithValue("shortdescription", (object?)row.ShortDescription ?? DBNull.Value);
                ins.Parameters.AddWithValue("owner", (object?)row.Owner ?? DBNull.Value);
                await ins.ExecuteNonQueryAsync(ct);
            }

            foreach (var row in agrupRows)
            {
                await using var ins = conn.CreateCommand();
                ins.Transaction = tx;
                ins.CommandText = @"
                    INSERT INTO fps.bulk_rates_downloaded_key
                        (jobqueueid, download_version, sheetname, testcode, buyer, source_rate,
                         norequired, datecreated, active, projectbuyercode, testbuyercode)
                    VALUES
                        (@jobqueueid, @downloadversion, 'AGRUP', @testcode, @buyer, @sourcerate,
                         @norequired, @datecreated, @active, @projectbuyercode, @testbuyercode);";
                ins.Parameters.AddWithValue("jobqueueid", jobQueueId);
                ins.Parameters.AddWithValue("downloadversion", downloadVersion);
                ins.Parameters.AddWithValue("testcode", row.TestCode);
                ins.Parameters.AddWithValue("buyer", row.Buyer);
                ins.Parameters.AddWithValue("sourcerate", (object?)row.Agrup ?? DBNull.Value);
                ins.Parameters.AddWithValue("norequired", (object?)row.NoRequired ?? DBNull.Value);
                ins.Parameters.AddWithValue("datecreated", (object?)row.DateCreated ?? DBNull.Value);
                ins.Parameters.AddWithValue("active", row.Active.HasValue ? (object)(row.Active.Value != 0) : DBNull.Value);
                ins.Parameters.AddWithValue("projectbuyercode", (object?)row.ProjectBuyerCode ?? DBNull.Value);
                ins.Parameters.AddWithValue("testbuyercode", (object?)row.TestBuyerCode ?? DBNull.Value);
                await ins.ExecuteNonQueryAsync(ct);
            }

            await tx.CommitAsync(ct);

            _logger.LogInformation(
                "CreateDownloadSnapshot | JobQueueId={JobQueueId} | DownloadVersion={DownloadVersion} | FecRows={FecRows} | AgrupRows={AgrupRows}",
                jobQueueId, downloadVersion, fecRows.Count, agrupRows.Count);
        }

        // Kept fully raw, deliberately not split into a mixed EF/SQL method: this transaction
        // touches bulk_rates_download (unmapped) and job_queue (mapped via BatchJobQueue)
        // in the same atomic unit. Converting only the second statement to ExecuteUpdateAsync
        // while leaving the first as raw SQL would be exactly the "mixed styles within one
        // method" outcome to avoid — and the acceptance bar here is strict (the guarded
        // active_download_version UPDATE must stay a single atomic UPDATE...WHERE either way,
        // which raw SQL already gives it untouched). See BulkRatesRepositoryDownloadConcurrencyTests
        // for the test proving the guard.
        public async Task MarkDownloadReadyAsync(Guid jobQueueId, int downloadVersion, CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var tx = await conn.BeginTransactionAsync(ct);

            await using (var upd = conn.CreateCommand())
            {
                upd.Transaction = tx;
                upd.CommandText = @"
                    UPDATE fps.bulk_rates_download
                    SET status = 'Ready', ready_at_utc = now()
                    WHERE jobqueueid = @jobqueueid AND download_version = @downloadversion;";
                upd.Parameters.AddWithValue("jobqueueid", jobQueueId);
                upd.Parameters.AddWithValue("downloadversion", downloadVersion);
                await upd.ExecuteNonQueryAsync(ct);
            }

            // Guard against an older Generating download
            // finishing after a newer one has already activated — the WHERE clause is the
            // concurrency-safety mechanism ensuring active_download_version can never be
            // overwritten back to itself. A late-finishing older
            // version still marks its own header row Ready above (an accurate historical
            // record of that version), it just never regresses the active pointer. Shared by
            // FEC/AGRUP and Staff/Animal alike — all three call this same method.
            await using (var upd = conn.CreateCommand())
            {
                upd.Transaction = tx;
                upd.CommandText = @"
                    UPDATE fps.job_queue
                    SET active_download_version = @downloadversion
                    WHERE jobqueueid = @jobqueueid
                      AND (active_download_version IS NULL OR active_download_version < @downloadversion);";
                upd.Parameters.AddWithValue("jobqueueid", jobQueueId);
                upd.Parameters.AddWithValue("downloadversion", downloadVersion);
                await upd.ExecuteNonQueryAsync(ct);
            }

            await tx.CommitAsync(ct);
        }

        public async Task MarkDownloadFailedAsync(Guid jobQueueId, int downloadVersion, CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE fps.bulk_rates_download
                SET status = 'Failed'
                WHERE jobqueueid = @jobqueueid AND download_version = @downloadversion AND status = 'Generating';";
            cmd.Parameters.AddWithValue("jobqueueid", jobQueueId);
            cmd.Parameters.AddWithValue("downloadversion", downloadVersion);
            await cmd.ExecuteNonQueryAsync(ct);
        }

        public async Task<IReadOnlyList<TestOrProductStagingRow>> GetFecSnapshotRowsAsync(
            Guid jobQueueId, int downloadVersion, CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT testcode, unitpricevla, source_rate, itemdescription, shortdescription, owner,
                       source_rate AS fecnewrate
                FROM fps.bulk_rates_downloaded_key
                WHERE jobqueueid = @jobqueueid AND download_version = @downloadversion AND sheetname = 'FEC'
                ORDER BY id;";
            cmd.Parameters.AddWithValue("jobqueueid", jobQueueId);
            cmd.Parameters.AddWithValue("downloadversion", downloadVersion);

            var result = new List<TestOrProductStagingRow>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                result.Add(new TestOrProductStagingRow
                {
                    JobQueueId = jobQueueId,
                    TestCode = reader.GetString(0),
                    UnitPriceVla = reader.IsDBNull(1) ? null : reader.GetDecimal(1),
                    DefraUnitPrice = reader.IsDBNull(2) ? null : reader.GetDecimal(2),
                    ItemDescription = reader.IsDBNull(3) ? null : reader.GetString(3),
                    ShortDescription = reader.IsDBNull(4) ? null : reader.GetString(4),
                    Owner = reader.IsDBNull(5) ? null : reader.GetString(5),
                    FecNewRate = reader.IsDBNull(6) ? null : reader.GetDecimal(6),
                });
            }
            return result;
        }

        public async Task<IReadOnlyList<TestRequirementStagingRow>> GetAgrupSnapshotRowsAsync(
            Guid jobQueueId, int downloadVersion, CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT testcode, buyer, source_rate, norequired, datecreated, active,
                       projectbuyercode, testbuyercode, source_rate AS agrupnew
                FROM fps.bulk_rates_downloaded_key
                WHERE jobqueueid = @jobqueueid AND download_version = @downloadversion AND sheetname = 'AGRUP'
                ORDER BY id;";
            cmd.Parameters.AddWithValue("jobqueueid", jobQueueId);
            cmd.Parameters.AddWithValue("downloadversion", downloadVersion);

            var result = new List<TestRequirementStagingRow>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                result.Add(new TestRequirementStagingRow
                {
                    JobQueueId = jobQueueId,
                    TestCode = reader.GetString(0),
                    Buyer = reader.GetString(1),
                    Agrup = reader.IsDBNull(2) ? null : reader.GetDecimal(2),
                    NoRequired = reader.IsDBNull(3) ? null : reader.GetDouble(3),
                    DateCreated = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                    Active = reader.IsDBNull(5) ? null : (short?)(reader.GetBoolean(5) ? 1 : 0),
                    ProjectBuyerCode = reader.IsDBNull(6) ? null : reader.GetString(6),
                    TestBuyerCode = reader.IsDBNull(7) ? null : reader.GetString(7),
                    AgrupNew = reader.IsDBNull(8) ? null : reader.GetDecimal(8),
                });
            }
            return result;
        }

        // ── Download snapshot — Staff/Animal ──────────────────────────────────────

        public async Task CreateStaffDownloadSnapshotAsync(
            Guid jobQueueId, int downloadVersion,
            IReadOnlyList<ProfitCentreGradeStagingRow> rows,
            CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var tx = await conn.BeginTransactionAsync(ct);

            await using (var ins = conn.CreateCommand())
            {
                ins.Transaction = tx;
                ins.CommandText = @"
                    INSERT INTO fps.bulk_rates_download (jobqueueid, download_version, status)
                    VALUES (@jobqueueid, @downloadversion, 'Generating');";
                ins.Parameters.AddWithValue("jobqueueid", jobQueueId);
                ins.Parameters.AddWithValue("downloadversion", downloadVersion);
                await ins.ExecuteNonQueryAsync(ct);
            }

            foreach (var row in rows)
            {
                await using var ins = conn.CreateCommand();
                ins.Transaction = tx;
                ins.CommandText = @"
                    INSERT INTO fps.bulk_rates_staff_download_detail
                        (jobqueueid, download_version, pcgrade, source_payrate, source_npr, source_ohr)
                    VALUES
                        (@jobqueueid, @downloadversion, @pcgrade, @sourcepayrate, @sourcenpr, @sourceohr);";
                ins.Parameters.AddWithValue("jobqueueid", jobQueueId);
                ins.Parameters.AddWithValue("downloadversion", downloadVersion);
                ins.Parameters.AddWithValue("pcgrade", row.PcGrade);
                ins.Parameters.AddWithValue("sourcepayrate", (object?)row.PayRate ?? DBNull.Value);
                ins.Parameters.AddWithValue("sourcenpr", (object?)row.Npr ?? DBNull.Value);
                ins.Parameters.AddWithValue("sourceohr", (object?)row.Ohr ?? DBNull.Value);
                await ins.ExecuteNonQueryAsync(ct);
            }

            await tx.CommitAsync(ct);

            _logger.LogInformation(
                "CreateStaffDownloadSnapshot | JobQueueId={JobQueueId} | DownloadVersion={DownloadVersion} | StaffRows={StaffRows}",
                jobQueueId, downloadVersion, rows.Count);
        }

        public async Task<IReadOnlyList<ProfitCentreGradeStagingRow>> GetStaffSnapshotRowsAsync(
            Guid jobQueueId, int downloadVersion, CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT pcgrade, source_payrate, source_npr, source_ohr
                FROM fps.bulk_rates_staff_download_detail
                WHERE jobqueueid = @jobqueueid AND download_version = @downloadversion
                ORDER BY id;";
            cmd.Parameters.AddWithValue("jobqueueid", jobQueueId);
            cmd.Parameters.AddWithValue("downloadversion", downloadVersion);

            var result = new List<ProfitCentreGradeStagingRow>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                result.Add(new ProfitCentreGradeStagingRow
                {
                    JobQueueId = jobQueueId,
                    PcGrade    = reader.GetString(0),
                    PayRate    = reader.IsDBNull(1) ? null : reader.GetDecimal(1),
                    Npr        = reader.IsDBNull(2) ? null : reader.GetDecimal(2),
                    Ohr        = reader.IsDBNull(3) ? null : reader.GetDecimal(3)
                });
            }
            return result;
        }

        public async Task CreateAnimalDownloadSnapshotAsync(
            Guid jobQueueId, int downloadVersion,
            IReadOnlyList<AnimalStagingRow> rows,
            CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var tx = await conn.BeginTransactionAsync(ct);

            await using (var ins = conn.CreateCommand())
            {
                ins.Transaction = tx;
                ins.CommandText = @"
                    INSERT INTO fps.bulk_rates_download (jobqueueid, download_version, status)
                    VALUES (@jobqueueid, @downloadversion, 'Generating');";
                ins.Parameters.AddWithValue("jobqueueid", jobQueueId);
                ins.Parameters.AddWithValue("downloadversion", downloadVersion);
                await ins.ExecuteNonQueryAsync(ct);
            }

            foreach (var row in rows)
            {
                await using var ins = conn.CreateCommand();
                ins.Transaction = tx;
                ins.CommandText = @"
                    INSERT INTO fps.bulk_rates_animal_download_detail
                        (jobqueueid, download_version, animaltype, source_dailyrate,
                         source_defradailyrate, source_planbyweek, source_species, source_securitylevel)
                    VALUES
                        (@jobqueueid, @downloadversion, @animaltype, @sourcedailyrate,
                         @sourcedefradailyrate, @sourceplanbyweek, @sourcespecies, @sourcesecuritylevel);";
                ins.Parameters.AddWithValue("jobqueueid", jobQueueId);
                ins.Parameters.AddWithValue("downloadversion", downloadVersion);
                ins.Parameters.AddWithValue("animaltype", row.AnimalType);
                ins.Parameters.AddWithValue("sourcedailyrate", (object?)row.DailyRate ?? DBNull.Value);
                ins.Parameters.AddWithValue("sourcedefradailyrate", (object?)row.DefraDailyRate ?? DBNull.Value);
                ins.Parameters.AddWithValue("sourceplanbyweek", (object?)row.PlanByWeek ?? DBNull.Value);
                ins.Parameters.AddWithValue("sourcespecies", (object?)row.Species ?? DBNull.Value);
                ins.Parameters.AddWithValue("sourcesecuritylevel", (object?)row.SecurityLevel ?? DBNull.Value);
                await ins.ExecuteNonQueryAsync(ct);
            }

            await tx.CommitAsync(ct);

            _logger.LogInformation(
                "CreateAnimalDownloadSnapshot | JobQueueId={JobQueueId} | DownloadVersion={DownloadVersion} | AnimalRows={AnimalRows}",
                jobQueueId, downloadVersion, rows.Count);
        }

        public async Task<IReadOnlyList<AnimalStagingRow>> GetAnimalSnapshotRowsAsync(
            Guid jobQueueId, int downloadVersion, CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT animaltype, source_species, source_securitylevel, source_dailyrate,
                       source_defradailyrate, source_planbyweek
                FROM fps.bulk_rates_animal_download_detail
                WHERE jobqueueid = @jobqueueid AND download_version = @downloadversion
                ORDER BY id;";
            cmd.Parameters.AddWithValue("jobqueueid", jobQueueId);
            cmd.Parameters.AddWithValue("downloadversion", downloadVersion);

            var result = new List<AnimalStagingRow>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                result.Add(new AnimalStagingRow
                {
                    JobQueueId     = jobQueueId,
                    AnimalType     = reader.GetString(0),
                    Species        = reader.IsDBNull(1) ? null : reader.GetString(1),
                    SecurityLevel  = reader.IsDBNull(2) ? null : reader.GetString(2),
                    DailyRate      = reader.IsDBNull(3) ? null : reader.GetDecimal(3),
                    DefraDailyRate = reader.IsDBNull(4) ? null : reader.GetDecimal(4),
                    PlanByWeek     = reader.IsDBNull(5) ? null : reader.GetBoolean(5)
                });
            }
            return result;
        }

        // ── Export: live table reads ──────────────────────────────────────────────
        // fps.testorproduct/tlkptestreqmt/profitcentregrade already have EF entities elsewhere
        // (TestOrProduct, ProfitCentreGrade, ...), but each carries the same ambient FpsYear
        // query-filter risk as GetExistingProjectCodesAsync above — these methods take fpsYear
        // as an explicit parameter, not necessarily the ambient request-context year. Kept raw
        // rather than routing around another feature's filtered entity.

        public async Task<IReadOnlyList<TestOrProductStagingRow>> GetFecRowsForExportAsync(
            int fpsYear, CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            // fecnewrate is pre-populated with the current Defra Unit Price (not left NULL) so a
            // downloaded workbook shows "FEC New" already carrying the current rate for every
            // row — an untouched row then classifies as No Change rather than the existing-row
            // blank/zero rule (Zero-Rate Withdrawal), and the Change formula the caller writes
            // over this data starts from a real number instead of blank.
            cmd.CommandText = @"
                SELECT itemcode, unitpricevla::numeric, defraunitprice::numeric,
                       defraunitprice::numeric AS fecnewrate, NULL::numeric AS change,
                       itemdescription, shortdescription, owner, NULL::text AS comments
                FROM fps.testorproduct
                WHERE fpsyear = @fpsyear
                ORDER BY itemcode;";
            cmd.Parameters.AddWithValue("fpsyear", fpsYear);

            var rows = new List<TestOrProductStagingRow>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                rows.Add(new TestOrProductStagingRow
                {
                    JobQueueId       = Guid.Empty,
                    TestCode         = reader.GetString(0),
                    UnitPriceVla     = reader.IsDBNull(1) ? null : reader.GetDecimal(1),
                    DefraUnitPrice   = reader.IsDBNull(2) ? null : reader.GetDecimal(2),
                    FecNewRate       = reader.IsDBNull(3) ? null : reader.GetDecimal(3),
                    Change           = null,
                    ItemDescription  = reader.IsDBNull(5) ? null : reader.GetString(5),
                    ShortDescription = reader.IsDBNull(6) ? null : reader.GetString(6),
                    Owner            = reader.IsDBNull(7) ? null : reader.GetString(7),
                    Comments         = reader.IsDBNull(8) ? null : reader.GetString(8)
                });
            }
            return rows;
        }

        public async Task<IReadOnlyList<TestRequirementStagingRow>> GetAgrupRowsForExportAsync(
            int fpsYear, CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            // agrupnew is pre-populated with the current unit price (not left NULL) — same
            // rationale as fecnewrate above.
            cmd.CommandText = @"
                SELECT testcode, buyer,
                       unitprice::numeric AS agrup, unitprice::numeric AS agrupnew, NULL::numeric AS change,
                       norequired, datecreated, active, NULL::text AS comments,
                       projectbuyercode, testbuyercode
                FROM fps.tlkptestreqmt
                WHERE fpsyear = @fpsyear
                ORDER BY testcode, buyer;";
            cmd.Parameters.AddWithValue("fpsyear", fpsYear);

            var rows = new List<TestRequirementStagingRow>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                rows.Add(new TestRequirementStagingRow
                {
                    JobQueueId  = Guid.Empty,
                    TestCode    = reader.GetString(0),
                    Buyer       = reader.GetString(1),
                    Agrup       = reader.IsDBNull(2) ? null : reader.GetDecimal(2),
                    AgrupNew    = reader.IsDBNull(3) ? null : reader.GetDecimal(3),
                    Change      = null,
                    NoRequired  = reader.IsDBNull(5) ? null : reader.GetDouble(5),
                    DateCreated = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
                    Active      = reader.IsDBNull(7) ? null : reader.GetInt16(7),
                    Comments    = reader.IsDBNull(8) ? null : reader.GetString(8),
                    ProjectBuyerCode = reader.IsDBNull(9) ? null : reader.GetString(9),
                    TestBuyerCode    = reader.IsDBNull(10) ? null : reader.GetString(10)
                });
            }
            return rows;
        }

        public async Task<IReadOnlyList<ProfitCentreGradeStagingRow>> GetStaffRowsForExportAsync(
            int fpsYear, CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT pcgrade, payrate::numeric, npr::numeric, ohr::numeric
                FROM fps.profitcentregrade
                WHERE fpsyear = @fpsyear
                ORDER BY pcgrade;";
            cmd.Parameters.AddWithValue("fpsyear", fpsYear);

            var rows = new List<ProfitCentreGradeStagingRow>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                rows.Add(new ProfitCentreGradeStagingRow
                {
                    JobQueueId = Guid.Empty,
                    PcGrade    = reader.GetString(0),
                    PayRate    = reader.IsDBNull(1) ? null : reader.GetDecimal(1),
                    Npr        = reader.IsDBNull(2) ? null : reader.GetDecimal(2),
                    Ohr        = reader.IsDBNull(3) ? null : reader.GetDecimal(3)
                });
            }
            return rows;
        }

        public async Task<IReadOnlyList<AnimalStagingRow>> GetAnimalRowsForExportAsync(
            int fpsYear, CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT animaltype, species, security_level,
                       dailyrate::numeric, defradailyrate::numeric, planbyweek
                FROM fps.tblanimals
                WHERE fpsyear = @fpsyear
                ORDER BY animaltype;";
            cmd.Parameters.AddWithValue("fpsyear", fpsYear);

            var rows = new List<AnimalStagingRow>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                rows.Add(new AnimalStagingRow
                {
                    JobQueueId     = Guid.Empty,
                    AnimalType     = reader.GetString(0),
                    Species        = reader.IsDBNull(1) ? null : reader.GetString(1),
                    SecurityLevel  = reader.IsDBNull(2) ? null : reader.GetString(2),
                    DailyRate      = reader.IsDBNull(3) ? null : reader.GetDecimal(3),
                    DefraDailyRate = reader.IsDBNull(4) ? null : reader.GetDecimal(4),
                    PlanByWeek     = reader.IsDBNull(5) ? null : reader.GetBoolean(5)
                });
            }
            return rows;
        }

        // ── Freeze reviewed classification onto staging ───────────────────────────
        // Same unmapped-staging-tables rationale as the Staging block above — per-row UPDATEs
        // keyed by business key (TestCode / TestCode+Buyer / PcGrade / AnimalType) against
        // tables with no existing EF entity.

        public async Task FreezeStagingCalculatedActionsAsync(
            Guid jobQueueId, int validationVersion,
            IReadOnlyList<TestFreezeEntry> fecFreezes,
            IReadOnlyList<TestFreezeEntry> agrupFreezes,
            CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var tx = await conn.BeginTransactionAsync(ct);

            foreach (var entry in fecFreezes)
            {
                await using var upd = conn.CreateCommand();
                upd.Transaction = tx;
                upd.CommandText = @"
                    UPDATE fps.tblstagingtestorproduct
                    SET calculated_action    = @calculated_action,
                        effective_new_rate   = @effective_new_rate,
                        source_current_rate  = @source_current_rate,
                        validation_version   = @validation_version
                    WHERE jobqueueid = @jobqueueid AND testcode = @testcode;";
                ApplyFecFreezeParams(upd, jobQueueId, entry, validationVersion);
                await upd.ExecuteNonQueryAsync(ct);
            }

            foreach (var entry in agrupFreezes)
            {
                await using var upd = conn.CreateCommand();
                upd.Transaction = tx;
                upd.CommandText = @"
                    UPDATE fps.tblstagingtlkptestreqmt
                    SET calculated_action    = @calculated_action,
                        effective_new_rate   = @effective_new_rate,
                        source_current_rate  = @source_current_rate,
                        validation_version   = @validation_version
                    WHERE jobqueueid = @jobqueueid AND testcode = @testcode AND buyer = @buyer;";
                ApplyAgrupFreezeParams(upd, jobQueueId, entry, validationVersion);
                await upd.ExecuteNonQueryAsync(ct);
            }

            await tx.CommitAsync(ct);

            _logger.LogInformation(
                "FreezeStagingCalculatedActions | JobQueueId={JobQueueId} | ValidationVersion={ValidationVersion} | FecRows={FecRows} | AgrupRows={AgrupRows}",
                jobQueueId, validationVersion, fecFreezes.Count, agrupFreezes.Count);
        }

        public async Task FreezeStaffStagingAsync(
            Guid jobQueueId, int validationVersion,
            IReadOnlyList<StaffFreezeEntry> freezes,
            CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var tx = await conn.BeginTransactionAsync(ct);

            foreach (var entry in freezes)
            {
                await using var upd = conn.CreateCommand();
                upd.Transaction = tx;
                upd.CommandText = @"
                    UPDATE fps.tblstagingprofitcentregrade
                    SET source_payrate        = @source_payrate,
                        source_npr            = @source_npr,
                        source_ohr            = @source_ohr,
                        effective_payrate     = @effective_payrate,
                        effective_npr         = @effective_npr,
                        effective_ohr         = @effective_ohr,
                        effective_chargerate  = @effective_chargerate,
                        calculated_action     = @calculated_action,
                        validation_version    = @validation_version
                    WHERE jobqueueid = @jobqueueid AND pcgrade = @pcgrade;";
                ApplyStaffFreezeParams(upd, jobQueueId, entry, validationVersion);
                await upd.ExecuteNonQueryAsync(ct);
            }

            await tx.CommitAsync(ct);

            _logger.LogInformation(
                "FreezeStaffStaging | JobQueueId={JobQueueId} | ValidationVersion={ValidationVersion} | StaffRows={StaffRows}",
                jobQueueId, validationVersion, freezes.Count);
        }

        public async Task FreezeAnimalStagingAsync(
            Guid jobQueueId, int validationVersion,
            IReadOnlyList<AnimalFreezeEntry> freezes,
            CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var tx = await conn.BeginTransactionAsync(ct);

            foreach (var entry in freezes)
            {
                await using var upd = conn.CreateCommand();
                upd.Transaction = tx;
                upd.CommandText = @"
                    UPDATE fps.tblstaginganimals
                    SET source_dailyrate         = @source_dailyrate,
                        source_defradailyrate    = @source_defradailyrate,
                        source_planbyweek        = @source_planbyweek,
                        source_species           = @source_species,
                        source_securitylevel     = @source_securitylevel,
                        effective_dailyrate      = @effective_dailyrate,
                        effective_defradailyrate = @effective_defradailyrate,
                        effective_planbyweek     = @effective_planbyweek,
                        effective_species        = @effective_species,
                        effective_securitylevel  = @effective_securitylevel,
                        calculated_action        = @calculated_action,
                        validation_version       = @validation_version
                    WHERE jobqueueid = @jobqueueid AND animaltype = @animaltype;";
                upd.Parameters.AddWithValue("jobqueueid", jobQueueId);
                upd.Parameters.AddWithValue("animaltype", entry.AnimalType);
                upd.Parameters.AddWithValue("source_dailyrate", (object?)entry.SourceDailyRate ?? DBNull.Value);
                upd.Parameters.AddWithValue("source_defradailyrate", (object?)entry.SourceDefraDailyRate ?? DBNull.Value);
                upd.Parameters.AddWithValue("source_planbyweek", (object?)entry.SourcePlanByWeek ?? DBNull.Value);
                upd.Parameters.AddWithValue("source_species", (object?)entry.SourceSpecies ?? DBNull.Value);
                upd.Parameters.AddWithValue("source_securitylevel", (object?)entry.SourceSecurityLevel ?? DBNull.Value);
                upd.Parameters.AddWithValue("effective_dailyrate", (object?)entry.EffectiveDailyRate ?? DBNull.Value);
                upd.Parameters.AddWithValue("effective_defradailyrate", (object?)entry.EffectiveDefraDailyRate ?? DBNull.Value);
                upd.Parameters.AddWithValue("effective_planbyweek", (object?)entry.EffectivePlanByWeek ?? DBNull.Value);
                upd.Parameters.AddWithValue("effective_species", (object?)entry.EffectiveSpecies ?? DBNull.Value);
                upd.Parameters.AddWithValue("effective_securitylevel", (object?)entry.EffectiveSecurityLevel ?? DBNull.Value);
                upd.Parameters.AddWithValue("calculated_action", entry.CalculatedAction);
                upd.Parameters.AddWithValue("validation_version", validationVersion);
                await upd.ExecuteNonQueryAsync(ct);
            }

            await tx.CommitAsync(ct);

            _logger.LogInformation(
                "FreezeAnimalStaging | JobQueueId={JobQueueId} | ValidationVersion={ValidationVersion} | AnimalRows={AnimalRows}",
                jobQueueId, validationVersion, freezes.Count);
        }

        // ── Private helpers ──────────────────────────────────────────────────────

        private static async Task DeleteFromAsync(
            NpgsqlConnection conn, NpgsqlTransaction tx,
            string qualifiedTable, Guid jobQueueId, CancellationToken ct)
        {
            await using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            // Table name is hardcoded in callers — no user input reaches here
            cmd.CommandText = $"DELETE FROM {qualifiedTable} WHERE jobqueueid = @jqid;";
            cmd.Parameters.AddWithValue("jqid", jobQueueId);
            await cmd.ExecuteNonQueryAsync(ct);
        }

        private static IQueryable<BulkRatesQueueRow> ApplySortOrder(
            IQueryable<BulkRatesQueueRow> q, string? sortBy, bool descending) =>
            sortBy?.ToLowerInvariant() switch
            {
                "jobname"     => descending ? q.OrderByDescending(r => r.JobName)        : q.OrderBy(r => r.JobName),
                "fpsyear"     => descending ? q.OrderByDescending(r => r.FpsYear)        : q.OrderBy(r => r.FpsYear),
                "status"      => descending ? q.OrderByDescending(r => r.Status)         : q.OrderBy(r => r.Status),
                "requestedby" => descending ? q.OrderByDescending(r => r.RequestedBy)    : q.OrderBy(r => r.RequestedBy),
                _             => descending ? q.OrderByDescending(r => r.RequestedAtUtc) : q.OrderBy(r => r.RequestedAtUtc)
            };

        private static TestOrProductStagingRow MapFecStagingRow(NpgsqlDataReader r) =>
            new()
            {
                JobQueueId        = r.GetGuid(0),
                TestCode          = r.GetString(1),
                UnitPriceVla      = r.IsDBNull(2)  ? null : r.GetDecimal(2),
                DefraUnitPrice    = r.IsDBNull(3)  ? null : r.GetDecimal(3),
                FecNewRate        = r.IsDBNull(4)  ? null : r.GetDecimal(4),
                Change            = r.IsDBNull(5)  ? null : r.GetDecimal(5),
                ItemDescription   = r.IsDBNull(6)  ? null : r.GetString(6),
                ShortDescription  = r.IsDBNull(7)  ? null : r.GetString(7),
                Owner             = r.IsDBNull(8)  ? null : r.GetString(8),
                Comments          = r.IsDBNull(9)  ? null : r.GetString(9),
                CalculatedAction  = r.IsDBNull(10) ? null : r.GetString(10),
                EffectiveNewRate  = r.IsDBNull(11) ? null : r.GetDecimal(11),
                SourceCurrentRate = r.IsDBNull(12) ? null : r.GetDecimal(12),
                ValidationVersion = r.IsDBNull(13) ? null : r.GetInt32(13)
            };

        private static TestRequirementStagingRow MapAgrupStagingRow(NpgsqlDataReader r) =>
            new()
            {
                JobQueueId         = r.GetGuid(0),
                TestCode           = r.GetString(1),
                Buyer              = r.GetString(2),
                Agrup              = r.IsDBNull(3)  ? null : r.GetDecimal(3),
                AgrupNew           = r.IsDBNull(4)  ? null : r.GetDecimal(4),
                Change             = r.IsDBNull(5)  ? null : r.GetDecimal(5),
                NoRequired         = r.IsDBNull(6)  ? null : r.GetDouble(6),
                DateCreated        = r.IsDBNull(7)  ? null : r.GetDateTime(7),
                Active             = r.IsDBNull(8)  ? null : r.GetInt16(8),
                Comments           = r.IsDBNull(9)  ? null : r.GetString(9),
                ProjectBuyerCode   = r.IsDBNull(10) ? null : r.GetString(10),
                TestBuyerCode      = r.IsDBNull(11) ? null : r.GetString(11),
                TestBuyerWorkGroup = r.IsDBNull(12) ? null : r.GetString(12),
                CalculatedAction   = r.IsDBNull(13) ? null : r.GetString(13),
                EffectiveNewRate   = r.IsDBNull(14) ? null : r.GetDecimal(14),
                SourceCurrentRate  = r.IsDBNull(15) ? null : r.GetDecimal(15),
                ValidationVersion  = r.IsDBNull(16) ? null : r.GetInt32(16)
            };

        private static void ApplyFecFreezeParams(
            NpgsqlCommand cmd, Guid jobQueueId, TestFreezeEntry entry, int validationVersion)
        {
            cmd.Parameters.AddWithValue("jobqueueid",          jobQueueId);
            cmd.Parameters.AddWithValue("testcode",            entry.TestCode);
            cmd.Parameters.AddWithValue("calculated_action",   entry.CalculatedAction);
            cmd.Parameters.AddWithValue("effective_new_rate",  (object?)entry.EffectiveNewRate  ?? DBNull.Value);
            cmd.Parameters.AddWithValue("source_current_rate", (object?)entry.SourceCurrentRate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("validation_version",  validationVersion);
        }

        private static void ApplyAgrupFreezeParams(
            NpgsqlCommand cmd, Guid jobQueueId, TestFreezeEntry entry, int validationVersion)
        {
            cmd.Parameters.AddWithValue("jobqueueid",          jobQueueId);
            cmd.Parameters.AddWithValue("testcode",            entry.TestCode);
            cmd.Parameters.AddWithValue("buyer",               (object?)entry.Buyer           ?? DBNull.Value);
            cmd.Parameters.AddWithValue("calculated_action",   entry.CalculatedAction);
            cmd.Parameters.AddWithValue("effective_new_rate",  (object?)entry.EffectiveNewRate  ?? DBNull.Value);
            cmd.Parameters.AddWithValue("source_current_rate", (object?)entry.SourceCurrentRate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("validation_version",  validationVersion);
        }

        private static void ApplyStaffFreezeParams(
            NpgsqlCommand cmd, Guid jobQueueId, StaffFreezeEntry entry, int validationVersion)
        {
            cmd.Parameters.AddWithValue("jobqueueid",           jobQueueId);
            cmd.Parameters.AddWithValue("pcgrade",              entry.PcGrade);
            cmd.Parameters.AddWithValue("source_payrate",       (object?)entry.SourcePayRate    ?? DBNull.Value);
            cmd.Parameters.AddWithValue("source_npr",           (object?)entry.SourceNpr        ?? DBNull.Value);
            cmd.Parameters.AddWithValue("source_ohr",           (object?)entry.SourceOhr        ?? DBNull.Value);
            cmd.Parameters.AddWithValue("effective_payrate",    (object?)entry.EffectivePayRate  ?? DBNull.Value);
            cmd.Parameters.AddWithValue("effective_npr",        (object?)entry.EffectiveNpr      ?? DBNull.Value);
            cmd.Parameters.AddWithValue("effective_ohr",        (object?)entry.EffectiveOhr      ?? DBNull.Value);
            cmd.Parameters.AddWithValue("effective_chargerate", (object?)entry.EffectiveChargeRate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("calculated_action",    entry.CalculatedAction);
            cmd.Parameters.AddWithValue("validation_version",   validationVersion);
        }
    }
}

