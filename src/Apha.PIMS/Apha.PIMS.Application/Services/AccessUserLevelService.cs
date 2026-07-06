/*
 * TRANSFORMENGINE MIGRATION — AccessUserLevelService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Application service implementing IAccessUserLevelService for AccessUserLevel CRUD (Admin Maintenance Tab user-access grid, frmMaintainance)
 *   - Three-column composite PK (systemid, ntlogin, accesslevelid) — no UpdateAsync (assignment table: add/delete only)
 *   - Delegates all persistence to IAccessUserLevelRepository; no direct DbContext usage
 *   - All methods are async end-to-end
 *   - Throws ArgumentException on null/invalid input; KeyNotFoundException when entity not found;
 *     InvalidOperationException on duplicate-assignment guard
 *   - AutoMapper used for all entity <-> DTO conversions
 *
 * PRESERVED:
 *   - Duplicate-assignment guard: cannot add a user-level assignment that already exists (three-column PK uniqueness enforced at service layer)
 *   - GetByUserAsync supports user access management grid (all level assignments for a given user)
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
    // TRANSFORMENGINE: service orchestrates IAccessUserLevelRepository; three-column composite PK (systemid, ntlogin, accesslevelid); no update — assignment table
    public class AccessUserLevelService : IAccessUserLevelService
    {
        private readonly IAccessUserLevelRepository _repository;
        private readonly IMapper _mapper;

        public AccessUserLevelService(IAccessUserLevelRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // TRANSFORMENGINE: returns all user-level assignments across all systems
        public async Task<List<AccessUserLevelDto>> GetAllAsync()
        {
            List<AccessUserLevel> entities = await _repository.GetAllAsync();
            return _mapper.Map<List<AccessUserLevelDto>>(entities);
        }

        // TRANSFORMENGINE: returns all user-level assignments for a given system
        public async Task<List<AccessUserLevelDto>> GetBySystemIdAsync(int systemid)
        {
            List<AccessUserLevel> entities = await _repository.GetBySystemIdAsync(systemid);
            return _mapper.Map<List<AccessUserLevelDto>>(entities);
        }

        // TRANSFORMENGINE: returns all level assignments for a given user — used for user access management grid
        public async Task<List<AccessUserLevelDto>> GetByUserAsync(int systemid, string ntlogin)
        {
            if (string.IsNullOrWhiteSpace(ntlogin))
                throw new ArgumentException("NT login is required.", nameof(ntlogin));

            List<AccessUserLevel> entities = await _repository.GetByUserAsync(systemid, ntlogin);
            return _mapper.Map<List<AccessUserLevelDto>>(entities);
        }

        // TRANSFORMENGINE: returns nullable — controller maps null to 404; three-column composite PK lookup
        public async Task<AccessUserLevelDto?> GetByIdAsync(int systemid, string ntlogin, int accesslevelid)
        {
            if (string.IsNullOrWhiteSpace(ntlogin))
                throw new ArgumentException("NT login is required.", nameof(ntlogin));

            AccessUserLevel? entity = await _repository.GetByIdAsync(systemid, ntlogin, accesslevelid);
            return entity is null ? null : _mapper.Map<AccessUserLevelDto>(entity);
        }

        // TRANSFORMENGINE: duplicate-assignment guard — throws InvalidOperationException if assignment already exists
        public async Task<AccessUserLevelDto> CreateAsync(AccessUserLevelDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Ntlogin))
                throw new ArgumentException("NT login is required.", nameof(dto));

            bool alreadyExists = await _repository.ExistsAsync(dto.Systemid, dto.Ntlogin, dto.Accesslevelid);
            if (alreadyExists)
                throw new InvalidOperationException(
                    $"AccessUserLevel (systemid={dto.Systemid}, ntlogin='{dto.Ntlogin}', accesslevelid={dto.Accesslevelid}) already exists.");

            AccessUserLevel entity = _mapper.Map<AccessUserLevel>(dto);
            AccessUserLevel created = await _repository.AddAsync(entity);
            return _mapper.Map<AccessUserLevelDto>(created);
        }

        // TRANSFORMENGINE: throws KeyNotFoundException if assignment not found before delete
        public async Task DeleteAsync(int systemid, string ntlogin, int accesslevelid)
        {
            if (string.IsNullOrWhiteSpace(ntlogin))
                throw new ArgumentException("NT login is required.", nameof(ntlogin));

            bool exists = await _repository.ExistsAsync(systemid, ntlogin, accesslevelid);
            if (!exists)
                throw new KeyNotFoundException(
                    $"AccessUserLevel (systemid={systemid}, ntlogin='{ntlogin}', accesslevelid={accesslevelid}) was not found.");

            await _repository.DeleteAsync(systemid, ntlogin, accesslevelid);
        }

        public async Task<bool> ExistsAsync(int systemid, string ntlogin, int accesslevelid)
        {
            if (string.IsNullOrWhiteSpace(ntlogin))
                throw new ArgumentException("NT login is required.", nameof(ntlogin));

            return await _repository.ExistsAsync(systemid, ntlogin, accesslevelid);
        }
    }
}
