using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Apha.PACT.DataAccess.Repository
{
    public class MonthHourRepository : BaseRepository, IMonthHourRepository
    {
        public MonthHourRepository(FpsDbContext context) : base(context)
        {
        }

        public async Task<PagedData<MonthHour>> GetAllAsync(PaginationParameters<string> query)
        {
            var baseQuery = _context.MonthHours.AsNoTracking();

            baseQuery = ApplyMonthHourFilter(baseQuery, query.Filter);

            var sortBy = query.SortBy is nameof(MonthHour.Year)
                or nameof(MonthHour.Month)
                or nameof(MonthHour.Days)
                or nameof(MonthHour.CvlHours)
                or nameof(MonthHour.VidHours)
                ? query.SortBy
                : nameof(MonthHour.Year);

            baseQuery = query.Descending
                ? baseQuery.OrderByDescending(e => EF.Property<object>(e, sortBy)).ThenByDescending(e => e.Month)
                : baseQuery.OrderBy(e => EF.Property<object>(e, sortBy)).ThenBy(e => e.Month);
           
            return await ApplyPaging(baseQuery, query.Page, query.PageSize);
        }

        public async Task<IEnumerable<MonthHour>> GetByYearAsync(short year)
        {
            return await _context.MonthHours
                .AsNoTracking()
                .Where(m => m.Year == year)
                .OrderBy(m => m.Month)
                .ToListAsync();
        }

        public async Task<IEnumerable<short>> GetDistinctYearsAsync()
        {
            return await _context.MonthHours
                .AsNoTracking()
                .Select(m => m.Year)
                .Distinct()
                .OrderBy(y => y)
                .ToListAsync();
        }

        private static IQueryable<MonthHour> ApplyMonthHourFilter(
            IQueryable<MonthHour> query, string? filterJson)
        {
            if (string.IsNullOrWhiteSpace(filterJson)) return query;

            var filters = JsonConvert.DeserializeObject<Dictionary<string, string>>(filterJson);
            if (filters is null) return query;

            if (filters.TryGetValue(nameof(MonthHour.Year), out var year) && short.TryParse(year, out var parsedYear))
                query = query.Where(m => m.Year == parsedYear);

            if (filters.TryGetValue(nameof(MonthHour.Month), out var month) && short.TryParse(month, out var parsedMonth))
                query = query.Where(m => m.Month == parsedMonth);

            return query;
        }
    }
}
