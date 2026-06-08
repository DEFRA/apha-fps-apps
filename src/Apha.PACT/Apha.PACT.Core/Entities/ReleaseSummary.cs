namespace Apha.PACT.Core.Entities
{
    public class ReleaseSummary
    {
        public IList<ReleasePeriod> ReleasePeriods { get; set; } = [];
        public string? Setting { get; set; }
    }
}