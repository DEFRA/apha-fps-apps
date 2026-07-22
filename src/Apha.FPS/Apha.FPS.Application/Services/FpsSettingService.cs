using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    public class FpsSettingService : IFpsSettingService
    {
        private const string HoursPerDayKey = "HoursInDay";
        private const decimal DefaultHoursPerDay = 8m;

        private readonly IFpsSettingRepository _repository;
        private readonly IMapper _mapper;

        public FpsSettingService(IFpsSettingRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
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

        public async Task<List<FpsYearEndSettingDto>> GetYearEndSettingsAsync()
        {
            var settings = await _repository.GetYearEndSettingsAsync();
            return _mapper.Map<List<FpsYearEndSettingDto>>(settings);
        }

        public async Task<FpsSettingDto> AddSettingAsync(FpsSettingDto dto)
        {
            var entity = _mapper.Map<FpsSetting>(dto);
            var result = await _repository.AddAsync(entity);
            return _mapper.Map<FpsSettingDto>(result);
        }

        public async Task<FpsSettingDto> UpdateSettingAsync(FpsSettingDto dto)
        {
            var entity = _mapper.Map<FpsSetting>(dto);
            var result = await _repository.UpdateAsync(entity);
            return _mapper.Map<FpsSettingDto>(result);
        }

        public async Task<FpsSettingDto> SaveAsync(FpsSettingDto dto)
        {
            var entity = _mapper.Map<FpsSetting>(dto);
            var result = await _repository.SaveAsync(entity);
            return _mapper.Map<FpsSettingDto>(result);
        }
    }
}
