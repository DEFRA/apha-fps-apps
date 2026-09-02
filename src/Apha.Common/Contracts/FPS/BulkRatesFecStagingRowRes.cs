namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// Wire contract for a single FEC staging row on the "FEC Data (Staging)" grid.
    /// </summary>
    public class BulkRatesFecStagingRowRes
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
