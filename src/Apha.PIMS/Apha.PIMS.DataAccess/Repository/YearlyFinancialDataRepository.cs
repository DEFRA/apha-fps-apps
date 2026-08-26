using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using Apha.PIMS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Text.Json;

namespace Apha.PIMS.DataAccess.Repository
{
    public class YearlyFinancialDataRepository : BaseRepository, IYearlyFinancialDataRepository
    {
        public YearlyFinancialDataRepository(PimsDbContext context) : base(context)
        {
        }

        public async Task<PagedData<YearlyFinancialData>> GetAllAsync(
            string project,
            PaginationParameters<string> paging)
        {
            IQueryable<YearlyFinancialData> query = _context.YearlyFinancialData
                .AsNoTracking()
                .Where(e => e.Project == project);

            query = ApplyFilter(query, paging.Filter);
            query = ApplySearch(query, paging.Search);
            query = ApplySorting(query, paging.SortBy, paging.Descending);

            return await ApplyPaging(query, paging.Page, paging.PageSize);
        }

        public async Task<YearlyFinancialData?> GetByKeyAsync(short year, string project)
        {
            return await _context.YearlyFinancialData
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Year == year && e.Project == project);
        }

       
        public async Task<bool> ExistsAsync(short year, string project)
        {
            return await _context.YearlyFinancialData
                .AsNoTracking()
                .AnyAsync(e => e.Year == year && e.Project == project);
        }

       
        public async Task<YearlyFinancialData> CreateAsync(YearlyFinancialData entity)
        {
            await _context.YearlyFinancialData.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<YearlyFinancialData> UpdateAsync(YearlyFinancialData entity)
        {
            _context.YearlyFinancialData.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        
        public async Task<bool> DeleteAsync(short year, string project)
        {
            int affected = await _context.YearlyFinancialData
                .Where(e => e.Year == year && e.Project == project)
                .ExecuteDeleteAsync();

            return affected > 0;
        }

      
        public async Task<IReadOnlyList<PactProjectYearCosts>> GetPactCostsAsync(
            string project,
            short year)
        {
            
            ProjectRadTrackData? rtd = await _context.ProjectRadTrackData
                .AsNoTracking()
                .Where(r => r.Parentproject == project)
                .FirstOrDefaultAsync();

           
            List<ProjectMonthFinal> allMonths = await _context.ProjectMonthFinals
                .AsNoTracking()
                .Where(pmf => pmf.Project == project)
                .ToListAsync();

           
            static short DeriveFiscalYear(ProjectMonthFinal pmf, ProjectRadTrackData? rtd)
            {
                if (rtd is { Useprojectyear: -1 } && rtd.Startdate.HasValue)
                {
                    int shift = (int)pmf.Monthno + 3 - rtd.Startdate.Value.Month;
                    return (short)new DateTime(pmf.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(shift).Year;
                }
                return pmf.Year;
            }

            List<ProjectMonthFinal> monthsForYear = allMonths
                .Where(pmf => DeriveFiscalYear(pmf, rtd) == year)
                .ToList();

            if (monthsForYear.Count == 0)
                return Array.Empty<PactProjectYearCosts>();

            IEnumerable<short>  calendarYears = monthsForYear.Select(m => m.Year).Distinct();
            IEnumerable<double> monthNos      = monthsForYear.Select(m => m.Monthno).Distinct();

            List<TimeCostCalcs> tccRows = await _context.TimeCostCalcs
                .AsNoTracking()
                .Where(t => t.Project == project
                         && calendarYears.Contains(t.Year)
                         && monthNos.Contains(t.Month))
                .ToListAsync();

           
            var tccLookup = tccRows
                .GroupBy(t => (t.Year, t.Month))
                .ToDictionary(
                    g => g.Key,
                    g => (
                        Pay:      g.Sum(t => t.Pay      ?? 0m),
                        NonPayOH: g.Sum(t => (t.Nonpay  ?? 0m) + (t.Overhead ?? 0m))
                    ));

            // ── 4. Aggregate per monthno into PactProjectYearCosts rows ──────────────
            List<PactProjectYearCosts> rows = monthsForYear
                .GroupBy(pmf => pmf.Monthno)
                .OrderBy(g => g.Key)
                .Select(g =>
                {
                    short calYear = g.First().Year;
                    tccLookup.TryGetValue((calYear, g.Key), out var tcc);

                    return new PactProjectYearCosts
                    {
                        Project      = project,
                        Year         = (double)year,
                        MonthNo      = g.Key,
                        SubContracts = g.Sum(pmf => pmf.Subcontracts  ?? 0m),
                        Animals      = g.Sum(pmf => pmf.Animals       ?? 0m),
                        Tests        = g.Sum(pmf => pmf.Transfercosts ?? 0m),
                        Pay          = tcc.Pay,
                        NonPayOH     = tcc.NonPayOH,
                        Hours        = g.Sum(pmf => pmf.Totalhours    ?? 0d),
                        TotalCosts   = g.Sum(pmf => pmf.Totalcost     ?? 0m),
                        TimeCost     = g.Sum(pmf => pmf.Timecosts     ?? 0m),
                    };
                })
                .ToList();

           
            Projects? proj = await _context.MyTlkpProjects
                .AsNoTracking()
                .Where(p => p.Parentproject == project && p.Year == year)
                .FirstOrDefaultAsync();

            decimal? custIncome = proj?.Custincome;
            decimal? budgetCvl  = proj?.BudgetCvl;

            foreach (PactProjectYearCosts row in rows)
            {
                row.CustIncome = custIncome;
                row.BudgetCvl  = budgetCvl;
            }

            return rows.AsReadOnly();
        }

        public async Task<string?> GetSettingValueByIdAsync(string id)
        {
            return await _context.DatabaseSettings
                .AsNoTracking()
                .Where(s => s.Id == id)
                .Select(s => s.Setting)
                .FirstOrDefaultAsync();
        }

        // ─── Private helpers ────────────────────────────────────────────────────────

      
        private static IQueryable<YearlyFinancialData> ApplyFilter(
            IQueryable<YearlyFinancialData> query,
            string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter) || filter == "{}")
            {
                return query;
            }

            try
            {
                using JsonDocument doc = JsonDocument.Parse(filter);
                if (!doc.RootElement.TryGetProperty("Year", out JsonElement yearElement)
                    && !doc.RootElement.TryGetProperty("year", out yearElement))
                {
                    return query;
                }

                string? yearText = yearElement.ValueKind == JsonValueKind.String
                    ? yearElement.GetString()
                    : yearElement.ToString();

                if (short.TryParse(yearText, out short year))
                {
                    query = query.Where(e => e.Year == year);
                }
            }
            catch (JsonException)
            {
                return query;
            }

            return query;
        }

        private static IQueryable<YearlyFinancialData> ApplySearch(
            IQueryable<YearlyFinancialData> query,
            string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return query;
            }

            string s = search.Trim();
            return query.Where(e => e.Year.ToString().Contains(s));
        }

        private static IQueryable<YearlyFinancialData> ApplySorting(
            IQueryable<YearlyFinancialData> query,
            string? sortBy,
            bool descending)
        {
            string key = (sortBy ?? string.Empty).Trim().ToLowerInvariant();

            return key switch
            {
                "year" => ApplyOrder(query, e => e.Year, descending),
                "project" => ApplyOrder(query, e => e.Project, descending),

                "bfbudget" => ApplyOrder(query, e => e.BfBudget, descending),
                "pp/acc" => ApplyOrder(query, e => e.BfBudget, descending),

                "pybudget" => ApplyOrder(query, e => e.PyBudget, descending),
                "customer income" => ApplyOrder(query, e => e.PyBudget, descending),

                "vlabudget" => ApplyOrder(query, e => e.VlaBudget, descending),
                "vla budget" => ApplyOrder(query, e => e.VlaBudget, descending),

                "actualexpenditure" => ApplyOrder(query, e => e.ActualExpenditure, descending),
                "actual exp" => ApplyOrder(query, e => e.ActualExpenditure, descending),

                "seedcorn" => ApplyOrder(query, e => e.Seedcorn, descending),
                "manhours" => ApplyOrder(query, e => e.ManHours, descending),
                "man hours" => ApplyOrder(query, e => e.ManHours, descending),

                "paycosts" => ApplyOrder(query, e => e.PayCosts, descending),
                "pay costs" => ApplyOrder(query, e => e.PayCosts, descending),

                "nonpayohcosts" => ApplyOrder(query, e => e.NonPayOhCosts, descending),
                "non-pay & oh" => ApplyOrder(query, e => e.NonPayOhCosts, descending),

                "testcosts" => ApplyOrder(query, e => e.TestCosts, descending),
                "test costs" => ApplyOrder(query, e => e.TestCosts, descending),

                "nonanimalcosts" => ApplyOrder(query, e => e.NonAnimalCosts, descending),
                "project specific" => ApplyOrder(query, e => e.NonAnimalCosts, descending),

                "animalcosts" => ApplyOrder(query, e => e.AnimalCosts, descending),
                "animal costs" => ApplyOrder(query, e => e.AnimalCosts, descending),

                "adjustment" => ApplyOrder(query, e => e.Adjustment, descending),
                "exc/adj" => ApplyOrder(query, e => e.Adjustment, descending),

                "adjustmentcomment" => ApplyOrder(query, e => e.AdjustmentComment, descending),
                "adj comment" => ApplyOrder(query, e => e.AdjustmentComment, descending),

                "mandays" => ApplyOrder(query, e => e.ManDays, descending),
                "manyears" => ApplyOrder(query, e => e.ManYears, descending),
                "actualmanyears" => ApplyOrder(query, e => e.ActualManYears, descending),
                "locked" => ApplyOrder(query, e => e.Locked, descending),
                "datecosted" => ApplyOrder(query, e => e.DateCosted, descending),
                "costedby" => ApplyOrder(query, e => e.CostedBy, descending),
                _ => query.OrderBy(e => e.Year).ThenBy(e => e.Project)
            };
        }

        private static IQueryable<T> ApplyOrder<T, TKey>(
            IQueryable<T> query,
            Expression<Func<T, TKey>> keySelector,
            bool descending)
            => descending
               ? query.OrderByDescending(keySelector)
               : query.OrderBy(keySelector);
    }
}
