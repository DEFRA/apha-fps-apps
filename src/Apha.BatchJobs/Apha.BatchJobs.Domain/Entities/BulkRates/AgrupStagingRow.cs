namespace Apha.BatchJobs.Domain.Entities.BulkRates;

/// <summary>
/// Represents a row from fps.tblstagingtlkptestreqmt for a specific request.
/// Maps to AGRUP (tlkpTestReqmt) staging data uploaded by the initiator.
/// </summary>
public sealed record AgrupStagingRow(
    Guid JobQueueId,
    string TestCode,
    string Buyer,
    decimal? Agrup,
    decimal? AgrupNew,
    decimal? Change,
    double? NoRequired,
    DateTime? DateCreated,
    short? Active,
    string? Comments);
