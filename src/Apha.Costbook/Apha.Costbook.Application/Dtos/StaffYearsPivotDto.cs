namespace Apha.Costbook.Application.Dtos
{
    public class StaffYearsPivotDto
    {
        public List<int> Years { get; set; } = [];
        public List<StaffYearsRowDto> Rows { get; set; } = [];
        public int TotalCount { get; set; }
    }
}
