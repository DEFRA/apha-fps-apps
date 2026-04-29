using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.Costbook;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.CostBook.Controllers;
using Apha.FPSApps.Web.Areas.CostBook.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NSubstitute;
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.Costbook.YearlyDetailsControllerTest;

public class YearlyDetailsControllerTests
{
    private readonly ICostBookYearlyDetailsService _service;
    private readonly IMapper _mapper;
    private readonly YearlyDetailsController _controller;

    public YearlyDetailsControllerTests()
    {
        _service = Substitute.For<ICostBookYearlyDetailsService>();
        _mapper = Substitute.For<IMapper>();
        _controller = new YearlyDetailsController(_service, _mapper);
        _controller.TempData = Substitute.For<ITempDataDictionary>();
    }

    private static JsonElement GetJsonResultElement(JsonResult jsonResult)
    {
        var json = JsonSerializer.Serialize(jsonResult.Value);
        return JsonSerializer.Deserialize<JsonElement>(json);
    }

    #region Index Tests

    [Fact]
    public async Task Index_RedirectsToProjects_WhenHeaderNotFound()
    {
        // Arrange
        _service.GetProjectHeaderAsync(Arg.Any<string>())
            .Returns(ApiResponseDto<ProjectHeaderDto>.FailureResponse(null, new ApiMetaDto()));

        // Act
        var result = await _controller.Index("2024/001");

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Projects", redirect.ControllerName);
    }

    [Fact]
    public async Task Index_ReturnsViewWithViewModel_WhenHeaderExists()
    {
        // Arrange
        var header = new ProjectHeaderDto { ProjectId = "2024/001", ProjectTitle = "Test", IsDefraProject = 0 };
        _service.GetProjectHeaderAsync(Arg.Any<string>())
            .Returns(ApiResponseDto<ProjectHeaderDto>.SuccessResponse(header));

        var years = new List<ProjectYearDto> { new() { YearValue = 1 }, new() { YearValue = 2 } };
        _service.GetProjectYearsAsync(Arg.Any<string>())
            .Returns(ApiResponseDto<List<ProjectYearDto>>.SuccessResponse(years));

        _mapper.Map<List<ProjectYearRateItem>>(Arg.Any<List<ProjectYearDto>>())
            .Returns(new List<ProjectYearRateItem>());

        SetupLookupMocks();

        // Act
        var result = await _controller.Index("2024/001");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<YearlyDetailsViewModel>(viewResult.Model);
        Assert.Equal("2024/001", model.ProjectHeaderDto.ProjectId);
        Assert.Equal(2, model.ProjectYears.Count);
        Assert.Equal(1, model.SelectedYear); // defaults to first year
    }

    [Fact]
    public async Task Index_UsesSelectedYear_WhenProvided()
    {
        // Arrange
        var header = new ProjectHeaderDto { ProjectId = "2024/001", IsDefraProject = 0 };
        _service.GetProjectHeaderAsync(Arg.Any<string>())
            .Returns(ApiResponseDto<ProjectHeaderDto>.SuccessResponse(header));

        var years = new List<ProjectYearDto> { new() { YearValue = 1 }, new() { YearValue = 2 } };
        _service.GetProjectYearsAsync(Arg.Any<string>())
            .Returns(ApiResponseDto<List<ProjectYearDto>>.SuccessResponse(years));

        _mapper.Map<List<ProjectYearRateItem>>(Arg.Any<List<ProjectYearDto>>())
            .Returns(new List<ProjectYearRateItem>());

        SetupLookupMocks();

        // Act
        var result = await _controller.Index("2024/001", 2);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<YearlyDetailsViewModel>(viewResult.Model);
        Assert.Equal(2, model.SelectedYear);
    }

    #endregion

    #region AddProjectYear Tests

    [Fact]
    public async Task AddProjectYearGet_ReturnsPartialView()
    {
        // Act
        var result = _controller.AddProjectYearGet("2024/001", 3);

        // Assert
        var partialResult = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("_AddProjectYear", partialResult.ViewName);
        var model = Assert.IsType<ProjectYearRateItem>(partialResult.Model);
        Assert.Equal(3, model.YearValue);
    }

    [Fact]
    public async Task AddProjectYear_ReturnsSuccess_WhenServiceSucceeds()
    {
        // Arrange
        var item = new ProjectYearRateItem { YearValue = 2 };
        var dto = new ProjectYearDto { YearValue = 2 };

        _mapper.Map<ProjectYearDto>(item).Returns(dto);
        _service.AddProjectYearAsync(Arg.Any<string>(), 2, dto)
            .Returns(ApiResponseDto<ProjectYearDto>.SuccessResponse(new ProjectYearDto { YearValue = 2 }));

        // Act
        var result = await _controller.AddProjectYear("2024/001", 2, item);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        var element = GetJsonResultElement(jsonResult);
        Assert.True(element.GetProperty("success").GetBoolean());
        Assert.Equal(2, element.GetProperty("year").GetInt32());
    }

    [Fact]
    public async Task AddProjectYear_ReturnsFailure_WhenServiceFails()
    {
        // Arrange
        _mapper.Map<ProjectYearDto>(Arg.Any<ProjectYearRateItem>()).Returns(new ProjectYearDto());
        _service.AddProjectYearAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<ProjectYearDto>())
            .Returns(ApiResponseDto<ProjectYearDto>.FailureResponse(null, new ApiMetaDto()));

        // Act
        var result = await _controller.AddProjectYear("2024/001", 2, new ProjectYearRateItem());

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        var element = GetJsonResultElement(jsonResult);
        Assert.False(element.GetProperty("success").GetBoolean());
    }

    #endregion

    #region Staff CRUD Tests

    [Fact]
    public async Task CreateStaff_Post_ReturnsSuccess_WhenServiceSucceeds()
    {
        // Arrange
        var item = new StaffRequirementItem { WgGrade = "HEO" };
        var dto = new StaffRequirementDto();

        _mapper.Map<StaffRequirementDto>(item).Returns(dto);
        _service.AddStaffRequirementAsync(Arg.Any<string>(), Arg.Any<int>(), dto)
            .Returns(ApiResponseDto<StaffRequirementDto>.SuccessResponse(new StaffRequirementDto()));

        // Act
        var result = await _controller.CreateStaff("2024/001", 2024, item);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        var element = GetJsonResultElement(jsonResult);
        Assert.True(element.GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task CreateStaff_Post_ReturnsFailure_WhenServiceFails()
    {
        // Arrange
        _mapper.Map<StaffRequirementDto>(Arg.Any<StaffRequirementItem>()).Returns(new StaffRequirementDto());
        _service.AddStaffRequirementAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<StaffRequirementDto>())
            .Returns(ApiResponseDto<StaffRequirementDto>.FailureResponse(null, new ApiMetaDto()));

        // Act
        var result = await _controller.CreateStaff("2024/001", 2024, new StaffRequirementItem { WgGrade = "HEO" });

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        var element = GetJsonResultElement(jsonResult);
        Assert.False(element.GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task EditStaff_Get_ReturnsNotFound_WhenRowNotFound()
    {
        // Arrange
        var pagedResult = new PaginatedResult<StaffRequirementDto>(new List<StaffRequirementDto>(), 0);
        _service.GetStaffRequirementsAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<QueryParameters<string>>())
            .Returns(ApiResponseDto<PaginatedResult<StaffRequirementDto>>.SuccessResponse(pagedResult));
        _service.GetPayRatesAsync(Arg.Any<bool>())
            .Returns(ApiResponseDto<List<PayRateDto>>.SuccessResponse(new List<PayRateDto>()));

        // Act
        var result = await _controller.EditStaff("2024/001", 2024, 999, false);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task EditStaff_Get_ReturnsPartialView_WhenRowExists()
    {
        // Arrange
        var staffDto = new StaffRequirementDto { SrIdentity = 1, WgGrade = "HEO" };
        var pagedResult = new PaginatedResult<StaffRequirementDto>(new List<StaffRequirementDto> { staffDto }, 1);
        var staffItem = new StaffRequirementItem { SrIdentity = 1, WgGrade = "HEO" };

        _service.GetStaffRequirementsAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<QueryParameters<string>>())
            .Returns(ApiResponseDto<PaginatedResult<StaffRequirementDto>>.SuccessResponse(pagedResult));
        _service.GetPayRatesAsync(Arg.Any<bool>())
            .Returns(ApiResponseDto<List<PayRateDto>>.SuccessResponse(new List<PayRateDto>()));
        _mapper.Map<StaffRequirementItem>(staffDto).Returns(staffItem);

        // Act
        var result = await _controller.EditStaff("2024/001", 2024, 1, false);

        // Assert
        var partialResult = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("_AddEditStaffRequirement", partialResult.ViewName);
    }

    [Fact]
    public async Task EditStaff_Post_ReturnsSuccess_WhenServiceSucceeds()
    {
        // Arrange
        var item = new StaffRequirementItem { WgGrade = "HEO" };
        _mapper.Map<StaffRequirementDto>(item).Returns(new StaffRequirementDto());
        _service.UpdateStaffRequirementAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<StaffRequirementDto>())
            .Returns(ApiResponseDto<StaffRequirementDto>.SuccessResponse(new StaffRequirementDto()));

        // Act
        var result = await _controller.EditStaff("2024/001", 2024, 1, item);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        var element = GetJsonResultElement(jsonResult);
        Assert.True(element.GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task DeleteStaff_ReturnsSuccess_WhenServiceSucceeds()
    {
        // Arrange
        _service.DeleteStaffRequirementAsync(Arg.Any<string>(), Arg.Any<int>(), 1)
            .Returns(ApiResponseDto<bool>.SuccessResponse(true));

        // Act
        var result = await _controller.DeleteStaff("2024/001", 2024, 1);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        var element = GetJsonResultElement(jsonResult);
        Assert.True(element.GetProperty("success").GetBoolean());
    }

    #endregion

    #region Test CRUD Tests

    [Fact]
    public async Task CreateTest_Post_ReturnsSuccess_WhenServiceSucceeds()
    {
        _mapper.Map<TestRequirementDto>(Arg.Any<TestRequirementItem>()).Returns(new TestRequirementDto());
        _service.AddTestRequirementAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<TestRequirementDto>())
            .Returns(ApiResponseDto<TestRequirementDto>.SuccessResponse(new TestRequirementDto()));

        var result = await _controller.CreateTest("2024/001", 2024, new TestRequirementItem { TestCode = "TC001" });

        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.True(GetJsonResultElement(jsonResult).GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task EditTest_Get_ReturnsNotFound_WhenRowNotFound()
    {
        _service.GetTestRequirementsAsync(Arg.Any<string>(), Arg.Any<int>())
            .Returns(ApiResponseDto<List<TestRequirementDto>>.SuccessResponse(new List<TestRequirementDto>()));
        _service.GetTestCodeLookupsAsync(Arg.Any<bool>())
            .Returns(ApiResponseDto<List<TestCodeLookupDto>>.SuccessResponse(new List<TestCodeLookupDto>()));

        var result = await _controller.EditTest("2024/001", 2024, "NOTFOUND", false);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task EditTest_Post_ReturnsSuccess_WhenServiceSucceeds()
    {
        _mapper.Map<TestRequirementDto>(Arg.Any<TestRequirementItem>()).Returns(new TestRequirementDto());
        _service.UpdateTestRequirementAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<TestRequirementDto>())
            .Returns(ApiResponseDto<TestRequirementDto>.SuccessResponse(new TestRequirementDto()));

        var result = await _controller.EditTest("2024/001", 2024, "TC001", new TestRequirementItem { TestCode = "TC001" });

        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.True(GetJsonResultElement(jsonResult).GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task DeleteTest_ReturnsSuccess_WhenServiceSucceeds()
    {
        _service.DeleteTestRequirementAsync(Arg.Any<string>(), Arg.Any<int>(), "TC001")
            .Returns(ApiResponseDto<bool>.SuccessResponse(true));

        var result = await _controller.DeleteTest("2024/001", 2024, "TC001");

        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.True(GetJsonResultElement(jsonResult).GetProperty("success").GetBoolean());
    }

    #endregion

    #region Animal CRUD Tests

    [Fact]
    public async Task CreateAnimal_Post_ReturnsSuccess_WhenServiceSucceeds()
    {
        _mapper.Map<AnimalRequirementDto>(Arg.Any<AnimalRequirementItem>()).Returns(new AnimalRequirementDto());
        _service.AddAnimalRequirementAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<AnimalRequirementDto>())
            .Returns(ApiResponseDto<AnimalRequirementDto>.SuccessResponse(new AnimalRequirementDto()));

        var result = await _controller.CreateAnimal("2024/001", 2024, new AnimalRequirementItem { AnimalType = "CAT" });

        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.True(GetJsonResultElement(jsonResult).GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task EditAnimal_Get_ReturnsNotFound_WhenRowNotFound()
    {
        _service.GetAnimalRequirementsAsync(Arg.Any<string>(), Arg.Any<int>())
            .Returns(ApiResponseDto<List<AnimalRequirementDto>>.SuccessResponse(new List<AnimalRequirementDto>()));
        _service.GetAllAnimalsAsync()
            .Returns(ApiResponseDto<List<AnimalLookupDto>>.SuccessResponse(new List<AnimalLookupDto>()));

        var result = await _controller.EditAnimal("2024/001", 2024, 999, false);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task EditAnimal_Post_ReturnsSuccess_WhenServiceSucceeds()
    {
        _mapper.Map<AnimalRequirementDto>(Arg.Any<AnimalRequirementItem>()).Returns(new AnimalRequirementDto());
        _service.UpdateAnimalRequirementAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<AnimalRequirementDto>())
            .Returns(ApiResponseDto<AnimalRequirementDto>.SuccessResponse(new AnimalRequirementDto()));

        var result = await _controller.EditAnimal("2024/001", 2024, 1, new AnimalRequirementItem { AnimalType = "CAT" });

        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.True(GetJsonResultElement(jsonResult).GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task DeleteAnimal_ReturnsSuccess_WhenServiceSucceeds()
    {
        _service.DeleteAnimalRequirementAsync(Arg.Any<string>(), Arg.Any<int>(), 1)
            .Returns(ApiResponseDto<bool>.SuccessResponse(true));

        var result = await _controller.DeleteAnimal("2024/001", 2024, 1);

        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.True(GetJsonResultElement(jsonResult).GetProperty("success").GetBoolean());
    }

    #endregion

    #region AdditionalCost CRUD Tests

    [Fact]
    public async Task CreateAdditionalCost_Get_ReturnsPartialView()
    {
        _service.GetAccountCategoriesAsync()
            .Returns(ApiResponseDto<List<AccountCategoryDto>>.SuccessResponse(new List<AccountCategoryDto>()));

        var result = await _controller.CreateAdditionalCost("2024/001", 2024);

        var partialResult = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("_AddEditAdditionalCost", partialResult.ViewName);
    }

    [Fact]
    public async Task CreateAdditionalCost_Post_ReturnsSuccess_WhenServiceSucceeds()
    {
        _mapper.Map<AdditionalCostDto>(Arg.Any<AdditionalCostItem>()).Returns(new AdditionalCostDto());
        _service.AddAdditionalCostAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<AdditionalCostDto>())
            .Returns(ApiResponseDto<AdditionalCostDto>.SuccessResponse(new AdditionalCostDto()));

        var result = await _controller.CreateAdditionalCost("2024/001", 2024,
            new AdditionalCostItem { Description = "Travel", AccountCat = "TRAVEL" });

        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.True(GetJsonResultElement(jsonResult).GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task EditAdditionalCost_Get_ReturnsNotFound_WhenRowNotFound()
    {
        _service.GetAdditionalCostsAsync(Arg.Any<string>(), Arg.Any<int>())
            .Returns(ApiResponseDto<List<AdditionalCostDto>>.SuccessResponse(new List<AdditionalCostDto>()));
        _service.GetAccountCategoriesAsync()
            .Returns(ApiResponseDto<List<AccountCategoryDto>>.SuccessResponse(new List<AccountCategoryDto>()));

        var result = await _controller.EditAdditionalCost("2024/001", 2024, 999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task EditAdditionalCost_Post_ReturnsSuccess_WhenServiceSucceeds()
    {
        _mapper.Map<AdditionalCostDto>(Arg.Any<AdditionalCostItem>()).Returns(new AdditionalCostDto());
        _service.UpdateAdditionalCostAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<AdditionalCostDto>())
            .Returns(ApiResponseDto<AdditionalCostDto>.SuccessResponse(new AdditionalCostDto()));

        var result = await _controller.EditAdditionalCost("2024/001", 2024, 1,
            new AdditionalCostItem { Description = "Travel", AccountCat = "TRAVEL" });

        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.True(GetJsonResultElement(jsonResult).GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task DeleteAdditionalCost_ReturnsSuccess_WhenServiceSucceeds()
    {
        _service.DeleteAdditionalCostAsync(Arg.Any<string>(), Arg.Any<int>(), 1)
            .Returns(ApiResponseDto<bool>.SuccessResponse(true));

        var result = await _controller.DeleteAdditionalCost("2024/001", 2024, 1);

        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.True(GetJsonResultElement(jsonResult).GetProperty("success").GetBoolean());
    }

    #endregion

    #region MarkupAndProfit Tests

    [Fact]
    public async Task EditMarkupAndProfit_ReturnsNotFound_WhenYearNotFound()
    {
        _service.GetProjectYearsAsync(Arg.Any<string>())
            .Returns(ApiResponseDto<List<ProjectYearDto>>.SuccessResponse(new List<ProjectYearDto>()));

        var result = await _controller.EditMarkupAndProfit("2024/001", 99);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task EditMarkupAndProfit_ReturnsPartialView_WhenYearExists()
    {
        var yearDto = new ProjectYearDto { YearValue = 1 };
        var rateItem = new ProjectYearRateItem { YearValue = 1 };

        _service.GetProjectYearsAsync(Arg.Any<string>())
            .Returns(ApiResponseDto<List<ProjectYearDto>>.SuccessResponse(new List<ProjectYearDto> { yearDto }));
        _mapper.Map<ProjectYearRateItem>(yearDto).Returns(rateItem);

        var result = await _controller.EditMarkupAndProfit("2024/001", 1);

        var partialResult = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("_AddProjectYear", partialResult.ViewName);
    }

    [Fact]
    public async Task UpdateProjectYearRate_ReturnsSuccess_WhenServiceSucceeds()
    {
        _mapper.Map<ProjectYearDto>(Arg.Any<ProjectYearRateItem>()).Returns(new ProjectYearDto());
        _service.UpdateProjectYearAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<ProjectYearDto>())
            .Returns(ApiResponseDto<ProjectYearDto>.SuccessResponse(new ProjectYearDto()));

        var result = await _controller.UpdateProjectYearRate("2024/001", 1, new ProjectYearRateItem());

        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.True(GetJsonResultElement(jsonResult).GetProperty("success").GetBoolean());
    }

    #endregion

    #region Helpers

    private void SetupLookupMocks()
    {
        _service.GetPayRatesAsync(Arg.Any<bool>())
            .Returns(ApiResponseDto<List<PayRateDto>>.SuccessResponse(new List<PayRateDto>()));
        _service.GetTestCodeLookupsAsync(Arg.Any<bool>())
            .Returns(ApiResponseDto<List<TestCodeLookupDto>>.SuccessResponse(new List<TestCodeLookupDto>()));
        _service.GetAllAnimalsAsync()
            .Returns(ApiResponseDto<List<AnimalLookupDto>>.SuccessResponse(new List<AnimalLookupDto>()));
        _service.GetAccountCategoriesAsync()
            .Returns(ApiResponseDto<List<AccountCategoryDto>>.SuccessResponse(new List<AccountCategoryDto>()));
        _service.GetAnimalRatesAsync(Arg.Any<bool>())
            .Returns(ApiResponseDto<List<AnimalRateDto>>.SuccessResponse(new List<AnimalRateDto>()));
    }

    #endregion
}
