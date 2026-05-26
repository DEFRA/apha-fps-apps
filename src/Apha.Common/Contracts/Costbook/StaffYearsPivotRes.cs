namespace Apha.Common.Contracts.Costbook
{
    public class StaffYearsPivotRes
    {
        public List<int> Years { get; set; } = [];
        public List<StaffYearsRowRes> Rows { get; set; } = [];
        public int TotalCount { get; set; }
    }
}
