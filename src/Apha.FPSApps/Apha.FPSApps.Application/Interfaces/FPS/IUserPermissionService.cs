using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface IUserPermissionService
    {
        Task<ApiResponseDto<IEnumerable<UserPermissionDto>>> GetAllUsersAsync();
        Task<ApiResponseDto<List<UserPermissionDto>>> GetAllUsersPagedAsync(QueryParameters<string> query);
        Task<ApiResponseDto<List<UserPermissionDto>>> GetNonSuperUsersPagedAsync(QueryParameters<string> query);
        Task<ApiResponseDto<UserPermissionDto?>> GetUserByIdAsync(int userId);
        Task<ApiResponseDto<UserPermissionDto>> AddUserAsync(UserPermissionDto dto);
        Task<ApiResponseDto<UserPermissionDto>> UpdateUserAsync(UserPermissionDto dto);
        Task<ApiResponseDto<bool>> DeleteUserAsync(int userId);
        Task<ApiResponseDto<UserPermissionDataDto>> GetUserPermissionsAsync(int userId);
        Task<ApiResponseDto<bool>> SaveUserPermissionsAsync(int userId, UserPermissionDataDto dto);
        Task<ApiResponseDto<PermissionOptionsDto>> GetPermissionOptionsAsync();
    }
}
