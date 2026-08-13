using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Text.Json;
using Xunit;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.YearEndCutOverControllerTest
{
    public class YearEndCutOverControllerTests
    {
        private const string JobName = "YearEnd-CutOver";

        private readonly IMapper _mapper;
        private readonly IYearMasterService _yearMasterService;
        private readonly IYearEndService _yearEndService;
        private readonly ILogger<YearEndCutOverController> _logger;
        private readonly YearEndCutOverController _controller;

        public YearEndCutOverControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _yearMasterService = Substitute.For<IYearMasterService>();
            _yearEndService = Substitute.For<IYearEndService>();
            _logger = Substitute.For<ILogger<YearEndCutOverController>>();

            _controller = new YearEndCutOverController(
                _mapper,
                _yearMasterService,
                _yearEndService,
                _logger);

            // Default mapper wiring needed by BuildHistoryGridAsync
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                   .Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                   .Returns(new PaginationModel());
        }

        // -----------------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------------

        private static JsonElement GetJsonElement(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        private void SetupIndexDefaults(
            int plannedYear = 2025,
            bool canInitiate = true,
            bool canApprove = false)
        {
            _yearMasterService.GetFpsPlannedYearAsync()
                .Returns(ApiResponseDto<int>.SuccessResponse(plannedYear));

            _yearEndService.CanInitiateCutOverRequestAsync(JobName)
                .Returns(ApiResponseDto<bool>.SuccessResponse(canInitiate));

            _yearEndService.CanApproveOrRejectCutOverRequestAsync(JobName)
                .Returns(ApiResponseDto<bool>.SuccessResponse(canApprove));

            _yearEndService.GetYearEndCutOverBatchJobHistoryAsync(
                    Arg.Any<QueryParameters<string>>(), JobName)
                .Returns(ApiResponseDto<PaginatedResult<BatchJobHistoryDto>>
                    .SuccessResponse(new PaginatedResult<BatchJobHistoryDto>()));
        }

        #region Index

        [Fact]
        public async Task Index_ReturnsViewResult()
        {
            // Arrange
            SetupIndexDefaults();

            // Act
            var result = await _controller.Index();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Index_ViewModelHasCorrectPlannedYear()
        {
            // Arrange
            SetupIndexDefaults(plannedYear: 2025);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<YearEndCutOverViewModel>(viewResult.Model);
            Assert.Equal(2025, model.PlannedYear);
        }

        [Fact]
        public async Task Index_ViewModelCanInitiateIsTrue_WhenServiceReturnsTrue()
        {
            // Arrange
            SetupIndexDefaults(canInitiate: true, canApprove: false);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<YearEndCutOverViewModel>(viewResult.Model);
            Assert.True(model.CanInitiate);
            Assert.False(model.CanApprove);
        }

        [Fact]
        public async Task Index_ViewModelCanApproveIsTrue_WhenServiceReturnsTrue()
        {
            // Arrange
            SetupIndexDefaults(canInitiate: false, canApprove: true);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<YearEndCutOverViewModel>(viewResult.Model);
            Assert.False(model.CanInitiate);
            Assert.True(model.CanApprove);
        }

        [Fact]
        public async Task Index_ViewModelContainsHistoryGrid()
        {
            // Arrange
            SetupIndexDefaults();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<YearEndCutOverViewModel>(viewResult.Model);
            Assert.NotNull(model.HistoryGrid);
        }

        [Fact]
        public async Task Index_WhenPlannedYearServiceFails_PlannedYearIsZero()
        {
            // Arrange
            _yearMasterService.GetFpsPlannedYearAsync()
                .Returns(new ApiResponseDto<int> { Success = false });
            _yearEndService.CanInitiateCutOverRequestAsync(JobName)
                .Returns(ApiResponseDto<bool>.SuccessResponse(false));
            _yearEndService.CanApproveOrRejectCutOverRequestAsync(JobName)
                .Returns(ApiResponseDto<bool>.SuccessResponse(false));
            _yearEndService.GetYearEndCutOverBatchJobHistoryAsync(
                    Arg.Any<QueryParameters<string>>(), JobName)
                .Returns(ApiResponseDto<PaginatedResult<BatchJobHistoryDto>>
                    .SuccessResponse(new PaginatedResult<BatchJobHistoryDto>()));

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<YearEndCutOverViewModel>(viewResult.Model);
            Assert.Equal(0, model.PlannedYear);
        }

        [Fact]
        public async Task Index_WhenCanInitiateServiceFails_CanInitiateIsFalse()
        {
            // Arrange
            _yearMasterService.GetFpsPlannedYearAsync()
                .Returns(ApiResponseDto<int>.SuccessResponse(2025));
            _yearEndService.CanInitiateCutOverRequestAsync(JobName)
                .Returns(new ApiResponseDto<bool> { Success = false });
            _yearEndService.CanApproveOrRejectCutOverRequestAsync(JobName)
                .Returns(ApiResponseDto<bool>.SuccessResponse(false));
            _yearEndService.GetYearEndCutOverBatchJobHistoryAsync(
                    Arg.Any<QueryParameters<string>>(), JobName)
                .Returns(ApiResponseDto<PaginatedResult<BatchJobHistoryDto>>
                    .SuccessResponse(new PaginatedResult<BatchJobHistoryDto>()));

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<YearEndCutOverViewModel>(viewResult.Model);
            Assert.False(model.CanInitiate);
        }

        #endregion

        #region LoadHistoryGrid

        [Fact]
        public async Task LoadHistoryGrid_ReturnsPartialViewResult()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _yearEndService.GetYearEndCutOverBatchJobHistoryAsync(
                    Arg.Any<QueryParameters<string>>(), JobName)
                .Returns(ApiResponseDto<PaginatedResult<BatchJobHistoryDto>>
                    .SuccessResponse(new PaginatedResult<BatchJobHistoryDto>()));

            // Act
            var result = await _controller.LoadHistoryGrid(request);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadHistoryGrid_ReturnsDataGridConfigWithHistoryData()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", SortBy = "StartDateTime", Descending = true };
            var history = new List<BatchJobHistoryDto>
            {
                new BatchJobHistoryDto { JobName = JobName, RequestedBy = "user@test.com", Status = "Completed", StartDateTime = DateTime.UtcNow }
            };
            var paginated = new PaginatedResult<BatchJobHistoryDto>(history, 1);

            _yearEndService.GetYearEndCutOverBatchJobHistoryAsync(
                    Arg.Any<QueryParameters<string>>(), JobName)
                .Returns(ApiResponseDto<PaginatedResult<BatchJobHistoryDto>>.SuccessResponse(paginated));

            // Act
            var result = await _controller.LoadHistoryGrid(request);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<YearEndHistoryItem>>(partial.Model);
            Assert.Single(model.Data);
            Assert.Equal(JobName, model.Data[0].JobName);
            Assert.Equal("Completed", model.Data[0].Status);
        }

        [Fact]
        public async Task LoadHistoryGrid_WhenServiceReturnsEmpty_ReturnsEmptyDataGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _yearEndService.GetYearEndCutOverBatchJobHistoryAsync(
                    Arg.Any<QueryParameters<string>>(), JobName)
                .Returns(ApiResponseDto<PaginatedResult<BatchJobHistoryDto>>
                    .SuccessResponse(new PaginatedResult<BatchJobHistoryDto>()));

            // Act
            var result = await _controller.LoadHistoryGrid(request);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<YearEndHistoryItem>>(partial.Model);
            Assert.Empty(model.Data);
        }

        [Fact]
        public async Task LoadHistoryGrid_SortColumnAndDirectionSetFromRequest()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", SortBy = "StartDateTime", Descending = true };
            _yearEndService.GetYearEndCutOverBatchJobHistoryAsync(
                    Arg.Any<QueryParameters<string>>(), JobName)
                .Returns(ApiResponseDto<PaginatedResult<BatchJobHistoryDto>>
                    .SuccessResponse(new PaginatedResult<BatchJobHistoryDto>()));

            // Act
            var result = await _controller.LoadHistoryGrid(request);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<YearEndHistoryItem>>(partial.Model);
            Assert.Equal("StartDateTime", model.Pagination.SortColumn);
            Assert.True(model.Pagination.SortDirection);
        }

        [Fact]
        public async Task LoadHistoryGrid_WhenNoPaginationReturned_UsesFallbackPaginationModel()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            var response = new ApiResponseDto<PaginatedResult<BatchJobHistoryDto>>
            {
                Success = true,
                Data = new PaginatedResult<BatchJobHistoryDto>(),
                Pagination = null
            };
            _yearEndService.GetYearEndCutOverBatchJobHistoryAsync(
                    Arg.Any<QueryParameters<string>>(), JobName)
                .Returns(response);

            // Act
            var result = await _controller.LoadHistoryGrid(request);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<YearEndHistoryItem>>(partial.Model);
            Assert.NotNull(model.Pagination);
        }

        #endregion

        #region TriggerInitiate

        [Fact]
        public async Task TriggerInitiate_WhenServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            const int plannedYear = 2025;
            var queued = new BatchJobQueueDto { JobId = 1, RequestedBy = "user@test.com" };
            _yearEndService.EnqueueYearEndCutOverInitiationJobAsync(plannedYear)
                .Returns(ApiResponseDto<BatchJobQueueDto>.SuccessResponse(queued));

            // Act
            var result = await _controller.TriggerInitiate(plannedYear);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = GetJsonElement(jsonResult);
            Assert.True(json.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task TriggerInitiate_WhenServiceReturnsFailure_ReturnsJsonWithSuccessFalseAndErrors()
        {
            // Arrange
            const int plannedYear = 2025;
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Already initiated", Code = "CONFLICT" } };
            _yearEndService.EnqueueYearEndCutOverInitiationJobAsync(plannedYear)
                .Returns(ApiResponseDto<BatchJobQueueDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.TriggerInitiate(plannedYear);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = GetJsonElement(jsonResult);
            Assert.False(json.GetProperty("success").GetBoolean());
            Assert.True(json.GetProperty("errors").GetArrayLength() > 0);
        }

        [Fact]
        public async Task TriggerInitiate_WhenServiceReturnsNullErrors_ReturnsDefaultErrorMessage()
        {
            // Arrange
            const int plannedYear = 2025;
            _yearEndService.EnqueueYearEndCutOverInitiationJobAsync(plannedYear)
                .Returns(new ApiResponseDto<BatchJobQueueDto> { Success = false, Errors = null });

            // Act
            var result = await _controller.TriggerInitiate(plannedYear);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = GetJsonElement(jsonResult);
            Assert.False(json.GetProperty("success").GetBoolean());
            var errorsArray = json.GetProperty("errors");
            Assert.Equal(1, errorsArray.GetArrayLength());
            Assert.Contains("CutOver initiation", errorsArray[0].GetProperty("message").GetString()!);
        }

        [Fact]
        public async Task TriggerInitiate_PassesCorrectPlannedYearToService()
        {
            // Arrange
            const int plannedYear = 2026;
            _yearEndService.EnqueueYearEndCutOverInitiationJobAsync(plannedYear)
                .Returns(ApiResponseDto<BatchJobQueueDto>.SuccessResponse(new BatchJobQueueDto()));

            // Act
            await _controller.TriggerInitiate(plannedYear);

            // Assert
            await _yearEndService.Received(1).EnqueueYearEndCutOverInitiationJobAsync(plannedYear);
        }

        #endregion

        #region TriggerApprove

        [Fact]
        public async Task TriggerApprove_WhenServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            const int plannedYear = 2025;
            var triggerDto = new BatchJobEventTriggerDto { EventId = "evt-cutover-001" };
            _yearEndService.TriggerYearEndCutOverApprovalJobAsync(plannedYear)
                .Returns(ApiResponseDto<BatchJobEventTriggerDto>.SuccessResponse(triggerDto));

            // Act
            var result = await _controller.TriggerApprove(plannedYear);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = GetJsonElement(jsonResult);
            Assert.True(json.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task TriggerApprove_WhenServiceReturnsFailure_ReturnsJsonWithSuccessFalseAndErrors()
        {
            // Arrange
            const int plannedYear = 2025;
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Approval not allowed", Code = "VALIDATION_ERROR" } };
            _yearEndService.TriggerYearEndCutOverApprovalJobAsync(plannedYear)
                .Returns(ApiResponseDto<BatchJobEventTriggerDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.TriggerApprove(plannedYear);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = GetJsonElement(jsonResult);
            Assert.False(json.GetProperty("success").GetBoolean());
            Assert.True(json.GetProperty("errors").GetArrayLength() > 0);
        }

        [Fact]
        public async Task TriggerApprove_WhenServiceReturnsNullErrors_ReturnsDefaultErrorMessage()
        {
            // Arrange
            const int plannedYear = 2025;
            _yearEndService.TriggerYearEndCutOverApprovalJobAsync(plannedYear)
                .Returns(new ApiResponseDto<BatchJobEventTriggerDto> { Success = false, Errors = null });

            // Act
            var result = await _controller.TriggerApprove(plannedYear);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = GetJsonElement(jsonResult);
            Assert.False(json.GetProperty("success").GetBoolean());
            var errorsArray = json.GetProperty("errors");
            Assert.Equal(1, errorsArray.GetArrayLength());
            Assert.Contains("CutOver approval", errorsArray[0].GetProperty("message").GetString()!);
        }

        [Fact]
        public async Task TriggerApprove_PassesCorrectPlannedYearToService()
        {
            // Arrange
            const int plannedYear = 2026;
            _yearEndService.TriggerYearEndCutOverApprovalJobAsync(plannedYear)
                .Returns(ApiResponseDto<BatchJobEventTriggerDto>.SuccessResponse(new BatchJobEventTriggerDto()));

            // Act
            await _controller.TriggerApprove(plannedYear);

            // Assert
            await _yearEndService.Received(1).TriggerYearEndCutOverApprovalJobAsync(plannedYear);
        }

        #endregion

        #region TriggerReject

        [Fact]
        public async Task TriggerReject_WhenServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            const int plannedYear = 2025;
            _yearEndService.EnqueueYearEndCutOverRejectJobAsync(plannedYear)
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.TriggerReject(plannedYear);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = GetJsonElement(jsonResult);
            Assert.True(json.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task TriggerReject_WhenServiceReturnsFailure_ReturnsJsonWithSuccessFalseAndErrors()
        {
            // Arrange
            const int plannedYear = 2025;
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Rejection not allowed", Code = "VALIDATION_ERROR" } };
            _yearEndService.EnqueueYearEndCutOverRejectJobAsync(plannedYear)
                .Returns(ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.TriggerReject(plannedYear);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = GetJsonElement(jsonResult);
            Assert.False(json.GetProperty("success").GetBoolean());
            Assert.True(json.GetProperty("errors").GetArrayLength() > 0);
        }

        [Fact]
        public async Task TriggerReject_WhenServiceReturnsNullErrors_ReturnsDefaultErrorMessage()
        {
            // Arrange
            const int plannedYear = 2025;
            _yearEndService.EnqueueYearEndCutOverRejectJobAsync(plannedYear)
                .Returns(new ApiResponseDto<bool> { Success = false, Errors = null });

            // Act
            var result = await _controller.TriggerReject(plannedYear);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = GetJsonElement(jsonResult);
            Assert.False(json.GetProperty("success").GetBoolean());
            var errorsArray = json.GetProperty("errors");
            Assert.Equal(1, errorsArray.GetArrayLength());
            Assert.Contains("CutOver reject", errorsArray[0].GetProperty("message").GetString()!);
        }

        [Fact]
        public async Task TriggerReject_PassesCorrectPlannedYearToService()
        {
            // Arrange
            const int plannedYear = 2026;
            _yearEndService.EnqueueYearEndCutOverRejectJobAsync(plannedYear)
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            await _controller.TriggerReject(plannedYear);

            // Assert
            await _yearEndService.Received(1).EnqueueYearEndCutOverRejectJobAsync(plannedYear);
        }

        [Fact]
        public async Task TriggerReject_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            const int plannedYear = 2025;
            _yearEndService.EnqueueYearEndCutOverRejectJobAsync(plannedYear)
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.TriggerReject(plannedYear));
        }

        #endregion
    }
}
