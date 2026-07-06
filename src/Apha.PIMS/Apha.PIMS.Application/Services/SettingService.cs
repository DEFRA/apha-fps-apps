/*
 * TRANSFORMENGINE MIGRATION — SettingService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Application service implementing ISettingService for Setting read/update (Time Tab, frmMaintainance)
 *   - String PK (id) — pre-seeded configuration records; no CreateAsync/DeleteAsync
 *   - Delegates all persistence to ISettingRepository; no direct DbContext usage
 *   - All methods are async end-to-end
 *   - Throws ArgumentException on null/invalid input; KeyNotFoundException when entity not found;
 *     InvalidOperationException when caller attempts to update a non-user-updateable setting
 *   - AutoMapper used for all entity <-> DTO conversions
 *
 * PRESERVED:
 *   - Userupdateable guard: UpdateAsync enforces Userupdateable == true before persisting — prevents non-admin callers from modifying protected settings
 *   - GetAllUserUpdateableAsync delegates directly to repository filter; no service-layer re-filter needed
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Testsetting edits should be restricted to non-production environments — consider environment guard in UpdateAsync
 */

using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using AutoMapper;

namespace Apha.PIMS.Application.Services
{
    // TRANSFORMENGINE: service orchestrates ISettingRepository; string PK; no add/delete (pre-seeded config); enforces Userupdateable guard
    public class SettingService : ISettingService
    {
        private readonly ISettingRepository _repository;
        private readonly IMapper _mapper;

        public SettingService(ISettingRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // TRANSFORMENGINE: returns all settings — admin listing
        public async Task<List<SettingDto>> GetAllAsync()
        {
            List<Setting> entities = await _repository.GetAllAsync();
            return _mapper.Map<List<SettingDto>>(entities);
        }

        // TRANSFORMENGINE: returns only settings where Userupdateable == true — used by non-admin user edit flows
        public async Task<List<SettingDto>> GetAllUserUpdateableAsync()
        {
            List<Setting> entities = await _repository.GetAllUserUpdateableAsync();
            return _mapper.Map<List<SettingDto>>(entities);
        }

        // TRANSFORMENGINE: returns nullable — controller maps null to 404; string PK lookup
        public async Task<SettingDto?> GetByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Setting id is required.", nameof(id));

            Setting? entity = await _repository.GetByIdAsync(id);
            return entity is null ? null : _mapper.Map<SettingDto>(entity);
        }

        // TRANSFORMENGINE: Userupdateable guard — throws InvalidOperationException if caller tries to update a non-user-updateable setting
        public async Task<SettingDto> UpdateAsync(SettingDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Id))
                throw new ArgumentException("Setting id is required.", nameof(dto));

            Setting? existing = await _repository.GetByIdAsync(dto.Id);
            if (existing is null)
                throw new KeyNotFoundException($"Setting '{dto.Id}' was not found.");

            // TRANSFORMENGINE: enforce Userupdateable guard — protected settings may not be changed by standard update flow
            if (!existing.Userupdateable)
                throw new InvalidOperationException(
                    $"Setting '{dto.Id}' is not user-updateable and cannot be modified through this operation.");

            Setting entity = _mapper.Map<Setting>(dto);
            Setting updated = await _repository.UpdateAsync(entity);
            return _mapper.Map<SettingDto>(updated);
        }

        public async Task<bool> ExistsAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Setting id is required.", nameof(id));

            return await _repository.ExistsAsync(id);
        }
    }
}
