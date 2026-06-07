using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PACT.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.Areas.PACT.Controllers
{
    [Area("PACT")]
    [Authorize(Roles = "FPSAdmin,FPSUser,PACTAdmin,PACTUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class ProfitCenterCostSummaryController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IFpsProfitCentreApiClient _profitCentreApiClient;
        private readonly ICalenderMonthService _calenderMonthService;

        public ProfitCenterCostSummaryController(
            IMapper mapper,
            IFpsProfitCentreApiClient profitCentreApiClient,
            ICalenderMonthService calenderMonthService)
        {
            _mapper = mapper;
            _profitCentreApiClient = profitCentreApiClient;
            _calenderMonthService = calenderMonthService;
        }

        /// <summary>
        /// Renders the Profit Center Cost Summary page with period selector and data grid.
        /// </summary>
        /// <param name="monthNumber">Optional month number to pre-filter the grid.</param>
        /// <returns>The Index view populated with period dropdown options and grid configuration.</returns>
        public async Task<IActionResult> Index(short? monthNumber = null)
        {
            var periodsList = await GetPeriodsListAsync();

            var defaultRequest = new PaginationFilter<string> { Filter = "{}" };
            var grid = await BuildProfitCenterCostGridAsync(defaultRequest, monthNumber);

            var viewModel = new ProfitCenterCostSummaryViewModel
            {
                Periods = periodsList,
                CostGrid = grid
            };

            return View(viewModel);
        }

        /// <summary>
        /// Reloads the profit center cost data grid partial view based on the supplied pagination, sort, and filter parameters.
        /// </summary>
        /// <param name="request">Pagination and filter parameters submitted from the data grid.</param>
        /// <param name="monthNumber">Optional month number to filter profit center costs.</param>
        /// <returns>A partial view containing the refreshed data grid, or <see cref="BadRequestResult"/> if the model state is invalid.</returns>
        [HttpPost]
        public async Task<IActionResult> LoadProfitCenterCostGrid(PaginationFilter<string> request, short? monthNumber = null)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });

            var gridConfig = await BuildProfitCenterCostGridAsync(request, monthNumber);
            return PartialView("_DataGrid", gridConfig);
        }

        /// <summary>
        /// Gets profit centers with their aggregated costs, optionally filtered by month.
        /// </summary>
        /// <param name="monthNumber">Optional month number to filter the cost calculations.</param>
        /// <returns>A JSON response containing profit center cost data, or <see cref="BadRequestResult"/> if the request fails.</returns>
        [HttpGet]
        public async Task<IActionResult> GetProfitCenterCostData([FromQuery] short? monthNumber = null)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });

            var response = await _profitCentreApiClient.GetProfitCenterCostSummaryAsync(monthNumber);

            if (response.Success && response.Data != null)
            {
                return Ok(response.Data);
            }

            return BadRequest(new { errors = response.Errors });
        }

        /// <summary>
        /// Builds the profit center cost data grid configuration by fetching paged cost data from the service
        /// and applying any active pagination, sort, and filter state.
        /// </summary>
        /// <param name="request">Pagination and filter parameters for the grid query.</param>
        /// <param name="monthNumber">Optional month number to filter cost calculations.</param>
        /// <returns>A fully configured <see cref="DataGridConfig{T}"/> ready for rendering.</returns>
        private async Task<DataGridConfig<ProfitCenterCostItem>> BuildProfitCenterCostGridAsync(
            PaginationFilter<string> request, short? monthNumber = null)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? new Dictionary<string, string>();

            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _profitCentreApiClient.GetPagedProfitCenterCostSummaryAsync(query, monthNumber);

            var items = response.Success && response.Data != null
                ? _mapper.Map<List<ProfitCenterCostItem>>(response.Data)
                : [];

            var pagination = response.Pagination != null
                ? _mapper.Map<PaginationModel>(response.Pagination)
                : new PaginationModel();
            pagination.SortColumn = request.SortBy;
            pagination.SortDirection = request.Descending;

            var queryParams = new List<string>();
            if (monthNumber.HasValue)
                queryParams.Add($"monthNumber={monthNumber.Value}");

            string queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : string.Empty;

            return new DataGridConfig<ProfitCenterCostItem>
            {
                GridId = "profitCenterCostGrid",
                Title = "Profit Center Costs",
                KeyProperty = "ProfitCentre",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                BindGridUrl = $"/PACT/ProfitCenterCostSummary/LoadProfitCenterCostGrid{queryString}",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<ProfitCenterCostItem>(),
                Pagination = pagination,
                CurrentFilters = filterDict
            };
        }

        /// <summary>
        /// Retrieves an ordered list of all calendar months formatted as <see cref="SelectListItem"/> entries
        /// for use in period selector dropdown.
        /// </summary>
        /// <returns>An ordered list of period select items in the format "number - name", or an empty list if none are available.</returns>
        private async Task<List<SelectListItem>> GetPeriodsListAsync()
        {
            var result = await _calenderMonthService.GetCalenderMonthsAsync();

            if (result.Success && result.Data != null && result.Data.Count > 0)
            {
                return result.Data
                    .OrderBy(m => m.MonthNumber)
                    .Select(m => new SelectListItem
                    {
                        Value = m.MonthNumber.ToString(),
                        Text = $"{m.MonthNumber} - {m.MonthName}"
                    })
                    .ToList();
            }

            return [];
        }
    }
}
