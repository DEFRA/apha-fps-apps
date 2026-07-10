namespace Apha.BatchJobs.Domain.Entities.BulkRates;

/// <summary>
/// Represents a row from fps.tblstagingprofitcentregrade for a specific request.
/// Maps to Staff profit-centre grade staging data uploaded by the initiator.
/// </summary>
public sealed record StaffStagingRow(
    Guid JobQueueId,
    string PcGrade,
    decimal? PayRate,
    decimal? Npr,
    decimal? Ohr);
