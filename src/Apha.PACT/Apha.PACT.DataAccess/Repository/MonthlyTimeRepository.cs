using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Apha.PACT.DataAccess.Repository
{
    public class MonthlyTimeRepository : BaseRepository, IMonthlyTimeRepository
    {
        private readonly IFpsRequestContext _fpsRequestContext;       

        public MonthlyTimeRepository(FpsDbContext context, IFpsRequestContext fpsRequestContext) : base(context)
        {
            _fpsRequestContext = fpsRequestContext;
        }       

        public async Task<bool> HasMonthlyTimeEntriesAsync(string workGroup, string timeCode, string parentProject)
        {
            return await _context.MonthlyTimes
                .AsNoTracking()
                .AnyAsync(m => m.WorkGroup == workGroup && m.TimeCode == timeCode && m.ParentProject == parentProject);
        }

        public async Task<PagedData<MonthlyTime>> SearchLiveAsync(
            PaginationParameters<string> query,
            string? workGroup,
            string? timeCode,
            string? pactStaffId,
            string? parentProject,
            double? month)
        {
            var monthlyTimes = _context.MonthlyTimes.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(workGroup))
                monthlyTimes = monthlyTimes.Where(x => x.WorkGroup == workGroup);

            if (!string.IsNullOrWhiteSpace(timeCode))
                monthlyTimes = monthlyTimes.Where(x => x.TimeCode == timeCode);

            if (!string.IsNullOrWhiteSpace(pactStaffId))
                monthlyTimes = monthlyTimes.Where(x => x.PactStaffId == pactStaffId);

            if (!string.IsNullOrWhiteSpace(parentProject))
                monthlyTimes = monthlyTimes.Where(x => x.ParentProject == parentProject);

            if (month.HasValue)
                monthlyTimes = monthlyTimes.Where(x => (int)x.Month == (int)month.Value);
            
            monthlyTimes = (IQueryable<MonthlyTime>)ApplySorting(monthlyTimes, query.SortBy, query.Descending);

            return await ApplyPaging(monthlyTimes, query.Page, query.PageSize);
        }

        public async Task<MonthlyTime?> GetLiveByKeyAsync(string pactStaffId, string timeCode, double month, string parentProject)
        {
            return await _context.MonthlyTimes
                .FirstOrDefaultAsync(x => x.PactStaffId == pactStaffId
                    && x.TimeCode == timeCode
                    && x.Month == month
                    && x.ParentProject == parentProject
                    && x.FpsYear == _fpsRequestContext.FpsYear);
        }

        public async Task<MonthlyTime> UpdateLiveAsync(MonthlyTime monthlyTime)
        {
            monthlyTime.FpsYear = _fpsRequestContext.FpsYear;
            _context.Entry(monthlyTime).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return monthlyTime;
        }

        public async Task<bool> DeleteLiveAsync(string pactStaffId, string timeCode, double month, string parentProject)
        {
            var entity = await GetLiveByKeyAsync(pactStaffId, timeCode, month, parentProject);
            if (entity == null)
                return false;

            _context.MonthlyTimes.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(string pactStaffId, string timeCode, double month, string parentProject)
        {
            return await _context.MonthlyTimes
                .AsNoTracking()
                .AnyAsync(x => x.PactStaffId == pactStaffId
                    && x.TimeCode == timeCode
                    && x.Month == month
                    && x.ParentProject == parentProject
                    && x.FpsYear == _fpsRequestContext.FpsYear);
        }

        public async Task<PagedData<StagingMonthlyTime>> SearchStagingAsync(
            PaginationParameters<string> query,
            string importedBy,
            bool? passed)
        {
            var stagingQuery = _context.StagingMonthlyTimes
                .AsNoTracking()
                .Where(x => x.ImportedBy == importedBy);

            if (passed.HasValue)
                stagingQuery = stagingQuery.Where(x => x.Passed == passed.Value);

            stagingQuery = stagingQuery
                .OrderBy(x => x.WorkGroup)
                .ThenBy(x => x.PactStaffId)
                .ThenBy(x => x.TimeCode)
                .ThenBy(x => x.ParentProject)
                .ThenBy(x => x.Month)
                .ThenBy(x => x.Id);

            return await ApplyPaging(stagingQuery, query.Page, query.PageSize);
        }

        public async Task<StagingMonthlyTime?> GetStagingByIdAsync(int id, string importedBy)
        {
            return await _context.StagingMonthlyTimes
                .FirstOrDefaultAsync(x => x.Id == id && x.ImportedBy == importedBy);
        }

        public async Task<StagingMonthlyTime> CreateStagingAsync(StagingMonthlyTime stagingMonthlyTime)
        {                      
            await _context.StagingMonthlyTimes.AddAsync(stagingMonthlyTime);
            await _context.SaveChangesAsync();
            return stagingMonthlyTime;
        }

        public async Task<StagingMonthlyTime> UpdateStagingAsync(StagingMonthlyTime stagingMonthlyTime, string importedBy)
        {
            var existing = await GetStagingByIdAsync(stagingMonthlyTime.Id, importedBy)
                ?? throw new InvalidOperationException("Staging monthly time record not found.");

            existing.PactStaffId = stagingMonthlyTime.PactStaffId;
            existing.TimeCode = stagingMonthlyTime.TimeCode;
            existing.ParentProject = stagingMonthlyTime.ParentProject;
            existing.Month = stagingMonthlyTime.Month;
            existing.WorkGroup = stagingMonthlyTime.WorkGroup;
            existing.Hours = stagingMonthlyTime.Hours;
            existing.PactId = stagingMonthlyTime.PactId;
            existing.Name = stagingMonthlyTime.Name;
            existing.Passed = false;
            existing.FailureComments = "This record has been edited since being validated. Needs re-validating.";           

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteStagingAsync(int id, string importedBy)
        {
            var existing = await GetStagingByIdAsync(id, importedBy);
            if (existing == null)
                return false;

            _context.StagingMonthlyTimes.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> DeleteAllStagingByUserAsync(string importedBy)
        {
            return await _context.StagingMonthlyTimes
                .Where(x => x.ImportedBy == importedBy)
                .ExecuteDeleteAsync();
        }

        public async Task<int> DeleteFailedStagingByUserAsync(string importedBy)
        {
            return await _context.StagingMonthlyTimes
                .Where(x => x.ImportedBy == importedBy && x.Passed == false)
                .ExecuteDeleteAsync();
        }

        public async Task<int> ImportStagingAsync(IEnumerable<StagingMonthlyTime> stagingRows)
        {
            var rows = stagingRows.ToList();

            if (rows.Count == 0)
                return 0;

            await _context.StagingMonthlyTimes.AddRangeAsync(rows);
            await _context.SaveChangesAsync();
            return rows.Count;
        }

        public async Task<int> RemoveZeroAndNullHourRecordsAsync(string importedBy)
        {
            return await _context.StagingMonthlyTimes
                .Where(x => x.ImportedBy == importedBy && (x.Hours == null || x.Hours == 0))
                .ExecuteDeleteAsync();
        }

        public async Task<List<StagingMonthlyTime>> GetStagingRecordsForValidationAsync(string importedBy)
        {
            return await _context.StagingMonthlyTimes
                .Where(x => x.ImportedBy == importedBy && x.Passed == false)
                .OrderBy(x => x.Id)
                .Distinct()
                .ToListAsync();
        }

        public async Task UpdateStagingRecordsAsync(IEnumerable<StagingMonthlyTime> records)
        {
            foreach (var record in records)
            {
                _context.Entry(record).State = EntityState.Modified;
            }
            await _context.SaveChangesAsync();
        }

        public async Task<HashSet<string>> GetExistingLiveKeysAsync()
        {
            var keys = await _context.MonthlyTimes
                .AsNoTracking()
                .Select(x => x.PactStaffId + "|" + x.TimeCode + "|" + x.ParentProject + "|" + x.WorkGroup + "|" + x.Month)
                .ToListAsync();
            return new HashSet<string>(keys);
        }

        public async Task<(int PassedCount, int FailedCount)> ValidateStagingAsync(string importedBy)
        {
            // Validation logic has been moved to MonthlyTimeService.ValidateStagingAsync
            // This method is deprecated and should not be called directly.
            // The repository now provides helper methods for validation data retrieval.
            throw new NotImplementedException("Call MonthlyTimeService.ValidateStagingAsync() instead. Repository validation logic has been moved to the service layer.");
        }

        public async Task<bool> HasFailedStagingAsync(string importedBy)
        {
            return await _context.StagingMonthlyTimes
                .AsNoTracking()
                .AnyAsync(x => x.ImportedBy == importedBy && x.Passed == false);
        }

        public async Task<(int ProcessedCount, int ImportedCount, int FailedCount)> MakeLiveAsync(string importedBy)
        {
            var passedRows = await _context.StagingMonthlyTimes
                .Where(x => x.ImportedBy == importedBy && x.Passed == true)
                .OrderBy(x => x.Id)
                .ToListAsync();

            if (passedRows.Count == 0)
                return (0, 0, 0);

            var failedCount = await _context.StagingMonthlyTimes
                .AsNoTracking()
                .CountAsync(x => x.ImportedBy == importedBy && x.Passed == false);

            var importedCount = 0;
            foreach (var row in passedRows)
            {
                if (string.IsNullOrWhiteSpace(row.PactId)
                    || string.IsNullOrWhiteSpace(row.TimeCode)
                    || !row.Month.HasValue
                    || string.IsNullOrWhiteSpace(row.ParentProject))
                {
                    row.Passed = false;
                    row.FailureComments = "Import of this record failed. Re-validate and try importing again.";
                    continue;
                }

                var liveRow = new MonthlyTime
                {
                    PactStaffId = row.PactId,
                    TimeCode = row.TimeCode,
                    Month = row.Month.Value,
                    ParentProject = row.ParentProject,
                    WorkGroup = row.WorkGroup,
                    Hours = row.Hours,
                    FpsYear = _fpsRequestContext.FpsYear
                };

                await _context.MonthlyTimes.AddAsync(liveRow);
                _context.StagingMonthlyTimes.Remove(row);
                importedCount++;
            }

            await _context.SaveChangesAsync();
            return (passedRows.Count, importedCount, failedCount);
        }

        public async Task<PagedData<MonthlyTimeLog>> SearchAsync(
            PaginationParameters<string> query,
            MonthlyTimeLogFilter monthlyTimeLogFilter)
        {
            string? workGroup = monthlyTimeLogFilter.WorkGroup;
            string? timeCode = monthlyTimeLogFilter.TimeCode;
            string? pactStaffId = monthlyTimeLogFilter.PactStaffId;
            string? parentProject = monthlyTimeLogFilter.ParentProject;
            DateTime? dateImported = monthlyTimeLogFilter.DateImported;
            double? month = monthlyTimeLogFilter.Month;
            string? userId = monthlyTimeLogFilter.UserId;
            string? insertDelete = monthlyTimeLogFilter.InsertDelete;

            var baseQuery = _context.MonthlyTimeLogs.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(workGroup))
                baseQuery = baseQuery.Where(x => x.WorkGroup == workGroup);

            if (!string.IsNullOrWhiteSpace(timeCode))
                baseQuery = baseQuery.Where(x => x.TimeCode == timeCode);

            if (!string.IsNullOrWhiteSpace(pactStaffId))
                baseQuery = baseQuery.Where(x => x.PactStaffId == pactStaffId);

            if (!string.IsNullOrWhiteSpace(parentProject))
                baseQuery = baseQuery.Where(x => x.ParentProject == parentProject);

            if (dateImported.HasValue)
            {
                var dateOnly = dateImported.Value.Date;
                baseQuery = baseQuery.Where(x => x.DateTime.HasValue
                    && x.DateTime.Value.Date == dateOnly);
            }

            if (month.HasValue)
                baseQuery = baseQuery.Where(x => (int)x.Month == (int)month.Value);

            if (!string.IsNullOrWhiteSpace(userId))
                baseQuery = baseQuery.Where(x => x.UserId != null && x.UserId.Contains(userId));

            if (!string.IsNullOrWhiteSpace(insertDelete))
                baseQuery = baseQuery.Where(x => x.InsertDelete != null
                    && x.InsertDelete.StartsWith(insertDelete));

            baseQuery = baseQuery.OrderByDescending(x => x.DateTime).ThenBy(x => x.SequenceNo);

            return await ApplyPaging(baseQuery, query.Page, query.PageSize);
        }

        private static IQueryable ApplySorting(IQueryable<MonthlyTime> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                return query.OrderBy(x => x.WorkGroup)
                .ThenBy(x => x.PactStaffId)
                .ThenBy(x => x.TimeCode)
                .ThenBy(x => x.ParentProject)
                .ThenBy(x => x.Month);
            }               

            return ApplySortingByProperty(query, sortBy.ToLower(), descending);
        }

        private static IQueryable ApplySortingByProperty(IQueryable<MonthlyTime> query, string property, bool descending)
        {
            return property switch
            {
                "workgroup" => ApplyOrder(query, s => s.WorkGroup, descending),
                "pactstaffid" => ApplyOrder(query, s => s.PactStaffId, descending),
                "timecode" => ApplyOrder(query, s => s.TimeCode, descending),
                "parentproject" => ApplyOrder(query, s => s.ParentProject, descending),
                "period" => ApplyOrder(query, s => s.Month, descending),
                "hours" => ApplyOrder(query, s => s.Hours, descending),
                _ => query.OrderBy(x => x.WorkGroup).ThenBy(x => x.PactStaffId).ThenBy(x => x.TimeCode).ThenBy(x => x.ParentProject).ThenBy(x => x.Month)
            };
        }

        private static IQueryable ApplyOrder<T>(IQueryable<MonthlyTime> query, Expression<Func<MonthlyTime, T>> keySelector, bool descending)
        {
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }
    }
}
