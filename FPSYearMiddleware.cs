using Apha.FPSApps.Web.Handler;
using Apha.FPSApps.Web.Enums;
using System.Linq;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Dtos.FPS;
using Microsoft.Extensions.Caching.Memory;

namespace Apha.FPSApps.Web.Middleware
{
    public class FpsYearMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private const string ExecutedKey = "FpsYearMiddleware.Executed";
        private const string YearsCacheKey = "FpsYearMiddleware.AllYears";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

        public FpsYearMiddleware(RequestDelegate next, IMemoryCache cache)
        {
            _next = next;
            _cache = cache;
        }

        public async Task Invoke(
            HttpContext context,
            IFpsYearContext fyContext,
            IYearMasterService yearMasterService)
        {
            // Skip static assets (fonts, css, js, images, etc.) and prevent double-execution
            var path = context.Request.Path.Value;
            if ((path is not null && Path.HasExtension(path)) ||
                context.Items.ContainsKey(ExecutedKey))
            {
                await _next(context);
                return;
            }

            context.Items[ExecutedKey] = true;

            var tempYear = GetCurrentFPSYear();
            context.Items["SelectedFPSYear"] = tempYear;

            int year = tempYear;
            YearStatus yearStatus = YearStatus.Closed;

            try
            {
                var allYears = await GetCachedYearsAsync(yearMasterService);

                if (allYears != null)
                {
                    year = ResolveYear(context, allYears, tempYear);

                    var selectedYear = allYears.FirstOrDefault(y => y.FpsYear == year);
                    yearStatus = selectedYear?.YearStatus is null
                        ? YearStatus.Closed
                        : Enum.Parse<YearStatus>(selectedYear.YearStatus, ignoreCase: true);
                }
            }
            catch
            {
                year = tempYear;
            }

            fyContext.Year = year;
            context.Items["SelectedFPSYear"] = year;

            var area = context.GetRouteData()?.Values["area"]?.ToString();
            fyContext.IsReadOnly = ResolveIsReadOnly(area, yearStatus);

            await _next(context);
        }

        private async Task<IEnumerable<YearMasterDto>?> GetCachedYearsAsync(IYearMasterService yearMasterService)
        {
            if (_cache.TryGetValue(YearsCacheKey, out IEnumerable<YearMasterDto>? cached))
                return cached;

            var response = await yearMasterService.GetAllFpsYearsAsync();
            var data = response?.Data;

            // Only cache successful responses — avoid persisting nulls/errors
            if (data != null)
                _cache.Set(YearsCacheKey, data, CacheDuration);

            return data;
        }

        private static int ResolveYear(HttpContext context, IEnumerable<YearMasterDto> allYears, int fallback)
        {
            if (context.Request.Query.TryGetValue("year", out var q) && !string.IsNullOrEmpty(q))
                return int.Parse(q!);

            if (context.Request.Headers.TryGetValue("X-FPS-Year", out var h) && !string.IsNullOrEmpty(h))
                return int.Parse(h!);

            if (context.Request.HasFormContentType &&
                context.Request.Form.TryGetValue("FPSYear", out var f) && !string.IsNullOrEmpty(f))
                return int.Parse(f!);

            return allYears.FirstOrDefault(y => y.YearStatus?.Equals("Open", StringComparison.OrdinalIgnoreCase) == true)
                           ?.FpsYear ?? fallback;
        }

        private static bool ResolveIsReadOnly(string? area, YearStatus yearStatus) =>
            area?.ToUpperInvariant() switch
            {
                "FPS"  => yearStatus is not (YearStatus.Planned or YearStatus.Open),
                "PACT" => yearStatus != YearStatus.Open,
                _      => false
            };

        private static int GetCurrentFPSYear()
        {
            var today = DateTime.Today;
            return today.Month >= 4 ? 2025 : today.Year - 1;
        }
    }
}
