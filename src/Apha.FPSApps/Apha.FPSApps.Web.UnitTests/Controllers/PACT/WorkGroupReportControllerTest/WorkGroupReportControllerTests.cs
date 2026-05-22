using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PACT.Controllers;
using Apha.FPSApps.Web.Areas.PACT.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.PACT.WorkGroupReportControllerTest
{
    public class WorkGroupReportControllerTests
    {
        private readonly IWorkGroupReportEmailService _emailSendService;
        private readonly IWorkGroupService _workGroupService;
        private readonly ICalenderMonthService _calenderMonthService;
        private readonly IProfitCentreService _profitCentreService;
        private readonly WorkGroupReportController _controller;

        public WorkGroupReportControllerTests()
        {
            _emailSendService     = Substitute.For<IWorkGroupReportEmailService>();
            _workGroupService     = Substitute.For<IWorkGroupService>();
            _calenderMonthService = Substitute.For<ICalenderMonthService>();
            _profitCentreService  = Substitute.For<IProfitCentreService>();

            _controller = new WorkGroupReportController(
                _emailSendService,
                _workGroupService,
                _calenderMonthService,
                _profitCentreService);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static JsonElement GetJsonElement(object? value)
        {
            var json = JsonSerializer.Serialize(value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        private void SetupProfitCentres(List<ProfitCentreSettingsDto> data)
        {
            _profitCentreService.GetAllProfitCentresAsync()
                .Returns(ApiResponseDto<IEnumerable<ProfitCentreSettingsDto>>.SuccessResponse(data));
        }

        private void SetupCalenderMonths(List<CalenderMonthDto> data)
        {
            _calenderMonthService.GetCalenderMonthsAsync()
                .Returns(ApiResponseDto<List<CalenderMonthDto>>.SuccessResponse(data));
        }

        private void SetupWorkGroupsByProfitCentre(string profitCentre, List<WorkGroupDto> data)
        {
            _workGroupService
                .GetWorkGroupsByProfitCentreAsync(Arg.Any<QueryParameters<string>>(), profitCentre)
                .Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse(data));
        }

        private void SetupProfitCentreSettings(string profitCentre, ProfitCentreSettingsDto settings)
        {
            _profitCentreService.GetProfitCentreSettingsAsync(profitCentre)
                .Returns(ApiResponseDto<ProfitCentreSettingsDto>.SuccessResponse(settings));
        }

        // ── Index ─────────────────────────────────────────────────────────────

        #region Index Tests

        [Fact]
        public async Task Index_WithProfitCentres_ReturnsViewWithFirstProfitCentreSelected()
        {
            // Arrange
            var profitCentres = new List<ProfitCentreSettingsDto>
            {
                new() { ProfitCentre = "PC001" },
                new() { ProfitCentre = "PC002" }
            };
            SetupProfitCentres(profitCentres);
            SetupCalenderMonths([]);
            SetupWorkGroupsByProfitCentre("PC001", []);
            SetupProfitCentreSettings("PC001", new ProfitCentreSettingsDto());

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupReportEmailViewModel>(viewResult.Model);
            Assert.Equal("PC001", model.SelectedProfitCentre);
            Assert.Equal(2, model.ProfitCentreOptions.Count);
        }

        [Fact]
        public async Task Index_WithNoProfitCentres_ReturnsViewWithEmptyGrid()
        {
            // Arrange
            SetupProfitCentres([]);
            SetupCalenderMonths([]);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupReportEmailViewModel>(viewResult.Model);
            Assert.Null(model.SelectedProfitCentre);
            Assert.Empty(model.WorkGroupGrid.Data!);
        }

        [Fact]
        public async Task Index_WithCalenderMonths_PopulatesCalenderMonthItems()
        {
            // Arrange
            var months = new List<CalenderMonthDto>
            {
                new() { MonthNumber = 1, MonthName = "January" },
                new() { MonthNumber = 2, MonthName = "February" }
            };
            SetupProfitCentres([]);
            SetupCalenderMonths(months);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupReportEmailViewModel>(viewResult.Model);
            Assert.Equal(2, model.CalenderMonthItems.Count);
        }

        [Fact]
        public async Task Index_WithProfitCentreSettings_AppliesSettingsToViewModel()
        {
            // Arrange
            SetupProfitCentres([new() { ProfitCentre = "PC001" }]);
            SetupCalenderMonths([]);
            SetupWorkGroupsByProfitCentre("PC001", []);
            SetupProfitCentreSettings("PC001", new ProfitCentreSettingsDto
            {
                Timesheet       = -1,
                Outputsheet     = -1,
                TimesheetLayout = 2
            });

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupReportEmailViewModel>(viewResult.Model);
            Assert.True(model.SendTimeSheet);
            Assert.True(model.SendOutputSheet);
            Assert.False(model.TimesheetLayoutFlat);
            Assert.True(model.TimesheetLayoutCrossTab);
        }

        [Fact]
        public async Task Index_WithProfitCentreServiceFailure_ReturnsEmptyProfitCentreOptions()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Code = "API_ERROR", Message = "Failed" } };
            _profitCentreService.GetAllProfitCentresAsync()
                .Returns(ApiResponseDto<IEnumerable<ProfitCentreSettingsDto>>.FailureResponse(errors, new ApiMetaDto()));
            SetupCalenderMonths([]);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupReportEmailViewModel>(viewResult.Model);
            Assert.Empty(model.ProfitCentreOptions);
        }

        #endregion

        // ── LoadWorkGroupGrid ────────────────────────────────────────────────

        #region LoadWorkGroupGrid Tests

        [Fact]
        public async Task LoadWorkGroupGrid_WithValidProfitCentre_ReturnsPartialViewWithGrid()
        {
            // Arrange
            const string profitCentre = "PC001";
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var workGroups = new List<WorkGroupDto>
            {
                new() { WorkGroupName = "WG001", ProfitCentre = profitCentre, SendEmail = 1 }
            };
            SetupWorkGroupsByProfitCentre(profitCentre, workGroups);

            // Act
            var result = await _controller.LoadWorkGroupGrid(request, profitCentre);

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialResult.ViewName);
            var model = Assert.IsType<DataGridConfig<WorkGroupEmailItem>>(partialResult.Model);
            Assert.Single(model.Data!);
        }

        [Fact]
        public async Task LoadWorkGroupGrid_WithNullProfitCentre_ReturnsEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };

            // Act
            var result = await _controller.LoadWorkGroupGrid(request, null);

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialResult.ViewName);
            var model = Assert.IsType<DataGridConfig<WorkGroupEmailItem>>(partialResult.Model);
            Assert.Empty(model.Data!);
        }

        [Fact]
        public async Task LoadWorkGroupGrid_WithEmptyProfitCentre_ReturnsEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };

            // Act
            var result = await _controller.LoadWorkGroupGrid(request, "   ");

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<WorkGroupEmailItem>>(partialResult.Model);
            Assert.Empty(model.Data!);
        }

        [Fact]
        public async Task LoadWorkGroupGrid_WhenServiceFails_ReturnsEmptyGrid()
        {
            // Arrange
            const string profitCentre = "PC001";
            var request = new PaginationFilter<string> { Filter = "{}" };
            var errors = new List<ApiErrorDto> { new() { Code = "API_ERROR", Message = "Failed" } };
            _workGroupService
                .GetWorkGroupsByProfitCentreAsync(Arg.Any<QueryParameters<string>>(), profitCentre)
                .Returns(ApiResponseDto<List<WorkGroupDto>>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.LoadWorkGroupGrid(request, profitCentre);

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<WorkGroupEmailItem>>(partialResult.Model);
            Assert.Empty(model.Data!);
        }

        #endregion

        // ── SelectPCWorkGroups ───────────────────────────────────────────────

        #region SelectPCWorkGroups Tests

        [Fact]
        public async Task SelectPCWorkGroups_WithValidProfitCentre_ReturnsOk()
        {
            // Arrange
            const string profitCentre = "PC001";
            _workGroupService.SetSendEmailForProfitCentreWorkGroupsAsync(profitCentre, 1)
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.SelectPCWorkGroups(profitCentre);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var json = GetJsonElement(okResult.Value);
            Assert.True(json.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task SelectPCWorkGroups_WithEmptyProfitCentre_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.SelectPCWorkGroups("");

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            await _workGroupService.DidNotReceive()
                .SetSendEmailForProfitCentreWorkGroupsAsync(Arg.Any<string>(), Arg.Any<short>());
        }

        [Fact]
        public async Task SelectPCWorkGroups_WhenServiceFails_Returns500()
        {
            // Arrange
            const string profitCentre = "PC001";
            var errors = new List<ApiErrorDto> { new() { Code = "API_ERROR", Message = "Failed" } };
            _workGroupService.SetSendEmailForProfitCentreWorkGroupsAsync(profitCentre, 1)
                .Returns(ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.SelectPCWorkGroups(profitCentre);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusResult.StatusCode);
        }

        #endregion

        // ── ClearPCWorkGroups ────────────────────────────────────────────────

        #region ClearPCWorkGroups Tests

        [Fact]
        public async Task ClearPCWorkGroups_WithValidProfitCentre_ReturnsOk()
        {
            // Arrange
            const string profitCentre = "PC001";
            _workGroupService.SetSendEmailForProfitCentreWorkGroupsAsync(profitCentre, 0)
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.ClearPCWorkGroups(profitCentre);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var json = GetJsonElement(okResult.Value);
            Assert.True(json.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task ClearPCWorkGroups_WithEmptyProfitCentre_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.ClearPCWorkGroups("");

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            await _workGroupService.DidNotReceive()
                .SetSendEmailForProfitCentreWorkGroupsAsync(Arg.Any<string>(), Arg.Any<short>());
        }

        [Fact]
        public async Task ClearPCWorkGroups_WhenServiceFails_Returns500()
        {
            // Arrange
            const string profitCentre = "PC001";
            var errors = new List<ApiErrorDto> { new() { Code = "API_ERROR", Message = "Failed" } };
            _workGroupService.SetSendEmailForProfitCentreWorkGroupsAsync(profitCentre, 0)
                .Returns(ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.ClearPCWorkGroups(profitCentre);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusResult.StatusCode);
        }

        #endregion

        // ── ClearAllWorkGroups ───────────────────────────────────────────────

        #region ClearAllWorkGroups Tests

        [Fact]
        public async Task ClearAllWorkGroups_WhenServiceSucceeds_ReturnsOk()
        {
            // Arrange
            _workGroupService.SetSendEmailForAllWorkGroupsAsync(0)
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.ClearAllWorkGroups();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var json = GetJsonElement(okResult.Value);
            Assert.True(json.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task ClearAllWorkGroups_WhenServiceFails_Returns500()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Code = "API_ERROR", Message = "Failed" } };
            _workGroupService.SetSendEmailForAllWorkGroupsAsync(0)
                .Returns(ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.ClearAllWorkGroups();

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusResult.StatusCode);
        }

        #endregion

        // ── Send ─────────────────────────────────────────────────────────────

        #region Send Tests

        [Fact]
        public async Task Send_WithSuccessResponse_ReturnsOkWithResults()
        {
            // Arrange
            const string profitCentre = "PC001";
            const short monthNumber = 3;
            var emailResults = new List<WorkGroupReportEmailResultDto>
            {
                new() { WorkGroupName = "WG001", EmailRecipient = "a@example.com", Status = "Sent" },
                new() { WorkGroupName = "WG002", EmailRecipient = "b@example.com", Status = "Sent" }
            };
            _emailSendService.SendEmailsAsync(profitCentre, monthNumber)
                .Returns(ApiResponseDto<List<WorkGroupReportEmailResultDto>>.SuccessResponse(emailResults));

            // Act
            var result = await _controller.Send(profitCentre, monthNumber);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var json = GetJsonElement(okResult.Value);
            Assert.True(json.GetProperty("success").GetBoolean());
            Assert.Equal(2, json.GetProperty("results").GetArrayLength());
        }

        [Fact]
        public async Task Send_WithNullData_ReturnsOkWithEmptyResults()
        {
            // Arrange
            const string profitCentre = "PC001";
            const short monthNumber = 1;
            var response = ApiResponseDto<List<WorkGroupReportEmailResultDto>>.SuccessResponse(null!);
            _emailSendService.SendEmailsAsync(profitCentre, monthNumber).Returns(response);

            // Act
            var result = await _controller.Send(profitCentre, monthNumber);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var json = GetJsonElement(okResult.Value);
            Assert.Equal(0, json.GetProperty("results").GetArrayLength());
        }

        [Fact]
        public async Task Send_WhenServiceFails_Returns500()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Code = "SEND_ERROR", Message = "Failed" } };
            _emailSendService.SendEmailsAsync(Arg.Any<string>(), Arg.Any<short>())
                .Returns(ApiResponseDto<List<WorkGroupReportEmailResultDto>>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.Send("PC001", 3);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusResult.StatusCode);
        }

        #endregion

        // ── GetWorkGroupEdit ─────────────────────────────────────────────────

        #region GetWorkGroupEdit Tests

        [Fact]
        public void GetWorkGroupEdit_WithValidInput_ReturnsPartialViewWithModel()
        {
            // Arrange
            const string workGroupName = "WG001";
            const bool flaggedForEmail = true;
            const string emailRecipient = "test@example.com";

            // Act
            var result = _controller.GetWorkGroupEdit(workGroupName, flaggedForEmail, emailRecipient);

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_WorkGroupEditModal", partialResult.ViewName);
            var model = Assert.IsType<WorkGroupEmailItem>(partialResult.Model);
            Assert.Equal(workGroupName, model.WorkGroupName);
            Assert.True(model.FlaggedForEmail);
            Assert.Equal(emailRecipient, model.EmailRecipient);
        }

        [Fact]
        public void GetWorkGroupEdit_WithNullEmailRecipient_ReturnsPartialViewWithNullRecipient()
        {
            // Act
            var result = _controller.GetWorkGroupEdit("WG001", false, null);

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<WorkGroupEmailItem>(partialResult.Model);
            Assert.False(model.FlaggedForEmail);
            Assert.Null(model.EmailRecipient);
        }

        #endregion

        // ── UpdateWorkGroupEmail ─────────────────────────────────────────────

        #region UpdateWorkGroupEmail Tests

        [Fact]
        public async Task UpdateWorkGroupEmail_WithValidInput_ReturnsOk()
        {
            // Arrange
            const string workGroupName = "WG001";
            const short sendEmail = 1;
            const string emailRecipient = "test@example.com";
            _workGroupService.UpdateWorkGroupEmailAsync(workGroupName, sendEmail, emailRecipient)
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.UpdateWorkGroupEmail(workGroupName, sendEmail, emailRecipient);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var json = GetJsonElement(okResult.Value);
            Assert.True(json.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task UpdateWorkGroupEmail_WithEmptyWorkGroupName_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.UpdateWorkGroupEmail("", 1, "test@example.com");

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            await _workGroupService.DidNotReceive()
                .UpdateWorkGroupEmailAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<string?>());
        }

        [Fact]
        public async Task UpdateWorkGroupEmail_WhenServiceFails_Returns500()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Code = "UPDATE_ERROR", Message = "Failed" } };
            _workGroupService.UpdateWorkGroupEmailAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<string?>())
                .Returns(ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.UpdateWorkGroupEmail("WG001", 1, "test@example.com");

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusResult.StatusCode);
        }

        [Fact]
        public async Task UpdateWorkGroupEmail_WithNullEmailRecipient_ReturnsOk()
        {
            // Arrange
            _workGroupService.UpdateWorkGroupEmailAsync("WG001", 0, null)
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.UpdateWorkGroupEmail("WG001", 0, null);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        // ── GetProfitCentreSettings ──────────────────────────────────────────

        #region GetProfitCentreSettings Tests

        [Fact]
        public async Task GetProfitCentreSettings_WithValidProfitCentre_ReturnsJsonWithSettings()
        {
            // Arrange
            const string profitCentre = "PC001";
            SetupProfitCentreSettings(profitCentre, new ProfitCentreSettingsDto
            {
                Timesheet       = -1,
                Outputsheet     = 0,
                TimesheetLayout = 1
            });

            // Act
            var result = await _controller.GetProfitCentreSettings(profitCentre);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = GetJsonElement(jsonResult.Value);
            Assert.True(json.GetProperty("timesheet").GetBoolean());
            Assert.False(json.GetProperty("outputsheet").GetBoolean());
            Assert.Equal(1, json.GetProperty("timesheetLayout").GetInt32());
        }

        [Fact]
        public async Task GetProfitCentreSettings_WithEmptyProfitCentre_ReturnsDefaults()
        {
            // Act
            var result = await _controller.GetProfitCentreSettings("");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = GetJsonElement(jsonResult.Value);
            Assert.False(json.GetProperty("timesheet").GetBoolean());
            Assert.False(json.GetProperty("outputsheet").GetBoolean());
            Assert.Equal(1, json.GetProperty("timesheetLayout").GetInt32());
            await _profitCentreService.DidNotReceive().GetProfitCentreSettingsAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task GetProfitCentreSettings_WhenServiceFails_ReturnsDefaults()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Code = "NOT_FOUND", Message = "Not found" } };
            _profitCentreService.GetProfitCentreSettingsAsync("PC001")
                .Returns(ApiResponseDto<ProfitCentreSettingsDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.GetProfitCentreSettings("PC001");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = GetJsonElement(jsonResult.Value);
            Assert.False(json.GetProperty("timesheet").GetBoolean());
            Assert.Equal(1, json.GetProperty("timesheetLayout").GetInt32());
        }

        [Fact]
        public async Task GetProfitCentreSettings_WithNullTimesheetLayout_DefaultsToOne()
        {
            // Arrange
            SetupProfitCentreSettings("PC001", new ProfitCentreSettingsDto
            {
                Timesheet       = 0,
                Outputsheet     = 0,
                TimesheetLayout = null
            });

            // Act
            var result = await _controller.GetProfitCentreSettings("PC001");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = GetJsonElement(jsonResult.Value);
            Assert.Equal(1, json.GetProperty("timesheetLayout").GetInt32());
        }

        #endregion

        // ── PatchProfitCentreSettings ────────────────────────────────────────

        #region PatchProfitCentreSettings Tests

        [Fact]
        public async Task PatchProfitCentreSettings_WithAllTrueFlags_ReturnsOkAndPassesCorrectValues()
        {
            // Arrange
            const string profitCentre = "PC001";
            _profitCentreService
                .UpdateProfitCentreSettingsAsync(profitCentre, -1, -1, 1)
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.PatchProfitCentreSettings(profitCentre, true, true, true);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var json = GetJsonElement(okResult.Value);
            Assert.True(json.GetProperty("success").GetBoolean());
            await _profitCentreService.Received(1)
                .UpdateProfitCentreSettingsAsync(profitCentre, -1, -1, 1);
        }

        [Fact]
        public async Task PatchProfitCentreSettings_WithFlatFalse_PassesCrossTabLayout()
        {
            // Arrange
            const string profitCentre = "PC001";
            _profitCentreService
                .UpdateProfitCentreSettingsAsync(profitCentre, 0, 0, 2)
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            await _controller.PatchProfitCentreSettings(profitCentre, false, false, false);

            // Assert
            await _profitCentreService.Received(1)
                .UpdateProfitCentreSettingsAsync(profitCentre, 0, 0, 2);
        }

        [Fact]
        public async Task PatchProfitCentreSettings_WithEmptyProfitCentre_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.PatchProfitCentreSettings("", true, true, true);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            await _profitCentreService.DidNotReceive()
                .UpdateProfitCentreSettingsAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<short>());
        }

        [Fact]
        public async Task PatchProfitCentreSettings_WhenServiceFails_Returns500()
        {
            // Arrange
            const string profitCentre = "PC001";
            var errors = new List<ApiErrorDto> { new() { Code = "UPDATE_ERROR", Message = "Failed" } };
            _profitCentreService
                .UpdateProfitCentreSettingsAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<short>())
                .Returns(ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.PatchProfitCentreSettings(profitCentre, true, false, true);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusResult.StatusCode);
        }

        #endregion
    }
}
