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
        { }

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
            await SavePlannedYearFmonthHoursAsync();

            var existing = await _context.MonthHours.IgnoreQueryFilters()
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

            var openmonthHours = await _context.MonthHours.IgnoreQueryFilters()
                .AsNoTracking()
                .Where(m => m.FpsYear == openYear && m.Fmonth > 0).ToListAsync();

            openmonthHours
                .Where(m => m.Fmonth == 10 || m.Fmonth == 11 || m.Fmonth == 12)
                .ToList()
                .ForEach(m => m.Fmonth = 0);

            var plannedHours = await _context.MonthHours.IgnoreQueryFilters()
                .AsNoTracking()
                .Where(m => m.FpsYear == plannedYear).ToListAsync();

            var monthHours = openmonthHours.Concat(plannedHours).ToList();

            // Remove duplicates: prefer the plannedYear record for each (Year, Month, Fmonth) slot.
            // Only keep the openYear record when no plannedYear record exists for that slot.
            monthHours = monthHours
                .GroupBy(m => new { m.Month, m.Fmonth })
                .Select(g =>
                {
                    var planned = plannedYear.HasValue
                        ? g.FirstOrDefault(x => x.FpsYear == plannedYear.Value)
                        : null;

                    // Prefer planned; fall back to open only when no planned record exists
                    return planned ?? g.First(x => x.FpsYear == openYear);
                }).ToList();

            var result = GetYearEndMonthHour(openYear, plannedYear, monthHours);

            return result;
        }

        private static List<YearEndMonthHour> GetYearEndMonthHour(int openYear, int? plannedYear, List<MonthHour> monthHours)
        {
            var result = new List<YearEndMonthHour>();
            List<(int Year, int Month, int Fmonth)> monthHourKeys = GetMonthHourKeys(openYear);

            foreach (var key in monthHourKeys)
            {
                var monthHour = monthHours.FirstOrDefault(m =>
                    m.Year == key.Year &&
                    m.Month == key.Month &&
                    m.Fmonth == key.Fmonth);

                if (monthHour != null)
                {
                    result.Add(new YearEndMonthHour
                    {
                        Year = monthHour.Year,
                        Month = monthHour.Month,
                        Fmonth = monthHour.Fmonth,
                        Days = monthHour.Days,
                        CvlHours = monthHour.CvlHours,
                        VidHours = monthHour.VidHours,
                        FpsYear = (int)(plannedYear.HasValue ? plannedYear.Value : openYear + 1),
                        ExistsForPlannedYear = (monthHour.Fmonth == 0 && monthHour.FpsYear < monthHour.Year)
                        ? "No"
                        : "Yes"
                    });
                }
                else
                {
                    monthHour = monthHours.FirstOrDefault(m =>
                    m.Year == key.Year - 1 &&
                    m.Month == key.Month &&
                    m.Fmonth == key.Fmonth);

                    if (monthHour != null)
                    {
                        result.Add(new YearEndMonthHour
                        {
                            Year = (short)(key.Year),
                            Month = monthHour.Month,
                            Fmonth = monthHour.Fmonth,
                            Days = monthHour.Days,
                            CvlHours = monthHour.CvlHours,
                            VidHours = monthHour.VidHours,
                            FpsYear = (int)(plannedYear.HasValue ? plannedYear.Value : openYear + 1),
                            ExistsForPlannedYear = "No"
                        });
                    }
                    else
                    {
                        result.Add(new YearEndMonthHour
                        {
                            Year = (short)(key.Year),
                            Month = (short)key.Month,
                            Fmonth = (short)key.Fmonth,
                            Days = null,
                            CvlHours = null,
                            VidHours = null,
                            FpsYear = (int)(plannedYear.HasValue ? plannedYear.Value : openYear + 1),
                            ExistsForPlannedYear = "No"
                        });
                    }
                }
            }

            return result;
        }

        private static List<(int Year, int Month, int Fmonth)> GetMonthHourKeys(int openYear)
        {
            var result = new List<(int Year, int Month, int Fmonth)>();

            for (int i = 0; i < 15; i++)
            {
                int year = openYear + 1 + (i / 12);
                int month = (i % 12) + 1;
                int fmonth = Math.Max(0, i - 2);

                result.Add((year, month, fmonth));
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

        private async Task SavePlannedYearFmonthHoursAsync()
        {
            var entity = new MonthHour();
            var result = await GetYearEndMonthHoursAsync();

            var filteredList = result.Where(x => x.Fmonth == 0
            && string.Equals(x.ExistsForPlannedYear, "no", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (filteredList.Count > 0)
            {
                foreach (var item in filteredList)
                {
                    var existing = await _context.MonthHours.IgnoreQueryFilters()
                        .FirstOrDefaultAsync(m => m.Year == item.Year
                        && m.Month == item.Month
                        && m.FpsYear == item.FpsYear);

                    if (existing == null)
                    {
                        if (existing is null)
                        {
                            entity.Year = item.Year;
                            entity.Month = item.Month;
                            entity.Days = item.Days;
                            entity.CvlHours = item.CvlHours;
                            entity.VidHours = item.VidHours;
                            entity.Fmonth = item.Fmonth;
                            entity.FpsYear = item.FpsYear;
                            _context.MonthHours.Add(entity);
                            await _context.SaveChangesAsync();
                        }
                    }
                }
            }
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
