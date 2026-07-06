/*
 * TRANSFORMENGINE MIGRATION — RadTrackProgService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 5 — API Layer - Controller + RequestMapper + DI (Steps 8-9)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Application service implementing IRadTrackProgService for RadTrackProg CRUD (Programme Tab, frmPIMSMainForm)
 *   - Natural string PK (program varchar(10)) — client-supplied natural key
 *   - Delegates all persistence to IRadTrackProgRepository; no direct DbContext usage
 *   - All methods are async end-to-end
 *   - Throws ArgumentException on null/invalid input; KeyNotFoundException when entity not found
 *   - AutoMapper used for all entity <-> DTO conversions
 *
 * PRESERVED:
 *   - All business guards: null-input validation, not-found checks before update/delete
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
    // TRANSFORMENGINE: service orchestrates IRadTrackProgRepository; natural string PK (program varchar(10)); Programme Tab
    public class RadTrackProgService : IRadTrackProgService
    {
        private readonly IRadTrackProgRepository _repository;
        private readonly IMapper _mapper;

        public RadTrackProgService(IRadTrackProgRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // TRANSFORMENGINE: returns full list for Programme Tab administration
        public async Task<List<RadTrackProgDto>> GetAllAsync()
        {
            List<RadtrackProg> entities = await _repository.GetAllAsync();
            return _mapper.Map<List<RadTrackProgDto>>(entities);
        }

        // TRANSFORMENGINE: returns nullable — controller maps null to 404; natural string PK lookup
        public async Task<RadTrackProgDto?> GetByIdAsync(string program)
        {
            if (string.IsNullOrWhiteSpace(program)) throw new ArgumentException("program must not be empty.", nameof(program));

            RadtrackProg? entity = await _repository.GetByIdAsync(program);
            return entity is null ? null : _mapper.Map<RadTrackProgDto>(entity);
        }

        // TRANSFORMENGINE: validate non-null DTO before first await
        public async Task<RadTrackProgDto> CreateAsync(RadTrackProgDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Program)) throw new ArgumentException("Program must not be empty.", nameof(dto));

            RadtrackProg entity = _mapper.Map<RadtrackProg>(dto);
            RadtrackProg created = await _repository.AddAsync(entity);
            return _mapper.Map<RadTrackProgDto>(created);
        }

        // TRANSFORMENGINE: validate existence before update — throws KeyNotFoundException if not found
        public async Task<RadTrackProgDto> UpdateAsync(RadTrackProgDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Program)) throw new ArgumentException("Program must not be empty.", nameof(dto));

            bool exists = await _repository.ExistsAsync(dto.Program);
            if (!exists)
                throw new KeyNotFoundException($"RadTrackProg with program '{dto.Program}' was not found.");

            RadtrackProg entity = _mapper.Map<RadtrackProg>(dto);
            RadtrackProg updated = await _repository.UpdateAsync(entity);
            return _mapper.Map<RadTrackProgDto>(updated);
        }

        // TRANSFORMENGINE: throws KeyNotFoundException if not found before delete
        public async Task DeleteAsync(string program)
        {
            if (string.IsNullOrWhiteSpace(program)) throw new ArgumentException("program must not be empty.", nameof(program));

            bool exists = await _repository.ExistsAsync(program);
            if (!exists)
                throw new KeyNotFoundException($"RadTrackProg with program '{program}' was not found.");

            await _repository.DeleteAsync(program);
        }

        public async Task<bool> ExistsAsync(string program)
        {
            if (string.IsNullOrWhiteSpace(program)) return false;
            return await _repository.ExistsAsync(program);
        }
    }
}
