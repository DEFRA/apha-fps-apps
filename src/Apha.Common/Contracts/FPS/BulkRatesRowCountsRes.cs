namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// Wire contract for a Bulk Rates upload's row-count summary.
    /// </summary>
    public class BulkRatesRowCountsRes
    {
        public int Total { get; set; }
        public int Valid { get; set; }
        public int Invalid { get; set; }
        public int Insert { get; set; }
        public int Update { get; set; }
        public int Unchanged { get; set; }
    }
}
