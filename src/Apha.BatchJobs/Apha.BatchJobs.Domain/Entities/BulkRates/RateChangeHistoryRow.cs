namespace Apha.BatchJobs.Domain.Entities.BulkRates;

/// <summary>
/// Write model for a single field-level row in fps.rate_change_history.
/// One row per changed field per business key per request.
/// Written before staging deletion to ensure audit outlives staging.
/// </summary>
public sealed record RateChangeHistoryRow(
    Guid JobQueueId,
    Guid JobExecutionId,
    int JobId,
    int FpsYear,
    string RateCategory,
    string BusinessKeyJson,
    string FieldName,
    string? OldValue,
    string? NewValue,
    string ChangeType,
    string? RequestedBy,
    string? ApprovedBy,
    DateTime AppliedAtUtc);
