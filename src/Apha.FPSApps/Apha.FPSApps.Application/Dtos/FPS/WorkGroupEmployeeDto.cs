// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — WorkGroupEmployeeDto.cs (Frontend)
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-06-11
 *
 * CHANGED:
 *   - Added TimeRecorder (int) property — mirrors Apha.FPS.Application.Dtos.WorkGroupEmployeeDto.TimeRecorder
 *   - Added StartDate (DateTime?) property — mirrors backend DTO StartDate
 *   - Added EndDate (DateTime?) property — mirrors backend DTO EndDate
 *   - Added HoursPerWeek (double?) property — mirrors backend DTO HoursPerWeek
 *
 * PRESERVED:
 *   - All original properties: PactId, SpNumber, WorkGroupGrade, Name, PersonStatus, PersonClass,
 *     HrsPaid, Leave, SickSpecial, HrsAvail, MakeAvailable
 *   - Nullability contracts unchanged for pre-existing fields
 *   - Namespace Apha.FPSApps.Application.Dtos.FPS unchanged
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: FpsYear is absent from the backend DTO (Apha.FPS.Application.Dtos.WorkGroupEmployeeDto)
 *     and is therefore also absent here. Confirm whether FpsYear must be exposed in create/update workflows.
 *   - TRANSFORMENGINE TODO: TimeRecorder is stored as int (0/1) on the backend; the view layer maps this
 *     to a bool checkbox. Verify that FpsViewModelMapper correctly converts int <-> bool for TimeRecorder.
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

        // TRANSFORMENGINE: Phase 7 — new fields added to mirror backend Apha.FPS.Application.Dtos.WorkGroupEmployeeDto
        // sourced from WorkGroupEmployee entity (fps.tblwgemployee), added in Phase 3 backend expansion
        public int TimeRecorder { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public double? HoursPerWeek { get; set; }
    }
}
