/*
 * TRANSFORMENGINE MIGRATION — ProfitCentreManagerLinkService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Application service implementing IProfitCentreManagerLinkService for ProfitCentreManagerLink CRUD (Manager Tab resource centre sub-grid, frmMaintainance)
 *   - Composite PK (profitcentre, manager) — both string — no UpdateAsync (link table: add/delete only)
 *   - Delegates all persistence to IProfitCentreManagerLinkRepository; no direct DbContext usage
 *   - All methods are async end-to-end
 *   - Throws ArgumentException on null/invalid input; KeyNotFoundException when entity not found;
 *     InvalidOperationException on duplicate-link guard
 *   - AutoMapper used for all entity <-> DTO conversions
 *
 * PRESERVED:
 *   - Duplicate-link guard: cannot add a profit-centre-manager link that already exists
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
    // TRANSFORMENGINE: service orchestrates IProfitCentreManagerLinkRepository; composite PK (profitcentre, manager); link table — no update
    public class ProfitCentreManagerLinkService : IProfitCentreManagerLinkService
    {
        private readonly IProfitCentreManagerLinkRepository _repository;
        private readonly IMapper _mapper;

        public ProfitCentreManagerLinkService(IProfitCentreManagerLinkRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // TRANSFORMENGINE: returns full list of all profit-centre-manager links
        public async Task<List<ProfitCentreManagerLinkDto>> GetAllAsync()
        {
            List<ProfitCentreManagerLink> entities = await _repository.GetAllAsync();
            return _mapper.Map<List<ProfitCentreManagerLinkDto>>(entities);
        }

        // TRANSFORMENGINE: returns all manager links for a given profit centre — used for sub-grid population
        public async Task<List<ProfitCentreManagerLinkDto>> GetByProfitCentreAsync(string profitcentre)
        {
            if (string.IsNullOrWhiteSpace(profitcentre))
                throw new ArgumentException("Profit centre is required.", nameof(profitcentre));

            List<ProfitCentreManagerLink> entities = await _repository.GetByProfitCentreAsync(profitcentre);
            return _mapper.Map<List<ProfitCentreManagerLinkDto>>(entities);
        }

        // TRANSFORMENGINE: returns nullable — controller maps null to 404; composite PK lookup
        public async Task<ProfitCentreManagerLinkDto?> GetByIdAsync(string profitcentre, string manager)
        {
            if (string.IsNullOrWhiteSpace(profitcentre))
                throw new ArgumentException("Profit centre is required.", nameof(profitcentre));
            if (string.IsNullOrWhiteSpace(manager))
                throw new ArgumentException("Manager is required.", nameof(manager));

            ProfitCentreManagerLink? entity = await _repository.GetByIdAsync(profitcentre, manager);
            return entity is null ? null : _mapper.Map<ProfitCentreManagerLinkDto>(entity);
        }

        // TRANSFORMENGINE: duplicate-link guard — throws InvalidOperationException if link already exists
        public async Task<ProfitCentreManagerLinkDto> CreateAsync(ProfitCentreManagerLinkDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Profitcentre))
                throw new ArgumentException("Profit centre is required.", nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Manager))
                throw new ArgumentException("Manager is required.", nameof(dto));

            bool alreadyExists = await _repository.ExistsAsync(dto.Profitcentre, dto.Manager);
            if (alreadyExists)
                throw new InvalidOperationException(
                    $"ProfitCentreManagerLink (profitcentre='{dto.Profitcentre}', manager='{dto.Manager}') already exists.");

            ProfitCentreManagerLink entity = _mapper.Map<ProfitCentreManagerLink>(dto);
            ProfitCentreManagerLink created = await _repository.AddAsync(entity);
            return _mapper.Map<ProfitCentreManagerLinkDto>(created);
        }

        // TRANSFORMENGINE: throws KeyNotFoundException if link not found before delete
        public async Task DeleteAsync(string profitcentre, string manager)
        {
            if (string.IsNullOrWhiteSpace(profitcentre))
                throw new ArgumentException("Profit centre is required.", nameof(profitcentre));
            if (string.IsNullOrWhiteSpace(manager))
                throw new ArgumentException("Manager is required.", nameof(manager));

            bool exists = await _repository.ExistsAsync(profitcentre, manager);
            if (!exists)
                throw new KeyNotFoundException(
                    $"ProfitCentreManagerLink (profitcentre='{profitcentre}', manager='{manager}') was not found.");

            await _repository.DeleteAsync(profitcentre, manager);
        }

        public async Task<bool> ExistsAsync(string profitcentre, string manager)
        {
            if (string.IsNullOrWhiteSpace(profitcentre))
                throw new ArgumentException("Profit centre is required.", nameof(profitcentre));
            if (string.IsNullOrWhiteSpace(manager))
                throw new ArgumentException("Manager is required.", nameof(manager));

            return await _repository.ExistsAsync(profitcentre, manager);
        }
    }
}
