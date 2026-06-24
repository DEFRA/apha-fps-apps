/*
 * TRANSFORMENGINE MIGRATION — ICapsStaffService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + Services
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New service interface created for CAPS Staff CRUD (Tab 5 of frmMaintainance)
 *   - Full async CRUD: GetAll, GetPaginated, GetByMNumber, Add, Update, Delete
 *   - Service methods map to backend routes: GET /api/v1/CapsStaff, POST, PUT, DELETE
 *   - GetPaginatedAsync supports the Tab 5 data grid with pagination
 *
 * PRESERVED:
 *   - Async-only signatures per Application layer convention
 *   - No infrastructure-specific types (no DbContext, EF references)
 *   - Uses CapsStaffDto as the service contract — no entity types exposed
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Verify MNumber string key is treated consistently as varchar(50) in all callers
 */

using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Pagination;

namespace Apha.Costbook.Application.Interfaces
{
    // TRANSFORMENGINE: Service interface for mabarchive.tblcapsstaff CRUD — covers Tab 5 (CAPS Staff) maintenance operations
    public interface ICapsStaffService
    {
        /// <summary>Returns all CAPS staff members ordered by MNumber.</summary>
        Task<List<CapsStaffDto>> GetAllAsync();

        /// <summary>Returns a paginated list of CAPS staff members.</summary>
        Task<PaginatedResult<CapsStaffDto>> GetPaginatedAsync(QueryParameters<string> queryParameters);

        /// <summary>Returns a single CAPS staff member by primary key, or null if not found.</summary>
        Task<CapsStaffDto?> GetByMNumberAsync(string mNumber);

        /// <summary>
        /// Adds a new CAPS staff member.
        /// Throws <see cref="ArgumentException"/> if MNumber is null/empty or already exists.
        /// </summary>
        Task<CapsStaffDto> AddAsync(CapsStaffDto dto);

        /// <summary>
        /// Updates an existing CAPS staff member.
        /// Throws <see cref="KeyNotFoundException"/> if no record with the given MNumber exists.
        /// </summary>
        Task<CapsStaffDto> UpdateAsync(string mNumber, CapsStaffDto dto);

        /// <summary>
        /// Deletes the CAPS staff member identified by MNumber.
        /// Throws <see cref="KeyNotFoundException"/> if no record with the given MNumber exists.
        /// </summary>
        Task DeleteAsync(string mNumber);
    }
}
