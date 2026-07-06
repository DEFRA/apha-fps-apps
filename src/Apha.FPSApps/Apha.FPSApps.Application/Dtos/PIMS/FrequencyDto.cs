/*
 * TRANSFORMENGINE MIGRATION — FrequencyDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New frontend DTO mirroring Apha.PIMS.Application.Dtos.FrequencyDto
 *   - Placed in Apha.FPSApps.Application.Dtos.PIMS namespace for frontend consumption
 *   - Other Tab lookup CRUD; integer PK (Frequencyid)
 *
 * PRESERVED:
 *   - All property names match backend DTO exactly (case-sensitive)
 *   - Nullable reference types match backend DTO nullability
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - DEFERRED: none — fully automated.
 */

namespace Apha.FPSApps.Application.Dtos.PIMS
{
    // TRANSFORMENGINE: Frontend DTO mirroring Apha.PIMS.Application.Dtos.FrequencyDto — Other Tab lookup CRUD; integer PK (Frequencyid)
    public class FrequencyDto
    {
        public int Frequencyid { get; set; }
        public string? FrequencyValue { get; set; }
    }
}
