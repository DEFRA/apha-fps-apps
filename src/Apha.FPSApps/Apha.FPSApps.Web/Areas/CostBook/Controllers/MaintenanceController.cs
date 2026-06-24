/*
 * TRANSFORMENGINE MIGRATION — MaintenanceController.cs (Frontend MVC)
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-06-23
 * Security : Phase 14 — Pre-Build Security Review Gate (2026-06-23)
 *
 * CHANGED:
 *   - New frontend MVC controller created for frmMaintainance (all 5 tabs)
 *   - Injects ICostBookMaintenanceService (Tabs 1, 2, 4), ICostBookAccountGroupService (Tab 3 + dropdown),
 *     ICostBookCapsStaffService (Tab 5) — no API clients or repositories injected directly
 *   - Index() loads MaintenanceSettingsDto, builds 3 DataGridConfigs, populates Csg7GroupList dropdown
 *   - Tab 1/4 settings: SaveInflationSettings, SaveProfitMargins (POST → PUT to backend)
 *   - Tab 2 AccountCategory grid: LoadAccountCategoryGrid (list), EditAccountCategory (PUT only —
 *     backend has no ADD or DELETE for account categories; AllowAdd=false, AllowDelete=false)
 *   - Tab 3 Csg7Group grid: LoadCsg7GroupGrid (list), CreateCsg7Group, EditCsg7Group, DeleteCsg7Group
 *   - Tab 5 CapsStaff grid: LoadCapsStaffGrid (paginated), CreateCapsStaff, EditCapsStaff, DeleteCapsStaff
 *   - Phase 14 (Security): Added [ValidateAntiForgeryToken] to form-post grid-load endpoints
 *     (LoadAccountCategoryGrid, LoadCsg7GroupGrid, LoadCapsStaffGrid) — matches app-wide per-action pattern
 *   - Phase 14 (Security): Added ModelState.IsValid guard to all 5 mutating CRUD POST endpoints that
 *     were missing it (EditAccountCategory, CreateCsg7Group, EditCsg7Group, CreateCapsStaff, EditCapsStaff)
 *
 * PRESERVED:
 *   - AllowAdd/Edit/Delete flags derived from JS prototype callbacks and backend route availability:
 *     - AccountCategory: AllowAdd=false, AllowDelete=false, AllowEdit=true (backend PUT only)
 *     - Csg7Group: AllowAdd=true, AllowEdit=true, AllowDelete=true (full CRUD)
 *     - CapsStaff: AllowAdd=true, AllowEdit=true, AllowDelete=true (full CRUD)
 *   - Grid titles match JS DataGridComponent titles exactly
 *   - Class-level [Authorize(Roles = "CostbookAdmin,CostbookUser")] + [AuthorizeForScopes] preserved
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether a Year selector is needed for account-categories endpoint
 *   - TRANSFORMENGINE TODO: Confirm pagination size for CapsStaff grid (JS uses pageSize 5)
 *   - TRANSFORMENGINE TODO: Verify Csg7GroupList dropdown population covers all active CSG7 groups
 *   - TRANSFORMENGINE TODO SECURITY: JSON [FromBody] mutating endpoints (SaveInflationSettings,
 *     SaveProfitMargins, EditAccountCategory POST, CreateCsg7Group POST, EditCsg7Group POST,
 *     CreateCapsStaff POST, EditCapsStaff POST) use AJAX JSON pattern — antiforgery header
 *     (X-XSRF-TOKEN or RequestVerificationToken) should be included in AJAX fetch/axios defaults
 *     in the frontend JS. Verify _Layout.cshtml or the area JS bundle sets this up.
 *   - TRANSFORMENGINE TODO SECURITY: result.Errors forwarded to client in JSON responses —
 *     verify service layer returns only business-level messages, not raw DB/exception text.
 */

using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.Costbook;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.CostBook.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.Areas.CostBook.Controllers
{
    // TRANSFORMENGINE: [Area] + [Authorize] matches all other CostBook MVC controllers
    [Area("CostBook")]
    [Authorize(Roles = "CostbookAdmin,CostbookUser")]
    [AuthorizeForScopes(ScopeKeySection = "CostBookApiSettings:Scope")]
    public class MaintenanceController : Controller
    {
        private readonly IMapper _mapper;

        // TRANSFORMENGINE: CRUD service for Tabs 1, 2, 4 — Maintenance settings + Account Categories
        private readonly ICostBookMaintenanceService _maintenanceService;

        // TRANSFORMENGINE: CRUD + lookup service for Tab 3 (CSG7 groups); also drives AccCat modal dropdown
        private readonly ICostBookAccountGroupService _accountGroupService;

        // TRANSFORMENGINE: CRUD service for Tab 5 (CAPS Staff)
        private readonly ICostBookCapsStaffService _capsStaffService;

        public MaintenanceController(
            IMapper mapper,
            ICostBookMaintenanceService maintenanceService,
            ICostBookAccountGroupService accountGroupService,
            ICostBookCapsStaffService capsStaffService)
        {
            _mapper = mapper;
            _maintenanceService = maintenanceService;
            _accountGroupService = accountGroupService;
            _capsStaffService = capsStaffService;
        }

        // ── Index (GET) ───────────────────────────────────────────────────────

        // TRANSFORMENGINE: frmMaintainance OnLoad — load settings, initialise 3 grid configs,
        //   populate Csg7GroupList dropdown for AccountCategory modal
        public async Task<IActionResult> Index()
        {
            var viewModel = new MaintenanceViewModel();

            // TRANSFORMENGINE: Load Tabs 1 + 4 — MaintenanceSettingsDto → scalar ViewModel properties
            var settingsResult = await _maintenanceService.GetSettingsAsync();
            if (settingsResult.Success && settingsResult.Data != null)
            {
                var dto = settingsResult.Data;
                viewModel.InflationAnimals          = dto.InflationAnimals;
                viewModel.InflationExceptionalCosts = dto.InflationExceptionalCosts;
                viewModel.InflationStaff            = dto.InflationStaff;
                viewModel.InflationTests            = dto.InflationTests;
                viewModel.CurrentFinancialYear       = dto.CurrentFinancialYear;
                viewModel.WorkingHoursInDay          = dto.WorkingHoursInDay;
                viewModel.WorkingDaysInYear          = dto.WorkingDaysInYear;
                viewModel.ProfitAnimals              = dto.ProfitAnimals;
                viewModel.ProfitExceptionalCosts     = dto.ProfitExceptionalCosts;
                viewModel.ProfitStaff                = dto.ProfitStaff;
                viewModel.ProfitTests                = dto.ProfitTests;
            }

            // TRANSFORMENGINE: Populate Csg7GroupList — drives modal-acccat-csg7group <select>
            //   Uses ICostBookAccountGroupService (lookup flow, separate from CRUD service)
            await PopulateDropdownsAsync(viewModel);

            // TRANSFORMENGINE: Build Tab 2 DataGridConfig — AccountCategory (AllowAdd=false, AllowDelete=false)
            //   Backend only exposes GET + PUT /account-categories/{accShortName}
            viewModel.AccountCategoryGrid = new DataGridConfig<AccountCategoryItem>
            {
                GridId             = "accCatGrid",
                Title              = "Enter CSG7 Groups for Exceptional Cost Account Categories",
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "AccShortName",
                AllowAdd           = false,
                AllowEdit          = true,
                EditFunction       = "editAccountCategory",
                AllowDelete        = false,
                BindGridUrl        = "/CostBook/Maintenance/LoadAccountCategoryGrid",
                Data               = new List<AccountCategoryItem>(),
                Columns            = GridDataProvider.GetColumnsDefination<AccountCategoryItem>(),
                Pagination         = new PaginationModel()
            };

            // TRANSFORMENGINE: Build Tab 3 DataGridConfig — Csg7Group (full CRUD)
            //   JS showAddButton: true, onEdit: present, onDelete: present
            viewModel.Csg7GroupGrid = new DataGridConfig<Csg7GroupItem>
            {
                GridId             = "csg7Grid",
                Title              = "Set Inflation Option for CSG7 groups",
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "Csg7Group",
                AllowAdd           = true,
                AddFunction        = "addCsg7Group",
                AllowEdit          = true,
                EditFunction       = "editCsg7Group",
                AllowDelete        = true,
                DeleteFunction     = "deleteCsg7Group",
                BindGridUrl        = "/CostBook/Maintenance/LoadCsg7GroupGrid",
                Data               = new List<Csg7GroupItem>(),
                Columns            = GridDataProvider.GetColumnsDefination<Csg7GroupItem>(),
                Pagination         = new PaginationModel()
            };

            // TRANSFORMENGINE: Build Tab 5 DataGridConfig — CapsStaff (full CRUD)
            //   JS showAddButton: true, onEdit: present, onDelete: present
            viewModel.CapsStaffGrid = new DataGridConfig<CapsStaffItem>
            {
                GridId             = "capsStaffGrid",
                Title              = string.Empty,
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "MNumber",
                AllowAdd           = true,
                AddFunction        = "addCapsStaff",
                AllowEdit          = true,
                EditFunction       = "editCapsStaff",
                AllowDelete        = true,
                DeleteFunction     = "deleteCapsStaff",
                BindGridUrl        = "/CostBook/Maintenance/LoadCapsStaffGrid",
                Data               = new List<CapsStaffItem>(),
                Columns            = GridDataProvider.GetColumnsDefination<CapsStaffItem>(),
                Pagination         = new PaginationModel()
            };

            return View(viewModel);
        }

        // ── Tab 1: Inflation Settings Save ────────────────────────────────────

        // TRANSFORMENGINE: formInflation submit → PUT /api/v1/maintenance/settings
        //   Only inflation + system fields updated (profit fields preserved from current settings)
        [HttpPost]
        public async Task<IActionResult> SaveInflationSettings([FromBody] InflationSettingsItem item)
        {
            if (item is null)
                return Json(new { success = false, message = "Invalid data." });

            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Validation failed.",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            // TRANSFORMENGINE: Load current settings first to preserve profit values during inflation-only update
            var currentResult = await _maintenanceService.GetSettingsAsync();
            var dto = currentResult.Success && currentResult.Data != null
                ? currentResult.Data
                : new MaintenanceSettingsDto();

            dto.InflationAnimals          = item.InflationAnimals;
            dto.InflationExceptionalCosts = item.InflationExceptionalCosts;
            dto.InflationStaff            = item.InflationStaff;
            dto.InflationTests            = item.InflationTests;
            dto.CurrentFinancialYear       = item.CurrentFinancialYear;
            dto.WorkingHoursInDay          = item.WorkingHoursInDay;
            dto.WorkingDaysInYear          = item.WorkingDaysInYear;

            var result = await _maintenanceService.UpdateSettingsAsync(dto);
            return result.Success
                ? Json(new { success = true, message = "Inflation values saved successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

        // ── Tab 4: Profit Margins Save ────────────────────────────────────────

        // TRANSFORMENGINE: formProfitMargins submit → PUT /api/v1/maintenance/settings
        //   Only profit fields updated (inflation fields preserved from current settings)
        [HttpPost]
        public async Task<IActionResult> SaveProfitMargins([FromBody] ProfitMarginsItem item)
        {
            if (item is null)
                return Json(new { success = false, message = "Invalid data." });

            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Validation failed.",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            // TRANSFORMENGINE: Load current settings first to preserve inflation values during profit-only update
            var currentResult = await _maintenanceService.GetSettingsAsync();
            var dto = currentResult.Success && currentResult.Data != null
                ? currentResult.Data
                : new MaintenanceSettingsDto();

            dto.ProfitAnimals          = item.ProfitAnimals;
            dto.ProfitExceptionalCosts = item.ProfitExceptionalCosts;
            dto.ProfitStaff            = item.ProfitStaff;
            dto.ProfitTests            = item.ProfitTests;

            var result = await _maintenanceService.UpdateSettingsAsync(dto);
            return result.Success
                ? Json(new { success = true, message = "Profit margins saved successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

        // ── Tab 2: Account Category Grid ──────────────────────────────────────

        // TRANSFORMENGINE: Tab 2 grid AJAX reload — GET all account categories (no pagination on backend)
        // TRANSFORMENGINE: Phase 14 (Security) — [ValidateAntiForgeryToken] added; matches app-wide per-action CSRF pattern
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoadAccountCategoryGrid(PaginationFilter<string> request)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid request data.",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var gridConfig = await GetAccountCategoryGridConfigAsync(request);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<AccountCategoryItem>> GetAccountCategoryGridConfigAsync(
            PaginationFilter<string> request)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                request.Filter ?? "{}") ?? new Dictionary<string, string>();

            // TRANSFORMENGINE: Backend GET /api/v1/maintenance/account-categories — full list (no pagination)
            var result = await _maintenanceService.GetAccountCategoriesAsync();
            var items = result.Success && result.Data != null
                ? _mapper.Map<List<AccountCategoryItem>>(result.Data)
                : new List<AccountCategoryItem>();

            return new DataGridConfig<AccountCategoryItem>
            {
                GridId             = "accCatGrid",
                Title              = "Enter CSG7 Groups for Exceptional Cost Account Categories",
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "AccShortName",
                AllowAdd           = false,
                AllowEdit          = true,
                EditFunction       = "editAccountCategory",
                AllowDelete        = false,
                BindGridUrl        = "/CostBook/Maintenance/LoadAccountCategoryGrid",
                Data               = items,
                Columns            = GridDataProvider.GetColumnsDefination<AccountCategoryItem>(null),
                Pagination         = new PaginationModel(),
                CurrentFilters     = filterDict
            };
        }

        // TRANSFORMENGINE: Tab 2 — Edit AccountCategory GET (returns partial for modal)
        [HttpGet]
        public async Task<IActionResult> EditAccountCategory(string accShortName)
        {
            if (string.IsNullOrWhiteSpace(accShortName))
                return NotFound("Account Short Name is required.");

            var result = await _maintenanceService.GetAccountCategoriesAsync();
            if (!result.Success || result.Data == null)
                return NotFound($"Account category '{accShortName}' not found.");

            var dto = result.Data.FirstOrDefault(x =>
                string.Equals(x.AccShortName, accShortName, StringComparison.OrdinalIgnoreCase));
            if (dto == null)
                return NotFound($"Account category '{accShortName}' not found.");

            var item = _mapper.Map<AccountCategoryItem>(dto);
            return PartialView("_AddEditAccountCategory", item);
        }

        // TRANSFORMENGINE: Tab 2 — Edit AccountCategory POST → PUT /account-categories/{accShortName}
        //   Only Csg7Group is writable; AccShortName and AccountDescription are read-only
        [HttpPost]
        public async Task<IActionResult> EditAccountCategory(string accShortName, [FromBody] AccountCategoryItem item)
        {
            if (item is null || string.IsNullOrWhiteSpace(accShortName))
                return Json(new { success = false, message = "Invalid data." });

            // TRANSFORMENGINE: Phase 14 (Security) — ModelState.IsValid guard added; was missing on this mutating POST
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Validation failed.",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var dto = _mapper.Map<AccountCategoryMaintenanceDto>(item);
            var result = await _maintenanceService.UpdateAccountCategoryAsync(accShortName, dto);
            return result.Success
                ? Json(new { success = true, message = "Account category updated successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

        // ── Tab 3: CSG7 Group Grid ────────────────────────────────────────────

        // TRANSFORMENGINE: Tab 3 grid AJAX reload — GET all CSG7 groups (no pagination on backend)
        // TRANSFORMENGINE: Phase 14 (Security) — [ValidateAntiForgeryToken] added; matches app-wide per-action CSRF pattern
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoadCsg7GroupGrid(PaginationFilter<string> request)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid request data.",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var gridConfig = await GetCsg7GroupGridConfigAsync(request);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<Csg7GroupItem>> GetCsg7GroupGridConfigAsync(
            PaginationFilter<string> request)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                request.Filter ?? "{}") ?? new Dictionary<string, string>();

            // TRANSFORMENGINE: Backend GET /api/v1/accountgroup — full list (no pagination)
            var result = await _accountGroupService.GetAllAccountGroupsAsync();
            var items = result.Success && result.Data != null
                ? _mapper.Map<List<Csg7GroupItem>>(result.Data)
                : new List<Csg7GroupItem>();

            return new DataGridConfig<Csg7GroupItem>
            {
                GridId             = "csg7Grid",
                Title              = "Set Inflation Option for CSG7 groups",
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "Csg7Group",
                AllowAdd           = true,
                AddFunction        = "addCsg7Group",
                AllowEdit          = true,
                EditFunction       = "editCsg7Group",
                AllowDelete        = true,
                DeleteFunction     = "deleteCsg7Group",
                BindGridUrl        = "/CostBook/Maintenance/LoadCsg7GroupGrid",
                Data               = items,
                Columns            = GridDataProvider.GetColumnsDefination<Csg7GroupItem>(null),
                Pagination         = new PaginationModel(),
                CurrentFilters     = filterDict
            };
        }

        // TRANSFORMENGINE: Tab 3 — Create CSG7 Group GET (returns partial for modal)
        [HttpGet]
        public IActionResult CreateCsg7Group()
        {
            return PartialView("_AddEditCsg7Group", new Csg7GroupItem());
        }

        // TRANSFORMENGINE: Tab 3 — Create CSG7 Group POST → POST /api/v1/accountgroup
        [HttpPost]
        public async Task<IActionResult> CreateCsg7Group([FromBody] AccountGroupDto dto)
        {
            if (dto is null)
                return Json(new { success = false, message = "Invalid data." });

            // TRANSFORMENGINE: Phase 14 (Security) — ModelState.IsValid guard added; was missing on this mutating POST
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Validation failed.",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var result = await _accountGroupService.AddAccountGroupAsync(dto);
            return result.Success
                ? Json(new { success = true, message = "CSG7 group saved successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

        // TRANSFORMENGINE: Tab 3 — Edit CSG7 Group GET → GET /api/v1/accountgroup/{csg7Group}
        [HttpGet]
        public async Task<IActionResult> EditCsg7Group(string csg7Group)
        {
            if (string.IsNullOrWhiteSpace(csg7Group))
                return NotFound("CSG7 Group is required.");

            var result = await _accountGroupService.GetAccountGroupAsync(csg7Group);
            if (!result.Success || result.Data == null)
                return NotFound($"CSG7 group '{csg7Group}' not found.");

            var item = _mapper.Map<Csg7GroupItem>(result.Data);
            return PartialView("_AddEditCsg7Group", item);
        }

        // TRANSFORMENGINE: Tab 3 — Edit CSG7 Group POST → PUT /api/v1/accountgroup/{csg7Group}
        [HttpPost]
        public async Task<IActionResult> EditCsg7Group(string csg7Group, [FromBody] AccountGroupDto dto)
        {
            if (dto is null || string.IsNullOrWhiteSpace(csg7Group))
                return Json(new { success = false, message = "Invalid data." });

            // TRANSFORMENGINE: Phase 14 (Security) — ModelState.IsValid guard added; was missing on this mutating POST
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Validation failed.",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var result = await _accountGroupService.UpdateAccountGroupAsync(csg7Group, dto);
            return result.Success
                ? Json(new { success = true, message = "CSG7 group updated successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

        // TRANSFORMENGINE: Tab 3 — Delete CSG7 Group → DELETE /api/v1/accountgroup/{csg7Group}
        [HttpDelete]
        public async Task<IActionResult> DeleteCsg7Group(string csg7Group)
        {
            if (string.IsNullOrWhiteSpace(csg7Group))
                return Json(new { success = false, message = "CSG7 Group is required." });

            var result = await _accountGroupService.DeleteAccountGroupAsync(csg7Group);
            return result.Success
                ? Json(new { success = true, message = "CSG7 group deleted successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

        // ── Tab 5: CAPS Staff Grid ────────────────────────────────────────────

        // TRANSFORMENGINE: Tab 5 grid AJAX reload — GET paginated CapsStaff
        // TRANSFORMENGINE: Phase 14 (Security) — [ValidateAntiForgeryToken] added; matches app-wide per-action CSRF pattern
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoadCapsStaffGrid(PaginationFilter<string> request)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid request data.",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var gridConfig = await GetCapsStaffGridConfigAsync(request);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<CapsStaffItem>> GetCapsStaffGridConfigAsync(
            PaginationFilter<string> request)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                request.Filter ?? "{}") ?? new Dictionary<string, string>();

            // TRANSFORMENGINE: Backend GET /api/v1/capsstaff/paginated — paginated result
            var queryParameters = _mapper.Map<QueryParameters<string>>(request);
            var pagedData = await _capsStaffService.GetPaginatedCapsStaffAsync(queryParameters);

            var items = pagedData.Success && pagedData.Data != null
                ? _mapper.Map<List<CapsStaffItem>>(pagedData.Data)
                : new List<CapsStaffItem>();

            var paginationModel = pagedData.Pagination == null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(pagedData.Pagination);
            paginationModel.SortColumn    = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<CapsStaffItem>
            {
                GridId             = "capsStaffGrid",
                Title              = string.Empty,
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "MNumber",
                AllowAdd           = true,
                AddFunction        = "addCapsStaff",
                AllowEdit          = true,
                EditFunction       = "editCapsStaff",
                AllowDelete        = true,
                DeleteFunction     = "deleteCapsStaff",
                BindGridUrl        = "/CostBook/Maintenance/LoadCapsStaffGrid",
                Data               = items,
                Columns            = GridDataProvider.GetColumnsDefination<CapsStaffItem>(null),
                Pagination         = paginationModel,
                CurrentFilters     = filterDict
            };
        }

        // TRANSFORMENGINE: Tab 5 — Create CapsStaff GET (returns partial for modal)
        [HttpGet]
        public IActionResult CreateCapsStaff()
        {
            return PartialView("_AddEditCapsStaff", new CapsStaffItem());
        }

        // TRANSFORMENGINE: Tab 5 — Create CapsStaff POST → POST /api/v1/capsstaff
        [HttpPost]
        public async Task<IActionResult> CreateCapsStaff([FromBody] CapsStaffDto dto)
        {
            if (dto is null)
                return Json(new { success = false, message = "Invalid data." });

            // TRANSFORMENGINE: Phase 14 (Security) — ModelState.IsValid guard added; was missing on this mutating POST
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Validation failed.",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var result = await _capsStaffService.AddCapsStaffAsync(dto);
            return result.Success
                ? Json(new { success = true, message = "Staff member saved successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

        // TRANSFORMENGINE: Tab 5 — Edit CapsStaff GET → GET /api/v1/capsstaff/{mNumber}
        [HttpGet]
        public async Task<IActionResult> EditCapsStaff(string mNumber)
        {
            if (string.IsNullOrWhiteSpace(mNumber))
                return NotFound("mNumber is required.");

            var result = await _capsStaffService.GetCapsStaffByMNumberAsync(mNumber);
            if (!result.Success || result.Data == null)
                return NotFound($"Staff member '{mNumber}' not found.");

            var item = _mapper.Map<CapsStaffItem>(result.Data);
            return PartialView("_AddEditCapsStaff", item);
        }

        // TRANSFORMENGINE: Tab 5 — Edit CapsStaff POST → PUT /api/v1/capsstaff/{mNumber}
        [HttpPost]
        public async Task<IActionResult> EditCapsStaff(string mNumber, [FromBody] CapsStaffDto dto)
        {
            if (dto is null || string.IsNullOrWhiteSpace(mNumber))
                return Json(new { success = false, message = "Invalid data." });

            // TRANSFORMENGINE: Phase 14 (Security) — ModelState.IsValid guard added; was missing on this mutating POST
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Validation failed.",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var result = await _capsStaffService.UpdateCapsStaffAsync(mNumber, dto);
            return result.Success
                ? Json(new { success = true, message = "Staff member updated successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

        // TRANSFORMENGINE: Tab 5 — Delete CapsStaff → DELETE /api/v1/capsstaff/{mNumber}
        [HttpDelete]
        public async Task<IActionResult> DeleteCapsStaff(string mNumber)
        {
            if (string.IsNullOrWhiteSpace(mNumber))
                return Json(new { success = false, message = "mNumber is required." });

            var result = await _capsStaffService.DeleteCapsStaffAsync(mNumber);
            return result.Success
                ? Json(new { success = true, message = "Staff member deleted successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

        // ── Private helpers ───────────────────────────────────────────────────

        // TRANSFORMENGINE: PopulateDropdownsAsync — loads Csg7GroupList for AccountCategory modal
        //   modal-acccat-csg7group is an explicit <select> in tblAccCatModal (Tab 2)
        //   Source: ICostBookAccountGroupService (lookup flow, separate from Tab 2 CRUD MaintenanceService)
        private async Task PopulateDropdownsAsync(MaintenanceViewModel model)
        {
            var groupResult = await _accountGroupService.GetAllAccountGroupsAsync();
            if (groupResult.Success && groupResult.Data != null)
            {
                model.Csg7GroupList = groupResult.Data
                    .Select(item => new SelectListItem
                    {
                        Value = item.Csg7Group,
                        Text  = item.Csg7Group
                    })
                    .ToList();
            }
        }
    }
}
