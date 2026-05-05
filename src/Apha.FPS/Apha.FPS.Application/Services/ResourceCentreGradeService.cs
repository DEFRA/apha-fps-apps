using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Interfaces;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    public class ResourceCentreGradeService : IResourceCentreGradeService
    {
        private readonly IResourceCentreGradeRepository _repository;
        private readonly IMapper _mapper;

        public ResourceCentreGradeService(IResourceCentreGradeRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<ProfitCentreGradeDto>> GetResourceCentreGradesAsync(QueryParameters<string> query, string profitCentre)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(profitCentre);
            var filter = _mapper.Map<Apha.FPS.Core.Pagination.PaginationParameters<string>>(query);
            var pagedData = await _repository.GetResourceCentreGradesAsync(filter, profitCentre);
            return _mapper.Map<PaginatedResult<ProfitCentreGradeDto>>(pagedData);
        }
    }
}
