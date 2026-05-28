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

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_WithMonthFilter_ReturnsFilteredResult()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", Month = 3, FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ2", Month = 6, FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 3, ProjectParent = "PRJ3", Month = 3, FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);
            var query = new PaginationParameters<string>
            {
                Filter = "{\"Month\":\"3\"}"
            };

            var result = await repo.GetPagedProjectInvoicesAsync(query, null);

            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, invoice => Assert.Equal(3, invoice.Month));
        }

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_WithDetailFilter_ReturnsFilteredResult()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", Detail = "Q1 Invoice", FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ2", Detail = "Monthly Report", FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 3, ProjectParent = "PRJ3", Detail = "Q1 Report", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);
            var query = new PaginationParameters<string>
            {
                Filter = "{\"Detail\":\"Q1\"}"
            };

            var result = await repo.GetPagedProjectInvoicesAsync(query, null);

            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, invoice => Assert.Contains("Q1", invoice.Detail));
        }

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_WithSorting_AppliesSortCorrectly()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "C-Project", Amount = 3000m, FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "A-Project", Amount = 1000m, FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 3, ProjectParent = "B-Project", Amount = 2000m, FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);
            var query = new PaginationParameters<string>
            {
                SortBy = "Amount",
                Descending = true
            };

            var result = await repo.GetPagedProjectInvoicesAsync(query, null);

            var dataList = result.Data.ToList();
            Assert.Equal(3000m, dataList[0].Amount);
            Assert.Equal(2000m, dataList[1].Amount);
            Assert.Equal(1000m, dataList[2].Amount);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_WithPagination_ReturnsCorrectPage()
        {
            var invoices = new List<ProjectInvoice>();
            for (int i = 1; i <= 25; i++)
            {
                invoices.Add(new ProjectInvoice
                {
                    InvoiceCounter = i,
                    ProjectParent = $"PRJ{i}",
                    FpsYear = DefaultTestFpsYear
                });
            }
            var repo = CreateRepository(invoices);
            var query = new PaginationParameters<string>
            {
                Page = 2,
                PageSize = 10
            };

            var result = await repo.GetPagedProjectInvoicesAsync(query, null);

            Assert.Equal(10, result.Data.Count);
            Assert.Equal(25, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.PageNumber);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_EmptyParentProject_ReturnsAllRecords()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ2", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);
            var query = new PaginationParameters<string>();

            var result = await repo.GetPagedProjectInvoicesAsync(query, "");

            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_NonExistentParentProject_ReturnsEmpty()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);
            var query = new PaginationParameters<string>();

            var result = await repo.GetPagedProjectInvoicesAsync(query, "NONEXISTENT");

            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_WithMultipleFilters_AppliesAllFilters()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "CORE-001", Month = 3, Detail = "Q1", FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "CORE-002", Month = 3, Detail = "Monthly", FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 3, ProjectParent = "TEST-001", Month = 3, Detail = "Q1", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);
            var query = new PaginationParameters<string>
            {
                Filter = "{\"ProjectParent\":\"CORE\",\"Detail\":\"Q1\"}"
            };

            var result = await repo.GetPagedProjectInvoicesAsync(query, null);

            Assert.Single(result.Data);
            Assert.Equal(1, result.Data.First().InvoiceCounter);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_WithInvalidJsonFilter_ThrowsException()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ2", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);
            var query = new PaginationParameters<string>
            {
                Filter = "invalid json"
            };

            await Assert.ThrowsAsync<Newtonsoft.Json.JsonReaderException>(() => repo.GetPagedProjectInvoicesAsync(query, null));
        }

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_WithEmptyFilter_ReturnsAllRecords()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ2", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);
            var query = new PaginationParameters<string>
            {
                Filter = ""
            };

            var result = await repo.GetPagedProjectInvoicesAsync(query, null);

            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_WithNullDetail_DoesNotMatch()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", Detail = null, FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ2", Detail = "Test", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);
            var query = new PaginationParameters<string>
            {
                Filter = "{\"Detail\":\"Test\"}"
            };

            var result = await repo.GetPagedProjectInvoicesAsync(query, null);

            Assert.Single(result.Data);
            Assert.Equal(2, result.Data.First().InvoiceCounter);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_SortByProjectParent_Ascending()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "C-Project", FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "A-Project", FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 3, ProjectParent = "B-Project", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);
            var query = new PaginationParameters<string>
            {
                SortBy = "ProjectParent",
                Descending = false
            };

            var result = await repo.GetPagedProjectInvoicesAsync(query, null);

            var dataList = result.Data.ToList();
            Assert.Equal("A-Project", dataList[0].ProjectParent);
            Assert.Equal("B-Project", dataList[1].ProjectParent);
            Assert.Equal("C-Project", dataList[2].ProjectParent);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_SortByMonth_Descending()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", Month = 3, FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ2", Month = 6, FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 3, ProjectParent = "PRJ3", Month = 1, FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);
            var query = new PaginationParameters<string>
            {
                SortBy = "Month",
                Descending = true
            };

            var result = await repo.GetPagedProjectInvoicesAsync(query, null);

            var dataList = result.Data.ToList();
            Assert.Equal(6, dataList[0].Month);
            Assert.Equal(3, dataList[1].Month);
            Assert.Equal(1, dataList[2].Month);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_SortByCostOfWork_Ascending()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", CostOfWork = 3000m, FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ2", CostOfWork = 1000m, FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 3, ProjectParent = "PRJ3", CostOfWork = 2000m, FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);
            var query = new PaginationParameters<string>
            {
                SortBy = "CostOfWork",
                Descending = false
            };

            var result = await repo.GetPagedProjectInvoicesAsync(query, null);

            var dataList = result.Data.ToList();
            Assert.Equal(1000m, dataList[0].CostOfWork);
            Assert.Equal(2000m, dataList[1].CostOfWork);
            Assert.Equal(3000m, dataList[2].CostOfWork);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_SortByWip_Descending()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", Wip = 100m, FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ2", Wip = 300m, FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 3, ProjectParent = "PRJ3", Wip = 200m, FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);
            var query = new PaginationParameters<string>
            {
                SortBy = "Wip",
                Descending = true
            };

            var result = await repo.GetPagedProjectInvoicesAsync(query, null);

            var dataList = result.Data.ToList();
            Assert.Equal(300m, dataList[0].Wip);
            Assert.Equal(200m, dataList[1].Wip);
            Assert.Equal(100m, dataList[2].Wip);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_SortByProfitLoss_Ascending()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", ProfitLoss = 50m, FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ2", ProfitLoss = -20m, FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 3, ProjectParent = "PRJ3", ProfitLoss = 30m, FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);
            var query = new PaginationParameters<string>
            {
                SortBy = "ProfitLoss",
                Descending = false
            };

            var result = await repo.GetPagedProjectInvoicesAsync(query, null);

            var dataList = result.Data.ToList();
            Assert.Equal(-20m, dataList[0].ProfitLoss);
            Assert.Equal(30m, dataList[1].ProfitLoss);
            Assert.Equal(50m, dataList[2].ProfitLoss);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_SortByDetail_Ascending()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", Detail = "Charlie", FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ2", Detail = "Alpha", FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 3, ProjectParent = "PRJ3", Detail = "Bravo", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);
            var query = new PaginationParameters<string>
            {
                SortBy = "Detail",
                Descending = false
            };

            var result = await repo.GetPagedProjectInvoicesAsync(query, null);

            var dataList = result.Data.ToList();
            Assert.Equal("Alpha", dataList[0].Detail);
            Assert.Equal("Bravo", dataList[1].Detail);
            Assert.Equal("Charlie", dataList[2].Detail);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_SortByInvoiceCounter_Descending()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ2", FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 3, ProjectParent = "PRJ3", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);
            var query = new PaginationParameters<string>
            {
                SortBy = "InvoiceCounter",
                Descending = true
            };

            var result = await repo.GetPagedProjectInvoicesAsync(query, null);

            var dataList = result.Data.ToList();
            Assert.Equal(3, dataList[0].InvoiceCounter);
            Assert.Equal(2, dataList[1].InvoiceCounter);
            Assert.Equal(1, dataList[2].InvoiceCounter);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_SortByUnknownProperty_DefaultsToInvoiceCounter()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 3, ProjectParent = "PRJ3", FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ2", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);
            var query = new PaginationParameters<string>
            {
                SortBy = "UnknownProperty",
                Descending = false
            };

            var result = await repo.GetPagedProjectInvoicesAsync(query, null);

            var dataList = result.Data.ToList();
            Assert.Equal(1, dataList[0].InvoiceCounter);
            Assert.Equal(2, dataList[1].InvoiceCounter);
            Assert.Equal(3, dataList[2].InvoiceCounter);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_NoSortBy_DefaultsToInvoiceCounterAscending()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 3, ProjectParent = "PRJ3", FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ2", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);
            var query = new PaginationParameters<string>();

            var result = await repo.GetPagedProjectInvoicesAsync(query, null);

            var dataList = result.Data.ToList();
            Assert.Equal(1, dataList[0].InvoiceCounter);
            Assert.Equal(2, dataList[1].InvoiceCounter);
            Assert.Equal(3, dataList[2].InvoiceCounter);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_FilterWithNullValues_IgnoresNullFilters()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ2", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);
            var query = new PaginationParameters<string>
            {
                Filter = "{\"ProjectParent\":null,\"Detail\":null}"
            };

            var result = await repo.GetPagedProjectInvoicesAsync(query, null);

            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_FilterWithInvalidMonthString_IgnoresMonthFilter()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", Month = 3, FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ2", Month = 6, FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);
            var query = new PaginationParameters<string>
            {
                Filter = "{\"Month\":\"invalid\"}"
            };

            var result = await repo.GetPagedProjectInvoicesAsync(query, null);

            // Should return all records since month filter is ignored
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

        [Fact]
        public async Task GetPagedProjectInvoicesByMonthAsync_WithMonthFilterInQueryString_IgnoresMonthFilterFromQuery()
        {
            // ApplyInvoiceFilterExcludingMonth should ignore Month from filter JSON
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", Month = 3, FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ2", Month = 6, FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 50,
                Filter = "{\"Month\":6}" // This should be ignored
            };

            var result = await repo.GetPagedProjectInvoicesByMonthAsync(query, 3);

            // Should only get month 3 records, not month 6 despite filter
            Assert.Single(result.Data);
            Assert.Equal(3, result.Data.First().Month);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesByMonthAsync_WithEmptyFilter_ReturnsAllForMonth()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", Month = 3, FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ2", Month = 3, FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 50,
                Filter = ""
            };

            var result = await repo.GetPagedProjectInvoicesByMonthAsync(query, 3);

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesByMonthAsync_WithInvalidJsonFilter_ThrowsException()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", Month = 3, FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 50,
                Filter = "invalid json"
            };

            await Assert.ThrowsAsync<Newtonsoft.Json.JsonReaderException>(() => repo.GetPagedProjectInvoicesByMonthAsync(query, 3));
        }

        [Fact]
        public async Task GetPagedProjectInvoicesByMonthAsync_WithNullDetailFilter_DoesNotMatchNullDetails()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", Month = 3, Detail = null, FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ2", Month = 3, Detail = "Test", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 50,
                Filter = "{\"Detail\":\"Test\"}"
            };

            var result = await repo.GetPagedProjectInvoicesByMonthAsync(query, 3);

            Assert.Single(result.Data);
            Assert.Equal(2, result.Data.First().InvoiceCounter);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesByMonthAsync_FilterWithNullValues_IgnoresNullFilters()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", Month = 3, FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ2", Month = 3, FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 50,
                Filter = "{\"ProjectParent\":null,\"Detail\":null}"
            };

            var result = await repo.GetPagedProjectInvoicesByMonthAsync(query, 3);

            Assert.Equal(2, result.Data.Count);
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

        [Fact]
        public async Task GetTotalAmountAsync_EmptyParentProject_ReturnsTotalOfAllAmounts()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", Amount = 1000m, FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ2", Amount = 500m, FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);

            var result = await repo.GetTotalAmountAsync("");

            Assert.Equal(1500m, result);
        }

        [Fact]
        public async Task GetTotalAmountAsync_WithNegativeAmounts_ReturnsCorrectSum()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", Amount = 1000m, FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ1", Amount = -500m, FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);

            var result = await repo.GetTotalAmountAsync("PRJ1");

            Assert.Equal(500m, result);
        }

        [Fact]
        public async Task GetTotalAmountAsync_WithZeroAmounts_ReturnsZero()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", Amount = 0m, FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ1", Amount = 0m, FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);

            var result = await repo.GetTotalAmountAsync("PRJ1");

            Assert.Equal(0m, result);
        }

        [Fact]
        public async Task GetTotalAmountAsync_LargeAmounts_HandlesCorrectly()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", Amount = 999999999.99m, FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ1", Amount = 0.01m, FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);

            var result = await repo.GetTotalAmountAsync("PRJ1");

            Assert.Equal(1000000000.00m, result);
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

        [Fact]
        public async Task GetByIdAsync_ZeroId_ReturnsNull()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);

            var result = await repo.GetByIdAsync(0);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_NegativeId_ReturnsNull()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);

            var result = await repo.GetByIdAsync(-1);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_DifferentFpsYear_StillReturnsInvoice()
        {
            // GetByIdAsync does not filter by FpsYear in the current implementation
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", FpsYear = 2020 }
            };
            var repo = CreateRepository(invoices, fpsYear: DefaultTestFpsYear);

            var result = await repo.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.InvoiceCounter);
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

        [Fact]
        public async Task CreateAsync_WithZeroAmount_CreatesSuccessfully()
        {
            var (repo, invoicesMockSet, mockContext) = CreateRepositoryWithMocks([]);
            var entity = new ProjectInvoice { ProjectParent = "PRJ1", Amount = 0m };

            var result = await repo.CreateAsync(entity);

            Assert.NotNull(result);
            Assert.Equal(0m, result.Amount);
            invoicesMockSet.Verify(x => x.AddAsync(It.IsAny<ProjectInvoice>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WithNegativeAmount_CreatesSuccessfully()
        {
            var (repo, invoicesMockSet, mockContext) = CreateRepositoryWithMocks([]);
            var entity = new ProjectInvoice { ProjectParent = "PRJ1", Amount = -500m };

            var result = await repo.CreateAsync(entity);

            Assert.NotNull(result);
            Assert.Equal(-500m, result.Amount);
            invoicesMockSet.Verify(x => x.AddAsync(It.IsAny<ProjectInvoice>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WithCompleteEntity_PreservesAllProperties()
        {
            var (repo, _, _) = CreateRepositoryWithMocks([]);
            var entity = new ProjectInvoice
            {
                ProjectParent = "PRJ1",
                Month = 3,
                Amount = 1000m,
                CostOfWork = 800m,
                Wip = 200m,
                ProfitLoss = 0m,
                Detail = "Test Invoice"
            };

            var result = await repo.CreateAsync(entity);

            Assert.Equal("PRJ1", result.ProjectParent);
            Assert.Equal(3, result.Month);
            Assert.Equal(1000m, result.Amount);
            Assert.Equal(800m, result.CostOfWork);
            Assert.Equal(200m, result.Wip);
            Assert.Equal(0m, result.ProfitLoss);
            Assert.Equal("Test Invoice", result.Detail);
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

        [Fact]
        public async Task UpdateAsync_WithCompleteEntity_PreservesAllProperties()
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(DefaultTestFpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);
            var invoicesMockSet = RepositoryTestHelper.CreateMockDbSet<ProjectInvoice>([]);
            mockContext.Setup(x => x.ProjectInvoices).Returns(invoicesMockSet.Object);

            mockContext.Setup(x => x.Entry(It.IsAny<ProjectInvoice>()))
                .Throws(new NotSupportedException("Entry() is not supported in mocked DbContext"));

            var repo = new ProjectInvoiceRepository(mockContext.Object, fpsRequestContext);
            var entity = new ProjectInvoice 
            { 
                InvoiceCounter = 1, 
                ProjectParent = "PRJ1", 
                Month = 3, 
                Amount = 2000m,
                CostOfWork = 1500m,
                Wip = 500m,
                ProfitLoss = 100m,
                Detail = "Updated Invoice"
            };

            await Assert.ThrowsAsync<NotSupportedException>(() => repo.UpdateAsync(entity));

            Assert.Equal("PRJ1", entity.ProjectParent);
            Assert.Equal(3, entity.Month);
            Assert.Equal(2000m, entity.Amount);
            Assert.Equal(DefaultTestFpsYear, entity.FpsYear);
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

        [Fact]
        public async Task GetInvoicesByMonthAsync_Month1_ReturnsCorrectInvoices()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", Month = 1, FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ2", Month = 2, FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);

            var result = await repo.GetInvoicesByMonthAsync(1);

            Assert.Single(result);
            Assert.Equal(1, result[0].Month);
        }

        [Fact]
        public async Task GetInvoicesByMonthAsync_Month12_ReturnsCorrectInvoices()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", Month = 12, FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ2", Month = 11, FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(invoices);

            var result = await repo.GetInvoicesByMonthAsync(12);

            Assert.Single(result);
            Assert.Equal(12, result[0].Month);
        }

        [Fact]
        public async Task GetInvoicesByMonthAsync_DifferentFpsYears_ReturnsAllYears()
        {
            // Current implementation does not filter by FpsYear
            var invoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", Month = 3, FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ2", Month = 3, FpsYear = 2020 }
            };
            var repo = CreateRepository(invoices, fpsYear: DefaultTestFpsYear);

            var result = await repo.GetInvoicesByMonthAsync(3);

            Assert.Equal(2, result.Count);
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
        public async Task HasInvoicesForMonthAsync_WithNoMatchingMonth_ReturnsFalse()
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
        public async Task HasInvoicesForMonthAsync_WithDifferentFpsYear_ReturnsFalse()
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
        public async Task HasInvoicesForMonthAsync_EmptyDatabase_ReturnsFalse()
        {
            var repo = CreateRepository([]);

            var result = await repo.HasInvoicesForMonthAsync(3);

            Assert.False(result);
        }

        #endregion

        #region CopyInvoicesByMonthAsync

        [Fact]
        public async Task CopyInvoicesByMonthAsync_BulkCopyAllInvoices_CreatesNewInvoicesWithCorrectMonth()
        {
            var sourceInvoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", Month = 3, Amount = 1000m, CostOfWork = 800m, Wip = 200m, ProfitLoss = 100m, Detail = "Q1 Invoice", FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ2", Month = 3, Amount = 2000m, CostOfWork = 1600m, Wip = 400m, ProfitLoss = 200m, Detail = "Q1 Report", FpsYear = DefaultTestFpsYear }
            };
            var (repo, invoicesDbSet, mockContext) = CreateRepositoryWithMocks(sourceInvoices);

            var result = await repo.CopyInvoicesByMonthAsync(sourceMonth: 3, targetMonth: 4, specificInvoiceIds: null);

            invoicesDbSet.Verify(x => x.AddRangeAsync(
                It.Is<IEnumerable<ProjectInvoice>>(invoices => invoices.Count() == 2),
                It.IsAny<CancellationToken>()), Times.Once);
            mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CopyInvoicesByMonthAsync_SelectiveCopyByIds_CopiesOnlySpecifiedInvoices()
        {
            var sourceInvoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", Month = 3, Amount = 1000m, FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ2", Month = 3, Amount = 2000m, FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 3, ProjectParent = "PRJ3", Month = 3, Amount = 3000m, FpsYear = DefaultTestFpsYear }
            };
            var (repo, invoicesDbSet, mockContext) = CreateRepositoryWithMocks(sourceInvoices);

            var result = await repo.CopyInvoicesByMonthAsync(sourceMonth: 3, targetMonth: 4, specificInvoiceIds: new List<int> { 1, 3 });

            invoicesDbSet.Verify(x => x.AddRangeAsync(
                It.Is<IEnumerable<ProjectInvoice>>(invoices => invoices.Count() == 2),
                It.IsAny<CancellationToken>()), Times.Once);
            mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CopyInvoicesByMonthAsync_EmptySourceMonth_DoesNotCopyAnyInvoices()
        {
            var sourceInvoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", Month = 6, Amount = 1000m, FpsYear = DefaultTestFpsYear }
            };
            var (repo, invoicesDbSet, mockContext) = CreateRepositoryWithMocks(sourceInvoices);
            List<ProjectInvoice>? capturedInvoices = null;

            invoicesDbSet
                .Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<ProjectInvoice>>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<ProjectInvoice>, CancellationToken>((invoices, _) => capturedInvoices = invoices.ToList())
                .Returns(Task.CompletedTask);

            var result = await repo.CopyInvoicesByMonthAsync(sourceMonth: 3, targetMonth: 4, specificInvoiceIds: null);

            Assert.NotNull(capturedInvoices);
            Assert.Empty(capturedInvoices);
            mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CopyInvoicesByMonthAsync_EmptyInvoiceIdsList_FallsBackToBulkCopy()
        {
            // When an empty list is provided, it falls back to bulk copy behavior
            var sourceInvoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", Month = 3, Amount = 1000m, FpsYear = DefaultTestFpsYear }
            };
            var (repo, invoicesDbSet, mockContext) = CreateRepositoryWithMocks(sourceInvoices);
            List<ProjectInvoice>? capturedInvoices = null;

            invoicesDbSet
                .Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<ProjectInvoice>>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<ProjectInvoice>, CancellationToken>((invoices, _) => capturedInvoices = invoices.ToList())
                .Returns(Task.CompletedTask);

            var result = await repo.CopyInvoicesByMonthAsync(sourceMonth: 3, targetMonth: 4, specificInvoiceIds: new List<int>());

            Assert.NotNull(capturedInvoices);
            Assert.Single(capturedInvoices); // Falls back to bulk copy, so gets the one invoice from month 3
            Assert.Equal(4, capturedInvoices[0].Month);
            mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CopyInvoicesByMonthAsync_NonExistentInvoiceIds_DoesNotCopyAnyInvoices()
        {
            var sourceInvoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", Month = 3, Amount = 1000m, FpsYear = DefaultTestFpsYear }
            };
            var (repo, invoicesDbSet, mockContext) = CreateRepositoryWithMocks(sourceInvoices);
            List<ProjectInvoice>? capturedInvoices = null;

            invoicesDbSet
                .Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<ProjectInvoice>>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<ProjectInvoice>, CancellationToken>((invoices, _) => capturedInvoices = invoices.ToList())
                .Returns(Task.CompletedTask);

            var result = await repo.CopyInvoicesByMonthAsync(sourceMonth: 3, targetMonth: 4, specificInvoiceIds: new List<int> { 99, 100 });

            Assert.NotNull(capturedInvoices);
            Assert.Empty(capturedInvoices);
            mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CopyInvoicesByMonthAsync_CopiedInvoicesPreserveAllProperties()
        {
            var sourceInvoices = new List<ProjectInvoice>
            {
                new() 
                { 
                    InvoiceCounter = 1, 
                    ProjectParent = "PRJ1", 
                    Month = 3, 
                    Amount = 1500m, 
                    CostOfWork = 1200m, 
                    Wip = 300m, 
                    ProfitLoss = 150m, 
                    Detail = "Test Invoice", 
                    FpsYear = DefaultTestFpsYear 
                }
            };
            var (repo, invoicesDbSet, mockContext) = CreateRepositoryWithMocks(sourceInvoices);
            List<ProjectInvoice>? capturedInvoices = null;

            invoicesDbSet
                .Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<ProjectInvoice>>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<ProjectInvoice>, CancellationToken>((invoices, _) => capturedInvoices = invoices.ToList())
                .Returns(Task.CompletedTask);

            var result = await repo.CopyInvoicesByMonthAsync(sourceMonth: 3, targetMonth: 4, specificInvoiceIds: null);

            Assert.NotNull(capturedInvoices);
            Assert.Single(capturedInvoices);
            var copiedInvoice = capturedInvoices.First();
            Assert.Equal("PRJ1", copiedInvoice.ProjectParent);
            Assert.Equal(4, copiedInvoice.Month);
            Assert.Equal(1500m, copiedInvoice.Amount);
            Assert.Equal(1200m, copiedInvoice.CostOfWork);
            Assert.Equal(300m, copiedInvoice.Wip);
            Assert.Equal(150m, copiedInvoice.ProfitLoss);
            Assert.Equal("Test Invoice", copiedInvoice.Detail);
            Assert.Equal(DefaultTestFpsYear, copiedInvoice.FpsYear);
        }

        [Fact]
        public async Task CopyInvoicesByMonthAsync_CopiedInvoicesDoNotIncludeInvoiceCounter()
        {
            var sourceInvoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 5, ProjectParent = "PRJ1", Month = 3, Amount = 1000m, FpsYear = DefaultTestFpsYear }
            };
            var (repo, invoicesDbSet, mockContext) = CreateRepositoryWithMocks(sourceInvoices);
            List<ProjectInvoice>? capturedInvoices = null;

            invoicesDbSet
                .Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<ProjectInvoice>>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<ProjectInvoice>, CancellationToken>((invoices, _) => capturedInvoices = invoices.ToList())
                .Returns(Task.CompletedTask);

            var result = await repo.CopyInvoicesByMonthAsync(sourceMonth: 3, targetMonth: 4, specificInvoiceIds: null);

            Assert.NotNull(capturedInvoices);
            Assert.Single(capturedInvoices);
            var copiedInvoice = capturedInvoices.First();
            Assert.Equal(0, copiedInvoice.InvoiceCounter);
        }

        [Fact]
        public async Task CopyInvoicesByMonthAsync_MultipleInvoices_MaintainsCorrectOrder()
        {
            var sourceInvoices = new List<ProjectInvoice>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ1", Month = 3, Amount = 1000m, FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ2", Month = 3, Amount = 2000m, FpsYear = DefaultTestFpsYear },
                new() { InvoiceCounter = 3, ProjectParent = "PRJ3", Month = 3, Amount = 3000m, FpsYear = DefaultTestFpsYear }
            };
            var (repo, invoicesDbSet, mockContext) = CreateRepositoryWithMocks(sourceInvoices);
            List<ProjectInvoice>? capturedInvoices = null;

            invoicesDbSet
                .Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<ProjectInvoice>>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<ProjectInvoice>, CancellationToken>((invoices, _) => capturedInvoices = invoices.ToList())
                .Returns(Task.CompletedTask);

            var result = await repo.CopyInvoicesByMonthAsync(sourceMonth: 3, targetMonth: 4, specificInvoiceIds: null);

            Assert.NotNull(capturedInvoices);
            Assert.Equal(3, capturedInvoices.Count);
            Assert.Equal("PRJ1", capturedInvoices[0].ProjectParent);
            Assert.Equal("PRJ2", capturedInvoices[1].ProjectParent);
            Assert.Equal("PRJ3", capturedInvoices[2].ProjectParent);
            Assert.All(capturedInvoices, invoice => Assert.Equal(4, invoice.Month));
        }

        #endregion
    }
}
