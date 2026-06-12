// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — WorkGroupEmployeeDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services
 * Migrated : 2026-06-11
 *
 * CHANGED:
 *   - Added TimeRecorder (int) property — maps to fps.tblwgemployee.time_recorder; stored as int (0/1), displayed as checkbox in UI
 *   - Added StartDate (DateTime?) property — maps to fps.tblwgemployee.start_date
 *   - Added EndDate (DateTime?) property — maps to fps.tblwgemployee.end_date
 *   - Added HoursPerWeek (double?) property — maps to fps.tblwgemployee.hours_per_week
 *
 * PRESERVED:
 *   - All existing properties: PactId, SpNumber, WorkGroupGrade, Name, PersonStatus, PersonClass,
 *     HrsPaid, Leave, SickSpecial, HrsAvail, MakeAvailable
 *   - Nullability contracts unchanged for pre-existing fields
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: FpsYear is on the entity (composite PK with PactId) but absent from this DTO.
 *     Confirm whether FpsYear should be exposed via the DTO for create/update workflows.
 */

namespace Apha.FPS.Application.Dtos
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

        // TRANSFORMENGINE: new fields added — TimeRecorder, StartDate, EndDate, HoursPerWeek
        // sourced from WorkGroupEmployee entity (fps.tblwgemployee) Phase 3 expansion
        public int TimeRecorder { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public double? HoursPerWeek { get; set; }
    }
}
