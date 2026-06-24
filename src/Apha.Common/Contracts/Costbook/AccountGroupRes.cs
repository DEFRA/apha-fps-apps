/*
 * TRANSFORMENGINE MIGRATION — AccountGroupRes.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New contract created from mabarchive.tblcsg7_accountgroups table surface
 *   - Full RecordSource for Tab 3 (CSG7 Inflation Options) data grid and dropdown lookup in Tab 2 (Account Categories modal)
 *   - Covers GET /api/v1/AccountGroup list and single-item responses
 *
 * PRESERVED:
 *   - All column names preserved from PostgreSQL DDL (csg7group, useinflation)
 *   - No EF entity or repository concerns in contract
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated.
 */

namespace Apha.Common.Contracts.Costbook
{
    // TRANSFORMENGINE: Res contract for GET /api/v1/AccountGroup — drives Tab 3 data grid and AccCat modal dropdown
    public class AccountGroupRes
    {
        // TRANSFORMENGINE: maps to mabarchive.tblcsg7_accountgroups.csg7group (PK varchar 15)
        /// <summary>CSG7 group name. Primary key.</summary>
        public string Csg7Group { get; set; } = string.Empty;

        // TRANSFORMENGINE: maps to mabarchive.tblcsg7_accountgroups.useinflation (boolean, default true)
        /// <summary>Whether inflation is applied to costs in this CSG7 group.</summary>
        public bool UseInflation { get; set; }
    }
}
