namespace Apha.Common.Contracts.Costbook
{
    public class StaffYearsRowRes
    {
        public string Project { get; set; } = null!;
        public string Grade { get; set; } = null!;
        public double Total { get; set; }
        public Dictionary<int, double> YearlyAmounts { get; set; } = [];
    }
}
