using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    public class ProfitCentreService : IProfitCentreService
    {
        private readonly IProfitCentreRepository _repository;
        private readonly IMapper _mapper;

        public ProfitCentreService(IProfitCentreRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<ProfitCentreDto>> GetProfitCentresAsync()
        {
            var result = await _repository.GetProfitCentresAsync();
            return _mapper.Map<List<ProfitCentreDto>>(result);
        }

        public async Task<PaginatedResult<ProfitCentreDto>> GetAllProfitCentresPagedAsync(QueryParameters<string> query)
        {
            ArgumentNullException.ThrowIfNull(query);

            var queryParams = _mapper.Map<Apha.FPS.Core.Pagination.PaginationParameters<string>>(query);
            var pagedResult = await _repository.GetAllProfitCentresPagedAsync(queryParams);
            return _mapper.Map<PaginatedResult<ProfitCentreDto>>(pagedResult);
        }

        public async Task<ProfitCentreDto?> GetProfitCentreByIdAsync(string profitCentreId)
        {
            if (string.IsNullOrWhiteSpace(profitCentreId))
                throw new ArgumentException("Profit centre ID cannot be null or empty.", nameof(profitCentreId));

            var profitCentre = await _repository.GetProfitCentreByIdAsync(profitCentreId);
            return profitCentre == null ? null : _mapper.Map<ProfitCentreDto>(profitCentre);
        }

        public async Task<ProfitCentreDto> CreateProfitCentreAsync(ProfitCentreDto profitCentreDto)
        {
            ArgumentNullException.ThrowIfNull(profitCentreDto);

            if (string.IsNullOrWhiteSpace(profitCentreDto.ProfitCentreId))
                throw new ArgumentException("Profit centre ID is required.", nameof(profitCentreDto));

            if (string.IsNullOrWhiteSpace(profitCentreDto.ProfitCentreName))
                throw new ArgumentException("Profit centre name is required.", nameof(profitCentreDto));

            if (await _repository.ProfitCentreExistsAsync(profitCentreDto.ProfitCentreId))
                throw new InvalidOperationException($"Profit centre '{profitCentreDto.ProfitCentreId}' already exists.");

            var entity = _mapper.Map<ProfitCentre>(profitCentreDto);
            var created = await _repository.CreateProfitCentreAsync(entity);
            return _mapper.Map<ProfitCentreDto>(created);
        }

        public async Task<ProfitCentreDto> UpdateProfitCentreAsync(string originalProfitCentreId, ProfitCentreDto profitCentreDto)
        {
            ArgumentNullException.ThrowIfNull(profitCentreDto);

            if (string.IsNullOrWhiteSpace(originalProfitCentreId))
                throw new ArgumentException("Original profit centre ID is required.", nameof(originalProfitCentreId));

            var entity = _mapper.Map<ProfitCentre>(profitCentreDto);
            var updated = await _repository.UpdateProfitCentreAsync(originalProfitCentreId, entity);
            return _mapper.Map<ProfitCentreDto>(updated);
        }

        public async Task<bool> DeleteProfitCentreAsync(string profitCentreId)
        {
            if (string.IsNullOrWhiteSpace(profitCentreId))
                throw new ArgumentException("Profit centre ID cannot be null or empty.", nameof(profitCentreId));

            return await _repository.DeleteProfitCentreAsync(profitCentreId);
        }
    }
}
