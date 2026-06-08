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

        #region GetPagedBySupplierTestCodeAsync

        private static TestRequirementRepository CreateRepositoryWithSupplierMocks(
            IEnumerable<TestRequirement>? testReqmts = null,
            IEnumerable<Project>? projects = null,
            int fpsYear = DefaultFpsYear)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(fpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);

            var testReqmtsMockSet = RepositoryTestHelper.CreateMockDbSet(testReqmts ?? []);
            RepositoryTestHelper.SetupDbSetOperations(testReqmtsMockSet);

            var projectsMockSet = RepositoryTestHelper.CreateMockDbSet(projects ?? []);
            RepositoryTestHelper.SetupDbSetOperations(projectsMockSet);

            var monthlyOutputsMockSet = RepositoryTestHelper.CreateMockDbSet(new List<MonthlyOutput>());
            var testReqLogsMockSet = RepositoryTestHelper.CreateMockDbSet(new List<TestRequirementLog>());

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.TestRequirements).Returns(testReqmtsMockSet.Object);
            mockContext.Setup(x => x.Projects).Returns(projectsMockSet.Object);
            mockContext.Setup(x => x.MonthlyOutputs).Returns(monthlyOutputsMockSet.Object);
            mockContext.Setup(x => x.TestRequirementLogs).Returns(testReqLogsMockSet.Object);

            return new TestRequirementRepository(mockContext.Object, fpsRequestContext);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCodeAsync_MatchingTestCode_ReturnsMatchingRows()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", Active = 1, UnitPrice = 10m, NoRequired = 3 },
                new() { TestCode = "URINE", Buyer = "PRJ2", Active = 1, UnitPrice = 5m,  NoRequired = 2 }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", Manager = "MGR1", ProjectStatus = "Active" },
                new() { ParentProject = "PRJ2", Manager = "MGR2", ProjectStatus = "Active" }
            };
            var repo = CreateRepositoryWithSupplierMocks(testReqmts, projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedBySupplierTestCodeAsync(query, "BLOOD", showRejected: false);

            Assert.Single(result.Data);
            Assert.Equal("BLOOD", result.Data.First().TestCode);
            Assert.Equal("PRJ1",  result.Data.First().Buyer);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCodeAsync_NoMatchingTestCode_ReturnsEmpty()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", Active = 1 }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", Manager = "MGR1", ProjectStatus = "Active" }
            };
            var repo = CreateRepositoryWithSupplierMocks(testReqmts, projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedBySupplierTestCodeAsync(query, "MISSING", showRejected: false);

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCodeAsync_ShowRejectedFalse_ExcludesInactiveRows()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", Active = 1 },
                new() { TestCode = "BLOOD", Buyer = "PRJ2", Active = 0 }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", Manager = "MGR1", ProjectStatus = "Active" },
                new() { ParentProject = "PRJ2", Manager = "MGR2", ProjectStatus = "Rejected" }
            };
            var repo = CreateRepositoryWithSupplierMocks(testReqmts, projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedBySupplierTestCodeAsync(query, "BLOOD", showRejected: false);

            Assert.Single(result.Data);
            Assert.Equal("PRJ1", result.Data.First().Buyer);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCodeAsync_ShowRejectedTrue_IncludesInactiveRows()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", Active = 1 },
                new() { TestCode = "BLOOD", Buyer = "PRJ2", Active = 0 }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", Manager = "MGR1", ProjectStatus = "Active" },
                new() { ParentProject = "PRJ2", Manager = "MGR2", ProjectStatus = "Rejected" }
            };
            var repo = CreateRepositoryWithSupplierMocks(testReqmts, projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedBySupplierTestCodeAsync(query, "BLOOD", showRejected: true);

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCodeAsync_ProjectsManagerFromProject()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", Active = 1 }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", Manager = "John Smith", ProjectStatus = "Active" }
            };
            var repo = CreateRepositoryWithSupplierMocks(testReqmts, projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedBySupplierTestCodeAsync(query, "BLOOD", showRejected: false);

            Assert.Single(result.Data);
            Assert.Equal("John Smith", result.Data.First().ProjectManager);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCodeAsync_ProjectsProjectStatusFromProject()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", Active = 1 }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", Manager = "MGR1", ProjectStatus = "Closed" }
            };
            var repo = CreateRepositoryWithSupplierMocks(testReqmts, projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedBySupplierTestCodeAsync(query, "BLOOD", showRejected: false);

            Assert.Single(result.Data);
            Assert.Equal("Closed", result.Data.First().ProjectStatus);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCodeAsync_TestCostComputedClientSide()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", Active = 1, UnitPrice = 10m, NoRequired = 3 }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", Manager = "MGR1", ProjectStatus = "Active" }
            };
            var repo = CreateRepositoryWithSupplierMocks(testReqmts, projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedBySupplierTestCodeAsync(query, "BLOOD", showRejected: false);

            Assert.Single(result.Data);
            Assert.Equal(30m, result.Data.First().TestCost);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCodeAsync_TestCostNullWhenUnitPriceNull()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", Active = 1, UnitPrice = null, NoRequired = 3 }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", Manager = "MGR1", ProjectStatus = "Active" }
            };
            var repo = CreateRepositoryWithSupplierMocks(testReqmts, projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedBySupplierTestCodeAsync(query, "BLOOD", showRejected: false);

            Assert.Null(result.Data.First().TestCost);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCodeAsync_TestCostNullWhenNoRequiredNull()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", Active = 1, UnitPrice = 10m, NoRequired = null }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", Manager = "MGR1", ProjectStatus = "Active" }
            };
            var repo = CreateRepositoryWithSupplierMocks(testReqmts, projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedBySupplierTestCodeAsync(query, "BLOOD", showRejected: false);

            Assert.Null(result.Data.First().TestCost);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCodeAsync_FilterByBuyer_ReturnsMatchingOnly()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "ALPHA001", Active = 1 },
                new() { TestCode = "BLOOD", Buyer = "BETA002",  Active = 1 }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "ALPHA001", Manager = "MGR1", ProjectStatus = "Active" },
                new() { ParentProject = "BETA002",  Manager = "MGR2", ProjectStatus = "Active" }
            };
            var repo = CreateRepositoryWithSupplierMocks(testReqmts, projects);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"Buyer\":\"ALPHA\"}"
            };

            var result = await repo.GetPagedBySupplierTestCodeAsync(query, "BLOOD", showRejected: false);

            Assert.Single(result.Data);
            Assert.Equal("ALPHA001", result.Data.First().Buyer);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCodeAsync_FilterByProjectStatus_ReturnsMatchingOnly()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", Active = 1 },
                new() { TestCode = "BLOOD", Buyer = "PRJ2", Active = 1 }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", Manager = "MGR1", ProjectStatus = "Active" },
                new() { ParentProject = "PRJ2", Manager = "MGR2", ProjectStatus = "Closed" }
            };
            var repo = CreateRepositoryWithSupplierMocks(testReqmts, projects);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"ProjectStatus\":\"Closed\"}"
            };

            var result = await repo.GetPagedBySupplierTestCodeAsync(query, "BLOOD", showRejected: false);

            Assert.Single(result.Data);
            Assert.Equal("PRJ2", result.Data.First().Buyer);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCodeAsync_NullFilter_ReturnsAll()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", Active = 1 },
                new() { TestCode = "BLOOD", Buyer = "PRJ2", Active = 1 }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", Manager = "MGR1", ProjectStatus = "Active" },
                new() { ParentProject = "PRJ2", Manager = "MGR2", ProjectStatus = "Active" }
            };
            var repo = CreateRepositoryWithSupplierMocks(testReqmts, projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = null };

            var result = await repo.GetPagedBySupplierTestCodeAsync(query, "BLOOD", showRejected: false);

            Assert.Equal(2, result.Data.Count);
        }

        [Theory]
        [InlineData(nameof(TestSupplierView.Buyer), false)]
        [InlineData(nameof(TestSupplierView.Buyer), true)]
        [InlineData(nameof(TestSupplierView.ProjectManager), false)]
        [InlineData(nameof(TestSupplierView.ProjectManager), true)]
        [InlineData(nameof(TestSupplierView.UnitPrice), false)]
        [InlineData(nameof(TestSupplierView.UnitPrice), true)]
        [InlineData(nameof(TestSupplierView.NoRequired), false)]
        [InlineData(nameof(TestSupplierView.NoRequired), true)]
        [InlineData(nameof(TestSupplierView.ProjectStatus), false)]
        [InlineData(nameof(TestSupplierView.ProjectStatus), true)]
        public async Task GetPagedBySupplierTestCodeAsync_DbSortColumns_DoesNotThrow(string sortBy, bool descending)
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", Active = 1, UnitPrice = 10m, NoRequired = 2 },
                new() { TestCode = "BLOOD", Buyer = "PRJ2", Active = 1, UnitPrice = 5m,  NoRequired = 4 }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", Manager = "MGR1", ProjectStatus = "Active" },
                new() { ParentProject = "PRJ2", Manager = "MGR2", ProjectStatus = "Closed" }
            };
            var repo = CreateRepositoryWithSupplierMocks(testReqmts, projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = sortBy, Descending = descending };

            var result = await repo.GetPagedBySupplierTestCodeAsync(query, "BLOOD", showRejected: false);

            Assert.Equal(2, result.Data.Count);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task GetPagedBySupplierTestCodeAsync_SortByTestCost_AppliedClientSide(bool descending)
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", Active = 1, UnitPrice = 5m,  NoRequired = 2 },  // TestCost=10
                new() { TestCode = "BLOOD", Buyer = "PRJ2", Active = 1, UnitPrice = 10m, NoRequired = 3 }   // TestCost=30
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", Manager = "MGR1", ProjectStatus = "Active" },
                new() { ParentProject = "PRJ2", Manager = "MGR2", ProjectStatus = "Active" }
            };
            var repo = CreateRepositoryWithSupplierMocks(testReqmts, projects);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                SortBy = nameof(TestSupplierView.TestCost),
                Descending = descending
            };

            var result = await repo.GetPagedBySupplierTestCodeAsync(query, "BLOOD", showRejected: false);

            Assert.Equal(2, result.Data.Count);
            if (descending)
                Assert.Equal("PRJ2", result.Data.First().Buyer);
            else
                Assert.Equal("PRJ1", result.Data.First().Buyer);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCodeAsync_NoSortBy_DefaultsToOrderByBuyer()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "ZZZ", Active = 1 },
                new() { TestCode = "BLOOD", Buyer = "AAA", Active = 1 }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "ZZZ", Manager = "MGR1", ProjectStatus = "Active" },
                new() { ParentProject = "AAA", Manager = "MGR2", ProjectStatus = "Active" }
            };
            var repo = CreateRepositoryWithSupplierMocks(testReqmts, projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedBySupplierTestCodeAsync(query, "BLOOD", showRejected: false);

            Assert.Equal("AAA", result.Data.First().Buyer);
            Assert.Equal("ZZZ", result.Data.ElementAt(1).Buyer);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCodeAsync_UnknownSortColumn_DefaultsToOrderByBuyer()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "ZZZ", Active = 1 },
                new() { TestCode = "BLOOD", Buyer = "AAA", Active = 1 }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "ZZZ", Manager = "MGR1", ProjectStatus = "Active" },
                new() { ParentProject = "AAA", Manager = "MGR2", ProjectStatus = "Active" }
            };
            var repo = CreateRepositoryWithSupplierMocks(testReqmts, projects);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                SortBy = "UnknownColumn", Descending = false
            };

            var result = await repo.GetPagedBySupplierTestCodeAsync(query, "BLOOD", showRejected: false);

            Assert.Equal("AAA", result.Data.First().Buyer);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCodeAsync_UnknownSortColumnDescending_DefaultsToOrderByBuyerDescending()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "ZZZ", Active = 1 },
                new() { TestCode = "BLOOD", Buyer = "AAA", Active = 1 }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "ZZZ", Manager = "MGR1", ProjectStatus = "Active" },
                new() { ParentProject = "AAA", Manager = "MGR2", ProjectStatus = "Active" }
            };
            var repo = CreateRepositoryWithSupplierMocks(testReqmts, projects);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                SortBy = "UnknownColumn", Descending = true
            };

            var result = await repo.GetPagedBySupplierTestCodeAsync(query, "BLOOD", showRejected: false);

            Assert.Equal("ZZZ", result.Data.First().Buyer);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCodeAsync_Paging_ReturnsCorrectPage()
        {
            var testReqmts = Enumerable.Range(1, 5).Select(i =>
                new TestRequirement { TestCode = "BLOOD", Buyer = $"PRJ{i:D3}", Active = 1 }).ToList();
            var projects = testReqmts.Select(t =>
                new Project { ParentProject = t.Buyer, Manager = "MGR", ProjectStatus = "Active" }).ToList();            var repo = CreateRepositoryWithSupplierMocks(testReqmts, projects);
            var query = new PaginationParameters<string> { Page = 2, PageSize = 2 };

            var result = await repo.GetPagedBySupplierTestCodeAsync(query, "BLOOD", showRejected: false);

            Assert.Equal(2, result.Data.Count);
            Assert.Equal(5, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCodeAsync_EmptyRepository_ReturnsEmpty()
        {
            var repo = CreateRepositoryWithSupplierMocks();
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedBySupplierTestCodeAsync(query, "BLOOD", showRejected: false);

            Assert.Empty(result.Data);
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
