/*
 * TRANSFORMENGINE MIGRATION — CostbookApiDtoMapper.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 10 — AutoMapper Profiles + DI Registration (Step 15)
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - Added Phase 10 CreateMap entries for frmMaintainance Maintenance screen contracts ↔ frontend DTOs:
 *       MaintenanceSettingsRes / MaintenanceSettingsReq ↔ MaintenanceSettingsDto
 *       CapsStaffRes / CapsStaffReq ↔ CapsStaffDto
 *       AccountGroupRes / AccountGroupReq ↔ AccountGroupDto
 *       AccountCategoryMaintenanceRes / AccountCategoryMaintenanceReq ↔ AccountCategoryMaintenanceDto
 *
 * PRESERVED:
 *   - All existing Phase 7/9 CreateMap entries for Project, Customer, Disease, Program, Staff, Contract,
 *     YearlyDetails, Pivot/Summary, and all lookup types — unchanged
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether AccountCategoryMaintenanceReq only carries Csg7Group (1 field) —
 *     the current mapping relies on AutoMapper convention; mismatch will throw Missing type map configuration
 */

using Apha.Common.Contracts;
using Apha.Common.Contracts.Costbook;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Pagination;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Mappings
{
    public class CostbookApiDtoMapper : Profile
    {
        public CostbookApiDtoMapper()
        {
            // ── Existing project mappings ─────────────────────────────────────
            CreateMap<ProjectDto, ProjectRes>().ReverseMap();
            CreateMap<ProjectDto, ProjectReq>().ReverseMap();
            CreateMap<CustomerDto, CustomerRes>().ReverseMap();
            CreateMap<DiseaseDto, DiseaseRes>().ReverseMap();
            CreateMap<Application.Dtos.CostBook.ProgramDto, Common.Contracts.Costbook.ProgramRes>().ReverseMap();
            CreateMap<StaffDto, StaffRes>().ReverseMap();
            CreateMap<ContractDto, ContractRes>().ReverseMap();
            CreateMap<ProjectEditDataDto, ProjectEditRes>().ReverseMap();

            // ── Yearly details: Res/Req ↔ Dto (used by CostBookYearlyDetailsApiClient) ──
            CreateMap<ProjectHeaderRes, ProjectHeaderDto>().ReverseMap();
            CreateMap<ProjectYearRes, ProjectYearDto>().ReverseMap();
            CreateMap<ProjectYearDto, ProjectYearReq>().ReverseMap();
            CreateMap<StaffRequirementRes, StaffRequirementDto>().ReverseMap();
            CreateMap<StaffRequirementDto, StaffRequirementReq>().ReverseMap();
            CreateMap<TestRequirementRes, TestRequirementDto>().ReverseMap();
            CreateMap<TestRequirementDto, TestRequirementReq>().ReverseMap();
            CreateMap<AnimalRequirementRes, AnimalRequirementDto>().ReverseMap();
            CreateMap<AnimalRequirementDto, AnimalRequirementReq>().ReverseMap();
            CreateMap<AdditionalCostRes, AdditionalCostDto>().ReverseMap();
            CreateMap<AdditionalCostDto, AdditionalCostReq>().ReverseMap();
            CreateMap<PayRateRes, PayRateDto>().ReverseMap();
            CreateMap<AnimalRateRes, AnimalRateDto>().ReverseMap();
            CreateMap<AccountCategoryRes, AccountCategoryDto>().ReverseMap();
            CreateMap<TestCodeLookupRes, TestCodeLookupDto>().ReverseMap();
            CreateMap<AnimalLookupRes, AnimalLookupDto>().ReverseMap();

            CreateMap<StaffYearsRowRes, StaffYearsRowDto>().ReverseMap();
            CreateMap<StaffYearsPivotRes, StaffYearsPivotDto>().ReverseMap();
            CreateMap<StaffEffortRowRes, StaffEffortRowDto>().ReverseMap();
            CreateMap<StaffEffortPivotRes, StaffEffortPivotDto>().ReverseMap();
            CreateMap<ProjectCostsRowRes, ProjectCostsRowDto>().ReverseMap();
            CreateMap<ProjectCostsPivotRes, ProjectCostsPivotDto>().ReverseMap();
            CreateMap<ProjectYearCostSummaryRes, ProjectYearCostSummaryDto>().ReverseMap();

            // ── Phase 10: Maintenance screen — Res/Req ↔ Dto ─────────────────

            // TRANSFORMENGINE: MaintenanceSettings — GET /api/v1/Maintenance/settings (Tab 1 + Tab 4)
            CreateMap<MaintenanceSettingsRes, MaintenanceSettingsDto>().ReverseMap();
            CreateMap<MaintenanceSettingsDto, MaintenanceSettingsReq>().ReverseMap();

            // TRANSFORMENGINE: CapsStaff — CRUD endpoints GET/POST/PUT/DELETE /api/v1/CapsStaff (Tab 5)
            CreateMap<CapsStaffRes, CapsStaffDto>().ReverseMap();
            CreateMap<CapsStaffDto, CapsStaffReq>().ReverseMap();

            // TRANSFORMENGINE: AccountGroup (CSG7) — CRUD endpoints GET/POST/PUT/DELETE /api/v1/AccountGroup (Tab 3)
            //   Also used as lookup DTO for CSG7 group dropdown in AccountCategory maintenance modal (Tab 2)
            CreateMap<AccountGroupRes, AccountGroupDto>().ReverseMap();
            CreateMap<AccountGroupDto, AccountGroupReq>().ReverseMap();

            // TRANSFORMENGINE: AccountCategoryMaintenance — GET /api/v1/Maintenance/account-categories (Tab 2 grid)
            //   and PUT /api/v1/Maintenance/account-categories/{accShortName}
            CreateMap<AccountCategoryMaintenanceRes, AccountCategoryMaintenanceDto>().ReverseMap();
            CreateMap<AccountCategoryMaintenanceDto, AccountCategoryMaintenanceReq>().ReverseMap();
        }
    }
}
