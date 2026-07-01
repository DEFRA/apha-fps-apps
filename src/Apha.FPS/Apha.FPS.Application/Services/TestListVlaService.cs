/*
 * TRANSFORMENGINE MIGRATION — TestListVlaService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - New service implementation for TestOrProduct VLA list operations
 *   - Orchestrates ITestListVlaRepository via async calls; no direct DbContext usage
 *   - Business guards extracted from VBA/SP logic:
 *     - Duplicate PK check before insert (ExistsAsync) -> InvalidOperationException
 *     - owner CHECK constraint validation (PT/PA/SD/LT) in CreateAsync and UpdateAsync
 *     - ItemCode null/whitespace guard on all key-bearing operations
 *     - FpsYear range guard (year must be positive) on key-bearing operations
 *   - Route-key consistency check on UpdateAsync (route keys must match DTO body keys)
 *   - Existence check before UpdateAsync -> KeyNotFoundException if not found
 *   - AutoMapper used for entity <-> DTO round-trips
 *
 * PRESERVED:
 *   - All async call chains (GetPagedAsync, GetAllByYearAsync, GetByKeyAsync, ExistsAsync, AddAsync, UpdateAsync, DeleteAsync)
 *   - Business conditional branches for owner validation, duplicate check, existence check
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Verify allowed owner values (PT/PA/SD/LT) are correct per latest DDL
 *     CHECK constraint — update ValidOwnerValues array if constraint changes.
 */

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
    /// Service implementation for TestOrProduct VLA list operations.
    /// Enforces business rules extracted from frmTestList / fsubTest_MainList VBA logic
    /// and fps.testorproduct DDL constraints.
    /// </summary>
    public class TestListVlaService : ITestListVlaService
    {
        private readonly ITestListVlaRepository _repository;
        private readonly IMapper _mapper;

        // TRANSFORMENGINE: owner CHECK constraint values from fps.testorproduct DDL
        private static readonly string[] ValidOwnerValues = { "PT", "PA", "SD", "LT" };

        public TestListVlaService(ITestListVlaRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // TRANSFORMENGINE: GetAllAsync — paged list for GET /api/v1/testlistvla?fpsYear={year}
        public async Task<PaginatedResult<TestListVlaDto>> GetAllAsync(QueryParameters<string> query, int fpsYear)
        {
            ArgumentNullException.ThrowIfNull(query);
            if (fpsYear <= 0)
                throw new ArgumentException("FpsYear must be a positive integer.", nameof(fpsYear));

            var paginationParams = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetPagedAsync(paginationParams, fpsYear);
            return _mapper.Map<PaginatedResult<TestListVlaDto>>(pagedData);
        }

        // TRANSFORMENGINE: GetAllByYearAsync — unpaged list for lookup/select-list use case
        public async Task<IEnumerable<TestListVlaDto>> GetAllByYearAsync(int fpsYear)
        {
            if (fpsYear <= 0)
                throw new ArgumentException("FpsYear must be a positive integer.", nameof(fpsYear));

            var entities = await _repository.GetAllByYearAsync(fpsYear);
            return _mapper.Map<IEnumerable<TestListVlaDto>>(entities);
        }

        // TRANSFORMENGINE: GetByKeyAsync — single record fetch for GET /api/v1/testlistvla/{itemCode}/{fpsYear}
        public async Task<TestListVlaDto?> GetByKeyAsync(string itemCode, int fpsYear)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(itemCode);
            if (fpsYear <= 0)
                throw new ArgumentException("FpsYear must be a positive integer.", nameof(fpsYear));

            var entity = await _repository.GetByKeyAsync(itemCode, fpsYear);
            return entity == null ? null : _mapper.Map<TestListVlaDto>(entity);
        }

        // TRANSFORMENGINE: CreateAsync — POST /api/v1/testlistvla
        //   Guards: null check, non-empty keys, valid owner value, duplicate PK check
        public async Task<TestListVlaDto> CreateAsync(TestListVlaDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);
            ArgumentException.ThrowIfNullOrWhiteSpace(dto.ItemCode);
            if (dto.FpsYear <= 0)
                throw new ArgumentException("FpsYear must be a positive integer.", nameof(dto));

            // TRANSFORMENGINE: owner CHECK constraint — service enforces (PT/PA/SD/LT) per fps.testorproduct DDL
            if (dto.Owner != null && !ValidOwnerValues.Contains(dto.Owner))
                throw new ArgumentException(
                    $"Owner value '{dto.Owner}' is not valid. Allowed values: {string.Join(", ", ValidOwnerValues)}.",
                    nameof(dto));

            // TRANSFORMENGINE: Duplicate PK guard — avoids composite PK violation on fps.testorproduct
            var exists = await _repository.ExistsAsync(dto.ItemCode, dto.FpsYear);
            if (exists)
                throw new InvalidOperationException(
                    $"A TestOrProduct VLA entry with ItemCode '{dto.ItemCode}' and FpsYear '{dto.FpsYear}' already exists.");

            var entity = _mapper.Map<TestOrProduct>(dto);
            var created = await _repository.AddAsync(entity);
            return _mapper.Map<TestListVlaDto>(created);
        }

        // TRANSFORMENGINE: UpdateAsync — PUT /api/v1/testlistvla/{itemCode}/{fpsYear}
        //   Guards: null check, route-key/body-key consistency, existence check, owner value validation
        public async Task<TestListVlaDto> UpdateAsync(string itemCode, int fpsYear, TestListVlaDto dto)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(itemCode);
            if (fpsYear <= 0)
                throw new ArgumentException("FpsYear must be a positive integer.", nameof(fpsYear));
            ArgumentNullException.ThrowIfNull(dto);
            ArgumentException.ThrowIfNullOrWhiteSpace(dto.ItemCode);

            // TRANSFORMENGINE: Route-key / body-key consistency check — prevents silent mismatched updates
            if (!string.Equals(itemCode, dto.ItemCode, StringComparison.OrdinalIgnoreCase) || fpsYear != dto.FpsYear)
                throw new ArgumentException(
                    "Route keys (itemCode, fpsYear) must match the DTO body keys.");

            // TRANSFORMENGINE: owner CHECK constraint — enforce before persistence
            if (dto.Owner != null && !ValidOwnerValues.Contains(dto.Owner))
                throw new ArgumentException(
                    $"Owner value '{dto.Owner}' is not valid. Allowed values: {string.Join(", ", ValidOwnerValues)}.",
                    nameof(dto));

            // TRANSFORMENGINE: Existence check — fail fast if record not found before attempting update
            var existing = await _repository.GetByKeyAsync(itemCode, fpsYear);
            if (existing == null)
                throw new KeyNotFoundException(
                    $"TestOrProduct VLA entry with ItemCode '{itemCode}' and FpsYear '{fpsYear}' was not found.");

            var entity = _mapper.Map<TestOrProduct>(dto);
            var updated = await _repository.UpdateAsync(entity);
            return _mapper.Map<TestListVlaDto>(updated);
        }

        // TRANSFORMENGINE: DeleteAsync — DELETE /api/v1/testlistvla/{itemCode}/{fpsYear}
        public async Task<bool> DeleteAsync(string itemCode, int fpsYear)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(itemCode);
            if (fpsYear <= 0)
                throw new ArgumentException("FpsYear must be a positive integer.", nameof(fpsYear));

            return await _repository.DeleteAsync(itemCode, fpsYear);
        }
    }
}
