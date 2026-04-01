using Apha.FPS.Core.Entities;

namespace Apha.FPS.Core.Interfaces
{
    public interface IFpsSettingRepository
    {
        Task<List<FpsSetting>> GetAllAsync();
        Task<FpsSetting?> GetByKeyAsync(string key);
    }
}
