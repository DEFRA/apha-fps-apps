using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.WorkGroupGradeMaintenanceControllerTest
{
    public class WorkGroupGradeMaintenanceControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IWorkGroupGradeService _wgGradeService;
        private readonly WorkGroupGradeMaintenanceController _controller;

        public WorkGroupGradeMaintenanceControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _wgGradeService = Substitute.For<IWorkGroupGradeService>();
            _controller = new WorkGroupGradeMaintenanceController(_mapper, _wgGradeService);
        }

        private static T? GetJsonResultValue<T>(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<T>(json);
        }

        #region Constructor

        [Fact]
        public void Constructor_NullMapper_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new WorkGroupGradeMaintenanceController(null!, _wgGradeService));
        }

        [Fact]
        public void Constructor_NullService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new WorkGroupGradeMaintenanceController(_mapper, null!));
        }

        #endregion

        #region Index

        [Fact]
        public async Task Index_ReturnsViewResult_WithViewModel()
        {
            var data = new List<WorkgroupGradeDto>
            {
                new() { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" }
            };
            var apiResponse = ApiResponseDto<List<WorkgroupGradeDto>>.SuccessResponse(data, new PaginationDto { PageNumber = 1, PageSize = 15, TotalRecords = 1 });

            _wgGradeService.GetAllWorkgroupGradesPagedAsync(Arg.Any<QueryParameters<string>>()).Returns(apiResponse);
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>()).Returns(new QueryParameters<string>());
            _mapper.Map<List<MaintWGGradeItem>>(Arg.Any<List<WorkgroupGradeDto>>()).Returns(new List<MaintWGGradeItem> { new() { WgGrade = "WG01" } });
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<MaintWGGradeViewModel>(viewResult.Model);
            Assert.NotNull(model.WGGradeGrid);
            Assert.Equal("wgGradeGrid", model.WGGradeGrid.GridId);
        }

        [Fact]
        public async Task Index_CallsService_WithDefaultParameters()
        {
            var apiResponse = ApiResponseDto<List<WorkgroupGradeDto>>.SuccessResponse(new List<WorkgroupGradeDto>(), new PaginationDto());
            _wgGradeService.GetAllWorkgroupGradesPagedAsync(Arg.Any<QueryParameters<string>>()).Returns(apiResponse);
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>()).Returns(new QueryParameters<string>());
            _mapper.Map<List<MaintWGGradeItem>>(Arg.Any<List<WorkgroupGradeDto>>()).Returns(new List<MaintWGGradeItem>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            await _controller.Index();

            await _wgGradeService.Received(1).GetAllWorkgroupGradesPagedAsync(Arg.Any<QueryParameters<string>>());
        }

        #endregion

        #region LoadMaintWGGradeGrid

        [Fact]
        public async Task LoadMaintWGGradeGrid_WithValidRequest_ReturnsPartialView()
        {
            var request = new PaginationFilter<string> { Filter = "{}" };
            var apiResponse = ApiResponseDto<List<WorkgroupGradeDto>>.SuccessResponse(new List<WorkgroupGradeDto>(), new PaginationDto());

            _wgGradeService.GetAllWorkgroupGradesPagedAsync(Arg.Any<QueryParameters<string>>()).Returns(apiResponse);
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>()).Returns(new QueryParameters<string>());
            _mapper.Map<List<MaintWGGradeItem>>(Arg.Any<List<WorkgroupGradeDto>>()).Returns(new List<MaintWGGradeItem>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            var result = await _controller.LoadMaintWGGradeGrid(request);

            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialViewResult.ViewName);
            Assert.IsType<DataGridConfig<MaintWGGradeItem>>(partialViewResult.Model);
        }

        [Fact]
        public async Task LoadMaintWGGradeGrid_WithInvalidModelState_ReturnsJsonError()
        {
            var request = new PaginationFilter<string> { Filter = "{}" };
            _controller.ModelState.AddModelError("Test", "Test error");

            var result = await _controller.LoadMaintWGGradeGrid(request);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Invalid request data", value.message);
        }

        [Fact]
        public async Task LoadMaintWGGradeGrid_WithNullFilter_HandlesGracefully()
        {
            var request = new PaginationFilter<string> { Filter = null };
            var apiResponse = ApiResponseDto<List<WorkgroupGradeDto>>.SuccessResponse(new List<WorkgroupGradeDto>(), new PaginationDto());

            _wgGradeService.GetAllWorkgroupGradesPagedAsync(Arg.Any<QueryParameters<string>>()).Returns(apiResponse);
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>()).Returns(new QueryParameters<string>());
            _mapper.Map<List<MaintWGGradeItem>>(Arg.Any<List<WorkgroupGradeDto>>()).Returns(new List<MaintWGGradeItem>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            var result = await _controller.LoadMaintWGGradeGrid(request);

            Assert.IsType<PartialViewResult>(result);
        }

        [Fact]
        public async Task LoadMaintWGGradeGrid_ConfiguresGridCorrectly()
        {
            var request = new PaginationFilter<string> { Filter = "{}", SortBy = "WgGrade", Descending = true };
            var apiResponse = ApiResponseDto<List<WorkgroupGradeDto>>.SuccessResponse(new List<WorkgroupGradeDto>(), new PaginationDto());

            _wgGradeService.GetAllWorkgroupGradesPagedAsync(Arg.Any<QueryParameters<string>>()).Returns(apiResponse);
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>()).Returns(new QueryParameters<string>());
            _mapper.Map<List<MaintWGGradeItem>>(Arg.Any<List<WorkgroupGradeDto>>()).Returns(new List<MaintWGGradeItem>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            var result = await _controller.LoadMaintWGGradeGrid(request);

            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<MaintWGGradeItem>>(partialViewResult.Model);
            Assert.Equal("wgGradeGrid", gridConfig.GridId);
            Assert.Equal("WorkGroup Grades", gridConfig.Title);
            Assert.Equal("WgGrade", gridConfig.KeyProperty);
            Assert.Equal("addWGGrade", gridConfig.AddFunction);
            Assert.Equal("editWGGrade", gridConfig.EditFunction);
            Assert.Equal("deleteWGGrade", gridConfig.DeleteFunction);
            Assert.Equal("/FPS/WorkGroupGradeMaintenance/LoadMaintWGGradeGrid", gridConfig.BindGridUrl);
        }

        [Fact]
        public async Task LoadMaintWGGradeGrid_WithNullData_ReturnsEmptyGrid()
        {
            var request = new PaginationFilter<string> { Filter = "{}" };
            var apiResponse = new ApiResponseDto<List<WorkgroupGradeDto>> { Success = true, Data = null, Pagination = new PaginationDto() };

            _wgGradeService.GetAllWorkgroupGradesPagedAsync(Arg.Any<QueryParameters<string>>()).Returns(apiResponse);
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>()).Returns(new QueryParameters<string>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            var result = await _controller.LoadMaintWGGradeGrid(request);

            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<MaintWGGradeItem>>(partialViewResult.Model);
            Assert.Empty(gridConfig.Data);
        }

        [Fact]
        public async Task LoadMaintWGGradeGrid_WithNullPagination_UsesDefaultPaginationModel()
        {
            var request = new PaginationFilter<string> { Filter = "{}" };
            var apiResponse = new ApiResponseDto<List<WorkgroupGradeDto>> { Success = true, Data = new List<WorkgroupGradeDto>(), Pagination = null };

            _wgGradeService.GetAllWorkgroupGradesPagedAsync(Arg.Any<QueryParameters<string>>()).Returns(apiResponse);
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>()).Returns(new QueryParameters<string>());
            _mapper.Map<List<MaintWGGradeItem>>(Arg.Any<List<WorkgroupGradeDto>>()).Returns(new List<MaintWGGradeItem>());

            var result = await _controller.LoadMaintWGGradeGrid(request);

            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<MaintWGGradeItem>>(partialViewResult.Model);
            Assert.NotNull(gridConfig.Pagination);
        }

        #endregion

        #region Create GET

        [Fact]
        public void Create_Get_ReturnsPartialViewWithEmptyModel()
        {
            var result = _controller.Create();

            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditMaintWGGrade", partialViewResult.ViewName);
            Assert.IsType<MaintWGGradeItem>(partialViewResult.Model);
        }

        #endregion

        #region Create POST

        [Fact]
        public async Task Create_Post_WithValidModel_ReturnsSuccessJson()
        {
            var item = new MaintWGGradeItem { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" };
            var dto = new WorkgroupGradeDto { WgGrade = "WG01" };
            var apiResponse = ApiResponseDto<WorkgroupGradeDto>.SuccessResponse(dto);

            _mapper.Map<WorkgroupGradeDto>(item).Returns(dto);
            _wgGradeService.CreateAsync(dto).Returns(apiResponse);

            var result = await _controller.Create(item);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.True(value!.success);
            Assert.Equal("WorkgroupGrade created successfully", value.message);
        }

        [Fact]
        public async Task Create_Post_WhenServiceFails_ReturnsJsonError()
        {
            var item = new MaintWGGradeItem { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" };
            var dto = new WorkgroupGradeDto { WgGrade = "WG01" };
            var errors = new List<ApiErrorDto> { new() { Message = "Duplicate", Code = "DUPLICATE" } };
            var apiResponse = ApiResponseDto<WorkgroupGradeDto>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<WorkgroupGradeDto>(item).Returns(dto);
            _wgGradeService.CreateAsync(dto).Returns(apiResponse);

            var result = await _controller.Create(item);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.False(value!.success);
        }

        [Fact]
        public async Task Create_Post_WithInvalidModelState_ReturnsValidationErrors()
        {
            var item = new MaintWGGradeItem { WgGrade = "", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" };
            _controller.ModelState.AddModelError("WgGrade", "WGGrade is required");

            var result = await _controller.Create(item);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.False(value!.success);
            Assert.Equal("Please correct the errors below.", value.message);
        }

        [Fact]
        public async Task Create_Post_CallsMapperAndService()
        {
            var item = new MaintWGGradeItem { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" };
            var dto = new WorkgroupGradeDto { WgGrade = "WG01" };
            var apiResponse = ApiResponseDto<WorkgroupGradeDto>.SuccessResponse(dto);

            _mapper.Map<WorkgroupGradeDto>(item).Returns(dto);
            _wgGradeService.CreateAsync(dto).Returns(apiResponse);

            await _controller.Create(item);

            _mapper.Received(1).Map<WorkgroupGradeDto>(item);
            await _wgGradeService.Received(1).CreateAsync(dto);
        }

        #endregion

        #region Edit GET

        [Fact]
        public async Task Edit_Get_WithValidCode_ReturnsPartialView()
        {
            var dto = new WorkgroupGradeDto { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" };
            var item = new MaintWGGradeItem { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" };
            var apiResponse = ApiResponseDto<WorkgroupGradeDto>.SuccessResponse(dto);

            _wgGradeService.GetByWgGradeAsync("WG01").Returns(apiResponse);
            _mapper.Map<MaintWGGradeItem>(dto).Returns(item);

            var result = await _controller.Edit("WG01");

            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditMaintWGGrade", partialViewResult.ViewName);
            var model = Assert.IsType<MaintWGGradeItem>(partialViewResult.Model);
            Assert.Equal("WG01", model.WgGrade);
        }

        [Fact]
        public async Task Edit_Get_WhenNotFound_ReturnsJsonError()
        {
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var apiResponse = ApiResponseDto<WorkgroupGradeDto>.FailureResponse(errors, new ApiMetaDto());

            _wgGradeService.GetByWgGradeAsync("INVALID").Returns(apiResponse);

            var result = await _controller.Edit("INVALID");

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.False(value!.success);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Edit_Get_WithNullOrEmptyCode_ReturnsJsonError(string? wgGrade)
        {
            var result = await _controller.Edit(wgGrade!);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.False(value!.success);
            Assert.Equal("WgGrade is required", value.message);
        }

        #endregion

        #region Edit POST

        [Fact]
        public async Task Edit_Post_WithValidModel_ReturnsSuccessJson()
        {
            var item = new MaintWGGradeItem { WgGrade = "WG01", ProfitCentreGrade = "PC02", GradeCode = "G02", Workgroup = "HR" };
            var dto = new WorkgroupGradeDto { WgGrade = "WG01" };
            var apiResponse = ApiResponseDto<WorkgroupGradeDto>.SuccessResponse(dto);

            _mapper.Map<WorkgroupGradeDto>(item).Returns(dto);
            _wgGradeService.UpdateAsync("WG01", dto).Returns(apiResponse);

            var result = await _controller.Edit(item);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.True(value!.success);
            Assert.Equal("WorkgroupGrade updated successfully", value.message);
        }

        [Fact]
        public async Task Edit_Post_WhenServiceFails_ReturnsJsonError()
        {
            var item = new MaintWGGradeItem { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" };
            var dto = new WorkgroupGradeDto { WgGrade = "WG01" };
            var errors = new List<ApiErrorDto> { new() { Message = "Update failed", Code = "UPDATE_ERROR" } };
            var apiResponse = ApiResponseDto<WorkgroupGradeDto>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<WorkgroupGradeDto>(item).Returns(dto);
            _wgGradeService.UpdateAsync("WG01", dto).Returns(apiResponse);

            var result = await _controller.Edit(item);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.False(value!.success);
        }

        [Fact]
        public async Task Edit_Post_WithInvalidModelState_ReturnsValidationErrors()
        {
            var item = new MaintWGGradeItem { WgGrade = "WG01" };
            _controller.ModelState.AddModelError("GradeCode", "Grade is required");

            var result = await _controller.Edit(item);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.False(value!.success);
            Assert.Equal("Please correct the errors below.", value.message);
        }

        #endregion

        #region Delete

        [Fact]
        public async Task Delete_WithValidCode_ReturnsSuccessJson()
        {
            var apiResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _wgGradeService.DeleteAsync("WG01").Returns(apiResponse);

            var result = await _controller.Delete("WG01");

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.True(value!.success);
            Assert.Equal("WorkgroupGrade deleted successfully", value.message);
        }

        [Fact]
        public async Task Delete_WhenServiceFails_ReturnsJsonError()
        {
            var errors = new List<ApiErrorDto> { new() { Message = "Delete failed", Code = "DELETE_ERROR" } };
            var apiResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _wgGradeService.DeleteAsync("WG01").Returns(apiResponse);

            var result = await _controller.Delete("WG01");

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.False(value!.success);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Delete_WithNullOrEmptyCode_ReturnsJsonError(string? wgGrade)
        {
            var result = await _controller.Delete(wgGrade!);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.False(value!.success);
            Assert.Equal("WgGrade is required", value.message);
        }

        [Theory]
        [InlineData("WG01")]
        [InlineData("WG02")]
        [InlineData("TEST")]
        public async Task Delete_WithVariousCodes_CallsService(string wgGrade)
        {
            var apiResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _wgGradeService.DeleteAsync(wgGrade).Returns(apiResponse);

            await _controller.Delete(wgGrade);

            await _wgGradeService.Received(1).DeleteAsync(wgGrade);
        }

        #endregion

        #region GetPcGrades

        [Fact]
        public async Task GetPcGrades_WithSuccessResponse_ReturnsSuccessJson()
        {
            var apiResponse = ApiResponseDto<List<string>>.SuccessResponse(new List<string> { "PC01", "PC02" });
            _wgGradeService.GetAllPcGradesAsync().Returns(apiResponse);

            var result = await _controller.GetPcGrades();

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.True(value!.success);
        }

        [Fact]
        public async Task GetPcGrades_WhenServiceFails_ReturnsJsonError()
        {
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } };
            var apiResponse = ApiResponseDto<List<string>>.FailureResponse(errors, new ApiMetaDto());
            _wgGradeService.GetAllPcGradesAsync().Returns(apiResponse);

            var result = await _controller.GetPcGrades();

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.False(value!.success);
        }

        #endregion

        #region GetGradeCodes

        [Fact]
        public async Task GetGradeCodes_WithSuccessResponse_ReturnsSuccessJson()
        {
            var apiResponse = ApiResponseDto<List<string>>.SuccessResponse(new List<string> { "G01", "G02" });
            _wgGradeService.GetAllGradeCodesAsync().Returns(apiResponse);

            var result = await _controller.GetGradeCodes();

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.True(value!.success);
        }

        [Fact]
        public async Task GetGradeCodes_WhenServiceFails_ReturnsJsonError()
        {
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } };
            var apiResponse = ApiResponseDto<List<string>>.FailureResponse(errors, new ApiMetaDto());
            _wgGradeService.GetAllGradeCodesAsync().Returns(apiResponse);

            var result = await _controller.GetGradeCodes();

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.False(value!.success);
        }

        #endregion

        #region GetWorkgroups

        [Fact]
        public async Task GetWorkgroups_WithSuccessResponse_ReturnsSuccessJson()
        {
            var apiResponse = ApiResponseDto<List<string>>.SuccessResponse(new List<string> { "IT", "HR" });
            _wgGradeService.GetAllWorkgroupNamesAsync().Returns(apiResponse);

            var result = await _controller.GetWorkgroups();

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.True(value!.success);
        }

        [Fact]
        public async Task GetWorkgroups_WhenServiceFails_ReturnsJsonError()
        {
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } };
            var apiResponse = ApiResponseDto<List<string>>.FailureResponse(errors, new ApiMetaDto());
            _wgGradeService.GetAllWorkgroupNamesAsync().Returns(apiResponse);

            var result = await _controller.GetWorkgroups();

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.False(value!.success);
        }

        #endregion

        private class JsonResponse
        {
            public bool success { get; set; }
            public string? message { get; set; }
            public object? data { get; set; }
            public object? errors { get; set; }
        }
    }
}
