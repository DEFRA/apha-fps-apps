using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    /// <summary>
    /// Service implementation for project-specific component charges (TestRequirementRCCost) CRUD operations.
    /// Enforces business rules extracted from fsubTestequirementRCPrice VBA logic
    /// and fps.tbltestrequirementrccost DDL constraints.
    /// </summary>
    public class TestRequirementRCCostService : ITestRequirementRCCostService
    {
        private readonly ITestRequirementRCCostRepository _repository;
        private readonly IMapper _mapper;

        public TestRequirementRCCostService(ITestRequirementRCCostRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IEnumerable<TestRequirementRCCostDto>> GetByTestCodeAsync(string testCode, int fpsYear)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(testCode);
            if (fpsYear <= 0)
                throw new ArgumentException("FpsYear must be a positive integer.", nameof(fpsYear));

            var entities = await _repository.GetByTestCodeAsync(testCode, fpsYear);
            return _mapper.Map<IEnumerable<TestRequirementRCCostDto>>(entities);
        }

        public async Task<TestRequirementRCCostDto?> GetByKeyAsync(string testCode, string buyer, string profitCentre, int fpsYear)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(testCode);
            ArgumentException.ThrowIfNullOrWhiteSpace(buyer);
            ArgumentException.ThrowIfNullOrWhiteSpace(profitCentre);
            if (fpsYear <= 0)
                throw new ArgumentException("FpsYear must be a positive integer.", nameof(fpsYear));

            var entity = await _repository.GetByKeyAsync(testCode, buyer, profitCentre, fpsYear);
            return entity == null ? null : _mapper.Map<TestRequirementRCCostDto>(entity);
        }

        //   Guards: null check, non-empty keys, FpsYear positive, duplicate PK check
        public async Task<TestRequirementRCCostDto> CreateAsync(TestRequirementRCCostDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);
            ArgumentException.ThrowIfNullOrWhiteSpace(dto.TestCode);
            ArgumentException.ThrowIfNullOrWhiteSpace(dto.Buyer);
            ArgumentException.ThrowIfNullOrWhiteSpace(dto.ProfitCentre);
            if (dto.FpsYear <= 0)
                throw new ArgumentException("FpsYear must be a positive integer.", nameof(dto));

            var exists = await _repository.ExistsAsync(dto.TestCode, dto.Buyer, dto.ProfitCentre, dto.FpsYear);
            if (exists)
                throw new InvalidOperationException(
                    $"A TestRequirementRCCost entry with TestCode '{dto.TestCode}', Buyer '{dto.Buyer}', " +
                    $"ProfitCentre '{dto.ProfitCentre}' and FpsYear '{dto.FpsYear}' already exists.");

            var entity = _mapper.Map<TestRequirementRCCost>(dto);
            var created = await _repository.AddAsync(entity);
            return _mapper.Map<TestRequirementRCCostDto>(created);
        }

        //   Guards: non-empty keys, route-key/body-key consistency, existence check
        public async Task<TestRequirementRCCostDto> UpdateAsync(string testCode, string buyer, string profitCentre, int fpsYear, TestRequirementRCCostDto dto)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(testCode);
            ArgumentException.ThrowIfNullOrWhiteSpace(buyer);
            ArgumentException.ThrowIfNullOrWhiteSpace(profitCentre);
            if (fpsYear <= 0)
                throw new ArgumentException("FpsYear must be a positive integer.", nameof(fpsYear));
            ArgumentNullException.ThrowIfNull(dto);
            ArgumentException.ThrowIfNullOrWhiteSpace(dto.TestCode);
            ArgumentException.ThrowIfNullOrWhiteSpace(dto.Buyer);
            ArgumentException.ThrowIfNullOrWhiteSpace(dto.ProfitCentre);

            if (!string.Equals(testCode, dto.TestCode, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(buyer, dto.Buyer, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(profitCentre, dto.ProfitCentre, StringComparison.OrdinalIgnoreCase) ||
                fpsYear != dto.FpsYear)
                throw new ArgumentException(
                    "Route keys (testCode, buyer, profitCentre, fpsYear) must match the DTO body keys.");

            var existing = await _repository.GetByKeyAsync(testCode, buyer, profitCentre, fpsYear);
            if (existing == null)
                throw new KeyNotFoundException(
                    $"TestRequirementRCCost entry with TestCode '{testCode}', Buyer '{buyer}', " +
                    $"ProfitCentre '{profitCentre}' and FpsYear '{fpsYear}' was not found.");

            var entity = _mapper.Map<TestRequirementRCCost>(dto);
            var updated = await _repository.UpdateAsync(entity);
            return _mapper.Map<TestRequirementRCCostDto>(updated);
        }

        public async Task<bool> DeleteAsync(string testCode, string buyer, string profitCentre, int fpsYear)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(testCode);
            ArgumentException.ThrowIfNullOrWhiteSpace(buyer);
            ArgumentException.ThrowIfNullOrWhiteSpace(profitCentre);
            if (fpsYear <= 0)
                throw new ArgumentException("FpsYear must be a positive integer.", nameof(fpsYear));

            return await _repository.DeleteAsync(testCode, buyer, profitCentre, fpsYear);
        }
    }
}
