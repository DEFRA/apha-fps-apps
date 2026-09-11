using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    /// <summary>
    /// API client interface for Master Lookup operations.
    /// </summary>
    public interface IFpsMasterLookupApiClient
    {
        /// <summary>
        /// Retrieves all master lookups from the API.
        /// </summary>
        /// <returns>API response containing master lookup collection.</returns>
        Task<ApiResponseDto<IEnumerable<MasterLookupDto>>> GetAllMasterLookupsAsync();

        /// <summary>
        /// Retrieves all values from a registered lookup table.
        /// </summary>
        Task<ApiResponseDto<IEnumerable<LookupItemDto>>> GetLookupItemsAsync(string tableName);

        /// <summary>
        /// Retrieves a paginated, filtered and sorted set of values from a registered lookup table.
        /// </summary>
        Task<ApiResponseDto<PaginatedResult<LookupItemDto>>> GetLookupItemsPagedAsync(string tableName, QueryParameters<string> query);

        /// <summary>
        /// Creates a new value in a registered lookup table.
        /// </summary>
        Task<ApiResponseDto<bool>> CreateLookupItemAsync(string tableName, string value);

        /// <summary>
        /// Updates an existing value in a registered lookup table.
        /// </summary>
        Task<ApiResponseDto<bool>> UpdateLookupItemAsync(string tableName, string originalValue, string newValue);

        /// <summary>
        /// Deletes a value from a registered lookup table.
        /// </summary>
        Task<ApiResponseDto<bool>> DeleteLookupItemAsync(string tableName, string value);
    }
}
