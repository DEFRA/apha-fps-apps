/*
 * TRANSFORMENGINE MIGRATION — FrequencyDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Application DTO mirroring Apha.PIMS.Core.Entities.Frequency
 *   - Single integer PK (Frequencyid) — lookup/reference table
 *
 * PRESERVED:
 *   - All field names consistent with entity naming convention
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated.
 */

namespace Apha.PIMS.Application.Dtos
{
    // TRANSFORMENGINE: DTO maps to/from Apha.PIMS.Core.Entities.Frequency via EntityMapper; single integer PK (Frequencyid); lookup/reference
    public class FrequencyDto
    {
        public int Frequencyid { get; set; }

        public string? FrequencyValue { get; set; }
    }
}
