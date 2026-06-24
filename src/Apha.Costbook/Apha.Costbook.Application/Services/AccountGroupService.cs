/*
 * TRANSFORMENGINE MIGRATION — AccountGroupService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + Services
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New service implementation created for AccountGroup (CSG7) CRUD (Tab 3 of frmMaintainance)
 *   - Orchestrates IAccountGroupRepository calls and maps results via AutoMapper
 *   - Business guards extracted from VBA form validation and JS duplicate-check patterns:
 *       - AddAsync: validates non-null Csg7Group; rejects duplicate key
 *       - UpdateAsync: validates non-null csg7Group route key; throws if record missing
 *       - DeleteAsync: throws if record missing
 *   - Property mapping: AccountGroupDto.UseInflation (bool) ↔ AccountGroup.Useinflation (bool?)
 *
 * PRESERVED:
 *   - All async-only patterns per Application layer convention
 *   - No direct DbContext usage — repository-only orchestration
 *   - ArgumentException for invalid input; KeyNotFoundException for missing records
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm Csg7Group varchar(15) max-length validation is enforced at controller level
 */

using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Interfaces;
using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Interfaces;
using AutoMapper;

namespace Apha.Costbook.Application.Services
{
    // TRANSFORMENGINE: Service implementation for IAccountGroupService — full CRUD for mabarchive.tblcsg7_accountgroups
    public class AccountGroupService : IAccountGroupService
    {
        private readonly IAccountGroupRepository _repository;
        private readonly IMapper _mapper;

        public AccountGroupService(IAccountGroupRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        // TRANSFORMENGINE: GetAllAsync — maps List<AccountGroup> → List<AccountGroupDto>; used for grid + CSG7 dropdown in Tab 2
        public async Task<List<AccountGroupDto>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            return _mapper.Map<List<AccountGroupDto>>(entities);
        }

        // TRANSFORMENGINE: GetByCsg7GroupAsync — single-record lookup; returns null if not found
        public async Task<AccountGroupDto?> GetByCsg7GroupAsync(string csg7Group)
        {
            if (string.IsNullOrWhiteSpace(csg7Group))
                throw new ArgumentException("Csg7Group must not be null or empty.", nameof(csg7Group));

            var entity = await _repository.GetByCsg7GroupAsync(csg7Group);
            return entity is null ? null : _mapper.Map<AccountGroupDto>(entity);
        }

        // TRANSFORMENGINE: AddAsync — validates non-null PK and uniqueness before insert; mirrors JS duplicate guard in formTblCsg7
        public async Task<AccountGroupDto> AddAsync(AccountGroupDto dto)
        {
            if (dto is null)
                throw new ArgumentException("AccountGroupDto must not be null.", nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Csg7Group))
                throw new ArgumentException("Csg7Group must not be null or empty.", nameof(dto));

            // TRANSFORMENGINE: Duplicate guard — mirrors VBA/JS uniqueness check before INSERT on tblcsg7_accountgroups
            var exists = await _repository.ExistsAsync(dto.Csg7Group);
            if (exists)
                throw new ArgumentException($"An AccountGroup with Csg7Group '{dto.Csg7Group}' already exists.", nameof(dto));

            var entity = _mapper.Map<AccountGroup>(dto);
            var created = await _repository.AddAsync(entity);
            return _mapper.Map<AccountGroupDto>(created);
        }

        // TRANSFORMENGINE: UpdateAsync — validates route key; throws KeyNotFoundException if record missing
        public async Task<AccountGroupDto> UpdateAsync(string csg7Group, AccountGroupDto dto)
        {
            if (string.IsNullOrWhiteSpace(csg7Group))
                throw new ArgumentException("Csg7Group must not be null or empty.", nameof(csg7Group));
            if (dto is null)
                throw new ArgumentException("AccountGroupDto must not be null.", nameof(dto));

            // TRANSFORMENGINE: Existence guard — throws 404-mapping exception if record not found
            var exists = await _repository.ExistsAsync(csg7Group);
            if (!exists)
                throw new KeyNotFoundException($"AccountGroup with Csg7Group '{csg7Group}' was not found.");

            // TRANSFORMENGINE: Ensure entity key matches route key (prevents body PK injection)
            dto.Csg7Group = csg7Group;
            var entity = _mapper.Map<AccountGroup>(dto);
            var updated = await _repository.UpdateAsync(entity);
            return _mapper.Map<AccountGroupDto>(updated);
        }

        // TRANSFORMENGINE: DeleteAsync — throws KeyNotFoundException if record missing (mirrors JS delete guard)
        public async Task DeleteAsync(string csg7Group)
        {
            if (string.IsNullOrWhiteSpace(csg7Group))
                throw new ArgumentException("Csg7Group must not be null or empty.", nameof(csg7Group));

            var exists = await _repository.ExistsAsync(csg7Group);
            if (!exists)
                throw new KeyNotFoundException($"AccountGroup with Csg7Group '{csg7Group}' was not found.");

            await _repository.DeleteAsync(csg7Group);
        }
    }
}
