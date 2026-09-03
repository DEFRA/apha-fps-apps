using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Steps;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>How a table relates to the FPS year for Year End purposes.</summary>
public enum YearEndTableRole
{
    /// <summary>Copy to target year.</summary>
    YearScopedBusinessParticipant,

    /// <summary>
    /// Not a business-copy table, but still year-scoped and read during schema validation
    /// (e.g. <c>fps.tblsettings</c>/<c>fps.tlkpmonthhours</c>).
    /// </summary>
    YearScopedConfigurationDependency,

    /// <summary>Not year-scoped at all — no target-year row concept, no partition to validate.</summary>
    GlobalReference,

    /// <summary>
    /// Year-scoped but must stay empty in the target year — validated, never populated or deleted,
    /// by <see cref="ValidateTargetYearEmptyTablesStep"/> and re-checked by <see cref="FinalValidationStep"/>.
    /// </summary>
    YearScopedTargetMustBeEmpty
}

/// <summary>
/// The Year End action approved for a table. <see cref="PendingClassification"/> and
/// <see cref="AlreadyImplementedViaDedicatedStep"/> are placeholders — not safe to treat as a
/// generic copy.
/// </summary>
public enum YearEndTableRuleAction
{
    PendingClassification,

    /// <summary>Has bespoke handling via its own step (e.g. <c>tblperiod</c> via <see cref="PeriodSetupStep"/>).</summary>
    AlreadyImplementedViaDedicatedStep,

    CopyToTargetYear,
    CreateTargetYearRow,
    ResetTargetYearRows,

    ValidateExists,
    SkipLegacyObsolete,
    ManualReviewRequired,

    /// <summary>See <see cref="YearEndTableRole.YearScopedTargetMustBeEmpty"/>.</summary>
    TargetYearMustBeEmpty
}

/// <summary>
/// Reset-phase names shared between the matrix data below and
/// <see cref="ProjectFinancialResetStep"/>/<see cref="ConfiguredPlanningResetStep"/> so they can't
/// drift out of sync via a typo'd literal.
/// </summary>
public static class YearEndResetPhase
{
    public const string ProjectFinancialReset = "ProjectFinancialReset";
    public const string ConfiguredPlanningReset = "ConfiguredPlanningReset";
}

/// <summary>
/// How <see cref="FinalValidationStep"/> compares a <see cref="YearEndTableRuleAction.CopyToTargetYear"/>
/// table's final row count against its source-year count.
/// </summary>
public enum YearEndFinalRowCountRule
{
    /// <summary>Row-count comparison doesn't apply (non-<c>CopyToTargetYear</c> entries).</summary>
    NotApplicable,

    /// <summary>Target row count must equal source row count — the default for a plain copy.</summary>
    MatchSource,

    /// <summary>
    /// Target row count must be at most the source row count — for tables a later step legitimately
    /// removes rows from after the copy (currently only <c>tblstaffjob</c>, via
    /// <see cref="InactiveEmployeeCleanupStep"/>).
    /// </summary>
    AtMostSource
}

/// <summary>
/// One row of the Year End Table Rule Matrix — the single source of truth for which tables Year
/// End knows about, how each relates to the FPS year, and what's approved to happen to it.
/// </summary>
/// <param name="PrimaryKeyColumns">
/// Composite primary key, in column order. Year-scoped roles must end the list with
/// <c>"fpsyear"</c>; <see cref="YearEndTableRole.GlobalReference"/> entries leave it empty.
/// </param>
/// <param name="CopyOrder">
/// Dependency order for <see cref="YearEndTableRuleAction.CopyToTargetYear"/> entries — lower
/// numbers are copied first so referenced rows exist before referencing ones. <c>null</c> otherwise.
/// </param>
/// <param name="ResetPhase">
/// Which reset step applies <see cref="Overrides"/> to this table, after the copy step has run.
/// <c>null</c> if the table has no column-level reset.
/// </param>
/// <param name="Overrides">
/// Column name -> literal SQL value applied via <c>UPDATE ... SET</c> to target-year rows, by
/// whichever step matches <see cref="ResetPhase"/>.
/// </param>
/// <param name="FinalRowCountRule">
/// How <see cref="FinalValidationStep"/> checks this table's final row count. Only meaningful for
/// <see cref="YearEndTableRuleAction.CopyToTargetYear"/> entries.
/// </param>
/// <param name="ExpectedTargetRowCount">
/// Exact target-year row count <see cref="FinalValidationStep"/> must see for
/// <see cref="YearEndTableRuleAction.AlreadyImplementedViaDedicatedStep"/> entries (e.g. tblperiod's
/// fixed 12 calendar periods). <c>null</c> falls back to the looser "at least one row" check — use
/// only when the dedicated step's output count is a genuine fixed business invariant, not an
/// incidental default.
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
    YearEndFinalRowCountRule FinalRowCountRule = YearEndFinalRowCountRule.NotApplicable,
    int? ExpectedTargetRowCount = null);

/// <summary>
/// The Year End Table Rule Matrix — single source of truth for every table Year End's schema
/// validation and business-data steps use. No step should keep its own second, independent list.
/// </summary>
/// <remarks>
/// 65 entries total: 39 business participants + 21 must-stay-empty tables (the 60 that Year End
/// actually copies, resets, or checks are empty) plus 5 read-only reference/config tables.
/// </remarks>
public static class YearEndTableRuleMatrix
{
    private const string Schema = "fps";

    public static IReadOnlyList<YearEndTableRuleMatrixEntry> Entries { get; } =
    [
        // 39 year-scoped business participants (Table 23). CopyOrder = FK-dependency layer (0-5).
        // FinalRowCountRule is MatchSource except tblstaffjob (AtMostSource — InactiveEmployeeCleanupStep
        // may remove rows after copy).
        new(Schema, "costcentre", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["costcentre", "fpsyear"], CopyOrder: 0, FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "divisiongrade", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["divisiongrade", "fpsyear"], CopyOrder: 1, FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "grade", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["gradecode", "fpsyear"], CopyOrder: 0, FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "milestone", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["project", "milestoneref", "objectiveref", "fpsyear"], CopyOrder: 2, FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "plancatwggrade", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["plancategory", "wggrade", "fpsyear"], CopyOrder: 4, FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "profitcentregrade", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["pcgrade", "fpsyear"], CopyOrder: 2, FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "profitcentregrade_nondefra", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["pcgrade", "fpsyear"], CopyOrder: 2, FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "projectmonth2", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["project", "monthno", "fpsyear"], CopyOrder: 0,
            Notes: "RecreateSummaries independently rebuilds this table's rows afterward — unrelated to Year End's copy.",
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
            Notes: "Implemented via PeriodSetupStep, not the generic copy mechanism — no CopyOrder. PeriodSetupStep enforces exactly 12 source/target rows itself; FinalValidationStep re-checks the same invariant via ExpectedTargetRowCount.",
            ExpectedTargetRowCount: 12),
        new(Schema, "tblstaffjob", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["staffid", "jobcode", "fpsyear"],
            CopyOrder: 5,
            ResetPhase: YearEndResetPhase.ConfiguredPlanningReset,
            Overrides: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["plannedhours"] = "0" },
            Notes: "FK-dependent on tblwgemployee (staffid) and tlkpproject (jobcode), hence CopyOrder 5. InactiveEmployeeCleanupStep removes inactive-employee rows here first, hence AtMostSource, not MatchSource.",
            FinalRowCountRule: YearEndFinalRowCountRule.AtMostSource),
        new(Schema, "tbltestrccost", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["testcode", "profitcentre", "fpsyear"], CopyOrder: 1, FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "tbltestrequirementrccost", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["testcode", "buyer", "profitcentre", "fpsyear"], CopyOrder: 2, FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "tbltestreqwg", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["testcode", "buyer", "workgroup", "fpsyear"], CopyOrder: 0, FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "tbltotalbusinessoverheads", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["fpsyear"], CopyOrder: 0,
            Notes: "Plain copy of the singleton row — totalbusinessoverheads is preserved as-is (including NULL), never forced to NULL.",
            FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "tbluser_profitcentre", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["profitcentre", "user_id", "fpsyear"], CopyOrder: 0, FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "tbluser_program", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["programno", "user_id", "fpsyear"], CopyOrder: 0, FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "tbluser_projectgroup", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["projectgroup", "user_id", "fpsyear"], CopyOrder: 0, FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "tbluser_testowner", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["test_owner", "user_id", "fpsyear"], CopyOrder: 0, FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "tblwgemployee", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["pactid", "fpsyear"], CopyOrder: 4,
            Notes: "Plain copy, no resets. InactiveEmployeeCleanupStep deletes inactive-employee rows here too (not just tblstaffjob), hence AtMostSource, matching tblstaffjob. tblstaffjob rows are removed first (FK dependency).",
            FinalRowCountRule: YearEndFinalRowCountRule.AtMostSource),
        new(Schema, "testorproduct", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["itemcode", "fpsyear"], CopyOrder: 0, FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "timecodevalid", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["workgroup", "timecode", "parentproject", "fpsyear"], CopyOrder: 2, FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "tlkpjobcode", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["jobcode", "fpsyear"], CopyOrder: 2, FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "tlkpmanager", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["manager", "fpsyear"], CopyOrder: 0, FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "tlkpprogram", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["programno", "fpsyear"], CopyOrder: 0, FinalRowCountRule: YearEndFinalRowCountRule.MatchSource),
        new(Schema, "tlkpprojectgroup", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CopyToTargetYear, ["projectgroup", "fpsyear"],
            CopyOrder: 0,
            FinalRowCountRule: YearEndFinalRowCountRule.MatchSource,
            Notes: "tlkpproject.projectgroup FKs here, hence CopyOrder 0 (before tlkpproject's CopyOrder 1). Year End now owns and copies this year-scoped dependency directly instead of relying on external provisioning."),
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

        // 2 year-scoped configuration dependencies — not Table 23, but still partitioned by fpsyear.
        new(Schema, "tblsettings", YearEndTableRole.YearScopedConfigurationDependency, YearEndTableRuleAction.ValidateExists, ["id", "fpsyear"]),
        new(Schema, "tlkpmonthhours", YearEndTableRole.YearScopedConfigurationDependency, YearEndTableRuleAction.ValidateExists, ["year", "month", "fpsyear"]),

        // 3 global/reference participants — not year-scoped, no target-year row concept.
        new(Schema, "tblusers", YearEndTableRole.GlobalReference, YearEndTableRuleAction.ValidateExists, []),
        new(Schema, "tblcategory", YearEndTableRole.GlobalReference, YearEndTableRuleAction.ValidateExists, []),
        new(Schema, "tblkpprofitcentre", YearEndTableRole.GlobalReference, YearEndTableRuleAction.ValidateExists, []),

        // 21 tables that must stay empty in the target year. The old one-database-per-year
        // architecture deleted these each Year End; the current single-database architecture never
        // writes target-year rows for them at all, so the correct check is "assert zero rows", never
        // a DELETE. Validated by ValidateTargetYearEmptyTablesStep, re-checked by FinalValidationStep.
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
