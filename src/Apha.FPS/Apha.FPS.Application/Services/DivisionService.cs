using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    /// <summary>
    /// Service implementation for Division maintenance business logic.
    /// </summary>
    public class DivisionService : IDivisionService
    {
        private readonly IDivisionRepository _divisionRepository;
        private readonly IMapper _mapper;

        public DivisionService(
            IDivisionRepository divisionRepository,
            IMapper mapper)
        {
            _divisionRepository = divisionRepository ?? throw new ArgumentNullException(nameof(divisionRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<List<DivisionDto>> GetAllDivisionsAsync()
        {
            var divisions = await _divisionRepository.GetAllDivisionsAsync();
            return _mapper.Map<List<DivisionDto>>(divisions);
        }

        public async Task<PaginatedResult<DivisionDto>> GetAllDivisionsPagedAsync(QueryParameters<string> query)
        {
            ArgumentNullException.ThrowIfNull(query);

            var queryParams = _mapper.Map<Apha.FPS.Core.Pagination.PaginationParameters<string>>(query);
            var pagedDivisions = await _divisionRepository.GetAllDivisionsPagedAsync(queryParams);
            return _mapper.Map<PaginatedResult<DivisionDto>>(pagedDivisions);
        }

        public async Task<DivisionDto?> GetDivisionByNameAsync(string divName)
        {
            if (string.IsNullOrWhiteSpace(divName))
            {
                throw new ArgumentException("Division name cannot be null or empty.", nameof(divName));
            }

            var division = await _divisionRepository.GetDivisionByNameAsync(divName);
            return division == null ? null : _mapper.Map<DivisionDto>(division);
        }

        public async Task<DivisionDto> CreateDivisionAsync(DivisionDto divisionDto)
        {
            ArgumentNullException.ThrowIfNull(divisionDto);

            if (string.IsNullOrWhiteSpace(divisionDto.DivName))
            {
                throw new ArgumentException("Division name is required.", nameof(divisionDto));
            }

            // Check if the division name is already referenced in other tables (foreign key check)
            // This check should happen FIRST to give better error message than PK constraint violation
            var referencedTables = await _divisionRepository.GetDivisionForeignKeyReferencesAsync(divisionDto.DivName);

            if (referencedTables.Count != 0)
            {
                throw new InvalidOperationException("Unable to add the division name as it is already in use.");
            }

            // Check if division already exists in the main table
            if (await _divisionRepository.DivisionExistsAsync(divisionDto.DivName))
            {
                throw new InvalidOperationException($"Division '{divisionDto.DivName}' already exists.");
            }

            var division = _mapper.Map<Division>(divisionDto);
            var createdDivision = await _divisionRepository.CreateDivisionAsync(division);
            return _mapper.Map<DivisionDto>(createdDivision);
        }

        public async Task<DivisionDto> UpdateDivisionAsync(string originalDivName, DivisionDto divisionDto)
        {
            ArgumentNullException.ThrowIfNull(divisionDto);

            if (string.IsNullOrWhiteSpace(originalDivName))
            {
                throw new ArgumentException("Original division name is required to identify the record.", nameof(originalDivName));
            }

            if (string.IsNullOrWhiteSpace(divisionDto.DivName))
            {
                throw new ArgumentException("Division name is required.", nameof(divisionDto));
            }

            // Use originalDivName to find the record to update
            var existingDivision = await _divisionRepository.GetDivisionByNameAsync(originalDivName);
            if (existingDivision == null)
            {
                throw new InvalidOperationException($"Division '{originalDivName}' not found.");
            }

            // Check if new name conflicts with another division (only if name is changing)
            if (!originalDivName.Equals(divisionDto.DivName, StringComparison.OrdinalIgnoreCase))
            {
                var nameConflict = await _divisionRepository.DivisionExistsAsync(divisionDto.DivName);
                if (nameConflict)
                {
                    throw new InvalidOperationException($"Cannot rename to '{divisionDto.DivName}' - division already exists.");
                }

                // Check if the division name is referenced in other tables (foreign key check)
                var referencedTables = await _divisionRepository.GetDivisionForeignKeyReferencesAsync(originalDivName);
                if (referencedTables.Count != 0)
                {
                    throw new InvalidOperationException("Unable to edit the division name as it is already in use.");
                }
            }

            // Map new values and update (pass originalDivName to repository)
            var division = _mapper.Map<Division>(divisionDto);
            var updatedDivision = await _divisionRepository.UpdateDivisionAsync(originalDivName, division);
            return _mapper.Map<DivisionDto>(updatedDivision);
        }

        public async Task<bool> DeleteDivisionAsync(string divName)
        {
            if (string.IsNullOrWhiteSpace(divName))
            {
                throw new ArgumentException("Division name cannot be null or empty.", nameof(divName));
            }

            // Check if the division name is referenced in other tables (foreign key check)
            var referencedTables = await _divisionRepository.GetDivisionForeignKeyReferencesAsync(divName);

            if (referencedTables.Count != 0)
            {
                throw new InvalidOperationException("Unable to delete the division name as it is already in use.");
            }

            return await _divisionRepository.DeleteDivisionAsync(divName);
        }
    }
}
