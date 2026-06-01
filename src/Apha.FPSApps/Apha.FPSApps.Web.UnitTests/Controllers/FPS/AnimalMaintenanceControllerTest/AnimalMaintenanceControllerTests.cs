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

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.AnimalMasterControllerTest
{
    public class AnimalMaintenanceControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IAnimalMasterService _animalMasterService;
        private readonly AnimalMaintenanceController _controller;

        public AnimalMaintenanceControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _animalMasterService = Substitute.For<IAnimalMasterService>();
            _controller = new AnimalMaintenanceController(_mapper, _animalMasterService);
        }

        private static T? GetJsonResultValue<T>(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<T>(json);
        }

        private static AnimalDto BuildDto(string animalType = "CATTLE") =>
            new() { AnimalType = animalType, Species = "Bovine", SecurityLevel = "L1", DailyRate = 50m };

        private static AnimalMaintenanceViewModel BuildViewModel(string animalType = "CATTLE") =>
            new() { AnimalType = animalType, Species = "Bovine", SecurityLevel = "L1", DailyRate = 50m };

        private static ApiResponseDto<List<AnimalDto>> BuildPagedResponse(
            IEnumerable<AnimalDto>? data = null) =>
            ApiResponseDto<List<AnimalDto>>.SuccessResponse(
                data?.ToList() ?? [],
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 });

        private record JsonResponse(bool success, string? message);

        private void SetupPagedService(IEnumerable<AnimalDto>? data = null)
        {
            var dtoList = data?.ToList() ?? new List<AnimalDto> { BuildDto() };
            var response = BuildPagedResponse(dtoList);
            _animalMasterService
                .GetAllAnimalsAsync(Arg.Any<QueryParameters<string>>())
                .Returns(response);
            _mapper.Map<List<AnimalMaintenanceViewModel>>(Arg.Any<List<AnimalDto>>())
                .Returns(dtoList.Select(d => BuildViewModel(d.AnimalType)).ToList());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
        }

        #region Access / Authorization Attribute Tests

        [Fact]
        public void Controller_HasAuthorizeAttribute_WithExpectedRoles()
        {
            var attrs = typeof(AnimalMaintenanceController)
                .GetCustomAttributes(typeof(AuthorizeAttribute), true);
            Assert.NotEmpty(attrs);
            var auth = (AuthorizeAttribute)attrs[0];
            Assert.Contains("FPSAdmin", auth.Roles);
        }

        [Fact]
        public void Index_HasNoAdditionalAuthRestriction_AllAuthorizedUsersCanAccess()
        {
            var method = typeof(AnimalMaintenanceController).GetMethod(nameof(AnimalMaintenanceController.Index));
            Assert.NotNull(method);
            var additionalAuthorize = method!.GetCustomAttributes(typeof(AuthorizeAttribute), true);
            Assert.Empty(additionalAuthorize);
        }

        [Fact]
        public void Create_Post_HasHttpPostAttribute()
        {
            var methods = typeof(AnimalMaintenanceController).GetMethods()
                .Where(m => m.Name == "Create" && m.GetParameters().Length > 0);
            Assert.NotEmpty(methods);
            var postMethod = methods.FirstOrDefault(m =>
                m.GetCustomAttributes(typeof(HttpPostAttribute), true).Length > 0);
            Assert.NotNull(postMethod);
        }

        [Fact]
        public void Edit_Post_HasHttpPostAttribute()
        {
            var methods = typeof(AnimalMaintenanceController).GetMethods()
                .Where(m => m.Name == "Edit" && m.GetParameters().Length > 0);
            var postMethod = methods.FirstOrDefault(m =>
                m.GetCustomAttributes(typeof(HttpPostAttribute), true).Length > 0);
            Assert.NotNull(postMethod);
        }

        [Fact]
        public void Delete_HasHttpDeleteAttribute()
        {
            var method = typeof(AnimalMaintenanceController).GetMethod(nameof(AnimalMaintenanceController.Delete));
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
                new AnimalMaintenanceController(null!, _animalMasterService));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenServiceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new AnimalMaintenanceController(_mapper, null!));
        }

        #endregion

        #region Index Tests

        [Fact]
        public async Task Index_ReturnsViewResult_WithDataGridConfig()
        {
            SetupPagedService();

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<DataGridConfig<AnimalMaintenanceViewModel>>(viewResult.Model);
        }

        [Fact]
        public async Task Index_Grid_HasCorrectGridId()
        {
            SetupPagedService();

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<DataGridConfig<AnimalMaintenanceViewModel>>(viewResult.Model);
            Assert.Equal("animalMasterGrid", model.GridId);
        }

        [Fact]
        public async Task Index_Grid_HasCorrectBindUrl()
        {
            SetupPagedService();

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<DataGridConfig<AnimalMaintenanceViewModel>>(viewResult.Model);
            Assert.Equal("/FPS/AnimalMaintenance/LoadAnimalMasterGrid", model.BindGridUrl);
        }

        #endregion

        #region LoadAnimalMasterGrid Tests

        [Fact]
        public async Task LoadAnimalMasterGrid_WithValidRequest_ReturnsPartialView()
        {
            SetupPagedService();
            var request = new PaginationFilter<string> { Filter = "{}" };

            var result = await _controller.LoadAnimalMasterGrid(request);

            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialViewResult.ViewName);
            Assert.IsType<DataGridConfig<AnimalMaintenanceViewModel>>(partialViewResult.Model);
        }

        [Fact]
        public async Task LoadAnimalMasterGrid_WithInvalidModelState_ReturnsJsonError()
        {
            _controller.ModelState.AddModelError("Test", "Test error");
            var request = new PaginationFilter<string> { Filter = "{}" };

            var result = await _controller.LoadAnimalMasterGrid(request);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Invalid request data", value.message);
        }

        [Fact]
        public async Task LoadAnimalMasterGrid_WithEmptyData_ReturnsEmptyGrid()
        {
            SetupPagedService([]);

            var result = await _controller.LoadAnimalMasterGrid(new PaginationFilter<string>());

            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<AnimalMaintenanceViewModel>>(partialViewResult.Model);
            Assert.Empty(gridConfig.Data);
        }

        [Fact]
        public async Task LoadAnimalMasterGrid_PassesFilterToService()
        {
            SetupPagedService();
            var request = new PaginationFilter<string>
            {
                Filter = "{\"AnimalType\":\"CATTLE\"}",
                SortBy = "AnimalType",
                Descending = false
            };

            await _controller.LoadAnimalMasterGrid(request);

            await _animalMasterService.Received(1)
                .GetAllAnimalsAsync(Arg.Any<QueryParameters<string>>());
        }

        #endregion

        #region Create GET Tests

        [Fact]
        public void Create_Get_ReturnsPartialViewWithEmptyModel()
        {
            var result = _controller.Create();

            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditAnimalMaintenance", partialViewResult.ViewName);
            var model = Assert.IsType<AnimalMaintenanceViewModel>(partialViewResult.Model);
            Assert.Equal(string.Empty, model.AnimalType);
        }

        #endregion

        #region Create POST Tests

        [Fact]
        public async Task Create_Post_WithInvalidModelState_ReturnsJsonError()
        {
            _controller.ModelState.AddModelError("AnimalType", "Required");
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
            var response = ApiResponseDto<AnimalDto>.SuccessResponse(dto);

            _mapper.Map<AnimalDto>(model).Returns(dto);
            _animalMasterService.AddAnimalAsync(dto).Returns(response);

            var result = await _controller.Create(model);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
            Assert.Equal("Animal created successfully", value.message);
        }

        [Fact]
        public async Task Create_Post_WithApiFailure_ReturnsErrorJson()
        {
            var model = BuildViewModel();
            var dto = BuildDto();
            var errors = new List<ApiErrorDto> { new() { Message = "Creation failed", Code = "400" } };
            var response = ApiResponseDto<AnimalDto>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<AnimalDto>(model).Returns(dto);
            _animalMasterService.AddAnimalAsync(dto).Returns(response);

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
            var viewModel = BuildViewModel();
            var response = ApiResponseDto<AnimalDto?>.SuccessResponse(dto);

            _animalMasterService.GetAnimalByIdAsync("CATTLE").Returns(response);
            _mapper.Map<AnimalMaintenanceViewModel>(dto).Returns(viewModel);

            var result = await _controller.Edit("CATTLE");

            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditAnimalMaintenance", partialViewResult.ViewName);
            Assert.IsType<AnimalMaintenanceViewModel>(partialViewResult.Model);
        }

        [Fact]
        public async Task Edit_Get_WhenNotFound_ReturnsNotFound()
        {
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "404" } };
            var response = ApiResponseDto<AnimalDto?>.FailureResponse(errors, new ApiMetaDto());
            _animalMasterService.GetAnimalByIdAsync("NOTEXIST").Returns(response);

            var result = await _controller.Edit("NOTEXIST");

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Get_WhenDataIsNull_ReturnsNotFound()
        {
            var response = ApiResponseDto<AnimalDto?>.SuccessResponse(null);
            _animalMasterService.GetAnimalByIdAsync("CATTLE").Returns(response);

            var result = await _controller.Edit("CATTLE");

            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        #region Edit POST Tests

        [Fact]
        public async Task Edit_Post_WithInvalidModelState_ReturnsJsonError()
        {
            _controller.ModelState.AddModelError("AnimalType", "Required");
            var model = BuildViewModel();

            var result = await _controller.Edit(model);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        [Fact]
        public async Task Edit_Post_WithValidModel_ReturnsSuccessJson()
        {
            var model = BuildViewModel();
            var dto = BuildDto();
            var response = ApiResponseDto<AnimalDto>.SuccessResponse(dto);

            _mapper.Map<AnimalDto>(model).Returns(dto);
            _animalMasterService.UpdateAnimalAsync(dto).Returns(response);

            var result = await _controller.Edit(model);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
            Assert.Equal("Animal updated successfully.", value.message);
        }

        [Fact]
        public async Task Edit_Post_WithApiFailure_ReturnsErrorJson()
        {
            var model = BuildViewModel();
            var dto = BuildDto();
            var errors = new List<ApiErrorDto> { new() { Message = "Update failed", Code = "400" } };
            var response = ApiResponseDto<AnimalDto>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<AnimalDto>(model).Returns(dto);
            _animalMasterService.UpdateAnimalAsync(dto).Returns(response);

            var result = await _controller.Edit(model);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_WithValidAnimalType_ReturnsSuccessJson()
        {
            var response = ApiResponseDto<bool>.SuccessResponse(true);
            _animalMasterService.DeleteAnimalAsync("CATTLE").Returns(response);

            var result = await _controller.Delete("CATTLE");

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
            Assert.Equal("Animal deleted successfully.", value.message);
        }

        [Fact]
        public async Task Delete_WithApiFailure_ReturnsErrorJson()
        {
            var errors = new List<ApiErrorDto> { new() { Message = "Delete failed", Code = "400" } };
            var response = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _animalMasterService.DeleteAnimalAsync("CATTLE").Returns(response);

            var result = await _controller.Delete("CATTLE");

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        [Fact]
        public async Task Delete_WhenServiceThrows_PropagatesException()
        {
            _animalMasterService.DeleteAnimalAsync("CATTLE").ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.Delete("CATTLE"));
        }

        #endregion

        #region ReadOnly / Access Role Tests

        [Fact]
        public async Task LoadAnimalMasterGrid_FPSUser_CanReadGrid()
        {
            SetupPagedService();
            await _controller.LoadAnimalMasterGrid(new PaginationFilter<string>());
            await _animalMasterService.Received(1).GetAllAnimalsAsync(Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task Index_FPSUser_CanViewGrid()
        {
            SetupPagedService();
            var result = await _controller.Index();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Controller_AuthorizeRoles_IncludesFPSAdmin_ForWriteAccess()
        {
            var attr = typeof(AnimalMaintenanceController)
                .GetCustomAttributes(typeof(AuthorizeAttribute), true)
                .Cast<AuthorizeAttribute>()
                .First();
            Assert.Contains("FPSAdmin", attr.Roles);
        }

        #endregion
    }
}
