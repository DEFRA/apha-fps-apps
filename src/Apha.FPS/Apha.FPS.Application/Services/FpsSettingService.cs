using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Core.Interfaces;

namespace Apha.FPS.Application.Services
{
    public class FpsSettingService : IFpsSettingService
    {
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
                TestSetting = s.TestSetting,
                FpsCalYear = s.FpsYear
            }).ToList();
        }
    }
}
