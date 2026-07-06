/*
 * TRANSFORMENGINE MIGRATION — AccessLevelRes.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - MS Access tblaccesslevels VBA/DAO lookup binding -> .NET 10 ASP.NET Core response contract
 *   - Full RecordSource surface from mabarchive.tblaccesslevels exposed for lookup/dropdown responses
 *   - Composite PK (SystemId, AccessLevelId) included in Res for round-trip identity
 *
 * PRESERVED:
 *   - All column names and data types from mabarchive.tblaccesslevels DDL
 *   - NOT NULL / PK constraints reflected as non-nullable properties
 *   - FK relationship to tblaccesssystems via SystemId preserved semantically
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: none — fully automated from DDL
 */

namespace Apha.Common.Contracts.PIMS
{
    // TRANSFORMENGINE: lookup/dropdown response — composite PK (SystemId, AccessLevelId) included for identity
    public class AccessLevelRes
    {
        // TRANSFORMENGINE: composite PK part 1 — FK to tblaccesssystems
        public int SystemId { get; set; }

        // TRANSFORMENGINE: composite PK part 2 — access level identifier (integer, NOT NULL)
        public int AccessLevelId { get; set; }

        public string? AccessLevel { get; set; }
    }
}
