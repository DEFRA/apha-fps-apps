// TRANSFORMENGINE: human_review — verify before running
/*
 * TRANSFORMENGINE MIGRATION — tblwgemployee (PostgreSQL) → WorkGroupEmployee.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-06-11
 *
 * CHANGED:
 *   - PostgreSQL table fps.tblwgemployee (list-partitioned by fpsyear) → EF Core partial entity class
 *   - snake_case column names mapped to PascalCase C# properties
 *   - Composite PK (pactid, fpsyear) — key configuration deferred to IEntityTypeConfiguration in Infrastructure layer
 *   - PostgreSQL double precision → C# double; varchar → string; timestamp without time zone → DateTime?
 *   - makeavailable and timerecorder stored as integer (0/1) — displayed as checkboxes in UI; mapping preserved as int
 *
 * PRESERVED:
 *   - All 15 column mappings from tblwgemployee DDL
 *   - Nullable/non-nullable semantics match DDL constraints exactly
 *   - Partial class declaration to allow Infrastructure-layer EF configuration partial
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: EF IEntityTypeConfiguration must register composite PK (PactId, FpsYear) and table name "tblwgemployee" in schema "fps"
 *   - TRANSFORMENGINE TODO: Partition-transparent access — EF targets the parent table; verify query plans against partitions y2016–y2026 in load testing
 *   - TRANSFORMENGINE TODO: HrsAvail is stored in the DB (not computed in C#); ensure the repository does NOT recompute it on read — use the stored value
 */

namespace Apha.FPS.Core.Entities
{
    // TRANSFORMENGINE: entity maps to fps.tblwgemployee (partitioned by fpsyear); composite PK = (PactId, FpsYear)
    public partial class WorkGroupEmployee
    {
        public string PactId { get; set; } = null!;

        public string SpNumber { get; set; } = null!;

        public string WorkGroupGrade { get; set; } = null!;

        // TRANSFORMENGINE: DEFAULT 'A' in DDL; non-nullable in entity — application must supply value on insert
        public string PersonStatus { get; set; } = null!;

        public string? PersonClass { get; set; }

        public double HrsPaid { get; set; }

        public double Leave { get; set; }

        public double SickSpecial { get; set; }

        // TRANSFORMENGINE: HrsAvail = HrsPaid - (Leave + SickSpecial); stored in DB, not computed by C# entity
        public double HrsAvail { get; set; }

        // TRANSFORMENGINE: stored as int (-1 = available/true, 0 = false) — matches MS Access boolean convention
        public int MakeAvailable { get; set; }

        // TRANSFORMENGINE: stored as int (0/1) — displayed as checkbox in UI; kept as int per DDL
        public int TimeRecorder { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public double? HoursPerWeek { get; set; }

        public int FpsYear { get; set; }
    }
}
