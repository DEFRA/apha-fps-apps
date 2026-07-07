/*
 * TRANSFORMENGINE MIGRATION — ProfitCentreDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-frontend  Phase 1 — Frontend DTOs + API Client Interfaces
 * Migrated : 2026-07-07
 *
 * CHANGED:
 *   - Frontend DTO created mirroring Apha.FPS.Application.Dtos.ProfitCentreDto
 *   - Namespace scoped to Apha.FPSApps.Application.Dtos.FPS (frontend application layer)
 *   - Used as both CRUD DTO (profit centre maintenance) and lookup DTO (Resource Centre dropdown in
 *     Set Up Staff Resources — ProfitCentreId + ProfitCentreName consumed as dropdown items)
 *
 * PRESERVED:
 *   - All property names exactly match backend DTO (ProfitCentreId, ProfitCentreName, Division,
 *     ContTarget, ProfitCentreHead, DivisionId, EmailRecipient, Timesheet, Outputsheet, TimesheetLayout)
 *   - Property types and nullability annotations preserved verbatim
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Re-verify field parity if backend ProfitCentreDto gains new columns
 */

namespace Apha.FPSApps.Application.Dtos.FPS
{
    public class ProfitCentreDto
    {
        public string ProfitCentreId { get; set; } = null!;
        public string ProfitCentreName { get; set; } = null!;
        public string? Division { get; set; }
        public decimal? ContTarget { get; set; }
        public string? ProfitCentreHead { get; set; }
        public int? DivisionId { get; set; }
        public string? EmailRecipient { get; set; }
        public int? Timesheet { get; set; }
        public int? Outputsheet { get; set; }
        public short? TimesheetLayout { get; set; }
    }
}
