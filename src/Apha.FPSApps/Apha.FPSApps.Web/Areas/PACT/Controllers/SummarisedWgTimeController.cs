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
public class SummarisedWgTimeController : Controller
{
    private readonly IMapper _mapper;
    private readonly ISummarisedWorkgroupTimeService _service;

    public SummarisedWgTimeController(
        IMapper mapper,
        ISummarisedWorkgroupTimeService service)
    {
        _mapper = mapper;
        _service = service;
    }

    public async Task<IActionResult> Index(string? workGroup)
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

    [HttpPost]
    public async Task<IActionResult> LoadGrid(PaginationFilter<string> request, string? workGroup, decimal yrPlanAmount = 0)
    {
        var query = _mapper.Map<QueryParameters<string>>(request);
        var response = await _service.GetSummarisedWorkgroupTimeSummaryAsync(query, workGroup);

        return PartialView("_DataGrid", MapToGridConfig(response, request.SortBy, request.Descending, workGroup, yrPlanAmount));
    }

    private DataGridConfig<SummarisedWgTimePivotRow> MapToGridConfig(
        ApiResponseDto<SummarisedWgTimeViewDto> response,
        string? sortBy,
        bool descending,
        string? workGroup,
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

    private SummarisedWgTimeSummary MapToSummary(ApiResponseDto<SummarisedWgTimeViewDto> response)
    {
        var result= response.Success && response.Data is not null
            ? _mapper.Map<SummarisedWgTimeSummary>(response.Data.Summary)
            : new SummarisedWgTimeSummary();

        return result;
    }

    private static DataGridConfig<SummarisedWgTimePivotRow> SummarisedWgTimeGridConfig(string? workGroup) => new()
    {
        GridId            = "summarisedWorkgroupTimeGrid",
        KeyProperty       = "ParentProject",
        AllowAdd          = false,
        AllowEdit         = false,
        AllowDelete       = false,
        ShowPagination    = true,
        ExtraFilterMethod = "getSummarisedWgTimeExtraFilters",
        BindGridUrl       = $"/PACT/SummarisedWgTime/LoadGrid?workGroup={Uri.EscapeDataString(workGroup ?? "")}",
        Columns           = GridDataProvider.GetColumnsDefination<SummarisedWgTimePivotRow>()
    };
}