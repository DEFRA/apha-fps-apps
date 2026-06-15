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
