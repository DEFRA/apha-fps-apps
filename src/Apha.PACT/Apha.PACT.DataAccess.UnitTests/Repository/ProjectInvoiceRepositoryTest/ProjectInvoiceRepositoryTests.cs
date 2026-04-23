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
    }
}
