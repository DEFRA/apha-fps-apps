/*
 * TRANSFORMENGINE MIGRATION — AccountCategoryMaintenanceDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + Services
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New DTO created for AccountCategory maintenance operations (Tab 2 of frmMaintainance)
 *   - Mirrors FpsAccountCategory entity surface scoped to the maintenance grid columns:
 *     AccShortName (PK), AccountDescription, Csg7Group (maintained field), FpsYear
 *   - AutoMapper CreateMap<FpsAccountCategory, AccountCategoryMaintenanceDto>().ReverseMap() in EntityMapper
 *   - Source: fps[year].tblkpaccountcategory; only Csg7Group is writable via maintenance endpoint
 *
 * PRESERVED:
 *   - Nullability matches entity: AccShortName non-null; AccountDescription and Csg7Group nullable
 *   - FpsYear retained for partition context (matches AccountCategoryMaintenanceRes contract)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether ConstituentAccountCodes, AccountType, ProjectSpecific, RcSpecific are needed in the maintenance grid DTO
 *   - TRANSFORMENGINE TODO: Confirm whether FpsYear is supplied from server-side CurrentFinancialYear setting or client context
 */

namespace Apha.Costbook.Application.Dtos
{
    // TRANSFORMENGINE: Service-layer DTO for fps[year].tblkpaccountcategory maintenance view — drives Tab 2 (Account Categories) grid
    public class AccountCategoryMaintenanceDto
    {
        // TRANSFORMENGINE: Maps to FpsAccountCategory.AccShortName (PK, varchar 50)
        /// <summary>Account short name. Part of the composite primary key.</summary>
        public string AccShortName { get; set; } = null!;

        // TRANSFORMENGINE: Maps to FpsAccountCategory.AccountDescription (varchar 50, nullable)
        /// <summary>Human-readable description of the account category.</summary>
        public string? AccountDescription { get; set; }

        // TRANSFORMENGINE: Maps to FpsAccountCategory.Csg7Group (char 15, nullable) — the maintained field
        /// <summary>Assigned CSG7 group for this account category. Nullable — may be unassigned.</summary>
        public string? Csg7Group { get; set; }

        // TRANSFORMENGINE: Maps to FpsAccountCategory.FpsYear (int, PK component) — identifies partition
        /// <summary>FPS financial year. Part of the composite primary key (partitioned table).</summary>
        public int FpsYear { get; set; }
    }
}
