using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Core.Interfaces;
using AutoMapper;

namespace Apha.PACT.Application.Services
{
    public class RecreateAndReleaseSummaryService : IRecreateAndReleaseSummaryService
    {
        private readonly IRecreateSummariesLogRepository _repository;
        private readonly IMapper _mapper;

        public RecreateAndReleaseSummaryService(IRecreateSummariesLogRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<RecreateSummariesLogDto>> GetAllLogsAsync()
        {
            var logs = await _repository.GetAllLogsAsync();
            return _mapper.Map<IEnumerable<RecreateSummariesLogDto>>(logs);
        }
    }
}
