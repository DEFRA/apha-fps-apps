namespace Apha.BatchJobs.Domain.Entities.BulkRates;

/// <summary>
/// Represents a row from fps.tblstagingtestorproduct for a specific request.
/// Maps to FEC Test/Product staging data uploaded by the initiator.
/// </summary>
public sealed record FecStagingRow(
    Guid JobQueueId,
    string TestCode,
    decimal? UnitPriceVla,
    decimal? DefraUnitPrice,
    decimal? FecNewRate,
    decimal? Change,
    string? ItemDescription,
    string? ShortDescription,
    string? Owner,
    string? Comments);
