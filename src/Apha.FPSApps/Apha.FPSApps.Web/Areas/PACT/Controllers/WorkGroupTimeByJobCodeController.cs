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
    public class WorkGroupTimeByJobCodeController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IWorkGroupService _workGroupService;

        public WorkGroupTimeByJobCodeController(IMapper mapper, IWorkGroupService workGroupService)
        {
            _mapper = mapper;
            _workGroupService = workGroupService;
        }

        /// <summary>
        /// Renders the Work Group Time By Job Code page (frmCluedo1 equivalent).
        /// Fetches the full summarised staff time-usage dataset for the supplied work group,
        /// builds the initial data-grid configuration with no sort applied, and computes the
        /// pre-aggregated footer summary before passing everything to the view.
        /// </summary>
        /// <param name="workGroup">Work group name whose time-usage data should be displayed.</param>
        /// <param name="personName">Person name passed in from the Work Group People page; stored on
        /// the view model so the view can pre-select the correct person.</param>
        /// <returns>
        /// A <see cref="ViewResult"/> containing a <see cref="WorkGroupTimeByJobCodeViewModel"/>
        /// with the populated grid, summary footer, and header fields.
        /// </returns>
        public async Task<IActionResult> Index(string workGroup, string personName)
        {
            var query    = _mapper.Map<QueryParameters<string>>(new PaginationFilter<string> { Filter = "{}" });
            var response = await _workGroupService.GetWgSummarisedStaffTimeUsageAsync(query, workGroup);

            return View(new WorkGroupTimeByJobCodeViewModel
            {
                SelectedWorkGroup  = workGroup,
                SelectedPersonName = personName,
                WorkGroupName      = workGroup,
                HrsPaid            = response.Data?.HrsPaid ?? 0,
                Grid               = MapToGridConfig(response, sortBy: null, descending: false),
                Summary            = MapToSummary(response)
            });
        }

        /// <summary>
        /// Handles partial-page grid refreshes triggered by the client-side data-grid component.
        /// Maps the incoming pagination/filter/sort request to query parameters, fetches an updated
        /// page of summarised staff time-usage data, and returns only the grid partial view so the
        /// page can be updated without a full reload.
        /// </summary>
        /// <param name="request">Pagination, sort, and column-filter parameters submitted by the grid.</param>
        /// <param name="workGroup">Work group name used to scope the data query.
        /// Must not be <see langword="null"/>, empty, or whitespace.</param>
        /// <returns>
        /// A <see cref="PartialViewResult"/> rendering the <c>_DataGrid</c> partial with an updated
        /// <see cref="DataGridConfig{WorkGroupTimeByJobCodeRow}"/> model.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="workGroup"/> is <see langword="null"/>, empty, or whitespace.
        /// </exception>
        [HttpPost]
        public async Task<IActionResult> LoadGrid(PaginationFilter<string> request, string workGroup)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(workGroup);

            var query    = _mapper.Map<QueryParameters<string>>(request);
            var response = await _workGroupService.GetWgSummarisedStaffTimeUsageAsync(query, workGroup);

            return PartialView("_DataGrid", MapToGridConfig(response, request.SortBy, request.Descending));
        }

        /// <summary>
        /// Builds a <see cref="DataGridConfig{WorkGroupTimeByJobCodeRow}"/> from the service response.
        /// When the response indicates failure or contains no data, the default empty grid configuration
        /// is returned immediately. On success, the response rows are mapped to view-model rows and the
        /// pagination model is populated from the response metadata together with the supplied sort state.
        /// </summary>
        /// <param name="response">The API response containing pivot rows and pagination metadata.</param>
        /// <param name="sortBy">The column name to sort by, or <see langword="null"/> if no sort is active.</param>
        /// <param name="descending"><see langword="true"/> for a descending sort; <see langword="false"/> for ascending.</param>
        /// <returns>
        /// A fully populated <see cref="DataGridConfig{WorkGroupTimeByJobCodeRow}"/>, or a default
        /// empty configuration when <paramref name="response"/> is unsuccessful or has no data.
        /// </returns>
        private DataGridConfig<WorkGroupTimeByJobCodeRow> MapToGridConfig(
            ApiResponseDto<WorkGroupTimeByJobCodeDto> response,
            string? sortBy,
            bool descending)
        {
            var grid = TimeByJobCodeGridConfig();

            if (!response.Success || response.Data is null)
                return grid;

            grid.Data       = _mapper.Map<List<WorkGroupTimeByJobCodeRow>>(response.Data.Rows);
            grid.Pagination = new PaginationModel
            {
                TotalRecords  = response.Data.Pagination.TotalRecords,
                PageNumber    = response.Data.Pagination.PageNumber,
                PageSize      = response.Data.Pagination.PageSize,
                SortColumn    = sortBy,
                SortDirection = descending
            };

            return grid;
        }

        /// <summary>
        /// Maps the service response to the <see cref="WorkGroupTimeByJobCodeSummary"/> footer model.
        /// Returns a default (zero-valued) summary when the response is unsuccessful or contains no data,
        /// mirroring the three-row footer of the legacy MS-Access form frmCluedo1.
        /// </summary>
        /// <param name="response">The API response whose <c>Summary</c> property is mapped to the view model.</param>
        /// <returns>
        /// A mapped <see cref="WorkGroupTimeByJobCodeSummary"/> on success, or a new default instance
        /// when <paramref name="response"/> is unsuccessful or its data is <see langword="null"/>.
        /// </returns>
        private WorkGroupTimeByJobCodeSummary MapToSummary(ApiResponseDto<WorkGroupTimeByJobCodeDto> response)
        {
            return response.Success && response.Data is not null
                ? _mapper.Map<WorkGroupTimeByJobCodeSummary>(response.Data.Summary)
                : new WorkGroupTimeByJobCodeSummary();
        }

        /// <summary>
        /// Returns the static <see cref="DataGridConfig{WorkGroupTimeByJobCodeRow}"/> skeleton shared by
        /// both <see cref="Index"/> and <see cref="LoadGrid"/>. The configuration defines the grid identity,
        /// bound URL, column definitions, and interaction flags; it intentionally contains no data or
        /// pagination state so callers can populate those fields independently.
        /// </summary>
        /// <returns>A new <see cref="DataGridConfig{WorkGroupTimeByJobCodeRow}"/> with static configuration applied.</returns>
        private static DataGridConfig<WorkGroupTimeByJobCodeRow> TimeByJobCodeGridConfig() => new()
        {
            GridId            = "timeUsageGrid",
            Title             = string.Empty,
            BindGridUrl       = "/PACT/WorkGroupTimeByJobCode/LoadGrid",
            ExtraFilterMethod = "getWorkGroupTimeByJobCodeExtraFilters",
            ShowCheckboxColumn = false,
            AllowAdd          = false,
            AllowEdit         = false,
            AllowDelete       = false,
            AllowRowSelection = false,
            ShowPagination    = true,
            Columns           = GridDataProvider.GetColumnsDefination<WorkGroupTimeByJobCodeRow>()
        };
    }
}