// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — ProjectProfitabilityVlaRes.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-06-15
 *
 * CHANGED:
 *   - New file: no legacy C# equivalent existed for the VLA variant.
 *   - Source artefacts: HTML prototype frmJobcodeTotalsVLA.html,
 *     projectprofitability_vla.js (normalizeRow, grid columns, updateSummary).
 *   - Extended ProjectProfitabilityRes shape to include the VLA-specific fields:
 *     Project (display name), Program, Customer, Manager, Status — all present
 *     in the JS normalizeRow() but absent from the base ProjectProfitabilityRes.
 *   - Cost/profit fields renamed to camelCase-aligned PascalCase properties that
 *     match the JS field names in normalizeRow() to simplify frontend mapping:
 *       staffCosts  -> StaffCosts   (was JcTotalStaffCosts in base contract)
 *       testCost    -> TestCost     (was JcTotalTestCosts)
 *       animal      -> AnimalCosts  (was JcTotalAnimalCosts)
 *       addCosts    -> AdditionalCosts (was JcTotalAdditionalCosts)
 *       budget      -> Budget       (was BudgetCvl)
 *       profit      -> Profit       (was JcProfit)
 *   - TotalCount added to support server-side pagination metadata.
 *
 * PRESERVED:
 *   - All nine financial summary fields visible in HTML summary section and
 *     tracked in projectprofitability_vla.js updateSummary():
 *     StaffCosts, TestCost, AnimalCosts, AdditionalCosts, TotalCosts, Budget,
 *     Profit, TargetProfit, OffTarget.
 *   - Nullable budget field (Budget) kept nullable as BudgetCvl was nullable
 *     in the base ProjectProfitabilityRes.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm Project/Program/Customer/Manager field lengths
 *     against vprojectprofitability view column definitions (Phase 2 entity).
 *   - TRANSFORMENGINE TODO: confirm Budget nullability matches view column;
 *     if the view column is NOT NULL, remove the '?' from Budget.
 *   - TRANSFORMENGINE TODO: confirm Id type (int vs string) matches the view's
 *     primary key / row identifier; update if needed.
 */

namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// Response contract for a single row in the Project Profitability VLA list
    /// (<c>GET /api/v1/project/profitability-vla</c>).
    /// Includes the project identifier, filter-dimension fields (Program, Customer,
    /// Manager, Status), and all nine financial summary columns rendered in the
    /// HTML prototype summary bar.
    /// </summary>
    public class ProjectProfitabilityVlaRes
    {
        // TRANSFORMENGINE: row identifier — maps JS normalizeRow() id (Number)
        /// <summary>Row identifier from the underlying view.</summary>
        public int Id { get; set; }

        // TRANSFORMENGINE: maps JS normalizeRow() 'project' — grid column header "Project"
        /// <summary>Project code or short name displayed in the grid Project column.</summary>
        public string Project { get; set; } = null!;

        // TRANSFORMENGINE: maps JS normalizeRow() 'program' — grid column header "Program"; filter dimension
        /// <summary>Program number / name. Used to populate the Program filter dropdown.</summary>
        public string? Program { get; set; }

        // TRANSFORMENGINE: maps JS normalizeRow() 'customer' — grid column header "Customer"; filter dimension
        /// <summary>Customer name. Used to populate the Customer filter dropdown.</summary>
        public string? Customer { get; set; }

        // TRANSFORMENGINE: maps JS normalizeRow() 'manager' — VLA-specific field not in base ProjectProfitabilityRes; filter dimension
        /// <summary>Manager name. Used to populate the Manager filter dropdown.</summary>
        public string? Manager { get; set; }

        // TRANSFORMENGINE: maps JS normalizeRow() 'status' — filter dimension; static values: Approved, Completed, Not Approved
        /// <summary>Project status (e.g. "Approved", "Completed", "Not Approved").</summary>
        public string? Status { get; set; }

        // ── Financial columns ─────────────────────────────────────────────────

        // TRANSFORMENGINE: maps JS 'staffCosts' / HTML ppf-total-staff-costs; was JcTotalStaffCosts in base contract
        /// <summary>Total staff costs for the job code.</summary>
        public decimal StaffCosts { get; set; }

        // TRANSFORMENGINE: maps JS 'testCost' / HTML ppf-total-test-cost; was JcTotalTestCosts in base contract
        /// <summary>Total test costs for the job code.</summary>
        public decimal TestCost { get; set; }

        // TRANSFORMENGINE: maps JS 'animal' / HTML ppf-total-animal; was JcTotalAnimalCosts in base contract
        /// <summary>Total animal costs for the job code.</summary>
        public decimal AnimalCosts { get; set; }

        // TRANSFORMENGINE: maps JS 'addCosts' / HTML ppf-total-add-costs; was JcTotalAdditionalCosts in base contract
        /// <summary>Total additional costs for the job code.</summary>
        public decimal AdditionalCosts { get; set; }

        // TRANSFORMENGINE: maps JS 'totalCosts' / HTML ppf-total-total-costs
        /// <summary>Sum of all cost categories (StaffCosts + TestCost + AnimalCosts + AdditionalCosts).</summary>
        public decimal TotalCosts { get; set; }

        // TRANSFORMENGINE: maps JS 'budget' / HTML ppf-total-budget; nullable — was BudgetCvl in base contract
        /// <summary>Budget (CVL) for the project. Nullable if not set.</summary>
        public decimal? Budget { get; set; }

        // TRANSFORMENGINE: maps JS 'profit' / HTML ppf-total-profit; was JcProfit in base contract
        /// <summary>Actual profit for the job code.</summary>
        public decimal Profit { get; set; }

        // TRANSFORMENGINE: maps JS 'targetProfit' / HTML ppf-total-target-profit
        /// <summary>Target profit for the project.</summary>
        public decimal TargetProfit { get; set; }

        // TRANSFORMENGINE: maps JS 'offTarget' / HTML ppf-total-off-target; can be negative (highlighted in UI)
        /// <summary>Difference between actual profit and target profit. Negative value triggers red highlight in UI.</summary>
        public decimal OffTarget { get; set; }

        // ── Pagination metadata ───────────────────────────────────────────────

        // TRANSFORMENGINE: pagination support — total record count for frontend DataGrid pagination
        /// <summary>
        /// Total number of records matching the current filter, used by the frontend
        /// DataGrid for server-side pagination.  Populated on list responses only;
        /// defaults to 0 for single-record lookups.
        /// </summary>
        public int TotalCount { get; set; }
    }
}
