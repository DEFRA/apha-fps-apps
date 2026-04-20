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

        #endregion
    }
}
