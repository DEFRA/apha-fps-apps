namespace Apha.FPS.Application.Dtos.BulkRates
{
    /// <summary>
    /// API/JSON contract shape mirroring <see cref="Apha.FPS.Application.Services.BulkRatesUploadMetadata"/>
    /// exactly (wire-preserving — a boundary correction, not a contract trim).
    /// </summary>
    public class BulkRatesUploadMetadataDto
    {
        public string? Filename { get; set; }
        public string? ChecksumSha256 { get; set; }
        public int UploadVersion { get; set; }
        public DateTime? ValidationCompletedAtUtc { get; set; }
        public BulkRatesRowCountsDto RowCounts { get; set; } = new();
    }

    /// <summary>
    /// API/JSON contract shape mirroring <see cref="Apha.FPS.Core.Entities.BulkRatesRowCounts"/> exactly.
    /// </summary>
    public class BulkRatesRowCountsDto
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
