using Apha.Common.Helpers.Repository;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Apha.PACT.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;
using NSubstitute;

namespace Apha.PACT.DataAccess.UnitTests.Repository.JobCodeRepositoryTest
{
    public class JobCodeRepositoryTests
    {
        private const int DefaultTestFpsYear = 2024;

        /// <summary>
        /// Creates a JobCodeRepository alongside mocked DbSet and context for call verification.
        /// AddAsync and Entry() are set up explicitly since they differ from the base SetupDbSetOperations.
        /// UpdateJobCodeAsync uses Entry().State — covered here; ExecuteDeleteAsync is not used (Remove is).
        /// </summary>
        private static (
            JobCodeRepository Repo,
            Mock<DbSet<JobCode>> JobCodesDbSet,
            Mock<FpsDbContext> Context)
            CreateRepositoryWithMocks(
                IEnumerable<JobCode> jobCodes,
                int fpsYear = DefaultTestFpsYear)
        {
            var fpsYearContext = Substitute.For<IFpsYearContext>();
            fpsYearContext.FPSYear.Returns(fpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsYearContext);
            var jobCodesMockSet = RepositoryTestHelper.CreateMockDbSet(jobCodes);

            RepositoryTestHelper.SetupDbSetOperations(jobCodesMockSet);
            jobCodesMockSet
                .Setup(x => x.AddAsync(It.IsAny<JobCode>(), It.IsAny<CancellationToken>()))
                .Returns((JobCode _, CancellationToken __) => new ValueTask<EntityEntry<JobCode>>());
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.JobCodes).Returns(jobCodesMockSet.Object);

            var repo = new JobCodeRepository(mockContext.Object, fpsYearContext);
            return (repo, jobCodesMockSet, mockContext);
        }

        private static JobCodeRepository CreateRepository(
            IEnumerable<JobCode> jobCodes,
            int fpsYear = DefaultTestFpsYear)
            => CreateRepositoryWithMocks(jobCodes, fpsYear).Repo;

        #region GetJobCodesByProjectAsync

        [Fact]
        public async Task GetJobCodesByProjectAsync_MatchingProject_ReturnsFilteredList()
        {
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = "JC1", ParentProject = "PRJ1", FpsYear = DefaultTestFpsYear },
                new() { JobCodeId = "JC2", ParentProject = "PRJ2", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(jobCodes);

            var result = (await repo.GetJobCodesByProjectAsync("PRJ1")).ToList();

            Assert.Single(result);
            Assert.Equal("JC1", result[0].JobCodeId);
        }

        [Fact]
        public async Task GetJobCodesByProjectAsync_NoMatch_ReturnsEmptyList()
        {
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = "JC1", ParentProject = "PRJ1", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(jobCodes);

            var result = await repo.GetJobCodesByProjectAsync("PRJ_NONE");

            Assert.Empty(result);
        }

        #endregion

        #region GetPagedJobCodesAsync

        [Fact]
        public async Task GetPagedJobCodesAsync_WithProject_ReturnsFilteredPagedResult()
        {
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = "JC1", ParentProject = "PRJ1", FpsYear = DefaultTestFpsYear },
                new() { JobCodeId = "JC2", ParentProject = "PRJ2", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(jobCodes);
            var query = new PaginationParameters<string>();

            var result = await repo.GetPagedJobCodesAsync(query, "PRJ1");

            Assert.Single(result.Data);
            Assert.Equal("JC1", result.Data.First().JobCodeId);
            Assert.Equal(1, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetPagedJobCodesAsync_NullProject_ReturnsAllRecordsPaged()
        {
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = "JC1", ParentProject = "PRJ1", FpsYear = DefaultTestFpsYear },
                new() { JobCodeId = "JC2", ParentProject = "PRJ2", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(jobCodes);
            var query = new PaginationParameters<string>();

            var result = await repo.GetPagedJobCodesAsync(query, null);

            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        #endregion

        #region GetJobCodeByIdAsync

        [Fact]
        public async Task GetJobCodeByIdAsync_ExistingId_ReturnsJobCode()
        {
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = "JC1", ParentProject = "PRJ1", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(jobCodes);

            var result = await repo.GetJobCodeByIdAsync("JC1");

            Assert.NotNull(result);
            Assert.Equal("JC1", result.JobCodeId);
        }

        [Fact]
        public async Task GetJobCodeByIdAsync_NonExistentId_ReturnsNull()
        {
            var repo = CreateRepository([]);

            var result = await repo.GetJobCodeByIdAsync("MISSING");

            Assert.Null(result);
        }

        #endregion

        #region GetTypesAsync

        [Fact]
        public async Task GetTypesAsync_WithTypes_ReturnsDistinctOrderedTypes()
        {
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = "JC1", Type = "TypeB", FpsYear = DefaultTestFpsYear },
                new() { JobCodeId = "JC2", Type = "TypeA", FpsYear = DefaultTestFpsYear },
                new() { JobCodeId = "JC3", Type = "TypeA", FpsYear = DefaultTestFpsYear },
                new() { JobCodeId = "JC4", Type = null,    FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(jobCodes);

            var result = (await repo.GetTypesAsync()).ToList();

            Assert.Equal(2, result.Count);
            Assert.Equal("TypeA", result[0]);
            Assert.Equal("TypeB", result[1]);
        }

        [Fact]
        public async Task GetTypesAsync_AllTypesNull_ReturnsEmptyList()
        {
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = "JC1", Type = null, FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(jobCodes);

            var result = await repo.GetTypesAsync();

            Assert.Empty(result);
        }

        #endregion

        #region CreateJobCodeAsync

        [Fact]
        public async Task CreateJobCodeAsync_ValidJobCode_SetsFpsYearAndSaves()
        {
            var (repo, jobCodesMockSet, mockContext) = CreateRepositoryWithMocks([]);
            var jobCode = new JobCode { JobCodeId = "JC1", ParentProject = "PRJ1" };

            var result = await repo.CreateJobCodeAsync(jobCode);

            Assert.NotNull(result);
            Assert.Equal(DefaultTestFpsYear, result.FpsYear);
            jobCodesMockSet.Verify(x => x.AddAsync(It.IsAny<JobCode>(), It.IsAny<CancellationToken>()), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task CreateJobCodeAsync_SetsFpsYear_FromYearContext()
        {
            const int customYear = 2025;
            var (repo, _, _) = CreateRepositoryWithMocks([], fpsYear: customYear);
            var jobCode = new JobCode { JobCodeId = "JC1" };

            var result = await repo.CreateJobCodeAsync(jobCode);

            Assert.Equal(customYear, result.FpsYear);
        }

        #endregion

        #region UpdateJobCodeAsync

        [Fact]
        public async Task UpdateJobCodeAsync_ValidJobCode_SetsFpsYearBeforeEntryIsCalled()
        {
            // Arrange — Entry() cannot be proxied by Moq; use Callback+Throws to verify
            // FpsYear is stamped on the entity BEFORE Entry() is invoked (mirrors EmployeeRepositoryTests pattern)
            var fpsYearContext = Substitute.For<IFpsYearContext>();
            fpsYearContext.FPSYear.Returns(DefaultTestFpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsYearContext);
            var jobCodesMockSet = RepositoryTestHelper.CreateMockDbSet<JobCode>([]);
            mockContext.Setup(x => x.JobCodes).Returns(jobCodesMockSet.Object);

            var entryWasCalled = false;
            mockContext.Setup(x => x.Entry(It.IsAny<JobCode>()))
                .Callback(() => entryWasCalled = true)
                .Throws(new NotSupportedException("Entry() is not supported in mocked DbContext"));

            var repo = new JobCodeRepository(mockContext.Object, fpsYearContext);
            var jobCode = new JobCode { JobCodeId = "JC1", ParentProject = "PRJ1" };

            // Act & Assert
            await Assert.ThrowsAsync<NotSupportedException>(() => repo.UpdateJobCodeAsync(jobCode));

            // FpsYear is assigned before Entry() is called, so it must be set despite the exception
            Assert.Equal(DefaultTestFpsYear, jobCode.FpsYear);
            Assert.True(entryWasCalled);
        }

        [Fact]
        public async Task UpdateJobCodeAsync_SetsFpsYear_FromYearContext()
        {
            const int customYear = 2025;
            var fpsYearContext = Substitute.For<IFpsYearContext>();
            fpsYearContext.FPSYear.Returns(customYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsYearContext);
            var jobCodesMockSet = RepositoryTestHelper.CreateMockDbSet<JobCode>([]);
            mockContext.Setup(x => x.JobCodes).Returns(jobCodesMockSet.Object);

            mockContext.Setup(x => x.Entry(It.IsAny<JobCode>()))
                .Throws(new NotSupportedException("Entry() is not supported in mocked DbContext"));

            var repo = new JobCodeRepository(mockContext.Object, fpsYearContext);
            var jobCode = new JobCode { JobCodeId = "JC1" };

            await Assert.ThrowsAsync<NotSupportedException>(() => repo.UpdateJobCodeAsync(jobCode));

            Assert.Equal(customYear, jobCode.FpsYear);
        }

        #endregion

        #region DeleteJobCodeAsync

        [Fact]
        public async Task DeleteJobCodeAsync_ExistingId_RemovesAndReturnsTrue()
        {
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = "JC1", FpsYear = DefaultTestFpsYear }
            };
            var (repo, jobCodesMockSet, mockContext) = CreateRepositoryWithMocks(jobCodes);

            var result = await repo.DeleteJobCodeAsync("JC1");

            Assert.True(result);
            RepositoryTestHelper.VerifyRemove(jobCodesMockSet);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task DeleteJobCodeAsync_NonExistentId_ReturnsFalse()
        {
            var repo = CreateRepository([]);

            var result = await repo.DeleteJobCodeAsync("MISSING");

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteJobCodeAsync_WrongFpsYear_ReturnsFalse()
        {
            // Entity exists but its FpsYear doesn't match the context year
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = "JC1", FpsYear = 2020 }
            };
            var repo = CreateRepository(jobCodes, fpsYear: DefaultTestFpsYear);

            var result = await repo.DeleteJobCodeAsync("JC1");

            Assert.False(result);
        }

        #endregion
    }
}
