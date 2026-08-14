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
    /// read by <see cref="ValidateYearEndConfigurationStep"/>.
    /// </summary>
    YearScopedConfigurationDependency,

    /// <summary>Not year-scoped at all — no target-year row concept, no partition to validate.</summary>
    GlobalReference
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
    ClearTargetYearRows,
    ValidateExists,
    SkipLegacyObsolete,
    ManualReviewRequired
}

/// <summary>
/// One row of the Year End Table Rule Matrix — the single source of truth for which tables Year
/// End is aware of, how each relates to the FPS year, and what (if anything) has been approved to
/// happen to it. Consumed by <see cref="ValidateYearScopedSchemaStep"/> now (partition-existence
/// validation) and by the Phase 2 copy/create/reset implementation and
/// <see cref="FinalValidationStep"/> later.
/// </summary>
public sealed record YearEndTableRuleMatrixEntry(
    string Schema,
    string TableName,
    YearEndTableRole Role,
    YearEndTableRuleAction Action,
    IReadOnlyList<string> PrimaryKeyColumns,
    string? Notes = null);

/// <summary>
/// The Year End Table Rule Matrix. All 38 Table 23 entries, the 2 year-scoped configuration
/// dependencies, and the 3 global/reference participants — every table Year End's schema
/// validation and (eventually) business-data steps are aware of.
/// </summary>
public static class YearEndTableRuleMatrix
{
    private const string Schema = "fps";

    public static IReadOnlyList<YearEndTableRuleMatrixEntry> Entries { get; } =
    [
        // 38 year-scoped business participants (Table 23).
        new(Schema, "costcentre", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.PendingClassification, ["costcentre", "fpsyear"]),
        new(Schema, "divisiongrade", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.PendingClassification, ["divisiongrade", "fpsyear"]),
        new(Schema, "grade", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.PendingClassification, ["gradecode", "fpsyear"]),
        new(Schema, "milestone", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.PendingClassification, ["project", "milestoneref", "objectiveref", "fpsyear"]),
        new(Schema, "plancatwggrade", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.PendingClassification, ["plancategory", "wggrade", "fpsyear"]),
        new(Schema, "profitcentregrade", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.PendingClassification, ["pcgrade", "fpsyear"]),
        new(Schema, "profitcentregrade_nondefra", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.PendingClassification, ["pcgrade", "fpsyear"]),
        new(Schema, "projectmonth2", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.PendingClassification, ["project", "monthno", "fpsyear"]),
        new(Schema, "projectmonth3", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.PendingClassification, ["endperiod", "project", "fpsyear"]),
        new(Schema, "tbladditionalcosts", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.PendingClassification, ["jobcode", "account", "description", "fpsyear"]),
        new(Schema, "tbladminusers", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.PendingClassification, ["mnumber", "fpsyear"]),
        new(Schema, "tblanimalreq", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.PendingClassification, ["indcounter", "fpsyear"]),
        new(Schema, "tblanimals", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.PendingClassification, ["animaltype", "fpsyear"]),
        new(Schema, "tblcontract", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.PendingClassification, ["contractno", "fpsyear"]),
        new(Schema, "tblemployee", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.PendingClassification, ["spnumber", "fpsyear"]),
        new(Schema, "tblkpaccountcategory", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.PendingClassification, ["accshortname", "fpsyear"]),
        new(Schema, "tblperiod", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.AlreadyImplementedViaDedicatedStep, ["periodname", "fpsyear"],
            Notes: "Implemented via PeriodSetupStep (copy + period-lock reset); Phase 3 adds exactly-12-target-year-periods validation. Not part of the generic Phase 2 copy mechanism."),
        new(Schema, "tblstaffjob", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.PendingClassification, ["staffid", "jobcode", "fpsyear"]),
        new(Schema, "tbltestrccost", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.PendingClassification, ["testcode", "profitcentre", "fpsyear"]),
        new(Schema, "tbltestrequirementrccost", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.PendingClassification, ["testcode", "buyer", "profitcentre", "fpsyear"]),
        new(Schema, "tbltestreqwg", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.PendingClassification, ["testcode", "buyer", "workgroup", "fpsyear"]),
        new(Schema, "tbltotalbusinessoverheads", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.CreateTargetYearRow, ["fpsyear"],
            Notes: "Create exactly one row for plannedYear with totalbusinessoverheads = NULL; do not copy the previous year's value. Confirmed 2026-08-14."),
        new(Schema, "tbluser_profitcentre", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.PendingClassification, ["profitcentre", "user_id", "fpsyear"]),
        new(Schema, "tbluser_program", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.PendingClassification, ["programno", "user_id", "fpsyear"]),
        new(Schema, "tbluser_projectgroup", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.PendingClassification, ["projectgroup", "user_id", "fpsyear"]),
        new(Schema, "tbluser_testowner", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.PendingClassification, ["test_owner", "user_id", "fpsyear"]),
        new(Schema, "tblwgemployee", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.PendingClassification, ["pactid", "fpsyear"]),
        new(Schema, "testorproduct", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.PendingClassification, ["itemcode", "fpsyear"]),
        new(Schema, "timecodevalid", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.PendingClassification, ["workgroup", "timecode", "parentproject", "fpsyear"]),
        new(Schema, "tlkpjobcode", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.PendingClassification, ["jobcode", "fpsyear"]),
        new(Schema, "tlkpmanager", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.PendingClassification, ["manager", "fpsyear"]),
        new(Schema, "tlkpprogram", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.PendingClassification, ["programno", "fpsyear"]),
        new(Schema, "tlkpproject", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.PendingClassification, ["parentproject", "fpsyear"]),
        new(Schema, "tlkptestcapability", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.PendingClassification, ["testcode", "workgroup", "fpsyear"]),
        new(Schema, "tlkptestreqmt", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.PendingClassification, ["testcode", "buyer", "fpsyear"]),
        new(Schema, "workgroup", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.PendingClassification, ["workgroup", "fpsyear"]),
        new(Schema, "workgroupgrade", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.PendingClassification, ["wggrade", "fpsyear"]),
        new(Schema, "workgroupmonth", YearEndTableRole.YearScopedBusinessParticipant, YearEndTableRuleAction.PendingClassification, ["workgroup", "month", "fpsyear"]),

        // 2 year-scoped configuration dependencies — not Table 23, but still partitioned by fpsyear.
        new(Schema, "tblsettings", YearEndTableRole.YearScopedConfigurationDependency, YearEndTableRuleAction.ValidateExists, ["id", "fpsyear"]),
        new(Schema, "tlkpmonthhours", YearEndTableRole.YearScopedConfigurationDependency, YearEndTableRuleAction.ValidateExists, ["year", "month", "fpsyear"]),

        // 3 global/reference participants — not year-scoped, no target-year row concept.
        new(Schema, "tblusers", YearEndTableRole.GlobalReference, YearEndTableRuleAction.ValidateExists, []),
        new(Schema, "tblcategory", YearEndTableRole.GlobalReference, YearEndTableRuleAction.ValidateExists, []),
        new(Schema, "tblkpprofitcentre", YearEndTableRole.GlobalReference, YearEndTableRuleAction.ValidateExists, [])
    ];
}
