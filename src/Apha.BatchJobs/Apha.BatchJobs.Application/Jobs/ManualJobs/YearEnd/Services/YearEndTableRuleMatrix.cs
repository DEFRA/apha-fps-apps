using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Steps;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>What Year End Data Setup does for a table — reset, cleanup, and implementation are independent, orthogonal facts.</summary>
public enum YearEndPrimaryRole
{
    /// <summary>Copy the current year's rows to the target year.</summary>
    CopyToTargetYear,

    /// <summary>Populate target-year configuration from Approve-frozen staging (e.g. tblsettings/tlkpmonthhours).</summary>
    TargetYearConfiguration,

    /// <summary>Create the target year's row in fps.tblyearmaster — a new row, not a copy.</summary>
    CreateTargetYear
}

/// <summary>Expected post-execution outcome for a table, checked by <see cref="FinalValidationStep"/>.</summary>
public enum YearEndFinalValidationRule
{
    /// <summary>Target-year row count must equal source-year row count — the default for a plain copy.</summary>
    MatchSource,

    /// <summary>Target-year count must be at most source-year count — some rows are legitimately removed after copy (e.g. by <see cref="InactiveEmployeeCleanupStep"/>).</summary>
    AtMostSource,

    /// <summary>Target-year row count must equal <see cref="YearEndTableRuleMatrixEntry.ExpectedTargetRowCount"/> exactly.</summary>
    ExactTargetRowCount,

    /// <summary>Target-year row count must be at least one — the table exists and has been populated for the year, exact count doesn't matter.</summary>
    AtLeastOneTargetYearRow
}

/// <summary>Reset-phase names shared with <see cref="ProjectFinancialResetStep"/>/<see cref="ConfiguredPlanningResetStep"/> so they can't drift via a typo'd literal.</summary>
public static class YearEndResetPhase
{
    public const string ProjectFinancialReset = "ProjectFinancialReset";
    public const string ConfiguredPlanningReset = "ConfiguredPlanningReset";
}

/// <summary>One row of the Year End Table Rule Matrix — the single source of truth for a table's Data Setup responsibility.</summary>
/// <param name="PrimaryKeyColumns">Composite primary key, in column order. Must end with <c>"fpsyear"</c>.</param>
/// <param name="FinalValidation">Expected post-execution outcome, checked by <see cref="FinalValidationStep"/>.</param>
/// <param name="DedicatedStep">Step implementing <see cref="PrimaryRole"/> when it isn't the generic mechanism (<see cref="Steps.CopyFpsYearScopedTablesStep"/>); <c>null</c> means the generic mechanism handles it.</param>
/// <param name="CopyOrder">Dependency order among generic-mechanism <see cref="YearEndPrimaryRole.CopyToTargetYear"/> entries — lower copied first; <c>null</c> when <see cref="DedicatedStep"/> is set.</param>
/// <param name="ResetPhase">Which reset step applies <see cref="Overrides"/> to this table after copy; <c>null</c> if there's no column-level reset.</param>
/// <param name="Overrides">Column name -> literal SQL value applied via <c>UPDATE ... SET</c> to target-year rows by the matching <see cref="ResetPhase"/> step.</param>
/// <param name="Cleanup">Row-removal behaviour applied after copy, if any (currently only <see cref="InactiveEmployeeCleanupStep"/>'s inactive-employee removal).</param>
/// <param name="ExpectedTargetRowCount">Exact target-year row count required when <see cref="FinalValidation"/> is <see cref="YearEndFinalValidationRule.ExactTargetRowCount"/>.</param>
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
    int? ExpectedTargetRowCount = null);

/// <summary>Single source of truth for every table Year End Data Setup has an active responsibility for — no step keeps its own second list.</summary>
public static class YearEndTableRuleMatrix
{
    private const string Schema = "fps";

    public static IReadOnlyList<YearEndTableRuleMatrixEntry> Entries { get; } =
    [
        // CopyOrder = FK-dependency layer (0-5), meaningful only for entries with no DedicatedStep.
        new(Schema, "costcentre", YearEndPrimaryRole.CopyToTargetYear, ["costcentre", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 0),
        new(Schema, "divisiongrade", YearEndPrimaryRole.CopyToTargetYear, ["divisiongrade", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 1),
        new(Schema, "grade", YearEndPrimaryRole.CopyToTargetYear, ["gradecode", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 0),
        new(Schema, "milestone", YearEndPrimaryRole.CopyToTargetYear, ["project", "milestoneref", "objectiveref", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 2),
        new(Schema, "plancatwggrade", YearEndPrimaryRole.CopyToTargetYear, ["plancategory", "wggrade", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 4),
        new(Schema, "profitcentregrade", YearEndPrimaryRole.CopyToTargetYear, ["pcgrade", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 2),
        new(Schema, "profitcentregrade_nondefra", YearEndPrimaryRole.CopyToTargetYear, ["pcgrade", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 2),
        new(Schema, "projectmonth2", YearEndPrimaryRole.CopyToTargetYear, ["project", "monthno", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 0),
        new(Schema, "projectmonth3", YearEndPrimaryRole.CopyToTargetYear, ["endperiod", "project", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 0),
        new(Schema, "tbladditionalcosts", YearEndPrimaryRole.CopyToTargetYear, ["jobcode", "account", "description", "fpsyear"], YearEndFinalValidationRule.MatchSource,
            CopyOrder: 2,
            ResetPhase: YearEndResetPhase.ConfiguredPlanningReset,
            Overrides: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["itemcost"] = "0" }),
        new(Schema, "tbladminusers", YearEndPrimaryRole.CopyToTargetYear, ["mnumber", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 0),
        // indcounter is an identity column, excluded from copy automatically.
        new(Schema, "tblanimalreq", YearEndPrimaryRole.CopyToTargetYear, ["indcounter", "fpsyear"], YearEndFinalValidationRule.MatchSource,
            CopyOrder: 2,
            ResetPhase: YearEndResetPhase.ConfiguredPlanningReset,
            Overrides: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["numberofanimals"] = "0", ["numberofdays"] = "0" }),
        new(Schema, "tblanimals", YearEndPrimaryRole.CopyToTargetYear, ["animaltype", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 0),
        new(Schema, "tblcontract", YearEndPrimaryRole.CopyToTargetYear, ["contractno", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 0),
        new(Schema, "tblemployee", YearEndPrimaryRole.CopyToTargetYear, ["spnumber", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 0),
        new(Schema, "tblkpaccountcategory", YearEndPrimaryRole.CopyToTargetYear, ["accshortname", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 0),
        // Reset (FinalSummariesRun=0, PeriodLocked=0) happens inside PeriodSetupStep itself, not via Overrides.
        new(Schema, "tblperiod", YearEndPrimaryRole.CopyToTargetYear, ["periodname", "fpsyear"], YearEndFinalValidationRule.ExactTargetRowCount,
            DedicatedStep: nameof(PeriodSetupStep),
            ExpectedTargetRowCount: 12),
        new(Schema, "tblstaffjob", YearEndPrimaryRole.CopyToTargetYear, ["staffid", "jobcode", "fpsyear"], YearEndFinalValidationRule.AtMostSource,
            CopyOrder: 5,
            ResetPhase: YearEndResetPhase.ConfiguredPlanningReset,
            Overrides: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["plannedhours"] = "0" },
            Cleanup: "Inactive-employee cleanup (InactiveEmployeeCleanupStep)"),
        new(Schema, "tbltestrccost", YearEndPrimaryRole.CopyToTargetYear, ["testcode", "profitcentre", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 1),
        new(Schema, "tbltestrequirementrccost", YearEndPrimaryRole.CopyToTargetYear, ["testcode", "buyer", "profitcentre", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 2),
        new(Schema, "tbltestreqwg", YearEndPrimaryRole.CopyToTargetYear, ["testcode", "buyer", "workgroup", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 0),
        new(Schema, "tbltotalbusinessoverheads", YearEndPrimaryRole.CopyToTargetYear, ["fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 0),
        new(Schema, "tbluser_category", YearEndPrimaryRole.CopyToTargetYear, ["user_id", "category", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 0),
        new(Schema, "tbluser_profitcentre", YearEndPrimaryRole.CopyToTargetYear, ["profitcentre", "user_id", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 0),
        new(Schema, "tbluser_program", YearEndPrimaryRole.CopyToTargetYear, ["programno", "user_id", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 0),
        new(Schema, "tbluser_projectgroup", YearEndPrimaryRole.CopyToTargetYear, ["projectgroup", "user_id", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 0),
        new(Schema, "tbluser_testowner", YearEndPrimaryRole.CopyToTargetYear, ["test_owner", "user_id", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 0),
        new(Schema, "tblwgemployee", YearEndPrimaryRole.CopyToTargetYear, ["pactid", "fpsyear"], YearEndFinalValidationRule.AtMostSource,
            CopyOrder: 4,
            Cleanup: "Inactive-employee cleanup (InactiveEmployeeCleanupStep)"),
        new(Schema, "testorproduct", YearEndPrimaryRole.CopyToTargetYear, ["itemcode", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 0),
        new(Schema, "timecodevalid", YearEndPrimaryRole.CopyToTargetYear, ["workgroup", "timecode", "parentproject", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 2),
        new(Schema, "tlkpjobcode", YearEndPrimaryRole.CopyToTargetYear, ["jobcode", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 2),
        new(Schema, "tlkpmanager", YearEndPrimaryRole.CopyToTargetYear, ["manager", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 0),
        new(Schema, "tlkpprogram", YearEndPrimaryRole.CopyToTargetYear, ["programno", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 0),
        new(Schema, "tlkpprojectgroup", YearEndPrimaryRole.CopyToTargetYear, ["projectgroup", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 0),
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
            }),
        new(Schema, "tlkptestcapability", YearEndPrimaryRole.CopyToTargetYear, ["testcode", "workgroup", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 2),
        new(Schema, "tlkptestreqmt", YearEndPrimaryRole.CopyToTargetYear, ["testcode", "buyer", "fpsyear"], YearEndFinalValidationRule.MatchSource,
            CopyOrder: 1,
            ResetPhase: YearEndResetPhase.ConfiguredPlanningReset,
            Overrides: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["norequired"] = "0" }),
        new(Schema, "workgroup", YearEndPrimaryRole.CopyToTargetYear, ["workgroup", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 1),
        new(Schema, "workgroupgrade", YearEndPrimaryRole.CopyToTargetYear, ["wggrade", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 3),
        new(Schema, "workgroupmonth", YearEndPrimaryRole.CopyToTargetYear, ["workgroup", "month", "fpsyear"], YearEndFinalValidationRule.MatchSource, CopyOrder: 0),

        // TargetYearConfiguration entries — populated from Approve-frozen staging, not the generic copy mechanism.
        new(Schema, "tblsettings", YearEndPrimaryRole.TargetYearConfiguration, ["id", "fpsyear"], YearEndFinalValidationRule.AtLeastOneTargetYearRow,
            DedicatedStep: nameof(MaterializeYearEndConfigurationStep)),
        new(Schema, "tlkpmonthhours", YearEndPrimaryRole.TargetYearConfiguration, ["year", "month", "fpsyear"], YearEndFinalValidationRule.AtLeastOneTargetYearRow,
            DedicatedStep: nameof(MaterializeYearEndConfigurationStep)),

        // 1 CreateTargetYear entry — a genuinely new row, not a copy from the source year.
        new(Schema, "tblyearmaster", YearEndPrimaryRole.CreateTargetYear, ["fpsyear"], YearEndFinalValidationRule.ExactTargetRowCount,
            DedicatedStep: nameof(CreatePlannedYearStep),
            ExpectedTargetRowCount: 1)
    ];
}
