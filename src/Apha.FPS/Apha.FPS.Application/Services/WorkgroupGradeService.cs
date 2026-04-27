using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    /// <summary>
    /// Service implementation for WorkgroupGrade CRUD and lookup operations.
    /// </summary>
    public class WorkgroupGradeService : IWorkgroupGradeService
    {
        private readonly IWorkgroupGradeRepository _repository;
        private readonly IMapper _mapper;

        public WorkgroupGradeService(IWorkgroupGradeRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<WorkgroupGradeDto>> GetAllWorkgroupGradesPagedAsync(
            QueryParameters<string> query, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);

            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var result = await _repository.GetAllWorkgroupGradesPagedAsync(filter, cancellationToken);
            return _mapper.Map<PaginatedResult<WorkgroupGradeDto>>(result);
        }

        public async Task<WorkgroupGradeDto?> GetByWgGradeAsync(string wgGrade, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(wgGrade))
                throw new ArgumentException("WgGrade cannot be null or empty.", nameof(wgGrade));

            var entity = await _repository.GetByWgGradeAsync(wgGrade, cancellationToken);
            return entity is null ? null : _mapper.Map<WorkgroupGradeDto>(entity);
        }

        public async Task<WorkgroupGradeDto> CreateAsync(WorkgroupGradeDto dto, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var entity = _mapper.Map<WorkgroupGrade>(dto);
            var created = await _repository.CreateAsync(entity, cancellationToken);
            return _mapper.Map<WorkgroupGradeDto>(created);
        }

        public async Task<WorkgroupGradeDto> UpdateAsync(WorkgroupGradeDto dto, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var entity = _mapper.Map<WorkgroupGrade>(dto);
            var updated = await _repository.UpdateAsync(entity, cancellationToken);
            return _mapper.Map<WorkgroupGradeDto>(updated);
        }

        public async Task<bool> DeleteAsync(string wgGrade, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(wgGrade))
                throw new ArgumentException("WgGrade cannot be null or empty.", nameof(wgGrade));

            return await _repository.DeleteAsync(wgGrade, cancellationToken);
        }

        public async Task<List<string>> GetAllPcGradesAsync(CancellationToken cancellationToken = default)
            => await _repository.GetAllPcGradesAsync(cancellationToken);

        public async Task<List<string>> GetAllGradeCodesAsync(CancellationToken cancellationToken = default)
            => await _repository.GetAllGradeCodesAsync(cancellationToken);

        public async Task<List<string>> GetAllWorkgroupNamesAsync(CancellationToken cancellationToken = default)
            => await _repository.GetAllWorkgroupNamesAsync(cancellationToken);
    }
}
