namespace Apha.FPSApps.Application.Dtos.FPS
{
    /// <summary>
    /// Frontend DTO for TestOrProduct VLA list entries.
    /// Mirrors Apha.FPS.Application.Dtos.TestListVlaDto for use in the frontend
    /// application and infrastructure layers.
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
