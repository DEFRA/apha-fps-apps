namespace Apha.FPSApps.Application.Dtos.FPS
{
    /// <summary>Mirrors <c>BulkRatesFecStagingRowDto</c> (Apha.FPS.Application) as serialised over the wire.</summary>
    public class BulkRatesFecStagingRowDto
    {
        public string Status { get; set; } = string.Empty;
        public string TestCode { get; set; } = string.Empty;
        public decimal? UnitPriceVla { get; set; }
        public decimal? DefraUnitPrice { get; set; }
        public decimal? FecNewRate { get; set; }
        public string? ItemDescription { get; set; }
        public string? ShortDescription { get; set; }
        public string? Owner { get; set; }
        public string? Comments { get; set; }
    }
}
