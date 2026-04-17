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

        public async Task<PaginatedResult<CommentDto>> GetCommentsByProjectAsync(string project, int? year, QueryParameters<string> query)
        {
            PaginationParameters<string> filter = _mapper.Map<PaginationParameters<string>>(query);
            PagedData<Comment> result = await _repository.GetCommentsByProjectAsync(project, year, filter);
            return _mapper.Map<PaginatedResult<CommentDto>>(result);
        }

        public async Task<CommentDto?> GetByIdAsync(int commentno)
        {
            Comment? entity = await _repository.GetByIdAsync(commentno);
            return entity is null ? null : _mapper.Map<CommentDto>(entity);
        }

        public async Task<CommentDto> AddAsync(CommentDto dto)
        {
            var errors = new List<BusinessValidationError>();
            if (string.IsNullOrWhiteSpace(dto.Project))
                errors.Add(new BusinessValidationError("Project is required.", "PROJECT_REQUIRED"));
            if (string.IsNullOrWhiteSpace(dto.Commenttext))
                errors.Add(new BusinessValidationError("Comment text is required.", "COMMENT_TEXT_REQUIRED"));
            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            Comment entity = _mapper.Map<Comment>(dto);
            entity.Dateentered = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            Comment created = await _repository.AddAsync(entity);
            return _mapper.Map<CommentDto>(created);
        }

        public async Task<CommentDto> UpdateAsync(CommentDto dto)
        {
            var errors = new List<BusinessValidationError>();
            if (string.IsNullOrWhiteSpace(dto.Project))
                errors.Add(new BusinessValidationError("Project is required.", "PROJECT_REQUIRED"));
            if (string.IsNullOrWhiteSpace(dto.Commenttext))
                errors.Add(new BusinessValidationError("Comment text is required.", "COMMENT_TEXT_REQUIRED"));
            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            Comment entity = _mapper.Map<Comment>(dto);
            entity.Dateentered = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            Comment updated = await _repository.UpdateAsync(entity);
            return _mapper.Map<CommentDto>(updated);
        }

        public async Task<bool> DeleteAsync(int commentno)
        {
            return await _repository.DeleteAsync(commentno);
        }
    }
}
