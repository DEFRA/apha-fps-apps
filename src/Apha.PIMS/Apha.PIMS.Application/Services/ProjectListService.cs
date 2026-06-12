using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using AutoMapper;

namespace Apha.PIMS.Application.Services
{
    public class ProjectListService : IProjectListService
    {
        private readonly IProjectListRepository _repository;
        private readonly IMapper _mapper;

        public ProjectListService(IProjectListRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<ProjectListViewDto>> GetAllProjectsAsync(QueryParameters<string> query, int showWhichProjects = 2)
        {
            PaginationParameters<string> filter = _mapper.Map<PaginationParameters<string>>(query);
            PagedData<ProjectListView> result = await _repository.GetAllProjectsAsync(filter, showWhichProjects);
            return _mapper.Map<PaginatedResult<ProjectListViewDto>>(result);
        }

        public async Task<List<ProjectListViewDto>> GetAllProjectsForDropDownAsync()
        {
            List<ProjectListView> entities = await _repository.GetAllProjectsForDropDownAsync();
            return _mapper.Map<List<ProjectListViewDto>>(entities);
        }

        public async Task<List<ProjectsDto>> GetYearlyDetailsByProjectAsync(string parentproject)
        {
            List<Projects> entities = await _repository.GetYearlyDetailsByProjectAsync(parentproject);
            return _mapper.Map<List<ProjectsDto>>(entities);
        }
        public async Task<List<ProjectListMilestoneDto>> GetAllProjectsForMilestoneAsync()
        {
            List<ProjectListMilestone> entities = await _repository.GetAllProjectsForMilestone();
            return _mapper.Map<List<ProjectListMilestoneDto>>(entities);
        }
    }
}
