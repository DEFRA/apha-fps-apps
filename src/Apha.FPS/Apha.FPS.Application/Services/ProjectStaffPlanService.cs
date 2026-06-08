using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    public class ProjectStaffPlanService : IProjectStaffPlanService
    {
        private readonly IProjectStaffPlanRepository _repository;
        private readonly IMapper _mapper;

        public ProjectStaffPlanService(IProjectStaffPlanRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<ProjectStaffPlanViewDto>> GetPagedAsync(QueryParameters<string> query)
        {
            PaginationParameters<string> parameters = _mapper.Map<PaginationParameters<string>>(query);
            var result = await _repository.GetPagedAsync(parameters);
            return _mapper.Map<PaginatedResult<ProjectStaffPlanViewDto>>(result);
        }
    }
}
