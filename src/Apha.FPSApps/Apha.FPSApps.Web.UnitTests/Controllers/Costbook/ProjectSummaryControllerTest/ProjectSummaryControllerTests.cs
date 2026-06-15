using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.Costbook;
using Apha.FPSApps.Web.Areas.CostBook.Controllers;
using Apha.FPSApps.Web.Areas.CostBook.Models;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.FPSApps.Web.UnitTests.Controllers.Costbook.ProjectSummaryControllerTest;

public class ProjectSummaryControllerTests
{
    private readonly ICostBookYearlyDetailsService _yearlyDetailsService;
    private readonly ICostBookProjectSummaryService _projectSummaryService;
    private readonly ProjectSummaryController _controller;

    public ProjectSummaryControllerTests()
    {
        _yearlyDetailsService   = Substitute.For<ICostBookYearlyDetailsService>();
        _projectSummaryService  = Substitute.For<ICostBookProjectSummaryService>();

        _controller = new ProjectSummaryController(
            _yearlyDetailsService,
            _projectSummaryService);
    }

    // ── helpers ────────────────────────────────────────────────────────────

    private static ApiResponseDto<ProjectHeaderDto> HeaderSuccess(string projectId, string programme = "NonComm") =>
        ApiResponseDto<ProjectHeaderDto>.SuccessResponse(
            new ProjectHeaderDto { ProjectId = projectId, ProjectTitle = "Test Project", Programme = programme });

    private static ApiResponseDto<ProjectHeaderDto> HeaderFailure() =>
        ApiResponseDto<ProjectHeaderDto>.FailureResponse(
            [new ApiErrorDto { Code = "NOT_FOUND", Message = "Not found" }], new ApiMetaDto());

    private static ApiResponseDto<List<ProjectYearDto>> YearsSuccess(params int[] years) =>
        ApiResponseDto<List<ProjectYearDto>>.SuccessResponse(
            years.Select(y => new ProjectYearDto { Project = "P001", YearValue = y }).ToList());

    private static ApiResponseDto<List<ProjectYearDto>> YearsEmpty() =>
        ApiResponseDto<List<ProjectYearDto>>.SuccessResponse([]);

    private static ApiResponseDto<ProjectYearCostSummaryDto> CostSummarySuccess(
        string projectId, int year,
        double staffCost = 0, double testCost = 0,
        double animalCost = 0, double additionalCost = 0) =>
        ApiResponseDto<ProjectYearCostSummaryDto>.SuccessResponse(
            new ProjectYearCostSummaryDto
            {
                Project             = projectId,
                Year                = year,
                StaffCostTotal      = staffCost,
                TestCostTotal       = testCost,
                AnimalCostTotal     = animalCost,
                AdditionalCostTotal = additionalCost,
                GrandTotal          = staffCost + testCost + animalCost + additionalCost
            });

    private static ApiResponseDto<double> ProfitSuccess(double profit) =>
        ApiResponseDto<double>.SuccessResponse(profit);

    private void SetupYearServices(string projectId, int year,
        double staffCost = 0, double testCost = 0, double animalCost = 0,
        double additionalCost = 0, double profit = 0)
    {
        _projectSummaryService
            .GetProjectYearCostSummaryAsync(projectId, year)
            .Returns(CostSummarySuccess(projectId, year, staffCost, testCost, animalCost, additionalCost));
        _projectSummaryService
            .GetProfitIncludedTotalAsync(projectId, year)
            .Returns(ProfitSuccess(profit));
    }

    // ── Index Tests ────────────────────────────────────────────────────────

    #region Index – header not found

    [Fact]
    public async Task Index_WhenHeaderNotFound_RedirectsToProjectsIndex()
    {
        // Arrange
        _yearlyDetailsService.GetProjectHeaderAsync("P001").Returns(HeaderFailure());

        // Act
        var result = await _controller.Index("P001");

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Projects", redirect.ControllerName);
    }

    [Fact]
    public async Task Index_WhenHeaderDataIsNull_RedirectsToProjectsIndex()
    {
        // Arrange
        var response = ApiResponseDto<ProjectHeaderDto>.SuccessResponse(null!);
        _yearlyDetailsService.GetProjectHeaderAsync("P001").Returns(response);

        // Act
        var result = await _controller.Index("P001");

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Projects", redirect.ControllerName);
    }

    #endregion

    #region Index – no years

    [Fact]
    public async Task Index_WithNoYears_ReturnsViewWithEmptyRows()
    {
        // Arrange
        const string projectId = "P001";
        _yearlyDetailsService.GetProjectHeaderAsync(projectId).Returns(HeaderSuccess(projectId));
        _yearlyDetailsService.GetProjectYearsAsync(projectId).Returns(YearsEmpty());

        // Act
        var result = await _controller.Index(projectId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ProjectSummaryViewModel>(viewResult.Model);
        Assert.Empty(model.Rows);
    }

    [Fact]
    public async Task Index_WhenYearsResponseFails_ReturnsViewWithEmptyRows()
    {
        // Arrange
        const string projectId = "P001";
        var yearsFailure = ApiResponseDto<List<ProjectYearDto>>.FailureResponse(
            [new ApiErrorDto { Code = "ERR", Message = "Failed" }], new ApiMetaDto());

        _yearlyDetailsService.GetProjectHeaderAsync(projectId).Returns(HeaderSuccess(projectId));
        _yearlyDetailsService.GetProjectYearsAsync(projectId).Returns(yearsFailure);

        // Act
        var result = await _controller.Index(projectId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ProjectSummaryViewModel>(viewResult.Model);
        Assert.Empty(model.Rows);
    }

    #endregion

    #region Index – rows and cost aggregation

    [Fact]
    public async Task Index_WithOneYear_ReturnsViewWithOneRow()
    {
        // Arrange
        const string projectId = "P001";
        _yearlyDetailsService.GetProjectHeaderAsync(projectId).Returns(HeaderSuccess(projectId));
        _yearlyDetailsService.GetProjectYearsAsync(projectId).Returns(YearsSuccess(2024));
        SetupYearServices(projectId, 2024,
            staffCost: 100, testCost: 200, animalCost: 300, additionalCost: 400, profit: 50);

        // Act
        var result = await _controller.Index(projectId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ProjectSummaryViewModel>(viewResult.Model);
        Assert.Single(model.Rows);
        var row = model.Rows[0];
        Assert.Equal(2024, row.Year);
        Assert.Equal(100, row.StaffCost);
        Assert.Equal(200, row.TestCost);
        Assert.Equal(300, row.AnimalCost);
        Assert.Equal(400, row.AdditionalCost);
        Assert.Equal(50,  row.ProfitIncludedTotal);
    }

    [Fact]
    public async Task Index_WithMultipleYears_RowsOrderedByYearAscending()
    {
        // Arrange
        const string projectId = "P001";
        _yearlyDetailsService.GetProjectHeaderAsync(projectId).Returns(HeaderSuccess(projectId));
        _yearlyDetailsService.GetProjectYearsAsync(projectId).Returns(YearsSuccess(2026, 2024, 2025));
        SetupYearServices(projectId, 2024);
        SetupYearServices(projectId, 2025);
        SetupYearServices(projectId, 2026);

        // Act
        var result = await _controller.Index(projectId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ProjectSummaryViewModel>(viewResult.Model);
        Assert.Equal(3, model.Rows.Count);
        Assert.Equal(2024, model.Rows[0].Year);
        Assert.Equal(2025, model.Rows[1].Year);
        Assert.Equal(2026, model.Rows[2].Year);
    }

    [Fact]
    public async Task Index_WithMultipleYears_GrandTotalAggregatesAllRows()
    {
        // Arrange
        const string projectId = "P001";
        _yearlyDetailsService.GetProjectHeaderAsync(projectId).Returns(HeaderSuccess(projectId));
        _yearlyDetailsService.GetProjectYearsAsync(projectId).Returns(YearsSuccess(2024, 2025));
        SetupYearServices(projectId, 2024,
            staffCost: 100, testCost: 50, animalCost: 25, additionalCost: 25);
        SetupYearServices(projectId, 2025,
            staffCost: 200, testCost: 100, animalCost: 50, additionalCost: 50);

        // Act
        var result = await _controller.Index(projectId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ProjectSummaryViewModel>(viewResult.Model);
        Assert.Equal(300, model.TotalStaffCost);
        Assert.Equal(150, model.TotalTestCost);
        Assert.Equal(75,  model.TotalAnimalCost);
        Assert.Equal(75,  model.TotalAdditionalCost);
        Assert.Equal(600, model.GrandTotal);
    }

    #endregion

    #region Index – ShowInclProfit / Programme

    [Fact]
    public async Task Index_WhenProgrammeIsComm_ShowInclProfitIsTrue()
    {
        // Arrange
        const string projectId = "P001";
        _yearlyDetailsService.GetProjectHeaderAsync(projectId)
            .Returns(HeaderSuccess(projectId, programme: "Comm"));
        _yearlyDetailsService.GetProjectYearsAsync(projectId).Returns(YearsEmpty());

        // Act
        var result = await _controller.Index(projectId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ProjectSummaryViewModel>(viewResult.Model);
        Assert.True(model.ShowInclProfit);
    }

    [Fact]
    public async Task Index_WhenProgrammeIsNotComm_ShowInclProfitIsFalse()
    {
        // Arrange
        const string projectId = "P001";
        _yearlyDetailsService.GetProjectHeaderAsync(projectId)
            .Returns(HeaderSuccess(projectId, programme: "NonComm"));
        _yearlyDetailsService.GetProjectYearsAsync(projectId).Returns(YearsEmpty());

        // Act
        var result = await _controller.Index(projectId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ProjectSummaryViewModel>(viewResult.Model);
        Assert.False(model.ShowInclProfit);
    }

    [Fact]
    public async Task Index_WithYearHavingProfit_TotalProfitIncludedAggregatesCorrectly()
    {
        // Arrange
        const string projectId = "P001";
        _yearlyDetailsService.GetProjectHeaderAsync(projectId)
            .Returns(HeaderSuccess(projectId, programme: "Comm"));
        _yearlyDetailsService.GetProjectYearsAsync(projectId).Returns(YearsSuccess(2024, 2025));
        SetupYearServices(projectId, 2024, profit: 150);
        SetupYearServices(projectId, 2025, profit: 250);

        // Act
        var result = await _controller.Index(projectId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ProjectSummaryViewModel>(viewResult.Model);
        Assert.Equal(400, model.TotalProfitIncluded);
        Assert.Equal(400, model.InclProfit);
    }

    [Fact]
    public async Task Index_WhenProfitServiceFails_ProfitIncludedTotalDefaultsToZero()
    {
        // Arrange
        const string projectId = "P001";
        var profitFailure = ApiResponseDto<double>.FailureResponse(
            [new ApiErrorDto { Code = "ERR", Message = "Failed" }], new ApiMetaDto());

        _yearlyDetailsService.GetProjectHeaderAsync(projectId)
            .Returns(HeaderSuccess(projectId, programme: "Comm"));
        _yearlyDetailsService.GetProjectYearsAsync(projectId).Returns(YearsSuccess(2024));
        _projectSummaryService.GetProjectYearCostSummaryAsync(projectId, 2024)
            .Returns(CostSummarySuccess(projectId, 2024));
        _projectSummaryService.GetProfitIncludedTotalAsync(projectId, 2024).Returns(profitFailure);

        // Act
        var result = await _controller.Index(projectId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ProjectSummaryViewModel>(viewResult.Model);
        Assert.Equal(0, model.Rows[0].ProfitIncludedTotal);
    }

    [Fact]
    public async Task Index_WhenCostSummaryServiceFails_CostsDefaultToZero()
    {
        // Arrange
        const string projectId = "P001";
        var costFailure = ApiResponseDto<ProjectYearCostSummaryDto>.FailureResponse(
            [new ApiErrorDto { Code = "ERR", Message = "Failed" }], new ApiMetaDto());

        _yearlyDetailsService.GetProjectHeaderAsync(projectId)
            .Returns(HeaderSuccess(projectId));
        _yearlyDetailsService.GetProjectYearsAsync(projectId).Returns(YearsSuccess(2024));
        _projectSummaryService.GetProjectYearCostSummaryAsync(projectId, 2024).Returns(costFailure);
        _projectSummaryService.GetProfitIncludedTotalAsync(projectId, 2024).Returns(ProfitSuccess(0));

        // Act
        var result = await _controller.Index(projectId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ProjectSummaryViewModel>(viewResult.Model);
        Assert.Single(model.Rows);
        Assert.Equal(0, model.Rows[0].StaffCost);
        Assert.Equal(0, model.Rows[0].TestCost);
        Assert.Equal(0, model.Rows[0].AnimalCost);
        Assert.Equal(0, model.Rows[0].AdditionalCost);
        Assert.Equal(0, model.GrandTotal);
    }

    #endregion

    #region Index – URL-encoded project ID

    [Fact]
    public async Task Index_WithUrlEncodedProjectId_DecodesBeforeCallingServices()
    {
        // Arrange
        const string decodedId = "P001/2024";
        var encodedId = System.Web.HttpUtility.UrlEncode(decodedId); // "P001%2f2024"

        _yearlyDetailsService.GetProjectHeaderAsync(decodedId).Returns(HeaderSuccess(decodedId));
        _yearlyDetailsService.GetProjectYearsAsync(decodedId).Returns(YearsEmpty());

        // Act
        var result = await _controller.Index(encodedId);

        // Assert
        Assert.IsType<ViewResult>(result);
        await _yearlyDetailsService.Received(1).GetProjectHeaderAsync(decodedId);
        await _yearlyDetailsService.Received(1).GetProjectYearsAsync(decodedId);
    }

    #endregion

    #region Index – view model header

    [Fact]
    public async Task Index_ReturnsViewModelWithCorrectProjectHeader()
    {
        // Arrange
        const string projectId = "P001";
        var header = new ProjectHeaderDto
        {
            ProjectId    = projectId,
            ProjectTitle = "My Project",
            Programme    = "NonComm"
        };
        _yearlyDetailsService.GetProjectHeaderAsync(projectId)
            .Returns(ApiResponseDto<ProjectHeaderDto>.SuccessResponse(header));
        _yearlyDetailsService.GetProjectYearsAsync(projectId).Returns(YearsEmpty());

        // Act
        var result = await _controller.Index(projectId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ProjectSummaryViewModel>(viewResult.Model);
        Assert.Equal(projectId,   model.ProjectHeaderDto.ProjectId);
        Assert.Equal("My Project", model.ProjectHeaderDto.ProjectTitle);
    }

    #endregion
}