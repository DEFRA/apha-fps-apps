using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using AutoMapper;

namespace Apha.PACT.Application.Services
{
    public class MonthlyTimeService : IMonthlyTimeService
    {
        private readonly IMonthlyTimeRepository _repository;
        private readonly IMapper _mapper;

        public MonthlyTimeService(IMonthlyTimeRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<MonthlyTimeLogDto>> SearchAsync(
           QueryParameters<string> query,
           MonthlyTimeLogFilterDto monthlyTimeLogFilter
            )
        {
            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var logFilter = _mapper.Map<Apha.PACT.Core.Entities.MonthlyTimeLogFilter>(monthlyTimeLogFilter);

            var result = await _repository.SearchAsync(filter, logFilter);
            return _mapper.Map<PaginatedResult<MonthlyTimeLogDto>>(result);
        }
    }
}
