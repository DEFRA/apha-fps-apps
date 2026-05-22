using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using AutoMapper;

namespace Apha.PACT.Application.Services
{
    public class MonthlyOutputService : IMonthlyOutputService
    {
        private readonly IMonthlyOutputRepository _repository;
        private readonly IMapper _mapper;

        public MonthlyOutputService(IMonthlyOutputRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<MonthlyOutputLogDto>> GetMonthlyOutputLogAsync(
            QueryParameters<string> query,
            string? workGroup,
            string? testCode,
            string? buyer,
            DateTime? dateImported,
            double? month,
            string? userId,
            string? insertDelete)
        {
            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var result = await _repository.GetMonthlyOutputLogAsync(
                filter, workGroup, testCode, buyer, dateImported, month, userId, insertDelete);
            return _mapper.Map<PaginatedResult<MonthlyOutputLogDto>>(result);
        }
    }
}
