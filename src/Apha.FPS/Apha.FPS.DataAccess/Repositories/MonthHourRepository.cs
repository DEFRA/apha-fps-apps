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

            List<(int Year, int Month, int Fmonth)> monthHourKeys = GetMonthHourKeys(openYear);

            // Load records for Open + Planned
            var monthHours = await _context.MonthHours.IgnoreQueryFilters()
                .AsNoTracking()
                .Where(m => m.FpsYear == openYear || m.FpsYear == plannedYear)
                .ToListAsync();

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
                })
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
                        ExistsForPlannedYear = "Yes"
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
            int keyYear = openYear + 1;
            return new List<(int Year, int Month, int Fmonth)>
            {
                (keyYear , 1, 0),
                (keyYear , 2, 0),
                (keyYear , 3, 0),
                (keyYear , 4, 1),
                (keyYear , 5, 2),
                (keyYear , 6, 3),
                (keyYear , 7, 4),
                (keyYear , 8, 5),
                (keyYear , 9, 6),
                (keyYear , 10, 7),
                (keyYear , 11, 8),
                (keyYear , 12, 9),
                (keyYear+1, 1, 10),
                (keyYear+1, 2, 11),
                (keyYear+1, 3, 12),
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
