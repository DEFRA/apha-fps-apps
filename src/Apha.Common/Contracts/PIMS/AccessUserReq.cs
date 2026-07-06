/*
 * TRANSFORMENGINE MIGRATION — AccessUserReq.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - MS Access tblaccessusers VBA/DAO form binding -> .NET 10 ASP.NET Core request contract
 *   - Writable ControlSource fields from mabarchive.tblaccessusers surfaced as public properties
 *   - Composite PK (systemid, ntlogin) included in Req — both are required to create or identify a user
 *   - username and dt2login are the non-key writable fields
 *
 * PRESERVED:
 *   - All column names and data types from mabarchive.tblaccessusers DDL
 *   - NOT NULL / PK constraints reflected as non-nullable properties
 *   - FK relationship to tblaccesssystems via systemid preserved semantically
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm whether systemid is supplied by the client or inferred from session/route
 *   - TRANSFORMENGINE TODO: verify NT login uniqueness constraints across systems
 *   - TRANSFORMENGINE TODO: confirm dt2login purpose — secondary login field or timestamp string
 */

namespace Apha.Common.Contracts.PIMS
{
    // TRANSFORMENGINE: composite PK (systemid, ntlogin) included — both required for create/update/delete
    public class AccessUserReq
    {
        // TRANSFORMENGINE: composite PK part 1 — FK to tblaccesssystems
        public int SystemId { get; set; }

        // TRANSFORMENGINE: composite PK part 2 — NT login identifier (varchar 50, NOT NULL)
        public string NtLogin { get; set; } = null!;

        public string? UserName { get; set; }
        public string? Dt2Login { get; set; }
    }
}
