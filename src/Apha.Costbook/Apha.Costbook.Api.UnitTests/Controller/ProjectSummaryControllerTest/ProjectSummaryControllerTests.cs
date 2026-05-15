using Apha.Common.Contracts;
using Apha.Common.Contracts.Costbook;
using Apha.Costbook.Api.Controllers;
using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Interfaces;
using Apha.Costbook.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.Costbook.Api.UnitTests.Controller.ProjectSummaryControllerTest;

public class ProjectSummaryControllerTests
{
    private readonly IProjectSummaryService _service;
    private readonly IMapper _mapper;
    private readonly ProjectSummaryController _controller;

    public ProjectSummaryControllerTests()
    {
        _service = Substitute.For<IProjectSummaryService>();
        _mapper = Substitute.For<IMapper>();
        _controller = new ProjectSummaryController(_service, _mapper);
    }

    // ─── GetProfitIncludedTotal ───────────────────────────────────────────────

    [Fact]
    public async Task GetProfitIncludedTotal_ReturnsOkResult_WithApiResponse()
    {
        // Arrange
        var projectId = "PRJ-001";
        var year = 2024;
        var total = 123456.78;

        _service.GetProfitIncludedTotalAsync(projectId, year).Returns(total);

        // Act
        var result = await _controller.GetProfitIncludedTotal(projectId, year);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<double>>(okResult.Value);

        Assert.True(response.Success);
        Assert.Equal(total, response.Data);
        Assert.NotNull(response.Errors);
        Assert.Empty(response.Errors);
        Assert.NotNull(response.Meta);

        await _service.Received(1).GetProfitIncludedTotalAsync(projectId, year);
    }

    [Fact]
    public async Task GetProfitIncludedTotal_WithZeroTotal_ReturnsOkResult_WithZero()
    {
        // Arrange
        var projectId = "PRJ-001";
        var year = 2024;

        _service.GetProfitIncludedTotalAsync(projectId, year).Returns(0.0);

        // Act
        var result = await _controller.GetProfitIncludedTotal(projectId, year);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<double>>(okResult.Value);

        Assert.True(response.Success);
        Assert.Equal(0.0, response.Data);

        await _service.Received(1).GetProfitIncludedTotalAsync(projectId, year);
    }

    [Fact]
    public async Task GetProfitIncludedTotal_WithException_ThrowsException()
    {
        // Arrange
        var projectId = "PRJ-001";
        var year = 2024;

        _service.GetProfitIncludedTotalAsync(projectId, year).Throws(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _controller.GetProfitIncludedTotal(projectId, year));

        await _service.Received(1).GetProfitIncludedTotalAsync(projectId, year);
    }

    // ─── GetStaffYearsPivot ───────────────────────────────────────────────────

    [Fact]
    public async Task GetStaffYearsPivot_ReturnsOkResult_WithMappedResult()
    {
        // Arrange
        var id = "PRJ-001";
        var query = new QueryParameters<string>();
        var dto = new StaffYearsPivotDto { TotalCount = 2, Years = [2023, 2024] };
        var res = new StaffYearsPivotRes { TotalCount = 2, Years = [2023, 2024] };

        _service.GetStaffYearsPivotAsync(id, query).Returns(dto);
        _mapper.Map<StaffYearsPivotRes>(dto).Returns(res);

        // Act
        var result = await _controller.GetStaffYearsPivot(id, query);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(res, okResult.Value);

        await _service.Received(1).GetStaffYearsPivotAsync(id, query);
        _mapper.Received(1).Map<StaffYearsPivotRes>(dto);
    }

    [Fact]
    public async Task GetStaffYearsPivot_WithNullQuery_ReturnsOkResult()
    {
        // Arrange
        var id = "PRJ-001";
        var dto = new StaffYearsPivotDto();
        var res = new StaffYearsPivotRes();

        _service.GetStaffYearsPivotAsync(id, null).Returns(dto);
        _mapper.Map<StaffYearsPivotRes>(dto).Returns(res);

        // Act
        var result = await _controller.GetStaffYearsPivot(id, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(res, okResult.Value);

        await _service.Received(1).GetStaffYearsPivotAsync(id, null);
        _mapper.Received(1).Map<StaffYearsPivotRes>(dto);
    }

    [Fact]
    public async Task GetStaffYearsPivot_WithException_ThrowsException()
    {
        // Arrange
        var id = "PRJ-001";

        _service.GetStaffYearsPivotAsync(id, Arg.Any<QueryParameters<string>?>())
            .Throws(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _controller.GetStaffYearsPivot(id));

        await _service.Received(1).GetStaffYearsPivotAsync(id, Arg.Any<QueryParameters<string>?>());
    }

    // ─── GetStaffEffort ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetStaffEffort_ReturnsOkResult_WithMappedResult()
    {
        // Arrange
        var id = "PRJ-001";
        var query = new QueryParameters<string>();
        var dto = new StaffEffortPivotDto { TotalCount = 3, Years = [2022, 2023, 2024] };
        var res = new StaffEffortPivotRes { TotalCount = 3, Years = [2022, 2023, 2024] };

        _service.GetStaffEffortAsync(id, query).Returns(dto);
        _mapper.Map<StaffEffortPivotRes>(dto).Returns(res);

        // Act
        var result = await _controller.GetStaffEffort(id, query);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(res, okResult.Value);

        await _service.Received(1).GetStaffEffortAsync(id, query);
        _mapper.Received(1).Map<StaffEffortPivotRes>(dto);
    }

    [Fact]
    public async Task GetStaffEffort_WithNullQuery_ReturnsOkResult()
    {
        // Arrange
        var id = "PRJ-001";
        var dto = new StaffEffortPivotDto();
        var res = new StaffEffortPivotRes();

        _service.GetStaffEffortAsync(id, null).Returns(dto);
        _mapper.Map<StaffEffortPivotRes>(dto).Returns(res);

        // Act
        var result = await _controller.GetStaffEffort(id, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(res, okResult.Value);

        await _service.Received(1).GetStaffEffortAsync(id, null);
        _mapper.Received(1).Map<StaffEffortPivotRes>(dto);
    }

    [Fact]
    public async Task GetStaffEffort_WithException_ThrowsException()
    {
        // Arrange
        var id = "PRJ-001";

        _service.GetStaffEffortAsync(id, Arg.Any<QueryParameters<string>?>())
            .Throws(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _controller.GetStaffEffort(id));

        await _service.Received(1).GetStaffEffortAsync(id, Arg.Any<QueryParameters<string>?>());
    }

    // ─── GetProjectCostsPivot ─────────────────────────────────────────────────

    [Fact]
    public async Task GetProjectCostsPivot_ReturnsOkResult_WithMappedResult()
    {
        // Arrange
        var id = "PRJ-001";
        var query = new QueryParameters<string>();
        var dto = new ProjectCostsPivotDto { TotalCount = 1, Years = [2024] };
        var res = new ProjectCostsPivotRes { TotalCount = 1, Years = [2024] };

        _service.GetProjectCostsPivotAsync(id, query).Returns(dto);
        _mapper.Map<ProjectCostsPivotRes>(dto).Returns(res);

        // Act
        var result = await _controller.GetProjectCostsPivot(id, query);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(res, okResult.Value);

        await _service.Received(1).GetProjectCostsPivotAsync(id, query);
        _mapper.Received(1).Map<ProjectCostsPivotRes>(dto);
    }

    [Fact]
    public async Task GetProjectCostsPivot_WithNullQuery_ReturnsOkResult()
    {
        // Arrange
        var id = "PRJ-001";
        var dto = new ProjectCostsPivotDto();
        var res = new ProjectCostsPivotRes();

        _service.GetProjectCostsPivotAsync(id, null).Returns(dto);
        _mapper.Map<ProjectCostsPivotRes>(dto).Returns(res);

        // Act
        var result = await _controller.GetProjectCostsPivot(id, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(res, okResult.Value);

        await _service.Received(1).GetProjectCostsPivotAsync(id, null);
        _mapper.Received(1).Map<ProjectCostsPivotRes>(dto);
    }

    [Fact]
    public async Task GetProjectCostsPivot_WithException_ThrowsException()
    {
        // Arrange
        var id = "PRJ-001";

        _service.GetProjectCostsPivotAsync(id, Arg.Any<QueryParameters<string>?>())
            .Throws(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _controller.GetProjectCostsPivot(id));

        await _service.Received(1).GetProjectCostsPivotAsync(id, Arg.Any<QueryParameters<string>?>());
    }
}