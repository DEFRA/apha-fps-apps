using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.PlanStaffZTCodeControllerTest
{
    public class PlanStaffZTCodeControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IPlanStaffZTCodeService _planStaffZTCodeService;
        private readonly PlanStaffZTCodeController _controller;

        public PlanStaffZTCodeControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _planStaffZTCodeService = Substitute.For<IPlanStaffZTCodeService>();
            _controller = new PlanStaffZTCodeController(_mapper, _planStaffZTCodeService);
        }

        private static JsonElement GetJsonResultElement(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        #region Index Tests

        [Fact]
        public async Task Index_WithValidStaffId_ReturnsViewResult()
        {
            // Arrange
            var staffId = "S001";
            var staffSummary = new StaffWorkgroupLookupDto
            {
                StaffID = staffId,
                Name = "John Doe",
                WorkGroupGrade = "WG1",
                HrsPaid = 40.0,
                Leave = 2.0,
                SickSpecial = 0.5,
                HrsAvail = 37.5
            };
            var staffResponse = ApiResponseDto<StaffWorkgroupLookupDto>.SuccessResponse(staffSummary);
            var ztTotalResponse = ApiResponseDto<double>.SuccessResponse(10.0);
            var ztJobsResponse = ApiResponseDto<List<ZtStaffJobViewDto>>.SuccessResponse(new List<ZtStaffJobViewDto>());

            _planStaffZTCodeService.GetStaffSummaryByIdAsync(staffId).Returns(staffResponse);
            _planStaffZTCodeService.GetZtTotalHoursByStaffIdAsync(staffId).Returns(ztTotalResponse);
            _planStaffZTCodeService.GetZtStaffJobsByStaffIdPagedAsync(Arg.Any<QueryParameters<string>>(), staffId)
                .Returns(ztJobsResponse);

            // Act
            var result = await _controller.Index(staffId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
        }

        [Fact]
        public async Task Index_WithNullStaffId_ReturnsViewResult()
        {
            // Arrange
            var ztJobsResponse = ApiResponseDto<List<ZtStaffJobViewDto>>.SuccessResponse(new List<ZtStaffJobViewDto>());
            _planStaffZTCodeService.GetZtStaffJobsByStaffIdPagedAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(ztJobsResponse);

            // Act
            var result = await _controller.Index(null!);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
        }

        #endregion

        #region GetStaffSummary Tests

        [Fact]
        public async Task GetStaffSummary_WithValidStaffId_ReturnsSuccessJson()
        {
            // Arrange
            var staffId = "S001";
            var staffSummary = new StaffWorkgroupLookupDto
            {
                StaffID = staffId,
                Name = "John Doe",
                WorkGroupGrade = "WG1",
                HrsPaid = 40.0,
                Leave = 2.0,
                SickSpecial = 0.5,
                HrsAvail = 37.5
            };
            var staffResponse = ApiResponseDto<StaffWorkgroupLookupDto>.SuccessResponse(staffSummary);
            var ztTotalResponse = ApiResponseDto<double>.SuccessResponse(10.0);

            _planStaffZTCodeService.GetStaffSummaryByIdAsync(staffId).Returns(staffResponse);
            _planStaffZTCodeService.GetZtTotalHoursByStaffIdAsync(staffId).Returns(ztTotalResponse);

            // Act
            var result = await _controller.GetStaffSummary(staffId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.Equal("John Doe", value.GetProperty("name").GetString());
            Assert.Equal("WG1", value.GetProperty("workGroupGrade").GetString());
            Assert.Equal(10.0, value.GetProperty("plannedAdminZT").GetDouble());
        }

        [Fact]
        public async Task GetStaffSummary_WithEmptyStaffId_ReturnsFailureJson()
        {
            // Act
            var result = await _controller.GetStaffSummary(string.Empty);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("StaffId is required.", value.GetProperty("message").GetString());
        }

        [Fact]
        public async Task GetStaffSummary_WithWhitespaceStaffId_ReturnsFailureJson()
        {
            // Act
            var result = await _controller.GetStaffSummary("   ");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task GetStaffSummary_WhenServiceFails_ReturnsFailureJson()
        {
            // Arrange
            var staffId = "S001";
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Not Found", Code = "NOT_FOUND" }
            };
            var staffResponse = ApiResponseDto<StaffWorkgroupLookupDto>.FailureResponse(errors, new ApiMetaDto());
            var ztTotalResponse = ApiResponseDto<double>.SuccessResponse(0.0);

            _planStaffZTCodeService.GetStaffSummaryByIdAsync(staffId).Returns(staffResponse);
            _planStaffZTCodeService.GetZtTotalHoursByStaffIdAsync(staffId).Returns(ztTotalResponse);

            // Act
            var result = await _controller.GetStaffSummary(staffId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        #endregion

        #region GetZtCodes Tests

        [Fact]
        public async Task GetZtCodes_WithSuccessResponse_ReturnsSuccessJson()
        {
            // Arrange
            var ztCodes = new List<FpsZtJobCodeDto>
            {
                new FpsZtJobCodeDto { JobCode = "ZT001", Description = "Admin Work" },
                new FpsZtJobCodeDto { JobCode = "ZT002", Description = "Training" }
            };
            var expectedResponse = ApiResponseDto<IEnumerable<FpsZtJobCodeDto>>.SuccessResponse(ztCodes);

            _planStaffZTCodeService.GetZtJobCodesAsync().Returns(expectedResponse);

            // Act
            var result = await _controller.GetZtCodes();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.True(value.GetProperty("data").GetArrayLength() > 0);
        }

        [Fact]
        public async Task GetZtCodes_WhenServiceFails_ReturnsFailureJson()
        {
            // Arrange
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "API Error", Code = "API_ERROR" }
            };
            var expectedResponse = ApiResponseDto<IEnumerable<FpsZtJobCodeDto>>.FailureResponse(errors, new ApiMetaDto());

            _planStaffZTCodeService.GetZtJobCodesAsync().Returns(expectedResponse);

            // Act
            var result = await _controller.GetZtCodes();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        #endregion

        #region Create (GET) Tests

        [Fact]
        public async Task Create_Get_WithJobCode_ReturnsPartialViewWithModel()
        {
            // Arrange
            var ztCodes = new List<FpsZtJobCodeDto>
            {
                new FpsZtJobCodeDto { JobCode = "ZT001", Description = "Admin Work" }
            };
            var ztResponse = ApiResponseDto<IEnumerable<FpsZtJobCodeDto>>.SuccessResponse(ztCodes);
            _planStaffZTCodeService.GetZtJobCodesAsync().Returns(ztResponse);

            // Act
            var result = await _controller.Create("ZT001");

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditPlanStaffZTCode", partialViewResult.ViewName);
            var model = Assert.IsType<PlanStaffZTCodeItemViewModel>(partialViewResult.Model);
            Assert.Equal("ZT001", model.JobCode);
        }

        [Fact]
        public async Task Create_Get_WithNullJobCode_ReturnsPartialViewWithEmptyJobCode()
        {
            // Arrange
            var ztResponse = ApiResponseDto<IEnumerable<FpsZtJobCodeDto>>.SuccessResponse(new List<FpsZtJobCodeDto>());
            _planStaffZTCodeService.GetZtJobCodesAsync().Returns(ztResponse);

            // Act
            var result = await _controller.Create((string?)null);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<PlanStaffZTCodeItemViewModel>(partialViewResult.Model);
            Assert.Equal(string.Empty, model.JobCode);
        }

        #endregion

        #region Create (POST) Tests

        [Fact]
        public async Task Create_Post_WithValidItem_ReturnsSuccessJson()
        {
            // Arrange
            var item = new PlanStaffZTCodeItemViewModel
            {
                StaffID = "S001",
                JobCode = "ZT001",
                PlannedHours = 40
            };
            var createdDto = new StaffJobDto { StaffId = "S001", JobCode = "ZT001", PlannedHours = 40 };
            var expectedResponse = ApiResponseDto<StaffJobDto>.SuccessResponse(createdDto);

            _planStaffZTCodeService.CreateStaffJobAsync(Arg.Any<StaffJobDto>()).Returns(expectedResponse);

            // Act
            var result = await _controller.Create(item);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.Equal("ZT plan entry created successfully.", value.GetProperty("message").GetString());
        }

        [Fact]
        public async Task Create_Post_WithMissingStaffId_ReturnsFailureJson()
        {
            // Arrange
            var item = new PlanStaffZTCodeItemViewModel
            {
                StaffID = "",
                JobCode = "ZT001",
                PlannedHours = 40
            };

            // Act
            var result = await _controller.Create(item);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task Create_Post_WithMissingJobCode_ReturnsFailureJson()
        {
            // Arrange
            var item = new PlanStaffZTCodeItemViewModel
            {
                StaffID = "S001",
                JobCode = "",
                PlannedHours = 40
            };

            // Act
            var result = await _controller.Create(item);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task Create_Post_WhenServiceFails_ReturnsFailureJson()
        {
            // Arrange
            var item = new PlanStaffZTCodeItemViewModel
            {
                StaffID = "S001",
                JobCode = "ZT001",
                PlannedHours = 40
            };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Duplicate entry", Code = "DUPLICATE" }
            };
            var expectedResponse = ApiResponseDto<StaffJobDto>.FailureResponse(errors, new ApiMetaDto());

            _planStaffZTCodeService.CreateStaffJobAsync(Arg.Any<StaffJobDto>()).Returns(expectedResponse);

            // Act
            var result = await _controller.Create(item);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        #endregion

        #region Edit (GET) Tests

        [Fact]
        public async Task Edit_Get_WithValidIds_ReturnsPartialViewWithModel()
        {
            // Arrange
            var staffId = "S001";
            var jobCode = "ZT001";
            var ztDetail = new ZtStaffJobViewDto
            {
                StaffID = staffId,
                JobCode = jobCode,
                PlannedHours = 40,
                Name = "Admin Work",
                ZtDescription = "Administrative Tasks"
            };
            var detailResponse = ApiResponseDto<ZtStaffJobViewDto>.SuccessResponse(ztDetail);
            var ztCodes = new List<FpsZtJobCodeDto>
            {
                new FpsZtJobCodeDto { JobCode = "ZT001", Description = "Admin Work" }
            };
            var ztResponse = ApiResponseDto<IEnumerable<FpsZtJobCodeDto>>.SuccessResponse(ztCodes);

            _planStaffZTCodeService.GetZtStaffJobDetailsByIdAsync(staffId, jobCode).Returns(detailResponse);
            _planStaffZTCodeService.GetZtJobCodesAsync().Returns(ztResponse);

            // Act
            var result = await _controller.Edit(staffId, jobCode);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditPlanStaffZTCode", partialViewResult.ViewName);
            var model = Assert.IsType<PlanStaffZTCodeItemViewModel>(partialViewResult.Model);
            Assert.Equal(staffId, model.StaffID);
            Assert.Equal(jobCode, model.JobCode);
            Assert.Equal(40, model.PlannedHours);
        }

        [Fact]
        public async Task Edit_Get_WhenNotFound_ReturnsFailureJson()
        {
            // Arrange
            var staffId = "S999";
            var jobCode = "ZT999";
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Not Found", Code = "NOT_FOUND" }
            };
            var detailResponse = ApiResponseDto<ZtStaffJobViewDto>.FailureResponse(errors, new ApiMetaDto());

            _planStaffZTCodeService.GetZtStaffJobDetailsByIdAsync(staffId, jobCode).Returns(detailResponse);

            // Act
            var result = await _controller.Edit(staffId, jobCode);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        #endregion

        #region Edit (POST) Tests

        [Fact]
        public async Task Edit_Post_WithValidItem_ReturnsSuccessJson()
        {
            // Arrange
            var item = new PlanStaffZTCodeItemViewModel
            {
                StaffID = "S001",
                JobCode = "ZT001",
                PlannedHours = 60
            };
            var updatedDto = new StaffJobDto { StaffId = "S001", JobCode = "ZT001", PlannedHours = 60 };
            var expectedResponse = ApiResponseDto<StaffJobDto>.SuccessResponse(updatedDto);

            _planStaffZTCodeService.UpdateStaffJobAsync(Arg.Any<StaffJobDto>()).Returns(expectedResponse);

            // Act
            var result = await _controller.Edit(item);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.Equal("ZT plan entry updated successfully.", value.GetProperty("message").GetString());
        }

        [Fact]
        public async Task Edit_Post_WithMissingStaffId_ReturnsFailureJson()
        {
            // Arrange
            var item = new PlanStaffZTCodeItemViewModel
            {
                StaffID = "",
                JobCode = "ZT001",
                PlannedHours = 60
            };

            // Act
            var result = await _controller.Edit(item);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task Edit_Post_WithMissingJobCode_ReturnsFailureJson()
        {
            // Arrange
            var item = new PlanStaffZTCodeItemViewModel
            {
                StaffID = "S001",
                JobCode = "",
                PlannedHours = 60
            };

            // Act
            var result = await _controller.Edit(item);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task Edit_Post_WhenServiceFails_ReturnsFailureJson()
        {
            // Arrange
            var item = new PlanStaffZTCodeItemViewModel
            {
                StaffID = "S001",
                JobCode = "ZT001",
                PlannedHours = 60
            };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Not Found", Code = "NOT_FOUND" }
            };
            var expectedResponse = ApiResponseDto<StaffJobDto>.FailureResponse(errors, new ApiMetaDto());

            _planStaffZTCodeService.UpdateStaffJobAsync(Arg.Any<StaffJobDto>()).Returns(expectedResponse);

            // Act
            var result = await _controller.Edit(item);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_WithValidIds_ReturnsSuccessJson()
        {
            // Arrange
            var staffId = "S001";
            var jobCode = "ZT001";
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);

            _planStaffZTCodeService.DeleteStaffJobAsync(staffId, jobCode).Returns(expectedResponse);

            // Act
            var result = await _controller.Delete(staffId, jobCode);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.Equal("ZT plan entry deleted successfully.", value.GetProperty("message").GetString());
        }

        [Fact]
        public async Task Delete_WhenServiceFails_ReturnsFailureJson()
        {
            // Arrange
            var staffId = "S999";
            var jobCode = "ZT999";
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Not Found", Code = "NOT_FOUND" }
            };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());

            _planStaffZTCodeService.DeleteStaffJobAsync(staffId, jobCode).Returns(expectedResponse);

            // Act
            var result = await _controller.Delete(staffId, jobCode);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Theory]
        [InlineData("S001", "ZT001")]
        [InlineData("S002", "ZT002")]
        [InlineData("EMP123", "ZT_TEST")]
        public async Task Delete_WithVariousIds_CallsService(string staffId, string jobCode)
        {
            // Arrange
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _planStaffZTCodeService.DeleteStaffJobAsync(staffId, jobCode).Returns(expectedResponse);

            // Act
            await _controller.Delete(staffId, jobCode);

            // Assert
            await _planStaffZTCodeService.Received(1).DeleteStaffJobAsync(staffId, jobCode);
        }

        #endregion
    }
}
