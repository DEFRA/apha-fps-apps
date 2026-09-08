using Apha.FPS.Core.Entities;

namespace Apha.FPS.Core.Interfaces
{
    /// <summary>
    /// Persistence for the Year End planned-year staging tables (CR067, singleton shape per the
    /// 2026-09-07 staging simplification). Primitives only — no Initiated/Approved status enforcement
    /// here. The service/application layer owns that decision (Initiated -> writable, Approved+ ->
    /// immutable); this repository only gives it enough request/status information
    /// (<see cref="YearEndRequestSummary"/>) to make it.
    ///
    /// Staging is not scoped by jobqueueid any more — <c>YearEndRepository.CanInitiateRequest</c>
    /// guarantees at most one non-terminal Year End DataSetup request exists at a time, so there is
    /// only ever one candidate row set to read, upsert, or clear.
    /// </summary>
    public interface IYearEndStagingRepository
    {
        /// <summary>
        /// Resolves a Year End request's job_queue row by its JobExecutionId. Returns null if no
        /// matching row exists.
        /// </summary>
        Task<YearEndRequestSummary?> ResolveRequestAsync(Guid jobExecutionId);

        Task<List<FpsSettingStaging>> GetStagedSettingsAsync();
        Task<List<MonthHourStaging>> GetStagedMonthHoursAsync();

        /// <summary>Upserts by (Id, FpsYear) — re-Confirming the same setting updates in place.</summary>
        Task UpsertStagedSettingAsync(FpsSettingStaging setting);

        /// <summary>Upserts by (Year, Month, FpsYear) — re-Confirming the same month updates in place.</summary>
        Task UpsertStagedMonthHourAsync(MonthHourStaging monthHour);

        /// <summary>Deletes every staged setting and month-hour row.</summary>
        Task DeleteStagingAsync();
    }

    /// <summary>
    /// The subset of a job_queue row's identity/lifecycle state that callers resolving a Year End
    /// request by JobExecutionId actually need — enough for the service layer's own
    /// Initiated/Approved+ status check, without exposing the full BatchJobQueue entity.
    /// </summary>
    public sealed record YearEndRequestSummary(Guid JobQueueId, int FpsYear, int? TargetFpsYear, string Status);
}
