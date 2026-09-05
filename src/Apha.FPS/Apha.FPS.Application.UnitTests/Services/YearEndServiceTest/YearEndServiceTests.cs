using Apha.Common.Contracts.Email;
using Apha.Common.Utilities.Email;
using Apha.Common.Utilities.EventPublisher;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Application.Services;
using Apha.FPS.Application.Validation;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPS.Application.UnitTests.Services.YearEndServiceTest
{
    public class YearEndServiceTests
    {
        private const string JobName = "YearEnd-DataSetup";
        private const int PlannedYear = 2025;
        private const int ContextYear = 2024;
        private const string RequestedBy = "user@example.com";
        private const string CorrelationId = "corr-001";
        private static readonly Guid JobExecutionId = Guid.NewGuid();

        private readonly IYearEndRepository _yearEndRepository;
        private readonly IFpsSettingRepository _fpsSettingRepository;
        private readonly IMonthHourRepository _monthHourRepository;
        private readonly IYearMasterRepository _yearMasterRepository;
        private readonly IYearEndStagingRepository _yearEndStagingRepository;
        private readonly IEventPublisherService _eventPublisherService;
        private readonly IGraphEmailService _emailService;
        private readonly ILogger<YearEndService> _logger;
        private readonly IMapper _mapper;
        private readonly IOptions<YearEndEmailSettings> _emailSettings;
        private readonly YearEndService _sut;

        public YearEndServiceTests()
        {
            _yearEndRepository = Substitute.For<IYearEndRepository>();
            _fpsSettingRepository = Substitute.For<IFpsSettingRepository>();
            _monthHourRepository = Substitute.For<IMonthHourRepository>();
            _yearMasterRepository = Substitute.For<IYearMasterRepository>();
            _yearEndStagingRepository = Substitute.For<IYearEndStagingRepository>();
            _eventPublisherService = Substitute.For<IEventPublisherService>();
            _emailService = Substitute.For<IGraphEmailService>();
            _logger = Substitute.For<ILogger<YearEndService>>();
            _mapper = Substitute.For<IMapper>();
            _emailSettings = Options.Create(new YearEndEmailSettings
            {
                DataSetupInitiatedEmailRecipient = "initiation@example.com",
                DataSetupInitiatedEmailSubject = "Year End Initiated",
                DataSetupInitiatedEmailBody = "Initiation body",
                DataSetupApprovalEmailRecipient = "approval@example.com",
                DataSetupApprovalEmailSubject = "Year End Approved",
                DataSetupApprovalEmailBody = "Approval body",
                DataSetupRejectionEmailRecipient = "rejection@example.com",
                DataSetupRejectionEmailSubject = "Year End Rejected",
                DataSetupRejectionEmailBody = "Rejection body",
                CutOverInitiatedEmailRecipient = "cutover-initiation@example.com",
                CutOverInitiatedEmailSubject = "CutOver Initiated",
                CutOverInitiatedEmailBody = "CutOver initiation body",
                CutOverApprovalEmailRecipient = "cutover-approval@example.com",
                CutOverApprovalEmailSubject = "CutOver Approved",
                CutOverApprovalEmailBody = "CutOver approval body",
                CutOverRejectionEmailRecipient = "cutover-rejection@example.com",
                CutOverRejectionEmailSubject = "CutOver Rejected",
                CutOverRejectionEmailBody = "CutOver rejection body"
            });

            _sut = new YearEndService(
                _yearEndRepository,
                _fpsSettingRepository,
                _monthHourRepository,
                _yearMasterRepository,
                _yearEndStagingRepository,
                _eventPublisherService,
                _emailService,
                _emailSettings,
                _logger,
                _mapper);
        }

        // -----------------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------------

        private static List<YearEndFpsSetting> ValidSettings() =>
        [
            new YearEndFpsSetting { Id = "HoursInDay", Setting = "8", ExistsForPlannedYear = "Yes" },
            new YearEndFpsSetting { Id = "CapApprovalReceivedForReset", Setting = "yes", ExistsForPlannedYear = "Yes" }
        ];

        private static List<YearEndMonthHour> ValidMonthHours() =>
            Enumerable.Range(1, 12)
                .Select(m => new YearEndMonthHour
                {
                    Month = (short)m,
                    Days = 20,
                    VidHours = 5,
                    CvlHours = 3,
                    ExistsForPlannedYear = "Yes"
                })
                .ToList();

        private void SetupValidConfiguration()
        {
            _fpsSettingRepository.GetYearEndSettingsAsync().Returns(ValidSettings());
            _monthHourRepository.GetYearEndMonthHoursAsync().Returns(ValidMonthHours());
        }

        private void SetupYearMasterNotFound() =>
            _yearMasterRepository.GetFpsYearByIdAsync(PlannedYear).Returns((YearMaster?)null);

        // -----------------------------------------------------------------------
        // Planned-year staging design helpers (Workstream 6 — Approve/Reject)
        // -----------------------------------------------------------------------

        private static YearEndRequestSummary ValidInitiatedRequest(Guid? jobQueueId = null, int? targetFpsYear = PlannedYear) =>
            new(jobQueueId ?? Guid.NewGuid(), ContextYear, targetFpsYear, "Initiated");

        private void SetupResolvableRequest(YearEndRequestSummary request) =>
            _yearEndStagingRepository.ResolveRequestAsync(JobExecutionId).Returns(request);

        private void SetupValidStagingConfiguration(YearEndRequestSummary request)
        {
            _fpsSettingRepository.GetYearEndSettingsAsync(request).Returns(ValidSettings());
            _monthHourRepository.GetYearEndMonthHoursAsync(request).Returns(ValidMonthHours());
        }

        #region GetBatchJobsHistoryAsync

        [Fact]
        public async Task GetBatchJobsHistoryAsync_WhenDataExists_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var filter = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var pagedData = new PagedData<BatchJobHistory>
            {
                Data = [new BatchJobHistory { JobId = 1, JobName = JobName, Status = "Completed" }],
                PaginationData = new PaginationData { TotalRecords = 1, PageNumber = 1, PageSize = 10 }
            };
            var expectedResult = new PaginatedResult<BatchJobHistoryDto>
            {
                Data = [new BatchJobHistoryDto { JobId = 1, JobName = JobName, Status = "Completed" }]
            };

            _mapper.Map<PaginationParameters<string>>(query).Returns(filter);
            _yearEndRepository.GetBatchJobsHistoryAsync(filter, JobName).Returns(pagedData);
            _mapper.Map<PaginatedResult<BatchJobHistoryDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.GetBatchJobsHistoryAsync(query, JobName);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().ContainSingle();
            result.Data.First().JobId.Should().Be(1);

            await _yearEndRepository.Received(1).GetBatchJobsHistoryAsync(filter, JobName);
            _mapper.Received(1).Map<PaginatedResult<BatchJobHistoryDto>>(pagedData);
        }

        [Fact]
        public async Task GetBatchJobsHistoryAsync_WhenNoData_ReturnsEmptyPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var filter = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var pagedData = new PagedData<BatchJobHistory> { Data = [], PaginationData = new PaginationData { TotalRecords = 0, PageNumber = 1, PageSize = 10 } };
            var expectedResult = new PaginatedResult<BatchJobHistoryDto> { Data = [] };

            _mapper.Map<PaginationParameters<string>>(query).Returns(filter);
            _yearEndRepository.GetBatchJobsHistoryAsync(filter, JobName).Returns(pagedData);
            _mapper.Map<PaginatedResult<BatchJobHistoryDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.GetBatchJobsHistoryAsync(query, JobName);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();

            await _yearEndRepository.Received(1).GetBatchJobsHistoryAsync(filter, JobName);
        }

        [Fact]
        public async Task GetBatchJobsHistoryAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var filter = new PaginationParameters<string>();
            _mapper.Map<PaginationParameters<string>>(query).Returns(filter);
            _yearEndRepository.GetBatchJobsHistoryAsync(filter, JobName)
                .Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sut.GetBatchJobsHistoryAsync(query, JobName));
        }

        #endregion

        #region CanInitiateYearEndDataSetupRequestAsync

        [Fact]
        public async Task CanInitiateYearEndDataSetupRequestAsync_WhenRepositoryReturnsTrue_ReturnsTrue()
        {
            // Arrange
            _yearEndRepository.CanInitiateYearEndDataSetupRequestAsync(JobName).Returns(true);

            // Act
            var result = await _sut.CanInitiateYearEndDataSetupRequestAsync(JobName);

            // Assert
            result.Should().BeTrue();
            await _yearEndRepository.Received(1).CanInitiateYearEndDataSetupRequestAsync(JobName);
        }

        [Fact]
        public async Task CanInitiateYearEndDataSetupRequestAsync_WhenRepositoryReturnsFalse_ReturnsFalse()
        {
            // Arrange
            _yearEndRepository.CanInitiateYearEndDataSetupRequestAsync(JobName).Returns(false);

            // Act
            var result = await _sut.CanInitiateYearEndDataSetupRequestAsync(JobName);

            // Assert
            result.Should().BeFalse();
            await _yearEndRepository.Received(1).CanInitiateYearEndDataSetupRequestAsync(JobName);
        }

        [Fact]
        public async Task CanInitiateYearEndDataSetupRequestAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            _yearEndRepository.CanInitiateYearEndDataSetupRequestAsync(JobName)
                .Throws(new Exception("Repository error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sut.CanInitiateYearEndDataSetupRequestAsync(JobName));
        }

        #endregion

        #region CanApproveOrRejectYearEndDataSetupRequestAsync

        [Fact]
        public async Task CanApproveOrRejectYearEndDataSetupRequestAsync_WhenRepositoryReturnsTrue_ReturnsTrue()
        {
            // Arrange
            _yearEndRepository.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Returns(true);

            // Act
            var result = await _sut.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName);

            // Assert
            result.Should().BeTrue();
            await _yearEndRepository.Received(1).CanApproveOrRejectYearEndDataSetupRequestAsync(JobName);
        }

        [Fact]
        public async Task CanApproveOrRejectYearEndDataSetupRequestAsync_WhenRepositoryReturnsFalse_ReturnsFalse()
        {
            // Arrange
            _yearEndRepository.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Returns(false);

            // Act
            var result = await _sut.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName);

            // Assert
            result.Should().BeFalse();
            await _yearEndRepository.Received(1).CanApproveOrRejectYearEndDataSetupRequestAsync(JobName);
        }

        [Fact]
        public async Task CanApproveOrRejectYearEndDataSetupRequestAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            _yearEndRepository.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName)
                .Throws(new Exception("Repository error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sut.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName));
        }

        #endregion

        #region EnqueueYearEndDataSetupInitiationJobAsync — validation

        [Theory]
        [InlineData(0)]
        [InlineData(1899)]
        [InlineData(10000)]
        public async Task EnqueueYearEndDataSetupInitiationJobAsync_WhenPlannedYearIsInvalid_ThrowsBusinessValidationError(int invalidYear)
        {
            // Arrange
            SetupValidConfiguration();
            _yearMasterRepository.GetFpsYearByIdAsync(invalidYear).Returns((YearMaster?)null);
            _yearEndRepository.CanInitiateYearEndDataSetupRequestAsync(JobName).Returns(true);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndDataSetupInitiationJobAsync(invalidYear, ContextYear, RequestedBy, CorrelationId));

            ex.Errors.Should().ContainSingle(e => e.Code == "INVALID_PlannedYear");
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupInitiationJobAsync_WhenRequestedByIsEmpty_ThrowsBusinessValidationError()
        {
            // Arrange
            SetupValidConfiguration();
            _yearMasterRepository.GetFpsYearByIdAsync(PlannedYear).Returns((YearMaster?)null);
            _yearEndRepository.CanInitiateYearEndDataSetupRequestAsync(JobName).Returns(true);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndDataSetupInitiationJobAsync(PlannedYear, ContextYear, string.Empty, CorrelationId));

            ex.Errors.Should().ContainSingle(e => e.Code == "INVALID_User");
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupInitiationJobAsync_WhenPlannedYearAlreadyCompleted_ThrowsBusinessValidationError()
        {
            // Arrange
            SetupValidConfiguration();
            _yearMasterRepository.GetFpsYearByIdAsync(PlannedYear)
                .Returns(new YearMaster { FpsYear = PlannedYear, YearStatus = "planned", Active = true });
            _yearEndRepository.CanInitiateYearEndDataSetupRequestAsync(JobName).Returns(true);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndDataSetupInitiationJobAsync(PlannedYear, ContextYear, RequestedBy, CorrelationId));

            ex.Errors.Should().ContainSingle(e => e.Code == "INVALID_Rerun");
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupInitiationJobAsync_WhenJobIsAlreadyRunning_ThrowsBusinessValidationError()
        {
            // Arrange
            SetupValidConfiguration();
            _yearMasterRepository.GetFpsYearByIdAsync(PlannedYear).Returns((YearMaster?)null);
            _yearEndRepository.CanInitiateYearEndDataSetupRequestAsync(JobName).Returns(false);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndDataSetupInitiationJobAsync(PlannedYear, ContextYear, RequestedBy, CorrelationId));

            ex.Errors.Should().ContainSingle(e => e.Code == "INVALID_Initiation");
        }

        // The five tests that used to live here (missing settings / invalid HoursInDay / invalid
        // CapApprovalReceivedForReset / missing month config / negative month hours, all asserting
        // "Missing_Config"-family errors) were removed 2026-09-03: Initiate no longer calls
        // ValidateConfiguration at all (planned-year staging design) — that check moves to Approve,
        // which still validates unchanged (see the Approval region below). Keeping them here would
        // assert behaviour Initiate genuinely no longer has.

        [Fact]
        public async Task EnqueueYearEndDataSetupInitiationJobAsync_DoesNotValidateConfiguration()
        {
            // Deliberately guards the planned-year staging design's ordering change against being
            // accidentally "restored" later: Initiate must succeed and must never even read config,
            // regardless of whether it's valid, missing, or invalid — that check belongs to Approve
            // only now. See the paired
            // EnqueueYearEndDataSetupApprovalJobAsync_StillValidatesConfiguration test below, which
            // proves Approve is unchanged.

            // Arrange — deliberately NOT calling SetupValidConfiguration(); config mocks are left
            // unconfigured (NSubstitute default: empty list) so a real read would very likely also
            // fail Initiate's old Missing_Config check if it still happened.
            SetupYearMasterNotFound();
            _yearEndRepository.CanInitiateYearEndDataSetupRequestAsync(JobName).Returns(true);

            var queueEntry = new BatchJobQueue { JobqueueId = Guid.NewGuid(), RequestedBy = RequestedBy };
            _yearEndRepository.EnqueueDataSetupInitiationBatchJobAsync(JobName, RequestedBy, CorrelationId, Arg.Any<string>(), PlannedYear)
                .Returns(queueEntry);
            _mapper.Map<BatchJobQueueDto>(queueEntry).Returns(new BatchJobQueueDto());

            // Act
            var result = await _sut.EnqueueYearEndDataSetupInitiationJobAsync(PlannedYear, ContextYear, RequestedBy, CorrelationId);

            // Assert — succeeds despite "empty" config, and never even looked at it.
            result.Should().NotBeNull();
            await _fpsSettingRepository.DidNotReceive().GetYearEndSettingsAsync();
            await _monthHourRepository.DidNotReceive().GetYearEndMonthHoursAsync();
        }

        #endregion

        #region EnqueueYearEndDataSetupInitiationJobAsync — success

        [Fact]
        public async Task EnqueueYearEndDataSetupInitiationJobAsync_WhenAllValid_ReturnsQueueDto()
        {
            // Arrange
            SetupValidConfiguration();
            SetupYearMasterNotFound();
            _yearEndRepository.CanInitiateYearEndDataSetupRequestAsync(JobName).Returns(true);

            var queueEntry = new BatchJobQueue { JobqueueId = Guid.NewGuid(), RequestedBy = RequestedBy };
            _yearEndRepository.EnqueueDataSetupInitiationBatchJobAsync(JobName, RequestedBy, CorrelationId, Arg.Any<string>(), PlannedYear)
                .Returns(queueEntry);

            var expectedDto = new BatchJobQueueDto { RequestedBy = RequestedBy };
            _mapper.Map<BatchJobQueueDto>(queueEntry).Returns(expectedDto);

            // Act
            var result = await _sut.EnqueueYearEndDataSetupInitiationJobAsync(PlannedYear, ContextYear, RequestedBy, CorrelationId);

            // Assert
            result.Should().NotBeNull();
            result.RequestedBy.Should().Be(RequestedBy);

            // Confirms plannedYear is what actually reaches the repository as targetFpsYear —
            // not just any int (planned-year staging design, 2026-09-03).
            await _yearEndRepository.Received(1).EnqueueDataSetupInitiationBatchJobAsync(
                JobName, RequestedBy, CorrelationId, Arg.Any<string>(), PlannedYear);
            await _emailService.Received(1).SendEmailAsync(Arg.Any<EmailMessageModel>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupInitiationJobAsync_WhenEmailFails_StillReturnsQueueDto()
        {
            // Arrange — email failure must be swallowed, not propagated
            SetupValidConfiguration();
            SetupYearMasterNotFound();
            _yearEndRepository.CanInitiateYearEndDataSetupRequestAsync(JobName).Returns(true);

            var queueEntry = new BatchJobQueue { JobqueueId = Guid.NewGuid(), RequestedBy = RequestedBy };
            _yearEndRepository.EnqueueDataSetupInitiationBatchJobAsync(JobName, RequestedBy, CorrelationId, Arg.Any<string>(), PlannedYear)
                .Returns(queueEntry);

            var expectedDto = new BatchJobQueueDto { RequestedBy = RequestedBy };
            _mapper.Map<BatchJobQueueDto>(queueEntry).Returns(expectedDto);

            _emailService.SendEmailAsync(Arg.Any<EmailMessageModel>(), Arg.Any<CancellationToken>())
                .Throws(new Exception("SMTP failure"));

            // Act
            var result = await _sut.EnqueueYearEndDataSetupInitiationJobAsync(PlannedYear, ContextYear, RequestedBy, CorrelationId);

            // Assert — no exception propagated; dto still returned
            result.Should().NotBeNull();
            result.RequestedBy.Should().Be(RequestedBy);
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupInitiationJobAsync_WhenAllValid_SendsInitiationEmail()
        {
            // Arrange
            SetupValidConfiguration();
            SetupYearMasterNotFound();
            _yearEndRepository.CanInitiateYearEndDataSetupRequestAsync(JobName).Returns(true);

            var queueEntry = new BatchJobQueue { JobqueueId = Guid.NewGuid(), RequestedBy = RequestedBy };
            _yearEndRepository.EnqueueDataSetupInitiationBatchJobAsync(JobName, RequestedBy, CorrelationId, Arg.Any<string>(), PlannedYear)
                .Returns(queueEntry);
            _mapper.Map<BatchJobQueueDto>(queueEntry).Returns(new BatchJobQueueDto());

            // Act
            await _sut.EnqueueYearEndDataSetupInitiationJobAsync(PlannedYear, ContextYear, RequestedBy, CorrelationId);

            // Assert — email sent with recipient from settings
            await _emailService.Received(1).SendEmailAsync(
                Arg.Is<EmailMessageModel>(m => m.To.Contains("initiation@example.com")),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupInitiationJobAsync_WhenAllValid_CallsEnqueueOnRepository()
        {
            // Arrange
            SetupValidConfiguration();
            SetupYearMasterNotFound();
            _yearEndRepository.CanInitiateYearEndDataSetupRequestAsync(JobName).Returns(true);

            var queueEntry = new BatchJobQueue { JobqueueId = Guid.NewGuid() };
            _yearEndRepository.EnqueueDataSetupInitiationBatchJobAsync(JobName, RequestedBy, CorrelationId, Arg.Any<string>(), PlannedYear)
                .Returns(queueEntry);
            _mapper.Map<BatchJobQueueDto>(queueEntry).Returns(new BatchJobQueueDto());

            // Act
            await _sut.EnqueueYearEndDataSetupInitiationJobAsync(PlannedYear, ContextYear, RequestedBy, CorrelationId);

            // Assert
            await _yearEndRepository.Received(1).EnqueueDataSetupInitiationBatchJobAsync(
                JobName, RequestedBy, CorrelationId, Arg.Any<string>(), PlannedYear);
        }

        #endregion

        #region EnqueueYearEndDataSetupApprovalJobAsync — validation

        [Fact]
        public async Task EnqueueYearEndDataSetupApprovalJobAsync_WhenRequestCannotBeResolved_ThrowsKeyNotFoundException()
        {
            // Arrange — no ResolveRequestAsync setup, so it resolves to null: matches Confirm's
            // Workstream 5 convention (a wiring failure, not a business validation error).

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _sut.EnqueueYearEndDataSetupApprovalJobAsync(JobExecutionId, PlannedYear, ContextYear, RequestedBy, CorrelationId));
        }

        [Theory]
        [InlineData("Approved")]
        [InlineData("Running")]
        [InlineData("Completed")]
        [InlineData("Failed")]
        [InlineData("Rejected")]
        public async Task EnqueueYearEndDataSetupApprovalJobAsync_WhenRequestNotInitiated_ThrowsBusinessValidationError(string status)
        {
            // Arrange
            var request = ValidInitiatedRequest() with { Status = status };
            SetupResolvableRequest(request);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndDataSetupApprovalJobAsync(JobExecutionId, PlannedYear, ContextYear, RequestedBy, CorrelationId));

            ex.Errors.Should().ContainSingle(e => e.Code == "REQUEST_NOT_EDITABLE");
            await _yearEndRepository.DidNotReceive().EnqueueDataSetupApprovalBatchJobAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupApprovalJobAsync_WhenTargetFpsYearIsMissing_ThrowsBusinessValidationError()
        {
            // Arrange — a resolved request without a target year can't be materialized; must fail with
            // a clear, named error rather than a bare nullable-.Value exception deep in a helper.
            var request = ValidInitiatedRequest(targetFpsYear: null);
            SetupResolvableRequest(request);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndDataSetupApprovalJobAsync(JobExecutionId, PlannedYear, ContextYear, RequestedBy, CorrelationId));

            ex.Errors.Should().ContainSingle(e => e.Code == "TARGET_FPSYEAR_MISSING");
            await _yearEndRepository.DidNotReceive().EnqueueDataSetupApprovalBatchJobAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupApprovalJobAsync_WhenPlannedYearDoesNotMatchTargetFpsYear_ThrowsBusinessValidationError()
        {
            // Arrange — caller-supplied plannedYear disagrees with the resolved request's persisted
            // target year: stale UI state. Approval must stop here, before building any trigger — a
            // mismatch and a published event are mutually exclusive outcomes.
            var request = ValidInitiatedRequest(targetFpsYear: PlannedYear);
            SetupResolvableRequest(request);
            var mismatchedPlannedYear = PlannedYear + 1;

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndDataSetupApprovalJobAsync(JobExecutionId, mismatchedPlannedYear, ContextYear, RequestedBy, CorrelationId));

            ex.Errors.Should().ContainSingle(e => e.Code == "PLANNEDYEAR_MISMATCH");
            await _yearEndRepository.DidNotReceive().EnqueueDataSetupApprovalBatchJobAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>());
            await _eventPublisherService.DidNotReceive().PublishAsync(Arg.Any<EventDetail>(), Arg.Any<CancellationToken>());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1899)]
        [InlineData(10000)]
        public async Task EnqueueYearEndDataSetupApprovalJobAsync_WhenPlannedYearIsInvalid_ThrowsBusinessValidationError(int invalidYear)
        {
            // Arrange — request's target year matches invalidYear so PLANNEDYEAR_MISMATCH doesn't fire
            // first, letting the range check underneath it actually run.
            var request = ValidInitiatedRequest(targetFpsYear: invalidYear);
            SetupResolvableRequest(request);
            SetupValidStagingConfiguration(request);
            _yearMasterRepository.GetFpsYearByIdAsync(invalidYear).Returns((YearMaster?)null);
            _yearEndRepository.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Returns(true);
            _yearEndRepository.GetYearEndDataSetupRequestInitiatorAsync(JobName).Returns(string.Empty);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndDataSetupApprovalJobAsync(JobExecutionId, invalidYear, ContextYear, RequestedBy, CorrelationId));

            ex.Errors.Should().ContainSingle(e => e.Code == "INVALID_PlannedYear");
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupApprovalJobAsync_WhenRequestedByIsEmpty_ThrowsBusinessValidationError()
        {
            // Arrange
            var request = ValidInitiatedRequest();
            SetupResolvableRequest(request);
            SetupValidStagingConfiguration(request);
            _yearMasterRepository.GetFpsYearByIdAsync(PlannedYear).Returns((YearMaster?)null);
            _yearEndRepository.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Returns(true);
            _yearEndRepository.GetYearEndDataSetupRequestInitiatorAsync(JobName).Returns(string.Empty);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndDataSetupApprovalJobAsync(JobExecutionId, PlannedYear, ContextYear, string.Empty, CorrelationId));

            ex.Errors.Should().ContainSingle(e => e.Code == "INVALID_User");
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupApprovalJobAsync_WhenPlannedYearAlreadyCompleted_ThrowsBusinessValidationError()
        {
            // Arrange
            var request = ValidInitiatedRequest();
            SetupResolvableRequest(request);
            SetupValidStagingConfiguration(request);
            _yearMasterRepository.GetFpsYearByIdAsync(PlannedYear)
                .Returns(new YearMaster { FpsYear = PlannedYear, YearStatus = "close", Active = true });
            _yearEndRepository.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Returns(true);
            _yearEndRepository.GetYearEndDataSetupRequestInitiatorAsync(JobName).Returns(string.Empty);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndDataSetupApprovalJobAsync(JobExecutionId, PlannedYear, ContextYear, RequestedBy, CorrelationId));

            ex.Errors.Should().ContainSingle(e => e.Code == "INVALID_Rerun");
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupApprovalJobAsync_WhenNoInitiatedRequestExists_ThrowsBusinessValidationError()
        {
            // Arrange
            var request = ValidInitiatedRequest();
            SetupResolvableRequest(request);
            SetupValidStagingConfiguration(request);
            SetupYearMasterNotFound();
            _yearEndRepository.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Returns(false);
            _yearEndRepository.GetYearEndDataSetupRequestInitiatorAsync(JobName).Returns(string.Empty);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndDataSetupApprovalJobAsync(JobExecutionId, PlannedYear, ContextYear, RequestedBy, CorrelationId));

            ex.Errors.Should().ContainSingle(e => e.Code == "INVALID_Approval");
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupApprovalJobAsync_WhenInitiatorAndApproverAreSamePerson_ThrowsBusinessValidationError()
        {
            // Arrange
            var request = ValidInitiatedRequest();
            SetupResolvableRequest(request);
            SetupValidStagingConfiguration(request);
            SetupYearMasterNotFound();
            _yearEndRepository.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Returns(true);
            _yearEndRepository.GetYearEndDataSetupRequestInitiatorAsync(JobName).Returns(RequestedBy);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndDataSetupApprovalJobAsync(JobExecutionId, PlannedYear, ContextYear, RequestedBy, CorrelationId));

            ex.Errors.Should().ContainSingle(e => e.Code == "INVALID_Approval");
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupApprovalJobAsync_WhenSettingsAreMissingForPlannedYear_ThrowsBusinessValidationError()
        {
            // Arrange — staging overlay overload (request-scoped), not the real-table parameterless one.
            var request = ValidInitiatedRequest();
            SetupResolvableRequest(request);
            var missingSettings = new List<YearEndFpsSetting>
            {
                new YearEndFpsSetting { Id = "HoursInDay", Setting = "8", ExistsForPlannedYear = "No" }
            };
            _fpsSettingRepository.GetYearEndSettingsAsync(request).Returns(missingSettings);
            _monthHourRepository.GetYearEndMonthHoursAsync(request).Returns(ValidMonthHours());
            SetupYearMasterNotFound();
            _yearEndRepository.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Returns(true);
            _yearEndRepository.GetYearEndDataSetupRequestInitiatorAsync(JobName).Returns(string.Empty);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndDataSetupApprovalJobAsync(JobExecutionId, PlannedYear, ContextYear, RequestedBy, CorrelationId));

            ex.Errors.Should().ContainSingle(e => e.Code == "Missing_Config");
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupApprovalJobAsync_WhenRepositoryFindsRequestNoLongerInitiated_TranslatesToBusinessValidationError()
        {
            // Arrange — resolved as Initiated here, but the repository's own in-transaction re-check
            // finds it no longer is (narrow race — e.g. another Approve/Reject won in the meantime).
            // Same caller-visible error as the earlier, more common check.
            var request = ValidInitiatedRequest();
            SetupResolvableRequest(request);
            SetupValidStagingConfiguration(request);
            SetupYearMasterNotFound();
            _yearEndRepository.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Returns(true);
            _yearEndRepository.GetYearEndDataSetupRequestInitiatorAsync(JobName).Returns(string.Empty);
            _yearEndRepository.EnqueueDataSetupApprovalBatchJobAsync(request.JobQueueId, RequestedBy, Arg.Any<string>())
                .Throws(new KeyNotFoundException("No initiated Data Setup request was found."));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndDataSetupApprovalJobAsync(JobExecutionId, PlannedYear, ContextYear, RequestedBy, CorrelationId));

            ex.Errors.Should().ContainSingle(e => e.Code == "REQUEST_NOT_EDITABLE");
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupApprovalJobAsync_StillValidatesConfiguration()
        {
            // Paired with EnqueueYearEndDataSetupInitiationJobAsync_DoesNotValidateConfiguration
            // above — proves the other half of the planned-year staging design's ordering change:
            // Approve is unchanged and still reads/validates config every time, unlike Initiate. Now
            // reads the staging overlay (request-scoped), not the parameterless real-table overload.

            // Arrange
            var request = ValidInitiatedRequest();
            SetupResolvableRequest(request);
            SetupValidStagingConfiguration(request);
            SetupYearMasterNotFound();
            _yearEndRepository.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Returns(true);
            _yearEndRepository.GetYearEndDataSetupRequestInitiatorAsync(JobName).Returns(string.Empty);

            var queueEntry = new BatchJobQueue { JobqueueId = request.JobQueueId, JobExecutionId = Guid.NewGuid() };
            _yearEndRepository.EnqueueDataSetupApprovalBatchJobAsync(request.JobQueueId, RequestedBy, Arg.Any<string>())
                .Returns(queueEntry);
            _mapper.Map<BatchJobEventTriggerDto>(queueEntry).Returns(new BatchJobEventTriggerDto());

            // Act
            await _sut.EnqueueYearEndDataSetupApprovalJobAsync(JobExecutionId, PlannedYear, ContextYear, RequestedBy, CorrelationId);

            // Assert
            await _fpsSettingRepository.Received(1).GetYearEndSettingsAsync(request);
            await _monthHourRepository.Received(1).GetYearEndMonthHoursAsync(request);
        }

        #endregion

        #region EnqueueYearEndDataSetupApprovalJobAsync — success

        [Fact]
        public async Task EnqueueYearEndDataSetupApprovalJobAsync_WhenAllValid_ReturnsEventTriggerDto()
        {
            // Arrange
            var request = ValidInitiatedRequest();
            SetupResolvableRequest(request);
            SetupValidStagingConfiguration(request);
            SetupYearMasterNotFound();
            _yearEndRepository.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Returns(true);
            _yearEndRepository.GetYearEndDataSetupRequestInitiatorAsync(JobName).Returns("other@example.com");

            var queueEntry = new BatchJobQueue { JobqueueId = request.JobQueueId, RequestedBy = RequestedBy };
            _yearEndRepository.EnqueueDataSetupApprovalBatchJobAsync(request.JobQueueId, RequestedBy, Arg.Any<string>())
                .Returns(queueEntry);

            _eventPublisherService.PublishAsync(Arg.Any<EventDetail>(), Arg.Any<CancellationToken>())
                .Returns("evt-123");

            var triggerDto = new BatchJobEventTriggerDto { EventId = "evt-123" };
            _mapper.Map<BatchJobEventTriggerDto>(queueEntry).Returns(triggerDto);
            _mapper.Map<BatchJobEventTriggerDto>(triggerDto).Returns(triggerDto);

            // Act
            var result = await _sut.EnqueueYearEndDataSetupApprovalJobAsync(JobExecutionId, PlannedYear, ContextYear, RequestedBy, CorrelationId);

            // Assert
            result.Should().NotBeNull();
            result.EventId.Should().Be("evt-123");

            await _yearEndRepository.Received(1).EnqueueDataSetupApprovalBatchJobAsync(
                request.JobQueueId, RequestedBy, Arg.Any<string>());
            await _eventPublisherService.Received(1).PublishAsync(Arg.Any<EventDetail>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupApprovalJobAsync_WhenAllValid_SendsApprovalEmail()
        {
            // Arrange
            var request = ValidInitiatedRequest();
            SetupResolvableRequest(request);
            SetupValidStagingConfiguration(request);
            SetupYearMasterNotFound();
            _yearEndRepository.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Returns(true);
            _yearEndRepository.GetYearEndDataSetupRequestInitiatorAsync(JobName).Returns("other@example.com");

            var queueEntry = new BatchJobQueue { JobqueueId = request.JobQueueId };
            _yearEndRepository.EnqueueDataSetupApprovalBatchJobAsync(request.JobQueueId, RequestedBy, Arg.Any<string>())
                .Returns(queueEntry);
            _eventPublisherService.PublishAsync(Arg.Any<EventDetail>(), Arg.Any<CancellationToken>())
                .Returns("evt-456");

            var triggerDto = new BatchJobEventTriggerDto();
            _mapper.Map<BatchJobEventTriggerDto>(queueEntry).Returns(triggerDto);
            _mapper.Map<BatchJobEventTriggerDto>(triggerDto).Returns(triggerDto);

            // Act
            await _sut.EnqueueYearEndDataSetupApprovalJobAsync(JobExecutionId, PlannedYear, ContextYear, RequestedBy, CorrelationId);

            // Assert — email sent to approval recipient from settings
            await _emailService.Received(1).SendEmailAsync(
                Arg.Is<EmailMessageModel>(m => m.To.Contains("approval@example.com")),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupApprovalJobAsync_WhenEmailFails_StillReturnsEventTriggerDto()
        {
            // Arrange — email failure must be swallowed, not propagated
            var request = ValidInitiatedRequest();
            SetupResolvableRequest(request);
            SetupValidStagingConfiguration(request);
            SetupYearMasterNotFound();
            _yearEndRepository.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Returns(true);
            _yearEndRepository.GetYearEndDataSetupRequestInitiatorAsync(JobName).Returns("other@example.com");

            var queueEntry = new BatchJobQueue { JobqueueId = request.JobQueueId };
            _yearEndRepository.EnqueueDataSetupApprovalBatchJobAsync(request.JobQueueId, RequestedBy, Arg.Any<string>())
                .Returns(queueEntry);
            _eventPublisherService.PublishAsync(Arg.Any<EventDetail>(), Arg.Any<CancellationToken>())
                .Returns("evt-789");

            var triggerDto = new BatchJobEventTriggerDto { EventId = "evt-789" };
            _mapper.Map<BatchJobEventTriggerDto>(queueEntry).Returns(triggerDto);
            _mapper.Map<BatchJobEventTriggerDto>(triggerDto).Returns(triggerDto);

            _emailService.SendEmailAsync(Arg.Any<EmailMessageModel>(), Arg.Any<CancellationToken>())
                .Throws(new Exception("SMTP failure"));

            // Act
            var result = await _sut.EnqueueYearEndDataSetupApprovalJobAsync(JobExecutionId, PlannedYear, ContextYear, RequestedBy, CorrelationId);

            // Assert — no exception propagated; dto still returned
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupApprovalJobAsync_WhenAllValid_PublishesEventWithCorrectJobName()
        {
            // Arrange
            var request = ValidInitiatedRequest();
            SetupResolvableRequest(request);
            SetupValidStagingConfiguration(request);
            SetupYearMasterNotFound();
            _yearEndRepository.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Returns(true);
            _yearEndRepository.GetYearEndDataSetupRequestInitiatorAsync(JobName).Returns("other@example.com");

            var queueEntry = new BatchJobQueue { JobqueueId = request.JobQueueId };
            _yearEndRepository.EnqueueDataSetupApprovalBatchJobAsync(request.JobQueueId, RequestedBy, Arg.Any<string>())
                .Returns(queueEntry);
            _eventPublisherService.PublishAsync(Arg.Any<EventDetail>(), Arg.Any<CancellationToken>())
                .Returns("evt-999");

            var triggerDto = new BatchJobEventTriggerDto();
            _mapper.Map<BatchJobEventTriggerDto>(queueEntry).Returns(triggerDto);
            _mapper.Map<BatchJobEventTriggerDto>(triggerDto).Returns(triggerDto);

            // Act
            await _sut.EnqueueYearEndDataSetupApprovalJobAsync(JobExecutionId, PlannedYear, ContextYear, RequestedBy, CorrelationId);

            // Assert — event published with the correct job name
            await _eventPublisherService.Received(1).PublishAsync(
                Arg.Is<EventDetail>(e => e.JobName == JobName && e.RequestedBy == RequestedBy),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupApprovalJobAsync_WhenAllValid_PublishesEventWithRowsJobExecutionId_NotCorrelationId()
        {
            // Arrange — JobExecutionId is deliberately different from CorrelationId here: Initiate
            // and Approve are separate HTTP requests that never actually share a correlation id in
            // practice, so the event must carry the job_queue row's own persisted JobExecutionId,
            // not the raw X-Correlation-ID header of this Approval call.
            var request = ValidInitiatedRequest();
            SetupResolvableRequest(request);
            SetupValidStagingConfiguration(request);
            SetupYearMasterNotFound();
            _yearEndRepository.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Returns(true);
            _yearEndRepository.GetYearEndDataSetupRequestInitiatorAsync(JobName).Returns("other@example.com");

            var rowJobExecutionId = Guid.NewGuid();
            var queueEntry = new BatchJobQueue { JobqueueId = request.JobQueueId, JobExecutionId = rowJobExecutionId, RequestedBy = RequestedBy };
            _yearEndRepository.EnqueueDataSetupApprovalBatchJobAsync(request.JobQueueId, RequestedBy, Arg.Any<string>())
                .Returns(queueEntry);
            _eventPublisherService.PublishAsync(Arg.Any<EventDetail>(), Arg.Any<CancellationToken>())
                .Returns("evt-jobexec");

            var triggerDto = new BatchJobEventTriggerDto();
            _mapper.Map<BatchJobEventTriggerDto>(queueEntry).Returns(triggerDto);
            _mapper.Map<BatchJobEventTriggerDto>(triggerDto).Returns(triggerDto);

            // Act
            await _sut.EnqueueYearEndDataSetupApprovalJobAsync(JobExecutionId, PlannedYear, ContextYear, RequestedBy, CorrelationId);

            // Assert
            await _eventPublisherService.Received(1).PublishAsync(
                Arg.Is<EventDetail>(e => e.JobExecutionId == rowJobExecutionId.ToString() && e.JobExecutionId != CorrelationId),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupApprovalJobAsync_WhenAllValid_PublishesEventWithPersistedTargetFpsYear()
        {
            // Arrange — the trigger must be built from the resolved request's persisted TargetFpsYear
            // (already verified equal to the caller's plannedYear by the mismatch check), never rebuilt
            // from ambient request context.
            var request = ValidInitiatedRequest(targetFpsYear: PlannedYear);
            SetupResolvableRequest(request);
            SetupValidStagingConfiguration(request);
            SetupYearMasterNotFound();
            _yearEndRepository.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Returns(true);
            _yearEndRepository.GetYearEndDataSetupRequestInitiatorAsync(JobName).Returns("other@example.com");

            var queueEntry = new BatchJobQueue { JobqueueId = request.JobQueueId, JobExecutionId = Guid.NewGuid(), RequestedBy = RequestedBy };
            _yearEndRepository.EnqueueDataSetupApprovalBatchJobAsync(request.JobQueueId, RequestedBy, Arg.Any<string>())
                .Returns(queueEntry);
            _eventPublisherService.PublishAsync(Arg.Any<EventDetail>(), Arg.Any<CancellationToken>())
                .Returns("evt-target-year");

            var triggerDto = new BatchJobEventTriggerDto();
            _mapper.Map<BatchJobEventTriggerDto>(queueEntry).Returns(triggerDto);
            _mapper.Map<BatchJobEventTriggerDto>(triggerDto).Returns(triggerDto);

            // Act
            await _sut.EnqueueYearEndDataSetupApprovalJobAsync(JobExecutionId, PlannedYear, ContextYear, RequestedBy, CorrelationId);

            // Assert
            await _eventPublisherService.Received(1).PublishAsync(
                Arg.Is<EventDetail>(e => e.ParametersJson != null && e.ParametersJson.Contains($"{PlannedYear:D4}")),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupApprovalJobAsync_WhenPublishSucceeds_CallsSetTriggeredMetadataWithRowsJobExecutionId()
        {
            // Arrange
            var request = ValidInitiatedRequest();
            SetupResolvableRequest(request);
            SetupValidStagingConfiguration(request);
            SetupYearMasterNotFound();
            _yearEndRepository.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Returns(true);
            _yearEndRepository.GetYearEndDataSetupRequestInitiatorAsync(JobName).Returns("other@example.com");

            var rowJobExecutionId = Guid.NewGuid();
            var queueEntry = new BatchJobQueue { JobqueueId = request.JobQueueId, JobExecutionId = rowJobExecutionId, RequestedBy = RequestedBy };
            _yearEndRepository.EnqueueDataSetupApprovalBatchJobAsync(request.JobQueueId, RequestedBy, Arg.Any<string>())
                .Returns(queueEntry);
            _eventPublisherService.PublishAsync(Arg.Any<EventDetail>(), Arg.Any<CancellationToken>())
                .Returns("evt-trigger");

            var triggerDto = new BatchJobEventTriggerDto();
            _mapper.Map<BatchJobEventTriggerDto>(queueEntry).Returns(triggerDto);
            _mapper.Map<BatchJobEventTriggerDto>(triggerDto).Returns(triggerDto);

            // Act
            await _sut.EnqueueYearEndDataSetupApprovalJobAsync(JobExecutionId, PlannedYear, ContextYear, RequestedBy, CorrelationId);

            // Assert
            await _yearEndRepository.Received(1).SetTriggeredMetadataAsync(rowJobExecutionId.ToString(), RequestedBy);
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupApprovalJobAsync_WhenPublishFails_DoesNotSetTriggeredMetadataAndPropagatesException()
        {
            // Arrange — a failed publish must leave the row Approved with triggered_at_utc still
            // NULL, so SetTriggeredMetadataAsync must never be called on this path.
            var request = ValidInitiatedRequest();
            SetupResolvableRequest(request);
            SetupValidStagingConfiguration(request);
            SetupYearMasterNotFound();
            _yearEndRepository.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Returns(true);
            _yearEndRepository.GetYearEndDataSetupRequestInitiatorAsync(JobName).Returns("other@example.com");

            var queueEntry = new BatchJobQueue { JobqueueId = request.JobQueueId, JobExecutionId = Guid.NewGuid(), RequestedBy = RequestedBy };
            _yearEndRepository.EnqueueDataSetupApprovalBatchJobAsync(request.JobQueueId, RequestedBy, Arg.Any<string>())
                .Returns(queueEntry);
            _eventPublisherService.PublishAsync(Arg.Any<EventDetail>(), Arg.Any<CancellationToken>())
                .Throws(new InvalidOperationException("EventBridge unavailable"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.EnqueueYearEndDataSetupApprovalJobAsync(JobExecutionId, PlannedYear, ContextYear, RequestedBy, CorrelationId));

            await _yearEndRepository.DidNotReceive().SetTriggeredMetadataAsync(Arg.Any<string>(), Arg.Any<string>());
        }

        #endregion

        #region EnqueueYearEndDataSetupRejectJobAsync — validation

        [Fact]
        public async Task EnqueueYearEndDataSetupRejectJobAsync_WhenRequestCannotBeResolved_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _sut.EnqueueYearEndDataSetupRejectJobAsync(JobExecutionId, PlannedYear, ContextYear, RequestedBy, CorrelationId));
        }

        [Theory]
        [InlineData("Approved")]
        [InlineData("Running")]
        [InlineData("Completed")]
        [InlineData("Failed")]
        [InlineData("Rejected")]
        public async Task EnqueueYearEndDataSetupRejectJobAsync_WhenRequestNotInitiated_ThrowsBusinessValidationError(string status)
        {
            // Arrange
            var request = ValidInitiatedRequest() with { Status = status };
            SetupResolvableRequest(request);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndDataSetupRejectJobAsync(JobExecutionId, PlannedYear, ContextYear, RequestedBy, CorrelationId));

            ex.Errors.Should().ContainSingle(e => e.Code == "REQUEST_NOT_EDITABLE");
            await _yearEndRepository.DidNotReceive().EnqueueDataSetupRejectBatchJobAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupRejectJobAsync_WhenTargetFpsYearIsMissing_StillProceeds()
        {
            // Arrange — unlike Approve, Reject must remain usable to clean up a malformed/legacy
            // request even without a target year; the asymmetry is deliberate (Reject builds no
            // trigger, and exact JobExecutionId resolution already identifies the target unambiguously).
            var request = ValidInitiatedRequest(targetFpsYear: null);
            SetupResolvableRequest(request);
            _yearEndRepository.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Returns(true);
            _yearEndRepository.GetYearEndDataSetupRequestInitiatorAsync(JobName).Returns("other@example.com");

            var queueEntry = new BatchJobQueue { JobqueueId = request.JobQueueId, RequestedBy = RequestedBy };
            _yearEndRepository.EnqueueDataSetupRejectBatchJobAsync(request.JobQueueId, RequestedBy, Arg.Any<string>())
                .Returns(queueEntry);
            _mapper.Map<BatchJobEventTriggerDto>(queueEntry).Returns(new BatchJobEventTriggerDto());

            // Act
            var result = await _sut.EnqueueYearEndDataSetupRejectJobAsync(JobExecutionId, PlannedYear, ContextYear, RequestedBy, CorrelationId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupRejectJobAsync_WhenRequestedByIsEmpty_ThrowsBusinessValidationError()
        {
            // Arrange
            var request = ValidInitiatedRequest();
            SetupResolvableRequest(request);
            _yearEndRepository.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Returns(true);
            _yearEndRepository.GetYearEndDataSetupRequestInitiatorAsync(JobName).Returns(string.Empty);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndDataSetupRejectJobAsync(JobExecutionId, PlannedYear, ContextYear, string.Empty, CorrelationId));

            ex.Errors.Should().ContainSingle(e => e.Code == "INVALID_User");
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupRejectJobAsync_WhenNoInitiatedRequestExists_ThrowsBusinessValidationError()
        {
            // Arrange
            var request = ValidInitiatedRequest();
            SetupResolvableRequest(request);
            _yearEndRepository.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Returns(false);
            _yearEndRepository.GetYearEndDataSetupRequestInitiatorAsync(JobName).Returns(string.Empty);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndDataSetupRejectJobAsync(JobExecutionId, PlannedYear, ContextYear, RequestedBy, CorrelationId));

            ex.Errors.Should().ContainSingle(e => e.Code == "INVALID_Approval");
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupRejectJobAsync_WhenInitiatorAndRejectorAreSamePerson_ThrowsBusinessValidationError()
        {
            // Arrange
            var request = ValidInitiatedRequest();
            SetupResolvableRequest(request);
            _yearEndRepository.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Returns(true);
            _yearEndRepository.GetYearEndDataSetupRequestInitiatorAsync(JobName).Returns(RequestedBy);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndDataSetupRejectJobAsync(JobExecutionId, PlannedYear, ContextYear, RequestedBy, CorrelationId));

            ex.Errors.Should().ContainSingle(e => e.Code == "INVALID_Approval");
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupRejectJobAsync_WhenRepositoryFindsRequestNoLongerInitiated_TranslatesToBusinessValidationError()
        {
            // Arrange — same narrow-race translation as Approve.
            var request = ValidInitiatedRequest();
            SetupResolvableRequest(request);
            _yearEndRepository.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Returns(true);
            _yearEndRepository.GetYearEndDataSetupRequestInitiatorAsync(JobName).Returns(string.Empty);
            _yearEndRepository.EnqueueDataSetupRejectBatchJobAsync(request.JobQueueId, RequestedBy, Arg.Any<string>())
                .Throws(new KeyNotFoundException("No initiated Data Setup request was found."));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndDataSetupRejectJobAsync(JobExecutionId, PlannedYear, ContextYear, RequestedBy, CorrelationId));

            ex.Errors.Should().ContainSingle(e => e.Code == "REQUEST_NOT_EDITABLE");
        }

        #endregion

        #region EnqueueYearEndDataSetupRejectJobAsync — success

        [Fact]
        public async Task EnqueueYearEndDataSetupRejectJobAsync_WhenAllValid_ReturnsTrue()
        {
            // Arrange
            var request = ValidInitiatedRequest();
            SetupResolvableRequest(request);
            _yearEndRepository.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Returns(true);
            _yearEndRepository.GetYearEndDataSetupRequestInitiatorAsync(JobName).Returns("other@example.com");

            var queueEntry = new BatchJobQueue { JobqueueId = request.JobQueueId, RequestedBy = RequestedBy };
            _yearEndRepository.EnqueueDataSetupRejectBatchJobAsync(request.JobQueueId, RequestedBy, Arg.Any<string>())
                .Returns(queueEntry);
            _mapper.Map<BatchJobEventTriggerDto>(queueEntry).Returns(new BatchJobEventTriggerDto());

            // Act
            var result = await _sut.EnqueueYearEndDataSetupRejectJobAsync(JobExecutionId, PlannedYear, ContextYear, RequestedBy, CorrelationId);

            // Assert
            result.Should().BeTrue();
            await _yearEndRepository.Received(1).EnqueueDataSetupRejectBatchJobAsync(
                request.JobQueueId, RequestedBy, Arg.Any<string>());
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupRejectJobAsync_WhenAllValid_SendsEmail()
        {
            // Arrange
            var request = ValidInitiatedRequest();
            SetupResolvableRequest(request);
            _yearEndRepository.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Returns(true);
            _yearEndRepository.GetYearEndDataSetupRequestInitiatorAsync(JobName).Returns("other@example.com");

            var queueEntry = new BatchJobQueue { JobqueueId = request.JobQueueId, RequestedBy = RequestedBy };
            _yearEndRepository.EnqueueDataSetupRejectBatchJobAsync(request.JobQueueId, RequestedBy, Arg.Any<string>())
                .Returns(queueEntry);
            _mapper.Map<BatchJobEventTriggerDto>(queueEntry).Returns(new BatchJobEventTriggerDto());

            // Act
            await _sut.EnqueueYearEndDataSetupRejectJobAsync(JobExecutionId, PlannedYear, ContextYear, RequestedBy, CorrelationId);

            // Assert
            await _emailService.Received(1).SendEmailAsync(
                Arg.Is<EmailMessageModel>(m => m.To.Contains("rejection@example.com")),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupRejectJobAsync_WhenEmailFails_StillReturnsTrue()
        {
            // Arrange — email failure must be swallowed, not propagated
            var request = ValidInitiatedRequest();
            SetupResolvableRequest(request);
            _yearEndRepository.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Returns(true);
            _yearEndRepository.GetYearEndDataSetupRequestInitiatorAsync(JobName).Returns("other@example.com");

            var queueEntry = new BatchJobQueue { JobqueueId = request.JobQueueId };
            _yearEndRepository.EnqueueDataSetupRejectBatchJobAsync(request.JobQueueId, RequestedBy, Arg.Any<string>())
                .Returns(queueEntry);
            _mapper.Map<BatchJobEventTriggerDto>(queueEntry).Returns(new BatchJobEventTriggerDto());

            _emailService.SendEmailAsync(Arg.Any<EmailMessageModel>(), Arg.Any<CancellationToken>())
                .Throws(new Exception("SMTP failure"));

            // Act
            var result = await _sut.EnqueueYearEndDataSetupRejectJobAsync(JobExecutionId, PlannedYear, ContextYear, RequestedBy, CorrelationId);

            // Assert — email failure swallowed; true still returned
            result.Should().BeTrue();
        }

        #endregion

        #region CanInitiateYearEndCutOverRequestAsync

        [Fact]
        public async Task CanInitiateYearEndCutOverRequestAsync_WhenRepositoryReturnsTrue_ReturnsTrue()
        {
            // Arrange
            const string cutOverJobName = "YearEnd-CutOver";
            _yearEndRepository.CanInitiateYearEndCutOverRequestAsync(cutOverJobName).Returns(true);

            // Act
            var result = await _sut.CanInitiateYearEndCutOverRequestAsync(cutOverJobName);

            // Assert
            result.Should().BeTrue();
            await _yearEndRepository.Received(1).CanInitiateYearEndCutOverRequestAsync(cutOverJobName);
        }

        [Fact]
        public async Task CanInitiateYearEndCutOverRequestAsync_WhenRepositoryReturnsFalse_ReturnsFalse()
        {
            // Arrange
            const string cutOverJobName = "YearEnd-CutOver";
            _yearEndRepository.CanInitiateYearEndCutOverRequestAsync(cutOverJobName).Returns(false);

            // Act
            var result = await _sut.CanInitiateYearEndCutOverRequestAsync(cutOverJobName);

            // Assert
            result.Should().BeFalse();
            await _yearEndRepository.Received(1).CanInitiateYearEndCutOverRequestAsync(cutOverJobName);
        }

        [Fact]
        public async Task CanInitiateYearEndCutOverRequestAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            const string cutOverJobName = "YearEnd-CutOver";
            _yearEndRepository.CanInitiateYearEndCutOverRequestAsync(cutOverJobName)
                .Throws(new Exception("Repository error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sut.CanInitiateYearEndCutOverRequestAsync(cutOverJobName));
        }

        #endregion

        #region CanApproveOrRejectYearEndCutOverRequestAsync

        [Fact]
        public async Task CanApproveOrRejectYearEndCutOverRequestAsync_WhenRepositoryReturnsTrue_ReturnsTrue()
        {
            // Arrange
            const string cutOverJobName = "YearEnd-CutOver";
            _yearEndRepository.CanApproveOrRejectYearEndCutOverRequestAsync(cutOverJobName).Returns(true);

            // Act
            var result = await _sut.CanApproveOrRejectYearEndCutOverRequestAsync(cutOverJobName);

            // Assert
            result.Should().BeTrue();
            await _yearEndRepository.Received(1).CanApproveOrRejectYearEndCutOverRequestAsync(cutOverJobName);
        }

        [Fact]
        public async Task CanApproveOrRejectYearEndCutOverRequestAsync_WhenRepositoryReturnsFalse_ReturnsFalse()
        {
            // Arrange
            const string cutOverJobName = "YearEnd-CutOver";
            _yearEndRepository.CanApproveOrRejectYearEndCutOverRequestAsync(cutOverJobName).Returns(false);

            // Act
            var result = await _sut.CanApproveOrRejectYearEndCutOverRequestAsync(cutOverJobName);

            // Assert
            result.Should().BeFalse();
            await _yearEndRepository.Received(1).CanApproveOrRejectYearEndCutOverRequestAsync(cutOverJobName);
        }

        [Fact]
        public async Task CanApproveOrRejectYearEndCutOverRequestAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            const string cutOverJobName = "YearEnd-CutOver";
            _yearEndRepository.CanApproveOrRejectYearEndCutOverRequestAsync(cutOverJobName)
                .Throws(new Exception("Repository error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(
                () => _sut.CanApproveOrRejectYearEndCutOverRequestAsync(cutOverJobName));
        }

        #endregion

        #region EnqueueYearEndCutOverInitiationJobAsync — validation

        private const string CutOverJobName = "YearEnd-CutOver";

        [Theory]
        [InlineData(0)]
        [InlineData(1899)]
        [InlineData(10000)]
        public async Task EnqueueYearEndCutOverInitiationJobAsync_WhenPlannedYearIsInvalid_ThrowsBusinessValidationError(int invalidYear)
        {
            // Arrange
            _yearMasterRepository.GetFpsYearByIdAsync(invalidYear)
                .Returns(new YearMaster { FpsYear = invalidYear, YearStatus = "planned", Active = true });
            _yearEndRepository.CanInitiateYearEndCutOverRequestAsync(CutOverJobName).Returns(true);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndCutOverInitiationJobAsync(invalidYear, ContextYear, RequestedBy, CorrelationId));

            ex.Errors.Should().Contain(e => e.Code == "INVALID_PlannedYear");
        }

        [Fact]
        public async Task EnqueueYearEndCutOverInitiationJobAsync_WhenRequestedByIsEmpty_ThrowsBusinessValidationError()
        {
            // Arrange
            _yearMasterRepository.GetFpsYearByIdAsync(PlannedYear)
                .Returns(new YearMaster { FpsYear = PlannedYear, YearStatus = "planned", Active = true });
            _yearEndRepository.CanInitiateYearEndCutOverRequestAsync(CutOverJobName).Returns(true);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndCutOverInitiationJobAsync(PlannedYear, ContextYear, string.Empty, CorrelationId));

            ex.Errors.Should().ContainSingle(e => e.Code == "INVALID_User");
        }

        [Fact]
        public async Task EnqueueYearEndCutOverInitiationJobAsync_WhenYearMasterIsNull_ThrowsBusinessValidationError()
        {
            // Arrange — null means CutOver not valid (no planned year found or status already open)
            _yearMasterRepository.GetFpsYearByIdAsync(PlannedYear).Returns((YearMaster?)null);
            _yearEndRepository.CanInitiateYearEndCutOverRequestAsync(CutOverJobName).Returns(true);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndCutOverInitiationJobAsync(PlannedYear, ContextYear, RequestedBy, CorrelationId));

            ex.Errors.Should().ContainSingle(e => e.Code == "INVALID_Rerun");
        }

        [Fact]
        public async Task EnqueueYearEndCutOverInitiationJobAsync_WhenYearStatusIsOpen_ThrowsBusinessValidationError()
        {
            // Arrange — "open" status means cutover already live
            _yearMasterRepository.GetFpsYearByIdAsync(PlannedYear)
                .Returns(new YearMaster { FpsYear = PlannedYear, YearStatus = "open", Active = true });
            _yearEndRepository.CanInitiateYearEndCutOverRequestAsync(CutOverJobName).Returns(true);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndCutOverInitiationJobAsync(PlannedYear, ContextYear, RequestedBy, CorrelationId));

            ex.Errors.Should().ContainSingle(e => e.Code == "INVALID_Rerun");
        }

        [Fact]
        public async Task EnqueueYearEndCutOverInitiationJobAsync_WhenJobAlreadyRunning_ThrowsBusinessValidationError()
        {
            // Arrange
            _yearMasterRepository.GetFpsYearByIdAsync(PlannedYear)
                .Returns(new YearMaster { FpsYear = PlannedYear, YearStatus = "planned", Active = true });
            _yearEndRepository.CanInitiateYearEndCutOverRequestAsync(CutOverJobName).Returns(false);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndCutOverInitiationJobAsync(PlannedYear, ContextYear, RequestedBy, CorrelationId));

            ex.Errors.Should().ContainSingle(e => e.Code == "INVALID_Initiation");
        }

        #endregion

        #region EnqueueYearEndCutOverInitiationJobAsync — success

        private void SetupValidCutOverYearMaster() =>
            _yearMasterRepository.GetFpsYearByIdAsync(PlannedYear)
                .Returns(new YearMaster { FpsYear = PlannedYear, YearStatus = "planned", Active = true });

        [Fact]
        public async Task EnqueueYearEndCutOverInitiationJobAsync_WhenAllValid_ReturnsQueueDto()
        {
            // Arrange
            SetupValidCutOverYearMaster();
            _yearEndRepository.CanInitiateYearEndCutOverRequestAsync(CutOverJobName).Returns(true);

            var queueEntry = new BatchJobQueue { JobqueueId = Guid.NewGuid(), RequestedBy = RequestedBy };
            _yearEndRepository.EnqueueCutOverInitiationBatchJobAsync(CutOverJobName, RequestedBy, CorrelationId, Arg.Any<string>())
                .Returns(queueEntry);

            var expectedDto = new BatchJobQueueDto { RequestedBy = RequestedBy };
            _mapper.Map<BatchJobQueueDto>(queueEntry).Returns(expectedDto);

            // Act
            var result = await _sut.EnqueueYearEndCutOverInitiationJobAsync(PlannedYear, ContextYear, RequestedBy, CorrelationId);

            // Assert
            result.Should().NotBeNull();
            result.RequestedBy.Should().Be(RequestedBy);

            await _yearEndRepository.Received(1).EnqueueCutOverInitiationBatchJobAsync(
                CutOverJobName, RequestedBy, CorrelationId, Arg.Any<string>());
        }

        [Fact]
        public async Task EnqueueYearEndCutOverInitiationJobAsync_WhenAllValid_SendsInitiationEmail()
        {
            // Arrange
            SetupValidCutOverYearMaster();
            _yearEndRepository.CanInitiateYearEndCutOverRequestAsync(CutOverJobName).Returns(true);

            var queueEntry = new BatchJobQueue { JobqueueId = Guid.NewGuid() };
            _yearEndRepository.EnqueueCutOverInitiationBatchJobAsync(CutOverJobName, RequestedBy, CorrelationId, Arg.Any<string>())
                .Returns(queueEntry);
            _mapper.Map<BatchJobQueueDto>(queueEntry).Returns(new BatchJobQueueDto());

            // Act
            await _sut.EnqueueYearEndCutOverInitiationJobAsync(PlannedYear, ContextYear, RequestedBy, CorrelationId);

            // Assert — email sent to CutOver initiation recipient
            await _emailService.Received(1).SendEmailAsync(
                Arg.Is<EmailMessageModel>(m => m.To.Contains("cutover-initiation@example.com")),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task EnqueueYearEndCutOverInitiationJobAsync_WhenEmailFails_StillReturnsQueueDto()
        {
            // Arrange
            SetupValidCutOverYearMaster();
            _yearEndRepository.CanInitiateYearEndCutOverRequestAsync(CutOverJobName).Returns(true);

            var queueEntry = new BatchJobQueue { JobqueueId = Guid.NewGuid() };
            _yearEndRepository.EnqueueCutOverInitiationBatchJobAsync(CutOverJobName, RequestedBy, CorrelationId, Arg.Any<string>())
                .Returns(queueEntry);
            _mapper.Map<BatchJobQueueDto>(queueEntry).Returns(new BatchJobQueueDto { RequestedBy = RequestedBy });
            _emailService.SendEmailAsync(Arg.Any<EmailMessageModel>(), Arg.Any<CancellationToken>())
                .Throws(new Exception("SMTP failure"));

            // Act
            var result = await _sut.EnqueueYearEndCutOverInitiationJobAsync(PlannedYear, ContextYear, RequestedBy, CorrelationId);

            // Assert
            result.Should().NotBeNull();
        }

        #endregion

        #region EnqueueYearEndCutOverApprovalJobAsync — validation

        [Theory]
        [InlineData(0)]
        [InlineData(1899)]
        [InlineData(10000)]
        public async Task EnqueueYearEndCutOverApprovalJobAsync_WhenPlannedYearIsInvalid_ThrowsBusinessValidationError(int invalidYear)
        {
            // Arrange
            _yearMasterRepository.GetFpsYearByIdAsync(invalidYear)
                .Returns(new YearMaster { FpsYear = invalidYear, YearStatus = "planned", Active = true });
            _yearEndRepository.CanApproveOrRejectYearEndCutOverRequestAsync(CutOverJobName).Returns(true);
            _yearEndRepository.GetYearEndCutOverRequestInitiatorAsync(CutOverJobName).Returns(string.Empty);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndCutOverApprovalJobAsync(Guid.NewGuid(), invalidYear, ContextYear, RequestedBy, CorrelationId));

            ex.Errors.Should().Contain(e => e.Code == "INVALID_PlannedYear");
        }

        [Fact]
        public async Task EnqueueYearEndCutOverApprovalJobAsync_WhenRequestedByIsEmpty_ThrowsBusinessValidationError()
        {
            // Arrange
            SetupValidCutOverYearMaster();
            _yearEndRepository.CanApproveOrRejectYearEndCutOverRequestAsync(CutOverJobName).Returns(true);
            _yearEndRepository.GetYearEndCutOverRequestInitiatorAsync(CutOverJobName).Returns(string.Empty);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndCutOverApprovalJobAsync(Guid.NewGuid(), PlannedYear, ContextYear, string.Empty, CorrelationId));

            ex.Errors.Should().ContainSingle(e => e.Code == "INVALID_User");
        }

        [Fact]
        public async Task EnqueueYearEndCutOverApprovalJobAsync_WhenNoInitiatedRequestExists_ThrowsBusinessValidationError()
        {
            // Arrange
            SetupValidCutOverYearMaster();
            _yearEndRepository.CanApproveOrRejectYearEndCutOverRequestAsync(CutOverJobName).Returns(false);
            _yearEndRepository.GetYearEndCutOverRequestInitiatorAsync(CutOverJobName).Returns(string.Empty);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndCutOverApprovalJobAsync(Guid.NewGuid(), PlannedYear, ContextYear, RequestedBy, CorrelationId));

            ex.Errors.Should().ContainSingle(e => e.Code == "INVALID_Approval");
        }

        [Fact]
        public async Task EnqueueYearEndCutOverApprovalJobAsync_WhenInitiatorAndApproverAreSamePerson_ThrowsBusinessValidationError()
        {
            // Arrange
            SetupValidCutOverYearMaster();
            _yearEndRepository.CanApproveOrRejectYearEndCutOverRequestAsync(CutOverJobName).Returns(true);
            _yearEndRepository.GetYearEndCutOverRequestInitiatorAsync(CutOverJobName).Returns(RequestedBy);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndCutOverApprovalJobAsync(Guid.NewGuid(), PlannedYear, ContextYear, RequestedBy, CorrelationId));

            ex.Errors.Should().ContainSingle(e => e.Code == "INVALID_Approval");
        }

        [Fact]
        public async Task EnqueueYearEndCutOverApprovalJobAsync_WhenRepositoryFindsRequestNoLongerInitiated_TranslatesToBusinessValidationError()
        {
            // Arrange
            SetupValidCutOverYearMaster();
            _yearEndRepository.CanApproveOrRejectYearEndCutOverRequestAsync(CutOverJobName).Returns(true);
            _yearEndRepository.GetYearEndCutOverRequestInitiatorAsync(CutOverJobName).Returns(string.Empty);
            _yearEndRepository.EnqueueCutOverApprovalBatchJobAsync(Arg.Any<Guid>(), RequestedBy, Arg.Any<string>())
                .Throws(new KeyNotFoundException("gone"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndCutOverApprovalJobAsync(Guid.NewGuid(), PlannedYear, ContextYear, RequestedBy, CorrelationId));

            ex.Errors.Should().ContainSingle(e => e.Code == "REQUEST_NOT_EDITABLE");
        }

        #endregion

        #region EnqueueYearEndCutOverApprovalJobAsync — success

        [Fact]
        public async Task EnqueueYearEndCutOverApprovalJobAsync_WhenAllValid_ReturnsEventTriggerDto()
        {
            // Arrange
            SetupValidCutOverYearMaster();
            _yearEndRepository.CanApproveOrRejectYearEndCutOverRequestAsync(CutOverJobName).Returns(true);
            _yearEndRepository.GetYearEndCutOverRequestInitiatorAsync(CutOverJobName).Returns("other@example.com");

            var jobExecutionId = Guid.NewGuid();
            var queueEntry = new BatchJobQueue { JobqueueId = Guid.NewGuid(), RequestedBy = RequestedBy };
            _yearEndRepository.EnqueueCutOverApprovalBatchJobAsync(jobExecutionId, RequestedBy, Arg.Any<string>())
                .Returns(queueEntry);

            _eventPublisherService.PublishAsync(Arg.Any<EventDetail>(), Arg.Any<CancellationToken>())
                .Returns("evt-cutover-123");

            var triggerDto = new BatchJobEventTriggerDto { EventId = "evt-cutover-123" };
            _mapper.Map<BatchJobEventTriggerDto>(queueEntry).Returns(triggerDto);
            _mapper.Map<BatchJobEventTriggerDto>(triggerDto).Returns(triggerDto);

            // Act
            var result = await _sut.EnqueueYearEndCutOverApprovalJobAsync(jobExecutionId, PlannedYear, ContextYear, RequestedBy, CorrelationId);

            // Assert
            result.Should().NotBeNull();
            result.EventId.Should().Be("evt-cutover-123");

            await _yearEndRepository.Received(1).EnqueueCutOverApprovalBatchJobAsync(
                jobExecutionId, RequestedBy, Arg.Any<string>());
            await _eventPublisherService.Received(1).PublishAsync(Arg.Any<EventDetail>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task EnqueueYearEndCutOverApprovalJobAsync_WhenAllValid_PublishesEventWithRowsJobExecutionId_NotCorrelationId()
        {
            // Arrange — see the DataSetup approval equivalent for the full rationale.
            SetupValidCutOverYearMaster();
            _yearEndRepository.CanApproveOrRejectYearEndCutOverRequestAsync(CutOverJobName).Returns(true);
            _yearEndRepository.GetYearEndCutOverRequestInitiatorAsync(CutOverJobName).Returns("other@example.com");

            var jobExecutionId = Guid.NewGuid();
            var rowJobExecutionId = Guid.NewGuid();
            var queueEntry = new BatchJobQueue { JobqueueId = Guid.NewGuid(), JobExecutionId = rowJobExecutionId, RequestedBy = RequestedBy };
            _yearEndRepository.EnqueueCutOverApprovalBatchJobAsync(jobExecutionId, RequestedBy, Arg.Any<string>())
                .Returns(queueEntry);
            _eventPublisherService.PublishAsync(Arg.Any<EventDetail>(), Arg.Any<CancellationToken>())
                .Returns("evt-cutover-jobexec");

            var triggerDto = new BatchJobEventTriggerDto();
            _mapper.Map<BatchJobEventTriggerDto>(queueEntry).Returns(triggerDto);
            _mapper.Map<BatchJobEventTriggerDto>(triggerDto).Returns(triggerDto);

            // Act
            await _sut.EnqueueYearEndCutOverApprovalJobAsync(jobExecutionId, PlannedYear, ContextYear, RequestedBy, CorrelationId);

            // Assert
            await _eventPublisherService.Received(1).PublishAsync(
                Arg.Is<EventDetail>(e => e.JobExecutionId == rowJobExecutionId.ToString() && e.JobExecutionId != CorrelationId),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task EnqueueYearEndCutOverApprovalJobAsync_WhenPublishSucceeds_CallsSetTriggeredMetadataWithRowsJobExecutionId()
        {
            // Arrange
            SetupValidCutOverYearMaster();
            _yearEndRepository.CanApproveOrRejectYearEndCutOverRequestAsync(CutOverJobName).Returns(true);
            _yearEndRepository.GetYearEndCutOverRequestInitiatorAsync(CutOverJobName).Returns("other@example.com");

            var jobExecutionId = Guid.NewGuid();
            var rowJobExecutionId = Guid.NewGuid();
            var queueEntry = new BatchJobQueue { JobqueueId = Guid.NewGuid(), JobExecutionId = rowJobExecutionId, RequestedBy = RequestedBy };
            _yearEndRepository.EnqueueCutOverApprovalBatchJobAsync(jobExecutionId, RequestedBy, Arg.Any<string>())
                .Returns(queueEntry);
            _eventPublisherService.PublishAsync(Arg.Any<EventDetail>(), Arg.Any<CancellationToken>())
                .Returns("evt-cutover-trigger");

            var triggerDto = new BatchJobEventTriggerDto();
            _mapper.Map<BatchJobEventTriggerDto>(queueEntry).Returns(triggerDto);
            _mapper.Map<BatchJobEventTriggerDto>(triggerDto).Returns(triggerDto);

            // Act
            await _sut.EnqueueYearEndCutOverApprovalJobAsync(jobExecutionId, PlannedYear, ContextYear, RequestedBy, CorrelationId);

            // Assert
            await _yearEndRepository.Received(1).SetTriggeredMetadataAsync(rowJobExecutionId.ToString(), RequestedBy);
        }

        [Fact]
        public async Task EnqueueYearEndCutOverApprovalJobAsync_WhenPublishFails_DoesNotSetTriggeredMetadataAndPropagatesException()
        {
            // Arrange
            SetupValidCutOverYearMaster();
            _yearEndRepository.CanApproveOrRejectYearEndCutOverRequestAsync(CutOverJobName).Returns(true);
            _yearEndRepository.GetYearEndCutOverRequestInitiatorAsync(CutOverJobName).Returns("other@example.com");

            var jobExecutionId = Guid.NewGuid();
            var queueEntry = new BatchJobQueue { JobqueueId = Guid.NewGuid(), JobExecutionId = Guid.NewGuid(), RequestedBy = RequestedBy };
            _yearEndRepository.EnqueueCutOverApprovalBatchJobAsync(jobExecutionId, RequestedBy, Arg.Any<string>())
                .Returns(queueEntry);
            _eventPublisherService.PublishAsync(Arg.Any<EventDetail>(), Arg.Any<CancellationToken>())
                .Throws(new InvalidOperationException("EventBridge unavailable"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.EnqueueYearEndCutOverApprovalJobAsync(jobExecutionId, PlannedYear, ContextYear, RequestedBy, CorrelationId));

            await _yearEndRepository.DidNotReceive().SetTriggeredMetadataAsync(Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task EnqueueYearEndCutOverApprovalJobAsync_WhenAllValid_SendsApprovalEmail()
        {
            // Arrange
            SetupValidCutOverYearMaster();
            _yearEndRepository.CanApproveOrRejectYearEndCutOverRequestAsync(CutOverJobName).Returns(true);
            _yearEndRepository.GetYearEndCutOverRequestInitiatorAsync(CutOverJobName).Returns("other@example.com");

            var jobExecutionId = Guid.NewGuid();
            var queueEntry = new BatchJobQueue { JobqueueId = Guid.NewGuid() };
            _yearEndRepository.EnqueueCutOverApprovalBatchJobAsync(jobExecutionId, RequestedBy, Arg.Any<string>())
                .Returns(queueEntry);
            _eventPublisherService.PublishAsync(Arg.Any<EventDetail>(), Arg.Any<CancellationToken>())
                .Returns("evt-cutover-456");

            var triggerDto = new BatchJobEventTriggerDto();
            _mapper.Map<BatchJobEventTriggerDto>(queueEntry).Returns(triggerDto);
            _mapper.Map<BatchJobEventTriggerDto>(triggerDto).Returns(triggerDto);

            // Act
            await _sut.EnqueueYearEndCutOverApprovalJobAsync(jobExecutionId, PlannedYear, ContextYear, RequestedBy, CorrelationId);

            // Assert
            await _emailService.Received(1).SendEmailAsync(
                Arg.Is<EmailMessageModel>(m => m.To.Contains("cutover-approval@example.com")),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task EnqueueYearEndCutOverApprovalJobAsync_WhenEmailFails_StillReturnsEventTriggerDto()
        {
            // Arrange
            SetupValidCutOverYearMaster();
            _yearEndRepository.CanApproveOrRejectYearEndCutOverRequestAsync(CutOverJobName).Returns(true);
            _yearEndRepository.GetYearEndCutOverRequestInitiatorAsync(CutOverJobName).Returns("other@example.com");

            var jobExecutionId = Guid.NewGuid();
            var queueEntry = new BatchJobQueue { JobqueueId = Guid.NewGuid() };
            _yearEndRepository.EnqueueCutOverApprovalBatchJobAsync(jobExecutionId, RequestedBy, Arg.Any<string>())
                .Returns(queueEntry);
            _eventPublisherService.PublishAsync(Arg.Any<EventDetail>(), Arg.Any<CancellationToken>())
                .Returns("evt-cutover-789");

            var triggerDto = new BatchJobEventTriggerDto { EventId = "evt-cutover-789" };
            _mapper.Map<BatchJobEventTriggerDto>(queueEntry).Returns(triggerDto);
            _mapper.Map<BatchJobEventTriggerDto>(triggerDto).Returns(triggerDto);

            _emailService.SendEmailAsync(Arg.Any<EmailMessageModel>(), Arg.Any<CancellationToken>())
                .Throws(new Exception("SMTP failure"));

            // Act
            var result = await _sut.EnqueueYearEndCutOverApprovalJobAsync(jobExecutionId, PlannedYear, ContextYear, RequestedBy, CorrelationId);

            // Assert
            result.Should().NotBeNull();
        }

        #endregion

        #region EnqueueYearEndCutOverRejectJobAsync — validation

        [Fact]
        public async Task EnqueueYearEndCutOverRejectJobAsync_WhenRequestedByIsEmpty_ThrowsBusinessValidationError()
        {
            // Arrange
            _yearEndRepository.CanApproveOrRejectYearEndCutOverRequestAsync(CutOverJobName).Returns(true);
            _yearEndRepository.GetYearEndCutOverRequestInitiatorAsync(CutOverJobName).Returns(string.Empty);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndCutOverRejectJobAsync(Guid.NewGuid(), PlannedYear, ContextYear, string.Empty, CorrelationId));

            ex.Errors.Should().ContainSingle(e => e.Code == "INVALID_User");
        }

        [Fact]
        public async Task EnqueueYearEndCutOverRejectJobAsync_WhenNoInitiatedRequestExists_ThrowsBusinessValidationError()
        {
            // Arrange
            _yearEndRepository.CanApproveOrRejectYearEndCutOverRequestAsync(CutOverJobName).Returns(false);
            _yearEndRepository.GetYearEndCutOverRequestInitiatorAsync(CutOverJobName).Returns(string.Empty);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndCutOverRejectJobAsync(Guid.NewGuid(), PlannedYear, ContextYear, RequestedBy, CorrelationId));

            ex.Errors.Should().ContainSingle(e => e.Code == "INVALID_Approval");
        }

        [Fact]
        public async Task EnqueueYearEndCutOverRejectJobAsync_WhenInitiatorAndRejectorAreSamePerson_ThrowsBusinessValidationError()
        {
            // Arrange
            _yearEndRepository.CanApproveOrRejectYearEndCutOverRequestAsync(CutOverJobName).Returns(true);
            _yearEndRepository.GetYearEndCutOverRequestInitiatorAsync(CutOverJobName).Returns(RequestedBy);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndCutOverRejectJobAsync(Guid.NewGuid(), PlannedYear, ContextYear, RequestedBy, CorrelationId));

            ex.Errors.Should().ContainSingle(e => e.Code == "INVALID_Approval");
        }

        [Fact]
        public async Task EnqueueYearEndCutOverRejectJobAsync_WhenRepositoryFindsRequestNoLongerInitiated_TranslatesToBusinessValidationError()
        {
            // Arrange
            _yearEndRepository.CanApproveOrRejectYearEndCutOverRequestAsync(CutOverJobName).Returns(true);
            _yearEndRepository.GetYearEndCutOverRequestInitiatorAsync(CutOverJobName).Returns(string.Empty);
            _yearEndRepository.EnqueueCutOverRejectBatchJobAsync(Arg.Any<Guid>(), RequestedBy, Arg.Any<string>())
                .Throws(new KeyNotFoundException("gone"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndCutOverRejectJobAsync(Guid.NewGuid(), PlannedYear, ContextYear, RequestedBy, CorrelationId));

            ex.Errors.Should().ContainSingle(e => e.Code == "REQUEST_NOT_EDITABLE");
        }

        #endregion

        #region EnqueueYearEndCutOverRejectJobAsync — success

        [Fact]
        public async Task EnqueueYearEndCutOverRejectJobAsync_WhenAllValid_ReturnsTrue()
        {
            // Arrange
            _yearEndRepository.CanApproveOrRejectYearEndCutOverRequestAsync(CutOverJobName).Returns(true);
            _yearEndRepository.GetYearEndCutOverRequestInitiatorAsync(CutOverJobName).Returns("other@example.com");

            var jobExecutionId = Guid.NewGuid();
            var queueEntry = new BatchJobQueue { JobqueueId = Guid.NewGuid(), RequestedBy = RequestedBy };
            _yearEndRepository.EnqueueCutOverRejectBatchJobAsync(jobExecutionId, RequestedBy, Arg.Any<string>())
                .Returns(queueEntry);
            _mapper.Map<BatchJobEventTriggerDto>(queueEntry).Returns(new BatchJobEventTriggerDto());

            // Act
            var result = await _sut.EnqueueYearEndCutOverRejectJobAsync(jobExecutionId, PlannedYear, ContextYear, RequestedBy, CorrelationId);

            // Assert
            result.Should().BeTrue();
            await _yearEndRepository.Received(1).EnqueueCutOverRejectBatchJobAsync(
                jobExecutionId, RequestedBy, Arg.Any<string>());
        }

        [Fact]
        public async Task EnqueueYearEndCutOverRejectJobAsync_WhenAllValid_SendsRejectionEmail()
        {
            // Arrange
            _yearEndRepository.CanApproveOrRejectYearEndCutOverRequestAsync(CutOverJobName).Returns(true);
            _yearEndRepository.GetYearEndCutOverRequestInitiatorAsync(CutOverJobName).Returns("other@example.com");

            var jobExecutionId = Guid.NewGuid();
            var queueEntry = new BatchJobQueue { JobqueueId = Guid.NewGuid(), RequestedBy = RequestedBy };
            _yearEndRepository.EnqueueCutOverRejectBatchJobAsync(jobExecutionId, RequestedBy, Arg.Any<string>())
                .Returns(queueEntry);
            _mapper.Map<BatchJobEventTriggerDto>(queueEntry).Returns(new BatchJobEventTriggerDto());

            // Act
            await _sut.EnqueueYearEndCutOverRejectJobAsync(jobExecutionId, PlannedYear, ContextYear, RequestedBy, CorrelationId);

            // Assert
            await _emailService.Received(1).SendEmailAsync(
                Arg.Is<EmailMessageModel>(m => m.To.Contains("cutover-rejection@example.com")),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task EnqueueYearEndCutOverRejectJobAsync_WhenEmailFails_StillReturnsTrue()
        {
            // Arrange
            _yearEndRepository.CanApproveOrRejectYearEndCutOverRequestAsync(CutOverJobName).Returns(true);
            _yearEndRepository.GetYearEndCutOverRequestInitiatorAsync(CutOverJobName).Returns("other@example.com");

            var jobExecutionId = Guid.NewGuid();
            var queueEntry = new BatchJobQueue { JobqueueId = Guid.NewGuid() };
            _yearEndRepository.EnqueueCutOverRejectBatchJobAsync(jobExecutionId, RequestedBy, Arg.Any<string>())
                .Returns(queueEntry);
            _mapper.Map<BatchJobEventTriggerDto>(queueEntry).Returns(new BatchJobEventTriggerDto());

            _emailService.SendEmailAsync(Arg.Any<EmailMessageModel>(), Arg.Any<CancellationToken>())
                .Throws(new Exception("SMTP failure"));

            // Act
            var result = await _sut.EnqueueYearEndCutOverRejectJobAsync(jobExecutionId, PlannedYear, ContextYear, RequestedBy, CorrelationId);

            // Assert
            result.Should().BeTrue();
        }

        #endregion
    }
}