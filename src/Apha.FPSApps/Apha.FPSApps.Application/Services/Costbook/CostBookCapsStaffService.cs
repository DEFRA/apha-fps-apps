/*
 * TRANSFORMENGINE MIGRATION — CostBookCapsStaffService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 8 — Frontend Service Interface + Implementation (Steps 12-13)
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New frontend service implementation created for frmMaintainance Tab 5 (CAPS Staff)
 *   - Implements ICostBookCapsStaffService as a thin delegate to ICostBookApiClient.CostbookCapsStaff
 *   - GetAllCapsStaffAsync()           → _costBookClient.CostbookCapsStaff.GetAllCapsStaffAsync()
 *   - GetPaginatedCapsStaffAsync()     → _costBookClient.CostbookCapsStaff.GetPaginatedCapsStaffAsync()
 *   - GetCapsStaffByMNumberAsync()     → _costBookClient.CostbookCapsStaff.GetCapsStaffByMNumberAsync()
 *   - AddCapsStaffAsync()              → _costBookClient.CostbookCapsStaff.AddCapsStaffAsync()
 *   - UpdateCapsStaffAsync()           → _costBookClient.CostbookCapsStaff.UpdateCapsStaffAsync()
 *   - DeleteCapsStaffAsync()           → _costBookClient.CostbookCapsStaff.DeleteCapsStaffAsync()
 *   - _costBookClient is private readonly (Sonar S2933 compliance)
 *
 * PRESERVED:
 *   - No business logic — all methods are single-line return delegates (Sonar S4144 intentional)
 *   - MNumber string PK used for route-based lookups, updates, and deletes
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether Dt2Number is surfaced in the maintenance form modal — currently not in HTML prototype
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.Costbook;
using Apha.FPSApps.Application.Interfaces.CostBookApiClients;
using Apha.FPSApps.Application.Pagination;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apha.FPSApps.Application.Services.Costbook
{
    // TRANSFORMENGINE: Thin delegate service for frmMaintainance Tab 5 (CAPS Staff) full CRUD — forwards to ICostBookApiClient.CostbookCapsStaff
    public class CostBookCapsStaffService : ICostBookCapsStaffService
    {
        // TRANSFORMENGINE: private readonly — Sonar S2933 compliance
        private readonly ICostBookApiClient _costBookClient;

        public CostBookCapsStaffService(ICostBookApiClient costBookClient)
        {
            _costBookClient = costBookClient;
        }

        // TRANSFORMENGINE: delegate → GET /api/v1/capsstaff
        public Task<ApiResponseDto<List<CapsStaffDto>>> GetAllCapsStaffAsync()
        {
            return _costBookClient.CostbookCapsStaff.GetAllCapsStaffAsync();
        }

        // TRANSFORMENGINE: delegate → GET /api/v1/capsstaff/paginated
        public Task<ApiResponseDto<List<CapsStaffDto>>> GetPaginatedCapsStaffAsync(QueryParameters<string> query)
        {
            return _costBookClient.CostbookCapsStaff.GetPaginatedCapsStaffAsync(query);
        }

        // TRANSFORMENGINE: delegate → GET /api/v1/capsstaff/{mNumber}
        public Task<ApiResponseDto<CapsStaffDto>> GetCapsStaffByMNumberAsync(string mNumber)
        {
            return _costBookClient.CostbookCapsStaff.GetCapsStaffByMNumberAsync(mNumber);
        }

        // TRANSFORMENGINE: delegate → POST /api/v1/capsstaff
        public Task<ApiResponseDto<CapsStaffDto>> AddCapsStaffAsync(CapsStaffDto dto)
        {
            return _costBookClient.CostbookCapsStaff.AddCapsStaffAsync(dto);
        }

        // TRANSFORMENGINE: delegate → PUT /api/v1/capsstaff/{mNumber}
        public Task<ApiResponseDto<CapsStaffDto>> UpdateCapsStaffAsync(string mNumber, CapsStaffDto dto)
        {
            return _costBookClient.CostbookCapsStaff.UpdateCapsStaffAsync(mNumber, dto);
        }

        // TRANSFORMENGINE: delegate → DELETE /api/v1/capsstaff/{mNumber}
        public Task<ApiResponseDto<bool>> DeleteCapsStaffAsync(string mNumber)
        {
            return _costBookClient.CostbookCapsStaff.DeleteCapsStaffAsync(mNumber);
        }
    }
}
