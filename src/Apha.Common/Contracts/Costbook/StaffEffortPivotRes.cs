namespace Apha.Common.Contracts.Costbook
{
    public class StaffEffortPivotRes
    {
        public List<int> Years { get; set; } = [];
        public List<StaffEffortRowRes> Rows { get; set; } = [];
        public int TotalCount { get; set; }
    }
}
