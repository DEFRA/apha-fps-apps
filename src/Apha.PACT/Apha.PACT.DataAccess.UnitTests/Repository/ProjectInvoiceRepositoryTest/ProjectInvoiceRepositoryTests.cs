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
            invoicesMockSet
                .Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<ProjectInvoice>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
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

        #region GetPagedProjectInvoicesByMonthAsync

        [Fact]
        public async Task GetPagedProjectInvoicesByMonthAsync_WithMonth_ReturnsFilteredPagedResult()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", Month = 3, FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ2", Month = 3, FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 3, ProjectParent = "PRJ3", Month = 6, FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 50 };

            var result = await repo.GetPagedProjectInvoicesByMonthAsync(query, 3);

            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, invoice => Assert.Equal(3, invoice.Month));
            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesByMonthAsync_NullMonth_ReturnsAllRecords()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", Month = 3, FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ2", Month = 6, FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 3, ProjectParent = "PRJ3", Month = 9, FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 50 };

            var result = await repo.GetPagedProjectInvoicesByMonthAsync(query, null);

            Assert.Equal(3, result.Data.Count);
            Assert.Equal(3, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesByMonthAsync_WithMonthAndProjectParentFilter_AppliesBothFilters()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "CORE-001", Month = 3, FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "CORE-002", Month = 3, FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 3, ProjectParent = "TEST-001", Month = 3, FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 50,
                Filter = "{\"ProjectParent\":\"CORE\"}"
            };

            var result = await repo.GetPagedProjectInvoicesByMonthAsync(query, 3);

            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, invoice =>
            {
                Assert.Equal(3, invoice.Month);
                Assert.Contains("CORE", invoice.ProjectParent);
            });
        }

        [Fact]
        public async Task GetPagedProjectInvoicesByMonthAsync_WithMonthAndDetailFilter_AppliesBothFilters()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", Month = 3, Detail = "Q1 Invoice", FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ2", Month = 3, Detail = "Monthly Invoice", FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 3, ProjectParent = "PRJ3", Month = 3, Detail = "Q1 Report", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 50,
                Filter = "{\"Detail\":\"Q1\"}"
            };

            var result = await repo.GetPagedProjectInvoicesByMonthAsync(query, 3);

            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, invoice =>
            {
                Assert.Equal(3, invoice.Month);
                Assert.Contains("Q1", invoice.Detail);
            });
        }

        [Fact]
        public async Task GetPagedProjectInvoicesByMonthAsync_WithSorting_AppliesSortCorrectly()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "C-Project", Month = 3, Amount = 3000m, FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "A-Project", Month = 3, Amount = 1000m, FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 3, ProjectParent = "B-Project", Month = 3, Amount = 2000m, FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 50,
                SortBy = "Amount",
                Descending = true
            };

            var result = await repo.GetPagedProjectInvoicesByMonthAsync(query, 3);

            Assert.Equal(3, result.Data.Count);
            var dataList = result.Data.ToList();
            Assert.Equal(3000m, dataList[0].Amount);
            Assert.Equal(2000m, dataList[1].Amount);
            Assert.Equal(1000m, dataList[2].Amount);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesByMonthAsync_WithPagination_ReturnsCorrectPage()
        {
            var invoices = new List<ProjectInvoice>();
            for (int i = 1; i <= 25; i++)
            {
                invoices.Add(new ProjectInvoice
                {
                    InvoiceCounter = i,
                    ProjectParent = $"PRJ{i}",
                    Month = 3,
                    FpsYear = DefaultTestFpsYear
                });
            }
            var repo = CreateRepository(invoices);
            var query = new PaginationParameters<string>
            {
                Page = 2,
                PageSize = 10
            };

            var result = await repo.GetPagedProjectInvoicesByMonthAsync(query, 3);

            Assert.Equal(10, result.Data.Count);
            Assert.Equal(25, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.PageNumber);
            Assert.Equal(3, result.PaginationData.TotalPages);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesByMonthAsync_NoMatchingMonth_ReturnsEmpty()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", Month = 3, FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ2", Month = 6, FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 50 };

            var result = await repo.GetPagedProjectInvoicesByMonthAsync(query, 12);

            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
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

        #region GetInvoicesByMonthAsync

        [Fact]
        public async Task GetInvoicesByMonthAsync_WithValidMonth_ReturnsInvoicesForMonth()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", Month = 3, FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ2", Month = 3, FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 3, ProjectParent = "PRJ3", Month = 6, FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);

            var result = await repo.GetInvoicesByMonthAsync(3);

            Assert.Equal(2, result.Count);
            Assert.All(result, invoice => Assert.Equal(3, invoice.Month));
        }

        [Fact]
        public async Task GetInvoicesByMonthAsync_OrdersByProjectParent()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "C-Project", Month = 3, FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "A-Project", Month = 3, FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 3, ProjectParent = "B-Project", Month = 3, FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);

            var result = await repo.GetInvoicesByMonthAsync(3);

            Assert.Equal(3, result.Count);
            Assert.Equal("A-Project", result[0].ProjectParent);
            Assert.Equal("B-Project", result[1].ProjectParent);
            Assert.Equal("C-Project", result[2].ProjectParent);
        }

        [Fact]
        public async Task GetInvoicesByMonthAsync_NoMatchingMonth_ReturnsEmptyList()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", Month = 3, FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);

            var result = await repo.GetInvoicesByMonthAsync(12);

            Assert.Empty(result);
        }

        #endregion

        #region GetInvoicesByIdsAsync

        [Fact]
        public async Task GetInvoicesByIdsAsync_WithValidIds_ReturnsMatchingInvoices()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ2", FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 3, ProjectParent = "PRJ3", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);

            var result = await repo.GetInvoicesByIdsAsync(new List<int> { 1, 3 });

            Assert.Equal(2, result.Count);
            Assert.Contains(result, i => i.InvoiceCounter == 1);
            Assert.Contains(result, i => i.InvoiceCounter == 3);
        }

        [Fact]
        public async Task GetInvoicesByIdsAsync_FiltersOutWrongFpsYear()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ2", FpsYear = 2020 },
                new() { InvoiceCounter = 3, ProjectParent = "PRJ3", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices, fpsYear: DefaultTestFpsYear);

            var result = await repo.GetInvoicesByIdsAsync(new List<int> { 1, 2, 3 });

            Assert.Equal(2, result.Count);
            Assert.All(result, invoice => Assert.Equal(DefaultTestFpsYear, invoice.FpsYear));
        }

        [Fact]
        public async Task GetInvoicesByIdsAsync_EmptyIdList_ReturnsEmptyList()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);

            var result = await repo.GetInvoicesByIdsAsync(new List<int>());

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetInvoicesByIdsAsync_NonExistentIds_ReturnsEmptyList()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);

            var result = await repo.GetInvoicesByIdsAsync(new List<int> { 99, 100 });

            Assert.Empty(result);
        }

        #endregion

        #region HasInvoicesForMonthAsync

        [Fact]
        public async Task HasInvoicesForMonthAsync_WithMatchingMonthAndYear_ReturnsTrue()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", Month = 3, FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);

            var result = await repo.HasInvoicesForMonthAsync(3);

            Assert.True(result);
        }

        [Fact]
        public async Task HasInvoicesForMonthAsync_NoMatchingMonth_ReturnsFalse()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", Month = 3, FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);

            var result = await repo.HasInvoicesForMonthAsync(6);

            Assert.False(result);
        }

        [Fact]
        public async Task HasInvoicesForMonthAsync_WrongFpsYear_ReturnsFalse()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", Month = 3, FpsYear = 2020 }
            };
            var repo = CreateRepository(invoices, fpsYear: DefaultTestFpsYear);

            var result = await repo.HasInvoicesForMonthAsync(3);

            Assert.False(result);
        }

        [Fact]
        public async Task HasInvoicesForMonthAsync_EmptyRepository_ReturnsFalse()
        {
            var repo = CreateRepository([]);

            var result = await repo.HasInvoicesForMonthAsync(3);

            Assert.False(result);
        }

        #endregion

        #region CreateBulkAsync

        [Fact]
        public async Task CreateBulkAsync_ValidEntities_SetsFpsYearForAll()
        {
            var (repo, invoicesMockSet, mockContext) = CreateRepositoryWithMocks([]);
            var entities = new List<ProjectInvoice>
            {
                new() { ProjectParent = "PRJ1", Amount = 1000m },
                new() { ProjectParent = "PRJ2", Amount = 2000m },
                new() { ProjectParent = "PRJ3", Amount = 3000m }
            };

            var result = await repo.CreateBulkAsync(entities);

            Assert.All(entities, entity => Assert.Equal(DefaultTestFpsYear, entity.FpsYear));
            invoicesMockSet.Verify(x => x.AddRangeAsync(It.IsAny<IEnumerable<ProjectInvoice>>(), It.IsAny<CancellationToken>()), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task CreateBulkAsync_EmptyList_DoesNotThrowException()
        {
            var (repo, invoicesMockSet, mockContext) = CreateRepositoryWithMocks([]);
            var entities = new List<ProjectInvoice>();

            var result = await repo.CreateBulkAsync(entities);

            invoicesMockSet.Verify(x => x.AddRangeAsync(It.IsAny<IEnumerable<ProjectInvoice>>(), It.IsAny<CancellationToken>()), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task CreateBulkAsync_SetsFpsYear_FromYearContext()
        {
            const int customYear = 2025;
            var (repo, _, _) = CreateRepositoryWithMocks([], fpsYear: customYear);
            var entities = new List<ProjectInvoice>
            {
                new() { ProjectParent = "PRJ1" },
                new() { ProjectParent = "PRJ2" }
            };

            await repo.CreateBulkAsync(entities);

            Assert.All(entities, entity => Assert.Equal(customYear, entity.FpsYear));
        }

        [Fact]
        public async Task CreateBulkAsync_LargeNumberOfEntities_HandlesSuccessfully()
        {
            var (repo, invoicesMockSet, mockContext) = CreateRepositoryWithMocks([]);
            var entities = new List<ProjectInvoice>();
            for (int i = 1; i <= 100; i++)
            {
                entities.Add(new ProjectInvoice
                {
                    ProjectParent = $"PRJ{i}",
                    Month = i % 12 + 1,
                    Amount = i * 1000m
                });
            }

            var result = await repo.CreateBulkAsync(entities);

            Assert.Equal(100, entities.Count);
            Assert.All(entities, entity => Assert.Equal(DefaultTestFpsYear, entity.FpsYear));
            invoicesMockSet.Verify(x => x.AddRangeAsync(It.IsAny<IEnumerable<ProjectInvoice>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region GetMonthlyInvoicesSummaryAsync

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_WithProgramFilter_ReturnsFilteredData()
        {
            var summaries = new List<MonthlyInvoicesSummary>
            {
                new() { FpsYear = DefaultTestFpsYear, Program = "ADMIN", ParentProject = "PRJ1", Month = 3, MonthlyAmount = 1000m },
                new() { FpsYear = DefaultTestFpsYear, Program = "CORE", ParentProject = "PRJ2", Month = 3, MonthlyAmount = 2000m },
                new() { FpsYear = DefaultTestFpsYear, Program = "ADMIN", ParentProject = "PRJ3", Month = 6, MonthlyAmount = 1500m }
            };
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(DefaultTestFpsYear);
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);
            var summariesMockSet = RepositoryTestHelper.CreateMockDbSet(summaries);
            mockContext.Setup(x => x.MonthlyInvoicesSummary).Returns(summariesMockSet.Object);
            var repo = new ProjectInvoiceRepository(mockContext.Object, fpsRequestContext);

            var query = new PaginationParameters<string>
            {
                Filter = "{\"Program\":\"ADMIN\"}"
            };

            var result = await repo.GetMonthlyInvoicesSummaryAsync(query);

            Assert.Equal(2, result.Count);
            Assert.All(result, s => Assert.Contains("ADMIN", s.Program));
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_WithParentProjectFilter_ReturnsFilteredData()
        {
            var summaries = new List<MonthlyInvoicesSummary>
            {
                new() { FpsYear = DefaultTestFpsYear, Program = "ADMIN", ParentProject = "CORE-001", Month = 3, MonthlyAmount = 1000m },
                new() { FpsYear = DefaultTestFpsYear, Program = "ADMIN", ParentProject = "CORE-002", Month = 3, MonthlyAmount = 2000m },
                new() { FpsYear = DefaultTestFpsYear, Program = "ADMIN", ParentProject = "TEST-001", Month = 3, MonthlyAmount = 1500m }
            };
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(DefaultTestFpsYear);
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);
            var summariesMockSet = RepositoryTestHelper.CreateMockDbSet(summaries);
            mockContext.Setup(x => x.MonthlyInvoicesSummary).Returns(summariesMockSet.Object);
            var repo = new ProjectInvoiceRepository(mockContext.Object, fpsRequestContext);

            var query = new PaginationParameters<string>
            {
                Filter = "{\"ParentProject\":\"CORE\"}"
            };

            var result = await repo.GetMonthlyInvoicesSummaryAsync(query);

            Assert.Equal(2, result.Count);
            Assert.All(result, s => Assert.Contains("CORE", s.ParentProject));
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_NoFilter_ReturnsAllDataSorted()
        {
            var summaries = new List<MonthlyInvoicesSummary>
            {
                new() { FpsYear = DefaultTestFpsYear, Program = "CORE", ParentProject = "B-Project", Month = 6, MonthlyAmount = 3000m },
                new() { FpsYear = DefaultTestFpsYear, Program = "ADMIN", ParentProject = "A-Project", Month = 3, MonthlyAmount = 1000m },
                new() { FpsYear = DefaultTestFpsYear, Program = "ADMIN", ParentProject = "C-Project", Month = 9, MonthlyAmount = 2000m }
            };
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(DefaultTestFpsYear);
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);
            var summariesMockSet = RepositoryTestHelper.CreateMockDbSet(summaries);
            mockContext.Setup(x => x.MonthlyInvoicesSummary).Returns(summariesMockSet.Object);
            var repo = new ProjectInvoiceRepository(mockContext.Object, fpsRequestContext);

            var query = new PaginationParameters<string>();

            var result = await repo.GetMonthlyInvoicesSummaryAsync(query);

            Assert.Equal(3, result.Count);
            // Should be ordered by Program, ParentProject, Month
            Assert.Equal("ADMIN", result[0].Program);
            Assert.Equal("ADMIN", result[1].Program);
            Assert.Equal("CORE", result[2].Program);
        }

        #endregion
    }
}
