/*
 * TRANSFORMENGINE MIGRATION — CapsStaffDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + Services
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New DTO created for CapsStaff entity (mabarchive.tblcapsstaff)
 *   - Mirrors CapsStaff Core entity surface: MNumber (PK), Name, Dt2Number
 *   - Used as internal service-layer contract between ICapsStaffService and API/Frontend layers
 *   - AutoMapper CreateMap<CapsStaff, CapsStaffDto>().ReverseMap() registered in EntityMapper
 *
 * PRESERVED:
 *   - All property names match Core entity (MNumber, Name, Dt2Number)
 *   - Nullability constraints preserved (MNumber and Name are non-null; Dt2Number is nullable)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether Dt2Number should be surfaced in the maintenance form modal (currently not in HTML prototype)
 */

namespace Apha.Costbook.Application.Dtos
{
    // TRANSFORMENGINE: Service-layer DTO for mabarchive.tblcapsstaff — drives Tab 5 (CAPS Staff) CRUD surface
    public class CapsStaffDto
    {
        // TRANSFORMENGINE: Maps to CapsStaff.MNumber (PK, varchar 50)
        /// <summary>Staff member M-Number. Primary key.</summary>
        public string MNumber { get; set; } = null!;

        // TRANSFORMENGINE: Maps to CapsStaff.Name (varchar 50, NOT NULL)
        /// <summary>Staff member full name.</summary>
        public string Name { get; set; } = null!;

        // TRANSFORMENGINE: Maps to CapsStaff.Dt2Number (varchar 50, nullable)
        /// <summary>DT2 number reference. Optional.</summary>
        public string? Dt2Number { get; set; }
    }
}
