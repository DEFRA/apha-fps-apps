/*
 * TRANSFORMENGINE MIGRATION — ProgramManagerLinkService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Application service implementing IProgramManagerLinkService for ProgramManagerLink CRUD (Manager Tab program sub-grid, frmMaintainance)
 *   - Composite PK (program, manager) — both string — no UpdateAsync (link table: add/delete only)
 *   - Delegates all persistence to IProgramManagerLinkRepository; no direct DbContext usage
 *   - All methods are async end-to-end
 *   - Throws ArgumentException on null/invalid input; KeyNotFoundException when entity not found;
 *     InvalidOperationException on duplicate-link guard
 *   - AutoMapper used for all entity <-> DTO conversions
 *
 * PRESERVED:
 *   - Duplicate-link guard: cannot add a program-manager link that already exists
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
    // TRANSFORMENGINE: service orchestrates IProgramManagerLinkRepository; composite PK (program, manager); link table — no update
    public class ProgramManagerLinkService : IProgramManagerLinkService
    {
        private readonly IProgramManagerLinkRepository _repository;
        private readonly IMapper _mapper;

        public ProgramManagerLinkService(IProgramManagerLinkRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // TRANSFORMENGINE: returns full list of all program-manager links
        public async Task<List<ProgramManagerLinkDto>> GetAllAsync()
        {
            List<ProgramManagerLink> entities = await _repository.GetAllAsync();
            return _mapper.Map<List<ProgramManagerLinkDto>>(entities);
        }

        // TRANSFORMENGINE: returns all manager links for a given program — used for sub-grid population
        public async Task<List<ProgramManagerLinkDto>> GetByProgramAsync(string program)
        {
            if (string.IsNullOrWhiteSpace(program))
                throw new ArgumentException("Program is required.", nameof(program));

            List<ProgramManagerLink> entities = await _repository.GetByProgramAsync(program);
            return _mapper.Map<List<ProgramManagerLinkDto>>(entities);
        }

        // TRANSFORMENGINE: returns nullable — controller maps null to 404; composite PK lookup
        public async Task<ProgramManagerLinkDto?> GetByIdAsync(string program, string manager)
        {
            if (string.IsNullOrWhiteSpace(program))
                throw new ArgumentException("Program is required.", nameof(program));
            if (string.IsNullOrWhiteSpace(manager))
                throw new ArgumentException("Manager is required.", nameof(manager));

            ProgramManagerLink? entity = await _repository.GetByIdAsync(program, manager);
            return entity is null ? null : _mapper.Map<ProgramManagerLinkDto>(entity);
        }

        // TRANSFORMENGINE: duplicate-link guard — throws InvalidOperationException if link already exists
        public async Task<ProgramManagerLinkDto> CreateAsync(ProgramManagerLinkDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Program))
                throw new ArgumentException("Program is required.", nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Manager))
                throw new ArgumentException("Manager is required.", nameof(dto));

            bool alreadyExists = await _repository.ExistsAsync(dto.Program, dto.Manager);
            if (alreadyExists)
                throw new InvalidOperationException(
                    $"ProgramManagerLink (program='{dto.Program}', manager='{dto.Manager}') already exists.");

            ProgramManagerLink entity = _mapper.Map<ProgramManagerLink>(dto);
            ProgramManagerLink created = await _repository.AddAsync(entity);
            return _mapper.Map<ProgramManagerLinkDto>(created);
        }

        // TRANSFORMENGINE: throws KeyNotFoundException if link not found before delete
        public async Task DeleteAsync(string program, string manager)
        {
            if (string.IsNullOrWhiteSpace(program))
                throw new ArgumentException("Program is required.", nameof(program));
            if (string.IsNullOrWhiteSpace(manager))
                throw new ArgumentException("Manager is required.", nameof(manager));

            bool exists = await _repository.ExistsAsync(program, manager);
            if (!exists)
                throw new KeyNotFoundException(
                    $"ProgramManagerLink (program='{program}', manager='{manager}') was not found.");

            await _repository.DeleteAsync(program, manager);
        }

        public async Task<bool> ExistsAsync(string program, string manager)
        {
            if (string.IsNullOrWhiteSpace(program))
                throw new ArgumentException("Program is required.", nameof(program));
            if (string.IsNullOrWhiteSpace(manager))
                throw new ArgumentException("Manager is required.", nameof(manager));

            return await _repository.ExistsAsync(program, manager);
        }
    }
}
