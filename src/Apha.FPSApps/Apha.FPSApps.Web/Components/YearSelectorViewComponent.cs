using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Web.Constants;
using Apha.FPSApps.Web.Handler;
using Apha.FPSApps.Web.Models.Components.YearSelector;
using Apha.Common.Utilities.StateManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Components
{
    public class YearSelectorViewComponent : ViewComponent
    {
        private readonly IYearMasterService _yearMasterService;
        private readonly IFpsYearContext _fyContext;
        private readonly IAppStateService _appStateService;

        public YearSelectorViewComponent(
            IYearMasterService yearMasterService,
            IFpsYearContext fyContext,
            IAppStateService appStateService)
        {
            _yearMasterService = yearMasterService;
            _fyContext = fyContext;
            _appStateService = appStateService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                // Reuse cache warmed by FpsYearMiddleware — no additional API call
                var allYears = await _appStateService.GetCacheValueAsync<IEnumerable<YearMasterDto>>(FpsCacheKeys.AllYears);
                if (allYears == null)
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

                // Get the selected year's status
                var selectedYear = allYears?.FirstOrDefault(y => y.FpsYear == _fyContext.Year);
                var selectedYearStatus = selectedYear?.YearStatus;

                return View(new YearSelectorViewModel
                {
                    Years = items,
                    SelectedYear = _fyContext.Year,
                    SelectedYearStatus = selectedYearStatus,
                    IsReadOnly = _fyContext.IsReadOnly
                });
            }
            catch
            {
                // API unavailable or token issue — render with empty list, no crash
                return View(new YearSelectorViewModel
                {
                    Years = [],
                    SelectedYear = _fyContext.Year,
                    SelectedYearStatus = null,
                    IsReadOnly = _fyContext.IsReadOnly
                });
            }
        }
    }
}