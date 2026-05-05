using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Interfaces;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    public class WgGradeService : IWgGradeService
    {
        private readonly IWgGradeRepository _repository;
        private readonly IMapper _mapper;

        public WgGradeService(IWgGradeRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<WorkgroupGradeDto>> GetWgGradesAsync(QueryParameters<string> query, string pcGrade, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(pcGrade);
            var filter = _mapper.Map<Apha.FPS.Core.Pagination.PaginationParameters<string>>(query);
            var pagedData = await _repository.GetWgGradesAsync(filter, pcGrade, cancellationToken);
            return _mapper.Map<PaginatedResult<WorkgroupGradeDto>>(pagedData);
        }

        public async Task DeleteWgGradeAsync(string wgGrade, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(wgGrade);
            await _repository.DeleteWgGradeAsync(wgGrade, cancellationToken);
        }
    }
}
