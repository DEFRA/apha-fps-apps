using Apha.FPSApps.Application.Interfaces.Costbook;
using Apha.FPSApps.Web.Areas.CostBook.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Newtonsoft.Json;
using System.Web;

namespace Apha.FPSApps.Web.Areas.CostBook.Controllers;

[Area("CostBook")]
[Authorize(Roles = "CostbookAdmin,CostbookUser")]
[AuthorizeForScopes(ScopeKeySection = "CostBookApiSettings:Scope")]
public class ProjectSummaryController : Controller
{
    private readonly ICostBookYearlyDetailsService _yearlyDetailsService;
    private readonly ICostBookProjectSummaryService _projectSummaryService;

    public ProjectSummaryController(
        ICostBookYearlyDetailsService yearlyDetailsService,
        ICostBookProjectSummaryService projectSummaryService)
    {
        _yearlyDetailsService = yearlyDetailsService;
        _projectSummaryService = projectSummaryService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string projectId)
    {
        var decodedProjectId = HttpUtility.UrlDecode(projectId);

        var headerResponse = await _yearlyDetailsService.GetProjectHeaderAsync(decodedProjectId);
        if (!headerResponse.Success || headerResponse.Data is null)
            return RedirectToAction("Index", "Projects");

        var rows = await BuildProjectSummaryRowsAsync(decodedProjectId);
        var summaryGrid = BuildProjectSummaryGrid(rows, decodedProjectId);

        var viewModel = new ProjectSummaryViewModel
        {
            ProjectHeaderDto = headerResponse.Data,
            Rows             = rows,
            SummaryGrid      = summaryGrid,
            ShowInclProfit   = headerResponse.Data.Programme == "Comm"
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> LoadProjectSummaryGrid(PaginationFilter<string> request, string projectId)
    {
        if (!ModelState.IsValid)
        {
            return Json(new
            {
                success = false,
                message = "Invalid request data",
                errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
            });
        }

        var decodedProjectId = HttpUtility.UrlDecode(projectId);
        var rows = await BuildProjectSummaryRowsAsync(decodedProjectId);
        var summaryGrid = BuildProjectSummaryGrid(rows, decodedProjectId, request);

        return PartialView("_DataGrid", summaryGrid);
    }

    private async Task<List<ProjectSummaryRow>> BuildProjectSummaryRowsAsync(string projectId)
    {
        var yearsResponse = await _yearlyDetailsService.GetProjectYearsAsync(projectId);
        var yearDtos = yearsResponse.Success && yearsResponse.Data != null
            ? yearsResponse.Data.OrderBy(y => y.YearValue).ToList()
            : new List<Application.Dtos.CostBook.ProjectYearDto>();

        var rows = new List<ProjectSummaryRow>();

        foreach (var yearDto in yearDtos)
        {
            var year = yearDto.YearValue;

            var costResponse = await _projectSummaryService.GetProjectYearCostSummaryAsync(projectId, year);
            var profitResponse = await _projectSummaryService.GetProfitIncludedTotalAsync(projectId, year);

            var cost = costResponse.Success && costResponse.Data is not null ? costResponse.Data : null;

            rows.Add(new ProjectSummaryRow
            {
                Year = year,
                StaffCost = cost?.StaffCostTotal ?? 0,
                TestCost = cost?.TestCostTotal ?? 0,
                AnimalCost = cost?.AnimalCostTotal ?? 0,
                AdditionalCost = cost?.AdditionalCostTotal ?? 0,
                ProfitIncludedTotal = profitResponse.Success ? profitResponse.Data : 0.0
            });
        }

        return rows;
    }

    private static DataGridConfig<ProjectSummaryRow> BuildProjectSummaryGrid(
        List<ProjectSummaryRow> rows,
        string projectId,
        PaginationFilter<string>? request = null)
    {
        var filterDict = request == null || string.IsNullOrWhiteSpace(request.Filter)
            ? new Dictionary<string, string>()
            : JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter) ?? new Dictionary<string, string>();

        var gridRows = rows.ToList();
        if (request != null && !string.IsNullOrWhiteSpace(request.SortBy))
        {
            gridRows = SortRows(gridRows, request.SortBy, request.Descending);
        }

        var paginationModel = new PaginationModel
        {
            TotalRecords = gridRows.Count,
            PageNumber = request?.Page ?? 1,
            PageSize = request?.PageSize ?? Math.Max(gridRows.Count, 1),
            SortColumn = request?.SortBy,
            SortDirection = request?.Descending ?? false
        };

        return new DataGridConfig<ProjectSummaryRow>
        {
            GridId = "projectSummaryGrid",
            Title = string.Empty,
            ShowCheckboxColumn = false,
            ShowPagination = false,
            KeyProperty = nameof(ProjectSummaryRow.Year),
            AllowAdd = false,
            AllowCopy = false,
            AllowEdit = false,
            AllowDelete = false,
            AllowView = false,
            AllowConfirm = false,
            AllowExport = false,
            BindGridUrl = $"/CostBook/ProjectSummary/LoadProjectSummaryGrid?projectId={HttpUtility.UrlEncode(projectId)}",
            Data = gridRows,
            Columns = GridDataProvider.GetColumnsDefination<ProjectSummaryRow>(null),
            Pagination = paginationModel,
            CurrentFilters = filterDict
        };
    }

    private static List<ProjectSummaryRow> SortRows(List<ProjectSummaryRow> rows, string sortBy, bool descending)
    {
        return sortBy switch
        {
            nameof(ProjectSummaryRow.FinancialYearDisplay) or nameof(ProjectSummaryRow.Year)
                => descending ? rows.OrderByDescending(r => r.Year).ToList() : rows.OrderBy(r => r.Year).ToList(),
            nameof(ProjectSummaryRow.AdditionalCost)
                => descending ? rows.OrderByDescending(r => r.AdditionalCost).ToList() : rows.OrderBy(r => r.AdditionalCost).ToList(),
            nameof(ProjectSummaryRow.StaffCost)
                => descending ? rows.OrderByDescending(r => r.StaffCost).ToList() : rows.OrderBy(r => r.StaffCost).ToList(),
            nameof(ProjectSummaryRow.TestCost)
                => descending ? rows.OrderByDescending(r => r.TestCost).ToList() : rows.OrderBy(r => r.TestCost).ToList(),
            nameof(ProjectSummaryRow.AnimalCost)
                => descending ? rows.OrderByDescending(r => r.AnimalCost).ToList() : rows.OrderBy(r => r.AnimalCost).ToList(),
            nameof(ProjectSummaryRow.GrandTotal)
                => descending ? rows.OrderByDescending(r => r.GrandTotal).ToList() : rows.OrderBy(r => r.GrandTotal).ToList(),
            _ => rows
        };
    }
}
