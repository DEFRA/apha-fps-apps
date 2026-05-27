namespace Apha.Costbook.Core.Entities
{
    public class ProjectCostsPivotData
    {
        public List<int> Years { get; set; } = [];
        public List<ProjectCostsRowData> Rows { get; set; } = [];
        public int TotalCount { get; set; }
    }

    public class ProjectCostsRowData
    {
        public string Project { get; set; } = null!;
        public string Category { get; set; } = null!;
        public double Total { get; set; }
        public Dictionary<int, double> YearlyAmounts { get; set; } = [];
    }
}
