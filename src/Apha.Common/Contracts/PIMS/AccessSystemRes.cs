/*
 * TRANSFORMENGINE MIGRATION — AccessSystemRes.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - MS Access tblaccesssystems VBA/DAO lookup binding -> .NET 10 ASP.NET Core response contract
 *   - Full RecordSource surface from mabarchive.tblaccesssystems exposed for lookup/dropdown responses
 *   - PK (SystemId integer) included in Res for round-trip identity
 *
 * PRESERVED:
 *   - All column names and data types from mabarchive.tblaccesssystems DDL
 *   - NOT NULL constraints reflected as non-nullable properties
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: none — fully automated from DDL
 */

namespace Apha.Common.Contracts.PIMS
{
    // TRANSFORMENGINE: lookup/dropdown response — PK SystemId included for round-trip identity
    public class AccessSystemRes
    {
        // TRANSFORMENGINE: PK — integer system identifier (NOT NULL)
        public int SystemId { get; set; }

        // TRANSFORMENGINE: SystemName NOT NULL — required display field for dropdown
        public string SystemName { get; set; } = null!;
    }
}
