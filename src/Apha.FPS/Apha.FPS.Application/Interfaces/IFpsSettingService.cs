using Apha.FPS.Application.Dtos;

namespace Apha.FPS.Application.Interfaces
{
    public interface IFpsSettingService
    {
        Task<List<FpsSettingDto>> GetAllSettingsAsync();
        Task<decimal> GetHoursPerDayAsync();
        Task<List<FpsYearEndSettingDto>> GetYearEndSettingsAsync();
        Task<FpsSettingDto> AddSettingAsync(FpsSettingDto dto);
        Task<FpsSettingDto> UpdateSettingAsync(FpsSettingDto dto);
        Task<FpsSettingDto> SaveAsync(FpsSettingDto dto);
    }
}
