using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Web.Areas.PACT.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace Apha.FPSApps.Web.Areas.PACT.Controllers
{
    [Area("PACT")]
    [Authorize(Roles = "PACTAdmin,PACTUser")]
    [AuthorizeForScopes(ScopeKeySection = "PACTApiSettings:Scope")]
    public class ReleaseSummaryController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IReleaseSummaryService _releaseSummaryService;

        public ReleaseSummaryController(IMapper mapper, IReleaseSummaryService releaseSummaryService)
        {
            _mapper = mapper;
            _releaseSummaryService = releaseSummaryService;
        }

        /// <summary>
        /// Displays the Release Summary index page, including the current setting
        /// and a data grid populated with all available release periods.
        /// </summary>
        /// <returns>The Index view bound to a <see cref="ReleaseSummaryViewModel"/>.</returns>
        public async Task<IActionResult> Index()
        {
            var response = await _releaseSummaryService.GetReleaseSummariesAsync();
            return View(new ReleaseSummaryViewModel
            {
                Setting = response.Data?.Setting,
                ReleaseSummaryGrid = MapToGridConfig(response.Data?.ReleasePeriods)
            });
        }
       
        /// <summary>
        /// Fetches the latest release summary data and returns the <c>_DataGrid</c> partial view
        /// for use in AJAX-driven grid refresh scenarios.
        /// </summary>
        /// <returns>A partial view containing the refreshed release summary data grid.</returns>
        [HttpPost]
        public async Task<IActionResult> LoadReleaseSummaryGrid()
        {
            var response = await _releaseSummaryService.GetReleaseSummariesAsync();
            return PartialView("_DataGrid", MapToGridConfig(response.Data?.ReleasePeriods));
        }

        /// <summary>
        /// Updates the final summary run flag for the specified release period and optionally
        /// triggers an email notification.
        /// </summary>
        /// <param name="periodName">The name of the release period to update.</param>
        /// <param name="finalSummariesRun">The value indicating whether final summaries have been run.</param>
        /// <param name="sendEmail">A flag indicating whether an email notification should be sent.</param>
        /// <returns>
        /// <see cref="OkResult"/> containing the updated <c>FinalSummariesRun</c> value on success;
        /// otherwise <see cref="BadRequestResult"/> with the error details.
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> SetFinalSummaryRun(string? periodName = null, short? finalSummariesRun = null, string? sendEmail = null)
        {
            var response = await _releaseSummaryService.SetFinalSummaryRunAsync(periodName, finalSummariesRun, sendEmail);
            if (response.Success)
                return Ok(response.Data?.FinalSummariesRun);

            return BadRequest(response.Errors);
        }

        /// <summary>
        /// Maps a list of <see cref="ReleasePeriodDto"/> objects to a
        /// <see cref="DataGridConfig{T}"/> containing the grid configuration and row data.
        /// Returns an empty grid configuration when <paramref name="data"/> is <see langword="null"/>.
        /// </summary>
        /// <param name="data">The release period data to populate the grid with.</param>
        /// <returns>A <see cref="DataGridConfig{ReleasePeriodItem}"/> ready for rendering.</returns>
        private static DataGridConfig<ReleasePeriodItem> MapToGridConfig(IReadOnlyList<ReleasePeriodDto>? data)
        {
            var grid = ReleaseSummaryGridConfig();

            if (data is null)
                return grid;

            grid.Data = data.Select(p => new ReleasePeriodItem
            {
                PeriodName = p.PeriodName,
                StartPeriod = p.StartPeriod,
                EndPeriod = p.EndPeriod,
                FinalSummariesRun = p.FinalSummariesRun
            }).ToList();

            return grid;
        }

        /// <summary>
        /// Builds and returns the default <see cref="DataGridConfig{T}"/> for the release summary grid,
        /// including column definitions, grid identity, and behaviour flags.
        /// </summary>
        /// <returns>A pre-configured <see cref="DataGridConfig{ReleasePeriodItem}"/> instance.</returns>
        private static DataGridConfig<ReleasePeriodItem> ReleaseSummaryGridConfig() => new()
        {
            GridId = "releaseSummariesGrid",
            Title = string.Empty,
            BindGridUrl = "/PACT/ReleaseSummary/LoadReleaseSummaryGrid",
            KeyProperty = nameof(ReleasePeriodItem.PeriodName),
            ShowCheckboxColumn = false,
            AllowAdd = false,
            AllowEdit = false,
            AllowDelete = false,
            AllowExport = false,
            AllowRowSelection = false,
            ShowPagination = false,
            Columns = GridDataProvider.GetColumnsDefination<ReleasePeriodItem>()
        };
    }
}