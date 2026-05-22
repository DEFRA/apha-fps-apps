namespace Apha.Common.Contracts.PACT
{
    public class MonthlyOutputLogRes
    {
        public int SequenceNo { get; set; }
        public string TestCode { get; set; } = null!;
        public string Buyer { get; set; } = null!;
        public double Month { get; set; }
        public string WorkGroup { get; set; } = null!;
        public double? Volume { get; set; }
        public DateTime? DateTime { get; set; }
        public string? UserId { get; set; }
        public string? InsertDelete { get; set; }
        public int? FpsYear { get; set; }
    }
}
