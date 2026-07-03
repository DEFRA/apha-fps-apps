using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    /// <summary>
    /// Service implementation for component charges per profit centre (TestRCCost) CRUD operations.
    /// Enforces business rules extracted from fsubTestRCPrice VBA logic
    /// and fps.tbltestrccost DDL constraints.
    /// </summary>
    public class TestRCCostService : ITestRCCostService
    {
        private readonly ITestRCCostRepository _repository;
        private readonly IMapper _mapper;

        public TestRCCostService(ITestRCCostRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IEnumerable<TestRCCostDto>> GetByTestCodeAsync(string testCode, int fpsYear)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(testCode);
            if (fpsYear <= 0)
                throw new ArgumentException("FpsYear must be a positive integer.", nameof(fpsYear));

            var entities = await _repository.GetByTestCodeAsync(testCode, fpsYear);
            return _mapper.Map<IEnumerable<TestRCCostDto>>(entities);
        }

        public async Task<TestRCCostDto?> GetByKeyAsync(string testCode, string profitCentre, int fpsYear)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(testCode);
            ArgumentException.ThrowIfNullOrWhiteSpace(profitCentre);
            if (fpsYear <= 0)
                throw new ArgumentException("FpsYear must be a positive integer.", nameof(fpsYear));

            var entity = await _repository.GetByKeyAsync(testCode, profitCentre, fpsYear);
            return entity == null ? null : _mapper.Map<TestRCCostDto>(entity);
        }

        //   Guards: null check, non-empty keys, FpsYear positive, duplicate PK check
        public async Task<TestRCCostDto> CreateAsync(TestRCCostDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);
            ArgumentException.ThrowIfNullOrWhiteSpace(dto.TestCode);
            ArgumentException.ThrowIfNullOrWhiteSpace(dto.ProfitCentre);
            if (dto.FpsYear <= 0)
                throw new ArgumentException("FpsYear must be a positive integer.", nameof(dto));

            var exists = await _repository.ExistsAsync(dto.TestCode, dto.ProfitCentre, dto.FpsYear);
            if (exists)
                throw new InvalidOperationException(
                    $"A TestRCCost entry with TestCode '{dto.TestCode}', ProfitCentre '{dto.ProfitCentre}' " +
                    $"and FpsYear '{dto.FpsYear}' already exists.");

            var entity = _mapper.Map<TestRCCost>(dto);
            var created = await _repository.AddAsync(entity);
            return _mapper.Map<TestRCCostDto>(created);
        }

        //   Guards: non-empty keys, route-key/body-key consistency, existence check
        public async Task<TestRCCostDto> UpdateAsync(string testCode, string profitCentre, int fpsYear, TestRCCostDto dto)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(testCode);
            ArgumentException.ThrowIfNullOrWhiteSpace(profitCentre);
            if (fpsYear <= 0)
                throw new ArgumentException("FpsYear must be a positive integer.", nameof(fpsYear));
            ArgumentNullException.ThrowIfNull(dto);
            ArgumentException.ThrowIfNullOrWhiteSpace(dto.TestCode);
            ArgumentException.ThrowIfNullOrWhiteSpace(dto.ProfitCentre);

            if (!string.Equals(testCode, dto.TestCode, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(profitCentre, dto.ProfitCentre, StringComparison.OrdinalIgnoreCase) ||
                fpsYear != dto.FpsYear)
                throw new ArgumentException(
                    "Route keys (testCode, profitCentre, fpsYear) must match the DTO body keys.");

            var existing = await _repository.GetByKeyAsync(testCode, profitCentre, fpsYear);
            if (existing == null)
                throw new KeyNotFoundException(
                    $"TestRCCost entry with TestCode '{testCode}', ProfitCentre '{profitCentre}' " +
                    $"and FpsYear '{fpsYear}' was not found.");

            var entity = _mapper.Map<TestRCCost>(dto);
            var updated = await _repository.UpdateAsync(entity);
            return _mapper.Map<TestRCCostDto>(updated);
        }

        public async Task<bool> DeleteAsync(string testCode, string profitCentre, int fpsYear)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(testCode);
            ArgumentException.ThrowIfNullOrWhiteSpace(profitCentre);
            if (fpsYear <= 0)
                throw new ArgumentException("FpsYear must be a positive integer.", nameof(fpsYear));

            return await _repository.DeleteAsync(testCode, profitCentre, fpsYear);
        }
    }
}
