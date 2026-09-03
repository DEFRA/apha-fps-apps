using Apha.FPS.Core.Entities;

namespace Apha.FPS.Core.Interfaces
{
    public interface IFpsSettingRepository
    {
        Task<List<FpsSetting>> GetAllAsync();
        Task<FpsSetting?> GetByKeyAsync(string key);
        /// <summary>
        /// Legacy read: current/Open + Planned (YearMasters-status-driven) year-end settings. Still
        /// used by YearEndService's Approve-time ValidateConfiguration, unchanged by the planned-year
        /// staging design — do not repurpose this overload for the grid read path.
        /// </summary>
        Task<List<YearEndFpsSetting>> GetYearEndSettingsAsync();

        /// <summary>
        /// Grid read path (planned-year staging design): current/Open-year real values overlaid with
        /// staged rows for <paramref name="request"/>'s JobQueueId. ExistsForPlannedYear reflects
        /// "has a staged row", not "a real target-year row exists" — there isn't one pre-Approval.
        /// </summary>
        Task<List<YearEndFpsSetting>> GetYearEndSettingsAsync(YearEndRequestSummary request);
        Task<FpsSetting> AddAsync(FpsSetting setting);
        Task<FpsSetting> UpdateAsync(FpsSetting setting);
        Task<FpsSetting> SaveAsync(FpsSetting setting);
    }
}
