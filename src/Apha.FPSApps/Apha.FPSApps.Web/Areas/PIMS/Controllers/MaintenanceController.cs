/*
 * TRANSFORMENGINE MIGRATION — MaintenanceController.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 14 — Pre-Build Security Review Gate
 * Migrated : 2026-07-06
 *
 * CHANGED (Phase 14 — Security Review):
 *   - SaveRadTrackProg: replaced broken ViewBag.IsAddingNew pattern (ViewBag is request-scoped;
 *     never populated in POST context) with server-side existence check via GetRadTrackProgByIdAsync.
 *     Previously isNew was always false, meaning Create operations silently fell through to Update.
 *   - SaveProjectManager: same fix — replaced ViewBag.IsAddingNew with server-side existence
 *     check via GetProjectManagerByIdAsync.
 *   - Added inline TRANSFORMENGINE SECURITY annotations on all corrected paths.
 *
 * CHANGED (Phase 11 — ViewModels + MVC Controller, Steps 16-17):
 *   - New MVC controller for frmMaintainance (PIMS Admin Maintenance) — all 6 tabs
 *   - Injects only IMaintenanceService (frontend aggregate service); no API clients or repositories
 *   - Produces DataGridConfig built explicitly in Index() and each Load*Grid action
 *   - Grid operations derived from HTML prototype + admin.js:
 *       Reports:               AllowAdd=true,  AllowEdit=true,  AllowDelete=true
 *       ReportGroups:          AllowAdd=true,  AllowEdit=true,  AllowDelete=true
 *       RadTrackProgs:         AllowAdd=true,  AllowEdit=true,  AllowDelete=true
 *       ProjectManagers:       AllowAdd=true,  AllowEdit=true,  AllowDelete=true
 *       ProgramManagerLinks:   AllowAdd=true,  AllowEdit=false, AllowDelete=true
 *       ProfitCentreLinks:     AllowAdd=true,  AllowEdit=false, AllowDelete=true
 *       Settings:              AllowAdd=false, AllowEdit=true,  AllowDelete=false
 *       AccessUsers:           AllowAdd=true,  AllowEdit=true,  AllowDelete=true (admin.js)
 *       AccessUserLevels:      AllowAdd=true,  AllowEdit=true,  AllowDelete=true (admin.js)
 *       Frequencies:           AllowAdd=true,  AllowEdit=true,  AllowDelete=true
 *       ReviewItems:           AllowAdd=true,  AllowEdit=true,  AllowDelete=true
 *   - No page-level filter dropdowns — HTML has no <select> outside any grid container
 *
 * PRESERVED:
 *   - All CRUD endpoint signatures match IMaintenanceService exactly
 *   - Composite-PK delete routes (program+manager, profitcentre+manager, systemid+ntlogin+accesslevelid)
 *   - Setting is read/update only (no Create/Delete)
 *   - All ModelState.IsValid guards and [ValidateAntiForgeryToken] on every mutating POST
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: PimsMaintenanceViewModelMapper.cs CreateMap entries needed for
 *     all Item types (see mapper file header for full list)
 *   - TRANSFORMENGINE TODO: Verify role requirements — "PIMSAdmin,PIMSUser" used here;
 *     confirm if Maintenance tab requires a separate "PIMSMaintenance" role
 *   - TRANSFORMENGINE TODO: AccessUserLevel grid uses Ntlogin as display "user" and
 *     Accesslevelid as "access level" — confirm this matches the frontend JS admin.js rendering
 *   - TRANSFORMENGINE TODO: [HttpDelete] endpoints (DeleteReport, DeleteReportGroup,
 *     DeleteRadTrackProg, DeleteProjectManager, DeleteProgramManagerLink,
 *     DeleteProfitCentreManagerLink, DeleteAccessUser, DeleteAccessUserLevel,
 *     DeleteFrequency, DeleteReviewItem) lack [ValidateAntiForgeryToken]. Index.cshtml
 *     JavaScript DELETE AJAX calls do not send RequestVerificationToken header. Fix requires
 *     coordinated change to both this controller and Index.cshtml.
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
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.Areas.PIMS.Controllers
{
    [Area("PIMS")]
    [Authorize(Roles = "PIMSAdmin,PIMSUser")]
    [AuthorizeForScopes(ScopeKeySection = "PIMSApiSettings:Scope")]
    public class MaintenanceController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IMaintenanceService _service;

        // TRANSFORMENGINE: Only IMaintenanceService injected — no API clients or repositories directly
        public MaintenanceController(IMapper mapper, IMaintenanceService service)
        {
            _mapper = mapper;
            _service = service;
        }

        // ── Index ─────────────────────────────────────────────────────────────────────

        public async Task<IActionResult> Index()
        {
            var viewModel = new MaintenanceViewModel();

            // TRANSFORMENGINE: Build all grid configs explicitly — never leave as new()
            var defaultRequest = new PaginationFilter<string> { Filter = "{}" };

            viewModel.ReportsGrid = await BuildReportsGridAsync(defaultRequest);
            viewModel.ReportGroupsGrid = await BuildReportGroupsGridAsync(defaultRequest);
            viewModel.RadTrackProgsGrid = await BuildRadTrackProgsGridAsync(defaultRequest);
            viewModel.ProjectManagersGrid = await BuildProjectManagersGridAsync(defaultRequest);
            viewModel.ProgramManagerLinksGrid = await BuildProgramManagerLinksGridAsync(defaultRequest, null);
            viewModel.ProfitCentreManagerLinksGrid = await BuildProfitCentreManagerLinksGridAsync(defaultRequest, null);
            viewModel.AccessUsersGrid = await BuildAccessUsersGridAsync(defaultRequest);
            viewModel.AccessUserLevelsGrid = await BuildAccessUserLevelsGridAsync(defaultRequest);
            viewModel.FrequenciesGrid = await BuildFrequenciesGridAsync(defaultRequest);
            viewModel.ReviewItemsGrid = await BuildReviewItemsGridAsync(defaultRequest);

            // TRANSFORMENGINE: Time Tab — load working hours and days settings by key
            await PopulateTimeTabSettingsAsync(viewModel);

            return View(viewModel);
        }

        // TRANSFORMENGINE: Populate Time Tab settings from known setting keys
        private async Task PopulateTimeTabSettingsAsync(MaintenanceViewModel viewModel)
        {
            var allSettingsResult = await _service.GetAllUserUpdateableSettingsAsync();
            if (allSettingsResult.Success && allSettingsResult.Data != null)
            {
                var workingHours = allSettingsResult.Data.FirstOrDefault(s =>
                    s.Id != null && s.Id.Equals("WorkingHours", StringComparison.OrdinalIgnoreCase));
                if (workingHours != null)
                    viewModel.WorkingHoursSettingItem = _mapper.Map<SettingItem>(workingHours);

                var workingDays = allSettingsResult.Data.FirstOrDefault(s =>
                    s.Id != null && s.Id.Equals("WorkingDays", StringComparison.OrdinalIgnoreCase));
                if (workingDays != null)
                    viewModel.WorkingDaysSettingItem = _mapper.Map<SettingItem>(workingDays);
            }
        }

        // ════════════════════════════════════════════════════════════════════════════
        //  REPORTS TAB
        // ════════════════════════════════════════════════════════════════════════════

        // ── Reports Grid ─────────────────────────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> LoadReportsGrid(PaginationFilter<string> request)
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

            var gridConfig = await BuildReportsGridAsync(request);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<ReportItem>> BuildReportsGridAsync(PaginationFilter<string> request)
        {
            // TRANSFORMENGINE: GetAllReportsAsync — no pagination on this endpoint; returns flat list
            var result = await _service.GetAllReportsAsync();

            var items = result.Success && result.Data != null
                ? _mapper.Map<List<ReportItem>>(result.Data)
                : new List<ReportItem>();

            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                request.Filter ?? "{}") ?? new Dictionary<string, string>();

            return new DataGridConfig<ReportItem>
            {
                GridId = "reportsGrid",
                Title = "Reports",
                ShowCheckboxColumn = true,
                ShowPagination = true,
                KeyProperty = "Id",
                AllowAdd = true,       // btnAddReport in HTML
                AddFunction = "addReport",
                AllowEdit = true,      // edit button in actions column
                EditFunction = "editReport",
                AllowDelete = true,    // delete button in actions column
                DeleteFunction = "deleteReport",
                ExtraFilterMethod = "getReportsExtraFilters",
                BindGridUrl = "/PIMS/Maintenance/LoadReportsGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<ReportItem>(),
                Pagination = new PaginationModel(),
                CurrentFilters = filterDict
            };
        }

        // ── Reports CRUD ─────────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> GetAddEditReportPartial(int? id = null)
        {
            var model = new ReportItem();
            if (id.HasValue)
            {
                var result = await _service.GetReportByIdAsync(id.Value);
                if (result is { Success: true, Data: not null })
                    model = _mapper.Map<ReportItem>(result.Data);
            }
            ViewBag.IsAddingNew = !id.HasValue;
            return PartialView("_AddEditReport", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveReport(ReportItem item)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any())
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new { field = kvp.Key, message = e.ErrorMessage }))
                });
            }

            var dto = _mapper.Map<ReportDto>(item);
            ApiResponseDto<ReportDto> result = item.Id == 0
                ? await _service.CreateReportAsync(dto)
                : await _service.UpdateReportAsync(item.Id, dto);

            return result.Success
                ? Json(new { success = true, message = item.Id == 0 ? "Report created successfully." : "Report updated successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteReport(int id)
        {
            var result = await _service.DeleteReportAsync(id);
            return result.Success
                ? Json(new { success = true, message = "Report deleted successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

        // ── Report Groups Grid ────────────────────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> LoadReportGroupsGrid(PaginationFilter<string> request)
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

            var gridConfig = await BuildReportGroupsGridAsync(request);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<ReportGroupItem>> BuildReportGroupsGridAsync(PaginationFilter<string> request)
        {
            var result = await _service.GetAllReportGroupsAsync();

            var items = result.Success && result.Data != null
                ? _mapper.Map<List<ReportGroupItem>>(result.Data)
                : new List<ReportGroupItem>();

            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                request.Filter ?? "{}") ?? new Dictionary<string, string>();

            return new DataGridConfig<ReportGroupItem>
            {
                GridId = "reportGroupsGrid",
                Title = "Report Groups",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "Groupid",
                AllowAdd = true,        // btnAddReportGroup in HTML
                AddFunction = "addReportGroup",
                AllowEdit = true,
                EditFunction = "editReportGroup",
                AllowDelete = true,
                DeleteFunction = "deleteReportGroup",
                ExtraFilterMethod = "getReportGroupsExtraFilters",
                BindGridUrl = "/PIMS/Maintenance/LoadReportGroupsGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<ReportGroupItem>(),
                Pagination = new PaginationModel(),
                CurrentFilters = filterDict
            };
        }

        // ── Report Groups CRUD ────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> GetAddEditReportGroupPartial(int? groupid = null)
        {
            var model = new ReportGroupItem();
            if (groupid.HasValue)
            {
                var result = await _service.GetReportGroupByIdAsync(groupid.Value);
                if (result is { Success: true, Data: not null })
                    model = _mapper.Map<ReportGroupItem>(result.Data);
            }
            ViewBag.IsAddingNew = !groupid.HasValue;
            return PartialView("_AddEditReportGroup", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveReportGroup(ReportGroupItem item)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any())
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new { field = kvp.Key, message = e.ErrorMessage }))
                });
            }

            var dto = _mapper.Map<ReportGroupDto>(item);
            ApiResponseDto<ReportGroupDto> result = item.Groupid == 0
                ? await _service.CreateReportGroupAsync(dto)
                : await _service.UpdateReportGroupAsync(item.Groupid, dto);

            return result.Success
                ? Json(new { success = true, message = item.Groupid == 0 ? "Report group created successfully." : "Report group updated successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteReportGroup(int groupid)
        {
            var result = await _service.DeleteReportGroupAsync(groupid);
            return result.Success
                ? Json(new { success = true, message = "Report group deleted successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

        // ════════════════════════════════════════════════════════════════════════════
        //  PROGRAMME TAB
        // ════════════════════════════════════════════════════════════════════════════

        [HttpPost]
        public async Task<IActionResult> LoadRadTrackProgsGrid(PaginationFilter<string> request)
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

            var gridConfig = await BuildRadTrackProgsGridAsync(request);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<RadTrackProgItem>> BuildRadTrackProgsGridAsync(PaginationFilter<string> request)
        {
            var result = await _service.GetAllRadTrackProgsAsync();

            var items = result.Success && result.Data != null
                ? _mapper.Map<List<RadTrackProgItem>>(result.Data)
                : new List<RadTrackProgItem>();

            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                request.Filter ?? "{}") ?? new Dictionary<string, string>();

            return new DataGridConfig<RadTrackProgItem>
            {
                GridId = "radTrackProgsGrid",
                Title = "PIMS Programmes",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "Program",
                AllowAdd = true,       // btnAddProg in HTML
                AddFunction = "addRadTrackProg",
                AllowEdit = true,
                EditFunction = "editRadTrackProg",
                AllowDelete = true,
                DeleteFunction = "deleteRadTrackProg",
                ExtraFilterMethod = "getRadTrackProgsExtraFilters",
                BindGridUrl = "/PIMS/Maintenance/LoadRadTrackProgsGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<RadTrackProgItem>(),
                Pagination = new PaginationModel(),
                CurrentFilters = filterDict
            };
        }

        [HttpGet]
        public async Task<IActionResult> GetAddEditRadTrackProgPartial(string? program = null)
        {
            var model = new RadTrackProgItem();
            if (!string.IsNullOrWhiteSpace(program))
            {
                var result = await _service.GetRadTrackProgByIdAsync(program);
                if (result is { Success: true, Data: not null })
                    model = _mapper.Map<RadTrackProgItem>(result.Data);
            }
            ViewBag.IsAddingNew = string.IsNullOrWhiteSpace(program);
            return PartialView("_AddEditRadTrackProg", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveRadTrackProg(RadTrackProgItem item)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any())
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new { field = kvp.Key, message = e.ErrorMessage }))
                });
            }

            var dto = _mapper.Map<RadTrackProgDto>(item);
            // TRANSFORMENGINE SECURITY FIX: ViewBag.IsAddingNew is request-scoped and is only set
            // in the GET handler; it is never populated in this POST context, so it was always null
            // (evaluating to false) — meaning Create operations silently fell through to Update.
            // Fix: use a server-side existence check on the natural string PK to determine intent.
            var existsResult = await _service.GetRadTrackProgByIdAsync(item.Program!);
            var isNew = !existsResult.Success || existsResult.Data == null;
            ApiResponseDto<RadTrackProgDto> result = isNew
                ? await _service.CreateRadTrackProgAsync(dto)
                : await _service.UpdateRadTrackProgAsync(item.Program!, dto);

            return result.Success
                ? Json(new { success = true, message = isNew ? "Programme created successfully." : "Programme updated successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteRadTrackProg(string program)
        {
            var result = await _service.DeleteRadTrackProgAsync(program);
            return result.Success
                ? Json(new { success = true, message = "Programme deleted successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

        // ════════════════════════════════════════════════════════════════════════════
        //  MANAGER TAB
        // ════════════════════════════════════════════════════════════════════════════

        // ── Project Manager Grid ─────────────────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> LoadProjectManagersGrid(PaginationFilter<string> request)
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

            var gridConfig = await BuildProjectManagersGridAsync(request);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<ProjectManagerItem>> BuildProjectManagersGridAsync(PaginationFilter<string> request)
        {
            var result = await _service.GetAllProjectManagersAsync();

            var items = result.Success && result.Data != null
                ? _mapper.Map<List<ProjectManagerItem>>(result.Data)
                : new List<ProjectManagerItem>();

            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                request.Filter ?? "{}") ?? new Dictionary<string, string>();

            return new DataGridConfig<ProjectManagerItem>
            {
                GridId = "projectManagersGrid",
                Title = "Manager",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "Projectmanager",
                AllowAdd = true,       // btnAddManager in HTML
                AddFunction = "addProjectManager",
                AllowEdit = true,
                EditFunction = "editProjectManager",
                AllowDelete = true,
                DeleteFunction = "deleteProjectManager",
                ExtraFilterMethod = "getProjectManagersExtraFilters",
                BindGridUrl = "/PIMS/Maintenance/LoadProjectManagersGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<ProjectManagerItem>(),
                Pagination = new PaginationModel(),
                CurrentFilters = filterDict
            };
        }

        [HttpGet]
        public async Task<IActionResult> GetAddEditProjectManagerPartial(string? projectmanager = null)
        {
            var model = new ProjectManagerItem();
            if (!string.IsNullOrWhiteSpace(projectmanager))
            {
                var result = await _service.GetProjectManagerByIdAsync(projectmanager);
                if (result is { Success: true, Data: not null })
                    model = _mapper.Map<ProjectManagerItem>(result.Data);
            }
            ViewBag.IsAddingNew = string.IsNullOrWhiteSpace(projectmanager);
            return PartialView("_AddEditProjectManager", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveProjectManager(ProjectManagerItem item)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any())
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new { field = kvp.Key, message = e.ErrorMessage }))
                });
            }

            var dto = _mapper.Map<ProjectManagerDto>(item);
            // TRANSFORMENGINE SECURITY FIX: ViewBag.IsAddingNew is request-scoped and only set
            // in the GET handler; it is never populated in this POST context, causing isNew to
            // always evaluate to false — Create operations silently fell through to Update.
            // Fix: server-side existence check on the natural string PK determines Create vs Update.
            var existsResult = await _service.GetProjectManagerByIdAsync(item.Projectmanager!);
            var isNew = !existsResult.Success || existsResult.Data == null;
            ApiResponseDto<ProjectManagerDto> result = isNew
                ? await _service.CreateProjectManagerAsync(dto)
                : await _service.UpdateProjectManagerAsync(item.Projectmanager!, dto);

            return result.Success
                ? Json(new { success = true, message = isNew ? "Manager created successfully." : "Manager updated successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteProjectManager(string projectmanager)
        {
            var result = await _service.DeleteProjectManagerAsync(projectmanager);
            return result.Success
                ? Json(new { success = true, message = "Manager deleted successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

        // ── Program Manager Link Sub-Grid ────────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> LoadProgramManagerLinksGrid(
            PaginationFilter<string> request, string? manager = null)
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

            var gridConfig = await BuildProgramManagerLinksGridAsync(request, manager);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<ProgramManagerLinkItem>> BuildProgramManagerLinksGridAsync(
            PaginationFilter<string> request, string? manager)
        {
            // TRANSFORMENGINE: GetAllProgramManagerLinksAsync — no pagination; filtered client-side by manager context
            var result = await _service.GetAllProgramManagerLinksAsync();

            List<ProgramManagerLinkItem> items = new();
            if (result.Success && result.Data != null)
            {
                var filtered = string.IsNullOrWhiteSpace(manager)
                    ? result.Data
                    : result.Data.Where(x => x.Manager == manager).ToList();
                items = _mapper.Map<List<ProgramManagerLinkItem>>(filtered);
            }

            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                request.Filter ?? "{}") ?? new Dictionary<string, string>();

            return new DataGridConfig<ProgramManagerLinkItem>
            {
                GridId = "programManagerLinksGrid",
                Title = "Program",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "Program",
                AllowAdd = true,        // btnAddMgrProgram in HTML
                AddFunction = "addProgramManagerLink",
                AllowEdit = false,      // composite link — no update
                AllowDelete = true,
                DeleteFunction = "deleteProgramManagerLink",
                ExtraFilterMethod = "getProgramManagerLinksExtraFilters",
                BindGridUrl = "/PIMS/Maintenance/LoadProgramManagerLinksGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<ProgramManagerLinkItem>(),
                Pagination = new PaginationModel(),
                CurrentFilters = filterDict
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveProgramManagerLink([FromBody] ProgramManagerLinkDto dto)
        {
            if (dto is null)
                return Json(new { success = false, message = "Invalid data" });

            var result = await _service.CreateProgramManagerLinkAsync(dto);
            return result.Success
                ? Json(new { success = true, message = "Programme assignment added successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteProgramManagerLink(string program, string manager)
        {
            var result = await _service.DeleteProgramManagerLinkAsync(program, manager);
            return result.Success
                ? Json(new { success = true, message = "Programme assignment deleted successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

        // ── Profit Centre Manager Link Sub-Grid ──────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> LoadProfitCentreManagerLinksGrid(
            PaginationFilter<string> request, string? manager = null)
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

            var gridConfig = await BuildProfitCentreManagerLinksGridAsync(request, manager);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<ProfitCentreManagerLinkItem>> BuildProfitCentreManagerLinksGridAsync(
            PaginationFilter<string> request, string? manager)
        {
            var result = await _service.GetAllProfitCentreManagerLinksAsync();

            List<ProfitCentreManagerLinkItem> items = new();
            if (result.Success && result.Data != null)
            {
                var filtered = string.IsNullOrWhiteSpace(manager)
                    ? result.Data
                    : result.Data.Where(x => x.Manager == manager).ToList();
                items = _mapper.Map<List<ProfitCentreManagerLinkItem>>(filtered);
            }

            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                request.Filter ?? "{}") ?? new Dictionary<string, string>();

            return new DataGridConfig<ProfitCentreManagerLinkItem>
            {
                GridId = "profitCentreManagerLinksGrid",
                Title = "Resource Centre",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "Profitcentre",
                AllowAdd = true,        // btnAddMgrResource in HTML
                AddFunction = "addProfitCentreManagerLink",
                AllowEdit = false,      // composite link — no update
                AllowDelete = true,
                DeleteFunction = "deleteProfitCentreManagerLink",
                ExtraFilterMethod = "getProfitCentreManagerLinksExtraFilters",
                BindGridUrl = "/PIMS/Maintenance/LoadProfitCentreManagerLinksGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<ProfitCentreManagerLinkItem>(),
                Pagination = new PaginationModel(),
                CurrentFilters = filterDict
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveProfitCentreManagerLink([FromBody] ProfitCentreManagerLinkDto dto)
        {
            if (dto is null)
                return Json(new { success = false, message = "Invalid data" });

            var result = await _service.CreateProfitCentreManagerLinkAsync(dto);
            return result.Success
                ? Json(new { success = true, message = "Resource Centre assignment added successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteProfitCentreManagerLink(string profitcentre, string manager)
        {
            var result = await _service.DeleteProfitCentreManagerLinkAsync(profitcentre, manager);
            return result.Success
                ? Json(new { success = true, message = "Resource Centre assignment deleted successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

        // ════════════════════════════════════════════════════════════════════════════
        //  TIME TAB (Settings)
        // ════════════════════════════════════════════════════════════════════════════

        // TRANSFORMENGINE: Settings are read/update only — no Create/Delete endpoints

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSetting([FromBody] SettingDto dto)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.Id))
                return Json(new { success = false, message = "Invalid data" });

            var result = await _service.UpdateSettingAsync(dto.Id, dto);
            return result.Success
                ? Json(new { success = true, message = "Setting updated successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

        [HttpGet]
        public async Task<IActionResult> GetTimeTabSettings()
        {
            var result = await _service.GetAllUserUpdateableSettingsAsync();
            if (!result.Success || result.Data == null)
                return Json(new { success = false, message = "Failed to load settings." });

            var workingHours = result.Data.FirstOrDefault(s =>
                s.Id != null && s.Id.Equals("WorkingHours", StringComparison.OrdinalIgnoreCase));
            var workingDays = result.Data.FirstOrDefault(s =>
                s.Id != null && s.Id.Equals("WorkingDays", StringComparison.OrdinalIgnoreCase));

            return Json(new
            {
                success = true,
                workingHours = workingHours?.SettingValue,
                workingDays = workingDays?.SettingValue
            });
        }

        // ════════════════════════════════════════════════════════════════════════════
        //  ADMIN MAINTENANCE TAB
        // ════════════════════════════════════════════════════════════════════════════

        // ── Access Users Grid ────────────────────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> LoadAccessUsersGrid(PaginationFilter<string> request)
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

            var gridConfig = await BuildAccessUsersGridAsync(request);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<AccessUserItem>> BuildAccessUsersGridAsync(PaginationFilter<string> request)
        {
            // TRANSFORMENGINE: GetAllAccessUsersAsync — flat list; columns from admin.js DataGridComponent
            var result = await _service.GetAllAccessUsersAsync();

            var items = result.Success && result.Data != null
                ? _mapper.Map<List<AccessUserItem>>(result.Data)
                : new List<AccessUserItem>();

            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                request.Filter ?? "{}") ?? new Dictionary<string, string>();

            return new DataGridConfig<AccessUserItem>
            {
                // TRANSFORMENGINE: admin.js columns: ntlogin (170), username (240), actions (120)
                GridId = "adminUsersGrid",
                Title = "Users",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "Ntlogin",
                AllowAdd = true,       // btnAddUser in HTML (admin tab)
                AddFunction = "addAccessUser",
                AllowEdit = true,      // edit button in admin.js actions column
                EditFunction = "editAccessUser",
                AllowDelete = true,    // delete button in admin.js actions column
                DeleteFunction = "deleteAccessUser",
                ExtraFilterMethod = "getAccessUsersExtraFilters",
                BindGridUrl = "/PIMS/Maintenance/LoadAccessUsersGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<AccessUserItem>(),
                Pagination = new PaginationModel(),
                CurrentFilters = filterDict
            };
        }

        [HttpGet]
        public async Task<IActionResult> GetAddEditAccessUserPartial(int? systemid = null, string? ntlogin = null)
        {
            var model = new AccessUserItem();
            if (systemid.HasValue && !string.IsNullOrWhiteSpace(ntlogin))
            {
                var result = await _service.GetAccessUserByIdAsync(systemid.Value, ntlogin);
                if (result is { Success: true, Data: not null })
                    model = _mapper.Map<AccessUserItem>(result.Data);
            }
            ViewBag.IsAddingNew = !systemid.HasValue;
            return PartialView("_AddEditAccessUser", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAccessUser(AccessUserItem item)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any())
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new { field = kvp.Key, message = e.ErrorMessage }))
                });
            }

            var dto = _mapper.Map<AccessUserDto>(item);
            ApiResponseDto<AccessUserDto> result = item.Systemid == 0
                ? await _service.CreateAccessUserAsync(dto)
                : await _service.UpdateAccessUserAsync(item.Systemid, item.Ntlogin!, dto);

            return result.Success
                ? Json(new { success = true, message = item.Systemid == 0 ? "User added successfully." : "User updated successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAccessUser(int systemid, string ntlogin)
        {
            var result = await _service.DeleteAccessUserAsync(systemid, ntlogin);
            return result.Success
                ? Json(new { success = true, message = "User deleted successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

        // ── Access User Levels Grid ──────────────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> LoadAccessUserLevelsGrid(PaginationFilter<string> request)
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

            var gridConfig = await BuildAccessUserLevelsGridAsync(request);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<AccessUserLevelItem>> BuildAccessUserLevelsGridAsync(PaginationFilter<string> request)
        {
            // TRANSFORMENGINE: admin.js columns: user (220), accessLevel (180), actions (120)
            var result = await _service.GetAllAccessUserLevelsAsync();

            var items = result.Success && result.Data != null
                ? _mapper.Map<List<AccessUserLevelItem>>(result.Data)
                : new List<AccessUserLevelItem>();

            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                request.Filter ?? "{}") ?? new Dictionary<string, string>();

            return new DataGridConfig<AccessUserLevelItem>
            {
                GridId = "adminAccessGrid",
                Title = "User Access",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "Ntlogin",
                AllowAdd = true,       // btnAddAccess in HTML (admin tab)
                AddFunction = "addAccessUserLevel",
                AllowEdit = true,      // edit button in admin.js access actions column
                EditFunction = "editAccessUserLevel",
                AllowDelete = true,    // delete button in admin.js access actions column
                DeleteFunction = "deleteAccessUserLevel",
                ExtraFilterMethod = "getAccessUserLevelsExtraFilters",
                BindGridUrl = "/PIMS/Maintenance/LoadAccessUserLevelsGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<AccessUserLevelItem>(),
                Pagination = new PaginationModel(),
                CurrentFilters = filterDict
            };
        }

        [HttpGet]
        public async Task<IActionResult> GetAddEditAccessUserLevelPartial(
            int? systemid = null, string? ntlogin = null, int? accesslevelid = null)
        {
            var model = new AccessUserLevelItem();
            if (systemid.HasValue && !string.IsNullOrWhiteSpace(ntlogin) && accesslevelid.HasValue)
            {
                var result = await _service.GetAccessUserLevelByIdAsync(systemid.Value, ntlogin, accesslevelid.Value);
                if (result is { Success: true, Data: not null })
                    model = _mapper.Map<AccessUserLevelItem>(result.Data);
            }
            ViewBag.IsAddingNew = !systemid.HasValue;
            return PartialView("_AddEditAccessUserLevel", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAccessUserLevel(AccessUserLevelItem item)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any())
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new { field = kvp.Key, message = e.ErrorMessage }))
                });
            }

            var dto = _mapper.Map<AccessUserLevelDto>(item);
            var result = await _service.CreateAccessUserLevelAsync(dto);

            return result.Success
                ? Json(new { success = true, message = "User access added successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAccessUserLevel(int systemid, string ntlogin, int accesslevelid)
        {
            var result = await _service.DeleteAccessUserLevelAsync(systemid, ntlogin, accesslevelid);
            return result.Success
                ? Json(new { success = true, message = "User access deleted successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

        // ════════════════════════════════════════════════════════════════════════════
        //  OTHER TAB — Frequencies
        // ════════════════════════════════════════════════════════════════════════════

        [HttpPost]
        public async Task<IActionResult> LoadFrequenciesGrid(PaginationFilter<string> request)
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

            var gridConfig = await BuildFrequenciesGridAsync(request);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<FrequencyItem>> BuildFrequenciesGridAsync(PaginationFilter<string> request)
        {
            var result = await _service.GetAllFrequenciesAsync();

            var items = result.Success && result.Data != null
                ? _mapper.Map<List<FrequencyItem>>(result.Data)
                : new List<FrequencyItem>();

            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                request.Filter ?? "{}") ?? new Dictionary<string, string>();

            return new DataGridConfig<FrequencyItem>
            {
                GridId = "frequenciesGrid",
                Title = "Frequency",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "Frequencyid",
                AllowAdd = true,
                AddFunction = "addFrequency",
                AllowEdit = true,
                EditFunction = "editFrequency",
                AllowDelete = true,
                DeleteFunction = "deleteFrequency",
                ExtraFilterMethod = "getFrequenciesExtraFilters",
                BindGridUrl = "/PIMS/Maintenance/LoadFrequenciesGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<FrequencyItem>(),
                Pagination = new PaginationModel(),
                CurrentFilters = filterDict
            };
        }

        [HttpGet]
        public async Task<IActionResult> GetAddEditFrequencyPartial(int? frequencyid = null)
        {
            var model = new FrequencyItem();
            if (frequencyid.HasValue)
            {
                var result = await _service.GetFrequencyByIdAsync(frequencyid.Value);
                if (result is { Success: true, Data: not null })
                    model = _mapper.Map<FrequencyItem>(result.Data);
            }
            ViewBag.IsAddingNew = !frequencyid.HasValue;
            return PartialView("_AddEditFrequency", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveFrequency(FrequencyItem item)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any())
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new { field = kvp.Key, message = e.ErrorMessage }))
                });
            }

            var dto = _mapper.Map<FrequencyDto>(item);
            ApiResponseDto<FrequencyDto> result = item.Frequencyid == 0
                ? await _service.CreateFrequencyAsync(dto)
                : await _service.UpdateFrequencyAsync(item.Frequencyid, dto);

            return result.Success
                ? Json(new { success = true, message = item.Frequencyid == 0 ? "Frequency created successfully." : "Frequency updated successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteFrequency(int frequencyid)
        {
            var result = await _service.DeleteFrequencyAsync(frequencyid);
            return result.Success
                ? Json(new { success = true, message = "Frequency deleted successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

        // ════════════════════════════════════════════════════════════════════════════
        //  OTHER TAB — Review Items
        // ════════════════════════════════════════════════════════════════════════════

        [HttpPost]
        public async Task<IActionResult> LoadReviewItemsGrid(PaginationFilter<string> request)
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

            var gridConfig = await BuildReviewItemsGridAsync(request);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<ReviewItemItem>> BuildReviewItemsGridAsync(PaginationFilter<string> request)
        {
            var result = await _service.GetAllReviewItemsAsync();

            var items = result.Success && result.Data != null
                ? _mapper.Map<List<ReviewItemItem>>(result.Data)
                : new List<ReviewItemItem>();

            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                request.Filter ?? "{}") ?? new Dictionary<string, string>();

            return new DataGridConfig<ReviewItemItem>
            {
                GridId = "reviewItemsGrid",
                Title = "Review Items",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "Itemid",
                AllowAdd = true,
                AddFunction = "addReviewItem",
                AllowEdit = true,
                EditFunction = "editReviewItem",
                AllowDelete = true,
                DeleteFunction = "deleteReviewItem",
                ExtraFilterMethod = "getReviewItemsExtraFilters",
                BindGridUrl = "/PIMS/Maintenance/LoadReviewItemsGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<ReviewItemItem>(),
                Pagination = new PaginationModel(),
                CurrentFilters = filterDict
            };
        }

        [HttpGet]
        public async Task<IActionResult> GetAddEditReviewItemPartial(int? itemid = null)
        {
            var model = new ReviewItemItem();
            if (itemid.HasValue)
            {
                var result = await _service.GetReviewItemByIdAsync(itemid.Value);
                if (result is { Success: true, Data: not null })
                    model = _mapper.Map<ReviewItemItem>(result.Data);
            }
            ViewBag.IsAddingNew = !itemid.HasValue;
            return PartialView("_AddEditReviewItem", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // TRANSFORMENGINE: Parameter renamed to reviewItem to avoid MVC1004 conflict with ReviewItemItem.Item property
        public async Task<IActionResult> SaveReviewItem(ReviewItemItem reviewItem)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any())
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new { field = kvp.Key, message = e.ErrorMessage }))
                });
            }

            var dto = _mapper.Map<ReviewItemDto>(reviewItem);
            ApiResponseDto<ReviewItemDto> result = reviewItem.Itemid == 0
                ? await _service.CreateReviewItemAsync(dto)
                : await _service.UpdateReviewItemAsync(reviewItem.Itemid, dto);

            return result.Success
                ? Json(new { success = true, message = reviewItem.Itemid == 0 ? "Review item created successfully." : "Review item updated successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteReviewItem(int itemid)
        {
            var result = await _service.DeleteReviewItemAsync(itemid);
            return result.Success
                ? Json(new { success = true, message = "Review item deleted successfully." })
                : Json(new { success = false, errors = result.Errors });
        }
    }
}
