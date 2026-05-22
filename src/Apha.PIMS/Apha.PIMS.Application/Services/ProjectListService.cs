using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using Apha.PIMS.Application.Validation;
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

        public async Task<ProjectDto?> GetFpsProjectByIdAsync(string parentproject)
        {
            Project? entity = await _repository.GetFpsProjectByIdAsync(parentproject);
            return entity is null ? null : _mapper.Map<ProjectDto>(entity);
        }

        public async Task<ProposedProjectDto?> GetProposedProjectByIdAsync(string parentproject)
        {
            ProposedProject? entity = await _repository.GetProposedProjectByIdAsync(parentproject);
            return entity is null ? null : _mapper.Map<ProposedProjectDto>(entity);
        }

        public async Task<List<ProjectsDto>> GetYearlyDetailsByProjectAsync(string parentproject)
        {
            List<Projects> entities = await _repository.GetYearlyDetailsByProjectAsync(parentproject);
            return _mapper.Map<List<ProjectsDto>>(entities);
        }

        public async Task<ProposedProjectDto> AddProjectAsync(ProposedProjectDto dto)
        {
            var errors = new List<BusinessValidationError>();
            if (string.IsNullOrWhiteSpace(dto.Parentproject))
                errors.Add(new BusinessValidationError("Project is required.", "PROJECT_REQUIRED"));            
            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            // VBA Project_BeforeUpdate: check FPS (MY_tlkpProject) for existing project
            Project? fpsProject = await _repository.GetFpsProjectByIdAsync(dto.Parentproject!);
            if (fpsProject != null)
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError(
            "This project already exists in FPS. Only use this form for projects NOT on FPS.",
            "PROJECT_EXISTS_IN_FPS")
                ]);

            // VBA Project_BeforeUpdate: check tblProposedProject for already-planned project
            ProposedProject? existing = await _repository.GetProposedProjectByIdAsync(dto.Parentproject!);
            if (existing != null)
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError(
                "This project has already been planned. Please select it from the list.",
                "PROJECT_ALREADY_PLANNED")
                ]);

            ProposedProject entity = _mapper.Map<ProposedProject>(dto);
            ProposedProject created = await _repository.AddProjectAsync(entity);
            return _mapper.Map<ProposedProjectDto>(created);
        }

        public async Task<List<string>> GetProjectProgramsAsync()
            => await _repository.GetProjectProgramsAsync();

        public async Task<List<string>> GetProjectCustomersAsync()
            => await _repository.GetProjectCustomersAsync();

        public async Task<List<string>> GetProjectStatusesAsync()
        {
            var statuses = await _repository.GetProjectStatusesAsync();
            return statuses.Select(s => s.Projectstatus).ToList();
        }
    }
}
