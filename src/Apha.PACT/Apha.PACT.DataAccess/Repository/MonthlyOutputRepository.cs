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
    public class MonthlyOutputRepository : BaseRepository, IMonthlyOutputRepository
    {
        public MonthlyOutputRepository(FpsDbContext context) : base(context)
        {
        }

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
            IQueryable<MonthlyOutputLog> baseQuery = _context.MonthlyOutputLogs.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(workGroup))
                baseQuery = baseQuery.Where(x => x.WorkGroup == workGroup);

            if (!string.IsNullOrWhiteSpace(testCode))
                baseQuery = baseQuery.Where(x => x.TestCode == testCode);

            if (!string.IsNullOrWhiteSpace(buyer))
                baseQuery = baseQuery.Where(x => x.Buyer == buyer);

            if (dateImported.HasValue)
            {
                var dateOnly = dateImported.Value.Date;
                baseQuery = baseQuery.Where(x => x.DateTime.HasValue
                    && x.DateTime.Value.Date == dateOnly);
            }

            if (month.HasValue)
                baseQuery = baseQuery.Where(x => x.Month.HasValue && (int)x.Month.Value == (int)month.Value);

            if (!string.IsNullOrWhiteSpace(userId))
                baseQuery = baseQuery.Where(x => x.UserId != null && x.UserId.Contains(userId));

            if (!string.IsNullOrWhiteSpace(insertDelete))
                baseQuery = baseQuery.Where(x => x.InsertDelete != null
                    && x.InsertDelete.StartsWith(insertDelete));

            baseQuery = ApplyMonthlyOutputFilter(baseQuery, query.Filter);
            baseQuery = (IQueryable<MonthlyOutputLog>)ApplySorting(baseQuery, query.SortBy, query.Descending);

            return await ApplyPaging(baseQuery, query.Page, query.PageSize);
        }

        private static IQueryable<MonthlyOutputLog> ApplyMonthlyOutputFilter(IQueryable<MonthlyOutputLog> query, string? filter)
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

            if (dict.TryGetValue("TestCode", out object? testCode) && testCode != null)
                query = query.Where(x => x.TestCode != null && EF.Functions.ILike(x.TestCode, $"%{testCode}%"));

            if (dict.TryGetValue("Buyer", out object? buyer) && buyer != null)
                query = query.Where(x => x.Buyer != null && EF.Functions.ILike(x.Buyer, $"%{buyer}%"));

            if (dict.TryGetValue("WorkGroup", out object? workGroup) && workGroup != null)
                query = query.Where(x => x.WorkGroup != null && EF.Functions.ILike(x.WorkGroup, $"%{workGroup}%"));

            if (dict.TryGetValue("Month", out object? month) && month != null && double.TryParse(month.ToString(), out double monthValue))
                query = query.Where(x => x.Month.HasValue && (int)x.Month.Value == (int)monthValue);

            if (dict.TryGetValue("Volume", out object? volume) && volume != null && double.TryParse(volume.ToString(), out double volumeValue))
                query = query.Where(x => x.Volume.HasValue && x.Volume.Value == volumeValue);

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

        private static IQueryable ApplySorting(IQueryable<MonthlyOutputLog> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                return query.OrderByDescending(x => x.DateTime).ThenBy(x => x.SequenceNo);

            return sortBy.ToLower() switch
            {
                "sequenceno" or "id" => ApplyOrder(query, x => x.SequenceNo, descending),
                "testcode" => ApplyOrder(query, x => x.TestCode, descending),
                "buyer" => ApplyOrder(query, x => x.Buyer, descending),
                "month" => ApplyOrder(query, x => x.Month, descending),
                "workgroup" => ApplyOrder(query, x => x.WorkGroup, descending),
                "volume" => ApplyOrder(query, x => x.Volume, descending),
                "datetime" or "dateimported" => ApplyOrder(query, x => x.DateTime, descending),
                "userid" => ApplyOrder(query, x => x.UserId, descending),
                "insertdelete" or "action" => ApplyOrder(query, x => x.InsertDelete, descending),
                _ => query.OrderByDescending(x => x.DateTime).ThenBy(x => x.SequenceNo)
            };
        }

        private static IQueryable ApplyOrder<T>(IQueryable<MonthlyOutputLog> query, Expression<Func<MonthlyOutputLog, T>> keySelector, bool descending)
        {
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }

        public async Task<bool> ExistsByTestCodeAndWorkGroupAsync(string testCode, string workGroup)
        {
            return await _context.MonthlyOutputs
                .AsNoTracking()
                .AnyAsync(m => m.TestCode == testCode && m.WorkGroup == workGroup);
        }
    }
}
