/*
 * TRANSFORMENGINE MIGRATION — StaffJobLogRes.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - fps.staffjob_log PostgreSQL table + projectaudit_trail.js initializeStaffPlanChangesTable() grid → C# response contract
 *   - 8 grid columns mapped to C# properties (7 from DB + 1 computed: UserEmail)
 *   - PostgreSQL double precision → double; timestamp without time zone → DateTime?;
 *     character varying → string?; char(2) → string?; integer → int
 *   - Note: "name" field appears in JS grid but is not a column in staffjob_log;
 *     it is resolved from a staff lookup join (TRANSFORMENGINE TODO below)
 *
 * PRESERVED:
 *   - All column names from initializeStaffPlanChangesTable() grid definition (camelCase → PascalCase)
 *   - All nullable semantics from the PostgreSQL DDL
 *   - Audit metadata fields: DateTime, UserId, UserEmail, InsertDelete
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: "Name" field is rendered by the JS grid but has no corresponding column
 *     in fps.staffjob_log — backend must join to staff/employee table to resolve staffid → full name
 *   - TRANSFORMENGINE TODO: UserEmail is resolved client-side in legacy JS; backend should resolve
 *     from a staff/user directory service
 */

namespace Apha.Common.Contracts.FPS
{
    // TRANSFORMENGINE: response contract for GET /fps/project-audit-trail — staffjob_log tab (8 columns)
    // Source: fps.staffjob_log table + initializeStaffPlanChangesTable() in projectaudit_trail.js
    public class StaffJobLogRes
    {
        // TRANSFORMENGINE: grid field "staffId" — DB col staffid varchar(50) NOT NULL
        public string StaffId { get; set; } = null!;

        // TRANSFORMENGINE: grid field "name" — NOT in staffjob_log; resolved via staff lookup join on staffid
        public string? Name { get; set; }

        // TRANSFORMENGINE: grid field "jobcode" — DB col jobcode varchar(20) NOT NULL
        public string JobCode { get; set; } = null!;

        // TRANSFORMENGINE: grid field "plannedHours" — DB col plannedhours double precision NOT NULL
        public double PlannedHours { get; set; }

        // TRANSFORMENGINE: grid field "dateTime" / header "Date_Time" — DB col date_time timestamp nullable
        public DateTime? DateTime { get; set; }

        // TRANSFORMENGINE: grid field "userId" / header "User_ID" — DB col user_id varchar(255) nullable
        public string? UserId { get; set; }

        // TRANSFORMENGINE: grid field "userEmail" / header "User_Email" — computed from userId by backend staff lookup
        public string? UserEmail { get; set; }

        // TRANSFORMENGINE: grid field "insertDelete" / header "Insert_Delete" — DB col insert_delete char(2) nullable
        public string? InsertDelete { get; set; }
    }
}
