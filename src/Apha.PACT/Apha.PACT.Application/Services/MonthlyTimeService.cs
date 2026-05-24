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
            string? workGroup,
            string? timeCode,
            string? pactStaffId,
            string? parentProject,
            DateTime? dateImported,
            double? month,
            string? userId,
            string? insertDelete)
        {
            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var result = await _repository.SearchAsync(
                filter, workGroup, timeCode, pactStaffId, parentProject,
                dateImported, month, userId, insertDelete);
            return _mapper.Map<PaginatedResult<MonthlyTimeLogDto>>(result);
        }
    }
}
