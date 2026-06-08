using Apha.Common.Helpers.Repository;
using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.Core.Pagination;
using Apha.Costbook.DataAccess.Data;
using Apha.Costbook.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Apha.Costbook.DataAccess.UnitTests.Repository.TestRequirementRepositoryTest;

public class TestRequirementRepositoryTests
{
    private const int DefaultFpsYear = 2025;

    /// <summary>
    /// Creates a TestRequirementRepository with in-memory DbSets.
    /// Methods using multi-table JOINs (GetTestRequirementsByProjectYearAsync) and
    /// ExecuteDeleteAsync (DeleteTestRequirementAsync) are covered by integration tests.
    /// </summary>
    private static (
        TestRequirementRepository Repo,
        Mock<DbSet<TestRequirement>> TestRequirementsDbSet,
        Mock<CostbookDbContext> Context)
        CreateRepository(
            IEnumerable<TestRequirement>? testRequirements = null,
            IEnumerable<FpsTestOrProduct>? fpsTestorProducts = null,
            IEnumerable<Project>? projects = null,
            int fpsYear = DefaultFpsYear)
    {
        var mockFpsYearContext = new Mock<IFPSYearContext>();
        mockFpsYearContext.Setup(x => x.FPSYear).Returns(fpsYear);

        var mockContext = RepositoryTestHelper.CreateMockDbContext<CostbookDbContext>(mockFpsYearContext.Object);

        var testReqMockSet = RepositoryTestHelper.CreateMockDbSet(testRequirements ?? []);
        RepositoryTestHelper.SetupDbSetOperations(testReqMockSet);
        mockContext.Setup(x => x.TestRequirements).Returns(testReqMockSet.Object);

        var testProductsMockSet = RepositoryTestHelper.CreateMockDbSet(fpsTestorProducts ?? []);
        mockContext.Setup(x => x.FpsTestorProducts).Returns(testProductsMockSet.Object);

        var projectsMockSet = RepositoryTestHelper.CreateMockDbSet(projects ?? []);
        mockContext.Setup(x => x.Projects).Returns(projectsMockSet.Object);

        RepositoryTestHelper.SetupSaveChanges(mockContext);

        var settingsRepo = new Mock<ISettingsRepository>();
        settingsRepo.Setup(x => x.GetSettingValueByIdAsync("CurrentYear"))
            .ReturnsAsync(fpsYear.ToString());

        var projectRepo = new Mock<IProjectRepository>();
        projectRepo.Setup(x => x.GetInflationFactorAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(1.0);

        var repo = new TestRequirementRepository(mockContext.Object, mockFpsYearContext.Object, settingsRepo.Object, projectRepo.Object);
        return (repo, testReqMockSet, mockContext);
    }

    #region AddTestRequirementAsync

    [Fact]
    public async Task AddTestRequirementAsync_AddsEntity_AndCallsSaveChanges()
    {
        // Arrange
        var (repo, testReqDbSet, mockContext) = CreateRepository();
        var newReq = new TestRequirement
        {
            Project = "2024/001",
            Year = 2024,
            TestCode = "TC001",
            UnitPrice = 100.0,
            NumberOfTests = 5.0
        };

        // Act
        var result = await repo.AddTestRequirementAsync(newReq);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("2024/001", result.Project);
        Assert.Equal("TC001", result.TestCode);
        Assert.Equal(100.0, result.UnitPrice);
        Assert.Equal(5.0, result.NumberOfTests);
        testReqDbSet.Verify(x => x.Add(It.IsAny<TestRequirement>()), Times.Once);
        RepositoryTestHelper.VerifySaveChanges(mockContext);
    }

    [Fact]
    public async Task AddTestRequirementAsync_DecodesUrlEncodedProject()
    {
        // Arrange
        var (repo, _, _) = CreateRepository();
        var newReq = new TestRequirement
        {
            Project = "2024%2F001",
            Year = 2024,
            TestCode = "TC001"
        };

        // Act
        var result = await repo.AddTestRequirementAsync(newReq);

        // Assert
        Assert.Equal("2024/001", result.Project);
    }

    [Fact]
    public async Task AddTestRequirementAsync_ReturnsSameEntityReference()
    {
        // Arrange
        var (repo, _, _) = CreateRepository();
        var newReq = new TestRequirement
        {
            Project = "2024/001",
            Year = 2024,
            TestCode = "TC002"
        };

        // Act
        var result = await repo.AddTestRequirementAsync(newReq);

        // Assert
        Assert.Same(newReq, result);
    }

    #endregion

    #region UpdateTestRequirementAsync

    [Fact]
    public async Task UpdateTestRequirementAsync_UpdatesEntity_AndCallsSaveChanges()
    {
        // Arrange
        var existing = new TestRequirement
        {
            Project = "2024/001",
            Year = 2024,
            TestCode = "TC001",
            UnitPrice = 100.0,
            NumberOfTests = 5.0
        };
        var (repo, testReqDbSet, mockContext) = CreateRepository(testRequirements: [existing]);

        existing.NumberOfTests = 10.0;

        // Act
        var result = await repo.UpdateTestRequirementAsync(existing);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10.0, result.NumberOfTests);
        testReqDbSet.Verify(x => x.Update(It.IsAny<TestRequirement>()), Times.Once);
        RepositoryTestHelper.VerifySaveChanges(mockContext);
    }

    [Fact]
    public async Task UpdateTestRequirementAsync_DecodesUrlEncodedProject()
    {
        // Arrange
        var (repo, _, _) = CreateRepository();
        var entity = new TestRequirement
        {
            Project = "2024%2F001",
            Year = 2024,
            TestCode = "TC001"
        };

        // Act
        var result = await repo.UpdateTestRequirementAsync(entity);

        // Assert
        Assert.Equal("2024/001", result.Project);
    }

    #endregion

    #region GetTestCodeLookupsAsync

    [Fact]
    public async Task GetTestCodeLookupsAsync_ReturnsEmptyList_WhenNoProducts()
    {
        // Arrange
        var (repo, _, _) = CreateRepository(fpsTestorProducts: []);

        // Act
        var result = await repo.GetTestCodeLookupsAsync("2024/001", 2024, isDefra: false);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetTestCodeLookupsAsync_FiltersOutItemCodesStartingWithPA()
    {
        // Arrange
        var products = new List<FpsTestOrProduct>
        {
            new() { ItemCode = "PA001", ItemDescription = "Should be excluded", UnitPriceVla = 10m, DefraUnitPrice = 15m, FpsYear = DefaultFpsYear },
            new() { ItemCode = "TC001", ItemDescription = "Valid test", UnitPriceVla = 20m, DefraUnitPrice = 25m, FpsYear = DefaultFpsYear }
        };
        var (repo, _, _) = CreateRepository(fpsTestorProducts: products);

        // Act
        var result = (await repo.GetTestCodeLookupsAsync("2024/001", 2024, isDefra: false)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("TC001", result[0].ItemCode);
    }

    [Fact]
    public async Task GetTestCodeLookupsAsync_FiltersOutItemCodesEndingWithND()
    {
        // Arrange
        var products = new List<FpsTestOrProduct>
        {
            new() { ItemCode = "TC001ND", ItemDescription = "Should be excluded", UnitPriceVla = 10m, DefraUnitPrice = 15m, FpsYear = DefaultFpsYear },
            new() { ItemCode = "TC002", ItemDescription = "Valid test", UnitPriceVla = 20m, DefraUnitPrice = 25m, FpsYear = DefaultFpsYear }
        };
        var (repo, _, _) = CreateRepository(fpsTestorProducts: products);

        // Act
        var result = (await repo.GetTestCodeLookupsAsync("2024/001", 2024, isDefra: false)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("TC002", result[0].ItemCode);
    }

    [Fact]
    public async Task GetTestCodeLookupsAsync_ReturnsUnitPriceVla_WhenNotDefra()
    {
        // Arrange
        var products = new List<FpsTestOrProduct>
        {
            new() { ItemCode = "TC001", ItemDescription = "Test", UnitPriceVla = 100m, DefraUnitPrice = 200m, FpsYear = DefaultFpsYear }
        };
        var (repo, _, _) = CreateRepository(fpsTestorProducts: products);

        // Act
        var result = (await repo.GetTestCodeLookupsAsync("2024/001", 2024, isDefra: false)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(100m, result[0].UnitPrice);
    }

    [Fact]
    public async Task GetTestCodeLookupsAsync_ReturnsDefraUnitPrice_WhenIsDefra()
    {
        // Arrange
        var products = new List<FpsTestOrProduct>
        {
            new() { ItemCode = "TC001", ItemDescription = "Test", UnitPriceVla = 100m, DefraUnitPrice = 200m, FpsYear = DefaultFpsYear }
        };
        var (repo, _, _) = CreateRepository(fpsTestorProducts: products);

        // Act
        var result = (await repo.GetTestCodeLookupsAsync("2024/001", 2024, isDefra: true)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(200m, result[0].UnitPrice);
    }

    [Fact]
    public async Task GetTestCodeLookupsAsync_ReturnsResultsOrderedByItemCode()
    {
        // Arrange
        var products = new List<FpsTestOrProduct>
        {
            new() { ItemCode = "ZZ001", ItemDescription = "Last", UnitPriceVla = 10m, DefraUnitPrice = 10m, FpsYear = DefaultFpsYear },
            new() { ItemCode = "AA001", ItemDescription = "First", UnitPriceVla = 20m, DefraUnitPrice = 20m, FpsYear = DefaultFpsYear },
            new() { ItemCode = "MM001", ItemDescription = "Middle", UnitPriceVla = 30m, DefraUnitPrice = 30m, FpsYear = DefaultFpsYear }
        };
        var (repo, _, _) = CreateRepository(fpsTestorProducts: products);

        // Act
        var result = (await repo.GetTestCodeLookupsAsync("2024/001", 2024, isDefra: false)).ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("AA001", result[0].ItemCode);
        Assert.Equal("MM001", result[1].ItemCode);
        Assert.Equal("ZZ001", result[2].ItemCode);
    }

    [Fact]
    public async Task GetTestCodeLookupsAsync_MapsItemDescription()
    {
        // Arrange
        var products = new List<FpsTestOrProduct>
        {
            new() { ItemCode = "TC001", ItemDescription = "Blood Test", UnitPriceVla = 100m, DefraUnitPrice = 200m, FpsYear = DefaultFpsYear }
        };
        var (repo, _, _) = CreateRepository(fpsTestorProducts: products);

        // Act
        var result = (await repo.GetTestCodeLookupsAsync("2024/001", 2024, isDefra: false)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("TC001", result[0].ItemCode);
        Assert.Equal("Blood Test", result[0].ItemDescription);
    }

    [Fact]
    public async Task GetTestCodeLookupsAsync_FiltersOutBothPAAndND()
    {
        // Arrange
        var products = new List<FpsTestOrProduct>
        {
            new() { ItemCode = "PA001", ItemDescription = "Excluded PA", UnitPriceVla = 10m, DefraUnitPrice = 10m, FpsYear = DefaultFpsYear },
            new() { ItemCode = "TC001ND", ItemDescription = "Excluded ND", UnitPriceVla = 20m, DefraUnitPrice = 20m, FpsYear = DefaultFpsYear },
            new() { ItemCode = "PA002ND", ItemDescription = "Excluded both", UnitPriceVla = 30m, DefraUnitPrice = 30m, FpsYear = DefaultFpsYear },
            new() { ItemCode = "TC002", ItemDescription = "Valid", UnitPriceVla = 40m, DefraUnitPrice = 40m, FpsYear = DefaultFpsYear }
        };
        var (repo, _, _) = CreateRepository(fpsTestorProducts: products);

        // Act
        var result = (await repo.GetTestCodeLookupsAsync("2024/001", 2024, isDefra: false)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("TC002", result[0].ItemCode);
    }

    #endregion

    #region GetTestRequirementsByProjectYearAsync

    [Fact]
    public async Task GetTestRequirementsByProjectYearAsync_ReturnsEmptyList_WhenNoRequirements()
    {
        // Arrange
        var (repo, _, _) = CreateRepository();
        var query = new PaginationParameters<string>();

        // Act
        var result = await repo.GetTestRequirementsByProjectYearAsync("2024/001", 2024, query);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task GetTestRequirementsByProjectYearAsync_FiltersByProjectAndYear()
    {
        // Arrange
        var reqs = new List<TestRequirement>
        {
            new() { Project = "2024/001", Year = 2024, TestCode = "TC001", UnitPrice = 100, NumberOfTests = 5 },
            new() { Project = "2024/002", Year = 2024, TestCode = "TC002", UnitPrice = 200, NumberOfTests = 3 },
            new() { Project = "2024/001", Year = 2025, TestCode = "TC003", UnitPrice = 50, NumberOfTests = 1 }
        };
        var (repo, _, _) = CreateRepository(testRequirements: reqs);
        var query = new PaginationParameters<string>();

        // Act
        var result = (await repo.GetTestRequirementsByProjectYearAsync("2024/001", 2024, query)).Data.ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("TC001", result[0].TestCode);
    }

    [Fact]
    public async Task GetTestRequirementsByProjectYearAsync_CalculatesTestCost()
    {
        // Arrange
        var reqs = new List<TestRequirement>
        {
            new() { Project = "2024/001", Year = 2024, TestCode = "TC001", UnitPrice = 100, NumberOfTests = 5 }
        };
        var (repo, _, _) = CreateRepository(testRequirements: reqs);
        var query = new PaginationParameters<string>();

        // Act
        var result = (await repo.GetTestRequirementsByProjectYearAsync("2024/001", 2024, query)).Data.ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(500.0, result[0].TestCost); // 100 * 5
    }

    [Fact]
    public async Task GetTestRequirementsByProjectYearAsync_JoinsTestDescription_WhenProductExists()
    {
        // Arrange
        var reqs = new List<TestRequirement>
        {
            new() { Project = "2024/001", Year = 2024, TestCode = "TC001", UnitPrice = 100, NumberOfTests = 5 }
        };
        var products = new List<FpsTestOrProduct>
        {
            new() { ItemCode = "TC001", ItemDescription = "Blood Test", FpsYear = DefaultFpsYear, UnitPriceVla = 100m, DefraUnitPrice = 150m }
        };
        var (repo, _, _) = CreateRepository(testRequirements: reqs, fpsTestorProducts: products);
        var query = new PaginationParameters<string>();

        // Act
        var result = (await repo.GetTestRequirementsByProjectYearAsync("2024/001", 2024, query)).Data.ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Blood Test", result[0].TestDescription);
    }

    [Fact]
    public async Task GetTestRequirementsByProjectYearAsync_ReturnsNullDescription_WhenNoMatchingProduct()
    {
        // Arrange
        var reqs = new List<TestRequirement>
        {
            new() { Project = "2024/001", Year = 2024, TestCode = "TC001", UnitPrice = 100, NumberOfTests = 5 }
        };
        var (repo, _, _) = CreateRepository(testRequirements: reqs, fpsTestorProducts: []);
        var query = new PaginationParameters<string>();

        // Act
        var result = (await repo.GetTestRequirementsByProjectYearAsync("2024/001", 2024, query)).Data.ToList();

        // Assert
        Assert.Single(result);
        Assert.Null(result[0].TestDescription);
    }

    [Fact]
    public async Task GetTestRequirementsByProjectYearAsync_JoinsProjectData()
    {
        // Arrange
        var reqs = new List<TestRequirement>
        {
            new() { Project = "2024/001", Year = 2024, TestCode = "TC001", UnitPrice = 100, NumberOfTests = 5 }
        };
        var projects = new List<Project>
        {
            new() { ProjectId = "2024/001", Programme = "Programme Z", Euroconvrate = 1.10 }
        };
        var (repo, _, _) = CreateRepository(testRequirements: reqs, projects: projects);
        var query = new PaginationParameters<string>();

        // Act
        var result = (await repo.GetTestRequirementsByProjectYearAsync("2024/001", 2024, query)).Data.ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Programme Z", result[0].Programme);
        Assert.Equal(1.10, result[0].EuroConvRate);
    }

    [Fact]
    public async Task GetTestRequirementsByProjectYearAsync_ReturnsNullProjectFields_WhenNoMatchingProject()
    {
        // Arrange
        var reqs = new List<TestRequirement>
        {
            new() { Project = "2024/001", Year = 2024, TestCode = "TC001", UnitPrice = 100, NumberOfTests = 5 }
        };
        var (repo, _, _) = CreateRepository(testRequirements: reqs, projects: []);
        var query = new PaginationParameters<string>();

        // Act
        var result = (await repo.GetTestRequirementsByProjectYearAsync("2024/001", 2024, query)).Data.ToList();

        // Assert
        Assert.Single(result);
        Assert.Null(result[0].Programme);
        Assert.Null(result[0].EuroConvRate);
    }

    [Fact]
    public async Task GetTestRequirementsByProjectYearAsync_ReturnsOrderedByTestCode()
    {
        // Arrange
        var reqs = new List<TestRequirement>
        {
            new() { Project = "2024/001", Year = 2024, TestCode = "ZZ001", UnitPrice = 10, NumberOfTests = 1 },
            new() { Project = "2024/001", Year = 2024, TestCode = "AA001", UnitPrice = 20, NumberOfTests = 2 },
            new() { Project = "2024/001", Year = 2024, TestCode = "MM001", UnitPrice = 30, NumberOfTests = 3 }
        };
        var (repo, _, _) = CreateRepository(testRequirements: reqs);
        var query = new PaginationParameters<string>();

        // Act
        var result = (await repo.GetTestRequirementsByProjectYearAsync("2024/001", 2024, query)).Data.ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("AA001", result[0].TestCode);
        Assert.Equal("MM001", result[1].TestCode);
        Assert.Equal("ZZ001", result[2].TestCode);
    }

    [Fact]
    public async Task GetTestRequirementsByProjectYearAsync_DecodesUrlEncodedProject()
    {
        // Arrange
        var reqs = new List<TestRequirement>
        {
            new() { Project = "2024/001", Year = 2024, TestCode = "TC001", UnitPrice = 100, NumberOfTests = 1 }
        };
        var (repo, _, _) = CreateRepository(testRequirements: reqs);
        var query = new PaginationParameters<string>();

        // Act
        var result = (await repo.GetTestRequirementsByProjectYearAsync("2024%2F001", 2024, query)).Data.ToList();

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task GetTestRequirementsByProjectYearAsync_MapsAllFields()
    {
        // Arrange
        var reqs = new List<TestRequirement>
        {
            new() { Project = "2024/001", Year = 2024, TestCode = "TC001", UnitPrice = 75.5, NumberOfTests = 4 }
        };
        var (repo, _, _) = CreateRepository(testRequirements: reqs);
        var query = new PaginationParameters<string>();

        // Act
        var result = (await repo.GetTestRequirementsByProjectYearAsync("2024/001", 2024, query)).Data.ToList();

        // Assert
        var item = Assert.Single(result);
        Assert.Equal("2024/001", item.Project);
        Assert.Equal(2024, item.Year);
        Assert.Equal("TC001", item.TestCode);
        Assert.Equal(75.5, item.UnitPrice);
        Assert.Equal(4, item.NumberOfTests);
        Assert.Equal(302.0, item.TestCost); // 75.5 * 4
    }

    #endregion

    #region GetTestRequirementsByProjectYearAsync - additional cases

    [Fact]
    public async Task GetTestRequirementsByProjectYearAsync_TestCostIsNull_WhenUnitPriceIsNull()
    {
        var reqs = new List<TestRequirement>
        {
            new() { Project = "2024/001", Year = 2024, TestCode = "TC001", UnitPrice = null, NumberOfTests = 5 }
        };
        var (repo, _, _) = CreateRepository(testRequirements: reqs);
        var query = new PaginationParameters<string>();

        var result = (await repo.GetTestRequirementsByProjectYearAsync("2024/001", 2024, query)).Data.ToList();

        Assert.Single(result);
        Assert.Null(result[0].TestCost);
    }

    [Fact]
    public async Task GetTestRequirementsByProjectYearAsync_TestCostIsNull_WhenNumberOfTestsIsNull()
    {
        var reqs = new List<TestRequirement>
        {
            new() { Project = "2024/001", Year = 2024, TestCode = "TC001", UnitPrice = 100, NumberOfTests = null }
        };
        var (repo, _, _) = CreateRepository(testRequirements: reqs);
        var query = new PaginationParameters<string>();

        var result = (await repo.GetTestRequirementsByProjectYearAsync("2024/001", 2024, query)).Data.ToList();

        Assert.Single(result);
        Assert.Null(result[0].TestCost);
    }

    [Fact]
    public async Task GetTestRequirementsByProjectYearAsync_ReturnsMultipleRecords_WhenMultipleExist()
    {
        var reqs = new List<TestRequirement>
        {
            new() { Project = "2024/001", Year = 2024, TestCode = "AA001", UnitPrice = 10, NumberOfTests = 1 },
            new() { Project = "2024/001", Year = 2024, TestCode = "BB001", UnitPrice = 20, NumberOfTests = 2 },
            new() { Project = "2024/001", Year = 2024, TestCode = "CC001", UnitPrice = 30, NumberOfTests = 3 }
        };
        var (repo, _, _) = CreateRepository(testRequirements: reqs);
        var query = new PaginationParameters<string>();

        var result = (await repo.GetTestRequirementsByProjectYearAsync("2024/001", 2024, query)).Data.ToList();

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetTestRequirementsByProjectYearAsync_ReturnsNullDescription_WhenProductFpsYearDoesNotMatch()
    {
        var reqs = new List<TestRequirement>
        {
            new() { Project = "2024/001", Year = 2024, TestCode = "TC001", UnitPrice = 100, NumberOfTests = 5 }
        };
        var products = new List<FpsTestOrProduct>
        {
            new() { ItemCode = "TC001", ItemDescription = "Old Year Product", FpsYear = 9999, UnitPriceVla = 100m, DefraUnitPrice = 150m }
        };
        var (repo, _, _) = CreateRepository(testRequirements: reqs, fpsTestorProducts: products);
        var query = new PaginationParameters<string>();

        var result = (await repo.GetTestRequirementsByProjectYearAsync("2024/001", 2024, query)).Data.ToList();

        Assert.Single(result);
        Assert.Null(result[0].TestDescription);
    }

    [Fact]
    public async Task GetTestRequirementsByProjectYearAsync_MapsProjectProgrammeAndEuroConvRate()
    {
        var reqs = new List<TestRequirement>
        {
            new() { Project = "2024/001", Year = 2024, TestCode = "TC001", UnitPrice = 50, NumberOfTests = 2 }
        };
        var projects = new List<Project>
        {
            new() { ProjectId = "2024/001", Programme = "PRG-A", Euroconvrate = 1.25 }
        };
        var (repo, _, _) = CreateRepository(testRequirements: reqs, projects: projects);
        var query = new PaginationParameters<string>();

        var result = (await repo.GetTestRequirementsByProjectYearAsync("2024/001", 2024, query)).Data.ToList();

        Assert.Single(result);
        Assert.Equal("PRG-A", result[0].Programme);
        Assert.Equal(1.25, result[0].EuroConvRate);
    }

    #endregion

    #region UpdateTestRequirementAsync - additional cases

    [Fact]
    public async Task UpdateTestRequirementAsync_ReturnsSameEntityReference()
    {
        var (repo, _, _) = CreateRepository();
        var entity = new TestRequirement
        {
            Project = "2024/001",
            Year = 2024,
            TestCode = "TC001"
        };

        var result = await repo.UpdateTestRequirementAsync(entity);

        Assert.Same(entity, result);
    }

    #endregion

    #region AddTestRequirementAsync - additional cases

    [Fact]
    public async Task AddTestRequirementAsync_WithNullOptionalFields_Succeeds()
    {
        var (repo, _, _) = CreateRepository();
        var newReq = new TestRequirement
        {
            Project = "2024/001",
            Year = 2024,
            TestCode = "TC001",
            UnitPrice = null,
            NumberOfTests = null
        };

        var result = await repo.AddTestRequirementAsync(newReq);

        Assert.NotNull(result);
        Assert.Null(result.UnitPrice);
        Assert.Null(result.NumberOfTests);
    }

    #endregion

    #region GetTestCodeLookupsAsync - additional cases

    [Fact]
    public async Task GetTestCodeLookupsAsync_ReturnsNullUnitPrice_WhenUnitPriceVlaIsNull()
    {
        var products = new List<FpsTestOrProduct>
        {
            new() { ItemCode = "TC001", ItemDescription = "Test", UnitPriceVla = null, DefraUnitPrice = 0m, FpsYear = DefaultFpsYear }
        };
        var (repo, _, _) = CreateRepository(fpsTestorProducts: products);

        var result = (await repo.GetTestCodeLookupsAsync("2024/001", 2024, isDefra: false)).ToList();

        Assert.Single(result);
        Assert.Null(result[0].UnitPrice);
    }

    [Fact]
    public async Task GetTestCodeLookupsAsync_ReturnsMultipleRecords_WithCorrectPriceSelection()
    {
        var products = new List<FpsTestOrProduct>
        {
            new() { ItemCode = "AA001", ItemDescription = "First", UnitPriceVla = 10m, DefraUnitPrice = 15m, FpsYear = DefaultFpsYear },
            new() { ItemCode = "BB001", ItemDescription = "Second", UnitPriceVla = 20m, DefraUnitPrice = 25m, FpsYear = DefaultFpsYear }
        };
        var (repo, _, _) = CreateRepository(fpsTestorProducts: products);

        var result = (await repo.GetTestCodeLookupsAsync("2024/001", 2024, isDefra: true)).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal(15m, result[0].UnitPrice);
        Assert.Equal(25m, result[1].UnitPrice);
    }

    #endregion
}
