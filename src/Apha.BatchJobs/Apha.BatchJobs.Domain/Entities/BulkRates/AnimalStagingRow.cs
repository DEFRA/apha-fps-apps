namespace Apha.BatchJobs.Domain.Entities.BulkRates;

/// <summary>
/// Represents a row from fps.tblstaginganimals for a specific request.
/// Maps to Animal rate staging data uploaded by the initiator.
/// </summary>
public sealed record AnimalStagingRow(
    Guid JobQueueId,
    string AnimalType,
    string? Species,
    string? SecurityLevel,
    decimal? DailyRate,
    decimal? DefraDailyRate,
    bool? PlanByWeek);
