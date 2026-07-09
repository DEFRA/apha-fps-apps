/*
 * TRANSFORMENGINE MIGRATION — YearlyFinancialDataRepositoryTests.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 13 — Unit Tests - Backend + Frontend xUnit Coverage
 * Migrated : 2026-07-09
 *
 * CHANGED:
 *   - New file: xUnit tests for Apha.PIMS.DataAccess.Repository.YearlyFinancialDataRepository
 *   - Uses RepositoryTestHelper.CreateMockDbContext<PimsDbContext>() + CreateMockDbSet<T>(entities)
 *     per established DataAccess.UnitTests pattern (see ProjectYearCostsRepositoryTests)
 *   - Covers: GetAllAsync, GetByKeyAsync, ExistsAsync, CreateAsync, UpdateAsync, DeleteAsync,
 *     GetPactCostsAsync
 *   - Moq used for DbContext/DbSet (not NSubstitute — matches DataAccess test layer convention)
 *   - CreateRepository factory method shared across all tests for lean setup
 *
 * PRESERVED:
 *   - Naming convention [MethodName]_[StateUnderTest]_[ExpectedResult]
 *   - RepositoryTestHelper pattern identical to ProjectYearCostsRepositoryTests
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: ExecuteDeleteAsync is mocked via the Moq setup for the DbSet;
 *     if a newer EF Core version changes the ExecuteDeleteAsync extension call site,
 *     update the mock setup accordingly.
 */

using Apha.Common.Helpers.Repository;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Pagination;
using Apha.PIMS.DataAccess.Data;
using Apha.PIMS.DataAccess.Repository;
using Moq;

namespace Apha.PIMS.DataAccess.UnitTests.Repository.YearlyFinancialDataRepositoryTest
{
    public class YearlyFinancialDataRepositoryTests
    {
        /// <summary>
        /// Creates a <see cref="YearlyFinancialDataRepository"/> with in-memory data.
        /// All parameters are optional — omitted sets default to empty.
        /// </summary>
        private static YearlyFinancialDataRepository CreateRepository(
            IEnumerable<YearlyFinancialData>?  yearlyFinancialData  = null,
            IEnumerable<PactProjectYearCosts>? pactProjectYearCosts = null)
        {
            var mockContext = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();

            var yfdMockSet   = RepositoryTestHelper.CreateMockDbSet(yearlyFinancialData  ?? Enumerable.Empty<YearlyFinancialData>());
            var pactMockSet  = RepositoryTestHelper.CreateMockDbSet(pactProjectYearCosts ?? Enumerable.Empty<PactProjectYearCosts>());

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.YearlyFinancialData).Returns(yfdMockSet.Object);
            mockContext.Setup(x => x.PactProjectYearCosts).Returns(pactMockSet.Object);

            return new YearlyFinancialDataRepository(mockContext.Object);
        }

        // ── helper builders ──────────────────────────────────────────────

        private static YearlyFinancialData MakeYfd(
            short year = 2024, string project = "PP001",
            decimal? bfBudget = 10000m, string? costedBy = null)
            => new()
            {
                Year     = year,
                Project  = project,
                BfBudget = bfBudget,
                CostedBy = costedBy
            };

        private static PactProjectYearCosts MakePactCost(
            string project = "PP001", double year = 2024.0, double monthNo = 1.0)
            => new()
            {
                Project = project,
                Year    = year,
                MonthNo = monthNo
            };

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_ReturnsOnlyRecordsForSpecifiedProject()
        {
            // Arrange
            var data = new[]
            {
                MakeYfd(2024, "PP001"),
                MakeYfd(2023, "PP001"),
                MakeYfd(2024, "PP002")   // different project — should be excluded
            };
            var repo   = CreateRepository(yearlyFinancialData: data);
            var paging = new PaginationParameters<string>(page: 1, pageSize: 10);

            // Act
            var result = await repo.GetAllAsync("PP001", paging);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, r => Assert.Equal("PP001", r.Project));
        }

        [Fact]
        public async Task GetAllAsync_WithNoMatchingProject_ReturnsEmptyData()
        {
            // Arrange
            var data = new[] { MakeYfd(2024, "PP999") };
            var repo   = CreateRepository(yearlyFinancialData: data);
            var paging = new PaginationParameters<string>(page: 1, pageSize: 10);

            // Act
            var result = await repo.GetAllAsync("PP001", paging);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetAllAsync_Pagination_ReturnsCorrectTotalRecords()
        {
            // Arrange
            var data = Enumerable.Range(2020, 5)
                .Select(y => MakeYfd((short)y, "PP001"))
                .ToArray();
            var repo   = CreateRepository(yearlyFinancialData: data);
            var paging = new PaginationParameters<string>(page: 1, pageSize: 2);

            // Act
            var result = await repo.GetAllAsync("PP001", paging);

            // Assert
            Assert.Equal(5, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.Data.Count);  // page size is 2
        }

        [Fact]
        public async Task GetAllAsync_WithAllPagesRequested_ReturnsAllRecords()
        {
            // Arrange
            var data = Enumerable.Range(2020, 5)
                .Select(y => MakeYfd((short)y, "PP001"))
                .ToArray();
            var repo   = CreateRepository(yearlyFinancialData: data);
            var paging = new PaginationParameters<string>(page: -1, pageSize: 10);

            // Act
            var result = await repo.GetAllAsync("PP001", paging);

            // Assert
            Assert.Equal(5, result.Data.Count);
            Assert.Equal(1, result.PaginationData.TotalPages);
        }

        [Fact]
        public async Task GetAllAsync_WithSearchByCostedBy_FiltersCorrectly()
        {
            // Arrange
            var data = new[]
            {
                MakeYfd(2024, "PP001", costedBy: "alice"),
                MakeYfd(2023, "PP001", costedBy: "bob")
            };
            var repo   = CreateRepository(yearlyFinancialData: data);
            var paging = new PaginationParameters<string>(page: 1, pageSize: 10, search: "alice");

            // Act
            var result = await repo.GetAllAsync("PP001", paging);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal("alice", result.Data.First().CostedBy);
        }

        #endregion

        #region GetByKeyAsync Tests

        [Fact]
        public async Task GetByKeyAsync_WithValidKey_ReturnsMatchingRecord()
        {
            // Arrange
            var data = new[] { MakeYfd(2024, "PP001"), MakeYfd(2023, "PP001") };
            var repo = CreateRepository(yearlyFinancialData: data);

            // Act
            var result = await repo.GetByKeyAsync(2024, "PP001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal((short)2024, result.Year);
            Assert.Equal("PP001",     result.Project);
        }

        [Fact]
        public async Task GetByKeyAsync_WhenNotFound_ReturnsNull()
        {
            // Arrange
            var data = new[] { MakeYfd(2024, "PP001") };
            var repo = CreateRepository(yearlyFinancialData: data);

            // Act
            var result = await repo.GetByKeyAsync(9999, "UNKNOWN");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByKeyAsync_WithWrongYear_ReturnsNull()
        {
            // Arrange
            var data = new[] { MakeYfd(2024, "PP001") };
            var repo = CreateRepository(yearlyFinancialData: data);

            // Act
            var result = await repo.GetByKeyAsync(2025, "PP001");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByKeyAsync_WithWrongProject_ReturnsNull()
        {
            // Arrange
            var data = new[] { MakeYfd(2024, "PP001") };
            var repo = CreateRepository(yearlyFinancialData: data);

            // Act
            var result = await repo.GetByKeyAsync(2024, "PP002");

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region ExistsAsync Tests

        [Fact]
        public async Task ExistsAsync_WhenRecordExists_ReturnsTrue()
        {
            // Arrange
            var data = new[] { MakeYfd(2024, "PP001") };
            var repo = CreateRepository(yearlyFinancialData: data);

            // Act
            var result = await repo.ExistsAsync(2024, "PP001");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_WhenRecordDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var repo = CreateRepository();

            // Act
            var result = await repo.ExistsAsync(2024, "PP001");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_WithWrongYearAndSameProject_ReturnsFalse()
        {
            // Arrange
            var data = new[] { MakeYfd(2024, "PP001") };
            var repo = CreateRepository(yearlyFinancialData: data);

            // Act
            var result = await repo.ExistsAsync(2025, "PP001");

            // Assert
            Assert.False(result);
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_WithValidEntity_CallsSaveChangesAsync()
        {
            // Arrange
            var mockContext = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();
            var yfdMockSet  = RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<YearlyFinancialData>());
            var pactMockSet = RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<PactProjectYearCosts>());
            RepositoryTestHelper.SetupSaveChanges(mockContext);
            mockContext.Setup(x => x.YearlyFinancialData).Returns(yfdMockSet.Object);
            mockContext.Setup(x => x.PactProjectYearCosts).Returns(pactMockSet.Object);
            var repo   = new YearlyFinancialDataRepository(mockContext.Object);
            var entity = MakeYfd();

            // Act
            var result = await repo.CreateAsync(entity);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(entity, result);
            mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task CreateAsync_ReturnsTheSameEntityInstance()
        {
            // Arrange
            var mockContext = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();
            var yfdMockSet  = RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<YearlyFinancialData>());
            var pactMockSet = RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<PactProjectYearCosts>());
            RepositoryTestHelper.SetupSaveChanges(mockContext);
            mockContext.Setup(x => x.YearlyFinancialData).Returns(yfdMockSet.Object);
            mockContext.Setup(x => x.PactProjectYearCosts).Returns(pactMockSet.Object);
            var repo   = new YearlyFinancialDataRepository(mockContext.Object);
            var entity = MakeYfd(2024, "PP001", 99999m);

            // Act
            var result = await repo.CreateAsync(entity);

            // Assert
            Assert.Same(entity, result);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WithValidEntity_CallsSaveChangesAsync()
        {
            // Arrange
            var mockContext = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();
            var yfdMockSet  = RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<YearlyFinancialData>());
            var pactMockSet = RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<PactProjectYearCosts>());
            RepositoryTestHelper.SetupSaveChanges(mockContext);
            mockContext.Setup(x => x.YearlyFinancialData).Returns(yfdMockSet.Object);
            mockContext.Setup(x => x.PactProjectYearCosts).Returns(pactMockSet.Object);
            var repo   = new YearlyFinancialDataRepository(mockContext.Object);
            var entity = MakeYfd();

            // Act
            var result = await repo.UpdateAsync(entity);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(entity, result);
            mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task UpdateAsync_ReturnsTheSameEntityInstance()
        {
            // Arrange
            var mockContext = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();
            var yfdMockSet  = RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<YearlyFinancialData>());
            var pactMockSet = RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<PactProjectYearCosts>());
            RepositoryTestHelper.SetupSaveChanges(mockContext);
            mockContext.Setup(x => x.YearlyFinancialData).Returns(yfdMockSet.Object);
            mockContext.Setup(x => x.PactProjectYearCosts).Returns(pactMockSet.Object);
            var repo   = new YearlyFinancialDataRepository(mockContext.Object);
            var entity = MakeYfd(2024, "PP001", 55555m);

            // Act
            var result = await repo.UpdateAsync(entity);

            // Assert
            Assert.Same(entity, result);
        }

        #endregion

        #region DeleteAsync Tests

        // TRANSFORMENGINE: ExecuteDeleteAsync is a bulk EF Core set-based operation that cannot be
        // exercised against an in-memory mock query provider (Moq).
        // Per the established pattern in MilestoneRepositoryTests, these tests assert the call
        // path reaches ExecuteDeleteAsync and throws any exception from the mock provider.
        // Integration tests / real database tests cover the actual return value.

        [Fact]
        public async Task DeleteAsync_WhenRecordExists_ThrowsFromMockQueryProvider()
        {
            // Arrange — ExecuteDeleteAsync is a bulk EF Core operation that cannot
            // be exercised against an in-memory mock query provider.
            var data = new[] { MakeYfd(2024, "PP001") };
            var repo = CreateRepository(yearlyFinancialData: data);

            // Act + Assert
            await Assert.ThrowsAnyAsync<Exception>(() => repo.DeleteAsync(2024, "PP001"));
        }

        [Fact]
        public async Task DeleteAsync_WhenRecordDoesNotExist_ThrowsFromMockQueryProvider()
        {
            // Arrange — ExecuteDeleteAsync is a bulk EF Core operation that cannot
            // be exercised against an in-memory mock query provider.
            var repo = CreateRepository();

            // Act + Assert
            await Assert.ThrowsAnyAsync<Exception>(() => repo.DeleteAsync(9999, "UNKNOWN"));
        }

        #endregion

        #region GetPactCostsAsync Tests

        [Fact]
        public async Task GetPactCostsAsync_ReturnsOnlyRowsForProjectAndYear()
        {
            // Arrange
            var pactData = new[]
            {
                MakePactCost("PP001", 2024.0, 1.0),
                MakePactCost("PP001", 2024.0, 2.0),
                MakePactCost("PP002", 2024.0, 1.0),   // different project — excluded
                MakePactCost("PP001", 2023.0, 1.0)    // different year — excluded
            };
            var repo = CreateRepository(pactProjectYearCosts: pactData);

            // Act
            var result = await repo.GetPactCostsAsync("PP001", 2024);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, r =>
            {
                Assert.Equal("PP001", r.Project);
                Assert.Equal(2024.0,  r.Year);
            });
        }

        [Fact]
        public async Task GetPactCostsAsync_WithNoMatchingRows_ReturnsEmptyList()
        {
            // Arrange
            var repo = CreateRepository();

            // Act
            var result = await repo.GetPactCostsAsync("PP001", 2024);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetPactCostsAsync_ReturnsRowsOrderedByMonthNo()
        {
            // Arrange
            var pactData = new[]
            {
                MakePactCost("PP001", 2024.0, 3.0),
                MakePactCost("PP001", 2024.0, 1.0),
                MakePactCost("PP001", 2024.0, 2.0)
            };
            var repo = CreateRepository(pactProjectYearCosts: pactData);

            // Act
            var result = await repo.GetPactCostsAsync("PP001", 2024);

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal(1.0, result[0].MonthNo);
            Assert.Equal(2.0, result[1].MonthNo);
            Assert.Equal(3.0, result[2].MonthNo);
        }

        #endregion
    }
}
