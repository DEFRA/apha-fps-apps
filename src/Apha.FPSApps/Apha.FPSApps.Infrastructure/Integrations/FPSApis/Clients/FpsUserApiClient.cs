using Apha.Common.Constants;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsUserApiClient : IFpsUserApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        public FpsUserApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ApiResponseDto<IEnumerable<UserDto>>> GetAllUsersAsync()
        {
            var response = await _http.GetAsync<IEnumerable<UserDto>>(FpsApiEndpoints.GetAllUsers);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<IEnumerable<UserDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<IEnumerable<UserDto>>>(response);
            return ApiResponseDto<IEnumerable<UserDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<List<UserDto>>> GetAllUsersPagedAsync(QueryParameters<string> query)
        {
            var url = QueryStringHelper.AddQueryString(FpsApiEndpoints.GetPagedUsers, query);
            var response = await _http.GetAsync<List<UserDto>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<UserDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<UserDto>>>(response);
            return ApiResponseDto<List<UserDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<List<UserDto>>> GetNonSuperUsersPagedAsync(QueryParameters<string> query)
        {
            var url = QueryStringHelper.AddQueryString(FpsApiEndpoints.GetPagedNonSuperUsers, query);
            var response = await _http.GetAsync<List<UserDto>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<UserDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<UserDto>>>(response);
            return ApiResponseDto<List<UserDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<UserDto?>> GetUserByIdAsync(int userId)
        {
            var response = await _http.GetAsync<UserDto>(string.Format(FpsApiEndpoints.GetUserById, userId));
            if (response.Success)
                return _mapper.Map<ApiResponseDto<UserDto?>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<UserDto?>>(response);
            return ApiResponseDto<UserDto?>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<UserDto>> AddUserAsync(UserDto dto)
        {
            var response = await _http.PostAsync<UserDto, UserDto>(FpsApiEndpoints.CreateUser, dto);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<UserDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<UserDto>>(response);
            return ApiResponseDto<UserDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<UserDto>> UpdateUserAsync(UserDto dto)
        {
            var response = await _http.PutAsync<UserDto, UserDto>(FpsApiEndpoints.UpdateUser, dto);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<UserDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<UserDto>>(response);
            return ApiResponseDto<UserDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<bool>> DeleteUserAsync(int userId)
        {
            var response = await _http.DeleteAsync<bool?>(string.Format(FpsApiEndpoints.DeleteUser, userId));
            if (response.Success)
                return _mapper.Map<ApiResponseDto<bool>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<UserPermissionDataDto>> GetUserPermissionsAsync(int userId)
        {
            var response = await _http.GetAsync<UserPermissionDataDto>(string.Format(FpsApiEndpoints.GetUserPermissions, userId));
            if (response.Success)
                return _mapper.Map<ApiResponseDto<UserPermissionDataDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<UserPermissionDataDto>>(response);
            return ApiResponseDto<UserPermissionDataDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<bool>> SaveUserPermissionsAsync(int userId, UserPermissionDataDto dto)
        {
            var response = await _http.PutAsync<UserPermissionDataDto, bool>(string.Format(FpsApiEndpoints.SaveUserPermissions, userId), dto);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<bool>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<PermissionOptionsDto>> GetPermissionOptionsAsync()
        {
            var response = await _http.GetAsync<PermissionOptionsDto>(FpsApiEndpoints.GetPermissionOptions);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<PermissionOptionsDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<PermissionOptionsDto>>(response);
            return ApiResponseDto<PermissionOptionsDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }
    }
}
