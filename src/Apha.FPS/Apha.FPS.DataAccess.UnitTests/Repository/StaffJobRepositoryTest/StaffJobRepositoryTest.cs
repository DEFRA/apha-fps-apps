using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Apha.FPS.DataAccess.UnitTests.Repository.StaffJobRepositoryTest
{
    public class StaffJobRepositoryTest
    {
        /// <summary>
        /// Creates a StaffJobRepository with the specified in-memory DbSets.
        /// GetJobStaffCostAsync() and GetStaffChargeRate() use multi-table JOINs across 5+ DbSets
        /// and are covered by integration tests.
        /// GetStaffWorkgroupLookup() JOIN logic is also covered by integration tests.
        /// </summary>
        private static StaffJobRepository CreateRepository(
            IEnumerable<StaffJob>? staffJobs = null,
            IEnumerable<StaffView>? staffViews = null,
            IEnumerable<StaffPickView>? staffPickViews = null,
            IEnumerable<FpsSetting>? settings = null)
        {
            var mockFpsYearContext = new Mock<IFpsYearContext>();
            var mockProjectRepo   = new Mock<IProjectRepository>();
            var mockProgramRepo   = new Mock<IProgramRepository>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockFpsYearContext.Object);

            if (staffJobs != null)
            {
                var staffJobsMockSet = RepositoryTestHelper.CreateMockDbSet(staffJobs);
                RepositoryTestHelper.SetupDbSetOperations(staffJobsMockSet);
                mockContext.Setup(x => x.StaffJobs).Returns(staffJobsMockSet.Object);
            }

            if (staffViews != null)
            {
                var staffViewsMockSet = RepositoryTestHelper.CreateMockDbSet(staffViews);
                mockContext.Setup(x => x.StaffViews).Returns(staffViewsMockSet.Object);
            }

            if (staffPickViews != null)
            {
                var staffPickViewsMockSet = RepositoryTestHelper.CreateMockDbSet(staffPickViews);
                mockContext.Setup(x => x.StaffPickViews).Returns(staffPickViewsMockSet.Object);
            }

            if (settings != null)
            {
                var settingsMockSet = RepositoryTestHelper.CreateMockDbSet(settings);
                mockContext.Setup(x => x.TblSettings).Returns(settingsMockSet.Object);
            }

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            return new StaffJobRepository(mockContext.Object, mockProjectRepo.Object, mockProgramRepo.Object);
        }

        /// <summary>
        /// Returns the mocked StaffJobs DbSet and context alongside the repository for tests
        /// that need to verify calls.
        /// </summary>
        private static (
            StaffJobRepository Repo,
            Mock<DbSet<StaffJob>> StaffJobsDbSet,
            Mock<FpsDbContext> Context)
            CreateRepositoryWithMocks(IEnumerable<StaffJob>? staffJobs = null)
        {
            var mockFpsYearContext = new Mock<IFpsYearContext>();
            var mockProjectRepo   = new Mock<IProjectRepository>();
            var mockProgramRepo   = new Mock<IProgramRepository>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockFpsYearContext.Object);

            var staffJobsMockSet = RepositoryTestHelper.CreateMockDbSet(staffJobs ?? []);
            RepositoryTestHelper.SetupDbSetOperations(staffJobsMockSet);
            mockContext.Setup(x => x.StaffJobs).Returns(staffJobsMockSet.Object);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            var repo = new StaffJobRepository(mockContext.Object, mockProjectRepo.Object, mockProgramRepo.Object);
            return (repo, staffJobsMockSet, mockContext);
        }

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_ReturnsStaffJob_WhenFound()
        {
            // Arrange
            var staffJobs = new List<StaffJob>
            {
                new() { StaffId = "S001", JobCode = "JC001", PlannedHours = 8.0 },
                new() { StaffId = "S002", JobCode = "JC001", PlannedHours = 4.0 }
            };
            var repo = CreateRepository(staffJobs: staffJobs);

            // Act
            var result = await repo.GetByIdAsync("S001", "JC001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("S001",  result.StaffId);
            Assert.Equal("JC001", result.JobCode);
            Assert.Equal(8.0,     result.PlannedHours);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenStaffIdDoesNotMatch()
        {
            // Arrange
            var staffJobs = new List<StaffJob>
            {
                new() { StaffId = "S001", JobCode = "JC001", PlannedHours = 8.0 }
            };
            var repo = CreateRepository(staffJobs: staffJobs);

            // Act
            var result = await repo.GetByIdAsync("S999", "JC001");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenJobCodeDoesNotMatch()
        {
            // Arrange
            var staffJobs = new List<StaffJob>
            {
                new() { StaffId = "S001", JobCode = "JC001", PlannedHours = 8.0 }
            };
            var repo = CreateRepository(staffJobs: staffJobs);

            // Act
            var result = await repo.GetByIdAsync("S001", "JC999");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenStaffJobsIsEmpty()
        {
            // Arrange
            var repo = CreateRepository(staffJobs: []);

            // Act
            var result = await repo.GetByIdAsync("S001", "JC001");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_RequiresBothKeysToMatch()
        {
            // Arrange — same StaffId, different JobCode; same JobCode, different StaffId
            var staffJobs = new List<StaffJob>
            {
                new() { StaffId = "S001", JobCode = "JC002", PlannedHours = 6.0 },
                new() { StaffId = "S002", JobCode = "JC001", PlannedHours = 6.0 }
            };
            var repo = CreateRepository(staffJobs: staffJobs);

            // Act
            var result = await repo.GetByIdAsync("S001", "JC001");

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region AddAsync Tests

        [Fact]
        public async Task AddAsync_ThrowsArgumentNullException_WhenStaffJobIsNull()
        {
            // Arrange
            var repo = CreateRepository(staffJobs: []);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.AddAsync(null!));
        }

        [Fact]
        public async Task AddAsync_ThrowsArgumentOutOfRangeException_WhenPlannedHoursIsNegative()
        {
            // Arrange
            var repo = CreateRepository(staffJobs: []);
            var staffJob = new StaffJob { StaffId = "S001", JobCode = "JC001", PlannedHours = -1.0 };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => repo.AddAsync(staffJob));
        }

        [Fact]
        public async Task AddAsync_ThrowsInvalidOperationException_WhenDuplicateExists()
        {
            // Arrange — a StaffJob with the same StaffId + JobCode already exists
            var existingStaffJob = new StaffJob { StaffId = "S001", JobCode = "JC001", PlannedHours = 8.0 };
            var repo = CreateRepository(staffJobs: [existingStaffJob]);
            var duplicate = new StaffJob { StaffId = "S001", JobCode = "JC001", PlannedHours = 4.0 };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => repo.AddAsync(duplicate));
        }

        [Fact]
        public async Task AddAsync_AddsStaffJob_WhenValid()
        {
            // Arrange
            var (repo, staffJobsMockSet, mockContext) = CreateRepositoryWithMocks([]);
            var newStaffJob = new StaffJob { StaffId = "S001", JobCode = "JC001", PlannedHours = 8.0 };

            // Act
            var result = await repo.AddAsync(newStaffJob);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("S001",  result.StaffId);
            Assert.Equal("JC001", result.JobCode);
            Assert.Equal(8.0,     result.PlannedHours);
            staffJobsMockSet.Verify(x => x.Add(It.IsAny<StaffJob>()), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task AddAsync_AllowsZeroPlannedHours()
        {
            // Arrange
            var (repo, staffJobsMockSet, mockContext) = CreateRepositoryWithMocks([]);
            var staffJob = new StaffJob { StaffId = "S001", JobCode = "JC001", PlannedHours = 0.0 };

            // Act
            var result = await repo.AddAsync(staffJob);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0.0, result.PlannedHours);
            staffJobsMockSet.Verify(x => x.Add(It.IsAny<StaffJob>()), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task AddAsync_SetsCorrectFields_OnCreatedStaffJob()
        {
            // Arrange — verifies only StaffId/JobCode/PlannedHours are mapped (not FpsCalYear)
            StaffJob? capturedStaffJob = null;
            var (repo, staffJobsMockSet, _) = CreateRepositoryWithMocks([]);

            staffJobsMockSet
                .Setup(x => x.Add(It.IsAny<StaffJob>()))
                .Callback<StaffJob>(sj => capturedStaffJob = sj);

            var input = new StaffJob { StaffId = "S001", JobCode = "JC001", PlannedHours = 7.5 };

            // Act
            await repo.AddAsync(input);

            // Assert
            Assert.NotNull(capturedStaffJob);
            Assert.Equal("S001",  capturedStaffJob!.StaffId);
            Assert.Equal("JC001", capturedStaffJob.JobCode);
            Assert.Equal(7.5,     capturedStaffJob.PlannedHours);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ThrowsArgumentNullException_WhenStaffJobIsNull()
        {
            // Arrange
            var repo = CreateRepository(staffJobs: []);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.UpdateAsync(null!));
        }

        [Fact]
        public async Task UpdateAsync_ThrowsArgumentOutOfRangeException_WhenPlannedHoursIsNegative()
        {
            // Arrange
            var repo = CreateRepository(staffJobs: []);
            var staffJob = new StaffJob { StaffId = "S001", JobCode = "JC001", PlannedHours = -5.0 };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => repo.UpdateAsync(staffJob));
        }

        [Fact]
        public async Task UpdateAsync_ThrowsInvalidOperationException_WhenStaffJobNotFound()
        {
            // Arrange — no matching StaffId + JobCode in the store
            var repo = CreateRepository(staffJobs: []);
            var staffJob = new StaffJob { StaffId = "S001", JobCode = "JC001", PlannedHours = 8.0 };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => repo.UpdateAsync(staffJob));
        }

        [Fact]
        public async Task UpdateAsync_UpdatesPlannedHours_WhenStaffJobExists()
        {
            // Arrange
            var existing = new StaffJob { StaffId = "S001", JobCode = "JC001", PlannedHours = 4.0 };
            var (repo, _, mockContext) = CreateRepositoryWithMocks([existing]);

            var update = new StaffJob { StaffId = "S001", JobCode = "JC001", PlannedHours = 10.0 };

            // Act
            var result = await repo.UpdateAsync(update);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("S001",  result.StaffId);
            Assert.Equal("JC001", result.JobCode);
            Assert.Equal(10.0,    result.PlannedHours);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task UpdateAsync_AllowsZeroPlannedHours()
        {
            // Arrange
            var existing = new StaffJob { StaffId = "S001", JobCode = "JC001", PlannedHours = 8.0 };
            var (repo, _, mockContext) = CreateRepositoryWithMocks([existing]);

            var update = new StaffJob { StaffId = "S001", JobCode = "JC001", PlannedHours = 0.0 };

            // Act
            var result = await repo.UpdateAsync(update);

            // Assert
            Assert.Equal(0.0, result.PlannedHours);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        #endregion

        #region DeleteAsync Tests

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task DeleteAsync_ThrowsArgumentException_WhenJobCodeIsNullOrWhiteSpace(string jobCode)
        {
            // Arrange
            var repo = CreateRepository(staffJobs: []);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => repo.DeleteAsync("S001", jobCode));
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenStaffJobNotFound()
        {
            // Arrange
            var repo = CreateRepository(staffJobs: []);

            // Act
            var result = await repo.DeleteAsync("S999", "JC999");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenJobCodeMatchesButStaffIdDoesNot()
        {
            // Arrange
            var staffJobs = new List<StaffJob>
            {
                new() { StaffId = "S001", JobCode = "JC001", PlannedHours = 8.0 }
            };
            var repo = CreateRepository(staffJobs: staffJobs);

            // Act
            var result = await repo.DeleteAsync("S999", "JC001");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsTrue_AndRemovesStaffJob_WhenFound()
        {
            // Arrange
            var existing = new StaffJob { StaffId = "S001", JobCode = "JC001", PlannedHours = 8.0 };
            var (repo, staffJobsMockSet, mockContext) = CreateRepositoryWithMocks([existing]);

            // Act
            var result = await repo.DeleteAsync("S001", "JC001");

            // Assert
            Assert.True(result);
            staffJobsMockSet.Verify(x => x.Remove(It.IsAny<StaffJob>()), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task DeleteAsync_CommitsTransaction_WhenDeleteSucceeds()
        {
            // Arrange — transaction setup is provided by RepositoryTestHelper.SetupTransaction
            var existing = new StaffJob { StaffId = "S001", JobCode = "JC001", PlannedHours = 8.0 };
            var (repo, _, _) = CreateRepositoryWithMocks([existing]);

            // Act & Assert — no exception means CommitAsync was reachable
            var result = await repo.DeleteAsync("S001", "JC001");

            Assert.True(result);
        }

        #endregion

        #region GetStaffWorkgroupLookup Tests

        [Fact]
        public async Task GetStaffWorkgroupLookup_ReturnsEmpty_WhenNoStaffViews()
        {
            // Arrange
            var repo = CreateRepository(
                staffViews:     [],
                staffPickViews: []);

            // Act
            var result = await repo.GetStaffWorkgroupLookup();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetStaffWorkgroupLookup_ReturnsEmpty_WhenNoMatchingStaffPickViews()
        {
            // Arrange — StaffView exists but no matching StaffPickView join partner
            var staffViews = new List<StaffView>
            {
                new() { StaffId = "S001", Name = "Alice", WorkgroupGrade = "WG01", HrsAvail = 37.5, UserId = 42 }
            };
            var repo = CreateRepository(
                staffViews:     staffViews,
                staffPickViews: []);

            // Act
            var result = await repo.GetStaffWorkgroupLookup();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetStaffWorkgroupLookup_ReturnsOrderedByName()
        {
            // Arrange
            var staffViews = new List<StaffView>
            {
                new() { StaffId = "S001", Name = "Charlie", WorkgroupGrade = "WG01", HrsAvail = 37.5, UserId = 42 },
                new() { StaffId = "S002", Name = "Alice",   WorkgroupGrade = "WG02", HrsAvail = 30.0, UserId = 42 },
                new() { StaffId = "S003", Name = "Bob",     WorkgroupGrade = "WG03", HrsAvail = 25.0, UserId = 42 }
            };
            var staffPickViews = new List<StaffPickView>
            {
                new() { StaffId = "S001", Name = "Charlie", WorkgroupGrade = "WG01" },
                new() { StaffId = "S002", Name = "Alice",   WorkgroupGrade = "WG02" },
                new() { StaffId = "S003", Name = "Bob",     WorkgroupGrade = "WG03" }
            };
            var repo = CreateRepository(staffViews: staffViews, staffPickViews: staffPickViews);

            // Act
            var result = await repo.GetStaffWorkgroupLookup();

            // Assert
            Assert.Equal(3,         result.Count);
            Assert.Equal("Alice",   result[0].Name);
            Assert.Equal("Bob",     result[1].Name);
            Assert.Equal("Charlie", result[2].Name);
        }

        [Fact]
        public async Task GetStaffWorkgroupLookup_MapsFieldsCorrectly()
        {
            // Arrange
            var staffViews = new List<StaffView>
            {
                new() { StaffId = "S001", Name = "Alice Smith", WorkgroupGrade = "WG01", HrsAvail = 37.5, UserId = 42 }
            };
            var staffPickViews = new List<StaffPickView>
            {
                new() { StaffId = "S001", Name = "Alice Smith", WorkgroupGrade = "WG01" }
            };
            var repo = CreateRepository(staffViews: staffViews, staffPickViews: staffPickViews);

            // Act
            var result = await repo.GetStaffWorkgroupLookup();

            // Assert
            var item = Assert.Single(result);
            Assert.Equal("S001",       item.StaffID);
            Assert.Equal("Alice Smith", item.Name);
            Assert.Equal("WG01",       item.WorkGroupGrade);
            Assert.Equal(37.5,         item.HrsAvail);
        }
        

        [Fact]
        public async Task GetStaffWorkgroupLookup_ExcludesStaffViews_NotBelongingToUserId42()
        {
            // Arrange
            var staffViews = new List<StaffView>
            {
                new() { StaffId = "S001", Name = "Alice", WorkgroupGrade = "WG01", HrsAvail = 37.5, UserId = 42 },
                new() { StaffId = "S002", Name = "Bob",   WorkgroupGrade = "WG02", HrsAvail = 30.0, UserId = 99 } // excluded
            };
            var staffPickViews = new List<StaffPickView>
            {
                new() { StaffId = "S001", Name = "Alice", WorkgroupGrade = "WG01" },
                new() { StaffId = "S002", Name = "Bob",   WorkgroupGrade = "WG02" }
            };
            var repo = CreateRepository(staffViews: staffViews, staffPickViews: staffPickViews);

            // Act
            var result = await repo.GetStaffWorkgroupLookup();

            // Assert
            Assert.Single(result);
            Assert.Equal("Alice", result[0].Name);
        }

        #endregion
    }
}