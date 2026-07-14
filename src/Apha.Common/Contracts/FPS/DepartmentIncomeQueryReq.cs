/*
 * TRANSFORMENGINE MIGRATION — DepartmentIncomeQueryReq.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - New request contract created from MS Access frmDepartmentIncome form parameters
 *   - QueryType enum replaces VBA tab/option-group branching (fnDeptIncomeQueryType)
 *   - Project maps to fnDeptIncomeProject() VBA function parameter (nullable, wildcard "*" when null)
 *   - MonthFrom maps to fnDeptIncomeMonthFrom() — defaults to 1 if null in service layer
 *   - MonthTo maps to fnDeptIncomeMonthTo() — defaults to 12 (or MonthFrom) if null in service layer
 *
 * PRESERVED:
 *   - All four filter parameters from the legacy Access form surface
 *   - Nullable semantics preserved: null = "all" / default behaviour as per VBA nz() usage
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: MonthFrom/MonthTo default logic (1 and 12) must be enforced in DepartmentIncomeService, not here
 */

namespace Apha.Common.Contracts.FPS
{
    // TRANSFORMENGINE: Writable input fields only — no response or entity fields
    public class DepartmentIncomeQueryReq
    {
        // TRANSFORMENGINE: Replaces Access option group / tab control selecting which qryDeptIncome* query to run
        public DepartmentIncomeQueryType QueryType { get; set; }

        // TRANSFORMENGINE: Maps to fnDeptIncomeProject() — null means "all projects" (Like "*" in VBA nz())
        public string? Project { get; set; }

        // TRANSFORMENGINE: Maps to fnDeptIncomeMonthFrom() — null defaults to 1 in service layer
        public int? MonthFrom { get; set; }

        // TRANSFORMENGINE: Maps to fnDeptIncomeMonthTo() — null defaults to 12 (or MonthFrom) in service layer
        public int? MonthTo { get; set; }
    }
}
