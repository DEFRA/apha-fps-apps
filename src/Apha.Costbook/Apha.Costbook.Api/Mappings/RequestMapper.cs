/*
 * TRANSFORMENGINE MIGRATION — RequestMapper.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 5 — API Layer - Controller + RequestMapper + DI (Steps 8-9)
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - Added CapsStaffDto <-> CapsStaffReq / CapsStaffRes mappings (Tab 5 CAPS Staff)
 *   - Added AccountGroupDto <-> AccountGroupReq / AccountGroupRes mappings (Tab 3 CSG7 Inflation Options)
 *   - Added MaintenanceSettingsDto <-> MaintenanceSettingsReq / MaintenanceSettingsRes mappings (Tabs 1 + 4)
 *   - Added AccountCategoryMaintenanceDto <-> AccountCategoryMaintenanceReq / AccountCategoryMaintenanceRes mappings (Tab 2)
 *
 * PRESERVED:
 *   - All existing pagination, Project, lookup, and yearly-details mappings unchanged
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: AccountCategoryMaintenanceReq only carries Csg7Group; the UpdateCsg7GroupAsync service
 *     method accepts csg7Group directly from controller — no DTO mapping needed on the request path,
 *     but the Req->Dto mapping is registered here for completeness and frontend use.
 */

using Apha.Common.Contracts;
using Apha.Common.Contracts.Costbook;
using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Pagination;
using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Pagination;
using Apha.Costbook.DataAccess;
using AutoMapper;

namespace Apha.Costbook.Api.Mappings;

public class RequestMapper : Profile
{
    public RequestMapper()
    {
        // ── Pagination ────────────────────────────────────────────────────────
        CreateMap(typeof(PaginationReq<>),       typeof(QueryParameters<>)).ReverseMap();
        CreateMap(typeof(PaginationRes<>),        typeof(PaginatedResult<>)).ReverseMap();
        CreateMap(typeof(QueryParameters<>),      typeof(PaginationParameters<>)).ReverseMap();
        CreateMap(typeof(PagedData<>),            typeof(PaginatedResult<>)).ReverseMap();
        CreateMap<Pagination,     PaginationDto>().ReverseMap();
        CreateMap<PaginationData, PaginationDto>().ReverseMap();

        // ── Project entity ↔ Dto/Res/Req ─────────────────────────────────────
        CreateMap<Project, ProjectDto>().ReverseMap();
        CreateMap<Project, ProjectHeaderDto>()
            .ForMember(dest => dest.EuroConvRate, opt => opt.MapFrom(src => src.Euroconvrate));
        CreateMap<ProjectDto, ProjectRes>().ReverseMap();
        CreateMap<ProjectDto, ProjectReq>().ReverseMap();

        // ── Lookup entities ───────────────────────────────────────────────────
        CreateMap<Customer,    CustomerDto>().ReverseMap();
        CreateMap<Disease,     DiseaseDto>().ReverseMap();
        CreateMap<Program,     ProgramDto>().ReverseMap();
        CreateMap<Staff,       StaffDto>().ReverseMap();
        CreateMap<CustomerDto, CustomerRes>().ReverseMap();
        CreateMap<DiseaseDto,  DiseaseRes>().ReverseMap();
        CreateMap<ProgramDto,  ProgramRes>().ReverseMap();
        CreateMap<StaffDto,    StaffRes>().ReverseMap();

        // ── Yearly details: entity ↔ Dto ─────────────────────────────────────
        CreateMap<ProjectYear,        ProjectYearDto>().ReverseMap();
        CreateMap<StaffRequirement,   StaffRequirementDto>().ReverseMap();
        CreateMap<TestRequirement,    TestRequirementDto>().ReverseMap();
        CreateMap<AnimalRequirement,  AnimalRequirementDto>().ReverseMap();
        CreateMap<AdditionalCost,     AdditionalCostDto>().ReverseMap();

        // ── Yearly details: Dto ↔ Res/Req ────────────────────────────────────
        CreateMap<ProjectHeaderDto,       ProjectHeaderRes>().ReverseMap();
        CreateMap<ProjectYearDto,         ProjectYearRes>().ReverseMap();
        CreateMap<ProjectYearDto,         ProjectYearReq>().ReverseMap();
        CreateMap<AddProjectYearReq,      ProjectYearDto>()
            .ForMember(dest => dest.YearValue, opt => opt.MapFrom(src => src.Year));
        CreateMap<StaffRequirementDto,    StaffRequirementRes>().ReverseMap();
        CreateMap<StaffRequirementDto,    StaffRequirementReq>().ReverseMap();
        CreateMap<TestRequirementDto,     TestRequirementRes>().ReverseMap();
        CreateMap<TestRequirementDto,     TestRequirementReq>().ReverseMap();
        CreateMap<AnimalRequirementDto,   AnimalRequirementRes>().ReverseMap();
        CreateMap<AnimalRequirementDto,   AnimalRequirementReq>().ReverseMap();
        CreateMap<AdditionalCostDto,      AdditionalCostRes>().ReverseMap();
        CreateMap<AdditionalCostDto,      AdditionalCostReq>().ReverseMap();
        CreateMap<PayRateDto,             PayRateRes>().ReverseMap();
        CreateMap<AnimalRateDto,          AnimalRateRes>().ReverseMap();
        CreateMap<AccountCategoryDto,     AccountCategoryRes>().ReverseMap();
        CreateMap<TestCodeLookupDto,       TestCodeLookupRes>().ReverseMap();
        CreateMap<AnimalLookupDto,         AnimalLookupRes>().ReverseMap();

        CreateMap<StaffYearsRowDto, StaffYearsRowRes>().ReverseMap();
        CreateMap<StaffYearsPivotDto, StaffYearsPivotRes>().ReverseMap();
        CreateMap<StaffEffortRowDto, StaffEffortRowRes>().ReverseMap();
        CreateMap<StaffEffortPivotDto, StaffEffortPivotRes>().ReverseMap();
        CreateMap<ProjectCostsRowDto, ProjectCostsRowRes>().ReverseMap();
        CreateMap<ProjectCostsPivotDto, ProjectCostsPivotRes>().ReverseMap();
        CreateMap<ProjectYearCostSummaryDto, ProjectYearCostSummaryRes>().ReverseMap();

        // ── Maintenance: CapsStaff (Tab 5) ───────────────────────────────────────
        // TRANSFORMENGINE: CapsStaffDto <-> CapsStaffReq/Res — added Phase 5 for CapsStaffController
        CreateMap<CapsStaffDto, CapsStaffRes>().ReverseMap();
        CreateMap<CapsStaffDto, CapsStaffReq>().ReverseMap();

        // ── Maintenance: AccountGroup / CSG7 (Tab 3) ────────────────────────────
        // TRANSFORMENGINE: AccountGroupDto <-> AccountGroupReq/Res — added Phase 5 for AccountGroupController
        CreateMap<AccountGroupDto, AccountGroupRes>().ReverseMap();
        CreateMap<AccountGroupDto, AccountGroupReq>().ReverseMap();

        // ── Maintenance: Settings (Tabs 1 + 4) ──────────────────────────────────
        // TRANSFORMENGINE: MaintenanceSettingsDto <-> MaintenanceSettingsReq/Res — added Phase 5 for MaintenanceController
        CreateMap<MaintenanceSettingsDto, MaintenanceSettingsRes>().ReverseMap();
        CreateMap<MaintenanceSettingsDto, MaintenanceSettingsReq>().ReverseMap();

        // ── Maintenance: AccountCategory (Tab 2) ─────────────────────────────────
        // TRANSFORMENGINE: AccountCategoryMaintenanceDto <-> AccountCategoryMaintenanceReq/Res — added Phase 5 for MaintenanceController
        CreateMap<AccountCategoryMaintenanceDto, AccountCategoryMaintenanceRes>().ReverseMap();
        CreateMap<AccountCategoryMaintenanceDto, AccountCategoryMaintenanceReq>().ReverseMap();
    }
}
