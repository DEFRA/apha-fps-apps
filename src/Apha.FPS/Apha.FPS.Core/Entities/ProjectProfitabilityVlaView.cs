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
