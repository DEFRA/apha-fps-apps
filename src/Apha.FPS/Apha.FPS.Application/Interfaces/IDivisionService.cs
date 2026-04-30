using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    /// <summary>
    /// Service interface for Division maintenance business logic.
    /// </summary>
    public interface IDivisionService
    {
        /// <summary>
        /// Retrieves all divisions with agency information.
        /// </summary>
        /// <returns>Collection of division DTOs.</returns>
        Task<List<DivisionDto>> GetAllDivisionsAsync();

        /// <summary>
        /// Retrieves a paginated list of divisions.
        /// </summary>
        /// <param name="query">Pagination parameters.</param>
        /// <returns>Paginated division DTOs.</returns>
        Task<PaginatedResult<DivisionDto>> GetAllDivisionsPagedAsync(QueryParameters<string> query);

        /// <summary>
        /// Retrieves a single division by name.
        /// </summary>
        /// <param name="divName">Division name (case-insensitive).</param>
        /// <returns>Division DTO if found; null otherwise.</returns>
        Task<DivisionDto?> GetDivisionByNameAsync(string divName);

        /// <summary>
        /// Creates a new division after validation.
        /// </summary>
        /// <param name="divisionDto">Division data to create.</param>
        /// <returns>Created division DTO.</returns>
        Task<DivisionDto> CreateDivisionAsync(DivisionDto divisionDto);

        /// <summary>
        /// Updates an existing division after validation.
        /// </summary>
        /// <param name="originalDivName">Original division name to identify the record.</param>
        /// <param name="divisionDto">Division data to update (may contain new DivName).</param>
        /// <returns>Updated division DTO.</returns>
        Task<DivisionDto> UpdateDivisionAsync(string originalDivName, DivisionDto divisionDto);

        /// <summary>
        /// Deletes a division by name.
        /// </summary>
        /// <param name="divName">Division name to delete.</param>
        /// <returns>True if deleted; false if not found.</returns>
        Task<bool> DeleteDivisionAsync(string divName);
    }
}
