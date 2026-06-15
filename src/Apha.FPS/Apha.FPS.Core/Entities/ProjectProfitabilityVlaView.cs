// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — ProjectProfitabilityVlaView.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination (Steps 2-3)
 * Migrated : 2026-06-15
 *
 * CHANGED:
 *   - New file: keyless view entity for the Project Profitability VLA list.
 *   - Source artefacts: qryJobCodeTotals.msaccsql, qryJobCodeTotals2.msaccsql,
 *     frmJobcodeTotalsVLA.html prototype, projectprofitability_vla.js (normalizeRow).
 *   - Entity property names aligned with ProjectProfitabilityVlaRes contract to
 *     simplify Application-layer mapper (AutoMapper / manual).
 *   - Access query cost column names (JCTotalStaffCosts, JCTotalTestCosts, etc.)
 *     renamed to match Res contract names: StaffCosts, TestCost, AnimalCosts,
 *     AdditionalCosts.
 *   - Budget_CVL → Budget (nullable decimal) aligned with Res contract.
 *   - JCProfit → Profit; tlkpProgram.Target → TargetProfit; OffTarget preserved.
 *   - Manager and Customer added — VLA-specific columns absent from base
 *     ProjectProfitabilityView.
 *   - ProjectStatus → Status to align with Res contract naming.
 *   - Marked keyless (HasNoKey) — must be configured in EF DbContext; no PK.
 *
 * PRESERVED:
 *   - All nine financial columns from qryJobCodeTotals (StaffCosts, TestCost,
 *     AnimalCosts, AdditionalCosts, TotalCosts, Budget, Profit, TargetProfit,
 *     OffTarget) matching the nine summary fields in frmJobcodeTotalsVLA.html.
 *   - Nullable Budget (Budget_CVL in tlkpProject may be NULL).
 *   - All four filter dimension fields (Program, Customer, Manager, Status)
 *     needed by the VLA filter bar.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm view name — assumed vprojectprofitabilityvla;
 *     verify against actual PostgreSQL DDL (view may need creating from qryJobCodeTotals
 *     aggregation logic). Register in FpsDbContext as:
 *     modelBuilder.Entity<ProjectProfitabilityVlaView>().HasNoKey()
 *                 .ToView("vprojectprofitabilityvla", "fps");
 *   - TRANSFORMENGINE TODO: confirm Id column — Res contract uses int Id but
 *     qryJobCodeTotals has no numeric PK; the view may require ROW_NUMBER() or
 *     may use JobCode as the row identifier. Remove / retype Id if the view omits it.
 *   - TRANSFORMENGINE TODO: confirm OffTarget is a computed column in the PostgreSQL
 *     view (JCProfit - Target) or whether it must be derived in the Application layer.
 *   - TRANSFORMENGINE TODO: confirm Budget nullability — remove '?' if the view
 *     column is defined NOT NULL.
 *   - TRANSFORMENGINE TODO: verify column data-types and lengths for Program, Customer,
 *     Manager, Status against the final vprojectprofitabilityvla view DDL.
 */

namespace Apha.FPS.Core.Entities
{
    /// <summary>
    /// Keyless view entity for the Project Profitability VLA list.
    /// Maps to the <c>vprojectprofitabilityvla</c> PostgreSQL view in the fps schema.
    /// Must be registered with <c>HasNoKey()</c> in the EF DbContext configuration.
    /// </summary>
    /// <remarks>
    /// Source artefact: MS Access <c>qryJobCodeTotals</c> / <c>qryJobCodeTotals2</c>
    /// aggregating staff, test, animal, and additional costs per job code
    /// (<c>tlkpProject.ParentProject</c>), joined to <c>tlkpProgram</c> for Manager,
    /// Target profit, and ProgramNo.  The VLA variant adds Manager and Customer filter
    /// dimensions that are absent from the base <see cref="ProjectProfitabilityView"/>.
    /// </remarks>
    public class ProjectProfitabilityVlaView
    {
        // TRANSFORMENGINE: optional numeric row identifier — Res contract exposes int Id;
        //   verify whether the PostgreSQL view includes a ROW_NUMBER() / sequence column.
        /// <summary>
        /// Optional numeric row identifier.
        /// Verify whether the view provides a ROW_NUMBER() or surrogate id column.
        /// </summary>
        public int? Id { get; set; }

        // TRANSFORMENGINE: maps qryJobCodeTotals.JobCode (tlkpProject.ParentProject);
        //   grid column header "Project"; also the natural row key for the view.
        /// <summary>
        /// Job code (project code). Maps to <c>tlkpProject.ParentProject</c>
        /// in the source Access query.
        /// </summary>
        public string JobCode { get; set; } = null!;

        // TRANSFORMENGINE: maps qryJobCodeTotals.Program (tlkpProject.Program / ProgramNo);
        //   grid column "Program"; VLA filter dimension filterProgram.
        /// <summary>Program number. Used to populate the Program filter dropdown.</summary>
        public string? Program { get; set; }

        // TRANSFORMENGINE: maps qryJobCodeTotals.Customer (tlkpProject.Customer);
        //   VLA-specific field absent from base ProjectProfitabilityView; filter dimension filterCustomer.
        /// <summary>
        /// Customer name. VLA-specific filter dimension absent from the base
        /// <see cref="ProjectProfitabilityView"/>.
        /// </summary>
        public string? Customer { get; set; }

        // TRANSFORMENGINE: maps qryJobCodeTotals2.Manager (tlkpProgram.Manager);
        //   VLA-specific field absent from base ProjectProfitabilityView; filter dimension filterManager.
        /// <summary>
        /// Manager name. VLA-specific filter dimension absent from the base
        /// <see cref="ProjectProfitabilityView"/>.
        /// </summary>
        public string? Manager { get; set; }

        // TRANSFORMENGINE: maps qryJobCodeTotals.ProjectStatus (tlkpProject.ProjectStatus);
        //   static filter values: Approved, Completed, Not Approved (filterProjectStatus).
        /// <summary>
        /// Project status. Filter dimension with static options:
        /// "Approved", "Completed", "Not Approved".
        /// </summary>
        public string? Status { get; set; }

        // ── Financial columns ─────────────────────────────────────────────────────

        // TRANSFORMENGINE: maps qryJobCodeTotals JCTotalStaffCosts (IIf(IsNull…) expression);
        //   was JcTotalStaffCosts in base ProjectProfitabilityView; Res contract: StaffCosts.
        /// <summary>
        /// Total staff costs for the job code.
        /// Derived from <c>qryTotalStaffCosts</c> sub-query in the source Access query.
        /// </summary>
        public decimal StaffCosts { get; set; }

        // TRANSFORMENGINE: maps qryJobCodeTotals JCTotalTestCosts; was JcTotalTestCosts; Res contract: TestCost.
        /// <summary>
        /// Total test costs for the job code.
        /// Derived from <c>qryTotalTestCosts</c> sub-query.
        /// </summary>
        public decimal TestCost { get; set; }

        // TRANSFORMENGINE: maps qryJobCodeTotals JCTotalAnimalCosts; was JcTotalAnimalCosts; Res contract: AnimalCosts.
        /// <summary>
        /// Total animal costs for the job code.
        /// Derived from <c>qryTotalAnimalCosts</c> sub-query.
        /// </summary>
        public decimal AnimalCosts { get; set; }

        // TRANSFORMENGINE: maps qryJobCodeTotals JCTotalAdditionalCosts; was JcTotalAdditionalCosts; Res contract: AdditionalCosts.
        /// <summary>
        /// Total additional costs for the job code.
        /// Derived from <c>qryTotalAdditionalCosts</c> sub-query.
        /// </summary>
        public decimal AdditionalCosts { get; set; }

        // TRANSFORMENGINE: maps qryJobCodeTotals TotalCosts =
        //   JCTotalAnimalCosts + JCTotalAdditionalCosts + JCTotalStaffCosts + JCTotalTestCosts.
        /// <summary>
        /// Sum of all cost categories (StaffCosts + TestCost + AnimalCosts + AdditionalCosts).
        /// Computed expression in the source Access query.
        /// </summary>
        public decimal TotalCosts { get; set; }

        // TRANSFORMENGINE: maps qryJobCodeTotals Budget_CVL (tlkpProject.Budget_CVL);
        //   nullable — may be NULL; was BudgetCvl in base ProjectProfitabilityView.
        /// <summary>
        /// Budget (CVL) for the project. Nullable if no budget has been set on the project.
        /// </summary>
        public decimal? Budget { get; set; }

        // TRANSFORMENGINE: maps qryJobCodeTotals JCProfit = Budget_CVL - TotalCosts;
        //   was JcProfit in base ProjectProfitabilityView.
        /// <summary>
        /// Actual profit (Budget − TotalCosts). Computed column in the source query.
        /// </summary>
        public decimal Profit { get; set; }

        // TRANSFORMENGINE: maps qryJobCodeTotals2 Target (tlkpProgram.Target);
        //   was ProgrammeTarget in base ProjectProfitabilityView.
        /// <summary>
        /// Target profit for the programme. Sourced from <c>tlkpProgram.Target</c>.
        /// </summary>
        public decimal TargetProfit { get; set; }

        // TRANSFORMENGINE: OffTarget = Profit - TargetProfit; verify whether this is a
        //   computed column in the PostgreSQL view or derived in the Application layer.
        /// <summary>
        /// Difference between actual profit and target profit.
        /// A negative value triggers a red highlight in the UI
        /// (see <c>projectprofitability_vla.js</c> grid rendering).
        /// </summary>
        public decimal OffTarget { get; set; }
    }
}
