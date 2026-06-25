using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Api.Controllers;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPS.Api.UnitTests.Controller.UserControllerTest
{
    public class UserControllerTests
    {
        private readonly IUserService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            _serviceMock = Substitute.For<IUserService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new UserController(_serviceMock, _mapperMock);
        }

        private static UserDto BuildDto(int userId = 1) =>
            new() { UserId = userId, Username = "testuser", Comments = "Test User", UserEmail = "test@example.com", Dt2Username = "dt2user" };

        private static UserReq BuildReq(int userId = 0) =>
            new() { UserId = userId, Username = "testuser", Comments = "Test User", UserEmail = "test@example.com", Dt2Username = "dt2user" };

        private static UserRes BuildRes(int userId = 1) =>
            new() { UserId = userId, Username = "testuser", Comments = "Test User", UserEmail = "test@example.com", Dt2Username = "dt2user" };

        #region Constructor Tests

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenServiceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new UserController(null!, _mapperMock));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenMapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new UserController(_serviceMock, null!));
        }

        #endregion

        #region Access / Authorization Attribute Tests

        [Fact]
        public void Controller_HasAuthorizeAttribute_WithExpectedRoles()
        {
            var attrs = typeof(UserController)
                .GetCustomAttributes(typeof(AuthorizeAttribute), true);
            Assert.NotEmpty(attrs);
            var auth = (AuthorizeAttribute)attrs[0];
            Assert.Contains("API-FPSAdmin", auth.Roles);
        }

        [Fact]
        public void GetAllUsersAsync_HasHttpGetAttribute()
        {
            var method = typeof(UserController).GetMethod(nameof(UserController.GetAllUsersAsync));
            Assert.NotNull(method);
            var attr = method!.GetCustomAttributes(typeof(HttpGetAttribute), true);
            Assert.NotEmpty(attr);
        }

        [Fact]
        public void GetAllUsersPagedAsync_HasHttpGetAttribute()
        {
            var method = typeof(UserController).GetMethod(nameof(UserController.GetAllUsersPagedAsync));
            Assert.NotNull(method);
            var attr = method!.GetCustomAttributes(typeof(HttpGetAttribute), true);
            Assert.NotEmpty(attr);
        }

        [Fact]
        public void GetNonSuperUsersPagedAsync_HasHttpGetAttribute()
        {
            var method = typeof(UserController).GetMethod(nameof(UserController.GetNonSuperUsersPagedAsync));
            Assert.NotNull(method);
            var attr = method!.GetCustomAttributes(typeof(HttpGetAttribute), true);
            Assert.NotEmpty(attr);
        }

        [Fact]
        public void CreateUser_HasHttpPostAttribute()
        {
            var method = typeof(UserController).GetMethod(nameof(UserController.CreateUser));
            Assert.NotNull(method);
            var attr = method!.GetCustomAttributes(typeof(HttpPostAttribute), true);
            Assert.NotEmpty(attr);
        }

        [Fact]
        public void UpdateUser_HasHttpPutAttribute()
        {
            var method = typeof(UserController).GetMethod(nameof(UserController.UpdateUser));
            Assert.NotNull(method);
            var attr = method!.GetCustomAttributes(typeof(HttpPutAttribute), true);
            Assert.NotEmpty(attr);
        }

        [Fact]
        public void DeleteUser_HasHttpDeleteAttribute()
        {
            var method = typeof(UserController).GetMethod(nameof(UserController.DeleteUser));
            Assert.NotNull(method);
            var attr = method!.GetCustomAttributes(typeof(HttpDeleteAttribute), true);
            Assert.NotEmpty(attr);
        }

        #endregion

        #region GetAllUsersAsync Tests

        [Fact]
        public async Task GetAllUsersAsync_ReturnsOk_WithMappedList()
        {
            var dtos = new List<UserDto> { BuildDto() };
            var resList = new List<UserRes> { BuildRes() };

            _serviceMock.GetAllUsersAsync().Returns(dtos);
            _mapperMock.Map<List<UserRes>>(dtos).Returns(resList);

            var result = await _controller.GetAllUsersAsync();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(resList, ok.Value);
            await _serviceMock.Received(1).GetAllUsersAsync();
        }

        [Fact]
        public async Task GetAllUsersAsync_ReturnsEmptyList_WhenNoUsers()
        {
            _serviceMock.GetAllUsersAsync().Returns(new List<UserDto>());
            _mapperMock.Map<List<UserRes>>(Arg.Any<IEnumerable<UserDto>>())
                .Returns(new List<UserRes>());

            var result = await _controller.GetAllUsersAsync();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(new List<UserRes>(), ok.Value);
        }

        [Fact]
        public async Task GetAllUsersAsync_ThrowsException_WhenServiceThrows()
        {
            _serviceMock.GetAllUsersAsync().ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetAllUsersAsync());
        }

        #endregion

        #region GetAllUsersPagedAsync Tests

        [Fact]
        public async Task GetAllUsersPagedAsync_ReturnsOk_WithPagedResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paged = new PaginatedResult<UserDto>
            {
                Data = [BuildDto()],
                PaginationData = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };
            var expected = new PaginationRes<UserRes>
            {
                Data = [BuildRes()],
                PaginationData = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };

            _serviceMock.GetAllUsersPagedAsync(query).Returns(paged);
            _mapperMock.Map<PaginationRes<UserRes>>(paged).Returns(expected);

            var result = await _controller.GetAllUsersPagedAsync(query);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expected, ok.Value);
            await _serviceMock.Received(1).GetAllUsersPagedAsync(query);
        }

        #endregion

        #region GetNonSuperUsersPagedAsync Tests

        [Fact]
        public async Task GetNonSuperUsersPagedAsync_ReturnsOk_WithPagedResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paged = new PaginatedResult<UserDto>
            {
                Data = [BuildDto()],
                PaginationData = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };
            var expected = new PaginationRes<UserRes>
            {
                Data = [BuildRes()],
                PaginationData = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };

            _serviceMock.GetNonSuperUsersPagedAsync(query).Returns(paged);
            _mapperMock.Map<PaginationRes<UserRes>>(paged).Returns(expected);

            var result = await _controller.GetNonSuperUsersPagedAsync(query);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expected, ok.Value);
            await _serviceMock.Received(1).GetNonSuperUsersPagedAsync(query);
        }

        #endregion

        #region GetUserByIdAsync Tests

        [Fact]
        public async Task GetUserByIdAsync_ReturnsOk_WhenFound()
        {
            var dto = BuildDto();
            var res = BuildRes();

            _serviceMock.GetUserByIdAsync(1).Returns(dto);
            _mapperMock.Map<UserRes>(dto).Returns(res);

            var result = await _controller.GetUserByIdAsync(1);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(res, ok.Value);
        }

        [Fact]
        public async Task GetUserByIdAsync_ThrowsArgumentException_WhenNotFound()
        {
            _serviceMock.GetUserByIdAsync(999).Returns((UserDto?)null);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _controller.GetUserByIdAsync(999));
        }

        #endregion

        #region CreateUser Tests

        [Fact]
        public async Task CreateUser_ReturnsOk_WhenSuccessful()
        {
            var req = BuildReq();
            var dto = BuildDto();
            var addedDto = BuildDto();
            var res = BuildRes();

            _mapperMock.Map<UserDto>(req).Returns(dto);
            _serviceMock.AddUserAsync(dto).Returns(addedDto);
            _mapperMock.Map<UserRes>(addedDto).Returns(res);

            var result = await _controller.CreateUser(req);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(res, ok.Value);
            await _serviceMock.Received(1).AddUserAsync(dto);
        }

        [Fact]
        public async Task CreateUser_ThrowsException_WhenServiceThrows()
        {
            var req = BuildReq();
            var dto = BuildDto();
            _mapperMock.Map<UserDto>(req).Returns(dto);
            _serviceMock.AddUserAsync(dto).ThrowsAsync(new InvalidOperationException("User already exists."));

            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.CreateUser(req));
        }

        #endregion

        #region UpdateUser Tests

        [Fact]
        public async Task UpdateUser_ReturnsOk_WhenSuccessful()
        {
            var req = BuildReq(1);
            var dto = BuildDto();
            var updatedDto = BuildDto();
            var res = BuildRes();

            _mapperMock.Map<UserDto>(req).Returns(dto);
            _serviceMock.UpdateUserAsync(dto).Returns(updatedDto);
            _mapperMock.Map<UserRes>(updatedDto).Returns(res);

            var result = await _controller.UpdateUser(req);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(res, ok.Value);
            await _serviceMock.Received(1).UpdateUserAsync(dto);
        }

        [Fact]
        public async Task UpdateUser_ThrowsException_WhenServiceThrows()
        {
            var req = BuildReq(1);
            var dto = BuildDto();
            _mapperMock.Map<UserDto>(req).Returns(dto);
            _serviceMock.UpdateUserAsync(dto).ThrowsAsync(new KeyNotFoundException("User not found."));

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.UpdateUser(req));
        }

        #endregion

        #region DeleteUser Tests

        [Fact]
        public async Task DeleteUser_ReturnsOk_WhenSuccessful()
        {
            _serviceMock.DeleteUserAsync(1).Returns(true);

            var result = await _controller.DeleteUser(1);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)ok.Value!);
        }

        [Fact]
        public async Task DeleteUser_ThrowsArgumentException_WhenNotFound()
        {
            _serviceMock.DeleteUserAsync(999).Returns(false);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _controller.DeleteUser(999));
        }

        #endregion

        #region GetUserPermissionsAsync Tests

        [Fact]
        public async Task GetUserPermissionsAsync_ReturnsOk_WithMappedPermissions()
        {
            var dto = new UserPermissionDto
            {
                UserId = 1,
                ProfitCentres = ["PC1"],
                Programs = ["P1"],
                Categories = ["C1"],
                TestOwners = ["T1"],
                ProjectGroups = ["PG1"]
            };
            var res = new UserPermissionRes
            {
                UserId = 1,
                ProfitCentres = ["PC1"],
                Programs = ["P1"],
                Categories = ["C1"],
                TestOwners = ["T1"],
                ProjectGroups = ["PG1"]
            };

            _serviceMock.GetUserPermissionsAsync(1).Returns(dto);
            _mapperMock.Map<UserPermissionRes>(dto).Returns(res);

            var result = await _controller.GetUserPermissionsAsync(1);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(res, ok.Value);
        }

        #endregion

        #region SaveUserPermissionsAsync Tests

        [Fact]
        public async Task SaveUserPermissionsAsync_ReturnsOkTrue_WhenSuccessful()
        {
            var req = new UserPermissionReq { UserId = 1, ProfitCentres = ["PC1"] };
            var dto = new UserPermissionDto { UserId = 1, ProfitCentres = ["PC1"] };

            _mapperMock.Map<UserPermissionDto>(req).Returns(dto);

            var result = await _controller.SaveUserPermissionsAsync(1, req);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)ok.Value!);
            await _serviceMock.Received(1).SaveUserPermissionsAsync(
                Arg.Is<UserPermissionDto>(d => d.UserId == 1));
        }

        [Fact]
        public async Task SaveUserPermissionsAsync_SetsUserIdFromRoute()
        {
            var req = new UserPermissionReq { UserId = 0, ProfitCentres = ["PC1"] };
            var dto = new UserPermissionDto { UserId = 0 };

            _mapperMock.Map<UserPermissionDto>(req).Returns(dto);

            await _controller.SaveUserPermissionsAsync(5, req);

            await _serviceMock.Received(1).SaveUserPermissionsAsync(
                Arg.Is<UserPermissionDto>(d => d.UserId == 5));
        }

        [Fact]
        public async Task SaveUserPermissionsAsync_ThrowsException_WhenServiceThrows()
        {
            var req = new UserPermissionReq { UserId = 1 };
            var dto = new UserPermissionDto();
            _mapperMock.Map<UserPermissionDto>(req).Returns(dto);
            _serviceMock.SaveUserPermissionsAsync(Arg.Any<UserPermissionDto>())
                .ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() =>
                _controller.SaveUserPermissionsAsync(1, req));
        }

        #endregion

        #region GetPermissionOptionsAsync Tests

        [Fact]
        public async Task GetPermissionOptionsAsync_ReturnsOk_WithMappedOptions()
        {
            var dto = new PermissionOptionsDto
            {
                ProfitCentres = ["PC1"],
                Programs = ["P1"],
                Categories = ["C1"],
                TestOwners = ["T1"],
                ProjectGroups = ["PG1"]
            };
            var res = new PermissionOptionsRes
            {
                ProfitCentres = ["PC1"],
                Programs = ["P1"],
                Categories = ["C1"],
                TestOwners = ["T1"],
                ProjectGroups = ["PG1"]
            };

            _serviceMock.GetPermissionOptionsAsync().Returns(dto);
            _mapperMock.Map<PermissionOptionsRes>(dto).Returns(res);

            var result = await _controller.GetPermissionOptionsAsync();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(res, ok.Value);
        }

        #endregion
    }
}
