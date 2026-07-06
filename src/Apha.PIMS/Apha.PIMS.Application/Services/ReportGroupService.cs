/*
 * TRANSFORMENGINE MIGRATION — ReportGroupService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Application service implementing IReportGroupService for ReportGroup CRUD (Reports Tab group lookup, frmMaintainance)
 *   - Delegates all persistence to IReportGroupRepository; no direct DbContext usage
 *   - All methods are async end-to-end
 *   - Throws ArgumentException on null/invalid input; KeyNotFoundException when entity not found
 *   - AutoMapper used for all entity <-> DTO conversions
 *
 * PRESERVED:
 *   - All business guards: null-input validation, not-found checks
 *   - Group description required guard on create/update
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
    // TRANSFORMENGINE: service orchestrates IReportGroupRepository; backed by EF Core in Infrastructure layer
    public class ReportGroupService : IReportGroupService
    {
        private readonly IReportGroupRepository _repository;
        private readonly IMapper _mapper;

        public ReportGroupService(IReportGroupRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // TRANSFORMENGINE: returns full list for report-group dropdown / lookup
        public async Task<List<ReportGroupDto>> GetAllAsync()
        {
            List<ReportGroup> entities = await _repository.GetAllAsync();
            return _mapper.Map<List<ReportGroupDto>>(entities);
        }

        // TRANSFORMENGINE: returns nullable — controller maps null to 404
        public async Task<ReportGroupDto?> GetByIdAsync(int groupid)
        {
            ReportGroup? entity = await _repository.GetByIdAsync(groupid);
            return entity is null ? null : _mapper.Map<ReportGroupDto>(entity);
        }

        // TRANSFORMENGINE: validate non-null DTO before first await
        public async Task<ReportGroupDto> CreateAsync(ReportGroupDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Description))
                throw new ArgumentException("Group description is required.", nameof(dto));

            ReportGroup entity = _mapper.Map<ReportGroup>(dto);
            ReportGroup created = await _repository.AddAsync(entity);
            return _mapper.Map<ReportGroupDto>(created);
        }

        // TRANSFORMENGINE: validate existence before update — throws KeyNotFoundException if not found
        public async Task<ReportGroupDto> UpdateAsync(ReportGroupDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Description))
                throw new ArgumentException("Group description is required.", nameof(dto));

            bool exists = await _repository.ExistsAsync(dto.Groupid);
            if (!exists)
                throw new KeyNotFoundException($"ReportGroup with groupid {dto.Groupid} was not found.");

            ReportGroup entity = _mapper.Map<ReportGroup>(dto);
            ReportGroup updated = await _repository.UpdateAsync(entity);
            return _mapper.Map<ReportGroupDto>(updated);
        }

        // TRANSFORMENGINE: throws KeyNotFoundException if not found before delete
        public async Task DeleteAsync(int groupid)
        {
            bool exists = await _repository.ExistsAsync(groupid);
            if (!exists)
                throw new KeyNotFoundException($"ReportGroup with groupid {groupid} was not found.");

            await _repository.DeleteAsync(groupid);
        }

        public async Task<bool> ExistsAsync(int groupid)
        {
            return await _repository.ExistsAsync(groupid);
        }
    }
}
