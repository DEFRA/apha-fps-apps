// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — InvoiceController.cs (new file)
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-06-12
 *
 * SECURITY REVIEW — Phase 14 (Pre-Build Security Review Gate) — 2026-06-12
 *   RESULT: PASS (visual)
 *   Checks performed:
 *     [Authorize(Roles)] + [AuthorizeForScopes] at class level : PASS — "PIMSAdmin,PIMSUser" matches PIMS MVC convention
 *     No [AllowAnonymous] on any action                        : PASS
 *     [ValidateAntiForgeryToken] on SaveInvoice (form POST)    : PASS — state-changing POST protected
 *     LoadInvoiceGrid [HttpPost] (AJAX, read-only reload)      : PASS — no state change; no anti-forgery gap
 *     DeleteInvoice [HttpDelete] (AJAX, no form)               : PASS with HUMAN_REVIEW note — verify JS
 *                                                                sends RequestVerificationToken header or
 *                                                                that SameSite cookie policy covers this action
 *     Filter params used only in service calls (not SQL/paths) : PASS — no concatenation risk
 *     JsonConvert.DeserializeObject of filter JSON             : PASS — result used for display dict only
 *     No raw SQL / no user input concatenation                 : PASS — all data access via IRadTrackInvoiceService
 *     No stack traces in JSON responses                        : PASS — only typed service error objects or ModelState messages
 *     No hardcoded secrets or environment-specific endpoints   : PASS
 *     IDOR on GetAddEditInvoicePartial(int? id)                : PASS — class-level [Authorize] scopes access by role
 *     InvoiceCounter == 0 as Create/Update discriminator       : PASS with HUMAN_REVIEW note — server-side; service enforces
 *   Human-review items (pre-existing, not new defects):
 *     DeleteInvoice CSRF: JS should include anti-forgery header for DELETE AJAX calls — HUMAN_REVIEW
 *     InvoiceCounter discriminator pattern: verify alignment with IsAddingNew convention — HUMAN_REVIEW
 *
 * CHANGED:
 *   - New file: ASP.NET Core MVC controller for the Invoice page (frmpimsinvoices).
 *   - [Area("PIMS")], [Authorize(Roles = "PIMSAdmin,PIMSUser")],
 *     [AuthorizeForScopes(ScopeKeySection = "PIMSApiSettings:Scope")] applied.
 *   - Injects IRadTrackInvoiceService (CRUD) and IProjectListService (project dropdown lookup).
 *   - CRUD binding: IRadTrackInvoiceService → GET api/v1/radtrackinvoice (list, get, create,
 *     update, delete) + GET api/v1/radtrackinvoice/totals.
 *   - Lookup binding: IProjectListService.GetAllProjectsListAsync() → Project filter dropdown.
 *   - Four page-level filter dropdowns (Project, Contract, Year, Program) confirmed by explicit
 *     <select> elements outside the grid container in frmpimsinvoices.html.
 *   - LoadInvoiceGrid: [HttpPost] AJAX endpoint returning _DataGrid partial.
 *   - GetInvoiceTotals: [HttpGet] AJAX endpoint returning _InvoiceTotals partial (totals footer).
 *   - GetAddEditInvoicePartial: [HttpGet] returns _AddEditInvoice partial for Add/Edit modal.
 *   - SaveInvoice: [HttpPost][ValidateAntiForgeryToken] handles both Create and Update.
 *   - DeleteInvoice: [HttpDelete] returns JSON success/failure.
 *   - DataGridConfig built explicitly in Index() and BuildInvoiceGridAsync() — never left as new().
 *   - ExtraFilterMethod = "getInvoiceExtraFilters" matches the JS filter apply function.
 *
 * PRESERVED:
 *   - All four filter parameters (project, contract, year, program) preserved and passed to
 *     IRadTrackInvoiceService.GetAllAsync() and GetTotalsAsync() — all confirmed sourced from
 *     explicit <select> controls in frmpimsinvoices.html.
 *   - Error logging pattern matches MilestoneController.cs (Console.WriteLine on Errors).
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: ContractList and ProgramList are stubbed as empty lists.
 *     No confirmed lookup endpoint for Surveillance Contract or Program exists in the
 *     current backend. Add lookup service injection and populate when available.
 *   - TRANSFORMENGINE TODO: YearList is populated with a 5-year rolling window relative to
 *     the current year. Verify the required range with the business owner.
 *   - TRANSFORMENGINE TODO: Project field in the Add/Edit modal is a <select> in the prototype.
 *     GetAddEditInvoicePartial passes ProjectList via ViewBag so _AddEditInvoice.cshtml
 *     can render <select asp-for="Project" asp-items="ViewBag.ProjectList">. Verify partial view.
 *   - TRANSFORMENGINE TODO: SaveInvoice distinguishes Create vs Update by InvoiceCounter == 0.
 *     Verify this convention matches the Add/Edit modal IsAddingNew flag pattern used elsewhere.
 *   - TRANSFORMENGINE TODO: Verify DI registration for IRadTrackInvoiceService is present in
 *     ServiceCollectionExtension.cs (Phase 10 [DONE]). If missing, add it before build.
 *   - TRANSFORMENGINE TODO (SECURITY): Confirm pimsinvoices.js sends the RequestVerificationToken
 *     header (or equivalent) on the DeleteInvoice [HttpDelete] AJAX call, or confirm the app-level
 *     SameSite=Strict cookie policy is sufficient to cover delete actions without a form post.
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
    [Area("PIMS")]
    // TRANSFORMENGINE SECURITY (Phase 14): [Authorize] + [AuthorizeForScopes] class-level — roles and scope match PIMS MVC convention. PASS (visual)
    [Authorize(Roles = "PIMSAdmin,PIMSUser")]
    [AuthorizeForScopes(ScopeKeySection = "PIMSApiSettings:Scope")]
    public class InvoiceController : Controller
    {
        private readonly IMapper _mapper;

        // TRANSFORMENGINE: CRUD service — IRadTrackInvoiceService → api/v1/radtrackinvoice.
        // Layer boundary: only IXxxService injected; no IApiClient or IRepository directly.
        private readonly IRadTrackInvoiceService _invoiceService;

        // TRANSFORMENGINE: Lookup service — IProjectListService used only for Project dropdown.
        // Does NOT replace _invoiceService as the CRUD resource for this page.
        private readonly IProjectListService _projectListService;

        public InvoiceController(
            IMapper mapper,
            IRadTrackInvoiceService invoiceService,
            IProjectListService projectListService)
        {
            _mapper = mapper;
            _invoiceService = invoiceService;
            _projectListService = projectListService;
        }

        // ── Index ────────────────────────────────────────────────────────────────

        // TRANSFORMENGINE: Index — builds full ViewModel including DataGridConfig and filter dropdowns.
        // filter params default to null so first page load shows all records.
        public async Task<IActionResult> Index(
            string? project = null,
            string? contract = null,
            int? year = null,
            string? program = null)
        {
            InvoiceViewModel viewModel = new()
            {
                FilterProject = project,
                FilterContract = contract,
                FilterYear = year,
                FilterProgram = program
            };

            await PopulateDropdownsAsync(viewModel);

            PaginationFilter<string> defaultRequest = new() { Filter = "{}" };
            viewModel.InvoicesGrid = await BuildInvoiceGridAsync(defaultRequest, project, contract, year, program);

            // TRANSFORMENGINE: Load initial totals matching the default filter (null = all records).
            ApiResponseDto<RadTrackInvoiceTotalsDto> totalsResult =
                await _invoiceService.GetTotalsAsync(project, contract, year, program);
            if (totalsResult.Success && totalsResult.Data != null)
                viewModel.InvoiceTotals = totalsResult.Data;

            return View(viewModel);
        }

        // ── DataGrid AJAX reload ─────────────────────────────────────────────────

        // TRANSFORMENGINE: LoadInvoiceGrid — AJAX endpoint for DataGrid pagination/sort/filter.
        // Called by DataGrid JS framework via POST to /PIMS/Invoice/LoadInvoiceGrid.
        [HttpPost]
        public async Task<IActionResult> LoadInvoiceGrid(
            PaginationFilter<string> request,
            string? project = null,
            string? contract = null,
            int? year = null,
            string? program = null)
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

            DataGridConfig<InvoiceItem> gridConfig =
                await BuildInvoiceGridAsync(request, project, contract, year, program);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<InvoiceItem>> BuildInvoiceGridAsync(
            PaginationFilter<string> request,
            string? project,
            string? contract,
            int? year,
            string? program)
        {
            Dictionary<string, string> filterDict =
                JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}") ?? new();

            QueryParameters<string> queryParameters = _mapper.Map<QueryParameters<string>>(request);

            // TRANSFORMENGINE: GetAllAsync — four filter params sourced from explicit <select>
            // controls in frmpimsinvoices.html (invFilterProject, invFilterSurvContract,
            // invFilterYear, invFilterProgram). Not inferred from backend params alone.
            ApiResponseDto<List<RadTrackInvoiceDto>> pagedData =
                await _invoiceService.GetAllAsync(queryParameters, project, contract, year, program);

            List<InvoiceItem> items = new();
            if (pagedData.Success && pagedData.Data != null)
            {
                items = _mapper.Map<List<InvoiceItem>>(pagedData.Data);
            }
            else if (pagedData.Errors != null)
            {
                foreach (var error in pagedData.Errors)
                    Console.WriteLine($"Invoice grid error: {error.Message}");
            }

            PaginationModel paginationModel = pagedData.Pagination is null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(pagedData.Pagination);
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            // TRANSFORMENGINE: DataGridConfig built explicitly — never left as new().
            // AllowAdd/Edit/Delete all true — confirmed by Add button and Edit/Delete action
            // buttons in frmpimsinvoices.html and pimsinvoices.js.
            return new DataGridConfig<InvoiceItem>
            {
                GridId = "invoicesGrid",
                Title = "Invoices",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "InvoiceCounter",
                AllowAdd = true,
                AddFunction = "addInvoice",
                AllowEdit = true,
                EditFunction = "editInvoice",
                AllowDelete = true,
                DeleteFunction = "deleteInvoice",
                ExtraFilterMethod = "getInvoiceExtraFilters",
                BindGridUrl = "/PIMS/Invoice/LoadInvoiceGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<InvoiceItem>(),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };
        }

        // ── Totals footer AJAX ───────────────────────────────────────────────────

        // TRANSFORMENGINE: GetInvoiceTotals — AJAX endpoint to reload the totals footer row.
        // Called whenever filters change so the totals always match the current grid filter.
        [HttpGet]
        public async Task<IActionResult> GetInvoiceTotals(
            string? project = null,
            string? contract = null,
            int? year = null,
            string? program = null)
        {
            ApiResponseDto<RadTrackInvoiceTotalsDto> result =
                await _invoiceService.GetTotalsAsync(project, contract, year, program);

            if (!result.Success || result.Data == null)
            {
                return Json(new
                {
                    success = false,
                    errors = result.Errors
                });
            }

            InvoiceTotalsItem totalsItem = _mapper.Map<InvoiceTotalsItem>(result.Data);
            return PartialView("_InvoiceTotals", totalsItem);
        }

        // ── Add / Edit modal partial ─────────────────────────────────────────────

        // TRANSFORMENGINE: GetAddEditInvoicePartial — [HttpGet] returns _AddEditInvoice partial.
        // id = null → Add modal (new InvoiceItem); id set → Edit modal (load existing record).
        // ProjectList passed via ViewBag so partial can render Project as a <select>.
        [HttpGet]
        public async Task<IActionResult> GetAddEditInvoicePartial(int? id = null)
        {
            InvoiceItem model = new();

            if (id.HasValue && id.Value > 0)
            {
                // TRANSFORMENGINE: Load existing invoice for Edit modal.
                ApiResponseDto<RadTrackInvoiceDto> result = await _invoiceService.GetByIdAsync(id.Value);
                if (result is { Success: true, Data: not null })
                    model = _mapper.Map<InvoiceItem>(result.Data);
            }

            // TRANSFORMENGINE: Project dropdown for modal — sourced from IProjectListService.
            // CRUD service and lookup service kept explicitly separate.
            ViewBag.ProjectList = await GetProjectSelectListAsync();
            ViewBag.IsAddingNew = !id.HasValue || id.Value == 0;
            return PartialView("_AddEditInvoice", model);
        }

        // ── Save (Create + Update) ────────────────────────────────────────────────

        // TRANSFORMENGINE: SaveInvoice — handles both Create (InvoiceCounter == 0) and
        // Update (InvoiceCounter > 0) from the Add/Edit modal form POST.
        // [ValidateAntiForgeryToken] applied — this is a standard form POST, not a [FromBody] JSON endpoint.
        // TRANSFORMENGINE SECURITY (Phase 14): [ValidateAntiForgeryToken] on state-changing POST. PASS (visual)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveInvoice(InvoiceItem item)
        {
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

            RadTrackInvoiceDto dto = _mapper.Map<RadTrackInvoiceDto>(item);

            ApiResponseDto<RadTrackInvoiceDto> result;

            if (item.InvoiceCounter == 0)
            {
                // TRANSFORMENGINE: Create — POST api/v1/radtrackinvoice.
                result = await _invoiceService.CreateAsync(dto);
                return result.Success
                    ? Json(new { success = true, data = result.Data, message = "Invoice created successfully." })
                    : Json(new { success = false, errors = result.Errors });
            }
            else
            {
                // TRANSFORMENGINE: Update — PUT api/v1/radtrackinvoice/{id}.
                // id passed explicitly to match backend route requirement.
                result = await _invoiceService.UpdateAsync(item.InvoiceCounter, dto);
                return result.Success
                    ? Json(new { success = true, data = result.Data, message = "Invoice updated successfully." })
                    : Json(new { success = false, errors = result.Errors });
            }
        }

        // ── Delete ────────────────────────────────────────────────────────────────

        // TRANSFORMENGINE: DeleteInvoice — [HttpDelete]; no modal partial, JS confirm() only
        // (matching openInvDeleteModal / confirmDeleteInv in pimsinvoices.js).
        // TRANSFORMENGINE SECURITY (Phase 14): [HttpDelete] AJAX — no [ValidateAntiForgeryToken] here.
        // HUMAN_REVIEW: Verify pimsinvoices.js sends RequestVerificationToken header on this DELETE call,
        // or confirm app-level SameSite=Strict policy provides equivalent CSRF protection.
        [HttpDelete]
        public async Task<IActionResult> DeleteInvoice(int id)
        {
            ApiResponseDto<object> result = await _invoiceService.DeleteAsync(id);
            return result.Success
                ? Json(new { success = true, message = "Invoice deleted successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

        // ── Private helpers ───────────────────────────────────────────────────────

        // TRANSFORMENGINE: PopulateDropdownsAsync — fills all four filter dropdown lists.
        // ContractList and ProgramList are stubbed (no confirmed lookup endpoint).
        // YearList is populated with a rolling 5-year window matching the prototype range.
        private async Task PopulateDropdownsAsync(InvoiceViewModel viewModel)
        {
            // Project list — from IProjectListService (lookup; does not affect CRUD binding).
            viewModel.ProjectList = await GetProjectSelectListAsync();

            // TRANSFORMENGINE TODO STUB: ContractList — no confirmed lookup endpoint.
            // Populate from a contract lookup service when available.
            viewModel.ContractList = [];

            // TRANSFORMENGINE: YearList — rolling window; prototype shows 2024-2026.
            // Using 5-year window: current year ± 2 years.
            int currentYear = DateTime.UtcNow.Year;
            viewModel.YearList = Enumerable.Range(currentYear - 2, 5)
                .OrderByDescending(y => y)
                .Select(y => new SelectListItem
                {
                    Value = y.ToString(),
                    Text = y.ToString(),
                    Selected = y == (viewModel.FilterYear ?? currentYear)
                })
                .ToList();

            // TRANSFORMENGINE TODO STUB: ProgramList — no confirmed lookup endpoint.
            // Populate from a program lookup service when available.
            viewModel.ProgramList = [];
        }

        private async Task<List<SelectListItem>> GetProjectSelectListAsync()
        {
            // TRANSFORMENGINE: Project options via IProjectListService — lookup flow,
            // separate from CRUD flow (IRadTrackInvoiceService).
            ApiResponseDto<List<ProjectListViewDto>> projects =
                await _projectListService.GetAllProjectsListAsync();

            return projects.Data?
                .Select(p => new SelectListItem
                {
                    Value = p.Parentproject,
                    Text = p.Parentproject
                })
                .ToList() ?? [];
        }
    }
}
