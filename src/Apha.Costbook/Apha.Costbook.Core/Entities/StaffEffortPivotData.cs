namespace Apha.Costbook.Core.Entities
{
    public class StaffEffortPivotData
    {
        public List<int> Years { get; set; } = [];
        public List<StaffEffortRowData> Rows { get; set; } = [];
        public int TotalCount { get; set; }
    }

    public class StaffEffortRowData
    {
        public string Project { get; set; } = null!;
        public string WorkGroup { get; set; } = null!;
        public string GradeCode { get; set; } = null!;
        public string Name { get; set; } = null!;
        public double Total { get; set; }
        public Dictionary<int, double> YearlyAmounts { get; set; } = [];
    }
}
