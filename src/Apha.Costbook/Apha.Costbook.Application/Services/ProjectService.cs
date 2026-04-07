using Apha.Costbook.Application.Interfaces;
using Apha.Costbook.Application.Pagination;
using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.Core.Pagination;
using Apha.Costbook.Application.Dtos;
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
          
            var validationMsg = ValidateProject(dto);
            if (!string.IsNullOrEmpty(validationMsg))
                throw new ArgumentException(validationMsg);
                        
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

            var existingProject = await _repo.GetProjectByIdAsync(id);
            if (existingProject == null)
                throw new ArgumentException($"Project with ID {id} not found");

            var validationMsg = ValidateProject(dto);
            if (!string.IsNullOrEmpty(validationMsg))
                throw new ArgumentException(validationMsg);

            CalculateStartFinancialYear(dto);

            // Capture old values BEFORE overwriting — needed for recost comparison below
            var oldInflation = existingProject.Inflation;
            var oldIsdefraproject = existingProject.Isdefraproject;

            // Map dto onto the EXISTING tracked entity (in-place update — avoids EF tracking conflict).
            // dto.ProjectId = decoded ID from the form hidden field, so the PK is NOT changed.
            // Do NOT set existingProject.ProjectId = id here — `id` is URL-encoded ("2025%2F001")
            // but EF tracked the entity with the decoded key ("2025/001"), so overwriting would
            // cause EF to throw: "cannot modify key 'ProjectId' on a tracked entity".
            _mapper.Map(dto, existingProject);

            var result = await _repo.UpdateProjectAsync(existingProject);

            bool shouldRecost =
                oldInflation != existingProject.Inflation ||
                oldIsdefraproject != existingProject.Isdefraproject;

            if (shouldRecost)
            {
                await RecostProjectAsync(id);
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
          
            newProjectDto.DateOfSubmission = DateOnly.FromDateTime(DateTime.Now);

            var entity = _mapper.Map<Project>(newProjectDto);
            var result = await _repo.AddProjectAsync(entity);

            return _mapper.Map<ProjectDto>(result);
        }
        public async Task<bool> RecostProjectAsync(string id)
        {
            // Implement recost logic similar to fnRecostProject
            // This would involve recalculating costs based on current rates
            // For now, return true as placeholder
            await Task.Delay(1);
            return true;
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

            if (string.IsNullOrEmpty(dto.Projecttitle))
                msg += "Please enter a title.\n";

            if (!string.IsNullOrEmpty(dto.Projecttitle) && dto.Projecttitle.Length > 255)
                msg += "Please enter a title of less than 255 characters.\n";

            if (!dto.Isdefraproject.HasValue)
                msg += "Please choose Defra/Non-Defra.\n";

            return msg;
        }

        // Calculate financial year from start date (like SetStartDate function)
        private static void CalculateStartFinancialYear(ProjectDto dto)
        {
            if (!dto.Startdate.HasValue) return;

            var startDate = dto.Startdate.Value;

            // If FinancialYears is "0" (Project Years)
            if (dto.Financialyears == 0)
            {
                dto.Startfyear = startDate.Year;
            }
            // If it's Financial Years (or default)
            else
            {
                if (startDate.Month <= 3) // January to March
                {
                    dto.Startfyear = startDate.Year - 1;
                }
                else // April to December
                {
                    dto.Startfyear = startDate.Year;
                }
            }
        }       
       
    }
}
