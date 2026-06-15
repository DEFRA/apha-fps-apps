// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — RadTrackInvoiceDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-06-12
 *
 * CHANGED:
 *   - New file: frontend DTO mirroring Apha.PIMS.Application.Dtos.RadTrackInvoiceDto.
 *   - Resides in Apha.FPSApps.Application.Dtos.PIMS namespace for use in frontend
 *     service and infrastructure layers.
 *   - All 11 properties carried forward with identical names, types, and nullability
 *     to the backend DTO to support convention mapping in PimsApiDtoMapper.
 *
 * PRESERVED:
 *   - Property names and types match backend RadTrackInvoiceDto exactly.
 *   - InvoicePaid remains short (non-nullable) matching backend DTO.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: If InvoicePaid is updated to bool on the backend DTO,
 *     update this frontend DTO accordingly and adjust PimsApiDtoMapper mapping.
 */

using System;

namespace Apha.FPSApps.Application.Dtos.PIMS
{
    // TRANSFORMENGINE: Frontend DTO mirroring Apha.PIMS.Application.Dtos.RadTrackInvoiceDto.
    // Used by IPimsRadTrackInvoiceApiClient and the frontend RadTrackInvoiceService.
    public class RadTrackInvoiceDto
    {
        // TRANSFORMENGINE: PK — required for Edit/Delete identification; mirrors backend DTO.
        public int InvoiceCounter { get; set; }

        public string? Project { get; set; }

        public double? PlannedAmount { get; set; }

        public double? DueAmount { get; set; }

        public DateTime? DueDate { get; set; }

        public double? ActualAmount { get; set; }

        public DateTime? DateInvoiced { get; set; }

        public string? Contract { get; set; }

        public DateTime? DateJobsheetRaised { get; set; }

        public string? InvoiceRef { get; set; }

        // TRANSFORMENGINE: smallint NOT NULL DEFAULT 0 — see backend DTO deferred note re: bool.
        public short InvoicePaid { get; set; }
    }
}
