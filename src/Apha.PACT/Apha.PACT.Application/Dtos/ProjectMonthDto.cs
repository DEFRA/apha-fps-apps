namespace Apha.PACT.Application.Dtos
{
    public class ProjectMonthDto
    {
        public string Project { get; set; } = null!;
        public int MonthNo { get; set; }
        public decimal? CostProfile { get; set; }
        public int FpsYear { get; set; }
    }
}