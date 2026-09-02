namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// Wire contract for a single AGRUP staging row on the "Agrup Details" grid.
    /// </summary>
    public class BulkRatesAgrupStagingRowRes
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
