namespace Apha.FPS.Application.Dtos
{
    /// <summary>
    /// Internal DTO for TestOrProduct VLA list entries.
    /// Used as the service-layer transfer object between repository and API controller.
    /// Maps to fps.testorproduct (composite PK: ItemCode + FpsYear).
    /// </summary>
    public class TestListVlaDto
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
