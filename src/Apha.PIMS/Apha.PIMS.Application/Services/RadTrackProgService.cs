using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using Apha.PIMS.Application.Validation;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using MapsterMapper;

namespace Apha.PIMS.Application.Services
{
    public class RadTrackProgService : IRadTrackProgService
    {
        private readonly IRadTrackProgRepository _repository;
        private readonly IMapper _mapper;

        public RadTrackProgService(IRadTrackProgRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<List<RadTrackProgDto>> GetAllRadTrackProgsAsync()
        {
            List<RadtrackProg> entities = await _repository.GetAllRadTrackProgsAsync();
            return _mapper.Map<List<RadTrackProgDto>>(entities);
        }

        public async Task<PaginatedResult<RadTrackProgDto>> GetPagedRadTrackProgsAsync(QueryParameters<string> query)
        {
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetPagedRadTrackProgsAsync(parameters);
            return _mapper.Map<PaginatedResult<RadTrackProgDto>>(pagedData);
        }

        public async Task<RadTrackProgDto?> GetRadTrackProgByProgramAsync(string program)
        {
            if (string.IsNullOrWhiteSpace(program)) return null;

            // Trim the program name to normalize it (remove leading/trailing spaces)
            program = program.Trim();

            // Load all programs and find the one matching case-insensitively
            var allPrograms = await _repository.GetAllRadTrackProgsAsync();
            var entity = allPrograms.FirstOrDefault(p => StringEqualsTrimmedIgnoreCase(p.Program, program));
            return entity is null ? null : _mapper.Map<RadTrackProgDto>(entity);
        }

        
        public async Task<RadTrackProgDto> CreateRadTrackProgAsync(RadTrackProgDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            string? normalizedProgram = dto.Program?.Trim();

            if (string.IsNullOrWhiteSpace(normalizedProgram))
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError("Program is required.", "PROGRAM_REQUIRED")
                ]);

            dto.Program = normalizedProgram;

            // Check if program already exists (case-insensitive, trimmed comparison)
            var existingPrograms = await _repository.GetAllRadTrackProgsAsync();
            if (existingPrograms.Any(p => StringEqualsTrimmedIgnoreCase(p.Program, dto.Program)))
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError($"Program '{dto.Program}' already exists. Please enter a unique program name.", "PROGRAM_DUPLICATE")
                ]);

            RadtrackProg entity = _mapper.Map<RadtrackProg>(dto);
            RadtrackProg created = await _repository.AddRadTrackProgAsync(entity);
            return _mapper.Map<RadTrackProgDto>(created);
        }

        
        public async Task<RadTrackProgDto> UpdateRadTrackProgAsync(RadTrackProgDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            RadtrackProg entity = _mapper.Map<RadtrackProg>(dto);
            RadtrackProg updated = await _repository.UpdateRadTrackProgAsync(entity);
            return _mapper.Map<RadTrackProgDto>(updated);
        }

        
        public async Task<bool> DeleteRadTrackProgAsync(string program)
        {
            if (string.IsNullOrWhiteSpace(program))
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError("Program is required.", "PROGRAM_REQUIRED")
                ]);

            // Trim the program name to normalize it (remove leading/trailing spaces)
            program = program.Trim();

            var existingPrograms = await _repository.GetAllRadTrackProgsAsync();
            if (!existingPrograms.Any(p => StringEqualsTrimmedIgnoreCase(p.Program, program)))
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError($"RadTrackProg with program '{program}' was not found.", "PROGRAM_NOT_FOUND")
                ]);

            return await _repository.DeleteRadTrackProgAsync(program);
        }

        public async Task<bool> RadTrackProgExistsAsync(string program)
        {
            if (string.IsNullOrWhiteSpace(program)) return false;

            program = program.Trim();
            var allPrograms = await _repository.GetAllRadTrackProgsAsync();
            return allPrograms.Any(p => StringEqualsTrimmedIgnoreCase(p.Program, program));
        }

        // Returns distinct non-null Program values from MY_tlkpProject for populating the Programme dropdown
        public async Task<List<string>> GetAllProgramNamesAsync()
        {
            return await _repository.GetAllProgramNamesAsync();
        }

        /// <summary>
        /// Case-insensitive comparison of trimmed program names.
        /// </summary>
        private static bool StringEqualsTrimmedIgnoreCase(string? left, string? right) =>
            left is not null && right is not null && string.Equals(left.Trim(), right.Trim(), StringComparison.OrdinalIgnoreCase);
    }
}
