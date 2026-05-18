namespace Apha.PACT.Core.Entities;

public partial class MonthlyOutputLog
{
    public int SequenceNo { get; set; }

    public string? TestCode { get; set; }

    public string? Buyer { get; set; }

    public double? Month { get; set; }

    public string? WorkGroup { get; set; }

    public double? Volume { get; set; }

    public string? WgBuyer { get; set; }
    public DateTime? DateTime { get; set; }

    public string? UserId { get; set; }

    public string? InsertDelete { get; set; }

    public int FpsYear { get; set; }
}
