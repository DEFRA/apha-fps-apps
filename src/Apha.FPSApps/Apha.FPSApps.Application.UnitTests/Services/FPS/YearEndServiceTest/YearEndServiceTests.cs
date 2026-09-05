using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.YearEndServiceTest
{
    public class YearEndServiceTests
    {
        private const string JobName = "YearEnd-DataSetup";
        private const int PlannedYear = 2025;

        private readonly IFpsApiClient _fpsClient;
        private readonly IFpsYearEndApiClient _fpsYearEndApiClient;
        private readonly YearEndService _sut;

        public YearEndServiceTests()
        {
            _fpsClient = Substitute.For<IFpsApiClient>();
            _fpsYearEndApiClient = Substitute.For<IFpsYearEndApiClient>();
            _fpsClient.FpsYearEnd.Returns(_fpsYearEndApiClient);
            _sut = new YearEndService(_fpsClient);
        }

        #region GetYearEndDataSetupBatchJobHistoryAsync

        [Fact]
        public async Task GetYearEndDataSetupBatchJobHistoryAsync_WhenApiReturnsSuccess_ReturnsPaginatedHistory()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var history = new List<BatchJobHistoryDto>
            {
                new BatchJobHistoryDto { JobId = 1, JobName = JobName, Status = "Completed", RequestedBy = "user@test.com" },
                new BatchJobHistoryDto { JobId = 1, JobName = JobName, Status = "Failed",    RequestedBy = "user@test.com" }
            };
            var paginated = new PaginatedResult<BatchJobHistoryDto>(history, 2);
            var expectedResponse = ApiResponseDto<PaginatedResult<BatchJobHistoryDto>>.SuccessResponse(paginated);
            _fpsYearEndApiClient.GetYearEndDataSetupBatchJobHistoryAsync(query, JobName).Returns(expectedResponse);

            // Act
            var result = await _sut.GetYearEndDataSetupBatchJobHistoryAsync(query, JobName);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.TotalCount);
            await _fpsYearEndApiClient.Received(1).GetYearEndDataSetupBatchJobHistoryAsync(query, JobName);
        }

        [Fact]
        public async Task GetYearEndDataSetupBatchJobHistoryAsync_WhenApiReturnsEmptyResult_ReturnsSuccessWithEmptyData()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginated = new PaginatedResult<BatchJobHistoryDto>(new List<BatchJobHistoryDto>(), 0);
            var expectedResponse = ApiResponseDto<PaginatedResult<BatchJobHistoryDto>>.SuccessResponse(paginated);
            _fpsYearEndApiClient.GetYearEndDataSetupBatchJobHistoryAsync(query, JobName).Returns(expectedResponse);

            // Act
            var result = await _sut.GetYearEndDataSetupBatchJobHistoryAsync(query, JobName);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!.data);
            await _fpsYearEndApiClient.Received(1).GetYearEndDataSetupBatchJobHistoryAsync(query, JobName);
        }

        [Fact]
        public async Task GetYearEndDataSetupBatchJobHistoryAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Unauthorized", Code = "UNAUTHORIZED" } };
            var expectedResponse = ApiResponseDto<PaginatedResult<BatchJobHistoryDto>>.FailureResponse(errors, new ApiMetaDto());
            _fpsYearEndApiClient.GetYearEndDataSetupBatchJobHistoryAsync(query, JobName).Returns(expectedResponse);

            // Act
            var result = await _sut.GetYearEndDataSetupBatchJobHistoryAsync(query, JobName);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
            await _fpsYearEndApiClient.Received(1).GetYearEndDataSetupBatchJobHistoryAsync(query, JobName);
        }

        [Fact]
        public async Task GetYearEndDataSetupBatchJobHistoryAsync_WhenApiClientThrowsException_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string>();
            _fpsYearEndApiClient.GetYearEndDataSetupBatchJobHistoryAsync(query, JobName)
                .ThrowsAsync(new Exception("API unavailable"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _sut.GetYearEndDataSetupBatchJobHistoryAsync(query, JobName));
            Assert.Equal("API unavailable", exception.Message);
            await _fpsYearEndApiClient.Received(1).GetYearEndDataSetupBatchJobHistoryAsync(query, JobName);
        }

        #endregion

        #region CanInitiateDataSetupRequestAsync

        [Fact]
        public async Task CanInitiateDataSetupRequestAsync_WhenApiReturnsTrue_ReturnsSuccessWithTrue()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _fpsYearEndApiClient.CanInitiateDataSetupRequestAsync(JobName).Returns(expectedResponse);

            // Act
            var result = await _sut.CanInitiateDataSetupRequestAsync(JobName);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _fpsYearEndApiClient.Received(1).CanInitiateDataSetupRequestAsync(JobName);
        }

        [Fact]
        public async Task CanInitiateDataSetupRequestAsync_WhenApiReturnsFalse_ReturnsSuccessWithFalse()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(false);
            _fpsYearEndApiClient.CanInitiateDataSetupRequestAsync(JobName).Returns(expectedResponse);

            // Act
            var result = await _sut.CanInitiateDataSetupRequestAsync(JobName);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data);
            await _fpsYearEndApiClient.Received(1).CanInitiateDataSetupRequestAsync(JobName);
        }

        [Fact]
        public async Task CanInitiateDataSetupRequestAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Service error", Code = "SERVICE_ERROR" } };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _fpsYearEndApiClient.CanInitiateDataSetupRequestAsync(JobName).Returns(expectedResponse);

            // Act
            var result = await _sut.CanInitiateDataSetupRequestAsync(JobName);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            await _fpsYearEndApiClient.Received(1).CanInitiateDataSetupRequestAsync(JobName);
        }

        [Fact]
        public async Task CanInitiateDataSetupRequestAsync_WhenApiClientThrowsException_PropagatesException()
        {
            // Arrange
            _fpsYearEndApiClient.CanInitiateDataSetupRequestAsync(JobName)
                .ThrowsAsync(new Exception("API unavailable"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _sut.CanInitiateDataSetupRequestAsync(JobName));
            Assert.Equal("API unavailable", exception.Message);
            await _fpsYearEndApiClient.Received(1).CanInitiateDataSetupRequestAsync(JobName);
        }

        #endregion

        #region CanApproveOrRejectDataSetupRequestAsync

        [Fact]
        public async Task CanApproveOrRejectDataSetupRequestAsync_WhenApiReturnsTrue_ReturnsSuccessWithTrue()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _fpsYearEndApiClient.CanApproveOrRejectDataSetupRequestAsync(JobName).Returns(expectedResponse);

            // Act
            var result = await _sut.CanApproveOrRejectDataSetupRequestAsync(JobName);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _fpsYearEndApiClient.Received(1).CanApproveOrRejectDataSetupRequestAsync(JobName);
        }

        [Fact]
        public async Task CanApproveOrRejectDataSetupRequestAsync_WhenApiReturnsFalse_ReturnsSuccessWithFalse()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(false);
            _fpsYearEndApiClient.CanApproveOrRejectDataSetupRequestAsync(JobName).Returns(expectedResponse);

            // Act
            var result = await _sut.CanApproveOrRejectDataSetupRequestAsync(JobName);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data);
            await _fpsYearEndApiClient.Received(1).CanApproveOrRejectDataSetupRequestAsync(JobName);
        }

        [Fact]
        public async Task CanApproveOrRejectDataSetupRequestAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Forbidden", Code = "FORBIDDEN" } };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _fpsYearEndApiClient.CanApproveOrRejectDataSetupRequestAsync(JobName).Returns(expectedResponse);

            // Act
            var result = await _sut.CanApproveOrRejectDataSetupRequestAsync(JobName);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            await _fpsYearEndApiClient.Received(1).CanApproveOrRejectDataSetupRequestAsync(JobName);
        }

        [Fact]
        public async Task CanApproveOrRejectDataSetupRequestAsync_WhenApiClientThrowsException_PropagatesException()
        {
            // Arrange
            _fpsYearEndApiClient.CanApproveOrRejectDataSetupRequestAsync(JobName)
                .ThrowsAsync(new Exception("API unavailable"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _sut.CanApproveOrRejectDataSetupRequestAsync(JobName));
            Assert.Equal("API unavailable", exception.Message);
            await _fpsYearEndApiClient.Received(1).CanApproveOrRejectDataSetupRequestAsync(JobName);
        }

        #endregion

        #region GetInitiatedDataSetupJobExecutionIdAsync

        [Fact]
        public async Task GetInitiatedDataSetupJobExecutionIdAsync_WhenApiReturnsAnId_ReturnsIt()
        {
            // Arrange
            var jobExecutionId = Guid.NewGuid();
            var expectedResponse = ApiResponseDto<Guid?>.SuccessResponse(jobExecutionId);
            _fpsYearEndApiClient.GetInitiatedDataSetupJobExecutionIdAsync().Returns(expectedResponse);

            // Act
            var result = await _sut.GetInitiatedDataSetupJobExecutionIdAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(jobExecutionId, result.Data);
            await _fpsYearEndApiClient.Received(1).GetInitiatedDataSetupJobExecutionIdAsync();
        }

        [Fact]
        public async Task GetInitiatedDataSetupJobExecutionIdAsync_WhenApiReturnsNull_ReturnsNull()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<Guid?>.SuccessResponse(null);
            _fpsYearEndApiClient.GetInitiatedDataSetupJobExecutionIdAsync().Returns(expectedResponse);

            // Act
            var result = await _sut.GetInitiatedDataSetupJobExecutionIdAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task GetInitiatedDataSetupJobExecutionIdAsync_WhenApiClientThrowsException_PropagatesException()
        {
            // Arrange
            _fpsYearEndApiClient.GetInitiatedDataSetupJobExecutionIdAsync()
                .ThrowsAsync(new Exception("API unavailable"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _sut.GetInitiatedDataSetupJobExecutionIdAsync());
            Assert.Equal("API unavailable", exception.Message);
        }

        #endregion

        #region EnqueueYearEndDataSetupInitiationJobAsync

        [Fact]
        public async Task EnqueueYearEndDataSetupInitiationJobAsync_WhenApiReturnsSuccess_ReturnsBatchJobQueueDto()
        {
            // Arrange
            var queued = new BatchJobQueueDto
            {
                JobId = 1,
                RequestedBy = "user@test.com",
                RequestedAtUtc = DateTime.UtcNow
            };
            var expectedResponse = ApiResponseDto<BatchJobQueueDto>.SuccessResponse(queued);
            _fpsYearEndApiClient.EnqueueYearEndDataSetupInitiationJobAsync(PlannedYear).Returns(expectedResponse);

            // Act
            var result = await _sut.EnqueueYearEndDataSetupInitiationJobAsync(PlannedYear);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(1, result.Data?.JobId);
            await _fpsYearEndApiClient.Received(1).EnqueueYearEndDataSetupInitiationJobAsync(PlannedYear);
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupInitiationJobAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Job already running", Code = "CONFLICT" }
            };
            var expectedResponse = ApiResponseDto<BatchJobQueueDto>.FailureResponse(errors, new ApiMetaDto());
            _fpsYearEndApiClient.EnqueueYearEndDataSetupInitiationJobAsync(PlannedYear).Returns(expectedResponse);

            // Act
            var result = await _sut.EnqueueYearEndDataSetupInitiationJobAsync(PlannedYear);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
            await _fpsYearEndApiClient.Received(1).EnqueueYearEndDataSetupInitiationJobAsync(PlannedYear);
        }

        [Theory]
        [InlineData(2025)]
        [InlineData(2026)]
        public async Task EnqueueYearEndDataSetupInitiationJobAsync_PassesPlannedYearToApiClient(int plannedYear)
        {
            // Arrange
            var expectedResponse = ApiResponseDto<BatchJobQueueDto>.SuccessResponse(new BatchJobQueueDto());
            _fpsYearEndApiClient.EnqueueYearEndDataSetupInitiationJobAsync(plannedYear).Returns(expectedResponse);

            // Act
            await _sut.EnqueueYearEndDataSetupInitiationJobAsync(plannedYear);

            // Assert
            await _fpsYearEndApiClient.Received(1).EnqueueYearEndDataSetupInitiationJobAsync(plannedYear);
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupInitiationJobAsync_WhenApiClientThrowsException_PropagatesException()
        {
            // Arrange
            _fpsYearEndApiClient.EnqueueYearEndDataSetupInitiationJobAsync(PlannedYear)
                .ThrowsAsync(new Exception("Enqueue failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _sut.EnqueueYearEndDataSetupInitiationJobAsync(PlannedYear));
            Assert.Equal("Enqueue failed", exception.Message);
            await _fpsYearEndApiClient.Received(1).EnqueueYearEndDataSetupInitiationJobAsync(PlannedYear);
        }

        #endregion

        #region TriggerYearEndDataSetupApprovalJobAsync

        [Fact]
        public async Task TriggerYearEndDataSetupApprovalJobAsync_WhenApiReturnsSuccess_ReturnsBatchJobEventTriggerDto()
        {
            // Arrange
            var jobExecutionId = Guid.NewGuid();
            var eventTrigger = new BatchJobEventTriggerDto
            {
                EventId = "evt-001",
                Jobqueue = new BatchJobQueueDto { JobId = 1, RequestedBy = "approver@test.com" }
            };
            var expectedResponse = ApiResponseDto<BatchJobEventTriggerDto>.SuccessResponse(eventTrigger);
            _fpsYearEndApiClient.TriggerYearEndDataSetupApprovalJobAsync(PlannedYear, jobExecutionId).Returns(expectedResponse);

            // Act
            var result = await _sut.TriggerYearEndDataSetupApprovalJobAsync(PlannedYear, jobExecutionId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("evt-001", result.Data?.EventId);
            await _fpsYearEndApiClient.Received(1).TriggerYearEndDataSetupApprovalJobAsync(PlannedYear, jobExecutionId);
        }

        [Fact]
        public async Task TriggerYearEndDataSetupApprovalJobAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var jobExecutionId = Guid.NewGuid();
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Approval not allowed", Code = "VALIDATION_ERROR" }
            };
            var expectedResponse = ApiResponseDto<BatchJobEventTriggerDto>.FailureResponse(errors, new ApiMetaDto());
            _fpsYearEndApiClient.TriggerYearEndDataSetupApprovalJobAsync(PlannedYear, jobExecutionId).Returns(expectedResponse);

            // Act
            var result = await _sut.TriggerYearEndDataSetupApprovalJobAsync(PlannedYear, jobExecutionId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
            await _fpsYearEndApiClient.Received(1).TriggerYearEndDataSetupApprovalJobAsync(PlannedYear, jobExecutionId);
        }

        [Theory]
        [InlineData(2025)]
        [InlineData(2026)]
        public async Task TriggerYearEndDataSetupApprovalJobAsync_PassesPlannedYearToApiClient(int plannedYear)
        {
            // Arrange
            var jobExecutionId = Guid.NewGuid();
            var expectedResponse = ApiResponseDto<BatchJobEventTriggerDto>.SuccessResponse(
                new BatchJobEventTriggerDto { Jobqueue = new BatchJobQueueDto() });
            _fpsYearEndApiClient.TriggerYearEndDataSetupApprovalJobAsync(plannedYear, jobExecutionId).Returns(expectedResponse);

            // Act
            await _sut.TriggerYearEndDataSetupApprovalJobAsync(plannedYear, jobExecutionId);

            // Assert
            await _fpsYearEndApiClient.Received(1).TriggerYearEndDataSetupApprovalJobAsync(plannedYear, jobExecutionId);
        }

        [Fact]
        public async Task TriggerYearEndDataSetupApprovalJobAsync_WhenApiClientThrowsException_PropagatesException()
        {
            // Arrange
            var jobExecutionId = Guid.NewGuid();
            _fpsYearEndApiClient.TriggerYearEndDataSetupApprovalJobAsync(PlannedYear, jobExecutionId)
                .ThrowsAsync(new Exception("Approval failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _sut.TriggerYearEndDataSetupApprovalJobAsync(PlannedYear, jobExecutionId));
            Assert.Equal("Approval failed", exception.Message);
            await _fpsYearEndApiClient.Received(1).TriggerYearEndDataSetupApprovalJobAsync(PlannedYear, jobExecutionId);
        }

        #endregion

        #region EnqueueYearEndDataSetupRejectJobAsync

        [Fact]
        public async Task EnqueueYearEndDataSetupRejectJobAsync_WhenApiReturnsSuccess_ReturnsSuccessResponseWithTrue()
        {
            // Arrange
            var jobExecutionId = Guid.NewGuid();
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _fpsYearEndApiClient.EnqueueYearEndDataSetupRejectJobAsync(PlannedYear, jobExecutionId).Returns(expectedResponse);

            // Act
            var result = await _sut.EnqueueYearEndDataSetupRejectJobAsync(PlannedYear, jobExecutionId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _fpsYearEndApiClient.Received(1).EnqueueYearEndDataSetupRejectJobAsync(PlannedYear, jobExecutionId);
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupRejectJobAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var jobExecutionId = Guid.NewGuid();
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Rejection not allowed", Code = "VALIDATION_ERROR" }
            };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _fpsYearEndApiClient.EnqueueYearEndDataSetupRejectJobAsync(PlannedYear, jobExecutionId).Returns(expectedResponse);

            // Act
            var result = await _sut.EnqueueYearEndDataSetupRejectJobAsync(PlannedYear, jobExecutionId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
            await _fpsYearEndApiClient.Received(1).EnqueueYearEndDataSetupRejectJobAsync(PlannedYear, jobExecutionId);
        }

        [Theory]
        [InlineData(2025)]
        [InlineData(2026)]
        public async Task EnqueueYearEndDataSetupRejectJobAsync_PassesPlannedYearToApiClient(int plannedYear)
        {
            // Arrange
            var jobExecutionId = Guid.NewGuid();
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _fpsYearEndApiClient.EnqueueYearEndDataSetupRejectJobAsync(plannedYear, jobExecutionId).Returns(expectedResponse);

            // Act
            await _sut.EnqueueYearEndDataSetupRejectJobAsync(plannedYear, jobExecutionId);

            // Assert
            await _fpsYearEndApiClient.Received(1).EnqueueYearEndDataSetupRejectJobAsync(plannedYear, jobExecutionId);
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupRejectJobAsync_WhenApiClientThrowsException_PropagatesException()
        {
            // Arrange
            var jobExecutionId = Guid.NewGuid();
            _fpsYearEndApiClient.EnqueueYearEndDataSetupRejectJobAsync(PlannedYear, jobExecutionId)
                .ThrowsAsync(new Exception("Reject failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _sut.EnqueueYearEndDataSetupRejectJobAsync(PlannedYear, jobExecutionId));
            Assert.Equal("Reject failed", exception.Message);
            await _fpsYearEndApiClient.Received(1).EnqueueYearEndDataSetupRejectJobAsync(PlannedYear, jobExecutionId);
        }

        #endregion

        #region GetYearEndCutOverBatchJobHistoryAsync

        [Fact]
        public async Task GetYearEndCutOverBatchJobHistoryAsync_WhenApiReturnsSuccess_ReturnsPaginatedHistory()
        {
            // Arrange
            const string cutOverJobName = "YearEnd-CutOver";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var history = new List<BatchJobHistoryDto>
            {
                new BatchJobHistoryDto { JobId = 1, JobName = cutOverJobName, Status = "Completed", RequestedBy = "user@test.com" }
            };
            var paginated = new PaginatedResult<BatchJobHistoryDto>(history, 1);
            var expectedResponse = ApiResponseDto<PaginatedResult<BatchJobHistoryDto>>.SuccessResponse(paginated);
            _fpsYearEndApiClient.GetYearEndCutOverBatchJobHistoryAsync(query, cutOverJobName).Returns(expectedResponse);

            // Act
            var result = await _sut.GetYearEndCutOverBatchJobHistoryAsync(query, cutOverJobName);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(1, result.Data?.TotalCount);
            await _fpsYearEndApiClient.Received(1).GetYearEndCutOverBatchJobHistoryAsync(query, cutOverJobName);
        }

        [Fact]
        public async Task GetYearEndCutOverBatchJobHistoryAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            const string cutOverJobName = "YearEnd-CutOver";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Unauthorized", Code = "UNAUTHORIZED" } };
            var expectedResponse = ApiResponseDto<PaginatedResult<BatchJobHistoryDto>>.FailureResponse(errors, new ApiMetaDto());
            _fpsYearEndApiClient.GetYearEndCutOverBatchJobHistoryAsync(query, cutOverJobName).Returns(expectedResponse);

            // Act
            var result = await _sut.GetYearEndCutOverBatchJobHistoryAsync(query, cutOverJobName);

            // Assert
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
            await _fpsYearEndApiClient.Received(1).GetYearEndCutOverBatchJobHistoryAsync(query, cutOverJobName);
        }

        [Fact]
        public async Task GetYearEndCutOverBatchJobHistoryAsync_WhenApiClientThrowsException_PropagatesException()
        {
            // Arrange
            const string cutOverJobName = "YearEnd-CutOver";
            var query = new QueryParameters<string>();
            _fpsYearEndApiClient.GetYearEndCutOverBatchJobHistoryAsync(query, cutOverJobName)
                .ThrowsAsync(new Exception("API unavailable"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _sut.GetYearEndCutOverBatchJobHistoryAsync(query, cutOverJobName));
            Assert.Equal("API unavailable", exception.Message);
        }

        #endregion

        #region CanInitiateCutOverRequestAsync

        [Fact]
        public async Task CanInitiateCutOverRequestAsync_WhenApiReturnsTrue_ReturnsSuccessWithTrue()
        {
            // Arrange
            const string cutOverJobName = "YearEnd-CutOver";
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _fpsYearEndApiClient.CanInitiateCutOverRequestAsync(cutOverJobName).Returns(expectedResponse);

            // Act
            var result = await _sut.CanInitiateCutOverRequestAsync(cutOverJobName);

            // Assert
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _fpsYearEndApiClient.Received(1).CanInitiateCutOverRequestAsync(cutOverJobName);
        }

        [Fact]
        public async Task CanInitiateCutOverRequestAsync_WhenApiReturnsFalse_ReturnsSuccessWithFalse()
        {
            // Arrange
            const string cutOverJobName = "YearEnd-CutOver";
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(false);
            _fpsYearEndApiClient.CanInitiateCutOverRequestAsync(cutOverJobName).Returns(expectedResponse);

            // Act
            var result = await _sut.CanInitiateCutOverRequestAsync(cutOverJobName);

            // Assert
            Assert.True(result.Success);
            Assert.False(result.Data);
        }

        [Fact]
        public async Task CanInitiateCutOverRequestAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            const string cutOverJobName = "YearEnd-CutOver";
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Error", Code = "ERROR" } };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _fpsYearEndApiClient.CanInitiateCutOverRequestAsync(cutOverJobName).Returns(expectedResponse);

            // Act
            var result = await _sut.CanInitiateCutOverRequestAsync(cutOverJobName);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task CanInitiateCutOverRequestAsync_WhenApiClientThrowsException_PropagatesException()
        {
            // Arrange
            const string cutOverJobName = "YearEnd-CutOver";
            _fpsYearEndApiClient.CanInitiateCutOverRequestAsync(cutOverJobName)
                .ThrowsAsync(new Exception("API unavailable"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sut.CanInitiateCutOverRequestAsync(cutOverJobName));
        }

        #endregion

        #region CanApproveOrRejectCutOverRequestAsync

        [Fact]
        public async Task CanApproveOrRejectCutOverRequestAsync_WhenApiReturnsTrue_ReturnsSuccessWithTrue()
        {
            // Arrange
            const string cutOverJobName = "YearEnd-CutOver";
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _fpsYearEndApiClient.CanApproveOrRejectCutOverRequestAsync(cutOverJobName).Returns(expectedResponse);

            // Act
            var result = await _sut.CanApproveOrRejectCutOverRequestAsync(cutOverJobName);

            // Assert
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _fpsYearEndApiClient.Received(1).CanApproveOrRejectCutOverRequestAsync(cutOverJobName);
        }

        [Fact]
        public async Task CanApproveOrRejectCutOverRequestAsync_WhenApiReturnsFalse_ReturnsSuccessWithFalse()
        {
            // Arrange
            const string cutOverJobName = "YearEnd-CutOver";
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(false);
            _fpsYearEndApiClient.CanApproveOrRejectCutOverRequestAsync(cutOverJobName).Returns(expectedResponse);

            // Act
            var result = await _sut.CanApproveOrRejectCutOverRequestAsync(cutOverJobName);

            // Assert
            Assert.False(result.Data);
        }

        [Fact]
        public async Task CanApproveOrRejectCutOverRequestAsync_WhenApiClientThrowsException_PropagatesException()
        {
            // Arrange
            const string cutOverJobName = "YearEnd-CutOver";
            _fpsYearEndApiClient.CanApproveOrRejectCutOverRequestAsync(cutOverJobName)
                .ThrowsAsync(new Exception("API unavailable"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sut.CanApproveOrRejectCutOverRequestAsync(cutOverJobName));
        }

        #endregion

        #region EnqueueYearEndCutOverInitiationJobAsync

        [Fact]
        public async Task EnqueueYearEndCutOverInitiationJobAsync_WhenApiReturnsSuccess_ReturnsBatchJobQueueDto()
        {
            // Arrange
            var queued = new BatchJobQueueDto { JobId = 1, RequestedBy = "user@test.com" };
            var expectedResponse = ApiResponseDto<BatchJobQueueDto>.SuccessResponse(queued);
            _fpsYearEndApiClient.EnqueueYearEndCutOverInitiationJobAsync(PlannedYear).Returns(expectedResponse);

            // Act
            var result = await _sut.EnqueueYearEndCutOverInitiationJobAsync(PlannedYear);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(1, result.Data?.JobId);
            await _fpsYearEndApiClient.Received(1).EnqueueYearEndCutOverInitiationJobAsync(PlannedYear);
        }

        [Fact]
        public async Task EnqueueYearEndCutOverInitiationJobAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Job already running", Code = "CONFLICT" } };
            var expectedResponse = ApiResponseDto<BatchJobQueueDto>.FailureResponse(errors, new ApiMetaDto());
            _fpsYearEndApiClient.EnqueueYearEndCutOverInitiationJobAsync(PlannedYear).Returns(expectedResponse);

            // Act
            var result = await _sut.EnqueueYearEndCutOverInitiationJobAsync(PlannedYear);

            // Assert
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        [Theory]
        [InlineData(2025)]
        [InlineData(2026)]
        public async Task EnqueueYearEndCutOverInitiationJobAsync_PassesPlannedYearToApiClient(int plannedYear)
        {
            // Arrange
            var expectedResponse = ApiResponseDto<BatchJobQueueDto>.SuccessResponse(new BatchJobQueueDto());
            _fpsYearEndApiClient.EnqueueYearEndCutOverInitiationJobAsync(plannedYear).Returns(expectedResponse);

            // Act
            await _sut.EnqueueYearEndCutOverInitiationJobAsync(plannedYear);

            // Assert
            await _fpsYearEndApiClient.Received(1).EnqueueYearEndCutOverInitiationJobAsync(plannedYear);
        }

        [Fact]
        public async Task EnqueueYearEndCutOverInitiationJobAsync_WhenApiClientThrowsException_PropagatesException()
        {
            // Arrange
            _fpsYearEndApiClient.EnqueueYearEndCutOverInitiationJobAsync(PlannedYear)
                .ThrowsAsync(new Exception("Initiation failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _sut.EnqueueYearEndCutOverInitiationJobAsync(PlannedYear));
            Assert.Equal("Initiation failed", exception.Message);
        }

        #endregion

        #region GetInitiatedCutOverJobExecutionIdAsync

        [Fact]
        public async Task GetInitiatedCutOverJobExecutionIdAsync_WhenApiReturnsAnId_ReturnsIt()
        {
            // Arrange
            var jobExecutionId = Guid.NewGuid();
            var expectedResponse = ApiResponseDto<Guid?>.SuccessResponse(jobExecutionId);
            _fpsYearEndApiClient.GetInitiatedCutOverJobExecutionIdAsync().Returns(expectedResponse);

            // Act
            var result = await _sut.GetInitiatedCutOverJobExecutionIdAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(jobExecutionId, result.Data);
            await _fpsYearEndApiClient.Received(1).GetInitiatedCutOverJobExecutionIdAsync();
        }

        [Fact]
        public async Task GetInitiatedCutOverJobExecutionIdAsync_WhenApiReturnsNull_ReturnsNull()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<Guid?>.SuccessResponse(null);
            _fpsYearEndApiClient.GetInitiatedCutOverJobExecutionIdAsync().Returns(expectedResponse);

            // Act
            var result = await _sut.GetInitiatedCutOverJobExecutionIdAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Null(result.Data);
        }

        #endregion

        #region TriggerYearEndCutOverApprovalJobAsync

        [Fact]
        public async Task TriggerYearEndCutOverApprovalJobAsync_WhenApiReturnsSuccess_ReturnsBatchJobEventTriggerDto()
        {
            // Arrange
            var jobExecutionId = Guid.NewGuid();
            var eventTrigger = new BatchJobEventTriggerDto
            {
                EventId = "evt-cutover-001",
                Jobqueue = new BatchJobQueueDto { JobId = 1 }
            };
            var expectedResponse = ApiResponseDto<BatchJobEventTriggerDto>.SuccessResponse(eventTrigger);
            _fpsYearEndApiClient.TriggerYearEndCutOverApprovalJobAsync(PlannedYear, jobExecutionId).Returns(expectedResponse);

            // Act
            var result = await _sut.TriggerYearEndCutOverApprovalJobAsync(PlannedYear, jobExecutionId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("evt-cutover-001", result.Data?.EventId);
            await _fpsYearEndApiClient.Received(1).TriggerYearEndCutOverApprovalJobAsync(PlannedYear, jobExecutionId);
        }

        [Fact]
        public async Task TriggerYearEndCutOverApprovalJobAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var jobExecutionId = Guid.NewGuid();
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Approval not allowed", Code = "VALIDATION_ERROR" } };
            var expectedResponse = ApiResponseDto<BatchJobEventTriggerDto>.FailureResponse(errors, new ApiMetaDto());
            _fpsYearEndApiClient.TriggerYearEndCutOverApprovalJobAsync(PlannedYear, jobExecutionId).Returns(expectedResponse);

            // Act
            var result = await _sut.TriggerYearEndCutOverApprovalJobAsync(PlannedYear, jobExecutionId);

            // Assert
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        [Theory]
        [InlineData(2025)]
        [InlineData(2026)]
        public async Task TriggerYearEndCutOverApprovalJobAsync_PassesPlannedYearToApiClient(int plannedYear)
        {
            // Arrange
            var jobExecutionId = Guid.NewGuid();
            var expectedResponse = ApiResponseDto<BatchJobEventTriggerDto>.SuccessResponse(
                new BatchJobEventTriggerDto { Jobqueue = new BatchJobQueueDto() });
            _fpsYearEndApiClient.TriggerYearEndCutOverApprovalJobAsync(plannedYear, jobExecutionId).Returns(expectedResponse);

            // Act
            await _sut.TriggerYearEndCutOverApprovalJobAsync(plannedYear, jobExecutionId);

            // Assert
            await _fpsYearEndApiClient.Received(1).TriggerYearEndCutOverApprovalJobAsync(plannedYear, jobExecutionId);
        }

        [Fact]
        public async Task TriggerYearEndCutOverApprovalJobAsync_WhenApiClientThrowsException_PropagatesException()
        {
            // Arrange
            var jobExecutionId = Guid.NewGuid();
            _fpsYearEndApiClient.TriggerYearEndCutOverApprovalJobAsync(PlannedYear, jobExecutionId)
                .ThrowsAsync(new Exception("Approval failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _sut.TriggerYearEndCutOverApprovalJobAsync(PlannedYear, jobExecutionId));
            Assert.Equal("Approval failed", exception.Message);
        }

        #endregion

        #region EnqueueYearEndCutOverRejectJobAsync

        [Fact]
        public async Task EnqueueYearEndCutOverRejectJobAsync_WhenApiReturnsSuccess_ReturnsSuccessResponseWithTrue()
        {
            // Arrange
            var jobExecutionId = Guid.NewGuid();
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _fpsYearEndApiClient.EnqueueYearEndCutOverRejectJobAsync(PlannedYear, jobExecutionId).Returns(expectedResponse);

            // Act
            var result = await _sut.EnqueueYearEndCutOverRejectJobAsync(PlannedYear, jobExecutionId);

            // Assert
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _fpsYearEndApiClient.Received(1).EnqueueYearEndCutOverRejectJobAsync(PlannedYear, jobExecutionId);
        }

        [Fact]
        public async Task EnqueueYearEndCutOverRejectJobAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var jobExecutionId = Guid.NewGuid();
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Rejection not allowed", Code = "VALIDATION_ERROR" } };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _fpsYearEndApiClient.EnqueueYearEndCutOverRejectJobAsync(PlannedYear, jobExecutionId).Returns(expectedResponse);

            // Act
            var result = await _sut.EnqueueYearEndCutOverRejectJobAsync(PlannedYear, jobExecutionId);

            // Assert
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        [Theory]
        [InlineData(2025)]
        [InlineData(2026)]
        public async Task EnqueueYearEndCutOverRejectJobAsync_PassesPlannedYearToApiClient(int plannedYear)
        {
            // Arrange
            var jobExecutionId = Guid.NewGuid();
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _fpsYearEndApiClient.EnqueueYearEndCutOverRejectJobAsync(plannedYear, jobExecutionId).Returns(expectedResponse);

            // Act
            await _sut.EnqueueYearEndCutOverRejectJobAsync(plannedYear, jobExecutionId);

            // Assert
            await _fpsYearEndApiClient.Received(1).EnqueueYearEndCutOverRejectJobAsync(plannedYear, jobExecutionId);
        }

        [Fact]
        public async Task EnqueueYearEndCutOverRejectJobAsync_WhenApiClientThrowsException_PropagatesException()
        {
            // Arrange
            var jobExecutionId = Guid.NewGuid();
            _fpsYearEndApiClient.EnqueueYearEndCutOverRejectJobAsync(PlannedYear, jobExecutionId)
                .ThrowsAsync(new Exception("Reject failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _sut.EnqueueYearEndCutOverRejectJobAsync(PlannedYear, jobExecutionId));
            Assert.Equal("Reject failed", exception.Message);
        }

        #endregion
    }
}
