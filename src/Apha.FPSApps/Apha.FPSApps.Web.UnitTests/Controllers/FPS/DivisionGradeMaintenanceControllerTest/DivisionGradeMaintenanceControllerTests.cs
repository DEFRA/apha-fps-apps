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

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.DivisionGradeMaintenanceControllerTest
{
    public class DivisionGradeMaintenanceControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IDivisionGradeMaintenanceService _maintDGService;
        private readonly IDivisionService _divisionService;
        private readonly IFpsYearContext _fpsYearContext;
        private readonly DivisionGradeMaintenanceController _controller;

        public DivisionGradeMaintenanceControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _maintDGService = Substitute.For<IDivisionGradeMaintenanceService>();
            _divisionService = Substitute.For<IDivisionService>();
            _fpsYearContext = Substitute.For<IFpsYearContext>();
            _fpsYearContext.Year.Returns(2025);
            _controller = new DivisionGradeMaintenanceController(
                _mapper, _maintDGService, _divisionService, _fpsYearContext);
        }

        // Helper to extract JsonResult value
        private static T? GetJsonResultValue<T>(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<T>(json);
        }

        private static DivisionGradeMaintenanceDto BuildDto(string code = "A-VSD") =>
            new() { DivisionGradeCode = code, GradeCode = "A", Division = "VSD", ChargeRate = 100m };

        private static ApiResponseDto<List<DivisionGradeMaintenanceDto>> BuildPagedResponse(
            IEnumerable<DivisionGradeMaintenanceDto>? data = null) =>
            ApiResponseDto<List<DivisionGradeMaintenanceDto>>.SuccessResponse(
                data?.ToList() ?? [], new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 });

        private void SetupGradeAndDivisionDropdowns()
        {
            _maintDGService.GetAllGradeCodesAsync()
                .Returns(ApiResponseDto<List<string>>.SuccessResponse(["A", "B"]));
            _divisionService.GetAllDivisionsAsync()
                .Returns(ApiResponseDto<IEnumerable<DivisionDto>>.SuccessResponse(
                    new List<DivisionDto> { new() { DivName = "VSD" } }));
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenMapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new DivisionGradeMaintenanceController(null!, _maintDGService, _divisionService, _fpsYearContext));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenServiceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new DivisionGradeMaintenanceController(_mapper, null!, _divisionService, _fpsYearContext));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenDivisionServiceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new DivisionGradeMaintenanceController(_mapper, _maintDGService, null!, _fpsYearContext));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenYearContextIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new DivisionGradeMaintenanceController(_mapper, _maintDGService, _divisionService, null!));
        }

        #endregion

        #region Index Tests

        [Fact]
        public async Task Index_ReturnsViewResult_WithDivisionGradeMaintenanceViewModel()
        {
            // Arrange
            var dtos = new List<DivisionGradeMaintenanceDto> { BuildDto() };
            var pagedResponse = BuildPagedResponse(dtos);
            SetupGradeAndDivisionDropdowns();

            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _maintDGService.GetAllPagedAsync(Arg.Any<QueryParameters<string>>()).Returns(pagedResponse);
            _mapper.Map<List<DivisionGradeMaintenanceItem>>(Arg.Any<List<DivisionGradeMaintenanceDto>>())
                .Returns(new List<DivisionGradeMaintenanceItem> { new() { DivisionGradeCode = "A-VSD" } });
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.Index(2025);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<DivisionGradeMaintenanceViewModel>(viewResult.Model);
            Assert.NotNull(model.DivisionGradeGrid);
            Assert.Equal("divisionGradeGrid", model.DivisionGradeGrid.GridId);
        }

        #endregion

        #region LoadDivisionGradeGrid Tests

        [Fact]
        public async Task LoadDivisionGradeGrid_WithValidRequest_ReturnsPartialView()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            var pagedResponse = BuildPagedResponse([BuildDto()]);

            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _maintDGService.GetAllPagedAsync(Arg.Any<QueryParameters<string>>()).Returns(pagedResponse);
            _mapper.Map<List<DivisionGradeMaintenanceItem>>(Arg.Any<List<DivisionGradeMaintenanceDto>>())
                .Returns(new List<DivisionGradeMaintenanceItem> { new() { DivisionGradeCode = "A-VSD" } });
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadDivisionGradeGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialViewResult.ViewName);
            Assert.IsType<DataGridConfig<DivisionGradeMaintenanceItem>>(partialViewResult.Model);
        }

        [Fact]
        public async Task LoadDivisionGradeGrid_WithInvalidModelState_ReturnsJsonError()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _controller.ModelState.AddModelError("Test", "Test error");

            // Act
            var result = await _controller.LoadDivisionGradeGrid(request);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Invalid request data", value.message);
        }

        [Fact]
        public async Task LoadDivisionGradeGrid_WithEmptyData_ReturnsEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            var pagedResponse = BuildPagedResponse([]);

            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _maintDGService.GetAllPagedAsync(Arg.Any<QueryParameters<string>>()).Returns(pagedResponse);
            _mapper.Map<List<DivisionGradeMaintenanceItem>>(Arg.Any<List<DivisionGradeMaintenanceDto>>())
                .Returns([]);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadDivisionGradeGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<DivisionGradeMaintenanceItem>>(partialViewResult.Model);
            Assert.Empty(gridConfig.Data);
        }

        #endregion

        #region Create GET Tests

        [Fact]
        public async Task Create_Get_ReturnsPartialViewWithEmptyModel()
        {
            // Arrange
            SetupGradeAndDivisionDropdowns();

            // Act
            var result = await _controller.Create();

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditDivisionGradeMaintenance", partialViewResult.ViewName);
            Assert.IsType<DivisionGradeMaintenanceItem>(partialViewResult.Model);
        }

        #endregion

        #region Create POST Tests

        [Fact]
        public async Task Create_Post_WithNullDto_ReturnsJsonError()
        {
            // Act
            var result = await _controller.Create(null!);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Invalid data", value.message);
        }

        [Fact]
        public async Task Create_Post_WithInvalidModelState_ReturnsJsonError()
        {
            // Arrange
            var dto = BuildDto();
            _controller.ModelState.AddModelError("DivisionGradeCode", "Required");

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Please correct the errors below.", value.message);
        }

        [Fact]
        public async Task Create_Post_WithValidDto_ReturnsSuccessJson()
        {
            // Arrange
            var dto = BuildDto("A-VSD");
            var apiResponse = ApiResponseDto<DivisionGradeMaintenanceDto>.SuccessResponse(dto);
            _maintDGService.CreateAsync(dto).Returns(apiResponse);

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
            Assert.Equal("Division grade created successfully", value.message);
        }

        [Fact]
        public async Task Create_Post_WhenServiceFails_ReturnsJsonError()
        {
            // Arrange
            var dto = BuildDto();
            var errors = new List<ApiErrorDto> { new() { Message = "Creation failed", Code = "ERROR" } };
            var apiResponse = ApiResponseDto<DivisionGradeMaintenanceDto>.FailureResponse(errors, new ApiMetaDto());
            _maintDGService.CreateAsync(dto).Returns(apiResponse);

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Creation failed", value.message);
        }

        [Fact]
        public async Task Create_Post_NegativeRates_AreConvertedToPositive()
        {
            // Arrange
            var dto = new DivisionGradeMaintenanceDto
            {
                DivisionGradeCode = "A-VSD", GradeCode = "A", Division = "VSD",
                ChargeRate = -100m, DirectRate = -90m, PayRate = -80m, Npr = -10m, Ohr = -5m
            };
            DivisionGradeMaintenanceDto? capturedDto = null;
            var apiResponse = ApiResponseDto<DivisionGradeMaintenanceDto>.SuccessResponse(dto);
            _maintDGService.CreateAsync(Arg.Do<DivisionGradeMaintenanceDto>(d => capturedDto = d))
                .Returns(apiResponse);

            // Act
            await _controller.Create(dto);

            // Assert
            Assert.NotNull(capturedDto);
            Assert.Equal(100m, capturedDto!.ChargeRate);
            Assert.Equal(90m, capturedDto.DirectRate);
            Assert.Equal(80m, capturedDto.PayRate);
            Assert.Equal(10m, capturedDto.Npr);
            Assert.Equal(5m, capturedDto.Ohr);
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
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Division grade code is required", value.message);
        }

        [Fact]
        public async Task Edit_Get_WithWhiteSpaceId_ReturnsJsonError()
        {
            // Act
            var result = await _controller.Edit("   ");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        [Fact]
        public async Task Edit_Get_WithValidId_ReturnsPartialView()
        {
            // Arrange
            var dto = BuildDto("A-VSD");
            var item = new DivisionGradeMaintenanceItem { DivisionGradeCode = "A-VSD" };
            var apiResponse = ApiResponseDto<DivisionGradeMaintenanceDto>.SuccessResponse(dto);

            _maintDGService.GetByIdAsync("A-VSD").Returns(apiResponse);
            _mapper.Map<DivisionGradeMaintenanceItem>(dto).Returns(item);
            SetupGradeAndDivisionDropdowns();

            // Act
            var result = await _controller.Edit("A-VSD");

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditDivisionGradeMaintenance", partialViewResult.ViewName);
            var model = Assert.IsType<DivisionGradeMaintenanceItem>(partialViewResult.Model);
            Assert.Equal("A-VSD", model.DivisionGradeCode);
        }

        [Fact]
        public async Task Edit_Get_WhenNotFound_ReturnsJsonError()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var apiResponse = ApiResponseDto<DivisionGradeMaintenanceDto>.FailureResponse(errors, new ApiMetaDto());
            _maintDGService.GetByIdAsync("NOTEXIST").Returns(apiResponse);

            // Act
            var result = await _controller.Edit("NOTEXIST");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        #endregion

        #region Edit POST Tests

        [Fact]
        public async Task Edit_Post_WithNullDto_ReturnsJsonError()
        {
            // Act
            var result = await _controller.Edit("A-VSD", null!);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Invalid data", value.message);
        }

        [Fact]
        public async Task Edit_Post_WithInvalidModelState_ReturnsJsonError()
        {
            // Arrange
            var dto = BuildDto();
            _controller.ModelState.AddModelError("DivisionGradeCode", "Required");

            // Act
            var result = await _controller.Edit("A-VSD", dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Please correct the errors below.", value.message);
        }

        [Fact]
        public async Task Edit_Post_WithValidDto_ReturnsSuccessJson()
        {
            // Arrange
            var dto = BuildDto("A-VSD");
            var apiResponse = ApiResponseDto<DivisionGradeMaintenanceDto>.SuccessResponse(dto);
            _maintDGService.UpdateAsync("A-VSD", dto).Returns(apiResponse);

            // Act
            var result = await _controller.Edit("A-VSD", dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
            Assert.Equal("Division grade updated successfully", value.message);
        }

        [Fact]
        public async Task Edit_Post_WhenServiceFails_ReturnsJsonError()
        {
            // Arrange
            var dto = BuildDto();
            var errors = new List<ApiErrorDto> { new() { Message = "Update failed", Code = "ERROR" } };
            var apiResponse = ApiResponseDto<DivisionGradeMaintenanceDto>.FailureResponse(errors, new ApiMetaDto());
            _maintDGService.UpdateAsync("A-VSD", dto).Returns(apiResponse);

            // Act
            var result = await _controller.Edit("A-VSD", dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Update failed", value.message);
        }

        [Fact]
        public async Task Edit_Post_NegativeRates_AreConvertedToPositive()
        {
            // Arrange
            var dto = new DivisionGradeMaintenanceDto
            {
                DivisionGradeCode = "A-VSD", GradeCode = "A", Division = "VSD",
                ChargeRate = -100m, DirectRate = -90m, PayRate = -80m, Npr = -10m, Ohr = -5m
            };
            DivisionGradeMaintenanceDto? capturedDto = null;
            var apiResponse = ApiResponseDto<DivisionGradeMaintenanceDto>.SuccessResponse(dto);
            _maintDGService.UpdateAsync(Arg.Any<string>(), Arg.Do<DivisionGradeMaintenanceDto>(d => capturedDto = d))
                .Returns(apiResponse);

            // Act
            await _controller.Edit("A-VSD", dto);

            // Assert
            Assert.NotNull(capturedDto);
            Assert.Equal(100m, capturedDto!.ChargeRate);
            Assert.Equal(90m, capturedDto.DirectRate);
            Assert.Equal(80m, capturedDto.PayRate);
            Assert.Equal(10m, capturedDto.Npr);
            Assert.Equal(5m, capturedDto.Ohr);
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
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Division grade code is required", value.message);
        }

        [Fact]
        public async Task Delete_WithValidId_ReturnsSuccessJson()
        {
            // Arrange
            var apiResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _maintDGService.DeleteAsync("A-VSD").Returns(apiResponse);

            // Act
            var result = await _controller.Delete("A-VSD");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
            Assert.Equal("Division grade deleted successfully", value.message);
        }

        [Fact]
        public async Task Delete_WhenServiceFails_ReturnsJsonError()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "In use", Code = "IN_USE" } };
            var apiResponse = ApiResponseDto<bool>.SuccessResponse(false);
            _maintDGService.DeleteAsync("A-VSD").Returns(apiResponse);

            // Act
            var result = await _controller.Delete("A-VSD");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Unable to delete the division grade as it may be in use.", value.message);
        }

        [Fact]
        public async Task Delete_CallsDeleteAsync_Once()
        {
            // Arrange
            var apiResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _maintDGService.DeleteAsync("A-VSD").Returns(apiResponse);

            // Act
            await _controller.Delete("A-VSD");

            // Assert
            await _maintDGService.Received(1).DeleteAsync("A-VSD");
        }

        #endregion

        #region GetDistinctGradeCodes Tests

        [Fact]
        public async Task GetDistinctGradeCodes_ReturnsSuccessJsonWithGradeCodes()
        {
            // Arrange
            var gradeCodes = new List<string> { "A", "B", "C" };
            var apiResponse = ApiResponseDto<List<string>>.SuccessResponse(gradeCodes);
            _maintDGService.GetAllGradeCodesAsync().Returns(apiResponse);

            // Act
            var result = await _controller.GetDistinctGradeCodes();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
        }

        [Fact]
        public async Task GetDistinctGradeCodes_WhenServiceFails_ReturnsJsonError()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } };
            var apiResponse = ApiResponseDto<List<string>>.FailureResponse(errors, new ApiMetaDto());
            _maintDGService.GetAllGradeCodesAsync().Returns(apiResponse);

            // Act
            var result = await _controller.GetDistinctGradeCodes();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Failed to load grade codes", value.message);
        }

        [Fact]
        public async Task GetDistinctGradeCodes_WhenExceptionThrown_ReturnsJsonError()
        {
            // Arrange
            _maintDGService.GetAllGradeCodesAsync()
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _controller.GetDistinctGradeCodes();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Contains("Unexpected error", value.message);
        }

        #endregion

        #region GetDistinctDivisions Tests

        [Fact]
        public async Task GetDistinctDivisions_ReturnsSuccessJsonWithDivisions()
        {
            // Arrange
            var divisions = new List<DivisionDto>
            {
                new() { DivName = "BSD" },
                new() { DivName = "VSD" }
            };
            var apiResponse = ApiResponseDto<IEnumerable<DivisionDto>>.SuccessResponse(divisions);
            _divisionService.GetAllDivisionsAsync().Returns(apiResponse);

            // Act
            var result = await _controller.GetDistinctDivisions();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
        }

        [Fact]
        public async Task GetDistinctDivisions_WhenServiceFails_ReturnsJsonError()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } };
            var apiResponse = ApiResponseDto<IEnumerable<DivisionDto>>.FailureResponse(errors, new ApiMetaDto());
            _divisionService.GetAllDivisionsAsync().Returns(apiResponse);

            // Act
            var result = await _controller.GetDistinctDivisions();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
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
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Contains("Unexpected error", value.message);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task Create_Post_WithNullRates_HandledGracefully()
        {
            // Arrange
            var dto = new DivisionGradeMaintenanceDto
            {
                DivisionGradeCode = "A-VSD", GradeCode = "A", Division = "VSD",
                ChargeRate = null, DirectRate = null, PayRate = null, Npr = null, Ohr = null
            };
            var apiResponse = ApiResponseDto<DivisionGradeMaintenanceDto>.SuccessResponse(dto);
            _maintDGService.CreateAsync(dto).Returns(apiResponse);

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
        }

        [Fact]
        public async Task LoadDivisionGradeGrid_WithNullFilter_HandlesGracefully()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = null };
            var pagedResponse = BuildPagedResponse([]);

            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _maintDGService.GetAllPagedAsync(Arg.Any<QueryParameters<string>>()).Returns(pagedResponse);
            _mapper.Map<List<DivisionGradeMaintenanceItem>>(Arg.Any<List<DivisionGradeMaintenanceDto>>())
                .Returns([]);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadDivisionGradeGrid(request);

            // Assert
            Assert.IsType<PartialViewResult>(result);
        }

        #endregion
    }

    // Local helper record to deserialize JsonResult values
    internal record JsonResponse(bool success, string message, object? data = null, object? errors = null);
}
