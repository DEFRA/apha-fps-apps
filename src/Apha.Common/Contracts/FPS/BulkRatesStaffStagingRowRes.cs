namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// Wire contract for a single Staff staging row on the "Staff Data (Staging)" grid.
    /// </summary>
    public class BulkRatesStaffStagingRowRes
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
