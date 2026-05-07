namespace Apha.FPSApps.Application.Dtos.PACT
{
    public class ProjectProfileCumulativeGraphDto
    {
        public int MonthNo { get; set; }
        public decimal? CumulativeProfile { get; set; }
        public decimal? CumulativeCost { get; set; }
    }
}