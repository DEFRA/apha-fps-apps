using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.Costbook;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.CostBook.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace Apha.FPSApps.Web.Areas.CostBook.Controllers
{
    [Area("CostBook")]
    [Authorize(Roles = "CostbookAdmin,CostbookUser")]
    [AuthorizeForScopes(ScopeKeySection = "CostBookApiSettings:Scope")]
    public class StaffYearsController : Controller
    {
        private readonly ICostBookProjectSummaryService _projectSummaryService;
        private readonly ICostBookYearlyDetailsService _yearlyDetailsService;
        private readonly IMapper _mapper;

        public StaffYearsController(
            ICostBookProjectSummaryService projectSummaryService,
            ICostBookYearlyDetailsService yearlyDetailsService,
            IMapper mapper)
        {
            _projectSummaryService = projectSummaryService;
            _yearlyDetailsService = yearlyDetailsService;
            _mapper = mapper;
        }

        
        public async Task<IActionResult> Index(string projectId)
        {
            var headerResponse = await _yearlyDetailsService.GetProjectHeaderAsync(projectId);
            if (!headerResponse.Success || headerResponse.Data is null)
                return RedirectToAction("Index", "Projects");

            var grid = await BuildGridAsync(projectId);

            return View(new StaffYearsViewModel
            {
                ProjectId = projectId,
                ProjectHeaderDto = headerResponse.Data,
                Grid = grid
            });
        }

        
        [HttpPost]
        public async Task<IActionResult> LoadGrid(string projectId, PaginationFilter<string> request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var grid = await BuildGridAsync(projectId, request);
            return PartialView("_DataGrid", grid);
        }

        
        private async Task<DataGridConfig<StaffYearsPivotRow>> BuildGridAsync(
            string projectId, PaginationFilter<string>? request = null)
        {
            // Default to page 1, size 10 when called on initial load
            var query = request != null
                ? _mapper.Map<QueryParameters<string>>(request)
                : new QueryParameters<string> { Page = 1, PageSize = 10 };

            var response = await _projectSummaryService.GetStaffYearsPivotAsync(projectId, query);
            var pivot = response.Success && response.Data != null
                ? response.Data
                : new StaffYearsPivotDto();

            var filterDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(request?.Filter ?? "{}")
                             ?? new Dictionary<string, string>();

            var rows = pivot.Rows.Select(r =>
            {
                var row = StaffYearsPivotRow.Create(pivot.Years.Count);
                row.Project = r.Project;
                row.Grade   = r.Grade;
                row.Total   = Math.Round((decimal)r.Total, 2);

                for (int i = 0; i < pivot.Years.Count && i < 20; i++)
                {
                    int year = pivot.Years[i];
                    decimal? value = r.YearlyAmounts.TryGetValue(year, out double v)
                        ? Math.Round((decimal)v, 2)
                        : null;
                    row.SetYearValue(i + 1, value);
                }

                return row;
            }).ToList();

            if (!string.IsNullOrWhiteSpace(query.SortBy)
                && query.SortBy.StartsWith("Y", StringComparison.OrdinalIgnoreCase)
                && int.TryParse(query.SortBy[1..], out var yearIndex)
                && yearIndex >= 1 && yearIndex <= 20)
            {
                rows = query.Descending
                    ? rows.OrderBy(r => GetYearValue(r, yearIndex).HasValue ? 0 : 1)
                          .ThenByDescending(r => GetYearValue(r, yearIndex))
                          .ThenBy(r => r.Grade)
                          .ToList()
                    : rows.OrderBy(r => GetYearValue(r, yearIndex).HasValue ? 0 : 1)
                          .ThenBy(r => GetYearValue(r, yearIndex))
                          .ThenBy(r => r.Grade)
                          .ToList();
            }

            
            var columns = new List<DataGridColumn>
            {
                new() { PropertyName = "Project", DisplayName = "Project", ColumnType = GridColumnType.Text,          IsFilterable = false, Width = 100 },
                new() { PropertyName = "Grade",   DisplayName = "Grade",   ColumnType = GridColumnType.Text,          IsFilterable = true,  Width = 80  },
                new() { PropertyName = "Total",   DisplayName = "Total",   ColumnType = GridColumnType.DecimalNumber, IsFilterable = false, Width = 90  }
            };

            for (int i = 0; i < pivot.Years.Count && i < 20; i++)
            {
                columns.Add(new DataGridColumn
                {
                    PropertyName = $"Y{i + 1}",
                    DisplayName  = pivot.Years[i].ToString(),
                    ColumnType   = GridColumnType.DecimalNumber,
                    IsFilterable = false,
                    Width        = 90
                });
            }

            return new DataGridConfig<StaffYearsPivotRow>
            {
                GridId         = "staffYearsGrid",
                KeyProperty    = "Grade",
                AllowAdd       = false,
                AllowEdit      = false,
                AllowDelete    = false,
                ShowPagination = true,
                BindGridUrl    = $"/CostBook/StaffYears/LoadGrid?projectId={Uri.EscapeDataString(projectId)}",
                Columns        = columns,
                Data           = rows,
                CurrentFilters = filterDict,
                Pagination     = new PaginationModel
                {
                    TotalRecords  = pivot.TotalCount,
                    PageNumber    = query.Page,
                    PageSize      = query.PageSize,
                    SortColumn    = query.SortBy,
                    SortDirection = query.Descending
                }
            };
        }

        private static decimal? GetYearValue(StaffYearsPivotRow row, int yearIndex)
        {
            if (yearIndex < 1 || yearIndex > 20)
                return null;

            var propName = $"Y{yearIndex}";
            var prop = row.GetType().GetProperty(propName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.IgnoreCase);
            return (decimal?)prop?.GetValue(row);
        }
    }
}
