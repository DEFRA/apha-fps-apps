namespace Apha.Costbook.Core.Entities
{
    public class StaffYearsPivotData
    {
        public List<int> Years { get; set; } = [];
        public List<StaffYearsRowData> Rows { get; set; } = [];
        public int TotalCount { get; set; }
    }

    public class StaffYearsRowData
    {
        public string Project { get; set; } = null!;
        public string Grade { get; set; } = null!;
        public double Total { get; set; }
        public Dictionary<int, double> YearlyAmounts { get; set; } = [];
    }
}
