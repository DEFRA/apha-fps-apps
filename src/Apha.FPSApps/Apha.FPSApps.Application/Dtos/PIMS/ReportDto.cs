/*
 * TRANSFORMENGINE MIGRATION — ReportDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New frontend DTO mirroring Apha.PIMS.Application.Dtos.ReportDto
 *   - Placed in Apha.FPSApps.Application.Dtos.PIMS namespace for frontend consumption
 *   - All boolean Allowpick* fields preserved from backend DTO shape
 *
 * PRESERVED:
 *   - All property names match backend DTO exactly (case-sensitive)
 *   - Nullable reference types match backend DTO nullability
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - DEFERRED: none — fully automated.
 */

namespace Apha.FPSApps.Application.Dtos.PIMS
{
    // TRANSFORMENGINE: Frontend DTO mirroring Apha.PIMS.Application.Dtos.ReportDto — same shape, different namespace
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
        public string Type { get; set; } = null!;
    }
}
