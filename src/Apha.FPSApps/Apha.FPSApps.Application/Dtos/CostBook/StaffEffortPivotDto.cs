namespace Apha.FPSApps.Application.Dtos.CostBook
{
    public class StaffEffortPivotDto
    {
        public List<int> Years { get; set; } = [];
        public List<StaffEffortRowDto> Rows { get; set; } = [];
        public int TotalCount { get; set; }
    }
}
