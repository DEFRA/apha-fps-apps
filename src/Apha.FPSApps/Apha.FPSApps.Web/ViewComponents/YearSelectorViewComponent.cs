using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Web.Constants;
using Apha.FPSApps.Web.Handler;
using Apha.FPSApps.Web.Models.ViewComponents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;

namespace Apha.FPSApps.Web.ViewComponents
{
    public class YearSelectorViewComponent : ViewComponent
    {
        private readonly IYearMasterService _yearMasterService;
        private readonly IFpsYearContext _fyContext;
        private readonly IMemoryCache _cache;

        public YearSelectorViewComponent(
            IYearMasterService yearMasterService,
            IFpsYearContext fyContext,
            IMemoryCache cache)
        {
            _yearMasterService = yearMasterService;
            _fyContext = fyContext;
            _cache = cache;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                // Reuse cache warmed by FpsYearMiddleware — no additional API call
                if (!_cache.TryGetValue(FpsCacheKeys.AllYears, out IEnumerable<YearMasterDto>? allYears))
                {
                    var response = await _yearMasterService.GetAllFpsYearsAsync();
                    allYears = response?.Data;
                }

                var items = (allYears ?? [])
                    .Where(y => y.Active)
                    .OrderByDescending(y => y.FpsYear)
                    .Select(y => new SelectListItem(
                        text: y.FpsYearCode,
                        value: y.FpsYear.ToString(),
                        selected: y.FpsYear == _fyContext.Year))
                    .ToList();

                return View(new YearSelectorViewModel { Years = items, SelectedYear = _fyContext.Year });
            }
            catch
            {
                // API unavailable or token issue — render with empty list, no crash
                return View(new YearSelectorViewModel
                {
                    Years = [],
                    SelectedYear = _fyContext.Year
                });
            }
        }
    }
}