namespace Apha.FPS.Core.Entities
{
    /// <summary>
    /// Everything SetApprovalAsync needs to record an approval on fps.job_queue — bundled to
    /// keep the repository method under the parameter-count limit.
    /// </summary>
    public sealed record SetBulkRatesApprovalParams(
        Guid JobQueueId,
        Guid JobExecutionId,
        string ApprovedBy,
        DateTime ApprovedAtUtc,
        string TriggeredBy,
        DateTime TriggeredAtUtc,
        int ApprovedStatusId);
}
