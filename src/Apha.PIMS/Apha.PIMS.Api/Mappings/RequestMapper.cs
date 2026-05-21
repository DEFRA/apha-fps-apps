using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Pagination;
using AutoMapper;

namespace Apha.PIMS.Api.Mappings
{
    public class RequestMapper : Profile
    {
        public RequestMapper()
        {
            CreateMap(typeof(PaginationReq<>), typeof(QueryParameters<>)).ReverseMap();
            CreateMap(typeof(PaginationRes<>), typeof(PaginatedResult<>)).ReverseMap();
            CreateMap<Pagination, PaginationDto>().ReverseMap();

            CreateMap<ProjectListViewDto, ProjectListRes>().ReverseMap();
            CreateMap<ProjectDto, ProjectRes>().ReverseMap();
            CreateMap<ProposedProjectDto, ProposedProjectReq>().ReverseMap();
            CreateMap<ProposedProjectDto, ProposedProjectRes>().ReverseMap();
            CreateMap<ProjectsDto, ProjectsRes>().ReverseMap();
            CreateMap<CommentDto, CommentReq>()
                .ForMember(dest => dest.Comment, opt => opt.MapFrom(src => src.CommentText))
                .ReverseMap()
                .ForMember(dest => dest.CommentText, opt => opt.MapFrom(src => src.Comment));

            CreateMap<CommentDto, CommentRes>()
                .ForMember(dest => dest.Comment, opt => opt.MapFrom(src => src.CommentText))
                .ReverseMap()
                .ForMember(dest => dest.CommentText, opt => opt.MapFrom(src => src.Comment));
            CreateMap<ProjectDetailDto, ProjectDetailReq>().ReverseMap();
            CreateMap<ProjectDetailDto, ProjectDetailRes>().ReverseMap();
            CreateMap<RiskDto, RiskRes>().ReverseMap();
            CreateMap<YearDto, YearRes>().ReverseMap();
            CreateMap<CommentTopicDto, CommentTopicRes>().ReverseMap();
        }
    }
}
