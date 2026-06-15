namespace Apha.PACT.Application.Dtos
{
    public class ReleaseSummaryDto
    {
        public IReadOnlyList<ReleasePeriodDto> ReleasePeriods { get; set; } = [];
        public string? Setting { get; set; }
    }
}