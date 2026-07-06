/*
 * TRANSFORMENGINE MIGRATION — FrequencyRes.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - MS Access tlkpfrequency VBA/DAO lookup binding -> .NET 10 ASP.NET Core response contract
 *   - Full RecordSource surface from mabarchive.tlkpfrequency exposed for lookup/dropdown responses
 *   - PK (FrequencyId integer) included in Res for round-trip identity
 *
 * PRESERVED:
 *   - All column names and data types from mabarchive.tlkpfrequency DDL
 *   - Nullable columns preserved as nullable properties
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: none — fully automated from DDL
 */

namespace Apha.Common.Contracts.PIMS
{
    // TRANSFORMENGINE: lookup/dropdown response — PK FrequencyId included for round-trip identity
    public class FrequencyRes
    {
        // TRANSFORMENGINE: PK — integer frequency identifier (NOT NULL)
        public int FrequencyId { get; set; }

        public string? Frequency { get; set; }
    }
}
