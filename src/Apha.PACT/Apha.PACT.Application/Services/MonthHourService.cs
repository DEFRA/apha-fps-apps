using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using AutoMapper;

namespace Apha.PACT.Application.Services
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
    }
}
