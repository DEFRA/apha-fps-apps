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
        /// <summary>
        /// Optional numeric row identifier.
        /// Verify whether the view provides a ROW_NUMBER() or surrogate id column.
        /// </summary>
        public int? Id { get; set; }
        /// <summary>
        /// Job code (project code). Maps to <c>tlkpProject.ParentProject</c>
        /// in the source Access query.
        /// </summary>
        public string JobCode { get; set; } = null!;
        /// <summary>Program number. Used to populate the Program filter dropdown.</summary>
        public string? Program { get; set; }
        /// <summary>
        /// Customer name. VLA-specific filter dimension absent from the base
        /// <see cref="ProjectProfitabilityView"/>.
        /// </summary>
        public string? Customer { get; set; }
        /// <summary>
        /// Manager name. VLA-specific filter dimension absent from the base
        /// <see cref="ProjectProfitabilityView"/>.
        /// </summary>
        public string? Manager { get; set; }
        /// <summary>
        /// Project status. Filter dimension with static options:
        /// "Approved", "Completed", "Not Approved".
        /// </summary>
        public string? Status { get; set; }

        // ── Financial columns ─────────────────────────────────────────────────────
        /// <summary>
        /// Total staff costs for the job code.
        /// Derived from <c>qryTotalStaffCosts</c> sub-query in the source Access query.
        /// </summary>
        public decimal StaffCosts { get; set; }
        /// <summary>
        /// Total test costs for the job code.
        /// Derived from <c>qryTotalTestCosts</c> sub-query.
        /// </summary>
        public decimal TestCost { get; set; }
        /// <summary>
        /// Total animal costs for the job code.
        /// Derived from <c>qryTotalAnimalCosts</c> sub-query.
        /// </summary>
        public decimal AnimalCosts { get; set; }
        /// <summary>
        /// Total additional costs for the job code.
        /// Derived from <c>qryTotalAdditionalCosts</c> sub-query.
        /// </summary>
        public decimal AdditionalCosts { get; set; }
        /// <summary>
        /// Sum of all cost categories (StaffCosts + TestCost + AnimalCosts + AdditionalCosts).
        /// Computed expression in the source Access query.
        /// </summary>
        public decimal TotalCosts { get; set; }
        /// <summary>
        /// Budget (CVL) for the project. Nullable if no budget has been set on the project.
        /// </summary>
        public decimal? Budget { get; set; }
        /// <summary>
        /// Actual profit (Budget − TotalCosts). Computed column in the source query.
        /// </summary>
        public decimal Profit { get; set; }
        /// <summary>
        /// Target profit for the programme. Sourced from <c>tlkpProgram.Target</c>.
        /// </summary>
        public decimal TargetProfit { get; set; }
        /// <summary>
        /// Difference between actual profit and target profit.
        /// A negative value triggers a red highlight in the UI
        /// (see <c>projectprofitability_vla.js</c> grid rendering).
        /// </summary>
        public decimal OffTarget { get; set; }
    }
}
