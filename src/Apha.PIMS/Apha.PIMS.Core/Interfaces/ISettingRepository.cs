using Apha.PIMS.Core.Entities;

namespace Apha.PIMS.Core.Interfaces
{
    public interface ISettingRepository
    {
        Task<List<Setting>> GetAllSettingsAsync();

        Task<List<Setting>> GetAllUserUpdateableSettingsAsync();

        Task<Setting?> GetSettingByIdAsync(string id);

        Task<Setting> UpdateSettingAsync(Setting entity);

        Task<bool> SettingExistsAsync(string id);
    }
}
