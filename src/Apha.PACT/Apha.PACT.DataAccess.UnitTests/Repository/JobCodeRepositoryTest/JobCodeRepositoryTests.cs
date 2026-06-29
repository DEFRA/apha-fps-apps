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
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(fpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);
            var jobCodesMockSet = RepositoryTestHelper.CreateMockDbSet(jobCodes);

            RepositoryTestHelper.SetupDbSetOperations(jobCodesMockSet);
            jobCodesMockSet
                .Setup(x => x.AddAsync(It.IsAny<JobCode>(), It.IsAny<CancellationToken>()))
                .Returns((JobCode _, CancellationToken __) => new ValueTask<EntityEntry<JobCode>>());
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.JobCodes).Returns(jobCodesMockSet.Object);

            var repo = new JobCodeRepository(mockContext.Object, fpsRequestContext);
            return (repo, jobCodesMockSet, mockContext);
        }

        private static JobCodeRepository CreateRepository(
            IEnumerable<JobCode> jobCodes,
            int fpsYear = DefaultTestFpsYear)
            => CreateRepositoryWithMocks(jobCodes, fpsYear).Repo;

        #region GetJobCodesAsync

        [Fact]
        public async Task GetJobCodesAsync_WithJobCodes_ReturnsAllOrderedByJobCodeId()
        {
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = "JC3", ParentProject = "PRJ1", FpsYear = DefaultTestFpsYear },
                new() { JobCodeId = "JC1", ParentProject = "PRJ2", FpsYear = DefaultTestFpsYear },
                new() { JobCodeId = "JC2", ParentProject = "PRJ3", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(jobCodes);

            var result = (await repo.GetJobCodesAsync()).ToList();

            Assert.Equal(3, result.Count);
            Assert.Equal("JC1", result[0].JobCodeId);
            Assert.Equal("JC2", result[1].JobCodeId);
            Assert.Equal("JC3", result[2].JobCodeId);
        }

        [Fact]
        public async Task GetJobCodesAsync_EmptyTable_ReturnsEmptyList()
        {
            var repo = CreateRepository([]);

            var result = await repo.GetJobCodesAsync();

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetJobCodesAsync_SingleRecord_ReturnsSingleItem()
        {
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = "JC1", ParentProject = "PRJ1", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(jobCodes);

            var result = (await repo.GetJobCodesAsync()).ToList();

            Assert.Single(result);
            Assert.Equal("JC1", result[0].JobCodeId);
        }

        #endregion

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

        [Fact]
        public async Task GetPagedJobCodesAsync_WithJobCodeIdFilter_ReturnsMatchingRecord()
        {
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = "ALPHA", ParentProject = "PRJ1", FpsYear = DefaultTestFpsYear },
                new() { JobCodeId = "BETA",  ParentProject = "PRJ1", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(jobCodes);
            var query = new PaginationParameters<string>
            {
                Filter = """{"JobCodeId":"ALPHA"}"""
            };

            var result = await repo.GetPagedJobCodesAsync(query, null);

            Assert.Equal(1, result.PaginationData.TotalRecords);
            Assert.Equal("ALPHA", result.Data.First().JobCodeId);
        }

        [Fact]
        public async Task GetPagedJobCodesAsync_WithParentProjectFilter_ReturnsMatchingRecords()
        {
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = "JC1", ParentProject = "PRJ1", FpsYear = DefaultTestFpsYear },
                new() { JobCodeId = "JC2", ParentProject = "PRJ2", FpsYear = DefaultTestFpsYear },
                new() { JobCodeId = "JC3", ParentProject = "PRJ1", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(jobCodes);
            var query = new PaginationParameters<string>
            {
                Filter = """{"ParentProject":"PRJ1"}"""
            };

            var result = await repo.GetPagedJobCodesAsync(query, null);

            Assert.Equal(2, result.PaginationData.TotalRecords);
            Assert.All(result.Data, j => Assert.Equal("PRJ1", j.ParentProject));
        }

        [Fact]
        public async Task GetPagedJobCodesAsync_WithJobCodeWorkGroupFilter_ReturnsMatchingRecords()
        {
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = "JC1", JobCodeWorkGroup = "WGA", FpsYear = DefaultTestFpsYear },
                new() { JobCodeId = "JC2", JobCodeWorkGroup = "WGB", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(jobCodes);
            var query = new PaginationParameters<string>
            {
                Filter = """{"JobCodeWorkGroup":"WGA"}"""
            };

            var result = await repo.GetPagedJobCodesAsync(query, null);

            Assert.Equal(1, result.PaginationData.TotalRecords);
            Assert.Equal("WGA", result.Data.First().JobCodeWorkGroup);
        }

        [Fact]
        public async Task GetPagedJobCodesAsync_WithTypeFilter_ReturnsMatchingRecords()
        {
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = "JC1", Type = "TypeA", FpsYear = DefaultTestFpsYear },
                new() { JobCodeId = "JC2", Type = "TypeB", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(jobCodes);
            var query = new PaginationParameters<string>
            {
                Filter = """{"Type":"TypeA"}"""
            };

            var result = await repo.GetPagedJobCodesAsync(query, null);

            Assert.Equal(1, result.PaginationData.TotalRecords);
            Assert.Equal("TypeA", result.Data.First().Type);
        }

        [Fact]
        public async Task GetPagedJobCodesAsync_WithJobCodeNameFilter_ReturnsMatchingRecords()
        {
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = "JC1", JobCodeName = "Analysis", FpsYear = DefaultTestFpsYear },
                new() { JobCodeId = "JC2", JobCodeName = "Review",   FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(jobCodes);
            var query = new PaginationParameters<string>
            {
                Filter = """{"JobCodeName":"Analysis"}"""
            };

            var result = await repo.GetPagedJobCodesAsync(query, null);

            Assert.Equal(1, result.PaginationData.TotalRecords);
            Assert.Equal("Analysis", result.Data.First().JobCodeName);
        }

        [Fact]
        public async Task GetPagedJobCodesAsync_NullFilter_ReturnsAllRecords()
        {
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = "JC1", FpsYear = DefaultTestFpsYear },
                new() { JobCodeId = "JC2", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(jobCodes);
            var query = new PaginationParameters<string> { Filter = null };

            var result = await repo.GetPagedJobCodesAsync(query, null);

            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetPagedJobCodesAsync_EmptyFilter_ReturnsAllRecords()
        {
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = "JC1", FpsYear = DefaultTestFpsYear },
                new() { JobCodeId = "JC2", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(jobCodes);
            var query = new PaginationParameters<string> { Filter = string.Empty };

            var result = await repo.GetPagedJobCodesAsync(query, null);

            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        [Theory]
        [InlineData("JobCodeId",       false, "JC1", "JC2")]
        [InlineData("JobCodeId",       true,  "JC2", "JC1")]
        [InlineData("ParentProject",   false, "JC1", "JC2")]
        [InlineData("ParentProject",   true,  "JC2", "JC1")]
        [InlineData("JobCodeWorkGroup",false, "JC1", "JC2")]
        [InlineData("JobCodeWorkGroup",true,  "JC2", "JC1")]
        [InlineData("Type",            false, "JC1", "JC2")]
        [InlineData("Type",            true,  "JC2", "JC1")]
        [InlineData("JobCodeName",     false, "JC1", "JC2")]
        [InlineData("JobCodeName",     true,  "JC2", "JC1")]
        public async Task GetPagedJobCodesAsync_WithSortBy_ReturnsSortedResults(
            string sortBy, bool descending, string expectedFirst, string expectedSecond)
        {
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = "JC1", ParentProject = "AAA", JobCodeWorkGroup = "WG1", Type = "Alpha", JobCodeName = "AAA-Name", FpsYear = DefaultTestFpsYear },
                new() { JobCodeId = "JC2", ParentProject = "BBB", JobCodeWorkGroup = "WG2", Type = "Beta",  JobCodeName = "BBB-Name", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(jobCodes);
            var query = new PaginationParameters<string> { SortBy = sortBy, Descending = descending };

            var result = await repo.GetPagedJobCodesAsync(query, null);

            Assert.Equal(expectedFirst,  result.Data.First().JobCodeId);
            Assert.Equal(expectedSecond, result.Data.Last().JobCodeId);
        }

        [Fact]
        public async Task GetPagedJobCodesAsync_UnknownSortBy_DefaultsSortByJobCodeId()
        {
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = "JC3", FpsYear = DefaultTestFpsYear },
                new() { JobCodeId = "JC1", FpsYear = DefaultTestFpsYear },
                new() { JobCodeId = "JC2", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(jobCodes);
            var query = new PaginationParameters<string> { SortBy = "UnknownColumn" };

            var result = await repo.GetPagedJobCodesAsync(query, null);

            Assert.Equal("JC1", result.Data.First().JobCodeId);
        }

        [Fact]
        public async Task GetPagedJobCodesAsync_NoSortBy_DefaultsSortByJobCodeId()
        {
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = "JC3", FpsYear = DefaultTestFpsYear },
                new() { JobCodeId = "JC1", FpsYear = DefaultTestFpsYear },
                new() { JobCodeId = "JC2", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(jobCodes);
            var query = new PaginationParameters<string>();

            var result = await repo.GetPagedJobCodesAsync(query, null);

            Assert.Equal("JC1", result.Data.First().JobCodeId);
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
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(DefaultTestFpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);
            var jobCodesMockSet = RepositoryTestHelper.CreateMockDbSet<JobCode>([]);
            mockContext.Setup(x => x.JobCodes).Returns(jobCodesMockSet.Object);

            var entryWasCalled = false;
            mockContext.Setup(x => x.Entry(It.IsAny<JobCode>()))
                .Callback(() => entryWasCalled = true)
                .Throws(new NotSupportedException("Entry() is not supported in mocked DbContext"));

            var repo = new JobCodeRepository(mockContext.Object, fpsRequestContext);
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
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(customYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);
            var jobCodesMockSet = RepositoryTestHelper.CreateMockDbSet<JobCode>([]);
            mockContext.Setup(x => x.JobCodes).Returns(jobCodesMockSet.Object);

            mockContext.Setup(x => x.Entry(It.IsAny<JobCode>()))
                .Throws(new NotSupportedException("Entry() is not supported in mocked DbContext"));

            var repo = new JobCodeRepository(mockContext.Object, fpsRequestContext);
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

        #region GetZtJobCodesAsync

        private static JobCodeRepository CreateRepositoryWithProjectViews(
            IEnumerable<ProjectView> projectViews,
            string userEmail,
            int fpsYear = DefaultTestFpsYear)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(fpsYear);
            fpsRequestContext.UserEmailId.Returns(userEmail);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);

            var jobCodesMockSet = RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<JobCode>());
            mockContext.Setup(x => x.JobCodes).Returns(jobCodesMockSet.Object);

            var projectViewsMockSet = RepositoryTestHelper.CreateMockDbSet(projectViews);
            mockContext.Setup(x => x.ProjectViews).Returns(projectViewsMockSet.Object);

            return new JobCodeRepository(mockContext.Object, fpsRequestContext);
        }

        [Fact]
        public async Task GetZtJobCodesAsync_WithMatchingRecords_ReturnsLookups()
        {
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "ZT001", ProjectTitle = "ZT Project 1", Program = "zt_prog", UserEmail = "user@test.com", FpsYear = DefaultTestFpsYear },
                new() { ParentProject = "ZT002", ProjectTitle = "ZT Project 2", Program = "zt_prog", UserEmail = "user@test.com", FpsYear = DefaultTestFpsYear },
                new() { ParentProject = "OTHER", ProjectTitle = "Non-ZT", Program = "other_prog", UserEmail = "user@test.com", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepositoryWithProjectViews(projectViews, "user@test.com");

            var result = (await repo.GetZtJobCodesAsync()).ToList();

            Assert.Equal(2, result.Count);
            Assert.Contains(result, r => r.JobCode == "ZT001" && r.Description == "ZT Project 1");
            Assert.Contains(result, r => r.JobCode == "ZT002" && r.Description == "ZT Project 2");
        }

        [Fact]
        public async Task GetZtJobCodesAsync_NoMatchingProgram_ReturnsEmpty()
        {
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "PRJ1", ProjectTitle = "Other Project", Program = "other_prog", UserEmail = "user@test.com", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepositoryWithProjectViews(projectViews, "user@test.com");

            var result = await repo.GetZtJobCodesAsync();

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetZtJobCodesAsync_NoMatchingEmail_ReturnsEmpty()
        {
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "ZT001", ProjectTitle = "ZT Project 1", Program = "zt_prog", UserEmail = "other@test.com", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepositoryWithProjectViews(projectViews, "user@test.com");

            var result = await repo.GetZtJobCodesAsync();

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetZtJobCodesAsync_EmptyTable_ReturnsEmpty()
        {
            var repo = CreateRepositoryWithProjectViews([], "user@test.com");

            var result = await repo.GetZtJobCodesAsync();

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetZtJobCodesAsync_CaseInsensitiveProgram_ReturnsMatches()
        {
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "ZT001", ProjectTitle = "ZT Project 1", Program = "ZT_PROG", UserEmail = "user@test.com", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepositoryWithProjectViews(projectViews, "user@test.com");

            var result = (await repo.GetZtJobCodesAsync()).ToList();

            Assert.Single(result);
            Assert.Equal("ZT001", result[0].JobCode);
        }

        #endregion
    }
}
