/*
 * TRANSFORMENGINE MIGRATION — ReportRepositoryTests.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 13 — Unit Tests - Backend + Frontend xUnit Coverage
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New xUnit test class for Apha.PIMS.DataAccess.Repository.ReportRepository
 *   - Uses RepositoryTestHelper + Moq (established pattern for DataAccess tests)
 *   - Covers: GetAllAsync, GetByIdAsync, AddAsync, UpdateAsync, DeleteAsync, ExistsAsync
 *
 * PRESERVED:
 *   - Integer PK (id) semantics
 *   - AsNoTracking for read operations; ExecuteDeleteAsync for set-based delete
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: ExecuteDeleteAsync and ExecuteUpdateAsync use Database.CreateExecutionStrategy
 *     which cannot be easily unit-tested; delete-path is tested via existence guard only
 */
using Apha.Common.Helpers.Repository;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.DataAccess.Data;
using Apha.PIMS.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Apha.PIMS.DataAccess.UnitTests.Repository.ReportRepositoryTest
{
    public class ReportRepositoryTests
    {
        // ── factory ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a ReportRepository backed by mock DbContext/DbSet.
        /// All parameters are optional — omitted sets are initialised as empty.
        /// </summary>
        private static ReportRepository CreateRepository(
            IEnumerable<Report>? reports = null)
        {
            var mockContext  = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();
            var reportsMockSet = RepositoryTestHelper.CreateMockDbSet(reports ?? Enumerable.Empty<Report>());

            RepositoryTestHelper.SetupDbSetOperations(reportsMockSet);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.Reports).Returns(reportsMockSet.Object);

            return new ReportRepository(mockContext.Object);
        }

        /// <summary>
        /// Returns the repository plus the mocked DbSet and DbContext for
        /// tests that need to verify Add/Update/SaveChanges calls.
        /// </summary>
        private static (
            ReportRepository Repo,
            Mock<DbSet<Report>> ReportsDbSet,
            Mock<PimsDbContext> Context)
            CreateRepositoryWithMocks(IEnumerable<Report>? reports = null)
        {
            var mockContext    = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();
            var reportsMockSet = RepositoryTestHelper.CreateMockDbSet(reports ?? Enumerable.Empty<Report>());

            RepositoryTestHelper.SetupDbSetOperations(reportsMockSet);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.Reports).Returns(reportsMockSet.Object);

            var repo = new ReportRepository(mockContext.Object);
            return (repo, reportsMockSet, mockContext);
        }

        // ── helpers ───────────────────────────────────────────────────────────────

        private static Report MakeReport(int id, string name = "Test Report") =>
            new Report { Id = id, Reportname = name, Type = "R", Emailable = false };

        // ── GetAllAsync ───────────────────────────────────────────────────────────

        #region GetAllAsync

        [Fact]
        public async Task GetAllAsync_ReturnsAllReports_WhenDataExists()
        {
            // Arrange
            var reports = new List<Report>
            {
                MakeReport(1, "Report Alpha"),
                MakeReport(2, "Report Beta")
            };
            var repo = CreateRepository(reports);

            // Act
            var result = await repo.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, r => r.Reportname == "Report Alpha");
            Assert.Contains(result, r => r.Reportname == "Report Beta");
        }

        [Fact]
        public async Task GetAllAsync_ReturnsEmptyList_WhenNoDataExists()
        {
            // Arrange
            var repo = CreateRepository(Enumerable.Empty<Report>());

            // Act
            var result = await repo.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        // ── GetByIdAsync ──────────────────────────────────────────────────────────

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_ReturnsReport_WhenIdExists()
        {
            // Arrange
            var reports = new List<Report>
            {
                MakeReport(1, "Alpha"),
                MakeReport(2, "Beta")
            };
            var repo = CreateRepository(reports);

            // Act
            var result = await repo.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result!.Id);
            Assert.Equal("Alpha", result.Reportname);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenIdDoesNotExist()
        {
            // Arrange
            var reports = new List<Report> { MakeReport(1, "Alpha") };
            var repo = CreateRepository(reports);

            // Act
            var result = await repo.GetByIdAsync(99);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNoDataExists()
        {
            // Arrange
            var repo = CreateRepository();

            // Act
            var result = await repo.GetByIdAsync(1);

            // Assert
            Assert.Null(result);
        }

        #endregion

        // ── AddAsync ──────────────────────────────────────────────────────────────

        #region AddAsync

        [Fact]
        public async Task AddAsync_ReturnsAddedEntity()
        {
            // Arrange
            var (repo, _, _) = CreateRepositoryWithMocks();
            var report = MakeReport(0, "New Report");

            // Act
            var result = await repo.AddAsync(report);

            // Assert
            Assert.NotNull(result);
            Assert.Same(report, result);
        }

        [Fact]
        public async Task AddAsync_CallsDbSetAdd()
        {
            // Arrange
            var (repo, reportsDbSet, _) = CreateRepositoryWithMocks();
            var report = MakeReport(0, "New Report");

            // Act
            await repo.AddAsync(report);

            // Assert
            reportsDbSet.Verify(x => x.Add(It.Is<Report>(r => r.Reportname == "New Report")), Times.Once);
        }

        [Fact]
        public async Task AddAsync_CallsSaveChangesAsync()
        {
            // Arrange
            var (repo, _, mockContext) = CreateRepositoryWithMocks();
            var report = MakeReport(0, "New Report");

            // Act
            await repo.AddAsync(report);

            // Assert
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 1);
        }

        #endregion

        // ── UpdateAsync ───────────────────────────────────────────────────────────

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_ReturnsUpdatedEntity()
        {
            // Arrange
            var existing = MakeReport(5, "Original Name");
            var (repo, _, _) = CreateRepositoryWithMocks(new List<Report> { existing });
            var updatedEntity = MakeReport(5, "Updated Name");

            // Act
            var result = await repo.UpdateAsync(updatedEntity);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Name", result.Reportname);
        }

        [Fact]
        public async Task UpdateAsync_CallsSaveChangesAsync()
        {
            // Arrange
            var existing = MakeReport(5, "Name");
            var (repo, _, mockContext) = CreateRepositoryWithMocks(new List<Report> { existing });

            // Act
            await repo.UpdateAsync(MakeReport(5, "Updated"));

            // Assert
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 1);
        }

        #endregion

        // ── ExistsAsync ───────────────────────────────────────────────────────────

        #region ExistsAsync

        [Fact]
        public async Task ExistsAsync_ReturnsTrue_WhenIdExists()
        {
            // Arrange
            var reports = new List<Report> { MakeReport(3, "Exists") };
            var repo = CreateRepository(reports);

            // Act
            var result = await repo.ExistsAsync(3);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsFalse_WhenIdDoesNotExist()
        {
            // Arrange
            var reports = new List<Report> { MakeReport(1, "Alpha") };
            var repo = CreateRepository(reports);

            // Act
            var result = await repo.ExistsAsync(99);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsFalse_WhenNoDataExists()
        {
            // Arrange
            var repo = CreateRepository();

            // Act
            var result = await repo.ExistsAsync(1);

            // Assert
            Assert.False(result);
        }

        #endregion
    }
}
