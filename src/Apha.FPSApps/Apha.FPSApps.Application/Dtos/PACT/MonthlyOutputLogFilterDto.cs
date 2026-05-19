namespace Apha.FPSApps.Application.Dtos.PACT
{
    public class MonthlyOutputLogFilterDto
    {
        public string? WorkGroup { get; init; }
        public string? TestCode { get; init; }
        public string? Buyer { get; init; }
        public DateTime? DateImported { get; init; }
        public double? Month { get; init; }
        public string? UserId { get; init; }
        public string? InsertDelete { get; init; }
    }
}
