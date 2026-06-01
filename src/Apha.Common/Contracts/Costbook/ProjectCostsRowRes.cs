namespace Apha.Common.Contracts.Costbook
{
    public class ProjectCostsRowRes
    {
        public string Project { get; set; } = null!;
        public string Category { get; set; } = null!;
        public double Total { get; set; }
        public Dictionary<int, double> YearlyAmounts { get; set; } = [];
    }
}
