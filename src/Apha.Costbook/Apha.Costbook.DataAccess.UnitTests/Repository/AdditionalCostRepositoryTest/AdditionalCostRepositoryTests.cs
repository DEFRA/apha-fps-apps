using Apha.Common.Helpers.Repository;
using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.DataAccess.Data;
using Apha.Costbook.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Apha.Costbook.DataAccess.UnitTests.Repository.AdditionalCostRepositoryTest;

public class AdditionalCostRepositoryTests
{
    private const int DefaultFpsYear = 2025;
    private static readonly string[] ProjectSpecificAccountCategories = ["TRAVEL", "STAFF"];

    /// <summary>
    /// Creates an AdditionalCostRepository with in-memory DbSets.
    /// Methods using multi-table JOINs (GetAdditionalCostsByProjectYearAsync, GetProjectSpecificAccountCategoriesAsync)
    /// and ExecuteDeleteAsync (DeleteAdditionalCostAsync) are covered by integration tests.
    /// </summary>
    private static (
        AdditionalCostRepository Repo,
        Mock<DbSet<AdditionalCost>> AdditionalCostsDbSet,
        Mock<CostbookDbContext> Context)
        CreateRepository(
            IEnumerable<AdditionalCost>? additionalCosts = null,
            IEnumerable<Project>? projects = null,
            IEnumerable<FpsAccountCategory>? accountCategories = null,
            IEnumerable<AccountGroup>? accountGroups = null)
    {
        var mockFpsYearContext = new Mock<IFPSYearContext>();
        mockFpsYearContext.Setup(x => x.FPSYear).Returns(DefaultFpsYear);

        var mockContext = RepositoryTestHelper.CreateMockDbContext<CostbookDbContext>(mockFpsYearContext.Object);

        var additionalCostMockSet = RepositoryTestHelper.CreateMockDbSet(additionalCosts ?? []);
        RepositoryTestHelper.SetupDbSetOperations(additionalCostMockSet);
        mockContext.Setup(x => x.AdditionalCosts).Returns(additionalCostMockSet.Object);

        var projectsMockSet = RepositoryTestHelper.CreateMockDbSet(projects ?? []);
        mockContext.Setup(x => x.Projects).Returns(projectsMockSet.Object);

        var acMockSet = RepositoryTestHelper.CreateMockDbSet(accountCategories ?? []);
        mockContext.Setup(x => x.FpsAccountCategories).Returns(acMockSet.Object);

        var agMockSet = RepositoryTestHelper.CreateMockDbSet(accountGroups ?? []);
        mockContext.Setup(x => x.AccountGroups).Returns(agMockSet.Object);

        RepositoryTestHelper.SetupSaveChanges(mockContext);

        var repo = new AdditionalCostRepository(mockContext.Object);
        return (repo, additionalCostMockSet, mockContext);
    }

    #region AddAdditionalCostAsync

    [Fact]
    public async Task AddAdditionalCostAsync_AddsEntity_AndCallsSaveChanges()
    {
        // Arrange
        var (repo, additionalCostDbSet, mockContext) = CreateRepository();
        var newCost = new AdditionalCost
        {
            AcIdentity = 1,
            Project = "2024/001",
            Year = 2024,
            AccountCat = "TRAVEL",
            Description = "Travel costs",
            ItemCost = 500.0,
            CostEntered = 500.0,
            Freq = "Annual"
        };

        // Act
        var result = await repo.AddAdditionalCostAsync(newCost);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("2024/001", result.Project);
        Assert.Equal("TRAVEL", result.AccountCat);
        Assert.Equal("Travel costs", result.Description);
        Assert.Equal(500.0, result.CostEntered);
        additionalCostDbSet.Verify(x => x.Add(It.IsAny<AdditionalCost>()), Times.Once);
        RepositoryTestHelper.VerifySaveChanges(mockContext);
    }

    [Fact]
    public async Task AddAdditionalCostAsync_DecodesUrlEncodedProject()
    {
        // Arrange
        var (repo, _, _) = CreateRepository();
        var newCost = new AdditionalCost
        {
            Project = "2024%2F001",
            Year = 2024,
            AccountCat = "TRAVEL",
            Description = "Test"
        };

        // Act
        var result = await repo.AddAdditionalCostAsync(newCost);

        // Assert
        Assert.Equal("2024/001", result.Project);
    }

    [Fact]
    public async Task AddAdditionalCostAsync_ReturnsSameEntityReference()
    {
        // Arrange
        var (repo, _, _) = CreateRepository();
        var newCost = new AdditionalCost
        {
            Project = "2024/001",
            Year = 2024,
            AccountCat = "EQUIP",
            Description = "Equipment"
        };

        // Act
        var result = await repo.AddAdditionalCostAsync(newCost);

        // Assert
        Assert.Same(newCost, result);
    }

    #endregion

    #region UpdateAdditionalCostAsync

    [Fact]
    public async Task UpdateAdditionalCostAsync_UpdatesEntity_AndCallsSaveChanges()
    {
        // Arrange
        var existing = new AdditionalCost
        {
            AcIdentity = 1,
            Project = "2024/001",
            Year = 2024,
            AccountCat = "TRAVEL",
            Description = "Travel costs",
            CostEntered = 500.0
        };
        var (repo, additionalCostDbSet, mockContext) = CreateRepository(additionalCosts: [existing]);

        existing.CostEntered = 750.0;

        // Act
        var result = await repo.UpdateAdditionalCostAsync(existing);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(750.0, result.CostEntered);
        additionalCostDbSet.Verify(x => x.Update(It.IsAny<AdditionalCost>()), Times.Once);
        RepositoryTestHelper.VerifySaveChanges(mockContext);
    }

    [Fact]
    public async Task UpdateAdditionalCostAsync_DecodesUrlEncodedProject()
    {
        // Arrange
        var (repo, _, _) = CreateRepository();
        var entity = new AdditionalCost
        {
            Project = "2024%2F001",
            Year = 2024,
            AccountCat = "TRAVEL",
            Description = "Test"
        };

        // Act
        var result = await repo.UpdateAdditionalCostAsync(entity);

        // Assert
        Assert.Equal("2024/001", result.Project);
    }

    #endregion

    #region GetAdditionalCostsByProjectYearAsync

    [Fact]
    public async Task GetAdditionalCostsByProjectYearAsync_ReturnsEmptyList_WhenNoCosts()
    {
        // Arrange
        var (repo, _, _) = CreateRepository();

        // Act
        var result = await repo.GetAdditionalCostsByProjectYearAsync("2024/001", 2024);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAdditionalCostsByProjectYearAsync_FiltersByProjectAndYear()
    {
        // Arrange
        var costs = new List<AdditionalCost>
        {
            new() { AcIdentity = 1, Project = "2024/001", Year = 2024, AccountCat = "TRAVEL", Description = "Match", CostEntered = 100 },
            new() { AcIdentity = 2, Project = "2024/002", Year = 2024, AccountCat = "EQUIP", Description = "Wrong project", CostEntered = 200 },
            new() { AcIdentity = 3, Project = "2024/001", Year = 2025, AccountCat = "TRAVEL", Description = "Wrong year", CostEntered = 300 }
        };
        var (repo, _, _) = CreateRepository(additionalCosts: costs);

        // Act
        var result = (await repo.GetAdditionalCostsByProjectYearAsync("2024/001", 2024)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(1, result[0].AcIdentity);
        Assert.Equal("Match", result[0].Description);
    }

    [Fact]
    public async Task GetAdditionalCostsByProjectYearAsync_JoinsProjectData_WhenProjectExists()
    {
        // Arrange
        var costs = new List<AdditionalCost>
        {
            new() { AcIdentity = 1, Project = "2024/001", Year = 2024, AccountCat = "TRAVEL", Description = "Test", CostEntered = 500 }
        };
        var projects = new List<Project>
        {
            new() { ProjectId = "2024/001", Programme = "Programme A", Euroconvrate = 1.15 }
        };
        var (repo, _, _) = CreateRepository(additionalCosts: costs, projects: projects);

        // Act
        var result = (await repo.GetAdditionalCostsByProjectYearAsync("2024/001", 2024)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Programme A", result[0].Programme);
        Assert.Equal(1.15, result[0].EuroConvRate);
    }

    [Fact]
    public async Task GetAdditionalCostsByProjectYearAsync_ReturnsNullProjectFields_WhenNoMatchingProject()
    {
        // Arrange
        var costs = new List<AdditionalCost>
        {
            new() { AcIdentity = 1, Project = "2024/001", Year = 2024, AccountCat = "TRAVEL", Description = "Test", CostEntered = 500 }
        };
        var (repo, _, _) = CreateRepository(additionalCosts: costs, projects: []);

        // Act
        var result = (await repo.GetAdditionalCostsByProjectYearAsync("2024/001", 2024)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Null(result[0].Programme);
        Assert.Null(result[0].EuroConvRate);
    }

    [Fact]
    public async Task GetAdditionalCostsByProjectYearAsync_MapsAllFields()
    {
        // Arrange
        var costs = new List<AdditionalCost>
        {
            new() { AcIdentity = 10, Project = "2024/001", Year = 2024, AccountCat = "EQUIP", Description = "Equipment", ItemCost = 250.0, CostEntered = 750.0, Freq = "Monthly" }
        };
        var (repo, _, _) = CreateRepository(additionalCosts: costs);

        // Act
        var result = (await repo.GetAdditionalCostsByProjectYearAsync("2024/001", 2024)).ToList();

        // Assert
        var item = Assert.Single(result);
        Assert.Equal(10, item.AcIdentity);
        Assert.Equal("2024/001", item.Project);
        Assert.Equal(2024, item.Year);
        Assert.Equal("EQUIP", item.AccountCat);
        Assert.Equal("Equipment", item.Description);
        Assert.Equal(250.0, item.ItemCost);
        Assert.Equal(750.0, item.CostEntered);
        Assert.Equal("Monthly", item.Freq);
    }

    [Fact]
    public async Task GetAdditionalCostsByProjectYearAsync_ReturnsOrderedByDescription()
    {
        // Arrange
        var costs = new List<AdditionalCost>
        {
            new() { AcIdentity = 1, Project = "2024/001", Year = 2024, AccountCat = "A", Description = "Zebra", CostEntered = 100 },
            new() { AcIdentity = 2, Project = "2024/001", Year = 2024, AccountCat = "B", Description = "Apple", CostEntered = 200 },
            new() { AcIdentity = 3, Project = "2024/001", Year = 2024, AccountCat = "C", Description = "Mango", CostEntered = 300 }
        };
        var (repo, _, _) = CreateRepository(additionalCosts: costs);

        // Act
        var result = (await repo.GetAdditionalCostsByProjectYearAsync("2024/001", 2024)).ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("Apple", result[0].Description);
        Assert.Equal("Mango", result[1].Description);
        Assert.Equal("Zebra", result[2].Description);
    }

    [Fact]
    public async Task GetAdditionalCostsByProjectYearAsync_DecodesUrlEncodedProject()
    {
        // Arrange
        var costs = new List<AdditionalCost>
        {
            new() { AcIdentity = 1, Project = "2024/001", Year = 2024, AccountCat = "TRAVEL", Description = "Test", CostEntered = 100 }
        };
        var (repo, _, _) = CreateRepository(additionalCosts: costs);

        // Act
        var result = (await repo.GetAdditionalCostsByProjectYearAsync("2024%2F001", 2024)).ToList();

        // Assert
        Assert.Single(result);
    }

    #endregion

    #region GetProjectSpecificAccountCategoriesAsync

    [Fact]
    public async Task GetProjectSpecificAccountCategoriesAsync_ReturnsEmptyList_WhenNoCategories()
    {
        // Arrange
        var (repo, _, _) = CreateRepository();

        // Act
        var result = await repo.GetProjectSpecificAccountCategoriesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetProjectSpecificAccountCategoriesAsync_FiltersOnlyProjectSpecificMinusOne()
    {
        // Arrange
        var categories = new List<FpsAccountCategory>
        {
            new() { AccShortName = "TRAVEL", ProjectSpecific = -1, Csg7Group = "GRP1", AccountType = "A", FpsYear = DefaultFpsYear },
            new() { AccShortName = "EQUIP", ProjectSpecific = 0, Csg7Group = "GRP2", AccountType = "B", FpsYear = DefaultFpsYear },
            new() { AccShortName = "STAFF", ProjectSpecific = -1, Csg7Group = "GRP3", AccountType = "C", FpsYear = DefaultFpsYear }
        };
        var groups = new List<AccountGroup>
        {
            new() { Csg7group = "GRP1", Useinflation = true },
            new() { Csg7group = "GRP2", Useinflation = false },
            new() { Csg7group = "GRP3", Useinflation = false }
        };
        var (repo, _, _) = CreateRepository(accountCategories: categories, accountGroups: groups);

        // Act
        var result = (await repo.GetProjectSpecificAccountCategoriesAsync()).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.Contains(r.AccShortName, ProjectSpecificAccountCategories));
    }

    [Fact]
    public async Task GetProjectSpecificAccountCategoriesAsync_JoinsUseInflationFromAccountGroup()
    {
        // Arrange
        var categories = new List<FpsAccountCategory>
        {
            new() { AccShortName = "TRAVEL", ProjectSpecific = -1, Csg7Group = "GRP1", AccountType = "A", FpsYear = DefaultFpsYear }
        };
        var groups = new List<AccountGroup>
        {
            new() { Csg7group = "GRP1", Useinflation = true }
        };
        var (repo, _, _) = CreateRepository(accountCategories: categories, accountGroups: groups);

        // Act
        var result = (await repo.GetProjectSpecificAccountCategoriesAsync()).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("TRAVEL", result[0].AccShortName);
        Assert.True(result[0].UseInflation);
    }

    [Fact]
    public async Task GetProjectSpecificAccountCategoriesAsync_UseInflationFalse_WhenAccountGroupFalse()
    {
        // Arrange
        var categories = new List<FpsAccountCategory>
        {
            new() { AccShortName = "EQUIP", ProjectSpecific = -1, Csg7Group = "GRP1", AccountType = "A", FpsYear = DefaultFpsYear }
        };
        var groups = new List<AccountGroup>
        {
            new() { Csg7group = "GRP1", Useinflation = false }
        };
        var (repo, _, _) = CreateRepository(accountCategories: categories, accountGroups: groups);

        // Act
        var result = (await repo.GetProjectSpecificAccountCategoriesAsync()).ToList();

        // Assert
        Assert.Single(result);
        Assert.False(result[0].UseInflation);
    }

    [Fact]
    public async Task GetProjectSpecificAccountCategoriesAsync_ReturnsOrderedByAccShortName()
    {
        // Arrange
        var categories = new List<FpsAccountCategory>
        {
            new() { AccShortName = "ZEBRA", ProjectSpecific = -1, Csg7Group = "GRP1", AccountType = "A", FpsYear = DefaultFpsYear },
            new() { AccShortName = "APPLE", ProjectSpecific = -1, Csg7Group = "GRP1", AccountType = "B", FpsYear = DefaultFpsYear },
            new() { AccShortName = "MANGO", ProjectSpecific = -1, Csg7Group = "GRP1", AccountType = "C", FpsYear = DefaultFpsYear }
        };
        var groups = new List<AccountGroup>
        {
            new() { Csg7group = "GRP1", Useinflation = false }
        };
        var (repo, _, _) = CreateRepository(accountCategories: categories, accountGroups: groups);

        // Act
        var result = (await repo.GetProjectSpecificAccountCategoriesAsync()).ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("APPLE", result[0].AccShortName);
        Assert.Equal("MANGO", result[1].AccShortName);
        Assert.Equal("ZEBRA", result[2].AccShortName);
    }

    [Fact]
    public async Task GetProjectSpecificAccountCategoriesAsync_ExcludesCategories_WithNoMatchingAccountGroup()
    {
        // Arrange
        var categories = new List<FpsAccountCategory>
        {
            new() { AccShortName = "TRAVEL", ProjectSpecific = -1, Csg7Group = "GRP_NO_MATCH", AccountType = "A", FpsYear = DefaultFpsYear }
        };
        var groups = new List<AccountGroup>
        {
            new() { Csg7group = "GRP_OTHER", Useinflation = true }
        };
        var (repo, _, _) = CreateRepository(accountCategories: categories, accountGroups: groups);

        // Act
        var result = (await repo.GetProjectSpecificAccountCategoriesAsync()).ToList();

        // Assert — inner join means no match = excluded
        Assert.Empty(result);
    }

    #endregion

    #region UpdateAdditionalCostAsync - additional cases

    [Fact]
    public async Task UpdateAdditionalCostAsync_ReturnsSameEntityReference()
    {
        var (repo, _, _) = CreateRepository();
        var entity = new AdditionalCost
        {
            Project = "2024/001",
            Year = 2024,
            AccountCat = "TRAVEL",
            Description = "Test"
        };

        var result = await repo.UpdateAdditionalCostAsync(entity);

        Assert.Same(entity, result);
    }

    #endregion

    #region AddAdditionalCostAsync - additional cases

    [Fact]
    public async Task AddAdditionalCostAsync_WithNullOptionalFields_Succeeds()
    {
        var (repo, _, _) = CreateRepository();
        var newCost = new AdditionalCost
        {
            Project = "2024/001",
            Year = 2024,
            AccountCat = "TRAVEL",
            Description = "Test",
            ItemCost = null,
            Freq = null
        };

        var result = await repo.AddAdditionalCostAsync(newCost);

        Assert.NotNull(result);
        Assert.Null(result.ItemCost);
        Assert.Null(result.Freq);
    }

    #endregion

    #region GetAdditionalCostsByProjectYearAsync - additional cases

    [Fact]
    public async Task GetAdditionalCostsByProjectYearAsync_ReturnsMultipleRecords_WhenMultipleExist()
    {
        var costs = new List<AdditionalCost>
        {
            new() { AcIdentity = 1, Project = "2024/001", Year = 2024, AccountCat = "A", Description = "First",  CostEntered = 100 },
            new() { AcIdentity = 2, Project = "2024/001", Year = 2024, AccountCat = "B", Description = "Second", CostEntered = 200 },
            new() { AcIdentity = 3, Project = "2024/001", Year = 2024, AccountCat = "C", Description = "Third",  CostEntered = 300 }
        };
        var (repo, _, _) = CreateRepository(additionalCosts: costs);

        var result = (await repo.GetAdditionalCostsByProjectYearAsync("2024/001", 2024)).ToList();

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetAdditionalCostsByProjectYearAsync_ReturnsNullItemCost_WhenNotSet()
    {
        var costs = new List<AdditionalCost>
        {
            new() { AcIdentity = 1, Project = "2024/001", Year = 2024, AccountCat = "A", Description = "Test", CostEntered = 100, ItemCost = null }
        };
        var (repo, _, _) = CreateRepository(additionalCosts: costs);

        var result = (await repo.GetAdditionalCostsByProjectYearAsync("2024/001", 2024)).ToList();

        Assert.Single(result);
        Assert.Null(result[0].ItemCost);
    }

    #endregion

    #region GetProjectSpecificAccountCategoriesAsync - additional cases

    [Fact]
    public async Task GetProjectSpecificAccountCategoriesAsync_UseInflationFalse_WhenUseinflationIsNull()
    {
        var categories = new List<FpsAccountCategory>
        {
            new() { AccShortName = "TRAVEL", ProjectSpecific = -1, Csg7Group = "GRP1", AccountType = "A", FpsYear = DefaultFpsYear }
        };
        var groups = new List<AccountGroup>
        {
            new() { Csg7group = "GRP1", Useinflation = null }
        };
        var (repo, _, _) = CreateRepository(accountCategories: categories, accountGroups: groups);

        var result = (await repo.GetProjectSpecificAccountCategoriesAsync()).ToList();

        Assert.Single(result);
        Assert.False(result[0].UseInflation);
    }

    [Fact]
    public async Task GetProjectSpecificAccountCategoriesAsync_ReturnsMultipleRecords_WithCorrectInflationMapping()
    {
        var categories = new List<FpsAccountCategory>
        {
            new() { AccShortName = "ALPHA", ProjectSpecific = -1, Csg7Group = "GRP1", AccountType = "A", FpsYear = DefaultFpsYear },
            new() { AccShortName = "BETA",  ProjectSpecific = -1, Csg7Group = "GRP2", AccountType = "B", FpsYear = DefaultFpsYear }
        };
        var groups = new List<AccountGroup>
        {
            new() { Csg7group = "GRP1", Useinflation = true  },
            new() { Csg7group = "GRP2", Useinflation = false }
        };
        var (repo, _, _) = CreateRepository(accountCategories: categories, accountGroups: groups);

        var result = (await repo.GetProjectSpecificAccountCategoriesAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.True(result.First(r => r.AccShortName == "ALPHA").UseInflation);
        Assert.False(result.First(r => r.AccShortName == "BETA").UseInflation);
    }

    #endregion
}
