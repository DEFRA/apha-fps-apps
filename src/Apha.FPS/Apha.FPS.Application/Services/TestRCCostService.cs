/*
 * TRANSFORMENGINE MIGRATION — TestRCCostService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - New service implementation for TestRCCost (component charges per profit centre) CRUD operations
 *   - Orchestrates ITestRCCostRepository via async calls; no direct DbContext usage
 *   - Business guards extracted from fsubTestRCPrice VBA logic and fps.tbltestrccost DDL:
 *     - Non-null / non-whitespace guard on TestCode and ProfitCentre
 *     - FpsYear positive-integer guard
 *     - Duplicate PK check before insert (ExistsAsync) -> InvalidOperationException
 *     - Route-key / body-key consistency check on UpdateAsync
 *     - Existence check before UpdateAsync -> KeyNotFoundException if not found
 *   - AutoMapper used for entity <-> DTO round-trips
 *
 * PRESERVED:
 *   - All async call chains (GetByTestCodeAsync, GetByKeyAsync, ExistsAsync, AddAsync, UpdateAsync, DeleteAsync)
 *   - Business conditional branches for duplicate check, existence check
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: FK validation (TestCode+FpsYear in fps.testorproduct,
 *     ProfitCentre in fps.tblkpprofitcentre) — requires repository lookup calls to
 *     ITestListVlaRepository and a ProfitCentre repository; add when those are available.
 */

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

        // TRANSFORMENGINE: GetByTestCodeAsync — list all profit-centre charges for GET /api/v1/testrccost/{testCode}/{fpsYear}
        public async Task<IEnumerable<TestRCCostDto>> GetByTestCodeAsync(string testCode, int fpsYear)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(testCode);
            if (fpsYear <= 0)
                throw new ArgumentException("FpsYear must be a positive integer.", nameof(fpsYear));

            var entities = await _repository.GetByTestCodeAsync(testCode, fpsYear);
            return _mapper.Map<IEnumerable<TestRCCostDto>>(entities);
        }

        // TRANSFORMENGINE: GetByKeyAsync — single record by composite PK for edit/delete confirmation
        public async Task<TestRCCostDto?> GetByKeyAsync(string testCode, string profitCentre, int fpsYear)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(testCode);
            ArgumentException.ThrowIfNullOrWhiteSpace(profitCentre);
            if (fpsYear <= 0)
                throw new ArgumentException("FpsYear must be a positive integer.", nameof(fpsYear));

            var entity = await _repository.GetByKeyAsync(testCode, profitCentre, fpsYear);
            return entity == null ? null : _mapper.Map<TestRCCostDto>(entity);
        }

        // TRANSFORMENGINE: CreateAsync — POST /api/v1/testrccost
        //   Guards: null check, non-empty keys, FpsYear positive, duplicate PK check
        public async Task<TestRCCostDto> CreateAsync(TestRCCostDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);
            ArgumentException.ThrowIfNullOrWhiteSpace(dto.TestCode);
            ArgumentException.ThrowIfNullOrWhiteSpace(dto.ProfitCentre);
            if (dto.FpsYear <= 0)
                throw new ArgumentException("FpsYear must be a positive integer.", nameof(dto));

            // TRANSFORMENGINE: Duplicate PK guard — avoids composite PK violation on fps.tbltestrccost
            var exists = await _repository.ExistsAsync(dto.TestCode, dto.ProfitCentre, dto.FpsYear);
            if (exists)
                throw new InvalidOperationException(
                    $"A TestRCCost entry with TestCode '{dto.TestCode}', ProfitCentre '{dto.ProfitCentre}' " +
                    $"and FpsYear '{dto.FpsYear}' already exists.");

            var entity = _mapper.Map<TestRCCost>(dto);
            var created = await _repository.AddAsync(entity);
            return _mapper.Map<TestRCCostDto>(created);
        }

        // TRANSFORMENGINE: UpdateAsync — PUT /api/v1/testrccost/{testCode}/{profitCentre}/{fpsYear}
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

            // TRANSFORMENGINE: Route-key / body-key consistency check — prevents silent mismatched updates
            if (!string.Equals(testCode, dto.TestCode, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(profitCentre, dto.ProfitCentre, StringComparison.OrdinalIgnoreCase) ||
                fpsYear != dto.FpsYear)
                throw new ArgumentException(
                    "Route keys (testCode, profitCentre, fpsYear) must match the DTO body keys.");

            // TRANSFORMENGINE: Existence check — fail fast if record not found before attempting update
            var existing = await _repository.GetByKeyAsync(testCode, profitCentre, fpsYear);
            if (existing == null)
                throw new KeyNotFoundException(
                    $"TestRCCost entry with TestCode '{testCode}', ProfitCentre '{profitCentre}' " +
                    $"and FpsYear '{fpsYear}' was not found.");

            var entity = _mapper.Map<TestRCCost>(dto);
            var updated = await _repository.UpdateAsync(entity);
            return _mapper.Map<TestRCCostDto>(updated);
        }

        // TRANSFORMENGINE: DeleteAsync — DELETE /api/v1/testrccost/{testCode}/{profitCentre}/{fpsYear}
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
