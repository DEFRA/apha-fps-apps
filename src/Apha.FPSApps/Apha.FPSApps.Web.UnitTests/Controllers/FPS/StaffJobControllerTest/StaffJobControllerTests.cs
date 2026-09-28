using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.StaffJobControllerTest
{
    public class StaffJobControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IStaffJobService _staffJobService;
        private readonly StaffJobController _controller;

        public StaffJobControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _staffJobService = Substitute.For<IStaffJobService>();
            _controller = new StaffJobController(_mapper, _staffJobService);
        }

        private static JsonElement GetJsonResultElement(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        #region GetTotalStaffCost Tests

        [Fact]
        public async Task GetTotalStaffCost_WithValidJobCode_ReturnsSuccessJson()
        {
            // Arrange
            var jobCode = "JOB001";
            var totalStaffCost = 4500m;
            var serviceResponse = ApiResponseDto<decimal>.SuccessResponse(totalStaffCost);

            _staffJobService.GetTotalStaffCostAsync(jobCode).Returns(serviceResponse);

            // Act
            var result = await _controller.GetTotalStaffCost(jobCode);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.Equal(totalStaffCost, value.GetProperty("totalStaffCost").GetDecimal());
            await _staffJobService.Received(1).GetTotalStaffCostAsync(jobCode);
        }

        [Fact]
        public async Task GetTotalStaffCost_WithEmptyJobCode_ReturnsFailureJson()
        {
            // Act
            var result = await _controller.GetTotalStaffCost(string.Empty);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Job Code is required", value.GetProperty("message").GetString());
            Assert.Equal(0m, value.GetProperty("totalStaffCost").GetDecimal());
            await _staffJobService.DidNotReceive().GetTotalStaffCostAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task GetTotalStaffCost_WithWhitespaceJobCode_ReturnsFailureJson()
        {
            // Act
            var result = await _controller.GetTotalStaffCost("   ");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Job Code is required", value.GetProperty("message").GetString());
            await _staffJobService.DidNotReceive().GetTotalStaffCostAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task GetTotalStaffCost_WhenServiceFails_ReturnsFailureJson()
        {
            // Arrange
            var jobCode = "JOB001";
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "API error", Code = "API_ERROR" }
            };
            var serviceResponse = ApiResponseDto<decimal>.FailureResponse(errors, new ApiMetaDto());

            _staffJobService.GetTotalStaffCostAsync(jobCode).Returns(serviceResponse);

            // Act
            var result = await _controller.GetTotalStaffCost(jobCode);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Failed to retrieve total staff cost.", value.GetProperty("message").GetString());
            Assert.Equal(0m, value.GetProperty("totalStaffCost").GetDecimal());
            await _staffJobService.Received(1).GetTotalStaffCostAsync(jobCode);
        }

        [Theory]
        [InlineData("JOB001", 1000.50)]
        [InlineData("FZ2000", 0.0)]
        [InlineData("PROJ123", 25000.75)]
        public async Task GetTotalStaffCost_WithVariousJobCodes_ReturnsCorrectTotal(string jobCode, double total)
        {
            // Arrange
            var expectedTotal = (decimal)total;
            var serviceResponse = ApiResponseDto<decimal>.SuccessResponse(expectedTotal);

            _staffJobService.GetTotalStaffCostAsync(jobCode).Returns(serviceResponse);

            // Act
            var result = await _controller.GetTotalStaffCost(jobCode);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.Equal(expectedTotal, value.GetProperty("totalStaffCost").GetDecimal());
            await _staffJobService.Received(1).GetTotalStaffCostAsync(jobCode);
        }

        #endregion

        #region Create (POST) Tests

        [Fact]
        public async Task Create_WhenServiceSucceeds_ReturnsSuccessJson()
        {
            // Arrange
            var viewModel = new Apha.FPSApps.Web.Areas.FPS.Models.StaffJobItemViewModel
            {
                StaffID = "STAFF001",
                JobCode = "JOB001",
                PlannedHours = 8
            };
            var staffJobData = new StaffJobDto { StaffId = "STAFF001", JobCode = "JOB001" };
            var serviceResponse = ApiResponseDto<StaffJobDto>.SuccessResponse(staffJobData);

            _staffJobService.CreateStaffJobAsync(Arg.Any<StaffJobDto>()).Returns(serviceResponse);

            // Act
            var result = await _controller.Create(viewModel);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.Equal("Staff job created successfully", value.GetProperty("message").GetString());
            await _staffJobService.Received(1).CreateStaffJobAsync(Arg.Any<StaffJobDto>());
        }

        [Fact]
        public async Task Create_WhenDuplicateErrorDetected_ReturnsFailureJsonWithEmptyField()
        {
            // Arrange
            var viewModel = new Apha.FPSApps.Web.Areas.FPS.Models.StaffJobItemViewModel
            {
                StaffID = "STAFF001",
                JobCode = "JOB001",
                PlannedHours = 8
            };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Code = "CONFLICT", Message = "already exists" }
            };
            var serviceResponse = ApiResponseDto<StaffJobDto>.FailureResponse(errors, new ApiMetaDto());

            _staffJobService.CreateStaffJobAsync(Arg.Any<StaffJobDto>()).Returns(serviceResponse);

            // Act
            var result = await _controller.Create(viewModel);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());

            const string expectedMessage = "This staff member has already been added to this project. Please update the existing entry instead.";
            Assert.Equal(expectedMessage, value.GetProperty("message").GetString());

            var errorsArray = value.GetProperty("errors");
            Assert.Equal(1, errorsArray.GetArrayLength());
            // field must be empty string so the error renders in the summary, not inline
            Assert.Equal(string.Empty, errorsArray[0].GetProperty("field").GetString());
            Assert.Equal(expectedMessage, errorsArray[0].GetProperty("message").GetString());
        }

        [Theory]
        [InlineData("DUPLICATE")]
        [InlineData("BUSINESS_RULE_VIOLATION")]
        public async Task Create_WhenDuplicateErrorCodeDetected_ReturnsFailureJsonWithEmptyField(string errorCode)
        {
            // Arrange
            var viewModel = new Apha.FPSApps.Web.Areas.FPS.Models.StaffJobItemViewModel
            {
                StaffID = "STAFF001",
                JobCode = "JOB001",
                PlannedHours = 8
            };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Code = errorCode, Message = "Some error" }
            };
            var serviceResponse = ApiResponseDto<StaffJobDto>.FailureResponse(errors, new ApiMetaDto());

            _staffJobService.CreateStaffJobAsync(Arg.Any<StaffJobDto>()).Returns(serviceResponse);

            // Act
            var result = await _controller.Create(viewModel);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());

            var errorsArray = value.GetProperty("errors");
            Assert.Equal(1, errorsArray.GetArrayLength());
            Assert.Equal(string.Empty, errorsArray[0].GetProperty("field").GetString());
        }

        [Fact]
        public async Task Create_WhenServiceFailsWithGenericError_ReturnsFailureJson()
        {
            // Arrange
            var viewModel = new Apha.FPSApps.Web.Areas.FPS.Models.StaffJobItemViewModel
            {
                StaffID = "STAFF001",
                JobCode = "JOB001",
                PlannedHours = 8
            };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Code = "SERVER_ERROR", Message = "Internal server error" }
            };
            var serviceResponse = ApiResponseDto<StaffJobDto>.FailureResponse(errors, new ApiMetaDto());

            _staffJobService.CreateStaffJobAsync(Arg.Any<StaffJobDto>()).Returns(serviceResponse);

            // Act
            var result = await _controller.Create(viewModel);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Internal server error", value.GetProperty("message").GetString());
            await _staffJobService.Received(1).CreateStaffJobAsync(Arg.Any<StaffJobDto>());
        }

        #endregion

        #region Index Tests

        [Fact]
        public void Index_ReturnsViewResult()
        {
            // Act
            var result = _controller.Index();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        #endregion

        #region IsDuplicateError via Create — null Code and null Message branch coverage

        [Fact]
        public async Task Create_WhenErrorCodeIsNullButMessageContainsAlreadyExists_ReturnsFriendlyDuplicateMessage()
        {
            // Arrange — Code is empty (no recognised duplicate code); IsDuplicateError falls through to Message.Contains("already exists")
            var viewModel = new Apha.FPSApps.Web.Areas.FPS.Models.StaffJobItemViewModel
            {
                StaffID = "STAFF001",
                JobCode = "JOB001",
                PlannedHours = 8
            };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Code = string.Empty, Message = "A record already exists for this staff member." }
            };
            var serviceResponse = ApiResponseDto<StaffJobDto>.FailureResponse(errors, new ApiMetaDto());

            _staffJobService.CreateStaffJobAsync(Arg.Any<StaffJobDto>()).Returns(serviceResponse);

            // Act
            var result = await _controller.Create(viewModel);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            const string expectedMessage = "This staff member has already been added to this project. Please update the existing entry instead.";
            Assert.Equal(expectedMessage, value.GetProperty("message").GetString());
            var errorsArray = value.GetProperty("errors");
            Assert.Equal(string.Empty, errorsArray[0].GetProperty("field").GetString());
        }

        [Fact]
        public async Task Create_WhenErrorCodeIsNullAndMessageDoesNotContainAlreadyExists_ReturnsGenericFailure()
        {
            // Arrange — Code is empty AND Message does not contain "already exists" → not a duplicate
            var viewModel = new Apha.FPSApps.Web.Areas.FPS.Models.StaffJobItemViewModel
            {
                StaffID = "STAFF001",
                JobCode = "JOB001",
                PlannedHours = 8
            };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Code = string.Empty, Message = "Unexpected server failure." }
            };
            var serviceResponse = ApiResponseDto<StaffJobDto>.FailureResponse(errors, new ApiMetaDto());

            _staffJobService.CreateStaffJobAsync(Arg.Any<StaffJobDto>()).Returns(serviceResponse);

            // Act
            var result = await _controller.Create(viewModel);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Unexpected server failure.", value.GetProperty("message").GetString());
        }

        [Fact]
        public async Task Create_WhenErrorCodeIsNullAndMessageIsNull_ReturnsGenericFailureWithFallbackMessage()
        {
            // Arrange — Code and Message are both empty strings → IsDuplicateError returns false, fallback message used
            var viewModel = new Apha.FPSApps.Web.Areas.FPS.Models.StaffJobItemViewModel
            {
                StaffID = "STAFF001",
                JobCode = "JOB001",
                PlannedHours = 8
            };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Code = string.Empty, Message = string.Empty }
            };
            var serviceResponse = ApiResponseDto<StaffJobDto>.FailureResponse(errors, new ApiMetaDto());

            _staffJobService.CreateStaffJobAsync(Arg.Any<StaffJobDto>()).Returns(serviceResponse);

            // Act
            var result = await _controller.Create(viewModel);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            // Message is empty string (not null), so the ?? fallback does not trigger
            Assert.Equal(string.Empty, value.GetProperty("message").GetString());
        }

        #endregion

        #region LoadStaffJobGrid Tests

        [Fact]
        public async Task LoadStaffJobGrid_WithValidJobCode_ReturnsPartialViewWithData()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var jobCode = "JOB001";
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var staffJobs = new List<StaffJobViewDto>
            {
                new() { StaffID = "STAFF001", JobCode = "JOB001" }
            };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 };
            var serviceResponse = ApiResponseDto<List<StaffJobViewDto>>.SuccessResponse(staffJobs, paginationDto);
            var staffJobViewModels = new List<StaffJobItemViewModel>
            {
                new() { StaffID = "STAFF001", JobCode = "JOB001" }
            };
            var paginationModel = new PaginationModel { PageNumber = 1, PageSize = 10, TotalRecords = 1 };

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _staffJobService.GetAllStaffJobsAsync(queryParameters, jobCode).Returns(serviceResponse);
            _mapper.Map<List<StaffJobItemViewModel>>(Arg.Any<List<StaffJobViewDto>>()).Returns(staffJobViewModels);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(paginationModel);

            // Act
            var result = await _controller.LoadStaffJobGrid(request, jobCode, "Staff Plan");

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
            var gridConfig = Assert.IsType<DataGridConfig<StaffJobItemViewModel>>(partialView.Model);
            Assert.Equal("staffBookedGrid", gridConfig.GridId);
            Assert.Equal("Staff Plan", gridConfig.Title);
            Assert.Equal("StaffID", gridConfig.KeyProperty);
            Assert.Single(gridConfig.Data);
            await _staffJobService.Received(1).GetAllStaffJobsAsync(queryParameters, jobCode);
        }

        [Fact]
        public async Task LoadStaffJobGrid_WhenModelStateIsInvalid_ReturnsFailureJson()
        {
            // Arrange
            _controller.ModelState.AddModelError("Page", "Page is required");
            var request = new PaginationFilter<string> { Filter = "{}" };

            // Act
            var result = await _controller.LoadStaffJobGrid(request, "JOB001", "Staff Plan");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Invalid request data", value.GetProperty("message").GetString());
            await _staffJobService.DidNotReceive().GetAllStaffJobsAsync(
                Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task LoadStaffJobGrid_WithEmptyJobCode_ReturnsEmptyGridWithoutCallingService(string? jobCode)
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);

            // Act
            var result = await _controller.LoadStaffJobGrid(request, jobCode, "Staff Plan");

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
            var gridConfig = Assert.IsType<DataGridConfig<StaffJobItemViewModel>>(partialView.Model);
            Assert.Empty(gridConfig.Data);
            await _staffJobService.DidNotReceive().GetAllStaffJobsAsync(
                Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
            _mapper.DidNotReceive().Map<List<StaffJobItemViewModel>>(Arg.Any<List<StaffJobViewDto>>());
        }

        [Fact]
        public async Task LoadStaffJobGrid_WithNullTitle_UsesDefaultTitle()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);

            // Act
            var result = await _controller.LoadStaffJobGrid(request, jobCode: null, title: null);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<StaffJobItemViewModel>>(partialView.Model);
            Assert.Equal("Staff Booked", gridConfig.Title);
        }

        [Fact]
        public async Task LoadStaffJobGrid_WhenServiceReturnsNullData_MapsEmptyStaffJobList()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var jobCode = "JOB001";
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var serviceResponse = ApiResponseDto<List<StaffJobViewDto>>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _staffJobService.GetAllStaffJobsAsync(queryParameters, jobCode).Returns(serviceResponse);

            // Act
            var result = await _controller.LoadStaffJobGrid(request, jobCode, "Staff Plan");

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<StaffJobItemViewModel>>(partialView.Model);
            Assert.Empty(gridConfig.Data);
            _mapper.DidNotReceive().Map<List<StaffJobItemViewModel>>(Arg.Any<List<StaffJobViewDto>>());
        }

        #endregion
    }
}
