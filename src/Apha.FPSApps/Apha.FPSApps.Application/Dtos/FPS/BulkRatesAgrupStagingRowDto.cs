namespace Apha.FPSApps.Application.Dtos.FPS
{
    /// <summary>Mirrors <c>BulkRatesAgrupStagingRowDto</c> (Apha.FPS.Application) as serialised over the wire.</summary>
    public class BulkRatesAgrupStagingRowDto
    {
        public string Status { get; set; } = string.Empty;
        public string TestCode { get; set; } = string.Empty;
        public string Buyer { get; set; } = string.Empty;
        public decimal? Agrup { get; set; }
        public decimal? AgrupNew { get; set; }
        public double? NoRequired { get; set; }
        public DateTime? DateCreated { get; set; }
        public short? Active { get; set; }
        public string? Comments { get; set; }
    }
}
