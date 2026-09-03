using Apha.FPS.Application.Dtos;

namespace Apha.FPS.Application.Interfaces
{
    public interface IFpsSettingService
    {
        Task<List<FpsSettingDto>> GetAllSettingsAsync();
        Task<decimal> GetHoursPerDayAsync();

        /// <summary>
        /// Legacy read: current/Open + Planned (YearMasters-status-driven) year-end settings.
        /// Unchanged by the planned-year staging design — kept for callers that don't yet supply a
        /// JobExecutionId (FPSApps page-load path; Workstream 8 will migrate/remove this).
        /// </summary>
        Task<List<YearEndFpsSettingDto>> GetYearEndSettingsAsync();

        /// <summary>
        /// Grid read path (planned-year staging design): resolves <paramref name="jobExecutionId"/>
        /// to its request, then returns current/Open-year values overlaid with that request's staged
        /// rows. Throws if <paramref name="jobExecutionId"/> doesn't resolve to a Year End Data Setup
        /// request — never falls back to "whichever request is currently active".
        /// </summary>
        Task<List<YearEndFpsSettingDto>> GetYearEndSettingsAsync(Guid jobExecutionId);
        Task<FpsSettingDto> AddSettingAsync(FpsSettingDto dto);
        Task<FpsSettingDto> UpdateSettingAsync(FpsSettingDto dto);

        /// <summary>
        /// Confirm (planned-year staging design): resolves <paramref name="jobExecutionId"/> to its
        /// Year End Data Setup request, requires it to be Initiated (staging is immutable once
        /// Approved/Running/Completed/Failed/Rejected), then upserts a staged row — never writes
        /// fps.tblsettings directly. JobExecutionId is required, not optional: a write with no
        /// resolvable request identity would undermine the whole staging design.
        /// </summary>
        Task<FpsSettingDto> SaveSettingAsync(Guid jobExecutionId, FpsSettingDto dto);
    }
}
