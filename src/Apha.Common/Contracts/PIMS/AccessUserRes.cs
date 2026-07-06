/*
 * TRANSFORMENGINE MIGRATION — AccessUserRes.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - MS Access tblaccessusers VBA/DAO form binding -> .NET 10 ASP.NET Core response contract
 *   - Full RecordSource surface from mabarchive.tblaccessusers exposed for CRUD responses
 *   - Composite PK (SystemId, NtLogin) included in Res for round-trip identity
 *
 * PRESERVED:
 *   - All column names and data types from mabarchive.tblaccessusers DDL
 *   - NOT NULL / PK constraints reflected as non-nullable properties
 *   - FK relationship to tblaccesssystems via SystemId preserved semantically
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm dt2login purpose — secondary login field or timestamp string
 *   - TRANSFORMENGINE TODO: consider including SystemName from joined tblaccesssystems for display purposes
 */

namespace Apha.Common.Contracts.PIMS
{
    // TRANSFORMENGINE: full RecordSource surface — composite PK (SystemId, NtLogin) included for list/detail responses
    public class AccessUserRes
    {
        // TRANSFORMENGINE: composite PK part 1 — FK to tblaccesssystems
        public int SystemId { get; set; }

        // TRANSFORMENGINE: composite PK part 2 — NT login identifier (varchar 50, NOT NULL)
        public string NtLogin { get; set; } = null!;

        public string? UserName { get; set; }
        public string? Dt2Login { get; set; }
    }
}
