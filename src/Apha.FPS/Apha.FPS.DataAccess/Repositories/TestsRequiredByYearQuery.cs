using Apha.FPS.Core.Entities;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.DataAccess.Repositories
{
    /// <summary>
    /// Year-specific equivalent of fps.vqrytestsrequiredbywg_export / fps.vqrytestsrequiredbyrc_export.
    /// The views always use the latest released year; here the FPS year is supplied as a parameter.
    /// Each source table is read with a small year-filtered LINQ query and the Access query chain
    /// (qryLastYearsTests, qryLastYearsWGTests, qryTestsRequiredThisYear, qryTestsRequiredByWG_RCCost,
    /// qryTestsRequiredByWG) is reproduced in memory.
    /// </summary>
    public static class TestsRequiredByYearQuery
    {
        private sealed record YearWorkgroup(string WgKey, string WorkGroup, string? ProfitCentre);

        private sealed record TestRequired(string TestCode, double NoRequired, string? ItemDescription, decimal? UnitPriceVla);

        public static async Task<List<TestsRequiredByWgView>> GetByWorkGroupAsync(FpsDbContext context, string? profitCentre, int fpsYear)
        {
            var pc = profitCentre?.Trim() ?? string.Empty;

            var yearWorkgroups = (await context.Workgroups
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Where(w => w.FpsYear == fpsYear)
                    .Select(w => new { w.WorkGroupName, w.ProfitCentre })
                    .ToListAsync())
                .Select(w => new YearWorkgroup(Normalise(w.WorkGroupName), w.WorkGroupName, w.ProfitCentre))
                .ToList();
            var workgroupsByKey = yearWorkgroups.ToLookup(w => w.WgKey);

            var testsRequired = await GetTestsRequiredAsync(context, fpsYear);
            var (yearTotals, wgTotals) = await GetLastYearOutputAsync(context, fpsYear);
            var defaultWorkgroups = await GetDefaultWorkgroupsAsync(context, fpsYear, workgroupsByKey);
            var rcPrices = await GetRcPricesAsync(context, fpsYear, yearWorkgroups);

            var rows = new List<TestsRequiredByWgView>();

            foreach (var tr in testsRequired)
            {
                foreach (var (wg, unitPrice, projectedTotal) in ProjectTest(tr, yearTotals, wgTotals, defaultWorkgroups, rcPrices))
                {
                    if (wg is null)
                    {
                        continue;
                    }

                    foreach (var yw in workgroupsByKey[wg])
                    {
                        if (yw.ProfitCentre is null
                            || (pc.Length > 0 && !string.Equals(yw.ProfitCentre, pc, StringComparison.OrdinalIgnoreCase)))
                        {
                            continue;
                        }

                        rows.Add(new TestsRequiredByWgView
                        {
                            ProfitCentre = yw.ProfitCentre,
                            WorkGroup = yw.WorkGroup,
                            TestCode = tr.TestCode,
                            ItemDescription = tr.ItemDescription,
                            ProjectedTotal = projectedTotal,
                            UnitPrice = unitPrice
                        });
                    }
                }
            }

            return rows
                .OrderBy(r => r.ProfitCentre, StringComparer.Ordinal)
                .ThenBy(r => r.WorkGroup, StringComparer.Ordinal)
                .ThenBy(r => r.TestCode, StringComparer.Ordinal)
                .ToList();
        }

        public static async Task<List<TestsRequiredByRcView>> GetByResourceCentreAsync(FpsDbContext context, string? profitCentre, int fpsYear)
        {
            var wgRows = await GetByWorkGroupAsync(context, profitCentre, fpsYear);

            return wgRows
                .GroupBy(r => new { r.ProfitCentre, r.TestCode, r.ItemDescription, r.UnitPrice })
                .Select(g => new TestsRequiredByRcView
                {
                    ProfitCentre = g.Key.ProfitCentre,
                    TestCode = g.Key.TestCode,
                    ItemDescription = g.Key.ItemDescription,
                    ProjectedTotal = g.Sum(r => r.ProjectedTotal ?? 0),
                    UnitPrice = g.Key.UnitPrice
                })
                .OrderBy(r => r.ProfitCentre, StringComparer.Ordinal)
                .ThenBy(r => r.TestCode, StringComparer.Ordinal)
                .ToList();
        }

        /// <summary>
        /// qryTestsByWG: splits the requirement across last year's workgroups in proportion to their volume.
        /// Falls back to the default workgroup (first capable workgroup) when there is no history.
        /// </summary>
        private static IEnumerable<(string? Wg, decimal? UnitPrice, int ProjectedTotal)> ProjectTest(
            TestRequired tr,
            Dictionary<string, double?> yearTotals,
            ILookup<string, (string WorkGroup, double? Volume)> wgTotals,
            Dictionary<string, string> defaultWorkgroups,
            Dictionary<(string TestCode, string WgKey), decimal> rcPrices)
        {
            defaultWorkgroups.TryGetValue(tr.TestCode, out var defaultWg);

            if (!yearTotals.TryGetValue(tr.TestCode, out var yearTotal))
            {
                yield return (defaultWg, tr.UnitPriceVla, RoundHalfAwayFromZero(tr.NoRequired));
                yield break;
            }

            var total = yearTotal ?? 0;

            foreach (var lwg in wgTotals[tr.TestCode])
            {
                decimal? unitPrice = rcPrices.TryGetValue((tr.TestCode, lwg.WorkGroup), out var price)
                    ? price
                    : tr.UnitPriceVla;

                var projected = total == 0
                    ? RoundHalfAwayFromZero(tr.NoRequired)
                    : RoundHalfAwayFromZero(tr.NoRequired * (lwg.Volume ?? 0) / total + 0.49);

                yield return (lwg.WorkGroup, unitPrice, projected);
            }
        }

        /// <summary>qryTestsRequiredThisYear WHERE NoRequired &lt;&gt; 0, joined to TestOrProduct for description/price.</summary>
        private static async Task<List<TestRequired>> GetTestsRequiredAsync(FpsDbContext context, int fpsYear)
        {
            var requirements = await context.TestRequirements
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(t => t.FpsYear == fpsYear)
                .Select(t => new { t.TestCode, t.NoRequired })
                .ToListAsync();

            var products = (await context.TestOrProducts
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Where(p => p.FpsYear == fpsYear)
                    .Select(p => new { p.ItemCode, p.ItemDescription, p.UnitPriceVla })
                    .ToListAsync())
                .GroupBy(p => Normalise(p.ItemCode))
                .ToDictionary(
                    g => g.Key,
                    g => (Description: g.Select(p => p.ItemDescription).Where(d => d != null).Min(StringComparer.Ordinal),
                          UnitPrice: g.Min(p => p.UnitPriceVla)));

            return requirements
                .GroupBy(t => Normalise(t.TestCode))
                .Where(g => g.Any(t => t.NoRequired.HasValue))
                .Select(g =>
                {
                    products.TryGetValue(g.Key, out var product);
                    return new TestRequired(g.Key, g.Sum(t => t.NoRequired ?? 0), product.Description, product.UnitPrice);
                })
                .Where(t => t.NoRequired != 0)
                .ToList();
        }

        /// <summary>
        /// qryLastYearsTests / qryLastYearsWGTests over the 12 released months ending at the latest
        /// released month for the selected year (fnOneYearBackFromMonthReleased .. fnLatestYearMonthReleased).
        /// </summary>
        private static async Task<(Dictionary<string, double?> YearTotals, ILookup<string, (string WorkGroup, double? Volume)> WgTotals)>
            GetLastYearOutputAsync(FpsDbContext context, int fpsYear)
        {
            var release = await context.ArchiveYears
                .AsNoTracking()
                .Where(y => y.LatestMonthReleased != null && y.Year <= fpsYear)
                .OrderByDescending(y => y.Year)
                .Select(y => new { y.Year, Month = y.LatestMonthReleased!.Value })
                .FirstOrDefaultAsync();

            if (release is null)
            {
                return (new Dictionary<string, double?>(), Array.Empty<(string, string, double?)>()
                    .ToLookup(x => x.Item1, x => (x.Item2, x.Item3)));
            }

            var latestYm = release.Year * 100 + release.Month;
            var fromYm = release.Month == 12
                ? release.Year * 100 + 1
                : (release.Year - 1) * 100 + release.Month + 1;
            var fromYear = (short)(fromYm / 100);
            var latestYear = (short)release.Year;

            var output = (await context.ArchiveMonthlyOutputs
                    .AsNoTracking()
                    .Where(o => o.Year >= fromYear && o.Year <= latestYear)
                    .GroupBy(o => new { o.Year, o.Month, o.TestCode, o.WorkGroup })
                    .Select(g => new { g.Key.Year, g.Key.Month, g.Key.TestCode, g.Key.WorkGroup, Volume = g.Sum(o => o.Volume) })
                    .ToListAsync())
                .Where(o =>
                {
                    var ym = o.Year * 100 + (int)o.Month;
                    return ym >= fromYm && ym <= latestYm;
                })
                .Select(o => new { TestCode = Normalise(o.TestCode), WorkGroup = Normalise(o.WorkGroup), o.Volume })
                .ToList();

            var yearTotals = output
                .GroupBy(o => o.TestCode)
                .ToDictionary(g => g.Key, g => SumNullable(g.Select(o => o.Volume)));

            var wgTotals = output
                .GroupBy(o => new { o.TestCode, o.WorkGroup })
                .ToLookup(g => g.Key.TestCode, g => (g.Key.WorkGroup, SumNullable(g.Select(o => o.Volume))));

            return (yearTotals, wgTotals);
        }

        /// <summary>DLookUp on qryWGTestCapability: first capable workgroup (alphabetically) for each test.</summary>
        private static async Task<Dictionary<string, string>> GetDefaultWorkgroupsAsync(
            FpsDbContext context, int fpsYear, ILookup<string, YearWorkgroup> workgroupsByKey)
        {
            var capabilities = await context.TestCapabilities
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(c => c.FpsYear == fpsYear)
                .Select(c => new { c.TestCode, c.WorkGroup })
                .ToListAsync();

            return capabilities
                .Select(c => new { TestCode = Normalise(c.TestCode), WgKey = Normalise(c.WorkGroup) })
                .Where(c => workgroupsByKey.Contains(c.WgKey))
                .GroupBy(c => c.TestCode)
                .ToDictionary(g => g.Key, g => g.Select(c => c.WgKey).Min(StringComparer.Ordinal)!);
        }

        /// <summary>qryTestsRequiredByWG_RCCost: RC price per test and workgroup (via the workgroup's profit centre).</summary>
        private static async Task<Dictionary<(string TestCode, string WgKey), decimal>> GetRcPricesAsync(
            FpsDbContext context, int fpsYear, List<YearWorkgroup> yearWorkgroups)
        {
            var costs = await context.TestRCCosts
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(c => c.FpsYear == fpsYear)
                .Select(c => new { c.TestCode, c.ProfitCentre, c.Price })
                .ToListAsync();

            var workgroupsByPc = yearWorkgroups
                .Where(w => w.ProfitCentre != null)
                .ToLookup(w => w.ProfitCentre!);

            var prices = new Dictionary<(string, string), decimal>();
            foreach (var cost in costs)
            {
                foreach (var wg in workgroupsByPc[cost.ProfitCentre])
                {
                    prices.TryAdd((Normalise(cost.TestCode), wg.WgKey), cost.Price);
                }
            }

            return prices;
        }

        private static string Normalise(string? value) => (value ?? string.Empty).Trim().ToUpperInvariant();

        private static double? SumNullable(IEnumerable<double?> values)
        {
            double? sum = null;
            foreach (var v in values)
            {
                if (v.HasValue)
                {
                    sum = (sum ?? 0) + v.Value;
                }
            }
            return sum;
        }

        /// <summary>PostgreSQL round(numeric) semantics: half away from zero.</summary>
        private static int RoundHalfAwayFromZero(double value) =>
            (int)Math.Round((decimal)value, MidpointRounding.AwayFromZero);
    }
}
