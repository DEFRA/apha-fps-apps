/*
 * TRANSFORMENGINE MIGRATION — ProjectManagerService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Application service implementing IProjectManagerService for ProjectManager CRUD (Manager Tab, frmMaintainance)
 *   - String PK (projectmanager name) — not identity-generated
 *   - Delegates all persistence to IProjectManagerRepository; no direct DbContext usage
 *   - All methods are async end-to-end
 *   - Throws ArgumentException on null/invalid input; KeyNotFoundException when entity not found;
 *     InvalidOperationException on duplicate-name guard
 *   - AutoMapper used for all entity <-> DTO conversions
 *
 * PRESERVED:
 *   - Duplicate-name guard: cannot create a manager with an already-existing name (string PK uniqueness)
 *   - Disable flag preserved — used to soft-disable managers without deletion
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
    // TRANSFORMENGINE: service orchestrates IProjectManagerRepository; string PK (projectmanager name)
    public class ProjectManagerService : IProjectManagerService
    {
        private readonly IProjectManagerRepository _repository;
        private readonly IMapper _mapper;

        public ProjectManagerService(IProjectManagerRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // TRANSFORMENGINE: returns full list for manager lookup/dropdown
        public async Task<List<ProjectManagerDto>> GetAllAsync()
        {
            List<ProjectManager> entities = await _repository.GetAllAsync();
            return _mapper.Map<List<ProjectManagerDto>>(entities);
        }

        // TRANSFORMENGINE: returns nullable — controller maps null to 404; string PK lookup
        public async Task<ProjectManagerDto?> GetByIdAsync(string projectmanager)
        {
            if (string.IsNullOrWhiteSpace(projectmanager))
                throw new ArgumentException("Project manager name is required.", nameof(projectmanager));

            ProjectManager? entity = await _repository.GetByIdAsync(projectmanager);
            return entity is null ? null : _mapper.Map<ProjectManagerDto>(entity);
        }

        // TRANSFORMENGINE: duplicate-name guard — throws InvalidOperationException if manager name already exists
        public async Task<ProjectManagerDto> CreateAsync(ProjectManagerDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Projectmanager))
                throw new ArgumentException("Project manager name is required.", nameof(dto));

            bool alreadyExists = await _repository.ExistsAsync(dto.Projectmanager);
            if (alreadyExists)
                throw new InvalidOperationException(
                    $"ProjectManager '{dto.Projectmanager}' already exists.");

            ProjectManager entity = _mapper.Map<ProjectManager>(dto);
            ProjectManager created = await _repository.AddAsync(entity);
            return _mapper.Map<ProjectManagerDto>(created);
        }

        // TRANSFORMENGINE: validate existence before update — throws KeyNotFoundException if not found
        public async Task<ProjectManagerDto> UpdateAsync(ProjectManagerDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Projectmanager))
                throw new ArgumentException("Project manager name is required.", nameof(dto));

            bool exists = await _repository.ExistsAsync(dto.Projectmanager);
            if (!exists)
                throw new KeyNotFoundException($"ProjectManager '{dto.Projectmanager}' was not found.");

            ProjectManager entity = _mapper.Map<ProjectManager>(dto);
            ProjectManager updated = await _repository.UpdateAsync(entity);
            return _mapper.Map<ProjectManagerDto>(updated);
        }

        // TRANSFORMENGINE: throws KeyNotFoundException if not found before delete
        public async Task DeleteAsync(string projectmanager)
        {
            if (string.IsNullOrWhiteSpace(projectmanager))
                throw new ArgumentException("Project manager name is required.", nameof(projectmanager));

            bool exists = await _repository.ExistsAsync(projectmanager);
            if (!exists)
                throw new KeyNotFoundException($"ProjectManager '{projectmanager}' was not found.");

            await _repository.DeleteAsync(projectmanager);
        }

        public async Task<bool> ExistsAsync(string projectmanager)
        {
            if (string.IsNullOrWhiteSpace(projectmanager))
                throw new ArgumentException("Project manager name is required.", nameof(projectmanager));

            return await _repository.ExistsAsync(projectmanager);
        }
    }
}
