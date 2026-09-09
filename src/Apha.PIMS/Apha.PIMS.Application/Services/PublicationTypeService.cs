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
    public class PublicationTypeService : IPublicationTypeService
    {
        private readonly IPublicationTypeRepository _repository;
        private readonly IMapper _mapper;

        public PublicationTypeService(IPublicationTypeRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Helper method for trimmed, case-insensitive string comparison.
        /// </summary>
        private static bool StringEqualsTrimmedIgnoreCase(string? a, string? b)
        {
            var trimmedA = (a ?? string.Empty).Trim();
            var trimmedB = (b ?? string.Empty).Trim();
            return string.Equals(trimmedA, trimmedB, StringComparison.OrdinalIgnoreCase);
        }

        public async Task<List<PublicationTypeDto>> GetAllPublicationTypesAsync()
        {
            List<PublicationType> entities = await _repository.GetAllPublicationTypesAsync();
            return _mapper.Map<List<PublicationTypeDto>>(entities);
        }

        public async Task<PaginatedResult<PublicationTypeDto>> GetPagedPublicationTypesAsync(QueryParameters<string> query)
        {
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetPagedPublicationTypesAsync(parameters);
            return _mapper.Map<PaginatedResult<PublicationTypeDto>>(pagedData);
        }

        public async Task<PublicationTypeDto?> GetPublicationTypeByCodeAsync(string type)
        {
            PublicationType? entity = await _repository.GetPublicationTypeByCodeAsync(type);
            return entity is null ? null : _mapper.Map<PublicationTypeDto>(entity);
        }

        public async Task<PublicationTypeDto> CreatePublicationTypeAsync(PublicationTypeDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));

            // Normalize the type with trimmed value
            var normalizedType = (dto.Type ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(normalizedType))
            {
                var errors = new List<BusinessValidationError>
                {
                    new BusinessValidationError("Type code is required.", "TYPE_REQUIRED")
                };
                throw new BusinessValidationErrorException(errors);
            }

            // Check if publication type with trimmed, case-insensitive match already exists
            var existingType = await _repository.GetPublicationTypeByCodeAsync(normalizedType);
            if (existingType != null)
            {
                var errors = new List<BusinessValidationError>
                {
                    new BusinessValidationError(
                        $"Publication type '{normalizedType}' already exists.",
                        "PUBLICATION_TYPE_ALREADY_EXISTS")
                };
                throw new BusinessValidationErrorException(errors);
            }

            // Create with normalized type
            var entity = _mapper.Map<PublicationType>(dto);
            entity.Type = normalizedType;

            PublicationType created = await _repository.AddPublicationTypeAsync(entity);
            return _mapper.Map<PublicationTypeDto>(created);
        }

        public async Task<PublicationTypeDto> UpdatePublicationTypeAsync(PublicationTypeDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));

            var normalizedType = (dto.Type ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(normalizedType))
            {
                var errors = new List<BusinessValidationError>
                {
                    new BusinessValidationError("Type code is required.", "TYPE_REQUIRED")
                };
                throw new BusinessValidationErrorException(errors);
            }

            var existing = await _repository.GetPublicationTypeByCodeAsync(normalizedType);
            if (existing is null)
            {
                var errors = new List<BusinessValidationError>
                {
                    new BusinessValidationError(
                        $"Publication type '{normalizedType}' was not found.",
                        "PUBLICATION_TYPE_NOT_FOUND")
                };
                throw new BusinessValidationErrorException(errors);
            }

            var entity = _mapper.Map<PublicationType>(dto);
            entity.Type = normalizedType;

            PublicationType updated = await _repository.UpdatePublicationTypeAsync(entity);
            return _mapper.Map<PublicationTypeDto>(updated);
        }

        public async Task<bool> DeletePublicationTypeAsync(string type)
        {
            var normalizedType = (type ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(normalizedType))
            {
                var errors = new List<BusinessValidationError>
                {
                    new BusinessValidationError("Type code is required.", "TYPE_REQUIRED")
                };
                throw new BusinessValidationErrorException(errors);
            }

            var existing = await _repository.PublicationTypeExistsAsync(normalizedType);
            if (!existing)
            {
                var errors = new List<BusinessValidationError>
                {
                    new BusinessValidationError(
                        $"Publication type '{normalizedType}' was not found.",
                        "PUBLICATION_TYPE_NOT_FOUND")
                };
                throw new BusinessValidationErrorException(errors);
            }

            return await _repository.DeletePublicationTypeAsync(normalizedType);
        }

        public async Task<bool> PublicationTypeExistsAsync(string type)
        {
            return await _repository.PublicationTypeExistsAsync(type);
        }
    }
}
