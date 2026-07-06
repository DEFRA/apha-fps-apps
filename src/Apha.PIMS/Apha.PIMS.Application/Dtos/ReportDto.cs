/*
 * TRANSFORMENGINE MIGRATION — ReportDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Application DTO mirroring Apha.PIMS.Core.Entities.Report
 *   - All entity properties represented with matching nullability
 *   - Type char(1) stored as string(1) — no enum mapping applied (deferred)
 *
 * PRESERVED:
 *   - All field names consistent with entity (lowercased convention)
 *   - boolean NOT NULL columns kept non-nullable
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: verify 'Type' char(1) usage — confirm no enum mapping needed before shipping
 */

namespace Apha.PIMS.Application.Dtos
{
    // TRANSFORMENGINE: DTO maps to/from Apha.PIMS.Core.Entities.Report via EntityMapper
    public class ReportDto
    {
        public int Id { get; set; }

        public string Reportname { get; set; } = null!;

        public string? Reportdescription { get; set; }

        public string? Filter { get; set; }

        public string? Mailcomment { get; set; }

        public string? Mailtitle { get; set; }

        public bool Emailable { get; set; }

        public int? Sortorder { get; set; }

        public bool Allowpickprogramme { get; set; }

        public bool Allowpickproject { get; set; }

        public bool Allowpickmanager { get; set; }

        public bool Allowpickcontract { get; set; }

        public bool Allowpickcustomer { get; set; }

        public bool Allowpickmonth { get; set; }

        public bool Allowpickfyear { get; set; }

        public string? Reporthelp { get; set; }

        // TRANSFORMENGINE: char(1) in PostgreSQL — mapped as string for EF Core compatibility
        public string Type { get; set; } = null!;
    }
}
