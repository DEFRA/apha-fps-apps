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

namespace Apha.PACT.DataAccess.UnitTests.Repository.TestCapabilityRepositoryTest
{
    public class TestCapabilityRepositoryTests
    {
        private const int DefaultFpsYear = 2024;

        private static (
            TestCapabilityRepository Repo,
            Mock<DbSet<TestCapability>> DbSet,
            Mock<FpsDbContext> Context)
            CreateRepositoryWithMocks(
                IEnumerable<TestCapability> testCapabilities,
                int fpsYear = DefaultFpsYear)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(fpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);
            var dbSetMock = RepositoryTestHelper.CreateMockDbSet(testCapabilities);

            RepositoryTestHelper.SetupDbSetOperations(dbSetMock);
            dbSetMock
                .Setup(x => x.AddAsync(It.IsAny<TestCapability>(), It.IsAny<CancellationToken>()))
                .Returns((TestCapability _, CancellationToken __) => new ValueTask<EntityEntry<TestCapability>>());
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.TestCapabilities).Returns(dbSetMock.Object);

            var repo = new TestCapabilityRepository(mockContext.Object, fpsRequestContext);
            return (repo, dbSetMock, mockContext);
        }

        private static TestCapabilityRepository CreateRepository(
            IEnumerable<TestCapability> testCapabilities,
            int fpsYear = DefaultFpsYear)
            => CreateRepositoryWithMocks(testCapabilities, fpsYear).Repo;

        private static TestCapabilityRepository CreateRepositoryForWgTestCapabilitiesWithDescription(
            IEnumerable<TestCapability> testCapabilities,
            IEnumerable<TestorProduct> testorProducts,
            int fpsYear = DefaultFpsYear)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(fpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);
            var testCapabilitiesDbSet = RepositoryTestHelper.CreateMockDbSet(testCapabilities);
            var testorProductsDbSet = RepositoryTestHelper.CreateMockDbSet(testorProducts);

            RepositoryTestHelper.SetupSaveChanges(mockContext);
            RepositoryTestHelper.SetupDbSetOperations(testCapabilitiesDbSet);
            RepositoryTestHelper.SetupDbSetOperations(testorProductsDbSet);

            mockContext.Setup(x => x.TestCapabilities).Returns(testCapabilitiesDbSet.Object);
            mockContext.Setup(x => x.TestorProducts).Returns(testorProductsDbSet.Object);

            return new TestCapabilityRepository(mockContext.Object, fpsRequestContext);
        }

        #region GetPagedByWorkGroupAsync

        [Fact]
        public async Task GetPagedByWorkGroupAsync_WithNullWorkGroup_ReturnsAllCapabilities()
        {
            var capabilities = new List<TestCapability>
            {
                new() { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear },
                new() { TestCode = "TC2", WorkGroup = "WG2", PlanPortfolio = "PP2", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(capabilities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedByWorkGroupAsync(query, null);

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetPagedByWorkGroupAsync_WithEmptyWorkGroup_ReturnsAllCapabilities()
        {
            var capabilities = new List<TestCapability>
            {
                new() { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear },
                new() { TestCode = "TC2", WorkGroup = "WG2", PlanPortfolio = "PP2", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(capabilities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedByWorkGroupAsync(query, "");

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetPagedByWorkGroupAsync_WithWhitespaceWorkGroup_ReturnsAllCapabilities()
        {
            var capabilities = new List<TestCapability>
            {
                new() { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear },
                new() { TestCode = "TC2", WorkGroup = "WG2", PlanPortfolio = "PP2", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(capabilities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedByWorkGroupAsync(query, "   ");

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetPagedByWorkGroupAsync_WithSpecificWorkGroup_FiltersResults()
        {
            var capabilities = new List<TestCapability>
            {
                new() { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear },
                new() { TestCode = "TC2", WorkGroup = "WG2", PlanPortfolio = "PP2", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(capabilities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedByWorkGroupAsync(query, "WG1");

            Assert.Single(result.Data);
            Assert.Equal("TC1", result.Data.First().TestCode);
        }

        [Fact]
        public async Task GetPagedByWorkGroupAsync_WithTestCodeFilter_FiltersCorrectly()
        {
            var capabilities = new List<TestCapability>
            {
                new() { TestCode = "BLOOD", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear },
                new() { TestCode = "URINE", WorkGroup = "WG1", PlanPortfolio = "PP2", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(capabilities);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"TestCode\":\"BLO\"}"
            };

            var result = await repo.GetPagedByWorkGroupAsync(query, null);

            Assert.Single(result.Data);
            Assert.Equal("BLOOD", result.Data.First().TestCode);
        }

        [Fact]
        public async Task GetPagedByWorkGroupAsync_WithWorkGroupFilter_FiltersCorrectly()
        {
            var capabilities = new List<TestCapability>
            {
                new() { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear },
                new() { TestCode = "TC2", WorkGroup = "WG2", PlanPortfolio = "PP2", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(capabilities);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"WorkGroup\":\"WG1\"}"
            };

            var result = await repo.GetPagedByWorkGroupAsync(query, null);

            Assert.Single(result.Data);
            Assert.Equal("WG1", result.Data.First().WorkGroup);
        }

        [Fact]
        public async Task GetPagedByWorkGroupAsync_WithPlanPortfolioFilter_FiltersCorrectly()
        {
            var capabilities = new List<TestCapability>
            {
                new() { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "ALPHA", FpsYear = DefaultFpsYear },
                new() { TestCode = "TC2", WorkGroup = "WG2", PlanPortfolio = "BETA", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(capabilities);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"PlanPortfolio\":\"ALP\"}"
            };

            var result = await repo.GetPagedByWorkGroupAsync(query, null);

            Assert.Single(result.Data);
            Assert.Equal("ALPHA", result.Data.First().PlanPortfolio);
        }

        [Fact]
        public async Task GetPagedByWorkGroupAsync_WithCombinedFilters_FiltersCorrectly()
        {
            var capabilities = new List<TestCapability>
            {
                new() { TestCode = "BLOOD", WorkGroup = "WG1", PlanPortfolio = "ALPHA", FpsYear = DefaultFpsYear },
                new() { TestCode = "BLOOD", WorkGroup = "WG2", PlanPortfolio = "ALPHA", FpsYear = DefaultFpsYear },
                new() { TestCode = "URINE", WorkGroup = "WG1", PlanPortfolio = "BETA", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(capabilities);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"TestCode\":\"BLOOD\",\"WorkGroup\":\"WG1\"}"
            };

            var result = await repo.GetPagedByWorkGroupAsync(query, null);

            Assert.Single(result.Data);
            Assert.Equal("BLOOD", result.Data.First().TestCode);
            Assert.Equal("WG1", result.Data.First().WorkGroup);
        }

        [Fact]
        public async Task GetPagedByWorkGroupAsync_NullFilter_ReturnsAll()
        {
            var capabilities = new List<TestCapability>
            {
                new() { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear },
                new() { TestCode = "TC2", WorkGroup = "WG2", PlanPortfolio = "PP2", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(capabilities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = null };

            var result = await repo.GetPagedByWorkGroupAsync(query, null);

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetPagedByWorkGroupAsync_EmptyFilter_ReturnsAll()
        {
            var capabilities = new List<TestCapability>
            {
                new() { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear },
                new() { TestCode = "TC2", WorkGroup = "WG2", PlanPortfolio = "PP2", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(capabilities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "" };

            var result = await repo.GetPagedByWorkGroupAsync(query, null);

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetPagedByWorkGroupAsync_WhitespaceFilter_ReturnsAll()
        {
            var capabilities = new List<TestCapability>
            {
                new() { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear },
                new() { TestCode = "TC2", WorkGroup = "WG2", PlanPortfolio = "PP2", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(capabilities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "   " };

            var result = await repo.GetPagedByWorkGroupAsync(query, null);

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetPagedByWorkGroupAsync_FilterWithUnknownKey_IgnoresUnknownKey()
        {
            var capabilities = new List<TestCapability>
            {
                new() { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear },
                new() { TestCode = "TC2", WorkGroup = "WG2", PlanPortfolio = "PP2", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(capabilities);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"Unknown\":\"value\"}"
            };

            var result = await repo.GetPagedByWorkGroupAsync(query, null);

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetPagedByWorkGroupAsync_FilterWithEmptyValues_ReturnsAll()
        {
            var capabilities = new List<TestCapability>
            {
                new() { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear },
                new() { TestCode = "TC2", WorkGroup = "WG2", PlanPortfolio = "PP2", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(capabilities);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"TestCode\":\"\",\"WorkGroup\":\"\",\"PlanPortfolio\":\"\"}"
            };

            var result = await repo.GetPagedByWorkGroupAsync(query, null);

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetPagedByWorkGroupAsync_FilterWithWhitespaceValues_ReturnsAll()
        {
            var capabilities = new List<TestCapability>
            {
                new() { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear },
                new() { TestCode = "TC2", WorkGroup = "WG2", PlanPortfolio = "PP2", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(capabilities);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"TestCode\":\"   \",\"WorkGroup\":\"   \",\"PlanPortfolio\":\"   \"}"
            };

            var result = await repo.GetPagedByWorkGroupAsync(query, null);

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetPagedByWorkGroupAsync_EmptyData_ReturnsEmptyPagedResult()
        {
            var repo = CreateRepository([]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedByWorkGroupAsync(query, null);

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetPagedByWorkGroupAsync_OrdersByTestCodeAscending()
        {
            var capabilities = new List<TestCapability>
            {
                new() { TestCode = "ZZZ", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear },
                new() { TestCode = "AAA", WorkGroup = "WG1", PlanPortfolio = "PP2", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(capabilities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedByWorkGroupAsync(query, null);

            Assert.Equal("AAA", result.Data.ElementAt(0).TestCode);
            Assert.Equal("ZZZ", result.Data.ElementAt(1).TestCode);
        }

        [Fact]
        public async Task GetPagedByWorkGroupAsync_SortByAscending_UsesEfPropertySort()
        {
            var capabilities = new List<TestCapability>
            {
                new() { TestCode = "ZZZ", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear },
                new() { TestCode = "AAA", WorkGroup = "WG1", PlanPortfolio = "PP2", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(capabilities);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                SortBy = "TestCode", Descending = false
            };

            // EF.Property<object> cannot be evaluated in-memory; verify the code path is entered
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => repo.GetPagedByWorkGroupAsync(query, null));
        }

        [Fact]
        public async Task GetPagedByWorkGroupAsync_SortByDescending_UsesEfPropertySort()
        {
            var capabilities = new List<TestCapability>
            {
                new() { TestCode = "AAA", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear },
                new() { TestCode = "ZZZ", WorkGroup = "WG1", PlanPortfolio = "PP2", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(capabilities);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                SortBy = "TestCode", Descending = true
            };

            // EF.Property<object> cannot be evaluated in-memory; verify the code path is entered
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => repo.GetPagedByWorkGroupAsync(query, null));
        }

        [Fact]
        public async Task GetPagedByWorkGroupAsync_Paging_ReturnsCorrectPage()
        {
            var capabilities = Enumerable.Range(1, 5).Select(i =>
                new TestCapability { TestCode = $"TC{i:D3}", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear }).ToList();
            var repo = CreateRepository(capabilities);
            var query = new PaginationParameters<string> { Page = 2, PageSize = 2 };

            var result = await repo.GetPagedByWorkGroupAsync(query, null);

            Assert.Equal(2, result.Data.Count);
            Assert.Equal(5, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.PageNumber);
            Assert.Equal(3, result.PaginationData.TotalPages);
        }

        #endregion

        #region GetPagedByTestCodeAsync

        [Fact]
        public async Task GetPagedByTestCodeAsync_WithNullTestCode_ReturnsAllCapabilities()
        {
            var capabilities = new List<TestCapability>
            {
                new() { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear },
                new() { TestCode = "TC2", WorkGroup = "WG2", PlanPortfolio = "PP2", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(capabilities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedByTestCodeAsync(query, null);

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_WithEmptyTestCode_ReturnsAllCapabilities()
        {
            var capabilities = new List<TestCapability>
            {
                new() { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear },
                new() { TestCode = "TC2", WorkGroup = "WG2", PlanPortfolio = "PP2", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(capabilities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedByTestCodeAsync(query, "");

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_WithWhitespaceTestCode_ReturnsAllCapabilities()
        {
            var capabilities = new List<TestCapability>
            {
                new() { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear },
                new() { TestCode = "TC2", WorkGroup = "WG2", PlanPortfolio = "PP2", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(capabilities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedByTestCodeAsync(query, "   ");

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_WithSpecificTestCode_FiltersResults()
        {
            var capabilities = new List<TestCapability>
            {
                new() { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear },
                new() { TestCode = "TC2", WorkGroup = "WG2", PlanPortfolio = "PP2", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(capabilities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedByTestCodeAsync(query, "TC1");

            Assert.Single(result.Data);
            Assert.Equal("TC1", result.Data.First().TestCode);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_EmptyData_ReturnsEmptyPagedResult()
        {
            var repo = CreateRepository([]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedByTestCodeAsync(query, "TC1");

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_NoSortBy_DefaultsToOrderByTestCode()
        {
            var capabilities = new List<TestCapability>
            {
                new() { TestCode = "ZZZ", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear },
                new() { TestCode = "AAA", WorkGroup = "WG1", PlanPortfolio = "PP2", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(capabilities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedByTestCodeAsync(query, null);

            Assert.Equal("AAA", result.Data.ElementAt(0).TestCode);
            Assert.Equal("ZZZ", result.Data.ElementAt(1).TestCode);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_SortByAscending_UsesEfPropertySort()
        {
            var capabilities = new List<TestCapability>
            {
                new() { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear },
                new() { TestCode = "TC2", WorkGroup = "WG2", PlanPortfolio = "PP2", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(capabilities);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                SortBy = "WorkGroup", Descending = false
            };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => repo.GetPagedByTestCodeAsync(query, null));
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_SortByDescending_UsesEfPropertySort()
        {
            var capabilities = new List<TestCapability>
            {
                new() { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear },
                new() { TestCode = "TC2", WorkGroup = "WG2", PlanPortfolio = "PP2", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(capabilities);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                SortBy = "WorkGroup", Descending = true
            };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => repo.GetPagedByTestCodeAsync(query, null));
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_WithFilter_FiltersCorrectly()
        {
            var capabilities = new List<TestCapability>
            {
                new() { TestCode = "BLOOD", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear },
                new() { TestCode = "URINE", WorkGroup = "WG2", PlanPortfolio = "PP2", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(capabilities);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"TestCode\":\"BLO\"}"
            };

            var result = await repo.GetPagedByTestCodeAsync(query, null);

            Assert.Single(result.Data);
            Assert.Equal("BLOOD", result.Data.First().TestCode);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_Paging_ReturnsCorrectPage()
        {
            var capabilities = Enumerable.Range(1, 5).Select(i =>
                new TestCapability { TestCode = $"TC{i:D3}", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear }).ToList();
            var repo = CreateRepository(capabilities);
            var query = new PaginationParameters<string> { Page = 2, PageSize = 2 };

            var result = await repo.GetPagedByTestCodeAsync(query, null);

            Assert.Equal(2, result.Data.Count);
            Assert.Equal(5, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.PageNumber);
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_MatchingRecord_ReturnsEntity()
        {
            var capabilities = new List<TestCapability>
            {
                new() { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(capabilities);

            var result = await repo.GetByIdAsync("TC1", "WG1");

            Assert.NotNull(result);
            Assert.Equal("TC1", result.TestCode);
            Assert.Equal("WG1", result.WorkGroup);
        }

        [Fact]
        public async Task GetByIdAsync_TestCodeNotFound_ReturnsNull()
        {
            var capabilities = new List<TestCapability>
            {
                new() { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(capabilities);

            var result = await repo.GetByIdAsync("MISSING", "WG1");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_WorkGroupNotFound_ReturnsNull()
        {
            var capabilities = new List<TestCapability>
            {
                new() { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(capabilities);

            var result = await repo.GetByIdAsync("TC1", "WG_MISSING");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_EmptyRepository_ReturnsNull()
        {
            var repo = CreateRepository([]);

            var result = await repo.GetByIdAsync("TC1", "WG1");

            Assert.Null(result);
        }

        #endregion

        #region AddAsync

        [Fact]
        public async Task AddAsync_SetsEntityFpsYearFromContext()
        {
            var (repo, _, mockContext) = CreateRepositoryWithMocks([], fpsYear: 2025);
            var entity = new TestCapability { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1" };

            var result = await repo.AddAsync(entity);

            Assert.Equal(2025, result.FpsYear);
            mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task AddAsync_ReturnsTheSameEntityInstance()
        {
            var (repo, _, _) = CreateRepositoryWithMocks([]);
            var entity = new TestCapability { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1" };

            var result = await repo.AddAsync(entity);

            Assert.Same(entity, result);
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_RecordExistsWithCorrectYear_RemovesAndReturnsTrue()
        {
            var capabilities = new List<TestCapability>
            {
                new() { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear }
            };
            var (repo, dbSetMock, mockContext) = CreateRepositoryWithMocks(capabilities);

            var result = await repo.DeleteAsync("TC1", "WG1");

            Assert.True(result);
            dbSetMock.Verify(x => x.Remove(It.IsAny<TestCapability>()), Times.Once);
            mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_RecordNotFound_ReturnsFalse()
        {
            var capabilities = new List<TestCapability>
            {
                new() { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear }
            };
            var (repo, dbSetMock, _) = CreateRepositoryWithMocks(capabilities);

            var result = await repo.DeleteAsync("MISSING", "WG1");

            Assert.False(result);
            dbSetMock.Verify(x => x.Remove(It.IsAny<TestCapability>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_RecordExistsButWrongFpsYear_ReturnsFalse()
        {
            var capabilities = new List<TestCapability>
            {
                new() { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = 2023 }
            };
            var (repo, dbSetMock, _) = CreateRepositoryWithMocks(capabilities, fpsYear: DefaultFpsYear);

            var result = await repo.DeleteAsync("TC1", "WG1");

            Assert.False(result);
            dbSetMock.Verify(x => x.Remove(It.IsAny<TestCapability>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_EmptyRepository_ReturnsFalse()
        {
            var (repo, dbSetMock, _) = CreateRepositoryWithMocks([]);

            var result = await repo.DeleteAsync("TC1", "WG1");

            Assert.False(result);
            dbSetMock.Verify(x => x.Remove(It.IsAny<TestCapability>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_WorkGroupNotMatching_ReturnsFalse()
        {
            var capabilities = new List<TestCapability>
            {
                new() { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear }
            };
            var (repo, dbSetMock, _) = CreateRepositoryWithMocks(capabilities);

            var result = await repo.DeleteAsync("TC1", "WG_WRONG");

            Assert.False(result);
            dbSetMock.Verify(x => x.Remove(It.IsAny<TestCapability>()), Times.Never);
        }

        #endregion

        #region GetPagedTestCapabilityByPortfolioAsync

        [Fact]
        public async Task GetPagedTestCapabilityByPortfolioAsync_WithNullPortfolio_ReturnsAll()
        {
            var capabilities = new List<TestCapability>
            {
                new() { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear },
                new() { TestCode = "TC2", WorkGroup = "WG2", PlanPortfolio = "PP2", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(capabilities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedTestCapabilityByPortfolioAsync(query, null);

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetPagedTestCapabilityByPortfolioAsync_WithEmptyPortfolio_ReturnsAll()
        {
            var capabilities = new List<TestCapability>
            {
                new() { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear },
                new() { TestCode = "TC2", WorkGroup = "WG2", PlanPortfolio = "PP2", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(capabilities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedTestCapabilityByPortfolioAsync(query, "");

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetPagedTestCapabilityByPortfolioAsync_WithWhitespacePortfolio_ReturnsAll()
        {
            var capabilities = new List<TestCapability>
            {
                new() { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear },
                new() { TestCode = "TC2", WorkGroup = "WG2", PlanPortfolio = "PP2", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(capabilities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedTestCapabilityByPortfolioAsync(query, "   ");

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetPagedTestCapabilityByPortfolioAsync_WithSpecificPortfolio_FiltersResults()
        {
            var capabilities = new List<TestCapability>
            {
                new() { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear },
                new() { TestCode = "TC2", WorkGroup = "WG2", PlanPortfolio = "PP2", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(capabilities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedTestCapabilityByPortfolioAsync(query, "PP1");

            Assert.Single(result.Data);
            Assert.Equal("TC1", result.Data.First().TestCode);
        }

        [Fact]
        public async Task GetPagedTestCapabilityByPortfolioAsync_NoMatchingPortfolio_ReturnsEmpty()
        {
            var capabilities = new List<TestCapability>
            {
                new() { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(capabilities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedTestCapabilityByPortfolioAsync(query, "MISSING");

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetPagedTestCapabilityByPortfolioAsync_EmptyData_ReturnsEmpty()
        {
            var repo = CreateRepository([]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedTestCapabilityByPortfolioAsync(query, null);

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetPagedTestCapabilityByPortfolioAsync_NoSortBy_DefaultsToOrderByTestCode()
        {
            var capabilities = new List<TestCapability>
            {
                new() { TestCode = "ZZZ", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear },
                new() { TestCode = "AAA", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(capabilities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedTestCapabilityByPortfolioAsync(query, null);

            Assert.Equal("AAA", result.Data.ElementAt(0).TestCode);
            Assert.Equal("ZZZ", result.Data.ElementAt(1).TestCode);
        }

        [Fact]
        public async Task GetPagedTestCapabilityByPortfolioAsync_SortByItemDescription_DefaultsToOrderByTestCode()
        {
            var capabilities = new List<TestCapability>
            {
                new() { TestCode = "ZZZ", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear },
                new() { TestCode = "AAA", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(capabilities);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                SortBy = "ItemDescription", Descending = false
            };

            var result = await repo.GetPagedTestCapabilityByPortfolioAsync(query, null);

            Assert.Equal("AAA", result.Data.ElementAt(0).TestCode);
            Assert.Equal("ZZZ", result.Data.ElementAt(1).TestCode);
        }

        [Fact]
        public async Task GetPagedTestCapabilityByPortfolioAsync_SortByItemDescriptionDescending_DefaultsToOrderByTestCode()
        {
            var capabilities = new List<TestCapability>
            {
                new() { TestCode = "ZZZ", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear },
                new() { TestCode = "AAA", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(capabilities);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                SortBy = "ItemDescription", Descending = true
            };

            var result = await repo.GetPagedTestCapabilityByPortfolioAsync(query, null);

            Assert.Equal("AAA", result.Data.ElementAt(0).TestCode);
            Assert.Equal("ZZZ", result.Data.ElementAt(1).TestCode);
        }

        [Fact]
        public async Task GetPagedTestCapabilityByPortfolioAsync_SortByValidColumnAscending_UsesEfPropertySort()
        {
            var capabilities = new List<TestCapability>
            {
                new() { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear },
                new() { TestCode = "TC2", WorkGroup = "WG2", PlanPortfolio = "PP2", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(capabilities);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                SortBy = "WorkGroup", Descending = false
            };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => repo.GetPagedTestCapabilityByPortfolioAsync(query, null));
        }

        [Fact]
        public async Task GetPagedTestCapabilityByPortfolioAsync_SortByValidColumnDescending_UsesEfPropertySort()
        {
            var capabilities = new List<TestCapability>
            {
                new() { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear },
                new() { TestCode = "TC2", WorkGroup = "WG2", PlanPortfolio = "PP2", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(capabilities);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                SortBy = "WorkGroup", Descending = true
            };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => repo.GetPagedTestCapabilityByPortfolioAsync(query, null));
        }

        [Fact]
        public async Task GetPagedTestCapabilityByPortfolioAsync_WithFilter_FiltersCorrectly()
        {
            var capabilities = new List<TestCapability>
            {
                new() { TestCode = "BLOOD", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear },
                new() { TestCode = "URINE", WorkGroup = "WG2", PlanPortfolio = "PP2", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(capabilities);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"TestCode\":\"BLO\"}"
            };

            var result = await repo.GetPagedTestCapabilityByPortfolioAsync(query, null);

            Assert.Single(result.Data);
            Assert.Equal("BLOOD", result.Data.First().TestCode);
        }

        [Fact]
        public async Task GetPagedTestCapabilityByPortfolioAsync_Paging_ReturnsCorrectPage()
        {
            var capabilities = Enumerable.Range(1, 5).Select(i =>
                new TestCapability { TestCode = $"TC{i:D3}", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear }).ToList();
            var repo = CreateRepository(capabilities);
            var query = new PaginationParameters<string> { Page = 2, PageSize = 2 };

            var result = await repo.GetPagedTestCapabilityByPortfolioAsync(query, null);

            Assert.Equal(2, result.Data.Count);
            Assert.Equal(5, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.PageNumber);
        }

        #endregion

        #region GetPagedUserTestCapabilitiesAsync

        [Fact]
        public async Task GetPagedUserTestCapabilitiesAsync_WithMatchingJoin_DefaultSortsByTestCodeAscending()
        {
            var capabilities = new List<TestCapability>
            {
                new() { WorkGroup = "WG1", TestCode = "TC2", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear },
                new() { WorkGroup = "WG1", TestCode = "TC1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear },
                new() { WorkGroup = "WG2", TestCode = "TC3", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear }
            };
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "TC1", ItemDescription = "Desc 1", FpsYear = DefaultFpsYear },
                new() { ItemCode = "TC2", ItemDescription = "Desc 2", FpsYear = DefaultFpsYear },
                new() { ItemCode = "TC3", ItemDescription = "Desc 3", FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepositoryForWgTestCapabilitiesWithDescription(capabilities, testorProducts);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedWgTestCapabilitiesWithDescriptionAsync(query, "WG1");

            Assert.Equal(2, result.Data.Count);
            Assert.Equal("TC1", result.Data.ElementAt(0).TestCode);
            Assert.Equal("TC2", result.Data.ElementAt(1).TestCode);
            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetPagedUserTestCapabilitiesAsync_WithDuplicateJoinRows_ReturnsJoinedRows()
        {
            var capabilities = new List<TestCapability>
            {
                new() { WorkGroup = "WG1", TestCode = "TC1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear },
                new() { WorkGroup = "WG1", TestCode = "TC1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear }
            };
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "TC1", ItemDescription = "Desc 1", FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepositoryForWgTestCapabilitiesWithDescription(capabilities, testorProducts);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedWgTestCapabilitiesWithDescriptionAsync(query, "WG1");

            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, x => Assert.Equal("TC1", x.TestCode));
        }

        [Fact]
        public async Task GetPagedUserTestCapabilitiesAsync_FilterByWorkGroup_AppliesFilter()
        {
            var capabilities = new List<TestCapability>
            {
                new() { WorkGroup = "WG1", TestCode = "TC1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear },
                new() { WorkGroup = "WG1", TestCode = "TC2", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear }
            };
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "TC1", ItemDescription = "Desc 1", FpsYear = DefaultFpsYear },
                new() { ItemCode = "TC2", ItemDescription = "Desc 2", FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepositoryForWgTestCapabilitiesWithDescription(capabilities, testorProducts);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"WorkGroup\":\"WG1\"}"
            };

            var result = await repo.GetPagedWgTestCapabilitiesWithDescriptionAsync(query, "WG1");

            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, x => Assert.Equal("WG1", x.WorkGroup));
        }

        [Fact]
        public async Task GetPagedWgTestCapabilitiesWithDescriptionAsync_FilterByTestCode_AppliesFilter()
        {
            var capabilities = new List<TestCapability>
            {
                new() { WorkGroup = "WG1", TestCode = "BLOOD", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear },
                new() { WorkGroup = "WG1", TestCode = "URINE", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear }
            };
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = "Blood test", FpsYear = DefaultFpsYear },
                new() { ItemCode = "URINE", ItemDescription = "Urine test", FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepositoryForWgTestCapabilitiesWithDescription(capabilities, testorProducts);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"TestCode\":\"BLO\"}"
            };

            var result = await repo.GetPagedWgTestCapabilitiesWithDescriptionAsync(query, "WG1");

            Assert.Single(result.Data);
            Assert.Equal("BLOOD", result.Data.First().TestCode);
        }

        [Fact]
        public async Task GetPagedWgTestCapabilitiesWithDescriptionAsync_FilterByItemDescription_AppliesFilter()
        {
            var capabilities = new List<TestCapability>
            {
                new() { WorkGroup = "WG1", TestCode = "TC1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear },
                new() { WorkGroup = "WG1", TestCode = "TC2", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear }
            };
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "TC1", ItemDescription = "Alpha Desc", FpsYear = DefaultFpsYear },
                new() { ItemCode = "TC2", ItemDescription = "Beta Desc", FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepositoryForWgTestCapabilitiesWithDescription(capabilities, testorProducts);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"ItemDescription\":\"Alpha\"}"
            };

            var result = await repo.GetPagedWgTestCapabilitiesWithDescriptionAsync(query, "WG1");

            Assert.Single(result.Data);
            Assert.Equal("TC1", result.Data.First().TestCode);
            Assert.Equal("Alpha Desc", result.Data.First().ItemDescription);
        }

        [Fact]
        public async Task GetPagedWgTestCapabilitiesWithDescriptionAsync_NullFilter_ReturnsAllForWorkGroup()
        {
            var capabilities = new List<TestCapability>
            {
                new() { WorkGroup = "WG1", TestCode = "TC1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear },
                new() { WorkGroup = "WG1", TestCode = "TC2", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear }
            };
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "TC1", ItemDescription = "Desc 1", FpsYear = DefaultFpsYear },
                new() { ItemCode = "TC2", ItemDescription = "Desc 2", FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepositoryForWgTestCapabilitiesWithDescription(capabilities, testorProducts);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = null };

            var result = await repo.GetPagedWgTestCapabilitiesWithDescriptionAsync(query, "WG1");

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetPagedWgTestCapabilitiesWithDescriptionAsync_WhitespaceFilter_ReturnsAllForWorkGroup()
        {
            var capabilities = new List<TestCapability>
            {
                new() { WorkGroup = "WG1", TestCode = "TC1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear },
                new() { WorkGroup = "WG1", TestCode = "TC2", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear }
            };
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "TC1", ItemDescription = "Desc 1", FpsYear = DefaultFpsYear },
                new() { ItemCode = "TC2", ItemDescription = "Desc 2", FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepositoryForWgTestCapabilitiesWithDescription(capabilities, testorProducts);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "   " };

            var result = await repo.GetPagedWgTestCapabilitiesWithDescriptionAsync(query, "WG1");

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetPagedUserTestCapabilitiesAsync_FilterJsonNull_IgnoresFilter()
        {
            var capabilities = new List<TestCapability>
            {
                new() { WorkGroup = "WG1", TestCode = "TC1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear }
            };
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "TC1", ItemDescription = "Desc 1", FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepositoryForWgTestCapabilitiesWithDescription(capabilities, testorProducts);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "null" };

            var result = await repo.GetPagedWgTestCapabilitiesWithDescriptionAsync(query, "WG1");

            Assert.Single(result.Data);
        }

        [Fact]
        public async Task GetPagedUserTestCapabilitiesAsync_FilterUnknownAndWhitespaceValues_IgnoresNonUsableFilters()
        {
            var capabilities = new List<TestCapability>
            {
                new() { WorkGroup = "WG1", TestCode = "TC1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear },
                new() { WorkGroup = "WG1", TestCode = "TC2", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear }
            };
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "TC1", ItemDescription = "Desc 1", FpsYear = DefaultFpsYear },
                new() { ItemCode = "TC2", ItemDescription = "Desc 2", FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepositoryForWgTestCapabilitiesWithDescription(capabilities, testorProducts);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"Unknown\":\"value\",\"WorkGroup\":\"   \",\"TestCode\":\"\",\"ItemDescription\":\"   \"}"
            };

            var result = await repo.GetPagedWgTestCapabilitiesWithDescriptionAsync(query, "WG1");

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetPagedWgTestCapabilitiesWithDescriptionAsync_SortByAscending_UsesEfPropertySort()
        {
            var capabilities = new List<TestCapability>
            {
                new() { WorkGroup = "WG1", TestCode = "TC2", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear },
                new() { WorkGroup = "WG1", TestCode = "TC1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear }
            };
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "TC1", ItemDescription = "Desc 1", FpsYear = DefaultFpsYear },
                new() { ItemCode = "TC2", ItemDescription = "Desc 2", FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepositoryForWgTestCapabilitiesWithDescription(capabilities, testorProducts);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "TestCode",
                Descending = false
            };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => repo.GetPagedWgTestCapabilitiesWithDescriptionAsync(query, "WG1"));
        }

        [Fact]
        public async Task GetPagedUserTestCapabilitiesAsync_SortByDescending_UsesEfPropertySort()
        {
            var capabilities = new List<TestCapability>
            {
                new() { WorkGroup = "WG1", TestCode = "TC1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear },
                new() { WorkGroup = "WG1", TestCode = "TC2", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear }
            };
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "TC1", ItemDescription = "Desc 1", FpsYear = DefaultFpsYear },
                new() { ItemCode = "TC2", ItemDescription = "Desc 2", FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepositoryForWgTestCapabilitiesWithDescription(capabilities, testorProducts);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "TestCode",
                Descending = true
            };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => repo.GetPagedWgTestCapabilitiesWithDescriptionAsync(query, "WG1"));
        }

        [Fact]
        public async Task GetPagedWgTestCapabilitiesWithDescriptionAsync_Paging_ReturnsCorrectPageAndMetadata()
        {
            var capabilities = new List<TestCapability>
            {
                new() { WorkGroup = "WG1", TestCode = "TC1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear },
                new() { WorkGroup = "WG1", TestCode = "TC2", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear },
                new() { WorkGroup = "WG1", TestCode = "TC3", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear },
                new() { WorkGroup = "WG1", TestCode = "TC4", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear }
            };
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "TC1", ItemDescription = "Desc 1", FpsYear = DefaultFpsYear },
                new() { ItemCode = "TC2", ItemDescription = "Desc 2", FpsYear = DefaultFpsYear },
                new() { ItemCode = "TC3", ItemDescription = "Desc 3", FpsYear = DefaultFpsYear },
                new() { ItemCode = "TC4", ItemDescription = "Desc 4", FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepositoryForWgTestCapabilitiesWithDescription(capabilities, testorProducts);
            var query = new PaginationParameters<string> { Page = 2, PageSize = 2 };

            var result = await repo.GetPagedWgTestCapabilitiesWithDescriptionAsync(query, "WG1");

            Assert.Equal(2, result.Data.Count);
            Assert.Equal("TC3", result.Data.First().TestCode);
            Assert.Equal(4, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.PageNumber);
            Assert.Equal(2, result.PaginationData.PageSize);
            Assert.Equal(2, result.PaginationData.TotalPages);
        }

        [Fact]
        public async Task GetPagedUserTestCapabilitiesAsync_NoMatchingJoinRows_ReturnsEmpty()
        {
            var capabilities = new List<TestCapability>
            {
                new() { WorkGroup = "WG1", TestCode = "TC_NO_PRODUCT", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear }
            };
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "DIFFERENT_CODE", ItemDescription = "Desc", FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepositoryForWgTestCapabilitiesWithDescription(capabilities, testorProducts);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedWgTestCapabilitiesWithDescriptionAsync(query, "WG1");

            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_SetsEntityFpsYearFromContext()
        {
            var (repo, _, _) = CreateRepositoryWithMocks([], fpsYear: 2025);
            var entity = new TestCapability { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = 2020 };

            // Entry() cannot be mocked (internal EF ctor); verify FpsYear is set before that call
            await Assert.ThrowsAsync<NullReferenceException>(() => repo.UpdateAsync(entity));

            Assert.Equal(2025, entity.FpsYear);
        }

        [Fact]
        public async Task UpdateAsync_SetsEntityStateToModified_ThrowsBecauseEntryCannotBeMocked()
        {
            var (repo, _, _) = CreateRepositoryWithMocks([]);
            var entity = new TestCapability { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1" };

            // Confirms code path reaches _context.Entry(entity).State assignment
            await Assert.ThrowsAsync<NullReferenceException>(() => repo.UpdateAsync(entity));
        }

        [Fact]
        public async Task UpdateAsync_SetsEntityFpsYearBeforeEntryCall()
        {
            var (repo, _, _) = CreateRepositoryWithMocks([], fpsYear: 2025);
            var entity = new TestCapability { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1", UnitCost = 50m, FpsYear = 2020 };

            await Assert.ThrowsAsync<NullReferenceException>(() => repo.UpdateAsync(entity));

            Assert.Equal(2025, entity.FpsYear);
            Assert.Equal(50m, entity.UnitCost);
        }

        #endregion

        #region ExistsAsync

        [Fact]
        public async Task ExistsAsync_RecordExists_ReturnsTrue()
        {
            var capabilities = new List<TestCapability>
            {
                new() { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(capabilities);

            var result = await repo.ExistsAsync("TC1", "PP1");

            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_TestCodeNotFound_ReturnsFalse()
        {
            var capabilities = new List<TestCapability>
            {
                new() { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(capabilities);

            var result = await repo.ExistsAsync("MISSING", "PP1");

            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_PortfolioNotFound_ReturnsFalse()
        {
            var capabilities = new List<TestCapability>
            {
                new() { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(capabilities);

            var result = await repo.ExistsAsync("TC1", "PP_WRONG");

            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_EmptyRepository_ReturnsFalse()
        {
            var repo = CreateRepository([]);

            var result = await repo.ExistsAsync("TC1", "PP1");

            Assert.False(result);
        }

        #endregion
    }
}
