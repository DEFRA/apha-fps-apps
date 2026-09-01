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

        public int FecTotal { get; set; }
        public int FecInsert { get; set; }
        public int FecUpdate { get; set; }
        public int FecUnchanged { get; set; }
        public int FecInvalid { get; set; }
        public int AgrupTotal { get; set; }
        public int AgrupInsert { get; set; }
        public int AgrupUpdate { get; set; }
        public int AgrupUnchanged { get; set; }
        public int AgrupInvalid { get; set; }
    }
}
