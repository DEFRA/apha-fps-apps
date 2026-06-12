// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — GradeMaintenanceController.cs (new file)
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet8-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-06-10
 *
 * CHANGED:
 *   - New ASP.NET Core MVC controller created for the Grade maintenance form (frmMaintGrade)
 *   - [Area("FPS")], [Authorize(Roles = "FPSAdmin")], [AuthorizeForScopes] attributes applied
 *     matching existing FPS area controller conventions (e.g. DivisionGradeMaintenanceController)
 *   - Index() builds full DataGridConfig<GradeItem> explicitly — never left as new()
 *   - LoadGradeGrid(): HTTP POST AJAX endpoint returning _DataGrid partial with paginated GradeItem list
 *   - GetGradeGridConfigAsync(): private helper — maps QueryParameters → IGradeService.GetAllPagedAsync
 *   - Create GET: returns _AddEditGrade partial with empty GradeItem
 *   - Create POST [FromBody]: accepts GradeDto, calls IGradeService.CreateAsync
 *   - Edit GET: fetches record by gradeCode → _AddEditGrade partial
 *   - Edit POST [FromBody]: accepts GradeDto with originalCode route param, calls IGradeService.UpdateAsync
 *   - Delete [HttpDelete]: calls IGradeService.DeleteAsync; returns JSON success/failure
 *   - No PopulateDropdownsAsync — HTML prototype (frmMaintGrade.html) has no <select> outside the grid
 *   - No ExtraFilterMethod on DataGridConfig — no page-level filter controls in prototype
 *   - DataGridConfig: AllowAdd=true (JS showAddButton:true), AllowEdit=true, AllowDelete=true
 *     (edit + delete buttons confirmed in JS actions column render function)
 *
 * PRESERVED:
 *   - CRUD separation: only IGradeService injected (no lookup services, no API clients, no repositories)
 *   - Error response shape matches existing FPS controller conventions (success, message, errors[])
 *   - BindGridUrl matches controller route: /FPS/GradeMaintenance/LoadGradeGrid
 *   - KeyProperty = "GradeCode" (sole visible PK column in JS grid; composite PK partner FpsYear is hidden)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Register IGradeService → GradeService in
 *     Apha.FPSApps.Web/Extensions/ServiceCollectionExtension.cs if not already done in Phase 10.
 *   - TRANSFORMENGINE TODO: Verify GradeMaintenanceController is discovered by the FPS Area route in
 *     Program.cs (MapAreaControllerRoute or MapControllerRoute with area constraint).
 *   - TRANSFORMENGINE TODO: Edit POST — originalCode is taken from dto.GradeCode before any potential
 *     rename value is applied. Confirm the frontend JS sends the original code as a separate field or
 *     route parameter when GradeCode rename is desired; adjust if needed.
 *   - TRANSFORMENGINE TODO: _AddEditGrade.cshtml partial (Phase 12) must set GradeCode input
 *     as read-only when editing an existing record (editingGradeId pattern in JS prototype).
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{
    // TRANSFORMENGINE: [Area] + [Authorize] + [AuthorizeForScopes] — matches FPS area controller convention
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class GradeMaintenanceController : Controller
    {
        private readonly IMapper _mapper;

        // TRANSFORMENGINE: only IGradeService injected — no API clients, no repositories per layer boundary rule
        private readonly IGradeService _gradeService;

        public GradeMaintenanceController(IMapper mapper, IGradeService gradeService)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _gradeService = gradeService ?? throw new ArgumentNullException(nameof(gradeService));
        }

        // ── Index ─────────────────────────────────────────────────────────────

        public async Task<IActionResult> Index()
        {
            var viewModel = new GradeMaintenanceViewModel();

            // TRANSFORMENGINE: DataGridConfig built explicitly — leaving as new() would render
            // an empty grid with default Add button regardless of JS-derived operations profile.
            // AllowAdd:true from JS showAddButton:true; AllowEdit+AllowDelete:true from JS
            // actions column containing both edit and delete buttons (fps_grade_maintenance.js).
            var defaultRequest = new PaginationFilter<string>
            {
                Filter = "{}",
                SortBy = "GradeCode",
                Descending = false
            };
            viewModel.GradeGrid = await GetGradeGridConfigAsync(defaultRequest);

            return View(viewModel);
        }

        // ── DataGrid AJAX Reload ─────────────────────────────────────────────

        // TRANSFORMENGINE: HTTP POST — DataGrid sends pagination + filter payload to this endpoint
        [HttpPost]
        public async Task<IActionResult> LoadGradeGrid(PaginationFilter<string> request)
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

            var gridConfig = await GetGradeGridConfigAsync(request);
            return PartialView("_DataGrid", gridConfig);
        }

        // TRANSFORMENGINE: private grid config builder — called from Index() and LoadGradeGrid()
        private async Task<DataGridConfig<GradeItem>> GetGradeGridConfigAsync(PaginationFilter<string> request)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                request.Filter ?? "{}") ?? new Dictionary<string, string>();

            var queryParameters = _mapper.Map<QueryParameters<string>>(request);
            var pagedData = await _gradeService.GetAllPagedAsync(queryParameters);

            var items = new List<GradeItem>();
            if (pagedData.Data != null)
            {
                items = _mapper.Map<List<GradeItem>>(pagedData.Data);
            }

            var paginationModel = pagedData.Pagination == null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(pagedData.Pagination);
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<GradeItem>
            {
                // TRANSFORMENGINE: GridId from JS gradeGrid; Title from JS DataGridComponent title
                GridId             = "gradeGrid",
                Title              = "Grade Maintenance",
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                // TRANSFORMENGINE: KeyProperty = GradeCode (sole visible PK column; FpsYear is hidden)
                KeyProperty        = "GradeCode",
                // TRANSFORMENGINE: AllowAdd=true from JS showAddButton:true
                AllowAdd           = true,
                AddFunction        = "addGrade",
                // TRANSFORMENGINE: AllowEdit=true — edit button present in JS actions column render
                AllowEdit          = true,
                EditFunction       = "editGrade",
                // TRANSFORMENGINE: AllowDelete=true — delete button present in JS actions column render
                AllowDelete        = true,
                DeleteFunction     = "deleteGrade",
                // TRANSFORMENGINE: no ExtraFilterMethod — no page-level filter controls in HTML prototype
                BindGridUrl        = "/FPS/GradeMaintenance/LoadGradeGrid",
                Data               = items,
                Columns            = GridDataProvider.GetColumnsDefination<GradeItem>(null),
                Pagination         = paginationModel,
                CurrentFilters     = filterDict
            };
        }

        // ── CRUD — Create ────────────────────────────────────────────────────

        // TRANSFORMENGINE: GET Create — returns empty GradeItem to _AddEditGrade partial
        [HttpGet]
        public IActionResult Create()
        {
            return PartialView("_AddEditGrade", new GradeItem());
        }

        // TRANSFORMENGINE: POST Create — accepts GradeDto [FromBody] (JSON from modal form submit)
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] GradeDto dto)
        {
            if (dto is null)
            {
                return Json(new { success = false, message = "Invalid data" });
            }

            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any())
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new
                        {
                            field = kvp.Key,
                            message = e.ErrorMessage
                        }))
                });
            }

            var result = await _gradeService.CreateAsync(dto);

            if (result.Success)
            {
                return Json(new { success = true, data = result.Data, message = "Grade created successfully" });
            }

            var errorMessage = result.Errors?.FirstOrDefault()?.Message ?? "Failed to create grade.";
            return Json(new
            {
                success = false,
                message = errorMessage,
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        // ── CRUD — Edit ──────────────────────────────────────────────────────

        // TRANSFORMENGINE: GET Edit — loads existing grade by gradeCode → _AddEditGrade partial
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return Json(new { success = false, message = "Grade code is required" });
            }

            var result = await _gradeService.GetByIdAsync(id);

            if (result.Success && result.Data != null)
            {
                var item = _mapper.Map<GradeItem>(result.Data);
                return PartialView("_AddEditGrade", item);
            }

            return Json(new { success = false, message = $"Grade '{id}' not found." });
        }

        // TRANSFORMENGINE: POST Edit — id = originalCode (before potential GradeCode rename);
        // IGradeService.UpdateAsync signature: UpdateAsync(string originalCode, GradeDto dto)
        [HttpPost]
        public async Task<IActionResult> Edit(string id, [FromBody] GradeDto dto)
        {
            if (dto is null)
            {
                return Json(new { success = false, message = "Invalid data" });
            }

            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any())
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new
                        {
                            field = kvp.Key,
                            message = e.ErrorMessage
                        }))
                });
            }

            // TRANSFORMENGINE: originalCode = id (route param) to support GradeCode rename
            // (matches IGradeService.UpdateAsync(string originalCode, GradeDto dto) signature)
            var result = await _gradeService.UpdateAsync(id, dto);

            if (result.Success)
            {
                return Json(new { success = true, data = result.Data, message = "Grade updated successfully" });
            }

            var errorMessage = result.Errors?.FirstOrDefault()?.Message ?? "Failed to update grade.";
            return Json(new
            {
                success = false,
                message = errorMessage,
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        // ── CRUD — Delete ────────────────────────────────────────────────────

        // TRANSFORMENGINE: [HttpDelete] — JS confirm() in grid action handler; no modal partial needed
        [HttpDelete]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return Json(new { success = false, message = "Grade code is required" });
            }

            var result = await _gradeService.DeleteAsync(id);

            if (result.Success && result.Data)
            {
                return Json(new { success = true, message = "Grade deleted successfully" });
            }

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Unable to delete the grade as it may be in use."
            });
        }
    }
}
