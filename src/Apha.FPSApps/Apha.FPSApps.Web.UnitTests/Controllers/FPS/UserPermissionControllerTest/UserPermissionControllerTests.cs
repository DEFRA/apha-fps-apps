using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Text.Json;
using Xunit;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.UserPermissionControllerTest
{
    public class UserPermissionControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IUserPermissionService _userPermissionService;
        private readonly UserPermissionController _controller;

        public UserPermissionControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _userPermissionService = Substitute.For<IUserPermissionService>();
            _controller = new UserPermissionController(_mapper, _userPermissionService);
        }

        private static T? GetJsonResultValue<T>(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<T>(json);
        }

        private static UserPermissionDto BuildDto(int userId = 1) =>
            new() { UserId = userId, Username = "testuser", Comments = "Test User", UserEmail = "test@example.com", Dt2Username = "dt2user" };

        private static UserPermissionViewModel BuildViewModel(int userId = 0) =>
            new() { UserId = userId, Username = "testuser", Comments = "Test User", UserEmail = "test@example.com", Dt2Username = "dt2user" };

        private static ApiResponseDto<List<UserPermissionDto>> BuildPagedResponse(
            IEnumerable<UserPermissionDto>? data = null) =>
            ApiResponseDto<List<UserPermissionDto>>.SuccessResponse(
                data?.ToList() ?? [],
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 });

        private record JsonResponse(bool success, string? message);

        private void SetupPagedService(IEnumerable<UserPermissionDto>? data = null)
        {
            var dtoList = data?.ToList() ?? new List<UserPermissionDto> { BuildDto() };
            var response = BuildPagedResponse(dtoList);
            _userPermissionService
                .GetAllUsersPagedAsync(Arg.Any<QueryParameters<string>>())
                .Returns(response);
            _mapper.Map<List<UserPermissionViewModel>>(Arg.Any<List<UserPermissionDto>>())
                .Returns(dtoList.Select(d => BuildViewModel(d.UserId)).ToList());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
        }

        private void SetupPermissionOptions()
        {
            var options = new PermissionOptionsDto
            {
                ProfitCentres = ["PC1"],
                Programs = ["P1"],
                Categories = ["C1"],
                TestOwners = ["T1"],
                ProjectGroups = ["PG1"]
            };
            _userPermissionService.GetPermissionOptionsAsync()
                .Returns(ApiResponseDto<PermissionOptionsDto>.SuccessResponse(options));
        }

        #region Access / Authorization Attribute Tests

        [Fact]
        public void Controller_HasAuthorizeAttribute_WithExpectedRoles()
        {
            var attrs = typeof(UserPermissionController)
                .GetCustomAttributes(typeof(AuthorizeAttribute), true);
            Assert.NotEmpty(attrs);
            var auth = (AuthorizeAttribute)attrs[0];
            Assert.Contains("FPSAdmin", auth.Roles);
        }

        [Fact]
        public void Create_Post_HasHttpPostAttribute()
        {
            var methods = typeof(UserPermissionController).GetMethods()
                .Where(m => m.Name == "Create" && m.GetParameters().Length > 0);
            Assert.NotEmpty(methods);
            var postMethod = methods.FirstOrDefault(m =>
                m.GetCustomAttributes(typeof(HttpPostAttribute), true).Length > 0);
            Assert.NotNull(postMethod);
        }

        [Fact]
        public void Edit_Post_HasHttpPostAttribute()
        {
            var methods = typeof(UserPermissionController).GetMethods()
                .Where(m => m.Name == "Edit" && m.GetParameters().Length > 0);
            var postMethod = methods.FirstOrDefault(m =>
                m.GetCustomAttributes(typeof(HttpPostAttribute), true).Length > 0);
            Assert.NotNull(postMethod);
        }

        [Fact]
        public void Delete_HasHttpDeleteAttribute()
        {
            var method = typeof(UserPermissionController).GetMethod(nameof(UserPermissionController.Delete));
            Assert.NotNull(method);
            var attr = method!.GetCustomAttributes(typeof(HttpDeleteAttribute), true);
            Assert.NotEmpty(attr);
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenMapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new UserPermissionController(null!, _userPermissionService));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenServiceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new UserPermissionController(_mapper, null!));
        }

        #endregion

        #region Index Tests

        [Fact]
        public async Task Index_ReturnsViewResult_WithDataGridConfig()
        {
            SetupPagedService();
            SetupPermissionOptions();

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<DataGridConfig<UserPermissionViewModel>>(viewResult.Model);
        }

        [Fact]
        public async Task Index_Grid_HasCorrectGridId()
        {
            SetupPagedService();
            SetupPermissionOptions();

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<DataGridConfig<UserPermissionViewModel>>(viewResult.Model);
            Assert.Equal("userPermissionGrid", model.GridId);
        }

        [Fact]
        public async Task Index_Grid_HasCorrectBindUrl()
        {
            SetupPagedService();
            SetupPermissionOptions();

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<DataGridConfig<UserPermissionViewModel>>(viewResult.Model);
            Assert.Equal("/FPS/UserPermission/LoadUserGrid", model.BindGridUrl);
        }

        [Fact]
        public async Task Index_Grid_AllowsRowSelection()
        {
            SetupPagedService();
            SetupPermissionOptions();

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<DataGridConfig<UserPermissionViewModel>>(viewResult.Model);
            Assert.True(model.AllowRowSelection);
        }

        #endregion

        #region LoadUserGrid Tests

        [Fact]
        public async Task LoadUserGrid_WithValidRequest_ReturnsPartialView()
        {
            SetupPagedService();
            var request = new PaginationFilter<string> { Filter = "{}" };

            var result = await _controller.LoadUserGrid(request);

            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialViewResult.ViewName);
            Assert.IsType<DataGridConfig<UserPermissionViewModel>>(partialViewResult.Model);
        }

        [Fact]
        public async Task LoadUserGrid_WithInvalidModelState_ReturnsJsonError()
        {
            _controller.ModelState.AddModelError("Test", "Test error");
            var request = new PaginationFilter<string> { Filter = "{}" };

            var result = await _controller.LoadUserGrid(request);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Invalid request data", value.message);
        }

        [Fact]
        public async Task LoadUserGrid_WithEmptyData_ReturnsEmptyGrid()
        {
            SetupPagedService([]);

            var result = await _controller.LoadUserGrid(new PaginationFilter<string>());

            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<UserPermissionViewModel>>(partialViewResult.Model);
            Assert.Empty(gridConfig.Data);
        }

        [Fact]
        public async Task LoadUserGrid_PassesFilterToService()
        {
            SetupPagedService();
            var request = new PaginationFilter<string>
            {
                Filter = "{\"Username\":\"test\"}",
                SortBy = "Username",
                Descending = false
            };

            await _controller.LoadUserGrid(request);

            await _userPermissionService.Received(1)
                .GetAllUsersPagedAsync(Arg.Any<QueryParameters<string>>());
        }

        #endregion

        #region Create GET Tests

        [Fact]
        public void Create_Get_ReturnsPartialViewWithEmptyModel()
        {
            var result = _controller.Create();

            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditUser", partialViewResult.ViewName);
            var model = Assert.IsType<UserPermissionViewModel>(partialViewResult.Model);
            Assert.Equal(0, model.UserId);
        }

        #endregion

        #region Create POST Tests

        [Fact]
        public async Task Create_Post_WithInvalidModelState_ReturnsJsonError()
        {
            _controller.ModelState.AddModelError("Username", "Required");
            var model = BuildViewModel();

            var result = await _controller.Create(model);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        [Fact]
        public async Task Create_Post_WithValidModel_ReturnsSuccessJson()
        {
            var model = BuildViewModel();
            var dto = BuildDto();
            var response = ApiResponseDto<UserPermissionDto>.SuccessResponse(dto);

            _mapper.Map<UserPermissionDto>(model).Returns(dto);
            _userPermissionService.AddUserAsync(dto).Returns(response);

            var result = await _controller.Create(model);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
            Assert.Equal("User created successfully.", value.message);
        }

        [Fact]
        public async Task Create_Post_WithApiFailure_ReturnsErrorJson()
        {
            var model = BuildViewModel();
            var dto = BuildDto();
            var errors = new List<ApiErrorDto> { new() { Message = "Creation failed", Code = "400" } };
            var response = ApiResponseDto<UserPermissionDto>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<UserPermissionDto>(model).Returns(dto);
            _userPermissionService.AddUserAsync(dto).Returns(response);

            var result = await _controller.Create(model);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        #endregion

        #region Edit GET Tests

        [Fact]
        public async Task Edit_Get_WithValidId_ReturnsPartialView()
        {
            var dto = BuildDto();
            var viewModel = BuildViewModel(1);
            var response = ApiResponseDto<UserPermissionDto?>.SuccessResponse(dto);

            _userPermissionService.GetUserByIdAsync(1).Returns(response);
            _mapper.Map<UserPermissionViewModel>(dto).Returns(viewModel);

            var result = await _controller.Edit(1);

            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditUser", partialViewResult.ViewName);
            Assert.IsType<UserPermissionViewModel>(partialViewResult.Model);
        }

        [Fact]
        public async Task Edit_Get_WhenNotFound_ReturnsNotFound()
        {
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "404" } };
            var response = ApiResponseDto<UserPermissionDto?>.FailureResponse(errors, new ApiMetaDto());
            _userPermissionService.GetUserByIdAsync(999).Returns(response);

            var result = await _controller.Edit(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Get_WhenDataIsNull_ReturnsNotFound()
        {
            var response = ApiResponseDto<UserPermissionDto?>.SuccessResponse(null);
            _userPermissionService.GetUserByIdAsync(1).Returns(response);

            var result = await _controller.Edit(1);

            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        #region Edit POST Tests

        [Fact]
        public async Task Edit_Post_WithInvalidModelState_ReturnsJsonError()
        {
            _controller.ModelState.AddModelError("Username", "Required");
            var model = BuildViewModel(1);

            var result = await _controller.Edit(model);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        [Fact]
        public async Task Edit_Post_WithValidModel_ReturnsSuccessJson()
        {
            var model = BuildViewModel(1);
            var dto = BuildDto();
            var response = ApiResponseDto<UserPermissionDto>.SuccessResponse(dto);

            _mapper.Map<UserPermissionDto>(model).Returns(dto);
            _userPermissionService.UpdateUserAsync(dto).Returns(response);

            var result = await _controller.Edit(model);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
            Assert.Equal("User updated successfully.", value.message);
        }

        [Fact]
        public async Task Edit_Post_WithApiFailure_ReturnsErrorJson()
        {
            var model = BuildViewModel(1);
            var dto = BuildDto();
            var errors = new List<ApiErrorDto> { new() { Message = "Update failed", Code = "400" } };
            var response = ApiResponseDto<UserPermissionDto>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<UserPermissionDto>(model).Returns(dto);
            _userPermissionService.UpdateUserAsync(dto).Returns(response);

            var result = await _controller.Edit(model);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_WithValidUserId_ReturnsSuccessJson()
        {
            var response = ApiResponseDto<bool>.SuccessResponse(true);
            _userPermissionService.DeleteUserAsync(1).Returns(response);

            var result = await _controller.Delete(1);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
            Assert.Equal("User deleted successfully.", value.message);
        }

        [Fact]
        public async Task Delete_WithApiFailure_ReturnsErrorJson()
        {
            var errors = new List<ApiErrorDto> { new() { Message = "Delete failed", Code = "400" } };
            var response = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _userPermissionService.DeleteUserAsync(1).Returns(response);

            var result = await _controller.Delete(1);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        [Fact]
        public async Task Delete_WhenServiceThrows_PropagatesException()
        {
            _userPermissionService.DeleteUserAsync(1).ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.Delete(1));
        }

        #endregion

        #region GetPermissions Tests

        [Fact]
        public async Task GetPermissions_WithValidUserId_ReturnsSuccessJson()
        {
            var dto = new UserPermissionDataDto
            {
                UserId = 1,
                ProfitCentres = ["PC1"],
                Programs = ["P1"],
                Categories = ["C1"],
                TestOwners = ["T1"],
                ProjectGroups = ["PG1"]
            };
            var response = ApiResponseDto<UserPermissionDataDto>.SuccessResponse(dto);
            _userPermissionService.GetUserPermissionsAsync(1).Returns(response);

            var result = await _controller.GetPermissions(1);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
        }

        [Fact]
        public async Task GetPermissions_WithApiFailure_ReturnsErrorJson()
        {
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "404" } };
            var response = ApiResponseDto<UserPermissionDataDto>.FailureResponse(errors, new ApiMetaDto());
            _userPermissionService.GetUserPermissionsAsync(999).Returns(response);

            var result = await _controller.GetPermissions(999);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        #endregion

        #region SavePermissions Tests

        [Fact]
        public async Task SavePermissions_WithValidData_ReturnsSuccessJson()
        {
            var dto = new UserPermissionDataDto
            {
                UserId = 1,
                ProfitCentres = ["PC1"],
                Programs = ["P1"]
            };
            var response = ApiResponseDto<bool>.SuccessResponse(true);
            _userPermissionService.SaveUserPermissionsAsync(1, dto).Returns(response);

            var result = await _controller.SavePermissions(dto);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
            Assert.Equal("Permissions saved successfully.", value.message);
        }

        [Fact]
        public async Task SavePermissions_WithInvalidUserId_ReturnsErrorJson()
        {
            var dto = new UserPermissionDataDto { UserId = 0 };

            var result = await _controller.SavePermissions(dto);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Please select a user first.", value.message);
        }

        [Fact]
        public async Task SavePermissions_WithApiFailure_ReturnsErrorJson()
        {
            var dto = new UserPermissionDataDto { UserId = 1 };
            var errors = new List<ApiErrorDto> { new() { Message = "Save failed", Code = "500" } };
            var response = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _userPermissionService.SaveUserPermissionsAsync(1, dto).Returns(response);

            var result = await _controller.SavePermissions(dto);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        #endregion
    }
}
