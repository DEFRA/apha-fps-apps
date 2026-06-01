using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Interfaces;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    public class ProfitCentreGradeService : IProfitCentreGradeService
    {
        private readonly IProfitCentreGradeRepository _repository;
        private readonly IMapper _mapper;

        public ProfitCentreGradeService(IProfitCentreGradeRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<ProfitCentreGradeDto>> GetProfitCentreGradesAsync(QueryParameters<string> query, string profitCentre)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(profitCentre);
            var filter = _mapper.Map<Apha.FPS.Core.Pagination.PaginationParameters<string>>(query);
            var pagedData = await _repository.GetProfitCentreGradesAsync(filter, profitCentre);
            return _mapper.Map<PaginatedResult<ProfitCentreGradeDto>>(pagedData);
        }

        public async Task<List<string>> GetAllPcGradesAsync(CancellationToken cancellationToken = default)
            => await _repository.GetAllPcGradesAsync(cancellationToken);
    }
}
