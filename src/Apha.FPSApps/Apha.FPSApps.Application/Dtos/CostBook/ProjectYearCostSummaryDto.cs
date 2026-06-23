namespace Apha.FPSApps.Application.Dtos.CostBook
{
    public class ProjectYearCostSummaryDto
    {
        public string Project { get; set; } = null!;
        public int Year { get; set; }
        public double StaffCostTotal { get; set; }
        public double TestCostTotal { get; set; }
        public double AnimalCostTotal { get; set; }
        public double AdditionalCostTotal { get; set; }
        public double GrandTotal { get; set; }
    }
}
