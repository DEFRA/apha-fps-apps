/*
 * TRANSFORMENGINE MIGRATION — AnimalRequestLogRes.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - fps.animalreq_log PostgreSQL table + projectaudit_trail.js initializeAnimalRequirementChangesTable() grid → C# response contract
 *   - 8 grid columns mapped to C# properties (7 from DB + 1 computed: UserEmail)
 *   - PostgreSQL double precision → double; timestamp without time zone → DateTime?;
 *     character varying → string?; char(2) → string?
 *
 * PRESERVED:
 *   - All column names from initializeAnimalRequirementChangesTable() grid definition (camelCase → PascalCase)
 *   - All nullable semantics from the PostgreSQL DDL
 *   - Audit metadata fields: DateTime, UserId, UserEmail, InsertDelete
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: UserEmail is resolved client-side in legacy JS; backend should resolve
 *     from a staff/user directory service
 */

namespace Apha.Common.Contracts.FPS
{
    // TRANSFORMENGINE: response contract for GET /fps/project-audit-trail — animalreq_log tab (8 columns)
    // Source: fps.animalreq_log table + initializeAnimalRequirementChangesTable() in projectaudit_trail.js
    public class AnimalRequestLogRes
    {
        // TRANSFORMENGINE: grid field "jobCode" — DB col jobcode varchar(20) NOT NULL
        public string JobCode { get; set; } = null!;

        // TRANSFORMENGINE: grid field "animalType" — DB col animaltype varchar(50) NOT NULL
        public string AnimalType { get; set; } = null!;

        // TRANSFORMENGINE: grid field "numberOfDays" — DB col numberofdays double precision NOT NULL
        public double NumberOfDays { get; set; }

        // TRANSFORMENGINE: grid field "numberOfAnimals" — DB col numberofanimals double precision NOT NULL
        public double NumberOfAnimals { get; set; }

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
