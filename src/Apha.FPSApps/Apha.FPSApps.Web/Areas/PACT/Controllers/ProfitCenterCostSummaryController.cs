using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PACT.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace Apha.FPSApps.Web.Areas.PACT.Controllers
{
    [Area("PACT")]
    [Authorize(Roles = "FPSAdmin,FPSUser,PACTAdmin,PACTUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class ProfitCenterCostSummaryController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProfitCentreService _profitCentreService;
        private readonly IReleaseSummaryService _releaseSummaryService;

        public ProfitCenterCostSummaryController(
            IMapper mapper,
            IProfitCentreService profitCentreService,
            IReleaseSummaryService releaseSummaryService)
        {
            _mapper = mapper;
            _profitCentreService = profitCentreService;
            _releaseSummaryService = releaseSummaryService;
        }

        /// <summary>
        /// Renders the Profit Center Cost Summary page with period selector.
        /// Grid data is loaded dynamically when a period is selected via JavaScript.
        /// </summary>
        /// <returns>The Index view populated with period dropdown options.</returns>
        public async Task<IActionResult> Index()
        {
            var periodsList = await GetPeriodsAsync();

            var viewModel = new ProfitCenterCostSummaryViewModel
            {   
                PeriodMonths = periodsList,
                SelectedMonthNumber = null,
                CostGrid = null
            };

            return View(viewModel);
        }

        /// <summary>
        /// Reloads the profit center cost data grid partial view based on the supplied pagination, sort, and filter parameters.
        /// </summary>
        /// <param name="request">Pagination and filter parameters submitted from the data grid.</param>
        /// <param name="monthNumber">Month number to filter profit center costs.</param>
        /// <returns>A partial view containing the refreshed data grid, or <see cref="BadRequestResult"/> if the model state is invalid.</returns>
        [HttpPost]
        public async Task<IActionResult> LoadProfitCenterCostGrid(PaginationFilter<string> request, double monthNumber)
        {
            var gridConfig = await BuildProfitCenterCostGridAsync(request, monthNumber);
            return PartialView("_DataGrid", gridConfig);
        }

        /// <summary>
        /// API endpoint to retrieve all release periods as JSON.
        /// Used by client-side JavaScript for dynamic period loading.
        /// </summary>
        /// <returns>JSON array of release periods with period name and number.</returns>
        [HttpGet]
        public async Task<IActionResult> GetReleasePeriods()
        {
            var result = await _releaseSummaryService.GetReleasePeriodsAsync();

            if (result.Success && result.Data != null && result.Data.Count > 0)
            {
                var orderedPeriods = result.Data.OrderBy(p => p.EndPeriod).ToList();
                var periodItems = _mapper.Map<List<ReleasePeriodItem>>(orderedPeriods);
                return Json(periodItems);
            }

            return Json(new List<ReleasePeriodItem>());
        }

        /// <summary>
        /// Builds the profit center cost data grid configuration by fetching paged cost data from the service
        /// and applying any active pagination, sort, and filter state.
        /// </summary>
        /// <param name="request">Pagination and filter parameters for the grid query.</param>
        /// <param name="monthNumber">Month number to filter cost calculations.</param>
        /// <returns>A fully configured <see cref="DataGridConfig{T}"/> ready for rendering.</returns>
        private async Task<DataGridConfig<ProfitCenterCostItem>> BuildProfitCenterCostGridAsync(
            PaginationFilter<string> request, double monthNumber)
        {
            var grid = ProfitCenterCostGridConfig();
            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _profitCentreService.GetPagedProfitCenterCostSummaryAsync(query, monthNumber);

            grid.Data = response.Data != null ? _mapper.Map<List<ProfitCenterCostItem>>(response.Data.data) : [];

            // Extract pagination from PaginatedResult (response.Data)
            if (response.Data != null)
            {
                grid.Pagination = new PaginationModel
                {
                    PageNumber = response.Data.PageNumber,
                    PageSize = response.Data.PageSize,
                    TotalRecords = response.Data.TotalCount,
                    SortColumn = request.SortBy,
                    SortDirection = request.Descending
                };
            }
            else
            {
                grid.Pagination = new PaginationModel
                {
                    SortColumn = request.SortBy,
                    SortDirection = request.Descending
                };
            }

            // Update BindGridUrl with monthNumber
            grid.BindGridUrl = monthNumber == 0 
                ? "/PACT/ProfitCenterCostSummary/LoadProfitCenterCostGrid"
                : $"/PACT/ProfitCenterCostSummary/LoadProfitCenterCostGrid?monthNumber={monthNumber}";

            return grid;
        }

        /// <summary>
        /// Retrieves an ordered list of release periods formatted as PeriodMonth objects
        /// for use in period selector dropdown.
        /// </summary>
        /// <returns>An ordered list of PeriodMonth objects, or an empty list if none are available.</returns>
        private async Task<List<PeriodMonth>> GetPeriodsAsync()
        {
            var result = await _releaseSummaryService.GetReleasePeriodsAsync();

            if (result.Success && result.Data != null && result.Data.Count > 0)
            {
                var orderedPeriods = result.Data.OrderBy(p => p.EndPeriod).ToList();
                return _mapper.Map<List<PeriodMonth>>(orderedPeriods);
            }

            return [];
        }

        /// <summary>
        /// Returns the static <see cref="DataGridConfig{ProfitCenterCostItem}"/> skeleton shared by
        /// both <see cref="Index"/> and <see cref="LoadProfitCenterCostGrid"/>.
        /// The configuration defines the grid identity, bound AJAX URL, column definitions,
        /// and interaction flags; it intentionally contains no data or pagination state so
        /// callers can populate those fields independently after calling this method.
        /// </summary>
        /// <returns>A new <see cref="DataGridConfig{ProfitCenterCostItem}"/> with static configuration applied.</returns>
        private static DataGridConfig<ProfitCenterCostItem> ProfitCenterCostGridConfig() => new()
        {
            GridId = "profitCenterCostGrid",
            Title = "",
            KeyProperty = "ProfitCentre",
            BindGridUrl = "/PACT/ProfitCenterCostSummary/LoadProfitCenterCostGrid",
            ExtraFilterMethod = "getProfitCenterCostGridExtraFilters",
            ShowCheckboxColumn = false,
            AllowAdd = false,
            AllowEdit = false,
            AllowDelete = false,
            AllowExport = false,
            AllowRowSelection = false,
            ShowPagination = true,
            Columns = GridDataProvider.GetColumnsDefination<ProfitCenterCostItem>()
        };
    }
}
