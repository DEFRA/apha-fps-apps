using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    public class DivisionGradeMaintenanceService : IDivisionGradeMaintenanceService
    {
        private readonly IDivisionGradeMaintenanceRepository _divisionGradeRepository;
        private readonly IMapper _mapper;

        public DivisionGradeMaintenanceService(IDivisionGradeMaintenanceRepository divisionGradeRepository, IMapper mapper)
        {
            _divisionGradeRepository = divisionGradeRepository ?? throw new ArgumentNullException(nameof(divisionGradeRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<PaginatedResult<DivisionGradeMaintenanceDto>> GetAllPagedAsync(QueryParameters<string> query)
        {
            ArgumentNullException.ThrowIfNull(query);

            var queryParams = _mapper.Map<Apha.FPS.Core.Pagination.PaginationParameters<string>>(query);
            var pagedData = await _divisionGradeRepository.GetAllPagedAsync(queryParams);
            return _mapper.Map<PaginatedResult<DivisionGradeMaintenanceDto>>(pagedData);
        }

        public async Task<DivisionGradeMaintenanceDto?> GetByIdAsync(string divisionGradeCode)
        {
            if (string.IsNullOrWhiteSpace(divisionGradeCode))
                throw new ArgumentException("Division grade code cannot be null or empty.", nameof(divisionGradeCode));

            var entity = await _divisionGradeRepository.GetByIdAsync(divisionGradeCode);
            return entity == null ? null : _mapper.Map<DivisionGradeMaintenanceDto>(entity);
        }

        public async Task<DivisionGradeMaintenanceDto> CreateAsync(DivisionGradeMaintenanceDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var entity = _mapper.Map<DivisionGradeMaintenance>(dto);
            var created = await _divisionGradeRepository.CreateAsync(entity);
            return _mapper.Map<DivisionGradeMaintenanceDto>(created);
        }

        public async Task<DivisionGradeMaintenanceDto> UpdateAsync(string originalCode, DivisionGradeMaintenanceDto dto)
        {
            if (string.IsNullOrWhiteSpace(originalCode))
                throw new ArgumentException("Original division grade code is required.", nameof(originalCode));

            ArgumentNullException.ThrowIfNull(dto);

            var entity = _mapper.Map<DivisionGradeMaintenance>(dto);
            var updated = await _divisionGradeRepository.UpdateAsync(originalCode, entity);
            return _mapper.Map<DivisionGradeMaintenanceDto>(updated);
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
    }
}
