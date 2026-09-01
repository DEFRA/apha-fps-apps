namespace Apha.FPS.Core.Entities
{
    /// <summary>
    /// Value object for a Bulk Rates row-count summary — no identity, no table, no lifecycle.
    /// Shared across independent Application-layer contexts: BulkRatesUploadMetadata,
    /// BulkRatesValidationResult, and BulkRatesNotificationContext. Kept flat under Entities/
    /// rather than a dedicated value-object folder — no such Core-level convention exists yet
    /// for a single six-property type to justify inventing one.
    /// </summary>
    public class BulkRatesRowCounts
    {
        public int Total { get; set; }
        public int Valid { get; set; }
        public int Invalid { get; set; }
        public int Insert { get; set; }
        public int Update { get; set; }
        public int Unchanged { get; set; }

        // Per-sheet breakdown — populated for FEC/AGRUP requests; zero for Staff/Animal.
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
