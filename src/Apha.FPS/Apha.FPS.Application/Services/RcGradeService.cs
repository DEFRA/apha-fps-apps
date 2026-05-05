using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Interfaces;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    public class RcGradeService : IRcGradeService
    {
        private readonly IRcGradeRepository _repository;
        private readonly IMapper _mapper;

        public RcGradeService(IRcGradeRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<ProfitCentreGradeDto>> GetRcGradesAsync(QueryParameters<string> query, string profitCentre, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(profitCentre);
            var filter = _mapper.Map<Apha.FPS.Core.Pagination.PaginationParameters<string>>(query);
            var pagedData = await _repository.GetRcGradesAsync(filter, profitCentre, cancellationToken);
            return _mapper.Map<PaginatedResult<ProfitCentreGradeDto>>(pagedData);
        }
    }
}
