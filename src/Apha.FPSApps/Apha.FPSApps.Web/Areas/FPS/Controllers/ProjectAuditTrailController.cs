/*
 * TRANSFORMENGINE MIGRATION — ProjectAuditTrailController.cs (MVC)
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - New file — MVC frontend controller for the Project Audit Trail tabbed read-only view
 *   - Index() builds ProjectAuditTrailViewModel with 5 empty read-only DataGridConfigs +
 *     project dropdown populated from IProjectService.GetAllProjectsAsync()
 *   - 5 AJAX POST endpoints (one per tab): LoadProjectLogsGrid, LoadStaffJobLogsGrid,
 *     LoadTestRequirementLogsGrid, LoadAnimalRequestLogsGrid, LoadAdditionalCostLogsGrid
 *   - Each AJAX endpoint calls the corresponding IProjectAuditTrailService method, maps DTOs
 *     to *LogItem via AutoMapper, and returns _DataGrid partial view
 *   - IProjectService injected for project dropdown only (lookup flow)
 *   - IProjectAuditTrailService injected for all 5 audit log CRUD-less reads (main resource flow)
 *   - No Create/Edit/Delete — showAddButton: false in all 5 JS grids
 *
 * PRESERVED:
 *   - Filter parameters matching HTML prototype: ParentProject (required for API call),
 *     FromDate and ToDate (optional DateOnly? matching IProjectAuditTrailService signatures)
 *   - Project select filter comes from HTML explicit <select id="filter-project"> outside grids
 *   - Date range filters from HTML <input id="filter-from"> and <input id="filter-to"> outside grids
 *   - Tab-per-grid structure: Project Detail Changes, Staff Plan Changes, Test Requirement Changes,
 *     Animal Requirement Changes, Exceptional Cost Changes
 *
 * --- Phase 14 — Pre-Build Security Review Gate (2026-06-22) ---
 * CHANGED (security):
 *   - Replaced Newtonsoft.Json JsonConvert.DeserializeObject with System.Text.Json
 *     JsonSerializer.Deserialize for user-supplied filter JSON in all 5 AJAX endpoints.
 *     System.Text.Json has no TypeNameHandling concept and is the secure-by-default
 *     BCL JSON library for .NET 10; removes the Newtonsoft.Json import from this file.
 *
 * SECURITY REVIEW FINDINGS:
 *   - [Authorize(Roles = "FPSAdmin,FPSUser")] present at class level — PASS
 *   - [AuthorizeForScopes] present (MSAL/token-based auth) — PASS
 *   - No [AllowAnonymous] overrides — PASS
 *   - ModelState.IsValid checked on all 5 POST endpoints — PASS
 *   - No [ValidateAntiForgeryToken] — consistent with FPS app convention (token-based auth,
 *     no global antiforgery configured); recorded in checklist as REVIEW
 *   - No raw SQL, no secret exposure, no stack trace leakage — PASS
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: UserEmail is ignored by AutoMapper for all 5 log types — requires
 *     backend service update to include resolved email from UserId before display.
 *   - TRANSFORMENGINE TODO: StaffJobLogItem.Name is ignored by AutoMapper — requires staff lookup
 *     join at backend or service layer before display.
 *   - TRANSFORMENGINE TODO: IProjectAuditTrailService methods require project (string) as a
 *     required parameter. The AJAX endpoints receive it from the posted filter dict.
 *     If project is empty/null, the endpoint returns an empty grid (not an error) to match
 *     the JS prototype behavior of clearing all grids when no project is selected.
 *   - TRANSFORMENGINE TODO: DateOnly? serialization from HTTP query — confirm JSON/form binding
 *     for FromDate/ToDate parameters in AJAX posts (ISO 8601 expected).
 *   - TRANSFORMENGINE TODO (SECURITY REVIEW): Anti-forgery token not applied on POST endpoints —
 *     consistent with FPS app convention (MSAL token-based auth). If cookie-based sessions are
 *     ever enabled, add [ValidateAntiForgeryToken] or configure AutoValidateAntiforgeryToken
 *     globally in Program.cs.
 */
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.Text.Json;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin,FPSUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class ProjectAuditTrailController : Controller
    {
        private readonly IMapper _mapper;
        // TRANSFORMENGINE: Main audit log service — IProjectAuditTrailService for all 5 log types
        private readonly IProjectAuditTrailService _auditTrailService;
        // TRANSFORMENGINE: Lookup-only service — IProjectService for project dropdown population only
        private readonly IProjectService _projectService;

        public ProjectAuditTrailController(
            IMapper mapper,
            IProjectAuditTrailService auditTrailService,
            IProjectService projectService)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _auditTrailService = auditTrailService ?? throw new ArgumentNullException(nameof(auditTrailService));
            _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
        }

        // TRANSFORMENGINE: GET /FPS/ProjectAuditTrail — renders tabbed audit trail page with
        // 5 empty read-only grids and project dropdown pre-populated from IProjectService
        public async Task<IActionResult> Index()
        {
            var viewModel = new ProjectAuditTrailViewModel();
            await PopulateDropdownsAsync(viewModel);

            // TRANSFORMENGINE: Tab 1 — Project Detail Changes grid config (AllowAdd/Edit/Delete all false)
            viewModel.ProjectLogsGrid = BuildProjectLogsGridConfig(
                new List<ProjectLogItem>(), new PaginationModel(), null);

            // TRANSFORMENGINE: Tab 2 — Staff Plan Changes grid config
            viewModel.StaffJobLogsGrid = BuildStaffJobLogsGridConfig(
                new List<StaffJobLogItem>(), new PaginationModel(), null);

            // TRANSFORMENGINE: Tab 3 — Test Requirement Changes grid config
            viewModel.TestRequirementLogsGrid = BuildTestRequirementLogsGridConfig(
                new List<TestRequirementLogItem>(), new PaginationModel(), null);

            // TRANSFORMENGINE: Tab 4 — Animal Requirement Changes grid config
            viewModel.AnimalRequestLogsGrid = BuildAnimalRequestLogsGridConfig(
                new List<AnimalRequestLogItem>(), new PaginationModel(), null);

            // TRANSFORMENGINE: Tab 5 — Exceptional Cost Changes grid config
            viewModel.AdditionalCostLogsGrid = BuildAdditionalCostLogsGridConfig(
                new List<AdditionalCostLogItem>(), new PaginationModel(), null);

            return View(viewModel);
        }

        // TRANSFORMENGINE: Populate project dropdown from IProjectService (lookup flow — separate from audit CRUD)
        private async Task PopulateDropdownsAsync(ProjectAuditTrailViewModel model)
        {
            var projectsResult = await _projectService.GetAllProjectsAsync();
            if (projectsResult.Success && projectsResult.Data != null)
            {
                model.ProjectList = projectsResult.Data
                    .OrderBy(p => p.ParentProject)
                    .Select(p => new SelectListItem
                    {
                        Value = p.ParentProject,
                        Text = p.ParentProject,
                        Selected = string.Equals(model.ParentProject, p.ParentProject,
                            StringComparison.OrdinalIgnoreCase)
                    })
                    .ToList();
            }
        }

        // ── AJAX Reload Endpoints — one per tab ───────────────────────────────

        // TRANSFORMENGINE: Tab 1 — Project Detail Changes AJAX reload
        // project param required by IProjectAuditTrailService; empty → return empty grid
        [HttpPost]
        public async Task<IActionResult> LoadProjectLogsGrid(
            PaginationFilter<string> request,
            string? project = null,
            DateOnly? fromDate = null,
            DateOnly? toDate = null)
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

            // TRANSFORMENGINE: Phase 14 security fix — System.Text.Json replaces Newtonsoft.Json
            // (no TypeNameHandling risk; BCL secure-by-default JSON library for .NET 10)
            var filterDict = !string.IsNullOrEmpty(request.Filter)
                ? JsonSerializer.Deserialize<Dictionary<string, string>>(request.Filter)
                : null;

            // TRANSFORMENGINE: If no project selected, return empty grid (matches JS prototype resetFilters behavior)
            if (string.IsNullOrWhiteSpace(project))
            {
                var emptyConfig = BuildProjectLogsGridConfig(new List<ProjectLogItem>(), new PaginationModel(), filterDict);
                return PartialView("_DataGrid", emptyConfig);
            }

            var queryParameters = _mapper.Map<QueryParameters<string>>(request);
            var response = await _auditTrailService.GetProjectLogsAsync(queryParameters, project, fromDate, toDate);

            var items = response.Success && response.Data != null
                ? _mapper.Map<List<ProjectLogItem>>(response.Data)
                : new List<ProjectLogItem>();

            var pagination = BuildPagination(response.Pagination, request);
            var gridConfig = BuildProjectLogsGridConfig(items, pagination, filterDict);
            return PartialView("_DataGrid", gridConfig);
        }

        // TRANSFORMENGINE: Tab 2 — Staff Plan Changes AJAX reload
        [HttpPost]
        public async Task<IActionResult> LoadStaffJobLogsGrid(
            PaginationFilter<string> request,
            string? project = null,
            DateOnly? fromDate = null,
            DateOnly? toDate = null)
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

            // TRANSFORMENGINE: Phase 14 security fix — System.Text.Json replaces Newtonsoft.Json
            // (no TypeNameHandling risk; BCL secure-by-default JSON library for .NET 10)
            var filterDict = !string.IsNullOrEmpty(request.Filter)
                ? JsonSerializer.Deserialize<Dictionary<string, string>>(request.Filter)
                : null;

            if (string.IsNullOrWhiteSpace(project))
            {
                var emptyConfig = BuildStaffJobLogsGridConfig(new List<StaffJobLogItem>(), new PaginationModel(), filterDict);
                return PartialView("_DataGrid", emptyConfig);
            }

            var queryParameters = _mapper.Map<QueryParameters<string>>(request);
            var response = await _auditTrailService.GetStaffJobLogsAsync(queryParameters, project, fromDate, toDate);

            var items = response.Success && response.Data != null
                ? _mapper.Map<List<StaffJobLogItem>>(response.Data)
                : new List<StaffJobLogItem>();

            var pagination = BuildPagination(response.Pagination, request);
            var gridConfig = BuildStaffJobLogsGridConfig(items, pagination, filterDict);
            return PartialView("_DataGrid", gridConfig);
        }

        // TRANSFORMENGINE: Tab 3 — Test Requirement Changes AJAX reload
        [HttpPost]
        public async Task<IActionResult> LoadTestRequirementLogsGrid(
            PaginationFilter<string> request,
            string? project = null,
            DateOnly? fromDate = null,
            DateOnly? toDate = null)
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

            // TRANSFORMENGINE: Phase 14 security fix — System.Text.Json replaces Newtonsoft.Json
            // (no TypeNameHandling risk; BCL secure-by-default JSON library for .NET 10)
            var filterDict = !string.IsNullOrEmpty(request.Filter)
                ? JsonSerializer.Deserialize<Dictionary<string, string>>(request.Filter)
                : null;

            if (string.IsNullOrWhiteSpace(project))
            {
                var emptyConfig = BuildTestRequirementLogsGridConfig(new List<TestRequirementLogItem>(), new PaginationModel(), filterDict);
                return PartialView("_DataGrid", emptyConfig);
            }

            var queryParameters = _mapper.Map<QueryParameters<string>>(request);
            var response = await _auditTrailService.GetTestRequirementLogsAsync(queryParameters, project, fromDate, toDate);

            var items = response.Success && response.Data != null
                ? _mapper.Map<List<TestRequirementLogItem>>(response.Data)
                : new List<TestRequirementLogItem>();

            var pagination = BuildPagination(response.Pagination, request);
            var gridConfig = BuildTestRequirementLogsGridConfig(items, pagination, filterDict);
            return PartialView("_DataGrid", gridConfig);
        }

        // TRANSFORMENGINE: Tab 4 — Animal Requirement Changes AJAX reload
        [HttpPost]
        public async Task<IActionResult> LoadAnimalRequestLogsGrid(
            PaginationFilter<string> request,
            string? project = null,
            DateOnly? fromDate = null,
            DateOnly? toDate = null)
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

            // TRANSFORMENGINE: Phase 14 security fix — System.Text.Json replaces Newtonsoft.Json
            // (no TypeNameHandling risk; BCL secure-by-default JSON library for .NET 10)
            var filterDict = !string.IsNullOrEmpty(request.Filter)
                ? JsonSerializer.Deserialize<Dictionary<string, string>>(request.Filter)
                : null;

            if (string.IsNullOrWhiteSpace(project))
            {
                var emptyConfig = BuildAnimalRequestLogsGridConfig(new List<AnimalRequestLogItem>(), new PaginationModel(), filterDict);
                return PartialView("_DataGrid", emptyConfig);
            }

            var queryParameters = _mapper.Map<QueryParameters<string>>(request);
            var response = await _auditTrailService.GetAnimalRequestLogsAsync(queryParameters, project, fromDate, toDate);

            var items = response.Success && response.Data != null
                ? _mapper.Map<List<AnimalRequestLogItem>>(response.Data)
                : new List<AnimalRequestLogItem>();

            var pagination = BuildPagination(response.Pagination, request);
            var gridConfig = BuildAnimalRequestLogsGridConfig(items, pagination, filterDict);
            return PartialView("_DataGrid", gridConfig);
        }

        // TRANSFORMENGINE: Tab 5 — Exceptional Cost Changes AJAX reload
        [HttpPost]
        public async Task<IActionResult> LoadAdditionalCostLogsGrid(
            PaginationFilter<string> request,
            string? project = null,
            DateOnly? fromDate = null,
            DateOnly? toDate = null)
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

            // TRANSFORMENGINE: Phase 14 security fix — System.Text.Json replaces Newtonsoft.Json
            // (no TypeNameHandling risk; BCL secure-by-default JSON library for .NET 10)
            var filterDict = !string.IsNullOrEmpty(request.Filter)
                ? JsonSerializer.Deserialize<Dictionary<string, string>>(request.Filter)
                : null;

            if (string.IsNullOrWhiteSpace(project))
            {
                var emptyConfig = BuildAdditionalCostLogsGridConfig(new List<AdditionalCostLogItem>(), new PaginationModel(), filterDict);
                return PartialView("_DataGrid", emptyConfig);
            }

            var queryParameters = _mapper.Map<QueryParameters<string>>(request);
            var response = await _auditTrailService.GetAdditionalCostLogsAsync(queryParameters, project, fromDate, toDate);

            var items = response.Success && response.Data != null
                ? _mapper.Map<List<AdditionalCostLogItem>>(response.Data)
                : new List<AdditionalCostLogItem>();

            var pagination = BuildPagination(response.Pagination, request);
            var gridConfig = BuildAdditionalCostLogsGridConfig(items, pagination, filterDict);
            return PartialView("_DataGrid", gridConfig);
        }

        // ── Private Grid Config Builders — one per tab ───────────────────────

        // TRANSFORMENGINE: showAddButton=false, no edit/delete buttons in any tab → all Allow* false
        // ExtraFilterMethod wired to JS functions in Index.cshtml that pass project/fromDate/toDate
        private DataGridConfig<ProjectLogItem> BuildProjectLogsGridConfig(
            List<ProjectLogItem> items,
            PaginationModel pagination,
            Dictionary<string, string>? filterDict)
        {
            return new DataGridConfig<ProjectLogItem>
            {
                GridId             = "projectAuditTrailGrid",
                Title              = "Project Detail Changes",
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "SequenceNo",
                AllowAdd           = false,
                AddFunction        = string.Empty,
                AllowEdit          = false,
                EditFunction       = string.Empty,
                AllowDelete        = false,
                DeleteFunction     = string.Empty,
                // TRANSFORMENGINE: JS function declared in Index.cshtml; sends project/fromDate/toDate
                ExtraFilterMethod  = "getProjectAuditTrailExtraFilters",
                BindGridUrl        = "/FPS/ProjectAuditTrail/LoadProjectLogsGrid",
                Data               = items,
                Columns            = GridDataProvider.GetColumnsDefination<ProjectLogItem>(null),
                Pagination         = pagination,
                CurrentFilters     = filterDict
            };
        }

        private DataGridConfig<StaffJobLogItem> BuildStaffJobLogsGridConfig(
            List<StaffJobLogItem> items,
            PaginationModel pagination,
            Dictionary<string, string>? filterDict)
        {
            return new DataGridConfig<StaffJobLogItem>
            {
                GridId             = "staffPlanChangesGrid",
                Title              = "Staff Plan Changes",
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "SequenceNo",
                AllowAdd           = false,
                AddFunction        = string.Empty,
                AllowEdit          = false,
                EditFunction       = string.Empty,
                AllowDelete        = false,
                DeleteFunction     = string.Empty,
                // TRANSFORMENGINE: JS function declared in Index.cshtml; sends project/fromDate/toDate
                ExtraFilterMethod  = "getStaffPlanChangesExtraFilters",
                BindGridUrl        = "/FPS/ProjectAuditTrail/LoadStaffJobLogsGrid",
                Data               = items,
                Columns            = GridDataProvider.GetColumnsDefination<StaffJobLogItem>(null),
                Pagination         = pagination,
                CurrentFilters     = filterDict
            };
        }

        private DataGridConfig<TestRequirementLogItem> BuildTestRequirementLogsGridConfig(
            List<TestRequirementLogItem> items,
            PaginationModel pagination,
            Dictionary<string, string>? filterDict)
        {
            return new DataGridConfig<TestRequirementLogItem>
            {
                GridId             = "testRequirementChangesGrid",
                Title              = "Test Requirement Changes",
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "SequenceNo",
                AllowAdd           = false,
                AddFunction        = string.Empty,
                AllowEdit          = false,
                EditFunction       = string.Empty,
                AllowDelete        = false,
                DeleteFunction     = string.Empty,
                // TRANSFORMENGINE: JS function declared in Index.cshtml; sends project/fromDate/toDate
                ExtraFilterMethod  = "getTestRequirementChangesExtraFilters",
                BindGridUrl        = "/FPS/ProjectAuditTrail/LoadTestRequirementLogsGrid",
                Data               = items,
                Columns            = GridDataProvider.GetColumnsDefination<TestRequirementLogItem>(null),
                Pagination         = pagination,
                CurrentFilters     = filterDict
            };
        }

        private DataGridConfig<AnimalRequestLogItem> BuildAnimalRequestLogsGridConfig(
            List<AnimalRequestLogItem> items,
            PaginationModel pagination,
            Dictionary<string, string>? filterDict)
        {
            return new DataGridConfig<AnimalRequestLogItem>
            {
                GridId             = "animalRequirementChangesGrid",
                Title              = "Animal Requirement Changes",
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "SequenceNo",
                AllowAdd           = false,
                AddFunction        = string.Empty,
                AllowEdit          = false,
                EditFunction       = string.Empty,
                AllowDelete        = false,
                DeleteFunction     = string.Empty,
                // TRANSFORMENGINE: JS function declared in Index.cshtml; sends project/fromDate/toDate
                ExtraFilterMethod  = "getAnimalRequirementChangesExtraFilters",
                BindGridUrl        = "/FPS/ProjectAuditTrail/LoadAnimalRequestLogsGrid",
                Data               = items,
                Columns            = GridDataProvider.GetColumnsDefination<AnimalRequestLogItem>(null),
                Pagination         = pagination,
                CurrentFilters     = filterDict
            };
        }

        private DataGridConfig<AdditionalCostLogItem> BuildAdditionalCostLogsGridConfig(
            List<AdditionalCostLogItem> items,
            PaginationModel pagination,
            Dictionary<string, string>? filterDict)
        {
            return new DataGridConfig<AdditionalCostLogItem>
            {
                GridId             = "exceptionalCostChangesGrid",
                Title              = "Exceptional Cost Changes",
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "SequenceNo",
                AllowAdd           = false,
                AddFunction        = string.Empty,
                AllowEdit          = false,
                EditFunction       = string.Empty,
                AllowDelete        = false,
                DeleteFunction     = string.Empty,
                // TRANSFORMENGINE: JS function declared in Index.cshtml; sends project/fromDate/toDate
                ExtraFilterMethod  = "getExceptionalCostChangesExtraFilters",
                BindGridUrl        = "/FPS/ProjectAuditTrail/LoadAdditionalCostLogsGrid",
                Data               = items,
                Columns            = GridDataProvider.GetColumnsDefination<AdditionalCostLogItem>(null),
                Pagination         = pagination,
                CurrentFilters     = filterDict
            };
        }

        // TRANSFORMENGINE: Helper to build PaginationModel from API response PaginationDto
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
