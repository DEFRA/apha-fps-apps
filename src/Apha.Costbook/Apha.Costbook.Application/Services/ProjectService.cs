using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Interfaces;
using Apha.Costbook.Application.Pagination;
using Apha.Costbook.Application.Validation;
using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.Core.Pagination;
using AutoMapper;

namespace Apha.Costbook.Application.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _repo;
        private readonly IMapper _mapper;

        public ProjectService(IProjectRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<ProjectDto>> GetPaginatedProjectsAsync(QueryParameters<string> queryFilter)
        {
            // Map QueryParameters to PaginationParameters
            var paginationParams = _mapper.Map<PaginationParameters<string>>(queryFilter);
            var projects = await _repo.GetPaginatedProjectsAsync(paginationParams);
            return _mapper.Map<PaginatedResult<ProjectDto>>(projects);
        }

        public async Task<IEnumerable<ProjectDto>> GetProjectsAsync(string? contractFilter, string? submittedByFilter)
        {
            var projects = await _repo.GetProjectsAsync(contractFilter, submittedByFilter);
            return _mapper.Map<IEnumerable<ProjectDto>>(projects);
        }

        public async Task<ProjectDto?> GetProjectByIdAsync(string id)
        {
            var project = await _repo.GetProjectByIdAsync(id);
            return project == null ? null : _mapper.Map<ProjectDto>(project);
        }
        public async Task<ProjectDto> AddProjectAsync(ProjectDto dto)
        {
            var errors = new List<BusinessValidationError>();

            if (dto.Startdate == null)
                errors.Add(new BusinessValidationError("Please enter Start Date", "Please enter Start Date"));           

            if (string.IsNullOrEmpty(dto.ProjectTitle))
                errors.Add(new BusinessValidationError("Please enter a title", "Please enter a title"));

            if (!string.IsNullOrEmpty(dto.ProjectTitle) && dto.ProjectTitle.Length > 100)
                errors.Add(new BusinessValidationError("Please enter a title of less than 100 characters", "Please enter a title of less than 100 characters"));

            if (!dto.IsDefraProject.HasValue)
                errors.Add(new BusinessValidationError("Please choose Defra/Non-Defra", "Please choose Defra/Non-Defra"));

            if (!string.IsNullOrEmpty(dto.Notes) && dto.Notes.Length > 255)
                errors.Add(new BusinessValidationError("Please enter notes of less than 255 characters", "Please enter notes of less than 255 characters"));
            
            if (string.IsNullOrEmpty(dto.PreparedBy))
                errors.Add(new BusinessValidationError("Please enter who has prepared this", "Please enter who has prepared this"));

            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            if (string.IsNullOrEmpty(dto.ProjectId))
            {
                dto.ProjectId = await GetNextProjectNumberAsync("");
            }

            if (!dto.Inflation.HasValue)
                dto.Inflation = 1;

            CalculateStartFinancialYear(dto);

            var entity = _mapper.Map<Project>(dto);
            var result = await _repo.AddProjectAsync(entity);
            return _mapper.Map<ProjectDto>(result);
        }

        public async Task<ProjectDto> UpdateProjectAsync(string id, ProjectDto dto)
        {
            var errors = new List<BusinessValidationError>();            
           

            var existingProject = await _repo.GetProjectByIdAsync(id);           

            // Add null check to satisfy nullable reference analysis
            if (existingProject == null)
                throw new InvalidOperationException("Project not found");            

            if (dto.Startdate == null)
                errors.Add(new BusinessValidationError("Please enter Start Date", "Please enter Start Date"));           
            
            if (string.IsNullOrEmpty(dto.ProjectTitle))
                errors.Add(new BusinessValidationError("Please enter a title", "Please enter a title"));    
            if (!string.IsNullOrEmpty(dto.ProjectTitle) && dto.ProjectTitle.Length > 100)
                errors.Add(new BusinessValidationError("Please enter a title of less than 100 characters", "Please enter a title of less than 100 characters"));

            if (!string.IsNullOrEmpty(dto.Notes) && dto.Notes.Length > 255)
                errors.Add(new BusinessValidationError("Please enter notes of less than 255 characters", "Please enter notes of less than 255 characters"));
            if (!dto.IsDefraProject.HasValue)
                errors.Add(new BusinessValidationError("Please choose Defra/Non-Defra", "Please choose Defra/Non-Defra"));

            if (string.IsNullOrEmpty(dto.PreparedBy))
                errors.Add(new BusinessValidationError("Please enter who has prepared this", "Please enter who has prepared this"));
            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            

            // Capture old values BEFORE overwriting — needed for recost comparison below
            decimal? oldInflation = existingProject.Inflation;
            var oldIsdefraproject = existingProject.IsDefraProject;

            
            _mapper.Map(dto, existingProject);

            var result = await _repo.UpdateProjectAsync(existingProject);

            bool shouldRecost =
                oldInflation != existingProject.Inflation ||
                oldIsdefraproject != existingProject.IsDefraProject;

            try
            {
                if (shouldRecost)
                {
                    await RecostProjectAsync(id);
                }
            }
            catch 
            {
               
            }

            return _mapper.Map<ProjectDto>(result);
        }

        public async Task<bool> DeleteProjectAsync(string id)
        {           
            return await _repo.DeleteProjectAsync(id);
        }

        public async Task<ProjectDto> CopyProjectAsync(string oldId, string newId)
        {
            var oldProject = await _repo.GetProjectByIdAsync(oldId);           

            if (oldProject == null)
                throw new ArgumentException($"Project with ID '{oldId}' not found");

            if (string.IsNullOrEmpty(newId))
            {
                newId = await GetNextProjectNumberAsync(oldId);
                if (string.IsNullOrEmpty(newId))
                    throw new InvalidOperationException("Failed to generate a new project ID");
            }

            var newProjectDto = _mapper.Map<ProjectDto>(oldProject);
            newProjectDto.ProjectId = newId;         
            

            var entity = _mapper.Map<Project>(newProjectDto);
            var result = await _repo.CopyProjectAsync(entity, oldId);

            return _mapper.Map<ProjectDto>(result);
        }
        public async Task<bool> RecostProjectAsync(string id)
        {
            return await _repo.RecostProjectAsync(id);

        }

        public async Task<string> GetNextProjectNumberAsync(string? baseNumber) =>
            await _repo.GetNextProjectNumberAsync(baseNumber);

       

        // Validation logic from fnSaveRecOK
        private static string ValidateProject(ProjectDto dto)
        {
            var msg = "";

            if (dto.Startdate == null)
                msg += "Please enter Start Date.\n";

            if (string.IsNullOrEmpty(dto.PreparedBy))
                msg += "Please enter who has prepared this.\n";

            if (string.IsNullOrEmpty(dto.ProjectTitle))
                msg += "Please enter a title.\n";

            if (!string.IsNullOrEmpty(dto.ProjectTitle) && dto.ProjectTitle.Length > 255)
                msg += "Please enter a title of less than 255 characters.\n";

            if (!dto.IsDefraProject.HasValue)
                msg += "Please choose Defra/Non-Defra.\n";

            return msg;
        }

        // Calculate financial year from start date (like SetStartDate function)
        private static void CalculateStartFinancialYear(ProjectDto dto)
        {
            if (!dto.Startdate.HasValue) return;

            var startDate = dto.Startdate.Value;

            // If FinancialYears is "0" (Project Years)
            if (dto.FinancialYears == 0)
            {
                dto.StartFYear  = startDate.Year;
            }
            // If it's Financial Years (or default)
            else
            {
                if (startDate.Month <= 3) // January to March
                {
                    dto.StartFYear = startDate.Year - 1;
                }
                else // April to December
                {
                    dto.StartFYear = startDate.Year;
                }
            }
        }       
       
    }
}
