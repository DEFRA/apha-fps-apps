/*
 * TRANSFORMENGINE MIGRATION — RadTrackProgReq.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - MS Access tblradtrackprog VBA/DAO form binding -> .NET 10 ASP.NET Core request contract
 *   - Writable ControlSource fields from mabarchive.tblradtrackprog surfaced as public properties
 *   - PK (program varchar(10)) included in Req — natural string key supplied on create; also route-bound on update
 *   - radtrackprog boolean NOT NULL DEFAULT true preserved as non-nullable bool
 *   - publicationprefix varchar(5) is optional (nullable)
 *
 * PRESERVED:
 *   - All column names and data types from mabarchive.tblradtrackprog DDL
 *   - NOT NULL / DEFAULT constraints reflected accurately
 *   - FK usage: tblradtrackprog.program is referenced by tblaccessprograms FK
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm whether Program (natural varchar PK) is client-supplied or validated against a lookup on create
 *   - TRANSFORMENGINE TODO: verify publicationprefix varchar(5) max length is enforced via validation attribute
 *   - TRANSFORMENGINE TODO: confirm Programme Tab in form maps solely to tblradtrackprog or also tblaccessprograms
 */

namespace Apha.Common.Contracts.PIMS
{
    // TRANSFORMENGINE: natural varchar PK (program varchar 10) included in Req — supplied by client on create; route-bound on update
    public class RadTrackProgReq
    {
        // TRANSFORMENGINE: natural PK varchar(10) — NOT NULL, must be supplied by client
        public string Program { get; set; } = null!;

        // TRANSFORMENGINE: boolean NOT NULL DEFAULT true — radtrack tracking flag for this programme
        public bool RadTrackProg { get; set; } = true;

        // TRANSFORMENGINE: varchar(5) nullable — publication prefix for the programme
        public string? PublicationPrefix { get; set; }
    }
}
