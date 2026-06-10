namespace Apha.Common.Contracts.PACT
{
    public class WorkGroupCos90sExportReq
    {
        public string ProfitCentre { get; set; } = string.Empty;
        public short MonthNumber { get; set; }
        public short Year { get; set; }
        public string? PactId { get; set; }
    }
}
