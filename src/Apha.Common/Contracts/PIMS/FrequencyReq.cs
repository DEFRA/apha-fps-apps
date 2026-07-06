/*
 * TRANSFORMENGINE MIGRATION — FrequencyReq.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - MS Access tlkpfrequency VBA/DAO form binding -> .NET 10 ASP.NET Core request contract
 *   - Writable ControlSource fields from mabarchive.tlkpfrequency surfaced as public properties
 *   - PK (frequencyid integer) excluded from Req body for update; supplied via route
 *   - For create, frequencyid is included as it is an integer PK (not auto-generated — must be client-supplied)
 *
 * PRESERVED:
 *   - All column names and data types from mabarchive.tlkpfrequency DDL
 *   - Nullable columns preserved as nullable properties
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm whether frequencyid is auto-generated or client-supplied on insert
 *   - TRANSFORMENGINE TODO: verify frequency varchar(50) domain — no DDL CHECK constraint found
 */

namespace Apha.Common.Contracts.PIMS
{
    // TRANSFORMENGINE: PK (frequencyid) included in Req — integer PK with no DDL sequence; client-supplied or route-bound on update
    public class FrequencyReq
    {
        // TRANSFORMENGINE: integer PK — include for create; supplied via route for update; omit if auto-generated
        public int FrequencyId { get; set; }

        public string? Frequency { get; set; }
    }
}
