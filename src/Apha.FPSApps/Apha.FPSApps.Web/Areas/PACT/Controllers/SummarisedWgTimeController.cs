using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PACT.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.Areas.PACT.Controllers;

[Area("PACT")]
[Authorize(Roles = "PACTAdmin,PACTUser")]
[AuthorizeForScopes(ScopeKeySection = "PACTApiSettings:Scope")]
public class SummarisedWgTimeController : Controller
{
    private readonly IMapper _mapper;
    private readonly ISummarisedWorkgroupTimeService _service;
    private readonly IProjectService _projectService;

    public SummarisedWgTimeController(
        IMapper mapper, 
        ISummarisedWorkgroupTimeService service,
        IProjectService projectService)
    {
        _mapper = mapper;
        _service = service;
        _projectService = projectService;
    }

    public async Task<IActionResult> Index(string? workGroup)
    {
        var grid = await BuildGridAsync(new PaginationFilter<string>(), workGroup);
        return View(new SummarisedWgTimeViewModel 
        { 
            Grid = grid,
            SelectedWorkgroup = workGroup
        });
    }

    [HttpPost]
    public async Task<IActionResult> LoadGrid(PaginationFilter<string> request, string? workGroup)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var grid = await BuildGridAsync(request, workGroup);
        return PartialView("_DataGrid", grid);
    }

    [HttpGet]
    public async Task<IActionResult> GetProjectDescription(string projectId)
    {
        if (string.IsNullOrWhiteSpace(projectId))
            return BadRequest(new { success = false, message = "Project ID is required" });

        try
        {
            var response = await _projectService.GetProjectByIdAsync(projectId);

            if (response.Success && response.Data != null)
            {
                return Ok(new 
                { 
                    success = true, 
                    projectTitle = response.Data.ProjectTitle ?? string.Empty 
                });
            }

            return NotFound(new 
            { 
                success = false, 
                message = "Project not found" 
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new 
            { 
                success = false, 
                message = "An error occurred while retrieving project details",
                error = ex.Message 
            });
        }
    }

    private async Task<DataGridConfig<SummarisedWgTimePivotRow>> BuildGridAsync(
        PaginationFilter<string> request,
        string? workGroup)
    {
        var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                         ?? new Dictionary<string, string>();

        var query = _mapper.Map<QueryParameters<string>>(request);
        var response = await _service.GetSummarisedWorkgroupTimeSummaryAsync(query, workGroup);
        var pivot = response.Success && response.Data != null
            ? response.Data
            : new SummarisedWgTimePivotDto();

        var rows = pivot.Rows.Select(r =>
        {
            var row = new SummarisedWgTimePivotRow
            {
                WorkGroup = r.WorkGroup,
                ProfitCentre = r.ProfitCentre,
                ParentProject = r.ParentProject,
                ProjectTitle = r.ProjectTitle,
                SumOfTime = r.SumOfTime,
                SumOfCost = r.SumOfCost,
                Budget = r.Budget,
                PercentSpent = r.PercentSpent
            };

            // Map month properties to M1-M12 (financial year: 1=Apr, 2=May, ..., 12=Mar)
            row.M1 = r.April;
            row.M2 = r.May;
            row.M3 = r.June;
            row.M4 = r.July;
            row.M5 = r.August;
            row.M6 = r.September;
            row.M7 = r.October;
            row.M8 = r.November;
            row.M9 = r.December;
            row.M10 = r.January;
            row.M11 = r.February;
            row.M12 = r.March;

            return row;
        }).ToList();

        var columns = new List<DataGridColumn>
        {
            new() { PropertyName = "ParentProject",  DisplayName = "Project",          ColumnType = GridColumnType.Text, IsFilterable = false, Width = 150 },
            new() { PropertyName = "ProjectTitle",   DisplayName = "ProjectTitle",     ColumnType = GridColumnType.Text, IsFilterable = false, Width = 0, IsVisible = false }
        };

        // Add fixed month columns (all 12 months of the financial year) - month names only
        var monthNames = new[] { "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", "Jan", "Feb", "Mar" };
        for (int month = 1; month <= 12; month++)
        {
            columns.Add(new DataGridColumn
            {
                PropertyName = $"M{month}",
                DisplayName  = monthNames[month - 1],
                ColumnType   = GridColumnType.DecimalNumber,
                IsFilterable = false,
                Width        = 90
            });
        }

        // Add total columns
        columns.Add(new DataGridColumn
        {
            PropertyName = "SumOfTime",
            DisplayName  = "Time",
            ColumnType   = GridColumnType.DecimalNumber,
            IsFilterable = false,
            Width        = 100
        });

        columns.Add(new DataGridColumn
        {
            PropertyName = "SumOfCost",
            DisplayName  = "Cost",
            ColumnType   = GridColumnType.GbpValue,
            IsFilterable = false,
            Width        = 110
        });

        columns.Add(new DataGridColumn
        {
            PropertyName = "Budget",
            DisplayName  = "YrPlan",
            ColumnType   = GridColumnType.GbpValue,
            IsFilterable = false,
            Width        = 110
        });

        columns.Add(new DataGridColumn
        {
            PropertyName = "PercentSpent",
            DisplayName  = "Spent",
            ColumnType   = GridColumnType.DecimalNumber,
            IsFilterable = false,
            Width        = 90
        });

        var pagination = pivot.Pagination != null
            ? _mapper.Map<PaginationModel>(pivot.Pagination)
            : new PaginationModel();
        pagination.SortColumn    = request.SortBy;
        pagination.SortDirection = request.Descending;

        return new DataGridConfig<SummarisedWgTimePivotRow>
        {
            GridId         = "summarisedWorkgroupTimeGrid",
            KeyProperty    = "ParentProject",
            AllowAdd       = false,
            AllowEdit      = false,
            AllowDelete    = false,
            ShowPagination = true,
            BindGridUrl    = $"/PACT/SummarisedWgTime/LoadGrid?workGroup={Uri.EscapeDataString(workGroup ?? "")}",
            Columns        = columns,
            Data           = rows,
            CurrentFilters = filterDict,
            Pagination     = pagination
        };
    }
}
