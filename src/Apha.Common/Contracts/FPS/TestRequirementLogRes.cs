/*
 * TRANSFORMENGINE MIGRATION — TestRequirementLogRes.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - fps.testreq_log PostgreSQL table + projectaudit_trail.js initializeTestRequirementChangesTable() grid → C# response contract
 *   - 11 grid columns mapped to C# properties (10 from DB + 1 computed: UserEmail)
 *   - PostgreSQL double precision → double?; integer → int?; smallint → short?;
 *     character varying → string?; timestamp without time zone → DateTime?; char(2) → string?
 *
 * PRESERVED:
 *   - All column names from initializeTestRequirementChangesTable() grid definition (camelCase → PascalCase)
 *   - All nullable semantics from the PostgreSQL DDL (most testreq_log columns are nullable)
 *   - Audit metadata fields: DateTime, UserId, UserEmail, InsertDelete
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: UserEmail is resolved client-side in legacy JS; backend should resolve
 *     from a staff/user directory service
 *   - TRANSFORMENGINE TODO: testreq_log.jobcode DDL comment says "Generated column based on projectbuyercode"
 *     — verify whether jobcode is a true generated column or a denormalised copy when mapping EF entity
 */

namespace Apha.Common.Contracts.FPS
{
    // TRANSFORMENGINE: response contract for GET /fps/project-audit-trail — testreq_log tab (11 columns)
    // Source: fps.testreq_log table + initializeTestRequirementChangesTable() in projectaudit_trail.js
    public class TestRequirementLogRes
    {
        // TRANSFORMENGINE: grid field "testCode" — DB col testcode varchar(20) nullable
        public string? TestCode { get; set; }

        // TRANSFORMENGINE: grid field "buyer" — DB col buyer varchar(20) nullable
        public string? Buyer { get; set; }

        // TRANSFORMENGINE: grid field "unitPrice" — DB col unitprice double precision nullable
        public double? UnitPrice { get; set; }

        // TRANSFORMENGINE: grid field "noRequired" — DB col norequired integer nullable
        public int? NoRequired { get; set; }

        // TRANSFORMENGINE: grid field "projectBuyerCode" — DB col projectbuyercode varchar(50) nullable
        public string? ProjectBuyerCode { get; set; }

        // TRANSFORMENGINE: grid field "testBuyerCode" — DB col testbuyercode varchar(50) nullable
        public string? TestBuyerCode { get; set; }

        // TRANSFORMENGINE: grid field "active" — DB col active smallint nullable
        public short? Active { get; set; }

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
