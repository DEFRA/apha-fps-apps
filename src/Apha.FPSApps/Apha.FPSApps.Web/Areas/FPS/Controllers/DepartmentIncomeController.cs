using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;
using System.Text.Json;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin,FPSUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class DepartmentIncomeController : Controller
    {
        private readonly IMapper _mapper;

        private readonly IDepartmentIncomeService _departmentIncomeService;

        // Separate from the CRUD resource per the Backend -> Frontend Handoff rule
        private readonly IProjectService _projectService;

        public DepartmentIncomeController(
            IMapper mapper,
            IDepartmentIncomeService departmentIncomeService,
            IProjectService projectService)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _departmentIncomeService = departmentIncomeService ?? throw new ArgumentNullException(nameof(departmentIncomeService));
            _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
        }

        // ── Index ──────────────────────────────────────────────────────────────────

        public async Task<IActionResult> Index()
        {
            var viewModel = new DepartmentIncomeViewModel();
            await PopulateDropdownsAsync(viewModel);

            // Built explicitly here; JS loadSnapshotQueryResults populates it via LoadSnapshotGrid AJAX on run
            viewModel.SnapshotGrid = BuildSnapshotGridConfig(
                new List<DepartmentIncomeSnapshotItem>(), new PaginationModel(), null);

            return View(viewModel);
        }

        // ── Dropdown / Lookup population ──────────────────────────────────────────

        // Period list from IDepartmentIncomeService.GetPeriodsAsync (period-table-dropdown control)
        private async Task PopulateDropdownsAsync(DepartmentIncomeViewModel model)
        {
            // Project dropdown — IProjectService (separate lookup service, not CRUD resource)
            var projectsResult = await _projectService.GetAllProjectsAsync();
            if (projectsResult.Success && projectsResult.Data != null)
            {
                model.ProjectList = projectsResult.Data
                    .OrderBy(p => p.ParentProject)
                    .Select(p => new SelectListItem
                    {
                        Value = p.ParentProject,
                        Text = p.ParentProject,
                        Selected = string.Equals(model.SelectedProject, p.ParentProject,
                            StringComparison.OrdinalIgnoreCase)
                    })
                    .ToList();
            }

            // Period list — IDepartmentIncomeService.GetPeriodsAsync (period-table-dropdown)
            var periodsResult = await _departmentIncomeService.GetPeriodsAsync();
            if (periodsResult.Success && periodsResult.Data != null)
            {
                model.PeriodList = periodsResult.Data
                    .Select(p => new PeriodItem
                    {
                        AccntsPeriod = p.AccntsPeriod,
                        MonthName = p.MonthName,
                        MonthNumber = p.MonthNumber
                    })
                    .ToList();
            }
        }

        // ── Snapshot tab DataGrid AJAX reload ──────────────────────────────────────

        // The snapshot data shows period-level status (periodName, finalSummariesRun, periodLocke)
        // Filtered by project and month range from page controls
        [HttpPost]
        public async Task<IActionResult> LoadSnapshotGrid(
            PaginationFilter<string> request,
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values
                        .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var filterDict = !string.IsNullOrEmpty(request.Filter)
                ? JsonSerializer.Deserialize<Dictionary<string, string>>(request.Filter)
                : null;

            // If no project selected, return empty grid (snapshot requires a project context)
            // TRANSFORMENGINE TODO STUB: Replace with dedicated snapshot service method when backend Phase adds endpoint
            var items = new List<DepartmentIncomeSnapshotItem>();
            var pagination = new PaginationModel { SortColumn = request.SortBy, SortDirection = request.Descending };

            var gridConfig = BuildSnapshotGridConfig(items, pagination, filterDict);
            return PartialView("_DataGrid", gridConfig);
        }

        // ── Query Result AJAX Endpoints ───────────────────────────────────────────

        // Bound to JS "Run query" click for 'qryDeptIncomeTime' / 'time' selection
        [HttpPost]
        public async Task<IActionResult> GetTimeData(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null)
        {
            var result = await _departmentIncomeService.GetTimeIncomeAsync(project, monthFrom, monthTo);

            if (!result.Success || result.Data == null)
            {
                var errorMsg = result.Errors?.FirstOrDefault()?.Message ?? "Failed to load time data";
                return Json(new { success = false, message = errorMsg });
            }

            var items = _mapper.Map<List<DepartmentIncomeTimeItem>>(result.Data);
            return Json(new { success = true, data = items });
        }

        // Bound to JS "Run query" click for 'qryDeptIncomeTest' / 'tests' selection
        [HttpPost]
        public async Task<IActionResult> GetTestData(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null)
        {
            var result = await _departmentIncomeService.GetTestIncomeAsync(project, monthFrom, monthTo);

            if (!result.Success || result.Data == null)
            {
                var errorMsg = result.Errors?.FirstOrDefault()?.Message ?? "Failed to load tests data";
                return Json(new { success = false, message = errorMsg });
            }

            var items = _mapper.Map<List<DepartmentIncomeTestItem>>(result.Data);
            return Json(new { success = true, data = items });
        }

        // Bound to JS "Run query" click for 'qryDeptIncomeAnimal' / 'animals' selection
        [HttpPost]
        public async Task<IActionResult> GetAnimalData(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null)
        {
            var result = await _departmentIncomeService.GetAnimalIncomeAsync(project, monthFrom, monthTo);

            if (!result.Success || result.Data == null)
            {
                var errorMsg = result.Errors?.FirstOrDefault()?.Message ?? "Failed to load animal data";
                return Json(new { success = false, message = errorMsg });
            }

            var items = _mapper.Map<List<DepartmentIncomeAnimalItem>>(result.Data);
            return Json(new { success = true, data = items });
        }

        // Bound to JS "Run query" click for 'qryDeptIncomeAdditional' / 'exceptional' selection
        [HttpPost]
        public async Task<IActionResult> GetAdditionalData(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null)
        {
            var result = await _departmentIncomeService.GetAdditionalIncomeAsync(project, monthFrom, monthTo);

            if (!result.Success || result.Data == null)
            {
                var errorMsg = result.Errors?.FirstOrDefault()?.Message ?? "Failed to load additional data";
                return Json(new { success = false, message = errorMsg });
            }

            var items = _mapper.Map<List<DepartmentIncomeAdditionalItem>>(result.Data);
            return Json(new { success = true, data = items });
        }

        // Bound to JS "Run query" click for 'qryDeptIncomeTotals' / 'totals' selection
        [HttpPost]
        public async Task<IActionResult> GetTotalsData(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null)
        {
            var result = await _departmentIncomeService.GetTotalsAsync(project, monthFrom, monthTo);

            if (!result.Success || result.Data == null)
            {
                var errorMsg = result.Errors?.FirstOrDefault()?.Message ?? "Failed to load totals data";
                return Json(new { success = false, message = errorMsg });
            }

            var items = _mapper.Map<List<DepartmentIncomeTotalsItem>>(result.Data);
            return Json(new { success = true, data = items });
        }

        // ── Private Grid Config Builders ──────────────────────────────────────────

        // showAddButton: false in JS → AllowAdd/Edit/Delete all false
        // KeyProperty uses implicit row index (no natural PK in snapshotData)
        private static DataGridConfig<DepartmentIncomeSnapshotItem> BuildSnapshotGridConfig(
            List<DepartmentIncomeSnapshotItem> items,
            PaginationModel pagination,
            Dictionary<string, string>? filterDict)
        {
            return new DataGridConfig<DepartmentIncomeSnapshotItem>
            {
                GridId             = "departmentIncomeSnapshotGrid",
                Title              = "Snapshot data",
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "PeriodName",      // period name is natural display key for snapshot rows
                AllowAdd           = false,             // JS showAddButton: false
                AddFunction        = string.Empty,
                AllowEdit          = false,             // read-only report grid
                EditFunction       = string.Empty,
                AllowDelete        = false,             // read-only report grid
                DeleteFunction     = string.Empty,
                ExtraFilterMethod  = "getDepartmentIncomeSnapshotExtraFilters",
                BindGridUrl        = "/FPS/DepartmentIncome/LoadSnapshotGrid",
                Data               = items,
                Columns            = GridDataProvider.GetColumnsDefination<DepartmentIncomeSnapshotItem>(null),
                Pagination         = pagination,
                CurrentFilters     = filterDict
            };
        }

        private PaginationModel BuildPagination(
            Apha.FPSApps.Application.Dtos.PaginationDto? paginationDto,
            PaginationFilter<string> request)
        {
            if (paginationDto == null)
                return new PaginationModel { SortColumn = request.SortBy, SortDirection = request.Descending };

            var pagination = _mapper.Map<PaginationModel>(paginationDto);
            pagination.SortColumn = request.SortBy;
            pagination.SortDirection = request.Descending;
            return pagination;
        }
    }
}
