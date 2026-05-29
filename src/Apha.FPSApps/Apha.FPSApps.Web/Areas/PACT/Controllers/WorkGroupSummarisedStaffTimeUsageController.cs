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
    public class WorkGroupSummarisedStaffTimeUsageController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IWorkGroupService _workGroupService;

        /// <summary>
        /// Initialises a new instance of <see cref="WorkGroupSummarisedStaffTimeUsageController"/>.
        /// </summary>
        /// <param name="mapper">AutoMapper instance used to map pagination filters, row collections, and summary DTOs to their corresponding view-model types.</param>
        /// <param name="workGroupService">Application service that retrieves summarised staff time-usage data from the PACT API.</param>
        public WorkGroupSummarisedStaffTimeUsageController(IMapper mapper, IWorkGroupService workGroupService)
        {
            _mapper = mapper;
            _workGroupService = workGroupService;
        }


        /// <summary>
        /// Renders the Work Group Time By Job Code page .
        /// Fetches the full summarised staff time-usage dataset for the supplied work group,
        /// builds the initial data-grid configuration with no sort applied, computes the
        /// pre-aggregated footer summary, and populates <c>ViewBag.JobTitleLookup</c> with a
        /// server-side dictionary that the client-side row-selection handler uses to resolve
        /// job titles without additional round-trips.
        /// </summary>
        /// <param name="workGroup">Work group name whose time-usage data should be displayed.</param>
        /// <param name="personName">Person name passed in from the Work Group People page; stored on
        /// the view model so the view can pre-select the correct person.</param>
        /// <returns>
        /// A <see cref="ViewResult"/> containing a <see cref="WgSummarisedStaffTimeUsageViewModel"/>
        /// with the populated grid, summary footer, header fields, and job-title lookup.
        /// </returns>
        public async Task<IActionResult> Index(string workGroup, string personName)
        {
            var query = _mapper.Map<QueryParameters<string>>(new PaginationFilter<string> { Filter = "{}" });
            var response = await _workGroupService.GetWgSummarisedStaffTimeUsageAsync(query, workGroup);

            ViewBag.JobTitleLookup = response.Data?.JobTitleLookup
                .ToDictionary(x => x.JobCode, x => x.JobTitle)
                ?? new Dictionary<string, string>();
            return View(new WgSummarisedStaffTimeUsageViewModel
            {
                SelectedWorkGroup = workGroup,
                SelectedPersonName = personName,
                WorkGroupName = workGroup,
                HrsPaid = response.Data?.HrsPaid ?? 0,
                Grid = MapToGridConfig(response, sortBy: null, descending: false),
                Summary = MapToSummary(response)
            });
        }

        /// <summary>
        /// Handles partial-page grid refreshes triggered by the client-side data-grid component
        /// (<c>_DataGrid.cshtml</c>) whenever the user pages, sorts, or filters the grid.
        /// Maps the incoming pagination/filter/sort request to query parameters, fetches an updated
        /// page of summarised staff time-usage data, and returns only the <c>_DataGrid</c> partial
        /// view so the page can be updated in-place without a full reload.
        /// After the partial is injected the grid fires a <c>gridReloaded</c> custom event which
        /// <c>work-group-summarised-staffTimeUsage.js</c> listens to in order to auto-select the first row.
        /// </summary>
        /// <param name="request">Pagination, sort, and column-filter parameters submitted by the grid via AJAX POST.</param>
        /// <param name="workGroup">Work group name used to scope the data query.
        /// Must not be <see langword="null"/>, empty, or whitespace.</param>
        /// <returns>
        /// A <see cref="PartialViewResult"/> rendering the <c>_DataGrid</c> partial with an updated
        /// <see cref="DataGridConfig{WgSummarisedStaffTimeUsageRow}"/> model.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="workGroup"/> is <see langword="null"/>, empty, or whitespace.
        /// </exception>
        [HttpPost]
        public async Task<IActionResult> LoadSummarisedStaffTimeGrid(PaginationFilter<string> request, string workGroup)
        {
            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _workGroupService.GetWgSummarisedStaffTimeUsageAsync(query, workGroup);

            return PartialView("_DataGrid", MapToGridConfig(response, request.SortBy, request.Descending));
        }

        /// <summary>
        /// Builds a <see cref="DataGridConfig{WgSummarisedStaffTimeUsageRow}"/> from the service response.
        /// When the response indicates failure or contains no data the default empty grid configuration
        /// is returned immediately.  On success, the response rows are mapped to view-model rows and the
        /// pagination model is populated from the response metadata together with the supplied sort state.
        /// </summary>
        /// <param name="response">The API response containing pivot rows and pagination metadata.</param>
        /// <param name="sortBy">The column name to sort by, or <see langword="null"/> if no sort is active.</param>
        /// <param name="descending"><see langword="true"/> for a descending sort; <see langword="false"/> for ascending.</param>
        /// <returns>
        /// A fully populated <see cref="DataGridConfig{WgSummarisedStaffTimeUsageRow}"/>, or a default
        /// empty configuration when <paramref name="response"/> is unsuccessful or has no data.
        /// </returns>
        private DataGridConfig<WgSummarisedStaffTimeUsageRow> MapToGridConfig(
            ApiResponseDto<WgSummarisedStaffTimeUsageDto> response,
            string? sortBy,
            bool descending)
        {
            var grid = TimeByJobCodeGridConfig();

            if (!response.Success || response.Data is null)
                return grid;

            grid.Data = _mapper.Map<List<WgSummarisedStaffTimeUsageRow>>(response.Data.Rows);
            grid.Pagination = new PaginationModel
            {
                TotalRecords = response.Data.Pagination.TotalRecords,
                PageNumber = response.Data.Pagination.PageNumber,
                PageSize = response.Data.Pagination.PageSize,
                SortColumn = sortBy,
                SortDirection = descending
            };

            return grid;
        }

        /// <summary>
        /// Maps the service response to the <see cref="WgSummarisedStaffTimeUsageSummary"/> footer model.
        /// Returns a default (zero-valued) summary when the response is unsuccessful or contains no data,
        /// </summary>
        /// <param name="response">The API response whose <c>Summary</c> property is mapped to the view model.</param>
        /// <returns>
        /// A mapped <see cref="WgSummarisedStaffTimeUsageSummary"/> on success, or a new default instance
        /// when <paramref name="response"/> is unsuccessful or its data is <see langword="null"/>.
        /// </returns>
        private WgSummarisedStaffTimeUsageSummary MapToSummary(ApiResponseDto<WgSummarisedStaffTimeUsageDto> response)
        {
            return response.Success && response.Data is not null
                ? _mapper.Map<WgSummarisedStaffTimeUsageSummary>(response.Data.Summary)
                : new WgSummarisedStaffTimeUsageSummary();
        }

        /// <summary>
        /// Returns the static <see cref="DataGridConfig{WgSummarisedStaffTimeUsageRow}"/> skeleton shared by
        /// both <see cref="Index"/> and <see cref="LoadSummarisedStaffTimeGrid"/>.
        /// The configuration defines the grid identity, bound AJAX URL, column definitions, row-selection
        /// callback, and interaction flags; it intentionally contains no data or pagination state so
        /// callers can populate those fields independently after calling this method.
        /// </summary>
        /// <returns>A new <see cref="DataGridConfig{WgSummarisedStaffTimeUsageRow}"/> with static configuration applied.</returns>
        private static DataGridConfig<WgSummarisedStaffTimeUsageRow> TimeByJobCodeGridConfig() => new()
        {
            GridId = "timeUsageGrid",
            Title = string.Empty,
            BindGridUrl = "/PACT/WorkGroupSummarisedStaffTimeUsage/LoadSummarisedStaffTimeGrid",
            ExtraFilterMethod = "getWorkGroupTimeByJobCodeExtraFilters",
            ShowCheckboxColumn = false,
            AllowAdd = false,
            AllowEdit = false,
            AllowDelete = false,
            AllowRowSelection = true,
            RowSelectFunction = "onTimeByJobCodeRowSelected",
            ShowPagination = true,
            Columns = GridDataProvider.GetColumnsDefination<WgSummarisedStaffTimeUsageRow>()
        };
    }
}
