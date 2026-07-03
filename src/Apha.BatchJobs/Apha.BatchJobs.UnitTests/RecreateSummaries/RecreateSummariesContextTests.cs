using Apha.BatchJobs.Infrastructure.Context;

namespace Apha.BatchJobs.UnitTests.RecreateSummaries;

public sealed class RecreateSummariesContextTests
{
    private const string MonthEnv = "BATCH_RECREATE_SUMMARIES_MONTH";
    private const string YearEnv = "BATCH_RECREATE_SUMMARIES_YEAR";
    private const string TriggeredByEnv = "BATCH_REQUESTED_BY";
    private const string ParametersJsonEnv = "BATCH_JOB_PARAMETERS_JSON";

    [Fact]
    public void Constructor_WhenParametersJsonMissing_ShouldThrow()
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
            // Act / Assert
            Assert.Throws<InvalidOperationException>(() => new RecreateSummariesContext());
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
    public void Constructor_WhenParametersJsonAndTriggeredByAreValid_ShouldUseValues()
    {
        // Arrange
        var previousMonth = Environment.GetEnvironmentVariable(MonthEnv);
        var previousYear = Environment.GetEnvironmentVariable(YearEnv);
        var previousTriggeredBy = Environment.GetEnvironmentVariable(TriggeredByEnv);
        Environment.SetEnvironmentVariable(MonthEnv, "12");
        Environment.SetEnvironmentVariable(YearEnv, "2027");
        Environment.SetEnvironmentVariable(TriggeredByEnv, "  test-user  ");
        Environment.SetEnvironmentVariable(ParametersJsonEnv, "{\"month\":\"2026-06\"}");

        try
        {
            // Act
            var context = new RecreateSummariesContext();

            // Assert
            Assert.Equal(6, context.Month);
            Assert.Equal(2026, context.Year);
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

    [Fact]
    public void Constructor_WhenMonthYearOverridesProvidedWithoutParametersJson_ShouldThrow()
    {
        // Arrange
        var previousMonth = Environment.GetEnvironmentVariable(MonthEnv);
        var previousYear = Environment.GetEnvironmentVariable(YearEnv);
        Environment.SetEnvironmentVariable(MonthEnv, "6");
        Environment.SetEnvironmentVariable(YearEnv, "2026");
        Environment.SetEnvironmentVariable(ParametersJsonEnv, null);

        try
        {
            // Act / Assert
            Assert.Throws<InvalidOperationException>(() => new RecreateSummariesContext());
        }
        finally
        {
            Environment.SetEnvironmentVariable(MonthEnv, previousMonth);
            Environment.SetEnvironmentVariable(YearEnv, previousYear);
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
