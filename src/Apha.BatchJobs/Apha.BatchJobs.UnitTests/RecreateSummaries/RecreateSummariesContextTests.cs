using Apha.BatchJobs.Infrastructure.Context;

namespace Apha.BatchJobs.UnitTests.RecreateSummaries;

public sealed class RecreateSummariesContextTests
{
    private const string MonthEnv = "BATCH_RECREATE_SUMMARIES_MONTH";
    private const string TriggeredByEnv = "BATCH_RECREATE_SUMMARIES_TRIGGERED_BY";

    [Fact]
    public void Constructor_WhenNoEnvironmentOverrides_ShouldUseDefaults()
    {
        // Arrange
        var previousMonth = Environment.GetEnvironmentVariable(MonthEnv);
        var previousTriggeredBy = Environment.GetEnvironmentVariable(TriggeredByEnv);
        Environment.SetEnvironmentVariable(MonthEnv, null);
        Environment.SetEnvironmentVariable(TriggeredByEnv, null);

        try
        {
            // Act
            var context = new RecreateSummariesContext();

            // Assert
            Assert.Equal(1, context.Month);
            Assert.Equal("system", context.TriggeredBy);
        }
        finally
        {
            Environment.SetEnvironmentVariable(MonthEnv, previousMonth);
            Environment.SetEnvironmentVariable(TriggeredByEnv, previousTriggeredBy);
        }
    }

    [Fact]
    public void Constructor_WhenEnvironmentOverridesAreValid_ShouldUseOverrides()
    {
        // Arrange
        var previousMonth = Environment.GetEnvironmentVariable(MonthEnv);
        var previousTriggeredBy = Environment.GetEnvironmentVariable(TriggeredByEnv);
        Environment.SetEnvironmentVariable(MonthEnv, "12");
        Environment.SetEnvironmentVariable(TriggeredByEnv, "  test-user  ");

        try
        {
            // Act
            var context = new RecreateSummariesContext();

            // Assert
            Assert.Equal(12, context.Month);
            Assert.Equal("test-user", context.TriggeredBy);
        }
        finally
        {
            Environment.SetEnvironmentVariable(MonthEnv, previousMonth);
            Environment.SetEnvironmentVariable(TriggeredByEnv, previousTriggeredBy);
        }
    }

    [Theory]
    [InlineData("0")]
    [InlineData("13")]
    [InlineData("-1")]
    [InlineData("abc")]
    [InlineData("")]
    public void Constructor_WhenMonthOverrideIsInvalid_ShouldKeepDefaultMonth(string invalidMonth)
    {
        // Arrange
        var previousMonth = Environment.GetEnvironmentVariable(MonthEnv);
        Environment.SetEnvironmentVariable(MonthEnv, invalidMonth);

        try
        {
            // Act
            var context = new RecreateSummariesContext();

            // Assert
            Assert.Equal(1, context.Month);
        }
        finally
        {
            Environment.SetEnvironmentVariable(MonthEnv, previousMonth);
        }
    }
}
