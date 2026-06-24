/*
 * TRANSFORMENGINE MIGRATION — AccountCategoryMaintenanceRes.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New contract created from fps.tblkpaccountcategory table surface for maintenance grid
 *   - Covers GET /api/v1/Maintenance/account-categories list and PUT response
 *   - Includes fields shown in Tab 2 (Account Categories) data grid: short name, description, CSG7 group
 *   - FpsYear included to identify the partitioned row and support display context
 *
 * PRESERVED:
 *   - All column names preserved from PostgreSQL DDL (accshortname, accountdescription, csg7_group, fpsyear)
 *   - No EF entity or repository concerns in contract
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether ConstituentAccountCodes, AccountType, ProjectSpecific, RcSpecific columns are needed in the maintenance grid
 */

namespace Apha.Common.Contracts.Costbook
{
    // TRANSFORMENGINE: Res contract for GET /api/v1/Maintenance/account-categories — drives Tab 2 data grid
    public class AccountCategoryMaintenanceRes
    {
        // TRANSFORMENGINE: maps to fps.tblkpaccountcategory.accshortname (PK component varchar 50)
        /// <summary>Account short name. Part of the composite primary key.</summary>
        public string AccShortName { get; set; } = string.Empty;

        // TRANSFORMENGINE: maps to fps.tblkpaccountcategory.accountdescription (varchar 50)
        /// <summary>Human-readable description of the account category.</summary>
        public string? AccountDescription { get; set; }

        // TRANSFORMENGINE: maps to fps.tblkpaccountcategory.csg7_group (char 15) — the maintained field
        /// <summary>Assigned CSG7 group for this account category. Nullable — may be unassigned.</summary>
        public string? Csg7Group { get; set; }

        // TRANSFORMENGINE: maps to fps.tblkpaccountcategory.fpsyear (int, PK component) — identifies the FPS partition
        /// <summary>FPS financial year. Part of the composite primary key (partitioned table).</summary>
        public int FpsYear { get; set; }
    }
}
