using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    public class DivisionGradeService : IDivisionGradeService
    {
        private readonly IDivisionGradeRepository _divisionGradeRepository;
        private readonly IMapper _mapper;

        public DivisionGradeService(IDivisionGradeRepository divisionGradeRepository, IMapper mapper)
        {
            _divisionGradeRepository = divisionGradeRepository ?? throw new ArgumentNullException(nameof(divisionGradeRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<PaginatedResult<DivisionGradeDto>> GetAllPagedAsync(QueryParameters<string> query)
        {
            ArgumentNullException.ThrowIfNull(query);

            var queryParams = _mapper.Map<Apha.FPS.Core.Pagination.PaginationParameters<string>>(query);
            var pagedData = await _divisionGradeRepository.GetAllPagedAsync(queryParams);
            return _mapper.Map<PaginatedResult<DivisionGradeDto>>(pagedData);
        }

        public async Task<DivisionGradeDto?> GetByIdAsync(string divisionGradeCode)
        {
            if (string.IsNullOrWhiteSpace(divisionGradeCode))
                throw new ArgumentException("Division grade code cannot be null or empty.", nameof(divisionGradeCode));

            var entity = await _divisionGradeRepository.GetByIdAsync(divisionGradeCode);
            return entity == null ? null : _mapper.Map<DivisionGradeDto>(entity);
        }

        public async Task<DivisionGradeDto> CreateAsync(DivisionGradeDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var entity = _mapper.Map<DivisionGrade>(dto);
            var created = await _divisionGradeRepository.CreateAsync(entity);
            return _mapper.Map<DivisionGradeDto>(created);
        }

        public async Task<DivisionGradeDto> UpdateAsync(string originalCode, DivisionGradeDto dto)
        {
            if (string.IsNullOrWhiteSpace(originalCode))
                throw new ArgumentException("Original division grade code is required.", nameof(originalCode));

            ArgumentNullException.ThrowIfNull(dto);

            var entity = _mapper.Map<DivisionGrade>(dto);
            var updated = await _divisionGradeRepository.UpdateAsync(originalCode, entity);
            return _mapper.Map<DivisionGradeDto>(updated);
        }

        public async Task<bool> DeleteAsync(string divisionGradeCode)
        {
            if (string.IsNullOrWhiteSpace(divisionGradeCode))
                throw new ArgumentException("Division grade code cannot be null or empty.", nameof(divisionGradeCode));

            return await _divisionGradeRepository.DeleteAsync(divisionGradeCode);
        }

        public async Task<List<string>> GetAllGradeCodesAsync()
        {
            return await _divisionGradeRepository.GetAllGradeCodesAsync();
        }

        public async Task<List<string>> GetAllDivisionGradeCodesAsync()
        {
            return await _divisionGradeRepository.GetAllDivisionGradeCodesAsync();
        }
    }
}
