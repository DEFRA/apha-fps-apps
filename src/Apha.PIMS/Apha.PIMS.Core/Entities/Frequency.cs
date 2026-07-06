/*
 * TRANSFORMENGINE MIGRATION — Frequency.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Core entity derived from PostgreSQL DDL mabarchive.tlkpfrequency
 *   - Single integer PK (frequencyid)
 *
 * PRESERVED:
 *   - Column naming convention consistent with other lookup entities in the project
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated.
 */

namespace Apha.PIMS.Core.Entities
{
    // TRANSFORMENGINE: entity maps to mabarchive.tlkpfrequency (PostgreSQL); lookup/reference table
    public partial class Frequency
    {
        public int Frequencyid { get; set; }

        public string? FrequencyValue { get; set; }
    }
}
