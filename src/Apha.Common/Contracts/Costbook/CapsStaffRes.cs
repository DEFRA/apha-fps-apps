/*
 * TRANSFORMENGINE MIGRATION — CapsStaffRes.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New contract created from mabarchive.tblcapsstaff table surface
 *   - Full RecordSource for Tab 5 (CAPS Staff) data grid and CRUD responses
 *   - Covers GET /api/v1/CapsStaff list, GET /api/v1/CapsStaff/{mNumber}, POST, and PUT responses
 *   - Dt2Number included as nullable to preserve the full entity surface (not shown in modal but present in DB)
 *
 * PRESERVED:
 *   - All column names preserved from PostgreSQL DDL (mnumber, name, dt2number)
 *   - No EF entity or repository concerns in contract
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated.
 */

namespace Apha.Common.Contracts.Costbook
{
    // TRANSFORMENGINE: Res contract for GET /api/v1/CapsStaff and CRUD responses — drives Tab 5 data grid
    public class CapsStaffRes
    {
        // TRANSFORMENGINE: maps to mabarchive.tblcapsstaff.mnumber (PK varchar 50)
        /// <summary>Staff member M-Number. Primary key.</summary>
        public string MNumber { get; set; } = string.Empty;

        // TRANSFORMENGINE: maps to mabarchive.tblcapsstaff.name (varchar 50)
        /// <summary>Staff member full name.</summary>
        public string Name { get; set; } = string.Empty;

        // TRANSFORMENGINE: maps to mabarchive.tblcapsstaff.dt2number (varchar 50, nullable)
        /// <summary>DT2 number reference. Optional — not required by the maintenance form modal.</summary>
        public string? Dt2Number { get; set; }
    }
}
