using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;
using AutoMapper;

namespace Apha.FPS.Api.Mappings
{
    public class RequestMapper : Profile
    {
        public RequestMapper()
        {
            CreateMap(typeof(PaginationReq<>), typeof(QueryParameters<>)).ReverseMap();
            CreateMap(typeof(PaginationRes<>), typeof(PaginatedResult<>)).ReverseMap();

            CreateMap<Pagination, PaginationDto>().ReverseMap();

            CreateMap<StaffJobViewDto, StaffJobViewRes>().ReverseMap();
            CreateMap<StaffWorkgroupLookupDto, StaffWorkgroupLookupRes>().ReverseMap();
            CreateMap<StaffJobDto, StaffJobReq>().ReverseMap();
            CreateMap<StaffJobDto, StaffJobRes>().ReverseMap();

            CreateMap<FpsSettingRes, FpsSettingDto>().ReverseMap();

            CreateMap<AnimalCostViewDto, AnimalCostViewRes>().ReverseMap();
            CreateMap<AnimalDto, AnimalRes>().ReverseMap();
            CreateMap<AnimalRequestDto, AnimalRequestReq>().ReverseMap();
            CreateMap<AnimalRequestDto, AnimalRequestRes>().ReverseMap();
            CreateMap<EmployeeDto, EmployeeReq>().ReverseMap();
            CreateMap<EmployeeDto, EmployeeRes>().ReverseMap();
            CreateMap<ManagerDto, ManagerRes>().ReverseMap();
            CreateMap<ProgramReq, ProgramDto>().ReverseMap();
            CreateMap<ProgramRes, ProgramDto>().ReverseMap();

            CreateMap<ProjectDto, ProjectReq>()
                .ForMember(d => d.BudgetExt, o => o.MapFrom(s => s.CustIncome)).ReverseMap()
                .ForMember(d => d.CustIncome, o => o.MapFrom(s => s.BudgetExt));
            CreateMap<ProjectDto, ProjectRes>()
                .ForMember(d => d.BudgetExt, o => o.MapFrom(s => s.CustIncome)).ReverseMap()
                .ForMember(d => d.CustIncome, o => o.MapFrom(s => s.BudgetExt));

            CreateMap<ContractDto, ContractRes>()
                .ForMember(d => d.ContractNo, o => o.MapFrom(s => s.Contractno))
                .ForMember(d => d.Category, o => o.MapFrom(s => s.Category));
            CreateMap<YearMasterRes, YearMasterDto>().ReverseMap();
            CreateMap<DivisionReq, DivisionDto>().ReverseMap();
            CreateMap<DivisionRes, DivisionDto>().ReverseMap();
            CreateMap<AgencyRes, AgencyDto>().ReverseMap();
        }
    }
}
