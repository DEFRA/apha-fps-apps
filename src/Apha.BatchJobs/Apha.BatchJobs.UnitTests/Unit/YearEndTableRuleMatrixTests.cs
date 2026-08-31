using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

namespace Apha.BatchJobs.UnitTests;

public sealed class YearEndTableRuleMatrixTests
{
    /// <summary>
    /// Every (referencing, referenced) pair found by a read-only FK/dependency scan of
    /// batchjob_testing where both sides are CopyToTargetYear matrix entries — i.e. the referencing
    /// table's target-year row cannot exist until the referenced table's target-year row does.
    /// </summary>
    private static readonly (string Referencing, string Referenced)[] KnownFkEdges =
    [
        ("divisiongrade", "grade"),
        ("milestone", "tlkpproject"),
        ("plancatwggrade", "workgroupgrade"),
        ("profitcentregrade", "divisiongrade"),
        ("profitcentregrade", "grade"),
        ("profitcentregrade_nondefra", "divisiongrade"),
        ("profitcentregrade_nondefra", "grade"),
        ("tbladditionalcosts", "tblkpaccountcategory"),
        ("tbladditionalcosts", "tlkpproject"),
        ("tblanimalreq", "tblanimals"),
        ("tblanimalreq", "tlkpproject"),
        ("tblstaffjob", "tblwgemployee"),
        ("tblstaffjob", "tlkpproject"),
        ("tbltestrccost", "testorproduct"),
        ("tbltestrequirementrccost", "tbltestrccost"),
        ("tbltestrequirementrccost", "tlkptestreqmt"),
        ("tblwgemployee", "tblemployee"),
        ("tblwgemployee", "workgroupgrade"),
        ("timecodevalid", "tlkpproject"),
        ("tlkpjobcode", "tlkpproject"),
        ("tlkpproject", "tblcontract"),
        ("tlkpproject", "tlkpprogram"),
        ("tlkptestcapability", "testorproduct"),
        ("tlkptestcapability", "tlkpproject"),
        ("tlkptestcapability", "workgroup"),
        ("tlkptestreqmt", "testorproduct"),
        ("workgroup", "costcentre"),
        ("workgroupgrade", "grade"),
        ("workgroupgrade", "profitcentregrade"),
        ("workgroupgrade", "workgroup"),
    ];

    [Fact]
    public void Entries_ShouldContainExactlyTheExpectedCountPerRole()
    {
        var entries = YearEndTableRuleMatrix.Entries;

        Assert.Equal(65, entries.Count);
        Assert.Equal(38, entries.Count(e => e.Role == YearEndTableRole.YearScopedBusinessParticipant));
        Assert.Equal(3, entries.Count(e => e.Role == YearEndTableRole.YearScopedConfigurationDependency));
        Assert.Equal(3, entries.Count(e => e.Role == YearEndTableRole.GlobalReference));
        Assert.Equal(21, entries.Count(e => e.Role == YearEndTableRole.YearScopedTargetMustBeEmpty));
    }

    [Fact]
    public void Entries_ShouldNotContainDuplicateTables()
    {
        var duplicates = YearEndTableRuleMatrix.Entries
            .GroupBy(e => (e.Schema, e.TableName))
            .Where(g => g.Count() > 1)
            .ToList();

        Assert.Empty(duplicates);
    }

    [Fact]
    public void NoEntry_ShouldUseCreateTargetYearRow()
    {
        var entries = YearEndTableRuleMatrix.Entries
            .Where(e => e.Action == YearEndTableRuleAction.CreateTargetYearRow)
            .ToList();

        Assert.Empty(entries);
    }

    [Fact]
    public void NoEntry_ShouldRemainPendingClassification()
    {
        var entries = YearEndTableRuleMatrix.Entries
            .Where(e => e.Action == YearEndTableRuleAction.PendingClassification)
            .ToList();

        Assert.Empty(entries);
    }

    [Fact]
    public void TblTotalBusinessOverheads_ShouldBeCopyToTargetYear()
    {
        var entry = YearEndTableRuleMatrix.Entries.Single(e => e.TableName == "tbltotalbusinessoverheads");

        Assert.Equal(YearEndTableRuleAction.CopyToTargetYear, entry.Action);
        Assert.Equal(YearEndTableRole.YearScopedBusinessParticipant, entry.Role);
        Assert.Equal(["fpsyear"], entry.PrimaryKeyColumns);
        Assert.NotNull(entry.CopyOrder);
        Assert.Null(entry.ResetPhase);
    }

    [Fact]
    public void TblPeriod_ShouldBeTheOnlyAlreadyImplementedViaDedicatedStepEntry()
    {
        var entries = YearEndTableRuleMatrix.Entries
            .Where(e => e.Action == YearEndTableRuleAction.AlreadyImplementedViaDedicatedStep)
            .ToList();

        var entry = Assert.Single(entries);
        Assert.Equal("tblperiod", entry.TableName);
        Assert.Null(entry.CopyOrder);
    }

    [Fact]
    public void GlobalAndConfigurationDependencyEntries_ShouldAllUseValidateExists()
    {
        var entries = YearEndTableRuleMatrix.Entries
            .Where(e => e.Role is YearEndTableRole.GlobalReference or YearEndTableRole.YearScopedConfigurationDependency)
            .ToList();

        Assert.Equal(6, entries.Count);
        Assert.All(entries, e => Assert.Equal(YearEndTableRuleAction.ValidateExists, e.Action));
        Assert.All(entries, e => Assert.Null(e.CopyOrder));
    }

    [Fact]
    public void TlkpProjectGroup_ShouldBeAYearScopedConfigurationDependency()
    {
        var entry = YearEndTableRuleMatrix.Entries.Single(e => e.TableName == "tlkpprojectgroup");

        Assert.Equal(YearEndTableRole.YearScopedConfigurationDependency, entry.Role);
        Assert.Equal(YearEndTableRuleAction.ValidateExists, entry.Action);
        Assert.Equal(["projectgroup", "fpsyear"], entry.PrimaryKeyColumns);
    }

    [Fact]
    public void GlobalReferenceEntries_ShouldBeExactlyTheThreeConfirmedTables()
    {
        var globalTableNames = YearEndTableRuleMatrix.Entries
            .Where(e => e.Role == YearEndTableRole.GlobalReference)
            .Select(e => e.TableName)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToList();

        Assert.Equal(["tblcategory", "tblkpprofitcentre", "tblusers"], globalTableNames);
    }

    [Fact]
    public void PartitionedRoles_ShouldAllHavePrimaryKeyEndingInFpsYear()
    {
        var partitionedEntries = YearEndTableRuleMatrix.Entries
            .Where(e => e.Role is YearEndTableRole.YearScopedBusinessParticipant
                or YearEndTableRole.YearScopedConfigurationDependency
                or YearEndTableRole.YearScopedTargetMustBeEmpty)
            .ToList();

        Assert.Equal(62, partitionedEntries.Count);
        Assert.All(partitionedEntries, e =>
        {
            Assert.NotEmpty(e.PrimaryKeyColumns);
            Assert.Equal("fpsyear", e.PrimaryKeyColumns[^1]);
        });
    }

    [Fact]
    public void GlobalReferenceEntries_ShouldHaveNoPrimaryKeyColumnsRecorded()
    {
        var entries = YearEndTableRuleMatrix.Entries
            .Where(e => e.Role == YearEndTableRole.GlobalReference)
            .ToList();

        Assert.All(entries, e => Assert.Empty(e.PrimaryKeyColumns));
    }

    [Fact]
    public void CopyToTargetYearEntries_ShouldNumberThirtySevenAndAllHaveCopyOrder()
    {
        var copyEntries = YearEndTableRuleMatrix.Entries
            .Where(e => e.Action == YearEndTableRuleAction.CopyToTargetYear)
            .ToList();

        Assert.Equal(37, copyEntries.Count);
        Assert.All(copyEntries, e => Assert.NotNull(e.CopyOrder));
    }

    [Fact]
    public void NonCopyToTargetYearEntries_ShouldNotHaveCopyOrder()
    {
        var nonCopyEntries = YearEndTableRuleMatrix.Entries
            .Where(e => e.Action != YearEndTableRuleAction.CopyToTargetYear)
            .ToList();

        Assert.All(nonCopyEntries, e => Assert.Null(e.CopyOrder));
    }

    [Fact]
    public void ExactlyFiveEntries_ShouldHaveResetPhaseAndOverrides()
    {
        var resetEntries = YearEndTableRuleMatrix.Entries
            .Where(e => e.ResetPhase is not null)
            .ToList();

        var resetTableNames = resetEntries.Select(e => e.TableName).OrderBy(n => n, StringComparer.Ordinal).ToList();
        Assert.Equal(
            new[] { "tblanimalreq", "tbladditionalcosts", "tblstaffjob", "tlkpproject", "tlkptestreqmt" }
                .OrderBy(n => n, StringComparer.Ordinal),
            resetTableNames);

        Assert.All(resetEntries, e =>
        {
            Assert.Equal(YearEndTableRuleAction.CopyToTargetYear, e.Action);
            Assert.NotNull(e.Overrides);
            Assert.NotEmpty(e.Overrides!);
        });
    }

    [Fact]
    public void TlkpProject_ShouldUseProjectFinancialResetPhase()
    {
        var entry = YearEndTableRuleMatrix.Entries.Single(e => e.TableName == "tlkpproject");

        Assert.Equal(YearEndResetPhase.ProjectFinancialReset, entry.ResetPhase);
        Assert.Equal(11, entry.Overrides!.Count);
    }

    [Fact]
    public void FourPlanningTables_ShouldUseConfiguredPlanningResetPhase()
    {
        var entries = YearEndTableRuleMatrix.Entries
            .Where(e => e.ResetPhase == YearEndResetPhase.ConfiguredPlanningReset)
            .Select(e => e.TableName)
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToList();

        Assert.Equal(
            new[] { "tblanimalreq", "tbladditionalcosts", "tblstaffjob", "tlkptestreqmt" }
                .OrderBy(n => n, StringComparer.Ordinal),
            entries);
    }

    [Fact]
    public void NoResetEntry_ShouldTargetMabArchive()
    {
        var resetEntries = YearEndTableRuleMatrix.Entries.Where(e => e.ResetPhase is not null);

        Assert.All(resetEntries, e => Assert.Equal("fps", e.Schema, StringComparer.OrdinalIgnoreCase));
    }

    [Fact]
    public void TblStaffJobAndTblWgEmployee_ShouldBeTheOnlyAtMostSourceEntries()
    {
        // InactiveEmployeeCleanupStep (redesigned 2026-08-14 around the legacy
        // Annual_WGEmployeeList.sql rule) deletes target-year rows from both tables: tblstaffjob
        // first (FK dependency), then tblwgemployee itself.
        var atMostSourceTableNames = YearEndTableRuleMatrix.Entries
            .Where(e => e.FinalRowCountRule == YearEndFinalRowCountRule.AtMostSource)
            .Select(e => e.TableName)
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToList();

        Assert.Equal(
            new[] { "tblstaffjob", "tblwgemployee" }.OrderBy(n => n, StringComparer.Ordinal),
            atMostSourceTableNames);
    }

    [Fact]
    public void CopyToTargetYearEntries_ShouldAllHaveAFinalRowCountRule()
    {
        var copyEntries = YearEndTableRuleMatrix.Entries
            .Where(e => e.Action == YearEndTableRuleAction.CopyToTargetYear)
            .ToList();

        Assert.All(copyEntries, e => Assert.NotEqual(YearEndFinalRowCountRule.NotApplicable, e.FinalRowCountRule));
    }

    [Fact]
    public void NonCopyToTargetYearEntries_ShouldHaveNotApplicableFinalRowCountRule()
    {
        var nonCopyEntries = YearEndTableRuleMatrix.Entries
            .Where(e => e.Action != YearEndTableRuleAction.CopyToTargetYear)
            .ToList();

        Assert.All(nonCopyEntries, e => Assert.Equal(YearEndFinalRowCountRule.NotApplicable, e.FinalRowCountRule));
    }

    [Fact]
    public void YearScopedTargetMustBeEmptyEntries_ShouldAllUseTargetYearMustBeEmpty()
    {
        var entries = YearEndTableRuleMatrix.Entries
            .Where(e => e.Role == YearEndTableRole.YearScopedTargetMustBeEmpty)
            .ToList();

        Assert.Equal(21, entries.Count);
        Assert.All(entries, e => Assert.Equal(YearEndTableRuleAction.TargetYearMustBeEmpty, e.Action));
        Assert.All(entries, e => Assert.Equal("fps", e.Schema, StringComparer.OrdinalIgnoreCase));
    }

    [Fact]
    public void YearScopedTargetMustBeEmptyEntries_ShouldBeExactlyTheTwentyOneKnownSectionNineteenTables()
    {
        var tableNames = YearEndTableRuleMatrix.Entries
            .Where(e => e.Role == YearEndTableRole.YearScopedTargetMustBeEmpty)
            .Select(e => e.TableName)
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToList();

        var expected = new[]
        {
            "additionalcosts_log", "animalreq_log", "fpsyeartotals", "mo_log", "monthlyoutput",
            "monthlytime", "mt_log", "proj_invoice", "proj_subcontract", "project_log",
            "projectmonth", "projectmonthfinal", "recreatesummaries_log", "staffjob_log", "tblbid",
            "tblpurchase", "tblsurvff_fees", "tblsurvff_submissions", "tbltestreqbaseline",
            "testreq_log", "timecostcalcs"
        }.OrderBy(n => n, StringComparer.Ordinal);

        Assert.Equal(expected, tableNames);
    }

    [Fact]
    public void TargetYearMustBeEmptyEntries_ShouldNotOverlapCopyToTargetYearEntries()
    {
        var copyTableNames = YearEndTableRuleMatrix.Entries
            .Where(e => e.Action == YearEndTableRuleAction.CopyToTargetYear)
            .Select(e => e.TableName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var mustBeEmptyTableNames = YearEndTableRuleMatrix.Entries
            .Where(e => e.Action == YearEndTableRuleAction.TargetYearMustBeEmpty)
            .Select(e => e.TableName);

        Assert.All(mustBeEmptyTableNames, name => Assert.DoesNotContain(name, copyTableNames));
    }

    [Fact]
    public void KnownFkEdges_ShouldSatisfyCopyOrderInvariant()
    {
        var copyOrderByTable = YearEndTableRuleMatrix.Entries
            .Where(e => e.Action == YearEndTableRuleAction.CopyToTargetYear)
            .ToDictionary(e => e.TableName, e => e.CopyOrder!.Value, StringComparer.OrdinalIgnoreCase);

        foreach (var (referencing, referenced) in KnownFkEdges)
        {
            Assert.True(copyOrderByTable.ContainsKey(referencing), $"'{referencing}' is missing from the CopyToTargetYear entries.");
            Assert.True(copyOrderByTable.ContainsKey(referenced), $"'{referenced}' is missing from the CopyToTargetYear entries.");

            Assert.True(
                copyOrderByTable[referenced] < copyOrderByTable[referencing],
                $"Expected CopyOrder({referenced})={copyOrderByTable[referenced]} < CopyOrder({referencing})={copyOrderByTable[referencing]} " +
                $"— '{referencing}' has a foreign key to '{referenced}' and must copy after it.");
        }
    }
}
