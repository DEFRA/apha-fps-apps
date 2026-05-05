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

namespace Apha.PACT.DataAccess.UnitTests.Repository.TestRequirementRepositoryTest
{
    public class TestRequirementRepositoryTests
    {
        private const int DefaultFpsYear = 2024;

        private static (
            TestRequirementRepository Repo,
            Mock<DbSet<TestRequirement>> TestReqmtsDbSet,
            Mock<DbSet<MonthlyOutput>> MonthlyOutputsDbSet,
            Mock<FpsDbContext> Context)
            CreateRepositoryWithMocks(
                IEnumerable<TestRequirement>? testReqmts = null,
                IEnumerable<MonthlyOutput>? monthlyOutputs = null,
                int fpsYear = DefaultFpsYear)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(fpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);

            var testReqmtsMockSet = RepositoryTestHelper.CreateMockDbSet(testReqmts ?? []);
            RepositoryTestHelper.SetupDbSetOperations(testReqmtsMockSet);
            testReqmtsMockSet
                .Setup(x => x.AddAsync(It.IsAny<TestRequirement>(), It.IsAny<CancellationToken>()))
                .Returns((TestRequirement _, CancellationToken __) => new ValueTask<EntityEntry<TestRequirement>>());

            var monthlyOutputsMockSet = RepositoryTestHelper.CreateMockDbSet(monthlyOutputs ?? []);

            var testReqLogsMockSet = RepositoryTestHelper.CreateMockDbSet(new List<TestRequirementLog>());
            RepositoryTestHelper.SetupDbSetOperations(testReqLogsMockSet);
            testReqLogsMockSet
                .Setup(x => x.AddAsync(It.IsAny<TestRequirementLog>(), It.IsAny<CancellationToken>()))
                .Returns((TestRequirementLog _, CancellationToken __) => new ValueTask<EntityEntry<TestRequirementLog>>());

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.TestRequirements).Returns(testReqmtsMockSet.Object);
            mockContext.Setup(x => x.MonthlyOutputs).Returns(monthlyOutputsMockSet.Object);
            mockContext.Setup(x => x.TestRequirementLogs).Returns(testReqLogsMockSet.Object);

            var repo = new TestRequirementRepository(mockContext.Object, fpsRequestContext);
            return (repo, testReqmtsMockSet, monthlyOutputsMockSet, mockContext);
        }

        private static TestRequirementRepository CreateRepositoryWithJoinMocks(
            IEnumerable<TestRequirement>? testReqmts = null,
            IEnumerable<TestorProduct>? testorProducts = null,
            IEnumerable<Project>? projects = null,
            int fpsYear = DefaultFpsYear)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(fpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);

            var testReqmtsMockSet = RepositoryTestHelper.CreateMockDbSet(testReqmts ?? []);
            RepositoryTestHelper.SetupDbSetOperations(testReqmtsMockSet);

            var testorProductsMockSet = RepositoryTestHelper.CreateMockDbSet(testorProducts ?? []);
            RepositoryTestHelper.SetupDbSetOperations(testorProductsMockSet);

            var projectsMockSet = RepositoryTestHelper.CreateMockDbSet(projects ?? []);
            RepositoryTestHelper.SetupDbSetOperations(projectsMockSet);

            var monthlyOutputsMockSet = RepositoryTestHelper.CreateMockDbSet(new List<MonthlyOutput>());
            var testReqLogsMockSet = RepositoryTestHelper.CreateMockDbSet(new List<TestRequirementLog>());

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.TestRequirements).Returns(testReqmtsMockSet.Object);
            mockContext.Setup(x => x.TestorProducts).Returns(testorProductsMockSet.Object);
            mockContext.Setup(x => x.Projects).Returns(projectsMockSet.Object);
            mockContext.Setup(x => x.MonthlyOutputs).Returns(monthlyOutputsMockSet.Object);
            mockContext.Setup(x => x.TestRequirementLogs).Returns(testReqLogsMockSet.Object);

            return new TestRequirementRepository(mockContext.Object, fpsRequestContext);
        }

        #region GetPagedByProjectAsync

        [Fact]
        public async Task GetPagedByProjectAsync_MatchingBuyer_ReturnsMatchingRecords()
        {
            var testorProduct = new TestorProduct { ItemCode = "BLOOD", UnitPriceVla = 10m, DefraUnitPrice = 12m };
            var project = new Project { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "Test", Program = "P1", Customer = "C1", Disease = "D1", Contract = "CT1", IncomeAccountCode = "INC1" };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear },
                new() { TestCode = "URINE", Buyer = "PRJ2", FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepositoryWithJoinMocks(
                testReqmts: testReqmts,
                testorProducts: [testorProduct],
                projects: [project]);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedByProjectAsync(query, "PRJ1");

            Assert.Single(result.Data);
            Assert.Equal("BLOOD", result.Data.First().TestCode);
            Assert.Equal("PRJ1", result.Data.First().Buyer);
        }

        [Fact]
        public async Task GetPagedByProjectAsync_NoMatchingBuyer_ReturnsEmptyList()
        {
            var testorProduct = new TestorProduct { ItemCode = "BLOOD", UnitPriceVla = 10m, DefraUnitPrice = 12m };
            var project = new Project { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "Test", Program = "P1", Customer = "C1", Disease = "D1", Contract = "CT1", IncomeAccountCode = "INC1" };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepositoryWithJoinMocks(
                testReqmts: testReqmts,
                testorProducts: [testorProduct],
                projects: [project]);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedByProjectAsync(query, "MISSING");

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetPagedByProjectAsync_MultipleTestsForSameBuyer_ReturnsAll()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", UnitPriceVla = 10m, DefraUnitPrice = 12m },
                new() { ItemCode = "URINE", UnitPriceVla = 8m,  DefraUnitPrice = 9m  }
            };
            var project = new Project { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "Test", Program = "P1", Customer = "C1", Disease = "D1", Contract = "CT1", IncomeAccountCode = "INC1" };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear },
                new() { TestCode = "URINE", Buyer = "PRJ1", FpsYear = DefaultFpsYear },
                new() { TestCode = "BLOOD", Buyer = "PRJ2", FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepositoryWithJoinMocks(
                testReqmts: testReqmts,
                testorProducts: testorProducts,
                projects: [project]);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedByProjectAsync(query, "PRJ1");

            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, d => Assert.Equal("PRJ1", d.Buyer));
        }

        [Fact]
        public async Task GetPagedByProjectAsync_DefraProject_UsesDefraUnitPrice()
        {
            var testorProduct = new TestorProduct { ItemCode = "BLOOD", UnitPriceVla = 10m, DefraUnitPrice = 20m };
            var project = new Project { ParentProject = "PRJ1", IsDefraProject = 1, ProjectTitle = "Test", Program = "P1", Customer = "C1", Disease = "D1", Contract = "CT1", IncomeAccountCode = "INC1" };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepositoryWithJoinMocks(
                testReqmts: testReqmts,
                testorProducts: [testorProduct],
                projects: [project]);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedByProjectAsync(query, "PRJ1");

            Assert.Single(result.Data);
            Assert.Equal(20m, result.Data.First().RecUnitPrice);
            Assert.Equal((short)1, result.Data.First().IsDefraProject);
        }

        [Fact]
        public async Task GetPagedByProjectAsync_NonDefraProject_UsesVlaUnitPrice()
        {
            var testorProduct = new TestorProduct { ItemCode = "BLOOD", UnitPriceVla = 10m, DefraUnitPrice = 20m };
            var project = new Project { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "Test", Program = "P1", Customer = "C1", Disease = "D1", Contract = "CT1", IncomeAccountCode = "INC1" };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepositoryWithJoinMocks(
                testReqmts: testReqmts,
                testorProducts: [testorProduct],
                projects: [project]);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedByProjectAsync(query, "PRJ1");

            Assert.Single(result.Data);
            Assert.Equal(10m, result.Data.First().RecUnitPrice);
        }

        [Fact]
        public async Task GetPagedByProjectAsync_EmptyRepository_ReturnsEmpty()
        {
            var repo = CreateRepositoryWithJoinMocks();
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedByProjectAsync(query, "PRJ1");

            Assert.Empty(result.Data);
        }

        #endregion

        #region GetPagedByTestCodeAsync

        [Fact]
        public async Task GetPagedByTestCodeAsync_MatchingTestCode_ReturnsMatchingRecords()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear },
                new() { TestCode = "URINE", Buyer = "PRJ2", FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(testReqmts);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedByTestCodeAsync(query, "BLOOD");

            Assert.Single(result.Data);
            Assert.Equal("BLOOD", result.Data.First().TestCode);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_NoMatchingTestCode_ReturnsEmptyList()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(testReqmts);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedByTestCodeAsync(query, "MISSING");

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_WithBuyerFilter_FiltersCorrectly()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "ALPHA", FpsYear = DefaultFpsYear },
                new() { TestCode = "BLOOD", Buyer = "BETA",  FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(testReqmts);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"Buyer\":\"ALP\"}"
            };

            var result = await repo.GetPagedByTestCodeAsync(query, "BLOOD");

            Assert.Single(result.Data);
            Assert.Equal("ALPHA", result.Data.First().Buyer);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_EmptyRepository_ReturnsEmpty()
        {
            var (repo, _, _, _) = CreateRepositoryWithMocks();
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedByTestCodeAsync(query, "BLOOD");

            Assert.Empty(result.Data);
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_MatchingRecord_ReturnsEntity()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(testReqmts);

            var result = await repo.GetByIdAsync("BLOOD", "PRJ1");

            Assert.NotNull(result);
            Assert.Equal("BLOOD", result.TestCode);
            Assert.Equal("PRJ1", result.Buyer);
        }

        [Fact]
        public async Task GetByIdAsync_TestCodeNotFound_ReturnsNull()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(testReqmts);

            var result = await repo.GetByIdAsync("MISSING", "PRJ1");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_BuyerNotFound_ReturnsNull()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(testReqmts);

            var result = await repo.GetByIdAsync("BLOOD", "PRJ_WRONG");

            Assert.Null(result);
        }

        #endregion

        #region ExistsByTestBuyerCodeAsync

        [Fact]
        public async Task ExistsByTestBuyerCodeAsync_CodeExists_ReturnsTrue()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", TestBuyerCode = "BLOOD-WG1", FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(testReqmts);

            var result = await repo.ExistsByTestBuyerCodeAsync("BLOOD-WG1");

            Assert.True(result);
        }

        [Fact]
        public async Task ExistsByTestBuyerCodeAsync_CodeNotExists_ReturnsFalse()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", TestBuyerCode = "BLOOD-WG1", FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(testReqmts);

            var result = await repo.ExistsByTestBuyerCodeAsync("MISSING-CODE");

            Assert.False(result);
        }

        [Fact]
        public async Task ExistsByTestBuyerCodeAsync_EmptyRepository_ReturnsFalse()
        {
            var (repo, _, _, _) = CreateRepositoryWithMocks();

            var result = await repo.ExistsByTestBuyerCodeAsync("ANY-CODE");

            Assert.False(result);
        }

        #endregion

        #region ExistsByTestCodeAndBuyerInMonthlyOutputAsync

        [Fact]
        public async Task ExistsByTestCodeAndBuyerInMonthlyOutputAsync_RecordExists_ReturnsTrue()
        {
            var monthlyOutputs = new List<MonthlyOutput>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", WorkGroup = "WG1", FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(monthlyOutputs: monthlyOutputs);

            var result = await repo.ExistsByTestCodeAndBuyerInMonthlyOutputAsync("BLOOD", "PRJ1");

            Assert.True(result);
        }

        [Fact]
        public async Task ExistsByTestCodeAndBuyerInMonthlyOutputAsync_NoRecord_ReturnsFalse()
        {
            var (repo, _, _, _) = CreateRepositoryWithMocks();

            var result = await repo.ExistsByTestCodeAndBuyerInMonthlyOutputAsync("BLOOD", "PRJ1");

            Assert.False(result);
        }

        [Fact]
        public async Task ExistsByTestCodeAndBuyerInMonthlyOutputAsync_DifferentBuyer_ReturnsFalse()
        {
            var monthlyOutputs = new List<MonthlyOutput>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", WorkGroup = "WG1", FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(monthlyOutputs: monthlyOutputs);

            var result = await repo.ExistsByTestCodeAndBuyerInMonthlyOutputAsync("BLOOD", "PRJ_OTHER");

            Assert.False(result);
        }

        [Fact]
        public async Task ExistsByTestCodeAndBuyerInMonthlyOutputAsync_DifferentTestCode_ReturnsFalse()
        {
            var monthlyOutputs = new List<MonthlyOutput>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", WorkGroup = "WG1", FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(monthlyOutputs: monthlyOutputs);

            var result = await repo.ExistsByTestCodeAndBuyerInMonthlyOutputAsync("URINE", "PRJ1");

            Assert.False(result);
        }

        #endregion

        #region GetPagedByProjectAsync — ItemDescription projection

        [Fact]
        public async Task GetPagedByProjectAsync_ProjectsItemDescription_FromTestorProduct()
        {
            var testorProduct = new TestorProduct
            {
                ItemCode = "BLOOD",
                ItemDescription = "Blood Test Analysis",
                UnitPriceVla = 10m,
                DefraUnitPrice = 12m
            };
            var project = new Project { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "Test", Program = "P1", Customer = "C1", Disease = "D1", Contract = "CT1", IncomeAccountCode = "INC1" };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepositoryWithJoinMocks(testReqmts, [testorProduct], [project]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedByProjectAsync(query, "PRJ1");

            Assert.Single(result.Data);
            Assert.Equal("Blood Test Analysis", result.Data.First().ItemDescription);
        }

        [Fact]
        public async Task GetPagedByProjectAsync_WhenItemDescriptionIsNull_ProjectsNull()
        {
            var testorProduct = new TestorProduct
            {
                ItemCode = "BLOOD",
                ItemDescription = null,
                UnitPriceVla = 10m,
                DefraUnitPrice = 12m
            };
            var project = new Project { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "Test", Program = "P1", Customer = "C1", Disease = "D1", Contract = "CT1", IncomeAccountCode = "INC1" };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepositoryWithJoinMocks(testReqmts, [testorProduct], [project]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedByProjectAsync(query, "PRJ1");

            Assert.Single(result.Data);
            Assert.Null(result.Data.First().ItemDescription);
        }

        [Fact]
        public async Task GetPagedByProjectAsync_MultipleItems_EachHasCorrectItemDescription()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = "Blood Test Analysis", UnitPriceVla = 10m, DefraUnitPrice = 12m },
                new() { ItemCode = "URINE", ItemDescription = "Urine Test Analysis", UnitPriceVla = 8m,  DefraUnitPrice = 9m  }
            };
            var project = new Project { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "Test", Program = "P1", Customer = "C1", Disease = "D1", Contract = "CT1", IncomeAccountCode = "INC1" };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear },
                new() { TestCode = "URINE", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepositoryWithJoinMocks(testReqmts, testorProducts, [project]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedByProjectAsync(query, "PRJ1");

            Assert.Equal(2, result.Data.Count);
            var blood = result.Data.First(d => d.TestCode == "BLOOD");
            var urine = result.Data.First(d => d.TestCode == "URINE");
            Assert.Equal("Blood Test Analysis", blood.ItemDescription);
            Assert.Equal("Urine Test Analysis", urine.ItemDescription);
        }

        [Fact]
        public async Task GetPagedByProjectAsync_ItemDescription_IndependentOfUnitPriceLogic()
        {
            // Defra project uses DefraUnitPrice but description still comes from TestorProduct
            var testorProduct = new TestorProduct
            {
                ItemCode = "BLOOD",
                ItemDescription = "Blood Test Analysis",
                UnitPriceVla = 10m,
                DefraUnitPrice = 20m
            };
            var project = new Project { ParentProject = "PRJ1", IsDefraProject = 1, ProjectTitle = "Test", Program = "P1", Customer = "C1", Disease = "D1", Contract = "CT1", IncomeAccountCode = "INC1" };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepositoryWithJoinMocks(testReqmts, [testorProduct], [project]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedByProjectAsync(query, "PRJ1");

            Assert.Single(result.Data);
            Assert.Equal("Blood Test Analysis", result.Data.First().ItemDescription);
            Assert.Equal(20m,                   result.Data.First().RecUnitPrice);
        }

        #endregion

        #region AddAsync

        [Fact]
        public async Task AddAsync_SetsEntityFpsYearFromContext()
        {
            var (repo, _, _, _) = CreateRepositoryWithMocks(fpsYear: 2025);
            var entity = new TestRequirement { TestCode = "BLOOD", Buyer = "PRJ1" };

            var result = await repo.AddAsync(entity);

            Assert.Equal(2025, result.FpsYear);
        }

        [Fact]
        public async Task AddAsync_SetsDateCreated()
        {
            var (repo, _, _, _) = CreateRepositoryWithMocks();
            var entity = new TestRequirement { TestCode = "BLOOD", Buyer = "PRJ1" };

            await repo.AddAsync(entity);

            Assert.NotNull(entity.DateCreated);
        }

        [Fact]
        public async Task AddAsync_CallsSaveChanges()
        {
            var (repo, _, _, mockContext) = CreateRepositoryWithMocks();
            var entity = new TestRequirement { TestCode = "BLOOD", Buyer = "PRJ1" };

            await repo.AddAsync(entity);

            mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_RecordExistsWithCorrectYear_RemovesAndReturnsTrue()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };
            var (repo, dbSetMock, _, _) = CreateRepositoryWithMocks(testReqmts);

            var result = await repo.DeleteAsync("BLOOD", "PRJ1");

            Assert.True(result);
            dbSetMock.Verify(x => x.Remove(It.IsAny<TestRequirement>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_RecordNotFound_ReturnsFalse()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };
            var (repo, dbSetMock, _, _) = CreateRepositoryWithMocks(testReqmts);

            var result = await repo.DeleteAsync("MISSING", "PRJ1");

            Assert.False(result);
            dbSetMock.Verify(x => x.Remove(It.IsAny<TestRequirement>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_RecordExistsButWrongFpsYear_ReturnsFalse()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = 2023 }
            };
            var (repo, dbSetMock, _, _) = CreateRepositoryWithMocks(testReqmts, fpsYear: DefaultFpsYear);

            var result = await repo.DeleteAsync("BLOOD", "PRJ1");

            Assert.False(result);
            dbSetMock.Verify(x => x.Remove(It.IsAny<TestRequirement>()), Times.Never);
        }

        #endregion
    }
}
