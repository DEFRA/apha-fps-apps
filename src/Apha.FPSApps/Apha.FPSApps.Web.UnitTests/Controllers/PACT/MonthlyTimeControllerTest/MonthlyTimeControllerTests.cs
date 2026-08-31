using Apha.Common.Utilities.ExcelExport;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PACT.Controllers;
using Apha.FPSApps.Web.Areas.PACT.Dependencies;
using Apha.FPSApps.Web.Areas.PACT.Models;
using Apha.FPSApps.Web.Handler;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.FPSApps.Web.UnitTests.Controllers.PACT.MonthlyTimeControllerTest
{
    public class MonthlyTimeControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IPactMonthlyTimeService _monthlyTimeService;
        private readonly IWorkGroupService _workGroupService;
        private readonly IEmployeeService _employeeService;
        private readonly IPactTimeCodeValidService _timeCodeValidService;
        private readonly IMonthService _monthService;
        private readonly IExcelExportService _excelExportService;
        private readonly IMonthlyImportControllerDependencies _monthlyImportDependencies;
        private readonly IFpsYearContext _fpsYearContext;
        private readonly MonthlyTimeController _controller;

        public MonthlyTimeControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _monthlyTimeService = Substitute.For<IPactMonthlyTimeService>();
            _workGroupService = Substitute.For<IWorkGroupService>();
            _employeeService = Substitute.For<IEmployeeService>();
            _timeCodeValidService = Substitute.For<IPactTimeCodeValidService>();
            _monthService = Substitute.For<IMonthService>();
            _excelExportService = Substitute.For<IExcelExportService>();
            _monthlyImportDependencies = Substitute.For<IMonthlyImportControllerDependencies>();
            _fpsYearContext = Substitute.For<IFpsYearContext>();

            _monthlyImportDependencies.WorkGroupService.Returns(_workGroupService);
            _monthlyImportDependencies.EmployeeService.Returns(_employeeService);
            _monthlyImportDependencies.TimeCodeValidService.Returns(_timeCodeValidService);
            _monthlyImportDependencies.MonthService.Returns(_monthService);
            _fpsYearContext.IsReadOnly.Returns(false);

            _controller = new MonthlyTimeController(
                _mapper,
                _monthlyTimeService,
                _monthlyImportDependencies,
                _excelExportService,
                _fpsYearContext);
            SetupDefaultServiceMocks();
        }

        private void SetupDefaultServiceMocks()
        {
            _workGroupService.GetAllWorkGroupsAsync().Returns(
                ApiResponseDto<List<WorkGroupDto>>.SuccessResponse(
                [
                    new WorkGroupDto { WorkGroupName = "WG1" }
                ]));
            _monthService.GetAllMonthsAsync().Returns(
                ApiResponseDto<List<MonthDto>>.SuccessResponse(
                [
                    new MonthDto { Monthnumber = 6, Monthname = "June" }
                ]));
            _monthlyTimeService.GetStagingAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<bool?>())
                .Returns(ApiResponseDto<List<StagingMonthlyTimeDto>>.SuccessResponse([]));
            _monthlyTimeService.GetLiveAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<double?>())
                .Returns(ApiResponseDto<List<MonthlyTimeDto>>.SuccessResponse([]));
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<MonthlyTimeLiveItem>>(Arg.Any<List<MonthlyTimeDto>>())
                .Returns([]);
            _mapper.Map<List<StagingMonthlyTimeItem>>(Arg.Any<List<StagingMonthlyTimeDto>>())
                .Returns([]);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());
            _timeCodeValidService.GetTimeCodeValidsByWorkGroupAsync(Arg.Any<string>())
                .Returns(ApiResponseDto<List<TimeCodeValidDto>>.SuccessResponse([]));
            _timeCodeValidService.GetTimeCodesProjectsByWorkGroupAndTimeCodeAsync(Arg.Any<string>(), Arg.Any<string>())
                .Returns(ApiResponseDto<List<string>>.SuccessResponse([]));
        }

        #region Index Tests

        [Fact]
        public async Task Index_ReturnsViewWithViewModel()
        {
            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<MonthlyTimeViewModel>(viewResult.Model);
            Assert.NotNull(model.LiveGrid);
            Assert.NotNull(model.StagingGrid);
            Assert.NotNull(model.WorkGroupOptions);
            Assert.NotNull(model.MonthOptions);
        }

        [Fact]
        public async Task Index_WhenWorkGroupServiceFails_ReturnsEmptyOptions()
        {
            _workGroupService.GetAllWorkGroupsAsync().Returns(
                ApiResponseDto<List<WorkGroupDto>>.FailureResponse([], new ApiMetaDto()));

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<MonthlyTimeViewModel>(viewResult.Model);
            Assert.Empty(model.WorkGroupOptions);
        }

        [Fact]
        public async Task Index_WhenMonthServiceFails_ReturnsEmptyMonthOptions()
        {
            _monthService.GetAllMonthsAsync().Returns(
                ApiResponseDto<List<MonthDto>>.FailureResponse([], new ApiMetaDto()));

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<MonthlyTimeViewModel>(viewResult.Model);
            Assert.Empty(model.MonthOptions);
        }

        #endregion

        #region LoadLiveGrid Tests

        [Fact]
        public async Task LoadLiveGrid_InvalidModelState_ReturnsBadRequest()
        {
            _controller.ModelState.AddModelError("WorkGroup", "Required");

            var result = await _controller.LoadLiveGrid(new PaginationFilter<string> { Filter = "{}" }, "WG1", "TC1", "S1", "PP1", 6);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task LoadLiveGrid_WithFilters_ReturnsPartialView()
        {
            var result = await _controller.LoadLiveGrid(
                new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 },
                "WG1", "TC1", "S1", "PP1", 6);

            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadLiveGrid_WithNoFilters_ReturnsEmptyGrid()
        {
            var result = await _controller.LoadLiveGrid(
                new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 },
                null, null, null, null, null);

            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            var grid = Assert.IsType<DataGridConfig<MonthlyTimeLiveItem>>(partial.Model);
            Assert.Empty(grid.Data);
        }

        [Fact]
        public async Task LoadLiveGrid_WhenServiceFails_ReturnsEmptyGrid()
        {
            _monthlyTimeService.GetLiveAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<double?>())
                .Returns(ApiResponseDto<List<MonthlyTimeDto>>.FailureResponse([], new ApiMetaDto()));

            var result = await _controller.LoadLiveGrid(
                new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 },
                "WG1", null, null, null, null);

            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<MonthlyTimeLiveItem>>(partial.Model);
            Assert.Empty(grid.Data);
        }

        [Fact]
        public async Task LoadLiveGrid_WithResponsePagination_MapsPagination()
        {
            var response = ApiResponseDto<List<MonthlyTimeDto>>.SuccessResponse([]);
            response.Pagination = new PaginationDto { TotalRecords = 100 };
            _monthlyTimeService.GetLiveAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<double?>())
                .Returns(response);

            var result = await _controller.LoadLiveGrid(
                new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 },
                "WG1", null, null, null, null);

            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.NotNull(partial.Model);
        }

        [Fact]
        public async Task LoadLiveGrid_WithNullPagination_UsesDefaultPagination()
        {
            var response = ApiResponseDto<List<MonthlyTimeDto>>.SuccessResponse([]);
            response.Pagination = null;
            _monthlyTimeService.GetLiveAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<double?>())
                .Returns(response);

            var result = await _controller.LoadLiveGrid(
                new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 },
                "WG1", null, null, null, null);

            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.NotNull(partial.Model);
        }

        #endregion

        #region LoadStagingGrid Tests

        [Fact]
        public async Task LoadStagingGrid_InvalidModelState_ReturnsBadRequest()
        {
            _controller.ModelState.AddModelError("Passed", "Invalid");

            var result = await _controller.LoadStagingGrid(new PaginationFilter<string> { Filter = "{}" }, true);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task LoadStagingGrid_ValidRequest_ReturnsPartialView()
        {
            var result = await _controller.LoadStagingGrid(
                new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 }, null);

            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadStagingGrid_WhenServiceFails_ReturnsEmptyGrid()
        {
            _monthlyTimeService.GetStagingAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<bool?>())
                .Returns(ApiResponseDto<List<StagingMonthlyTimeDto>>.FailureResponse([], new ApiMetaDto()));

            var result = await _controller.LoadStagingGrid(
                new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 }, null);

            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<StagingMonthlyTimeItem>>(partial.Model);
            Assert.Empty(grid.Data);
        }

        [Fact]
        public async Task LoadStagingGrid_WithNullPagination_UsesDefaultPagination()
        {
            var response = ApiResponseDto<List<StagingMonthlyTimeDto>>.SuccessResponse([]);
            response.Pagination = null;
            _monthlyTimeService.GetStagingAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<bool?>())
                .Returns(response);

            var result = await _controller.LoadStagingGrid(
                new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 }, null);

            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.NotNull(partial.Model);
        }

        #endregion

        #region GetLiveRecord Tests

        [Fact]
        public async Task GetLiveRecord_InvalidModelState_ReturnsBadRequest()
        {
            _controller.ModelState.AddModelError("key", "error");

            var result = await _controller.GetLiveRecord("S1", "TC1", 6, "PP1");

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid request data.", badRequest.Value);
        }

        [Fact]
        public async Task GetLiveRecord_WhenServiceFails_ReturnsNotFound()
        {
            _monthlyTimeService.GetLiveByKeyAsync("S1", "TC1", 6, "PP1")
                .Returns(ApiResponseDto<MonthlyTimeDto>.FailureResponse([], new ApiMetaDto()));

            var result = await _controller.GetLiveRecord("S1", "TC1", 6, "PP1");

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetLiveRecord_WhenDataIsNull_ReturnsNotFound()
        {
            var response = new ApiResponseDto<MonthlyTimeDto> { Success = true, Data = null };
            _monthlyTimeService.GetLiveByKeyAsync("S1", "TC1", 6, "PP1").Returns(response);

            var result = await _controller.GetLiveRecord("S1", "TC1", 6, "PP1");

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetLiveRecord_WhenServiceSucceeds_ReturnsPartialView()
        {
            var dto = new MonthlyTimeDto { PactStaffId = "S1", TimeCode = "TC1", Month = 6, ParentProject = "PP1" };
            _monthlyTimeService.GetLiveByKeyAsync("S1", "TC1", 6, "PP1")
                .Returns(ApiResponseDto<MonthlyTimeDto>.SuccessResponse(dto));
            _mapper.Map<MonthlyTimeLiveItem>(dto).Returns(new MonthlyTimeLiveItem { PactStaffId = "S1" });

            var result = await _controller.GetLiveRecord("S1", "TC1", 6, "PP1");

            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_EditMonthlyTimeLive", partial.ViewName);
            Assert.IsType<MonthlyTimeLiveItem>(partial.Model);
        }

        #endregion

        #region GetStaffByWorkGroup Tests

        [Fact]
        public async Task GetStaffByWorkGroup_WhenServiceFails_ReturnsEmptyJsonArray()
        {
            _employeeService.GetPactWorkGroupStaffAsync("WG1")
                .Returns(ApiResponseDto<List<PactStaffDto>>.FailureResponse([], new ApiMetaDto()));

            var result = await _controller.GetStaffByWorkGroup("WG1");

            var json = Assert.IsType<JsonResult>(result);
            var values = ((IEnumerable<object>)json.Value!).ToList();
            Assert.Empty(values);
        }

        [Fact]
        public async Task GetStaffByWorkGroup_WhenDataIsNull_ReturnsEmptyJsonArray()
        {
            var response = new ApiResponseDto<List<PactStaffDto>> { Success = true, Data = null };
            _employeeService.GetPactWorkGroupStaffAsync("WG1").Returns(response);

            var result = await _controller.GetStaffByWorkGroup("WG1");

            var json = Assert.IsType<JsonResult>(result);
            var values = ((IEnumerable<object>)json.Value!).ToList();
            Assert.Empty(values);
        }

        [Fact]
        public async Task GetStaffByWorkGroup_WhenServiceSucceeds_ReturnsFilteredStaff()
        {
            _employeeService.GetPactWorkGroupStaffAsync("WG1")
                .Returns(ApiResponseDto<List<PactStaffDto>>.SuccessResponse(
                [
                    new PactStaffDto { PactId = "S1", Name = "A" },
                    new PactStaffDto { PactId = null, Name = "B" }
                ]));

            var result = await _controller.GetStaffByWorkGroup("WG1");

            var json = Assert.IsType<JsonResult>(result);
            var values = ((IEnumerable<object>)json.Value!).ToList();
            Assert.Single(values);
        }

        [Fact]
        public async Task GetStaffByWorkGroup_FiltersOutWhitespaceOnlyPactId()
        {
            _employeeService.GetPactWorkGroupStaffAsync("WG1")
                .Returns(ApiResponseDto<List<PactStaffDto>>.SuccessResponse(
                [
                    new PactStaffDto { PactId = "  ", Name = "B" },
                    new PactStaffDto { PactId = "S2", Name = "C" }
                ]));

            var result = await _controller.GetStaffByWorkGroup("WG1");

            var json = Assert.IsType<JsonResult>(result);
            var values = ((IEnumerable<object>)json.Value!).ToList();
            Assert.Single(values);
        }

        #endregion

        #region GetTimeCodesByWorkGroup Tests

        [Fact]
        public async Task GetTimeCodesByWorkGroup_WithoutWorkGroup_ReturnsEmptyJsonArray()
        {
            var result = await _controller.GetTimeCodesByWorkGroup(null);

            var json = Assert.IsType<JsonResult>(result);
            var values = ((IEnumerable<object>)json.Value!).ToList();
            Assert.Empty(values);
        }

        [Fact]
        public async Task GetTimeCodesByWorkGroup_WithEmptyWorkGroup_ReturnsEmptyJsonArray()
        {
            var result = await _controller.GetTimeCodesByWorkGroup("  ");

            var json = Assert.IsType<JsonResult>(result);
            var values = ((IEnumerable<object>)json.Value!).ToList();
            Assert.Empty(values);
        }

        [Fact]
        public async Task GetTimeCodesByWorkGroup_WithValidWorkGroup_ReturnsTimeCodes()
        {
            _timeCodeValidService.GetTimeCodeValidsByWorkGroupAsync("WG1")
                .Returns(ApiResponseDto<List<TimeCodeValidDto>>.SuccessResponse(
                [
                    new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PP1" },
                    new TimeCodeValidDto { TimeCode = "TC2", WorkGroup = "WG1", ParentProject = "PP2" }
                ]));

            var result = await _controller.GetTimeCodesByWorkGroup("WG1");

            var json = Assert.IsType<JsonResult>(result);
            var values = ((IEnumerable<object>)json.Value!).ToList();
            Assert.Equal(2, values.Count);
        }

        [Fact]
        public async Task GetTimeCodesByWorkGroup_WhenServiceFails_ReturnsEmptyList()
        {
            _timeCodeValidService.GetTimeCodeValidsByWorkGroupAsync("WG1")
                .Returns(ApiResponseDto<List<TimeCodeValidDto>>.FailureResponse([], new ApiMetaDto()));

            var result = await _controller.GetTimeCodesByWorkGroup("WG1");

            var json = Assert.IsType<JsonResult>(result);
            var values = ((IEnumerable<object>)json.Value!).ToList();
            Assert.Empty(values);
        }

        #endregion

        #region GetProjectsByWorkGroupAndTimeCode Tests

        [Fact]
        public async Task GetProjectsByWorkGroupAndTimeCode_WithoutInputs_ReturnsEmptyJsonArray()
        {
            var result = await _controller.GetProjectsByWorkGroupAndTimeCode("WG1", null);

            var json = Assert.IsType<JsonResult>(result);
            var values = ((IEnumerable<object>)json.Value!).ToList();
            Assert.Empty(values);
        }

        [Fact]
        public async Task GetProjectsByWorkGroupAndTimeCode_WithNullWorkGroup_ReturnsEmptyJsonArray()
        {
            var result = await _controller.GetProjectsByWorkGroupAndTimeCode(null, "TC1");

            var json = Assert.IsType<JsonResult>(result);
            var values = ((IEnumerable<object>)json.Value!).ToList();
            Assert.Empty(values);
        }

        [Fact]
        public async Task GetProjectsByWorkGroupAndTimeCode_WithValidInputs_ReturnsProjects()
        {
            _timeCodeValidService.GetTimeCodesProjectsByWorkGroupAndTimeCodeAsync("WG1", "TC1")
                .Returns(ApiResponseDto<List<string>>.SuccessResponse(["PP1", "PP2"]));

            var result = await _controller.GetProjectsByWorkGroupAndTimeCode("WG1", "TC1");

            var json = Assert.IsType<JsonResult>(result);
            var values = ((IEnumerable<object>)json.Value!).ToList();
            Assert.Equal(2, values.Count);
        }

        [Fact]
        public async Task GetProjectsByWorkGroupAndTimeCode_WhenServiceFails_ReturnsEmptyList()
        {
            _timeCodeValidService.GetTimeCodesProjectsByWorkGroupAndTimeCodeAsync("WG1", "TC1")
                .Returns(ApiResponseDto<List<string>>.FailureResponse([], new ApiMetaDto()));

            var result = await _controller.GetProjectsByWorkGroupAndTimeCode("WG1", "TC1");

            var json = Assert.IsType<JsonResult>(result);
            var values = ((IEnumerable<object>)json.Value!).ToList();
            Assert.Empty(values);
        }

        #endregion

        #region GetAllTimeCodes Tests

        [Fact]
        public async Task GetAllTimeCodes_WhenServiceFails_ReturnsEmptyJsonArray()
        {
            _timeCodeValidService.GetAllDistinctTimeCodesAsync()
                .Returns(ApiResponseDto<List<string>>.FailureResponse([], new ApiMetaDto()));

            var result = await _controller.GetAllTimeCodes();

            var json = Assert.IsType<JsonResult>(result);
            var values = ((IEnumerable<object>)json.Value!).ToList();
            Assert.Empty(values);
        }

        [Fact]
        public async Task GetAllTimeCodes_WhenDataIsNull_ReturnsEmptyJsonArray()
        {
            var response = new ApiResponseDto<List<string>> { Success = true, Data = null };
            _timeCodeValidService.GetAllDistinctTimeCodesAsync().Returns(response);

            var result = await _controller.GetAllTimeCodes();

            var json = Assert.IsType<JsonResult>(result);
            var values = ((IEnumerable<object>)json.Value!).ToList();
            Assert.Empty(values);
        }

        [Fact]
        public async Task GetAllTimeCodes_WhenServiceSucceeds_ReturnsTimeCodes()
        {
            _timeCodeValidService.GetAllDistinctTimeCodesAsync()
                .Returns(ApiResponseDto<List<string>>.SuccessResponse(["TC1", "TC2"]));

            var result = await _controller.GetAllTimeCodes();

            var json = Assert.IsType<JsonResult>(result);
            var values = ((IEnumerable<object>)json.Value!).ToList();
            Assert.Equal(2, values.Count);
        }

        #endregion

        #region GetAllProjects Tests

        [Fact]
        public async Task GetAllProjects_WhenServiceFails_ReturnsEmptyJsonArray()
        {
            _timeCodeValidService.GetAllDistinctProjectsAsync()
                .Returns(ApiResponseDto<List<string>>.FailureResponse([], new ApiMetaDto()));

            var result = await _controller.GetAllProjects();

            var json = Assert.IsType<JsonResult>(result);
            var values = ((IEnumerable<object>)json.Value!).ToList();
            Assert.Empty(values);
        }

        [Fact]
        public async Task GetAllProjects_WhenDataIsNull_ReturnsEmptyJsonArray()
        {
            var response = new ApiResponseDto<List<string>> { Success = true, Data = null };
            _timeCodeValidService.GetAllDistinctProjectsAsync().Returns(response);

            var result = await _controller.GetAllProjects();

            var json = Assert.IsType<JsonResult>(result);
            var values = ((IEnumerable<object>)json.Value!).ToList();
            Assert.Empty(values);
        }

        [Fact]
        public async Task GetAllProjects_WhenServiceSucceeds_ReturnsProjects()
        {
            _timeCodeValidService.GetAllDistinctProjectsAsync()
                .Returns(ApiResponseDto<List<string>>.SuccessResponse(["PP1", "PP2"]));

            var result = await _controller.GetAllProjects();

            var json = Assert.IsType<JsonResult>(result);
            var values = ((IEnumerable<object>)json.Value!).ToList();
            Assert.Equal(2, values.Count);
        }

        #endregion

        #region SaveLiveRecord Tests

        [Fact]
        public async Task SaveLiveRecord_InvalidModelState_ReturnsFailureJson()
        {
            _controller.ModelState.AddModelError("Hours", "Invalid");

            var result = await _controller.SaveLiveRecord(new MonthlyTimeLiveItem());

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.NotNull(success);
            Assert.IsType<bool>(success);
            Assert.False((bool)success);
        }

        [Fact]
        public async Task SaveLiveRecord_InvalidModelState_WithDollarDotPrefix_StripsPrefix()
        {
            _controller.ModelState.AddModelError("$.Hours", "Invalid hours");

            var result = await _controller.SaveLiveRecord(new MonthlyTimeLiveItem());

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.False((bool)success!);
        }

        [Fact]
        public async Task SaveLiveRecord_InvalidModelState_SkipsDollarKey()
        {
            _controller.ModelState.AddModelError("$", "Root error");
            _controller.ModelState.AddModelError("Hours", "Invalid");

            var result = await _controller.SaveLiveRecord(new MonthlyTimeLiveItem());

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.False((bool)success!);
        }

        [Fact]
        public async Task SaveLiveRecord_WhenValidationFails_ReturnsValidationErrors()
        {
            var model = new MonthlyTimeLiveItem { CompositeKey = "S1|TC1|6|PP1" };
            var dto = new MonthlyTimeDto();
            _mapper.Map<MonthlyTimeDto>(model).Returns(dto);
            _monthlyTimeService.ValidateLiveAsync(dto)
                .Returns(ApiResponseDto<List<ValidationFieldErrorDto>>.SuccessResponse(
                [
                    new ValidationFieldErrorDto { Field = "Hours", Message = "Required" }
                ]));

            var result = await _controller.SaveLiveRecord(model);

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.False((bool)success!);
            var message = json.Value?.GetType().GetProperty("message")?.GetValue(json.Value);
            Assert.Equal("Validation failed.", message);
        }

        [Fact]
        public async Task SaveLiveRecord_WhenUpdateSucceeds_ReturnsSuccess()
        {
            var model = new MonthlyTimeLiveItem { CompositeKey = "S1|TC1|6|PP1" };
            var dto = new MonthlyTimeDto();
            _mapper.Map<MonthlyTimeDto>(model).Returns(dto);
            _monthlyTimeService.ValidateLiveAsync(dto)
                .Returns(ApiResponseDto<List<ValidationFieldErrorDto>>.SuccessResponse([]));
            _monthlyTimeService.UpdateLiveAsync(dto)
                .Returns(ApiResponseDto<MonthlyTimeDto>.SuccessResponse(new MonthlyTimeDto()));

            var result = await _controller.SaveLiveRecord(model);

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.True((bool)success!);
            var message = json.Value?.GetType().GetProperty("message")?.GetValue(json.Value);
            Assert.Equal("Monthly time record updated successfully.", message);
        }

        [Fact]
        public async Task SaveLiveRecord_WhenUpdateFails_ReturnsFailure()
        {
            var model = new MonthlyTimeLiveItem { CompositeKey = "S1|TC1|6|PP1" };
            var dto = new MonthlyTimeDto();
            _mapper.Map<MonthlyTimeDto>(model).Returns(dto);
            _monthlyTimeService.ValidateLiveAsync(dto)
                .Returns(ApiResponseDto<List<ValidationFieldErrorDto>>.SuccessResponse([]));
            _monthlyTimeService.UpdateLiveAsync(dto)
                .Returns(ApiResponseDto<MonthlyTimeDto>.FailureResponse(
                    [new ApiErrorDto { Code = "ERR", Message = "Update failed" }], new ApiMetaDto()));

            var result = await _controller.SaveLiveRecord(model);

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.False((bool)success!);
            var message = json.Value?.GetType().GetProperty("message")?.GetValue(json.Value);
            Assert.Equal("Failed to update monthly time record.", message);
        }

        [Fact]
        public async Task SaveLiveRecord_WhenUpdateFailsWithNoErrors_ReturnsDefaultErrorMessage()
        {
            var model = new MonthlyTimeLiveItem { CompositeKey = "S1|TC1|6|PP1" };
            var dto = new MonthlyTimeDto();
            _mapper.Map<MonthlyTimeDto>(model).Returns(dto);
            _monthlyTimeService.ValidateLiveAsync(dto)
                .Returns(ApiResponseDto<List<ValidationFieldErrorDto>>.SuccessResponse([]));
            var failResponse = new ApiResponseDto<MonthlyTimeDto>
            {
                Success = false,
                Errors = [new ApiErrorDto { Code = null!, Message = null! }]
            };
            _monthlyTimeService.UpdateLiveAsync(dto).Returns(failResponse);

            var result = await _controller.SaveLiveRecord(model);

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.False((bool)success!);
        }

        [Fact]
        public async Task SaveLiveRecord_WhenUpdateFailsWithNullErrors_ReturnsEmptyErrorsList()
        {
            var model = new MonthlyTimeLiveItem { CompositeKey = "S1|TC1|6|PP1" };
            var dto = new MonthlyTimeDto();
            _mapper.Map<MonthlyTimeDto>(model).Returns(dto);
            _monthlyTimeService.ValidateLiveAsync(dto)
                .Returns(ApiResponseDto<List<ValidationFieldErrorDto>>.SuccessResponse([]));
            var failResponse = new ApiResponseDto<MonthlyTimeDto>
            {
                Success = false,
                Errors = null
            };
            _monthlyTimeService.UpdateLiveAsync(dto).Returns(failResponse);

            var result = await _controller.SaveLiveRecord(model);

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.False((bool)success!);
        }

        [Fact]
        public async Task SaveLiveRecord_WhenCompositeKeyIsNull_SetsOriginalPactStaffIdToNull()
        {
            var model = new MonthlyTimeLiveItem { CompositeKey = null! };
            var dto = new MonthlyTimeDto();
            _mapper.Map<MonthlyTimeDto>(model).Returns(dto);
            _monthlyTimeService.ValidateLiveAsync(dto)
                .Returns(ApiResponseDto<List<ValidationFieldErrorDto>>.SuccessResponse([]));
            _monthlyTimeService.UpdateLiveAsync(dto)
                .Returns(ApiResponseDto<MonthlyTimeDto>.SuccessResponse(new MonthlyTimeDto()));

            var result = await _controller.SaveLiveRecord(model);

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.True((bool)success!);
        }

        [Fact]
        public async Task SaveLiveRecord_WhenValidationReturnsNullData_TreatsAsEmptyValidation()
        {
            var model = new MonthlyTimeLiveItem { CompositeKey = "S1|TC1|6|PP1" };
            var dto = new MonthlyTimeDto();
            _mapper.Map<MonthlyTimeDto>(model).Returns(dto);
            var validationResponse = new ApiResponseDto<List<ValidationFieldErrorDto>> { Success = true, Data = null };
            _monthlyTimeService.ValidateLiveAsync(dto).Returns(validationResponse);
            _monthlyTimeService.UpdateLiveAsync(dto)
                .Returns(ApiResponseDto<MonthlyTimeDto>.SuccessResponse(new MonthlyTimeDto()));

            var result = await _controller.SaveLiveRecord(model);

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.True((bool)success!);
        }

        #endregion

        #region GetStagingRecord Tests

        [Fact]
        public async Task GetStagingRecord_InvalidModelState_ReturnsBadRequest()
        {
            _controller.ModelState.AddModelError("id", "error");

            var result = await _controller.GetStagingRecord(1);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid request data.", badRequest.Value);
        }

        [Fact]
        public async Task GetStagingRecord_WhenServiceFails_ReturnsNotFound()
        {
            _monthlyTimeService.GetStagingByIdAsync(1)
                .Returns(ApiResponseDto<StagingMonthlyTimeDto>.FailureResponse([], new ApiMetaDto()));

            var result = await _controller.GetStagingRecord(1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetStagingRecord_WhenDataIsNull_ReturnsNotFound()
        {
            var response = new ApiResponseDto<StagingMonthlyTimeDto> { Success = true, Data = null };
            _monthlyTimeService.GetStagingByIdAsync(1).Returns(response);

            var result = await _controller.GetStagingRecord(1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetStagingRecord_WhenServiceSucceeds_ReturnsPartialView()
        {
            var dto = new StagingMonthlyTimeDto { Id = 1, WorkGroup = "WG1", TimeCode = "TC1" };
            _monthlyTimeService.GetStagingByIdAsync(1)
                .Returns(ApiResponseDto<StagingMonthlyTimeDto>.SuccessResponse(dto));
            _mapper.Map<StagingMonthlyTimeItem>(dto).Returns(new StagingMonthlyTimeItem { Id = 1, WorkGroup = "WG1", TimeCode = "TC1" });

            var result = await _controller.GetStagingRecord(1);

            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditStagingMonthlyTime", partial.ViewName);
        }

        [Fact]
        public async Task GetStagingRecord_WhenWorkGroupIsNull_DoesNotPopulateTimeCodeOptions()
        {
            var dto = new StagingMonthlyTimeDto { Id = 1, WorkGroup = null, TimeCode = "TC1" };
            _monthlyTimeService.GetStagingByIdAsync(1)
                .Returns(ApiResponseDto<StagingMonthlyTimeDto>.SuccessResponse(dto));
            _mapper.Map<StagingMonthlyTimeItem>(dto).Returns(new StagingMonthlyTimeItem { Id = 1, WorkGroup = null, TimeCode = "TC1" });

            var result = await _controller.GetStagingRecord(1);

            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditStagingMonthlyTime", partial.ViewName);
        }

        [Fact]
        public async Task GetStagingRecord_WhenWorkGroupSetButTimeCodeNull_PopulatesTimeCodesButNotProjects()
        {
            var dto = new StagingMonthlyTimeDto { Id = 1, WorkGroup = "WG1", TimeCode = null };
            _monthlyTimeService.GetStagingByIdAsync(1)
                .Returns(ApiResponseDto<StagingMonthlyTimeDto>.SuccessResponse(dto));
            _mapper.Map<StagingMonthlyTimeItem>(dto).Returns(new StagingMonthlyTimeItem { Id = 1, WorkGroup = "WG1", TimeCode = null });

            var result = await _controller.GetStagingRecord(1);

            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditStagingMonthlyTime", partial.ViewName);
        }

        #endregion

        #region AddStagingRecord Tests

        [Fact]
        public async Task AddStagingRecord_ReturnsPartialViewWithEmptyModel()
        {
            var result = await _controller.AddStagingRecord();

            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditStagingMonthlyTime", partial.ViewName);
            Assert.IsType<StagingMonthlyTimeItem>(partial.Model);
        }

        #endregion

        #region SaveStagingRecord Tests

        [Fact]
        public async Task SaveStagingRecord_InvalidModelState_ReturnsFailureJson()
        {
            _controller.ModelState.AddModelError("Hours", "Invalid");

            var result = await _controller.SaveStagingRecord(new StagingMonthlyTimeItem());

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.False((bool)success!);
        }

        [Fact]
        public async Task SaveStagingRecord_InvalidModelState_WithDollarDotPrefix_StripsPrefix()
        {
            _controller.ModelState.AddModelError("$.Hours", "Invalid");

            var result = await _controller.SaveStagingRecord(new StagingMonthlyTimeItem());

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.False((bool)success!);
        }

        [Fact]
        public async Task SaveStagingRecord_CreateNew_WhenServiceSucceeds_ReturnsAddedMessage()
        {
            var model = new StagingMonthlyTimeItem { Id = 0 };
            var dto = new StagingMonthlyTimeDto();
            _mapper.Map<StagingMonthlyTimeDto>(model).Returns(dto);
            _monthlyTimeService.CreateStagingAsync(dto)
                .Returns(ApiResponseDto<StagingMonthlyTimeDto>.SuccessResponse(new StagingMonthlyTimeDto()));

            var result = await _controller.SaveStagingRecord(model);

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.True((bool)success!);
            var message = json.Value?.GetType().GetProperty("message")?.GetValue(json.Value);
            Assert.Equal("Staging record added successfully.", message);
        }

        [Fact]
        public async Task SaveStagingRecord_UpdateExisting_WhenServiceSucceeds_ReturnsUpdatedMessage()
        {
            var model = new StagingMonthlyTimeItem { Id = 5, NameUpdating = false };
            var dto = new StagingMonthlyTimeDto();
            _mapper.Map<StagingMonthlyTimeDto>(model).Returns(dto);
            _monthlyTimeService.GetStagingByIdAsync(5)
                .Returns(ApiResponseDto<StagingMonthlyTimeDto>.SuccessResponse(new StagingMonthlyTimeDto { WorkGroup = "WG1", PactStaffId = "S1" }));
            _monthlyTimeService.UpdateStagingAsync(5, dto)
                .Returns(ApiResponseDto<StagingMonthlyTimeDto>.SuccessResponse(new StagingMonthlyTimeDto()));

            var result = await _controller.SaveStagingRecord(model);

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.True((bool)success!);
            var message = json.Value?.GetType().GetProperty("message")?.GetValue(json.Value);
            Assert.Equal("Staging record updated successfully.", message);
        }

        [Fact]
        public async Task SaveStagingRecord_WhenServiceFails_ReturnsFailure()
        {
            var model = new StagingMonthlyTimeItem { Id = 0 };
            var dto = new StagingMonthlyTimeDto();
            _mapper.Map<StagingMonthlyTimeDto>(model).Returns(dto);
            _monthlyTimeService.CreateStagingAsync(dto)
                .Returns(ApiResponseDto<StagingMonthlyTimeDto>.FailureResponse(
                    [new ApiErrorDto { Code = "ERR", Message = "fail" }], new ApiMetaDto()));

            var result = await _controller.SaveStagingRecord(model);

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.False((bool)success!);
            var message = json.Value?.GetType().GetProperty("message")?.GetValue(json.Value);
            Assert.Equal("Failed to save staging record.", message);
        }

        [Fact]
        public async Task SaveStagingRecord_WhenServiceFailsWithNullErrors_ReturnsEmptyErrors()
        {
            var model = new StagingMonthlyTimeItem { Id = 0 };
            var dto = new StagingMonthlyTimeDto();
            _mapper.Map<StagingMonthlyTimeDto>(model).Returns(dto);
            var failResponse = new ApiResponseDto<StagingMonthlyTimeDto>
            {
                Success = false,
                Errors = null
            };
            _monthlyTimeService.CreateStagingAsync(dto).Returns(failResponse);

            var result = await _controller.SaveStagingRecord(model);

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.False((bool)success!);
        }

        [Fact]
        public async Task SaveStagingRecord_UpdateWithNameUpdating_WhenNameChanged_AppliesBulkUpdate()
        {
            var model = new StagingMonthlyTimeItem { Id = 5, NameUpdating = true, Name = "NewName", PactStaffId = "S2", PactId = "P2" };
            var dto = new StagingMonthlyTimeDto();
            _mapper.Map<StagingMonthlyTimeDto>(model).Returns(dto);
            _monthlyTimeService.GetStagingByIdAsync(5)
                .Returns(ApiResponseDto<StagingMonthlyTimeDto>.SuccessResponse(
                    new StagingMonthlyTimeDto { WorkGroup = "WG1", PactStaffId = "S1", Name = "OldName" }));
            _monthlyTimeService.UpdateStagingAsync(5, dto)
                .Returns(ApiResponseDto<StagingMonthlyTimeDto>.SuccessResponse(new StagingMonthlyTimeDto()));
            _monthlyTimeService.BulkUpdateStagingNamesAsync(Arg.Any<BulkUpdateStagingMonthlyTimeNamesDto>())
                .Returns(ApiResponseDto<BulkUpdateStagingMonthlyTimeNamesResultDto>.SuccessResponse(
                    new BulkUpdateStagingMonthlyTimeNamesResultDto { UpdatedCount = 3 }));

            var result = await _controller.SaveStagingRecord(model);

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.True((bool)success!);
            var message = (string)json.Value?.GetType().GetProperty("message")?.GetValue(json.Value)!;
            Assert.Contains("3 related record(s)", message);
        }

        [Fact]
        public async Task SaveStagingRecord_UpdateWithNameUpdating_WhenBulkUpdateFails_ReturnsFailure()
        {
            var model = new StagingMonthlyTimeItem { Id = 5, NameUpdating = true, Name = "NewName", PactStaffId = "S2", PactId = "P2" };
            var dto = new StagingMonthlyTimeDto();
            _mapper.Map<StagingMonthlyTimeDto>(model).Returns(dto);
            _monthlyTimeService.GetStagingByIdAsync(5)
                .Returns(ApiResponseDto<StagingMonthlyTimeDto>.SuccessResponse(
                    new StagingMonthlyTimeDto { WorkGroup = "WG1", PactStaffId = "S1", Name = "OldName" }));
            _monthlyTimeService.UpdateStagingAsync(5, dto)
                .Returns(ApiResponseDto<StagingMonthlyTimeDto>.SuccessResponse(new StagingMonthlyTimeDto()));
            _monthlyTimeService.BulkUpdateStagingNamesAsync(Arg.Any<BulkUpdateStagingMonthlyTimeNamesDto>())
                .Returns(ApiResponseDto<BulkUpdateStagingMonthlyTimeNamesResultDto>.FailureResponse(
                    [new ApiErrorDto { Code = "ERR", Message = "bulk fail" }], new ApiMetaDto()));

            var result = await _controller.SaveStagingRecord(model);

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.False((bool)success!);
            var message = json.Value?.GetType().GetProperty("message")?.GetValue(json.Value);
            Assert.Equal("Failed to apply name updates to related records.", message);
        }

        [Fact]
        public async Task SaveStagingRecord_UpdateWithNameUpdating_WhenBulkUpdateFailsWithNullErrors_ReturnsEmptyErrors()
        {
            var model = new StagingMonthlyTimeItem { Id = 5, NameUpdating = true, Name = "NewName", PactStaffId = "S2", PactId = "P2" };
            var dto = new StagingMonthlyTimeDto();
            _mapper.Map<StagingMonthlyTimeDto>(model).Returns(dto);
            _monthlyTimeService.GetStagingByIdAsync(5)
                .Returns(ApiResponseDto<StagingMonthlyTimeDto>.SuccessResponse(
                    new StagingMonthlyTimeDto { WorkGroup = "WG1", PactStaffId = "S1", Name = "OldName" }));
            _monthlyTimeService.UpdateStagingAsync(5, dto)
                .Returns(ApiResponseDto<StagingMonthlyTimeDto>.SuccessResponse(new StagingMonthlyTimeDto()));
            var failResponse = new ApiResponseDto<BulkUpdateStagingMonthlyTimeNamesResultDto>
            {
                Success = false,
                Errors = null
            };
            _monthlyTimeService.BulkUpdateStagingNamesAsync(Arg.Any<BulkUpdateStagingMonthlyTimeNamesDto>()).Returns(failResponse);

            var result = await _controller.SaveStagingRecord(model);

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.False((bool)success!);
        }

        [Fact]
        public async Task SaveStagingRecord_UpdateWithNameUpdating_WhenZeroUpdated_ReturnsDefaultMessage()
        {
            var model = new StagingMonthlyTimeItem { Id = 5, NameUpdating = true, Name = "NewName", PactStaffId = "S2", PactId = "P2" };
            var dto = new StagingMonthlyTimeDto();
            _mapper.Map<StagingMonthlyTimeDto>(model).Returns(dto);
            _monthlyTimeService.GetStagingByIdAsync(5)
                .Returns(ApiResponseDto<StagingMonthlyTimeDto>.SuccessResponse(
                    new StagingMonthlyTimeDto { WorkGroup = "WG1", PactStaffId = "S1", Name = "OldName" }));
            _monthlyTimeService.UpdateStagingAsync(5, dto)
                .Returns(ApiResponseDto<StagingMonthlyTimeDto>.SuccessResponse(new StagingMonthlyTimeDto()));
            _monthlyTimeService.BulkUpdateStagingNamesAsync(Arg.Any<BulkUpdateStagingMonthlyTimeNamesDto>())
                .Returns(ApiResponseDto<BulkUpdateStagingMonthlyTimeNamesResultDto>.SuccessResponse(
                    new BulkUpdateStagingMonthlyTimeNamesResultDto { UpdatedCount = 0 }));

            var result = await _controller.SaveStagingRecord(model);

            var json = Assert.IsType<JsonResult>(result);
            var message = json.Value?.GetType().GetProperty("message")?.GetValue(json.Value);
            Assert.Equal("Staging record updated successfully.", message);
        }

        [Fact]
        public async Task SaveStagingRecord_UpdateWithNameUpdating_WhenBulkUpdateDataIsNull_ReturnsDefaultMessage()
        {
            var model = new StagingMonthlyTimeItem { Id = 5, NameUpdating = true, Name = "NewName", PactStaffId = "S2", PactId = "P2" };
            var dto = new StagingMonthlyTimeDto();
            _mapper.Map<StagingMonthlyTimeDto>(model).Returns(dto);
            _monthlyTimeService.GetStagingByIdAsync(5)
                .Returns(ApiResponseDto<StagingMonthlyTimeDto>.SuccessResponse(
                    new StagingMonthlyTimeDto { WorkGroup = "WG1", PactStaffId = "S1", Name = "OldName" }));
            _monthlyTimeService.UpdateStagingAsync(5, dto)
                .Returns(ApiResponseDto<StagingMonthlyTimeDto>.SuccessResponse(new StagingMonthlyTimeDto()));
            var response = new ApiResponseDto<BulkUpdateStagingMonthlyTimeNamesResultDto> { Success = true, Data = null };
            _monthlyTimeService.BulkUpdateStagingNamesAsync(Arg.Any<BulkUpdateStagingMonthlyTimeNamesDto>()).Returns(response);

            var result = await _controller.SaveStagingRecord(model);

            var json = Assert.IsType<JsonResult>(result);
            var message = json.Value?.GetType().GetProperty("message")?.GetValue(json.Value);
            Assert.Equal("Staging record updated successfully.", message);
        }

        [Fact]
        public async Task SaveStagingRecord_UpdateWithNameUpdating_WhenNameSame_SkipsBulkUpdate()
        {
            var model = new StagingMonthlyTimeItem { Id = 5, NameUpdating = true, Name = "SameName", PactStaffId = "S1", PactId = "P1" };
            var dto = new StagingMonthlyTimeDto();
            _mapper.Map<StagingMonthlyTimeDto>(model).Returns(dto);
            _monthlyTimeService.GetStagingByIdAsync(5)
                .Returns(ApiResponseDto<StagingMonthlyTimeDto>.SuccessResponse(
                    new StagingMonthlyTimeDto { WorkGroup = "WG1", PactStaffId = "S1", Name = "SameName" }));
            _monthlyTimeService.UpdateStagingAsync(5, dto)
                .Returns(ApiResponseDto<StagingMonthlyTimeDto>.SuccessResponse(new StagingMonthlyTimeDto()));

            var result = await _controller.SaveStagingRecord(model);

            var json = Assert.IsType<JsonResult>(result);
            var message = json.Value?.GetType().GetProperty("message")?.GetValue(json.Value);
            Assert.Equal("Staging record updated successfully.", message);
            await _monthlyTimeService.DidNotReceive().BulkUpdateStagingNamesAsync(Arg.Any<BulkUpdateStagingMonthlyTimeNamesDto>());
        }

        [Fact]
        public async Task SaveStagingRecord_UpdateWithNameUpdating_WhenExistingRecordIsNull_SkipsBulkUpdate()
        {
            var model = new StagingMonthlyTimeItem { Id = 5, NameUpdating = true, Name = "NewName", PactStaffId = "S2" };
            var dto = new StagingMonthlyTimeDto();
            _mapper.Map<StagingMonthlyTimeDto>(model).Returns(dto);
            _monthlyTimeService.GetStagingByIdAsync(5)
                .Returns(ApiResponseDto<StagingMonthlyTimeDto>.FailureResponse([], new ApiMetaDto()));
            _monthlyTimeService.UpdateStagingAsync(5, dto)
                .Returns(ApiResponseDto<StagingMonthlyTimeDto>.SuccessResponse(new StagingMonthlyTimeDto()));

            var result = await _controller.SaveStagingRecord(model);

            var json = Assert.IsType<JsonResult>(result);
            var message = json.Value?.GetType().GetProperty("message")?.GetValue(json.Value);
            Assert.Equal("Staging record updated successfully.", message);
            await _monthlyTimeService.DidNotReceive().BulkUpdateStagingNamesAsync(Arg.Any<BulkUpdateStagingMonthlyTimeNamesDto>());
        }

        [Fact]
        public async Task SaveStagingRecord_UpdateWithNameUpdating_WhenExistingWorkGroupIsNull_SkipsBulkUpdate()
        {
            var model = new StagingMonthlyTimeItem { Id = 5, NameUpdating = true, Name = "NewName", PactStaffId = "S2" };
            var dto = new StagingMonthlyTimeDto();
            _mapper.Map<StagingMonthlyTimeDto>(model).Returns(dto);
            _monthlyTimeService.GetStagingByIdAsync(5)
                .Returns(ApiResponseDto<StagingMonthlyTimeDto>.SuccessResponse(
                    new StagingMonthlyTimeDto { WorkGroup = null, PactStaffId = "S1", Name = "OldName" }));
            _monthlyTimeService.UpdateStagingAsync(5, dto)
                .Returns(ApiResponseDto<StagingMonthlyTimeDto>.SuccessResponse(new StagingMonthlyTimeDto()));

            var result = await _controller.SaveStagingRecord(model);

            var json = Assert.IsType<JsonResult>(result);
            var message = json.Value?.GetType().GetProperty("message")?.GetValue(json.Value);
            Assert.Equal("Staging record updated successfully.", message);
            await _monthlyTimeService.DidNotReceive().BulkUpdateStagingNamesAsync(Arg.Any<BulkUpdateStagingMonthlyTimeNamesDto>());
        }

        [Fact]
        public async Task SaveStagingRecord_UpdateWithNameUpdating_WhenExistingPactStaffIdIsNull_SkipsBulkUpdate()
        {
            var model = new StagingMonthlyTimeItem { Id = 5, NameUpdating = true, Name = "NewName", PactStaffId = "S2" };
            var dto = new StagingMonthlyTimeDto();
            _mapper.Map<StagingMonthlyTimeDto>(model).Returns(dto);
            _monthlyTimeService.GetStagingByIdAsync(5)
                .Returns(ApiResponseDto<StagingMonthlyTimeDto>.SuccessResponse(
                    new StagingMonthlyTimeDto { WorkGroup = "WG1", PactStaffId = null, Name = "OldName" }));
            _monthlyTimeService.UpdateStagingAsync(5, dto)
                .Returns(ApiResponseDto<StagingMonthlyTimeDto>.SuccessResponse(new StagingMonthlyTimeDto()));

            var result = await _controller.SaveStagingRecord(model);

            var json = Assert.IsType<JsonResult>(result);
            var message = json.Value?.GetType().GetProperty("message")?.GetValue(json.Value);
            Assert.Equal("Staging record updated successfully.", message);
            await _monthlyTimeService.DidNotReceive().BulkUpdateStagingNamesAsync(Arg.Any<BulkUpdateStagingMonthlyTimeNamesDto>());
        }

        [Fact]
        public async Task SaveStagingRecord_UpdateWithNameUpdatingFalse_SkipsBulkUpdate()
        {
            var model = new StagingMonthlyTimeItem { Id = 5, NameUpdating = false, Name = "NewName", PactStaffId = "S2" };
            var dto = new StagingMonthlyTimeDto();
            _mapper.Map<StagingMonthlyTimeDto>(model).Returns(dto);
            _monthlyTimeService.GetStagingByIdAsync(5)
                .Returns(ApiResponseDto<StagingMonthlyTimeDto>.SuccessResponse(
                    new StagingMonthlyTimeDto { WorkGroup = "WG1", PactStaffId = "S1", Name = "OldName" }));
            _monthlyTimeService.UpdateStagingAsync(5, dto)
                .Returns(ApiResponseDto<StagingMonthlyTimeDto>.SuccessResponse(new StagingMonthlyTimeDto()));

            var result = await _controller.SaveStagingRecord(model);

            var json = Assert.IsType<JsonResult>(result);
            var message = json.Value?.GetType().GetProperty("message")?.GetValue(json.Value);
            Assert.Equal("Staging record updated successfully.", message);
            await _monthlyTimeService.DidNotReceive().BulkUpdateStagingNamesAsync(Arg.Any<BulkUpdateStagingMonthlyTimeNamesDto>());
        }

        #endregion

        #region DeleteStagingRecord Tests

        [Fact]
        public async Task DeleteStagingRecord_InvalidModelState_ReturnsFailureJson()
        {
            _controller.ModelState.AddModelError("id", "error");

            var result = await _controller.DeleteStagingRecord(1);

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.False((bool)success!);
        }

        [Fact]
        public async Task DeleteStagingRecord_WhenServiceSucceeds_ReturnsSuccessTrue()
        {
            _monthlyTimeService.DeleteStagingAsync(1)
                .Returns(new ApiResponseDto<bool> { Success = true, Data = true });

            var result = await _controller.DeleteStagingRecord(1);

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.True((bool)success!);
        }

        [Fact]
        public async Task DeleteStagingRecord_WhenServiceFails_ReturnsSuccessFalse()
        {
            _monthlyTimeService.DeleteStagingAsync(1)
                .Returns(new ApiResponseDto<bool> { Success = false, Data = false });

            var result = await _controller.DeleteStagingRecord(1);

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.False((bool)success!);
        }

        [Fact]
        public async Task DeleteStagingRecord_WhenSuccessButDataFalse_ReturnsSuccessFalse()
        {
            _monthlyTimeService.DeleteStagingAsync(1)
                .Returns(new ApiResponseDto<bool> { Success = true, Data = false });

            var result = await _controller.DeleteStagingRecord(1);

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.False((bool)success!);
        }

        #endregion

        #region DeleteAllStagingRecords Tests

        [Fact]
        public async Task DeleteAllStagingRecords_WhenServiceSucceeds_ReturnsSuccess()
        {
            _monthlyTimeService.DeleteAllStagingByUserAsync()
                .Returns(new ApiResponseDto<bool> { Success = true, Data = true, Errors = [new ApiErrorDto { Message = "Deleted all" }] });

            var result = await _controller.DeleteAllStagingRecords();

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.True((bool)success!);
            var message = json.Value?.GetType().GetProperty("message")?.GetValue(json.Value);
            Assert.Equal("Deleted all", message);
        }

        [Fact]
        public async Task DeleteAllStagingRecords_WhenServiceFails_ReturnsFailure()
        {
            _monthlyTimeService.DeleteAllStagingByUserAsync()
                .Returns(new ApiResponseDto<bool> { Success = false, Data = false, Errors = null });

            var result = await _controller.DeleteAllStagingRecords();

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.False((bool)success!);
            var message = json.Value?.GetType().GetProperty("message")?.GetValue(json.Value);
            Assert.Equal("Failed to delete staging records.", message);
        }

        [Fact]
        public async Task DeleteAllStagingRecords_WhenSuccessButDataFalse_ReturnsSuccessFalse()
        {
            _monthlyTimeService.DeleteAllStagingByUserAsync()
                .Returns(new ApiResponseDto<bool> { Success = true, Data = false, Errors = [new ApiErrorDto { Message = "No records" }] });

            var result = await _controller.DeleteAllStagingRecords();

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.False((bool)success!);
        }

        #endregion

        #region DeleteFailedStagingRecords Tests

        [Fact]
        public async Task DeleteFailedStagingRecords_WhenServiceSucceeds_ReturnsSuccess()
        {
            _monthlyTimeService.DeleteFailedStagingByUserAsync()
                .Returns(new ApiResponseDto<bool> { Success = true, Data = true, Errors = [new ApiErrorDto { Message = "Deleted failed" }] });

            var result = await _controller.DeleteFailedStagingRecords();

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.True((bool)success!);
            var message = json.Value?.GetType().GetProperty("message")?.GetValue(json.Value);
            Assert.Equal("Deleted failed", message);
        }

        [Fact]
        public async Task DeleteFailedStagingRecords_WhenServiceFails_ReturnsFailure()
        {
            _monthlyTimeService.DeleteFailedStagingByUserAsync()
                .Returns(new ApiResponseDto<bool> { Success = false, Data = false, Errors = null });

            var result = await _controller.DeleteFailedStagingRecords();

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.False((bool)success!);
            var message = json.Value?.GetType().GetProperty("message")?.GetValue(json.Value);
            Assert.Equal("Failed to delete failed imported records.", message);
        }

        [Fact]
        public async Task DeleteFailedStagingRecords_WhenSuccessButDataFalse_ReturnsSuccessFalse()
        {
            _monthlyTimeService.DeleteFailedStagingByUserAsync()
                .Returns(new ApiResponseDto<bool> { Success = true, Data = false, Errors = [new ApiErrorDto { Message = "No failed records" }] });

            var result = await _controller.DeleteFailedStagingRecords();

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.False((bool)success!);
        }

        #endregion

        #region Import Tests

        [Fact]
        public async Task Import_InvalidModelState_ReturnsBadRequest()
        {
            _controller.ModelState.AddModelError("file", "error");

            var result = await _controller.Import(null!, 1);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid request data.", badRequest.Value);
        }

        [Fact]
        public async Task Import_NullFile_ReturnsFailureJson()
        {
            var result = await _controller.Import(null!, 1);

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.False((bool)success!);
            var message = json.Value?.GetType().GetProperty("message")?.GetValue(json.Value);
            Assert.Equal("Please select an Excel file to import.", message);
        }

        [Fact]
        public async Task Import_EmptyFile_ReturnsFailureJson()
        {
            var file = Substitute.For<IFormFile>();
            file.Length.Returns(0);

            var result = await _controller.Import(file, 1);

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.False((bool)success!);
            var message = json.Value?.GetType().GetProperty("message")?.GetValue(json.Value);
            Assert.Equal("Please select an Excel file to import.", message);
        }

        [Fact]
        public async Task Import_WhenServiceSucceeds_ReturnsSuccessWithCounts()
        {
            var file = Substitute.For<IFormFile>();
            file.Length.Returns(100);
            var importResult = new MonthlyTimeImportResultDto { ImportedCount = 10, PassedCount = 8, FailedCount = 2, Message = "done" };
            _monthlyTimeService.ImportMonthlyTimeAsync(file, 1)
                .Returns(ApiResponseDto<MonthlyTimeImportResultDto>.SuccessResponse(importResult));

            var result = await _controller.Import(file, 1);

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.True((bool)success!);
            var importedCount = json.Value?.GetType().GetProperty("importedCount")?.GetValue(json.Value);
            Assert.Equal(10, importedCount);
        }

        [Fact]
        public async Task Import_WhenServiceFails_ReturnsFailureWithErrorMessage()
        {
            var file = Substitute.For<IFormFile>();
            file.Length.Returns(100);
            _monthlyTimeService.ImportMonthlyTimeAsync(file, 1)
                .Returns(ApiResponseDto<MonthlyTimeImportResultDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Bad file" }], new ApiMetaDto()));

            var result = await _controller.Import(file, 1);

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.False((bool)success!);
            var message = json.Value?.GetType().GetProperty("message")?.GetValue(json.Value);
            Assert.Equal("Bad file", message);
        }

        [Fact]
        public async Task Import_WhenServiceFailsWithNullErrors_ReturnsDefaultMessage()
        {
            var file = Substitute.For<IFormFile>();
            file.Length.Returns(100);
            var failResponse = new ApiResponseDto<MonthlyTimeImportResultDto>
            {
                Success = false,
                Errors = null
            };
            _monthlyTimeService.ImportMonthlyTimeAsync(file, 1).Returns(failResponse);

            var result = await _controller.Import(file, 1);

            var json = Assert.IsType<JsonResult>(result);
            var message = json.Value?.GetType().GetProperty("message")?.GetValue(json.Value);
            Assert.Equal("Import failed.", message);
        }

        [Fact]
        public async Task Import_WhenServiceSucceedsButDataNull_ReturnsFailureDefaultMessage()
        {
            var file = Substitute.For<IFormFile>();
            file.Length.Returns(100);
            var response = new ApiResponseDto<MonthlyTimeImportResultDto> { Success = true, Data = null };
            _monthlyTimeService.ImportMonthlyTimeAsync(file, 1).Returns(response);

            var result = await _controller.Import(file, 1);

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.False((bool)success!);
        }

        #endregion

        #region Validate Tests

        [Fact]
        public async Task Validate_WhenServiceSucceeds_ReturnsSuccessWithCounts()
        {
            var validateResult = new MonthlyTimeValidateResultDto { PassedCount = 10, FailedCount = 2, Message = "ok" };
            _monthlyTimeService.ValidateStagingAsync()
                .Returns(ApiResponseDto<MonthlyTimeValidateResultDto>.SuccessResponse(validateResult));

            var result = await _controller.Validate();

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.True((bool)success!);
            var passedCount = json.Value?.GetType().GetProperty("passedCount")?.GetValue(json.Value);
            Assert.Equal(10, passedCount);
        }

        [Fact]
        public async Task Validate_WhenServiceFails_ReturnsFailureWithErrorMessage()
        {
            _monthlyTimeService.ValidateStagingAsync()
                .Returns(ApiResponseDto<MonthlyTimeValidateResultDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Validate error" }], new ApiMetaDto()));

            var result = await _controller.Validate();

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.False((bool)success!);
            var message = json.Value?.GetType().GetProperty("message")?.GetValue(json.Value);
            Assert.Equal("Validate error", message);
        }

        [Fact]
        public async Task Validate_WhenServiceFailsWithNullErrors_ReturnsDefaultMessage()
        {
            var failResponse = new ApiResponseDto<MonthlyTimeValidateResultDto>
            {
                Success = false,
                Errors = null
            };
            _monthlyTimeService.ValidateStagingAsync().Returns(failResponse);

            var result = await _controller.Validate();

            var json = Assert.IsType<JsonResult>(result);
            var message = json.Value?.GetType().GetProperty("message")?.GetValue(json.Value);
            Assert.Equal("Validation failed.", message);
        }

        [Fact]
        public async Task Validate_WhenServiceSucceedsButDataNull_ReturnsFailure()
        {
            var response = new ApiResponseDto<MonthlyTimeValidateResultDto> { Success = true, Data = null };
            _monthlyTimeService.ValidateStagingAsync().Returns(response);

            var result = await _controller.Validate();

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.False((bool)success!);
        }

        #endregion

        #region MakeLive Tests

        [Fact]
        public async Task MakeLive_WhenServiceSucceeds_ReturnsSuccessWithCounts()
        {
            var makeLiveResult = new MonthlyTimeMakeLiveResultDto { ProcessedCount = 10, ImportedCount = 8, FailedCount = 2, Message = "ok" };
            _monthlyTimeService.MakeLiveAsync()
                .Returns(ApiResponseDto<MonthlyTimeMakeLiveResultDto>.SuccessResponse(makeLiveResult));

            var result = await _controller.MakeLive();

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.True((bool)success!);
            var processedCount = json.Value?.GetType().GetProperty("processedCount")?.GetValue(json.Value);
            Assert.Equal(10, processedCount);
        }

        [Fact]
        public async Task MakeLive_WhenServiceFails_ReturnsFailureWithErrorMessage()
        {
            _monthlyTimeService.MakeLiveAsync()
                .Returns(ApiResponseDto<MonthlyTimeMakeLiveResultDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Make live error" }], new ApiMetaDto()));

            var result = await _controller.MakeLive();

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.False((bool)success!);
            var message = json.Value?.GetType().GetProperty("message")?.GetValue(json.Value);
            Assert.Equal("Make live error", message);
        }

        [Fact]
        public async Task MakeLive_WhenServiceFailsWithNullErrors_ReturnsDefaultMessage()
        {
            var failResponse = new ApiResponseDto<MonthlyTimeMakeLiveResultDto>
            {
                Success = false,
                Errors = null
            };
            _monthlyTimeService.MakeLiveAsync().Returns(failResponse);

            var result = await _controller.MakeLive();

            var json = Assert.IsType<JsonResult>(result);
            var message = json.Value?.GetType().GetProperty("message")?.GetValue(json.Value);
            Assert.Equal("Make live failed.", message);
        }

        [Fact]
        public async Task MakeLive_WhenServiceSucceedsButDataNull_ReturnsFailure()
        {
            var response = new ApiResponseDto<MonthlyTimeMakeLiveResultDto> { Success = true, Data = null };
            _monthlyTimeService.MakeLiveAsync().Returns(response);

            var result = await _controller.MakeLive();

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.False((bool)success!);
        }

        #endregion

        #region ExportStaging Tests

        [Fact]
        public async Task ExportStaging_InvalidModelState_ReturnsBadRequest()
        {
            _controller.ModelState.AddModelError("key", "error");

            var result = await _controller.ExportStaging(null);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid request data.", badRequest.Value);
        }

        [Fact]
        public async Task ExportStaging_WhenServiceFails_ReturnsNotFound()
        {
            _monthlyTimeService.GetStagingAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<bool?>())
                .Returns(ApiResponseDto<List<StagingMonthlyTimeDto>>.FailureResponse([], new ApiMetaDto()));

            var result = await _controller.ExportStaging(null);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ExportStaging_WhenDataIsNull_ReturnsNotFound()
        {
            var response = new ApiResponseDto<List<StagingMonthlyTimeDto>> { Success = true, Data = null };
            _monthlyTimeService.GetStagingAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<bool?>()).Returns(response);

            var result = await _controller.ExportStaging(null);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ExportStaging_WhenServiceSucceeds_ReturnsExcelFile()
        {
            var data = new List<StagingMonthlyTimeDto> { new() { Id = 1 } };
            _monthlyTimeService.GetStagingAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<bool?>())
                .Returns(ApiResponseDto<List<StagingMonthlyTimeDto>>.SuccessResponse(data));
            _mapper.Map<List<StagingMonthlyTimeExportItem>>(data).Returns([new StagingMonthlyTimeExportItem()]);
            _excelExportService.ExportToExcel(Arg.Any<List<StagingMonthlyTimeExportItem>>(), "MonthlyTime").Returns([0x50, 0x4B]);

            var result = await _controller.ExportStaging(true);

            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileResult.ContentType);
            Assert.Contains("ExportedTS_", fileResult.FileDownloadName);
            Assert.EndsWith(".xlsx", fileResult.FileDownloadName);
        }

        #endregion
    }
}
