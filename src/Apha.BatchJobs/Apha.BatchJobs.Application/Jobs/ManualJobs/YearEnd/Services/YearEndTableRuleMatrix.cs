using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Steps;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>
/// How a table relates to the FPS year for Year End purposes.
/// </summary>
public enum YearEndTableRole
{
    /// <summary>One of the 38 Table 23 entries — year-scoped business data.</summary>
    YearScopedBusinessParticipant,

    /// <summary>
    /// Not a Table 23 business-copy participant, but still year-scoped (partitioned by
    /// <c>fpsyear</c>) and depended on by Year End — <c>fps.tblsettings</c>/<c>fps.tlkpmonthhours</c>,
    /// read by the Year End context/configuration validation step (source branch's
    /// <c>ValidateYearEndConfigurationStep</c> — not yet ported to `main`, out of this port's scope).
    /// </summary>
    YearScopedConfigurationDependency,

    /// <summary>Not year-scoped at all — no target-year row concept, no partition to validate.</summary>
    GlobalReference,

    /// <summary>
    /// Spec §19 legacy candidates — year-scoped (partitioned by <c>fpsyear</c>) but must start (and
    /// stay) empty in the target year: validated, never populated, never deleted, by
    /// <see cref="ValidateTargetYearEmptyTablesStep"/> and independently re-verified by
    /// <see cref="FinalValidationStep"/>. Not Table 23 business participants.
    /// </summary>
    YearScopedTargetMustBeEmpty
}

/// <summary>
/// The Year End action approved for a table. <see cref="PendingClassification"/> and
/// <see cref="AlreadyImplementedViaDedicatedStep"/> are placeholders pending Phase 2 steps 3-4 —
/// they must not be treated as "safe to generically copy" by any step.
/// </summary>
public enum YearEndTableRuleAction
{
    /// <summary>Not yet decided — Phase 2 steps 3-4 replace this with an approved action.</summary>
    PendingClassification,

    /// <summary>
    /// Already has bespoke handling via an existing dedicated step (e.g. <c>tblperiod</c> via
    /// <see cref="PeriodSetupStep"/>) — must not be folded into a generic action.
    /// </summary>
    AlreadyImplementedViaDedicatedStep,

    CopyToTargetYear,
    CreateTargetYearRow,
    ResetTargetYearRows,

    ValidateExists,
    SkipLegacyObsolete,
    ManualReviewRequired,

    /// <summary>
    /// This matrix entry's target-year rows must be absent — validated and never populated by any
    /// Year End step, and never deleted by one either. The pre-modernization architecture (one
    /// database per FPS year) deleted these tables' contents as part of Year End; the current
    /// multi-year, single-database architecture instead never copies/inserts target-year rows for
    /// them in the first place, so the correct production behaviour is "assert zero rows, fail if
    /// not" — never a DELETE. Owned by <see cref="ValidateTargetYearEmptyTablesStep"/>, independently
    /// re-verified by <see cref="FinalValidationStep"/>.
    /// </summary>
    TargetYearMustBeEmpty
}

/// <summary>
/// Names for the pipeline reset step responsible for applying a matrix entry's
/// <see cref="YearEndTableRuleMatrixEntry.Overrides"/>. Shared between the matrix data below and
/// <see cref="ProjectFinancialResetStep"/>/<see cref="ConfiguredPlanningResetStep"/> so the two
/// never drift out of sync via a typo'd literal.
/// </summary>
public static class YearEndResetPhase
{
    public const string ProjectFinancialReset = "ProjectFinancialReset";
    public const string ConfiguredPlanningReset = "ConfiguredPlanningReset";
}

/// <summary>
/// How <see cref="FinalValidationStep"/> should compare a <see cref="YearEndTableRuleAction.CopyToTargetYear"/>
/// table's final target-year row count against its source-year row count. Deliberately an enum, not
/// a boolean flag — "does row count change after copy, and how" is a business rule with more than
/// two meaningfully different answers, and an enum reads that intent directly at each call site
/// instead of requiring a reader to know what <c>true</c>/<c>false</c> means for this specific flag.
/// </summary>
public enum YearEndFinalRowCountRule
{
    /// <summary>Row-count comparison doesn't apply (non-<c>CopyToTargetYear</c> entries).</summary>
    NotApplicable,

    /// <summary>
    /// Target-year row count must equal source-year row count — the default for a plain copy.
    /// Nothing between <see cref="CopyFpsYearScopedTablesStep"/> and Data Setup completion is
    /// expected to change this table's row count.
    /// </summary>
    MatchSource,

    /// <summary>
    /// Target-year row count must be less than or equal to source-year row count — for tables a
    /// later pipeline step is expected to legitimately remove rows from after the copy (currently
    /// only <c>tblstaffjob</c>, via <see cref="InactiveEmployeeCleanupStep"/>). This is a row-count
    /// sanity check only ("did *something* run"), not proof the removal itself was correct — that's
    /// a business-invariant check <see cref="InactiveEmployeeCleanupStep"/>'s own correctness fixes
    /// are responsible for, not <see cref="FinalValidationStep"/>'s row-count arithmetic.
    /// </summary>
    AtMostSource
}

/// <summary>
/// One row of the Year End Table Rule Matrix — the single source of truth for which tables Year
/// End is aware of, how each relates to the FPS year, and what (if anything) has been approved to
/// happen to it. Consumed by <see cref="ValidateYearScopedSchemaStep"/> (partition-existence
/// validation), <see cref="CopyFpsYearScopedTablesStep"/> (<see cref="CopyOrder"/>-driven copy),
/// <see cref="ProjectFinancialResetStep"/>/<see cref="ConfiguredPlanningResetStep"/>
/// (<see cref="ResetPhase"/>/<see cref="Overrides"/>-driven resets), <see cref="ValidateTargetYearEmptyTablesStep"/>
/// (<see cref="YearEndTableRuleAction.TargetYearMustBeEmpty"/>-driven validation), and
/// <see cref="FinalValidationStep"/> (dispatches per-entry validation by <see cref="Role"/>/<see cref="Action"/>,
/// including <see cref="FinalRowCountRule"/>).
/// </summary>
/// <param name="CopyOrder">
/// Dependency-ordering key for <see cref="YearEndTableRuleAction.CopyToTargetYear"/> entries only —
/// derived from the Phase 2 FK/dependency scan's topological layers (0 = no dependency on another
/// matrix entry, higher numbers depend on lower ones). <c>null</c> for every other action.
/// <see cref="CopyFpsYearScopedTablesStep"/> processes entries in ascending <see cref="CopyOrder"/>
/// so a referenced table's target-year row always exists before the referencing table is copied.
/// </param>
/// <param name="ResetPhase">
/// Which reset step (see <see cref="YearEndResetPhase"/>) applies <see cref="Overrides"/> to this
/// table's target-year rows, after <see cref="CopyFpsYearScopedTablesStep"/> has run. <c>null</c>
/// when the table has no column-level reset — <c>CopyToTargetYear</c> alone is a plain copy.
/// </param>
/// <param name="Overrides">
/// Column name -> literal SQL value (e.g. <c>"0"</c>, <c>"NULL"</c>) applied via <c>UPDATE ... SET</c>
/// to this table's target-year rows by whichever step matches <see cref="ResetPhase"/>. Only
/// meaningful when <see cref="ResetPhase"/> is set.
/// </param>
/// <param name="FinalRowCountRule">
/// How <see cref="FinalValidationStep"/> compares this table's final target-year row count against
/// its source-year row count. Only meaningful for <see cref="YearEndTableRuleAction.CopyToTargetYear"/>
/// entries — <see cref="YearEndFinalRowCountRule.NotApplicable"/> everywhere else.
/// </param>
public sealed record YearEndTableRuleMatrixEntry(
    string Schema,
    string TableName,
    YearEndTableRole Role,
    YearEndTableRuleAction Action,
    IReadOnlyList<string> PrimaryKeyColumns,
    string? Notes = null,
    int? CopyOrder = null,
    string? ResetPhase = null,
    IReadOnlyDictionary<string, string>? Overrides = null,
    YearEndFinalRowCountRule FinalRowCountRule = YearEndFinalRowCountRule.NotApplicable);

/// <summary>
/// The Year End Table Rule Matrix. All 38 Table 23 entries, the 3 year-scoped configuration
/// dependencies, the 3 global/reference participants, and the 21 spec §19 must-start-empty
/// tables — every table Year End's schema validation and business-data steps are aware of. Every
/// table-name list Year End steps use lives here; no step should own a second, independent list.
///
/// <para>
/// <see cref="Entries"/> totals <b>65</b> rows across 4 <see cref="YearEndTableRole"/> values —
/// the full schema-validation universe (<see cref="ValidateYearScopedSchemaStep"/> checks routing
/// for all 65). A commonly-quoted narrower figure, <b>59</b>, is the mutable-business-data subset:
/// 38 <see cref="YearEndTableRole.YearScopedBusinessParticipant"/> + 21
/// <see cref="YearEndTableRole.YearScopedTargetMustBeEmpty"/> — the tables Year End actually
/// copies, resets, or clears. The excluded 6 (3 <see cref="YearEndTableRole.YearScopedConfigurationDependency"/>
/// + 3 <see cref="YearEndTableRole.GlobalReference"/>) are validated but never written by the
/// business-data steps. Both numbers are correct; they answer different questions — don't collapse
/// them into a single count.
/// </para>
/// </summary>
public static class YearEndTableRuleMatrix
{
    private const string Schema = "fps";

    public static IReadOnlyList<YearEndTableRuleMatrixEntry> Entries { get; } =
    [
        // 38 year-scoped business participants (Table 23). Actions/CopyOrder/ResetPhase/Overrides/
        // FinalRowCountRule confirmed 2026-08-14 against the Table 23 workbook + a read-only FK/
        // dependency scan — see docs/fps-year-end-phase2-table-classification-draft-2026-08-14.md
        // for full rationale and evidence per table. CopyOrder values are the scan's 6 topological
        // layers (0-5). FinalRowCountRule is MatchSource for every CopyToTargetYear entry except
        // tblstaffjob (AtMostSource — InactiveEmployeeCleanupStep may remove rows afterward).
        new(Schema, "costcentre", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["costcentre", "fpsyear"], CopyOrder: 0, FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "divisiongrade", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["divisiongrade", "fpsyear"], CopyOrder: 1, FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "grade", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["gradecode", "fpsyear"], CopyOrder: 0, FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "milestone", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["project", "milestoneref", "objectiveref", "fpsyear"], CopyOrder: 2, FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "plancatwggrade", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["plancategory", "wggrade", "fpsyear"], CopyOrder: 4, FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "profitcentregrade", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["pcgrade", "fpsyear"], CopyOrder: 2, FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "profitcentregrade_nondefra", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["pcgrade", "fpsyear"], CopyOrder: 2, FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "projectmonth2", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["project", "monthno", "fpsyear"], CopyOrder: 0,
            Notes: "RecreateSummaries (a separate, already-implemented batch job) independently deletes/rebuilds this table's rows from source data; Year End's copy is unrelated to and unaffected by that.",
            FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "projectmonth3", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["endperiod", "project", "fpsyear"], CopyOrder: 0,
            Notes: "Same RecreateSummaries relationship as projectmonth2.",
            FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "tbladditionalcosts", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["jobcode", "account", "description", "fpsyear"],
            CopyOrder: 2,
            ResetPhase: YearEndResetPhase.ConfiguredPlanningReset,
            Overrides: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["itemcost"] = "0" },
            FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "tbladminusers", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["mnumber", "fpsyear"], CopyOrder: 0, FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "tblanimalreq", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["indcounter", "fpsyear"],
            CopyOrder: 2,
            ResetPhase: YearEndResetPhase.ConfiguredPlanningReset,
            Overrides: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["numberofanimals"] = "0", ["numberofdays"] = "0" },
            Notes: "indcounter is GENERATED BY DEFAULT AS IDENTITY, excluded from copy by the generic is_identity='NO' column filter.",
            FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "tblanimals", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["animaltype", "fpsyear"], CopyOrder: 0, FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "tblcontract", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["contractno", "fpsyear"], CopyOrder: 0, FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "tblemployee", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["spnumber", "fpsyear"], CopyOrder: 0, FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "tblkpaccountcategory", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["accshortname", "fpsyear"], CopyOrder: 0, FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "tblperiod", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.AlreadyImplementedViaDedicatedStep, ["periodname", "fpsyear"],
            Notes: "Implemented via PeriodSetupStep (copy + period-lock reset). FinalValidationStep applies a dedicated tblperiod check (target-year rows exist); exactly-12-target-year-periods enforcement is Phase 3 scope, not yet implemented. Not part of the generic copy mechanism; no CopyOrder."),
        new(Schema, "tblstaffjob", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["staffid", "jobcode", "fpsyear"],
            CopyOrder: 5,
            ResetPhase: YearEndResetPhase.ConfiguredPlanningReset,
            Overrides: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["plannedhours"] = "0" },
            Notes: "FK-dependent on tblwgemployee (staffid) and tlkpproject (jobcode), hence CopyOrder 5, the deepest layer. InactiveEmployeeCleanupStep (pipeline step 10) deletes this table's applicable inactive-employee target-year rows first (FK precedes tblwgemployee's own deletion) — hence FinalRowCountRule.AtMostSource, not MatchSource. Redesigned 2026-08-14 around the legacy Annual_WGEmployeeList.sql rule (personstatus='I' AND enddate IS NULL, General Staff exemption spnumber LIKE 'G%' AND firstname='GENERAL').",
            FinalRowCountRule: YearEndFinalRowCountRule.AtMostSource),
        new(Schema, "tbltestrccost", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["testcode", "profitcentre", "fpsyear"], CopyOrder: 1, FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "tbltestrequirementrccost", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["testcode", "buyer", "profitcentre", "fpsyear"], CopyOrder: 2, FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "tbltestreqwg", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["testcode", "buyer", "workgroup", "fpsyear"], CopyOrder: 0, FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "tbltotalbusinessoverheads", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["fpsyear"], CopyOrder: 0,
            Notes: "Copy the current-year singleton row into plannedYear, replacing only fpsyear; preserve totalbusinessoverheads as-is (including NULL) rather than forcing it to NULL. Corrected 2026-08-14: supersedes the earlier CreateTargetYearRow/force-NULL answer, which was Year-End-session-inferred rather than workbook-confirmed. The workbook is authoritative and places this table in the plain annual copy set.",
            FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "tbluser_profitcentre", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["profitcentre", "user_id", "fpsyear"], CopyOrder: 0, FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "tbluser_program", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["programno", "user_id", "fpsyear"], CopyOrder: 0, FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "tbluser_projectgroup", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["projectgroup", "user_id", "fpsyear"], CopyOrder: 0, FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "tbluser_testowner", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["test_owner", "user_id", "fpsyear"], CopyOrder: 0, FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "tblwgemployee", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["pactid", "fpsyear"], CopyOrder: 4,
            Notes: "Plain copy only — no column-level resets. Corrected 2026-08-14: InactiveEmployeeCleanupStep (pipeline step 10) was redesigned around the legacy Annual_WGEmployeeList.sql rule and now deletes target-year rows directly from this table (inactive non-General-Staff employees: personstatus='I' AND enddate IS NULL, excluding spnumber LIKE 'G%' AND firstname='GENERAL'), not just from tblstaffjob — hence FinalRowCountRule.AtMostSource, matching tblstaffjob. tblstaffjob rows are deleted first (FK dependency) inside the same step.",
            FinalRowCountRule: YearEndFinalRowCountRule.AtMostSource),
        new(Schema, "testorproduct", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["itemcode", "fpsyear"], CopyOrder: 0, FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "timecodevalid", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["workgroup", "timecode", "parentproject", "fpsyear"], CopyOrder: 2, FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "tlkpjobcode", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["jobcode", "fpsyear"], CopyOrder: 2, FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "tlkpmanager", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["manager", "fpsyear"], CopyOrder: 0, FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "tlkpprogram", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["programno", "fpsyear"], CopyOrder: 0, FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "tlkpproject", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["parentproject", "fpsyear"],
            CopyOrder: 1,
            ResetPhase: YearEndResetPhase.ProjectFinancialReset,
            Overrides: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["transferincome"] = "0",
                ["custincome"] = "0",
                ["wip_eoy"] = "0",
                ["feccost"] = "0",
                ["profit"] = "0",
                ["budget_cvl"] = "0",
                ["carryover"] = "0",
                ["wip_limit"] = "NULL",
                ["wip_current"] = "NULL",
                ["pvsincome"] = "NULL",
                ["plancaseworkdebit"] = "NULL"
            },
            Notes: "FK-dependent on tblcontract and tlkpprogram (both NOT NULL columns), hence CopyOrder 1, not 0.",
            FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "tlkptestcapability", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["testcode", "workgroup", "fpsyear"], CopyOrder: 2, FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "tlkptestreqmt", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["testcode", "buyer", "fpsyear"],
            CopyOrder: 1,
            ResetPhase: YearEndResetPhase.ConfiguredPlanningReset,
            Overrides: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["norequired"] = "0" },
            FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "workgroup", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["workgroup", "fpsyear"], CopyOrder: 1, FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "workgroupgrade", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["wggrade", "fpsyear"], CopyOrder: 3, FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "workgroupmonth", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["workgroup", "month", "fpsyear"], CopyOrder: 0, FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),

        // 3 year-scoped configuration dependencies — not Table 23, but still partitioned by fpsyear.
        new(Schema, "tblsettings", YearEndTableRole.YearScopedConfigurationDependency, YearEndTableRuleAction.ValidateExists, ["id", "fpsyear"]),
        new(Schema, "tlkpmonthhours", YearEndTableRole.YearScopedConfigurationDependency, YearEndTableRuleAction.ValidateExists, ["year", "month", "fpsyear"]),
        new(Schema, "tlkpprojectgroup", YearEndTableRole.YearScopedConfigurationDependency, YearEndTableRuleAction.ValidateExists, ["projectgroup", "fpsyear"],
            Notes: "Found 2026-08-14 via FK/dependency scan: tlkpproject.projectgroup FKs here (year-gated), but it is not a Table 23 entry and nothing in Apha.BatchJobs or Apha.FPS writes to it. Confirmed externally provisioned: 30 rows every year 2016-2026 including the already-Planned 2026 (read-only, batchjob_testing). Year End validates existence, does not copy it. One pre-existing, unrelated data-quality gap found (a lowercase 'wld_res' value used by one 2025 tlkpproject row with no matching tlkpprojectgroup row in any year, including 2025 itself) — predates and is independent of Year End, not a target-year copy gap."),

        // 3 global/reference participants — not year-scoped, no target-year row concept.
        new(Schema, "tblusers", YearEndTableRole.GlobalReference, YearEndTableRuleAction.ValidateExists, []),
        new(Schema, "tblcategory", YearEndTableRole.GlobalReference, YearEndTableRuleAction.ValidateExists, []),
        new(Schema, "tblkpprofitcentre", YearEndTableRole.GlobalReference, YearEndTableRuleAction.ValidateExists, []),

        // 21 spec §19 "must start empty in the target year" legacy candidates. Migrated 2026-08-14
        // from TargetYearEmptyTablesStep's own hardcoded CandidateTables list and
        // FinalValidationStep's separate (and incomplete — only 7 of these 21) hardcoded
        // MustBeEmptyTargetYearTables list, so there is exactly one authoritative table-name list
        // instead of three. All 21 confirmed to exist in batchjob_testing (read-only scan,
        // 2026-08-14), all use fpsyear (not year) as their partition column, all are native
        // LIST(fpsyear) partitioned tables like the Table 23 set, all have a real primary key.
        //
        // Action/step renamed 2026-08-28 (Phase 7B), off the Year End Process New Approach workbook:
        // every one of these 21 tables' old-architecture Annual_UpdateOtherTables.sql DELETE is
        // marked N/A there, remarked "Year Identification column in table" — the one-database-per-year
        // architecture deleted these tables' contents as part of Year End; the current multi-year,
        // single-database architecture never copies/inserts target-year rows for them at all, so the
        // correct production behaviour is "assert zero target-year rows, fail if not", never a DELETE.
        // ValidateTargetYearEmptyTablesStep (was TargetYearEmptyTablesStep) now only validates this;
        // it does not mutate. Unlike the old delete-then-continue behaviour, a missing table or
        // unresolvable year column is now a hard failure in that step, not a silent skip — schema
        // existence for these 21 is ValidateYearScopedSchemaStep's contract, and this step trusts and
        // re-affirms it rather than quietly tolerating drift. FinalValidationStep's independent
        // re-check at the end of the pipeline keeps its own pre-existing skip-if-missing behaviour,
        // unchanged — it is a defense-in-depth re-verification, not the schema's contract owner.
        new(Schema, "additionalcosts_log", YearEndTableRole.YearScopedTargetMustBeEmpty, YearEndTableRuleAction.TargetYearMustBeEmpty, ["sequenceno", "fpsyear"]),
        new(Schema, "animalreq_log", YearEndTableRole.YearScopedTargetMustBeEmpty, YearEndTableRuleAction.TargetYearMustBeEmpty, ["sequenceno", "fpsyear"]),
        new(Schema, "fpsyeartotals", YearEndTableRole.YearScopedTargetMustBeEmpty, YearEndTableRuleAction.TargetYearMustBeEmpty, ["parentproject", "fpsyear"]),
        new(Schema, "mo_log", YearEndTableRole.YearScopedTargetMustBeEmpty, YearEndTableRuleAction.TargetYearMustBeEmpty, ["sequenceno", "fpsyear"]),
        new(Schema, "monthlyoutput", YearEndTableRole.YearScopedTargetMustBeEmpty, YearEndTableRuleAction.TargetYearMustBeEmpty, ["testcode", "buyer", "month", "workgroup", "fpsyear"]),
        new(Schema, "monthlytime", YearEndTableRole.YearScopedTargetMustBeEmpty, YearEndTableRuleAction.TargetYearMustBeEmpty, ["pactstaffid", "timecode", "month", "parentproject", "fpsyear"]),
        new(Schema, "mt_log", YearEndTableRole.YearScopedTargetMustBeEmpty, YearEndTableRuleAction.TargetYearMustBeEmpty, ["sequenceno", "fpsyear"]),
        new(Schema, "proj_invoice", YearEndTableRole.YearScopedTargetMustBeEmpty, YearEndTableRuleAction.TargetYearMustBeEmpty, ["invoicecounter", "fpsyear"]),
        new(Schema, "proj_subcontract", YearEndTableRole.YearScopedTargetMustBeEmpty, YearEndTableRuleAction.TargetYearMustBeEmpty, ["subcontcounter", "fpsyear"]),
        new(Schema, "project_log", YearEndTableRole.YearScopedTargetMustBeEmpty, YearEndTableRuleAction.TargetYearMustBeEmpty, ["sequenceno", "fpsyear"]),
        new(Schema, "projectmonth", YearEndTableRole.YearScopedTargetMustBeEmpty, YearEndTableRuleAction.TargetYearMustBeEmpty, ["project", "monthno", "fpsyear"]),
        new(Schema, "projectmonthfinal", YearEndTableRole.YearScopedTargetMustBeEmpty, YearEndTableRuleAction.TargetYearMustBeEmpty, ["project", "monthno", "fpsyear"]),
        new(Schema, "recreatesummaries_log", YearEndTableRole.YearScopedTargetMustBeEmpty, YearEndTableRuleAction.TargetYearMustBeEmpty, ["id", "fpsyear"]),
        new(Schema, "staffjob_log", YearEndTableRole.YearScopedTargetMustBeEmpty, YearEndTableRuleAction.TargetYearMustBeEmpty, ["sequenceno", "fpsyear"]),
        new(Schema, "tblbid", YearEndTableRole.YearScopedTargetMustBeEmpty, YearEndTableRuleAction.TargetYearMustBeEmpty, ["workgroup", "account", "fpsyear"]),
        new(Schema, "tblpurchase", YearEndTableRole.YearScopedTargetMustBeEmpty, YearEndTableRuleAction.TargetYearMustBeEmpty, ["workgroup", "account", "itemdescription", "fpsyear"]),
        new(Schema, "tblsurvff_fees", YearEndTableRole.YearScopedTargetMustBeEmpty, YearEndTableRuleAction.TargetYearMustBeEmpty, ["owning_vic", "contract", "record_id", "fpsyear"]),
        new(Schema, "tblsurvff_submissions", YearEndTableRole.YearScopedTargetMustBeEmpty, YearEndTableRuleAction.TargetYearMustBeEmpty, ["sd_pact_wg", "contract", "fpsyear"]),
        new(Schema, "tbltestreqbaseline", YearEndTableRole.YearScopedTargetMustBeEmpty, YearEndTableRuleAction.TargetYearMustBeEmpty, ["program", "testcode", "buyer", "fpsyear"]),
        new(Schema, "testreq_log", YearEndTableRole.YearScopedTargetMustBeEmpty, YearEndTableRuleAction.TargetYearMustBeEmpty, ["sequenceno", "fpsyear"]),
        new(Schema, "timecostcalcs", YearEndTableRole.YearScopedTargetMustBeEmpty, YearEndTableRuleAction.TargetYearMustBeEmpty, ["workgroup", "jobcode", "project", "month", "staffid", "fpsyear"])
    ];
}
