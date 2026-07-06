/*
 * TRANSFORMENGINE MIGRATION — ReviewItemDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Application DTO mirroring Apha.PIMS.Core.Entities.ReviewItem
 *   - Single integer PK (Itemid) — lookup/reference table
 *
 * PRESERVED:
 *   - All field names consistent with entity naming convention
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated.
 */

namespace Apha.PIMS.Application.Dtos
{
    // TRANSFORMENGINE: DTO maps to/from Apha.PIMS.Core.Entities.ReviewItem via EntityMapper; single integer PK (Itemid); lookup/reference
    public class ReviewItemDto
    {
        public int Itemid { get; set; }

        public string? Item { get; set; }
    }
}
