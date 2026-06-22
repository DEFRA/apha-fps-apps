/*
 * TRANSFORMENGINE MIGRATION — CostCentreService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - MS Access frmMaintCostCentres VBA CRUD callbacks → ICostCentreService implementation
 *   - saveTblCostCentre() → CreateCostCentreAsync: duplicate-key guard (ExistsAsync) + ProfitCentre FK validation before insert
 *   - updateTblCostCentre() → UpdateCostCentreAsync: existence check + ProfitCentre FK validation before update
 *   - handleTblCostCentreDelete() → DeleteCostCentreAsync: existence check before delete
 *   - DataGrid paged source → GetAllCostCentresPagedAsync: delegates to repository with mapped pagination params
 *   - Edit modal lookup → GetCostCentreByIdAsync: null-safe return
 *   - ProfitCentre FK guard extracted from manual-review item (transform-plan.md line 339)
 *
 * PRESERVED:
 *   - All business guards from SP/VBA analysis: duplicate prevention, FK existence check, record-not-found check
 *   - Composite PK semantics: both costCentreNo (double) and fpsYear (int) required for key operations
 *   - Async end-to-end; no blocking calls
 *   - Domain exception types: ArgumentNullException, ArgumentException, InvalidOperationException, KeyNotFoundException
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: If child FK constraints exist on fps.costcentre (tables referencing it), add HasLinkedXxxAsync guards before delete, similar to ProfitCentreService.DeleteProfitCentreAsync.
 *   - TRANSFORMENGINE TODO: Verify FpsYear is injected from a global FPS session/setting context at the controller layer rather than being passed through the DTO directly.
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
    public class CostCentreService : ICostCentreService
    {
        private readonly ICostCentreRepository _repository;
        // TRANSFORMENGINE: IProfitCentreRepository injected to validate ProfitCentre FK (transform-plan.md manual review item 1)
        private readonly IProfitCentreRepository _profitCentreRepository;
        private readonly IMapper _mapper;

        public CostCentreService(
            ICostCentreRepository repository,
            IProfitCentreRepository profitCentreRepository,
            IMapper mapper)
        {
            _repository = repository;
            _profitCentreRepository = profitCentreRepository;
            _mapper = mapper;
        }

        // TRANSFORMENGINE: GET paged — DataGrid source for fps_costcenter_maintenance.html (#gridContainer_costcenterList)
        public async Task<PaginatedResult<CostCentreDto>> GetAllCostCentresPagedAsync(QueryParameters<string> query)
        {
            ArgumentNullException.ThrowIfNull(query);

            var queryParams = _mapper.Map<PaginationParameters<string>>(query);
            var pagedResult = await _repository.GetAllPagedAsync(queryParams);
            return _mapper.Map<PaginatedResult<CostCentreDto>>(pagedResult);
        }

        // TRANSFORMENGINE: GET by composite key — populates Edit modal (modal-cc-number, modal-cc-profit)
        public async Task<CostCentreDto?> GetCostCentreByIdAsync(double costCentreNo, int fpsYear)
        {
            var entity = await _repository.GetByIdAsync(costCentreNo, fpsYear);
            return entity == null ? null : _mapper.Map<CostCentreDto>(entity);
        }

        // TRANSFORMENGINE: POST create — maps to saveTblCostCentre() in costcenter_maintenance.js
        //   Guard 1: ArgumentNullException if dto null (fail fast before any I/O)
        //   Guard 2: InvalidOperationException if composite key already exists (duplicate prevention from VBA analysis)
        //   Guard 3: InvalidOperationException if ProfitCentre FK does not exist in tblkpprofitcentre (transform-plan.md item 1)
        public async Task<CostCentreDto> CreateCostCentreAsync(CostCentreDto costCentreDto)
        {
            ArgumentNullException.ThrowIfNull(costCentreDto);
            ArgumentException.ThrowIfNullOrWhiteSpace(costCentreDto.ProfitCentre);

            // TRANSFORMENGINE: Duplicate-key guard — prevents two rows with same (CostCentreNo, FpsYear)
            if (await _repository.ExistsAsync(costCentreDto.CostCentreNo, costCentreDto.FpsYear))
                throw new InvalidOperationException(
                    $"A cost centre with number '{costCentreDto.CostCentreNo}' already exists for FPS year '{costCentreDto.FpsYear}'.");

            // TRANSFORMENGINE: ProfitCentre FK validation — guard extracted from transform-plan.md manual review item 1
            //   Validates that the supplied ProfitCentre code exists in fps.tblkpprofitcentre before inserting
            var profitCentreExists = await _profitCentreRepository.ProfitCentreExistsAsync(costCentreDto.ProfitCentre);
            if (!profitCentreExists)
                throw new InvalidOperationException(
                    $"Profit centre '{costCentreDto.ProfitCentre}' does not exist. Select a valid profit centre.");

            var entity = _mapper.Map<CostCentre>(costCentreDto);
            var created = await _repository.CreateAsync(entity);
            return _mapper.Map<CostCentreDto>(created);
        }

        // TRANSFORMENGINE: PUT update — maps to updateTblCostCentre() in costcenter_maintenance.js
        //   Guard 1: ArgumentNullException if dto null
        //   Guard 2: KeyNotFoundException if original record does not exist
        //   Guard 3: InvalidOperationException if new ProfitCentre FK does not exist in tblkpprofitcentre (transform-plan.md item 1)
        public async Task<CostCentreDto> UpdateCostCentreAsync(double originalCostCentreNo, int fpsYear, CostCentreDto costCentreDto)
        {
            ArgumentNullException.ThrowIfNull(costCentreDto);
            ArgumentException.ThrowIfNullOrWhiteSpace(costCentreDto.ProfitCentre);

            // TRANSFORMENGINE: Existence check — record must exist before update can proceed
            if (!await _repository.ExistsAsync(originalCostCentreNo, fpsYear))
                throw new KeyNotFoundException(
                    $"Cost centre '{originalCostCentreNo}' for FPS year '{fpsYear}' was not found.");

            // TRANSFORMENGINE: ProfitCentre FK validation — guard extracted from transform-plan.md manual review item 1
            var profitCentreExists = await _profitCentreRepository.ProfitCentreExistsAsync(costCentreDto.ProfitCentre);
            if (!profitCentreExists)
                throw new InvalidOperationException(
                    $"Profit centre '{costCentreDto.ProfitCentre}' does not exist. Select a valid profit centre.");

            var entity = _mapper.Map<CostCentre>(costCentreDto);
            var updated = await _repository.UpdateAsync(originalCostCentreNo, fpsYear, entity);
            return _mapper.Map<CostCentreDto>(updated);
        }

        // TRANSFORMENGINE: DELETE — maps to handleTblCostCentreDelete() in costcenter_maintenance.js
        //   Guard: KeyNotFoundException if record does not exist before attempting delete
        public async Task<bool> DeleteCostCentreAsync(double costCentreNo, int fpsYear)
        {
            // TRANSFORMENGINE: Existence check — surface a clear error if the record is already gone
            if (!await _repository.ExistsAsync(costCentreNo, fpsYear))
                throw new KeyNotFoundException(
                    $"Cost centre '{costCentreNo}' for FPS year '{fpsYear}' was not found.");

            return await _repository.DeleteAsync(costCentreNo, fpsYear);
        }
    }
}
