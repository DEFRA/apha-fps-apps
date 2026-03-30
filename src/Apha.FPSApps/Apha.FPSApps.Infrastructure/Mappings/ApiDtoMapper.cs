using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.Common.Contracts.PACT;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Pagination;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Mappings
{
    public class ApiDtoMapper : Profile
    {
        public ApiDtoMapper()
        {
            CreateMap(typeof(ApiResponseDto<>), typeof(ApiResponse<>)).ReverseMap();
            CreateMap<ApiErrorDto, ApiError>().ReverseMap();
            CreateMap<ApiMetaDto, ApiMeta>().ReverseMap();
            CreateMap(typeof(PaginationRes<>), typeof(PaginatedResult<>)).ReverseMap();
            CreateMap<PaginationDto, Pagination>().ReverseMap();

            CreateMap<StaffJobViewDto, StaffJobViewRes>().ReverseMap();
            CreateMap<StaffWorkgroupLookupDto, StaffWorkgroupLookupRes>().ReverseMap();
            CreateMap<StaffJobDto, StaffJobReq>().ReverseMap();
            CreateMap<StaffJobDto, StaffJobRes>().ReverseMap();
            CreateMap<ProgramDto,ProgramReq>().ReverseMap();
            CreateMap<ProgramDto, ProgramRes>().ReverseMap();
            CreateMap<ManagerDto, ManagerRes>().ReverseMap();
            CreateMap<EmployeeDto, EmployeeReq>().ReverseMap();
            CreateMap<EmployeeDto, EmployeeRes>().ReverseMap();

            // FPS Project
            CreateMap<ProjectDto, ProjectReq>().ReverseMap();
            CreateMap<ProjectDto, ProjectRes>().ReverseMap();

            // FPS Lookups
            CreateMap<StatusDto, StatusRes>().ReverseMap();
            CreateMap<DiseaseDto, DiseaseRes>().ReverseMap();
            CreateMap<CustomerDto, CustomerRes>().ReverseMap();
            CreateMap<ContractDto, ContractRes>().ReverseMap();

            // PACT
            CreateMap<JobCodeDto, JobCodeReq>().ReverseMap();
            CreateMap<JobCodeDto, JobCodeRes>().ReverseMap();
            CreateMap<TimeCodeValidDto, TimeCodeValidReq>().ReverseMap();
            CreateMap<TimeCodeValidDto, TimeCodeValidRes>().ReverseMap();
            CreateMap<WorkGroupDto, WorkGroupRes>().ReverseMap();
        }
    }
}
