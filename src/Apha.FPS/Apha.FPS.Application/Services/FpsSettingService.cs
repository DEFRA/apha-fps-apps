using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Validation;
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
        private readonly IYearEndStagingRepository _yearEndStagingRepository;
        private readonly IMapper _mapper;

        public FpsSettingService(IFpsSettingRepository repository, IYearEndStagingRepository yearEndStagingRepository, IMapper mapper)
        {
            _repository = repository;
            _yearEndStagingRepository = yearEndStagingRepository;
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
                FpsYear = s.FpsYear,
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

        public async Task<List<YearEndFpsSettingDto>> GetYearEndSettingsAsync()
        {
            var settings = await _repository.GetYearEndSettingsAsync();
            return _mapper.Map<List<YearEndFpsSettingDto>>(settings);
        }

        public async Task<List<YearEndFpsSettingDto>> GetYearEndSettingsAsync(Guid jobExecutionId)
        {
            var request = await _yearEndStagingRepository.ResolveRequestAsync(jobExecutionId)
                ?? throw new KeyNotFoundException($"Year End Data Setup request '{jobExecutionId}' was not found.");

            var settings = await _repository.GetYearEndSettingsAsync(request);
            return _mapper.Map<List<YearEndFpsSettingDto>>(settings);
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

        public async Task<FpsSettingDto> SaveSettingAsync(Guid jobExecutionId, FpsSettingDto dto)
        {
            var errors = new List<BusinessValidationError>();

            if (dto?.Id == "HoursInDay")
            {
                var value = dto?.Setting;
                bool isValid = !string.IsNullOrWhiteSpace(value) &&
                    decimal.TryParse(value, out var number) && number > 0;

                if (!isValid)
                    errors.Add(new BusinessValidationError($"Configuration values for the IDs HoursInDay is not valid. Please provide a numeric value.", "Missing_HoursInDay"));
            }

            if (dto?.Id == "CapApprovalReceivedForReset")
            {
                var value = dto?.Setting;

                if (!string.IsNullOrWhiteSpace(value))
                {
                    bool isValid = string.Equals(value?.Trim().ToLower(), "yes", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(value?.Trim().ToLower(), "no", StringComparison.OrdinalIgnoreCase);

                    if (!isValid)
                        errors.Add(new BusinessValidationError($"Configuration values for the IDs CapApprovalReceivedForReset is not valid. Please provide 'Yes' or 'No'.", "Missing_CapApprovalReceivedForReset"));
                }
            }

            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            // Planned-year staging design: resolve the exact request and require it to still be
            // Initiated before staging anything — staging is immutable the instant Approve succeeds
            // (and stays so through Running/Completed/Failed/Rejected). Never falls back to
            // "whichever request is currently active".
            var request = await _yearEndStagingRepository.ResolveRequestAsync(jobExecutionId)
                ?? throw new KeyNotFoundException($"Year End Data Setup request '{jobExecutionId}' was not found.");

            if (!string.Equals(request.Status, "Initiated", StringComparison.OrdinalIgnoreCase))
            {
                throw new BusinessValidationErrorException([
                    new BusinessValidationError(
                        $"This request is no longer editable (status: {request.Status}).", "REQUEST_NOT_EDITABLE")
                ]);
            }

            await _yearEndStagingRepository.UpsertStagedSettingAsync(new YearEndSettingStaging
            {
                JobQueueId = request.JobQueueId,
                Id = dto!.Id,
                Setting = dto.Setting,
                Notes = dto.Notes
            });

            return new FpsSettingDto
            {
                Id = dto.Id,
                Setting = dto.Setting,
                Notes = dto.Notes,
                FpsYear = request.TargetFpsYear
            };
        }
    }
}
