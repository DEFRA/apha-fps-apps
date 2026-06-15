using Apha.BatchJobs.Infrastructure.Context;

namespace Apha.BatchJobs.UnitTests.RecreateSummaries;

public sealed class RecreateSummariesContextTests
{
    private const string MonthEnv = "BATCH_RECREATE_SUMMARIES_MONTH";
    private const string YearEnv = "BATCH_RECREATE_SUMMARIES_YEAR";
    private const string TriggeredByEnv = "BATCH_REQUESTED_BY";
    private const string ParametersJsonEnv = "BATCH_JOB_PARAMETERS_JSON";

    [Fact]
    public void Constructor_WhenNoEnvironmentOverrides_ShouldUseDefaults()
    {
        // Arrange
        var previousMonth = Environment.GetEnvironmentVariable(MonthEnv);
        var previousYear = Environment.GetEnvironmentVariable(YearEnv);
        var previousTriggeredBy = Environment.GetEnvironmentVariable(TriggeredByEnv);
        Environment.SetEnvironmentVariable(MonthEnv, null);
        Environment.SetEnvironmentVariable(YearEnv, null);
        Environment.SetEnvironmentVariable(TriggeredByEnv, null);
        Environment.SetEnvironmentVariable(ParametersJsonEnv, null);

        try
        {
            // Act
            var context = new RecreateSummariesContext();

            // Assert
            Assert.Equal(1, context.Month);
            Assert.Equal(DateTime.UtcNow.Year, context.Year);
            Assert.Equal("system", context.TriggeredBy);
        }
        finally
        {
            Environment.SetEnvironmentVariable(MonthEnv, previousMonth);
            Environment.SetEnvironmentVariable(YearEnv, previousYear);
            Environment.SetEnvironmentVariable(TriggeredByEnv, previousTriggeredBy);
            Environment.SetEnvironmentVariable(ParametersJsonEnv, null);
        }
    }

    [Fact]
    public void Constructor_WhenEnvironmentOverridesAreValid_ShouldUseOverrides()
    {
        // Arrange
        var previousMonth = Environment.GetEnvironmentVariable(MonthEnv);
        var previousYear = Environment.GetEnvironmentVariable(YearEnv);
        var previousTriggeredBy = Environment.GetEnvironmentVariable(TriggeredByEnv);
        Environment.SetEnvironmentVariable(MonthEnv, "12");
        Environment.SetEnvironmentVariable(YearEnv, "2027");
        Environment.SetEnvironmentVariable(TriggeredByEnv, "  test-user  ");
        Environment.SetEnvironmentVariable(ParametersJsonEnv, null);

        try
        {
            // Act
            var context = new RecreateSummariesContext();

            // Assert
            Assert.Equal(12, context.Month);
            Assert.Equal(2027, context.Year);
            Assert.Equal("test-user", context.TriggeredBy);
        }
        finally
        {
            Environment.SetEnvironmentVariable(MonthEnv, previousMonth);
            Environment.SetEnvironmentVariable(YearEnv, previousYear);
            Environment.SetEnvironmentVariable(TriggeredByEnv, previousTriggeredBy);
            Environment.SetEnvironmentVariable(ParametersJsonEnv, null);
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
        Environment.SetEnvironmentVariable(ParametersJsonEnv, null);

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
            Environment.SetEnvironmentVariable(ParametersJsonEnv, null);
        }
    }

    [Fact]
    public void Constructor_WhenParametersJsonIsValid_ShouldPopulateMonthAndYear()
    {
        var previousParametersJson = Environment.GetEnvironmentVariable(ParametersJsonEnv);
        Environment.SetEnvironmentVariable(ParametersJsonEnv, "{\"month\":\"2026-06\"}");

        try
        {
            var context = new RecreateSummariesContext();

            Assert.Equal(6, context.Month);
            Assert.Equal(2026, context.Year);
        }
        finally
        {
            Environment.SetEnvironmentVariable(ParametersJsonEnv, previousParametersJson);
        }
    }

    [Theory]
    [InlineData("[]")]
    [InlineData("\"month=2026-06\"")]
    [InlineData("123")]
    [InlineData("true")]
    [InlineData("{\"month\":6}")]
    [InlineData("{\"month\":\"2026-13\"}")]
    public void Constructor_WhenParametersJsonIsInvalid_ShouldThrow(string invalidJson)
    {
        var previousParametersJson = Environment.GetEnvironmentVariable(ParametersJsonEnv);
        Environment.SetEnvironmentVariable(ParametersJsonEnv, invalidJson);

        try
        {
            Assert.Throws<InvalidOperationException>(() => new RecreateSummariesContext());
        }
        finally
        {
            Environment.SetEnvironmentVariable(ParametersJsonEnv, previousParametersJson);
        }
    }
}
