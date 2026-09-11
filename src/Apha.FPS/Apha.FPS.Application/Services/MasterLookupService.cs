using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Application.Validation;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;
using Npgsql;

namespace Apha.FPS.Application.Services
{
    public class MasterLookupService : IMasterLookupService
    {
        private readonly IMasterLookupRepository _masterLookupRepository;
        private readonly IMapper _mapper;

        public MasterLookupService(IMasterLookupRepository masterLookupRepository, IMapper mapper)
        {
            _masterLookupRepository = masterLookupRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<string>> GetAllMasterLookupsAsync()
        {
            var masterLookups = await _masterLookupRepository.GetAllMasterLookupsAsync();
            return masterLookups.Select(m => m.MasterTableName);
        }

        public Task<IEnumerable<string>> GetLookupItemsAsync(string tableName)
        {
            return _masterLookupRepository.GetLookupItemsAsync(tableName);
        }

        public async Task<PaginatedResult<string>> GetLookupItemsPagedAsync(string tableName, QueryParameters<string> query)
        {
            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var result = await _masterLookupRepository.GetLookupItemsPagedAsync(tableName, filter);
            return _mapper.Map<PaginatedResult<string>>(result);
        }

        public async Task<bool> CreateLookupItemAsync(string tableName, string value)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException($"Entity name is missing");

            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"{tableName} value is missing");

            var existingItems = await _masterLookupRepository.GetLookupItemsAsync(tableName);
            if (existingItems.Any(item => string.Equals(item, value, StringComparison.OrdinalIgnoreCase)))
            {
                throw new BusinessValidationErrorException(new List<BusinessValidationError>
                {
                    new BusinessValidationError(
                        $"'{value}' already exists in '{tableName}'.",
                        "LOOKUP_DUPLICATE_VALUE")
                });
            }

            return await _masterLookupRepository.CreateLookupItemAsync(tableName, value);
        }

        public async Task<bool> UpdateLookupItemAsync(string tableName, string originalValue, string newValue)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException($"Entity name is missing");

            if (string.IsNullOrWhiteSpace(originalValue))
                throw new ArgumentException($"{tableName} original value is missing");

            if (string.IsNullOrWhiteSpace(newValue))
                throw new ArgumentException($"{tableName} value is missing");
            try
            {
                return await _masterLookupRepository.UpdateLookupItemAsync(tableName, originalValue, newValue);
            }
            catch (Exception ex) when (TryGetForeignKeyViolation(ex, out var pgEx))
            {
                throw new BusinessValidationErrorException(new List<BusinessValidationError>
                {
                            new BusinessValidationError(
                                    $"'{originalValue}' cannot be updated because it is still in use by other records.",
                                    "LOOKUP_FK_VIOLATION")
                            });
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"An unexpected error occurred while updating '{originalValue}' in '{tableName}'.", ex);
            }
        }

        public async Task<bool> DeleteLookupItemAsync(string tableName, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"Value is missing");
            try
            {
                return await _masterLookupRepository.DeleteLookupItemAsync(tableName, value);
            }
            catch (Exception ex) when (TryGetForeignKeyViolation(ex, out var pgEx))
            {
                throw new BusinessValidationErrorException(new List<BusinessValidationError>
                {
                            new BusinessValidationError(
                                    $"'{value}' cannot be deleted because it is still in use by other records.",
                                    "LOOKUP_FK_VIOLATION")
                            });
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"An unexpected error occurred while deleting '{value}' from '{tableName}'.", ex);
            }
        }

        // A foreign key violation (SqlState 23503) may surface as a top-level PostgresException
        // or wrapped inside another exception (e.g. DbUpdateException), so walk the inner chain.
        private static bool TryGetForeignKeyViolation(Exception? ex, out PostgresException postgresException)
        {
            for (var current = ex; current is not null; current = current.InnerException)
            {
                if (current is PostgresException pgEx
                    && pgEx.SqlState == PostgresErrorCodes.ForeignKeyViolation)
                {
                    postgresException = pgEx;
                    return true;
                }
            }

            postgresException = null!;
            return false;
        }
    }
}
