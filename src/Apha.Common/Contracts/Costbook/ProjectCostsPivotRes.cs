namespace Apha.Common.Contracts.Costbook
{
    public class ProjectCostsPivotRes
    {
        public List<int> Years { get; set; } = [];
        public List<ProjectCostsRowRes> Rows { get; set; } = [];
        public int TotalCount { get; set; }
    }
}
