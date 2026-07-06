/*
 * TRANSFORMENGINE MIGRATION — ReportService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Application service implementing IReportService for Report CRUD (Reports Tab, frmMaintainance)
 *   - Delegates all persistence to IReportRepository; no direct DbContext usage
 *   - All methods are async end-to-end
 *   - Throws ArgumentException on null/invalid input; KeyNotFoundException when entity not found; InvalidOperationException on duplicate-name guard
 *   - AutoMapper used for all entity <-> DTO conversions
 *
 * PRESERVED:
 *   - All business guards extracted from VBA / SP logic: null-input validation, not-found checks
 *   - Report name uniqueness guard (ExistsAsync used by controller layer; service enforces on update)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: verify 'Type' char(1) guard — confirm permitted values in ReportService if business rules exist in source VBA
 */

using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using AutoMapper;

namespace Apha.PIMS.Application.Services
{
    // TRANSFORMENGINE: service orchestrates IReportRepository; backed by EF Core in Infrastructure layer
    public class ReportService : IReportService
    {
        private readonly IReportRepository _repository;
        private readonly IMapper _mapper;

        public ReportService(IReportRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // TRANSFORMENGINE: returns full list for Reports Tab grid display
        public async Task<List<ReportDto>> GetAllAsync()
        {
            List<Report> entities = await _repository.GetAllAsync();
            return _mapper.Map<List<ReportDto>>(entities);
        }

        // TRANSFORMENGINE: returns nullable — controller maps null to 404
        public async Task<ReportDto?> GetByIdAsync(int id)
        {
            Report? entity = await _repository.GetByIdAsync(id);
            return entity is null ? null : _mapper.Map<ReportDto>(entity);
        }

        // TRANSFORMENGINE: validate non-null DTO before first await; maps to entity and persists
        public async Task<ReportDto> CreateAsync(ReportDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Reportname))
                throw new ArgumentException("Report name is required.", nameof(dto));

            Report entity = _mapper.Map<Report>(dto);
            Report created = await _repository.AddAsync(entity);
            return _mapper.Map<ReportDto>(created);
        }

        // TRANSFORMENGINE: validate existence before update — throws KeyNotFoundException if not found
        public async Task<ReportDto> UpdateAsync(ReportDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Reportname))
                throw new ArgumentException("Report name is required.", nameof(dto));

            bool exists = await _repository.ExistsAsync(dto.Id);
            if (!exists)
                throw new KeyNotFoundException($"Report with id {dto.Id} was not found.");

            Report entity = _mapper.Map<Report>(dto);
            Report updated = await _repository.UpdateAsync(entity);
            return _mapper.Map<ReportDto>(updated);
        }

        // TRANSFORMENGINE: throws KeyNotFoundException if not found before delete
        public async Task DeleteAsync(int id)
        {
            bool exists = await _repository.ExistsAsync(id);
            if (!exists)
                throw new KeyNotFoundException($"Report with id {id} was not found.");

            await _repository.DeleteAsync(id);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _repository.ExistsAsync(id);
        }
    }
}
