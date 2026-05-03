using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Pagination;
using Apha.Costbook.Application.Services;
using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.Core.Pagination;
using AutoMapper;
using NSubstitute;

namespace Apha.Costbook.Application.UnitTests.Services.YearlyDetailsServiceTest;

public class YearlyDetailsServiceTests
{
    private readonly IProjectRepository _projectRepo;
    private readonly IProjectYearRepository _projectYearRepo;
    private readonly IStaffRequirementRepository _staffRepo;
    private readonly ITestRequirementRepository _testRepo;
    private readonly IAnimalRequirementRepository _animalRepo;
    private readonly IAdditionalCostRepository _additionalCostRepo;
    private readonly IMapper _mapper;
    private readonly YearlyDetailsService _sut;

    public YearlyDetailsServiceTests()
    {
        _projectRepo = Substitute.For<IProjectRepository>();
        _projectYearRepo = Substitute.For<IProjectYearRepository>();
        _staffRepo = Substitute.For<IStaffRequirementRepository>();
        _testRepo = Substitute.For<ITestRequirementRepository>();
        _animalRepo = Substitute.For<IAnimalRequirementRepository>();
        _additionalCostRepo = Substitute.For<IAdditionalCostRepository>();
        _mapper = Substitute.For<IMapper>();

        _sut = new YearlyDetailsService(
            _projectRepo, _projectYearRepo, _staffRepo,
            _testRepo, _animalRepo, _additionalCostRepo, _mapper);
    }

    #region GetProjectHeaderAsync

    [Fact]
    public async Task GetProjectHeaderAsync_ReturnsNull_WhenProjectNotFound()
    {
        // Arrange
        _projectRepo.GetProjectByIdAsync("NOTFOUND").Returns((Project?)null);

        // Act
        var result = await _sut.GetProjectHeaderAsync("NOTFOUND");

        // Assert
        Assert.Null(result);
        await _projectRepo.Received(1).GetProjectByIdAsync("NOTFOUND");
    }

    [Fact]
    public async Task GetProjectHeaderAsync_ReturnsMappedDto_WhenProjectExists()
    {
        // Arrange
        var project = new Project { ProjectId = "2024/001", ProjectTitle = "Test" };
        var dto = new ProjectHeaderDto { ProjectId = "2024/001", ProjectTitle = "Test" };

        _projectRepo.GetProjectByIdAsync("2024/001").Returns(project);
        _mapper.Map<ProjectHeaderDto>(project).Returns(dto);

        // Act
        var result = await _sut.GetProjectHeaderAsync("2024/001");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("2024/001", result.ProjectId);
        _mapper.Received(1).Map<ProjectHeaderDto>(project);
    }

    #endregion

    #region GetProjectYearsAsync

    [Fact]
    public async Task GetProjectYearsAsync_ReturnsMappedDtos()
    {
        // Arrange
        var years = new List<ProjectYear> { new() { Project = "2024/001", YearValue = 1 } };
        var dtos = new List<ProjectYearDto> { new() { Project = "2024/001", YearValue = 1 } };

        _projectYearRepo.GetByProjectAsync("2024/001").Returns(years);
        _mapper.Map<IEnumerable<ProjectYearDto>>(years).Returns(dtos);

        // Act
        var result = await _sut.GetProjectYearsAsync("2024/001");

        // Assert
        Assert.Single(result);
        await _projectYearRepo.Received(1).GetByProjectAsync("2024/001");
    }

    #endregion

    #region AddProjectYearAsync

    [Fact]
    public async Task AddProjectYearAsync_MapsAndCallsRepo_ReturnsMappedDto()
    {
        // Arrange
        var dto = new ProjectYearDto { Project = "2024/001", YearValue = 2 };
        var entity = new ProjectYear { Project = "2024/001", YearValue = 2 };
        var added = new ProjectYear { Project = "2024/001", YearValue = 2 };
        var resultDto = new ProjectYearDto { Project = "2024/001", YearValue = 2 };

        _mapper.Map<ProjectYear>(dto).Returns(entity);
        _projectYearRepo.AddProjectYearAsync("2024/001", 2, entity).Returns(added);
        _mapper.Map<ProjectYearDto>(added).Returns(resultDto);

        // Act
        var result = await _sut.AddProjectYearAsync("2024/001", 2, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.YearValue);
        await _projectYearRepo.Received(1).AddProjectYearAsync("2024/001", 2, entity);
    }

    #endregion

    #region UpdateProjectYearAsync

    [Fact]
    public async Task UpdateProjectYearAsync_MapsAndCallsRepo_ReturnsMappedDto()
    {
        // Arrange
        var dto = new ProjectYearDto { Project = "2024/001", YearValue = 1 };
        var entity = new ProjectYear { Project = "2024/001", YearValue = 1 };
        var updated = new ProjectYear { Project = "2024/001", YearValue = 1 };
        var resultDto = new ProjectYearDto { Project = "2024/001", YearValue = 1 };

        _mapper.Map<ProjectYear>(dto).Returns(entity);
        _projectYearRepo.UpdateProjectYearAsync(entity).Returns(updated);
        _mapper.Map<ProjectYearDto>(updated).Returns(resultDto);

        // Act
        var result = await _sut.UpdateProjectYearAsync(dto);

        // Assert
        Assert.NotNull(result);
        await _projectYearRepo.Received(1).UpdateProjectYearAsync(entity);
    }

    #endregion

    #region GetStaffRequirementsAsync

    [Fact]
    public async Task GetStaffRequirementsAsync_ReturnsPaginatedResult_WithStaffCostCalculated()
    {
        // Arrange
        var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
        var filter = new PaginationParameters<string> { Page = 1, PageSize = 10 };
        var repoResult = new PagedData<StaffRequirementDetailView>
        {
            Data = new List<StaffRequirementDetailView>
            {
                new() { SrIdentity = 1, Project = "2024/001", Year = 2024, WgGrade = "HEO", Chargerate = 50.0, Nohours = 100.0 }
            },
            PaginationData = new PaginationData { TotalRecords = 1, PageNumber = 1, PageSize = 10, TotalPages = 1 }
        };
        var paginationDto = new PaginationDto { TotalRecords = 1, PageNumber = 1, PageSize = 10, TotalPages = 1 };

        _mapper.Map<PaginationParameters<string>>(query).Returns(filter);
        _staffRepo.GetStaffRequirementsByProjectYearAsync("2024/001", 2024, filter).Returns(repoResult);
        _mapper.Map<PaginationDto>(repoResult.PaginationData).Returns(paginationDto);

        // Act
        var result = await _sut.GetStaffRequirementsAsync("2024/001", 2024, query);

        // Assert
        Assert.NotNull(result);
        var staff = result.Data.First();
        Assert.Equal(1, staff.SrIdentity);
        Assert.Equal(5000.0, staff.StaffCost); // 50 * 100
        Assert.Equal(1, result.PaginationData.TotalRecords);
    }

    [Fact]
    public async Task GetStaffRequirementsAsync_StaffCostIsNull_WhenChargerateOrNohoursIsNull()
    {
        // Arrange
        var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
        var filter = new PaginationParameters<string> { Page = 1, PageSize = 10 };
        var repoResult = new PagedData<StaffRequirementDetailView>
        {
            Data = new List<StaffRequirementDetailView>
            {
                new() { SrIdentity = 1, Chargerate = null, Nohours = 100.0 }
            },
            PaginationData = new PaginationData()
        };

        _mapper.Map<PaginationParameters<string>>(query).Returns(filter);
        _staffRepo.GetStaffRequirementsByProjectYearAsync(Arg.Any<string>(), Arg.Any<int>(), filter).Returns(repoResult);
        _mapper.Map<PaginationDto>(Arg.Any<PaginationData>()).Returns(new PaginationDto());

        // Act
        var result = await _sut.GetStaffRequirementsAsync("2024/001", 2024, query);

        // Assert
        Assert.Null(result.Data.First().StaffCost);
    }

    #endregion

    #region AddStaffRequirementAsync

    [Fact]
    public async Task AddStaffRequirementAsync_MapsAndCallsRepo_ReturnsDto()
    {
        // Arrange
        var dto = new StaffRequirementDto { WgGrade = "HEO", Chargerate = 50, Nohours = 100 };
        var entity = new StaffRequirement { WgGrade = "HEO", Chargerate = 50, Nohours = 100 };

        _mapper.Map<StaffRequirement>(dto).Returns(entity);
        _staffRepo.AddStaffRequirementAsync(entity).Returns(entity);

        // Act
        var result = await _sut.AddStaffRequirementAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("HEO", result.WgGrade);
        Assert.Equal(5000.0, result.StaffCost);
        await _staffRepo.Received(1).AddStaffRequirementAsync(entity);
    }

    #endregion

    #region UpdateStaffRequirementAsync

    [Fact]
    public async Task UpdateStaffRequirementAsync_MapsAndCallsRepo_ReturnsDto()
    {
        // Arrange
        var dto = new StaffRequirementDto { SrIdentity = 1, WgGrade = "HEO" };
        var entity = new StaffRequirement { SrIdentity = 1, WgGrade = "HEO" };

        _mapper.Map<StaffRequirement>(dto).Returns(entity);
        _staffRepo.UpdateStaffRequirementAsync(entity).Returns(entity);

        // Act
        var result = await _sut.UpdateStaffRequirementAsync(dto);

        // Assert
        Assert.NotNull(result);
        await _staffRepo.Received(1).UpdateStaffRequirementAsync(entity);
    }

    #endregion

    #region DeleteStaffRequirementAsync

    [Fact]
    public async Task DeleteStaffRequirementAsync_ReturnsTrue_WhenDeleted()
    {
        _staffRepo.DeleteStaffRequirementAsync(1).Returns(true);

        var result = await _sut.DeleteStaffRequirementAsync(1);

        Assert.True(result);
        await _staffRepo.Received(1).DeleteStaffRequirementAsync(1);
    }

    [Fact]
    public async Task DeleteStaffRequirementAsync_ReturnsFalse_WhenNotFound()
    {
        _staffRepo.DeleteStaffRequirementAsync(999).Returns(false);

        var result = await _sut.DeleteStaffRequirementAsync(999);

        Assert.False(result);
    }

    #endregion

    #region GetTestRequirementsAsync

    [Fact]
    public async Task GetTestRequirementsAsync_MapsFieldsCorrectly()
    {
        // Arrange
        var rows = new List<TestRequirementDetailView>
        {
            new() { Project = "2024/001", Year = 2024, TestCode = "TC001", UnitPrice = 100, NumberOfTests = 5, TestCost = 500, TestDescription = "Blood Test" }
        };
        _testRepo.GetTestRequirementsByProjectYearAsync("2024/001", 2024).Returns(rows);

        // Act
        var result = (await _sut.GetTestRequirementsAsync("2024/001", 2024)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("TC001", result[0].TestCode);
        Assert.Equal(500.0, result[0].TestCost);
        Assert.Equal("Blood Test", result[0].TestDescription);
    }

    [Fact]
    public async Task GetTestRequirementsAsync_ReturnsEmptyList_WhenNoData()
    {
        _testRepo.GetTestRequirementsByProjectYearAsync("2024/001", 2024).Returns(new List<TestRequirementDetailView>());

        var result = await _sut.GetTestRequirementsAsync("2024/001", 2024);

        Assert.Empty(result);
    }

    #endregion

    #region AddTestRequirementAsync

    [Fact]
    public async Task AddTestRequirementAsync_MapsAndCallsRepo_ReturnsDtoWithTestCost()
    {
        var dto = new TestRequirementDto { TestCode = "TC001", UnitPrice = 100, NumberOfTests = 5 };
        var entity = new TestRequirement { TestCode = "TC001", UnitPrice = 100, NumberOfTests = 5 };

        _mapper.Map<TestRequirement>(dto).Returns(entity);
        _testRepo.AddTestRequirementAsync(entity).Returns(entity);

        var result = await _sut.AddTestRequirementAsync(dto);

        Assert.Equal("TC001", result.TestCode);
        Assert.Equal(500.0, result.TestCost); // 100 * 5
        await _testRepo.Received(1).AddTestRequirementAsync(entity);
    }

    #endregion

    #region UpdateTestRequirementAsync

    [Fact]
    public async Task UpdateTestRequirementAsync_MapsAndCallsRepo()
    {
        var dto = new TestRequirementDto { TestCode = "TC001" };
        var entity = new TestRequirement { TestCode = "TC001" };

        _mapper.Map<TestRequirement>(dto).Returns(entity);
        _testRepo.UpdateTestRequirementAsync(entity).Returns(entity);

        var result = await _sut.UpdateTestRequirementAsync(dto);

        Assert.Equal("TC001", result.TestCode);
        await _testRepo.Received(1).UpdateTestRequirementAsync(entity);
    }

    #endregion

    #region DeleteTestRequirementAsync

    [Fact]
    public async Task DeleteTestRequirementAsync_DelegatesToRepo()
    {
        _testRepo.DeleteTestRequirementAsync("2024/001", 2024, "TC001").Returns(true);

        var result = await _sut.DeleteTestRequirementAsync("2024/001", 2024, "TC001");

        Assert.True(result);
        await _testRepo.Received(1).DeleteTestRequirementAsync("2024/001", 2024, "TC001");
    }

    #endregion

    #region GetAnimalRequirementsAsync

    [Fact]
    public async Task GetAnimalRequirementsAsync_MapsFieldsCorrectly()
    {
        var rows = new List<AnimalRequirementDetailView>
        {
            new() { ArIdentity = 1, Project = "2024/001", Year = 2024, AnimalType = "CAT", NumberOfDays = 5, NumberOfAnimals = 3, DailyRate = 10, AnimalCost = 150 }
        };
        _animalRepo.GetAnimalRequirementsByProjectYearAsync("2024/001", 2024).Returns(rows);

        var result = (await _sut.GetAnimalRequirementsAsync("2024/001", 2024)).ToList();

        Assert.Single(result);
        Assert.Equal("CAT", result[0].AnimalType);
        Assert.Equal(150.0, result[0].AnimalCost);
    }

    #endregion

    #region AddAnimalRequirementAsync

    [Fact]
    public async Task AddAnimalRequirementAsync_MapsAndCallsRepo_ReturnsDtoWithAnimalCost()
    {
        var dto = new AnimalRequirementDto { AnimalType = "CAT" };
        var entity = new AnimalRequirement { AnimalType = "CAT", NumberOfDays = 5, NumberOfAnimals = 3, DailyRate = 10 };

        _mapper.Map<AnimalRequirement>(dto).Returns(entity);
        _animalRepo.AddAnimalRequirementAsync(entity).Returns(entity);

        var result = await _sut.AddAnimalRequirementAsync(dto);

        Assert.Equal("CAT", result.AnimalType);
        Assert.Equal(150.0, result.AnimalCost); // 5 * 3 * 10
        await _animalRepo.Received(1).AddAnimalRequirementAsync(entity);
    }

    [Fact]
    public async Task AddAnimalRequirementAsync_AnimalCostIsNull_WhenAnyFactorIsNull()
    {
        var dto = new AnimalRequirementDto();
        var entity = new AnimalRequirement { NumberOfDays = null, NumberOfAnimals = 3, DailyRate = 10 };

        _mapper.Map<AnimalRequirement>(dto).Returns(entity);
        _animalRepo.AddAnimalRequirementAsync(entity).Returns(entity);

        var result = await _sut.AddAnimalRequirementAsync(dto);

        Assert.Null(result.AnimalCost);
    }

    #endregion

    #region UpdateAnimalRequirementAsync

    [Fact]
    public async Task UpdateAnimalRequirementAsync_MapsAndCallsRepo()
    {
        var dto = new AnimalRequirementDto { ArIdentity = 1, AnimalType = "DOG" };
        var entity = new AnimalRequirement { ArIdentity = 1, AnimalType = "DOG" };

        _mapper.Map<AnimalRequirement>(dto).Returns(entity);
        _animalRepo.UpdateAnimalRequirementAsync(entity).Returns(entity);

        var result = await _sut.UpdateAnimalRequirementAsync(dto);

        Assert.Equal("DOG", result.AnimalType);
        await _animalRepo.Received(1).UpdateAnimalRequirementAsync(entity);
    }

    #endregion

    #region DeleteAnimalRequirementAsync

    [Fact]
    public async Task DeleteAnimalRequirementAsync_DelegatesToRepo()
    {
        _animalRepo.DeleteAnimalRequirementAsync(1).Returns(true);

        var result = await _sut.DeleteAnimalRequirementAsync(1);

        Assert.True(result);
        await _animalRepo.Received(1).DeleteAnimalRequirementAsync(1);
    }

    #endregion

    #region GetAdditionalCostsAsync

    [Fact]
    public async Task GetAdditionalCostsAsync_MapsFieldsCorrectly()
    {
        var rows = new List<AdditionalCostDetailView>
        {
            new() { AcIdentity = 1, Project = "2024/001", Year = 2024, AccountCat = "TRAVEL", Description = "Travel", ItemCost = 500, CostEntered = 500, Freq = "Annual" }
        };
        _additionalCostRepo.GetAdditionalCostsByProjectYearAsync("2024/001", 2024).Returns(rows);

        var result = (await _sut.GetAdditionalCostsAsync("2024/001", 2024)).ToList();

        Assert.Single(result);
        Assert.Equal("TRAVEL", result[0].AccountCat);
        Assert.Equal(500.0, result[0].CostEntered);
    }

    #endregion

    #region AddAdditionalCostAsync

    [Fact]
    public async Task AddAdditionalCostAsync_MapsAndCallsRepo()
    {
        var dto = new AdditionalCostDto { Description = "Travel" };
        var entity = new AdditionalCost { Description = "Travel" };
        var resultDto = new AdditionalCostDto { Description = "Travel" };

        _mapper.Map<AdditionalCost>(dto).Returns(entity);
        _additionalCostRepo.AddAdditionalCostAsync(entity).Returns(entity);
        _mapper.Map<AdditionalCostDto>(entity).Returns(resultDto);

        var result = await _sut.AddAdditionalCostAsync(dto);

        Assert.Equal("Travel", result.Description);
        await _additionalCostRepo.Received(1).AddAdditionalCostAsync(entity);
    }

    #endregion

    #region UpdateAdditionalCostAsync

    [Fact]
    public async Task UpdateAdditionalCostAsync_MapsAndCallsRepo()
    {
        var dto = new AdditionalCostDto { AcIdentity = 1 };
        var entity = new AdditionalCost { AcIdentity = 1 };
        var resultDto = new AdditionalCostDto { AcIdentity = 1 };

        _mapper.Map<AdditionalCost>(dto).Returns(entity);
        _additionalCostRepo.UpdateAdditionalCostAsync(entity).Returns(entity);
        _mapper.Map<AdditionalCostDto>(entity).Returns(resultDto);

        var result = await _sut.UpdateAdditionalCostAsync(dto);

        Assert.NotNull(result);
        await _additionalCostRepo.Received(1).UpdateAdditionalCostAsync(entity);
    }

    #endregion

    #region DeleteAdditionalCostAsync

    [Fact]
    public async Task DeleteAdditionalCostAsync_DelegatesToRepo()
    {
        _additionalCostRepo.DeleteAdditionalCostAsync(1).Returns(true);

        var result = await _sut.DeleteAdditionalCostAsync(1);

        Assert.True(result);
        await _additionalCostRepo.Received(1).DeleteAdditionalCostAsync(1);
    }

    #endregion

    #region GetPayRatesAsync

    [Fact]
    public async Task GetPayRatesAsync_MapsFieldsCorrectly()
    {
        var rates = new List<PayRateLookup>
        {
            new("HEO", 45.50, 30.0, 5.0, 10.0)
        };
        _projectYearRepo.GetPayRatesAsync(false).Returns(rates);

        var result = (await _sut.GetPayRatesAsync(false)).ToList();

        Assert.Single(result);
        Assert.Equal("HEO", result[0].WgGrade);
        Assert.Equal(45.50, result[0].ChargeRate);
        Assert.Equal(30.0, result[0].PayRate);
    }

    #endregion

    #region GetAnimalRatesAsync

    [Fact]
    public async Task GetAnimalRatesAsync_MapsFieldsCorrectly()
    {
        var rates = new List<AnimalRateLookup>
        {
            new("CAT", 10.50)
        };
        _animalRepo.GetAnimalRatesAsync(true).Returns(rates);

        var result = (await _sut.GetAnimalRatesAsync(true)).ToList();

        Assert.Single(result);
        Assert.Equal("CAT", result[0].AnimalType);
        Assert.Equal(10.50, result[0].DailyRate);
    }

    #endregion

    #region GetAccountCategoriesAsync

    [Fact]
    public async Task GetAccountCategoriesAsync_MapsFieldsCorrectly()
    {
        var cats = new List<AccountCategoryLookup>
        {
            new("TRAVEL", true)
        };
        _additionalCostRepo.GetProjectSpecificAccountCategoriesAsync().Returns(cats);

        var result = (await _sut.GetAccountCategoriesAsync()).ToList();

        Assert.Single(result);
        Assert.Equal("TRAVEL", result[0].AccShortName);
        Assert.True(result[0].UseInflation);
    }

    #endregion

    #region GetTestCodeLookupsAsync

    [Fact]
    public async Task GetTestCodeLookupsAsync_MapsFieldsCorrectly()
    {
        var lookups = new List<TestCodeLookup>
        {
            new("TC001", "Blood Test", 100m)
        };
        _testRepo.GetTestCodeLookupsAsync(false).Returns(lookups);

        var result = (await _sut.GetTestCodeLookupsAsync(false)).ToList();

        Assert.Single(result);
        Assert.Equal("TC001", result[0].ItemCode);
        Assert.Equal("Blood Test", result[0].ItemDescription);
        Assert.Equal(100m, result[0].UnitPrice);
    }

    #endregion

    #region GetAllAnimalsAsync

    [Fact]
    public async Task GetAllAnimalsAsync_MapsFieldsCorrectly()
    {
        var animals = new List<FpsAnimals>
        {
            new() { AnimalType = "CAT", Species = "Felis", SecurityLevel = "Low", DailyRate = 10m, PlanByWeek = true, DefraDailyRate = 15m }
        };
        _animalRepo.GetAllAnimalsAsync().Returns(animals);

        var result = (await _sut.GetAllAnimalsAsync()).ToList();

        Assert.Single(result);
        Assert.Equal("CAT", result[0].AnimalType);
        Assert.Equal("Felis", result[0].Species);
        Assert.Equal("Low", result[0].SecurityLevel);
        Assert.Equal(10m, result[0].DailyRate);
        Assert.True(result[0].PlanByWeek);
        Assert.Equal(15m, result[0].DefraDailyRate);
    }

    #endregion
}
