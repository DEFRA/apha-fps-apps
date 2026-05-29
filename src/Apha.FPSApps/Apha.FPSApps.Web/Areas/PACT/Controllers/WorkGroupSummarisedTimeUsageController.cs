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

namespace Apha.FPSApps.Web.Areas.PACT.Controllers;

[Area("PACT")]
[Authorize(Roles = "PACTAdmin,PACTUser")]
[AuthorizeForScopes(ScopeKeySection = "PACTApiSettings:Scope")]
public class WorkGroupSummarisedTimeUsageController : Controller
{
    private readonly IMapper _mapper;
    private readonly ISummarisedWorkgroupTimeService _service;

    public WorkGroupSummarisedTimeUsageController(
        IMapper mapper,
        ISummarisedWorkgroupTimeService service)
    {
        _mapper = mapper;
        _service = service;
    }

    /// <summary>
    /// Renders the Summarised Workgroup Time index page for the specified workgroup.
    /// Loads an initial (page 1, unsorted) grid, maps the summary totals row, and
    /// populates <c>ViewBag.ProjectTitleLookup</c> so the client-side script can
    /// display project descriptions alongside selected rows.
    /// </summary>
    /// <param name="workGroup">The workgroup code to filter data by, or an empty string to show all.</param>
    public async Task<IActionResult> Index(string workGroup = "")
    {
        var query = _mapper.Map<QueryParameters<string>>(new PaginationFilter<string> { Filter = "{}" });
        var response = await _service.GetSummarisedWorkgroupTimeSummaryAsync(query, workGroup);

        ViewBag.ProjectTitleLookup = response.Data?.ProjectTitleLookup
            .ToDictionary(x => x.ParentProject, x => x.ProjectTitle)
            ?? [];

        return View(new SummarisedWgTimeViewModel
        {
            Grid = MapToGridConfig(response, sortBy: null, descending: false, workGroup, yrPlanAmount: 0),
            Summary = MapToSummary(response),
            SelectedWorkgroup = workGroup
        });
    }

    /// <summary>
    /// AJAX endpoint that reloads the data grid partial view with the requested
    /// pagination, sorting, and year-plan amount applied.
    /// Called by the shared <c>_DataGrid</c> partial whenever the user pages,
    /// sorts, or submits a year-plan amount from the Calculate Year Plan modal.
    /// </summary>
    /// <param name="request">Pagination and sort state sent by the grid.</param>
    /// <param name="workGroup">The workgroup code to filter data by, or an empty string for all.</param>
    /// <param name="yrPlanAmount">
    /// The year-plan budget amount entered in the modal. When greater than zero it
    /// overrides the per-row <c>Budget</c> value and is used to calculate
    /// <c>PercentSpent</c>. Defaults to <c>0</c> (no override).
    /// </param>
    [HttpPost]
    public async Task<IActionResult> LoadSummarisedWgTimeGrid(PaginationFilter<string> request, string workGroup = "", decimal yrPlanAmount = 0)
    {
        var query = _mapper.Map<QueryParameters<string>>(request);
        var response = await _service.GetSummarisedWorkgroupTimeSummaryAsync(query, workGroup);

        return PartialView("_DataGrid", MapToGridConfig(response, request.SortBy, request.Descending, workGroup, yrPlanAmount));
    }

    /// <summary>
    /// Maps a service response to a <see cref="DataGridConfig{SummarisedWgTimePivotRow}"/>.
    /// If the response is unsuccessful or contains no data, an empty grid config is
    /// returned. Otherwise each row's <c>Budget</c> is optionally overridden by
    /// <paramref name="yrPlanAmount"/>, <c>PercentSpent</c> is calculated, and the
    /// human-readable <c>CostDisplay</c> (e.g. £1,234.56) and
    /// <c>SpentDisplay</c> (e.g. 42.5%) strings are set.
    /// </summary>
    /// <param name="response">The API response containing rows and pagination data.</param>
    /// <param name="sortBy">Column name to sort by, or <c>null</c> for default order.</param>
    /// <param name="descending"><c>true</c> for descending sort; <c>false</c> for ascending.</param>
    /// <param name="workGroup">Workgroup code embedded in the grid's reload URL.</param>
    /// <param name="yrPlanAmount">Year-plan budget override; ignored when zero.</param>
    private DataGridConfig<SummarisedWgTimePivotRow> MapToGridConfig(
        ApiResponseDto<SummarisedWgTimeViewDto> response,
        string? sortBy,
        bool descending,
        string workGroup,
        decimal yrPlanAmount = 0)
    {
        var grid = SummarisedWgTimeGridConfig(workGroup);

        if (!response.Success || response.Data is null)
            return grid;

        grid.Data = _mapper.Map<List<SummarisedWgTimePivotRow>>(response.Data.Rows);

        foreach (var row in grid.Data)
        {
            row.Budget       = yrPlanAmount > 0 ? yrPlanAmount : row.Budget;
            row.PercentSpent = row.Budget > 0
                ? Math.Round((row.SumOfCost / row.Budget.Value) * 100, 2)
                : 0;

            row.CostDisplay  = row.SumOfCost.ToString("£#,##0.00;-£#,##0.00");
            row.SpentDisplay = row.PercentSpent.HasValue ? row.PercentSpent.Value.ToString("0.##") + "%" : string.Empty;
        }

        grid.Pagination = new PaginationModel
        {
            TotalRecords = response.Data.Pagination.TotalRecords,
            PageNumber   = response.Data.Pagination.PageNumber,
            PageSize     = response.Data.Pagination.PageSize,
            SortColumn   = sortBy,
            SortDirection = descending
        };

        return grid;
    }

    /// <summary>
    /// Maps the summary totals from the service response to a
    /// <see cref="SummarisedWgTimeSummary"/> view model.
    /// Returns a default (zero-valued) summary when the response is unsuccessful
    /// or contains no data.
    /// </summary>
    /// <param name="response">The API response containing the summary DTO.</param>
    private SummarisedWgTimeSummary MapToSummary(ApiResponseDto<SummarisedWgTimeViewDto> response)
    {
        var result= response.Success && response.Data is not null
            ? _mapper.Map<SummarisedWgTimeSummary>(response.Data.Summary)
            : new SummarisedWgTimeSummary();

        return result;
    }

    /// <summary>
    /// Builds the static <see cref="DataGridConfig{SummarisedWgTimePivotRow}"/> shared
    /// by both the <see cref="Index"/> and <see cref="LoadSummarisedWgTimeGrid"/> actions.
    /// Sets grid identity, URL bindings, column definitions, and disables add/edit/delete
    /// operations since this is a read-only summary view.
    /// </summary>
    /// <param name="workGroup">Workgroup code appended to the grid's AJAX reload URL.</param>
    private static DataGridConfig<SummarisedWgTimePivotRow> SummarisedWgTimeGridConfig(string workGroup) => new()
    {
        GridId            = "summarisedWorkgroupTimeGrid",
        KeyProperty       = "ParentProject",
        AllowAdd          = false,
        AllowEdit         = false,
        AllowDelete       = false,
        ShowPagination    = true,
        ExtraFilterMethod = "getSummarisedWgTimeExtraFilters",
        BindGridUrl       = $"/PACT/WorkGroupSummarisedTimeUsage/LoadSummarisedWgTimeGrid?workGroup={Uri.EscapeDataString(workGroup)}",
        Columns           = GridDataProvider.GetColumnsDefination<SummarisedWgTimePivotRow>()
    };
}