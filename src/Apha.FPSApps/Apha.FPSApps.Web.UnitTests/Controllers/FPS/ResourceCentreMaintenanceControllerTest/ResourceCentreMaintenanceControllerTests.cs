using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Handler;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.ResourceCentreMaintenanceControllerTest
{
    public class ResourceCentreMaintenanceControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IProfitCentreService _profitCentreService;
        private readonly IDivisionService _divisionService;
        private readonly IEmployeeService _employeeService;
        private readonly ResourceCentreMaintenanceController _controller;

        public ResourceCentreMaintenanceControllerTests()
        {
            _mapper              = Substitute.For<IMapper>();
            _profitCentreService = Substitute.For<IProfitCentreService>();
            _divisionService     = Substitute.For<IDivisionService>();
            _employeeService     = Substitute.For<IEmployeeService>();
            _controller = new ResourceCentreMaintenanceController(
                _mapper, _profitCentreService, _divisionService, _employeeService);
        }

        private class JsonResponse
        {
            public bool success { get; set; }
            public string? message { get; set; }
            public object? data { get; set; }
            public object? errors { get; set; }
        }

        private static T? GetJsonResultValue<T>(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<T>(json);
        }

        private static ResourceCentreMaintenanceItem BuildItem(string id = "PC01") =>
            new() { ProfitCentreId = id, ProfitCentreName = "Centre One", Division = "DIV1" };

        private static ProfitCentreDto BuildDto(string id = "PC01") =>
            new() { ProfitCentreId = id, ProfitCentreName = "Centre One", Division = "DIV1" };

        private static ApiResponseDto<List<ProfitCentreDto>> BuildPagedResponse(
            IEnumerable<ProfitCentreDto>? data = null) =>
            ApiResponseDto<List<ProfitCentreDto>>.SuccessResponse(
                data?.ToList() ?? [], new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 });

        #region Constructor Tests

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenMapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ResourceCentreMaintenanceController(null!, _profitCentreService, _divisionService, _employeeService));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenProfitCentreServiceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ResourceCentreMaintenanceController(_mapper, null!, _divisionService, _employeeService));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenDivisionServiceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ResourceCentreMaintenanceController(_mapper, _profitCentreService, null!, _employeeService));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenEmployeeServiceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ResourceCentreMaintenanceController(_mapper, _profitCentreService, _divisionService, null!));
        }

        #endregion

        #region Index Tests

        [Fact]
        public async Task Index_ReturnsViewResult_WithResourceCentreViewModel()
        {
            // Arrange
            var dtos         = new List<ProfitCentreDto> { BuildDto() };
            var pagedResponse = BuildPagedResponse(dtos);

            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _profitCentreService.GetAllProfitCentresPagedAsync(Arg.Any<QueryParameters<string>>())
                .Returns(pagedResponse);
            _mapper.Map<List<ResourceCentreMaintenanceItem>>(Arg.Any<List<ProfitCentreDto>>())
                .Returns(new List<ResourceCentreMaintenanceItem> { BuildItem() });
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ResourceCentreMaintenanceViewModel>(viewResult.Model);
            Assert.NotNull(model.ResourceCentreGrid);
            Assert.Equal("resourceCentreGrid", model.ResourceCentreGrid.GridId);
        }

        [Fact]
        public async Task Index_GridHasNoDefaultSortColumn()
        {
            // Arrange
            var pagedResponse = BuildPagedResponse(new List<ProfitCentreDto> { BuildDto() });
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _profitCentreService.GetAllProfitCentresPagedAsync(Arg.Any<QueryParameters<string>>())
                .Returns(pagedResponse);
            _mapper.Map<List<ResourceCentreMaintenanceItem>>(Arg.Any<List<ProfitCentreDto>>())
                .Returns(new List<ResourceCentreMaintenanceItem>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model      = Assert.IsType<ResourceCentreMaintenanceViewModel>(viewResult.Model);
            Assert.True(string.IsNullOrEmpty(model.ResourceCentreGrid.Pagination.SortColumn),
                "No sort column should be applied on initial page load.");
        }

        [Fact]
        public async Task Index_GridHasNoDefaultSortDirection()
        {
            // Arrange
            var pagedResponse = BuildPagedResponse(new List<ProfitCentreDto> { BuildDto() });
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _profitCentreService.GetAllProfitCentresPagedAsync(Arg.Any<QueryParameters<string>>())
                .Returns(pagedResponse);
            _mapper.Map<List<ResourceCentreMaintenanceItem>>(Arg.Any<List<ProfitCentreDto>>())
                .Returns(new List<ResourceCentreMaintenanceItem>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model      = Assert.IsType<ResourceCentreMaintenanceViewModel>(viewResult.Model);
            Assert.False(model.ResourceCentreGrid.Pagination.SortDirection,
                "Sort direction should default to ascending (false) on initial page load.");
        }

        #endregion

        #region LoadResourceCentreGrid Tests

        [Fact]
        public async Task LoadResourceCentreGrid_WithValidRequest_ReturnsPartialView()
        {
            // Arrange
            var request      = new PaginationFilter<string> { Filter = "{}" };
            var pagedResponse = BuildPagedResponse([BuildDto()]);

            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _profitCentreService.GetAllProfitCentresPagedAsync(Arg.Any<QueryParameters<string>>())
                .Returns(pagedResponse);
            _mapper.Map<List<ResourceCentreMaintenanceItem>>(Arg.Any<List<ProfitCentreDto>>())
                .Returns(new List<ResourceCentreMaintenanceItem> { BuildItem() });
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadResourceCentreGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialViewResult.ViewName);
            Assert.IsType<DataGridConfig<ResourceCentreMaintenanceItem>>(partialViewResult.Model);
        }

        [Fact]
        public async Task LoadResourceCentreGrid_WithInvalidModelState_ReturnsJsonError()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _controller.ModelState.AddModelError("Test", "Test error");

            // Act
            var result = await _controller.LoadResourceCentreGrid(request);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Invalid request data", value.message);
        }

        [Fact]
        public async Task LoadResourceCentreGrid_WithEmptyData_ReturnsEmptyGrid()
        {
            // Arrange
            var request      = new PaginationFilter<string> { Filter = "{}" };
            var pagedResponse = BuildPagedResponse([]);

            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _profitCentreService.GetAllProfitCentresPagedAsync(Arg.Any<QueryParameters<string>>())
                .Returns(pagedResponse);
            _mapper.Map<List<ResourceCentreMaintenanceItem>>(Arg.Any<List<ProfitCentreDto>>())
                .Returns([]);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadResourceCentreGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ResourceCentreMaintenanceItem>>(partialViewResult.Model);
            Assert.Empty(gridConfig.Data);
        }

        #endregion

        #region Create GET Tests

        [Fact]
        public async Task Create_Get_ReturnsPartialViewWithEmptyModel()
        {
            // Act
            var result = await _controller.Create();

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditMaintResourceCentre", partialViewResult.ViewName);
            Assert.IsType<ResourceCentreMaintenanceItem>(partialViewResult.Model);
        }

        #endregion

        #region Create POST Tests

        [Fact]
        public async Task Create_Post_WithInvalidModelState_ReturnsJsonError()
        {
            // Arrange
            var item = BuildItem();
            _controller.ModelState.AddModelError("ProfitCentreId", "Required");

            // Act
            var result = await _controller.Create(item);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Please correct the errors below.", value.message);
        }

        [Fact]
        public async Task Create_Post_WithValidViewModel_ReturnsSuccessJson()
        {
            // Arrange
            var item        = BuildItem("PC01");
            var dto         = BuildDto("PC01");
            var apiResponse = ApiResponseDto<ProfitCentreDto>.SuccessResponse(dto);

            _mapper.Map<ProfitCentreDto>(item).Returns(dto);
            _profitCentreService.CreateProfitCentreAsync(dto).Returns(apiResponse);

            // Act
            var result = await _controller.Create(item);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
            Assert.Equal("Resource centre created successfully", value.message);
        }

        [Fact]
        public async Task Create_Post_WhenServiceFails_ReturnsJsonError()
        {
            // Arrange
            var item        = BuildItem();
            var dto         = BuildDto();
            var errors      = new List<ApiErrorDto> { new() { Message = "Already exists", Code = "CONFLICT" } };
            var apiResponse = ApiResponseDto<ProfitCentreDto>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<ProfitCentreDto>(item).Returns(dto);
            _profitCentreService.CreateProfitCentreAsync(dto).Returns(apiResponse);

            // Act
            var result = await _controller.Create(item);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Already exists", value.message);
        }

        #endregion

        #region Edit GET Tests

        [Fact]
        public async Task Edit_Get_WithEmptyId_ReturnsJsonError()
        {
            // Act
            var result = await _controller.Edit("");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Resource centre ID is required", value.message);
        }

        [Fact]
        public async Task Edit_Get_WithWhiteSpaceId_ReturnsJsonError()
        {
            // Act
            var result = await _controller.Edit("   ");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        [Fact]
        public async Task Edit_Get_WithValidId_ReturnsPartialView()
        {
            // Arrange
            var dto         = BuildDto("PC01");
            var item        = BuildItem("PC01");
            var apiResponse = ApiResponseDto<ProfitCentreDto>.SuccessResponse(dto);

            _profitCentreService.GetProfitCentreByIdAsync("PC01").Returns(apiResponse);
            _mapper.Map<ResourceCentreMaintenanceItem>(dto).Returns(item);

            // Act
            var result = await _controller.Edit("PC01");

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditMaintResourceCentre", partialViewResult.ViewName);
            var model = Assert.IsType<ResourceCentreMaintenanceItem>(partialViewResult.Model);
            Assert.Equal("PC01", model.ProfitCentreId);
        }

        [Fact]
        public async Task Edit_Get_WhenNotFound_ReturnsJsonError()
        {
            // Arrange
            var errors      = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var apiResponse = ApiResponseDto<ProfitCentreDto>.FailureResponse(errors, new ApiMetaDto());
            _profitCentreService.GetProfitCentreByIdAsync("NOTEXIST").Returns(apiResponse);

            // Act
            var result = await _controller.Edit("NOTEXIST");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        #endregion

        #region Edit POST Tests

        [Fact]
        public async Task Edit_Post_WithInvalidModelState_ReturnsJsonError()
        {
            // Arrange
            var item = BuildItem();
            _controller.ModelState.AddModelError("ProfitCentreId", "Required");

            // Act
            var result = await _controller.Edit(item);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Please correct the errors below.", value.message);
        }

        [Fact]
        public async Task Edit_Post_WithValidViewModel_ReturnsSuccessJson()
        {
            // Arrange
            var item        = BuildItem("PC01");
            var dto         = BuildDto("PC01");
            var apiResponse = ApiResponseDto<ProfitCentreDto>.SuccessResponse(dto);

            _mapper.Map<ProfitCentreDto>(item).Returns(dto);
            _profitCentreService.UpdateProfitCentreAsync(Arg.Any<string>(), dto).Returns(apiResponse);

            // Act
            var result = await _controller.Edit(item);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
            Assert.Equal("Resource centre updated successfully", value.message);
        }

        [Fact]
        public async Task Edit_Post_WhenServiceFails_ReturnsJsonError()
        {
            // Arrange
            var item        = BuildItem();
            var dto         = BuildDto();
            var errors      = new List<ApiErrorDto> { new() { Message = "Update failed", Code = "ERROR" } };
            var apiResponse = ApiResponseDto<ProfitCentreDto>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<ProfitCentreDto>(item).Returns(dto);
            _profitCentreService.UpdateProfitCentreAsync(Arg.Any<string>(), dto).Returns(apiResponse);

            // Act
            var result = await _controller.Edit(item);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Update failed", value.message);
        }

        [Fact]
        public async Task Edit_Post_UsesOriginalProfitCentreId_WhenProvided()
        {
            // Arrange
            var item        = BuildItem("PC01");
            var dto         = BuildDto("PC01");
            var apiResponse = ApiResponseDto<ProfitCentreDto>.SuccessResponse(dto);
            string? capturedId = null;

            _mapper.Map<ProfitCentreDto>(item).Returns(dto);
            _profitCentreService
                .UpdateProfitCentreAsync(Arg.Do<string>(id => capturedId = id), dto)
                .Returns(apiResponse);

            // Act
            await _controller.Edit(item, originalProfitCentreId: "ORIGINAL01");

            // Assert
            Assert.Equal("ORIGINAL01", capturedId);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_WithEmptyId_ReturnsJsonError()
        {
            // Act
            var result = await _controller.Delete("");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Resource centre ID is required", value.message);
        }

        [Fact]
        public async Task Delete_WithValidId_ReturnsSuccessJson()
        {
            // Arrange
            var apiResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _profitCentreService.DeleteProfitCentreAsync("PC01").Returns(apiResponse);

            // Act
            var result = await _controller.Delete("PC01");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
            Assert.Equal("Resource centre deleted successfully", value.message);
        }

        [Fact]
        public async Task Delete_WhenServiceFails_ReturnsJsonError()
        {
            // Arrange
            var errors      = new List<ApiErrorDto> { new() { Message = "In use", Code = "IN_USE" } };
            var apiResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _profitCentreService.DeleteProfitCentreAsync("PC01").Returns(apiResponse);

            // Act
            var result = await _controller.Delete("PC01");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("In use", value.message);
        }

        [Fact]
        public async Task Delete_WhenDeleteReturnsFalse_ReturnsDefaultError()
        {
            // Arrange
            var apiResponse = ApiResponseDto<bool>.SuccessResponse(false);
            _profitCentreService.DeleteProfitCentreAsync("PC01").Returns(apiResponse);

            // Act
            var result = await _controller.Delete("PC01");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        [Fact]
        public async Task Delete_CallsDeleteAsync_Once()
        {
            // Arrange
            var apiResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _profitCentreService.DeleteProfitCentreAsync("PC01").Returns(apiResponse);

            // Act
            await _controller.Delete("PC01");

            // Assert
            await _profitCentreService.Received(1).DeleteProfitCentreAsync("PC01");
        }

        #endregion

        #region GetDistinctDivisions Tests

        [Fact]
        public async Task GetDistinctDivisions_ReturnsSuccessJsonWithDivisions()
        {
            // Arrange
            var divisions = new List<DivisionDto>
            {
                new() { DivisionId = 1, DivName = "VSD" },
                new() { DivisionId = 2, DivName = "BSD" }
            };
            var apiResponse = ApiResponseDto<IEnumerable<DivisionDto>>.SuccessResponse(divisions);
            _divisionService.GetAllDivisionsAsync().Returns(apiResponse);

            // Act
            var result = await _controller.GetDistinctDivisions();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
        }

        [Fact]
        public async Task GetDistinctDivisions_WhenServiceFails_ReturnsJsonError()
        {
            // Arrange
            var errors      = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } };
            var apiResponse = ApiResponseDto<IEnumerable<DivisionDto>>.FailureResponse(errors, new ApiMetaDto());
            _divisionService.GetAllDivisionsAsync().Returns(apiResponse);

            // Act
            var result = await _controller.GetDistinctDivisions();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Failed to load divisions", value.message);
        }

        [Fact]
        public async Task GetDistinctDivisions_WhenExceptionThrown_ReturnsJsonError()
        {
            // Arrange
            _divisionService.GetAllDivisionsAsync()
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _controller.GetDistinctDivisions();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        #endregion

        #region GetManagers Tests

        [Fact]
        public async Task GetManagers_ReturnsSuccessJsonWithManagers()
        {
            // Arrange
            var managers = new List<ManagerDto>
            {
                new() { Name = "Alice Smith" },
                new() { Name = "Bob Jones" }
            };
            var apiResponse = ApiResponseDto<List<ManagerDto>>.SuccessResponse(managers);
            _employeeService.GetAllManagersAsync().Returns(apiResponse);

            // Act
            var result = await _controller.GetManagers();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
        }

        [Fact]
        public async Task GetManagers_WhenServiceFails_ReturnsJsonError()
        {
            // Arrange
            var errors      = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } };
            var apiResponse = ApiResponseDto<List<ManagerDto>>.FailureResponse(errors, new ApiMetaDto());
            _employeeService.GetAllManagersAsync().Returns(apiResponse);

            // Act
            var result = await _controller.GetManagers();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Failed to load managers", value.message);
        }

        #endregion
    }
}
