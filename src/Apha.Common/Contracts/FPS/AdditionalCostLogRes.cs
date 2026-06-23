/*
 * TRANSFORMENGINE MIGRATION — AdditionalCostLogRes.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - fps.additionalcosts_log PostgreSQL table + projectaudit_trail.js initializeExceptionalCostChangesTable() grid → C# response contract
 *   - 10 grid columns mapped to C# properties (9 from DB + 1 computed: UserEmail)
 *   - PostgreSQL money → decimal; timestamp without time zone → DateTime?;
 *     character varying → string?; char(2) → string?
 *
 * PRESERVED:
 *   - All column names from initializeExceptionalCostChangesTable() grid definition (camelCase → PascalCase)
 *   - All nullable semantics from the PostgreSQL DDL
 *   - Audit metadata fields: DateTime, UserId, UserEmail, InsertDelete
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: UserEmail is resolved client-side in legacy JS; backend should resolve
 *     from a staff/user directory service
 *   - TRANSFORMENGINE TODO: PostgreSQL money column itemcost mapped to decimal — verify EF Core mapping
 *     uses NpgsqlDbType.Money or a value converter to avoid precision loss
 */

namespace Apha.Common.Contracts.FPS
{
    // TRANSFORMENGINE: response contract for GET /fps/project-audit-trail — additionalcosts_log tab (10 columns)
    // Source: fps.additionalcosts_log table + initializeExceptionalCostChangesTable() in projectaudit_trail.js
    public class AdditionalCostLogRes
    {
        // TRANSFORMENGINE: grid field "jobCode" — DB col jobcode varchar(20) NOT NULL
        public string JobCode { get; set; } = null!;

        // TRANSFORMENGINE: grid field "account" — DB col account varchar(50) NOT NULL
        public string Account { get; set; } = null!;

        // TRANSFORMENGINE: grid field "description" — DB col description varchar(20) NOT NULL
        public string Description { get; set; } = null!;

        // TRANSFORMENGINE: grid field "itemCost" — DB col itemcost money NOT NULL
        public decimal ItemCost { get; set; }

        // TRANSFORMENGINE: grid field "freq" — DB col freq varchar(5) nullable
        public string? Freq { get; set; }

        // TRANSFORMENGINE: grid field "supplier" — DB col supplier varchar(50) nullable
        public string? Supplier { get; set; }

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
