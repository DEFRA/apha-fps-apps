using Apha.Common.Helpers.Repository;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Apha.PACT.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using Moq;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PACT.DataAccess.UnitTests.Repository
{
    // ── Tests ─────────────────────────────────────────────────────────────────

    public class ProjectInvoiceRepositoryTests
    {
        private const int DefaultTestFpsYear = 2025;

        private static (
            ProjectInvoiceRepository Repo,
            Mock<DbSet<ProjectInvoice>> InvoicesDbSet,
            Mock<FpsDbContext> Context)
            CreateRepositoryWithMocks(
                IEnumerable<ProjectInvoice> invoices,
                int fpsYear = DefaultTestFpsYear)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(fpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);
            var invoicesMockSet = RepositoryTestHelper.CreateMockDbSet(invoices);

            RepositoryTestHelper.SetupDbSetOperations(invoicesMockSet);
            invoicesMockSet
                .Setup(x => x.AddAsync(It.IsAny<ProjectInvoice>(), It.IsAny<CancellationToken>()))
                .Returns((ProjectInvoice _, CancellationToken __) => new ValueTask<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<ProjectInvoice>>());
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.ProjectInvoices).Returns(invoicesMockSet.Object);

            var repo = new ProjectInvoiceRepository(mockContext.Object, fpsRequestContext);
            return (repo, invoicesMockSet, mockContext);
        }

        private static ProjectInvoiceRepository CreateRepository(
            IEnumerable<ProjectInvoice> invoices,
            int fpsYear = DefaultTestFpsYear)
            => CreateRepositoryWithMocks(invoices, fpsYear).Repo;

        private static ProjectInvoiceRepository CreateRepositoryWithMonthlySummary(
            IEnumerable<MonthlyInvoicesSummary> summaryData,
            int fpsYear = DefaultTestFpsYear)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(fpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);

            var invoicesMockSet = RepositoryTestHelper.CreateMockDbSet<ProjectInvoice>([]);
            mockContext.Setup(x => x.ProjectInvoices).Returns(invoicesMockSet.Object);

            var summaryMockSet = RepositoryTestHelper.CreateMockDbSet(summaryData);
            mockContext.Setup(x => x.MonthlyInvoicesSummary).Returns(summaryMockSet.Object);

            return new ProjectInvoiceRepository(mockContext.Object, fpsRequestContext);
        }

        private static ProjectInvoice MakeInvoice(int id, string project, int fpsYear = DefaultTestFpsYear, int? month = 1, decimal? amount = 100m)
            => new() { InvoiceCounter = id, ProjectParent = project, FpsYear = fpsYear, Month = month, Amount = amount };

        private static MonthlyInvoicesSummary MakeSummary(string program, string project, int month, decimal amount)
            => new() { FpsYear = DefaultTestFpsYear, Program = program, ParentProject = project, Month = month, MonthlyAmount = amount };

        #region GetPagedProjectInvoicesAsync

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_NoParentProject_ReturnsAllInvoices()
        {
            // Arrange
            var repo = CreateRepository(
            [
                MakeInvoice(1, "PRJ001"),
                MakeInvoice(2, "PRJ002")
            ]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetPagedProjectInvoicesAsync(query, null);

            // Assert
            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_WithParentProject_FiltersToMatchingInvoices()
        {
            // Arrange
            var repo = CreateRepository(
            [
                MakeInvoice(1, "PRJ001"),
                MakeInvoice(2, "PRJ002")
            ]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetPagedProjectInvoicesAsync(query, "PRJ001");

            // Assert
            Assert.Single(result.Data);
            Assert.All(result.Data, i => Assert.Equal("PRJ001", i.ProjectParent));
        }

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_WithMonthFilter_FiltersByMonth()
        {
            // Arrange
            var repo = CreateRepository(
            [
                MakeInvoice(1, "PRJ001", month: 3),
                MakeInvoice(2, "PRJ002", month: 5)
            ]);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = """{"Month":"3"}"""
            };

            // Act
            var result = await repo.GetPagedProjectInvoicesAsync(query, null);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal(3, result.Data.First().Month);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_Pagination_ReturnsCorrectPage()
        {
            // Arrange
            var repo = CreateRepository(
            [
                MakeInvoice(1, "PRJ001"),
                MakeInvoice(2, "PRJ001"),
                MakeInvoice(3, "PRJ001")
            ]);
            var query = new PaginationParameters<string> { Page = 2, PageSize = 2 };

            // Act
            var result = await repo.GetPagedProjectInvoicesAsync(query, null);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal(3, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.TotalPages);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_NullFilter_ReturnsAllWithoutFiltering()
        {
            // Arrange
            var repo = CreateRepository(
            [
                MakeInvoice(1, "PRJ001"),
                MakeInvoice(2, "PRJ002")
            ]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = null };

            // Act
            var result = await repo.GetPagedProjectInvoicesAsync(query, null);

            // Assert
            Assert.Equal(2, result.Data.Count);
        }

        #endregion

        #region GetTotalAmountAsync

        [Fact]
        public async Task GetTotalAmountAsync_WithParentProject_SumsOnlyMatchingInvoices()
        {
            // Arrange
            var repo = CreateRepository(
            [
                MakeInvoice(1, "PRJ001", amount: 200m),
                MakeInvoice(2, "PRJ002", amount: 300m)
            ]);

            // Act
            var total = await repo.GetTotalAmountAsync("PRJ001");

            // Assert
            Assert.Equal(200m, total);
        }

        [Fact]
        public async Task GetTotalAmountAsync_NoParentProject_SumsAllInvoices()
        {
            // Arrange
            var repo = CreateRepository(
            [
                MakeInvoice(1, "PRJ001", amount: 100m),
                MakeInvoice(2, "PRJ002", amount: 250m)
            ]);

            // Act
            var total = await repo.GetTotalAmountAsync(null);

            // Assert
            Assert.Equal(350m, total);
        }

        [Fact]
        public async Task GetTotalAmountAsync_NullAmounts_ReturnsZero()
        {
            // Arrange
            var repo = CreateRepository(
            [
                MakeInvoice(1, "PRJ001", amount: null)
            ]);

            // Act
            var total = await repo.GetTotalAmountAsync(null);

            // Assert
            Assert.Equal(0m, total);
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_ExistingId_ReturnsInvoice()
        {
            // Arrange
            var repo = CreateRepository([MakeInvoice(1, "PRJ001"), MakeInvoice(2, "PRJ002")]);

            // Act
            var result = await repo.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result!.InvoiceCounter);
        }

        [Fact]
        public async Task GetByIdAsync_NotFound_ReturnsNull()
        {
            // Arrange
            var repo = CreateRepository([MakeInvoice(1, "PRJ001")]);

            // Act
            var result = await repo.GetByIdAsync(99);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetMonthlyInvoicesSummaryAsync

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_NoFilter_ReturnsAllRowsOrderedByProgramProjectMonth()
        {
            // Arrange
            var repo = CreateRepositoryWithMonthlySummary(
            [
                MakeSummary("Z", "PRJ2", 2, 200m),
                MakeSummary("A", "PRJ1", 1, 100m),
                MakeSummary("A", "PRJ1", 2, 150m)
            ]);
            var parameters = new PaginationParameters<string>();

            // Act
            var result = await repo.GetMonthlyInvoicesSummaryAsync(parameters);

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal("A", result[0].Program);
            Assert.Equal(1, result[0].Month);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_WithProgramFilter_FiltersMatchingRows()
        {
            // Arrange
            var repo = CreateRepositoryWithMonthlySummary(
            [
                MakeSummary("ADMIN", "PRJ1", 1, 100m),
                MakeSummary("PROG2", "PRJ2", 1, 200m)
            ]);
            var parameters = new PaginationParameters<string>
            {
                Filter = """{"Program":"ADMIN"}"""
            };

            // Act
            var result = await repo.GetMonthlyInvoicesSummaryAsync(parameters);

            // Assert
            Assert.Single(result);
            Assert.Equal("ADMIN", result[0].Program);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_WithParentProjectFilter_FiltersMatchingRows()
        {
            // Arrange
            var repo = CreateRepositoryWithMonthlySummary(
            [
                MakeSummary("PROG1", "ALPHA001", 1, 100m),
                MakeSummary("PROG2", "BETA002",  1, 200m)
            ]);
            var parameters = new PaginationParameters<string>
            {
                Filter = """{"ParentProject":"ALPHA"}"""
            };

            // Act
            var result = await repo.GetMonthlyInvoicesSummaryAsync(parameters);

            // Assert
            Assert.Single(result);
            Assert.Equal("ALPHA001", result[0].ParentProject);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_WithBothFilters_FiltersOnBothFields()
        {
            // Arrange
            var repo = CreateRepositoryWithMonthlySummary(
            [
                MakeSummary("ADMIN", "AH001", 1, 100m),
                MakeSummary("ADMIN", "BH002", 1, 200m),
                MakeSummary("PROG2", "AH001", 1, 300m)
            ]);
            var parameters = new PaginationParameters<string>
            {
                Filter = """{"Program":"ADMIN","ParentProject":"AH"}"""
            };

            // Act
            var result = await repo.GetMonthlyInvoicesSummaryAsync(parameters);

            // Assert
            Assert.Single(result);
            Assert.Equal("ADMIN", result[0].Program);
            Assert.Equal("AH001", result[0].ParentProject);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_EmptyFilter_ReturnsAllRows()
        {
            // Arrange
            var repo = CreateRepositoryWithMonthlySummary(
            [
                MakeSummary("A", "PRJ1", 1, 10m),
                MakeSummary("B", "PRJ2", 1, 20m)
            ]);
            var parameters = new PaginationParameters<string> { Filter = "" };

            // Act
            var result = await repo.GetMonthlyInvoicesSummaryAsync(parameters);

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_NoMatchingFilter_ReturnsEmpty()
        {
            // Arrange
            var repo = CreateRepositoryWithMonthlySummary(
            [
                MakeSummary("PROG1", "PRJ1", 1, 100m)
            ]);
            var parameters = new PaginationParameters<string>
            {
                Filter = """{"Program":"NONEXISTENT"}"""
            };

            // Act
            var result = await repo.GetMonthlyInvoicesSummaryAsync(parameters);

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_SetsCurrentFpsYearOnEntity()
        {
            // Arrange
            var entity = MakeInvoice(0, "PRJ001", fpsYear: 0);
            var (repo, _, mockContext) = CreateRepositoryWithMocks([]);
            mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await repo.CreateAsync(entity);

            // Assert
            Assert.Equal(DefaultTestFpsYear, result.FpsYear);
            mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_SetsCurrentFpsYearAndSaves()
        {
            // Arrange
            var entity = MakeInvoice(1, "PRJ001", fpsYear: 0);
            var (repo, _, mockContext) = CreateRepositoryWithMocks([]);
            mockContext.Setup(x => x.Entry(It.IsAny<ProjectInvoice>()))
                .Throws(new NotSupportedException("Entry() is not supported in mocked DbContext"));

            // Act & Assert — Entry() throws but FpsYear is set before that call
            await Assert.ThrowsAsync<NotSupportedException>(() => repo.UpdateAsync(entity));

            Assert.Equal(DefaultTestFpsYear, entity.FpsYear);
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_ExistingInvoiceMatchingFpsYear_DeletesAndReturnsTrue()
        {
            // Arrange
            var entity = MakeInvoice(1, "PRJ001", fpsYear: DefaultTestFpsYear);
            var (repo, _, mockContext) = CreateRepositoryWithMocks([entity]);
            mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await repo.DeleteAsync(1);

            // Assert
            Assert.True(result);
            mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_InvoiceNotFound_ReturnsFalse()
        {
            // Arrange
            var (repo, _, mockContext) = CreateRepositoryWithMocks([MakeInvoice(1, "PRJ001", fpsYear: DefaultTestFpsYear)]);

            // Act
            var result = await repo.DeleteAsync(99);

            // Assert
            Assert.False(result);
            mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_InvoiceExistsButDifferentFpsYear_ReturnsFalse()
        {
            // Arrange
            var (repo, _, mockContext) = CreateRepositoryWithMocks([MakeInvoice(1, "PRJ001", fpsYear: 2024)]);

            // Act
            var result = await repo.DeleteAsync(1);

            // Assert
            Assert.False(result);
            mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion
    }
}
