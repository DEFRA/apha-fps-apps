namespace Apha.PACT.Core.Entities
{
    /// <summary>LINQ result shape for qryTestPriceZero — read-only, not a mapped DB table or view.</summary>
    public class TestPriceCheckView
    {
        public string TestCode { get; set; } = null!;
        public string JobCode { get; set; } = null!;
        public double? NoTests { get; set; }
        public decimal? TestPrice { get; set; }
        public decimal? UnitPriceVla { get; set; }
        public decimal? DefraUnitPrice { get; set; }
        public string? Program { get; set; }
        public string? Manager { get; set; }
        public string? Owner { get; set; }
        public short IsDefraProject { get; set; }

        /// <summary>Computed: DefraUnitPrice when IsDefraProject != 0, else UnitPriceVla.</summary>
        public decimal? NormalPrice { get; set; }

        /// <summary>Computed: TestPrice == 0.</summary>
        public bool IsZeroPrice { get; set; }

        /// <summary>Computed: TestPrice != NormalPrice.</summary>
        public bool IsNotStandard { get; set; }

        public int FpsYear { get; set; }
    }
}
