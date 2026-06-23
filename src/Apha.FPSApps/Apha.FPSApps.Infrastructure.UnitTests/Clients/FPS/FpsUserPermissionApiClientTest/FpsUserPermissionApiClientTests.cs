using Apha.Common.Contracts;
using Apha.Common.Constants;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsUserPermissionApiClientTest
{
    public class FpsUserPermissionApiClientTests
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly FpsUserPermissionApiClient _client;

        public FpsUserPermissionApiClientTests()
        {
            _http = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsUserPermissionApiClient(_http, _mapper);
        }

        private static UserPermissionDto BuildDto(int userId = 1) =>
            new() { UserId = userId, Username = "testuser", Comments = "Test User", UserEmail = "test@example.com", Dt2Username = "dt2user" };

        private static ApiResponse<T> SuccessApiResponse<T>(T data) =>
            new() { Success = true, Data = data };

        private static ApiResponse<T> FailureApiResponse<T>() =>
            new()
            {
                Success = false,
                Errors = [new ApiError { Message = "Error", Code = "ERROR" }]
            };

        #region Constructor Tests

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenHttpIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new FpsUserPermissionApiClient(null!, _mapper));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenMapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new FpsUserPermissionApiClient(_http, null!));
        }

        #endregion

        #region GetAllUsersAsync (non-paged) Tests

        [Fact]
        public async Task GetAllUsersAsync_WithSuccessResponse_ReturnsMappedList()
        {
            var dtos = new List<UserPermissionDto> { BuildDto() };
            var apiResponse = SuccessApiResponse<IEnumerable<UserPermissionDto>>(dtos);
            var expected = ApiResponseDto<IEnumerable<UserPermissionDto>>.SuccessResponse(dtos);

            _http.GetAsync<IEnumerable<UserPermissionDto>>(FpsApiEndpoints.GetAllUsers).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<IEnumerable<UserPermissionDto>>>(apiResponse).Returns(expected);

            var result = await _client.GetAllUsersAsync();

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _http.Received(1).GetAsync<IEnumerable<UserPermissionDto>>(FpsApiEndpoints.GetAllUsers);
        }

        [Fact]
        public async Task GetAllUsersAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var apiResponse = FailureApiResponse<IEnumerable<UserPermissionDto>>();
            var failDto = new ApiResponseDto<IEnumerable<UserPermissionDto>>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Error", Code = "ERROR" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<IEnumerable<UserPermissionDto>>(FpsApiEndpoints.GetAllUsers).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<IEnumerable<UserPermissionDto>>>(apiResponse).Returns(failDto);

            var result = await _client.GetAllUsersAsync();

            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors!);
        }

        #endregion

        #region GetAllUsersPagedAsync Tests

        [Fact]
        public async Task GetAllUsersPagedAsync_WithSuccessResponse_ReturnsMappedList()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos = new List<UserPermissionDto> { BuildDto() };
            var apiResponse = SuccessApiResponse(dtos);
            var expected = ApiResponseDto<List<UserPermissionDto>>.SuccessResponse(dtos,
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 });

            _http.GetAsync<List<UserPermissionDto>>(Arg.Is<string>(u => u.Contains("user/users/paged")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<UserPermissionDto>>>(apiResponse).Returns(expected);

            var result = await _client.GetAllUsersPagedAsync(query);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
        }

        [Fact]
        public async Task GetAllUsersPagedAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = FailureApiResponse<List<UserPermissionDto>>();
            var failDto = new ApiResponseDto<List<UserPermissionDto>>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Error", Code = "ERROR" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<UserPermissionDto>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<UserPermissionDto>>>(apiResponse).Returns(failDto);

            var result = await _client.GetAllUsersPagedAsync(query);

            Assert.False(result.Success);
        }

        #endregion

        #region GetNonSuperUsersPagedAsync Tests

        [Fact]
        public async Task GetNonSuperUsersPagedAsync_WithSuccessResponse_ReturnsMappedList()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos = new List<UserPermissionDto> { BuildDto() };
            var apiResponse = SuccessApiResponse(dtos);
            var expected = ApiResponseDto<List<UserPermissionDto>>.SuccessResponse(dtos,
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 });

            _http.GetAsync<List<UserPermissionDto>>(Arg.Is<string>(u => u.Contains("nonsuperusers")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<UserPermissionDto>>>(apiResponse).Returns(expected);

            var result = await _client.GetNonSuperUsersPagedAsync(query);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
        }

        [Fact]
        public async Task GetNonSuperUsersPagedAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = FailureApiResponse<List<UserPermissionDto>>();
            var failDto = new ApiResponseDto<List<UserPermissionDto>>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Error", Code = "ERROR" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<UserPermissionDto>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<UserPermissionDto>>>(apiResponse).Returns(failDto);

            var result = await _client.GetNonSuperUsersPagedAsync(query);

            Assert.False(result.Success);
        }

        #endregion

        #region GetUserByIdAsync Tests

        [Fact]
        public async Task GetUserByIdAsync_WithSuccessResponse_ReturnsUser()
        {
            var dto = BuildDto();
            var apiResponse = SuccessApiResponse(dto);
            var expected = ApiResponseDto<UserPermissionDto?>.SuccessResponse(dto);

            _http.GetAsync<UserPermissionDto>(string.Format(FpsApiEndpoints.GetUserById, 1)).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<UserPermissionDto?>>(apiResponse).Returns(expected);

            var result = await _client.GetUserByIdAsync(1);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(1, result.Data!.UserId);
        }

        [Fact]
        public async Task GetUserByIdAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var apiResponse = FailureApiResponse<UserPermissionDto>();
            var failDto = new ApiResponseDto<UserPermissionDto?>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Not found", Code = "404" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<UserPermissionDto>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<UserPermissionDto?>>(apiResponse).Returns(failDto);

            var result = await _client.GetUserByIdAsync(999);

            Assert.False(result.Success);
        }

        #endregion

        #region AddUserAsync Tests

        [Fact]
        public async Task AddUserAsync_WithSuccessResponse_ReturnsCreatedUser()
        {
            var dto = BuildDto();
            var apiResponse = SuccessApiResponse(dto);
            var expected = ApiResponseDto<UserPermissionDto>.SuccessResponse(dto);

            _http.PostAsync<UserPermissionDto, UserPermissionDto>(FpsApiEndpoints.CreateUser, dto).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<UserPermissionDto>>(apiResponse).Returns(expected);

            var result = await _client.AddUserAsync(dto);

            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).PostAsync<UserPermissionDto, UserPermissionDto>(FpsApiEndpoints.CreateUser, dto);
        }

        [Fact]
        public async Task AddUserAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var dto = BuildDto();
            var apiResponse = FailureApiResponse<UserPermissionDto>();
            var failDto = new ApiResponseDto<UserPermissionDto>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Error", Code = "400" }],
                Meta = new ApiMetaDto()
            };

            _http.PostAsync<UserPermissionDto, UserPermissionDto>(FpsApiEndpoints.CreateUser, dto).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<UserPermissionDto>>(apiResponse).Returns(failDto);

            var result = await _client.AddUserAsync(dto);

            Assert.False(result.Success);
        }

        #endregion

        #region UpdateUserAsync Tests

        [Fact]
        public async Task UpdateUserAsync_WithSuccessResponse_ReturnsUpdatedUser()
        {
            var dto = BuildDto();
            var apiResponse = SuccessApiResponse(dto);
            var expected = ApiResponseDto<UserPermissionDto>.SuccessResponse(dto);

            _http.PutAsync<UserPermissionDto, UserPermissionDto>(FpsApiEndpoints.UpdateUser, dto).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<UserPermissionDto>>(apiResponse).Returns(expected);

            var result = await _client.UpdateUserAsync(dto);

            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).PutAsync<UserPermissionDto, UserPermissionDto>(FpsApiEndpoints.UpdateUser, dto);
        }

        [Fact]
        public async Task UpdateUserAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var dto = BuildDto();
            var apiResponse = FailureApiResponse<UserPermissionDto>();
            var failDto = new ApiResponseDto<UserPermissionDto>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Error", Code = "400" }],
                Meta = new ApiMetaDto()
            };

            _http.PutAsync<UserPermissionDto, UserPermissionDto>(FpsApiEndpoints.UpdateUser, dto).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<UserPermissionDto>>(apiResponse).Returns(failDto);

            var result = await _client.UpdateUserAsync(dto);

            Assert.False(result.Success);
        }

        #endregion

        #region DeleteUserAsync Tests

        [Fact]
        public async Task DeleteUserAsync_WithSuccessResponse_ReturnsTrue()
        {
            var apiResponse = SuccessApiResponse<bool?>(true);
            var expected = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool?>(string.Format(FpsApiEndpoints.DeleteUser, 1)).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expected);

            var result = await _client.DeleteUserAsync(1);

            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).DeleteAsync<bool?>(string.Format(FpsApiEndpoints.DeleteUser, 1));
        }

        [Fact]
        public async Task DeleteUserAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var apiResponse = FailureApiResponse<bool?>();
            var failDto = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Error", Code = "500" }],
                Meta = new ApiMetaDto()
            };

            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(failDto);

            var result = await _client.DeleteUserAsync(1);

            Assert.False(result.Success);
        }

        #endregion

        #region GetUserPermissionsAsync Tests

        [Fact]
        public async Task GetUserPermissionsAsync_WithSuccessResponse_ReturnsPermissions()
        {
            var data = new UserPermissionDataDto
            {
                UserId = 1,
                ProfitCentres = ["PC1"],
                Programs = ["P1"]
            };
            var apiResponse = SuccessApiResponse(data);
            var expected = ApiResponseDto<UserPermissionDataDto>.SuccessResponse(data);

            _http.GetAsync<UserPermissionDataDto>(string.Format(FpsApiEndpoints.GetUserPermissions, 1)).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<UserPermissionDataDto>>(apiResponse).Returns(expected);

            var result = await _client.GetUserPermissionsAsync(1);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(1, result.Data!.UserId);
        }

        [Fact]
        public async Task GetUserPermissionsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var apiResponse = FailureApiResponse<UserPermissionDataDto>();
            var failDto = new ApiResponseDto<UserPermissionDataDto>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Error", Code = "500" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<UserPermissionDataDto>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<UserPermissionDataDto>>(apiResponse).Returns(failDto);

            var result = await _client.GetUserPermissionsAsync(1);

            Assert.False(result.Success);
        }

        #endregion

        #region SaveUserPermissionsAsync Tests

        [Fact]
        public async Task SaveUserPermissionsAsync_WithSuccessResponse_ReturnsTrue()
        {
            var data = new UserPermissionDataDto
            {
                UserId = 1,
                ProfitCentres = ["PC1"],
                Programs = ["P1"]
            };
            var apiResponse = SuccessApiResponse(true);
            var expected = ApiResponseDto<bool>.SuccessResponse(true);

            _http.PutAsync<UserPermissionDataDto, bool>(string.Format(FpsApiEndpoints.SaveUserPermissions, 1), data)
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expected);

            var result = await _client.SaveUserPermissionsAsync(1, data);

            Assert.NotNull(result);
            Assert.True(result.Success);
        }

        [Fact]
        public async Task SaveUserPermissionsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var data = new UserPermissionDataDto { UserId = 1 };
            var apiResponse = FailureApiResponse<bool>();
            var failDto = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Error", Code = "500" }],
                Meta = new ApiMetaDto()
            };

            _http.PutAsync<UserPermissionDataDto, bool>(Arg.Any<string>(), data).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(failDto);

            var result = await _client.SaveUserPermissionsAsync(1, data);

            Assert.False(result.Success);
        }

        #endregion

        #region GetPermissionOptionsAsync Tests

        [Fact]
        public async Task GetPermissionOptionsAsync_WithSuccessResponse_ReturnsOptions()
        {
            var options = new PermissionOptionsDto
            {
                ProfitCentres = ["PC1"],
                Programs = ["P1"],
                Categories = ["C1"],
                TestOwners = ["T1"],
                ProjectGroups = ["PG1"]
            };
            var apiResponse = SuccessApiResponse(options);
            var expected = ApiResponseDto<PermissionOptionsDto>.SuccessResponse(options);

            _http.GetAsync<PermissionOptionsDto>(FpsApiEndpoints.GetPermissionOptions).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<PermissionOptionsDto>>(apiResponse).Returns(expected);

            var result = await _client.GetPermissionOptionsAsync();

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!.ProfitCentres);
            await _http.Received(1).GetAsync<PermissionOptionsDto>(FpsApiEndpoints.GetPermissionOptions);
        }

        [Fact]
        public async Task GetPermissionOptionsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var apiResponse = FailureApiResponse<PermissionOptionsDto>();
            var failDto = new ApiResponseDto<PermissionOptionsDto>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Error", Code = "500" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<PermissionOptionsDto>(FpsApiEndpoints.GetPermissionOptions).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<PermissionOptionsDto>>(apiResponse).Returns(failDto);

            var result = await _client.GetPermissionOptionsAsync();

            Assert.False(result.Success);
        }

        #endregion
    }
}
