using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    public class UserPermissionService : IUserPermissionService
    {
        private readonly IUserRepository _repository;
        private readonly IMapper _mapper;

        public UserPermissionService(IUserRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _repository.GetAllUsersAsync();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<PaginatedResult<UserDto>> GetAllUsersPagedAsync(QueryParameters<string> query)
        {
            ArgumentNullException.ThrowIfNull(query);
            var queryParams = _mapper.Map<PaginationParameters<string>>(query);
            var pagedResult = await _repository.GetAllUsersPagedAsync(queryParams);
            return _mapper.Map<PaginatedResult<UserDto>>(pagedResult);
        }

        public async Task<PaginatedResult<UserDto>> GetNonSuperUsersPagedAsync(QueryParameters<string> query)
        {
            ArgumentNullException.ThrowIfNull(query);
            var queryParams = _mapper.Map<PaginationParameters<string>>(query);
            var pagedResult = await _repository.GetNonSuperUsersPagedAsync(queryParams);
            return _mapper.Map<PaginatedResult<UserDto>>(pagedResult);
        }

        public async Task<UserDto?> GetUserByIdAsync(int userId)
        {
            var user = await _repository.GetUserByIdAsync(userId);
            return user == null ? null : _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> AddUserAsync(UserDto userDto)
        {
            ArgumentNullException.ThrowIfNull(userDto);

            userDto.Username = userDto.Username?.Trim();
            userDto.Comments = userDto.Comments?.Trim();
            userDto.UserEmail = userDto.UserEmail?.Trim();
            userDto.Dt2Username = userDto.Dt2Username?.Trim();

            if (string.IsNullOrWhiteSpace(userDto.Username))
                throw new ArgumentException("Username is required.");

            var existing = await _repository.GetUserByUsernameAsync(userDto.Username);
            if (existing != null)
                throw new InvalidOperationException($"User with username '{userDto.Username}' already exists.");

            if (!string.IsNullOrWhiteSpace(userDto.UserEmail))
            {
                var existingByEmail = await _repository.GetUserByEmailAsync(userDto.UserEmail);
                if (existingByEmail != null)
                    throw new InvalidOperationException($"User with email '{userDto.UserEmail}' already exists.");
            }

            var entity = _mapper.Map<User>(userDto);
            var added = await _repository.AddUserAsync(entity);
            return _mapper.Map<UserDto>(added);
        }

        public async Task<UserDto> UpdateUserAsync(UserDto userDto)
        {
            ArgumentNullException.ThrowIfNull(userDto);

            userDto.Username = userDto.Username?.Trim();
            userDto.Comments = userDto.Comments?.Trim();
            userDto.UserEmail = userDto.UserEmail?.Trim();
            userDto.Dt2Username = userDto.Dt2Username?.Trim();

            if (string.IsNullOrWhiteSpace(userDto.Username))
                throw new ArgumentException("Username is required.");

            var existingById = await _repository.GetUserByIdAsync(userDto.UserId)
                ?? throw new KeyNotFoundException($"User with ID {userDto.UserId} not found.");

            var existingByName = await _repository.GetUserByUsernameAsync(userDto.Username);
            if (existingByName != null && existingByName.UserId != userDto.UserId)
                throw new InvalidOperationException($"User with username '{userDto.Username}' already exists.");

            if (!string.IsNullOrWhiteSpace(userDto.UserEmail))
            {
                var existingByEmail = await _repository.GetUserByEmailAsync(userDto.UserEmail);
                if (existingByEmail != null && existingByEmail.UserId != userDto.UserId)
                    throw new InvalidOperationException($"User with email '{userDto.UserEmail}' already exists.");
            }

            var entity = _mapper.Map<User>(userDto);
            var updated = await _repository.UpdateUserAsync(entity);
            return _mapper.Map<UserDto>(updated);
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            return await _repository.DeleteUserAsync(userId);
        }

        public async Task<UserPermissionDto> GetUserPermissionsAsync(int userId)
        {
            return new UserPermissionDto
            {
                UserId = userId,
                ProfitCentres = await _repository.GetUserProfitCentresAsync(userId),
                Programs = await _repository.GetUserProgramsAsync(userId),
                Categories = await _repository.GetUserCategoriesAsync(userId),
                TestOwners = await _repository.GetUserTestOwnersAsync(userId),
                ProjectGroups = await _repository.GetUserProjectGroupsAsync(userId)
            };
        }

        public async Task SaveUserPermissionsAsync(UserPermissionDto permissionDto)
        {
            ArgumentNullException.ThrowIfNull(permissionDto);
            await _repository.SaveUserPermissionsAsync(
                permissionDto.UserId,
                permissionDto.ProfitCentres,
                permissionDto.Programs,
                permissionDto.Categories,
                permissionDto.TestOwners,
                permissionDto.ProjectGroups);
        }

        public async Task<PermissionOptionsDto> GetPermissionOptionsAsync()
        {
            return new PermissionOptionsDto
            {
                ProfitCentres = await _repository.GetAllProfitCentreOptionsAsync(),
                Programs = await _repository.GetAllProgramOptionsAsync(),
                Categories = await _repository.GetAllCategoryOptionsAsync(),
                TestOwners = await _repository.GetAllTestOwnerOptionsAsync(),
                ProjectGroups = await _repository.GetAllProjectGroupOptionsAsync()
            };
        }
    }
}
