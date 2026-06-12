namespace Apha.FPSApps.Application.Dtos.PACT
{
    public class ReleaseSummaryDto
    {
        public IReadOnlyList<ReleasePeriodDto> ReleasePeriods { get; set; } = [];
        public string? Setting { get; set; }
    }
}