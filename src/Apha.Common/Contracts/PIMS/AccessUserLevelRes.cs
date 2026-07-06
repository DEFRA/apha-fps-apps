/*
 * TRANSFORMENGINE MIGRATION — AccessUserLevelRes.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - MS Access tblaccessusers_levels VBA/DAO form binding -> .NET 10 ASP.NET Core response contract
 *   - Full RecordSource surface from mabarchive.tblaccessusers_levels exposed for CRUD responses
 *   - Composite PK (SystemId, NtLogin, AccessLevelId) included in Res for round-trip identity
 *
 * PRESERVED:
 *   - All column names and data types from mabarchive.tblaccessusers_levels DDL
 *   - NOT NULL / PK constraints reflected as non-nullable properties
 *   - FK relationships to tblaccesslevels and tblaccessusers preserved semantically
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: consider including AccessLevel display name and UserName from joined tables for UI display
 */

namespace Apha.Common.Contracts.PIMS
{
    // TRANSFORMENGINE: full RecordSource surface — composite PK (SystemId, NtLogin, AccessLevelId) included for responses
    public class AccessUserLevelRes
    {
        // TRANSFORMENGINE: composite PK part 1 — FK to tblaccesssystems
        public int SystemId { get; set; }

        // TRANSFORMENGINE: composite PK part 2 — NT login, FK to tblaccessusers
        public string NtLogin { get; set; } = null!;

        // TRANSFORMENGINE: composite PK part 3 — access level FK to tblaccesslevels
        public int AccessLevelId { get; set; }
    }
}
