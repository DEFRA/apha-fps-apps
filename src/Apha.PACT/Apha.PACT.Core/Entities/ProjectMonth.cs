namespace Apha.PACT.Core.Entities
{
    public partial class ProjectMonth
    {
        public string Project { get; set; } = null!;

        public int MonthNo { get; set; }

        public decimal? CostProfile { get; set; }

        public int FpsYear { get; set; }
    }
}