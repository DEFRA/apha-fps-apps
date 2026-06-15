// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — ProjectProfitabilityVlaDto.cs (Frontend)
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-06-15
 *
 * CHANGED:
 *   - New frontend DTO mirroring Apha.FPS.Application.Dtos.ProjectProfitabilityVlaDto
 *     (backend Phase 3 artefact) in the Apha.FPSApps.Application.Dtos.FPS namespace.
 *   - VLA-specific filter dimension fields (Program, Customer, Manager, Status) added —
 *     absent from base ProjectProfitabilityDto in both backend and frontend namespaces.
 *   - Financial column names differ from base DTO: StaffCosts (not JcTotalStaffCosts),
 *     TestCost (not JcTotalTestCosts), AnimalCosts, AdditionalCosts,
 *     Budget (not BudgetCvl), Profit (not JcProfit).
 *   - Id added as int? — optional row identifier mirroring backend DTO nullability.
 *
 * PRESERVED:
 *   - All property names exactly match Apha.FPS.Application.Dtos.ProjectProfitabilityVlaDto
 *     for zero-friction ApiDtoMapper binding (Res -> Dto -> frontend Dto chain).
 *   - JobCode as the natural project row key.
 *   - Budget nullable (decimal?) matching backend DTO nullability.
 *   - Nine financial columns: StaffCosts, TestCost, AnimalCosts, AdditionalCosts,
 *     TotalCosts, Budget, Profit, TargetProfit, OffTarget.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm Id nullability — remove '?' if the PostgreSQL view
 *     guarantees a non-null ROW_NUMBER() column (mirrors backend DTO open review item).
 *   - TRANSFORMENGINE TODO: confirm Budget nullability matches the final view column
 *     definition; remove '?' if the column is NOT NULL (mirrors backend DTO review item).
 */

namespace Apha.FPSApps.Application.Dtos.FPS
{
    /// <summary>
    /// Frontend DTO for a single row in the Project Profitability VLA list.
    /// Mirrors <c>Apha.FPS.Application.Dtos.ProjectProfitabilityVlaDto</c> (backend Phase 3).
    /// Consumed by <c>IFpsProjectApiClient.GetProjectProfitabilityVlaAsync()</c> and
    /// the FPS ProjectProfitabilityVla Razor view / PageModel.
    /// </summary>
    public class ProjectProfitabilityVlaDto
    {
        // TRANSFORMENGINE: optional row identifier — mirrors backend DTO int? Id; confirm nullability once view DDL is final
        /// <summary>Optional numeric row identifier from the underlying view.</summary>
        public int? Id { get; set; }

        // TRANSFORMENGINE: natural project row key — mirrors backend DTO JobCode (tlkpProject.ParentProject)
        /// <summary>Job code (project code). The natural row key for the VLA profitability list.</summary>
        public string JobCode { get; set; } = null!;

        // TRANSFORMENGINE: VLA filter dimension — mirrors backend DTO Program; absent from base ProjectProfitabilityDto
        /// <summary>Program number. Used to populate the Program filter dropdown.</summary>
        public string? Program { get; set; }

        // TRANSFORMENGINE: VLA filter dimension — mirrors backend DTO Customer; absent from base ProjectProfitabilityDto
        /// <summary>Customer name. Used to populate the Customer filter dropdown.</summary>
        public string? Customer { get; set; }

        // TRANSFORMENGINE: VLA filter dimension — mirrors backend DTO Manager; absent from base ProjectProfitabilityDto
        /// <summary>Manager name. Used to populate the Manager filter dropdown.</summary>
        public string? Manager { get; set; }

        // TRANSFORMENGINE: VLA filter dimension — mirrors backend DTO Status; static values: Approved, Completed, Not Approved
        /// <summary>Project status (e.g. "Approved", "Completed", "Not Approved").</summary>
        public string? Status { get; set; }

        // ── Financial columns ─────────────────────────────────────────────────

        // TRANSFORMENGINE: mirrors backend DTO StaffCosts (was JcTotalStaffCosts in base ProjectProfitabilityDto)
        /// <summary>Total staff costs for the job code.</summary>
        public decimal StaffCosts { get; set; }

        // TRANSFORMENGINE: mirrors backend DTO TestCost (was JcTotalTestCosts in base ProjectProfitabilityDto)
        /// <summary>Total test costs for the job code.</summary>
        public decimal TestCost { get; set; }

        // TRANSFORMENGINE: mirrors backend DTO AnimalCosts (was JcTotalAnimalCosts in base ProjectProfitabilityDto)
        /// <summary>Total animal costs for the job code.</summary>
        public decimal AnimalCosts { get; set; }

        // TRANSFORMENGINE: mirrors backend DTO AdditionalCosts (was JcTotalAdditionalCosts in base ProjectProfitabilityDto)
        /// <summary>Total additional costs for the job code.</summary>
        public decimal AdditionalCosts { get; set; }

        // TRANSFORMENGINE: mirrors backend DTO TotalCosts = StaffCosts + TestCost + AnimalCosts + AdditionalCosts
        /// <summary>Sum of all cost categories.</summary>
        public decimal TotalCosts { get; set; }

        // TRANSFORMENGINE: mirrors backend DTO Budget (nullable decimal?; was BudgetCvl in base ProjectProfitabilityDto)
        /// <summary>Budget (CVL) for the project. Nullable if no budget has been set.</summary>
        public decimal? Budget { get; set; }

        // TRANSFORMENGINE: mirrors backend DTO Profit (was JcProfit in base ProjectProfitabilityDto)
        /// <summary>Actual profit for the job code (Budget − TotalCosts).</summary>
        public decimal Profit { get; set; }

        // TRANSFORMENGINE: mirrors backend DTO TargetProfit — same name as base ProjectProfitabilityDto.TargetProfit
        /// <summary>Target profit for the programme.</summary>
        public decimal TargetProfit { get; set; }

        // TRANSFORMENGINE: mirrors backend DTO OffTarget = Profit - TargetProfit; negative value triggers red highlight in the VLA grid
        /// <summary>
        /// Difference between actual profit and target profit.
        /// A negative value triggers the red highlight in the ProjectProfitabilityVla grid
        /// (mirrors projectprofitability_vla.js updateSummary behaviour).
        /// </summary>
        public decimal OffTarget { get; set; }
    }
}
