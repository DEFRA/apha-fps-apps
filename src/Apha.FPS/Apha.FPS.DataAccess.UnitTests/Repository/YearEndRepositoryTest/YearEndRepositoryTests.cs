using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using NSubstitute;

namespace Apha.FPS.DataAccess.UnitTests.Repository.YearEndRepositoryTest
{
    public class YearEndRepositoryTests
    {
        private const int    DefaultFpsYear  = 2024;
        private const string DefaultJobName  = "YearEndSetup";
        private const string DefaultUserEmail = "test@example.com";
        private const string CutOverJobName = "YearEnd-CutOver";

        // -----------------------------------------------------------------------
        // Factory
        // -----------------------------------------------------------------------

        /// <summary>
        /// Creates a YearEndRepository wired to mocked in-memory DbSets.
        /// Returns the repository, the mocked context, and the mutable DbSet mocks
        /// so individual tests can verify Add / Update calls.
        /// </summary>
        private static (
            YearEndRepository          Repo,
            Mock<FpsDbContext>          Context,
            Mock<DbSet<BatchJobQueue>>  QueueSet,
            Mock<DbSet<BatchJobQueueLog>> LogSet)
            CreateRepository(
                IEnumerable<BatchJobMaster>?   jobs     = null,
                IEnumerable<BatchJobQueue>?    queues   = null,
                IEnumerable<BatchJobStatus>?   statuses = null,
                IEnumerable<BatchJobQueueLog>? logs     = null,
                int fpsYear = DefaultFpsYear,
                IYearEndStagingRepository?     stagingRepository = null)
        {
            var requestContext = Substitute.For<IFpsRequestContext>();
            requestContext.FpsYear.Returns(fpsYear);

            // Only used by the DataSetup Approve/Reject path's Reject staging deletion - callers that
            // need to assert on it (Received/DidNotReceive) pass their own substitute in; every other
            // test gets a throwaway one so CreateRepository's tuple shape doesn't need to change.
            stagingRepository ??= Substitute.For<IYearEndStagingRepository>();

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestContext);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            var jobsMockSet      = RepositoryTestHelper.CreateMockDbSet(jobs     ?? []);
            var queuesMockSet    = RepositoryTestHelper.CreateMockDbSet(queues   ?? []);
            var statusesMockSet  = RepositoryTestHelper.CreateMockDbSet(statuses ?? []);
            var logsMockSet      = RepositoryTestHelper.CreateMockDbSet(logs     ?? []);

            RepositoryTestHelper.SetupDbSetOperations(queuesMockSet);
            RepositoryTestHelper.SetupDbSetOperations(logsMockSet);

            mockContext.Setup(x => x.BatchJobs).Returns(jobsMockSet.Object);
            mockContext.Setup(x => x.BatchJobQueues).Returns(queuesMockSet.Object);
            mockContext.Setup(x => x.BatchJobStatuses).Returns(statusesMockSet.Object);
            mockContext.Setup(x => x.BatchJobQueueLogs).Returns(logsMockSet.Object);

            var repo = new YearEndRepository(mockContext.Object, requestContext, stagingRepository);
            return (repo, mockContext, queuesMockSet, logsMockSet);
        }

        // -----------------------------------------------------------------------
        // Builders
        // -----------------------------------------------------------------------

        private static BatchJobMaster BuildJob(int jobId = 1, string? jobName = null) =>
            new() { JobId = jobId, JobName = jobName ?? DefaultJobName };

        private static BatchJobQueue BuildQueue(
            int    jobId,
            int    statusId,
            Guid?  jobqueueId      = null,
            string requestedBy     = DefaultUserEmail,
            DateTime? startDateTime = null,
            int    fpsYear         = DefaultFpsYear) =>
            new()
            {
                JobqueueId    = jobqueueId ?? Guid.NewGuid(),
                JobExecutionId = Guid.NewGuid(),
                JobId         = jobId,
                StatusId      = statusId,
                RequestedBy   = requestedBy,
                StartDateTime = startDateTime ?? DateTime.UtcNow,
                FpsYear       = fpsYear
            };

        private static BatchJobStatus BuildStatus(int statusId, int jobId, string status) =>
            new() { StatusId = statusId, JobId = jobId, Status = status };

        private static PaginationParameters<string> BuildQuery(
            int page = 1, int pageSize = 10,
            string? sortBy = null, bool descending = false) =>
            new(page: page, pageSize: pageSize, descending: descending, sortBy: sortBy);

        // -----------------------------------------------------------------------
        // Shared test data — a complete three-way join seed
        // -----------------------------------------------------------------------

        /// <summary>
        /// Returns three entities that form a valid join for the given jobName/status.
        /// StatusId 10 = "initiated".
        /// </summary>
        private static (BatchJobMaster Job, BatchJobQueue Queue, BatchJobStatus Status)
            BuildJoinSeed(
                string jobName    = DefaultJobName,
                string statusText = "initiated",
                int    jobId      = 1,
                int    statusId   = 10,
                Guid?  queueId    = null,
                string requestedBy = DefaultUserEmail,
                DateTime? startDt  = null)
        {
            var job    = BuildJob(jobId, jobName);
            var status = BuildStatus(statusId, jobId, statusText);
            var queue  = BuildQueue(jobId, statusId, queueId, requestedBy, startDt);
            return (job, queue, status);
        }

        // -----------------------------------------------------------------------
        // Constructor
        // -----------------------------------------------------------------------

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenContextIsNull()
        {
            var ctx = Substitute.For<IFpsRequestContext>();
            var stagingRepo = Substitute.For<IYearEndStagingRepository>();
            Assert.Throws<ArgumentNullException>(() => new YearEndRepository(null!, ctx, stagingRepo));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenStagingRepositoryIsNull()
        {
            var (_, mockContext, _, _) = CreateRepository();
            var ctx = Substitute.For<IFpsRequestContext>();
            Assert.Throws<ArgumentNullException>(() => new YearEndRepository(mockContext.Object, ctx, null!));
        }

        #region GetBatchJobsHistoryAsync

        [Fact]
        public async Task GetBatchJobsHistoryAsync_WithMatchingJob_ReturnsHistoryRecord()
        {
            // Arrange
            var (job, queue, status) = BuildJoinSeed();
            var (repo, _, _, _) = CreateRepository(jobs: [job], queues: [queue], statuses: [status]);

            // Act
            var result = await repo.GetBatchJobsHistoryAsync(BuildQuery(), DefaultJobName);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            var item = result.Data.First();
            Assert.Equal(DefaultJobName,   item.JobName);
            Assert.Equal(status.Status,    item.Status);
            Assert.Equal(queue.RequestedBy, item.RequestedBy);
        }

        [Fact]
        public async Task GetBatchJobsHistoryAsync_WithNoMatchingJob_ReturnsEmptyPage()
        {
            // Arrange — job name does not match
            var (job, queue, status) = BuildJoinSeed(jobName: "OtherJob");
            var (repo, _, _, _) = CreateRepository(jobs: [job], queues: [queue], statuses: [status]);

            // Act
            var result = await repo.GetBatchJobsHistoryAsync(BuildQuery(), DefaultJobName);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetBatchJobsHistoryAsync_WithNoData_ReturnsEmptyPage()
        {
            // Arrange
            var (repo, _, _, _) = CreateRepository();

            // Act
            var result = await repo.GetBatchJobsHistoryAsync(BuildQuery(), DefaultJobName);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetBatchJobsHistoryAsync_IsJobNameCaseInsensitive()
        {
            // Arrange — job stored as uppercase, queried as lowercase
            var (job, queue, status) = BuildJoinSeed(jobName: "YEARENDSETUP");
            var (repo, _, _, _) = CreateRepository(jobs: [job], queues: [queue], statuses: [status]);

            // Act
            var result = await repo.GetBatchJobsHistoryAsync(BuildQuery(), "yearendsetup");

            // Assert
            Assert.Single(result.Data);
        }

        [Fact]
        public async Task GetBatchJobsHistoryAsync_ReturnsOnlyRecordsForMatchingJobName()
        {
            // Arrange — two jobs; only one matches
            var (job1, queue1, status1) = BuildJoinSeed(jobName: DefaultJobName, jobId: 1, statusId: 10);
            var (job2, queue2, status2) = BuildJoinSeed(jobName: "OtherJob",     jobId: 2, statusId: 20);
            var (repo, _, _, _) = CreateRepository(
                jobs:     [job1, job2],
                queues:   [queue1, queue2],
                statuses: [status1, status2]);

            // Act
            var result = await repo.GetBatchJobsHistoryAsync(BuildQuery(), DefaultJobName);

            // Assert
            Assert.Single(result.Data);
            Assert.All(result.Data, h => Assert.Equal(DefaultJobName, h.JobName));
        }

        [Fact]
        public async Task GetBatchJobsHistoryAsync_WithNoSortBy_OrdersByStartDateTimeDescending()
        {
            // Arrange — three records with distinct start times; default sort is descending StartDateTime
            var job = BuildJob();
            var status = BuildStatus(10, 1, "initiated");
            var earlier  = BuildQueue(1, 10, startDateTime: new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            var middle   = BuildQueue(1, 10, startDateTime: new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc));
            var later    = BuildQueue(1, 10, startDateTime: new DateTime(2024, 12, 1, 0, 0, 0, DateTimeKind.Utc));

            var (repo, _, _, _) = CreateRepository(
                jobs:     [job],
                queues:   [earlier, middle, later],
                statuses: [status]);

            // Act
            var result = await repo.GetBatchJobsHistoryAsync(BuildQuery(sortBy: null), DefaultJobName);

            // Assert — most recent first
            var list = result.Data.ToList();
            Assert.Equal(3, list.Count);
            Assert.True(list[0].StartDateTime >= list[1].StartDateTime);
            Assert.True(list[1].StartDateTime >= list[2].StartDateTime);
        }

        [Fact]
        public async Task GetBatchJobsHistoryAsync_WithSortByStartDateTime_Ascending()
        {
            // Arrange
            var job    = BuildJob();
            var status = BuildStatus(10, 1, "initiated");
            var older  = BuildQueue(1, 10, startDateTime: new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            var newer  = BuildQueue(1, 10, startDateTime: new DateTime(2024, 12, 1, 0, 0, 0, DateTimeKind.Utc));

            var (repo, _, _, _) = CreateRepository(
                jobs: [job], queues: [newer, older], statuses: [status]);

            // Act
            var result = await repo.GetBatchJobsHistoryAsync(
                BuildQuery(sortBy: "startdatetime", descending: false), DefaultJobName);

            // Assert — oldest first
            var list = result.Data.ToList();
            Assert.Equal(2, list.Count);
            Assert.True(list[0].StartDateTime <= list[1].StartDateTime);
        }

        [Fact]
        public async Task GetBatchJobsHistoryAsync_WithSortByStartDateTime_Descending()
        {
            // Arrange
            var job    = BuildJob();
            var status = BuildStatus(10, 1, "initiated");
            var older  = BuildQueue(1, 10, startDateTime: new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            var newer  = BuildQueue(1, 10, startDateTime: new DateTime(2024, 12, 1, 0, 0, 0, DateTimeKind.Utc));

            var (repo, _, _, _) = CreateRepository(
                jobs: [job], queues: [older, newer], statuses: [status]);

            // Act
            var result = await repo.GetBatchJobsHistoryAsync(
                BuildQuery(sortBy: "startdatetime", descending: true), DefaultJobName);

            // Assert — newest first
            var list = result.Data.ToList();
            Assert.Equal(2, list.Count);
            Assert.True(list[0].StartDateTime >= list[1].StartDateTime);
        }

        [Fact]
        public async Task GetBatchJobsHistoryAsync_WithUnknownSortBy_OrdersByStartDateTimeAscending()
        {
            // Arrange — unknown sortBy falls back to ascending StartDateTime
            var job    = BuildJob();
            var status = BuildStatus(10, 1, "initiated");
            var older  = BuildQueue(1, 10, startDateTime: new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            var newer  = BuildQueue(1, 10, startDateTime: new DateTime(2024, 12, 1, 0, 0, 0, DateTimeKind.Utc));

            var (repo, _, _, _) = CreateRepository(
                jobs: [job], queues: [newer, older], statuses: [status]);

            // Act
            var result = await repo.GetBatchJobsHistoryAsync(
                BuildQuery(sortBy: "nonexistent"), DefaultJobName);

            // Assert — ascending
            var list = result.Data.ToList();
            Assert.True(list[0].StartDateTime <= list[1].StartDateTime);
        }

        [Fact]
        public async Task GetBatchJobsHistoryAsync_WithPaging_ReturnsCorrectPage()
        {
            // Arrange — three records, page size 2
            var job    = BuildJob();
            var status = BuildStatus(10, 1, "initiated");
            var q1 = BuildQueue(1, 10, startDateTime: new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            var q2 = BuildQueue(1, 10, startDateTime: new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc));
            var q3 = BuildQueue(1, 10, startDateTime: new DateTime(2024, 12, 1, 0, 0, 0, DateTimeKind.Utc));

            var (repo, _, _, _) = CreateRepository(
                jobs: [job], queues: [q1, q2, q3], statuses: [status]);

            // Act
            var result = await repo.GetBatchJobsHistoryAsync(
                BuildQuery(page: 2, pageSize: 2, sortBy: "startdatetime"), DefaultJobName);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal(3, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.TotalPages);
            Assert.Equal(2, result.PaginationData.PageNumber);
        }

        [Fact]
        public async Task GetBatchJobsHistoryAsync_MapsAllBatchJobHistoryFields()
        {
            // Arrange
            var executionId = Guid.NewGuid();
            var startDt     = new DateTime(2024, 5, 1, 9, 0, 0, DateTimeKind.Utc);
            var endDt       = new DateTime(2024, 5, 1, 10, 0, 0, DateTimeKind.Utc);

            var job    = BuildJob(1, DefaultJobName);
            var status = BuildStatus(10, 1, "Completed");
            var queue  = new BatchJobQueue
            {
                JobqueueId     = Guid.NewGuid(),
                JobExecutionId = executionId,
                JobId          = 1,
                StatusId       = 10,
                RequestedBy    = "admin@example.com",
                StartDateTime  = startDt,
                EndDateTime    = endDt,
                ErrorMessage   = "none",
                FpsYear        = DefaultFpsYear
            };

            var (repo, _, _, _) = CreateRepository(
                jobs: [job], queues: [queue], statuses: [status]);

            // Act
            var result = await repo.GetBatchJobsHistoryAsync(BuildQuery(), DefaultJobName);

            // Assert
            var item = Assert.Single(result.Data);
            Assert.Equal(1,                  item.JobId);
            Assert.Equal(DefaultJobName,     item.JobName);
            Assert.Equal(executionId,        item.JobExecutionId);
            Assert.Equal("admin@example.com", item.RequestedBy);
            Assert.Equal("Completed",        item.Status);
            Assert.Equal(startDt,            item.StartDateTime);
            Assert.Equal(endDt,              item.EndDateTime);
            Assert.Equal("none",             item.ErrorMessage);
        }

        [Fact]
        public async Task GetBatchJobsHistoryAsync_WithSortByJobId_Ascending()
        {
            // Arrange — two records with different JobIds
            var job    = BuildJob(1, DefaultJobName);
            var status = BuildStatus(10, 1, "initiated");
            var q1 = new BatchJobQueue { JobqueueId = Guid.NewGuid(), JobExecutionId = Guid.NewGuid(), JobId = 1, StatusId = 10, RequestedBy = DefaultUserEmail, StartDateTime = DateTime.UtcNow, FpsYear = DefaultFpsYear };
            var q2 = new BatchJobQueue { JobqueueId = Guid.NewGuid(), JobExecutionId = Guid.NewGuid(), JobId = 1, StatusId = 10, RequestedBy = DefaultUserEmail, StartDateTime = DateTime.UtcNow.AddHours(1), FpsYear = DefaultFpsYear };

            var (repo, _, _, _) = CreateRepository(jobs: [job], queues: [q1, q2], statuses: [status]);

            // Act
            var result = await repo.GetBatchJobsHistoryAsync(
                BuildQuery(sortBy: "jobid", descending: false), DefaultJobName);

            // Assert — query returns records; sorting by jobId ascending compiles and runs
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetBatchJobsHistoryAsync_WithSortByJobId_Descending()
        {
            // Arrange
            var job    = BuildJob(1, DefaultJobName);
            var status = BuildStatus(10, 1, "initiated");
            var q1 = BuildQueue(1, 10, startDateTime: new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            var q2 = BuildQueue(1, 10, startDateTime: new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc));

            var (repo, _, _, _) = CreateRepository(jobs: [job], queues: [q1, q2], statuses: [status]);

            // Act
            var result = await repo.GetBatchJobsHistoryAsync(
                BuildQuery(sortBy: "jobid", descending: true), DefaultJobName);

            // Assert
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetBatchJobsHistoryAsync_WithSortByJobName_Ascending()
        {
            // Arrange
            var job    = BuildJob(1, DefaultJobName);
            var status = BuildStatus(10, 1, "initiated");
            var q1 = BuildQueue(1, 10);
            var q2 = BuildQueue(1, 10);

            var (repo, _, _, _) = CreateRepository(jobs: [job], queues: [q1, q2], statuses: [status]);

            // Act
            var result = await repo.GetBatchJobsHistoryAsync(
                BuildQuery(sortBy: "jobname", descending: false), DefaultJobName);

            // Assert
            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, h => Assert.Equal(DefaultJobName, h.JobName));
        }

        [Fact]
        public async Task GetBatchJobsHistoryAsync_WithSortByJobName_Descending()
        {
            // Arrange
            var job    = BuildJob(1, DefaultJobName);
            var status = BuildStatus(10, 1, "initiated");
            var q1 = BuildQueue(1, 10);
            var q2 = BuildQueue(1, 10);

            var (repo, _, _, _) = CreateRepository(jobs: [job], queues: [q1, q2], statuses: [status]);

            // Act
            var result = await repo.GetBatchJobsHistoryAsync(
                BuildQuery(sortBy: "jobname", descending: true), DefaultJobName);

            // Assert
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetBatchJobsHistoryAsync_WithSortByJobExecutionId_Ascending()
        {
            // Arrange
            var job    = BuildJob(1, DefaultJobName);
            var status = BuildStatus(10, 1, "initiated");
            var q1 = BuildQueue(1, 10, startDateTime: new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            var q2 = BuildQueue(1, 10, startDateTime: new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc));

            var (repo, _, _, _) = CreateRepository(jobs: [job], queues: [q1, q2], statuses: [status]);

            // Act
            var result = await repo.GetBatchJobsHistoryAsync(
                BuildQuery(sortBy: "jobexecutionid", descending: false), DefaultJobName);

            // Assert
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetBatchJobsHistoryAsync_WithSortByJobExecutionId_Descending()
        {
            // Arrange
            var job    = BuildJob(1, DefaultJobName);
            var status = BuildStatus(10, 1, "initiated");
            var q1 = BuildQueue(1, 10);
            var q2 = BuildQueue(1, 10);

            var (repo, _, _, _) = CreateRepository(jobs: [job], queues: [q1, q2], statuses: [status]);

            // Act
            var result = await repo.GetBatchJobsHistoryAsync(
                BuildQuery(sortBy: "jobexecutionid", descending: true), DefaultJobName);

            // Assert
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetBatchJobsHistoryAsync_WithSortByRequestedBy_Ascending()
        {
            // Arrange — two records with different RequestedBy values
            var job    = BuildJob(1, DefaultJobName);
            var status = BuildStatus(10, 1, "initiated");
            var q1 = BuildQueue(1, 10, requestedBy: "alpha@example.com");
            var q2 = BuildQueue(1, 10, requestedBy: "zeta@example.com");

            var (repo, _, _, _) = CreateRepository(jobs: [job], queues: [q1, q2], statuses: [status]);

            // Act
            var result = await repo.GetBatchJobsHistoryAsync(
                BuildQuery(sortBy: "requestedby", descending: false), DefaultJobName);

            // Assert — records sorted ascending by RequestedBy
            var list = result.Data.ToList();
            Assert.Equal(2, list.Count);
            Assert.True(string.Compare(list[0].RequestedBy, list[1].RequestedBy, StringComparison.Ordinal) <= 0);
        }

        [Fact]
        public async Task GetBatchJobsHistoryAsync_WithSortByRequestedBy_Descending()
        {
            // Arrange
            var job    = BuildJob(1, DefaultJobName);
            var status = BuildStatus(10, 1, "initiated");
            var q1 = BuildQueue(1, 10, requestedBy: "alpha@example.com");
            var q2 = BuildQueue(1, 10, requestedBy: "zeta@example.com");

            var (repo, _, _, _) = CreateRepository(jobs: [job], queues: [q1, q2], statuses: [status]);

            // Act
            var result = await repo.GetBatchJobsHistoryAsync(
                BuildQuery(sortBy: "requestedby", descending: true), DefaultJobName);

            // Assert — records sorted descending by RequestedBy
            var list = result.Data.ToList();
            Assert.Equal(2, list.Count);
            Assert.True(string.Compare(list[0].RequestedBy, list[1].RequestedBy, StringComparison.Ordinal) >= 0);
        }

        [Fact]
        public async Task GetBatchJobsHistoryAsync_WithSortByEndDateTime_Ascending()
        {
            // Arrange — two records with distinct EndDateTime values
            var job    = BuildJob(1, DefaultJobName);
            var status = BuildStatus(10, 1, "initiated");
            var earlierEnd = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            var laterEnd   = new DateTime(2024, 12, 1, 12, 0, 0, DateTimeKind.Utc);

            var q1 = new BatchJobQueue
            {
                JobqueueId = Guid.NewGuid(), JobExecutionId = Guid.NewGuid(),
                JobId = 1, StatusId = 10, RequestedBy = DefaultUserEmail,
                StartDateTime = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                EndDateTime   = laterEnd, FpsYear = DefaultFpsYear
            };
            var q2 = new BatchJobQueue
            {
                JobqueueId = Guid.NewGuid(), JobExecutionId = Guid.NewGuid(),
                JobId = 1, StatusId = 10, RequestedBy = DefaultUserEmail,
                StartDateTime = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc),
                EndDateTime   = earlierEnd, FpsYear = DefaultFpsYear
            };

            var (repo, _, _, _) = CreateRepository(jobs: [job], queues: [q1, q2], statuses: [status]);

            // Act
            var result = await repo.GetBatchJobsHistoryAsync(
                BuildQuery(sortBy: "enddatetime", descending: false), DefaultJobName);

            // Assert — earliest EndDateTime first
            var list = result.Data.ToList();
            Assert.Equal(2, list.Count);
            Assert.True(list[0].EndDateTime <= list[1].EndDateTime);
        }

        [Fact]
        public async Task GetBatchJobsHistoryAsync_WithSortByEndDateTime_Descending()
        {
            // Arrange
            var job    = BuildJob(1, DefaultJobName);
            var status = BuildStatus(10, 1, "initiated");
            var earlierEnd = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            var laterEnd   = new DateTime(2024, 12, 1, 12, 0, 0, DateTimeKind.Utc);

            var q1 = new BatchJobQueue
            {
                JobqueueId = Guid.NewGuid(), JobExecutionId = Guid.NewGuid(),
                JobId = 1, StatusId = 10, RequestedBy = DefaultUserEmail,
                StartDateTime = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                EndDateTime   = earlierEnd, FpsYear = DefaultFpsYear
            };
            var q2 = new BatchJobQueue
            {
                JobqueueId = Guid.NewGuid(), JobExecutionId = Guid.NewGuid(),
                JobId = 1, StatusId = 10, RequestedBy = DefaultUserEmail,
                StartDateTime = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc),
                EndDateTime   = laterEnd, FpsYear = DefaultFpsYear
            };

            var (repo, _, _, _) = CreateRepository(jobs: [job], queues: [q1, q2], statuses: [status]);

            // Act
            var result = await repo.GetBatchJobsHistoryAsync(
                BuildQuery(sortBy: "enddatetime", descending: true), DefaultJobName);

            // Assert — latest EndDateTime first
            var list = result.Data.ToList();
            Assert.Equal(2, list.Count);
            Assert.True(list[0].EndDateTime >= list[1].EndDateTime);
        }

        [Fact]
        public async Task GetBatchJobsHistoryAsync_WithSortByErrorMessage_Ascending()
        {
            // Arrange — two records with distinct ErrorMessage values
            var job    = BuildJob(1, DefaultJobName);
            var status = BuildStatus(10, 1, "initiated");

            var q1 = new BatchJobQueue
            {
                JobqueueId = Guid.NewGuid(), JobExecutionId = Guid.NewGuid(),
                JobId = 1, StatusId = 10, RequestedBy = DefaultUserEmail,
                StartDateTime = DateTime.UtcNow, ErrorMessage = "Zeta error",
                FpsYear = DefaultFpsYear
            };
            var q2 = new BatchJobQueue
            {
                JobqueueId = Guid.NewGuid(), JobExecutionId = Guid.NewGuid(),
                JobId = 1, StatusId = 10, RequestedBy = DefaultUserEmail,
                StartDateTime = DateTime.UtcNow.AddHours(1), ErrorMessage = "Alpha error",
                FpsYear = DefaultFpsYear
            };

            var (repo, _, _, _) = CreateRepository(jobs: [job], queues: [q1, q2], statuses: [status]);

            // Act
            var result = await repo.GetBatchJobsHistoryAsync(
                BuildQuery(sortBy: "errormessage", descending: false), DefaultJobName);

            // Assert — alphabetically first ErrorMessage appears first
            var list = result.Data.ToList();
            Assert.Equal(2, list.Count);
            Assert.True(string.Compare(list[0].ErrorMessage, list[1].ErrorMessage, StringComparison.Ordinal) <= 0);
        }

        [Fact]
        public async Task GetBatchJobsHistoryAsync_WithSortByErrorMessage_Descending()
        {
            // Arrange
            var job    = BuildJob(1, DefaultJobName);
            var status = BuildStatus(10, 1, "initiated");

            var q1 = new BatchJobQueue
            {
                JobqueueId = Guid.NewGuid(), JobExecutionId = Guid.NewGuid(),
                JobId = 1, StatusId = 10, RequestedBy = DefaultUserEmail,
                StartDateTime = DateTime.UtcNow, ErrorMessage = "Alpha error",
                FpsYear = DefaultFpsYear
            };
            var q2 = new BatchJobQueue
            {
                JobqueueId = Guid.NewGuid(), JobExecutionId = Guid.NewGuid(),
                JobId = 1, StatusId = 10, RequestedBy = DefaultUserEmail,
                StartDateTime = DateTime.UtcNow.AddHours(1), ErrorMessage = "Zeta error",
                FpsYear = DefaultFpsYear
            };

            var (repo, _, _, _) = CreateRepository(jobs: [job], queues: [q1, q2], statuses: [status]);

            // Act
            var result = await repo.GetBatchJobsHistoryAsync(
                BuildQuery(sortBy: "errormessage", descending: true), DefaultJobName);

            // Assert — alphabetically last ErrorMessage appears first
            var list = result.Data.ToList();
            Assert.Equal(2, list.Count);
            Assert.True(string.Compare(list[0].ErrorMessage, list[1].ErrorMessage, StringComparison.Ordinal) >= 0);
        }

        [Fact]
        public async Task GetBatchJobsHistoryAsync_WithSortByStatus_Ascending()
        {
            // Arrange — two records with the same job but different statuses
            var job            = BuildJob(1, DefaultJobName);
            var initiatedStatus = BuildStatus(10, 1, "initiated");
            var completedStatus = BuildStatus(20, 1, "completed");
            var q1 = BuildQueue(1, 20, startDateTime: new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            var q2 = BuildQueue(1, 10, startDateTime: new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc));

            var (repo, _, _, _) = CreateRepository(
                jobs: [job], queues: [q1, q2], statuses: [initiatedStatus, completedStatus]);

            // Act
            var result = await repo.GetBatchJobsHistoryAsync(
                BuildQuery(sortBy: "status", descending: false), DefaultJobName);

            // Assert — "completed" < "initiated" alphabetically
            var list = result.Data.ToList();
            Assert.Equal(2, list.Count);
            Assert.True(string.Compare(list[0].Status, list[1].Status, StringComparison.Ordinal) <= 0);
        }

        [Fact]
        public async Task GetBatchJobsHistoryAsync_WithSortByStatus_Descending()
        {
            // Arrange
            var job             = BuildJob(1, DefaultJobName);
            var initiatedStatus = BuildStatus(10, 1, "initiated");
            var completedStatus = BuildStatus(20, 1, "completed");
            var q1 = BuildQueue(1, 10, startDateTime: new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            var q2 = BuildQueue(1, 20, startDateTime: new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc));

            var (repo, _, _, _) = CreateRepository(
                jobs: [job], queues: [q1, q2], statuses: [initiatedStatus, completedStatus]);

            // Act
            var result = await repo.GetBatchJobsHistoryAsync(
                BuildQuery(sortBy: "status", descending: true), DefaultJobName);

            // Assert — "initiated" > "completed" alphabetically
            var list = result.Data.ToList();
            Assert.Equal(2, list.Count);
            Assert.True(string.Compare(list[0].Status, list[1].Status, StringComparison.Ordinal) >= 0);
        }

        #endregion

        #region CanInitiateYearEndDataSetupRequestAsync

        [Fact(Skip = "select jq.JobqueueId projects Guid (value type); TestAsyncEnumerable<T> requires T : class — covered by integration tests.")]
        public async Task CanInitiateYearEndDataSetupRequestAsync_ReturnsTrue_WhenNoRecordsExist()
        {
            // Arrange — no records at all; no non-terminal record → can initiate
            var (repo, _, _, _) = CreateRepository();

            // Act
            var result = await repo.CanInitiateYearEndDataSetupRequestAsync(DefaultJobName);

            // Assert
            Assert.True(result);
        }

        [Fact(Skip = "select jq.JobqueueId projects Guid (value type); TestAsyncEnumerable<T> requires T : class — covered by integration tests.")]
        public async Task CanInitiateYearEndDataSetupRequestAsync_ReturnsTrue_WhenAllRecordsAreRejected()
        {
            // Arrange
            var (job, queue, status) = BuildJoinSeed(statusText: "rejected");
            var (repo, _, _, _) = CreateRepository(jobs: [job], queues: [queue], statuses: [status]);

            // Act
            var result = await repo.CanInitiateYearEndDataSetupRequestAsync(DefaultJobName);

            // Assert
            Assert.True(result);
        }

        [Fact(Skip = "select jq.JobqueueId projects Guid (value type); TestAsyncEnumerable<T> requires T : class — covered by integration tests.")]
        public async Task CanInitiateYearEndDataSetupRequestAsync_ReturnsTrue_WhenAllRecordsAreFailed()
        {
            // Arrange
            var (job, queue, status) = BuildJoinSeed(statusText: "failed");
            var (repo, _, _, _) = CreateRepository(jobs: [job], queues: [queue], statuses: [status]);

            // Act
            var result = await repo.CanInitiateYearEndDataSetupRequestAsync(DefaultJobName);

            // Assert
            Assert.True(result);
        }

        [Fact(Skip = "select jq.JobqueueId projects Guid (value type); TestAsyncEnumerable<T> requires T : class — covered by integration tests.")]
        public async Task CanInitiateYearEndDataSetupRequestAsync_ReturnsTrue_WhenAllRecordsAreCancelled()
        {
            // Arrange
            var (job, queue, status) = BuildJoinSeed(statusText: "cancelled");
            var (repo, _, _, _) = CreateRepository(jobs: [job], queues: [queue], statuses: [status]);

            // Act
            var result = await repo.CanInitiateYearEndDataSetupRequestAsync(DefaultJobName);

            // Assert
            Assert.True(result);
        }

        [Fact(Skip = "select jq.JobqueueId projects Guid (value type); TestAsyncEnumerable<T> requires T : class — covered by integration tests.")]
        public async Task CanInitiateYearEndDataSetupRequestAsync_ReturnsFalse_WhenNonTerminalRecordExists()
        {
            // Arrange — "initiated" is a non-terminal status
            var (job, queue, status) = BuildJoinSeed(statusText: "initiated");
            var (repo, _, _, _) = CreateRepository(jobs: [job], queues: [queue], statuses: [status]);

            // Act
            var result = await repo.CanInitiateYearEndDataSetupRequestAsync(DefaultJobName);

            // Assert
            Assert.False(result);
        }

        [Fact(Skip = "select jq.JobqueueId projects Guid (value type); TestAsyncEnumerable<T> requires T : class — covered by integration tests.")]
        public async Task CanInitiateYearEndDataSetupRequestAsync_ReturnsFalse_WhenApprovedRecordExists()
        {
            // Arrange — "approved" is non-terminal
            var (job, queue, status) = BuildJoinSeed(statusText: "approved");
            var (repo, _, _, _) = CreateRepository(jobs: [job], queues: [queue], statuses: [status]);

            // Act
            var result = await repo.CanInitiateYearEndDataSetupRequestAsync(DefaultJobName);

            // Assert
            Assert.False(result);
        }

        [Fact(Skip = "select jq.JobqueueId projects Guid (value type); TestAsyncEnumerable<T> requires T : class — covered by integration tests.")]
        public async Task CanInitiateYearEndDataSetupRequestAsync_ReturnsFalse_WhenMixedTerminalAndNonTerminal()
        {
            // Arrange — one terminal (failed) + one non-terminal (initiated)
            var job = BuildJob(1, DefaultJobName);
            var failedStatus    = BuildStatus(10, 1, "failed");
            var initiatedStatus = BuildStatus(20, 1, "initiated");
            var queueFailed     = BuildQueue(1, 10);
            var queueInitiated  = BuildQueue(1, 20);

            var (repo, _, _, _) = CreateRepository(
                jobs:     [job],
                queues:   [queueFailed, queueInitiated],
                statuses: [failedStatus, initiatedStatus]);

            // Act
            var result = await repo.CanInitiateYearEndDataSetupRequestAsync(DefaultJobName);

            // Assert
            Assert.False(result);
        }

        [Fact(Skip = "select jq.JobqueueId projects Guid (value type); TestAsyncEnumerable<T> requires T : class — covered by integration tests.")]
        public async Task CanInitiateYearEndDataSetupRequestAsync_ReturnsTrue_WhenJobNameDoesNotMatch()
        {
            // Arrange — record exists but for a different job; looks like no record → can initiate
            var (job, queue, status) = BuildJoinSeed(jobName: "OtherJob", statusText: "initiated");
            var (repo, _, _, _) = CreateRepository(jobs: [job], queues: [queue], statuses: [status]);

            // Act
            var result = await repo.CanInitiateYearEndDataSetupRequestAsync(DefaultJobName);

            // Assert
            Assert.True(result);
        }

        [Fact(Skip = "select jq.JobqueueId projects Guid (value type); TestAsyncEnumerable<T> requires T : class — covered by integration tests.")]
        public async Task CanInitiateYearEndDataSetupRequestAsync_IsCaseInsensitive_ForJobName()
        {
            // Arrange — job stored as uppercase; status is non-terminal
            var (job, queue, status) = BuildJoinSeed(jobName: "YEARENDSETUP", statusText: "initiated");
            var (repo, _, _, _) = CreateRepository(jobs: [job], queues: [queue], statuses: [status]);

            // Act — query with lowercase version of the name
            var result = await repo.CanInitiateYearEndDataSetupRequestAsync("yearendsetup");

            // Assert — non-terminal record found → cannot initiate
            Assert.False(result);
        }

        #endregion
            
        #region CanApproveOrRejectYearEndDataSetupRequestAsync

        [Fact(Skip = "select jq.JobqueueId projects Guid (value type); TestAsyncEnumerable<T> requires T : class — covered by integration tests.")]
        public async Task CanApproveOrRejYearEndDataSetupRequestAsync_ReturnsFalse_WhenNoRecordsExist()
        {
            // Arrange
            var (repo, _, _, _) = CreateRepository();

            // Act
            var result = await repo.CanApproveOrRejectYearEndDataSetupRequestAsync(DefaultJobName);

            // Assert
            Assert.False(result);
        }

        [Fact(Skip = "select jq.JobqueueId projects Guid (value type); TestAsyncEnumerable<T> requires T : class — covered by integration tests.")]
        public async Task CanApproveOrRejectYearEndDataSetupRequestAsync_ReturnsTrue_WhenInitiatedRecordExists()
        {
            // Arrange
            var (job, queue, status) = BuildJoinSeed(statusText: "initiated");
            var (repo, _, _, _) = CreateRepository(jobs: [job], queues: [queue], statuses: [status]);

            // Act
            var result = await repo.CanApproveOrRejectYearEndDataSetupRequestAsync(DefaultJobName);

            // Assert
            Assert.True(result);
        }

        [Fact(Skip = "select jq.JobqueueId projects Guid (value type); TestAsyncEnumerable<T> requires T : class — covered by integration tests.")]
        public async Task CanApproveOrRejectYearEndDataSetupRequestAsync_ReturnsFalse_WhenNoInitiatedRecord()
        {
            // Arrange — only a completed record; no "initiated"
            var (job, queue, status) = BuildJoinSeed(statusText: "completed");
            var (repo, _, _, _) = CreateRepository(jobs: [job], queues: [queue], statuses: [status]);

            // Act
            var result = await repo.CanApproveOrRejectYearEndDataSetupRequestAsync(DefaultJobName);

            // Assert
            Assert.False(result);
        }

        [Fact(Skip = "select jq.JobqueueId projects Guid (value type); TestAsyncEnumerable<T> requires T : class — covered by integration tests.")]
        public async Task CanApproveOrRejectYearEndDataSetupRequestAsync_ReturnsFalse_WhenJobNameDoesNotMatch()
        {
            // Arrange — "initiated" record exists but for a different job
            var (job, queue, status) = BuildJoinSeed(jobName: "OtherJob", statusText: "initiated");
            var (repo, _, _, _) = CreateRepository(jobs: [job], queues: [queue], statuses: [status]);

            // Act
            var result = await repo.CanApproveOrRejectYearEndDataSetupRequestAsync(DefaultJobName);

            // Assert
            Assert.False(result);
        }

        [Fact(Skip = "select jq.JobqueueId projects Guid (value type); TestAsyncEnumerable<T> requires T : class — covered by integration tests.")]
        public async Task CanApproveOrRejectYearEndDataSetupRequestAsync_IsCaseInsensitive_ForJobName()
        {
            // Arrange — job stored in mixed case
            var (job, queue, status) = BuildJoinSeed(jobName: "YEARENDSETUP", statusText: "initiated");
            var (repo, _, _, _) = CreateRepository(jobs: [job], queues: [queue], statuses: [status]);

            // Act
            var result = await repo.CanApproveOrRejectYearEndDataSetupRequestAsync("yearendsetup");

            // Assert
            Assert.True(result);
        }

        [Fact(Skip = "select jq.JobqueueId projects Guid (value type); TestAsyncEnumerable<T> requires T : class — covered by integration tests.")]
        public async Task CanApproveOrRejectYearEndDataSetupRequestAsync_ReturnsFalse_WhenOnlyTerminalRecordsExist()
        {
            // Arrange — records exist but none with "initiated" status
            var job             = BuildJob(1, DefaultJobName);
            var rejectedStatus  = BuildStatus(10, 1, "rejected");
            var failedStatus    = BuildStatus(20, 1, "failed");
            var queueRejected   = BuildQueue(1, 10);
            var queueFailed     = BuildQueue(1, 20);

            var (repo, _, _, _) = CreateRepository(
                jobs:     [job],
                queues:   [queueRejected, queueFailed],
                statuses: [rejectedStatus, failedStatus]);

            // Act
            var result = await repo.CanApproveOrRejectYearEndDataSetupRequestAsync(DefaultJobName);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region GetYearEndDataSetupRequestInitiatorAsync

        [Fact]
        public async Task GetYearEndDataSetupRequestInitiatorAsync_ReturnsInitiator_WhenInitiatedRecordExists()
        {
            // Arrange
            const string expectedInitiator = "initiator@example.com";
            var (job, queue, status) = BuildJoinSeed(statusText: "initiated", requestedBy: expectedInitiator);
            var (repo, _, _, _) = CreateRepository(jobs: [job], queues: [queue], statuses: [status]);

            // Act
            var result = await repo.GetYearEndDataSetupRequestInitiatorAsync(DefaultJobName);

            // Assert
            Assert.Equal(expectedInitiator, result);
        }

        [Fact]
        public async Task GetYearEndDataSetupRequestInitiatorAsync_ReturnsEmptyString_WhenNoRecordsExist()
        {
            // Arrange
            var (repo, _, _, _) = CreateRepository();

            // Act
            var result = await repo.GetYearEndDataSetupRequestInitiatorAsync(DefaultJobName);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public async Task GetYearEndDataSetupRequestInitiatorAsync_ReturnsEmptyString_WhenNoInitiatedRecord()
        {
            // Arrange — record exists but is not "initiated"
            var (job, queue, status) = BuildJoinSeed(statusText: "completed");
            var (repo, _, _, _) = CreateRepository(jobs: [job], queues: [queue], statuses: [status]);

            // Act
            var result = await repo.GetYearEndDataSetupRequestInitiatorAsync(DefaultJobName);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public async Task GetYearEndDataSetupRequestInitiatorAsync_ReturnsEmptyString_WhenJobNameDoesNotMatch()
        {
            // Arrange
            var (job, queue, status) = BuildJoinSeed(jobName: "OtherJob", statusText: "initiated");
            var (repo, _, _, _) = CreateRepository(jobs: [job], queues: [queue], statuses: [status]);

            // Act
            var result = await repo.GetYearEndDataSetupRequestInitiatorAsync(DefaultJobName);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public async Task GetYearEndDataSetupRequestInitiatorAsync_IsCaseInsensitive_ForJobName()
        {
            // Arrange — stored as uppercase
            var (job, queue, status) = BuildJoinSeed(
                jobName: "YEARENDSETUP",
                statusText: "initiated",
                requestedBy: DefaultUserEmail);
            var (repo, _, _, _) = CreateRepository(jobs: [job], queues: [queue], statuses: [status]);

            // Act
            var result = await repo.GetYearEndDataSetupRequestInitiatorAsync("yearendsetup");

            // Assert
            Assert.Equal(DefaultUserEmail, result);
        }

        #endregion

        #region EnqueueDataSetupInitiationBatchJobAsync

        [Fact]
        public async Task EnqueueDataSetupInitiationBatchJobAsync_ThrowsKeyNotFoundException_WhenJobNotFound()
        {
            // Arrange — no batch jobs in the DbSet
            var (repo, _, _, _) = CreateRepository(
                jobs:     [],
                queues:   [],
                statuses: []);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => repo.EnqueueDataSetupInitiationBatchJobAsync(
                    DefaultJobName, DefaultUserEmail, Guid.NewGuid().ToString(), "note", 2026));
        }

        [Fact]
        public async Task EnqueueDataSetupInitiationBatchJobAsync_ThrowsKeyNotFoundException_WhenStatusNotFound()
        {
            // Arrange — job exists but no "initiated" status for it
            var job = BuildJob(1, DefaultJobName);
            var (repo, _, _, _) = CreateRepository(
                jobs:     [job],
                queues:   [],
                statuses: []);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => repo.EnqueueDataSetupInitiationBatchJobAsync(
                    DefaultJobName, DefaultUserEmail, Guid.NewGuid().ToString(), "note", 2026));
        }

        [Fact]
        public async Task EnqueueDataSetupInitiationBatchJobAsync_AddsQueueEntryAndLog_OnSuccess()
        {
            // Arrange
            var job    = BuildJob(1, DefaultJobName);
            var status = BuildStatus(10, 1, "initiated");
            var (repo, _, queueSet, logSet) = CreateRepository(
                jobs: [job], queues: [], statuses: [status]);

            // Act
            await repo.EnqueueDataSetupInitiationBatchJobAsync(
                DefaultJobName, DefaultUserEmail, Guid.NewGuid().ToString(), "init note", 2026);

            // Assert
            queueSet.Verify(x => x.Add(It.IsAny<BatchJobQueue>()), Times.Once);
            logSet.Verify(x => x.Add(It.IsAny<BatchJobQueueLog>()), Times.Once);
        }

        [Fact]
        public async Task EnqueueDataSetupInitiationBatchJobAsync_CallsSaveChangesAsync_OnSuccess()
        {
            // Arrange
            var job    = BuildJob(1, DefaultJobName);
            var status = BuildStatus(10, 1, "initiated");
            var (repo, mockContext, _, _) = CreateRepository(
                jobs: [job], queues: [], statuses: [status]);

            // Act
            await repo.EnqueueDataSetupInitiationBatchJobAsync(
                DefaultJobName, DefaultUserEmail, Guid.NewGuid().ToString(), "note", 2026);

            // Assert
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 1);
        }

        [Fact]
        public async Task EnqueueDataSetupInitiationBatchJobAsync_ReturnsQueueEntry_WithCorrectFields()
        {
            // Arrange
            var job    = BuildJob(1, DefaultJobName);
            var status = BuildStatus(10, 1, "initiated");
            var correlationId = Guid.NewGuid().ToString();

            var (repo, _, _, _) = CreateRepository(
                jobs: [job], queues: [], statuses: [status]);

            // Act
            var result = await repo.EnqueueDataSetupInitiationBatchJobAsync(
                DefaultJobName, DefaultUserEmail, correlationId, "test note", 2026);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1,               result.JobId);
            Assert.Equal(10,              result.StatusId);
            Assert.Equal(DefaultUserEmail, result.RequestedBy);
            Assert.Equal(Guid.Parse(correlationId), result.JobExecutionId);
            Assert.Equal("test note",     result.ErrorMessage);
            Assert.Equal(2026,            result.TargetFpsYear);
        }

        [Fact]
        public async Task EnqueueDataSetupInitiationBatchJobAsync_UsesRequestContextFpsYear()
        {
            // Arrange
            const int expectedYear = 2025;
            var job    = BuildJob(1, DefaultJobName);
            var status = BuildStatus(10, 1, "initiated");
            var (repo, _, _, _) = CreateRepository(
                jobs: [job], queues: [], statuses: [status], fpsYear: expectedYear);

            // Act
            var result = await repo.EnqueueDataSetupInitiationBatchJobAsync(
                DefaultJobName, DefaultUserEmail, Guid.NewGuid().ToString(), "note", 2026);

            // Assert — fpsyear stays the ambient current/Open year, unrelated to the
            // targetFpsYear argument (planned-year staging design, 2026-09-03).
            Assert.Equal(expectedYear, result.FpsYear);
        }

        [Fact]
        public async Task EnqueueDataSetupInitiationBatchJobAsync_PersistsTargetFpsYear()
        {
            // Arrange
            const int expectedTargetYear = 2027;
            var job    = BuildJob(1, DefaultJobName);
            var status = BuildStatus(10, 1, "initiated");
            var (repo, _, _, _) = CreateRepository(
                jobs: [job], queues: [], statuses: [status]);

            // Act
            var result = await repo.EnqueueDataSetupInitiationBatchJobAsync(
                DefaultJobName, DefaultUserEmail, Guid.NewGuid().ToString(), "note", expectedTargetYear);

            // Assert
            Assert.Equal(expectedTargetYear, result.TargetFpsYear);
        }

        [Fact]
        public async Task EnqueueDataSetupInitiationBatchJobAsync_GeneratesNewGuid_WhenCorrelationIdIsEmpty()
        {
            // Arrange
            var job    = BuildJob(1, DefaultJobName);
            var status = BuildStatus(10, 1, "initiated");
            var (repo, _, _, _) = CreateRepository(
                jobs: [job], queues: [], statuses: [status]);

            // Act
            var result = await repo.EnqueueDataSetupInitiationBatchJobAsync(
                DefaultJobName, DefaultUserEmail, string.Empty, "note", 2026);

            // Assert — a non-empty Guid was generated rather than Guid.Empty
            Assert.NotEqual(Guid.Empty, result.JobExecutionId);
        }

        [Fact]
        public async Task EnqueueDataSetupInitiationBatchJobAsync_IsJobNameCaseInsensitive()
        {
            // Arrange — job stored as "YEARENDSETUP", looked up as "yearendsetup"
            var job    = BuildJob(1, "YEARENDSETUP");
            var status = BuildStatus(10, 1, "initiated");
            var (repo, _, _, _) = CreateRepository(
                jobs: [job], queues: [], statuses: [status]);

            // Act
            var result = await repo.EnqueueDataSetupInitiationBatchJobAsync(
                "yearendsetup", DefaultUserEmail, Guid.NewGuid().ToString(), "note", 2026);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task EnqueueDataSetupInitiationBatchJobAsync_SaveFails_RollsBackTransaction()
        {
            // Arrange — SaveChangesAsync throws to simulate a DB error
            var job    = BuildJob(1, DefaultJobName);
            var status = BuildStatus(10, 1, "initiated");

            var requestCtx = Substitute.For<IFpsRequestContext>();
            requestCtx.FpsYear.Returns(DefaultFpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestCtx);
            mockContext.Setup(x => x.BatchJobs).Returns(RepositoryTestHelper.CreateMockDbSet<BatchJobMaster>([job]).Object);
            mockContext.Setup(x => x.BatchJobStatuses).Returns(RepositoryTestHelper.CreateMockDbSet<BatchJobStatus>([status]).Object);

            var queueSet = RepositoryTestHelper.CreateMockDbSet<BatchJobQueue>([]);
            RepositoryTestHelper.SetupDbSetOperations(queueSet);
            mockContext.Setup(x => x.BatchJobQueues).Returns(queueSet.Object);

            var logSet = RepositoryTestHelper.CreateMockDbSet<BatchJobQueueLog>([]);
            RepositoryTestHelper.SetupDbSetOperations(logSet);
            mockContext.Setup(x => x.BatchJobQueueLogs).Returns(logSet.Object);

            mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                       .ThrowsAsync(new InvalidOperationException("DB save failed"));

            var repo = new YearEndRepository(mockContext.Object, requestCtx, Substitute.For<IYearEndStagingRepository>());

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => repo.EnqueueDataSetupInitiationBatchJobAsync(
                    DefaultJobName, DefaultUserEmail, Guid.NewGuid().ToString(), "note", 2026));
        }

        #endregion

        #region EnqueueDataSetupApprovalBatchJobAsync

        [Fact]
        public async Task EnqueueDataSetupApprovalBatchJobAsync_ThrowsKeyNotFoundException_WhenNoInitiatedQueueEntry()
        {
            // Arrange — no records exist at all → no matching request found for this jobQueueId
            var (repo, _, _, _) = CreateRepository();

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => repo.EnqueueDataSetupApprovalBatchJobAsync(
                    Guid.NewGuid(), DefaultUserEmail, "note"));
        }

        [Fact]
        public async Task EnqueueDataSetupApprovalBatchJobAsync_ThrowsKeyNotFoundException_WhenRowExistsButNotInitiated()
        {
            // Arrange — a queue row exists for this exact jobQueueId, but its status is already
            // Approved, not Initiated — proves the query re-checks status at write time rather than
            // just trusting a row's mere presence.
            var queueId        = Guid.NewGuid();
            var approvedStatus = BuildStatus(20, 1, "approved");
            var existingQueue  = BuildQueue(1, 20, queueId);

            var (repo, _, _, _) = CreateRepository(
                statuses: [approvedStatus],
                queues:   [existingQueue]);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => repo.EnqueueDataSetupApprovalBatchJobAsync(
                    queueId, DefaultUserEmail, "note"));
        }

        [Fact]
        public async Task EnqueueDataSetupApprovalBatchJobAsync_ThrowsKeyNotFoundException_WhenApprovedStatusNotFound()
        {
            // Arrange — "initiated" queue entry found but no "approved" status defined for the job
            var queueId = Guid.NewGuid();
            var (job, queue, initiatedStatus) = BuildJoinSeed(
                statusText: "initiated", jobId: 1, statusId: 10, queueId: queueId);

            // No "approved" status — only "initiated" is defined
            var (repo, _, _, _) = CreateRepository(
                jobs:     [job],
                queues:   [queue],
                statuses: [initiatedStatus]);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => repo.EnqueueDataSetupApprovalBatchJobAsync(
                    queueId, DefaultUserEmail, "note"));
        }

        [Fact]
        public async Task EnqueueDataSetupApprovalBatchJobAsync_UpdatesQueueEntryStatus_OnSuccess()
        {
            // Arrange
            var queueId         = Guid.NewGuid();
            var initiatedStatus = BuildStatus(10, 1, "initiated");
            var approvedStatus  = BuildStatus(20, 1, "approved");
            var existingQueue   = BuildQueue(1, 10, queueId);

            var (repo, _, queueSet, _) = CreateRepository(
                queues:   [existingQueue],
                statuses: [initiatedStatus, approvedStatus]);

            // Act
            await repo.EnqueueDataSetupApprovalBatchJobAsync(
                queueId, DefaultUserEmail, "approve note");

            // Assert — the existing queue row was updated
            queueSet.Verify(x => x.Update(It.IsAny<BatchJobQueue>()), Times.Once);
        }

        [Fact]
        public async Task EnqueueDataSetupApprovalBatchJobAsync_AddsLogEntry_OnSuccess()
        {
            // Arrange
            var queueId         = Guid.NewGuid();
            var initiatedStatus = BuildStatus(10, 1, "initiated");
            var approvedStatus  = BuildStatus(20, 1, "approved");
            var existingQueue   = BuildQueue(1, 10, queueId);

            var (repo, _, _, logSet) = CreateRepository(
                queues:   [existingQueue],
                statuses: [initiatedStatus, approvedStatus]);

            // Act
            await repo.EnqueueDataSetupApprovalBatchJobAsync(
                queueId, DefaultUserEmail, "approve note");

            // Assert
            logSet.Verify(x => x.Add(It.IsAny<BatchJobQueueLog>()), Times.Once);
        }

        [Fact]
        public async Task EnqueueDataSetupApprovalBatchJobAsync_CallsSaveChangesAsync_OnSuccess()
        {
            // Arrange
            var queueId         = Guid.NewGuid();
            var initiatedStatus = BuildStatus(10, 1, "initiated");
            var approvedStatus  = BuildStatus(20, 1, "approved");
            var existingQueue   = BuildQueue(1, 10, queueId);

            var (repo, mockContext, _, _) = CreateRepository(
                queues:   [existingQueue],
                statuses: [initiatedStatus, approvedStatus]);

            // Act
            await repo.EnqueueDataSetupApprovalBatchJobAsync(
                queueId, DefaultUserEmail, "note");

            // Assert
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 1);
        }

        [Fact]
        public async Task EnqueueDataSetupApprovalBatchJobAsync_ReturnsUpdatedQueueRow_WithApprovedStatus()
        {
            // Arrange
            var queueId         = Guid.NewGuid();
            var initiatedStatus = BuildStatus(10, 1, "initiated");
            var approvedStatus  = BuildStatus(20, 1, "approved");
            var existingQueue   = BuildQueue(1, 10, queueId);

            var (repo, _, _, _) = CreateRepository(
                queues:   [existingQueue],
                statuses: [initiatedStatus, approvedStatus]);

            // Act
            var result = await repo.EnqueueDataSetupApprovalBatchJobAsync(
                queueId, DefaultUserEmail, "approve note");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(20,              result.StatusId);
            Assert.Equal(DefaultUserEmail, result.RequestedBy);
            Assert.Equal("approve note",  result.ErrorMessage);

            // Assert — approval audit columns are now populated; previously never written at all.
            Assert.Equal(DefaultUserEmail, result.ApprovedBy);
            Assert.NotNull(result.ApprovedAtUtc);
            Assert.Null(result.RejectedBy);
            Assert.Null(result.RejectedAtUtc);
        }

        [Fact]
        public async Task EnqueueDataSetupApprovalBatchJobAsync_DoesNotDeleteStaging_OnSuccess()
        {
            // Arrange — Approve retains staging (immutable, not deleted) so Workstream 7's Worker can
            // consume it; only Reject deletes it.
            var queueId         = Guid.NewGuid();
            var initiatedStatus = BuildStatus(10, 1, "initiated");
            var approvedStatus  = BuildStatus(20, 1, "approved");
            var existingQueue   = BuildQueue(1, 10, queueId);
            var stagingRepo     = Substitute.For<IYearEndStagingRepository>();

            var (repo, _, _, _) = CreateRepository(
                queues:   [existingQueue],
                statuses: [initiatedStatus, approvedStatus],
                stagingRepository: stagingRepo);

            // Act
            await repo.EnqueueDataSetupApprovalBatchJobAsync(
                queueId, DefaultUserEmail, "approve note");

            // Assert
            await stagingRepo.DidNotReceive().DeleteStagingAsync(Arg.Any<Guid>());
        }

        [Fact]
        public async Task EnqueueDataSetupApprovalBatchJobAsync_SaveFails_RollsBackTransaction()
        {
            // Arrange — SaveChangesAsync throws to simulate a DB error
            var queueId         = Guid.NewGuid();
            var initiatedStatus = BuildStatus(10, 1, "initiated");
            var approvedStatus  = BuildStatus(20, 1, "approved");
            var existingQueue   = BuildQueue(1, 10, queueId);

            var requestCtx = Substitute.For<IFpsRequestContext>();
            requestCtx.FpsYear.Returns(DefaultFpsYear);
            var stagingRepo = Substitute.For<IYearEndStagingRepository>();

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestCtx);

            mockContext.Setup(x => x.BatchJobStatuses)
                .Returns(RepositoryTestHelper.CreateMockDbSet<BatchJobStatus>([initiatedStatus, approvedStatus]).Object);

            var queueSet = RepositoryTestHelper.CreateMockDbSet<BatchJobQueue>([existingQueue]);
            RepositoryTestHelper.SetupDbSetOperations(queueSet);
            mockContext.Setup(x => x.BatchJobQueues).Returns(queueSet.Object);

            var logSet = RepositoryTestHelper.CreateMockDbSet<BatchJobQueueLog>([]);
            RepositoryTestHelper.SetupDbSetOperations(logSet);
            mockContext.Setup(x => x.BatchJobQueueLogs).Returns(logSet.Object);

            mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                       .ThrowsAsync(new InvalidOperationException("DB save failed"));

            var repo = new YearEndRepository(mockContext.Object, requestCtx, stagingRepo);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => repo.EnqueueDataSetupApprovalBatchJobAsync(
                    queueId, DefaultUserEmail, "note"));
        }

        #endregion

        #region EnqueueDataSetupRejectBatchJobAsync

        [Fact]
        public async Task EnqueueDataSetupRejectBatchJobAsync_ThrowsKeyNotFoundException_WhenNoInitiatedQueueEntry()
        {
            // Arrange — no records exist → no matching request found for this jobQueueId
            var (repo, _, _, _) = CreateRepository();

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => repo.EnqueueDataSetupRejectBatchJobAsync(
                    Guid.NewGuid(), DefaultUserEmail, "note"));
        }

        [Fact]
        public async Task EnqueueDataSetupRejectBatchJobAsync_ThrowsKeyNotFoundException_WhenRowExistsButNotInitiated()
        {
            // Arrange — a queue row exists for this exact jobQueueId, but its status is already
            // Rejected, not Initiated — proves the query re-checks status at write time.
            var queueId        = Guid.NewGuid();
            var rejectedStatus = BuildStatus(30, 1, "rejected");
            var existingQueue  = BuildQueue(1, 30, queueId);

            var (repo, _, _, _) = CreateRepository(
                statuses: [rejectedStatus],
                queues:   [existingQueue]);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => repo.EnqueueDataSetupRejectBatchJobAsync(
                    queueId, DefaultUserEmail, "note"));
        }

        [Fact]
        public async Task EnqueueDataSetupRejectBatchJobAsync_ThrowsKeyNotFoundException_WhenRejectedStatusNotFound()
        {
            // Arrange — "initiated" queue entry found but no "rejected" status defined for the job
            var queueId = Guid.NewGuid();
            var (job, queue, initiatedStatus) = BuildJoinSeed(
                statusText: "initiated", jobId: 1, statusId: 10, queueId: queueId);

            // No "rejected" status — only "initiated" is defined
            var (repo, _, _, _) = CreateRepository(
                jobs:     [job],
                queues:   [queue],
                statuses: [initiatedStatus]);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => repo.EnqueueDataSetupRejectBatchJobAsync(
                    queueId, DefaultUserEmail, "note"));
        }

        [Fact]
        public async Task EnqueueDataSetupRejectBatchJobAsync_UpdatesQueueEntryStatus_OnSuccess()
        {
            // Arrange
            var queueId         = Guid.NewGuid();
            var initiatedStatus = BuildStatus(10, 1, "initiated");
            var rejectedStatus  = BuildStatus(30, 1, "rejected");
            var existingQueue   = BuildQueue(1, 10, queueId);

            var (repo, _, queueSet, _) = CreateRepository(
                queues:   [existingQueue],
                statuses: [initiatedStatus, rejectedStatus]);

            // Act
            await repo.EnqueueDataSetupRejectBatchJobAsync(
                queueId, DefaultUserEmail, "reject note");

            // Assert — the existing queue row was updated
            queueSet.Verify(x => x.Update(It.IsAny<BatchJobQueue>()), Times.Once);
        }

        [Fact]
        public async Task EnqueueDataSetupRejectBatchJobAsync_AddsLogEntry_OnSuccess()
        {
            // Arrange
            var queueId         = Guid.NewGuid();
            var initiatedStatus = BuildStatus(10, 1, "initiated");
            var rejectedStatus  = BuildStatus(30, 1, "rejected");
            var existingQueue   = BuildQueue(1, 10, queueId);

            var (repo, _, _, logSet) = CreateRepository(
                queues:   [existingQueue],
                statuses: [initiatedStatus, rejectedStatus]);

            // Act
            await repo.EnqueueDataSetupRejectBatchJobAsync(
                queueId, DefaultUserEmail, "reject note");

            // Assert
            logSet.Verify(x => x.Add(It.IsAny<BatchJobQueueLog>()), Times.Once);
        }

        [Fact]
        public async Task EnqueueDataSetupRejectBatchJobAsync_CallsSaveChangesAsync_OnSuccess()
        {
            // Arrange
            var queueId         = Guid.NewGuid();
            var initiatedStatus = BuildStatus(10, 1, "initiated");
            var rejectedStatus  = BuildStatus(30, 1, "rejected");
            var existingQueue   = BuildQueue(1, 10, queueId);

            var (repo, mockContext, _, _) = CreateRepository(
                queues:   [existingQueue],
                statuses: [initiatedStatus, rejectedStatus]);

            // Act
            await repo.EnqueueDataSetupRejectBatchJobAsync(
                queueId, DefaultUserEmail, "note");

            // Assert
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 1);
        }

        [Fact]
        public async Task EnqueueDataSetupRejectBatchJobAsync_ReturnsUpdatedQueueRow_WithRejectedStatus()
        {
            // Arrange
            var queueId         = Guid.NewGuid();
            var initiatedStatus = BuildStatus(10, 1, "initiated");
            var rejectedStatus  = BuildStatus(30, 1, "rejected");
            var existingQueue   = BuildQueue(1, 10, queueId);

            var (repo, _, _, _) = CreateRepository(
                queues:   [existingQueue],
                statuses: [initiatedStatus, rejectedStatus]);

            // Act
            var result = await repo.EnqueueDataSetupRejectBatchJobAsync(
                queueId, DefaultUserEmail, "reject note");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(30,               result.StatusId);
            Assert.Equal(DefaultUserEmail, result.RequestedBy);
            Assert.Equal("reject note",    result.ErrorMessage);

            // Assert — rejection audit columns are now populated; previously never written at all.
            Assert.Equal(DefaultUserEmail, result.RejectedBy);
            Assert.NotNull(result.RejectedAtUtc);
            Assert.Equal("reject note",    result.RejectionReason);
            Assert.Null(result.ApprovedBy);
            Assert.Null(result.ApprovedAtUtc);
        }

        [Fact]
        public async Task EnqueueDataSetupRejectBatchJobAsync_DeletesStaging_OnSuccess()
        {
            // Arrange — Reject deletes both staging sets for this jobQueueId, in the same transaction
            // as the status flip (Reject-only; Approve must never do this — see the counterpart test
            // in the Approval region above).
            var queueId         = Guid.NewGuid();
            var initiatedStatus = BuildStatus(10, 1, "initiated");
            var rejectedStatus  = BuildStatus(30, 1, "rejected");
            var existingQueue   = BuildQueue(1, 10, queueId);
            var stagingRepo     = Substitute.For<IYearEndStagingRepository>();

            var (repo, _, _, _) = CreateRepository(
                queues:   [existingQueue],
                statuses: [initiatedStatus, rejectedStatus],
                stagingRepository: stagingRepo);

            // Act
            await repo.EnqueueDataSetupRejectBatchJobAsync(
                queueId, DefaultUserEmail, "reject note");

            // Assert
            await stagingRepo.Received(1).DeleteStagingAsync(queueId);
        }

        [Fact]
        public async Task EnqueueDataSetupRejectBatchJobAsync_SaveFails_RollsBackTransaction()
        {
            // Arrange — SaveChangesAsync throws to simulate a DB error
            var queueId         = Guid.NewGuid();
            var initiatedStatus = BuildStatus(10, 1, "initiated");
            var rejectedStatus  = BuildStatus(30, 1, "rejected");
            var existingQueue   = BuildQueue(1, 10, queueId);

            var requestCtx = Substitute.For<IFpsRequestContext>();
            requestCtx.FpsYear.Returns(DefaultFpsYear);
            var stagingRepo = Substitute.For<IYearEndStagingRepository>();

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestCtx);

            mockContext.Setup(x => x.BatchJobStatuses)
                .Returns(RepositoryTestHelper.CreateMockDbSet<BatchJobStatus>([initiatedStatus, rejectedStatus]).Object);

            var queueSet = RepositoryTestHelper.CreateMockDbSet<BatchJobQueue>([existingQueue]);
            RepositoryTestHelper.SetupDbSetOperations(queueSet);
            mockContext.Setup(x => x.BatchJobQueues).Returns(queueSet.Object);

            var logSet = RepositoryTestHelper.CreateMockDbSet<BatchJobQueueLog>([]);
            RepositoryTestHelper.SetupDbSetOperations(logSet);
            mockContext.Setup(x => x.BatchJobQueueLogs).Returns(logSet.Object);

            mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                       .ThrowsAsync(new InvalidOperationException("DB save failed"));

            var repo = new YearEndRepository(mockContext.Object, requestCtx, stagingRepo);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => repo.EnqueueDataSetupRejectBatchJobAsync(
                    queueId, DefaultUserEmail, "note"));

            // Assert — staging is never touched once SaveChangesAsync (the status flip) has already
            // failed and thrown.
            await stagingRepo.DidNotReceive().DeleteStagingAsync(Arg.Any<Guid>());
        }

        #endregion

        #region CanInitiateYearEndCutOverRequestAsync

        [Fact(Skip = "select jq.JobqueueId projects Guid (value type); TestAsyncEnumerable<T> requires T : class — covered by integration tests.")]
        public async Task CanInitiateYearEndCutOverRequestAsync_WhenNoRecordsExist_ReturnsTrue()
        {
            // Arrange — no data at all
            var (repo, _, _, _) = CreateRepository();

            // Act
            var result = await repo.CanInitiateYearEndCutOverRequestAsync(DefaultJobName);

            // Assert
            Assert.True(result);
        }

        [Fact(Skip = "select jq.JobqueueId projects Guid (value type); TestAsyncEnumerable<T> requires T : class — covered by integration tests.")]
        public async Task CanInitiateYearEndCutOverRequestAsync_WhenAllRecordsTerminal_ReturnsTrue()
        {
            // Arrange — every record is in a terminal status
            var (job, queue, rejectedStatus) = BuildJoinSeed(statusText: "rejected", statusId: 30);
            var (repo, _, _, _) = CreateRepository(jobs: [job], queues: [queue], statuses: [rejectedStatus]);

            // Act
            var result = await repo.CanInitiateYearEndCutOverRequestAsync(DefaultJobName);

            // Assert
            Assert.True(result);
        }

        [Fact(Skip = "select jq.JobqueueId projects Guid (value type); TestAsyncEnumerable<T> requires T : class — covered by integration tests.")]
        public async Task CanInitiateYearEndCutOverRequestAsync_WhenNonTerminalRecordExists_ReturnsFalse()
        {
            // Arrange — an "initiated" record is non-terminal
            var (job, queue, status) = BuildJoinSeed(statusText: "initiated", statusId: 10);
            var (repo, _, _, _) = CreateRepository(jobs: [job], queues: [queue], statuses: [status]);

            // Act
            var result = await repo.CanInitiateYearEndCutOverRequestAsync(DefaultJobName);

            // Assert
            Assert.False(result);
        }

        [Fact(Skip = "select jq.JobqueueId projects Guid (value type); TestAsyncEnumerable<T> requires T : class — covered by integration tests.")]
        public async Task CanInitiateYearEndCutOverRequestAsync_IsCaseInsensitiveOnJobName()
        {
            // Arrange — job stored upper, queried lower
            var (job, queue, status) = BuildJoinSeed(jobName: DefaultJobName.ToUpper(), statusText: "initiated", statusId: 10);
            var (repo, _, _, _) = CreateRepository(jobs: [job], queues: [queue], statuses: [status]);

            // Act
            var result = await repo.CanInitiateYearEndCutOverRequestAsync(DefaultJobName.ToLower());

            // Assert
            Assert.False(result);
        }

        #endregion

        #region CanApproveOrRejectYearEndCutOverRequestAsync

        [Fact(Skip = "select jq.JobqueueId projects Guid (value type); TestAsyncEnumerable<T> requires T : class — covered by integration tests.")]
        public async Task CanApproveOrRejectYearEndCutOverRequestAsync_WhenInitiatedRecordExists_ReturnsTrue()
        {
            // Arrange
            var (job, queue, status) = BuildJoinSeed(statusText: "initiated", statusId: 10);
            var (repo, _, _, _) = CreateRepository(jobs: [job], queues: [queue], statuses: [status]);

            // Act
            var result = await repo.CanApproveOrRejectYearEndCutOverRequestAsync(DefaultJobName);

            // Assert
            Assert.True(result);
        }

        [Fact(Skip = "select jq.JobqueueId projects Guid (value type); TestAsyncEnumerable<T> requires T : class — covered by integration tests.")]
        public async Task CanApproveOrRejectYearEndCutOverRequestAsync_WhenNoInitiatedRecord_ReturnsFalse()
        {
            // Arrange — only a rejected record exists
            var (job, queue, status) = BuildJoinSeed(statusText: "rejected", statusId: 30);
            var (repo, _, _, _) = CreateRepository(jobs: [job], queues: [queue], statuses: [status]);

            // Act
            var result = await repo.CanApproveOrRejectYearEndCutOverRequestAsync(DefaultJobName);

            // Assert
            Assert.False(result);
        }

        [Fact(Skip = "select jq.JobqueueId projects Guid (value type); TestAsyncEnumerable<T> requires T : class — covered by integration tests.")]
        public async Task CanApproveOrRejectYearEndCutOverRequestAsync_WhenNoRecordsExist_ReturnsFalse()
        {
            // Arrange
            var (repo, _, _, _) = CreateRepository();

            // Act
            var result = await repo.CanApproveOrRejectYearEndCutOverRequestAsync(DefaultJobName);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region GetYearEndCutOverRequestInitiatorAsync

        [Fact]
        public async Task GetYearEndCutOverRequestInitiatorAsync_WhenInitiatedRecordExists_ReturnsRequestedBy()
        {
            // Arrange
            const string expectedUser = "initiator@example.com";
            var (job, queue, status) = BuildJoinSeed(statusText: "initiated", statusId: 10, requestedBy: expectedUser);
            var (repo, _, _, _) = CreateRepository(jobs: [job], queues: [queue], statuses: [status]);

            // Act
            var result = await repo.GetYearEndCutOverRequestInitiatorAsync(DefaultJobName);

            // Assert
            Assert.Equal(expectedUser, result);
        }

        [Fact]
        public async Task GetYearEndCutOverRequestInitiatorAsync_WhenNoInitiatedRecord_ReturnsEmptyString()
        {
            // Arrange — no initiated record for this job
            var (repo, _, _, _) = CreateRepository();

            // Act
            var result = await repo.GetYearEndCutOverRequestInitiatorAsync(DefaultJobName);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public async Task GetYearEndCutOverRequestInitiatorAsync_WhenJobNameDoesNotMatch_ReturnsEmptyString()
        {
            // Arrange — job name differs
            var (job, queue, status) = BuildJoinSeed(jobName: "OtherJob", statusText: "initiated", statusId: 10);
            var (repo, _, _, _) = CreateRepository(jobs: [job], queues: [queue], statuses: [status]);

            // Act
            var result = await repo.GetYearEndCutOverRequestInitiatorAsync(DefaultJobName);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        #endregion

        #region EnqueueCutOverInitiationBatchJobAsync

        [Fact]
        public async Task EnqueueCutOverInitiationBatchJobAsync_WhenJobExists_ReturnsQueuedEntry()
        {
            // Arrange
            var job            = BuildJob(1, DefaultJobName);
            var initiatedStatus = BuildStatus(10, 1, "initiated");

            var (repo, _, _, _) = CreateRepository(jobs: [job], statuses: [initiatedStatus]);

            // Act
            var result = await repo.EnqueueCutOverInitiationBatchJobAsync(
                DefaultJobName, DefaultUserEmail, Guid.NewGuid().ToString(), "initiation note");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(DefaultUserEmail, result.RequestedBy);
            Assert.Equal("initiation note",  result.ErrorMessage);
        }

        [Fact]
        public async Task EnqueueCutOverInitiationBatchJobAsync_WhenJobNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange — empty seed; no job exists
            var (repo, _, _, _) = CreateRepository();

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => repo.EnqueueCutOverInitiationBatchJobAsync(
                    DefaultJobName, DefaultUserEmail, Guid.NewGuid().ToString(), "note"));
        }

        [Fact]
        public async Task EnqueueCutOverInitiationBatchJobAsync_WhenInitiatedStatusNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange — job exists but no "initiated" status row
            var job = BuildJob(1, DefaultJobName);
            var (repo, _, _, _) = CreateRepository(jobs: [job]);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => repo.EnqueueCutOverInitiationBatchJobAsync(
                    DefaultJobName, DefaultUserEmail, Guid.NewGuid().ToString(), "note"));
        }

        [Fact]
        public async Task EnqueueCutOverInitiationBatchJobAsync_AddsQueueEntry()
        {
            // Arrange
            var job             = BuildJob(1, DefaultJobName);
            var initiatedStatus = BuildStatus(10, 1, "initiated");

            var (repo, _, queueSet, _) = CreateRepository(jobs: [job], statuses: [initiatedStatus]);

            // Act
            await repo.EnqueueCutOverInitiationBatchJobAsync(
                DefaultJobName, DefaultUserEmail, Guid.NewGuid().ToString(), "note");

            // Assert
            queueSet.Verify(x => x.Add(It.IsAny<BatchJobQueue>()), Times.Once);
        }

        [Fact]
        public async Task EnqueueCutOverInitiationBatchJobAsync_AddsLogEntry()
        {
            // Arrange
            var job             = BuildJob(1, DefaultJobName);
            var initiatedStatus = BuildStatus(10, 1, "initiated");

            var (repo, _, _, logSet) = CreateRepository(jobs: [job], statuses: [initiatedStatus]);

            // Act
            await repo.EnqueueCutOverInitiationBatchJobAsync(
                DefaultJobName, DefaultUserEmail, Guid.NewGuid().ToString(), "note");

            // Assert
            logSet.Verify(x => x.Add(It.IsAny<BatchJobQueueLog>()), Times.Once);
        }

        [Fact]
        public async Task EnqueueCutOverInitiationBatchJobAsync_CallsSaveChangesAsync_OnSuccess()
        {
            // Arrange
            var job             = BuildJob(1, DefaultJobName);
            var initiatedStatus = BuildStatus(10, 1, "initiated");
            var (repo, mockContext, _, _) = CreateRepository(jobs: [job], statuses: [initiatedStatus]);

            // Act
            await repo.EnqueueCutOverInitiationBatchJobAsync(
                DefaultJobName, DefaultUserEmail, Guid.NewGuid().ToString(), "note");

            // Assert
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 1);
        }

        #endregion

        #region EnqueueCutOverApprovalBatchJobAsync

        [Fact]
        public async Task EnqueueCutOverApprovalBatchJobAsync_WhenInitiatedQueueExists_ReturnsApprovedEntry()
        {
            // Arrange
            var queueId         = Guid.NewGuid();
            var job             = BuildJob(1, CutOverJobName);
            var initiatedStatus = BuildStatus(10, 1, "initiated");
            var approvedStatus  = BuildStatus(20, 1, "approved");
            var existingQueue   = BuildQueue(1, 10, queueId);

            var (repo, _, _, _) = CreateRepository(
                jobs:     [job],
                queues:   [existingQueue],
                statuses: [initiatedStatus, approvedStatus]);

            // Act
            var result = await repo.EnqueueCutOverApprovalBatchJobAsync(
                existingQueue.JobExecutionId, "approver@example.com", "approval note");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(20,                    result.StatusId);
            Assert.Equal("approver@example.com", result.RequestedBy);
        }

        [Fact]
        public async Task EnqueueCutOverApprovalBatchJobAsync_WhenNoInitiatedQueueRow_ThrowsKeyNotFoundException()
        {
            // Arrange — no initiated queue row
            var job             = BuildJob(1, CutOverJobName);
            var approvedStatus  = BuildStatus(20, 1, "approved");

            var (repo, _, _, _) = CreateRepository(jobs: [job], statuses: [approvedStatus]);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => repo.EnqueueCutOverApprovalBatchJobAsync(
                    Guid.NewGuid(), DefaultUserEmail, "note"));
        }

        [Fact]
        public async Task EnqueueCutOverApprovalBatchJobAsync_WhenApprovedStatusNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange — queue row exists but no "approved" status row
            var queueId         = Guid.NewGuid();
            var job             = BuildJob(1, CutOverJobName);
            var initiatedStatus = BuildStatus(10, 1, "initiated");
            var existingQueue   = BuildQueue(1, 10, queueId);

            var (repo, _, _, _) = CreateRepository(
                jobs:     [job],
                queues:   [existingQueue],
                statuses: [initiatedStatus]);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => repo.EnqueueCutOverApprovalBatchJobAsync(
                    existingQueue.JobExecutionId, DefaultUserEmail, "note"));
        }

        [Fact]
        public async Task EnqueueCutOverApprovalBatchJobAsync_WhenJobExecutionIdBelongsToDifferentJob_ThrowsKeyNotFoundException()
        {
            // Arrange — row exists and is Initiated, but under a different job name entirely.
            var queueId         = Guid.NewGuid();
            var job             = BuildJob(1, "YearEnd-DataSetup");
            var initiatedStatus = BuildStatus(10, 1, "initiated");
            var approvedStatus  = BuildStatus(20, 1, "approved");
            var existingQueue   = BuildQueue(1, 10, queueId);

            var (repo, _, _, _) = CreateRepository(
                jobs:     [job],
                queues:   [existingQueue],
                statuses: [initiatedStatus, approvedStatus]);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => repo.EnqueueCutOverApprovalBatchJobAsync(
                    existingQueue.JobExecutionId, DefaultUserEmail, "note"));
        }

        [Fact]
        public async Task EnqueueCutOverApprovalBatchJobAsync_AddsLogEntry()
        {
            // Arrange
            var queueId         = Guid.NewGuid();
            var job             = BuildJob(1, CutOverJobName);
            var initiatedStatus = BuildStatus(10, 1, "initiated");
            var approvedStatus  = BuildStatus(20, 1, "approved");
            var existingQueue   = BuildQueue(1, 10, queueId);

            var (repo, _, _, logSet) = CreateRepository(
                jobs:     [job],
                queues:   [existingQueue],
                statuses: [initiatedStatus, approvedStatus]);

            // Act
            await repo.EnqueueCutOverApprovalBatchJobAsync(
                existingQueue.JobExecutionId, DefaultUserEmail, "note");

            // Assert
            logSet.Verify(x => x.Add(It.IsAny<BatchJobQueueLog>()), Times.Once);
        }

        [Fact]
        public async Task EnqueueCutOverApprovalBatchJobAsync_CallsSaveChangesAsync_OnSuccess()
        {
            // Arrange
            var queueId         = Guid.NewGuid();
            var job             = BuildJob(1, CutOverJobName);
            var initiatedStatus = BuildStatus(10, 1, "initiated");
            var approvedStatus  = BuildStatus(20, 1, "approved");
            var existingQueue   = BuildQueue(1, 10, queueId);

            var (repo, mockContext, _, _) = CreateRepository(
                jobs:     [job],
                queues:   [existingQueue],
                statuses: [initiatedStatus, approvedStatus]);

            // Act
            await repo.EnqueueCutOverApprovalBatchJobAsync(
                existingQueue.JobExecutionId, DefaultUserEmail, "note");

            // Assert
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 1);
        }

        #endregion

        #region EnqueueCutOverRejectBatchJobAsync

        [Fact]
        public async Task EnqueueCutOverRejectBatchJobAsync_WhenInitiatedQueueExists_ReturnsRejectedEntry()
        {
            // Arrange
            var queueId         = Guid.NewGuid();
            var job             = BuildJob(1, CutOverJobName);
            var initiatedStatus = BuildStatus(10, 1, "initiated");
            var rejectedStatus  = BuildStatus(30, 1, "rejected");
            var existingQueue   = BuildQueue(1, 10, queueId);

            var (repo, _, _, _) = CreateRepository(
                jobs:     [job],
                queues:   [existingQueue],
                statuses: [initiatedStatus, rejectedStatus]);

            // Act
            var result = await repo.EnqueueCutOverRejectBatchJobAsync(
                existingQueue.JobExecutionId, DefaultUserEmail, "reject note");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(30,               result.StatusId);
            Assert.Equal(DefaultUserEmail, result.RequestedBy);
            Assert.Equal("reject note",    result.ErrorMessage);
        }

        [Fact]
        public async Task EnqueueCutOverRejectBatchJobAsync_WhenNoInitiatedQueueRow_ThrowsKeyNotFoundException()
        {
            // Arrange
            var job            = BuildJob(1, CutOverJobName);
            var rejectedStatus = BuildStatus(30, 1, "rejected");

            var (repo, _, _, _) = CreateRepository(jobs: [job], statuses: [rejectedStatus]);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => repo.EnqueueCutOverRejectBatchJobAsync(
                    Guid.NewGuid(), DefaultUserEmail, "note"));
        }

        [Fact]
        public async Task EnqueueCutOverRejectBatchJobAsync_WhenRejectedStatusNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange — queue row exists but no "rejected" status
            var queueId         = Guid.NewGuid();
            var job             = BuildJob(1, CutOverJobName);
            var initiatedStatus = BuildStatus(10, 1, "initiated");
            var existingQueue   = BuildQueue(1, 10, queueId);

            var (repo, _, _, _) = CreateRepository(
                jobs:     [job],
                queues:   [existingQueue],
                statuses: [initiatedStatus]);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => repo.EnqueueCutOverRejectBatchJobAsync(
                    existingQueue.JobExecutionId, DefaultUserEmail, "note"));
        }

        [Fact]
        public async Task EnqueueCutOverRejectBatchJobAsync_WhenAlreadyApproved_ThrowsKeyNotFoundException()
        {
            // Arrange — row exists but its status is already Approved, not Initiated.
            var queueId          = Guid.NewGuid();
            var job              = BuildJob(1, CutOverJobName);
            var approvedStatus   = BuildStatus(20, 1, "approved");
            var rejectedStatus   = BuildStatus(30, 1, "rejected");
            var existingQueue    = BuildQueue(1, 20, queueId);

            var (repo, _, _, _) = CreateRepository(
                jobs:     [job],
                queues:   [existingQueue],
                statuses: [approvedStatus, rejectedStatus]);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => repo.EnqueueCutOverRejectBatchJobAsync(
                    existingQueue.JobExecutionId, DefaultUserEmail, "note"));
        }

        [Fact]
        public async Task EnqueueCutOverRejectBatchJobAsync_AddsLogEntry()
        {
            // Arrange
            var queueId         = Guid.NewGuid();
            var job             = BuildJob(1, CutOverJobName);
            var initiatedStatus = BuildStatus(10, 1, "initiated");
            var rejectedStatus  = BuildStatus(30, 1, "rejected");
            var existingQueue   = BuildQueue(1, 10, queueId);

            var (repo, _, _, logSet) = CreateRepository(
                jobs:     [job],
                queues:   [existingQueue],
                statuses: [initiatedStatus, rejectedStatus]);

            // Act
            await repo.EnqueueCutOverRejectBatchJobAsync(
                existingQueue.JobExecutionId, DefaultUserEmail, "reject note");

            // Assert
            logSet.Verify(x => x.Add(It.IsAny<BatchJobQueueLog>()), Times.Once);
        }

        [Fact]
        public async Task EnqueueCutOverRejectBatchJobAsync_CallsSaveChangesAsync_OnSuccess()
        {
            // Arrange
            var queueId         = Guid.NewGuid();
            var job             = BuildJob(1, CutOverJobName);
            var initiatedStatus = BuildStatus(10, 1, "initiated");
            var rejectedStatus  = BuildStatus(30, 1, "rejected");
            var existingQueue   = BuildQueue(1, 10, queueId);

            var (repo, mockContext, _, _) = CreateRepository(
                jobs:     [job],
                queues:   [existingQueue],
                statuses: [initiatedStatus, rejectedStatus]);

            // Act
            await repo.EnqueueCutOverRejectBatchJobAsync(
                existingQueue.JobExecutionId, DefaultUserEmail, "note");

            // Assert
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 1);
        }

        [Fact]
        public async Task EnqueueCutOverRejectBatchJobAsync_SaveFails_RollsBackTransaction()
        {
            // Arrange
            var queueId         = Guid.NewGuid();
            var job             = BuildJob(1, CutOverJobName);
            var initiatedStatus = BuildStatus(10, 1, "initiated");
            var rejectedStatus  = BuildStatus(30, 1, "rejected");
            var existingQueue   = BuildQueue(1, 10, queueId);

            var requestCtx = Substitute.For<IFpsRequestContext>();
            requestCtx.FpsYear.Returns(DefaultFpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestCtx);

            mockContext.Setup(x => x.BatchJobs)
                .Returns(RepositoryTestHelper.CreateMockDbSet<BatchJobMaster>([job]).Object);
            mockContext.Setup(x => x.BatchJobStatuses)
                .Returns(RepositoryTestHelper.CreateMockDbSet<BatchJobStatus>([initiatedStatus, rejectedStatus]).Object);

            var queueSet = RepositoryTestHelper.CreateMockDbSet<BatchJobQueue>([existingQueue]);
            RepositoryTestHelper.SetupDbSetOperations(queueSet);
            mockContext.Setup(x => x.BatchJobQueues).Returns(queueSet.Object);

            var logSet = RepositoryTestHelper.CreateMockDbSet<BatchJobQueueLog>([]);
            RepositoryTestHelper.SetupDbSetOperations(logSet);
            mockContext.Setup(x => x.BatchJobQueueLogs).Returns(logSet.Object);

            mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                       .ThrowsAsync(new InvalidOperationException("DB save failed"));

            var repo = new YearEndRepository(mockContext.Object, requestCtx, Substitute.For<IYearEndStagingRepository>());

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => repo.EnqueueCutOverRejectBatchJobAsync(
                    existingQueue.JobExecutionId, DefaultUserEmail, "note"));
        }

        #endregion

        #region SetTriggeredMetadataAsync

        [Theory]
        [InlineData("")]
        [InlineData("not-a-guid")]
        public async Task SetTriggeredMetadataAsync_ThrowsArgumentException_WhenJobExecutionIdIsInvalid(string invalidId)
        {
            // Arrange
            var (repo, _, _, _) = CreateRepository();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => repo.SetTriggeredMetadataAsync(invalidId, DefaultUserEmail));
        }

        [Fact]
        public async Task SetTriggeredMetadataAsync_ThrowsKeyNotFoundException_WhenRowDoesNotExist()
        {
            // Arrange — no queue rows exist at all
            var (repo, _, _, _) = CreateRepository();

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => repo.SetTriggeredMetadataAsync(Guid.NewGuid().ToString(), DefaultUserEmail));
        }

        [Fact]
        public async Task SetTriggeredMetadataAsync_UpdatesQueueEntry_OnSuccess()
        {
            // Arrange
            var job           = BuildJob(1, DefaultJobName);
            var approvedStatus = BuildStatus(20, 1, "approved");
            var existingQueue = BuildQueue(1, 20);

            var (repo, _, queueSet, _) = CreateRepository(
                jobs: [job], queues: [existingQueue], statuses: [approvedStatus]);

            // Act
            await repo.SetTriggeredMetadataAsync(existingQueue.JobExecutionId.ToString(), DefaultUserEmail);

            // Assert
            queueSet.Verify(x => x.Update(It.IsAny<BatchJobQueue>()), Times.Once);
        }

        [Fact]
        public async Task SetTriggeredMetadataAsync_CallsSaveChangesAsync_OnSuccess()
        {
            // Arrange
            var job           = BuildJob(1, DefaultJobName);
            var approvedStatus = BuildStatus(20, 1, "approved");
            var existingQueue = BuildQueue(1, 20);

            var (repo, mockContext, _, _) = CreateRepository(
                jobs: [job], queues: [existingQueue], statuses: [approvedStatus]);

            // Act
            await repo.SetTriggeredMetadataAsync(existingQueue.JobExecutionId.ToString(), DefaultUserEmail);

            // Assert
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 1);
        }

        #endregion
    }
}
