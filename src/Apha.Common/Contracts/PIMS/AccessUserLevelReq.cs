/*
 * TRANSFORMENGINE MIGRATION — AccessUserLevelReq.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - MS Access tblaccessusers_levels VBA/DAO form binding -> .NET 10 ASP.NET Core request contract
 *   - Composite PK (systemid, ntlogin, accesslevelid) included in Req — all three are required to assign or revoke an access level
 *   - Pure junction/link table: no non-key writable fields
 *
 * PRESERVED:
 *   - All column names and data types from mabarchive.tblaccessusers_levels DDL
 *   - NOT NULL / PK constraints reflected as non-nullable properties
 *   - FK relationships to tblaccesslevels and tblaccessusers preserved semantically
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm delete endpoint uses route params vs body binding for composite PK
 *   - TRANSFORMENGINE TODO: confirm systemid is consistent across tblaccessusers and tblaccesslevels joins
 */

namespace Apha.Common.Contracts.PIMS
{
    // TRANSFORMENGINE: composite PK junction table — all three FK columns are required writable inputs
    public class AccessUserLevelReq
    {
        // TRANSFORMENGINE: composite PK part 1 — FK to tblaccesssystems
        public int SystemId { get; set; }

        // TRANSFORMENGINE: composite PK part 2 — NT login, FK to tblaccessusers
        public string NtLogin { get; set; } = null!;

        // TRANSFORMENGINE: composite PK part 3 — access level FK to tblaccesslevels
        public int AccessLevelId { get; set; }
    }
}
