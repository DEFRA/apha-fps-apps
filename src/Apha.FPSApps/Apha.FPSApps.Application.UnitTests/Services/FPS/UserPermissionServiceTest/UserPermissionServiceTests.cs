using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.UserPermissionServiceTest
{
    public class UserPermissionServiceTests
    {
        private readonly IFpsApiClient _mockFpsClient;
        private readonly IFpsUserPermissionApiClient _mockApiClient;
        private readonly UserPermissionService _sut;

        public UserPermissionServiceTests()
        {
            _mockFpsClient = Substitute.For<IFpsApiClient>();
            _mockApiClient = Substitute.For<IFpsUserPermissionApiClient>();
            _mockFpsClient.FpsUserPermission.Returns(_mockApiClient);
            _sut = new UserPermissionService(_mockFpsClient);
        }

        private static UserPermissionDto BuildDto(int userId = 1) =>
            new() { UserId = userId, Username = "testuser", Comments = "Test User", UserEmail = "test@example.com", Dt2Username = "dt2user" };

        #region Constructor Tests

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenFpsClientIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new UserPermissionService(null!));
        }

        #endregion

        #region GetAllUsersAsync (non-paged) Tests

        [Fact]
        public async Task GetAllUsersAsync_ReturnsApiResponse()
        {
            var dtos = new List<UserPermissionDto> { BuildDto() };
            var response = ApiResponseDto<IEnumerable<UserPermissionDto>>.SuccessResponse(dtos);
            _mockApiClient.GetAllUsersAsync().Returns(response);

            var result = await _sut.GetAllUsersAsync();

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _mockApiClient.Received(1).GetAllUsersAsync();
        }

        [Fact]
        public async Task GetAllUsersAsync_PropagatesApiErrors()
        {
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } };
            var response = ApiResponseDto<IEnumerable<UserPermissionDto>>.FailureResponse(errors, new ApiMetaDto());
            _mockApiClient.GetAllUsersAsync().Returns(response);

            var result = await _sut.GetAllUsersAsync();

            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        #endregion

        #region GetAllUsersPagedAsync Tests

        [Fact]
        public async Task GetAllUsersPagedAsync_ReturnsApiResponse()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos = new List<UserPermissionDto> { BuildDto() };
            var pagination = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 };
            var response = ApiResponseDto<List<UserPermissionDto>>.SuccessResponse(dtos, pagination);
            _mockApiClient.GetAllUsersPagedAsync(query).Returns(response);

            var result = await _sut.GetAllUsersPagedAsync(query);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _mockApiClient.Received(1).GetAllUsersPagedAsync(query);
        }

        [Fact]
        public async Task GetAllUsersPagedAsync_PropagatesApiErrors()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } };
            var response = ApiResponseDto<List<UserPermissionDto>>.FailureResponse(errors, new ApiMetaDto());
            _mockApiClient.GetAllUsersPagedAsync(query).Returns(response);

            var result = await _sut.GetAllUsersPagedAsync(query);

            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetAllUsersPagedAsync_PassesFilterAndSortParameters()
        {
            var query = new QueryParameters<string>
            {
                Page = 2, PageSize = 5, SortBy = "Username", Descending = true,
                Filter = "{\"Username\":\"test\"}"
            };
            var response = ApiResponseDto<List<UserPermissionDto>>.SuccessResponse([]);
            _mockApiClient.GetAllUsersPagedAsync(query).Returns(response);

            await _sut.GetAllUsersPagedAsync(query);

            await _mockApiClient.Received(1).GetAllUsersPagedAsync(Arg.Is<QueryParameters<string>>(
                q => q.Page == 2 && q.PageSize == 5 && q.SortBy == "Username" && q.Descending == true));
        }

        #endregion

        #region GetUserByIdAsync Tests

        [Fact]
        public async Task GetUserByIdAsync_ReturnsApiResponse()
        {
            var dto = BuildDto();
            var response = ApiResponseDto<UserPermissionDto?>.SuccessResponse(dto);
            _mockApiClient.GetUserByIdAsync(1).Returns(response);

            var result = await _sut.GetUserByIdAsync(1);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(1, result.Data!.UserId);
            await _mockApiClient.Received(1).GetUserByIdAsync(1);
        }

        [Fact]
        public async Task GetUserByIdAsync_PropagatesApiErrors()
        {
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "404" } };
            var response = ApiResponseDto<UserPermissionDto?>.FailureResponse(errors, new ApiMetaDto());
            _mockApiClient.GetUserByIdAsync(999).Returns(response);

            var result = await _sut.GetUserByIdAsync(999);

            Assert.False(result.Success);
        }

        #endregion

        #region AddUserAsync Tests

        [Fact]
        public async Task AddUserAsync_ReturnsApiResponse()
        {
            var dto = BuildDto();
            var response = ApiResponseDto<UserPermissionDto>.SuccessResponse(dto);
            _mockApiClient.AddUserAsync(dto).Returns(response);

            var result = await _sut.AddUserAsync(dto);

            Assert.NotNull(result);
            Assert.True(result.Success);
            await _mockApiClient.Received(1).AddUserAsync(dto);
        }

        [Fact]
        public async Task AddUserAsync_PropagatesApiErrors()
        {
            var dto = BuildDto();
            var errors = new List<ApiErrorDto> { new() { Message = "Username exists", Code = "400" } };
            var response = ApiResponseDto<UserPermissionDto>.FailureResponse(errors, new ApiMetaDto());
            _mockApiClient.AddUserAsync(dto).Returns(response);

            var result = await _sut.AddUserAsync(dto);

            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        #endregion

        #region UpdateUserAsync Tests

        [Fact]
        public async Task UpdateUserAsync_ReturnsApiResponse()
        {
            var dto = BuildDto();
            var response = ApiResponseDto<UserPermissionDto>.SuccessResponse(dto);
            _mockApiClient.UpdateUserAsync(dto).Returns(response);

            var result = await _sut.UpdateUserAsync(dto);

            Assert.NotNull(result);
            Assert.True(result.Success);
            await _mockApiClient.Received(1).UpdateUserAsync(dto);
        }

        [Fact]
        public async Task UpdateUserAsync_PropagatesApiErrors()
        {
            var dto = BuildDto();
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "404" } };
            var response = ApiResponseDto<UserPermissionDto>.FailureResponse(errors, new ApiMetaDto());
            _mockApiClient.UpdateUserAsync(dto).Returns(response);

            var result = await _sut.UpdateUserAsync(dto);

            Assert.False(result.Success);
        }

        #endregion

        #region DeleteUserAsync Tests

        [Fact]
        public async Task DeleteUserAsync_ReturnsApiResponse()
        {
            var response = ApiResponseDto<bool>.SuccessResponse(true);
            _mockApiClient.DeleteUserAsync(1).Returns(response);

            var result = await _sut.DeleteUserAsync(1);

            Assert.NotNull(result);
            Assert.True(result.Success);
            await _mockApiClient.Received(1).DeleteUserAsync(1);
        }

        [Fact]
        public async Task DeleteUserAsync_PropagatesApiErrors()
        {
            var errors = new List<ApiErrorDto> { new() { Message = "Delete failed", Code = "500" } };
            var response = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _mockApiClient.DeleteUserAsync(1).Returns(response);

            var result = await _sut.DeleteUserAsync(1);

            Assert.False(result.Success);
        }

        #endregion

        #region GetUserPermissionsAsync Tests

        [Fact]
        public async Task GetUserPermissionsAsync_ReturnsApiResponse()
        {
            var data = new UserPermissionDataDto
            {
                UserId = 1,
                ProfitCentres = ["PC1"],
                Programs = ["P1"]
            };
            var response = ApiResponseDto<UserPermissionDataDto>.SuccessResponse(data);
            _mockApiClient.GetUserPermissionsAsync(1).Returns(response);

            var result = await _sut.GetUserPermissionsAsync(1);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(1, result.Data!.UserId);
            await _mockApiClient.Received(1).GetUserPermissionsAsync(1);
        }

        [Fact]
        public async Task GetUserPermissionsAsync_PropagatesApiErrors()
        {
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "500" } };
            var response = ApiResponseDto<UserPermissionDataDto>.FailureResponse(errors, new ApiMetaDto());
            _mockApiClient.GetUserPermissionsAsync(1).Returns(response);

            var result = await _sut.GetUserPermissionsAsync(1);

            Assert.False(result.Success);
        }

        #endregion

        #region SaveUserPermissionsAsync Tests

        [Fact]
        public async Task SaveUserPermissionsAsync_ReturnsApiResponse()
        {
            var data = new UserPermissionDataDto
            {
                UserId = 1,
                ProfitCentres = ["PC1"],
                Programs = ["P1"]
            };
            var response = ApiResponseDto<bool>.SuccessResponse(true);
            _mockApiClient.SaveUserPermissionsAsync(1, data).Returns(response);

            var result = await _sut.SaveUserPermissionsAsync(1, data);

            Assert.NotNull(result);
            Assert.True(result.Success);
            await _mockApiClient.Received(1).SaveUserPermissionsAsync(1, data);
        }

        [Fact]
        public async Task SaveUserPermissionsAsync_PropagatesApiErrors()
        {
            var data = new UserPermissionDataDto { UserId = 1 };
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "500" } };
            var response = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _mockApiClient.SaveUserPermissionsAsync(1, data).Returns(response);

            var result = await _sut.SaveUserPermissionsAsync(1, data);

            Assert.False(result.Success);
        }

        #endregion

        #region GetPermissionOptionsAsync Tests

        [Fact]
        public async Task GetPermissionOptionsAsync_ReturnsApiResponse()
        {
            var options = new PermissionOptionsDto
            {
                ProfitCentres = ["PC1"],
                Programs = ["P1"],
                Categories = ["C1"],
                TestOwners = ["T1"],
                ProjectGroups = ["PG1"]
            };
            var response = ApiResponseDto<PermissionOptionsDto>.SuccessResponse(options);
            _mockApiClient.GetPermissionOptionsAsync().Returns(response);

            var result = await _sut.GetPermissionOptionsAsync();

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!.ProfitCentres);
            await _mockApiClient.Received(1).GetPermissionOptionsAsync();
        }

        [Fact]
        public async Task GetPermissionOptionsAsync_PropagatesApiErrors()
        {
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "500" } };
            var response = ApiResponseDto<PermissionOptionsDto>.FailureResponse(errors, new ApiMetaDto());
            _mockApiClient.GetPermissionOptionsAsync().Returns(response);

            var result = await _sut.GetPermissionOptionsAsync();

            Assert.False(result.Success);
        }

        #endregion
    }
}
