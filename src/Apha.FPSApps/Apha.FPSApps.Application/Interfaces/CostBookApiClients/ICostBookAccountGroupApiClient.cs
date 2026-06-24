/*
 * TRANSFORMENGINE MIGRATION — ICostBookAccountGroupApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New frontend API client interface created for frmMaintainance Tab 3 (CSG7 Inflation Options)
 *   - Targets backend AccountGroupController at route /api/v1/accountgroup
 *   - GetAllAccountGroupsAsync()     → GET    /api/v1/accountgroup
 *   - GetAccountGroupAsync()         → GET    /api/v1/accountgroup/{csg7Group}
 *   - AddAccountGroupAsync()         → POST   /api/v1/accountgroup
 *   - UpdateAccountGroupAsync()      → PUT    /api/v1/accountgroup/{csg7Group}
 *   - DeleteAccountGroupAsync()      → DELETE /api/v1/accountgroup/{csg7Group}
 *   - GetAllAccountGroupsAsync() also drives the CSG7 group dropdown in the AccountCategory maintenance modal (Tab 2)
 *   - All return types wrapped in ApiResponseDto<T>
 *
 * PRESERVED:
 *   - All backend AccountGroupController action signatures mirrored exactly
 *   - Csg7Group string PK (varchar 15) used for route-based lookups, updates, and deletes
 *   - AccountGroupDto used for both CRUD and lookup (GET all) since the list is also used as a dropdown source
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm Csg7Group max length (varchar 15) validation enforced at service/controller level
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;

namespace Apha.FPSApps.Application.Interfaces.CostBookApiClients;

// TRANSFORMENGINE: API client interface for backend AccountGroupController — covers frmMaintainance Tab 3 (CSG7 Inflation Options) full CRUD
//   Also provides the CSG7 group list for the AccountCategory maintenance modal dropdown (Tab 2)
public interface ICostBookAccountGroupApiClient
{
    // TRANSFORMENGINE: GET /api/v1/accountgroup → returns full list for Tab 3 grid + AccCat modal dropdown
    /// <summary>Returns all CSG7 account groups ordered by Csg7Group key.</summary>
    Task<ApiResponseDto<List<AccountGroupDto>>> GetAllAccountGroupsAsync();

    // TRANSFORMENGINE: GET /api/v1/accountgroup/{csg7Group} → single record lookup
    /// <summary>Returns a single CSG7 account group by Csg7Group key.</summary>
    Task<ApiResponseDto<AccountGroupDto>> GetAccountGroupAsync(string csg7Group);

    // TRANSFORMENGINE: POST /api/v1/accountgroup → create from Tab 3 modal (formTblCsg7)
    /// <summary>Creates a new CSG7 account group. Csg7Group must be unique (varchar 15).</summary>
    Task<ApiResponseDto<AccountGroupDto>> AddAccountGroupAsync(AccountGroupDto dto);

    // TRANSFORMENGINE: PUT /api/v1/accountgroup/{csg7Group} → update from Tab 3 edit modal
    /// <summary>Updates an existing CSG7 account group.</summary>
    Task<ApiResponseDto<AccountGroupDto>> UpdateAccountGroupAsync(string csg7Group, AccountGroupDto dto);

    // TRANSFORMENGINE: DELETE /api/v1/accountgroup/{csg7Group} → delete from Tab 3 confirm modal
    /// <summary>Deletes the CSG7 account group identified by Csg7Group.</summary>
    Task<ApiResponseDto<bool>> DeleteAccountGroupAsync(string csg7Group);
}
