using Apha.Common.Contracts;
using Apha.Common.Contracts.Costbook;
using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Pagination;
using Apha.Costbook.Core.Entities;
using Apha.Costbook.DataAccess;
using AutoMapper;

namespace Apha.Costbook.Api.Mappings
{
    public class RequestMapper : Profile
    {
        public RequestMapper()
        {
            CreateMap(typeof(PaginationReq<>), typeof(QueryParameters<>)).ReverseMap();
            CreateMap(typeof(PaginationRes<>), typeof(PaginatedResult<>)).ReverseMap();

            CreateMap<Pagination, PaginationDto>().ReverseMap();
            CreateMap<ProjectDto, ProjectRes>().ReverseMap();
            CreateMap<ProjectDto, ProjectReq>().ReverseMap();
            CreateMap<Project, ProjectDto>().ReverseMap();
            CreateMap<Customer, CustomerDto>().ReverseMap();
            CreateMap<Disease, DiseaseDto>().ReverseMap();
            CreateMap<Program, ProgramDto>().ReverseMap();
            CreateMap<Staff, StaffDto>().ReverseMap();
            CreateMap<CustomerDto, CustomerRes>().ReverseMap();
            CreateMap<DiseaseDto, DiseaseRes>().ReverseMap();
            CreateMap<ProgramDto, ProgramRes>().ReverseMap();
            CreateMap<StaffDto, StaffRes>().ReverseMap();
        }
    }
}
