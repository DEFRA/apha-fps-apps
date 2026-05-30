namespace Apha.PACT.Application.Dtos
{
    public class ProjectMonthFinalDto
    {
        public string Project { get; set; } = null!;
        public int MonthNo { get; set; }
        public decimal? TotalCost { get; set; }
        public int FpsYear { get; set; }
    }
}