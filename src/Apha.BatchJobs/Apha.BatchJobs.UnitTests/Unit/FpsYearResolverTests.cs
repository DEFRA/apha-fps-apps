using Apha.BatchJobs.Domain;

namespace Apha.BatchJobs.UnitTests;

public sealed class FpsYearResolverTests
{
    [Fact]
    public void ResolveFpsYear_UsesParentFpsYear_RegardlessOfTimestamp()
    {
        var occurredAt = new DateTime(2016, 6, 15, 0, 0, 0, DateTimeKind.Utc);
        Assert.Equal(2025, FpsYearResolver.ResolveFpsYear(2025, occurredAt));
    }

    [Theory]
    [InlineData(2026, 4, 1, 2026)]   // 1-Apr-2026 -> FY2026
    [InlineData(2027, 3, 31, 2026)]  // 31-Mar-2027 -> FY2026
    [InlineData(2027, 4, 1, 2027)]   // 1-Apr-2027 -> FY2027
    public void ResolveFpsYear_DerivesFromTimestamp_WhenParentFpsYearIsNull(
        int year, int month, int day, int expectedFpsYear)
    {
        var occurredAt = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
        Assert.Equal(expectedFpsYear, FpsYearResolver.ResolveFpsYear(null, occurredAt));
    }
}
