/*
 * TRANSFORMENGINE MIGRATION — YearlyFinancialDataController.cs (frontend MVC)
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-07-09
 * Phase 14 security fix applied : 2026-07-09
 *
 * CHANGED:
 *   - New file: ASP.NET Core MVC controller for the Yearly Financial Data page (frmProjectRadTrackData)
 *   - [Area("PIMS")], [Authorize(Roles="PIMSAdmin,PIMSUser")], [AuthorizeForScopes] applied
 *   - Index() builds full DataGridConfig<YearlyFinancialDataItem> for costCenterListGrid
 *     — NEVER left as new(); always explicitly populated
 *   - Project dropdown: explicit <select id="yfdProject"> outside the grid in HTML prototype
 *     → PopulateDropdownsAsync() builds ProjectList from IProjectListService
 *   - StartDate / EndDate display fields populated from IProjectDetailsService.GetPimsDetailAsync()
 *   - CRUD endpoints: AllowAdd=true, AllowEdit=true, AllowDelete=true
 *     (Save/Update/edit/delete buttons confirmed in frmProjectRadTrackData.html modal)
 *   - GetById: GET endpoint for populating the edit modal (returns YearlyFinancialDataItem)
 *   - GetPactCosts: GET endpoint for the "Update Costing" button (returns PactCostsItem JSON)
 *   - Composite key: (short year, string project) — both required for all keyed endpoints
 *   - LoadYearlyFinancialDataGrid: POST AJAX reload endpoint for the DataGrid
 *   - Phase 14 security fix: [ValidateAntiForgeryToken] added to Create(POST) and Edit(POST)
 *     mutations — matches MilestoneController, ProjectDetailsController, InvoiceController
 *     convention; LoadYearlyFinancialDataGrid and Delete(HttpDelete) are intentionally
 *     excluded per InvoiceController.LoadInvoiceGrid and MilestoneController.DeleteMilestone
 *     peer pattern (non-mutating reload and DELETE method respectively)
 *
 * PRESERVED:
 *   - Backend CRUD resource: IYearlyFinancialDataService (not any lookup service family)
 *   - Lookup services (IProjectListService, IProjectDetailsService) used ONLY for dropdowns/display
 *   - All conditional branch logic around ModelState validation
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm the project string parameter passed to LoadYearlyFinancialDataGrid
 *     comes from the client JS ExtraFilterMethod ("getYearlyFinancialDataExtraFilters") sending
 *     the current SelectedProject — verify JS implementation in the Razor view
 *   - TRANSFORMENGINE TODO: Confirm Create/Update map YearlyFinancialDataItem back to
 *     YearlyFinancialDataDto correctly via AutoMapper before calling service
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PIMS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PIMS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.Areas.PIMS.Controllers
{
    // TRANSFORMENGINE: [Area], [Authorize], [AuthorizeForScopes] applied per project convention
    [Area("PIMS")]
    [Authorize(Roles = "PIMSAdmin,PIMSUser")]
    [AuthorizeForScopes(ScopeKeySection = "PIMSApiSettings:Scope")]
    public class YearlyFinancialDataController : Controller
    {
        private readonly IMapper _mapper;

        // TRANSFORMENGINE: Main CRUD service — IYearlyFinancialDataService bound to the
        //                  YearlyFinancialData backend resource (api/v1/yearlyfinancialdata).
        //                  Layer boundary: never inject API clients or repositories directly.
        private readonly IYearlyFinancialDataService _service;

        // TRANSFORMENGINE: Lookup services — ONLY used for project dropdown and date display.
        //                  NOT the CRUD resource for this controller.
        private readonly IProjectListService _projectListService;
        private readonly IProjectDetailsService _projectDetailsService;

        public YearlyFinancialDataController(
            IMapper mapper,
            IYearlyFinancialDataService service,
            IProjectListService projectListService,
            IProjectDetailsService projectDetailsService)
        {
            _mapper = mapper;
            _service = service;
            _projectListService = projectListService;
            _projectDetailsService = projectDetailsService;
        }

        // ── Index ─────────────────────────────────────────────────────────

        public async Task<IActionResult> Index(string? project)
        {
            YearlyFinancialDataViewModel viewModel = new();

            // TRANSFORMENGINE: Build project dropdown from lookup service (not CRUD service)
            await PopulateDropdownsAsync(viewModel);

            // Resolve selected project — from route param or first in list
            viewModel.SelectedProject = project
                ?? viewModel.ProjectList.FirstOrDefault()?.Value
                ?? string.Empty;

            // TRANSFORMENGINE: Load project start/end dates for display toolbar
            if (!string.IsNullOrWhiteSpace(viewModel.SelectedProject))
            {
                ApiResponseDto<ProjectDetailDto> detailResult =
                    await _projectDetailsService.GetPimsDetailAsync(viewModel.SelectedProject);

                if (detailResult.Success && detailResult.Data != null)
                {
                    ProjectDetailDto detail = detailResult.Data;
                    viewModel.StartDate = detail.StartDate.HasValue
                        ? detail.StartDate.Value.ToString("dd/MM/yyyy")
                        : string.Empty;
                    viewModel.EndDate = detail.RevisedEndDate.HasValue
                        ? detail.RevisedEndDate.Value.ToString("dd/MM/yyyy")
                        : (detail.EndDate.HasValue
                            ? detail.EndDate.Value.ToString("dd/MM/yyyy")
                            : string.Empty);
                }
            }

            // TRANSFORMENGINE: Build full DataGridConfig — NEVER left as new()
            //                  AllowAdd/Edit/Delete=true confirmed from Save/Update/edit/delete
            //                  buttons in frmProjectRadTrackData.html modal
            viewModel.CostCenterListGrid = new DataGridConfig<YearlyFinancialDataItem>
            {
                GridId             = "costCenterListGrid",
                Title              = "Yearly Financial Details",
                ShowCheckboxColumn = true,
                ShowPagination     = true,
                KeyProperty        = "Year",
                AllowAdd           = true,
                AddFunction        = "addYearlyFinancialData",
                AllowEdit          = true,
                EditFunction       = "editYearlyFinancialData",
                AllowDelete        = true,
                DeleteFunction     = "deleteYearlyFinancialData",
                AllowView          = true,
                ViewFunction       = "viewYearlyFinancialData",
                ExtraFilterMethod  = "getYearlyFinancialDataExtraFilters",
                BindGridUrl        = "/PIMS/YearlyFinancialData/LoadYearlyFinancialDataGrid",
                Data               = [],
                Columns            = GridDataProvider.GetColumnsDefination<YearlyFinancialDataItem>(null),
                Pagination         = new PaginationModel()
            };

            return View(viewModel);
        }

        // ── PopulateDropdownsAsync ────────────────────────────────────────

        // TRANSFORMENGINE: Explicit <select id="yfdProject"> found OUTSIDE the grid container
        //                  in frmProjectRadTrackData.html → justified page-level filter dropdown.
        private async Task PopulateDropdownsAsync(YearlyFinancialDataViewModel model)
        {
            ApiResponseDto<List<ProjectListViewDto>> projectResult =
                await _projectListService.GetAllProjectsListAsync();

            if (projectResult.Success && projectResult.Data != null)
            {
                model.ProjectList = projectResult.Data
                    .Select(p => new SelectListItem
                    {
                        Value    = p.Parentproject,
                        Text     = p.Parentproject,
                        Selected = string.Equals(model.SelectedProject, p.Parentproject,
                            StringComparison.OrdinalIgnoreCase)
                    })
                    .ToList();
            }
        }

        // ── DataGrid AJAX Reload ──────────────────────────────────────────

        // TRANSFORMENGINE: project param supplied by ExtraFilterMethod JS from current page selection
        [HttpPost]
        public async Task<IActionResult> LoadYearlyFinancialDataGrid(
            PaginationFilter<string> request, string? project = null)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors  = ModelState.Values
                        .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            DataGridConfig<YearlyFinancialDataItem> gridConfig =
                await BuildYearlyFinancialDataGridAsync(request, project);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<YearlyFinancialDataItem>> BuildYearlyFinancialDataGridAsync(
            PaginationFilter<string> request, string? project)
        {
            Dictionary<string, string> filterDict =
                JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                ?? new Dictionary<string, string>();

            // TRANSFORMENGINE: GetAllAsync requires project as a non-nullable route segment on the backend.
            //                  Resolve from filter dict or param; empty string triggers a 400 from the API.
            string resolvedProject = project
                ?? (filterDict.TryGetValue("project", out string? fp) ? fp : null)
                ?? string.Empty;

            QueryParameters<string> queryParameters = _mapper.Map<QueryParameters<string>>(request);

            ApiResponseDto<List<YearlyFinancialDataDto>> pagedData =
                await _service.GetAllAsync(resolvedProject, queryParameters);

            List<YearlyFinancialDataItem> items = pagedData.Success && pagedData.Data != null
                ? _mapper.Map<List<YearlyFinancialDataItem>>(pagedData.Data)
                : [];

            PaginationModel pagination = pagedData.Pagination is null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(pagedData.Pagination);
            pagination.SortColumn    = request.SortBy;
            pagination.SortDirection = request.Descending;

            return new DataGridConfig<YearlyFinancialDataItem>
            {
                GridId             = "costCenterListGrid",
                Title              = "Yearly Financial Details",
                ShowCheckboxColumn = true,
                ShowPagination     = true,
                KeyProperty        = "Year",
                AllowAdd           = true,
                AddFunction        = "addYearlyFinancialData",
                AllowEdit          = true,
                EditFunction       = "editYearlyFinancialData",
                AllowDelete        = true,
                DeleteFunction     = "deleteYearlyFinancialData",
                AllowView          = true,
                ViewFunction       = "viewYearlyFinancialData",
                ExtraFilterMethod  = "getYearlyFinancialDataExtraFilters",
                BindGridUrl        = "/PIMS/YearlyFinancialData/LoadYearlyFinancialDataGrid",
                Data               = items,
                Columns            = GridDataProvider.GetColumnsDefination<YearlyFinancialDataItem>(null),
                Pagination         = pagination,
                CurrentFilters     = filterDict
            };
        }

        // ── CRUD Endpoints ────────────────────────────────────────────────

        // TRANSFORMENGINE: AllowAdd=true — GET returns modal partial for adding a new yearly record
        [HttpGet]
        public IActionResult Create()
        {
            return PartialView("_AddEditYearlyFinancialData", new YearlyFinancialDataItem());
        }

        // TRANSFORMENGINE: AllowAdd=true — POST creates a new record; dto.Year + dto.Project required
        // TRANSFORMENGINE (Phase 14 — Security): [ValidateAntiForgeryToken] added — matches peer
        //   controllers (MilestoneController, InvoiceController); token sent by saveYFD() in Index.cshtml
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] YearlyFinancialDataDto dto)
        {
            if (dto is null)
                return Json(new { success = false, message = "Invalid data" });

            ApiResponseDto<YearlyFinancialDataDto> result = await _service.CreateAsync(dto);
            return result.Success
                ? Json(new { success = true, message = "Yearly financial record created successfully" })
                : Json(new { success = false, errors = result.Errors });
        }

        // TRANSFORMENGINE: AllowEdit=true — GET returns edit modal partial populated for composite key (year, project)
        [HttpGet]
        public async Task<IActionResult> Edit(short year, string project)
        {
            ApiResponseDto<YearlyFinancialDataDto> result = await _service.GetByKeyAsync(year, project);
            if (!result.Success || result.Data is null)
                return NotFound($"Yearly financial record for project '{project}' year {year} not found.");

            YearlyFinancialDataItem item = _mapper.Map<YearlyFinancialDataItem>(result.Data);
            return PartialView("_AddEditYearlyFinancialData", item);
        }

        // TRANSFORMENGINE: AllowEdit=true — POST updates a record by composite key (year, project)
        // TRANSFORMENGINE (Phase 14 — Security): [ValidateAntiForgeryToken] added — matches peer
        //   controllers; token sent by saveYFD() in Index.cshtml
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short year, string project, [FromBody] YearlyFinancialDataDto dto)
        {
            if (dto is null)
                return Json(new { success = false, message = "Invalid data" });

            ApiResponseDto<YearlyFinancialDataDto> result = await _service.UpdateAsync(year, project, dto);
            return result.Success
                ? Json(new { success = true, message = "Yearly financial record updated successfully" })
                : Json(new { success = false, errors = result.Errors });
        }

        // TRANSFORMENGINE: AllowDelete=true — DELETE by composite key; JS confirm() only (no modal partial)
        [HttpDelete]
        public async Task<IActionResult> Delete(short year, string project)
        {
            ApiResponseDto<bool> result = await _service.DeleteAsync(year, project);
            return result.Success
                ? Json(new { success = true, message = "Yearly financial record deleted successfully" })
                : Json(new { success = false, errors = result.Errors });
        }

        // TRANSFORMENGINE: AllowView=true — GET returns view-only modal partial (details/read-only view)
        [HttpGet]
        public async Task<IActionResult> GetById(short year, string project)
        {
            ApiResponseDto<YearlyFinancialDataDto> result = await _service.GetByKeyAsync(year, project);
            if (!result.Success || result.Data is null)
                return NotFound($"Yearly financial record for project '{project}' year {year} not found.");

            YearlyFinancialDataItem item = _mapper.Map<YearlyFinancialDataItem>(result.Data);
            return PartialView("_ViewYearlyFinancialData", item);
        }

        // TRANSFORMENGINE: GetPactCosts — called by the "Update Costing" button in the modal.
        //                  Returns PactCostsItem JSON for the left-column PACT actuals panel.
        //                  project + year are required business context — sourced from the
        //                  currently open row's composite key in the modal state.
        [HttpGet]
        public async Task<IActionResult> GetPactCosts(string project, short year)
        {
            ApiResponseDto<PactProjectYearCostsDto> result =
                await _service.GetPactCostsAsync(project, year);

            if (!result.Success || result.Data is null)
                return Json(new { success = false, message = "Failed to load PACT costs" });

            PactCostsItem item = _mapper.Map<PactCostsItem>(result.Data);
            return Json(new { success = true, data = item });
        }
    }
}
