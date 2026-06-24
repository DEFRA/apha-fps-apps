/*
 * TRANSFORMENGINE MIGRATION — CapsStaff.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New entity created from PostgreSQL DDL mabarchive.tblcapsstaff
 *   - MNumber mapped from mnumber (PK, varchar 50)
 *   - Name mapped from name (varchar 50, NOT NULL)
 *   - Dt2Number mapped from dt2number (varchar 50, nullable)
 *   - Supports CAPS Staff Tab (Tab 5) of frmMaintainance maintenance screen
 *
 * PRESERVED:
 *   - All column names and nullability constraints from DDL source
 *   - Partial class modifier for EF Core configuration split
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: EF map (CapsStaffMap.cs) must reference schema mabarchive and table tblcapsstaff
 */

namespace Apha.Costbook.Core.Entities
{
    // TRANSFORMENGINE: Entity derived from mabarchive.tblcapsstaff DDL — mnumber PK, name NOT NULL, dt2number nullable
    public partial class CapsStaff
    {
        /// <summary>Primary key — staff member number (mnumber in DDL).</summary>
        public string MNumber { get; set; } = null!;

        /// <summary>Staff member display name (name in DDL, NOT NULL).</summary>
        public string Name { get; set; } = null!;

        /// <summary>Optional DT2 number (dt2number in DDL, nullable).</summary>
        public string? Dt2Number { get; set; }
    }
}
