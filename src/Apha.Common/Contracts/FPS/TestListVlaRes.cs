namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// Response contract for a TestOrProduct VLA list item.
    /// Maps to fps.testorproduct (composite PK: ItemCode + FpsYear).
    /// </summary>
    public class TestListVlaRes
    {
        public string ItemCode { get; set; } = null!;
        public int FpsYear { get; set; }

        public string? ItemDescription { get; set; }
        public string? TestManager { get; set; }
        public string? JobStatus { get; set; }

        public decimal? UnitPriceVla { get; set; }
        public decimal? PriceAhvg { get; set; }

        public string? Owner { get; set; }
        public string? ChargeMethod { get; set; }
        public string? ShortDescription { get; set; }

        public decimal DefraUnitPrice { get; set; }
    }
}
