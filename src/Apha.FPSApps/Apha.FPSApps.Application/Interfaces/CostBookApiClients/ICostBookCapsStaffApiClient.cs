/*
 * TRANSFORMENGINE MIGRATION — ICostBookCapsStaffApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New frontend API client interface created for frmMaintainance Tab 5 (CAPS Staff)
 *   - Targets backend CapsStaffController at route /api/v1/capsstaff
 *   - GetAllCapsStaffAsync()         → GET    /api/v1/capsstaff
 *   - GetPaginatedCapsStaffAsync()   → GET    /api/v1/capsstaff/paginated
 *   - GetCapsStaffByMNumberAsync()   → GET    /api/v1/capsstaff/{mNumber}
 *   - AddCapsStaffAsync()            → POST   /api/v1/capsstaff
 *   - UpdateCapsStaffAsync()         → PUT    /api/v1/capsstaff/{mNumber}
 *   - DeleteCapsStaffAsync()         → DELETE /api/v1/capsstaff/{mNumber}
 *   - All return types wrapped in ApiResponseDto<T>
 *
 * PRESERVED:
 *   - All backend CapsStaffController action signatures mirrored exactly
 *   - MNumber string PK used for route-based lookups, updates, and deletes
 *   - Paginated endpoint included to match backend GetPaginatedCapsStaff action
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether Dt2Number is surfaced in the maintenance form modal — currently not in HTML prototype
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.CostBookApiClients;

// TRANSFORMENGINE: API client interface for backend CapsStaffController — covers frmMaintainance Tab 5 (CAPS Staff) full CRUD
public interface ICostBookCapsStaffApiClient
{
    // TRANSFORMENGINE: GET /api/v1/capsstaff → returns full list for Tab 5 grid
    /// <summary>Returns all CAPS staff members ordered by MNumber.</summary>
    Task<ApiResponseDto<List<CapsStaffDto>>> GetAllCapsStaffAsync();

    // TRANSFORMENGINE: GET /api/v1/capsstaff/paginated → paginated list for Tab 5 grid
    /// <summary>Returns a paginated list of CAPS staff members.</summary>
    Task<ApiResponseDto<List<CapsStaffDto>>> GetPaginatedCapsStaffAsync(QueryParameters<string> query);

    // TRANSFORMENGINE: GET /api/v1/capsstaff/{mNumber} → single record lookup
    /// <summary>Returns a single CAPS staff member by MNumber.</summary>
    Task<ApiResponseDto<CapsStaffDto>> GetCapsStaffByMNumberAsync(string mNumber);

    // TRANSFORMENGINE: POST /api/v1/capsstaff → create from Tab 5 modal (formTblCapsStaff)
    /// <summary>Creates a new CAPS staff member. MNumber must be unique.</summary>
    Task<ApiResponseDto<CapsStaffDto>> AddCapsStaffAsync(CapsStaffDto dto);

    // TRANSFORMENGINE: PUT /api/v1/capsstaff/{mNumber} → update from Tab 5 edit modal
    /// <summary>Updates an existing CAPS staff member.</summary>
    Task<ApiResponseDto<CapsStaffDto>> UpdateCapsStaffAsync(string mNumber, CapsStaffDto dto);

    // TRANSFORMENGINE: DELETE /api/v1/capsstaff/{mNumber} → delete from Tab 5 confirm modal
    /// <summary>Deletes the CAPS staff member identified by MNumber.</summary>
    Task<ApiResponseDto<bool>> DeleteCapsStaffAsync(string mNumber);
}
