namespace Apha.FPSApps.Application.Dtos.FPS
{
    /// <summary>Mirrors <c>BulkRatesStaffStagingRowDto</c> (Apha.FPS.Application) as serialised over the wire.</summary>
    public class BulkRatesStaffStagingRowDto
    {
        public string Status { get; set; } = string.Empty;
        public string PcGrade { get; set; } = string.Empty;
        public decimal? PayRate { get; set; }
        public decimal? PayRateNew { get; set; }
        public decimal? Npr { get; set; }
        public decimal? NprNew { get; set; }
        public decimal? Ohr { get; set; }
        public decimal? OhrNew { get; set; }
    }
}
