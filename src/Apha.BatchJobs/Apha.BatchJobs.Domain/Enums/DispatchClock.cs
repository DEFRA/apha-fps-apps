namespace Apha.BatchJobs.Domain.Enums;

/// <summary>Which timestamp marks the moment a job_queue row was handed to the worker.</summary>
public enum DispatchClock
{
    /// <summary>The request time — for jobs dispatched as soon as they are requested.</summary>
    RequestedAt = 0,

    /// <summary>The approval time — for maker-checker jobs dispatched on approval.</summary>
    ApprovedAt = 1
}
