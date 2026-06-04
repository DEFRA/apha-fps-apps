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

        public async Task<ReleaseSummaryDto> GetReleaseSummariesAsync()
        {
            var releaseSummary = await _repository.GetReleaseSummariesAsync();
            return _mapper.Map<ReleaseSummaryDto>(releaseSummary);
        }

        public async Task<ReleasePeriodDto?> SetFinalSummaryRunAsync(string? periodName, short? finalSummariesRun, string? sendEmail)
        {
            var releasePeriod = await _repository.SetFinalSummaryRunAsync(periodName, finalSummariesRun, sendEmail);
            return _mapper.Map<ReleasePeriodDto?>(releasePeriod);
        }
    }
}
