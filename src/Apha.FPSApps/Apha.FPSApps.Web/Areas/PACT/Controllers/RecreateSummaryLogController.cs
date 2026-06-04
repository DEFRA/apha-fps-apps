using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
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
    [Authorize(Roles = "PACTAdmin,PACTUser")]
    [AuthorizeForScopes(ScopeKeySection = "PACTApiSettings:Scope")]
    public class RecreateSummaryLogController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IRecreateAndReleaseSummaryService _logService;

        /// <summary>
        /// Initialises a new instance of <see cref="RecreateSummaryLogController"/>.
        /// </summary>
        /// <param name="mapper">AutoMapper instance used to map pagination filters and row collections to their corresponding view-model types.</param>
        /// <param name="logService">Application service that retrieves recreate summaries log data from the PACT API.</param>
        public RecreateSummaryLogController(IMapper mapper, IRecreateAndReleaseSummaryService logService)
        {
            _mapper = mapper;
            _logService = logService;
        }

        /// <summary>
        /// Renders the Recreate Summaries Log page.
        /// Fetches the full recreate summaries log dataset, builds the initial data-grid
        /// configuration with no sort applied, and returns the view.
        /// </summary>
        /// <returns>
        /// A <see cref="ViewResult"/> containing a <see cref="RecreateSummaryLogViewModel"/>
        /// with the populated grid.
        /// </returns>
        public async Task<IActionResult> Index()
        {
            var query = _mapper.Map<QueryParameters<string>>(new PaginationFilter<string> { Filter = "{}" });
            var response = await _logService.GetRecreateSummaryLogAsync(query);

            return View(new RecreateSummaryLogViewModel
            {
                LogsGrid = MapToGridConfig(response, sortBy: null, descending: false)
            });
        }

        /// <summary>
        /// Handles partial-page grid refreshes triggered by the client-side data-grid component
        /// (<c>_DataGrid.cshtml</c>) whenever the user pages, sorts, or filters the grid.
        /// Maps the incoming pagination/filter/sort request to query parameters, fetches an updated
        /// page of recreate summaries log data, and returns only the <c>_DataGrid</c> partial
        /// view so the page can be updated in-place without a full reload.
        /// </summary>
        /// <param name="request">Pagination, sort, and column-filter parameters submitted by the grid via AJAX POST.</param>
        /// <returns>
        /// A <see cref="PartialViewResult"/> rendering the <c>_DataGrid</c> partial with an updated
        /// <see cref="DataGridConfig{RecreateSummaryLogItem}"/> model.
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> LoadRecreateSummariesLogGrid(PaginationFilter<string> request)
        {
            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _logService.GetRecreateSummaryLogAsync(query);

            return PartialView("_DataGrid", MapToGridConfig(response, request.SortBy, request.Descending));
        }

        /// <summary>
        /// Builds a <see cref="DataGridConfig{RecreateSummaryLogItem}"/> from the service response.
        /// When the response indicates failure or contains no data the default empty grid configuration
        /// is returned immediately. On success, the response rows are mapped to view-model rows and the
        /// pagination model is populated from the response metadata together with the supplied sort state.
        /// </summary>
        /// <param name="response">The API response containing log rows and pagination metadata.</param>
        /// <param name="sortBy">The column name to sort by, or <see langword="null"/> if no sort is active.</param>
        /// <param name="descending"><see langword="true"/> for a descending sort; <see langword="false"/> for ascending.</param>
        /// <returns>
        /// A fully populated <see cref="DataGridConfig{RecreateSummaryLogItem}"/>, or a default
        /// empty configuration when <paramref name="response"/> is unsuccessful or has no data.
        /// </returns>
        private DataGridConfig<RecreateSummaryLogItem> MapToGridConfig(
            ApiResponseDto<PaginatedResult<RecreateSummaryLogDto>> response,
            string? sortBy,
            bool descending)
        {
            var grid = RecreateSummariesLogGridConfig();

            if (!response.Success || response.Data is null)
                return grid;

            grid.Data = _mapper.Map<List<RecreateSummaryLogItem>>(response.Data.data);

            grid.Pagination = new PaginationModel
            {
                TotalRecords = response.Data.TotalCount,
                PageNumber = response.Data.PageNumber,
                PageSize = response.Data.PageSize,
                SortColumn = sortBy,
                SortDirection = descending
            };

            return grid;
        }

        /// <summary>
        /// Returns the static <see cref="DataGridConfig{RecreateSummaryLogItem}"/> skeleton shared by
        /// both <see cref="Index"/> and <see cref="LoadRecreateSummariesLogGrid"/>.
        /// The configuration defines the grid identity, bound AJAX URL, column definitions,
        /// and interaction flags; it intentionally contains no data or pagination state so
        /// callers can populate those fields independently after calling this method.
        /// </summary>
        /// <returns>A new <see cref="DataGridConfig{RecreateSummaryLogItem}"/> with static configuration applied.</returns>
        private static DataGridConfig<RecreateSummaryLogItem> RecreateSummariesLogGridConfig() => new()
        {
            GridId = "releaseLogsGrid",
            Title = string.Empty,
            BindGridUrl = "/PACT/RecreateSummaryLog/LoadRecreateSummariesLogGrid",
            ShowCheckboxColumn = false,
            AllowAdd = false,
            AllowEdit = false,
            AllowDelete = false,
            AllowExport = false,
            AllowRowSelection = false,
            ShowPagination = true,
            Columns = GridDataProvider.GetColumnsDefination<RecreateSummaryLogItem>()
        };
    }
}
