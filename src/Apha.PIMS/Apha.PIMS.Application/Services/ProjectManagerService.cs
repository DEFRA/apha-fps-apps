using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using Apha.PIMS.Application.Validation;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using MapsterMapper;

namespace Apha.PIMS.Application.Services
{
    public class ProjectManagerService : IProjectManagerService
    {
        private readonly IProjectManagerRepository _repository;
        private readonly IMapper _mapper;

        public ProjectManagerService(IProjectManagerRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<List<ProjectManagerDto>> GetAllProjectManagersAsync()
        {
            List<ProjectManager> entities = await _repository.GetAllProjectManagersAsync();
            return _mapper.Map<List<ProjectManagerDto>>(entities);
        }

        public async Task<PaginatedResult<ProjectManagerDto>> GetPagedProjectManagersAsync(QueryParameters<string>? query = null)
        {
            query ??= new QueryParameters<string>();

            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetPagedProjectManagersAsync(parameters);
            return _mapper.Map<PaginatedResult<ProjectManagerDto>>(pagedData);
        }

        public async Task<List<string>> GetManagerNamesAsync()
        {
            return await _repository.GetManagerNamesAsync();
        }

        public async Task<ProjectManagerDto?> GetProjectManagerByNameAsync(string projectManagerName)
        {
            if (string.IsNullOrWhiteSpace(projectManagerName))
                return null;

            string normalizedName = projectManagerName.Trim();
            var allManagers = await _repository.GetAllProjectManagersAsync();
            var manager = allManagers.FirstOrDefault(m => StringEqualsTrimmedIgnoreCase(m.Projectmanager, normalizedName));
            return manager is null ? null : _mapper.Map<ProjectManagerDto>(manager);
        }

        public async Task<ProjectManagerDto> CreateProjectManagerAsync(ProjectManagerDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            string? normalizedProjectManager = dto.ProjectManager?.Trim();
            dto.LoginEmail = string.IsNullOrWhiteSpace(dto.LoginEmail) ? null : dto.LoginEmail.Trim();
            dto.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();
            dto.MNumber = string.IsNullOrWhiteSpace(dto.MNumber) ? null : dto.MNumber.Trim();

            if (string.IsNullOrWhiteSpace(normalizedProjectManager))
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError("Project manager name is required.", "PROJECT_MANAGER_NAME_REQUIRED")
                ]);

            dto.ProjectManager = normalizedProjectManager;

            var existingManagers = await _repository.GetAllProjectManagersAsync();

            // Check for duplicate ProjectManager name
            bool duplicateProjectManagerExists = existingManagers.Any(m =>
                StringEqualsTrimmedIgnoreCase(m.Projectmanager, dto.ProjectManager));

            if (duplicateProjectManagerExists)
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError($"Project manager '{dto.ProjectManager}' already exists. Please enter a unique manager name.", "PROJECT_MANAGER_DUPLICATE_NAME")
                ]);

            // Check for duplicate LoginEmail
            if (!string.IsNullOrWhiteSpace(dto.LoginEmail))
            {
                bool duplicateLoginEmailExists = existingManagers.Any(m =>
                    !string.IsNullOrWhiteSpace(m.LoginEmail)
                    && StringEqualsTrimmedIgnoreCase(m.LoginEmail, dto.LoginEmail));

                if (duplicateLoginEmailExists)
                    throw new BusinessValidationErrorException(
                    [
                        new BusinessValidationError("Manager's login email already exists. Please enter a unique login email.", "PROJECT_MANAGER_DUPLICATE_LOGIN_EMAIL")
                    ]);
            }

            // Check for duplicate Email
            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                bool duplicateEmailExists = existingManagers.Any(m =>
                    !string.IsNullOrWhiteSpace(m.Email)
                    && StringEqualsTrimmedIgnoreCase(m.Email, dto.Email));

                if (duplicateEmailExists)
                    throw new BusinessValidationErrorException(
                    [
                        new BusinessValidationError("Manager's email already exists. Please enter a unique email.", "PROJECT_MANAGER_DUPLICATE_EMAIL")
                    ]);
            }

            // Check for duplicate MNumber
            if (!string.IsNullOrWhiteSpace(dto.MNumber))
            {
                bool duplicateMNumberExists = existingManagers.Any(m =>
                    !string.IsNullOrWhiteSpace(m.Mnumber)
                    && StringEqualsTrimmedIgnoreCase(m.Mnumber, dto.MNumber));

                if (duplicateMNumberExists)
                    throw new BusinessValidationErrorException(
                    [
                        new BusinessValidationError("MNumber already exists. Please enter a unique MNumber.", "PROJECT_MANAGER_DUPLICATE_MNUMBER")
                    ]);
            }

            ProjectManager entity = _mapper.Map<ProjectManager>(dto);
            ProjectManager created = await _repository.AddProjectManagerAsync(entity);
            return _mapper.Map<ProjectManagerDto>(created);
        }

        public async Task<ProjectManagerDto> UpdateProjectManagerAsync(ProjectManagerDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            string? normalizedProjectManager = dto.ProjectManager?.Trim();
            dto.LoginEmail = string.IsNullOrWhiteSpace(dto.LoginEmail) ? null : dto.LoginEmail.Trim();
            dto.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();
            dto.MNumber = string.IsNullOrWhiteSpace(dto.MNumber) ? null : dto.MNumber.Trim();

            if (string.IsNullOrWhiteSpace(normalizedProjectManager))
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError("Project manager name is required.", "PROJECT_MANAGER_NAME_REQUIRED")
                ]);

            dto.ProjectManager = normalizedProjectManager;

            var existingManagers = await _repository.GetAllProjectManagersAsync();

            // Check if project manager exists
            if (!existingManagers.Any(m => StringEqualsTrimmedIgnoreCase(m.Projectmanager, dto.ProjectManager)))
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError($"Project manager '{dto.ProjectManager}' was not found.", "PROJECT_MANAGER_NOT_FOUND")
                ]);

            // Check for duplicate LoginEmail (excluding current manager)
            if (!string.IsNullOrWhiteSpace(dto.LoginEmail))
            {
                bool duplicateLoginEmailExists = existingManagers.Any(m =>
                    !string.IsNullOrWhiteSpace(m.LoginEmail)
                    && StringEqualsTrimmedIgnoreCase(m.LoginEmail, dto.LoginEmail)
                    && !StringEqualsTrimmedIgnoreCase(m.Projectmanager, dto.ProjectManager));

                if (duplicateLoginEmailExists)
                    throw new BusinessValidationErrorException(
                    [
                        new BusinessValidationError("Manager's login email already exists. Please enter a unique login email.", "PROJECT_MANAGER_DUPLICATE_LOGIN_EMAIL")
                    ]);
            }

            // Check for duplicate Email (excluding current manager)
            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                bool duplicateEmailExists = existingManagers.Any(m =>
                    !string.IsNullOrWhiteSpace(m.Email)
                    && StringEqualsTrimmedIgnoreCase(m.Email, dto.Email)
                    && !StringEqualsTrimmedIgnoreCase(m.Projectmanager, dto.ProjectManager));

                if (duplicateEmailExists)
                    throw new BusinessValidationErrorException(
                    [
                        new BusinessValidationError("Manager's email already exists. Please enter a unique email.", "PROJECT_MANAGER_DUPLICATE_EMAIL")
                    ]);
            }

            // Check for duplicate MNumber (excluding current manager)
            if (!string.IsNullOrWhiteSpace(dto.MNumber))
            {
                bool duplicateMNumberExists = existingManagers.Any(m =>
                    !string.IsNullOrWhiteSpace(m.Mnumber)
                    && StringEqualsTrimmedIgnoreCase(m.Mnumber, dto.MNumber)
                    && !StringEqualsTrimmedIgnoreCase(m.Projectmanager, dto.ProjectManager));

                if (duplicateMNumberExists)
                    throw new BusinessValidationErrorException(
                    [
                        new BusinessValidationError("MNumber already exists. Please enter a unique MNumber.", "PROJECT_MANAGER_DUPLICATE_MNUMBER")
                    ]);
            }

            ProjectManager entity = _mapper.Map<ProjectManager>(dto);
            ProjectManager updated = await _repository.UpdateProjectManagerAsync(entity);
            return _mapper.Map<ProjectManagerDto>(updated);
        }

        public async Task<bool> DeleteProjectManagerAsync(string projectManagerName)
        {
            if (string.IsNullOrWhiteSpace(projectManagerName))
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError("Project manager name is required.", "PROJECT_MANAGER_NAME_REQUIRED")
                ]);

            string normalizedName = projectManagerName.Trim();
            var existingManagers = await _repository.GetAllProjectManagersAsync();

            if (!existingManagers.Any(m => StringEqualsTrimmedIgnoreCase(m.Projectmanager, normalizedName)))
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError($"Project manager '{normalizedName}' was not found.", "PROJECT_MANAGER_NOT_FOUND")
                ]);

            var deleted = await _repository.DeleteProjectManagerAsync(normalizedName);
            if (!deleted)
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError($"Project manager '{normalizedName}' was not found.", "PROJECT_MANAGER_NOT_FOUND")
                ]);

            return true;
        }

        public async Task<bool> ProjectManagerExistsAsync(string projectManagerName)
        {
            if (string.IsNullOrWhiteSpace(projectManagerName))
                return false;

            string normalizedName = projectManagerName.Trim();
            var allManagers = await _repository.GetAllProjectManagersAsync();
            return allManagers.Any(m => StringEqualsTrimmedIgnoreCase(m.Projectmanager, normalizedName));
        }

        /// <summary>
        /// Case-insensitive comparison of trimmed strings.
        /// </summary>
        private static bool StringEqualsTrimmedIgnoreCase(string? left, string? right) =>
            left is not null && right is not null && string.Equals(left.Trim(), right.Trim(), StringComparison.OrdinalIgnoreCase);
    }
}
