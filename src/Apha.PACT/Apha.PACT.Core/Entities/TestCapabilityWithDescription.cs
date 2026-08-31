namespace Apha.PACT.Core.Entities
{
    /// <summary>
    /// Projection of a TestCapability joined to its TestorProduct master so that
    /// the description and unit cost (both owned by testorproduct) are available
    /// for server-side filtering, sorting and paging in a single query.
    /// </summary>
    public class TestCapabilityWithDescription
    {
        public string TestCode { get; set; } = null!;
        public string WorkGroup { get; set; } = null!;
        public string PlanPortfolio { get; set; } = null!;
        public string? ItemDescription { get; set; }
        public decimal? UnitCost { get; set; }
        public double? PredOutturn { get; set; }
        public string? Sop { get; set; }
        public string? SmsCode { get; set; }
        public int FpsYear { get; set; }
    }
}
