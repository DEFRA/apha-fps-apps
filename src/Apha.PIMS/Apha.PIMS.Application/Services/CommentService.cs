/*
 * TRANSFORMENGINE MIGRATION — CommentService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services
 * Migrated : 2026-07-22
 *
 * CHANGED:
 *   - MS Access form operations (frmtblComments RecordSource + VBA CRUD code-behind) → async service class implementing ICommentService
 *   - GetCommentsByProjectAsync: optional `string? topic` parameter added (forwarded to repository when ICommentRepository is updated in Phase 4)
 *   - AddAsync: server-side `DateEntered = DateTime.UtcNow` replaces SQL Server trigger UI_tblComments (INSERT path)
 *   - AddAsync: duplicate guard (ExistsAsync) enforces unique index ix_tblcomments (project, year, topic)
 *   - UpdateAsync: server-side field mapping replaces VBA bound-form Save operation; preserves existing DateEntered
 *   - Validation: BusinessValidationErrorException thrown on missing Project/Year/Topic (replaces Access Required property on controls)
 *
 * PRESERVED:
 *   - All 6 public method bodies and every conditional branch (validation, duplicate check, null guard, update field assignments)
 *   - DateEntered set as DateTimeKind.Unspecified to match PostgreSQL timestamptz convention
 *   - MadeBy forwarded from DTO (controller injects current user in Phase 5)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - DEFERRED: none — topic parameter forwarding to ICommentRepository completed in Phase 4.
 */
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using Apha.PIMS.Application.Validation;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using AutoMapper;

namespace Apha.PIMS.Application.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _repository;
        private readonly IMapper _mapper;

        public CommentService(ICommentRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        // TRANSFORMENGINE: optional topic parameter now forwarded to repository (Phase 4 completion)
        public async Task<PaginatedResult<CommentDto>> GetCommentsByProjectAsync(string project, int? year, QueryParameters<string> query, string? topic = null)
        {
            PaginationParameters<string> filter = _mapper.Map<PaginationParameters<string>>(query);
            PagedData<Comment> result = await _repository.GetCommentsByProjectAsync(project, year, filter, topic);
            return _mapper.Map<PaginatedResult<CommentDto>>(result);
        }

        public async Task<CommentDto?> GetByIdAsync(int CommentNo)
        {
            Comment? entity = await _repository.GetByIdAsync(CommentNo);
            return entity is null ? null : _mapper.Map<CommentDto>(entity);
        }



        public async Task<CommentDto> AddAsync(CommentDto dto)
        {
            var errors = new List<BusinessValidationError>();

            if (string.IsNullOrWhiteSpace(dto.Project))
                errors.Add(new BusinessValidationError("Project is required.", "PROJECT_REQUIRED"));

            if (dto.Year is null or 0)
                errors.Add(new BusinessValidationError("Year is required.", "YEAR_REQUIRED"));

            if (string.IsNullOrWhiteSpace(dto.Topic))
                errors.Add(new BusinessValidationError("Topic is required.", "TOPIC_REQUIRED"));

            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            
            bool duplicate = await _repository.ExistsAsync(dto.Project!, (short)dto.Year!.Value, dto.Topic!);
            if (duplicate)
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError(
                        $"A comment for project '{dto.Project}', year '{dto.Year}', topic '{dto.Topic}' already exists.",
                        "COMMENT_DUPLICATE")
                ]);

            Comment entity = _mapper.Map<Comment>(dto);
            entity.DateEntered = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            Comment created = await _repository.AddAsync(entity);
            return _mapper.Map<CommentDto>(created);
        }

        public async Task<CommentDto> UpdateAsync(CommentDto dto)
        {
            var errors = new List<BusinessValidationError>();

            if (string.IsNullOrWhiteSpace(dto.Project))
                errors.Add(new BusinessValidationError("Project is required.", "PROJECT_REQUIRED"));

            if (dto.Year is null or 0)
                errors.Add(new BusinessValidationError("Year is required.", "YEAR_REQUIRED"));

            if (string.IsNullOrWhiteSpace(dto.Topic))
                errors.Add(new BusinessValidationError("Topic is required.", "TOPIC_REQUIRED"));

            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            Comment existing = await _repository.GetByIdAsync(dto.CommentNo)
                ?? throw new KeyNotFoundException($"Comment {dto.CommentNo} not found.");

            existing.Project = dto.Project!;
            existing.Year = (short)dto.Year!.Value;
            existing.Topic = dto.Topic!;
            existing.CommentText = dto.CommentText;
            existing.MadeBy = dto.MadeBy;
            Comment updated = await _repository.UpdateAsync(existing);
            return _mapper.Map<CommentDto>(updated);
        }

        public async Task<bool> DeleteAsync(int CommentNo)
        {
            return await _repository.DeleteAsync(CommentNo);
        }

        public async Task<IEnumerable<CommentTopicDto>> GetCommentTopicsAsync()
        {
            var topics = await _repository.GetCommentTopicsAsync();
            return _mapper.Map<IEnumerable<CommentTopicDto>>(topics);
        }
    }
}
