using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Core.Interfaces;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    public class ProjectGroupService : IProjectGroupService
    {
        private readonly IProjectGroupRepository _projectGroupRepository;
        private readonly IMapper _mapper;

        public ProjectGroupService(IProjectGroupRepository projectGroupRepository, IMapper mapper)
        {
            _projectGroupRepository = projectGroupRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProjectGroupDto>> GetAllProjectGroupsAsync()
        {
            var projectGroups = await _projectGroupRepository.GetAllProjectGroupsAsync();
            return _mapper.Map<IEnumerable<ProjectGroupDto>>(projectGroups);
        }

        public async Task<IEnumerable<ProjectGroupDto>> GetAllProjectGroupsByUserAsync()
        {
            var projectGroups = await _projectGroupRepository.GetAllProjectGroupsByUserAsync();
            return _mapper.Map<IEnumerable<ProjectGroupDto>>(projectGroups);
        }
    }
}
