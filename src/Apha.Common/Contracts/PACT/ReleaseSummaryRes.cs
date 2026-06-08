namespace Apha.Common.Contracts.PACT
{
    public class ReleaseSummaryRes
    {
        public IReadOnlyList<ReleasePeriodRes> ReleasePeriods { get; set; } = [];
        public string? Setting { get; set; }
    }
}