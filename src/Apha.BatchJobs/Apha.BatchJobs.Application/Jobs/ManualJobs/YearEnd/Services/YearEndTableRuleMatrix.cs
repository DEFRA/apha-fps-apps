using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Steps;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>
/// What Year End Data Setup does for a table. The only three responsibilities Data Setup has —
/// everything else about a table (reset, cleanup, how it's implemented) is an independent, orthogonal
/// fact about one of these three, not a fourth role.
/// </summary>
public enum YearEndPrimaryRole
{
    /// <summary>Copy the current year's rows to the target year.</summary>
    CopyToTargetYear,

    /// <summary>Populate target-year configuration from Approve-frozen staging (e.g. tblsettings/tlkpmonthhours).</summary>
    TargetYearConfiguration,

    /// <summary>Create the target year's row in fps.tblyearmaster — a new row, not a copy.</summary>
    CreateTargetYear
}

/// <summary>
/// Expected post-execution outcome for a table, checked by <see cref="FinalValidationStep"/>.
/// </summary>
public enum YearEndFinalValidationRule
{
    /// <summary>Target-year row count must equal source-year row count — the default for a plain copy.</summary>
    MatchSource,

    /// <summary>
    /// Target-year row count must be at most the source-year row count — for tables a later step
    /// legitimately removes rows from after the copy (<see cref="InactiveEmployeeCleanupStep"/>).
    /// </summary>
    AtMostSource,

    /// <summary>Target-year row count must equal <see cref="YearEndTableRuleMatrixEntry.ExpectedTargetRowCount"/> exactly.</summary>
    ExactTargetRowCount,

    /// <summary>Target-year row count must be at least one — the table exists and has been populated for the year, exact count doesn't matter.</summary>
    AtLeastOneTargetYearRow
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
/// One row of the Year End Table Rule Matrix — the single source of truth for every table Year End
/// Data Setup has an active responsibility for, and what that responsibility is.
/// </summary>
/// <param name="PrimaryKeyColumns">Composite primary key, in column order. Must end with <c>"fpsyear"</c>.</param>
/// <param name="FinalValidation">Expected post-execution outcome, checked by <see cref="FinalValidationStep"/>.</param>
/// <param name="DedicatedStep">
/// Name of the step that implements <see cref="PrimaryRole"/> for this table, if it isn't the generic
/// mechanism (<see cref="Steps.CopyFpsYearScopedTablesStep"/> for <see cref="YearEndPrimaryRole.CopyToTargetYear"/>).
/// Orthogonal to <see cref="PrimaryRole"/> — e.g. <c>tblperiod</c> is still <c>CopyToTargetYear</c>
/// semantically, it's just implemented via <see cref="PeriodSetupStep"/> instead of the generic copy loop.
/// <c>null</c> means the generic mechanism handles it.
/// </param>
/// <param name="CopyOrder">
/// Dependency order among the generic-mechanism <see cref="YearEndPrimaryRole.CopyToTargetYear"/>
/// entries (<see cref="DedicatedStep"/> is <c>null</c>) — lower numbers copied first so referenced rows
/// exist before referencing ones. <c>null</c> for every entry with a <see cref="DedicatedStep"/>.
/// </param>
/// <param name="ResetPhase">
/// Which reset step applies <see cref="Overrides"/> to this table, after the copy step has run.
/// <c>null</c> if the table has no column-level reset. Independent of <see cref="DedicatedStep"/>.
/// </param>
/// <param name="Overrides">
/// Column name -> literal SQL value applied via <c>UPDATE ... SET</c> to target-year rows, by
/// whichever step matches <see cref="ResetPhase"/>.
/// </param>
/// <param name="Cleanup">
/// Description of row-removal behaviour applied to this table's target-year rows after copy, if any
/// (currently only <see cref="InactiveEmployeeCleanupStep"/>'s inactive-employee removal). Independent
/// of <see cref="Overrides"/> — <c>tblstaffjob</c> has both a reset and a cleanup at once.
/// </param>
/// <param name="ExpectedTargetRowCount">
/// Exact target-year row count required when <see cref="FinalValidation"/> is
/// <see cref="YearEndFinalValidationRule.ExactTargetRowCount"/>.
/// </param>
public sealed record YearEndTableRuleMatrixEntry(
    string Schema,
    string TableName,
    YearEndPrimaryRole PrimaryRole,
    IReadOnlyList<string> PrimaryKeyColumns,
    YearEndFinalValidationRule FinalValidation,
    string? DedicatedStep = null,
    int? CopyOrder = null,
    string? ResetPhase = null,
    IReadOnlyDictionary<string, string>? Overrides = null,
    string? Cleanup = null,
    int? ExpectedTargetRowCount = null,
    string? Notes = null);

/// <summary>
/// The Year End Data Setup Table Rule Matrix — single source of truth for every table Year End Data
/// Setup has an active responsibility for. No step should keep its own second, independent list.
/// </summary>
/// <remarks>
/// 43 entries: 40 <see cref="YearEndPrimaryRole.CopyToTargetYear"/> + 2
/// <see cref="YearEndPrimaryRole.TargetYearConfiguration"/> + 1 <see cref="YearEndPrimaryRole.CreateTargetYear"/>.
///
/// Scope rule: only tables where Year End has an active Data Setup responsibility are included here.
/// Tables Year End merely validates the existence of, tables that must stay empty, tables handled by
/// CutOver directly (not Data Setup), and tables Year End doesn't touch at all are out of scope for
/// this matrix — see the 2026-09-05 table audit for the full 113-table reconciliation.
///
/// Partitioning is deliberately not represented here: Year End does not create, alter, or manage
/// partitions, it assumes the required schema has already been deployed. Table/column existence
/// validation against a live schema is a separate concern from this matrix's job (declaring what Data
/// Setup's business-data actions are).
/// </remarks>
public static class YearEndTableRuleMatrix
{
    private const string Schema = "fps";

    public static IReadOnlyList<YearEndTableRuleMatrixEntry> Entries { get; } =
    [
        // 40 CopyToTargetYear entries (Table 23 + tbluser_category). CopyOrder = FK-dependency layer
        // (0-5), only meaningful for entries with no DedicatedStep (the generic copy mechanism uses it
        // for ordering; dedicated-step entries order via their own pipeline position instead).
        new(Schema, "costcentre", YearEndPrimaryRole.CopyToTargetYear, ["costcentre", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 0),
        new(Schema, "divisiongrade", YearEndPrimaryRole.CopyToTargetYear, ["divisiongrade", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 1),
        new(Schema, "grade", YearEndPrimaryRole.CopyToTargetYear, ["gradecode", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 0),
        new(Schema, "milestone", YearEndPrimaryRole.CopyToTargetYear, ["project", "milestoneref", "objectiveref", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 2),
        new(Schema, "plancatwggrade", YearEndPrimaryRole.CopyToTargetYear, ["plancategory", "wggrade", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 4),
        new(Schema, "profitcentregrade", YearEndPrimaryRole.CopyToTargetYear, ["pcgrade", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 2),
        new(Schema, "profitcentregrade_nondefra", YearEndPrimaryRole.CopyToTargetYear, ["pcgrade", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 2),
        new(Schema, "projectmonth2", YearEndPrimaryRole.CopyToTargetYear, ["project", "monthno", "fpsyear"], YearEndFinalValidationRule.MatchSource,
            CopyOrder: 0,
            Notes: "RecreateSummaries independently rebuilds this table's rows afterward — unrelated to Year End's copy."),
        new(Schema, "projectmonth3", YearEndPrimaryRole.CopyToTargetYear, ["endperiod", "project", "fpsyear"], YearEndFinalValidationRule.MatchSource,
            CopyOrder: 0,
            Notes: "Same RecreateSummaries relationship as projectmonth2."),
        new(Schema, "tbladditionalcosts", YearEndPrimaryRole.CopyToTargetYear, ["jobcode", "account", "description", "fpsyear"], YearEndFinalValidationRule.MatchSource,
            CopyOrder: 2,
            ResetPhase: YearEndResetPhase.ConfiguredPlanningReset,
            Overrides: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["itemcost"] = "0" },
            Notes: "Reset is CAP-conditional — ConfiguredPlanningResetStep gates the whole ConfiguredPlanningReset phase on fps.tblsettings.id='CapApprovalReceivedForReset' for the target year (YE-CAP-RESET)."),
        new(Schema, "tbladminusers", YearEndPrimaryRole.CopyToTargetYear, ["mnumber", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 0),
        new(Schema, "tblanimalreq", YearEndPrimaryRole.CopyToTargetYear, ["indcounter", "fpsyear"], YearEndFinalValidationRule.MatchSource,
            CopyOrder: 2,
            ResetPhase: YearEndResetPhase.ConfiguredPlanningReset,
            Overrides: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["numberofanimals"] = "0", ["numberofdays"] = "0" },
            Notes: "indcounter is GENERATED BY DEFAULT AS IDENTITY, excluded from copy by the generic is_identity='NO' column filter. Reset is CAP-conditional — ConfiguredPlanningResetStep gates the whole ConfiguredPlanningReset phase on fps.tblsettings.id='CapApprovalReceivedForReset' for the target year (YE-CAP-RESET)."),
        new(Schema, "tblanimals", YearEndPrimaryRole.CopyToTargetYear, ["animaltype", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 0),
        new(Schema, "tblcontract", YearEndPrimaryRole.CopyToTargetYear, ["contractno", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 0),
        new(Schema, "tblemployee", YearEndPrimaryRole.CopyToTargetYear, ["spnumber", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 0),
        new(Schema, "tblkpaccountcategory", YearEndPrimaryRole.CopyToTargetYear, ["accshortname", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 0),
        new(Schema, "tblperiod", YearEndPrimaryRole.CopyToTargetYear, ["periodname", "fpsyear"], YearEndFinalValidationRule.ExactTargetRowCount,
            DedicatedStep: nameof(PeriodSetupStep),
            ResetPhase: null,
            Overrides: null,
            Cleanup: null,
            ExpectedTargetRowCount: 12,
            Notes: "Reset (FinalSummariesRun=0, PeriodLocked=0) is applied internally by PeriodSetupStep itself, not via the generic Overrides mechanism. PeriodSetupStep enforces exactly 12 source/target rows itself; FinalValidationStep re-checks the same invariant via ExpectedTargetRowCount."),
        new(Schema, "tblstaffjob", YearEndPrimaryRole.CopyToTargetYear, ["staffid", "jobcode", "fpsyear"], YearEndFinalValidationRule.AtMostSource,
            CopyOrder: 5,
            ResetPhase: YearEndResetPhase.ConfiguredPlanningReset,
            Overrides: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["plannedhours"] = "0" },
            Cleanup: "Inactive-employee cleanup (InactiveEmployeeCleanupStep)",
            Notes: "FK-dependent on tblwgemployee (staffid) and tlkpproject (jobcode), hence CopyOrder 5. Reset is CAP-conditional — ConfiguredPlanningResetStep gates the whole ConfiguredPlanningReset phase on fps.tblsettings.id='CapApprovalReceivedForReset' for the target year (YE-CAP-RESET)."),
        new(Schema, "tbltestrccost", YearEndPrimaryRole.CopyToTargetYear, ["testcode", "profitcentre", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 1),
        new(Schema, "tbltestrequirementrccost", YearEndPrimaryRole.CopyToTargetYear, ["testcode", "buyer", "profitcentre", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 2),
        new(Schema, "tbltestreqwg", YearEndPrimaryRole.CopyToTargetYear, ["testcode", "buyer", "workgroup", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 0),
        new(Schema, "tbltotalbusinessoverheads", YearEndPrimaryRole.CopyToTargetYear, ["fpsyear"], YearEndFinalValidationRule.MatchSource,
            CopyOrder: 0,
            Notes: "Plain copy of the singleton row — totalbusinessoverheads is preserved as-is (including NULL), never forced to NULL."),
        new(Schema, "tbluser_category", YearEndPrimaryRole.CopyToTargetYear, ["user_id", "category", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 0),
        new(Schema, "tbluser_profitcentre", YearEndPrimaryRole.CopyToTargetYear, ["profitcentre", "user_id", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 0),
        new(Schema, "tbluser_program", YearEndPrimaryRole.CopyToTargetYear, ["programno", "user_id", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 0),
        new(Schema, "tbluser_projectgroup", YearEndPrimaryRole.CopyToTargetYear, ["projectgroup", "user_id", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 0),
        new(Schema, "tbluser_testowner", YearEndPrimaryRole.CopyToTargetYear, ["test_owner", "user_id", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 0),
        new(Schema, "tblwgemployee", YearEndPrimaryRole.CopyToTargetYear, ["pactid", "fpsyear"], YearEndFinalValidationRule.AtMostSource,
            CopyOrder: 4,
            Cleanup: "Inactive-employee cleanup (InactiveEmployeeCleanupStep)",
            Notes: "No reset. InactiveEmployeeCleanupStep deletes inactive-employee rows here too (not just tblstaffjob); tblstaffjob rows are removed first (FK dependency)."),
        new(Schema, "testorproduct", YearEndPrimaryRole.CopyToTargetYear, ["itemcode", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 0),
        new(Schema, "timecodevalid", YearEndPrimaryRole.CopyToTargetYear, ["workgroup", "timecode", "parentproject", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 2),
        new(Schema, "tlkpjobcode", YearEndPrimaryRole.CopyToTargetYear, ["jobcode", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 2),
        new(Schema, "tlkpmanager", YearEndPrimaryRole.CopyToTargetYear, ["manager", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 0),
        new(Schema, "tlkpprogram", YearEndPrimaryRole.CopyToTargetYear, ["programno", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 0),
        new(Schema, "tlkpprojectgroup", YearEndPrimaryRole.CopyToTargetYear, ["projectgroup", "fpsyear"], YearEndFinalValidationRule.MatchSource,
            CopyOrder: 0,
            Notes: "tlkpproject.projectgroup FKs here, hence CopyOrder 0 (before tlkpproject's CopyOrder 1). Year End owns and copies this year-scoped dependency directly instead of relying on external provisioning."),
        new(Schema, "tlkpproject", YearEndPrimaryRole.CopyToTargetYear, ["parentproject", "fpsyear"], YearEndFinalValidationRule.MatchSource,
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
            Notes: "FK-dependent on tblcontract and tlkpprogram (both NOT NULL columns), hence CopyOrder 1, not 0. Reset is unconditional, via its own step ProjectFinancialResetStep — NOT CAP-dependent, unlike the 4 ConfiguredPlanningResetStep tables."),
        new(Schema, "tlkptestcapability", YearEndPrimaryRole.CopyToTargetYear, ["testcode", "workgroup", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 2),
        new(Schema, "tlkptestreqmt", YearEndPrimaryRole.CopyToTargetYear, ["testcode", "buyer", "fpsyear"], YearEndFinalValidationRule.MatchSource,
            CopyOrder: 1,
            ResetPhase: YearEndResetPhase.ConfiguredPlanningReset,
            Overrides: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["norequired"] = "0" },
            Notes: "Reset is CAP-conditional — ConfiguredPlanningResetStep gates the whole ConfiguredPlanningReset phase on fps.tblsettings.id='CapApprovalReceivedForReset' for the target year (YE-CAP-RESET)."),
        new(Schema, "workgroup", YearEndPrimaryRole.CopyToTargetYear, ["workgroup", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 1),
        new(Schema, "workgroupgrade", YearEndPrimaryRole.CopyToTargetYear, ["wggrade", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 3),
        new(Schema, "workgroupmonth", YearEndPrimaryRole.CopyToTargetYear, ["workgroup", "month", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 0),

        // 2 TargetYearConfiguration entries — populated from Approve-frozen staging by a dedicated step,
        // not the generic copy mechanism.
        new(Schema, "tblsettings", YearEndPrimaryRole.TargetYearConfiguration, ["id", "fpsyear"], YearEndFinalValidationRule.AtLeastOneTargetYearRow,
            DedicatedStep: nameof(MaterializeYearEndConfigurationStep)),
        new(Schema, "tlkpmonthhours", YearEndPrimaryRole.TargetYearConfiguration, ["year", "month", "fpsyear"], YearEndFinalValidationRule.AtLeastOneTargetYearRow,
            DedicatedStep: nameof(MaterializeYearEndConfigurationStep)),

        // 1 CreateTargetYear entry — a genuinely new row, not a copy from the source year.
        new(Schema, "tblyearmaster", YearEndPrimaryRole.CreateTargetYear, ["fpsyear"], YearEndFinalValidationRule.ExactTargetRowCount,
            DedicatedStep: nameof(CreatePlannedYearStep),
            ExpectedTargetRowCount: 1,
            Notes: "CreatePlannedYearStep inserts fresh computed values (fpsyearcode, yearstatus, remarks, active, createdby) — never a copy of the source year's row. Confirmed live 2026-09-05: this table is NOT partitioned (plain relkind='r'), unlike every other entry in this matrix.")
    ];
}
