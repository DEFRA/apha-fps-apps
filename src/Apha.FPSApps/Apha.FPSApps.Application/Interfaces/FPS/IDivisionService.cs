using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    /// <summary>
    /// Frontend service interface for Division operations.
    /// </summary>
    public interface IDivisionService
    {
        /// <summary>
        /// Retrieves all divisions.
        /// </summary>
        /// <returns>API response containing division collection.</returns>
        Task<ApiResponseDto<IEnumerable<DivisionDto>>> GetAllDivisionsAsync();

        /// <summary>
        /// Retrieves a paginated list of divisions.
        /// </summary>
        /// <param name="query">Pagination parameters.</param>
        /// <returns>API response containing paginated divisions.</returns>
        Task<ApiResponseDto<List<DivisionDto>>> GetAllDivisionsPagedAsync(QueryParameters<string> query);

        /// <summary>
        /// Retrieves a single division by name.
        /// </summary>
        /// <param name="divName">Division name.</param>
        /// <returns>API response containing division data.</returns>
        Task<ApiResponseDto<DivisionDto>> GetDivisionByNameAsync(string divName);

        /// <summary>
        /// Creates a new division.
        /// </summary>
        /// <param name="divisionDto">Division data to create.</param>
        /// <returns>API response containing created division.</returns>
        Task<ApiResponseDto<DivisionDto>> CreateDivisionAsync(DivisionDto divisionDto);

        /// <summary>
        /// Updates an existing division.
        /// </summary>
        /// <param name="divName">Division name to update.</param>
        /// <param name="divisionDto">Updated division data.</param>
        /// <returns>API response containing updated division.</returns>
        Task<ApiResponseDto<DivisionDto>> UpdateDivisionAsync(string divName, DivisionDto divisionDto);

        /// <summary>
        /// Deletes a division.
        /// </summary>
        /// <param name="divName">Division name to delete.</param>
        /// <returns>API response indicating success or failure.</returns>
        Task<ApiResponseDto<bool>> DeleteDivisionAsync(string divName);

        /// <summary>
        /// Retrieves all agencies for dropdown population.
        /// </summary>
        /// <returns>API response containing agency collection.</returns>
        Task<ApiResponseDto<IEnumerable<AgencyDto>>> GetAllAgenciesAsync();
    }
}
