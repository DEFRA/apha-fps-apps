using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Apha.FPS.DataAccess.Repositories
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

            var result = await baseQuery.ToListAsync();
            return ApplyPaging(result, query.Page, query.PageSize);
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

        public async Task<MonthHour> SaveAsync(MonthHour monthHour)
        {
            var existing = await _context.MonthHours
                .FirstOrDefaultAsync(m =>
                    m.Year == monthHour.Year &&
                    m.Month == monthHour.Month &&
                    m.FpsYear == monthHour.FpsYear);

            if (existing is null)
            {
                _context.MonthHours.Add(monthHour);
            }
            else
            {
                existing.Days = monthHour.Days;
                existing.CvlHours = monthHour.CvlHours;
                existing.VidHours = monthHour.VidHours;
                existing.Fmonth = monthHour.Fmonth;
                _context.MonthHours.Update(existing);
            }

            await _context.SaveChangesAsync();
            return existing ?? monthHour;
        }

        public async Task<List<YearEndMonthHour>> GetYearEndMonthHoursAsync()
        {
            int openYear = await GetOpenYear();

            int? plannedYear = await GetPlannedYear();

            List<(int Year, int Month, int Fmonth)> monthHourKeys = GetMonthHourKeys(openYear);

            // Load records for Open + Planned
            var monthHours = await _context.MonthHours
                .AsNoTracking()
                .Where(m => m.FpsYear == openYear || m.FpsYear == plannedYear)
                .ToListAsync();

            // Remove duplicates and give Planned priority
            monthHours = monthHours
                .GroupBy(m => new { m.Year, m.Month, m.Fmonth })
                .Select(g =>
                    g.FirstOrDefault(x => x.FpsYear == plannedYear)
                    ?? g.First(x => x.FpsYear == openYear))
                .ToList();

            var result = GetYearEndMonthHour( openYear, plannedYear, monthHourKeys, monthHours);

            return result;
        }

        private static List<YearEndMonthHour> GetYearEndMonthHour(int openYear, int? plannedYear, List<(int Year, int Month, int Fmonth)> monthHourKeys, List<MonthHour> monthHours)
        {
            var result = new List<YearEndMonthHour>();

            foreach (var key in monthHourKeys)
            {
                var monthHour = monthHours.FirstOrDefault(m =>
                    m.FpsYear == key.Year &&
                    m.Month == key.Month &&
                    m.Fmonth == key.Fmonth);

                if (monthHour != null)
                {
                    result.Add(new YearEndMonthHour
                    {
                        Year = (short)(plannedYear.HasValue ? plannedYear.Value : openYear + 1),
                        Month = monthHour.Month,
                        Fmonth = monthHour.Fmonth,
                        Days = monthHour.Days,
                        CvlHours = monthHour.CvlHours,
                        VidHours = monthHour.VidHours,
                        FpsYear = (int)(plannedYear.HasValue ? plannedYear.Value : openYear + 1),
                        FpsYearType = monthHour.FpsYear == plannedYear ? "planned" : "open"
                    });
                }
                else
                {
                    result.Add(new YearEndMonthHour
                    {
                        Year = (short)(plannedYear.HasValue ? plannedYear.Value : openYear + 1),
                        Month = (short)key.Month,
                        Fmonth = (short)key.Fmonth,
                        Days = null,
                        CvlHours = null,
                        VidHours = null,
                        FpsYear = (int)(plannedYear.HasValue ? plannedYear.Value : openYear + 1),
                        FpsYearType = "new"
                    });
                }
            }

            return result;
        }

        private async Task<int> GetOpenYear()
        {
            var openFpsYears = await _context.YearMasters
                .AsNoTracking()
                .Where(y => y.Active && y.YearStatus.ToLower() == "open")
                .OrderByDescending(y => y.FpsYear)
                .Select(y => y.FpsYear)
                .FirstAsync();

            return openFpsYears;
        }

        private async Task<int?> GetPlannedYear()
        {
            var plannedFpsYears = await _context.YearMasters
                .AsNoTracking()
                .Where(y => y.Active && y.YearStatus.ToLower() == "planned")
                .OrderByDescending(y => y.FpsYear)
                .ToListAsync();

            var plannedYear = plannedFpsYears.FirstOrDefault()?.FpsYear;
            return plannedYear;
        }

        private static List<(int Year, int Month, int Fmonth)> GetMonthHourKeys(int openYear)
        {
            return new List<(int Year, int Month, int Fmonth)>
            {
                (openYear , 1, 0),
                (openYear , 2, 0),
                (openYear , 3, 0),
                (openYear , 4, 1),
                (openYear , 5, 2),
                (openYear , 6, 3),
                (openYear , 7, 4),
                (openYear , 8, 6),
                (openYear , 9, 6),
                (openYear , 10, 7),
                (openYear , 11, 8),
                (openYear , 12, 9),
                (openYear +1, 1, 10),
                (openYear +1, 2, 11),
                (openYear +1, 3, 2),
            };
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
