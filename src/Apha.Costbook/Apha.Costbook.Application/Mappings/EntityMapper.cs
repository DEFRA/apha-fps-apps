/*
 * TRANSFORMENGINE MIGRATION — EntityMapper.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + Services
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - Added CreateMap<CapsStaff, CapsStaffDto>().ReverseMap()
 *   - Added CreateMap<AccountGroup, AccountGroupDto> with explicit ForMember mappings:
 *       Csg7group (entity, lowercase g) → Csg7Group (DTO, uppercase G)
 *       Useinflation (entity, bool?) → UseInflation (DTO, bool) with null → false coalescing
 *   - Added CreateMap<FpsAccountCategory, AccountCategoryMaintenanceDto> with ForMember:
 *       FpsYear (entity, int?) → FpsYear (DTO, int) with null → 0 coalescing
 *   - MaintenanceSettingsDto has NO AutoMapper entry — service builds it manually from tbl_settings rows
 *
 * PRESERVED:
 *   - All pre-existing CreateMap entries (Pagination, Project, Program, Customer, Disease, Staff) unchanged
 *   - Namespace and class structure unchanged
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm FpsYear null → 0 coalescing is acceptable for FpsAccountCategory mapping (vs. throwing)
 *   - TRANSFORMENGINE TODO: Confirm UseInflation null → false coalescing is acceptable for AccountGroup mapping
 */

using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Pagination;
using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Pagination;
using Apha.Costbook.DataAccess;
using AutoMapper;

namespace Apha.Costbook.Application.Mappings
{
    public class EntityMapper : Profile
    {
        public EntityMapper()
        {
            CreateMap(typeof(PaginationParameters<>), typeof(QueryParameters<>)).ReverseMap();
            CreateMap(typeof(PagedData<>), typeof(PaginatedResult<>)).ReverseMap();

            CreateMap<PaginationData, PaginationDto>().ReverseMap();
			CreateMap<Project, ProjectDto>().ReverseMap();
            CreateMap<Program, ProgramDto>().ReverseMap();
            CreateMap<Customer, CustomerDto>().ReverseMap();
            CreateMap<Disease, DiseaseDto>().ReverseMap();
            CreateMap<Staff, StaffDto>().ReverseMap();

            // TRANSFORMENGINE: CapsStaff ↔ CapsStaffDto — property names match exactly; direct bidirectional map
            CreateMap<CapsStaff, CapsStaffDto>().ReverseMap();

            // TRANSFORMENGINE: AccountGroup → AccountGroupDto
            //   Csg7group (entity lowercase 'g') → Csg7Group (DTO uppercase 'G') — explicit to avoid AutoMapper case mismatch
            //   Useinflation (entity bool?) → UseInflation (DTO bool) — null coalesced to false
            CreateMap<AccountGroup, AccountGroupDto>()
                .ForMember(dest => dest.Csg7Group,    opt => opt.MapFrom(src => src.Csg7group))
                .ForMember(dest => dest.UseInflation, opt => opt.MapFrom(src => src.Useinflation ?? false));

            // TRANSFORMENGINE: AccountGroupDto → AccountGroup (reverse)
            //   Csg7Group (DTO) → Csg7group (entity) — explicit reverse member mapping
            //   UseInflation (DTO bool) → Useinflation (entity bool?) — direct cast is safe (bool → bool?)
            CreateMap<AccountGroupDto, AccountGroup>()
                .ForMember(dest => dest.Csg7group,    opt => opt.MapFrom(src => src.Csg7Group))
                .ForMember(dest => dest.Useinflation, opt => opt.MapFrom(src => (bool?)src.UseInflation));

            // TRANSFORMENGINE: FpsAccountCategory → AccountCategoryMaintenanceDto (maintenance grid surface only)
            //   FpsYear (entity int?) → FpsYear (DTO int) — null coalesced to 0
            //   AccShortName, AccountDescription, Csg7Group — direct name matches
            CreateMap<FpsAccountCategory, AccountCategoryMaintenanceDto>()
                .ForMember(dest => dest.FpsYear, opt => opt.MapFrom(src => src.FpsYear ?? 0));

            // TRANSFORMENGINE: AccountCategoryMaintenanceDto → FpsAccountCategory (reverse, used for entity construction)
            //   FpsYear (DTO int) → FpsYear (entity int?) — direct cast is safe (int → int?)
            CreateMap<AccountCategoryMaintenanceDto, FpsAccountCategory>()
                .ForMember(dest => dest.FpsYear, opt => opt.MapFrom(src => (int?)src.FpsYear));
        }
    }
}
