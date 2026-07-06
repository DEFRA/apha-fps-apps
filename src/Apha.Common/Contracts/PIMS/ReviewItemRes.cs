/*
 * TRANSFORMENGINE MIGRATION — ReviewItemRes.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - MS Access tlkpreviewitem VBA/DAO lookup binding -> .NET 10 ASP.NET Core response contract
 *   - Full RecordSource surface from mabarchive.tlkpreviewitem exposed for lookup/dropdown responses
 *   - PK (ItemId integer) included in Res for round-trip identity
 *
 * PRESERVED:
 *   - All column names and data types from mabarchive.tlkpreviewitem DDL
 *   - Nullable columns preserved as nullable properties
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: none — fully automated from DDL
 */

namespace Apha.Common.Contracts.PIMS
{
    // TRANSFORMENGINE: lookup/dropdown response — PK ItemId included for round-trip identity
    public class ReviewItemRes
    {
        // TRANSFORMENGINE: PK — integer review item identifier (NOT NULL)
        public int ItemId { get; set; }

        public string? Item { get; set; }
    }
}
