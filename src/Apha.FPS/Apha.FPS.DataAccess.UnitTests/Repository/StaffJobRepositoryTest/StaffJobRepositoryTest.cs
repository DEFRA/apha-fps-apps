using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Apha.FPS.DataAccess.UnitTests.Repository.StaffJobRepositoryTest
{
    public class StaffJobRepositoryTest
    {
        private const int DefaultTestFpsYear = 2024;

        private static Mock<IFpsYearContext> CreateMockFpsYearContext(int year = DefaultTestFpsYear)
        {
            var mock = new Mock<IFpsYearContext>();
            mock.Setup(x => x.FPSYear).Returns(year);
            return mock;
        }

        /// <summary>
        /// Creates a StaffJobRepository with in-memory StaffJobs data.
        /// IProgramRepository and IProjectRepository are mocked with empty queryables
        /// — their JOIN logic is covered by integration tests.
        /// </summary>
        private static StaffJobRepository CreateRepository(IEnumerable<StaffJob> staffJobs)
        {
            var mockFpsYear = CreateMockFpsYearContext();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockFpsYear.Object);

            var staffJobsMockSet = RepositoryTestHelper.CreateMockDbSet(staffJobs);
            RepositoryTestHelper.SetupDbSetOperations(staffJobsMockSet);
            mockContext.Setup(x => x.StaffJobs).Returns(staffJobsMockSet.Object);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            var mockProjectRepo = new Mock<IProjectRepository>();
            mockProjectRepo.Setup(r => r.Get()).Returns(Enumerable.Empty<Project>().AsQueryable());

            var mockProgramRepo = new Mock<IProgramRepository>();
            mockProgramRepo.Setup(r => r.Get()).Returns(Enumerable.Empty<Program>().AsQueryable());

            return new StaffJobRepository(mockContext.Object, mockProjectRepo.Object, mockProgramRepo.Object);
        }

        /// <summary>
        /// Returns the mocked DbSet alongside the repository for tests that need to verify calls.
        /// </summary>
        private static (StaffJobRepository Repo, Mock<DbSet<StaffJob>> DbSet, Mock<FpsDbContext> Context)
            CreateRepositoryWithMocks(IEnumerable<StaffJob> staffJobs)
        {
            var mockFpsYear = CreateMockFpsYearContext();
            var (mockContext, staffJobsMockSet) =
                RepositoryTestHelper.CreateRepositoryContext<FpsDbContext, StaffJob>(
                    staffJobs, mockFpsYear.Object);

            mockContext.Setup(x => x.StaffJobs).Returns(staffJobsMockSet.Object);

            var mockProjectRepo = new Mock<IProjectRepository>();
            mockProjectRepo.Setup(r => r.Get()).Returns(Enumerable.Empty<Project>().AsQueryable());

            var mockProgramRepo = new Mock<IProgramRepository>();
            mockProgramRepo.Setup(r => r.Get()).Returns(Enumerable.Empty<Program>().AsQueryable());

            var repo = new StaffJobRepository(mockContext.Object, mockProjectRepo.Object, mockProgramRepo.Object);
            return (repo, staffJobsMockSet, mockContext);
        }

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_ReturnsStaffJob_WhenFound()
        {
            // Arrange
            var staffJobs = new List<StaffJob>
            {
                new() { StaffId = "S001", JobCode = "JOB001", PlannedHours = 40 },
                new() { StaffId = "S002", JobCode = "JOB002", PlannedHours = 20 }
            };
            var repo = CreateRepository(staffJobs);

            // Act
            var result = await repo.GetByIdAsync("S001", "JOB001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("S001", result.StaffId);
            Assert.Equal("JOB001", result.JobCode);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
        {
            // Arrange
            var staffJobs = new List<StaffJob>
            {
                new() { StaffId = "S001", JobCode = "JOB001" }
            };
            var repo = CreateRepository(staffJobs);

            // Act
            var result = await repo.GetByIdAsync("S999", "JOB999");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenStaffJobsEmpty()
        {
            // Arrange
            var repo = CreateRepository(new List<StaffJob>());

            // Act
            var result = await repo.GetByIdAsync("S001", "JOB001");

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData("S001", "JOB_WRONG")]
        [InlineData("S_WRONG", "JOB001")]
        public async Task GetByIdAsync_ReturnsNull_WhenOnlyPartialKeyMatches(string staffId, string jobCode)
        {
            // Arrange
            var staffJobs = new List<StaffJob>
            {
                new() { StaffId = "S001", JobCode = "JOB001" }
            };
            var repo = CreateRepository(staffJobs);

            // Act
            var result = await repo.GetByIdAsync(staffId, jobCode);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region AddAsync

        [Fact]
        public async Task AddAsync_AddsStaffJob_WhenValid()
        {
            // Arrange
            var (repo, staffJobsMockSet, mockContext) = CreateRepositoryWithMocks(new List<StaffJob>());
            var newStaffJob = new StaffJob { StaffId = "S001", JobCode = "JOB001", PlannedHours = 40 };

            // Act
            var result = await repo.AddAsync(newStaffJob);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("S001", result.StaffId);
            Assert.Equal("JOB001", result.JobCode);
            Assert.Equal(40, result.PlannedHours);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task AddAsync_ThrowsArgumentNullException_WhenStaffJobIsNull()
        {
            // Arrange
            var repo = CreateRepository(new List<StaffJob>());

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.AddAsync(null!));
        }

        [Fact]
        public async Task AddAsync_ThrowsArgumentOutOfRangeException_WhenPlannedHoursIsNegative()
        {
            // Arrange
            var repo = CreateRepository(new List<StaffJob>());
            var staffJob = new StaffJob { StaffId = "S001", JobCode = "JOB001", PlannedHours = -1 };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => repo.AddAsync(staffJob));
        }

        [Fact]
        public async Task AddAsync_ThrowsInvalidOperationException_WhenStaffJobAlreadyExists()
        {
            // Arrange
            var existing = new StaffJob { StaffId = "S001", JobCode = "JOB001", PlannedHours = 40 };
            var repo = CreateRepository(new List<StaffJob> { existing });
            var duplicate = new StaffJob { StaffId = "S001", JobCode = "JOB001", PlannedHours = 20 };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => repo.AddAsync(duplicate));
            Assert.Contains("S001", ex.Message);
            Assert.Contains("JOB001", ex.Message);
        }

        [Fact]
        public async Task AddAsync_AllowsZeroPlannedHours()
        {
            // Arrange
            var (repo, _, mockContext) = CreateRepositoryWithMocks(new List<StaffJob>());
            var staffJob = new StaffJob { StaffId = "S001", JobCode = "JOB001", PlannedHours = 0 };

            // Act
            var result = await repo.AddAsync(staffJob);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.PlannedHours);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_UpdatesPlannedHours_WhenStaffJobExists()
        {
            // Arrange
            var existing = new StaffJob { StaffId = "S001", JobCode = "JOB001", PlannedHours = 40 };
            var (repo, _, mockContext) = CreateRepositoryWithMocks(new List<StaffJob> { existing });
            var update = new StaffJob { StaffId = "S001", JobCode = "JOB001", PlannedHours = 80 };

            // Act
            var result = await repo.UpdateAsync(update);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(80, result.PlannedHours);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task UpdateAsync_ThrowsArgumentNullException_WhenStaffJobIsNull()
        {
            // Arrange
            var repo = CreateRepository(new List<StaffJob>());

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.UpdateAsync(null!));
        }

        [Fact]
        public async Task UpdateAsync_ThrowsArgumentOutOfRangeException_WhenPlannedHoursIsNegative()
        {
            // Arrange
            var repo = CreateRepository(new List<StaffJob>());
            var staffJob = new StaffJob { StaffId = "S001", JobCode = "JOB001", PlannedHours = -5 };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => repo.UpdateAsync(staffJob));
        }

        [Fact]
        public async Task UpdateAsync_ThrowsInvalidOperationException_WhenStaffJobNotFound()
        {
            // Arrange
            var repo = CreateRepository(new List<StaffJob>());
            var staffJob = new StaffJob { StaffId = "S999", JobCode = "JOB999", PlannedHours = 40 };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => repo.UpdateAsync(staffJob));
            Assert.Contains("S999", ex.Message);
            Assert.Contains("JOB999", ex.Message);
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_ReturnsTrue_WhenStaffJobExists()
        {
            // Arrange
            var existing = new StaffJob { StaffId = "S001", JobCode = "JOB001" };
            var (repo, staffJobsMockSet, mockContext) =
                CreateRepositoryWithMocks(new List<StaffJob> { existing });

            // Act
            var result = await repo.DeleteAsync("S001", "JOB001");

            // Assert
            Assert.True(result);
            RepositoryTestHelper.VerifyRemove(staffJobsMockSet);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenStaffJobNotFound()
        {
            // Arrange
            var repo = CreateRepository(new List<StaffJob>());

            // Act
            var result = await repo.DeleteAsync("S999", "JOB999");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_ThrowsArgumentException_WhenJobCodeIsNullOrEmpty()
        {
            // Arrange
            var repo = CreateRepository(new List<StaffJob>());

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => repo.DeleteAsync("S001", ""));
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenOnlyStaffIdMatches()
        {
            // Arrange
            var existing = new StaffJob { StaffId = "S001", JobCode = "JOB001" };
            var repo = CreateRepository(new List<StaffJob> { existing });

            // Act
            var result = await repo.DeleteAsync("S001", "JOB_WRONG");

            // Assert
            Assert.False(result);
        }

        #endregion
    }
}