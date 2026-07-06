/*
 * TRANSFORMENGINE MIGRATION — RadTrackProgRes.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 5 — API Layer - Controller + RequestMapper + DI (Steps 8-9)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - MS Access tblradtrackprog VBA/DAO form binding -> .NET 10 ASP.NET Core response contract
 *   - Full surface from mabarchive.tblradtrackprog exposed for API responses
 *   - PK (Program varchar(10)) included in Res for round-trip identity
 *
 * PRESERVED:
 *   - All column names and data types from mabarchive.tblradtrackprog DDL
 *   - Nullable columns preserved as nullable properties
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated.
 */

namespace Apha.Common.Contracts.PIMS
{
    // TRANSFORMENGINE: response contract — PK Program (natural varchar(10)) included for round-trip identity; Programme Tab
    public class RadTrackProgRes
    {
        // TRANSFORMENGINE: natural string PK — varchar(10) NOT NULL
        public string Program { get; set; } = null!;

        // TRANSFORMENGINE: boolean NOT NULL DEFAULT true — radtrack tracking flag
        public bool RadTrackProg { get; set; }

        // TRANSFORMENGINE: varchar(5) nullable — publication prefix
        public string? PublicationPrefix { get; set; }
    }
}
