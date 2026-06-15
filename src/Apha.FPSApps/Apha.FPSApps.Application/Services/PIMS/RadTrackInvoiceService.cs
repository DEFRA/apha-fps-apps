// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — RadTrackInvoiceService.cs (new file)
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 8 — Frontend Service Interface + Implementation (Steps 12-13)
 * Migrated : 2026-06-12
 *
 * CHANGED:
 *   - New file: thin delegate frontend service forwarding all calls to
 *     IPimsApiClient.PimsRadTrackInvoice without any business logic.
 *   - Implements IRadTrackInvoiceService.
 *   - Injects only IPimsApiClient; no repository, no DbContext, no business logic.
 *   - Every method body is a single expression-bodied delegation to
 *     _client.PimsRadTrackInvoice.<MethodAsync>(...) per the thin-delegate pattern.
 *   - _client field is private readonly (Sonar S2933 compliance).
 *
 * PRESERVED:
 *   - All six method signatures mirror IRadTrackInvoiceService and
 *     IPimsRadTrackInvoiceApiClient exactly.
 *   - Nullable filter defaults (null) preserved on GetAllAsync and GetTotalsAsync
 *     so callers omitting filters get unfiltered results.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: After Phase 9 (PimsRadTrackInvoiceApiClient.cs) is
 *     implemented, confirm _client.PimsRadTrackInvoice resolves at runtime.
 *   - TRANSFORMENGINE TODO: If year filter type changes (int? vs string?) update
 *     both the interface and this implementation consistently.
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.PIMS
{
    public class RadTrackInvoiceService : IRadTrackInvoiceService
    {
        // TRANSFORMENGINE: private readonly per Sonar S2933 — injected via constructor DI.
        private readonly IPimsApiClient _client;

        public RadTrackInvoiceService(IPimsApiClient client)
        {
            _client = client;
        }

        // TRANSFORMENGINE: thin delegate — forwards to IPimsRadTrackInvoiceApiClient.GetAllAsync
        public async Task<ApiResponseDto<List<RadTrackInvoiceDto>>> GetAllAsync(
            QueryParameters<string> query,
            string? project = null,
            string? contract = null,
            int? year = null,
            string? program = null)
            => await _client.PimsRadTrackInvoice.GetAllAsync(query, project, contract, year, program);

        // TRANSFORMENGINE: thin delegate — forwards to IPimsRadTrackInvoiceApiClient.GetTotalsAsync
        public async Task<ApiResponseDto<RadTrackInvoiceTotalsDto>> GetTotalsAsync(
            string? project = null,
            string? contract = null,
            int? year = null,
            string? program = null)
            => await _client.PimsRadTrackInvoice.GetTotalsAsync(project, contract, year, program);

        // TRANSFORMENGINE: thin delegate — forwards to IPimsRadTrackInvoiceApiClient.GetByIdAsync
        public async Task<ApiResponseDto<RadTrackInvoiceDto>> GetByIdAsync(int id)
            => await _client.PimsRadTrackInvoice.GetByIdAsync(id);

        // TRANSFORMENGINE: thin delegate — forwards to IPimsRadTrackInvoiceApiClient.CreateAsync
        public async Task<ApiResponseDto<RadTrackInvoiceDto>> CreateAsync(RadTrackInvoiceDto dto)
            => await _client.PimsRadTrackInvoice.CreateAsync(dto);

        // TRANSFORMENGINE: thin delegate — forwards to IPimsRadTrackInvoiceApiClient.UpdateAsync
        // id = InvoiceCounter PK forwarded to the backend PUT route.
        public async Task<ApiResponseDto<RadTrackInvoiceDto>> UpdateAsync(int id, RadTrackInvoiceDto dto)
            => await _client.PimsRadTrackInvoice.UpdateAsync(id, dto);

        // TRANSFORMENGINE: thin delegate — forwards to IPimsRadTrackInvoiceApiClient.DeleteAsync
        public async Task<ApiResponseDto<object>> DeleteAsync(int id)
            => await _client.PimsRadTrackInvoice.DeleteAsync(id);
    }
}
