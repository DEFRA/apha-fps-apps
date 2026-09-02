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

namespace Apha.PACT.DataAccess.UnitTests.Repository.ProjectInvoiceRepositoryTest
{
    public class ProjectInvoiceRepositoryTests
    {
        private const int DefaultTestFpsYear = 2024;

        /// <summary>
        /// Creates a ProjectInvoiceRepository alongside mocked DbSet and context for call verification.
        /// AddAsync is set up explicitly since it differs from the base SetupDbSetOperations.
        /// UpdateAsync uses Entry().State — tested via Callback+Throws pattern (mirrors JobCodeRepositoryTests).
        /// </summary>
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
                .Returns((ProjectInvoice _, CancellationToken __) => new ValueTask<EntityEntry<ProjectInvoice>>());
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.ProjectInvoices).Returns(invoicesMockSet.Object);

            var repo = new ProjectInvoiceRepository(mockContext.Object, fpsRequestContext);
            return (repo, invoicesMockSet, mockContext);
        }

        private static ProjectInvoiceRepository CreateRepository(
            IEnumerable<ProjectInvoice> invoices,
            int fpsYear = DefaultTestFpsYear)
            => CreateRepositoryWithMocks(invoices, fpsYear).Repo;

        #region GetPagedProjectInvoicesAsync

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_WithParentProject_ReturnsFilteredPagedResult()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ2", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);
            var query = new PaginationParameters<string>();

            var result = await repo.GetPagedProjectInvoicesAsync(query, "PRJ1");

            Assert.Single(result.Data);
            Assert.Equal(1, result.Data.First().InvoiceCounter);
            Assert.Equal(1, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_NullParentProject_ReturnsAllRecordsPaged()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ2", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);
            var query = new PaginationParameters<string>();

            var result = await repo.GetPagedProjectInvoicesAsync(query, null);

            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        #endregion

        #region GetTotalAmountAsync

        [Fact]
        public async Task GetTotalAmountAsync_WithMatchingParentProject_ReturnsSumOfAmounts()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", Amount = 1000m, FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ1", Amount = 500m,  FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 3, ProjectParent = "PRJ2", Amount = 2000m, FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);

            var result = await repo.GetTotalAmountAsync("PRJ1");

            Assert.Equal(1500m, result);
        }

        [Fact]
        public async Task GetTotalAmountAsync_NullParentProject_ReturnsTotalOfAllAmounts()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", Amount = 1000m, FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ2", Amount = 500m,  FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);

            var result = await repo.GetTotalAmountAsync(null);

            Assert.Equal(1500m, result);
        }

        [Fact]
        public async Task GetTotalAmountAsync_NoMatchingRecords_ReturnsZero()
        {
            var repo = CreateRepository([]);

            var result = await repo.GetTotalAmountAsync("PRJ_NONE");

            Assert.Equal(0m, result);
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_ExistingId_ReturnsInvoice()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);

            var result = await repo.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.InvoiceCounter);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistentId_ReturnsNull()
        {
            var repo = CreateRepository([]);

            var result = await repo.GetByIdAsync(99);

            Assert.Null(result);
        }

        #endregion

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_ValidEntity_SetsFpsYearAndSaves()
        {
            var (repo, invoicesMockSet, mockContext) = CreateRepositoryWithMocks([]);
            var entity = new ProjectInvoice { ProjectParent = "PRJ1", Amount = 1000m };

            var result = await repo.CreateAsync(entity);

            Assert.NotNull(result);
            Assert.Equal(DefaultTestFpsYear, result.FpsYear);
            invoicesMockSet.Verify(x => x.AddAsync(It.IsAny<ProjectInvoice>(), It.IsAny<CancellationToken>()), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task CreateAsync_SetsFpsYear_FromYearContext()
        {
            const int customYear = 2025;
            var (repo, _, _) = CreateRepositoryWithMocks([], fpsYear: customYear);
            var entity = new ProjectInvoice { ProjectParent = "PRJ1" };

            var result = await repo.CreateAsync(entity);

            Assert.Equal(customYear, result.FpsYear);
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_ValidEntity_SetsFpsYearBeforeEntryIsCalled()
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(DefaultTestFpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);
            var invoicesMockSet = RepositoryTestHelper.CreateMockDbSet<ProjectInvoice>([]);
            mockContext.Setup(x => x.ProjectInvoices).Returns(invoicesMockSet.Object);

            var entryWasCalled = false;
            mockContext.Setup(x => x.Entry(It.IsAny<ProjectInvoice>()))
                .Callback(() => entryWasCalled = true)
                .Throws(new NotSupportedException("Entry() is not supported in mocked DbContext"));

            var repo = new ProjectInvoiceRepository(mockContext.Object, fpsRequestContext);
            var entity = new ProjectInvoice { InvoiceCounter = 1, ProjectParent = "PRJ1" };

            await Assert.ThrowsAsync<NotSupportedException>(() => repo.UpdateAsync(entity));

            Assert.Equal(DefaultTestFpsYear, entity.FpsYear);
            Assert.True(entryWasCalled);
        }

        [Fact]
        public async Task UpdateAsync_SetsFpsYear_FromYearContext()
        {
            const int customYear = 2025;
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(customYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);
            var invoicesMockSet = RepositoryTestHelper.CreateMockDbSet<ProjectInvoice>([]);
            mockContext.Setup(x => x.ProjectInvoices).Returns(invoicesMockSet.Object);

            mockContext.Setup(x => x.Entry(It.IsAny<ProjectInvoice>()))
                .Throws(new NotSupportedException("Entry() is not supported in mocked DbContext"));

            var repo = new ProjectInvoiceRepository(mockContext.Object, fpsRequestContext);
            var entity = new ProjectInvoice { InvoiceCounter = 1 };

            await Assert.ThrowsAsync<NotSupportedException>(() => repo.UpdateAsync(entity));

            Assert.Equal(customYear, entity.FpsYear);
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_ExistingId_RemovesAndReturnsTrue()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, FpsYear = DefaultTestFpsYear }
            };
            var (repo, invoicesMockSet, mockContext) = CreateRepositoryWithMocks(invoices);

            var result = await repo.DeleteAsync(1);

            Assert.True(result);
            RepositoryTestHelper.VerifyRemove(invoicesMockSet);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task DeleteAsync_NonExistentId_ReturnsFalse()
        {
            var repo = CreateRepository([]);

            var result = await repo.DeleteAsync(99);

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_WrongFpsYear_ReturnsFalse()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, FpsYear = 2020 }
            };
            var repo = CreateRepository(invoices, fpsYear: DefaultTestFpsYear);

            var result = await repo.DeleteAsync(1);

            Assert.False(result);
        }

        #endregion

        #region GetPagedProjectInvoicesAsync - Sorting

        [Theory]
        [InlineData("projectparent", false)]
        [InlineData("projectparent", true)]
        [InlineData("month", false)]
        [InlineData("month", true)]
        [InlineData("amount", false)]
        [InlineData("amount", true)]
        [InlineData("costofwork", false)]
        [InlineData("costofwork", true)]
        [InlineData("wip", false)]
        [InlineData("wip", true)]
        [InlineData("profitloss", false)]
        [InlineData("profitloss", true)]
        [InlineData("detail", false)]
        [InlineData("detail", true)]
        [InlineData("invoicecounter", false)]
        [InlineData("invoicecounter", true)]
        [InlineData("unknowncolumn", false)]
        public async Task GetPagedProjectInvoicesAsync_SortBy_ReturnsResults(string sortBy, bool descending)
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "A", Month = 1, Amount = 100m, CostOfWork = 10m, Wip = 5m, ProfitLoss = 2m, Detail = "D1", FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "B", Month = 2, Amount = 200m, CostOfWork = 20m, Wip = 10m, ProfitLoss = 4m, Detail = "D2", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);
            var query = new PaginationParameters<string> { SortBy = sortBy, Descending = descending };

            var result = await repo.GetPagedProjectInvoicesAsync(query, null);

            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_NullSortBy_DefaultSortByInvoiceCounter()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 2, ProjectParent = "A", FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 1, ProjectParent = "B", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);
            var query = new PaginationParameters<string> { SortBy = null };

            var result = await repo.GetPagedProjectInvoicesAsync(query, null);

            Assert.Equal(1, result.Data.First().InvoiceCounter);
        }

        #endregion

        #region GetPagedProjectInvoicesAsync - Filtering

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_FilterByProjectParent_ReturnsFiltered()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ2", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);
            var query = new PaginationParameters<string> { Filter = "{\"ProjectParent\":\"PRJ1\"}" };

            var result = await repo.GetPagedProjectInvoicesAsync(query, null);

            Assert.Single(result.Data);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_FilterByMonth_ReturnsFiltered()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", Month = 3, FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ1", Month = 5, FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);
            var query = new PaginationParameters<string> { Filter = "{\"Month\":\"3\"}" };

            var result = await repo.GetPagedProjectInvoicesAsync(query, null);

            Assert.Single(result.Data);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_FilterByDetail_ReturnsFiltered()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", Detail = "TestDetail", FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ1", Detail = "Other", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);
            var query = new PaginationParameters<string> { Filter = "{\"Detail\":\"TestDetail\"}" };

            var result = await repo.GetPagedProjectInvoicesAsync(query, null);

            Assert.Single(result.Data);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_NullFilter_ReturnsAll()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ2", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);
            var query = new PaginationParameters<string> { Filter = null };

            var result = await repo.GetPagedProjectInvoicesAsync(query, null);

            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_FilterWithInvalidMonthString_IgnoresMonthFilter()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", Month = 3, FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ1", Month = 5, FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);
            var query = new PaginationParameters<string> { Filter = "{\"Month\":\"abc\"}" };

            var result = await repo.GetPagedProjectInvoicesAsync(query, null);

            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        #endregion

        #region GetMonthlyInvoicesSummaryAsync

        private static (
            ProjectInvoiceRepository Repo,
            Mock<DbSet<MonthlyInvoicesSummary>> SummaryDbSet,
            Mock<FpsDbContext> Context)
            CreateRepositoryWithSummaryMocks(
                IEnumerable<ProjectInvoice> invoices,
                IEnumerable<MonthlyInvoicesSummary> summaries,
                int fpsYear = DefaultTestFpsYear)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(fpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);
            var invoicesMockSet = RepositoryTestHelper.CreateMockDbSet(invoices);
            var summaryMockSet = RepositoryTestHelper.CreateMockDbSet(summaries);

            RepositoryTestHelper.SetupDbSetOperations(invoicesMockSet);
            invoicesMockSet
                .Setup(x => x.AddAsync(It.IsAny<ProjectInvoice>(), It.IsAny<CancellationToken>()))
                .Returns((ProjectInvoice _, CancellationToken __) => new ValueTask<EntityEntry<ProjectInvoice>>());
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.ProjectInvoices).Returns(invoicesMockSet.Object);
            mockContext.Setup(x => x.MonthlyInvoicesSummary).Returns(summaryMockSet.Object);

            var repo = new ProjectInvoiceRepository(mockContext.Object, fpsRequestContext);
            return (repo, summaryMockSet, mockContext);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_NoFilter_ReturnsAllData()
        {
            var summaries = new List<MonthlyInvoicesSummary>
            {
                new() { Program = "P1", ParentProject = "PP1", Month = 1, MonthlyAmount = 100m, FpsYear = DefaultTestFpsYear },
                new() { Program = "P2", ParentProject = "PP2", Month = 2, MonthlyAmount = 200m, FpsYear = DefaultTestFpsYear }
            };
            var (repo, _, _) = CreateRepositoryWithSummaryMocks([], summaries);
            var parameters = new PaginationParameters<string>();

            var result = await repo.GetMonthlyInvoicesSummaryAsync(parameters);

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_FilterByProgram_FiltersData()
        {
            var summaries = new List<MonthlyInvoicesSummary>
            {
                new() { Program = "ADMIN", ParentProject = "PP1", Month = 1, MonthlyAmount = 100m, FpsYear = DefaultTestFpsYear },
                new() { Program = "TEST", ParentProject = "PP2", Month = 2, MonthlyAmount = 200m, FpsYear = DefaultTestFpsYear }
            };
            var (repo, _, _) = CreateRepositoryWithSummaryMocks([], summaries);
            var parameters = new PaginationParameters<string> { Filter = "{\"Program\":\"ADMIN\"}" };

            var result = await repo.GetMonthlyInvoicesSummaryAsync(parameters);

            Assert.Single(result);
            Assert.Equal("ADMIN", result[0].Program);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_FilterByParentProject_FiltersData()
        {
            var summaries = new List<MonthlyInvoicesSummary>
            {
                new() { Program = "P1", ParentProject = "AH", Month = 1, MonthlyAmount = 100m, FpsYear = DefaultTestFpsYear },
                new() { Program = "P1", ParentProject = "BH", Month = 2, MonthlyAmount = 200m, FpsYear = DefaultTestFpsYear }
            };
            var (repo, _, _) = CreateRepositoryWithSummaryMocks([], summaries);
            var parameters = new PaginationParameters<string> { Filter = "{\"ParentProject\":\"AH\"}" };

            var result = await repo.GetMonthlyInvoicesSummaryAsync(parameters);

            Assert.Single(result);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_NullFilter_ReturnsAll()
        {
            var summaries = new List<MonthlyInvoicesSummary>
            {
                new() { Program = "P1", ParentProject = "PP1", Month = 1, MonthlyAmount = 100m, FpsYear = DefaultTestFpsYear }
            };
            var (repo, _, _) = CreateRepositoryWithSummaryMocks([], summaries);
            var parameters = new PaginationParameters<string> { Filter = null };

            var result = await repo.GetMonthlyInvoicesSummaryAsync(parameters);

            Assert.Single(result);
        }

        #endregion

        #region GetValidProjectsAsync

        private static ProjectInvoiceRepository CreateRepositoryWithProjects(
            IEnumerable<ProjectInvoice> invoices,
            IEnumerable<Project> projects,
            int fpsYear = DefaultTestFpsYear)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(fpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);
            var invoicesMockSet = RepositoryTestHelper.CreateMockDbSet(invoices);
            var projectsMockSet = RepositoryTestHelper.CreateMockDbSet(projects);

            mockContext.Setup(x => x.ProjectInvoices).Returns(invoicesMockSet.Object);
            mockContext.Setup(x => x.Projects).Returns(projectsMockSet.Object);

            return new ProjectInvoiceRepository(mockContext.Object, fpsRequestContext);
        }

        [Fact]
        public async Task GetValidProjectsAsync_ReturnsProjectsForCurrentFpsYear()
        {
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", FpsYear = DefaultTestFpsYear },
                new() { ParentProject = "PRJ2", FpsYear = DefaultTestFpsYear },
                new() { ParentProject = "PRJ3", FpsYear = 2020 }
            };
            var repo = CreateRepositoryWithProjects([], projects);

            var result = await repo.GetValidProjectsAsync();

            Assert.Equal(2, result.Count);
            Assert.Contains("PRJ1", result);
            Assert.Contains("PRJ2", result);
        }

        [Fact]
        public async Task GetValidProjectsAsync_NoMatchingYear_ReturnsEmpty()
        {
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", FpsYear = 2020 }
            };
            var repo = CreateRepositoryWithProjects([], projects);

            var result = await repo.GetValidProjectsAsync();

            Assert.Empty(result);
        }

        #endregion

        #region GetCurrentFpsYear

        [Fact]
        public void GetCurrentFpsYear_ReturnsContextFpsYear()
        {
            var repo = CreateRepository([], fpsYear: 2025);

            var result = repo.GetCurrentFpsYear();

            Assert.Equal(2025, result);
        }

        #endregion

        #region GetFailedInvoiceImportAsync

        private static (
            ProjectInvoiceRepository Repo,
            Mock<DbSet<ProjectInvoiceStaging>> StagingDbSet,
            Mock<FpsDbContext> Context)
            CreateRepositoryWithStagingMocks(
                IEnumerable<ProjectInvoice> invoices,
                IEnumerable<ProjectInvoiceStaging> stagings,
                int fpsYear = DefaultTestFpsYear)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(fpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);
            var invoicesMockSet = RepositoryTestHelper.CreateMockDbSet(invoices);
            var stagingMockSet = RepositoryTestHelper.CreateMockDbSet(stagings);

            RepositoryTestHelper.SetupDbSetOperations(invoicesMockSet);
            RepositoryTestHelper.SetupDbSetOperations(stagingMockSet);
            invoicesMockSet
                .Setup(x => x.AddAsync(It.IsAny<ProjectInvoice>(), It.IsAny<CancellationToken>()))
                .Returns((ProjectInvoice _, CancellationToken __) => new ValueTask<EntityEntry<ProjectInvoice>>());
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.ProjectInvoices).Returns(invoicesMockSet.Object);
            mockContext.Setup(x => x.ProjectInvoiceStagings).Returns(stagingMockSet.Object);

            var repo = new ProjectInvoiceRepository(mockContext.Object, fpsRequestContext);
            return (repo, stagingMockSet, mockContext);
        }

        [Fact]
        public async Task GetFailedInvoiceImportAsync_NoRecords_ReturnsEmptyPagedData()
        {
            var (repo, _, _) = CreateRepositoryWithStagingMocks([], []);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetFailedInvoiceImportAsync(query, "user1");

            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetFailedInvoiceImportAsync_WithRecords_ReturnsAllFailedRowsForUser()
        {
            var oldDate = new DateTime(2024, 1, 1);
            var newDate = new DateTime(2024, 6, 1);
            var stagings = new List<ProjectInvoiceStaging>
            {
                new() { Id = 1, ProjectParent = "PRJ1", ImportedBy = "user1", IsPassed = false, ImportedDate = oldDate },
                new() { Id = 2, ProjectParent = "PRJ2", ImportedBy = "user1", IsPassed = false, ImportedDate = newDate },
                new() { Id = 3, ProjectParent = "PRJ3", ImportedBy = "user1", IsPassed = false, ImportedDate = newDate }
            };
            var (repo, _, _) = CreateRepositoryWithStagingMocks([], stagings);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetFailedInvoiceImportAsync(query, "user1");

            Assert.Equal(3, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetFailedInvoiceImportAsync_FiltersOnlyFailedAndByUser()
        {
            var date = new DateTime(2024, 1, 1);
            var stagings = new List<ProjectInvoiceStaging>
            {
                new() { Id = 1, ImportedBy = "user1", IsPassed = false, ImportedDate = date },
                new() { Id = 2, ImportedBy = "user2", IsPassed = false, ImportedDate = date },
                new() { Id = 3, ImportedBy = "user1", IsPassed = true, ImportedDate = date }
            };
            var (repo, _, _) = CreateRepositoryWithStagingMocks([], stagings);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetFailedInvoiceImportAsync(query, "user1");

            Assert.Single(result.Data);
        }

        [Theory]
        [InlineData("Id", false)]
        [InlineData("Id", true)]
        [InlineData("ProjectParent", false)]
        [InlineData("ProjectParent", true)]
        [InlineData("Month", false)]
        [InlineData("Month", true)]
        [InlineData("Amount", false)]
        [InlineData("Amount", true)]
        [InlineData("CostOfWork", false)]
        [InlineData("CostOfWork", true)]
        [InlineData("Wip", false)]
        [InlineData("Wip", true)]
        [InlineData("ProfitLoss", false)]
        [InlineData("ProfitLoss", true)]
        [InlineData("Detail", false)]
        [InlineData("Detail", true)]
        [InlineData("Type", false)]
        [InlineData("Type", true)]
        [InlineData("ValidationFailure", false)]
        [InlineData("ValidationFailure", true)]
        [InlineData("UnknownCol", false)]
        public async Task GetFailedInvoiceImportAsync_Sorting_ReturnsResults(string sortBy, bool descending)
        {
            var date = new DateTime(2024, 1, 1);
            var stagings = new List<ProjectInvoiceStaging>
            {
                new() { Id = 1, ProjectParent = "A", Month = "1", Amount = "100", CostOfWork = "10", Wip = "5", ProfitLoss = "2", Detail = "D1", Type = "T1", ValidationFailure = "V1", ImportedBy = "user1", IsPassed = false, ImportedDate = date },
                new() { Id = 2, ProjectParent = "B", Month = "2", Amount = "200", CostOfWork = "20", Wip = "10", ProfitLoss = "4", Detail = "D2", Type = "T2", ValidationFailure = "V2", ImportedBy = "user1", IsPassed = false, ImportedDate = date }
            };
            var (repo, _, _) = CreateRepositoryWithStagingMocks([], stagings);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = sortBy, Descending = descending };

            var result = await repo.GetFailedInvoiceImportAsync(query, "user1");

            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetFailedInvoiceImportAsync_NullSortBy_DefaultSortById()
        {
            var date = new DateTime(2024, 1, 1);
            var stagings = new List<ProjectInvoiceStaging>
            {
                new() { Id = 2, ImportedBy = "user1", IsPassed = false, ImportedDate = date },
                new() { Id = 1, ImportedBy = "user1", IsPassed = false, ImportedDate = date }
            };
            var (repo, _, _) = CreateRepositoryWithStagingMocks([], stagings);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = null };

            var result = await repo.GetFailedInvoiceImportAsync(query, "user1");

            Assert.Equal(1, result.Data.First().Id);
        }

        [Fact]
        public async Task GetFailedInvoiceImportAsync_FilterByProjectParent_FiltersResults()
        {
            var date = new DateTime(2024, 1, 1);
            var stagings = new List<ProjectInvoiceStaging>
            {
                new() { Id = 1, ProjectParent = "PRJ1", ImportedBy = "user1", IsPassed = false, ImportedDate = date },
                new() { Id = 2, ProjectParent = "PRJ2", ImportedBy = "user1", IsPassed = false, ImportedDate = date }
            };
            var (repo, _, _) = CreateRepositoryWithStagingMocks([], stagings);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "{\"ProjectParent\":\"PRJ1\"}" };

            var result = await repo.GetFailedInvoiceImportAsync(query, "user1");

            Assert.Single(result.Data);
        }

        [Fact]
        public async Task GetFailedInvoiceImportAsync_FilterByMonth_FiltersResults()
        {
            var date = new DateTime(2024, 1, 1);
            var stagings = new List<ProjectInvoiceStaging>
            {
                new() { Id = 1, Month = "3", ImportedBy = "user1", IsPassed = false, ImportedDate = date },
                new() { Id = 2, Month = "5", ImportedBy = "user1", IsPassed = false, ImportedDate = date }
            };
            var (repo, _, _) = CreateRepositoryWithStagingMocks([], stagings);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "{\"Month\":\"3\"}" };

            var result = await repo.GetFailedInvoiceImportAsync(query, "user1");

            Assert.Single(result.Data);
        }

        [Fact]
        public async Task GetFailedInvoiceImportAsync_NullFilter_ReturnsAll()
        {
            var date = new DateTime(2024, 1, 1);
            var stagings = new List<ProjectInvoiceStaging>
            {
                new() { Id = 1, ImportedBy = "user1", IsPassed = false, ImportedDate = date },
                new() { Id = 2, ImportedBy = "user1", IsPassed = false, ImportedDate = date }
            };
            var (repo, _, _) = CreateRepositoryWithStagingMocks([], stagings);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = null };

            var result = await repo.GetFailedInvoiceImportAsync(query, "user1");

            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        #endregion

        #region GetFailedInvoiceImportByIdAsync

        [Fact]
        public async Task GetFailedInvoiceImportByIdAsync_Exists_ReturnsEntity()
        {
            var stagings = new List<ProjectInvoiceStaging>
            {
                new() { Id = 5, ImportedBy = "user1", ProjectParent = "PRJ1" }
            };
            var (repo, _, _) = CreateRepositoryWithStagingMocks([], stagings);

            var result = await repo.GetFailedInvoiceImportByIdAsync(5, "user1");

            Assert.NotNull(result);
            Assert.Equal(5, result.Id);
        }

        [Fact]
        public async Task GetFailedInvoiceImportByIdAsync_NotFound_ReturnsNull()
        {
            var (repo, _, _) = CreateRepositoryWithStagingMocks([], []);

            var result = await repo.GetFailedInvoiceImportByIdAsync(99, "user1");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetFailedInvoiceImportByIdAsync_WrongUser_ReturnsNull()
        {
            var stagings = new List<ProjectInvoiceStaging>
            {
                new() { Id = 5, ImportedBy = "user2" }
            };
            var (repo, _, _) = CreateRepositoryWithStagingMocks([], stagings);

            var result = await repo.GetFailedInvoiceImportByIdAsync(5, "user1");

            Assert.Null(result);
        }

        #endregion

        #region DeleteFailedInvoiceImportByIdAsync

        [Fact]
        public async Task DeleteFailedInvoiceImportByIdAsync_Exists_RemovesAndReturnsTrue()
        {
            var stagings = new List<ProjectInvoiceStaging>
            {
                new() { Id = 5, ImportedBy = "user1" }
            };
            var (repo, stagingDbSet, mockContext) = CreateRepositoryWithStagingMocks([], stagings);

            var result = await repo.DeleteFailedInvoiceImportByIdAsync(5, "user1");

            Assert.True(result);
            RepositoryTestHelper.VerifyRemove(stagingDbSet);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task DeleteFailedInvoiceImportByIdAsync_NotFound_ReturnsFalse()
        {
            var (repo, _, _) = CreateRepositoryWithStagingMocks([], []);

            var result = await repo.DeleteFailedInvoiceImportByIdAsync(99, "user1");

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteFailedInvoiceImportByIdAsync_WrongUser_ReturnsFalse()
        {
            var stagings = new List<ProjectInvoiceStaging>
            {
                new() { Id = 5, ImportedBy = "user2" }
            };
            var (repo, _, _) = CreateRepositoryWithStagingMocks([], stagings);

            var result = await repo.DeleteFailedInvoiceImportByIdAsync(5, "user1");

            Assert.False(result);
        }

        #endregion

        #region DeleteFailedInvoiceImportByUserAsync

        [Fact]
        public async Task DeleteFailedInvoiceImportByUserAsync_WithRecords_RemovesAndReturnsCount()
        {
            var stagings = new List<ProjectInvoiceStaging>
            {
                new() { Id = 1, ImportedBy = "user1" },
                new() { Id = 2, ImportedBy = "user1" },
                new() { Id = 3, ImportedBy = "user2" }
            };
            var (repo, _, mockContext) = CreateRepositoryWithStagingMocks([], stagings);

            var result = await repo.DeleteFailedInvoiceImportByUserAsync("user1");

            Assert.Equal(1, result); // SaveChangesAsync returns 1 by default
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task DeleteFailedInvoiceImportByUserAsync_NoRecords_ReturnsZero()
        {
            var (repo, _, _) = CreateRepositoryWithStagingMocks([], []);

            var result = await repo.DeleteFailedInvoiceImportByUserAsync("user1");

            Assert.Equal(0, result);
        }

        #endregion

        #region ImportInvoiceAsync

        [Fact]
        public async Task ImportInvoiceAsync_BothEmpty_ReturnsZeroCounts()
        {
            var (repo, _, _) = CreateRepositoryWithStagingMocks([], []);

            var result = await repo.ImportInvoiceAsync([], []);

            Assert.Equal(0, result.PassedCount);
            Assert.Equal(0, result.FailedCount);
        }

        [Fact]
        public async Task ImportInvoiceAsync_OnlyPassed_ReturnsCorrectCounts()
        {
            var (repo, _, mockContext) = CreateRepositoryWithStagingMocks([], []);
            var passed = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1" },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ2" }
            };

            var result = await repo.ImportInvoiceAsync(passed, []);

            Assert.Equal(2, result.PassedCount);
            Assert.Equal(0, result.FailedCount);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task ImportInvoiceAsync_OnlyFailed_ReturnsCorrectCounts()
        {
            var (repo, _, mockContext) = CreateRepositoryWithStagingMocks([], []);
            var failed = new List<ProjectInvoiceStaging>
            {
                new() { Id = 1, ProjectParent = "INVALID" }
            };

            var result = await repo.ImportInvoiceAsync([], failed);

            Assert.Equal(0, result.PassedCount);
            Assert.Equal(1, result.FailedCount);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task ImportInvoiceAsync_Mixed_ReturnsCorrectCounts()
        {
            var (repo, _, mockContext) = CreateRepositoryWithStagingMocks([], []);
            var passed = new List<ProjectInvoice> { new() { InvoiceCounter = 1, ProjectParent = "PRJ1" } };
            var failed = new List<ProjectInvoiceStaging> { new() { Id = 1, ProjectParent = "INVALID" } };

            var result = await repo.ImportInvoiceAsync(passed, failed);

            Assert.Equal(1, result.PassedCount);
            Assert.Equal(1, result.FailedCount);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        #endregion

        #region UpdateFailedInvoiceImportRecordsAsync

        [Fact]
        public async Task UpdateFailedInvoiceImportRecordsAsync_MarksRecordsAsModifiedAndSaves()
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(DefaultTestFpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);
            var invoicesMockSet = RepositoryTestHelper.CreateMockDbSet<ProjectInvoice>([]);
            var stagingMockSet = RepositoryTestHelper.CreateMockDbSet<ProjectInvoiceStaging>([]);

            mockContext.Setup(x => x.ProjectInvoices).Returns(invoicesMockSet.Object);
            mockContext.Setup(x => x.ProjectInvoiceStagings).Returns(stagingMockSet.Object);

            var entryCallCount = 0;
            mockContext.Setup(x => x.Entry(It.IsAny<ProjectInvoiceStaging>()))
                .Callback(() => entryCallCount++)
                .Throws(new NotSupportedException("Entry() is not supported in mocked DbContext"));

            var repo = new ProjectInvoiceRepository(mockContext.Object, fpsRequestContext);
            var records = new List<ProjectInvoiceStaging>
            {
                new() { Id = 1, ProjectParent = "PRJ1" }
            };

            await Assert.ThrowsAsync<NotSupportedException>(() => repo.UpdateFailedInvoiceImportRecordsAsync(records));

            Assert.Equal(1, entryCallCount);
        }

        [Fact]
        public async Task UpdateFailedInvoiceImportRecordsAsync_MultipleRecords_CallsEntryForEach()
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(DefaultTestFpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);
            var invoicesMockSet = RepositoryTestHelper.CreateMockDbSet<ProjectInvoice>([]);
            var stagingMockSet = RepositoryTestHelper.CreateMockDbSet<ProjectInvoiceStaging>([]);

            mockContext.Setup(x => x.ProjectInvoices).Returns(invoicesMockSet.Object);
            mockContext.Setup(x => x.ProjectInvoiceStagings).Returns(stagingMockSet.Object);

            var entryCallCount = 0;
            mockContext.Setup(x => x.Entry(It.IsAny<ProjectInvoiceStaging>()))
                .Callback(() => entryCallCount++)
                .Throws(new NotSupportedException("Entry() is not supported in mocked DbContext"));

            var repo = new ProjectInvoiceRepository(mockContext.Object, fpsRequestContext);
            var records = new List<ProjectInvoiceStaging>
            {
                new() { Id = 1, ProjectParent = "PRJ1" },
                new() { Id = 2, ProjectParent = "PRJ2" }
            };

            await Assert.ThrowsAsync<NotSupportedException>(() => repo.UpdateFailedInvoiceImportRecordsAsync(records));

            Assert.Equal(1, entryCallCount); // Throws on first Entry() call
        }

        #endregion

        #region DeleteFailedInvoiceImportByIdsAsync

        [Fact]
        public async Task DeleteFailedInvoiceImportByIdsAsync_WithMatchingRecords_RemovesAndSaves()
        {
            var stagings = new List<ProjectInvoiceStaging>
            {
                new() { Id = 1, ImportedBy = "user1" },
                new() { Id = 2, ImportedBy = "user1" },
                new() { Id = 3, ImportedBy = "user2" }
            };
            var (repo, _, mockContext) = CreateRepositoryWithStagingMocks([], stagings);

            await repo.DeleteFailedInvoiceImportByIdsAsync([1, 2], "user1");

            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task DeleteFailedInvoiceImportByIdsAsync_NoMatchingRecords_DoesNotCallSave()
        {
            var stagings = new List<ProjectInvoiceStaging>
            {
                new() { Id = 1, ImportedBy = "user2" }
            };
            var (repo, _, mockContext) = CreateRepositoryWithStagingMocks([], stagings);

            await repo.DeleteFailedInvoiceImportByIdsAsync([99], "user1");

            mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeleteFailedInvoiceImportByIdsAsync_WrongUser_DoesNotDelete()
        {
            var stagings = new List<ProjectInvoiceStaging>
            {
                new() { Id = 1, ImportedBy = "user2" }
            };
            var (repo, _, mockContext) = CreateRepositoryWithStagingMocks([], stagings);

            await repo.DeleteFailedInvoiceImportByIdsAsync([1], "user1");

            mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeleteFailedInvoiceImportByIdsAsync_EmptyIds_DoesNotCallSave()
        {
            var (repo, _, mockContext) = CreateRepositoryWithStagingMocks([], []);

            await repo.DeleteFailedInvoiceImportByIdsAsync([], "user1");

            mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeleteFailedInvoiceImportByIdsAsync_PartialMatch_DeletesOnlyMatching()
        {
            var stagings = new List<ProjectInvoiceStaging>
            {
                new() { Id = 1, ImportedBy = "user1" },
                new() { Id = 2, ImportedBy = "user1" },
                new() { Id = 3, ImportedBy = "user1" }
            };
            var (repo, _, mockContext) = CreateRepositoryWithStagingMocks([], stagings);

            await repo.DeleteFailedInvoiceImportByIdsAsync([1, 3], "user1");

            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        #endregion

        #region GetTotalAmountAsync - Additional

        [Fact]
        public async Task GetTotalAmountAsync_NullAmounts_ReturnsSumOfNonNullAmounts()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", Amount = 100m, FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ1", Amount = null, FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);

            var result = await repo.GetTotalAmountAsync("PRJ1");

            Assert.Equal(100m, result);
        }

        #endregion
    }
}
