/*
 * TRANSFORMENGINE MIGRATION — AccessLevelService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Application service implementing IAccessLevelService for AccessLevel CRUD/lookup (Admin Maintenance Tab access level dropdown, frmMaintainance)
 *   - Composite PK (systemid, accesslevelid) — both required for lookup/update/delete
 *   - Delegates all persistence to IAccessLevelRepository; no direct DbContext usage
 *   - All methods are async end-to-end
 *   - Throws ArgumentException on null/invalid input; KeyNotFoundException when entity not found;
 *     InvalidOperationException on duplicate-level guard
 *   - AutoMapper used for all entity <-> DTO conversions
 *
 * PRESERVED:
 *   - Duplicate-level guard: cannot create an access level that already exists for the same systemid+accesslevelid combination
 *   - GetBySystemIdAsync supports dropdown population scoped to a specific system
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated.
 */

using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using AutoMapper;

namespace Apha.PIMS.Application.Services
{
    // TRANSFORMENGINE: service orchestrates IAccessLevelRepository; composite PK (systemid, accesslevelid)
    public class AccessLevelService : IAccessLevelService
    {
        private readonly IAccessLevelRepository _repository;
        private readonly IMapper _mapper;

        public AccessLevelService(IAccessLevelRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // TRANSFORMENGINE: returns all access levels across all systems
        public async Task<List<AccessLevelDto>> GetAllAsync()
        {
            List<AccessLevel> entities = await _repository.GetAllAsync();
            return _mapper.Map<List<AccessLevelDto>>(entities);
        }

        // TRANSFORMENGINE: returns all access levels for a given system — used for dropdown population
        public async Task<List<AccessLevelDto>> GetBySystemIdAsync(int systemid)
        {
            List<AccessLevel> entities = await _repository.GetBySystemIdAsync(systemid);
            return _mapper.Map<List<AccessLevelDto>>(entities);
        }

        // TRANSFORMENGINE: returns nullable — controller maps null to 404; composite PK lookup
        public async Task<AccessLevelDto?> GetByIdAsync(int systemid, int accesslevelid)
        {
            AccessLevel? entity = await _repository.GetByIdAsync(systemid, accesslevelid);
            return entity is null ? null : _mapper.Map<AccessLevelDto>(entity);
        }

        // TRANSFORMENGINE: duplicate-level guard — throws InvalidOperationException if level already exists for this system+levelid
        public async Task<AccessLevelDto> CreateAsync(AccessLevelDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));

            bool alreadyExists = await _repository.ExistsAsync(dto.Systemid, dto.Accesslevelid);
            if (alreadyExists)
                throw new InvalidOperationException(
                    $"AccessLevel (systemid={dto.Systemid}, accesslevelid={dto.Accesslevelid}) already exists.");

            AccessLevel entity = _mapper.Map<AccessLevel>(dto);
            AccessLevel created = await _repository.AddAsync(entity);
            return _mapper.Map<AccessLevelDto>(created);
        }

        // TRANSFORMENGINE: validate existence before update — throws KeyNotFoundException if not found
        public async Task<AccessLevelDto> UpdateAsync(AccessLevelDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));

            bool exists = await _repository.ExistsAsync(dto.Systemid, dto.Accesslevelid);
            if (!exists)
                throw new KeyNotFoundException(
                    $"AccessLevel (systemid={dto.Systemid}, accesslevelid={dto.Accesslevelid}) was not found.");

            AccessLevel entity = _mapper.Map<AccessLevel>(dto);
            AccessLevel updated = await _repository.UpdateAsync(entity);
            return _mapper.Map<AccessLevelDto>(updated);
        }

        // TRANSFORMENGINE: throws KeyNotFoundException if not found before delete
        public async Task DeleteAsync(int systemid, int accesslevelid)
        {
            bool exists = await _repository.ExistsAsync(systemid, accesslevelid);
            if (!exists)
                throw new KeyNotFoundException(
                    $"AccessLevel (systemid={systemid}, accesslevelid={accesslevelid}) was not found.");

            await _repository.DeleteAsync(systemid, accesslevelid);
        }

        public async Task<bool> ExistsAsync(int systemid, int accesslevelid)
        {
            return await _repository.ExistsAsync(systemid, accesslevelid);
        }
    }
}
