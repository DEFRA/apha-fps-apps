namespace Apha.FPS.Core.Entities
{
    /// <summary>
    /// Everything CreateRequestAsync needs to insert a new fps.job_queue row for a Bulk Rates
    /// request — bundled to keep the repository method under the parameter-count limit.
    /// </summary>
    public sealed record CreateBulkRatesRequestParams(
        Guid JobQueueId,
        Guid JobExecutionId,
        int JobId,
        int InitiatedStatusId,
        string RequestedBy,
        DateTime RequestedAtUtc,
        int FpsYear);
}
