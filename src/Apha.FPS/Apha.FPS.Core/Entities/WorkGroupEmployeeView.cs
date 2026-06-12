// TRANSFORMENGINE: human_review — verify before running
/*
 * TRANSFORMENGINE MIGRATION — fps.vtblwgemployee (PostgreSQL view) → WorkGroupEmployeeView.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-06-11
 *
 * CHANGED:
 *   - PostgreSQL view fps.vtblwgemployee (join of tblwgemployee + vworkgroupgrade) → EF keyless entity class
 *   - All view columns (15 base + UserId, Dt2Username, UserEmail from vworkgroupgrade join) mapped to nullable C# properties
 *   - Additional computed property Name (from Employee join in repository LINQ) added; NOT mapped to database view
 *   - snake_case column names mapped to PascalCase; user_id → UserId, dt2username → Dt2Username, useremail → UserEmail
 *
 * PRESERVED:
 *   - All 18 columns from vtblwgemployee SQL view (15 from tblwgemployee + 3 from vworkgroupgrade)
 *   - All properties nullable to reflect view semantics (outer join results may be null)
 *   - XML summary comments documenting computed vs DB-mapped fields
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: EF IEntityTypeConfiguration must call .HasNoKey() and map ToView("vtblwgemployee", "fps")
 *   - TRANSFORMENGINE TODO: Name property must be ignored by EF configuration (.Ignore(e => e.Name)) — it is populated by LINQ join in repository, not from the DB view
 */

namespace Apha.FPS.Core.Entities
{
    /// <summary>
    /// EF keyless entity mapped to view fps.vtblwgemployee.
    /// Also used as LINQ join result shape — <see cref="Name"/> is populated by the repository
    /// join with the Employee table and is NOT a column in the database view.
    /// </summary>
    // TRANSFORMENGINE: keyless entity — must be registered with .HasNoKey() in EF configuration
    public class WorkGroupEmployeeView
    {
        public string? PactId { get; set; }
        public string? SpNumber { get; set; }
        public string? WorkGroupGrade { get; set; }
        public string? PersonStatus { get; set; }
        public string? PersonClass { get; set; }
        public double? HrsPaid { get; set; }
        public double? Leave { get; set; }
        public double? SickSpecial { get; set; }

        // TRANSFORMENGINE: HrsAvail sourced from view column (= HrsPaid - Leave - SickSpecial computed in DB)
        public double? HrsAvail { get; set; }

        // TRANSFORMENGINE: MakeAvailable stored as int (-1/0) in DB — maps to checkbox in UI
        public int? MakeAvailable { get; set; }

        // TRANSFORMENGINE: TimeRecorder stored as int (0/1) in DB — maps to checkbox in UI
        public int? TimeRecorder { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public double? HoursPerWeek { get; set; }
        public int? FpsYear { get; set; }

        // TRANSFORMENGINE: UserId, Dt2Username, UserEmail sourced from vworkgroupgrade join in vtblwgemployee view
        public int? UserId { get; set; }
        public string? Dt2Username { get; set; }
        public string? UserEmail { get; set; }

        /// <summary>
        /// Computed from Employee join in repository; NOT mapped to the database view.
        /// Must be excluded from EF configuration with .Ignore(e => e.Name).
        /// </summary>
        // TRANSFORMENGINE: Name is populated by LINQ join with fps_staff/tblemployee in repository — not a DB view column
        public string? Name { get; set; }
    }
}
