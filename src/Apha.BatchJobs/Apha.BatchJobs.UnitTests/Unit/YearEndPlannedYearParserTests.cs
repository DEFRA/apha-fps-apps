using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

namespace Apha.BatchJobs.UnitTests;

public sealed class YearEndPlannedYearParserTests
{
    [Fact]
    public void Parse_ShouldReturnPlannedYear_WhenOnlyPlannedYearPresent()
    {
        var result = YearEndPlannedYearParser.Parse("{\"plannedYear\":\"2027\"}");

        Assert.Equal(2027, result);
    }

    [Fact]
    public void Parse_ShouldReturnTargetFpsYear_WhenOnlyLegacyKeyPresent()
    {
        var result = YearEndPlannedYearParser.Parse("{\"targetFpsYear\":2027}");

        Assert.Equal(2027, result);
    }

    [Fact]
    public void Parse_ShouldReturnValue_WhenBothPresentAndEqual()
    {
        var result = YearEndPlannedYearParser.Parse("{\"plannedYear\":2027,\"targetFpsYear\":2027}");

        Assert.Equal(2027, result);
    }

    [Fact]
    public void Parse_ShouldThrow_WhenBothPresentAndDifferent()
    {
        var action = () => { YearEndPlannedYearParser.Parse("{\"plannedYear\":2027,\"targetFpsYear\":2028}"); };

        var exception = Assert.Throws<InvalidOperationException>(action);
        Assert.Contains("2027", exception.Message);
        Assert.Contains("2028", exception.Message);
    }

    [Fact]
    public void Parse_ShouldReturnNull_WhenNeitherPresent()
    {
        var result = YearEndPlannedYearParser.Parse("{\"someOtherField\":1}");

        Assert.Null(result);
    }

    [Fact]
    public void Parse_ShouldReturnNull_WhenParametersJsonIsNull()
    {
        var result = YearEndPlannedYearParser.Parse(null);

        Assert.Null(result);
    }
}
