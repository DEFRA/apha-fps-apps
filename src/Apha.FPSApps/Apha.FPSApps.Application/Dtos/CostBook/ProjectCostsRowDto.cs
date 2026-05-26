namespace Apha.FPSApps.Application.Dtos.CostBook
{
    public class ProjectCostsRowDto
    {
        public string Project { get; set; } = null!;
        public string Category { get; set; } = null!;
        public double Total { get; set; }
        public Dictionary<int, double> YearlyAmounts { get; set; } = [];
    }
}
