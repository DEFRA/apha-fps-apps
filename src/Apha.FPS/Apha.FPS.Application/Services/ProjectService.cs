using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IMapper _mapper;

        public ProjectService(IProjectRepository projectRepository, IMapper mapper)
        {
            _projectRepository = projectRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProjectDto>> GetAllProjectsAsync()
        {
            var projects = await _projectRepository.GetAllProjectsAsync();
            return _mapper.Map<IEnumerable<ProjectDto>>(projects);
        }

        public async Task<IEnumerable<ProjectDto>> GetAllPactProjectsAsync()
        {
            var projects = await _projectRepository.GetAllPactProjectsAsync();
            return _mapper.Map<IEnumerable<ProjectDto>>(projects);
        }

        public async Task<PaginatedResult<ProjectDto>> GetPagedProjectsAsync(QueryParameters<string> query)
        {
            var pagedProjects = await _projectRepository.GetPagedProjectsAsync(
                _mapper.Map<PaginationParameters<string>>(query));
            return _mapper.Map<PaginatedResult<ProjectDto>>(pagedProjects);
        }

        public async Task<PaginatedResult<ProjectDto>> GetPagedPactProjectsAsync(QueryParameters<string> query)
        {
            var pagedProjects = await _projectRepository.GetPagedPactProjectsAsync(
                _mapper.Map<PaginationParameters<string>>(query));
            return _mapper.Map<PaginatedResult<ProjectDto>>(pagedProjects);
        }

        public async Task<ProjectDto?> GetProjectByIdAsync(string parentProject)
        {
            var project = await _projectRepository.GetProjectByIdAsync(parentProject);
            return project == null ? null : _mapper.Map<ProjectDto>(project);
        }

        public async Task<ProjectDto> CreateProjectAsync(ProjectDto projectDto)
        {
            var project = _mapper.Map<Project>(projectDto);
            var created = await _projectRepository.CreateProjectAsync(project);
            return _mapper.Map<ProjectDto>(created);
        }

        public async Task<ProjectDto> UpdateProjectAsync(ProjectDto projectDto)
        {
            var project = _mapper.Map<Project>(projectDto);
            var updated = await _projectRepository.UpdateProjectAsync(project);
            return _mapper.Map<ProjectDto>(updated);
        }

        public async Task<ProjectDto?> UpdatePactProjectDetailsAsync(ProjectDto projectDto)
        {
            var project = _mapper.Map<Project>(projectDto);
            var updated = await _projectRepository.UpdatePactProjectDetailsAsync(project);
            return updated == null ? null : _mapper.Map<ProjectDto>(updated);
        }

        public async Task<bool> DeleteProjectAsync(string parentProject)
        {
            return await _projectRepository.DeleteProjectAsync(parentProject);
        }   

        public async Task<PaginatedResult<ProjectDto>> GetProjectsByProgramAsync(QueryParameters<string> query, string programNo)
        {
            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var projects = await _projectRepository.GetProjectsByProgramAsync(filter, programNo);
            return _mapper.Map<PaginatedResult<ProjectDto>>(projects);
        }
    }
}
