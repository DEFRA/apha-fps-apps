/*
 * TRANSFORMENGINE MIGRATION — AccountCategoryMaintenanceReq.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New contract created from MS Access frmMaintainance Tab 2 (Account Categories) modal (formTblAccCat)
 *   - Covers PUT /api/v1/Maintenance/account-categories/{accShortName}
 *   - Key accShortName is in the route; request body carries the updatable field: CSG7 group assignment
 *   - Source table: fps.tblkpaccountcategory (partitioned by fpsyear); csg7_group is the maintained field
 *   - AccountCategory records originate from FPS — only the CSG7 group linkage is maintained here
 *
 * PRESERVED:
 *   - Writable fields only (csg7Group assignment); accShortName key is in the route not the body
 *   - No EF entity or repository concerns in contract
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether fpsYear must be supplied in the request body or is derived server-side from CurrentFinancialYear setting
 */

namespace Apha.Common.Contracts.Costbook
{
    // TRANSFORMENGINE: Req contract for PUT /api/v1/Maintenance/account-categories/{accShortName}
    // Only the CSG7 group assignment is maintained via this endpoint; accShortName is in the route.
    public class AccountCategoryMaintenanceReq
    {
        // TRANSFORMENGINE: maps to modal-acccat-csg7group select — the CSG7 group assignment being updated on fps.tblkpaccountcategory.csg7_group
        /// <summary>CSG7 group name to assign to this account category.</summary>
        public string Csg7Group { get; set; } = string.Empty;
    }
}
