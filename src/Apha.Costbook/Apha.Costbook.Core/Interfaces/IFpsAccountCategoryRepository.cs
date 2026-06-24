/*
 * TRANSFORMENGINE MIGRATION — IFpsAccountCategoryRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New repository interface for AccountCategory maintenance operations (Tab 2 of frmMaintainance)
 *   - GetAllForMaintenanceAsync — fetches all account categories for the maintenance grid
 *   - GetByAccShortNameAsync — single-record lookup by PK
 *   - ExistsAsync — duplicate check (mirrors JS saveTblAccCat duplicate guard)
 *   - AddAsync, UpdateAsync, DeleteAsync — full CRUD to support Add/Edit/Delete modal flows
 *   - UpdateCsg7GroupAsync — targeted update of CSG7 group assignment on an existing category
 *   - Source: fps[year].tblkpaccountcategory (AccShortName PK, AccountDescription, AccountType,
 *             ConstituentAccountCodes, Csg7Group, ProjectSpecific, RcSpecific, FpsYear)
 *
 * PRESERVED:
 *   - Async-only signatures per Core layer convention
 *   - No infrastructure-specific types (no DbContext, EF references)
 *   - FpsYear context awareness retained via optional year parameter where applicable
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: verify which FPS year drives the account category table at runtime (IFPSYearContext injection in repository impl)
 *   - TRANSFORMENGINE TODO: confirm whether AccountType and ConstituentAccountCodes are editable via the maintenance UI or read-only
 */

using Apha.Costbook.Core.Entities;

namespace Apha.Costbook.Core.Interfaces
{
    // TRANSFORMENGINE: New interface — CRUD for fps[year].tblkpaccountcategory; AccShortName is string PK
    public interface IFpsAccountCategoryRepository
    {
        /// <summary>Returns all account categories available for the maintenance grid (all years or current year).</summary>
        Task<List<FpsAccountCategory>> GetAllForMaintenanceAsync();

        /// <summary>Returns a single account category by primary key, or null if not found.</summary>
        Task<FpsAccountCategory?> GetByAccShortNameAsync(string accShortName);

        /// <summary>
        /// Returns true if an account category with the given AccShortName already exists.
        /// Used to enforce the uniqueness guard in the Add modal (mirrors JS duplicate check).
        /// </summary>
        Task<bool> ExistsAsync(string accShortName);

        /// <summary>Adds a new account category and returns the persisted entity.</summary>
        Task<FpsAccountCategory> AddAsync(FpsAccountCategory accountCategory);

        /// <summary>Updates an existing account category and returns the updated entity.</summary>
        Task<FpsAccountCategory> UpdateAsync(FpsAccountCategory accountCategory);

        /// <summary>
        /// Updates only the CSG7 group assignment on an existing account category.
        /// Maps to the saveTblAccCat update path (costbookmaintainance.js) where csg7Group is the key change.
        /// Returns true if the record was found and updated.
        /// </summary>
        Task<bool> UpdateCsg7GroupAsync(string accShortName, string? csg7Group);

        /// <summary>Deletes the account category identified by AccShortName. Returns true if deleted.</summary>
        Task<bool> DeleteAsync(string accShortName);
    }
}
