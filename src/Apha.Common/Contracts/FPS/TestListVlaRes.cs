/*
 * TRANSFORMENGINE MIGRATION — TestListVlaRes.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 6 — Backend Readiness Gate - Route + Contract + Mapper Confirmation
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - Phase 1: New contract created from fps.testorproduct PostgreSQL table DDL
 *   - Phase 1: Composite PK (itemcode, fpsyear) surfaced as response fields
 *   - Phase 1: All columns from fps.testorproduct mapped to PascalCase C# properties
 *   - Phase 1: money PostgreSQL type mapped to decimal?/decimal in C#
 *   - Phase 6: Readiness gate confirmed — all 11 response fields verified against frontend ViewModel needs
 *   - Phase 6: Field coverage confirmed: ItemCode, FpsYear (composite PK), ItemDescription, TestManager, JobStatus,
 *     UnitPriceVla, PriceAhvg, Owner, ChargeMethod, ShortDescription, DefraUnitPrice
 *   - Phase 6: All fields align with TestListVlaDto (1:1 mapping, no ForMember projections required)
 *   - Phase 6: Frontend ViewModel TestListVlaItem will mirror these 11 fields exactly
 *   - Phase 6: Nullability confirmed correct: ItemCode (required), FpsYear (int), all pricing nullable
 *     except DefraUnitPrice (NOT NULL DEFAULT 0 in DDL)
 *
 * PRESERVED:
 *   - All column names from fps.testorproduct (itemcode, itemdescription, testmanager,
 *     jobstatus, unitpricevla, priceahvg, owner, chargemethod, shortdescription,
 *     defraunitprice, fpsyear)
 *   - Nullability matches DDL nullable constraints
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Verify owner CHECK constraint values (PT/PA/SD/LT) are
 *     enforced at the service/validation layer, not the contract layer.
 */

namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// Response contract for a TestOrProduct VLA list item.
    /// Maps to fps.testorproduct (composite PK: ItemCode + FpsYear).
    /// </summary>
    public class TestListVlaRes
    {
        // TRANSFORMENGINE: Composite PK fields from fps.testorproduct — both required in response for route resolution
        public string ItemCode { get; set; } = null!;
        public int FpsYear { get; set; }

        public string? ItemDescription { get; set; }
        public string? TestManager { get; set; }
        public string? JobStatus { get; set; }

        // TRANSFORMENGINE: PostgreSQL money type → decimal? in C# response contract
        public decimal? UnitPriceVla { get; set; }
        public decimal? PriceAhvg { get; set; }

        public string? Owner { get; set; }
        public string? ChargeMethod { get; set; }
        public string? ShortDescription { get; set; }

        // TRANSFORMENGINE: defraunitprice is NOT NULL DEFAULT 0 in DDL — non-nullable decimal
        public decimal DefraUnitPrice { get; set; }
    }
}
