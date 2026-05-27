namespace Apha.FPSApps.Application.Dtos.CostBook
{
    public class ProjectCostsPivotDto
    {
        public List<int> Years { get; set; } = [];
        public List<ProjectCostsRowDto> Rows { get; set; } = [];
        public int TotalCount { get; set; }
    }
}
