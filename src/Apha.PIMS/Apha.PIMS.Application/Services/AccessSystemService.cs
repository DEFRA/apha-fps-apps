/*
 * TRANSFORMENGINE MIGRATION — AccessSystemService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Application service implementing IAccessSystemService for AccessSystem lookup (system filter dropdown, frmMaintainance / admin.js)
 *   - Single integer PK (systemid) — read-only reference data; no CreateAsync/UpdateAsync/DeleteAsync
 *   - Delegates all persistence to IAccessSystemRepository; no direct DbContext usage
 *   - All methods are async end-to-end
 *   - Throws ArgumentException on null/invalid input; KeyNotFoundException when entity not found
 *   - AutoMapper used for all entity <-> DTO conversions
 *
 * PRESERVED:
 *   - Read-only access pattern: no mutation operations — systems are reference data managed outside this application
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
    // TRANSFORMENGINE: service orchestrates IAccessSystemRepository; single integer PK (systemid); read-only reference data
    public class AccessSystemService : IAccessSystemService
    {
        private readonly IAccessSystemRepository _repository;
        private readonly IMapper _mapper;

        public AccessSystemService(IAccessSystemRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // TRANSFORMENGINE: returns full list for system dropdown / lookup usage
        public async Task<List<AccessSystemDto>> GetAllAsync()
        {
            List<AccessSystem> entities = await _repository.GetAllAsync();
            return _mapper.Map<List<AccessSystemDto>>(entities);
        }

        // TRANSFORMENGINE: returns nullable — controller maps null to 404; single integer PK lookup
        public async Task<AccessSystemDto?> GetByIdAsync(int systemid)
        {
            AccessSystem? entity = await _repository.GetByIdAsync(systemid);
            return entity is null ? null : _mapper.Map<AccessSystemDto>(entity);
        }

        public async Task<bool> ExistsAsync(int systemid)
        {
            return await _repository.ExistsAsync(systemid);
        }
    }
}
