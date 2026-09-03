using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Application.Validation;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    public class MonthHourService : IMonthHourService
    {
        private readonly IMonthHourRepository _repository;
        private readonly IYearEndStagingRepository _yearEndStagingRepository;
        private readonly IMapper _mapper;

        public MonthHourService(IMonthHourRepository repository, IYearEndStagingRepository yearEndStagingRepository, IMapper mapper)
        {
            _repository = repository;
            _yearEndStagingRepository = yearEndStagingRepository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<MonthHourDto>> GetAllMonthHourAsync(QueryParameters<string> query)
        {
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetAllAsync(parameters);
            return _mapper.Map<PaginatedResult<MonthHourDto>>(pagedData);
        }

        public async Task<IEnumerable<MonthHourDto>> GetMonthHoursByYearAsync(short year)
        {
            var items = await _repository.GetByYearAsync(year);
            return _mapper.Map<IEnumerable<MonthHourDto>>(items);
        }

        public async Task<IEnumerable<short>> GetDistinctYearsAsync()
        {
            return await _repository.GetDistinctYearsAsync();
        }

        public async Task<List<YearEndMonthHourDto>> GetYearEndMonthHoursAsync()
        {
            var items = await _repository.GetYearEndMonthHoursAsync();
            return _mapper.Map<List<YearEndMonthHourDto>>(items);
        }

        public async Task<List<YearEndMonthHourDto>> GetYearEndMonthHoursAsync(Guid jobExecutionId)
        {
            var request = await _yearEndStagingRepository.ResolveRequestAsync(jobExecutionId)
                ?? throw new KeyNotFoundException($"Year End Data Setup request '{jobExecutionId}' was not found.");

            var items = await _repository.GetYearEndMonthHoursAsync(request);
            return _mapper.Map<List<YearEndMonthHourDto>>(items);
        }

        public async Task<MonthHourDto> SaveMonthHourAsync(Guid jobExecutionId, MonthHourDto dto)
        {
            var errors = new List<BusinessValidationError>();


            bool hasmissingMissingVal = dto.Days < 0 || dto.VidHours < 0 || dto.CvlHours < 0;

            if (hasmissingMissingVal)
                errors.Add(new BusinessValidationError($"Provided Month Working days, VID hours and CVL hours values are not valid. Values should be non-negative and greater than zero. Please verify.", "Missing_Config"));


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

            await _yearEndStagingRepository.UpsertStagedMonthHourAsync(new YearEndMonthHourStaging
            {
                JobQueueId = request.JobQueueId,
                MonthYear = dto.Year,
                Month = dto.Month,
                Fmonth = dto.Fmonth ?? 0,
                Days = dto.Days,
                CvlHours = dto.CvlHours,
                VidHours = dto.VidHours
            });

            return new MonthHourDto
            {
                Year = dto.Year,
                Month = dto.Month,
                Fmonth = dto.Fmonth,
                Days = dto.Days,
                CvlHours = dto.CvlHours,
                VidHours = dto.VidHours,
                FpsYear = request.TargetFpsYear ?? dto.FpsYear
            };
        }
    }
}
