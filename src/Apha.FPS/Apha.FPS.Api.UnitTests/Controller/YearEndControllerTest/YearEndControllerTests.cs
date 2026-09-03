using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Api.Controllers;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Application.Validation;
using Apha.FPS.Core.Interfaces;
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPS.Api.UnitTests.Controller.YearEndControllerTest
{
    public class YearEndControllerTests
    {
        private const string JobName = "YearEnd-DataSetup";
        private const string CorrelationId = "corr-001";
        private const string UserEmail = "user@example.com";
        private const int FpsYear = 2024;
        private const int PlannedYear = 2025;
        private static readonly Guid JobExecutionId = Guid.NewGuid();

        private readonly IYearEndService _yearEndService;
        private readonly IFpsRequestContext _fpsRequestContext;
        private readonly IMapper _mapper;
        private readonly YearEndController _sut;

        public YearEndControllerTests()
        {
            _yearEndService = Substitute.For<IYearEndService>();
            _fpsRequestContext = Substitute.For<IFpsRequestContext>();
            _mapper = Substitute.For<IMapper>();

            _fpsRequestContext.FpsYear.Returns(FpsYear);
            _fpsRequestContext.UserEmailId.Returns(UserEmail);

            _sut = new YearEndController(_yearEndService, _fpsRequestContext, _mapper);
        }

        #region GetYearEndDataSetupBatchJobHistory

        [Fact]
        public async Task GetYearEndDataSetupBatchJobHistory_WhenDataExists_ReturnsOkWithMappedPaginationRes()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<BatchJobHistoryDto>
            {
                Data = [
                    new BatchJobHistoryDto { JobId = 1, JobName = JobName, Status = "Completed" },
                    new BatchJobHistoryDto { JobId = 1, JobName = JobName, Status = "Failed" }
                ]
            };
            var mappedResult = new PaginationRes<BatchJobHistoryRes>
            {
                Data = [
                    new BatchJobHistoryRes { JobId = 1, JobName = JobName, Status = "Completed" },
                    new BatchJobHistoryRes { JobId = 1, JobName = JobName, Status = "Failed" }
                ]
            };

            _yearEndService.GetBatchJobsHistoryAsync(query, JobName).Returns(serviceResult);
            _mapper.Map<PaginationRes<BatchJobHistoryRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _sut.GetYearEndDataSetupBatchJobHistory(query, JobName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(mappedResult);

            await _yearEndService.Received(1).GetBatchJobsHistoryAsync(query, JobName);
            _mapper.Received(1).Map<PaginationRes<BatchJobHistoryRes>>(serviceResult);
        }

        [Fact]
        public async Task GetYearEndDataSetupBatchJobHistory_WhenNoData_ReturnsOkWithEmptyPaginationRes()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<BatchJobHistoryDto> { Data = [] };
            var mappedResult = new PaginationRes<BatchJobHistoryRes> { Data = [] };

            _yearEndService.GetBatchJobsHistoryAsync(query, JobName).Returns(serviceResult);
            _mapper.Map<PaginationRes<BatchJobHistoryRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _sut.GetYearEndDataSetupBatchJobHistory(query, JobName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.StatusCode.Should().Be(200);

            await _yearEndService.Received(1).GetBatchJobsHistoryAsync(query, JobName);
        }

        [Fact]
        public async Task GetYearEndDataSetupBatchJobHistory_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string>();
            _yearEndService.GetBatchJobsHistoryAsync(query, JobName).Throws(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetYearEndDataSetupBatchJobHistory(query, JobName));
            exception.Message.Should().Be("Database error");
            await _yearEndService.Received(1).GetBatchJobsHistoryAsync(query, JobName);
        }

        #endregion

        #region CanInitiateYearEndDataSetupRequestAsync

        [Fact]
        public async Task CanInitiateYearEndDataSetupRequestAsync_WhenServiceReturnsTrue_ReturnsOkWithTrue()
        {
            // Arrange
            _yearEndService.CanInitiateYearEndDataSetupRequestAsync(JobName).Returns(true);

            // Act
            var result = await _sut.CanInitiateYearEndDataSetupRequestAsync(JobName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().Be(true);

            await _yearEndService.Received(1).CanInitiateYearEndDataSetupRequestAsync(JobName);
        }

        [Fact]
        public async Task CanInitiateYearEndDataSetupRequestAsync_WhenServiceReturnsFalse_ReturnsOkWithFalse()
        {
            // Arrange
            _yearEndService.CanInitiateYearEndDataSetupRequestAsync(JobName).Returns(false);

            // Act
            var result = await _sut.CanInitiateYearEndDataSetupRequestAsync(JobName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().Be(false);

            await _yearEndService.Received(1).CanInitiateYearEndDataSetupRequestAsync(JobName);
        }

        [Fact]
        public async Task CanInitiateYearEndDataSetupRequestAsync_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            _yearEndService.CanInitiateYearEndDataSetupRequestAsync(JobName).Throws(new Exception("Service error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.CanInitiateYearEndDataSetupRequestAsync(JobName));
            exception.Message.Should().Be("Service error");
            await _yearEndService.Received(1).CanInitiateYearEndDataSetupRequestAsync(JobName);
        }

        #endregion

        #region CanApproveOrRejectYearEndDataSetupRequestAsync

        [Fact]
        public async Task CanApproveOrRejectYearEndDataSetupRequestAsync_WhenServiceReturnsTrue_ReturnsOkWithTrue()
        {
            // Arrange
            _yearEndService.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Returns(true);

            // Act
            var result = await _sut.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().Be(true);

            await _yearEndService.Received(1).CanApproveOrRejectYearEndDataSetupRequestAsync(JobName);
        }

        [Fact]
        public async Task CanApproveOrRejectYearEndDataSetupRequestAsync_WhenServiceReturnsFalse_ReturnsOkWithFalse()
        {
            // Arrange
            _yearEndService.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Returns(false);

            // Act
            var result = await _sut.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().Be(false);

            await _yearEndService.Received(1).CanApproveOrRejectYearEndDataSetupRequestAsync(JobName);
        }

        [Fact]
        public async Task CanApproveOrRejectYearEndDataSetupRequestAsync_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            _yearEndService.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Throws(new Exception("Service error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName));
            exception.Message.Should().Be("Service error");
            await _yearEndService.Received(1).CanApproveOrRejectYearEndDataSetupRequestAsync(JobName);
        }

        #endregion

        #region EnqueueYearEndDataSetupInitiationJob

        [Fact]
        public async Task EnqueueYearEndDataSetupInitiationJob_WhenValid_ReturnsOkWithMappedResult()
        {
            // Arrange
            var request = new YearEndDataSetupReq { PlannedYear = PlannedYear };
            var serviceResult = new BatchJobQueueDto { RequestedBy = UserEmail };
            var mappedRes = new BatchJobQueueRes { RequestedBy = UserEmail };

            _yearEndService
                .EnqueueYearEndDataSetupInitiationJobAsync(PlannedYear, FpsYear, UserEmail, CorrelationId)
                .Returns(serviceResult);
            _mapper.Map<BatchJobQueueRes>(serviceResult).Returns(mappedRes);

            // Act
            var result = await _sut.EnqueueYearEndDataSetupInitiationJob(request, CorrelationId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(mappedRes);

            await _yearEndService.Received(1).EnqueueYearEndDataSetupInitiationJobAsync(
                PlannedYear, FpsYear, UserEmail, CorrelationId);
            _mapper.Received(1).Map<BatchJobQueueRes>(serviceResult);
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupInitiationJob_PassesFpsRequestContextValuesToService()
        {
            // Arrange
            var request = new YearEndDataSetupReq { PlannedYear = PlannedYear };
            var serviceResult = new BatchJobQueueDto();
            _yearEndService
                .EnqueueYearEndDataSetupInitiationJobAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(serviceResult);
            _mapper.Map<BatchJobQueueRes>(serviceResult).Returns(new BatchJobQueueRes());

            // Act
            await _sut.EnqueueYearEndDataSetupInitiationJob(request, CorrelationId);

            // Assert — context values forwarded correctly
            await _yearEndService.Received(1).EnqueueYearEndDataSetupInitiationJobAsync(
                PlannedYear,
                FpsYear,
                UserEmail,
                CorrelationId);
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupInitiationJob_WhenServiceThrowsBusinessValidationException_PropagatesException()
        {
            // Arrange
            var request = new YearEndDataSetupReq { PlannedYear = 0 };
            _yearEndService
                .EnqueueYearEndDataSetupInitiationJobAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>())
                .Throws(new BusinessValidationErrorException([new BusinessValidationError("Invalid year", "INVALID_PlannedYear")]));

            // Act & Assert
            await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndDataSetupInitiationJob(request, CorrelationId));
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupInitiationJob_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var request = new YearEndDataSetupReq { PlannedYear = PlannedYear };
            _yearEndService
                .EnqueueYearEndDataSetupInitiationJobAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>())
                .Throws(new Exception("Enqueue failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _sut.EnqueueYearEndDataSetupInitiationJob(request, CorrelationId));
            exception.Message.Should().Be("Enqueue failed");
        }

        #endregion

        #region EnqueueYearEndDataSetupApprovalJob

        [Fact]
        public async Task EnqueueYearEndDataSetupApprovalJob_WhenValid_ReturnsOkWithMappedResult()
        {
            // Arrange
            var request = new YearEndDataSetupReq { PlannedYear = PlannedYear };
            var serviceResult = new BatchJobEventTriggerDto { EventId = "evt-001" };
            var mappedRes = new BatchJobEventTriggerRes { EventId = "evt-001" };

            _yearEndService
                .EnqueueYearEndDataSetupApprovalJobAsync(JobExecutionId, PlannedYear, FpsYear, UserEmail, CorrelationId)
                .Returns(serviceResult);
            _mapper.Map<BatchJobEventTriggerRes>(serviceResult).Returns(mappedRes);

            // Act
            var result = await _sut.EnqueueYearEndDataSetupApprovalJob(JobExecutionId, request, CorrelationId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(mappedRes);

            await _yearEndService.Received(1).EnqueueYearEndDataSetupApprovalJobAsync(
                JobExecutionId, PlannedYear, FpsYear, UserEmail, CorrelationId);
            _mapper.Received(1).Map<BatchJobEventTriggerRes>(serviceResult);
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupApprovalJob_PassesFpsRequestContextValuesToService()
        {
            // Arrange
            var request = new YearEndDataSetupReq { PlannedYear = PlannedYear };
            var serviceResult = new BatchJobEventTriggerDto();
            _yearEndService
                .EnqueueYearEndDataSetupApprovalJobAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(serviceResult);
            _mapper.Map<BatchJobEventTriggerRes>(serviceResult).Returns(new BatchJobEventTriggerRes());

            // Act
            await _sut.EnqueueYearEndDataSetupApprovalJob(JobExecutionId, request, CorrelationId);

            // Assert — context values forwarded correctly
            await _yearEndService.Received(1).EnqueueYearEndDataSetupApprovalJobAsync(
                JobExecutionId,
                PlannedYear,
                FpsYear,
                UserEmail,
                CorrelationId);
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupApprovalJob_WhenServiceThrowsBusinessValidationException_PropagatesException()
        {
            // Arrange
            var request = new YearEndDataSetupReq { PlannedYear = PlannedYear };
            _yearEndService
                .EnqueueYearEndDataSetupApprovalJobAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>())
                .Throws(new BusinessValidationErrorException([new BusinessValidationError("Same person", "INVALID_Approval")]));

            // Act & Assert
            await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndDataSetupApprovalJob(JobExecutionId, request, CorrelationId));
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupApprovalJob_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var request = new YearEndDataSetupReq { PlannedYear = PlannedYear };
            _yearEndService
                .EnqueueYearEndDataSetupApprovalJobAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>())
                .Throws(new Exception("Approval failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _sut.EnqueueYearEndDataSetupApprovalJob(JobExecutionId, request, CorrelationId));
            exception.Message.Should().Be("Approval failed");
        }

        #endregion

        #region EnqueueYearEndDataSetupRejectJob

        [Fact]
        public async Task EnqueueYearEndDataSetupRejectJob_WhenValid_ReturnsOkWithServiceResult()
        {
            // Arrange
            var request = new YearEndDataSetupReq { PlannedYear = PlannedYear };
            const bool serviceResult = true;

            _yearEndService
                .EnqueueYearEndDataSetupRejectJobAsync(JobExecutionId, PlannedYear, FpsYear, UserEmail, CorrelationId)
                .Returns(serviceResult);

            // Act
            var result = await _sut.EnqueueYearEndDataSetupRejectJob(JobExecutionId, request, CorrelationId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().Be(serviceResult);

            await _yearEndService.Received(1).EnqueueYearEndDataSetupRejectJobAsync(
                JobExecutionId, PlannedYear, FpsYear, UserEmail, CorrelationId);
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupRejectJob_PassesFpsRequestContextValuesToService()
        {
            // Arrange
            var request = new YearEndDataSetupReq { PlannedYear = PlannedYear };
            _yearEndService
                .EnqueueYearEndDataSetupRejectJobAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(true);
            _mapper.Map<BatchJobEventTriggerRes>(true).Returns(new BatchJobEventTriggerRes());

            // Act
            await _sut.EnqueueYearEndDataSetupRejectJob(JobExecutionId, request, CorrelationId);

            // Assert — context values forwarded correctly
            await _yearEndService.Received(1).EnqueueYearEndDataSetupRejectJobAsync(
                JobExecutionId, PlannedYear, FpsYear, UserEmail, CorrelationId);
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupRejectJob_WhenServiceThrowsBusinessValidationException_PropagatesException()
        {
            // Arrange
            var request = new YearEndDataSetupReq { PlannedYear = PlannedYear };
            _yearEndService
                .EnqueueYearEndDataSetupRejectJobAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>())
                .Throws(new BusinessValidationErrorException([new BusinessValidationError("Same person", "INVALID_Approval")]));

            // Act & Assert
            await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndDataSetupRejectJob(JobExecutionId, request, CorrelationId));
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupRejectJob_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var request = new YearEndDataSetupReq { PlannedYear = PlannedYear };
            _yearEndService
                .EnqueueYearEndDataSetupRejectJobAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>())
                .Throws(new Exception("Rejection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _sut.EnqueueYearEndDataSetupRejectJob(JobExecutionId, request, CorrelationId));
            exception.Message.Should().Be("Rejection failed");
        }

        #endregion

        #region GetYearEndCutOverBatchJobHistory

        [Fact]
        public async Task GetYearEndCutOverBatchJobHistory_WhenDataExists_ReturnsOkWithMappedPaginationRes()
        {
            // Arrange
            const string cutOverJobName = "YearEnd-CutOver";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<BatchJobHistoryDto>
            {
                Data = [new BatchJobHistoryDto { JobId = 1, JobName = cutOverJobName, Status = "Completed" }]
            };
            var mappedResult = new PaginationRes<BatchJobHistoryRes>
            {
                Data = [new BatchJobHistoryRes { JobId = 1, JobName = cutOverJobName, Status = "Completed" }]
            };

            _yearEndService.GetBatchJobsHistoryAsync(query, cutOverJobName).Returns(serviceResult);
            _mapper.Map<PaginationRes<BatchJobHistoryRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _sut.GetYearEndCutOverBatchJobHistory(query, cutOverJobName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(mappedResult);

            await _yearEndService.Received(1).GetBatchJobsHistoryAsync(query, cutOverJobName);
        }

        [Fact]
        public async Task GetYearEndCutOverBatchJobHistory_WhenNoData_ReturnsOkWithEmptyPaginationRes()
        {
            // Arrange
            const string cutOverJobName = "YearEnd-CutOver";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<BatchJobHistoryDto> { Data = [] };
            var mappedResult = new PaginationRes<BatchJobHistoryRes> { Data = [] };

            _yearEndService.GetBatchJobsHistoryAsync(query, cutOverJobName).Returns(serviceResult);
            _mapper.Map<PaginationRes<BatchJobHistoryRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _sut.GetYearEndCutOverBatchJobHistory(query, cutOverJobName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.StatusCode.Should().Be(200);
            await _yearEndService.Received(1).GetBatchJobsHistoryAsync(query, cutOverJobName);
        }

        [Fact]
        public async Task GetYearEndCutOverBatchJobHistory_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            const string cutOverJobName = "YearEnd-CutOver";
            var query = new QueryParameters<string>();
            _yearEndService.GetBatchJobsHistoryAsync(query, cutOverJobName).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sut.GetYearEndCutOverBatchJobHistory(query, cutOverJobName));
        }

        #endregion

        #region CanInitiateYearEndCutOverRequestAsync

        [Fact]
        public async Task CanInitiateYearEndCutOverRequestAsync_WhenServiceReturnsTrue_ReturnsOkWithTrue()
        {
            // Arrange
            const string cutOverJobName = "YearEnd-CutOver";
            _yearEndService.CanInitiateYearEndCutOverRequestAsync(cutOverJobName).Returns(true);

            // Act
            var result = await _sut.CanInitiateYearEndCutOverRequestAsync(cutOverJobName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().Be(true);

            await _yearEndService.Received(1).CanInitiateYearEndCutOverRequestAsync(cutOverJobName);
        }

        [Fact]
        public async Task CanInitiateYearEndCutOverRequestAsync_WhenServiceReturnsFalse_ReturnsOkWithFalse()
        {
            // Arrange
            const string cutOverJobName = "YearEnd-CutOver";
            _yearEndService.CanInitiateYearEndCutOverRequestAsync(cutOverJobName).Returns(false);

            // Act
            var result = await _sut.CanInitiateYearEndCutOverRequestAsync(cutOverJobName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().Be(false);
        }

        [Fact]
        public async Task CanInitiateYearEndCutOverRequestAsync_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            const string cutOverJobName = "YearEnd-CutOver";
            _yearEndService.CanInitiateYearEndCutOverRequestAsync(cutOverJobName).Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sut.CanInitiateYearEndCutOverRequestAsync(cutOverJobName));
        }

        #endregion

        #region CanApproveOrRejectYearEndCutOverRequestAsync

        [Fact]
        public async Task CanApproveOrRejectYearEndCutOverRequestAsync_WhenServiceReturnsTrue_ReturnsOkWithTrue()
        {
            // Arrange
            const string cutOverJobName = "YearEnd-CutOver";
            _yearEndService.CanApproveOrRejectYearEndCutOverRequestAsync(cutOverJobName).Returns(true);

            // Act
            var result = await _sut.CanApproveOrRejectYearEndCutOverRequestAsync(cutOverJobName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().Be(true);
        }

        [Fact]
        public async Task CanApproveOrRejectYearEndCutOverRequestAsync_WhenServiceReturnsFalse_ReturnsOkWithFalse()
        {
            // Arrange
            const string cutOverJobName = "YearEnd-CutOver";
            _yearEndService.CanApproveOrRejectYearEndCutOverRequestAsync(cutOverJobName).Returns(false);

            // Act
            var result = await _sut.CanApproveOrRejectYearEndCutOverRequestAsync(cutOverJobName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().Be(false);
        }

        [Fact]
        public async Task CanApproveOrRejectYearEndCutOverRequestAsync_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            const string cutOverJobName = "YearEnd-CutOver";
            _yearEndService.CanApproveOrRejectYearEndCutOverRequestAsync(cutOverJobName).Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sut.CanApproveOrRejectYearEndCutOverRequestAsync(cutOverJobName));
        }

        #endregion

        #region EnqueueYearEndCutOverInitiationJob

        [Fact]
        public async Task EnqueueYearEndCutOverInitiationJob_WhenValid_ReturnsOkWithMappedResult()
        {
            // Arrange
            var request = new YearEndDataSetupReq { PlannedYear = PlannedYear };
            var serviceResult = new BatchJobQueueDto { RequestedBy = UserEmail };
            var mappedRes = new BatchJobQueueRes { RequestedBy = UserEmail };

            _yearEndService
                .EnqueueYearEndCutOverInitiationJobAsync(PlannedYear, FpsYear, UserEmail, CorrelationId)
                .Returns(serviceResult);
            _mapper.Map<BatchJobQueueRes>(serviceResult).Returns(mappedRes);

            // Act
            var result = await _sut.EnqueueYearEndCutOverInitiationJob(request, CorrelationId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(mappedRes);

            await _yearEndService.Received(1).EnqueueYearEndCutOverInitiationJobAsync(
                PlannedYear, FpsYear, UserEmail, CorrelationId);
        }

        [Fact]
        public async Task EnqueueYearEndCutOverInitiationJob_PassesFpsRequestContextValuesToService()
        {
            // Arrange
            var request = new YearEndDataSetupReq { PlannedYear = PlannedYear };
            _yearEndService
                .EnqueueYearEndCutOverInitiationJobAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(new BatchJobQueueDto());
            _mapper.Map<BatchJobQueueRes>(Arg.Any<BatchJobQueueDto>()).Returns(new BatchJobQueueRes());

            // Act
            await _sut.EnqueueYearEndCutOverInitiationJob(request, CorrelationId);

            // Assert
            await _yearEndService.Received(1).EnqueueYearEndCutOverInitiationJobAsync(
                PlannedYear, FpsYear, UserEmail, CorrelationId);
        }

        [Fact]
        public async Task EnqueueYearEndCutOverInitiationJob_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var request = new YearEndDataSetupReq { PlannedYear = PlannedYear };
            _yearEndService
                .EnqueueYearEndCutOverInitiationJobAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>())
                .Throws(new Exception("Initiation failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _sut.EnqueueYearEndCutOverInitiationJob(request, CorrelationId));
            exception.Message.Should().Be("Initiation failed");
        }

        #endregion

        #region EnqueueYearEndCutOverApprovalJob

        [Fact]
        public async Task EnqueueYearEndCutOverApprovalJob_WhenValid_ReturnsOkWithMappedResult()
        {
            // Arrange
            var request = new YearEndDataSetupReq { PlannedYear = PlannedYear };
            var serviceResult = new BatchJobEventTriggerDto { EventId = "evt-cutover-001" };
            var mappedRes = new BatchJobEventTriggerRes { EventId = "evt-cutover-001" };

            _yearEndService
                .EnqueueYearEndCutOverApprovalJobAsync(PlannedYear, FpsYear, UserEmail, CorrelationId)
                .Returns(serviceResult);
            _mapper.Map<BatchJobEventTriggerRes>(serviceResult).Returns(mappedRes);

            // Act
            var result = await _sut.EnqueueYearEndCutOverApprovalJob(request, CorrelationId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(mappedRes);

            await _yearEndService.Received(1).EnqueueYearEndCutOverApprovalJobAsync(
                PlannedYear, FpsYear, UserEmail, CorrelationId);
        }

        [Fact]
        public async Task EnqueueYearEndCutOverApprovalJob_PassesFpsRequestContextValuesToService()
        {
            // Arrange
            var request = new YearEndDataSetupReq { PlannedYear = PlannedYear };
            var serviceResult = new BatchJobEventTriggerDto();
            _yearEndService
                .EnqueueYearEndCutOverApprovalJobAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(serviceResult);
            _mapper.Map<BatchJobEventTriggerRes>(serviceResult).Returns(new BatchJobEventTriggerRes());

            // Act
            await _sut.EnqueueYearEndCutOverApprovalJob(request, CorrelationId);

            // Assert
            await _yearEndService.Received(1).EnqueueYearEndCutOverApprovalJobAsync(
                PlannedYear, FpsYear, UserEmail, CorrelationId);
        }

        [Fact]
        public async Task EnqueueYearEndCutOverApprovalJob_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var request = new YearEndDataSetupReq { PlannedYear = PlannedYear };
            _yearEndService
                .EnqueueYearEndCutOverApprovalJobAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>())
                .Throws(new Exception("Approval failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _sut.EnqueueYearEndCutOverApprovalJob(request, CorrelationId));
            exception.Message.Should().Be("Approval failed");
        }

        #endregion

        #region EnqueueYearEndCutOverRejectJobAsync

        [Fact]
        public async Task EnqueueYearEndCutOverRejectJobAsync_WhenValid_ReturnsOkWithServiceResult()
        {
            // Arrange
            var request = new YearEndDataSetupReq { PlannedYear = PlannedYear };
            _yearEndService
                .EnqueueYearEndCutOverRejectJobAsync(PlannedYear, FpsYear, UserEmail, CorrelationId)
                .Returns(true);

            // Act
            var result = await _sut.EnqueueYearEndCutOverRejectJobAsync(request, CorrelationId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().Be(true);

            await _yearEndService.Received(1).EnqueueYearEndCutOverRejectJobAsync(
                PlannedYear, FpsYear, UserEmail, CorrelationId);
        }

        [Fact]
        public async Task EnqueueYearEndCutOverRejectJobAsync_PassesFpsRequestContextValuesToService()
        {
            // Arrange
            var request = new YearEndDataSetupReq { PlannedYear = PlannedYear };
            _yearEndService
                .EnqueueYearEndCutOverRejectJobAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(true);

            // Act
            await _sut.EnqueueYearEndCutOverRejectJobAsync(request, CorrelationId);

            // Assert
            await _yearEndService.Received(1).EnqueueYearEndCutOverRejectJobAsync(
                PlannedYear, FpsYear, UserEmail, CorrelationId);
        }

        [Fact]
        public async Task EnqueueYearEndCutOverRejectJobAsync_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var request = new YearEndDataSetupReq { PlannedYear = PlannedYear };
            _yearEndService
                .EnqueueYearEndCutOverRejectJobAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>())
                .Throws(new Exception("Rejection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _sut.EnqueueYearEndCutOverRejectJobAsync(request, CorrelationId));
            exception.Message.Should().Be("Rejection failed");
        }

        #endregion
    }
}
