using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class UserService : IUserService
    {
        private readonly IFpsApiClient _fpsApiClient;

        public UserService(IFpsApiClient fpsApiClient)
        {
            _fpsApiClient = fpsApiClient ?? throw new ArgumentNullException(nameof(fpsApiClient));
        }

        public async Task<ApiResponseDto<IEnumerable<UserDto>>> GetAllUsersAsync()
        {
            return await _fpsApiClient.FpsUserPermission.GetAllUsersAsync();
        }

        public async Task<ApiResponseDto<List<UserDto>>> GetAllUsersPagedAsync(QueryParameters<string> query)
        {
            return await _fpsApiClient.FpsUserPermission.GetAllUsersPagedAsync(query);
        }

        public async Task<ApiResponseDto<List<UserDto>>> GetNonSuperUsersPagedAsync(QueryParameters<string> query)
        {
            return await _fpsApiClient.FpsUserPermission.GetNonSuperUsersPagedAsync(query);
        }

        public async Task<ApiResponseDto<UserDto?>> GetUserByIdAsync(int userId)
        {
            return await _fpsApiClient.FpsUserPermission.GetUserByIdAsync(userId);
        }

        public async Task<ApiResponseDto<UserDto>> AddUserAsync(UserDto dto)
        {
            return await _fpsApiClient.FpsUserPermission.AddUserAsync(dto);
        }

        public async Task<ApiResponseDto<UserDto>> UpdateUserAsync(UserDto dto)
        {
            return await _fpsApiClient.FpsUserPermission.UpdateUserAsync(dto);
        }

        public async Task<ApiResponseDto<bool>> DeleteUserAsync(int userId)
        {
            return await _fpsApiClient.FpsUserPermission.DeleteUserAsync(userId);
        }

        public async Task<ApiResponseDto<UserPermissionDataDto>> GetUserPermissionsAsync(int userId)
        {
            return await _fpsApiClient.FpsUserPermission.GetUserPermissionsAsync(userId);
        }

        public async Task<ApiResponseDto<bool>> SaveUserPermissionsAsync(int userId, UserPermissionDataDto dto)
        {
            return await _fpsApiClient.FpsUserPermission.SaveUserPermissionsAsync(userId, dto);
        }

        public async Task<ApiResponseDto<PermissionOptionsDto>> GetPermissionOptionsAsync()
        {
            return await _fpsApiClient.FpsUserPermission.GetPermissionOptionsAsync();
        }
    }
}
