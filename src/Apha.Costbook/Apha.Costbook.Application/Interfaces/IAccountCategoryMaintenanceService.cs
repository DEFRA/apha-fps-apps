/*
 * TRANSFORMENGINE MIGRATION — IAccountCategoryMaintenanceService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + Services
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New service interface created for AccountCategory maintenance operations (Tab 2 of frmMaintainance)
 *   - GetAllForMaintenanceAsync — returns all account categories for the maintenance grid
 *   - UpdateCsg7GroupAsync — targeted update of CSG7 group assignment (saveTblAccCat JS handler)
 *   - Maps to backend routes:
 *       GET  /api/v1/Maintenance/account-categories
 *       PUT  /api/v1/Maintenance/account-categories/{accShortName}
 *   - Source: fps[year].tblkpaccountcategory; AccShortName+FpsYear composite PK
 *
 * PRESERVED:
 *   - Async-only signatures per Application layer convention
 *   - No infrastructure-specific types (no DbContext, EF references)
 *   - UpdateCsg7GroupAsync preserves the targeted-field-update pattern from costbookmaintainance.js saveTblAccCat
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether FpsYear is derived server-side from CurrentFinancialYear setting or required as route param
 *   - TRANSFORMENGINE TODO: Confirm whether AccountType and ConstituentAccountCodes are editable via maintenance UI
 */

using Apha.Costbook.Application.Dtos;

namespace Apha.Costbook.Application.Interfaces
{
    // TRANSFORMENGINE: Service interface for fps[year].tblkpaccountcategory maintenance — covers Tab 2 (Account Categories)
    public interface IAccountCategoryMaintenanceService
    {
        /// <summary>
        /// Returns all account categories available for the maintenance grid.
        /// Maps to GET /api/v1/Maintenance/account-categories.
        /// </summary>
        Task<List<AccountCategoryMaintenanceDto>> GetAllForMaintenanceAsync();

        /// <summary>
        /// Updates the CSG7 group assignment on an existing account category.
        /// Maps to PUT /api/v1/Maintenance/account-categories/{accShortName}.
        /// Preserves the saveTblAccCat update flow from costbookmaintainance.js.
        /// Throws <see cref="ArgumentException"/> if accShortName is null/empty.
        /// Throws <see cref="KeyNotFoundException"/> if no record with the given AccShortName exists.
        /// </summary>
        Task<AccountCategoryMaintenanceDto> UpdateCsg7GroupAsync(string accShortName, string? csg7Group);
    }
}
