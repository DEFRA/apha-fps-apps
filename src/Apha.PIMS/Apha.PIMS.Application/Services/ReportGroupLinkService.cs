/*
 * TRANSFORMENGINE MIGRATION — ReportGroupLinkService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Application service implementing IReportGroupLinkService for ReportGroupLink CRUD (Reports Tab sub-grid, frmMaintainance)
 *   - Composite PK (reportid, groupid) — no UpdateAsync (link table: add/delete only)
 *   - Delegates all persistence to IReportGroupLinkRepository; no direct DbContext usage
 *   - All methods are async end-to-end
 *   - Throws ArgumentException on null/invalid input; KeyNotFoundException when entity not found;
 *     InvalidOperationException on duplicate-link guard
 *   - AutoMapper used for all entity <-> DTO conversions
 *
 * PRESERVED:
 *   - Duplicate-link guard: cannot add a link that already exists (composite PK uniqueness enforced at service layer)
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
    // TRANSFORMENGINE: service orchestrates IReportGroupLinkRepository; composite PK (reportid, groupid); no update — link table
    public class ReportGroupLinkService : IReportGroupLinkService
    {
        private readonly IReportGroupLinkRepository _repository;
        private readonly IMapper _mapper;

        public ReportGroupLinkService(IReportGroupLinkRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // TRANSFORMENGINE: returns full list of all report-group links
        public async Task<List<ReportGroupLinkDto>> GetAllAsync()
        {
            List<ReportGroupLink> entities = await _repository.GetAllAsync();
            return _mapper.Map<List<ReportGroupLinkDto>>(entities);
        }

        // TRANSFORMENGINE: returns all group links for a given report — used for sub-grid population
        public async Task<List<ReportGroupLinkDto>> GetByReportIdAsync(int reportid)
        {
            List<ReportGroupLink> entities = await _repository.GetByReportIdAsync(reportid);
            return _mapper.Map<List<ReportGroupLinkDto>>(entities);
        }

        // TRANSFORMENGINE: returns nullable — controller maps null to 404; composite PK lookup
        public async Task<ReportGroupLinkDto?> GetByIdAsync(int reportid, int groupid)
        {
            ReportGroupLink? entity = await _repository.GetByIdAsync(reportid, groupid);
            return entity is null ? null : _mapper.Map<ReportGroupLinkDto>(entity);
        }

        // TRANSFORMENGINE: duplicate-link guard — throws InvalidOperationException if link already exists
        public async Task<ReportGroupLinkDto> CreateAsync(ReportGroupLinkDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));

            bool alreadyExists = await _repository.ExistsAsync(dto.Reportid, dto.Groupid);
            if (alreadyExists)
                throw new InvalidOperationException(
                    $"ReportGroupLink (reportid={dto.Reportid}, groupid={dto.Groupid}) already exists.");

            ReportGroupLink entity = _mapper.Map<ReportGroupLink>(dto);
            ReportGroupLink created = await _repository.AddAsync(entity);
            return _mapper.Map<ReportGroupLinkDto>(created);
        }

        // TRANSFORMENGINE: throws KeyNotFoundException if link not found before delete
        public async Task DeleteAsync(int reportid, int groupid)
        {
            bool exists = await _repository.ExistsAsync(reportid, groupid);
            if (!exists)
                throw new KeyNotFoundException(
                    $"ReportGroupLink (reportid={reportid}, groupid={groupid}) was not found.");

            await _repository.DeleteAsync(reportid, groupid);
        }

        public async Task<bool> ExistsAsync(int reportid, int groupid)
        {
            return await _repository.ExistsAsync(reportid, groupid);
        }
    }
}
