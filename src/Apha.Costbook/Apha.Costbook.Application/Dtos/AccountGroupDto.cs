/*
 * TRANSFORMENGINE MIGRATION — AccountGroupDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + Services
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New DTO created for AccountGroup entity (mabarchive.tblcsg7_accountgroups)
 *   - Mirrors AccountGroup Core entity surface: Csg7Group (PK), UseInflation
 *   - Used as internal service-layer contract between IAccountGroupService and API/Frontend layers
 *   - AutoMapper CreateMap<AccountGroup, AccountGroupDto>().ReverseMap() registered in EntityMapper
 *   - Property name UseInflation (bool) matches API contract convention; entity uses Useinflation (bool?)
 *
 * PRESERVED:
 *   - All fields from mabarchive.tblcsg7_accountgroups: csg7group (PK varchar 15), useinflation (boolean)
 *   - Nullability: Csg7Group non-null (PK); UseInflation defaults false if entity value is null
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm Csg7Group max length (varchar 15) is enforced by controller/service validation
 */

namespace Apha.Costbook.Application.Dtos
{
    // TRANSFORMENGINE: Service-layer DTO for mabarchive.tblcsg7_accountgroups — drives Tab 3 (CSG7 Inflation Options) CRUD surface
    public class AccountGroupDto
    {
        // TRANSFORMENGINE: Maps to AccountGroup.Csg7group (PK, varchar 15)
        /// <summary>CSG7 group name. Primary key (varchar 15).</summary>
        public string Csg7Group { get; set; } = null!;

        // TRANSFORMENGINE: Maps to AccountGroup.Useinflation (bool?) — whether inflation applies to this group
        /// <summary>Whether inflation is applied to costs in this CSG7 group.</summary>
        public bool UseInflation { get; set; }
    }
}
