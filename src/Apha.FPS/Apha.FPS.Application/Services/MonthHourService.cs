using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    public class MonthHourService : IMonthHourService
    {
        private readonly IMonthHourRepository _repository;
        private readonly IMapper _mapper;

        public MonthHourService(IMonthHourRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<MonthHourDto>> GetAllAsync(QueryParameters<string> query)
        {
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetAllAsync(parameters);
            return _mapper.Map<PaginatedResult<MonthHourDto>>(pagedData);
        }

        public async Task<IEnumerable<MonthHourDto>> GetByYearAsync(short year)
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

        public async Task<MonthHourDto> SaveAsync(MonthHourDto dto)
        {
            var entity = _mapper.Map<MonthHour>(dto);
            var result = await _repository.SaveAsync(entity);
            return _mapper.Map<MonthHourDto>(result);
        }
    }
}
