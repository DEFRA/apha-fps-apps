/*
 * TRANSFORMENGINE MIGRATION — CapsStaffDto.cs (Frontend)
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New frontend DTO created in Apha.FPSApps.Application, mirroring backend Apha.Costbook.Application.Dtos.CapsStaffDto
 *   - Same shape as backend DTO — 3 properties: MNumber (PK), Name, Dt2Number (optional)
 *   - Used by ICostBookCapsStaffApiClient and frontend CostBookCapsStaffService to serialise/deserialise API calls
 *   - Namespace: Apha.FPSApps.Application.Dtos.CostBook (frontend application layer)
 *
 * PRESERVED:
 *   - All property names exactly match backend DTO (case-sensitive): MNumber, Name, Dt2Number
 *   - Nullability preserved: MNumber and Name are non-null; Dt2Number is nullable
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether Dt2Number should appear in the maintenance form modal (not in HTML prototype)
 */

namespace Apha.FPSApps.Application.Dtos.CostBook;

// TRANSFORMENGINE: Frontend mirror of Apha.Costbook.Application.Dtos.CapsStaffDto
//   Covers frmMaintainance Tab 5 (CAPS Staff) CRUD surface
public class CapsStaffDto
{
    // TRANSFORMENGINE: Maps to backend CapsStaffDto.MNumber (PK, varchar 50)
    /// <summary>Staff member M-Number. Primary key.</summary>
    public string MNumber { get; set; } = null!;

    // TRANSFORMENGINE: Maps to backend CapsStaffDto.Name (varchar 50, NOT NULL)
    /// <summary>Staff member full name.</summary>
    public string Name { get; set; } = null!;

    // TRANSFORMENGINE: Maps to backend CapsStaffDto.Dt2Number (varchar 50, nullable)
    /// <summary>DT2 number reference. Optional.</summary>
    public string? Dt2Number { get; set; }
}
