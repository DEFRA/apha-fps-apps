using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Apha.FPS.DataAccess.UnitTests.Repository.TotalBusinessOverheadsRepositoryTest
{
    public class TotalBusinessOverheadsRepositoryTests
    {
        #region Helpers

        private static TotalBusinessOverheads BuildEntity(
            decimal? overheads = 1000000m,
            int fpsYear = 2025) =>
            new()
            {
                BusinessOverheads = overheads,
                FpsYear = fpsYear
            };

        private static TotalBusinessOverheadsRepository CreateRepository(IEnumerable<TotalBusinessOverheads>? data = null)
        {
            var mockFpsYearContext = new Mock<IFpsRequestContext>();
            mockFpsYearContext.Setup(x => x.FpsYear).Returns(2025);
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockFpsYearContext.Object);

            if (data != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(data);
                RepositoryTestHelper.SetupDbSetOperations(mockSet);
                mockContext.Setup(x => x.TotalBusinessOverheads).Returns(mockSet.Object);
            }

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            return new TotalBusinessOverheadsRepository(mockContext.Object);
        }

        private static (
            TotalBusinessOverheadsRepository Repo,
            Mock<DbSet<TotalBusinessOverheads>> DbSet,
            Mock<FpsDbContext> Context)
            CreateRepositoryWithMocks(IEnumerable<TotalBusinessOverheads>? data = null)
        {
            var mockFpsYearContext = new Mock<IFpsRequestContext>();
            mockFpsYearContext.Setup(x => x.FpsYear).Returns(2025);
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockFpsYearContext.Object);

            var dbSet = RepositoryTestHelper.CreateMockDbSet(data ?? []);
            RepositoryTestHelper.SetupDbSetOperations(dbSet);
            mockContext.Setup(x => x.TotalBusinessOverheads).Returns(dbSet.Object);

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            var repo = new TotalBusinessOverheadsRepository(mockContext.Object);
            return (repo, dbSet, mockContext);
        }

        #endregion

        #region GetByYearAsync Tests

        [Fact]
        public async Task GetByYearAsync_ReturnsNull_WhenRecordNotFound()
        {
            // Arrange
            var data = new List<TotalBusinessOverheads>
            {
                BuildEntity(fpsYear: 2024),
                BuildEntity(fpsYear: 2026)
            };
            var repo = CreateRepository(data);

            // Act
            var result = await repo.GetByYearAsync(2025);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByYearAsync_ReturnsEntity_WhenRecordFound()
        {
            // Arrange
            var data = new List<TotalBusinessOverheads>
            {
                BuildEntity(1000000m, 2025)
            };
            var repo = CreateRepository(data);

            // Act
            var result = await repo.GetByYearAsync(2025);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2025, result.FpsYear);
            Assert.Equal(1000000m, result.BusinessOverheads);
        }

        [Fact]
        public async Task GetByYearAsync_ReturnsCorrectYear_WhenMultipleYearsExist()
        {
            // Arrange
            var data = new List<TotalBusinessOverheads>
            {
                BuildEntity(900000m, 2024),
                BuildEntity(1000000m, 2025),
                BuildEntity(1100000m, 2026)
            };
            var repo = CreateRepository(data);

            // Act
            var result = await repo.GetByYearAsync(2025);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2025, result.FpsYear);
            Assert.Equal(1000000m, result.BusinessOverheads);
        }

        [Fact]
        public async Task GetByYearAsync_WithNullOverheads_ReturnsEntityWithNullOverheads()
        {
            // Arrange
            var data = new List<TotalBusinessOverheads>
            {
                BuildEntity(null, 2025)
            };
            var repo = CreateRepository(data);

            // Act
            var result = await repo.GetByYearAsync(2025);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.BusinessOverheads);
        }

        [Fact]
        public async Task GetByYearAsync_WithZeroOverheads_ReturnsEntityWithZeroOverheads()
        {
            // Arrange
            var data = new List<TotalBusinessOverheads>
            {
                BuildEntity(0m, 2025)
            };
            var repo = CreateRepository(data);

            // Act
            var result = await repo.GetByYearAsync(2025);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0m, result.BusinessOverheads);
        }

        [Fact]
        public async Task GetByYearAsync_ReturnsAsNoTracking()
        {
            // Arrange
            var data = new List<TotalBusinessOverheads>
            {
                BuildEntity(fpsYear: 2025)
            };
            var repo = CreateRepository(data);

            // Act
            var result = await repo.GetByYearAsync(2025);

            // Assert
            Assert.NotNull(result);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ThrowsArgumentNullException_WhenEntityIsNull()
        {
            // Arrange
            var (repo, _, _) = CreateRepositoryWithMocks([]);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.UpdateAsync(null!));
        }

        [Fact]
        public async Task UpdateAsync_ThrowsInvalidOperationException_WhenRecordNotFound()
        {
            // Arrange
            var (repo, _, _) = CreateRepositoryWithMocks([]);
            var entity = BuildEntity(fpsYear: 2025);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => repo.UpdateAsync(entity));
            Assert.Contains("Total Business Overheads record for year 2025 not found", exception.Message);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesBusinessOverheadsAndReturnsSame()
        {
            // Arrange
            var existing = BuildEntity(1000000m, 2025);
            var (repo, _, mockContext) = CreateRepositoryWithMocks(new[] { existing });

            var entityToUpdate = BuildEntity(1500000m, 2025);

            // Act
            var result = await repo.UpdateAsync(entityToUpdate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1500000m, result.BusinessOverheads);
            Assert.Equal(2025, result.FpsYear);
            mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_CallsSaveChangesAsync()
        {
            // Arrange
            var existing = BuildEntity(1000000m, 2025);
            var (repo, _, mockContext) = CreateRepositoryWithMocks(new[] { existing });

            var entityToUpdate = BuildEntity(1500000m, 2025);

            // Act
            await repo.UpdateAsync(entityToUpdate);

            // Assert
            mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WithNullOverheads_UpdatesToNull()
        {
            // Arrange
            var existing = BuildEntity(1000000m, 2025);
            var (repo, _, mockContext) = CreateRepositoryWithMocks(new[] { existing });

            var entityToUpdate = BuildEntity(null, 2025);

            // Act
            var result = await repo.UpdateAsync(entityToUpdate);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.BusinessOverheads);
            mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WithZeroOverheads_UpdatesToZero()
        {
            // Arrange
            var existing = BuildEntity(1000000m, 2025);
            var (repo, _, mockContext) = CreateRepositoryWithMocks(new[] { existing });

            var entityToUpdate = BuildEntity(0m, 2025);

            // Act
            var result = await repo.UpdateAsync(entityToUpdate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0m, result.BusinessOverheads);
            mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WithLargeValue_UpdatesSuccessfully()
        {
            // Arrange
            var existing = BuildEntity(1000000m, 2025);
            var (repo, _, mockContext) = CreateRepositoryWithMocks(new[] { existing });

            var entityToUpdate = BuildEntity(999999999.99m, 2025);

            // Act
            var result = await repo.UpdateAsync(entityToUpdate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(999999999.99m, result.BusinessOverheads);
            mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_OnlyUpdatesBusinessOverheadsField()
        {
            // Arrange
            var existing = BuildEntity(1000000m, 2025);
            var (repo, _, _) = CreateRepositoryWithMocks(new[] { existing });

            var entityToUpdate = BuildEntity(1500000m, 2025);

            // Act
            var result = await repo.UpdateAsync(entityToUpdate);

            // Assert
            Assert.Equal(2025, result.FpsYear);
            Assert.Equal(1500000m, result.BusinessOverheads);
        }

        [Fact]
        public async Task UpdateAsync_WithDifferentYear_ThrowsInvalidOperationException()
        {
            // Arrange
            var existing = BuildEntity(1000000m, 2025);
            var (repo, _, _) = CreateRepositoryWithMocks(new[] { existing });

            var entityToUpdate = BuildEntity(1500000m, 2026);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => repo.UpdateAsync(entityToUpdate));
            Assert.Contains("Total Business Overheads record for year 2026 not found", exception.Message);
        }

        [Fact]
        public async Task UpdateAsync_WithMultipleRecords_UpdatesOnlyMatchingYear()
        {
            // Arrange
            var data = new List<TotalBusinessOverheads>
            {
                BuildEntity(900000m, 2024),
                BuildEntity(1000000m, 2025),
                BuildEntity(1100000m, 2026)
            };
            var (repo, _, mockContext) = CreateRepositoryWithMocks(data);

            var entityToUpdate = BuildEntity(1500000m, 2025);

            // Act
            var result = await repo.UpdateAsync(entityToUpdate);

            // Assert
            Assert.Equal(2025, result.FpsYear);
            Assert.Equal(1500000m, result.BusinessOverheads);
            mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
        }

        #endregion
    }
}
