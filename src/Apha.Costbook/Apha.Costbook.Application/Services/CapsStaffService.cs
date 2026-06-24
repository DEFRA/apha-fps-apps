/*
 * TRANSFORMENGINE MIGRATION — CapsStaffService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + Services
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New service implementation created for CAPS Staff CRUD (Tab 5 of frmMaintainance)
 *   - Orchestrates ICapsStaffRepository calls and maps results via AutoMapper
 *   - Business guards extracted from VBA form validation and JS duplicate-check patterns:
 *       - AddAsync: validates non-null MNumber/Name; rejects duplicate MNumber
 *       - UpdateAsync: validates non-null mNumber route key; throws if record missing
 *       - DeleteAsync: throws if record missing
 *   - GetPaginatedAsync delegates pagination to repository (preserves source paging contract)
 *
 * PRESERVED:
 *   - All async-only patterns per Application layer convention
 *   - No direct DbContext usage — repository-only orchestration
 *   - ArgumentException for invalid input; KeyNotFoundException for missing records
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Verify MNumber uniqueness constraint is also enforced at DB level (not just in service)
 */

using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Interfaces;
using Apha.Costbook.Application.Pagination;
using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.Core.Pagination;
using AutoMapper;

namespace Apha.Costbook.Application.Services
{
    // TRANSFORMENGINE: Service implementation for ICapsStaffService — full CRUD for mabarchive.tblcapsstaff
    public class CapsStaffService : ICapsStaffService
    {
        private readonly ICapsStaffRepository _repository;
        private readonly IMapper _mapper;

        public CapsStaffService(ICapsStaffRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        // TRANSFORMENGINE: GetAllAsync — maps List<CapsStaff> → List<CapsStaffDto>
        public async Task<List<CapsStaffDto>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            return _mapper.Map<List<CapsStaffDto>>(entities);
        }

        // TRANSFORMENGINE: GetPaginatedAsync — delegates pagination parameters to repository then maps paged result
        public async Task<PaginatedResult<CapsStaffDto>> GetPaginatedAsync(QueryParameters<string> queryParameters)
        {
            if (queryParameters == null)
                throw new ArgumentException("Query parameters must not be null.", nameof(queryParameters));

            // TRANSFORMENGINE: Map application QueryParameters<string> → core PaginationParameters<string>
            var coreParams = _mapper.Map<PaginationParameters<string>>(queryParameters);
            var pagedData = await _repository.GetPaginatedAsync(coreParams);

            // TRANSFORMENGINE: Map paged Core entity result → paged Application DTO result
            var pagedResult = _mapper.Map<PaginatedResult<CapsStaffDto>>(pagedData);
            return pagedResult;
        }

        // TRANSFORMENGINE: GetByMNumberAsync — single-record lookup; returns null if not found (controller maps to 404)
        public async Task<CapsStaffDto?> GetByMNumberAsync(string mNumber)
        {
            if (string.IsNullOrWhiteSpace(mNumber))
                throw new ArgumentException("MNumber must not be null or empty.", nameof(mNumber));

            var entity = await _repository.GetByMNumberAsync(mNumber);
            return entity is null ? null : _mapper.Map<CapsStaffDto>(entity);
        }

        // TRANSFORMENGINE: AddAsync — validates non-null PK and uniqueness before insert; mirrors JS duplicate guard in formTblCapsStaff
        public async Task<CapsStaffDto> AddAsync(CapsStaffDto dto)
        {
            if (dto is null)
                throw new ArgumentException("CapsStaffDto must not be null.", nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.MNumber))
                throw new ArgumentException("MNumber must not be null or empty.", nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Name must not be null or empty.", nameof(dto));

            // TRANSFORMENGINE: Duplicate guard — mirrors VBA/JS uniqueness check before INSERT
            var exists = await _repository.ExistsAsync(dto.MNumber);
            if (exists)
                throw new ArgumentException($"A CAPS staff member with MNumber '{dto.MNumber}' already exists.", nameof(dto));

            var entity = _mapper.Map<CapsStaff>(dto);
            var created = await _repository.AddAsync(entity);
            return _mapper.Map<CapsStaffDto>(created);
        }

        // TRANSFORMENGINE: UpdateAsync — validates route key; throws KeyNotFoundException if record missing
        public async Task<CapsStaffDto> UpdateAsync(string mNumber, CapsStaffDto dto)
        {
            if (string.IsNullOrWhiteSpace(mNumber))
                throw new ArgumentException("MNumber must not be null or empty.", nameof(mNumber));
            if (dto is null)
                throw new ArgumentException("CapsStaffDto must not be null.", nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Name must not be null or empty.", nameof(dto));

            // TRANSFORMENGINE: Existence guard — throws 404-mapping exception if record not found
            var exists = await _repository.ExistsAsync(mNumber);
            if (!exists)
                throw new KeyNotFoundException($"CAPS staff member with MNumber '{mNumber}' was not found.");

            // TRANSFORMENGINE: Ensure entity key matches route key (prevents body PK injection)
            dto.MNumber = mNumber;
            var entity = _mapper.Map<CapsStaff>(dto);
            var updated = await _repository.UpdateAsync(entity);
            return _mapper.Map<CapsStaffDto>(updated);
        }

        // TRANSFORMENGINE: DeleteAsync — throws KeyNotFoundException if record missing (mirrors JS delete guard)
        public async Task DeleteAsync(string mNumber)
        {
            if (string.IsNullOrWhiteSpace(mNumber))
                throw new ArgumentException("MNumber must not be null or empty.", nameof(mNumber));

            var exists = await _repository.ExistsAsync(mNumber);
            if (!exists)
                throw new KeyNotFoundException($"CAPS staff member with MNumber '{mNumber}' was not found.");

            await _repository.DeleteAsync(mNumber);
        }
    }
}
