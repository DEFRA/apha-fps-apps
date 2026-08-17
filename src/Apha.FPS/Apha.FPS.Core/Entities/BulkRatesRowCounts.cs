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
    }
}
