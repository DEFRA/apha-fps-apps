using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<PaginatedResult<UserDto>> GetAllUsersPagedAsync(QueryParameters<string> query);
        Task<PaginatedResult<UserDto>> GetNonSuperUsersPagedAsync(QueryParameters<string> query);
        Task<UserDto?> GetUserByIdAsync(int userId);
        Task<UserDto> AddUserAsync(UserDto userDto);
        Task<UserDto> UpdateUserAsync(UserDto userDto);
        Task<bool> DeleteUserAsync(int userId);
        Task<UserPermissionDto> GetUserPermissionsAsync(int userId);
        Task SaveUserPermissionsAsync(UserPermissionDto permissionDto);
        Task<PermissionOptionsDto> GetPermissionOptionsAsync();
    }
}
