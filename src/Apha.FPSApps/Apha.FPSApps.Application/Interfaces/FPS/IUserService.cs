using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface IUserService
    {
        Task<ApiResponseDto<IEnumerable<UserDto>>> GetAllUsersAsync();
        Task<ApiResponseDto<List<UserDto>>> GetAllUsersPagedAsync(QueryParameters<string> query);
        Task<ApiResponseDto<List<UserDto>>> GetNonSuperUsersPagedAsync(QueryParameters<string> query);
        Task<ApiResponseDto<UserDto?>> GetUserByIdAsync(int userId);
        Task<ApiResponseDto<UserDto>> AddUserAsync(UserDto dto);
        Task<ApiResponseDto<UserDto>> UpdateUserAsync(UserDto dto);
        Task<ApiResponseDto<bool>> DeleteUserAsync(int userId);
        Task<ApiResponseDto<UserPermissionDataDto>> GetUserPermissionsAsync(int userId);
        Task<ApiResponseDto<bool>> SaveUserPermissionsAsync(int userId, UserPermissionDataDto dto);
        Task<ApiResponseDto<PermissionOptionsDto>> GetPermissionOptionsAsync();
    }
}
