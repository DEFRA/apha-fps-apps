/*
 * TRANSFORMENGINE MIGRATION — MaintenanceController.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 14 — Pre-Build Security Review Gate
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New [ApiController] created from MS Access frmMaintainance (Tabs 1, 2, and 4)
 *   - Tab 1 (Inflation Figures) + Tab 4 (Profit Margins) → GET/PUT /api/v1/maintenance/settings
 *   - Tab 2 (Account Categories) → GET /api/v1/maintenance/account-categories
 *                                   PUT /api/v1/maintenance/account-categories/{accShortName}
 *   - Depends on IMaintenanceSettingsService and IAccountCategoryMaintenanceService (Phase 3)
 *   - Uses AutoMapper to convert Dto <-> Req/Res (RequestMapper registrations in Phase 5)
 *   - Authorization: [Authorize(Roles = "API-CostbookAdmin,API-CostbookUser")] applied at controller level
 *   - Phase 14 security fix: added null/empty guard on accShortName route parameter in UpdateAccountCategory
 *     (defense-in-depth — consistent with DeleteCapsStaff and DeleteAccountGroup guards in sibling controllers)
 *
 * PRESERVED:
 *   - All service operation semantics preserved from service interfaces
 *   - Exception-driven flow (ArgumentException, KeyNotFoundException, InvalidOperationException)
 *     is handled by ExceptionMiddleware — no try/catch in controller
 *   - Route casing follows lowercase REST convention used by all Costbook controllers
 *   - req.Csg7Group may be null/empty to clear the CSG7 assignment — this is intentional per VBA behaviour
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether fpsYear parameter is needed on account-categories GET
 *     (currently server-side derived from CurrentFinancialYear setting)
 */

using Apha.Common.Contracts.Costbook;
using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Interfaces;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.Costbook.Api.Controllers
{
    // TRANSFORMENGINE: Combined controller for frmMaintainance Tabs 1, 2, 4
    //   Tab 1 (Inflation Figures)  → settings GET/PUT
    //   Tab 2 (Account Categories) → account-categories GET/PUT
    //   Tab 4 (Profit Margins)     → covered by same settings GET/PUT
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/maintenance")]
    [Authorize(Roles = "API-CostbookAdmin,API-CostbookUser")]
    public class MaintenanceController : ControllerBase
    {
        private readonly IMaintenanceSettingsService _settingsService;
        private readonly IAccountCategoryMaintenanceService _accountCategoryService;
        private readonly IMapper _mapper;

        public MaintenanceController(
            IMaintenanceSettingsService settingsService,
            IAccountCategoryMaintenanceService accountCategoryService,
            IMapper mapper)
        {
            _settingsService = settingsService;
            _accountCategoryService = accountCategoryService;
            _mapper = mapper;
        }

        // ── Maintenance Settings (Tab 1 Inflation + Tab 4 Profit Margins) ────────

        /// <summary>
        /// Returns all user-updatable maintenance settings (inflation rates, working hours/days,
        /// profit margins) stored in mabarchive.tbl_settings.
        /// </summary>
        /// <returns>200 OK with <see cref="MaintenanceSettingsRes"/> payload.</returns>
        [HttpGet("settings")]
        public async Task<IActionResult> GetSettings()
        {
            // TRANSFORMENGINE: Maps tbl_settings rows → MaintenanceSettingsDto → MaintenanceSettingsRes
            var dto = await _settingsService.GetSettingsAsync();
            return Ok(_mapper.Map<MaintenanceSettingsRes>(dto));
        }

        /// <summary>
        /// Applies a bulk update of all user-updatable maintenance settings (inflation rates,
        /// working hours/days, profit margins) to mabarchive.tbl_settings.
        /// </summary>
        /// <param name="req">All updatable settings values.</param>
        /// <returns>200 OK with updated <see cref="MaintenanceSettingsRes"/> payload.</returns>
        [HttpPut("settings")]
        [Authorize(Roles = "API-CostbookAdmin")]
        public async Task<IActionResult> UpdateSettings([FromBody] MaintenanceSettingsReq req)
        {
            // TRANSFORMENGINE: Maps MaintenanceSettingsReq → MaintenanceSettingsDto → bulk tbl_settings update
            var dto = _mapper.Map<MaintenanceSettingsDto>(req);
            await _settingsService.UpdateSettingsAsync(dto);
            // Re-fetch the updated settings to return the current persisted state
            var updated = await _settingsService.GetSettingsAsync();
            return Ok(_mapper.Map<MaintenanceSettingsRes>(updated));
        }

        // ── Account Categories (Tab 2) ────────────────────────────────────────────

        /// <summary>
        /// Returns all account categories for the maintenance grid (Tab 2 of frmMaintainance).
        /// Account categories originate from FPS; only the CSG7 group linkage is maintained here.
        /// </summary>
        /// <returns>200 OK with list of <see cref="AccountCategoryMaintenanceRes"/> entries.</returns>
        [HttpGet("account-categories")]
        public async Task<IActionResult> GetAccountCategories()
        {
            // TRANSFORMENGINE: Returns fps[year].tblkpaccountcategory rows for the current financial year
            var dtos = await _accountCategoryService.GetAllForMaintenanceAsync();
            return Ok(_mapper.Map<List<AccountCategoryMaintenanceRes>>(dtos));
        }

        /// <summary>
        /// Updates the CSG7 group assignment on an existing account category.
        /// Preserves the saveTblAccCat update flow from costbookmaintainance.js.
        /// </summary>
        /// <param name="accShortName">The account short name (route key).</param>
        /// <param name="req">The CSG7 group to assign.</param>
        /// <returns>200 OK with updated <see cref="AccountCategoryMaintenanceRes"/> payload.</returns>
        [HttpPut("account-categories/{accShortName}")]
        [Authorize(Roles = "API-CostbookAdmin")]
        public async Task<IActionResult> UpdateAccountCategory(string accShortName, [FromBody] AccountCategoryMaintenanceReq req)
        {
            // TRANSFORMENGINE: Targeted CSG7 group update on fps[year].tblkpaccountcategory
            // Route key accShortName takes precedence; req.Csg7Group may be null/empty to clear the assignment
            // TRANSFORMENGINE (Phase 14 security): guard against empty accShortName reaching the service layer
            if (string.IsNullOrWhiteSpace(accShortName))
                throw new ArgumentException("AccShortName route parameter is required.");

            var updated = await _accountCategoryService.UpdateCsg7GroupAsync(accShortName, req.Csg7Group);
            return Ok(_mapper.Map<AccountCategoryMaintenanceRes>(updated));
        }
    }
}
