using Apha.FPS.Application.Dtos;

namespace Apha.FPS.Application.Interfaces
{
    public interface IFpsSettingService
    {
        Task<List<FpsSettingDto>> GetAllSettingsAsync();
    }
}
