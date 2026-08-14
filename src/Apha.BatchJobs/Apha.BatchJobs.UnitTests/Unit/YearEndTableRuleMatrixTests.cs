using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

namespace Apha.BatchJobs.UnitTests;

public sealed class YearEndTableRuleMatrixTests
{
    [Fact]
    public void Entries_ShouldContainExactlyTheExpectedCountPerRole()
    {
        var entries = YearEndTableRuleMatrix.Entries;

        Assert.Equal(43, entries.Count);
        Assert.Equal(38, entries.Count(e => e.Role == YearEndTableRole.YearScopedBusinessParticipant));
        Assert.Equal(2, entries.Count(e => e.Role == YearEndTableRole.YearScopedConfigurationDependency));
        Assert.Equal(3, entries.Count(e => e.Role == YearEndTableRole.GlobalReference));
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
    public void TblTotalBusinessOverheads_ShouldBeTheOnlyCreateTargetYearRowEntry()
    {
        var entries = YearEndTableRuleMatrix.Entries
            .Where(e => e.Action == YearEndTableRuleAction.CreateTargetYearRow)
            .ToList();

        var entry = Assert.Single(entries);
        Assert.Equal("tbltotalbusinessoverheads", entry.TableName);
        Assert.Equal(YearEndTableRole.YearScopedBusinessParticipant, entry.Role);
        Assert.Equal(["fpsyear"], entry.PrimaryKeyColumns);
    }

    [Fact]
    public void TblPeriod_ShouldBeTheOnlyAlreadyImplementedViaDedicatedStepEntry()
    {
        var entries = YearEndTableRuleMatrix.Entries
            .Where(e => e.Action == YearEndTableRuleAction.AlreadyImplementedViaDedicatedStep)
            .ToList();

        var entry = Assert.Single(entries);
        Assert.Equal("tblperiod", entry.TableName);
    }

    [Fact]
    public void GlobalAndConfigurationDependencyEntries_ShouldAllUseValidateExists()
    {
        var entries = YearEndTableRuleMatrix.Entries
            .Where(e => e.Role is YearEndTableRole.GlobalReference or YearEndTableRole.YearScopedConfigurationDependency)
            .ToList();

        Assert.Equal(5, entries.Count);
        Assert.All(entries, e => Assert.Equal(YearEndTableRuleAction.ValidateExists, e.Action));
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
            .Where(e => e.Role is YearEndTableRole.YearScopedBusinessParticipant or YearEndTableRole.YearScopedConfigurationDependency)
            .ToList();

        Assert.Equal(40, partitionedEntries.Count);
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
    public void RemainingYearScopedBusinessParticipants_ShouldBePendingClassification()
    {
        var pendingCount = YearEndTableRuleMatrix.Entries
            .Count(e => e.Role == YearEndTableRole.YearScopedBusinessParticipant
                && e.Action == YearEndTableRuleAction.PendingClassification);

        Assert.Equal(36, pendingCount);
    }
}
