using Apha.FPS.Core.Entities;

namespace Apha.FPS.Core.Interfaces
{
    /// <summary>
    /// Request-scoped persistence for the Year End planned-year staging tables (CR067). Primitives
    /// only — no Initiated/Approved status enforcement here. The service/application layer owns that
    /// decision (Initiated -> writable, Approved+ -> immutable); this repository only gives it enough
    /// request/status information (<see cref="YearEndRequestSummary"/>) to make it.
    /// </summary>
    public interface IYearEndStagingRepository
    {
        /// <summary>
        /// Resolves a Year End request's job_queue row by its JobExecutionId. Returns null if no
        /// matching row exists.
        /// </summary>
        Task<YearEndRequestSummary?> ResolveRequestAsync(Guid jobExecutionId);

        Task<List<YearEndSettingStaging>> GetStagedSettingsAsync(Guid jobQueueId);
        Task<List<YearEndMonthHourStaging>> GetStagedMonthHoursAsync(Guid jobQueueId);

        /// <summary>Upserts by (JobQueueId, Id) — re-Confirming the same setting updates in place.</summary>
        Task UpsertStagedSettingAsync(YearEndSettingStaging setting);

        /// <summary>Upserts by (JobQueueId, Month, Fmonth) — re-Confirming the same month updates in place.</summary>
        Task UpsertStagedMonthHourAsync(YearEndMonthHourStaging monthHour);

        /// <summary>Deletes every staged setting and month-hour row for this request.</summary>
        Task DeleteStagingAsync(Guid jobQueueId);
    }

    /// <summary>
    /// The subset of a job_queue row's identity/lifecycle state that callers resolving a Year End
    /// request by JobExecutionId actually need — enough for the service layer's own
    /// Initiated/Approved+ status check, without exposing the full BatchJobQueue entity.
    /// </summary>
    public sealed record YearEndRequestSummary(Guid JobQueueId, int FpsYear, int? TargetFpsYear, string Status);
}
