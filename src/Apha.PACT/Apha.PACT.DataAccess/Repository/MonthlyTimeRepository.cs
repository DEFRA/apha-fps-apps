using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.PACT.DataAccess.Repository
{
    public class MonthlyTimeRepository : BaseRepository, IMonthlyTimeRepository
    {
        public MonthlyTimeRepository(FpsDbContext context) : base(context) { }

        public async Task<bool> HasMonthlyTimeEntriesAsync(string workGroup, string timeCode, string parentProject)
        {
            return await _context.MonthlyTimes
                .AsNoTracking()
                .AnyAsync(m => m.WorkGroup == workGroup && m.TimeCode == timeCode && m.ParentProject == parentProject);
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

            var result = await baseQuery.ToListAsync();
            return ApplyPaging(result, query.Page, query.PageSize);
        }
    }
}
