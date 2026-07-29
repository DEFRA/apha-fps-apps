using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.PACT.DataAccess.Repository
{
    public class MonthlyOutputRepository : BaseRepository, IMonthlyOutputRepository
    {
        private readonly IFpsRequestContext _fpsRequestContext;

        public MonthlyOutputRepository(FpsDbContext context, IFpsRequestContext fpsRequestContext) : base(context)
        {
            _fpsRequestContext = fpsRequestContext;
        }

        // ── Log search ──────────────────────────────────────────────────────────

        public async Task<PagedData<MonthlyOutputLog>> GetMonthlyOutputLogAsync(
            PaginationParameters<string> query,
            string? workGroup,
            string? testCode,
            string? buyer,
            DateTime? dateImported,
            double? month,
            string? userId,
            string? insertDelete)
        {
            var baseQuery = _context.MonthlyOutputLogs.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(workGroup))
                baseQuery = baseQuery.Where(x => x.WorkGroup == workGroup);

            if (!string.IsNullOrWhiteSpace(testCode))
                baseQuery = baseQuery.Where(x => x.TestCode == testCode);

            if (!string.IsNullOrWhiteSpace(buyer))
                baseQuery = baseQuery.Where(x => x.Buyer == buyer);

            if (dateImported.HasValue)
            {
                var dateOnly = dateImported.Value.Date;
                baseQuery = baseQuery.Where(x => x.DateTime.HasValue && x.DateTime.Value.Date == dateOnly);
            }

            if (month.HasValue)
                baseQuery = baseQuery.Where(x => x.Month.HasValue && (int)x.Month.Value == (int)month.Value);

            if (!string.IsNullOrWhiteSpace(userId))
                baseQuery = baseQuery.Where(x => x.UserId != null && x.UserId.Contains(userId));

            if (!string.IsNullOrWhiteSpace(insertDelete))
                baseQuery = baseQuery.Where(x => x.InsertDelete != null && x.InsertDelete.StartsWith(insertDelete));

            baseQuery = baseQuery.OrderByDescending(x => x.DateTime).ThenBy(x => x.SequenceNo);

            return await ApplyPaging(baseQuery, query.Page, query.PageSize);
        }

        // ── Live record helpers ──────────────────────────────────────────────────

        public async Task<bool> ExistsByTestCodeAndWorkGroupAsync(string testCode, string workGroup)
        {
            return await _context.MonthlyOutputs
                .AsNoTracking()
                .AnyAsync(m => m.TestCode == testCode && m.WorkGroup == workGroup);
        }

        public async Task<bool> LiveRecordExistsAsync(string testCode, string buyer, double month, string workGroup)
        {
            return await _context.MonthlyOutputs
                .AsNoTracking()
                .AnyAsync(m => m.TestCode == testCode && m.Buyer == buyer
                            && (int)m.Month == (int)month && m.WorkGroup == workGroup);
        }

        // ── Live CRUD ────────────────────────────────────────────────────────────

        public async Task<PagedData<MonthlyOutput>> SearchLiveAsync(
            PaginationParameters<string> query,
            string? workGroup,
            string? testCode,
            string? buyer,
            double? month)
        {
            var monthlyOutputs = _context.MonthlyOutputs.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(workGroup))
                monthlyOutputs = monthlyOutputs.Where(x => x.WorkGroup == workGroup);
            if (!string.IsNullOrWhiteSpace(testCode))
                monthlyOutputs = monthlyOutputs.Where(x => x.TestCode == testCode);
            if (!string.IsNullOrWhiteSpace(buyer))
                monthlyOutputs = monthlyOutputs.Where(x => x.Buyer == buyer);
            if (month.HasValue)
                monthlyOutputs = monthlyOutputs.Where(x => (int)x.Month == (int)month.Value);

            monthlyOutputs = monthlyOutputs.OrderBy(x => x.WorkGroup).ThenBy(x => x.TestCode).ThenBy(x => x.Buyer).ThenBy(x => x.Month);

            var pagedLiveData = await ApplyPaging(monthlyOutputs, query.Page, query.PageSize);

            pagedLiveData.Total = await monthlyOutputs.SumAsync(x => (decimal)(x.Volume ?? 0));
            return pagedLiveData;
        }

        public async Task<MonthlyOutput?> GetLiveByKeyAsync(string testCode, string buyer, double month, string workGroup)
        {
            return await _context.MonthlyOutputs
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.TestCode == testCode && m.Buyer == buyer
                                       && (int)m.Month == (int)month && m.WorkGroup == workGroup);
        }

        public async Task<MonthlyOutput> UpdateLiveAsync(
            MonthlyOutput monthlyOutput,
            string originalTestCode,
            string originalBuyer,
            double originalMonth,
            string originalWorkGroup)
        {
            var existing = await _context.MonthlyOutputs
                .FirstOrDefaultAsync(m => m.TestCode == originalTestCode && m.Buyer == originalBuyer
                                       && (int)m.Month == (int)originalMonth && m.WorkGroup == originalWorkGroup)
                ?? throw new KeyNotFoundException("Monthly Output live record not found.");

            existing.TestCode = monthlyOutput.TestCode;
            existing.Buyer = monthlyOutput.Buyer;
            existing.Month = monthlyOutput.Month;
            existing.WorkGroup = monthlyOutput.WorkGroup;
            existing.Volume = monthlyOutput.Volume;

            var logEntry = BuildLogEntry(existing, "U");
            await _context.MonthlyOutputLogs.AddAsync(logEntry);

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteLiveAsync(string testCode, string buyer, double month, string workGroup)
        {
            var existing = await _context.MonthlyOutputs
                .FirstOrDefaultAsync(m => m.TestCode == testCode && m.Buyer == buyer
                                       && (int)m.Month == (int)month && m.WorkGroup == workGroup);

            if (existing is null)
                return false;

            var logEntry = BuildLogEntry(existing, "D");
            await _context.MonthlyOutputLogs.AddAsync(logEntry);

            _context.MonthlyOutputs.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }

        // ── Staging CRUD ─────────────────────────────────────────────────────────

        public async Task<PagedData<StagingMonthlyOutput>> SearchStagingAsync(
            PaginationParameters<string> query,
            string importedBy,
            bool? passed)
        {
            var stagingQuery = _context.StagingMonthlyOutputs
                .AsNoTracking()
                .Where(x => x.ImportedBy == importedBy);

            if (passed.HasValue)
                stagingQuery = stagingQuery.Where(x => x.Passed == passed.Value);

            stagingQuery = stagingQuery.OrderBy(x => x.Id);

            var pagedStagingData = await ApplyPaging(stagingQuery, query.Page, query.PageSize);

            pagedStagingData.Total = await stagingQuery.SumAsync(x => (decimal)(x.Volume ?? 0));
            return pagedStagingData;
        }

        public async Task<StagingMonthlyOutput?> GetStagingByIdAsync(int id, string importedBy)
        {
            return await _context.StagingMonthlyOutputs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && x.ImportedBy == importedBy);
        }

        public async Task<StagingMonthlyOutput> CreateStagingAsync(StagingMonthlyOutput stagingMonthlyOutput)
        {
            await _context.StagingMonthlyOutputs.AddAsync(stagingMonthlyOutput);
            await _context.SaveChangesAsync();
            return stagingMonthlyOutput;
        }

        public async Task<StagingMonthlyOutput> UpdateStagingAsync(StagingMonthlyOutput stagingMonthlyOutput, string importedBy)
        {
            var existing = await _context.StagingMonthlyOutputs
                .FirstOrDefaultAsync(x => x.Id == stagingMonthlyOutput.Id && x.ImportedBy == importedBy)
                ?? throw new KeyNotFoundException($"Staging Monthly Output record {stagingMonthlyOutput.Id} not found.");

            existing.TestCode = stagingMonthlyOutput.TestCode;
            existing.Buyer = stagingMonthlyOutput.Buyer;
            existing.Month = stagingMonthlyOutput.Month;
            existing.WorkGroup = stagingMonthlyOutput.WorkGroup;
            existing.Volume = stagingMonthlyOutput.Volume;
            existing.FailureComments = stagingMonthlyOutput.FailureComments;
            existing.Passed = stagingMonthlyOutput.Passed;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteStagingAsync(int id, string importedBy)
        {
            var row = await _context.StagingMonthlyOutputs
                .FirstOrDefaultAsync(x => x.Id == id && x.ImportedBy == importedBy);

            if (row is null) return false;
            _context.StagingMonthlyOutputs.Remove(row);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> DeleteAllStagingByUserAsync(string importedBy)
        {
            var rows = await _context.StagingMonthlyOutputs
                .Where(x => x.ImportedBy == importedBy)
                .ToListAsync();

            _context.StagingMonthlyOutputs.RemoveRange(rows);
            await _context.SaveChangesAsync();
            return rows.Count;
        }

        public async Task<int> DeleteFailedStagingByUserAsync(string importedBy)
        {
            var rows = await _context.StagingMonthlyOutputs
                .Where(x => x.ImportedBy == importedBy && x.Passed == false)
                .ToListAsync();

            _context.StagingMonthlyOutputs.RemoveRange(rows);
            await _context.SaveChangesAsync();
            return rows.Count;
        }

        public async Task<int> ImportStagingAsync(IEnumerable<StagingMonthlyOutput> stagingRows)
        {
            var list = stagingRows.ToList();
            await _context.StagingMonthlyOutputs.AddRangeAsync(list);
            await _context.SaveChangesAsync();
            return list.Count;
        }

        public async Task<int> RemoveZeroAndNullVolumeRecordsAsync(string importedBy)
        {
            var rows = await _context.StagingMonthlyOutputs
                .Where(x => x.ImportedBy == importedBy
                         && (x.Volume == null || x.Volume == 0))
                .ToListAsync();

            _context.StagingMonthlyOutputs.RemoveRange(rows);
            await _context.SaveChangesAsync();
            return rows.Count;
        }

        public async Task<List<StagingMonthlyOutput>> GetStagingRecordsForValidationAsync(string importedBy)
        {
            return await _context.StagingMonthlyOutputs
                .Where(x => x.ImportedBy == importedBy)
                .OrderBy(x => x.Id)
                .ToListAsync();
        }

        public async Task UpdateStagingRecordsAsync(IEnumerable<StagingMonthlyOutput> records)
        {
            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasFailedStagingAsync(string importedBy)
        {
            return await _context.StagingMonthlyOutputs
                .AsNoTracking()
                .AnyAsync(x => x.ImportedBy == importedBy && x.Passed == false);
        }

        // ── Make Live ────────────────────────────────────────────────────────────

        public async Task<(int ProcessedCount, int ImportedCount, int FailedCount)> MakeLiveAsync(string importedBy)
        {
            const string noLongerValidMessage = "This record is no longer valid. Needs re-validating";

            var passedRows = await _context.StagingMonthlyOutputs
                .Where(x => x.ImportedBy == importedBy && x.Passed == true)
                .OrderBy(x => x.Id)
                .ToListAsync();

            if (passedRows.Count == 0)
                return (0, 0, 0);

            var failedCount = await _context.StagingMonthlyOutputs
                .AsNoTracking()
                .CountAsync(x => x.ImportedBy == importedBy && x.Passed == false);

            var importedCount = 0;

            foreach (var row in passedRows)
            {
                if (string.IsNullOrWhiteSpace(row.TestCode)
                    || string.IsNullOrWhiteSpace(row.Buyer)
                    || row.Month == 0
                    || string.IsNullOrWhiteSpace(row.WorkGroup))
                {
                    row.Passed = false;
                    row.FailureComments = noLongerValidMessage;
                    failedCount++;
                    await _context.SaveChangesAsync();
                    continue;
                }

                MonthlyOutput? liveRow = null;
                MonthlyOutputLog? logEntry = null;

                try
                {
                    liveRow = new MonthlyOutput
                    {
                        TestCode = row.TestCode,
                        Buyer = row.Buyer,
                        Month = row.Month,
                        WorkGroup = row.WorkGroup,
                        Volume = row.Volume,
                        FpsYear = _fpsRequestContext.FpsYear
                    };

                    logEntry = BuildLogEntry(liveRow, "I");

                    await _context.MonthlyOutputs.AddAsync(liveRow);
                    await _context.MonthlyOutputLogs.AddAsync(logEntry);
                    _context.StagingMonthlyOutputs.Remove(row);

                    await _context.SaveChangesAsync();
                    importedCount++;
                }
                catch
                {
                    if (liveRow != null)
                    {
                        var entry = _context.Entry(liveRow);
                        if (entry.State != EntityState.Detached)
                            entry.State = EntityState.Detached;
                    }
                    if (logEntry != null)
                    {
                        var entry = _context.Entry(logEntry);
                        if (entry.State != EntityState.Detached)
                            entry.State = EntityState.Detached;
                    }

                    var rowEntry = _context.Entry(row);
                    if (rowEntry.State == EntityState.Deleted)
                        rowEntry.State = EntityState.Unchanged;

                    row.Passed = false;
                    row.FailureComments = noLongerValidMessage;
                    rowEntry.State = EntityState.Modified;

                    await _context.SaveChangesAsync();
                    failedCount++;
                }
            }

            return (passedRows.Count, importedCount, failedCount);
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private MonthlyOutputLog BuildLogEntry(MonthlyOutput row, string insertDelete)
        {
            return new MonthlyOutputLog
            {
                TestCode = row.TestCode,
                Buyer = row.Buyer,
                Month = row.Month,
                WorkGroup = row.WorkGroup,
                Volume = row.Volume,
                WgBuyer = row.WgBuyer,
                DateTime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                UserId = _fpsRequestContext.UserEmailId,
                InsertDelete = insertDelete,
                FpsYear = _fpsRequestContext.FpsYear
            };
        }
    }
}
