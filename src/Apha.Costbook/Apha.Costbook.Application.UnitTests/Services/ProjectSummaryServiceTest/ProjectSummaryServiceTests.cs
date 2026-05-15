using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Pagination;
using Apha.Costbook.Application.Services;
using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.Core.Pagination;
using AutoMapper;
using NSubstitute;

namespace Apha.Costbook.Application.UnitTests.Services.ProjectSummaryServiceTest;

public class ProjectSummaryServiceTests
{
    private readonly IProjectRepository _mockRepository;
    private readonly IMapper _mockMapper;
    private readonly ProjectSummaryService _service;

    public ProjectSummaryServiceTests()
    {
        _mockRepository = Substitute.For<IProjectRepository>();
        _mockMapper = Substitute.For<IMapper>();
        _service = new ProjectSummaryService(_mockRepository, _mockMapper);
    }

    #region GetProfitIncludedTotalAsync Tests

    [Fact]
    public async Task GetProfitIncludedTotalAsync_ValidParameters_ReturnsRepositoryResult()
    {
        // Arrange
        var projectId = "P001";
        var year = 2024;
        var expected = 12345.67;

        _mockRepository.GetProfitIncludedTotalAsync(projectId, year).Returns(expected);

        // Act
        var result = await _service.GetProfitIncludedTotalAsync(projectId, year);

        // Assert
        Assert.Equal(expected, result);
        await _mockRepository.Received(1).GetProfitIncludedTotalAsync(projectId, year);
    }

    [Fact]
    public async Task GetProfitIncludedTotalAsync_ReturnsZero_WhenRepositoryReturnsZero()
    {
        // Arrange
        var projectId = "P001";
        var year = 2024;

        _mockRepository.GetProfitIncludedTotalAsync(projectId, year).Returns(0.0);

        // Act
        var result = await _service.GetProfitIncludedTotalAsync(projectId, year);

        // Assert
        Assert.Equal(0.0, result);
        await _mockRepository.Received(1).GetProfitIncludedTotalAsync(projectId, year);
    }

    [Theory]
    [InlineData("P001", 2023, 5000.0)]
    [InlineData("P002", 2024, 9999.99)]
    [InlineData("P003", 2025, 0.01)]
    public async Task GetProfitIncludedTotalAsync_DifferentParameters_ReturnsCorrectValue(
        string projectId, int year, double expectedTotal)
    {
        // Arrange
        _mockRepository.GetProfitIncludedTotalAsync(projectId, year).Returns(expectedTotal);

        // Act
        var result = await _service.GetProfitIncludedTotalAsync(projectId, year);

        // Assert
        Assert.Equal(expectedTotal, result);
        await _mockRepository.Received(1).GetProfitIncludedTotalAsync(projectId, year);
    }

    #endregion

    #region GetStaffYearsPivotAsync Tests

    [Fact]
    public async Task GetStaffYearsPivotAsync_WithQuery_MapsParametersAndReturnsDto()
    {
        // Arrange
        var projectId = "P001";
        var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
        var paginationParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
        var pivotData = new StaffYearsPivotData
        {
            Years = [2023, 2024],
            TotalCount = 2,
            Rows =
            [
                new StaffYearsRowData
                {
                    Project = "P001",
                    Grade = "Grade A",
                    Total = 1000.0,
                    YearlyAmounts = new Dictionary<int, double> { { 2023, 400.0 }, { 2024, 600.0 } }
                },
                new StaffYearsRowData
                {
                    Project = "P001",
                    Grade = "Grade B",
                    Total = 2000.0,
                    YearlyAmounts = new Dictionary<int, double> { { 2023, 900.0 }, { 2024, 1100.0 } }
                }
            ]
        };

        _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
        _mockRepository.GetStaffYearsPivotAsync(projectId, paginationParams).Returns(pivotData);

        // Act
        var result = await _service.GetStaffYearsPivotAsync(projectId, query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal([2023, 2024], result.Years);
        Assert.Equal(2, result.Rows.Count);
        Assert.Equal("Grade A", result.Rows[0].Grade);
        Assert.Equal(1000.0, result.Rows[0].Total);
        Assert.Equal(400.0, result.Rows[0].YearlyAmounts[2023]);
        _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
        await _mockRepository.Received(1).GetStaffYearsPivotAsync(projectId, paginationParams);
    }

    [Fact]
    public async Task GetStaffYearsPivotAsync_WithNullQuery_PassesNullParametersToRepository()
    {
        // Arrange
        var projectId = "P001";
        var pivotData = new StaffYearsPivotData
        {
            Years = [2024],
            TotalCount = 1,
            Rows =
            [
                new StaffYearsRowData
                {
                    Project = "P001",
                    Grade = "Grade A",
                    Total = 500.0,
                    YearlyAmounts = new Dictionary<int, double> { { 2024, 500.0 } }
                }
            ]
        };

        _mockRepository.GetStaffYearsPivotAsync(projectId, null).Returns(pivotData);

        // Act
        var result = await _service.GetStaffYearsPivotAsync(projectId, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Rows);
        _mockMapper.DidNotReceive().Map<PaginationParameters<string>>(Arg.Any<QueryParameters<string>>());
        await _mockRepository.Received(1).GetStaffYearsPivotAsync(projectId, null);
    }

    [Fact]
    public async Task GetStaffYearsPivotAsync_EmptyRows_ReturnsEmptyRowsDto()
    {
        // Arrange
        var projectId = "P001";
        var pivotData = new StaffYearsPivotData
        {
            Years = [],
            TotalCount = 0,
            Rows = []
        };

        _mockRepository.GetStaffYearsPivotAsync(projectId, null).Returns(pivotData);

        // Act
        var result = await _service.GetStaffYearsPivotAsync(projectId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Years);
        Assert.Empty(result.Rows);
        await _mockRepository.Received(1).GetStaffYearsPivotAsync(projectId, null);
    }

    [Fact]
    public async Task GetStaffYearsPivotAsync_MapsAllRowProperties_Correctly()
    {
        // Arrange
        var projectId = "P001";
        var pivotData = new StaffYearsPivotData
        {
            Years = [2024],
            TotalCount = 1,
            Rows =
            [
                new StaffYearsRowData
                {
                    Project = "P001",
                    Grade = "Senior",
                    Total = 750.5,
                    YearlyAmounts = new Dictionary<int, double> { { 2024, 750.5 } }
                }
            ]
        };

        _mockRepository.GetStaffYearsPivotAsync(projectId, null).Returns(pivotData);

        // Act
        var result = await _service.GetStaffYearsPivotAsync(projectId);

        // Assert
        var row = Assert.Single(result.Rows);
        Assert.Equal("P001", row.Project);
        Assert.Equal("Senior", row.Grade);
        Assert.Equal(750.5, row.Total);
        Assert.Equal(750.5, row.YearlyAmounts[2024]);
    }

    #endregion

    #region GetStaffEffortAsync Tests

    [Fact]
    public async Task GetStaffEffortAsync_WithQuery_MapsParametersAndReturnsDto()
    {
        // Arrange
        var projectId = "P001";
        var query = new QueryParameters<string> { Page = 1, PageSize = 5 };
        var paginationParams = new PaginationParameters<string> { Page = 1, PageSize = 5 };
        var pivotData = new StaffEffortPivotData
        {
            Years = [2023, 2024],
            TotalCount = 1,
            Rows =
            [
                new StaffEffortRowData
                {
                    Project = "P001",
                    WorkGroup = "WG1",
                    GradeCode = "GC1",
                    Name = "John Doe",
                    Total = 300.0,
                    YearlyAmounts = new Dictionary<int, double> { { 2023, 100.0 }, { 2024, 200.0 } }
                }
            ]
        };

        _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
        _mockRepository.GetStaffEffortAsync(projectId, paginationParams).Returns(pivotData);

        // Act
        var result = await _service.GetStaffEffortAsync(projectId, query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TotalCount);
        Assert.Equal([2023, 2024], result.Years);
        Assert.Single(result.Rows);
        _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
        await _mockRepository.Received(1).GetStaffEffortAsync(projectId, paginationParams);
    }

    [Fact]
    public async Task GetStaffEffortAsync_WithNullQuery_PassesNullParametersToRepository()
    {
        // Arrange
        var projectId = "P001";
        var pivotData = new StaffEffortPivotData
        {
            Years = [2024],
            TotalCount = 1,
            Rows =
            [
                new StaffEffortRowData
                {
                    Project = "P001",
                    WorkGroup = "WG1",
                    GradeCode = "GC1",
                    Name = "Jane Doe",
                    Total = 150.0,
                    YearlyAmounts = new Dictionary<int, double> { { 2024, 150.0 } }
                }
            ]
        };

        _mockRepository.GetStaffEffortAsync(projectId, null).Returns(pivotData);

        // Act
        var result = await _service.GetStaffEffortAsync(projectId, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TotalCount);
        _mockMapper.DidNotReceive().Map<PaginationParameters<string>>(Arg.Any<QueryParameters<string>>());
        await _mockRepository.Received(1).GetStaffEffortAsync(projectId, null);
    }

    [Fact]
    public async Task GetStaffEffortAsync_EmptyRows_ReturnsEmptyDto()
    {
        // Arrange
        var projectId = "P001";
        var pivotData = new StaffEffortPivotData
        {
            Years = [],
            TotalCount = 0,
            Rows = []
        };

        _mockRepository.GetStaffEffortAsync(projectId, null).Returns(pivotData);

        // Act
        var result = await _service.GetStaffEffortAsync(projectId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Years);
        Assert.Empty(result.Rows);
    }

    [Fact]
    public async Task GetStaffEffortAsync_MapsAllRowProperties_Correctly()
    {
        // Arrange
        var projectId = "P001";
        var pivotData = new StaffEffortPivotData
        {
            Years = [2024],
            TotalCount = 1,
            Rows =
            [
                new StaffEffortRowData
                {
                    Project = "P001",
                    WorkGroup = "Engineering",
                    GradeCode = "E1",
                    Name = "Alice Smith",
                    Total = 450.0,
                    YearlyAmounts = new Dictionary<int, double> { { 2024, 450.0 } }
                }
            ]
        };

        _mockRepository.GetStaffEffortAsync(projectId, null).Returns(pivotData);

        // Act
        var result = await _service.GetStaffEffortAsync(projectId);

        // Assert
        var row = Assert.Single(result.Rows);
        Assert.Equal("P001", row.Project);
        Assert.Equal("Engineering", row.WorkGroup);
        Assert.Equal("E1", row.GradeCode);
        Assert.Equal("Alice Smith", row.Name);
        Assert.Equal(450.0, row.Total);
        Assert.Equal(450.0, row.YearlyAmounts[2024]);
    }

    #endregion

    #region GetProjectCostsPivotAsync Tests

    [Fact]
    public async Task GetProjectCostsPivotAsync_WithQuery_MapsParametersAndReturnsDto()
    {
        // Arrange
        var projectId = "P001";
        var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
        var paginationParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
        var pivotData = new ProjectCostsPivotData
        {
            Years = [2023, 2024],
            TotalCount = 2,
            Rows =
            [
                new ProjectCostsRowData
                {
                    Project = "P001",
                    Category = "Travel",
                    Total = 500.0,
                    YearlyAmounts = new Dictionary<int, double> { { 2023, 200.0 }, { 2024, 300.0 } }
                },
                new ProjectCostsRowData
                {
                    Project = "P001",
                    Category = "Equipment",
                    Total = 1500.0,
                    YearlyAmounts = new Dictionary<int, double> { { 2023, 700.0 }, { 2024, 800.0 } }
                }
            ]
        };

        _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
        _mockRepository.GetProjectCostsPivotAsync(projectId, paginationParams).Returns(pivotData);

        // Act
        var result = await _service.GetProjectCostsPivotAsync(projectId, query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal([2023, 2024], result.Years);
        Assert.Equal(2, result.Rows.Count);
        Assert.Equal("Travel", result.Rows[0].Category);
        Assert.Equal(500.0, result.Rows[0].Total);
        _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
        await _mockRepository.Received(1).GetProjectCostsPivotAsync(projectId, paginationParams);
    }

    [Fact]
    public async Task GetProjectCostsPivotAsync_WithNullQuery_PassesNullParametersToRepository()
    {
        // Arrange
        var projectId = "P001";
        var pivotData = new ProjectCostsPivotData
        {
            Years = [2024],
            TotalCount = 1,
            Rows =
            [
                new ProjectCostsRowData
                {
                    Project = "P001",
                    Category = "Consumables",
                    Total = 250.0,
                    YearlyAmounts = new Dictionary<int, double> { { 2024, 250.0 } }
                }
            ]
        };

        _mockRepository.GetProjectCostsPivotAsync(projectId, null).Returns(pivotData);

        // Act
        var result = await _service.GetProjectCostsPivotAsync(projectId, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TotalCount);
        _mockMapper.DidNotReceive().Map<PaginationParameters<string>>(Arg.Any<QueryParameters<string>>());
        await _mockRepository.Received(1).GetProjectCostsPivotAsync(projectId, null);
    }

    [Fact]
    public async Task GetProjectCostsPivotAsync_EmptyRows_ReturnsEmptyDto()
    {
        // Arrange
        var projectId = "P001";
        var pivotData = new ProjectCostsPivotData
        {
            Years = [],
            TotalCount = 0,
            Rows = []
        };

        _mockRepository.GetProjectCostsPivotAsync(projectId, null).Returns(pivotData);

        // Act
        var result = await _service.GetProjectCostsPivotAsync(projectId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Years);
        Assert.Empty(result.Rows);
    }

    [Fact]
    public async Task GetProjectCostsPivotAsync_MapsAllRowProperties_Correctly()
    {
        // Arrange
        var projectId = "P001";
        var pivotData = new ProjectCostsPivotData
        {
            Years = [2024],
            TotalCount = 1,
            Rows =
            [
                new ProjectCostsRowData
                {
                    Project = "P001",
                    Category = "Lab Costs",
                    Total = 3200.0,
                    YearlyAmounts = new Dictionary<int, double> { { 2024, 3200.0 } }
                }
            ]
        };

        _mockRepository.GetProjectCostsPivotAsync(projectId, null).Returns(pivotData);

        // Act
        var result = await _service.GetProjectCostsPivotAsync(projectId);

        // Assert
        var row = Assert.Single(result.Rows);
        Assert.Equal("P001", row.Project);
        Assert.Equal("Lab Costs", row.Category);
        Assert.Equal(3200.0, row.Total);
        Assert.Equal(3200.0, row.YearlyAmounts[2024]);
    }

    [Fact]
    public async Task GetProjectCostsPivotAsync_MultipleYears_YearlyAmountsCorrectlyMapped()
    {
        // Arrange
        var projectId = "P001";
        var pivotData = new ProjectCostsPivotData
        {
            Years = [2022, 2023, 2024],
            TotalCount = 1,
            Rows =
            [
                new ProjectCostsRowData
                {
                    Project = "P001",
                    Category = "Staff",
                    Total = 6000.0,
                    YearlyAmounts = new Dictionary<int, double>
                    {
                        { 2022, 1500.0 },
                        { 2023, 2000.0 },
                        { 2024, 2500.0 }
                    }
                }
            ]
        };

        _mockRepository.GetProjectCostsPivotAsync(projectId, null).Returns(pivotData);

        // Act
        var result = await _service.GetProjectCostsPivotAsync(projectId);

        // Assert
        var row = Assert.Single(result.Rows);
        Assert.Equal(3, row.YearlyAmounts.Count);
        Assert.Equal(1500.0, row.YearlyAmounts[2022]);
        Assert.Equal(2000.0, row.YearlyAmounts[2023]);
        Assert.Equal(2500.0, row.YearlyAmounts[2024]);
        Assert.Equal(6000.0, row.Total);
    }

    #endregion
}