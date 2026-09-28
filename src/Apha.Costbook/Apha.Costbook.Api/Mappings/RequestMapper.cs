using Apha.Common.Contracts;
using Apha.Common.Contracts.Costbook;
using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Pagination;
using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Pagination;
using Apha.Costbook.DataAccess;
using Mapster;

namespace Apha.Costbook.Api.Mappings;

public class RequestMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // ── Pagination ────────────────────────────────────────────────────────
        config.NewConfig(typeof(PaginationReq<>),       typeof(QueryParameters<>));
        config.NewConfig(typeof(QueryParameters<>),      typeof(PaginationReq<>));
        config.NewConfig(typeof(PaginationRes<>),        typeof(PaginatedResult<>));
        config.NewConfig(typeof(PaginatedResult<>),      typeof(PaginationRes<>));
        config.NewConfig(typeof(QueryParameters<>),      typeof(PaginationParameters<>));
        config.NewConfig(typeof(PaginationParameters<>), typeof(QueryParameters<>));
        config.NewConfig(typeof(PagedData<>),            typeof(PaginatedResult<>));
        config.NewConfig(typeof(PaginatedResult<>),      typeof(PagedData<>));
        config.NewConfig<Pagination,     PaginationDto>().TwoWays();
        config.NewConfig<PaginationData, PaginationDto>().TwoWays();

        // ── Project entity ↔ Dto/Res/Req ─────────────────────────────────────
        config.NewConfig<Project, ProjectDto>().TwoWays();
        config.NewConfig<Project, ProjectHeaderDto>()
            .Map(dest => dest.EuroConvRate, src => src.Euroconvrate);
        config.NewConfig<ProjectDto, ProjectRes>().TwoWays();
        config.NewConfig<ProjectDto, ProjectReq>().TwoWays();

        // ── Lookup entities ───────────────────────────────────────────────────
        config.NewConfig<Customer,    CustomerDto>().TwoWays();
        config.NewConfig<Disease,     DiseaseDto>().TwoWays();
        config.NewConfig<Program,     ProgramDto>().TwoWays();
        config.NewConfig<Staff,       StaffDto>().TwoWays();
        config.NewConfig<CustomerDto, CustomerRes>().TwoWays();
        config.NewConfig<DiseaseDto,  DiseaseRes>().TwoWays();
        config.NewConfig<ProgramDto,  ProgramRes>().TwoWays();
        config.NewConfig<StaffDto,    StaffRes>().TwoWays();

        // ── Yearly details: entity ↔ Dto ─────────────────────────────────────
        config.NewConfig<ProjectYear,        ProjectYearDto>().TwoWays();
        config.NewConfig<StaffRequirement,   StaffRequirementDto>().TwoWays();
        config.NewConfig<TestRequirement,    TestRequirementDto>().TwoWays();
        config.NewConfig<AnimalRequirement,  AnimalRequirementDto>().TwoWays();
        config.NewConfig<AdditionalCost,     AdditionalCostDto>().TwoWays();

        // ── Yearly details: Dto ↔ Res/Req ────────────────────────────────────
        config.NewConfig<ProjectHeaderDto,       ProjectHeaderRes>().TwoWays();
        config.NewConfig<ProjectYearDto,         ProjectYearRes>().TwoWays();
        config.NewConfig<ProjectYearDto,         ProjectYearReq>().TwoWays();
        config.NewConfig<AddProjectYearReq,      ProjectYearDto>()
            .Map(dest => dest.YearValue, src => src.Year);
        config.NewConfig<StaffRequirementDto,    StaffRequirementRes>().TwoWays();
        config.NewConfig<StaffRequirementDto,    StaffRequirementReq>().TwoWays();
        config.NewConfig<TestRequirementDto,     TestRequirementRes>().TwoWays();
        config.NewConfig<TestRequirementDto,     TestRequirementReq>().TwoWays();
        config.NewConfig<AnimalRequirementDto,   AnimalRequirementRes>().TwoWays();
        config.NewConfig<AnimalRequirementDto,   AnimalRequirementReq>().TwoWays();
        config.NewConfig<AdditionalCostDto,      AdditionalCostRes>().TwoWays();
        config.NewConfig<AdditionalCostDto,      AdditionalCostReq>().TwoWays();
        config.NewConfig<PayRateDto,             PayRateRes>().TwoWays();
        config.NewConfig<AnimalRateDto,          AnimalRateRes>().TwoWays();
        config.NewConfig<AccountCategoryDto,     AccountCategoryRes>().TwoWays();
        config.NewConfig<TestCodeLookupDto,       TestCodeLookupRes>().TwoWays();
        config.NewConfig<AnimalLookupDto,         AnimalLookupRes>().TwoWays();

        config.NewConfig<StaffYearsRowDto, StaffYearsRowRes>().TwoWays();
        config.NewConfig<StaffYearsPivotDto, StaffYearsPivotRes>().TwoWays();
        config.NewConfig<StaffEffortRowDto, StaffEffortRowRes>().TwoWays();
        config.NewConfig<StaffEffortPivotDto, StaffEffortPivotRes>().TwoWays();
        config.NewConfig<ProjectCostsRowDto, ProjectCostsRowRes>().TwoWays();
        config.NewConfig<ProjectCostsPivotDto, ProjectCostsPivotRes>().TwoWays();
        config.NewConfig<ProjectYearCostSummaryDto, ProjectYearCostSummaryRes>().TwoWays();

        // ── Maintenance: CapsStaff (Tab 5) ───────────────────────────────────────
        config.NewConfig<StaffDto, StaffRes>().TwoWays();
        config.NewConfig<StaffDto, StaffReq>().TwoWays();

        // ── Maintenance: AccountGroup / CSG7 (Tab 3) ────────────────────────────
        config.NewConfig<AccountGroupDto, AccountGroupRes>().TwoWays();
        config.NewConfig<AccountGroupDto, AccountGroupReq>().TwoWays();

        // ── Maintenance: Settings (Tabs 1 + 4) ──────────────────────────────────
        config.NewConfig<MaintenanceSettingsDto, MaintenanceSettingsRes>().TwoWays();
        config.NewConfig<MaintenanceSettingsDto, MaintenanceSettingsReq>().TwoWays();

        // ── Maintenance: AccountCategory (Tab 2) ─────────────────────────────────
        config.NewConfig<AccountCategoryMaintenanceDto, AccountCategoryMaintenanceRes>().TwoWays();
        config.NewConfig<AccountCategoryMaintenanceDto, AccountCategoryMaintenanceReq>().TwoWays();

    }
}
