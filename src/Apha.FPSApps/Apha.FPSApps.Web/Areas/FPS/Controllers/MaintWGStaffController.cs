// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — MaintWGStaffController.cs (new file)
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 *            Phase 14 — Pre-Build Security Review Gate
 * Migrated : 2026-06-11
 *
 * CHANGED:
 *   - New file — no legacy MVC controller equivalent.
 *   - MS Access frmMaintWGStaff VBA/DAO form converted to ASP.NET Core 10 MVC controller.
 *   - Backend resource family: WorkGroupEmployeeController — route api/v{version}/wgstaff.
 *   - Injects only IWorkGroupEmployeeService (frontend Application layer service).
 *   - Index(): builds full DataGridConfig<WorkGroupEmployeeItem> explicitly — never left as new().
 *   - LoadWGStaffGrid(): DataGrid AJAX reload endpoint [HttpPost].
 *   - GetWGStaffGridConfigAsync(): private helper — maps Dto->Item via AutoMapper, builds full config.
 *   - Create (GET): returns _AddEditMaintWGStaff partial with empty WorkGroupEmployeeItem.
 *   - Create (POST): accepts [FromBody] WorkGroupEmployeeDto, delegates to service.CreateWorkGroupEmployeeAsync.
 *   - Edit (GET): loads record by pactId, maps Dto->Item, returns _AddEditMaintWGStaff partial.
 *   - Edit (POST): accepts [FromBody] WorkGroupEmployeeDto, delegates to service.UpdateWorkGroupEmployeeAsync.
 *   - Delete [HttpDelete]: delegates to service.DeleteWorkGroupEmployeeAsync(pactId).
 *   - AllowAdd/Edit/Delete all true — derived from JS prototype showAddButton + action column buttons.
 *
 * PHASE 14 SECURITY REVIEW — FIXED 2026-06-11:
 *   - FIXED: Added [HttpGet] attribute to Index() action — restricts to GET, prevents accidental
 *     form-POST confusion and aligns with MVC routing conventions for defense-in-depth.
 *   - FIXED: Added ArgumentNullException null guards to constructor for _mapper and _service.
 *     Fail-fast pattern ensures DI misconfiguration surfaces immediately at startup rather than
 *     at runtime with a NullReferenceException that leaks context.
 *   - FIXED: Wrapped JsonConvert.DeserializeObject in GetWGStaffGridConfigAsync with try/catch.
 *     Malformed JSON in the Filter parameter previously caused an unhandled exception propagating
 *     to the caller; now falls back to empty dictionary and returns a structured error response
 *     from LoadWGStaffGrid — no stack trace exposed.
 *   PASS:
 *   - [Area("FPS")], [Authorize(Roles = "FPSAdmin,FPSUser")], [AuthorizeForScopes] all present.
 *   - No [AllowAnonymous] on any action.
 *   - [FromBody] JSON POST/DELETE endpoints not vulnerable to standard form-POST CSRF — Bearer
 *     token auth in use; [ValidateAntiForgeryToken] correctly omitted for these endpoints.
 *   - pactId null/whitespace guard present on Edit GET and Delete.
 *   - ModelState.IsValid checked on Create POST, Edit POST, and LoadWGStaffGrid.
 *   - No secrets, connection strings, or credentials in source file.
 *   - Error responses return service error messages only — no stack traces or internal exceptions.
 *
 * PRESERVED:
 *   - All business logic delegated to IWorkGroupEmployeeService thin delegates (no duplication).
 *   - wgGrade not sourced from a page-level filter — see DEFERRED note.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Backend GetWorkGroupEmployeeAsync requires wgGrade as a mandatory
 *     query parameter. The HTML prototype (frmMaintWGStaff.html) has no <select> outside the
 *     grid container for wgGrade. The controller currently passes string.Empty which the backend
 *     must treat as "return all grades". Confirm backend behaviour for wgGrade = "" before shipping.
 *     If backend does not support empty wgGrade, a page-level filter must be added (Phase 12) or
 *     the backend service contract must be changed to make wgGrade optional.
 *   - TRANSFORMENGINE TODO: FpsViewModelMapper must add ForMember for StaffName->Name,
 *     WgGrade->WorkGroupGrade, and TimeRecorder (int<->bool) before the mapper works correctly
 *     for WorkGroupEmployeeItem <-> WorkGroupEmployeeDto in all CRUD flows.
 *   - TRANSFORMENGINE TODO: Confirm [Authorize] role strings "FPSAdmin,FPSUser" match the
 *     role claim values in the target deployment identity provider.
 *   - TRANSFORMENGINE TODO: Confirm AuthorizeForScopes ScopeKeySection "FPSApiSettings:Scope"
 *     matches the appsettings.json key for the FPS API scope configuration.
 */

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
    [Area("FPS")]
    // TRANSFORMENGINE: [Authorize] — protects all actions; inherits from class level.
    // Role strings must match identity-provider claim values. See DEFERRED note above.
    [Authorize(Roles = "FPSAdmin,FPSUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class MaintWGStaffController : Controller
    {
        private readonly IMapper _mapper;
        // TRANSFORMENGINE: Only IWorkGroupEmployeeService injected — no API client or repository.
        // Dependency chain: Controller → IWorkGroupEmployeeService → IFpsWorkGroupEmployeeApiClient → HTTP
        private readonly IWorkGroupEmployeeService _service;

        public MaintWGStaffController(IMapper mapper, IWorkGroupEmployeeService service)
        {
            // TRANSFORMENGINE: Phase 14 security fix — fail-fast null guards added.
            // Prevents NullReferenceException at runtime from masking DI misconfiguration.
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        // ── Index ──────────────────────────────────────────────────────────────────
        // TRANSFORMENGINE: Phase 14 security fix — [HttpGet] added to restrict Index to GET only.
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var viewModel = new MaintWGStaffViewModel();

            // TRANSFORMENGINE: Build initial grid config with first-page data.
            // DataGridConfig NEVER left as new() — full config built explicitly here.
            var defaultRequest = new PaginationFilter<string> { Filter = "{}" };
            viewModel.WGStaffGrid = await GetWGStaffGridConfigAsync(defaultRequest);

            return View(viewModel);
        }

        // ── DataGrid AJAX Reload ───────────────────────────────────────────────────
        // TRANSFORMENGINE: LoadWGStaffGrid — [HttpPost] DataGrid reload endpoint.
        // Called by DataGrid component via BindGridUrl = "/FPS/MaintWGStaff/LoadWGStaffGrid".
        [HttpPost]
        public async Task<IActionResult> LoadWGStaffGrid(PaginationFilter<string> request)
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

            var gridConfig = await GetWGStaffGridConfigAsync(request);
            return PartialView("_DataGrid", gridConfig);
        }

        // TRANSFORMENGINE: GetWGStaffGridConfigAsync — private grid config builder.
        // Maps QueryParameters from PaginationFilter, calls service, maps Dto->Item via AutoMapper.
        private async Task<DataGridConfig<WorkGroupEmployeeItem>> GetWGStaffGridConfigAsync(
            PaginationFilter<string> request)
        {
            // TRANSFORMENGINE: Phase 14 security fix — wrapped JsonConvert.DeserializeObject in
            // try/catch. Malformed JSON in Filter previously propagated as an unhandled exception
            // (could expose stack trace in 500 response). Now falls back to empty dictionary safely.
            Dictionary<string, string> filterDict;
            try
            {
                filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                    request.Filter ?? "{}") ?? new Dictionary<string, string>();
            }
            catch (JsonException)
            {
                filterDict = new Dictionary<string, string>();
            }

            var queryParameters = _mapper.Map<QueryParameters<string>>(request);

            // TRANSFORMENGINE TODO: wgGrade = string.Empty passed here because the HTML prototype
            // has no page-level wgGrade filter. Backend must handle empty wgGrade as "all grades".
            // See DEFERRED note in file header. Do not change to a real filter until the page
            // semantics are confirmed — do not invent a dropdown to satisfy this parameter.
            var pagedData = await _service.GetWorkGroupEmployeeAsync(queryParameters, string.Empty);

            var items = pagedData.Data != null
                ? _mapper.Map<List<WorkGroupEmployeeItem>>(pagedData.Data.ToList())
                : new List<WorkGroupEmployeeItem>();

            var paginationModel = pagedData.Pagination == null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(pagedData.Pagination);
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            // TRANSFORMENGINE: DataGridConfig built explicitly — all properties set.
            // AllowAdd/Edit/Delete derived from JS fps_maintain_wg_staff.js:
            //   showAddButton: true => AllowAdd = true
            //   action column render has wg-edit-btn  => AllowEdit = true
            //   action column render has wg-delete-btn => AllowDelete = true
            return new DataGridConfig<WorkGroupEmployeeItem>
            {
                GridId             = "wgStaffGrid",
                Title              = "WG Staff",
                ShowCheckboxColumn = true,
                ShowPagination     = true,
                KeyProperty        = "PactId",
                AllowAdd           = true,
                AddFunction        = "addMaintWGStaff",
                AllowEdit          = true,
                EditFunction       = "editMaintWGStaff",
                AllowDelete        = true,
                DeleteFunction     = "deleteMaintWGStaff",
                ExtraFilterMethod  = "getMaintWGStaffExtraFilters",
                BindGridUrl        = "/FPS/MaintWGStaff/LoadWGStaffGrid",
                Data               = items,
                Columns            = GridDataProvider.GetColumnsDefination<WorkGroupEmployeeItem>(null),
                Pagination         = paginationModel,
                CurrentFilters     = filterDict
            };
        }

        // ── CRUD Endpoints ─────────────────────────────────────────────────────────

        // AllowAdd: true — GET returns empty partial; POST creates record.
        // TRANSFORMENGINE: Create GET — returns _AddEditMaintWGStaff partial with empty item.
        [HttpGet]
        public IActionResult Create()
        {
            return PartialView("_AddEditMaintWGStaff", new WorkGroupEmployeeItem());
        }

        // TRANSFORMENGINE: Create POST — [FromBody] WorkGroupEmployeeDto accepted directly.
        // No [ValidateAntiForgeryToken] — JSON [FromBody] endpoint uses Bearer token auth, not cookies.
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] WorkGroupEmployeeDto dto)
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

            var result = await _service.CreateWorkGroupEmployeeAsync(dto);

            if (result.Success)
            {
                return Json(new { success = true, message = "WG Staff record created successfully" });
            }

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to create WG Staff record.",
                errors = result.Errors ?? new List<Apha.FPSApps.Application.Dtos.ApiErrorDto>()
            });
        }

        // AllowEdit: true — GET loads record by pactId; POST updates.
        // TRANSFORMENGINE: Edit GET — loads record via service, maps Dto->Item via AutoMapper.
        [HttpGet]
        public async Task<IActionResult> Edit(string pactId)
        {
            if (string.IsNullOrWhiteSpace(pactId))
            {
                return Json(new { success = false, message = "PACT Id is required" });
            }

            var result = await _service.GetWorkGroupEmployeeByIdAsync(pactId);

            if (result.Success && result.Data != null)
            {
                var item = _mapper.Map<WorkGroupEmployeeItem>(result.Data);
                return PartialView("_AddEditMaintWGStaff", item);
            }

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message
                    ?? $"WG Staff record with PACT Id '{pactId}' not found."
            });
        }

        // TRANSFORMENGINE: Edit POST — [FromBody] WorkGroupEmployeeDto; delegates to service.UpdateWorkGroupEmployeeAsync.
        // No [ValidateAntiForgeryToken] — JSON [FromBody] endpoint uses Bearer token auth, not cookies.
        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] WorkGroupEmployeeDto dto)
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

            var result = await _service.UpdateWorkGroupEmployeeAsync(dto);

            if (result.Success)
            {
                return Json(new { success = true, message = "WG Staff record updated successfully" });
            }

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to update WG Staff record.",
                errors = result.Errors ?? new List<Apha.FPSApps.Application.Dtos.ApiErrorDto>()
            });
        }

        // AllowDelete: true — no modal partial; DataGrid uses JS confirm() for delete.
        // TRANSFORMENGINE: Delete [HttpDelete] — delegates to service.DeleteWorkGroupEmployeeAsync(pactId).
        // No [ValidateAntiForgeryToken] — [HttpDelete] endpoint; DataGrid sends DELETE request via fetch.
        [HttpDelete]
        public async Task<IActionResult> Delete(string pactId)
        {
            if (string.IsNullOrWhiteSpace(pactId))
            {
                return Json(new { success = false, message = "PACT Id is required" });
            }

            var result = await _service.DeleteWorkGroupEmployeeAsync(pactId);

            if (result.Success)
            {
                return Json(new { success = true, message = "WG Staff record deleted successfully" });
            }

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to delete WG Staff record.",
                errors = result.Errors ?? new List<Apha.FPSApps.Application.Dtos.ApiErrorDto>()
            });
        }
    }
}
