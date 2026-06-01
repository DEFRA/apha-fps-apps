using Apha.Common.Helpers.Repository;
using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.DataAccess.Data;
using Apha.Costbook.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Apha.Costbook.DataAccess.UnitTests.Repository.ProjectYearRepositoryTest;

public class ProjectYearRepositoryTests
{
    private const int DefaultFpsYear = 2025;

    /// <summary>
    /// Creates a ProjectYearRepository with in-memory DbSets.
    /// GetPayRatesAsync uses a Join across WorkGroupGrades and ProfitCentreGrades.
    /// AddProjectYearAsync uses Projects and ProjectYears DbSets plus ISettingsRepository.
    /// </summary>
    private static (
        ProjectYearRepository Repo,
        Mock<DbSet<ProjectYear>> ProjectYearsDbSet,
        Mock<CostbookDbContext> Context,
        Mock<ISettingsRepository> SettingsRepo)
        CreateRepository(
            IEnumerable<ProjectYear>? projectYears = null,
            IEnumerable<Project>? projects = null,
            IEnumerable<WorkGroupGrade>? workGroupGrades = null,
            IEnumerable<ProfitCentreGrade>? profitCentreGrades = null,
            Dictionary<string, string?>? settings = null,
            IEnumerable<StaffRequirement>? staffRequirements = null,
            IEnumerable<TestRequirement>? testRequirements = null,
            IEnumerable<AnimalRequirement>? animalRequirements = null,
            IEnumerable<AdditionalCost>? additionalCosts = null)
    {
        var mockFpsYearContext = new Mock<IFPSYearContext>();
        mockFpsYearContext.Setup(x => x.FPSYear).Returns(DefaultFpsYear);

        var mockContext = RepositoryTestHelper.CreateMockDbContext<CostbookDbContext>(mockFpsYearContext.Object);

        var projectYearsMockSet = RepositoryTestHelper.CreateMockDbSet(projectYears ?? []);
        RepositoryTestHelper.SetupDbSetOperations(projectYearsMockSet);
        mockContext.Setup(x => x.ProjectYears).Returns(projectYearsMockSet.Object);

        var projectsMockSet = RepositoryTestHelper.CreateMockDbSet(projects ?? []);
        mockContext.Setup(x => x.Projects).Returns(projectsMockSet.Object);

        var wggMockSet = RepositoryTestHelper.CreateMockDbSet(workGroupGrades ?? []);
        mockContext.Setup(x => x.WorkGroupGrades).Returns(wggMockSet.Object);

        var pcgMockSet = RepositoryTestHelper.CreateMockDbSet(profitCentreGrades ?? []);
        mockContext.Setup(x => x.ProfitCentreGrades).Returns(pcgMockSet.Object);

        var staffMockSet = RepositoryTestHelper.CreateMockDbSet(staffRequirements ?? []);
        mockContext.Setup(x => x.StaffRequirements).Returns(staffMockSet.Object);

        var testMockSet = RepositoryTestHelper.CreateMockDbSet(testRequirements ?? []);
        mockContext.Setup(x => x.TestRequirements).Returns(testMockSet.Object);

        var animalMockSet = RepositoryTestHelper.CreateMockDbSet(animalRequirements ?? []);
        mockContext.Setup(x => x.AnimalRequirements).Returns(animalMockSet.Object);

        var additionalCostMockSet = RepositoryTestHelper.CreateMockDbSet(additionalCosts ?? []);
        mockContext.Setup(x => x.AdditionalCosts).Returns(additionalCostMockSet.Object);

        RepositoryTestHelper.SetupSaveChanges(mockContext);

        var mockSettingsRepo = new Mock<ISettingsRepository>();
        mockSettingsRepo.Setup(x => x.GetSettingValueByIdAsync("CurrentYear"))
            .ReturnsAsync(DefaultFpsYear.ToString());
        if (settings != null)
        {
            foreach (var kvp in settings)
            {
                mockSettingsRepo.Setup(x => x.GetSettingValueByIdAsync(kvp.Key))
                    .ReturnsAsync(kvp.Value);
            }
        }

        var mockProjectRepo = new Mock<IProjectRepository>();
        mockProjectRepo.Setup(x => x.GetInflationFactorAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(1.0);

        var repo = new ProjectYearRepository(mockContext.Object, mockSettingsRepo.Object, mockProjectRepo.Object);
        return (repo, projectYearsMockSet, mockContext, mockSettingsRepo);
    }

    #region GetByProjectAsync

    [Fact]
    public async Task GetByProjectAsync_ReturnsEmptyList_WhenNoYears()
    {
        // Arrange
        var (repo, _, _, _) = CreateRepository();

        // Act
        var result = await repo.GetByProjectAsync("2024/001");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByProjectAsync_FiltersByProject()
    {
        // Arrange
        var years = new List<ProjectYear>
        {
            new() { Project = "2024/001", YearValue = 1 },
            new() { Project = "2024/002", YearValue = 1 },
            new() { Project = "2024/001", YearValue = 2 }
        };
        var (repo, _, _, _) = CreateRepository(projectYears: years);

        // Act
        var result = (await repo.GetByProjectAsync("2024/001")).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, py => Assert.Equal("2024/001", py.Project));
    }

    [Fact]
    public async Task GetByProjectAsync_ReturnsOrderedByYearValue()
    {
        // Arrange
        var years = new List<ProjectYear>
        {
            new() { Project = "2024/001", YearValue = 3 },
            new() { Project = "2024/001", YearValue = 1 },
            new() { Project = "2024/001", YearValue = 2 }
        };
        var (repo, _, _, _) = CreateRepository(projectYears: years);

        // Act
        var result = (await repo.GetByProjectAsync("2024/001")).ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(1, result[0].YearValue);
        Assert.Equal(2, result[1].YearValue);
        Assert.Equal(3, result[2].YearValue);
    }

    [Fact]
    public async Task GetByProjectAsync_DecodesUrlEncodedProject()
    {
        // Arrange
        var years = new List<ProjectYear>
        {
            new() { Project = "2024/001", YearValue = 1 }
        };
        var (repo, _, _, _) = CreateRepository(projectYears: years);

        // Act
        var result = (await repo.GetByProjectAsync("2024%2F001")).ToList();

        // Assert
        Assert.Single(result);
    }

    #endregion

    #region GetMaxProjectYearAsync

    [Fact]
    public async Task GetMaxProjectYearAsync_ReturnsNull_WhenNoYears()
    {
        // Arrange
        var (repo, _, _, _) = CreateRepository();

        // Act
        var result = await repo.GetMaxProjectYearAsync("2024/001");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetMaxProjectYearAsync_ReturnsMaxYear()
    {
        // Arrange
        var years = new List<ProjectYear>
        {
            new() { Project = "2024/001", YearValue = 1 },
            new() { Project = "2024/001", YearValue = 3 },
            new() { Project = "2024/001", YearValue = 2 }
        };
        var (repo, _, _, _) = CreateRepository(projectYears: years);

        // Act
        var result = await repo.GetMaxProjectYearAsync("2024/001");

        // Assert
        Assert.Equal(3, result);
    }

    [Fact]
    public async Task GetMaxProjectYearAsync_OnlyConsidersMatchingProject()
    {
        // Arrange
        var years = new List<ProjectYear>
        {
            new() { Project = "2024/001", YearValue = 2 },
            new() { Project = "2024/002", YearValue = 5 }
        };
        var (repo, _, _, _) = CreateRepository(projectYears: years);

        // Act
        var result = await repo.GetMaxProjectYearAsync("2024/001");

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public async Task GetMaxProjectYearAsync_DecodesUrlEncodedProject()
    {
        // Arrange
        var years = new List<ProjectYear>
        {
            new() { Project = "2024/001", YearValue = 1 }
        };
        var (repo, _, _, _) = CreateRepository(projectYears: years);

        // Act
        var result = await repo.GetMaxProjectYearAsync("2024%2F001");

        // Assert
        Assert.Equal(1, result);
    }

    #endregion

    #region UpdateProjectYearAsync

    [Fact]
    public async Task UpdateProjectYearAsync_UpdatesEntity_AndCallsSaveChanges()
    {
        // Arrange
        var existing = new ProjectYear
        {
            Project = "2024/001",
            YearValue = 1,
            MarkupTime = 10.0
        };
        var (repo, pyDbSet, mockContext, _) = CreateRepository(projectYears: [existing]);

        existing.MarkupTime = 25.0;

        // Act
        var result = await repo.UpdateProjectYearAsync(existing);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(25.0, result.MarkupTime);
        pyDbSet.Verify(x => x.Update(It.IsAny<ProjectYear>()), Times.Once);
        RepositoryTestHelper.VerifySaveChanges(mockContext);
    }

    [Fact]
    public async Task UpdateProjectYearAsync_ReturnsSameEntityReference()
    {
        // Arrange
        var entity = new ProjectYear { Project = "2024/001", YearValue = 1 };
        var (repo, _, _, _) = CreateRepository();

        // Act
        var result = await repo.UpdateProjectYearAsync(entity);

        // Assert
        Assert.Same(entity, result);
    }

    #endregion

    #region AddProjectYearAsync

    [Fact]
    public async Task AddProjectYearAsync_WithRateData_UsesProvidedRates()
    {
        // Arrange
        var projects = new List<Project>
        {
            new() { ProjectId = "2024/001", Programme = "Comm" }
        };
        var (repo, pyDbSet, mockContext, _) = CreateRepository(projects: projects);

        var yearData = new ProjectYear
        {
            MarkupTime = 10.0,
            MarkupTests = 11.0,
            MarkupAnimals = 12.0,
            MarkupAdditional = 13.0,
            ProfitTime = 5.0,
            ProfitTests = 6.0,
            ProfitAnimals = 7.0,
            ProfitAdditional = 8.0
        };

        // Act
        var result = await repo.AddProjectYearAsync("2024/001", 2, yearData);

        // Assert
        Assert.Equal("2024/001", result.Project);
        Assert.Equal(2, result.YearValue);
        Assert.Equal(10.0, result.MarkupTime);
        Assert.Equal(11.0, result.MarkupTests);
        Assert.Equal(12.0, result.MarkupAnimals);
        Assert.Equal(13.0, result.MarkupAdditional);
        Assert.Equal(5.0, result.ProfitTime);
        Assert.Equal(6.0, result.ProfitTests);
        Assert.Equal(7.0, result.ProfitAnimals);
        Assert.Equal(8.0, result.ProfitAdditional);
        pyDbSet.Verify(x => x.Add(It.IsAny<ProjectYear>()), Times.Once);
        RepositoryTestHelper.VerifySaveChanges(mockContext);
    }

    [Fact]
    public async Task AddProjectYearAsync_NonCommercialProject_CreatesWithNullRates()
    {
        // Arrange
        var projects = new List<Project>
        {
            new() { ProjectId = "2024/001", Programme = "Research" }
        };
        var (repo, _, _, _) = CreateRepository(projects: projects);

        // Act
        var result = await repo.AddProjectYearAsync("2024/001", 1, new ProjectYear());

        // Assert
        Assert.Equal("2024/001", result.Project);
        Assert.Equal(1, result.YearValue);
        Assert.Null(result.MarkupTime);
        Assert.Null(result.ProfitTime);
    }

    [Fact]
    public async Task AddProjectYearAsync_CommercialProject_NoPreviousYear_UsesSettings()
    {
        // Arrange
        var projects = new List<Project>
        {
            new() { ProjectId = "2024/001", Programme = "Comm" }
        };
        var settings = new Dictionary<string, string?>
        {
            ["Profitstaff"] = "5.0",
            ["Profittests"] = "6.0",
            ["ProfitAnimals"] = "7.0",
            ["ProfitExceptional"] = "8.0",
            ["Markupstaff"] = "10.0",
            ["Markuptests"] = "11.0",
            ["MarkupAnimals"] = "12.0",
            ["MarkupExceptional"] = "13.0"
        };
        var (repo, _, _, _) = CreateRepository(projects: projects, settings: settings);

        // Act
        var result = await repo.AddProjectYearAsync("2024/001", 1, new ProjectYear());

        // Assert
        Assert.Equal(10.0, result.MarkupTime);
        Assert.Equal(11.0, result.MarkupTests);
        Assert.Equal(12.0, result.MarkupAnimals);
        Assert.Equal(13.0, result.MarkupAdditional);
        Assert.Equal(5.0, result.ProfitTime);
        Assert.Equal(6.0, result.ProfitTests);
        Assert.Equal(7.0, result.ProfitAnimals);
        Assert.Equal(8.0, result.ProfitAdditional);
    }

    [Fact]
    public async Task AddProjectYearAsync_CommercialProject_WithPreviousYear_CopiesRates()
    {
        // Arrange
        var projects = new List<Project>
        {
            new() { ProjectId = "2024/001", Programme = "Comm" }
        };
        var existingYears = new List<ProjectYear>
        {
            new() { Project = "2024/001", YearValue = 1, MarkupTime = 20.0, MarkupTests = 21.0, MarkupAnimals = 22.0, MarkupAdditional = 23.0, ProfitTime = 15.0, ProfitTests = 16.0, ProfitAnimals = 17.0, ProfitAdditional = 18.0 }
        };
        var (repo, _, _, _) = CreateRepository(projects: projects, projectYears: existingYears);

        // Act
        var result = await repo.AddProjectYearAsync("2024/001", 2, new ProjectYear());

        // Assert
        Assert.Equal(20.0, result.MarkupTime);
        Assert.Equal(21.0, result.MarkupTests);
        Assert.Equal(22.0, result.MarkupAnimals);
        Assert.Equal(23.0, result.MarkupAdditional);
        Assert.Equal(15.0, result.ProfitTime);
        Assert.Equal(16.0, result.ProfitTests);
        Assert.Equal(17.0, result.ProfitAnimals);
        Assert.Equal(18.0, result.ProfitAdditional);
    }

    [Fact]
    public async Task AddProjectYearAsync_CommercialProject_PreviousYearNullRates_FallsBackToSettings()
    {
        // Arrange
        var projects = new List<Project>
        {
            new() { ProjectId = "2024/001", Programme = "Comm" }
        };
        var existingYears = new List<ProjectYear>
        {
            new() { Project = "2024/001", YearValue = 1 } // all rates null
        };
        var settings = new Dictionary<string, string?>
        {
            ["Profitstaff"] = "5.0",
            ["Profittests"] = "6.0",
            ["ProfitAnimals"] = "7.0",
            ["ProfitExceptional"] = "8.0",
            ["Markupstaff"] = "10.0",
            ["Markuptests"] = "11.0",
            ["MarkupAnimals"] = "12.0",
            ["MarkupExceptional"] = "13.0"
        };
        var (repo, _, _, _) = CreateRepository(projects: projects, projectYears: existingYears, settings: settings);

        // Act
        var result = await repo.AddProjectYearAsync("2024/001", 2, new ProjectYear());

        // Assert
        Assert.Equal(10.0, result.MarkupTime);
        Assert.Equal(5.0, result.ProfitTime);
    }

    [Fact]
    public async Task AddProjectYearAsync_DecodesUrlEncodedProject()
    {
        // Arrange
        var projects = new List<Project>
        {
            new() { ProjectId = "2024/001", Programme = "Research" }
        };
        var (repo, _, _, _) = CreateRepository(projects: projects);

        // Act
        var result = await repo.AddProjectYearAsync("2024%2F001", 1, new ProjectYear());

        // Assert
        Assert.Equal("2024/001", result.Project);
    }

    #endregion

    #region GetPayRatesAsync

    [Fact]
    public async Task GetPayRatesAsync_ReturnsEmptyList_WhenNoData()
    {
        // Arrange
        var (repo, _, _, _) = CreateRepository();

        // Act
        var result = await repo.GetPayRatesAsync("2024/001", 2024, isDefra: false);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetPayRatesAsync_NonDefra_ReturnsChargeRate()
    {
        // Arrange
        var wggs = new List<WorkGroupGrade>
        {
            new() { WgGrade = "HEO", ProfitCentreGrade = "PCG1", GradeCode = "GC01", WorkGroup = "Science", FpsYear = DefaultFpsYear }
        };
        var pcgs = new List<ProfitCentreGrade>
        {
            new() { PcGrade = "PCG1", ChargeRate = 45.50m, DefraChargeRate = 55.00m, PayRate = 30.0m, Npr = 5.0m, Ohr = 10.0m, FpsYear = DefaultFpsYear, DivisionGrade = "DG1", GradeCode = "GC01", ProfitCentre = "PC1" }
        };
        var (repo, _, _, _) = CreateRepository(workGroupGrades: wggs, profitCentreGrades: pcgs);

        // Act
        var result = (await repo.GetPayRatesAsync("2024/001", 2024, isDefra: false)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("HEO", result[0].WgGrade);
        Assert.Equal(45.50m, result[0].ChargeRate);
        Assert.Equal(30.0m, result[0].PayRate);
        Assert.Equal(5.0m, result[0].Npr);
        Assert.Equal(10.0m, result[0].Ohr);
    }

    [Fact]
    public async Task GetPayRatesAsync_Defra_ReturnsDefraChargeRate()
    {
        // Arrange
        var wggs = new List<WorkGroupGrade>
        {
            new() { WgGrade = "HEO", ProfitCentreGrade = "PCG1", GradeCode = "GC01", WorkGroup = "Science", FpsYear = DefaultFpsYear }
        };
        var pcgs = new List<ProfitCentreGrade>
        {
            new() { PcGrade = "PCG1", ChargeRate = 45.50m, DefraChargeRate = 55.00m, PayRate = 30.0m, Npr = 5.0m, Ohr = 10.0m, FpsYear = DefaultFpsYear, DivisionGrade = "DG1", GradeCode = "GC01", ProfitCentre = "PC1" }
        };
        var (repo, _, _, _) = CreateRepository(workGroupGrades: wggs, profitCentreGrades: pcgs);

        // Act
        var result = (await repo.GetPayRatesAsync("2024/001", 2024, isDefra: true)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(55.00m, result[0].ChargeRate);
    }

    [Fact]
    public async Task GetPayRatesAsync_NonDefra_FiltersOutZeroChargeRate()
    {
        // Arrange
        var wggs = new List<WorkGroupGrade>
        {
            new() { WgGrade = "HEO", ProfitCentreGrade = "PCG1", GradeCode = "GC01", WorkGroup = "Science", FpsYear = DefaultFpsYear },
            new() { WgGrade = "EO", ProfitCentreGrade = "PCG2", GradeCode = "GC02", WorkGroup = "Admin", FpsYear = DefaultFpsYear }
        };
        var pcgs = new List<ProfitCentreGrade>
        {
            new() { PcGrade = "PCG1", ChargeRate = 45.50m, DefraChargeRate = 55.00m, PayRate = 30.0m, Npr = 5.0m, Ohr = 10.0m, FpsYear = DefaultFpsYear, DivisionGrade = "DG1", GradeCode = "GC01", ProfitCentre = "PC1" },
            new() { PcGrade = "PCG2", ChargeRate = 0m, DefraChargeRate = 10.00m, PayRate = 20.0m, Npr = 3.0m, Ohr = 7.0m, FpsYear = DefaultFpsYear, DivisionGrade = "DG2", GradeCode = "GC02", ProfitCentre = "PC2" }
        };
        var (repo, _, _, _) = CreateRepository(workGroupGrades: wggs, profitCentreGrades: pcgs);

        // Act
        var result = (await repo.GetPayRatesAsync("2024/001", 2024, isDefra: false)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("HEO", result[0].WgGrade);
    }

    [Fact]
    public async Task GetPayRatesAsync_Defra_FiltersOutZeroDefraChargeRate()
    {
        // Arrange
        var wggs = new List<WorkGroupGrade>
        {
            new() { WgGrade = "HEO", ProfitCentreGrade = "PCG1", GradeCode = "GC01", WorkGroup = "Science", FpsYear = DefaultFpsYear },
            new() { WgGrade = "EO", ProfitCentreGrade = "PCG2", GradeCode = "GC02", WorkGroup = "Admin", FpsYear = DefaultFpsYear }
        };
        var pcgs = new List<ProfitCentreGrade>
        {
            new() { PcGrade = "PCG1", ChargeRate = 45.50m, DefraChargeRate = 55.00m, PayRate = 30.0m, Npr = 5.0m, Ohr = 10.0m, FpsYear = DefaultFpsYear, DivisionGrade = "DG1", GradeCode = "GC01", ProfitCentre = "PC1" },
            new() { PcGrade = "PCG2", ChargeRate = 20.00m, DefraChargeRate = 0m, PayRate = 20.0m, Npr = 3.0m, Ohr = 7.0m, FpsYear = DefaultFpsYear, DivisionGrade = "DG2", GradeCode = "GC02", ProfitCentre = "PC2" }
        };
        var (repo, _, _, _) = CreateRepository(workGroupGrades: wggs, profitCentreGrades: pcgs);

        // Act
        var result = (await repo.GetPayRatesAsync("2024/001", 2024, isDefra: true)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("HEO", result[0].WgGrade);
    }

    #endregion

    #region DeleteProjectYearAsync

    [Fact]
    public async Task DeleteProjectYearAsync_ReturnsSuccess_WhenNoChildRecords()
    {
        var years = new List<ProjectYear>
        {
            new() { Project = "2024/001", YearValue = 2024 }
        };
        var (repo, pyDbSet, mockContext, _) = CreateRepository(projectYears: years);

        var (deleted, errors) = await repo.DeleteProjectYearAsync("2024/001", 2024);

        Assert.True(deleted);
        Assert.Empty(errors);
        pyDbSet.Verify(x => x.Remove(It.IsAny<ProjectYear>()), Times.Once);
        RepositoryTestHelper.VerifySaveChanges(mockContext);
    }

    [Fact]
    public async Task DeleteProjectYearAsync_ReturnsFalse_WhenYearNotFound()
    {
        var (repo, pyDbSet, mockContext, _) = CreateRepository();

        var (deleted, errors) = await repo.DeleteProjectYearAsync("2024/001", 2024);

        Assert.False(deleted);
        Assert.Empty(errors);
        pyDbSet.Verify(x => x.Remove(It.IsAny<ProjectYear>()), Times.Never);
    }

    [Fact]
    public async Task DeleteProjectYearAsync_ReturnsFalse_WhenStaffRequirementsExist()
    {
        var years = new List<ProjectYear>
        {
            new() { Project = "2024/001", YearValue = 2024 }
        };
        var staff = new List<StaffRequirement>
        {
            new() { Project = "2024/001", Year = 2024, WgGrade = "HEO" },
            new() { Project = "2024/001", Year = 2024, WgGrade = "EO"  }
        };
        var (repo, pyDbSet, _, _) = CreateRepository(projectYears: years, staffRequirements: staff);

        var (deleted, errors) = await repo.DeleteProjectYearAsync("2024/001", 2024);

        Assert.False(deleted);
        Assert.Single(errors);
        Assert.Contains("2 staff requirements", errors[0]);
        pyDbSet.Verify(x => x.Remove(It.IsAny<ProjectYear>()), Times.Never);
    }

    [Fact]
    public async Task DeleteProjectYearAsync_ReturnsFalse_WhenTestRequirementsExist()
    {
        var years = new List<ProjectYear>
        {
            new() { Project = "2024/001", YearValue = 2024 }
        };
        var tests = new List<TestRequirement>
        {
            new() { Project = "2024/001", Year = 2024, TestCode = "TC001" }
        };
        var (repo, _, _, _) = CreateRepository(projectYears: years, testRequirements: tests);

        var (deleted, errors) = await repo.DeleteProjectYearAsync("2024/001", 2024);

        Assert.False(deleted);
        Assert.Single(errors);
        Assert.Contains("1 test requirements", errors[0]);
    }

    [Fact]
    public async Task DeleteProjectYearAsync_ReturnsFalse_WhenAnimalRequirementsExist()
    {
        var years = new List<ProjectYear>
        {
            new() { Project = "2024/001", YearValue = 2024 }
        };
        var animals = new List<AnimalRequirement>
        {
            new() { Project = "2024/001", Year = 2024, AnimalType = "CAT" }
        };
        var (repo, _, _, _) = CreateRepository(projectYears: years, animalRequirements: animals);

        var (deleted, errors) = await repo.DeleteProjectYearAsync("2024/001", 2024);

        Assert.False(deleted);
        Assert.Single(errors);
        Assert.Contains("1 animal requirements", errors[0]);
    }

    [Fact]
    public async Task DeleteProjectYearAsync_ReturnsFalse_WhenAdditionalCostsExist()
    {
        var years = new List<ProjectYear>
        {
            new() { Project = "2024/001", YearValue = 2024 }
        };
        var costs = new List<AdditionalCost>
        {
            new() { Project = "2024/001", Year = 2024, AccountCat = "TRAVEL", Description = "Test", CostEntered = 100 }
        };
        var (repo, _, _, _) = CreateRepository(projectYears: years, additionalCosts: costs);

        var (deleted, errors) = await repo.DeleteProjectYearAsync("2024/001", 2024);

        Assert.False(deleted);
        Assert.Single(errors);
        Assert.Contains("1 additional costs", errors[0]);
    }

    [Fact]
    public async Task DeleteProjectYearAsync_ReturnsAllErrors_WhenMultipleChildTypesExist()
    {
        var years = new List<ProjectYear>
        {
            new() { Project = "2024/001", YearValue = 2024 }
        };
        var staff  = new List<StaffRequirement>  { new() { Project = "2024/001", Year = 2024, WgGrade = "HEO" } };
        var tests  = new List<TestRequirement>   { new() { Project = "2024/001", Year = 2024, TestCode = "TC001" } };
        var animals = new List<AnimalRequirement>{ new() { Project = "2024/001", Year = 2024, AnimalType = "CAT" } };
        var costs  = new List<AdditionalCost>    { new() { Project = "2024/001", Year = 2024, AccountCat = "TRAVEL", Description = "T", CostEntered = 10 } };

        var (repo, _, _, _) = CreateRepository(
            projectYears: years,
            staffRequirements: staff,
            testRequirements: tests,
            animalRequirements: animals,
            additionalCosts: costs);

        var (deleted, errors) = await repo.DeleteProjectYearAsync("2024/001", 2024);

        Assert.False(deleted);
        Assert.Equal(4, errors.Count);
    }

    [Fact]
    public async Task DeleteProjectYearAsync_DecodesUrlEncodedProject()
    {
        var years = new List<ProjectYear>
        {
            new() { Project = "2024/001", YearValue = 2024 }
        };
        var (repo, pyDbSet, _, _) = CreateRepository(projectYears: years);

        var (deleted, errors) = await repo.DeleteProjectYearAsync("2024%2F001", 2024);

        Assert.True(deleted);
        Assert.Empty(errors);
        pyDbSet.Verify(x => x.Remove(It.IsAny<ProjectYear>()), Times.Once);
    }

    [Fact]
    public async Task DeleteProjectYearAsync_ErrorMessage_ContainsRemoveInstruction()
    {
        var years = new List<ProjectYear>
        {
            new() { Project = "2024/001", YearValue = 2024 }
        };
        var staff = new List<StaffRequirement>
        {
            new() { Project = "2024/001", Year = 2024, WgGrade = "HEO" }
        };
        var (repo, _, _, _) = CreateRepository(projectYears: years, staffRequirements: staff);

        var (_, errors) = await repo.DeleteProjectYearAsync("2024/001", 2024);

        Assert.Contains("Remove them first", errors[0]);
    }

    #endregion

    #region AddProjectYearAsync - additional cases

    [Fact]
    public async Task AddProjectYearAsync_NoMatchingProject_TreatsAsNonCommercial()
    {
        // No project entity at all — programme is null → not Comm → creates bare year
        var (repo, _, _, _) = CreateRepository();

        var result = await repo.AddProjectYearAsync("2024/001", 1, new ProjectYear());

        Assert.Equal("2024/001", result.Project);
        Assert.Equal(1, result.YearValue);
        Assert.Null(result.MarkupTime);
        Assert.Null(result.ProfitTime);
    }

    [Fact]
    public async Task AddProjectYearAsync_CommercialProject_NoPreviousYear_UnparsableSettings_ReturnsNullRates()
    {
        var projects = new List<Project>
        {
            new() { ProjectId = "2024/001", Programme = "Comm" }
        };
        var settings = new Dictionary<string, string?>
        {
            ["Profitstaff"]      = "not-a-number",
            ["Profittests"]      = null,
            ["ProfitAnimals"]    = "",
            ["ProfitExceptional"]= "not-a-number",
            ["Markupstaff"]      = null,
            ["Markuptests"]      = "",
            ["MarkupAnimals"]    = "not-a-number",
            ["MarkupExceptional"]= null
        };
        var (repo, _, _, _) = CreateRepository(projects: projects, settings: settings);

        var result = await repo.AddProjectYearAsync("2024/001", 1, new ProjectYear());

        Assert.Null(result.ProfitTime);
        Assert.Null(result.MarkupTime);
    }

    [Fact]
    public async Task AddProjectYearAsync_WithPartialRateData_UsesProvidedRates()
    {
        // Only MarkupTime is set — hasRateData becomes true, so all other rates stay null
        var projects = new List<Project>
        {
            new() { ProjectId = "2024/001", Programme = "Comm" }
        };
        var (repo, _, _, _) = CreateRepository(projects: projects);

        var yearData = new ProjectYear { MarkupTime = 99.0 };

        var result = await repo.AddProjectYearAsync("2024/001", 1, yearData);

        Assert.Equal(99.0, result.MarkupTime);
        Assert.Null(result.ProfitTime);
    }

    #endregion

    #region GetPayRatesAsync - additional cases

    [Fact]
    public async Task GetPayRatesAsync_ReturnsMultipleRecords()
    {
        var wggs = new List<WorkGroupGrade>
        {
            new() { WgGrade = "HEO", ProfitCentreGrade = "PCG1", GradeCode = "GC01", WorkGroup = "Sci", FpsYear = DefaultFpsYear },
            new() { WgGrade = "EO",  ProfitCentreGrade = "PCG2", GradeCode = "GC02", WorkGroup = "Adm", FpsYear = DefaultFpsYear }
        };
        var pcgs = new List<ProfitCentreGrade>
        {
            new() { PcGrade = "PCG1", ChargeRate = 45m, DefraChargeRate = 55m, PayRate = 30m, Npr = 5m, Ohr = 10m, FpsYear = DefaultFpsYear, DivisionGrade = "D1", GradeCode = "GC01", ProfitCentre = "PC1" },
            new() { PcGrade = "PCG2", ChargeRate = 30m, DefraChargeRate = 35m, PayRate = 20m, Npr = 3m, Ohr = 7m,  FpsYear = DefaultFpsYear, DivisionGrade = "D2", GradeCode = "GC02", ProfitCentre = "PC2" }
        };
        var (repo, _, _, _) = CreateRepository(workGroupGrades: wggs, profitCentreGrades: pcgs);

        var result = (await repo.GetPayRatesAsync("2024/001", 2024, isDefra: false)).ToList();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetPayRatesAsync_ReturnsEmpty_WhenNoMatchingJoin()
    {
        var wggs = new List<WorkGroupGrade>
        {
            new() { WgGrade = "HEO", ProfitCentreGrade = "PCG_NONE", GradeCode = "GC01", WorkGroup = "Sci", FpsYear = DefaultFpsYear }
        };
        var pcgs = new List<ProfitCentreGrade>
        {
            new() { PcGrade = "PCG_OTHER", ChargeRate = 45m, DefraChargeRate = 55m, PayRate = 30m, Npr = 5m, Ohr = 10m, FpsYear = DefaultFpsYear, DivisionGrade = "D1", GradeCode = "GC01", ProfitCentre = "PC1" }
        };
        var (repo, _, _, _) = CreateRepository(workGroupGrades: wggs, profitCentreGrades: pcgs);

        var result = (await repo.GetPayRatesAsync("2024/001", 2024, isDefra: false)).ToList();

        Assert.Empty(result);
    }

    #endregion
}
