namespace Apha.FPSApps.Application.Dtos.PACT
{
    public class MonthlyTimeLogFilterDto
    {
        public string? WorkGroup { get; init; }
        public string? TimeCode { get; init; }
        public string? PactStaffId { get; init; }
        public string? ParentProject { get; init; }
        public DateTime? DateImported { get; init; }
        public double? Month { get; init; }
        public string? UserId { get; init; }
        public string? InsertDelete { get; init; }
    }
}
