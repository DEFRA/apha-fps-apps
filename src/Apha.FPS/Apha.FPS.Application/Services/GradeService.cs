// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — GradeService.cs (new file)
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet8-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services
 * Migrated : 2026-06-10
 *
 * CHANGED:
 *   - New service implementation created — no prior equivalent existed in the codebase
 *   - Business guard checks extracted from frmMaintGrade VBA and SP analysis:
 *       CreateAsync: null check, duplicate GradeCode guard
 *       UpdateAsync: null checks, existence guard, rename conflict guard
 *       DeleteAsync: null check, existence guard
 *   - All methods delegate to IGradeRepository; no direct DbContext usage
 *   - AutoMapper used for Grade <-> GradeDto mapping (Description <-> DescLong handled in EntityMapper)
 *   - Pattern follows DivisionService as the canonical reference
 *
 * PRESERVED:
 *   - All business validation branches (duplicate check, existence check, rename conflict check)
 *   - Composite key pattern: GradeCode used for lookup; FpsYear resolved by DbContext HasQueryFilter
 *   - ArgumentNullException.ThrowIfNull used consistently with existing codebase pattern
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether foreign key reference checks are needed for DeleteAsync
 *     (e.g. fps.divisiongrade, fps.workgroupgrade reference fps.grade). If so, add a
 *     GetGradeForeignKeyReferencesAsync() method to IGradeRepository and call it here before delete.
 */

using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    /// <summary>
    /// Service implementation for Grade maintenance business logic.
    /// Orchestrates repository calls and enforces business rules extracted from frmMaintGrade VBA analysis.
    /// </summary>
    public class GradeService : IGradeService
    {
        private readonly IGradeRepository _gradeRepository;
        private readonly IMapper _mapper;

        public GradeService(
            IGradeRepository gradeRepository,
            IMapper mapper)
        {
            _gradeRepository = gradeRepository ?? throw new ArgumentNullException(nameof(gradeRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // TRANSFORMENGINE: GetAllPagedAsync — maps QueryParameters to Core PaginationParameters, delegates to repository, maps result back to DTO
        /// <inheritdoc />
        public async Task<PaginatedResult<GradeDto>> GetAllPagedAsync(QueryParameters<string> query)
        {
            ArgumentNullException.ThrowIfNull(query);

            var queryParams = _mapper.Map<Apha.FPS.Core.Pagination.PaginationParameters<string>>(query);
            var pagedGrades = await _gradeRepository.GetAllPagedAsync(queryParams);
            return _mapper.Map<PaginatedResult<GradeDto>>(pagedGrades);
        }

        // TRANSFORMENGINE: GetByIdAsync — look up single grade; FpsYear resolved via DbContext HasQueryFilter
        /// <inheritdoc />
        public async Task<GradeDto?> GetByIdAsync(string gradeCode)
        {
            if (string.IsNullOrWhiteSpace(gradeCode))
            {
                throw new ArgumentException("Grade code cannot be null or empty.", nameof(gradeCode));
            }

            var grade = await _gradeRepository.GetByIdAsync(gradeCode);
            return grade == null ? null : _mapper.Map<GradeDto>(grade);
        }

        // TRANSFORMENGINE: CreateAsync — duplicate GradeCode guard before insert; mirrors frmMaintGrade Add logic
        /// <inheritdoc />
        public async Task<GradeDto> CreateAsync(GradeDto gradeDto)
        {
            ArgumentNullException.ThrowIfNull(gradeDto);

            if (string.IsNullOrWhiteSpace(gradeDto.GradeCode))
            {
                throw new ArgumentException("Grade code is required.", nameof(gradeDto));
            }

            // Guard: check for duplicate GradeCode in the active FPS year
            var existing = await _gradeRepository.GetByIdAsync(gradeDto.GradeCode);
            if (existing != null)
            {
                throw new InvalidOperationException($"Grade '{gradeDto.GradeCode}' already exists for the current FPS year.");
            }

            var grade = _mapper.Map<Grade>(gradeDto);
            var createdGrade = await _gradeRepository.CreateAsync(grade);
            return _mapper.Map<GradeDto>(createdGrade);
        }

        // TRANSFORMENGINE: UpdateAsync — existence guard + rename conflict guard; mirrors frmMaintGrade Edit logic
        /// <inheritdoc />
        public async Task<GradeDto> UpdateAsync(string originalCode, GradeDto gradeDto)
        {
            ArgumentNullException.ThrowIfNull(gradeDto);

            if (string.IsNullOrWhiteSpace(originalCode))
            {
                throw new ArgumentException("Original grade code is required to identify the record.", nameof(originalCode));
            }

            if (string.IsNullOrWhiteSpace(gradeDto.GradeCode))
            {
                throw new ArgumentException("Grade code is required.", nameof(gradeDto));
            }

            // Guard: verify the record to update actually exists
            var existingGrade = await _gradeRepository.GetByIdAsync(originalCode);
            if (existingGrade == null)
            {
                throw new KeyNotFoundException($"Grade '{originalCode}' not found in the current FPS year.");
            }

            // Guard: if GradeCode is being renamed, ensure the new code does not already exist
            if (!originalCode.Equals(gradeDto.GradeCode, StringComparison.OrdinalIgnoreCase))
            {
                var conflictingGrade = await _gradeRepository.GetByIdAsync(gradeDto.GradeCode);
                if (conflictingGrade != null)
                {
                    throw new InvalidOperationException($"Cannot rename to '{gradeDto.GradeCode}' — a grade with that code already exists for the current FPS year.");
                }
            }

            var grade = _mapper.Map<Grade>(gradeDto);
            var updatedGrade = await _gradeRepository.UpdateAsync(originalCode, grade);
            return _mapper.Map<GradeDto>(updatedGrade);
        }

        // TRANSFORMENGINE: DeleteAsync — existence guard before delete; mirrors frmMaintGrade Delete logic
        /// <inheritdoc />
        public async Task<bool> DeleteAsync(string gradeCode)
        {
            if (string.IsNullOrWhiteSpace(gradeCode))
            {
                throw new ArgumentException("Grade code cannot be null or empty.", nameof(gradeCode));
            }

            // Guard: verify the record to delete actually exists
            var existing = await _gradeRepository.GetByIdAsync(gradeCode);
            if (existing == null)
            {
                throw new KeyNotFoundException($"Grade '{gradeCode}' not found in the current FPS year.");
            }

            // TRANSFORMENGINE TODO: Add foreign key reference check here if fps.divisiongrade or
            // fps.workgroupgrade reference fps.grade. Add GetGradeForeignKeyReferencesAsync() to
            // IGradeRepository and guard with InvalidOperationException if references exist.

            return await _gradeRepository.DeleteAsync(gradeCode);
        }
    }
}
