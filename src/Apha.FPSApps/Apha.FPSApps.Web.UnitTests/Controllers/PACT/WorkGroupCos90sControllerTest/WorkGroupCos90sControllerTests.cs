using Apha.Common.Utilities.ExcelExport;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PACT.Controllers;
using Apha.FPSApps.Web.Areas.PACT.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.PACT.WorkGroupCos90sControllerTest
{
    public class WorkGroupCos90sControllerTests
    {
        private readonly IWorkGroupService _workGroupService;
        private readonly IWorkGroupReportService _workGroupReportService;
        private readonly IExcelExportService _excelExportService;
        private readonly IMonthHourService _monthHourService;
        private readonly IProfitCentreService _profitCentreService;
        private readonly ICalenderMonthService _calenderMonthService;
        private readonly IEmployeeService _employeeService;
        private readonly IMapper _mapper;
        private readonly WorkGroupCos90sController _controller;

        public WorkGroupCos90sControllerTests()
        {
            _workGroupService = Substitute.For<IWorkGroupService>();
            _workGroupReportService = Substitute.For<IWorkGroupReportService>();
            _excelExportService = Substitute.For<IExcelExportService>();
            _monthHourService = Substitute.For<IMonthHourService>();
            _profitCentreService = Substitute.For<IProfitCentreService>();
            _calenderMonthService = Substitute.For<ICalenderMonthService>();
            _employeeService = Substitute.For<IEmployeeService>();
            _mapper = Substitute.For<IMapper>();

            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string> { Page = 1, PageSize = 10, Filter = "{}" });
            _mapper.Map<List<MonthHourRowItem>>(Arg.Any<List<MonthHourDto>>())
                .Returns(new List<MonthHourRowItem>());

            _controller = new WorkGroupCos90sController(
                _workGroupService,
                _workGroupReportService,
                _excelExportService,
                _monthHourService,
                _profitCentreService,
                _calenderMonthService,
                _employeeService,
                _mapper);
        }

        private static JsonElement ToJson(object? value)
        {
            var json = JsonSerializer.Serialize(value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        [Fact]
        public async Task Index_WithProfitCentreData_ReturnsViewWithSelectedProfitCentreAndGrid()
        {
            // Arrange
            _profitCentreService.GetAllProfitCentresAsync().Returns(
                ApiResponseDto<IEnumerable<ProfitCentreDto>>.SuccessResponse(
                    [new ProfitCentreDto { ProfitCentreId = "PC001" }]));
            _employeeService.GetActivePactStaffAsync().Returns(
                ApiResponseDto<List<PactStaffDto>>.SuccessResponse(
                    [new PactStaffDto { PactId = "S001", Name = "John", WorkGroupGrade = "WG1" }]));
            _calenderMonthService.GetCalenderMonthsAsync().Returns(
                ApiResponseDto<List<CalenderMonthDto>>.SuccessResponse(
                    [new CalenderMonthDto { MonthNumber = 1, MonthName = "Jan", AccntsPeriod = 1 }]));
            _monthHourService.GetDistinctYearsAsync().Returns(
                ApiResponseDto<List<short>>.SuccessResponse([2025]));
            _workGroupService.GetWorkGroupsByProfitCentreAsync(Arg.Any<QueryParameters<string>>(), "PC001").Returns(
                ApiResponseDto<List<WorkGroupDto>>.SuccessResponse(
                    [new WorkGroupDto { WorkGroupName = "WG1", ProfitCentre = "PC001", Cos90 = 1 }]));

            // Act
            var result = await _controller.Index();

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupCos90SViewModel>(view.Model);
            Assert.Equal("PC001", model.SelectedProfitCentre);
            Assert.NotNull(model.WorkGroupGrid);
            Assert.Single(model.WorkGroupGrid.Data);
        }

        [Fact]
        public async Task Index_WithoutProfitCentres_ReturnsViewWithEmptyWorkGroupGrid()
        {
            // Arrange
            _profitCentreService.GetAllProfitCentresAsync().Returns(
                ApiResponseDto<IEnumerable<ProfitCentreDto>>.SuccessResponse([]));
            _employeeService.GetActivePactStaffAsync().Returns(ApiResponseDto<List<PactStaffDto>>.SuccessResponse([]));
            _calenderMonthService.GetCalenderMonthsAsync().Returns(ApiResponseDto<List<CalenderMonthDto>>.SuccessResponse([]));
            _monthHourService.GetDistinctYearsAsync().Returns(ApiResponseDto<List<short>>.SuccessResponse([]));

            // Act
            var result = await _controller.Index();

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupCos90SViewModel>(view.Model);
            Assert.Empty(model.WorkGroupGrid.Data);
        }

        [Fact]
        public async Task LoadWorkGroupGrid_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("request", "invalid");

            // Act
            var result = await _controller.LoadWorkGroupGrid(new PaginationFilter<string>(), "PC001");

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task LoadWorkGroupGrid_WithoutProfitCentre_ReturnsEmptyDataGridPartial()
        {
            // Act
            var result = await _controller.LoadWorkGroupGrid(new PaginationFilter<string> { Filter = "{}" }, "");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            var model = Assert.IsType<DataGridConfig<WorkGroupCos90SWorkGroupItem>>(partial.Model);
            Assert.Empty(model.Data);
        }

        [Fact]
        public async Task SelectPCWorkGroups_ValidProfitCentreAndSuccess_ReturnsOk()
        {
            // Arrange
            _workGroupService.SetCos90ForProfitCentreWorkGroupsAsync("PC001", 1)
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.SelectPCWorkGroups("PC001");

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.True(ToJson(ok.Value).GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task ClearPCWorkGroups_WhenServiceFails_ReturnsServerError()
        {
            // Arrange
            _workGroupService.SetCos90ForProfitCentreWorkGroupsAsync("PC001", 0)
                .Returns(ApiResponseDto<bool>.FailureResponse([], new ApiMetaDto()));

            // Act
            var result = await _controller.ClearPCWorkGroups("PC001");

            // Assert
            var serverError = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, serverError.StatusCode);
        }

        [Fact]
        public async Task ClearAllWorkGroups_WhenServiceSucceeds_ReturnsOk()
        {
            // Arrange
            _workGroupService.SetCos90ForAllWorkGroupsAsync(0)
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.ClearAllWorkGroups();

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetFlaggedWorkGroups_WhenServiceFails_ReturnsServerError()
        {
            // Arrange
            _workGroupService.GetWorkGroupsFlaggedForCos90Async()
                .Returns(ApiResponseDto<List<WorkGroupDto>>.FailureResponse([], new ApiMetaDto()));

            // Act
            var result = await _controller.GetFlaggedWorkGroups();

            // Assert
            var serverError = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, serverError.StatusCode);
        }

        [Fact]
        public async Task GetMonthHourGrid_WithYear_ReturnsMonthHourPartialView()
        {
            // Arrange
            _monthHourService.GetAllAsync(Arg.Any<QueryParameters<string>>())
                .Returns(ApiResponseDto<List<MonthHourDto>>.SuccessResponse([]));

            // Act
            var result = await _controller.GetMonthHourGrid(2025);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_MonthHourGrid", partial.ViewName);
            Assert.IsType<DataGridConfig<MonthHourRowItem>>(partial.Model);
        }

        [Fact]
        public async Task LoadMonthHourGrid_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("request", "invalid");

            // Act
            var result = await _controller.LoadMonthHourGrid(new PaginationFilter<string>(), 2025);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetMonthHourYears_WithData_ReturnsOkWithYears()
        {
            // Arrange
            _monthHourService.GetDistinctYearsAsync().Returns(ApiResponseDto<List<short>>.SuccessResponse([2024, 2025]));

            // Act
            var result = await _controller.GetMonthHourYears();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var years = Assert.IsAssignableFrom<List<short>>(ok.Value);
            Assert.Equal(2, years.Count);
        }

        [Fact]
        public async Task GetPeriods_WithMonthData_ReturnsOrderedPeriods()
        {
            // Arrange
            _calenderMonthService.GetCalenderMonthsAsync().Returns(
                ApiResponseDto<List<CalenderMonthDto>>.SuccessResponse(
                [
                    new CalenderMonthDto { MonthNumber = 2, MonthName = "Feb", AccntsPeriod = 2 },
                    new CalenderMonthDto { MonthNumber = 1, MonthName = "Jan", AccntsPeriod = 1 }
                ]));

            // Act
            var result = await _controller.GetPeriods();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var json = ToJson(ok.Value);
            Assert.Equal(2, json.GetArrayLength());
            Assert.Equal(1, json[0].GetProperty("period").GetInt16());
        }

        [Fact]
        public async Task UpdateWorkGroupCos90_MissingRequiredFields_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.UpdateWorkGroupCos90("", "", 1);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateWorkGroupCos90_WorkGroupNotFound_ReturnsNotFound()
        {
            // Arrange
            _workGroupService.GetWorkGroupsByProfitCentreAsync(Arg.Any<QueryParameters<string>>(), "PC001")
                .Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse([]));

            // Act
            var result = await _controller.UpdateWorkGroupCos90("WG999", "PC001", 1);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task UpdateWorkGroupCos90_ValidInputAndSetSuccess_ReturnsOk()
        {
            // Arrange
            _workGroupService.GetWorkGroupsByProfitCentreAsync(Arg.Any<QueryParameters<string>>(), "PC001")
                .Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse([new WorkGroupDto { WorkGroupName = "WG1", ProfitCentre = "PC001" }]));
            _workGroupService.SetCos90ForWorkGroupAsync("PC001", "WG1", 1)
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.UpdateWorkGroupCos90("WG1", "PC001", 1);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task ExportCos90s_InvalidInputs_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.ExportCos90s(null, null, null, null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task ExportCos90s_WithNoRows_ReturnsBadRequest()
        {
            // Arrange
            _workGroupReportService.ExportCos90sAsync("PC001", 1, 2025, null)
                .Returns(ApiResponseDto<WorkGroupCos90SExportResultDto>.SuccessResponse(new WorkGroupCos90SExportResultDto { Rows = [] }));

            // Act
            var result = await _controller.ExportCos90s("PC001", 1, 2025, null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task ExportCos90s_WithRowsAndExcelContent_ReturnsFileResult()
        {
            // Arrange
            var rows = new List<WorkGroupCos90SExportRowDto>
            {
                new() { WorkGroupName = "WG1", StaffName = "John", Month = 1, Year = 2025 }
            };
            _workGroupReportService.ExportCos90sAsync("PC001", 1, 2025, null)
                .Returns(ApiResponseDto<WorkGroupCos90SExportResultDto>.SuccessResponse(
                    new WorkGroupCos90SExportResultDto { Rows = rows }));
            _excelExportService.BuildWorkGroupCos90sExcel(Arg.Any<IEnumerable<WorkGroupCos90SExportRow>>(), 1, 2025, "PC001", null)
                .Returns([1, 2, 3]);

            // Act
            var result = await _controller.ExportCos90s("PC001", 1, 2025, null);

            // Assert
            var file = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", file.ContentType);
            Assert.EndsWith("_Cos90.xlsx", file.FileDownloadName);
        }
    }
}
