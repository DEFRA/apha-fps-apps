namespace Apha.BatchJobs.Domain.Entities;

/// <summary>
/// Year End approval/configuration audit columns read from fps.job_queue (see CR025).
/// </summary>
public sealed record JobQueueApprovalMetadata(
    string? ConfigurationJson,
    string? ApprovedBy,
    DateTime? ApprovedAtUtc,
    string? RejectedBy,
    DateTime? RejectedAtUtc,
    string? RejectionReason,
    string? TriggeredBy,
    DateTime? TriggeredAtUtc);
