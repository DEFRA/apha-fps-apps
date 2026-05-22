namespace Apha.Common.Contracts.PACT
{
    public class ProjectMonthReq
    {
        public string Project { get; set; } = null!;
        public int MonthNo { get; set; }
        public decimal? CostProfile { get; set; }
    }
}