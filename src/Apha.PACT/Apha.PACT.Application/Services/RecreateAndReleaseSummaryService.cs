using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using AutoMapper;

namespace Apha.PACT.Application.Services
{
    public class RecreateAndReleaseSummaryService : IRecreateAndReleaseSummaryService
    {
        private readonly IRecreateAndReleaseSummaryRepository _repository;
        private readonly IMapper _mapper;

        public RecreateAndReleaseSummaryService(IRecreateAndReleaseSummaryRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<RecreateSummariesLogDto>> GetRecreateSummariesAllLogsAsync(QueryParameters<string> query)
        {
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetRecreateSummariesAllLogsAsync(parameters);
            return _mapper.Map<PaginatedResult<RecreateSummariesLogDto>>(pagedData);
        }

        public async Task<IReadOnlyList<ReleasePeriodDto>> GetReleaseSummariesAsync()
        {
            var periods = await _repository.GetReleaseSummariesAsync();
            return _mapper.Map<IReadOnlyList<ReleasePeriodDto>>(periods);
        }

        public async Task<ReleasePeriodDto?> SetFinalSummaryRunAsync(string periodName, short finalSummariesRun)
        {
            var period = await _repository.SetFinalSummaryRunAsync(periodName, finalSummariesRun);
            return _mapper.Map<ReleasePeriodDto?>(period);
        }
    }
}
