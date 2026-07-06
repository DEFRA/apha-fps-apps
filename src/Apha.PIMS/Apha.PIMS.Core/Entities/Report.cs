/*
 * TRANSFORMENGINE MIGRATION — Report.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Core entity derived from PostgreSQL DDL mabarchive.tblreport
 *   - All columns mapped to C# properties with appropriate nullability
 *   - Integer PK (id) — not identity-generated in this table
 *
 * PRESERVED:
 *   - All column names lowercased as per existing entity naming convention in this project
 *   - boolean NOT NULL columns kept as non-nullable bool
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: verify 'type' char(1) column usage — stored as string(1); confirm no enum mapping needed
 */

namespace Apha.PIMS.Core.Entities
{
    // TRANSFORMENGINE: entity maps to mabarchive.tblreport (PostgreSQL)
    public partial class Report
    {
        public int Id { get; set; }

        public string Reportname { get; set; } = null!;

        public string? Reportdescription { get; set; }

        public string? Filter { get; set; }

        public string? Mailcomment { get; set; }

        public string? Mailtitle { get; set; }

        public bool Emailable { get; set; }

        public int? Sortorder { get; set; }

        public bool Allowpickprogramme { get; set; }

        public bool Allowpickproject { get; set; }

        public bool Allowpickmanager { get; set; }

        public bool Allowpickcontract { get; set; }

        public bool Allowpickcustomer { get; set; }

        public bool Allowpickmonth { get; set; }

        public bool Allowpickfyear { get; set; }

        public string? Reporthelp { get; set; }

        // TRANSFORMENGINE: char(1) in PostgreSQL — mapped as string for EF Core compatibility
        public string Type { get; set; } = null!;
    }
}
