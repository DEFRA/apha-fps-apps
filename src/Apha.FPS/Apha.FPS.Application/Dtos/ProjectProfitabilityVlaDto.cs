namespace Apha.FPS.Application.Dtos
{
    /// <summary>
    /// Service-layer DTO for a single row in the Project Profitability VLA list.
    /// Maps to <see cref="Apha.FPS.Core.Entities.ProjectProfitabilityVlaView"/> and is
    /// consumed by the backend <c>IProjectService.GetProjectProfitabilityVlaAsync()</c>.
    /// Property names are aligned with <c>ProjectProfitabilityVlaRes</c> to simplify
    /// the API-layer mapper.
    /// </summary>
    public class ProjectProfitabilityVlaDto
    {
        // TRANSFORMENGINE: optional row identifier from view ROW_NUMBER() — verify presence in DDL
        /// <summary>Optional numeric row identifier from the underlying view.</summary>
        public int? Id { get; set; }

        // TRANSFORMENGINE: maps ProjectProfitabilityVlaView.JobCode (tlkpProject.ParentProject)
        /// <summary>Job code (project code). The natural row key for the VLA profitability list.</summary>
        public string JobCode { get; set; } = null!;

        // TRANSFORMENGINE: maps ProjectProfitabilityVlaView.Program — VLA filter dimension filterProgram
        /// <summary>Program number. Used to populate the Program filter dropdown.</summary>
        public string? Program { get; set; }

        // TRANSFORMENGINE: maps ProjectProfitabilityVlaView.Customer — VLA-specific filter dimension absent from base DTO
        /// <summary>Customer name. VLA-specific field absent from base ProjectProfitabilityDto.</summary>
        public string? Customer { get; set; }

        // TRANSFORMENGINE: maps ProjectProfitabilityVlaView.Manager — VLA-specific filter dimension absent from base DTO
        /// <summary>Manager name. VLA-specific field absent from base ProjectProfitabilityDto.</summary>
        public string? Manager { get; set; }

        // TRANSFORMENGINE: maps ProjectProfitabilityVlaView.Status — filter: Approved, Completed, Not Approved
        /// <summary>Project status. Static filter options: "Approved", "Completed", "Not Approved".</summary>
        public string? Status { get; set; }

        // ── Financial columns ─────────────────────────────────────────────────

        // TRANSFORMENGINE: maps StaffCosts — was JcTotalStaffCosts in base ProjectProfitabilityDto
        /// <summary>Total staff costs for the job code.</summary>
        public decimal StaffCosts { get; set; }

        // TRANSFORMENGINE: maps TestCost — was JcTotalTestCosts in base ProjectProfitabilityDto
        /// <summary>Total test costs for the job code.</summary>
        public decimal TestCost { get; set; }

        // TRANSFORMENGINE: maps AnimalCosts — was JcTotalAnimalCosts in base ProjectProfitabilityDto
        /// <summary>Total animal costs for the job code.</summary>
        public decimal AnimalCosts { get; set; }

        // TRANSFORMENGINE: maps AdditionalCosts — was JcTotalAdditionalCosts in base ProjectProfitabilityDto
        /// <summary>Total additional costs for the job code.</summary>
        public decimal AdditionalCosts { get; set; }

        // TRANSFORMENGINE: maps TotalCosts = StaffCosts + TestCost + AnimalCosts + AdditionalCosts
        /// <summary>Sum of all cost categories.</summary>
        public decimal TotalCosts { get; set; }

        // TRANSFORMENGINE: maps Budget (Budget_CVL) — was BudgetCvl (nullable) in base ProjectProfitabilityDto
        /// <summary>Budget (CVL) for the project. Nullable if no budget has been set.</summary>
        public decimal? Budget { get; set; }

        // TRANSFORMENGINE: maps Profit = Budget - TotalCosts — was JcProfit in base ProjectProfitabilityDto
        /// <summary>Actual profit for the job code (Budget − TotalCosts).</summary>
        public decimal Profit { get; set; }

        // TRANSFORMENGINE: maps TargetProfit (tlkpProgram.Target) — same name as base ProjectProfitabilityDto.TargetProfit
        /// <summary>Target profit for the programme.</summary>
        public decimal TargetProfit { get; set; }

        // TRANSFORMENGINE: maps OffTarget = Profit - TargetProfit — same name as base ProjectProfitabilityDto.OffTarget
        /// <summary>
        /// Difference between actual profit and target profit.
        /// A negative value triggers the red highlight in projectprofitability_vla.js.
        /// </summary>
        public decimal OffTarget { get; set; }
    }
}
