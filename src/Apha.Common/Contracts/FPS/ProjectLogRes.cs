/*
 * TRANSFORMENGINE MIGRATION — ProjectLogRes.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - fps.project_log PostgreSQL table + projectaudit_trail.js initializeProjectAuditTrailTable() grid → C# response contract
 *   - 33 grid columns mapped to C# properties (32 from DB + 1 computed: UserEmail)
 *   - PostgreSQL money → decimal?; timestamp without time zone → DateTime?; smallint → short?;
 *     character varying → string?; double precision → double?; integer → int; char(2) → string?
 *   - userEmail is a computed/resolved field (not in DB) — kept as string? matching JS decorateAuditRowsWithEmail()
 *   - Phase 6 Backend Readiness Gate — VERIFIED: 33 properties present matching
 *     initializeProjectAuditTrailTable() JS grid column definitions; all audit metadata fields
 *     (DateTime, UserId, UserEmail, InsertDelete) confirmed; AutoMapper convention mapping
 *     to ProjectLogDto confirmed via inspection; no structural changes required
 *
 * PRESERVED:
 *   - All column names from initializeProjectAuditTrailTable() grid definition (camelCase → PascalCase)
 *   - All nullable semantics from the PostgreSQL DDL
 *   - Audit metadata fields: DateTime, UserId, UserEmail, InsertDelete
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: UserEmail is resolved client-side in legacy JS via a hardcoded lookup map;
 *     the backend should resolve this from a staff/user directory service
 *   - TRANSFORMENGINE TODO: PostgreSQL money columns are mapped to decimal? — verify EF Core mapping
 *     uses NpgsqlDbType.Money or a value converter to avoid precision loss
 */

namespace Apha.Common.Contracts.FPS
{
    // TRANSFORMENGINE: response contract for GET /fps/project-audit-trail — project_log tab (33 columns)
    // Source: fps.project_log table + initializeProjectAuditTrailTable() in projectaudit_trail.js
    public class ProjectLogRes
    {
        // TRANSFORMENGINE: grid field "parentProject" — DB col parentproject varchar(20) NOT NULL
        public string ParentProject { get; set; } = null!;

        // TRANSFORMENGINE: grid field "projectTitle" — DB col projecttitle varchar(200) NOT NULL
        public string ProjectTitle { get; set; } = null!;

        // TRANSFORMENGINE: grid field "program" — DB col program varchar(10) NOT NULL
        public string Program { get; set; } = null!;

        // TRANSFORMENGINE: grid field "customer" — DB col customer varchar(50) NOT NULL
        public string Customer { get; set; } = null!;

        // TRANSFORMENGINE: grid field "manager" — DB col manager varchar(50) nullable
        public string? Manager { get; set; }

        // TRANSFORMENGINE: grid field "transferIncome" — DB col transferincome money NOT NULL
        public decimal TransferIncome { get; set; }

        // TRANSFORMENGINE: grid field "custIncome" — DB col custincome money NOT NULL
        public decimal CustIncome { get; set; }

        // TRANSFORMENGINE: grid field "wipEc" / header "WIP_EOY" — DB col wip_eoy money nullable
        public decimal? WipEoy { get; set; }

        // TRANSFORMENGINE: grid field "wipLim" / header "WIP_Limit" — DB col wip_limit money nullable
        public decimal? WipLimit { get; set; }

        // TRANSFORMENGINE: grid field "wipC" / header "WIP_Current" — DB col wip_current money nullable
        public decimal? WipCurrent { get; set; }

        // TRANSFORMENGINE: grid field "projectStatus" — DB col projectstatus varchar(50) NOT NULL
        public string ProjectStatus { get; set; } = null!;

        // TRANSFORMENGINE: grid field "costBookNo" — DB col costbookno varchar(50) nullable
        public string? CostBookNo { get; set; }

        // TRANSFORMENGINE: grid field "dateCreated" — DB col datecreated timestamp nullable
        public DateTime? DateCreated { get; set; }

        // TRANSFORMENGINE: grid field "feCost" / header "FECost" — DB col feccost money nullable
        public decimal? FecCost { get; set; }

        // TRANSFORMENGINE: grid field "profit" — DB col profit money nullable
        public decimal? Profit { get; set; }

        // TRANSFORMENGINE: grid field "budgetCvl" / header "Budget_CVL" — DB col budget_cvl money nullable
        public decimal? BudgetCvl { get; set; }

        // TRANSFORMENGINE: grid field "dateCosted" — DB col datecosted timestamp nullable
        public DateTime? DateCosted { get; set; }

        // TRANSFORMENGINE: grid field "disease" — DB col disease varchar(50) NOT NULL
        public string Disease { get; set; } = null!;

        // TRANSFORMENGINE: grid field "contract" — DB col contract varchar(10) NOT NULL
        public string Contract { get; set; } = null!;

        // TRANSFORMENGINE: grid field "projectParent" — DB col projectparent varchar(50) nullable
        public string? ProjectParent { get; set; }

        // TRANSFORMENGINE: grid field "shortTitle" — DB col shorttitle varchar(30) nullable
        public string? ShortTitle { get; set; }

        // TRANSFORMENGINE: grid field "caseworkSub" — DB col caseworksub numeric(5,4) nullable
        public decimal? CaseworkSub { get; set; }

        // TRANSFORMENGINE: grid field "pvsIncome" — DB col pvsincome money nullable
        public decimal? PvsIncome { get; set; }

        // TRANSFORMENGINE: grid field "planCaseworkDebit" — DB col plancaseworkdebit money nullable
        public decimal? PlanCaseworkDebit { get; set; }

        // TRANSFORMENGINE: grid field "finished" — DB col finished smallint nullable
        public short? Finished { get; set; }

        // TRANSFORMENGINE: grid field "owningRc" — DB col owningrc varchar(50) nullable
        public string? OwningRc { get; set; }

        // TRANSFORMENGINE: grid field "comments" — DB col comments text nullable
        public string? Comments { get; set; }

        // TRANSFORMENGINE: grid field "carryOver" — DB col carryover money nullable
        public decimal? CarryOver { get; set; }

        // TRANSFORMENGINE: grid field "carryOverSeed" — DB col carryoverseed money nullable
        public decimal? CarryOverSeed { get; set; }

        // TRANSFORMENGINE: grid field "dateTime" / header "Date_Time" — DB col date_time timestamp nullable; audit timestamp
        public DateTime? DateTime { get; set; }

        // TRANSFORMENGINE: grid field "userId" / header "User_ID" — DB col user_id varchar(255) nullable
        public string? UserId { get; set; }

        // TRANSFORMENGINE: grid field "userEmail" / header "User_Email" — computed from userId by backend staff lookup
        // (legacy: resolved client-side via hardcoded auditUserEmailById map in projectaudit_trail.js)
        public string? UserEmail { get; set; }

        // TRANSFORMENGINE: grid field "insertDelete" / header "Insert_Delete" — DB col insert_delete char(2) nullable
        public string? InsertDelete { get; set; }
    }
}
