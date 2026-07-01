/*
 * TRANSFORMENGINE MIGRATION — TestListVlaDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - New DTO class created for internal service-layer contract on TestOrProduct VLA records
 *   - Mirrors TestOrProduct entity fields (composite PK: ItemCode + FpsYear) for AutoMapper round-trip
 *   - money PostgreSQL columns mapped to decimal? / decimal (matching entity and Res/Req contracts)
 *   - owner CHECK constraint (PT/PA/SD/LT) is not enforced here — service layer is responsible
 *
 * PRESERVED:
 *   - All property names and nullability aligned with TestOrProduct entity and TestListVlaRes/Req contracts
 *   - Composite PK fields: ItemCode (string, not null) + FpsYear (int)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: If the service layer needs to expose additional computed/display
 *     fields (e.g. formatted prices), add them here and configure a custom AutoMapper projection.
 */

namespace Apha.FPS.Application.Dtos
{
    /// <summary>
    /// Internal DTO for TestOrProduct VLA list entries.
    /// Used as the service-layer transfer object between repository and API controller.
    /// Maps to fps.testorproduct (composite PK: ItemCode + FpsYear).
    /// </summary>
    public class TestListVlaDto
    {
        // TRANSFORMENGINE: Composite PK fields — (ItemCode, FpsYear) matching fps.testorproduct pk
        public string ItemCode { get; set; } = null!;
        public int FpsYear { get; set; }

        public string? ItemDescription { get; set; }
        public string? TestManager { get; set; }
        public string? JobStatus { get; set; }

        // TRANSFORMENGINE: PostgreSQL money -> decimal? (nullable) for unitpricevla / priceahvg
        public decimal? UnitPriceVla { get; set; }
        public decimal? PriceAhvg { get; set; }

        // TRANSFORMENGINE: owner CHECK constraint (PT/PA/SD/LT) — validation is service-layer responsibility
        public string? Owner { get; set; }
        public string? ChargeMethod { get; set; }
        public string? ShortDescription { get; set; }

        // TRANSFORMENGINE: defraunitprice NOT NULL DEFAULT 0 — non-nullable decimal
        public decimal DefraUnitPrice { get; set; }
    }
}
