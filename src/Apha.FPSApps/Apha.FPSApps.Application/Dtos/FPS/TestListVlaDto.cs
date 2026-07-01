/*
 * TRANSFORMENGINE MIGRATION — TestListVlaDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - New frontend DTO created mirroring Apha.FPS.Application.Dtos.TestListVlaDto
 *   - Namespace is Apha.FPSApps.Application.Dtos.FPS (frontend application layer)
 *   - Same property names, types, and nullability as backend DTO — 1:1 mirror for ApiDtoMapper round-trip
 *   - Composite PK (ItemCode + FpsYear) preserved
 *   - money PostgreSQL columns retained as decimal? / decimal (nullable/non-nullable matching backend)
 *
 * PRESERVED:
 *   - All 11 property names from backend TestListVlaDto / TestListVlaRes: ItemCode, FpsYear,
 *     ItemDescription, TestManager, JobStatus, UnitPriceVla, PriceAhvg, Owner,
 *     ChargeMethod, ShortDescription, DefraUnitPrice
 *   - Nullability: ItemCode (required, null!), FpsYear (int), string fields (string?),
 *     UnitPriceVla/PriceAhvg (decimal?), DefraUnitPrice (decimal, NOT NULL DEFAULT 0)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: If additional display/computed fields are added to the backend DTO,
 *     mirror them here and update FpsApiDtoMapper accordingly.
 *   - TRANSFORMENGINE TODO: owner CHECK constraint (PT/PA/SD/LT) — validation is service-layer
 *     responsibility; this DTO carries the value verbatim.
 */

namespace Apha.FPSApps.Application.Dtos.FPS
{
    /// <summary>
    /// Frontend DTO for TestOrProduct VLA list entries.
    /// Mirrors Apha.FPS.Application.Dtos.TestListVlaDto for use in the frontend
    /// application and infrastructure layers.
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

        // TRANSFORMENGINE: PostgreSQL money type → decimal? (nullable) for unitpricevla / priceahvg
        public decimal? UnitPriceVla { get; set; }
        public decimal? PriceAhvg { get; set; }

        // TRANSFORMENGINE: owner CHECK constraint (PT/PA/SD/LT) — carried verbatim, validated at service layer
        public string? Owner { get; set; }
        public string? ChargeMethod { get; set; }
        public string? ShortDescription { get; set; }

        // TRANSFORMENGINE: defraunitprice NOT NULL DEFAULT 0 in DDL — non-nullable decimal
        public decimal DefraUnitPrice { get; set; }
    }
}
