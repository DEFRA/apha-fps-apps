using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Core.Interfaces;

namespace Apha.FPS.Application.Services
{
    public class FpsSettingService : IFpsSettingService
    {
        private const string HoursPerDayKey = "HoursInDay";
        private const decimal DefaultHoursPerDay = 8m;

        private readonly IFpsSettingRepository _repository;

        public FpsSettingService(IFpsSettingRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<FpsSettingDto>> GetAllSettingsAsync()
        {
            var settings = await _repository.GetAllAsync();
            return settings.Select(s => new FpsSettingDto
            {
                Id = s.Id,
                Setting = s.Setting,
                Notes = s.Notes,
                FpsCalYear = s.FpsYear,
                UpdatedBy = s.UpdatedBy,
                UpdatedAt = s.UpdatedAt
            }).ToList();
        }

        public async Task<decimal> GetHoursPerDayAsync()
        {
            var setting = await _repository.GetByKeyAsync(HoursPerDayKey);
            if (setting != null && decimal.TryParse(setting.Setting, out var hours))
                return hours;

            return DefaultHoursPerDay;
        }
    }
}
