using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using AutoMapper;

namespace Apha.PACT.Application.Services
{
    public class TestActualBreakdownService : ITestActualBreakdownService
    {
        private readonly ITestActualBreakdownRepository _repository;
        private readonly IMapper _mapper;

        public TestActualBreakdownService(ITestActualBreakdownRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<TestActualBreakdownDto>> GetPagedAsync(QueryParameters<string> query)
        {
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetPagedAsync(parameters);
            return _mapper.Map<PaginatedResult<TestActualBreakdownDto>>(pagedData);
        }
    }
}
