using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;
using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Steps;

namespace Apha.BatchJobs.UnitTests;

public sealed class YearEndTableRuleMatrixTests
{
    /// <summary>
    /// Every (referencing, referenced) pair found by a read-only FK/dependency scan of
    /// batchjob_testing where both sides are generic-mechanism CopyToTargetYear matrix entries — i.e.
    /// the referencing table's target-year row cannot exist until the referenced table's target-year
    /// row does.
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
        ("tlkpproject", "tlkpprojectgroup"),
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
    public void Entries_ShouldContainExactlyTheExpectedCountPerPrimaryRole()
    {
        var entries = YearEndTableRuleMatrix.Entries;

        Assert.Equal(43, entries.Count);
        Assert.Equal(40, entries.Count(e => e.PrimaryRole == YearEndPrimaryRole.CopyToTargetYear));
        Assert.Equal(2, entries.Count(e => e.PrimaryRole == YearEndPrimaryRole.TargetYearConfiguration));
        Assert.Equal(1, entries.Count(e => e.PrimaryRole == YearEndPrimaryRole.CreateTargetYear));
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
    public void TblTotalBusinessOverheads_ShouldBeCopyToTargetYear()
    {
        var entry = YearEndTableRuleMatrix.Entries.Single(e => e.TableName == "tbltotalbusinessoverheads");

        Assert.Equal(YearEndPrimaryRole.CopyToTargetYear, entry.PrimaryRole);
        Assert.Equal(["fpsyear"], entry.PrimaryKeyColumns);
        Assert.NotNull(entry.CopyOrder);
        Assert.Null(entry.ResetPhase);
    }

    [Fact]
    public void TblPeriod_ShouldBeTheOnlyCopyToTargetYearEntryWithADedicatedStep()
    {
        var entries = YearEndTableRuleMatrix.Entries
            .Where(e => e.PrimaryRole == YearEndPrimaryRole.CopyToTargetYear && e.DedicatedStep is not null)
            .ToList();

        var entry = Assert.Single(entries);
        Assert.Equal("tblperiod", entry.TableName);
        Assert.Equal(nameof(PeriodSetupStep), entry.DedicatedStep);
        Assert.Null(entry.CopyOrder);
        Assert.Equal(YearEndFinalValidationRule.ExactTargetRowCount, entry.FinalValidation);
        Assert.Equal(12, entry.ExpectedTargetRowCount);
    }

    [Fact]
    public void TargetYearConfigurationAndCreateTargetYearEntries_ShouldAllHaveADedicatedStep()
    {
        var entries = YearEndTableRuleMatrix.Entries
            .Where(e => e.PrimaryRole is YearEndPrimaryRole.TargetYearConfiguration or YearEndPrimaryRole.CreateTargetYear)
            .ToList();

        Assert.Equal(3, entries.Count);
        Assert.All(entries, e => Assert.NotNull(e.DedicatedStep));
        Assert.All(entries, e => Assert.Null(e.CopyOrder));
    }

    [Fact]
    public void TlkpProjectGroup_ShouldBeCopyToTargetYearWithNoDedicatedStep()
    {
        var entry = YearEndTableRuleMatrix.Entries.Single(e => e.TableName == "tlkpprojectgroup");

        Assert.Equal(YearEndPrimaryRole.CopyToTargetYear, entry.PrimaryRole);
        Assert.Null(entry.DedicatedStep);
        Assert.Equal(["projectgroup", "fpsyear"], entry.PrimaryKeyColumns);
        Assert.Equal(0, entry.CopyOrder);
        Assert.Equal(YearEndFinalValidationRule.MatchSource, entry.FinalValidation);
    }

    [Fact]
    public void TblYearMaster_ShouldBeCreateTargetYearWithExactlyOneExpectedRow()
    {
        var entry = YearEndTableRuleMatrix.Entries.Single(e => e.TableName == "tblyearmaster");

        Assert.Equal(YearEndPrimaryRole.CreateTargetYear, entry.PrimaryRole);
        Assert.Equal(nameof(CreatePlannedYearStep), entry.DedicatedStep);
        Assert.Equal(["fpsyear"], entry.PrimaryKeyColumns);
        Assert.Equal(YearEndFinalValidationRule.ExactTargetRowCount, entry.FinalValidation);
        Assert.Equal(1, entry.ExpectedTargetRowCount);
    }

    [Fact]
    public void TblSettingsAndTlkpMonthHours_ShouldBeTargetYearConfigurationWithAtLeastOneRowExpected()
    {
        var entries = YearEndTableRuleMatrix.Entries
            .Where(e => e.PrimaryRole == YearEndPrimaryRole.TargetYearConfiguration)
            .ToList();

        var tableNames = entries.Select(e => e.TableName).OrderBy(n => n, StringComparer.Ordinal).ToList();
        Assert.Equal(new[] { "tblsettings", "tlkpmonthhours" }.OrderBy(n => n, StringComparer.Ordinal), tableNames);
        Assert.All(entries, e => Assert.Equal(nameof(MaterializeYearEndConfigurationStep), e.DedicatedStep));
        Assert.All(entries, e => Assert.Equal(YearEndFinalValidationRule.AtLeastOneTargetYearRow, e.FinalValidation));
    }

    [Fact]
    public void AllEntries_ShouldHavePrimaryKeyEndingInFpsYear()
    {
        var entries = YearEndTableRuleMatrix.Entries;

        Assert.All(entries, e =>
        {
            Assert.NotEmpty(e.PrimaryKeyColumns);
            Assert.Equal("fpsyear", e.PrimaryKeyColumns[^1]);
        });
    }

    [Fact]
    public void GenericCopyToTargetYearEntries_ShouldNumberThirtyNineAndAllHaveCopyOrder()
    {
        var copyEntries = YearEndTableRuleMatrix.Entries
            .Where(e => e.PrimaryRole == YearEndPrimaryRole.CopyToTargetYear && e.DedicatedStep is null)
            .ToList();

        Assert.Equal(39, copyEntries.Count);
        Assert.All(copyEntries, e => Assert.NotNull(e.CopyOrder));
    }

    [Fact]
    public void EntriesOutsideTheGenericCopyMechanism_ShouldNotHaveCopyOrder()
    {
        var nonGenericCopyEntries = YearEndTableRuleMatrix.Entries
            .Where(e => e.PrimaryRole != YearEndPrimaryRole.CopyToTargetYear || e.DedicatedStep is not null)
            .ToList();

        Assert.All(nonGenericCopyEntries, e => Assert.Null(e.CopyOrder));
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
            Assert.Equal(YearEndPrimaryRole.CopyToTargetYear, e.PrimaryRole);
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
    public void TblStaffJobAndTblWgEmployee_ShouldBeTheOnlyAtMostSourceEntriesAndTheOnlyCleanupEntries()
    {
        // InactiveEmployeeCleanupStep (redesigned 2026-08-14 around the legacy
        // Annual_WGEmployeeList.sql rule) deletes target-year rows from both tables: tblstaffjob
        // first (FK dependency), then tblwgemployee itself.
        var atMostSourceEntries = YearEndTableRuleMatrix.Entries
            .Where(e => e.FinalValidation == YearEndFinalValidationRule.AtMostSource)
            .ToList();

        var atMostSourceTableNames = atMostSourceEntries.Select(e => e.TableName).OrderBy(n => n, StringComparer.Ordinal).ToList();
        Assert.Equal(
            new[] { "tblstaffjob", "tblwgemployee" }.OrderBy(n => n, StringComparer.Ordinal),
            atMostSourceTableNames);
        Assert.All(atMostSourceEntries, e => Assert.NotNull(e.Cleanup));

        var cleanupEntries = YearEndTableRuleMatrix.Entries.Where(e => e.Cleanup is not null).ToList();
        Assert.Equal(2, cleanupEntries.Count);
    }

    [Fact]
    public void KnownFkEdges_ShouldSatisfyCopyOrderInvariant()
    {
        var copyOrderByTable = YearEndTableRuleMatrix.Entries
            .Where(e => e.PrimaryRole == YearEndPrimaryRole.CopyToTargetYear && e.DedicatedStep is null)
            .ToDictionary(e => e.TableName, e => e.CopyOrder!.Value, StringComparer.OrdinalIgnoreCase);

        foreach (var (referencing, referenced) in KnownFkEdges)
        {
            Assert.True(copyOrderByTable.ContainsKey(referencing), $"'{referencing}' is missing from the generic-mechanism CopyToTargetYear entries.");
            Assert.True(copyOrderByTable.ContainsKey(referenced), $"'{referenced}' is missing from the generic-mechanism CopyToTargetYear entries.");

            Assert.True(
                copyOrderByTable[referenced] < copyOrderByTable[referencing],
                $"Expected CopyOrder({referenced})={copyOrderByTable[referenced]} < CopyOrder({referencing})={copyOrderByTable[referencing]} " +
                $"— '{referencing}' has a foreign key to '{referenced}' and must copy after it.");
        }
    }
}
