using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
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
            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetProfitCentreGradesAsync(filter, profitCentre);
            return _mapper.Map<PaginatedResult<ProfitCentreGradeDto>>(pagedData);
        }

        public async Task<PaginatedResult<ProfitCentreGradeDto>> GetAllPagedAsync(QueryParameters<string> query)
        {
            ArgumentNullException.ThrowIfNull(query);
            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetAllPagedAsync(filter);
            return _mapper.Map<PaginatedResult<ProfitCentreGradeDto>>(pagedData);
        }

        public async Task<ProfitCentreGradeDto?> GetByIdAsync(string pcGrade)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(pcGrade);
            var entity = await _repository.GetByIdAsync(pcGrade);
            return entity is null ? null : _mapper.Map<ProfitCentreGradeDto>(entity);
        }

        public async Task<ProfitCentreGradeDto> CreateAsync(ProfitCentreGradeDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);
            var entityPcGrade = await _repository.GetByIdAsync(dto.PcGrade);
            if (entityPcGrade != null)
            {
                throw new InvalidOperationException(
                    $"Cannot insert ProfitCentreGrade because RC Grade '{dto.PcGrade}' already exists.");
            }
            // Converted trigger tI_ProfitCentreGrade — FK guard: ProfitCentre must exist in tblkpprofitcentre
            bool profitCentreExists = await _repository.ProfitCentreExistsAsync(dto.ProfitCentre);
            if (!profitCentreExists)
                throw new InvalidOperationException(
                    $"Cannot insert ProfitCentreGrade because ProfitCentre '{dto.ProfitCentre}' does not exist.");

            var entity = _mapper.Map<ProfitCentreGrade>(dto);
            var created = await _repository.CreateAsync(entity);
            return _mapper.Map<ProfitCentreGradeDto>(created);
        }

        public async Task<ProfitCentreGradeDto> UpdateAsync(string originalPcGrade, ProfitCentreGradeDto dto)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(originalPcGrade);
            ArgumentNullException.ThrowIfNull(dto);

            // Converted trigger tU_ProfitCentreGrade — FK guard: ProfitCentre must exist in tblkpprofitcentre
            bool profitCentreExists = await _repository.ProfitCentreExistsAsync(dto.ProfitCentre);
            if (!profitCentreExists)
                throw new InvalidOperationException(
                    $"Cannot update ProfitCentreGrade because ProfitCentre '{dto.ProfitCentre}' does not exist.");

            var entity = _mapper.Map<ProfitCentreGrade>(dto);
            var updated = await _repository.UpdateAsync(originalPcGrade, entity);
            return _mapper.Map<ProfitCentreGradeDto>(updated);
        }

        public async Task<bool> DeleteAsync(string pcGrade)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(pcGrade);
            return await _repository.DeleteAsync(pcGrade);
        }

        public async Task<List<string>> GetAllProfitCentreCodesAsync()
            => await _repository.GetAllProfitCentreCodesAsync();
    }
}
