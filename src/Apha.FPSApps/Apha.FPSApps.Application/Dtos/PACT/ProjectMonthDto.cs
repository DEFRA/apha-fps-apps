namespace Apha.FPSApps.Application.Dtos.PACT
{
    public class ProjectMonthDto
    {
        public string Project { get; set; } = null!;
        public int MonthNo { get; set; }
        public decimal? CostProfile { get; set; }
    }
}