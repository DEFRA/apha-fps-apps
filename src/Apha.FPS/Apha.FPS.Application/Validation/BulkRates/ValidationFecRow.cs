namespace Apha.FPS.Application.Validation.BulkRates
{
    /// <summary>
    /// A staged FEC row, in a neutral shape decoupled from
    /// <see cref="Apha.FPS.Core.Entities.TestOrProductStagingRow"/> — the caller maps its
    /// repository entity into this before calling the validation service, and maps
    /// ValidationFinding back out, so the rule engine doesn't depend on the exact staging
    /// entity shape.
    /// </summary>
    public sealed record ValidationFecRow
    {
        public required string TestCode { get; init; }
        public decimal? FecNewRate { get; init; }
        public string? ItemDescription { get; init; }
        public string? ShortDescription { get; init; }
        public string? Owner { get; init; }
        public string? Comments { get; init; }

        /// <summary>1-based worksheet row number (row 1 is the header), for finding attribution.</summary>
        public required int SourceRow { get; init; }
    }
}
