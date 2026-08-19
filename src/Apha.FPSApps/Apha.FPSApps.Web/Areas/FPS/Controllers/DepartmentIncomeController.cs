using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
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
    [Authorize(Roles = "FPSAdmin")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class DepartmentIncomeController : Controller
    {
        private static readonly System.Text.Json.JsonSerializerOptions CaseInsensitiveJsonOptions =
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        private readonly IMapper _mapper;

        private readonly IDepartmentIncomeService _departmentIncomeService;

        // Separate from the CRUD resource per the Backend -> Frontend Handoff rule
        private readonly IProjectService _projectService;

        // Used for Period From / Period To dropdowns (tblkpMonth: 12 fiscal months)
        private readonly IMonthService _monthService;

        public DepartmentIncomeController(
            IMapper mapper,
            IDepartmentIncomeService departmentIncomeService,
            IProjectService projectService,
            IMonthService monthService)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _departmentIncomeService = departmentIncomeService ?? throw new ArgumentNullException(nameof(departmentIncomeService));
            _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
            _monthService = monthService ?? throw new ArgumentNullException(nameof(monthService));
        }

        // ── Index ──────────────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var viewModel = new DepartmentIncomeViewModel();
            await PopulateDropdownsAsync(viewModel);

            // Built explicitly here; JS reloadDepartmentIncomeSnapshotGrid populates it via LoadSnapshotGrid AJAX on load
            viewModel.SnapshotGrid = BuildSnapshotGridConfig(
                new List<DepartmentIncomeSnapshotItem>(),
                new PaginationModel { SortColumn = nameof(DepartmentIncomeSnapshotItem.PeriodName) },
                null);

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
            else
            {
                model.ErrorMessage = periodsResult.Errors?.FirstOrDefault()?.Message
                    ?? "Failed to load period list. Please ensure the FPS API is running.";
            }

            // Month list — IMonthService.GetAllMonthsAsync (tblkpMonth: 12 fiscal months)
            // Matches Access combo box: SELECT DISTINCTROW MonthNumber, MonthName FROM tblkpMonth ORDER BY MonthNumber
            var monthsResult = await _monthService.GetAllMonthsAsync();
            if (monthsResult.Success && monthsResult.Data != null)
            {
                model.MonthList = monthsResult.Data
                    .OrderBy(m => m.Monthnumber)
                    .Select(m => new MonthItem
                    {
                        MonthNumber = m.Monthnumber,
                        MonthName   = m.Monthname
                    })
                    .ToList();
            }
        }

        // ── Snapshot tab DataGrid AJAX reload ──────────────────────────────────────

        // The snapshot data shows period-level status (periodName, finalSummariesRun, periodLocke)
        // Source: fps.tblperiod filtered by current FPS year — mirrors original MS Access tblPeriod query
        [HttpPost]
        [IgnoreAntiforgeryToken]
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

            var snapshotResult = await _departmentIncomeService.GetSnapshotPeriodsAsync();
            var allItems = snapshotResult.Success && snapshotResult.Data != null
                ? _mapper.Map<List<DepartmentIncomeSnapshotItem>>(snapshotResult.Data)
                : new List<DepartmentIncomeSnapshotItem>();

            // Apply filtering
            IEnumerable<DepartmentIncomeSnapshotItem> filtered = allItems;
            if (filterDict != null)
            {
                foreach (var (key, value) in filterDict)
                {
                    if (string.IsNullOrWhiteSpace(value)) continue;
                    var term = value.Trim();
                    filtered = filtered.Where(item =>
                    {
                        var prop = typeof(DepartmentIncomeSnapshotItem).GetProperty(key,
                            System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                        var cell = prop?.GetValue(item)?.ToString();
                        return cell != null && cell.Contains(term, StringComparison.OrdinalIgnoreCase);
                    });
                }
            }

            // Apply sorting — default to Month (EndPeriod) to match Access chronological order.
            // PeriodName is always sorted by Month (EndPeriod) because period names are not
            // alphabetically ordered; they follow the fiscal period sequence (April 2025 Only → Year Total).
            var sortBy    = !string.IsNullOrWhiteSpace(request.SortBy) ? request.SortBy : nameof(DepartmentIncomeSnapshotItem.PeriodName);
            var descending = request.Descending;

            // Map the column to sort to the actual property used for ordering.
            // PeriodName and all unrecognised columns fall back to Month so chronological order is preserved.
            var effectiveSortPropName = string.Equals(sortBy, nameof(DepartmentIncomeSnapshotItem.PeriodName), StringComparison.OrdinalIgnoreCase)
                ? nameof(DepartmentIncomeSnapshotItem.Month)
                : sortBy;

            var sortProp = typeof(DepartmentIncomeSnapshotItem).GetProperty(effectiveSortPropName,
                System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                ?? typeof(DepartmentIncomeSnapshotItem).GetProperty(nameof(DepartmentIncomeSnapshotItem.Month));

            filtered = descending
                ? filtered.OrderByDescending(i => sortProp!.GetValue(i))
                : filtered.OrderBy(i => sortProp!.GetValue(i));

            var filteredList = filtered.ToList();
            int total    = filteredList.Count;
            int page     = request.Page > 0     ? request.Page     : 1;
            int pageSize = request.PageSize > 0 ? request.PageSize : 10;

            var paged = filteredList
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var pagination = new PaginationModel
            {
                TotalRecords  = total,
                PageNumber    = page,
                PageSize      = pageSize,
                SortColumn    = sortBy,      // keep the user-visible column highlighted
                SortDirection = descending
            };

            var gridConfig = BuildSnapshotGridConfig(paged, pagination, filterDict);
            return PartialView("_DataGrid", gridConfig);
        }

        // ── Snapshot Period Edit ───────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> EditSnapshotPeriod(string periodName)
        {
            var result = await _departmentIncomeService.GetSnapshotPeriodsAsync();
            var dto = result.Data?.FirstOrDefault(p => p.PeriodName == periodName);
            if (dto == null) return NotFound();

            var model = _mapper.Map<DepartmentIncomeSnapshotItem>(dto);
            return PartialView("~/Areas/FPS/Views/DepartmentIncome/_AddEditDepartmentIncome.cshtml", model);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UpdateSnapshotPeriod([FromBody] DepartmentIncomeSnapshotUpdateDto model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid request." });

            var response = await _departmentIncomeService.UpdatePeriodLockedAsync(model.PeriodName, model.PeriodLocked);
            if (response.Success)
                return Json(new { success = true, message = "Period locked updated successfully." });

            return Json(new
            {
                success = false,
                message = response.Errors?.FirstOrDefault()?.Message ?? "Update failed."
            });
        }

        // ── Unified Current-Data Query Grid (CrossTab pattern) ───────────────────
        // Single endpoint for all five qryDeptIncome* query types.
        // queryType values: "qryDeptIncomeTime" | "qryDeptIncomeTest" | "qryDeptIncomeAnimal"
        //                   | "qryDeptIncomeExceptional" | "qryDeptIncomeTotals"
        // Columns are derived from the typed view-model's [GridColumn] attributes via GridDataProvider.
        // Rows are projected to Dictionary<string,string?> so _DataGrid can render any schema unchanged.

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> LoadGrid(
            PaginationFilter<string> request,
            string queryType   = "qryDeptIncomeTime",
            string? project    = null,
            int? monthFrom     = null,
            int? monthTo       = null,
            string source      = "current")   // "snapshot" | "current"
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var filterDict = !string.IsNullOrEmpty(request.Filter)
                ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(request.Filter,
                      CaseInsensitiveJsonOptions)
                : null;

            var (rows, columns, title) = await BuildCrossTabRowsAsync(queryType, project, monthFrom, monthTo, source);

            // Apply filtering and sorting before paging
            var filtered = ApplyFilterAndSort(rows, filterDict, request.SortBy, request.Descending);

            var isSnapshot = string.Equals(source, "snapshot", StringComparison.OrdinalIgnoreCase);
            var gridId     = isSnapshot ? "departmentIncomeSnapshotQueryGrid" : "departmentIncomeCurrentGrid";
            var extraFilter = isSnapshot ? "getDeptIncomeSnapshotQueryExtraFilters" : "getDeptIncomeCurrentExtraFilters";

            int page     = request.Page > 0     ? request.Page     : 1;
            int pageSize = request.PageSize > 0 ? request.PageSize : 20;
            int total    = filtered.Count;

            var pagination = new PaginationModel
            {
                TotalRecords  = total,
                PageNumber    = page,
                PageSize      = pageSize,
                SortColumn    = request.SortBy,
                SortDirection = request.Descending
            };

            var paged = filtered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var config = new DataGridConfig<Dictionary<string, string?>>
            {
                GridId             = gridId,
                Title              = title,
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = string.Empty,
                AllowAdd           = false,
                AddFunction        = string.Empty,
                AllowEdit          = false,
                EditFunction       = string.Empty,
                AllowDelete        = false,
                DeleteFunction     = string.Empty,
                ExtraFilterMethod  = extraFilter,
                BindGridUrl        = "/FPS/DepartmentIncome/LoadGrid",
                Columns            = columns,
                Data               = paged,
                Pagination         = pagination,
                CurrentFilters     = filterDict
            };

            return PartialView("_DataGrid", config);
        }

        // Dispatches to the appropriate service method and projects typed DTOs into dictionary rows.
        // When source == "current", uses the raw qryDeptIncome* live-table methods (no period aggregation).
        // When source == "snapshot", uses the fPeriod*-based snapshot methods (period diff with aggregation).
        private async Task<(List<Dictionary<string, string?>> rows, List<DataGridColumn> columns, string title)>
            BuildCrossTabRowsAsync(string queryType, string? project, int? monthFrom, int? monthTo, string source = "current")
        {
            var isCurrent = !string.Equals(source, "snapshot", StringComparison.OrdinalIgnoreCase);

            switch (queryType)
            {
                case "qryDeptIncomeTest":
                {
                    var result = isCurrent
                        ? await _departmentIncomeService.GetTestIncomeCurrentAsync(project, monthFrom, monthTo)
                        : await _departmentIncomeService.GetTestSnapshotIncomeAsync(project, monthFrom, monthTo);
                    var items  = result.Success && result.Data != null
                        ? _mapper.Map<List<DepartmentIncomeTestItem>>(result.Data)
                        : new List<DepartmentIncomeTestItem>();
                    var cols = RemapCurrencyColumnsForDictionary(GridDataProvider.GetColumnsDefination<DepartmentIncomeTestItem>());
                    if (isCurrent)
                    {
                        // Current Data (Old Style): Access qryDeptIncomeTest uses SPC → WorkGroup → SCC
                        // Snapshot uses SPC → SCC → WorkGroup (model declaration order).
                        // Swap SCC and WorkGroup to match the current-tab Access column sequence.
                        var sccIdx = cols.FindIndex(c => c.PropertyName == nameof(DepartmentIncomeTestItem.SCC));
                        var wgIdx  = cols.FindIndex(c => c.PropertyName == nameof(DepartmentIncomeTestItem.WorkGroup));
                        if (sccIdx >= 0 && wgIdx >= 0)
                            (cols[sccIdx], cols[wgIdx]) = (cols[wgIdx], cols[sccIdx]);
                    }
                    return (items.Select(RowToDictionary<DepartmentIncomeTestItem>).ToList(),
                            cols,
                            "Tests (qryDeptIncomeTest)");
                }
                case "qryDeptIncomeAnimal":
                {
                    var result = isCurrent
                        ? await _departmentIncomeService.GetAnimalIncomeCurrentAsync(project, monthFrom, monthTo)
                        : await _departmentIncomeService.GetAnimalIncomeAsync(project, monthFrom, monthTo);
                    var items  = result.Success && result.Data != null
                        ? _mapper.Map<List<DepartmentIncomeAnimalItem>>(result.Data)
                        : new List<DepartmentIncomeAnimalItem>();
                    var cols = RemapCurrencyColumnsForDictionary(GridDataProvider.GetColumnsDefination<DepartmentIncomeAnimalItem>());
                    if (!isCurrent)
                    {
                        // Snapshot tab: Access fPeriod* does not expose AnimalType, AnimalDays, or Rate —
                        // only Project … SCC + TotalCost are visible.
                        var snapshotHide = new[]
                        {
                            nameof(DepartmentIncomeAnimalItem.AnimalType),
                            nameof(DepartmentIncomeAnimalItem.AnimalDays),
                            nameof(DepartmentIncomeAnimalItem.Rate),
                        };
                        foreach (var col in cols.Where(c => snapshotHide.Contains(c.PropertyName)))
                            col.IsVisible = false;
                    }
                    return (items.Select(RowToDictionary<DepartmentIncomeAnimalItem>).ToList(),
                            cols,
                            "Animals (qryDeptIncomeAnimal)");
                }
                case "qryDeptIncomeExceptional":
                {
                    var result = isCurrent
                        ? await _departmentIncomeService.GetAdditionalIncomeCurrentAsync(project, monthFrom, monthTo)
                        : await _departmentIncomeService.GetAdditionalIncomeAsync(project, monthFrom, monthTo);
                    var items  = result.Success && result.Data != null
                        ? _mapper.Map<List<DepartmentIncomeAdditionalItem>>(result.Data)
                        : new List<DepartmentIncomeAdditionalItem>();
                    return (items.Select(RowToDictionary<DepartmentIncomeAdditionalItem>).ToList(),
                            RemapCurrencyColumnsForDictionary(GridDataProvider.GetColumnsDefination<DepartmentIncomeAdditionalItem>()),
                            "Exceptional (qryDeptIncomeExceptional)");
                }
                case "qryDeptIncomeTotals":
                {
                    var result = isCurrent
                        ? await _departmentIncomeService.GetTotalsCurrentAsync(project, monthFrom, monthTo)
                        : await _departmentIncomeService.GetTotalsAsync(project, monthFrom, monthTo);
                    var items  = result.Success && result.Data != null
                        ? _mapper.Map<List<DepartmentIncomeTotalsItem>>(result.Data)
                        : new List<DepartmentIncomeTotalsItem>();
                    var cols = RemapCurrencyColumnsForDictionary(GridDataProvider.GetColumnsDefination<DepartmentIncomeTotalsItem>());
                    if (isCurrent)
                    {
                        // Current tab shows all 7 columns including per-area subtotals
                        foreach (var col in cols)
                            col.IsVisible = true;
                    }
                    return (items.Select(RowToDictionary<DepartmentIncomeTotalsItem>).ToList(),
                            cols,
                            "Totals (qryDeptIncomeTotals)");
                }
                default: // "qryDeptIncomeTime"
                {
                    var result = isCurrent
                        ? await _departmentIncomeService.GetTimeIncomeCurrentAsync(project, monthFrom, monthTo)
                        : await _departmentIncomeService.GetTimeIncomeAsync(project, monthFrom, monthTo);
                    var items  = result.Success && result.Data != null
                        ? _mapper.Map<List<DepartmentIncomeTimeItem>>(result.Data)
                        : new List<DepartmentIncomeTimeItem>();
                    return (items.Select(RowToDictionary<DepartmentIncomeTimeItem>).ToList(),
                            RemapCurrencyColumnsForDictionary(GridDataProvider.GetColumnsDefination<DepartmentIncomeTimeItem>()),
                            "Time (qryDeptIncomeTime)");
                }
            }
        }

        // Projects a typed view-model row into a Dictionary<string, string?> keyed by PropertyName.
        // Mirrors TestCapabilityRepository.GetPagedTestPlanCrossTabAsync row-projection pattern.
        // Currency formatting (£) is applied by GridHelpers.FormatValue when the grid renders,
        // which handles string-backed decimal values via TryFormatStringAsGbp.
        private static Dictionary<string, string?> RowToDictionary<T>(T row)
        {
            var dict = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            foreach (var prop in typeof(T).GetProperties())
                dict[prop.Name] = prop.GetValue(row)?.ToString();
            return dict;
        }

        // Remaps GbpValue and GbpValueRounded column types to RoundTwoDecimal so that
        // string-backed decimal values in dictionary rows are formatted with £ by
        // GridHelpers.FormatValue — the same approach TestPlanCrossTab uses for all columns.
        private static List<DataGridColumn> RemapCurrencyColumnsForDictionary(List<DataGridColumn> columns)
            => columns;

        // Applies column-level filtering and sorting to dictionary rows.
        // filterDict keys are property names (case-insensitive); values are substring search terms.
        // sortBy is a property name; descending controls direction.
        private static List<Dictionary<string, string?>> ApplyFilterAndSort(
            List<Dictionary<string, string?>> rows,
            Dictionary<string, string>? filterDict,
            string? sortBy,
            bool descending)
        {
            IEnumerable<Dictionary<string, string?>> result = rows;

            // Apply per-column filters
            if (filterDict != null)
            {
                foreach (var (key, value) in filterDict)
                {
                    if (string.IsNullOrWhiteSpace(value))
                        continue;

                    var term = value.Trim();
                    result = result.Where(row =>
                        row.TryGetValue(key, out var cell) &&
                        cell != null &&
                        cell.Contains(term, StringComparison.OrdinalIgnoreCase));
                }
            }

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                result = descending
                    ? result.OrderByDescending(row => row.TryGetValue(sortBy, out var v) ? v : null,
                          StringComparer.OrdinalIgnoreCase)
                    : result.OrderBy(row => row.TryGetValue(sortBy, out var v) ? v : null,
                          StringComparer.OrdinalIgnoreCase);
            }

            return result.ToList();
        }

        // ── Private Grid Config Builders ──────────────────────────────────────────

        // PeriodName is the natural PK — used as key for Edit action
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
                KeyProperty        = "PeriodName",
                AllowAdd           = false,
                AddFunction        = string.Empty,
                AllowEdit          = true,
                EditFunction       = "editDeptIncomeSnapshotPeriod",
                AllowDelete        = false,
                DeleteFunction     = string.Empty,
                ExtraFilterMethod  = "getDepartmentIncomeSnapshotExtraFilters",
                BindGridUrl        = "/FPS/DepartmentIncome/LoadSnapshotGrid",
                Data               = items,
                Columns            = GridDataProvider.GetColumnsDefination<DepartmentIncomeSnapshotItem>(null),
                Pagination         = pagination,
                CurrentFilters     = filterDict
            };
        }



            }
        }
