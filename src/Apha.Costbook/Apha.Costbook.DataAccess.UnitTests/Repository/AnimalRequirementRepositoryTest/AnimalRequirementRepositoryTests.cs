using Apha.Common.Helpers.Repository;
using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.DataAccess.Data;
using Apha.Costbook.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Apha.Costbook.DataAccess.UnitTests.Repository.AnimalRequirementRepositoryTest;

public class AnimalRequirementRepositoryTests
{
    private const int DefaultFpsYear = 2025;

    /// <summary>
    /// Creates an AnimalRequirementRepository with in-memory DbSets.
    /// Methods using multi-table JOINs (GetAnimalRequirementsByProjectYearAsync) and
    /// ExecuteDeleteAsync (DeleteAnimalRequirementAsync) are covered by integration tests.
    /// </summary>
    private static (
        AnimalRequirementRepository Repo,
        Mock<DbSet<AnimalRequirement>> AnimalRequirementsDbSet,
        Mock<CostbookDbContext> Context)
        CreateRepository(
            IEnumerable<AnimalRequirement>? animalRequirements = null,
            IEnumerable<FpsAnimals>? fpsAnimals = null,
            IEnumerable<Project>? projects = null)
    {
        var mockFpsYearContext = new Mock<IFPSYearContext>();
        mockFpsYearContext.Setup(x => x.FPSYear).Returns(DefaultFpsYear);

        var mockContext = RepositoryTestHelper.CreateMockDbContext<CostbookDbContext>(mockFpsYearContext.Object);

        var animalReqMockSet = RepositoryTestHelper.CreateMockDbSet(animalRequirements ?? []);
        RepositoryTestHelper.SetupDbSetOperations(animalReqMockSet);
        mockContext.Setup(x => x.AnimalRequirements).Returns(animalReqMockSet.Object);

        var projectsMockSet = RepositoryTestHelper.CreateMockDbSet(projects ?? []);
        mockContext.Setup(x => x.Projects).Returns(projectsMockSet.Object);

        if (fpsAnimals != null)
        {
            var animalsMockSet = RepositoryTestHelper.CreateMockDbSet(fpsAnimals);
            mockContext.Setup(x => x.FpsAnimals).Returns(animalsMockSet.Object);
        }

        RepositoryTestHelper.SetupSaveChanges(mockContext);

        var repo = new AnimalRequirementRepository(mockContext.Object);
        return (repo, animalReqMockSet, mockContext);
    }

    #region AddAnimalRequirementAsync

    [Fact]
    public async Task AddAnimalRequirementAsync_AddsEntity_AndCallsSaveChanges()
    {
        // Arrange
        var (repo, animalReqDbSet, mockContext) = CreateRepository();
        var newReq = new AnimalRequirement
        {
            ArIdentity = 1,
            Project = "2024/001",
            Year = 2024,
            AnimalType = "CAT",
            NumberOfDays = 5.0,
            NumberOfAnimals = 3.0,
            DailyRate = 10.50
        };

        // Act
        var result = await repo.AddAnimalRequirementAsync(newReq);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("2024/001", result.Project);
        Assert.Equal("CAT", result.AnimalType);
        Assert.Equal(5.0, result.NumberOfDays);
        Assert.Equal(3.0, result.NumberOfAnimals);
        Assert.Equal(10.50, result.DailyRate);
        animalReqDbSet.Verify(x => x.Add(It.IsAny<AnimalRequirement>()), Times.Once);
        RepositoryTestHelper.VerifySaveChanges(mockContext);
    }

    [Fact]
    public async Task AddAnimalRequirementAsync_DecodesUrlEncodedProject()
    {
        // Arrange
        var (repo, _, _) = CreateRepository();
        var newReq = new AnimalRequirement
        {
            Project = "2024%2F001",
            Year = 2024,
            AnimalType = "DOG"
        };

        // Act
        var result = await repo.AddAnimalRequirementAsync(newReq);

        // Assert
        Assert.Equal("2024/001", result.Project);
    }

    [Fact]
    public async Task AddAnimalRequirementAsync_ReturnsSameEntityReference()
    {
        // Arrange
        var (repo, _, _) = CreateRepository();
        var newReq = new AnimalRequirement
        {
            Project = "2024/001",
            Year = 2024,
            AnimalType = "CAT"
        };

        // Act
        var result = await repo.AddAnimalRequirementAsync(newReq);

        // Assert
        Assert.Same(newReq, result);
    }

    #endregion

    #region UpdateAnimalRequirementAsync

    [Fact]
    public async Task UpdateAnimalRequirementAsync_UpdatesEntity_AndCallsSaveChanges()
    {
        // Arrange
        var existing = new AnimalRequirement
        {
            ArIdentity = 1,
            Project = "2024/001",
            Year = 2024,
            AnimalType = "CAT",
            NumberOfDays = 5.0,
            NumberOfAnimals = 3.0,
            DailyRate = 10.50
        };
        var (repo, animalReqDbSet, mockContext) = CreateRepository(animalRequirements: [existing]);

        existing.NumberOfDays = 10.0;

        // Act
        var result = await repo.UpdateAnimalRequirementAsync(existing);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10.0, result.NumberOfDays);
        animalReqDbSet.Verify(x => x.Update(It.IsAny<AnimalRequirement>()), Times.Once);
        RepositoryTestHelper.VerifySaveChanges(mockContext);
    }

    [Fact]
    public async Task UpdateAnimalRequirementAsync_DecodesUrlEncodedProject()
    {
        // Arrange
        var (repo, _, _) = CreateRepository();
        var entity = new AnimalRequirement
        {
            Project = "2024%2F001",
            Year = 2024,
            AnimalType = "CAT"
        };

        // Act
        var result = await repo.UpdateAnimalRequirementAsync(entity);

        // Assert
        Assert.Equal("2024/001", result.Project);
    }

    #endregion

    #region GetAnimalRatesAsync

    [Fact]
    public async Task GetAnimalRatesAsync_ReturnsEmptyList_WhenNoAnimals()
    {
        // Arrange
        var (repo, _, _) = CreateRepository(fpsAnimals: []);

        // Act
        var result = await repo.GetAnimalRatesAsync(isDefra: false);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAnimalRatesAsync_ReturnsDailyRate_WhenNotDefra()
    {
        // Arrange
        var animals = new List<FpsAnimals>
        {
            new() { AnimalType = "CAT", DailyRate = 10.50m, DefraDailyRate = 15.00m, FpsYear = DefaultFpsYear }
        };
        var (repo, _, _) = CreateRepository(fpsAnimals: animals);

        // Act
        var result = (await repo.GetAnimalRatesAsync(isDefra: false)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("CAT", result[0].AnimalType);
        Assert.Equal(10.50, result[0].DailyRate);
    }

    [Fact]
    public async Task GetAnimalRatesAsync_ReturnsDefraDailyRate_WhenIsDefra()
    {
        // Arrange
        var animals = new List<FpsAnimals>
        {
            new() { AnimalType = "CAT", DailyRate = 10.50m, DefraDailyRate = 15.00m, FpsYear = DefaultFpsYear }
        };
        var (repo, _, _) = CreateRepository(fpsAnimals: animals);

        // Act
        var result = (await repo.GetAnimalRatesAsync(isDefra: true)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(15.00, result[0].DailyRate);
    }

    [Fact]
    public async Task GetAnimalRatesAsync_ReturnsResultsOrderedByAnimalType()
    {
        // Arrange
        var animals = new List<FpsAnimals>
        {
            new() { AnimalType = "DOG", DailyRate = 20m, DefraDailyRate = 25m, FpsYear = DefaultFpsYear },
            new() { AnimalType = "CAT", DailyRate = 10m, DefraDailyRate = 15m, FpsYear = DefaultFpsYear },
            new() { AnimalType = "BIRD", DailyRate = 5m, DefraDailyRate = 8m, FpsYear = DefaultFpsYear }
        };
        var (repo, _, _) = CreateRepository(fpsAnimals: animals);

        // Act
        var result = (await repo.GetAnimalRatesAsync(isDefra: false)).ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("BIRD", result[0].AnimalType);
        Assert.Equal("CAT", result[1].AnimalType);
        Assert.Equal("DOG", result[2].AnimalType);
    }

    [Fact]
    public async Task GetAnimalRatesAsync_MapsAnimalTypeCorrectly()
    {
        // Arrange
        var animals = new List<FpsAnimals>
        {
            new() { AnimalType = "HORSE", DailyRate = 50m, DefraDailyRate = 60m, FpsYear = DefaultFpsYear }
        };
        var (repo, _, _) = CreateRepository(fpsAnimals: animals);

        // Act
        var result = (await repo.GetAnimalRatesAsync(isDefra: false)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("HORSE", result[0].AnimalType);
        Assert.Equal(50.0, result[0].DailyRate);
    }

    #endregion

    #region GetAllAnimalsAsync

    [Fact]
    public async Task GetAllAnimalsAsync_ReturnsEmptyList_WhenNoAnimals()
    {
        // Arrange
        var (repo, _, _) = CreateRepository(fpsAnimals: []);

        // Act
        var result = await repo.GetAllAnimalsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAnimalsAsync_ReturnsAllAnimals_OrderedByAnimalType()
    {
        // Arrange
        var animals = new List<FpsAnimals>
        {
            new() { AnimalType = "DOG", Species = "Canis", DailyRate = 20m, DefraDailyRate = 25m, FpsYear = DefaultFpsYear },
            new() { AnimalType = "CAT", Species = "Felis", DailyRate = 10m, DefraDailyRate = 15m, FpsYear = DefaultFpsYear }
        };
        var (repo, _, _) = CreateRepository(fpsAnimals: animals);

        // Act
        var result = (await repo.GetAllAnimalsAsync()).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("CAT", result[0].AnimalType);
        Assert.Equal("DOG", result[1].AnimalType);
    }

    #endregion

    #region GetAnimalRequirementsByProjectYearAsync

    [Fact]
    public async Task GetAnimalRequirementsByProjectYearAsync_ReturnsEmptyList_WhenNoRequirements()
    {
        // Arrange
        var (repo, _, _) = CreateRepository();

        // Act
        var result = await repo.GetAnimalRequirementsByProjectYearAsync("2024/001", 2024);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAnimalRequirementsByProjectYearAsync_FiltersByProjectAndYear()
    {
        // Arrange
        var reqs = new List<AnimalRequirement>
        {
            new() { ArIdentity = 1, Project = "2024/001", Year = 2024, AnimalType = "CAT", NumberOfDays = 5, NumberOfAnimals = 2, DailyRate = 10 },
            new() { ArIdentity = 2, Project = "2024/002", Year = 2024, AnimalType = "DOG", NumberOfDays = 3, NumberOfAnimals = 1, DailyRate = 20 },
            new() { ArIdentity = 3, Project = "2024/001", Year = 2025, AnimalType = "BIRD", NumberOfDays = 1, NumberOfAnimals = 1, DailyRate = 5 }
        };
        var (repo, _, _) = CreateRepository(animalRequirements: reqs);

        // Act
        var result = (await repo.GetAnimalRequirementsByProjectYearAsync("2024/001", 2024)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(1, result[0].ArIdentity);
    }

    [Fact]
    public async Task GetAnimalRequirementsByProjectYearAsync_JoinsProjectData()
    {
        // Arrange
        var reqs = new List<AnimalRequirement>
        {
            new() { ArIdentity = 1, Project = "2024/001", Year = 2024, AnimalType = "CAT", NumberOfDays = 5, NumberOfAnimals = 2, DailyRate = 10 }
        };
        var projects = new List<Project>
        {
            new() { ProjectId = "2024/001", Programme = "Programme X", Euroconvrate = 1.20 }
        };
        var (repo, _, _) = CreateRepository(animalRequirements: reqs, projects: projects);

        // Act
        var result = (await repo.GetAnimalRequirementsByProjectYearAsync("2024/001", 2024)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Programme X", result[0].Programme);
        Assert.Equal(1.20, result[0].EuroConvRate);
    }

    [Fact]
    public async Task GetAnimalRequirementsByProjectYearAsync_ReturnsNullProjectFields_WhenNoMatchingProject()
    {
        // Arrange
        var reqs = new List<AnimalRequirement>
        {
            new() { ArIdentity = 1, Project = "2024/001", Year = 2024, AnimalType = "CAT", NumberOfDays = 5, NumberOfAnimals = 2, DailyRate = 10 }
        };
        var (repo, _, _) = CreateRepository(animalRequirements: reqs, projects: []);

        // Act
        var result = (await repo.GetAnimalRequirementsByProjectYearAsync("2024/001", 2024)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Null(result[0].Programme);
        Assert.Null(result[0].EuroConvRate);
    }

    [Fact]
    public async Task GetAnimalRequirementsByProjectYearAsync_CalculatesAnimalCost()
    {
        // Arrange
        var reqs = new List<AnimalRequirement>
        {
            new() { ArIdentity = 1, Project = "2024/001", Year = 2024, AnimalType = "CAT", NumberOfDays = 5, NumberOfAnimals = 3, DailyRate = 10.0 }
        };
        var (repo, _, _) = CreateRepository(animalRequirements: reqs);

        // Act
        var result = (await repo.GetAnimalRequirementsByProjectYearAsync("2024/001", 2024)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(150.0, result[0].AnimalCost); // 5 * 3 * 10
    }

    [Fact]
    public async Task GetAnimalRequirementsByProjectYearAsync_ReturnsNullAnimalCost_WhenAnyFactorIsNull()
    {
        // Arrange
        var reqs = new List<AnimalRequirement>
        {
            new() { ArIdentity = 1, Project = "2024/001", Year = 2024, AnimalType = "CAT", NumberOfDays = null, NumberOfAnimals = 3, DailyRate = 10.0 }
        };
        var (repo, _, _) = CreateRepository(animalRequirements: reqs);

        // Act
        var result = (await repo.GetAnimalRequirementsByProjectYearAsync("2024/001", 2024)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Null(result[0].AnimalCost);
    }

    [Fact]
    public async Task GetAnimalRequirementsByProjectYearAsync_ReturnsOrderedByAnimalType()
    {
        // Arrange
        var reqs = new List<AnimalRequirement>
        {
            new() { ArIdentity = 1, Project = "2024/001", Year = 2024, AnimalType = "DOG", NumberOfDays = 1, NumberOfAnimals = 1, DailyRate = 1 },
            new() { ArIdentity = 2, Project = "2024/001", Year = 2024, AnimalType = "BIRD", NumberOfDays = 1, NumberOfAnimals = 1, DailyRate = 1 },
            new() { ArIdentity = 3, Project = "2024/001", Year = 2024, AnimalType = "CAT", NumberOfDays = 1, NumberOfAnimals = 1, DailyRate = 1 }
        };
        var (repo, _, _) = CreateRepository(animalRequirements: reqs);

        // Act
        var result = (await repo.GetAnimalRequirementsByProjectYearAsync("2024/001", 2024)).ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("BIRD", result[0].AnimalType);
        Assert.Equal("CAT", result[1].AnimalType);
        Assert.Equal("DOG", result[2].AnimalType);
    }

    [Fact]
    public async Task GetAnimalRequirementsByProjectYearAsync_MapsAllFields()
    {
        // Arrange
        var reqs = new List<AnimalRequirement>
        {
            new() { ArIdentity = 99, Project = "2024/001", Year = 2024, AnimalType = "CAT", NumberOfDays = 10, NumberOfAnimals = 3, DailyRate = 15.5 }
        };
        var (repo, _, _) = CreateRepository(animalRequirements: reqs);

        // Act
        var result = (await repo.GetAnimalRequirementsByProjectYearAsync("2024/001", 2024)).ToList();

        // Assert
        var item = Assert.Single(result);
        Assert.Equal(99, item.ArIdentity);
        Assert.Equal("2024/001", item.Project);
        Assert.Equal(2024, item.Year);
        Assert.Equal("CAT", item.AnimalType);
        Assert.Equal(10, item.NumberOfDays);
        Assert.Equal(3, item.NumberOfAnimals);
        Assert.Equal(15.5, item.DailyRate);
        Assert.Equal(465.0, item.AnimalCost); // 10 * 3 * 15.5
    }

    [Fact]
    public async Task GetAnimalRequirementsByProjectYearAsync_DecodesUrlEncodedProject()
    {
        // Arrange
        var reqs = new List<AnimalRequirement>
        {
            new() { ArIdentity = 1, Project = "2024/001", Year = 2024, AnimalType = "CAT", NumberOfDays = 1, NumberOfAnimals = 1, DailyRate = 1 }
        };
        var (repo, _, _) = CreateRepository(animalRequirements: reqs);

        // Act
        var result = (await repo.GetAnimalRequirementsByProjectYearAsync("2024%2F001", 2024)).ToList();

        // Assert
        Assert.Single(result);
    }

    #endregion
}
