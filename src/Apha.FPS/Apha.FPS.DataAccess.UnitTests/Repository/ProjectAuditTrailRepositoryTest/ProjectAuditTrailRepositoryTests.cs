/*
 * TRANSFORMENGINE MIGRATION — ProjectAuditTrailRepositoryTests.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 13 — Unit Tests - Backend + Frontend xUnit Coverage
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - New file — xUnit tests for ProjectAuditTrailRepository (DataAccess layer)
 *   - Covers all 5 query methods: GetProjectLogsAsync, GetStaffJobLogsAsync,
 *     GetTestRequirementLogsAsync, GetAnimalRequestLogsAsync, GetAdditionalCostLogsAsync
 *   - Tests: happy path (data returned), no-match (empty), date range filtering, search filtering
 *   - Uses RepositoryTestHelper.CreateMockDbContext + CreateMockDbSet (Moq) pattern
 *     consistent with all other FPS DataAccess.UnitTests repository tests
 *
 * PRESERVED:
 *   - Moq + RepositoryTestHelper pattern (NOT NSubstitute) for DbContext/DbSet mocking
 *   - Naming convention: [MethodName]_[StateUnderTest]_[ExpectedResult]
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Join-based tests (StaffJobLogs, AnimalRequestLogs, etc.) require
 *     both JobCodes and the log DbSet to be seeded for the join filter to work correctly.
 */
using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Moq;

namespace Apha.FPS.DataAccess.UnitTests.Repository.ProjectAuditTrailRepositoryTest
{
    public class ProjectAuditTrailRepositoryTests
    {
        private const int DefaultFpsYear = 2024;
        private const string DefaultUserEmail = "test@example.com";
        private const string TestProject = "PROJ001";
        private const string TestJobCode = "JOB001";

        private static Mock<IFpsRequestContext> CreateMockRequestContext(int year = DefaultFpsYear)
        {
            var mock = new Mock<IFpsRequestContext>();
            mock.Setup(x => x.FpsYear).Returns(year);
            mock.Setup(x => x.UserEmailId).Returns(DefaultUserEmail);
            return mock;
        }

        // TRANSFORMENGINE: factory method — provides only the DbSets needed by each test
        private static ProjectAuditTrailRepository CreateRepository(
            IEnumerable<ProjectLog>? projectLogs = null,
            IEnumerable<StaffJobLog>? staffJobLogs = null,
            IEnumerable<TestRequirementLog>? testRequirementLogs = null,
            IEnumerable<AnimalRequestLog>? animalRequestLogs = null,
            IEnumerable<AdditionalCostLog>? additionalCostLogs = null,
            IEnumerable<JobCode>? jobCodes = null,
            int fpsYear = DefaultFpsYear)
        {
            var mockRequestContext = CreateMockRequestContext(fpsYear);
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockRequestContext.Object);

            if (projectLogs != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(projectLogs);
                mockContext.Setup(x => x.ProjectLogs).Returns(mockSet.Object);
            }

            if (staffJobLogs != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(staffJobLogs);
                mockContext.Setup(x => x.StaffJobLogs).Returns(mockSet.Object);
            }

            if (testRequirementLogs != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(testRequirementLogs);
                mockContext.Setup(x => x.TestRequirementLogs).Returns(mockSet.Object);
            }

            if (animalRequestLogs != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(animalRequestLogs);
                mockContext.Setup(x => x.AnimalRequestLogs).Returns(mockSet.Object);
            }

            if (additionalCostLogs != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(additionalCostLogs);
                mockContext.Setup(x => x.AdditionalCostLogs).Returns(mockSet.Object);
            }

            if (jobCodes != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(jobCodes);
                mockContext.Setup(x => x.JobCodes).Returns(mockSet.Object);
            }

            return new ProjectAuditTrailRepository(mockContext.Object, mockRequestContext.Object);
        }

        // ── GetProjectLogsAsync ──────────────────────────────────────────────

        #region GetProjectLogsAsync

        [Fact]
        public async Task GetProjectLogsAsync_WithMatchingParentProject_ReturnsPagedData()
        {
            // Arrange
            var logs = new List<ProjectLog>
            {
                new() { SequenceNo = 1, ParentProject = TestProject, ProjectTitle = "Alpha", Program = "P1",
                        Customer = "C1", ProjectStatus = "Active", Disease = "D1", Contract = "K1",
                        FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, ParentProject = TestProject, ProjectTitle = "Beta", Program = "P1",
                        Customer = "C1", ProjectStatus = "Active", Disease = "D1", Contract = "K1",
                        FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(projectLogs: logs);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetProjectLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Data);
            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetProjectLogsAsync_WithNonMatchingParentProject_ReturnsEmpty()
        {
            // Arrange
            var logs = new List<ProjectLog>
            {
                new() { SequenceNo = 1, ParentProject = "OTHER", ProjectTitle = "Alpha", Program = "P1",
                        Customer = "C1", ProjectStatus = "Active", Disease = "D1", Contract = "K1",
                        FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(projectLogs: logs);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetProjectLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetProjectLogsAsync_WithFromDateFilter_ExcludesEarlierRecords()
        {
            // Arrange
            var cutoff = new DateTime(2024, 6, 1);
            var logs = new List<ProjectLog>
            {
                new() { SequenceNo = 1, ParentProject = TestProject, ProjectTitle = "Old", Program = "P1",
                        Customer = "C1", ProjectStatus = "Active", Disease = "D1", Contract = "K1",
                        DateTime = new DateTime(2024, 1, 1), FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, ParentProject = TestProject, ProjectTitle = "New", Program = "P1",
                        Customer = "C1", ProjectStatus = "Active", Disease = "D1", Contract = "K1",
                        DateTime = new DateTime(2024, 7, 1), FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(projectLogs: logs);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetProjectLogsAsync(query, TestProject, cutoff, null);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal(1, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetProjectLogsAsync_WithToDateFilter_ExcludesLaterRecords()
        {
            // Arrange
            var cutoff = new DateTime(2024, 6, 30);
            var logs = new List<ProjectLog>
            {
                new() { SequenceNo = 1, ParentProject = TestProject, ProjectTitle = "Early", Program = "P1",
                        Customer = "C1", ProjectStatus = "Active", Disease = "D1", Contract = "K1",
                        DateTime = new DateTime(2024, 3, 1), FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, ParentProject = TestProject, ProjectTitle = "Late", Program = "P1",
                        Customer = "C1", ProjectStatus = "Active", Disease = "D1", Contract = "K1",
                        DateTime = new DateTime(2024, 9, 1), FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(projectLogs: logs);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetProjectLogsAsync(query, TestProject, null, cutoff);

            // Assert
            Assert.Single(result.Data);
        }

        [Fact]
        public async Task GetProjectLogsAsync_WithSearchFilter_ReturnsMatchingRecords()
        {
            // Arrange
            var logs = new List<ProjectLog>
            {
                new() { SequenceNo = 1, ParentProject = TestProject, ProjectTitle = "Alpha", Program = "P1",
                        Customer = "C1", ProjectStatus = "Active", Disease = "D1", Contract = "K1",
                        InsertDelete = "INSERT", FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, ParentProject = TestProject, ProjectTitle = "Beta", Program = "P1",
                        Customer = "C1", ProjectStatus = "Active", Disease = "D1", Contract = "K1",
                        InsertDelete = "DELETE", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(projectLogs: logs);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Search = "INSERT" };

            // Act
            var result = await repo.GetProjectLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.Single(result.Data);
        }

        #endregion

        // ── GetStaffJobLogsAsync ─────────────────────────────────────────────

        #region GetStaffJobLogsAsync

        [Fact]
        public async Task GetStaffJobLogsAsync_WithMatchingJobCode_ReturnsPagedData()
        {
            // Arrange
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = TestJobCode, ParentProject = TestProject }
            };
            var logs = new List<StaffJobLog>
            {
                new() { SequenceNo = 1, JobCode = TestJobCode, StaffId = "S001", PlannedHours = 8, FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, JobCode = TestJobCode, StaffId = "S002", PlannedHours = 4, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(staffJobLogs: logs, jobCodes: jobCodes);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetStaffJobLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Data);
            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetStaffJobLogsAsync_NoMatchingJobCode_ReturnsEmpty()
        {
            // Arrange — job code has a different parent project
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = TestJobCode, ParentProject = "OTHERPROJ" }
            };
            var logs = new List<StaffJobLog>
            {
                new() { SequenceNo = 1, JobCode = TestJobCode, StaffId = "S001", PlannedHours = 8, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(staffJobLogs: logs, jobCodes: jobCodes);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetStaffJobLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetStaffJobLogsAsync_WithFromDateFilter_ExcludesEarlierRecords()
        {
            // Arrange
            var cutoff = new DateTime(2024, 6, 1);
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = TestJobCode, ParentProject = TestProject }
            };
            var logs = new List<StaffJobLog>
            {
                new() { SequenceNo = 1, JobCode = TestJobCode, StaffId = "S001", PlannedHours = 8,
                        DateTime = new DateTime(2024, 1, 1), FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, JobCode = TestJobCode, StaffId = "S002", PlannedHours = 4,
                        DateTime = new DateTime(2024, 9, 1), FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(staffJobLogs: logs, jobCodes: jobCodes);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetStaffJobLogsAsync(query, TestProject, cutoff, null);

            // Assert
            Assert.Single(result.Data);
        }

        #endregion

        // ── GetTestRequirementLogsAsync ──────────────────────────────────────

        #region GetTestRequirementLogsAsync

        [Fact]
        public async Task GetTestRequirementLogsAsync_WithMatchingJobCode_ReturnsPagedData()
        {
            // Arrange
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = TestJobCode, ParentProject = TestProject }
            };
            var logs = new List<TestRequirementLog>
            {
                new() { SequenceNo = 1, JobCode = TestJobCode, TestCode = "TC001", FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, JobCode = TestJobCode, TestCode = "TC002", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(testRequirementLogs: logs, jobCodes: jobCodes);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetTestRequirementLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Data);
            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetTestRequirementLogsAsync_NoMatchingJobCode_ReturnsEmpty()
        {
            // Arrange
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = TestJobCode, ParentProject = "DIFFERENT" }
            };
            var logs = new List<TestRequirementLog>
            {
                new() { SequenceNo = 1, JobCode = TestJobCode, TestCode = "TC001", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(testRequirementLogs: logs, jobCodes: jobCodes);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetTestRequirementLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetTestRequirementLogsAsync_WithDateRange_FiltersCorrectly()
        {
            // Arrange
            var fromDate = new DateTime(2024, 4, 1);
            var toDate = new DateTime(2024, 9, 30);
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = TestJobCode, ParentProject = TestProject }
            };
            var logs = new List<TestRequirementLog>
            {
                new() { SequenceNo = 1, JobCode = TestJobCode, FpsYear = DefaultFpsYear,
                        DateTime = new DateTime(2024, 2, 1) },    // before range
                new() { SequenceNo = 2, JobCode = TestJobCode, FpsYear = DefaultFpsYear,
                        DateTime = new DateTime(2024, 6, 1) },    // in range
                new() { SequenceNo = 3, JobCode = TestJobCode, FpsYear = DefaultFpsYear,
                        DateTime = new DateTime(2024, 11, 1) }    // after range
            };
            var repo = CreateRepository(testRequirementLogs: logs, jobCodes: jobCodes);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetTestRequirementLogsAsync(query, TestProject, fromDate, toDate);

            // Assert
            Assert.Single(result.Data);
        }

        #endregion

        // ── GetAnimalRequestLogsAsync ────────────────────────────────────────

        #region GetAnimalRequestLogsAsync

        [Fact]
        public async Task GetAnimalRequestLogsAsync_WithMatchingJobCode_ReturnsPagedData()
        {
            // Arrange
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = TestJobCode, ParentProject = TestProject }
            };
            var logs = new List<AnimalRequestLog>
            {
                new() { SequenceNo = 1, JobCode = TestJobCode, AnimalType = "Rat",
                        NumberOfDays = 10, NumberOfAnimals = 5, FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, JobCode = TestJobCode, AnimalType = "Mouse",
                        NumberOfDays = 20, NumberOfAnimals = 10, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(animalRequestLogs: logs, jobCodes: jobCodes);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetAnimalRequestLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Data);
            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetAnimalRequestLogsAsync_NoMatchingJobCode_ReturnsEmpty()
        {
            // Arrange
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = TestJobCode, ParentProject = "WRONGPROJECT" }
            };
            var logs = new List<AnimalRequestLog>
            {
                new() { SequenceNo = 1, JobCode = TestJobCode, AnimalType = "Rat",
                        NumberOfDays = 10, NumberOfAnimals = 5, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(animalRequestLogs: logs, jobCodes: jobCodes);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetAnimalRequestLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetAnimalRequestLogsAsync_WithSearchFilter_ReturnsMatchingByAnimalType()
        {
            // Arrange
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = TestJobCode, ParentProject = TestProject }
            };
            var logs = new List<AnimalRequestLog>
            {
                new() { SequenceNo = 1, JobCode = TestJobCode, AnimalType = "Rat",
                        NumberOfDays = 10, NumberOfAnimals = 5, FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, JobCode = TestJobCode, AnimalType = "Mouse",
                        NumberOfDays = 20, NumberOfAnimals = 10, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(animalRequestLogs: logs, jobCodes: jobCodes);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Search = "rat" };

            // Act
            var result = await repo.GetAnimalRequestLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.Single(result.Data);
        }

        #endregion

        // ── GetAdditionalCostLogsAsync ───────────────────────────────────────

        #region GetAdditionalCostLogsAsync

        [Fact]
        public async Task GetAdditionalCostLogsAsync_WithMatchingJobCode_ReturnsPagedData()
        {
            // Arrange
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = TestJobCode, ParentProject = TestProject }
            };
            var logs = new List<AdditionalCostLog>
            {
                new() { SequenceNo = 1, JobCode = TestJobCode, Account = "ACC001",
                        Description = "Lab Supplies", ItemCost = 100m, FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, JobCode = TestJobCode, Account = "ACC002",
                        Description = "Equipment", ItemCost = 200m, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(additionalCostLogs: logs, jobCodes: jobCodes);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetAdditionalCostLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Data);
            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetAdditionalCostLogsAsync_NoMatchingJobCode_ReturnsEmpty()
        {
            // Arrange
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = TestJobCode, ParentProject = "NOPROJECT" }
            };
            var logs = new List<AdditionalCostLog>
            {
                new() { SequenceNo = 1, JobCode = TestJobCode, Account = "ACC001",
                        Description = "Lab Supplies", ItemCost = 100m, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(additionalCostLogs: logs, jobCodes: jobCodes);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetAdditionalCostLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetAdditionalCostLogsAsync_WithSearchFilter_ReturnsMatchingByDescription()
        {
            // Arrange
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = TestJobCode, ParentProject = TestProject }
            };
            var logs = new List<AdditionalCostLog>
            {
                new() { SequenceNo = 1, JobCode = TestJobCode, Account = "ACC001",
                        Description = "Lab Supplies", ItemCost = 100m, FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, JobCode = TestJobCode, Account = "ACC002",
                        Description = "Equipment Rental", ItemCost = 200m, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(additionalCostLogs: logs, jobCodes: jobCodes);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Search = "Lab" };

            // Act
            var result = await repo.GetAdditionalCostLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.Single(result.Data);
        }

        [Fact]
        public async Task GetAdditionalCostLogsAsync_WithDateRange_FiltersCorrectly()
        {
            // Arrange
            var fromDate = new DateTime(2024, 3, 1);
            var toDate = new DateTime(2024, 8, 31);
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = TestJobCode, ParentProject = TestProject }
            };
            var logs = new List<AdditionalCostLog>
            {
                new() { SequenceNo = 1, JobCode = TestJobCode, Account = "ACC001", Description = "A",
                        ItemCost = 50m, DateTime = new DateTime(2024, 1, 15), FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, JobCode = TestJobCode, Account = "ACC002", Description = "B",
                        ItemCost = 75m, DateTime = new DateTime(2024, 5, 15), FpsYear = DefaultFpsYear },
                new() { SequenceNo = 3, JobCode = TestJobCode, Account = "ACC003", Description = "C",
                        ItemCost = 90m, DateTime = new DateTime(2024, 10, 15), FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(additionalCostLogs: logs, jobCodes: jobCodes);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetAdditionalCostLogsAsync(query, TestProject, fromDate, toDate);

            // Assert
            Assert.Single(result.Data);
        }

        #endregion
    }
}
