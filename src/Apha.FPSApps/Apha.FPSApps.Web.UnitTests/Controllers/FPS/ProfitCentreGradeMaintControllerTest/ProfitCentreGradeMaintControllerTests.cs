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

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.ProfitCentreGradeMaintControllerTest
{
    public class ProfitCentreGradeMaintControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IProfitCentreGradeService _profitCentreGradeService;
        private readonly IDivisionGradeService _divisionGradeService;
        private readonly IFpsYearContext _fpsYearContext;
        private readonly ProfitCentreGradeMaintController _controller;

        public ProfitCentreGradeMaintControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _profitCentreGradeService = Substitute.For<IProfitCentreGradeService>();
            _divisionGradeService = Substitute.For<IDivisionGradeService>();
            _fpsYearContext = Substitute.For<IFpsYearContext>();
            _fpsYearContext.Year.Returns(2025);
            _controller = new ProfitCentreGradeMaintController(
                _mapper, _profitCentreGradeService, _divisionGradeService, _fpsYearContext);
        }

        // Helper to extract JsonResult value
        private static T? GetJsonResultValue<T>(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<T>(json);
        }

        private static ProfitCentreGradeDto BuildDto(string pcGrade = "G001") =>
            new()
            {
                PcGrade = pcGrade,
                DivisionGrade = "A-VSD",
                GradeCode = "A",
                ProfitCentre = "PC01",
                ChargeRate = 100m,
                DirectRate = 90m,
                PayRate = 80m,
                NPR = 10m,
                OHR = 5m
            };

        private static ApiResponseDto<List<ProfitCentreGradeDto>> BuildPagedResponse(
            IEnumerable<ProfitCentreGradeDto>? data = null) =>
            ApiResponseDto<List<ProfitCentreGradeDto>>.SuccessResponse(
                data?.ToList() ?? [], new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 });

        private void SetupPagedGridCall(IEnumerable<ProfitCentreGradeDto>? data = null)
        {
            var pagedResponse = BuildPagedResponse(data);
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _profitCentreGradeService.GetAllPagedAsync(Arg.Any<QueryParameters<string>>())
                .Returns(pagedResponse);
            _mapper.Map<List<ProfitCentreGradeMaintItem>>(Arg.Any<List<ProfitCentreGradeDto>>())
                .Returns(data?.Select(d => new ProfitCentreGradeMaintItem { PcGrade = d.PcGrade }).ToList()
                         ?? []);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenMapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ProfitCentreGradeMaintController(
                    null!, _profitCentreGradeService, _divisionGradeService, _fpsYearContext));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenProfitCentreGradeServiceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ProfitCentreGradeMaintController(
                    _mapper, null!, _divisionGradeService, _fpsYearContext));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenDivisionGradeServiceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ProfitCentreGradeMaintController(
                    _mapper, _profitCentreGradeService, null!, _fpsYearContext));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenYearContextIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ProfitCentreGradeMaintController(
                    _mapper, _profitCentreGradeService, _divisionGradeService, null!));
        }

        #endregion

        #region Index Tests

        [Fact]
        public async Task Index_ReturnsViewResult_WithProfitCentreGradeMaintViewModel()
        {
            // Arrange
            SetupPagedGridCall([BuildDto()]);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProfitCentreGradeMaintViewModel>(viewResult.Model);
            Assert.NotNull(model.RcGradeMaintenanceGrid);
            Assert.Equal("rcGradeMaintenanceGrid", model.RcGradeMaintenanceGrid.GridId);
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithCorrectGridTitle()
        {
            // Arrange
            SetupPagedGridCall([]);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProfitCentreGradeMaintViewModel>(viewResult.Model);
            Assert.Equal("Resource Centre Grade Maintenance", model.RcGradeMaintenanceGrid.Title);
        }

        [Fact]
        public async Task Index_GridHasNoDefaultSortColumn()
        {
            // Arrange
            SetupPagedGridCall([BuildDto()]);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model      = Assert.IsType<ProfitCentreGradeMaintViewModel>(viewResult.Model);
            Assert.True(string.IsNullOrEmpty(model.RcGradeMaintenanceGrid.Pagination.SortColumn),
                "No sort column should be applied on initial page load.");
        }

        [Fact]
        public async Task Index_GridHasNoDefaultSortDirection()
        {
            // Arrange
            SetupPagedGridCall([BuildDto()]);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model      = Assert.IsType<ProfitCentreGradeMaintViewModel>(viewResult.Model);
            Assert.False(model.RcGradeMaintenanceGrid.Pagination.SortDirection,
                "Sort direction should default to ascending (false) on initial page load.");
        }

        #endregion

        #region LoadProfitCentreGradeMaintGrid Tests

        [Fact]
        public async Task LoadProfitCentreGradeMaintGrid_WithValidRequest_ReturnsPartialView()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            SetupPagedGridCall([BuildDto()]);

            // Act
            var result = await _controller.LoadProfitCentreGradeMaintGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialViewResult.ViewName);
            Assert.IsType<DataGridConfig<ProfitCentreGradeMaintItem>>(partialViewResult.Model);
        }

        [Fact]
        public async Task LoadProfitCentreGradeMaintGrid_WithInvalidModelState_ReturnsJsonError()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _controller.ModelState.AddModelError("Test", "Test error");

            // Act
            var result = await _controller.LoadProfitCentreGradeMaintGrid(request);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Invalid request data", value.message);
        }

        [Fact]
        public async Task LoadProfitCentreGradeMaintGrid_WithEmptyData_ReturnsEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            SetupPagedGridCall([]);

            // Act
            var result = await _controller.LoadProfitCentreGradeMaintGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ProfitCentreGradeMaintItem>>(partialViewResult.Model);
            Assert.Empty(gridConfig.Data);
        }

        [Fact]
        public async Task LoadProfitCentreGradeMaintGrid_WithNullFilter_HandlesGracefully()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = null };
            SetupPagedGridCall([]);

            // Act
            var result = await _controller.LoadProfitCentreGradeMaintGrid(request);

            // Assert
            Assert.IsType<PartialViewResult>(result);
        }

        #endregion

        #region Create GET Tests

        [Fact]
        public void Create_Get_ReturnsPartialViewWithEmptyModel()
        {
            // Act
            var result = _controller.Create();

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditMaintPCGrade", partialViewResult.ViewName);
            Assert.IsType<ProfitCentreGradeMaintItem>(partialViewResult.Model);
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
            _controller.ModelState.AddModelError("PcGrade", "Required");

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
            var dto = BuildDto("G001");
            var apiResponse = ApiResponseDto<ProfitCentreGradeDto>.SuccessResponse(dto);
            _profitCentreGradeService.CreateAsync(dto).Returns(apiResponse);

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
            Assert.Equal("RC grade created successfully", value.message);
        }

        [Fact]
        public async Task Create_Post_WhenServiceFails_ReturnsJsonError()
        {
            // Arrange
            var dto = BuildDto();
            var errors = new List<ApiErrorDto> { new() { Message = "ProfitCentre does not exist.", Code = "INVALID_PC" } };
            var apiResponse = ApiResponseDto<ProfitCentreGradeDto>.FailureResponse(errors, new ApiMetaDto());
            _profitCentreGradeService.CreateAsync(dto).Returns(apiResponse);

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("ProfitCentre does not exist.", value.message);
        }

        [Fact]
        public async Task Create_Post_NegativeRates_AreConvertedToPositive()
        {
            // Arrange
            var dto = new ProfitCentreGradeDto
            {
                PcGrade = "G001", DivisionGrade = "A-VSD", GradeCode = "A", ProfitCentre = "PC01",
                ChargeRate = -100m, DirectRate = -90m, PayRate = -80m, NPR = -10m, OHR = -5m
            };
            ProfitCentreGradeDto? capturedDto = null;
            var apiResponse = ApiResponseDto<ProfitCentreGradeDto>.SuccessResponse(dto);
            _profitCentreGradeService.CreateAsync(Arg.Do<ProfitCentreGradeDto>(d => capturedDto = d))
                .Returns(apiResponse);

            // Act
            await _controller.Create(dto);

            // Assert
            Assert.NotNull(capturedDto);
            Assert.Equal(100m, capturedDto!.ChargeRate);
            Assert.Equal(90m, capturedDto.DirectRate);
            Assert.Equal(80m, capturedDto.PayRate);
            Assert.Equal(10m, capturedDto.NPR);
            Assert.Equal(5m, capturedDto.OHR);
        }

        [Fact]
        public async Task Create_Post_WithNullRates_HandlesGracefully()
        {
            // Arrange
            var dto = new ProfitCentreGradeDto
            {
                PcGrade = "G001", DivisionGrade = "A-VSD", GradeCode = "A", ProfitCentre = "PC01",
                ChargeRate = null, DirectRate = null, PayRate = null, NPR = null, OHR = null
            };
            var apiResponse = ApiResponseDto<ProfitCentreGradeDto>.SuccessResponse(dto);
            _profitCentreGradeService.CreateAsync(dto).Returns(apiResponse);

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
        }

        [Fact]
        public async Task Create_Post_CallsCreateAsync_Once()
        {
            // Arrange
            var dto = BuildDto();
            var apiResponse = ApiResponseDto<ProfitCentreGradeDto>.SuccessResponse(dto);
            _profitCentreGradeService.CreateAsync(dto).Returns(apiResponse);

            // Act
            await _controller.Create(dto);

            // Assert
            await _profitCentreGradeService.Received(1).CreateAsync(dto);
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
            Assert.Equal("RC grade code is required", value.message);
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
            var dto = BuildDto("G001");
            var item = new ProfitCentreGradeMaintItem { PcGrade = "G001" };
            var apiResponse = ApiResponseDto<ProfitCentreGradeDto>.SuccessResponse(dto);

            _profitCentreGradeService.GetByIdAsync("G001").Returns(apiResponse);
            _mapper.Map<ProfitCentreGradeMaintItem>(dto).Returns(item);

            // Act
            var result = await _controller.Edit("G001");

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditMaintPCGrade", partialViewResult.ViewName);
            var model = Assert.IsType<ProfitCentreGradeMaintItem>(partialViewResult.Model);
            Assert.Equal("G001", model.PcGrade);
        }

        [Fact]
        public async Task Edit_Get_WhenNotFound_ReturnsJsonError()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var apiResponse = ApiResponseDto<ProfitCentreGradeDto>.FailureResponse(errors, new ApiMetaDto());
            _profitCentreGradeService.GetByIdAsync("NOTEXIST").Returns(apiResponse);

            // Act
            var result = await _controller.Edit("NOTEXIST");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        [Fact]
        public async Task Edit_Get_WhenDataIsNull_ReturnsJsonError()
        {
            // Arrange
            var apiResponse = ApiResponseDto<ProfitCentreGradeDto>.SuccessResponse(null!);
            _profitCentreGradeService.GetByIdAsync("G001").Returns(apiResponse);

            // Act
            var result = await _controller.Edit("G001");

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
            var result = await _controller.Edit("G001", null!);

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
            _controller.ModelState.AddModelError("PcGrade", "Required");

            // Act
            var result = await _controller.Edit("G001", dto);

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
            var dto = BuildDto("G001");
            var apiResponse = ApiResponseDto<ProfitCentreGradeDto>.SuccessResponse(dto);
            _profitCentreGradeService.UpdateAsync("G001", dto).Returns(apiResponse);

            // Act
            var result = await _controller.Edit("G001", dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
            Assert.Equal("RC grade updated successfully", value.message);
        }

        [Fact]
        public async Task Edit_Post_WhenServiceFails_ReturnsJsonError()
        {
            // Arrange
            var dto = BuildDto();
            var errors = new List<ApiErrorDto> { new() { Message = "Update failed", Code = "UPDATE_ERROR" } };
            var apiResponse = ApiResponseDto<ProfitCentreGradeDto>.FailureResponse(errors, new ApiMetaDto());
            _profitCentreGradeService.UpdateAsync("G001", dto).Returns(apiResponse);

            // Act
            var result = await _controller.Edit("G001", dto);

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
            var dto = new ProfitCentreGradeDto
            {
                PcGrade = "G001", DivisionGrade = "A-VSD", GradeCode = "A", ProfitCentre = "PC01",
                ChargeRate = -100m, DirectRate = -90m, PayRate = -80m, NPR = -10m, OHR = -5m
            };
            ProfitCentreGradeDto? capturedDto = null;
            var apiResponse = ApiResponseDto<ProfitCentreGradeDto>.SuccessResponse(dto);
            _profitCentreGradeService.UpdateAsync(Arg.Any<string>(), Arg.Do<ProfitCentreGradeDto>(d => capturedDto = d))
                .Returns(apiResponse);

            // Act
            await _controller.Edit("G001", dto);

            // Assert
            Assert.NotNull(capturedDto);
            Assert.Equal(100m, capturedDto!.ChargeRate);
            Assert.Equal(90m, capturedDto.DirectRate);
            Assert.Equal(80m, capturedDto.PayRate);
            Assert.Equal(10m, capturedDto.NPR);
            Assert.Equal(5m, capturedDto.OHR);
        }

        [Fact]
        public async Task Edit_Post_CallsUpdateAsync_Once()
        {
            // Arrange
            var dto = BuildDto("G001");
            var apiResponse = ApiResponseDto<ProfitCentreGradeDto>.SuccessResponse(dto);
            _profitCentreGradeService.UpdateAsync("G001", dto).Returns(apiResponse);

            // Act
            await _controller.Edit("G001", dto);

            // Assert
            await _profitCentreGradeService.Received(1).UpdateAsync("G001", dto);
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
            Assert.Equal("RC grade code is required", value.message);
        }

        [Fact]
        public async Task Delete_WithWhiteSpaceId_ReturnsJsonError()
        {
            // Act
            var result = await _controller.Delete("   ");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        [Fact]
        public async Task Delete_WithValidId_ReturnsSuccessJson()
        {
            // Arrange
            var apiResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _profitCentreGradeService.DeleteAsync("G001").Returns(apiResponse);

            // Act
            var result = await _controller.Delete("G001");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
            Assert.Equal("RC grade deleted successfully", value.message);
        }

        [Fact]
        public async Task Delete_WhenServiceReturnsDataFalse_ReturnsJsonError()
        {
            // Arrange
            var apiResponse = ApiResponseDto<bool>.SuccessResponse(false);
            _profitCentreGradeService.DeleteAsync("G001").Returns(apiResponse);

            // Act
            var result = await _controller.Delete("G001");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Unable to delete the RC grade as it may be in use.", value.message);
        }

        [Fact]
        public async Task Delete_CallsDeleteAsync_Once()
        {
            // Arrange
            var apiResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _profitCentreGradeService.DeleteAsync("G001").Returns(apiResponse);

            // Act
            await _controller.Delete("G001");

            // Assert
            await _profitCentreGradeService.Received(1).DeleteAsync("G001");
        }

        #endregion

        #region GetDistinctDivisionGrades Tests

        [Fact]
        public async Task GetDistinctDivisionGrades_ReturnsSuccessJsonWithDivisionGrades()
        {
            // Arrange
            var codes = new List<string> { "A-VSD", "B-VSD", "C-BSD" };
            var apiResponse = ApiResponseDto<List<string>>.SuccessResponse(codes);
            _divisionGradeService.GetAllDivisionGradeCodesAsync().Returns(apiResponse);

            // Act
            var result = await _controller.GetDistinctDivisionGrades();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
        }

        [Fact]
        public async Task GetDistinctDivisionGrades_WhenServiceFails_ReturnsJsonError()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } };
            var apiResponse = ApiResponseDto<List<string>>.FailureResponse(errors, new ApiMetaDto());
            _divisionGradeService.GetAllDivisionGradeCodesAsync().Returns(apiResponse);

            // Act
            var result = await _controller.GetDistinctDivisionGrades();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Failed to load division grades", value.message);
        }

        [Fact]
        public async Task GetDistinctDivisionGrades_WhenExceptionThrown_ReturnsJsonError()
        {
            // Arrange
            _divisionGradeService.GetAllDivisionGradeCodesAsync()
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _controller.GetDistinctDivisionGrades();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Contains("Unexpected error", value.message);
        }

        #endregion

        #region GetDistinctGradeCodes Tests

        [Fact]
        public async Task GetDistinctGradeCodes_ReturnsSuccessJsonWithGradeCodes()
        {
            // Arrange
            var gradeCodes = new List<string> { "A", "B", "C" };
            var apiResponse = ApiResponseDto<List<string>>.SuccessResponse(gradeCodes);
            _divisionGradeService.GetAllGradeCodesAsync().Returns(apiResponse);

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
            _divisionGradeService.GetAllGradeCodesAsync().Returns(apiResponse);

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
            _divisionGradeService.GetAllGradeCodesAsync()
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

        #region GetDistinctProfitCentres Tests

        [Fact]
        public async Task GetDistinctProfitCentres_ReturnsSuccessJsonWithProfitCentres()
        {
            // Arrange
            var profitCentres = new List<string> { "PC01", "PC02", "PC03" };
            var apiResponse = ApiResponseDto<List<string>>.SuccessResponse(profitCentres);
            _profitCentreGradeService.GetAllProfitCentreCodesAsync().Returns(apiResponse);

            // Act
            var result = await _controller.GetDistinctProfitCentres();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
        }

        [Fact]
        public async Task GetDistinctProfitCentres_WhenServiceFails_ReturnsJsonError()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } };
            var apiResponse = ApiResponseDto<List<string>>.FailureResponse(errors, new ApiMetaDto());
            _profitCentreGradeService.GetAllProfitCentreCodesAsync().Returns(apiResponse);

            // Act
            var result = await _controller.GetDistinctProfitCentres();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Failed to load profit centres", value.message);
        }

        [Fact]
        public async Task GetDistinctProfitCentres_WhenExceptionThrown_ReturnsJsonError()
        {
            // Arrange
            _profitCentreGradeService.GetAllProfitCentreCodesAsync()
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _controller.GetDistinctProfitCentres();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Contains("Unexpected error", value.message);
        }

        #endregion
    }

    // Local helper record to deserialize JsonResult values
    internal record JsonResponse(bool success, string message, object? data = null, object? errors = null);
}
