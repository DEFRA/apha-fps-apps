using Apha.FPS.Core.Entities;

namespace Apha.FPS.Core.Interfaces
{
    public interface IFpsSettingRepository
    {
        Task<List<FpsSetting>> GetAllAsync();
        Task<FpsSetting?> GetByKeyAsync(string key);
        Task<List<FpsYearEndSetting>> GetYearEndSettingsAsync();
        Task<FpsSetting> AddAsync(FpsSetting setting);
        Task<FpsSetting> UpdateAsync(FpsSetting setting);
    }
}
