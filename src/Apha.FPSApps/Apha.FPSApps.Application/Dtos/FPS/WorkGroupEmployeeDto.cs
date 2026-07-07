/*
 * TRANSFORMENGINE MIGRATION — WorkGroupEmployeeDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-frontend  Phase 1 — Frontend DTOs + API Client Interfaces
 * Migrated : 2026-07-07
 *
 * CHANGED:
 *   - Frontend DTO created mirroring Apha.FPS.Application.Dtos.WorkGroupEmployeeDto (base fields subset)
 *   - Namespace scoped to Apha.FPSApps.Application.Dtos.FPS (frontend application layer)
 *   - Extended staff-planning fields (TimeRecorder, StartDate, EndDate, HoursPerWeek) separated into
 *     WorkGroupEmployeeStaffDto to reflect the two distinct backend endpoints
 *
 * PRESERVED:
 *   - All property names exactly match backend DTO (PactId, SpNumber, WorkGroupGrade, Name, PersonStatus,
 *     PersonClass, HrsPaid, Leave, SickSpecial, HrsAvail, MakeAvailable)
 *   - Property types and nullability annotations preserved verbatim
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Re-verify field parity if backend WorkGroupEmployeeDto gains new columns
 */

namespace Apha.FPSApps.Application.Dtos.FPS
{
    public class WorkGroupEmployeeDto
    {
        public string PactId { get; set; } = null!;
        public string SpNumber { get; set; } = null!;
        public string WorkGroupGrade { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string PersonStatus { get; set; } = null!;
        public string? PersonClass { get; set; }
        public double HrsPaid { get; set; }
        public double Leave { get; set; }
        public double SickSpecial { get; set; }
        public double HrsAvail { get; set; }
        public int MakeAvailable { get; set; }
    }
}
