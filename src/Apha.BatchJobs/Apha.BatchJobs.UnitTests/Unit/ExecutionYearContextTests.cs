using Apha.BatchJobs.Infrastructure.Context;

namespace Apha.BatchJobs.UnitTests;

public sealed class ExecutionYearContextTests
{
    [Fact]
    public void Constructor_WhenCreated_ShouldExposeDefaultState()
    {
        var context = new ExecutionYearContext();

        Assert.Null(context.FpsYear);
        Assert.Equal("Unspecified", context.YearSource);
    }

    [Fact]
    public void Properties_WhenSet_ShouldRetainAssignedValues()
    {
        var context = new ExecutionYearContext();

        context.FpsYear = 2026;
        context.YearSource = "MABArchive.FullYearCycle";

        Assert.Equal(2026, context.FpsYear);
        Assert.Equal("MABArchive.FullYearCycle", context.YearSource);
    }
}