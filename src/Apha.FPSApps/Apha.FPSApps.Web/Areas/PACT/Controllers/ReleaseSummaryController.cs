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

        /// <summary>
        /// Initialises a new instance of <see cref="ReleaseSummaryController"/>.
        /// </summary>
        /// <param name="mapper">AutoMapper instance used to map DTOs to view-model types.</param>
        /// <param name="releaseSummaryService">Application service that retrieves and updates release summary data from the PACT API.</param>
        public ReleaseSummaryController(IMapper mapper, IReleaseSummaryService releaseSummaryService)
        {
            _mapper = mapper;
            _releaseSummaryService = releaseSummaryService;
        }

        /// <summary>
        /// Renders the Release Summaries page with a grid of all release periods.
        /// </summary>
        /// <returns>A <see cref="ViewResult"/> containing a <see cref="ReleaseSummaryViewModel"/> with the populated grid.</returns>
        public async Task<IActionResult> Index()
        {
            var response = await _releaseSummaryService.GetReleaseSummariesAsync();
            return View(new ReleaseSummaryViewModel
            {
                ReleaseSummaryGrid = MapToGridConfig(response.Data)
            });
        }

        /// <summary>
        /// Handles partial-page grid refreshes for the release summaries grid.
        /// </summary>
        /// <returns>A <see cref="PartialViewResult"/> rendering the <c>_DataGrid</c> partial with updated data.</returns>
        [HttpPost]
        public async Task<IActionResult> LoadReleaseSummaryGrid()
        {
            var response = await _releaseSummaryService.GetReleaseSummariesAsync();
            return PartialView("_DataGrid", MapToGridConfig(response.Data));
        }

        /// <summary>
        /// Updates the <c>FinalSummariesRun</c> flag for a release period.
        /// </summary>
        /// <param name="periodName">The period name (PK) to update.</param>
        /// <param name="finalSummariesRun">The new flag value (0 or 1).</param>
        /// <returns><c>200 OK</c> with the updated <c>finalSummariesRun</c> value on success; <c>400 Bad Request</c> on failure.</returns>
        [HttpPost]
        public async Task<IActionResult> SetFinalSummaryRun(string periodName, short finalSummariesRun)
        {
            var response = await _releaseSummaryService.SetFinalSummaryRunAsync(periodName, finalSummariesRun);
            if (response.Success)
                return Ok(response.Data?.FinalSummariesRun);

            return BadRequest(response.Errors);
        }

        private DataGridConfig<ReleasePeriodItem> MapToGridConfig(IReadOnlyList<ReleasePeriodDto>? data)
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