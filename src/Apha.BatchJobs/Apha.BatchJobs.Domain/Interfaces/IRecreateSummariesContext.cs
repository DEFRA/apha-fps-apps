namespace Apha.BatchJobs.Domain.Interfaces;

/// <summary>
/// Scoped execution context carrying the parameters supplied when
/// a RecreateSummaries job run is triggered.
/// Populated by the caller before the job handler executes.
/// </summary>
public interface IRecreateSummariesContext
{
    /// <summary>
    /// The FPS period month (equivalent to legacy <c>@Month</c>).
    /// Valid range: 1ΓÇô12.
    /// </summary>
    int Month { get; set; }

    /// <summary>
    /// FPS year scoped for this execution.
    /// </summary>
    int Year { get; set; }

    /// <summary>
    /// Identity of the user or process that triggered this run.
    /// Replaces <c>SYSTEM_USER</c> / <c>sp_Get_SP_No</c> from the legacy procedure.
    /// Written into <c>recreatesummaries_log.userid</c>.
    /// </summary>
    string TriggeredBy { get; set; }
}
