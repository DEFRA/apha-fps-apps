/*
 * TRANSFORMENGINE MIGRATION — ReviewItemReq.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - MS Access tlkpreviewitem VBA/DAO form binding -> .NET 10 ASP.NET Core request contract
 *   - Writable ControlSource fields from mabarchive.tlkpreviewitem surfaced as public properties
 *   - PK (itemid integer) included — integer PK with no DDL sequence; client-supplied or route-bound on update
 *
 * PRESERVED:
 *   - All column names and data types from mabarchive.tlkpreviewitem DDL
 *   - Nullable columns preserved as nullable properties
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm whether itemid is auto-generated or client-supplied on insert
 *   - TRANSFORMENGINE TODO: verify item varchar(50) domain constraints with business owner
 */

namespace Apha.Common.Contracts.PIMS
{
    // TRANSFORMENGINE: PK (itemid) included in Req — integer PK with no DDL sequence; client-supplied or route-bound on update
    public class ReviewItemReq
    {
        // TRANSFORMENGINE: integer PK — include for create; supplied via route for update; omit if auto-generated
        public int ItemId { get; set; }

        public string? Item { get; set; }
    }
}
