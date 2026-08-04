using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Dynamic;
using System.Linq.Expressions;

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

            IQueryable<MonthlyTimeLog> baseQuery = _context.MonthlyTimeLogs.AsNoTracking();

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

            baseQuery = ApplyMonthlyTimeFilter(baseQuery, query.Filter);
            baseQuery = (IQueryable<MonthlyTimeLog>)ApplySorting(baseQuery, query.SortBy, query.Descending);

            return await ApplyPaging(baseQuery, query.Page, query.PageSize);
        }

        private static IQueryable<MonthlyTimeLog> ApplyMonthlyTimeFilter(IQueryable<MonthlyTimeLog> query, string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            IDictionary<string, object> dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("SequenceNo", out object? sequenceNo)
                && sequenceNo != null
                && int.TryParse(sequenceNo.ToString(), out int sequenceNoValue))
            {
                query = query.Where(x => x.SequenceNo == sequenceNoValue);
            }

            if (dict.TryGetValue("TimeCode", out object? timeCode) && timeCode != null)
                query = query.Where(x => x.TimeCode != null && EF.Functions.ILike(x.TimeCode, $"%{timeCode}%"));

            if (dict.TryGetValue("ParentProject", out object? parentProject) && parentProject != null)
                query = query.Where(x => x.ParentProject != null && EF.Functions.ILike(x.ParentProject, $"%{parentProject}%"));

            if (dict.TryGetValue("Month", out object? month) && month != null && double.TryParse(month.ToString(), out double monthValue))
                query = query.Where(x => (int)x.Month == (int)monthValue);

            if (dict.TryGetValue("PactStaffId", out object? pactStaffId) && pactStaffId != null)
                query = query.Where(x => x.PactStaffId != null && EF.Functions.ILike(x.PactStaffId, $"%{pactStaffId}%"));

            if (dict.TryGetValue("WorkGroup", out object? workGroup) && workGroup != null)
                query = query.Where(x => x.WorkGroup != null && EF.Functions.ILike(x.WorkGroup, $"%{workGroup}%"));

            if (dict.TryGetValue("Hours", out object? hours) && hours != null && double.TryParse(hours.ToString(), out double hoursValue))
                query = query.Where(x => x.Hours.HasValue && x.Hours.Value == hoursValue);

            if (dict.TryGetValue("DateTime", out object? dateImported) && dateImported != null && DateTime.TryParse(dateImported.ToString(), out DateTime importedDate))
            {
                var dateOnly = importedDate.Date;
                query = query.Where(x => x.DateTime.HasValue && x.DateTime.Value.Date == dateOnly);
            }

            if (dict.TryGetValue("UserId", out object? userId) && userId != null)
                query = query.Where(x => x.UserId != null && EF.Functions.ILike(x.UserId, $"%{userId}%"));

            if (dict.TryGetValue("InsertDelete", out object? insertDelete) && insertDelete != null)
                query = query.Where(x => x.InsertDelete != null && EF.Functions.ILike(x.InsertDelete, $"%{insertDelete}%"));

            return query;
        }

        private static IQueryable ApplySorting(IQueryable<MonthlyTimeLog> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                return query.OrderByDescending(x => x.DateTime).ThenBy(x => x.SequenceNo);

            return sortBy.ToLower() switch
            {
                "sequenceno" or "id" => ApplyOrder(query, x => x.SequenceNo, descending),
                "timecode" => ApplyOrder(query, x => x.TimeCode, descending),
                "parentproject" or "project" => ApplyOrder(query, x => x.ParentProject, descending),
                "month" => ApplyOrder(query, x => x.Month, descending),
                "pactstaffid" or "staffid" => ApplyOrder(query, x => x.PactStaffId, descending),
                "workgroup" => ApplyOrder(query, x => x.WorkGroup, descending),
                "hours" => ApplyOrder(query, x => x.Hours, descending),
                "datetime" or "dateimported" => ApplyOrder(query, x => x.DateTime, descending),
                "userid" => ApplyOrder(query, x => x.UserId, descending),
                "insertdelete" or "action" => ApplyOrder(query, x => x.InsertDelete, descending),
                _ => query.OrderByDescending(x => x.DateTime).ThenBy(x => x.SequenceNo)
            };
        }

        private static IQueryable ApplyOrder<T>(IQueryable<MonthlyTimeLog> query, Expression<Func<MonthlyTimeLog, T>> keySelector, bool descending)
        {
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }
    }
}
