using Apha.Common.Contracts;
using Apha.Common.Contracts.Costbook;
using Apha.Costbook.Api.Controllers;
using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Interfaces;
using Apha.Costbook.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.Costbook.Api.UnitTests.Controller.YearlyDetailsControllerTest;

public class YearlyDetailsControllerTests
{
    private readonly IYearlyDetailsService _service;
    private readonly IMapper _mapper;
    private readonly YearlyDetailsController _controller;

    public YearlyDetailsControllerTests()
    {
        _service = Substitute.For<IYearlyDetailsService>();
        _mapper = Substitute.For<IMapper>();
        _controller = new YearlyDetailsController(_service, _mapper);
    }

    #region GetProjectHeader

    [Fact]
    public async Task GetProjectHeader_ReturnsNotFound_WhenProjectNotFound()
    {
        _service.GetProjectHeaderAsync("NOTFOUND").Returns((ProjectHeaderDto?)null);

        var result = await _controller.GetProjectHeader("NOTFOUND");

        Assert.IsType<NotFoundResult>(result);
        await _service.Received(1).GetProjectHeaderAsync("NOTFOUND");
    }

    [Fact]
    public async Task GetProjectHeader_ReturnsOk_WithMappedData()
    {
        var dto = new ProjectHeaderDto { ProjectId = "2024/001" };
        var res = new ProjectHeaderRes { ProjectId = "2024/001" };

        _service.GetProjectHeaderAsync("2024/001").Returns(dto);
        _mapper.Map<ProjectHeaderRes>(dto).Returns(res);

        var result = await _controller.GetProjectHeader("2024/001");

        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<ProjectHeaderRes>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Equal("2024/001", apiResponse.Data!.ProjectId);
    }

    #endregion

    #region GetProjectYears

    [Fact]
    public async Task GetProjectYears_ReturnsOk_WithMappedList()
    {
        var dtos = new List<ProjectYearDto> { new() { YearValue = 1 } };
        var resList = new List<ProjectYearRes> { new() { YearValue = 1 } };

        _service.GetProjectYearsAsync("2024/001").Returns(dtos);
        _mapper.Map<List<ProjectYearRes>>(dtos).Returns(resList);

        var result = await _controller.GetProjectYears("2024/001");

        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<List<ProjectYearRes>>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Single(apiResponse.Data!);
    }

    #endregion

    #region AddProjectYear

    [Fact]
    public async Task AddProjectYear_ReturnsOk_WithMappedResult()
    {
        var req = new AddProjectYearReq { Year = 2 };
        var yearDto = new ProjectYearDto();
        var addedDto = new ProjectYearDto { Project = "2024/001", YearValue = 2 };
        var res = new ProjectYearRes { YearValue = 2 };

        _mapper.Map<ProjectYearDto>(req).Returns(yearDto);
        _service.AddProjectYearAsync("2024/001", 2, yearDto).Returns(addedDto);
        _mapper.Map<ProjectYearRes>(addedDto).Returns(res);

        var result = await _controller.AddProjectYear("2024/001", req);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<ProjectYearRes>>(okResult.Value);
        Assert.Equal(2, apiResponse.Data!.YearValue);
        await _service.Received(1).AddProjectYearAsync("2024/001", 2, yearDto);
    }

    [Fact]
    public async Task AddProjectYear_SetsProjectIdAndYear_OnDto()
    {
        var req = new AddProjectYearReq { Year = 3 };
        var yearDto = new ProjectYearDto();

        _mapper.Map<ProjectYearDto>(req).Returns(yearDto);
        _service.AddProjectYearAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<ProjectYearDto>())
            .Returns(new ProjectYearDto());
        _mapper.Map<ProjectYearRes>(Arg.Any<ProjectYearDto>()).Returns(new ProjectYearRes());

        await _controller.AddProjectYear("2024/001", req);

        Assert.Equal("2024/001", yearDto.Project);
        Assert.Equal(3, yearDto.YearValue);
    }

    #endregion

    #region UpdateProjectYear

    [Fact]
    public async Task UpdateProjectYear_ReturnsOk_WithMappedResult()
    {
        var req = new ProjectYearReq();
        var dto = new ProjectYearDto();
        var updated = new ProjectYearDto { Project = "2024/001", YearValue = 1 };
        var res = new ProjectYearRes { YearValue = 1 };

        _mapper.Map<ProjectYearDto>(req).Returns(dto);
        _service.UpdateProjectYearAsync(dto).Returns(updated);
        _mapper.Map<ProjectYearRes>(updated).Returns(res);

        var result = await _controller.UpdateProjectYear("2024/001", 1, req);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal("2024/001", dto.Project);
        Assert.Equal(1, dto.YearValue);
    }

    #endregion

    #region GetStaffRequirements

    [Fact]
    public async Task GetStaffRequirements_ReturnsOk_WithPaginatedResult()
    {
        var queryReq = new PaginationReq<string> { Page = 1 };
        var filter = new QueryParameters<string> { Page = 1 };
        var serviceResult = new PaginatedResult<StaffRequirementDto>();
        var mappedResult = new PaginationRes<StaffRequirementRes>();

        _mapper.Map<QueryParameters<string>>(queryReq).Returns(filter);
        _service.GetStaffRequirementsAsync("2024/001", 2024, filter).Returns(serviceResult);
        _mapper.Map<PaginationRes<StaffRequirementRes>>(serviceResult).Returns(mappedResult);

        var result = await _controller.GetStaffRequirements("2024/001", 2024, queryReq);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        await _service.Received(1).GetStaffRequirementsAsync("2024/001", 2024, filter);
    }

    #endregion

    #region AddStaffRequirement

    [Fact]
    public async Task AddStaffRequirement_SetsProjectIdAndYear_ReturnsOk()
    {
        var req = new StaffRequirementReq();
        var dto = new StaffRequirementDto();
        var resultDto = new StaffRequirementDto { SrIdentity = 1 };
        var res = new StaffRequirementRes { SrIdentity = 1 };

        _mapper.Map<StaffRequirementDto>(req).Returns(dto);
        _service.AddStaffRequirementAsync(dto).Returns(resultDto);
        _mapper.Map<StaffRequirementRes>(resultDto).Returns(res);

        var result = await _controller.AddStaffRequirement("2024/001", 2024, req);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal("2024/001", dto.Project);
        Assert.Equal(2024, dto.Year);
        await _service.Received(1).AddStaffRequirementAsync(dto);
    }

    #endregion

    #region UpdateStaffRequirement

    [Fact]
    public async Task UpdateStaffRequirement_SetsProjectYearAndIdentity_ReturnsOk()
    {
        var req = new StaffRequirementReq();
        var dto = new StaffRequirementDto();
        var resultDto = new StaffRequirementDto();
        var res = new StaffRequirementRes();

        _mapper.Map<StaffRequirementDto>(req).Returns(dto);
        _service.UpdateStaffRequirementAsync(dto).Returns(resultDto);
        _mapper.Map<StaffRequirementRes>(resultDto).Returns(res);

        var result = await _controller.UpdateStaffRequirement("2024/001", 2024, 42, req);

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal("2024/001", dto.Project);
        Assert.Equal(2024, dto.Year);
        Assert.Equal(42, dto.SrIdentity);
    }

    #endregion

    #region DeleteStaffRequirement

    [Fact]
    public async Task DeleteStaffRequirement_ReturnsOk_WithDeleteResult()
    {
        _service.DeleteStaffRequirementAsync(42).Returns(true);

        var result = await _controller.DeleteStaffRequirement("2024/001", 2024, 42);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<bool>>(okResult.Value);
        Assert.True(apiResponse.Data);
        await _service.Received(1).DeleteStaffRequirementAsync(42);
    }

    #endregion

    #region GetTestRequirements

    [Fact]
    public async Task GetTestRequirements_ReturnsOk_WithMappedList()
    {
        var dtos = new List<TestRequirementDto> { new() { TestCode = "TC001" } };
        var resList = new List<TestRequirementRes> { new() { TestCode = "TC001" } };

        _service.GetTestRequirementsAsync("2024/001", 2024).Returns(dtos);
        _mapper.Map<List<TestRequirementRes>>(dtos).Returns(resList);

        var result = await _controller.GetTestRequirements("2024/001", 2024);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<List<TestRequirementRes>>>(okResult.Value);
        Assert.Single(apiResponse.Data!);
    }

    #endregion

    #region AddTestRequirement

    [Fact]
    public async Task AddTestRequirement_SetsProjectIdAndYear_ReturnsOk()
    {
        var req = new TestRequirementReq();
        var dto = new TestRequirementDto();
        var resultDto = new TestRequirementDto { TestCode = "TC001" };
        var res = new TestRequirementRes { TestCode = "TC001" };

        _mapper.Map<TestRequirementDto>(req).Returns(dto);
        _service.AddTestRequirementAsync(dto).Returns(resultDto);
        _mapper.Map<TestRequirementRes>(resultDto).Returns(res);

        var result = await _controller.AddTestRequirement("2024/001", 2024, req);

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal("2024/001", dto.Project);
        Assert.Equal(2024, dto.Year);
    }

    #endregion

    #region UpdateTestRequirement

    [Fact]
    public async Task UpdateTestRequirement_SetsProjectYearAndTestCode_ReturnsOk()
    {
        var req = new TestRequirementReq();
        var dto = new TestRequirementDto();
        var resultDto = new TestRequirementDto();
        var res = new TestRequirementRes();

        _mapper.Map<TestRequirementDto>(req).Returns(dto);
        _service.UpdateTestRequirementAsync(dto).Returns(resultDto);
        _mapper.Map<TestRequirementRes>(resultDto).Returns(res);

        var result = await _controller.UpdateTestRequirement("2024/001", 2024, "TC001", req);

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal("2024/001", dto.Project);
        Assert.Equal(2024, dto.Year);
        Assert.Equal("TC001", dto.TestCode);
    }

    #endregion

    #region DeleteTestRequirement

    [Fact]
    public async Task DeleteTestRequirement_ReturnsOk_WithDeleteResult()
    {
        _service.DeleteTestRequirementAsync("2024/001", 2024, "TC001").Returns(true);

        var result = await _controller.DeleteTestRequirement("2024/001", 2024, "TC001");

        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<bool>>(okResult.Value);
        Assert.True(apiResponse.Data);
    }

    #endregion

    #region GetAnimalRequirements

    [Fact]
    public async Task GetAnimalRequirements_ReturnsOk_WithMappedList()
    {
        var dtos = new List<AnimalRequirementDto> { new() { AnimalType = "CAT" } };
        var resList = new List<AnimalRequirementRes> { new() { AnimalType = "CAT" } };

        _service.GetAnimalRequirementsAsync("2024/001", 2024).Returns(dtos);
        _mapper.Map<List<AnimalRequirementRes>>(dtos).Returns(resList);

        var result = await _controller.GetAnimalRequirements("2024/001", 2024);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    #endregion

    #region AddAnimalRequirement

    [Fact]
    public async Task AddAnimalRequirement_SetsProjectIdAndYear_ReturnsOk()
    {
        var req = new AnimalRequirementReq();
        var dto = new AnimalRequirementDto();
        var resultDto = new AnimalRequirementDto { ArIdentity = 1 };
        var res = new AnimalRequirementRes { ArIdentity = 1 };

        _mapper.Map<AnimalRequirementDto>(req).Returns(dto);
        _service.AddAnimalRequirementAsync(dto).Returns(resultDto);
        _mapper.Map<AnimalRequirementRes>(resultDto).Returns(res);

        var result = await _controller.AddAnimalRequirement("2024/001", 2024, req);

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal("2024/001", dto.Project);
        Assert.Equal(2024, dto.Year);
    }

    #endregion

    #region UpdateAnimalRequirement

    [Fact]
    public async Task UpdateAnimalRequirement_SetsProjectYearAndIdentity_ReturnsOk()
    {
        var req = new AnimalRequirementReq();
        var dto = new AnimalRequirementDto();

        _mapper.Map<AnimalRequirementDto>(req).Returns(dto);
        _service.UpdateAnimalRequirementAsync(dto).Returns(new AnimalRequirementDto());
        _mapper.Map<AnimalRequirementRes>(Arg.Any<AnimalRequirementDto>()).Returns(new AnimalRequirementRes());

        var result = await _controller.UpdateAnimalRequirement("2024/001", 2024, 99, req);

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal("2024/001", dto.Project);
        Assert.Equal(2024, dto.Year);
        Assert.Equal(99, dto.ArIdentity);
    }

    #endregion

    #region DeleteAnimalRequirement

    [Fact]
    public async Task DeleteAnimalRequirement_ReturnsOk_WithDeleteResult()
    {
        _service.DeleteAnimalRequirementAsync(99).Returns(true);

        var result = await _controller.DeleteAnimalRequirement("2024/001", 2024, 99);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<bool>>(okResult.Value);
        Assert.True(apiResponse.Data);
    }

    #endregion

    #region GetAdditionalCosts

    [Fact]
    public async Task GetAdditionalCosts_ReturnsOk_WithMappedList()
    {
        var dtos = new List<AdditionalCostDto> { new() { Description = "Travel" } };
        var resList = new List<AdditionalCostRes> { new() { Description = "Travel" } };

        _service.GetAdditionalCostsAsync("2024/001", 2024).Returns(dtos);
        _mapper.Map<List<AdditionalCostRes>>(dtos).Returns(resList);

        var result = await _controller.GetAdditionalCosts("2024/001", 2024);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    #endregion

    #region AddAdditionalCost

    [Fact]
    public async Task AddAdditionalCost_SetsProjectIdAndYear_ReturnsOk()
    {
        var req = new AdditionalCostReq();
        var dto = new AdditionalCostDto();
        var resultDto = new AdditionalCostDto { AcIdentity = 1 };
        var res = new AdditionalCostRes { AcIdentity = 1 };

        _mapper.Map<AdditionalCostDto>(req).Returns(dto);
        _service.AddAdditionalCostAsync(dto).Returns(resultDto);
        _mapper.Map<AdditionalCostRes>(resultDto).Returns(res);

        var result = await _controller.AddAdditionalCost("2024/001", 2024, req);

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal("2024/001", dto.Project);
        Assert.Equal(2024, dto.Year);
    }

    #endregion

    #region UpdateAdditionalCost

    [Fact]
    public async Task UpdateAdditionalCost_SetsProjectYearAndIdentity_ReturnsOk()
    {
        var req = new AdditionalCostReq();
        var dto = new AdditionalCostDto();

        _mapper.Map<AdditionalCostDto>(req).Returns(dto);
        _service.UpdateAdditionalCostAsync(dto).Returns(new AdditionalCostDto());
        _mapper.Map<AdditionalCostRes>(Arg.Any<AdditionalCostDto>()).Returns(new AdditionalCostRes());

        var result = await _controller.UpdateAdditionalCost("2024/001", 2024, 55, req);

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal("2024/001", dto.Project);
        Assert.Equal(2024, dto.Year);
        Assert.Equal(55, dto.AcIdentity);
    }

    #endregion

    #region DeleteAdditionalCost

    [Fact]
    public async Task DeleteAdditionalCost_ReturnsOk_WithDeleteResult()
    {
        _service.DeleteAdditionalCostAsync(55).Returns(true);

        var result = await _controller.DeleteAdditionalCost("2024/001", 2024, 55);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<bool>>(okResult.Value);
        Assert.True(apiResponse.Data);
    }

    #endregion

    #region Lookups

    [Fact]
    public async Task GetPayRates_ReturnsOk_WithMappedList()
    {
        var dtos = new List<PayRateDto> { new() { WgGrade = "HEO" } };
        var resList = new List<PayRateRes> { new() { WgGrade = "HEO" } };

        _service.GetPayRatesAsync(false).Returns(dtos);
        _mapper.Map<List<PayRateRes>>(dtos).Returns(resList);

        var result = await _controller.GetPayRates(false);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<List<PayRateRes>>>(okResult.Value);
        Assert.Single(apiResponse.Data!);
    }

    [Fact]
    public async Task GetAnimalRates_ReturnsOk_WithMappedList()
    {
        var dtos = new List<AnimalRateDto> { new() { AnimalType = "CAT" } };
        var resList = new List<AnimalRateRes> { new() { AnimalType = "CAT" } };

        _service.GetAnimalRatesAsync(true).Returns(dtos);
        _mapper.Map<List<AnimalRateRes>>(dtos).Returns(resList);

        var result = await _controller.GetAnimalRates(true);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetAccountCategories_ReturnsOk_WithMappedList()
    {
        var dtos = new List<AccountCategoryDto> { new() { AccShortName = "TRAVEL" } };
        var resList = new List<AccountCategoryRes> { new() { AccShortName = "TRAVEL" } };

        _service.GetAccountCategoriesAsync().Returns(dtos);
        _mapper.Map<List<AccountCategoryRes>>(dtos).Returns(resList);

        var result = await _controller.GetAccountCategories();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetTestCodeLookups_ReturnsOk_WithMappedList()
    {
        var dtos = new List<TestCodeLookupDto> { new() { ItemCode = "TC001" } };
        var resList = new List<TestCodeLookupRes> { new() { ItemCode = "TC001" } };

        _service.GetTestCodeLookupsAsync(false).Returns(dtos);
        _mapper.Map<List<TestCodeLookupRes>>(dtos).Returns(resList);

        var result = await _controller.GetTestCodeLookups(false);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetAllAnimals_ReturnsOk_WithMappedList()
    {
        var dtos = new List<AnimalLookupDto> { new() { AnimalType = "CAT" } };
        var resList = new List<AnimalLookupRes> { new() { AnimalType = "CAT" } };

        _service.GetAllAnimalsAsync().Returns(dtos);
        _mapper.Map<List<AnimalLookupRes>>(dtos).Returns(resList);

        var result = await _controller.GetAllAnimals();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    #endregion

    #region DeleteProjectYear

    [Fact]
    public async Task DeleteProjectYear_ReturnsOk_WhenDeletedSuccessfully()
    {
        _service.DeleteProjectYearAsync("2024/001", 2024)
            .Returns((true, (IReadOnlyList<string>)Array.Empty<string>()));

        var result = await _controller.DeleteProjectYear("2024/001", 2024);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<bool>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.True(apiResponse.Data);
        await _service.Received(1).DeleteProjectYearAsync("2024/001", 2024);
    }

    [Fact]
    public async Task DeleteProjectYear_ReturnsBadRequest_WhenChildRecordsExist()
    {
        var errors = new List<string> { "Year has 2 staff requirements. Remove them first." };
        _service.DeleteProjectYearAsync("2024/001", 2024)
            .Returns((false, (IReadOnlyList<string>)errors));

        var result = await _controller.DeleteProjectYear("2024/001", 2024);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<bool>>(badRequest.Value);
        Assert.False(apiResponse.Success);
        Assert.Single(apiResponse.Errors!);
        Assert.Contains("staff requirements", apiResponse.Errors![0].Message);
    }

    [Fact]
    public async Task DeleteProjectYear_ReturnsBadRequest_WithJoinedErrors_WhenMultipleChildTypesExist()
    {
        var errors = new List<string>
        {
            "Year has 1 staff requirements. Remove them first.",
            "Year has 1 test requirements. Remove them first."
        };
        _service.DeleteProjectYearAsync("2024/001", 2024)
            .Returns((false, (IReadOnlyList<string>)errors));

        var result = await _controller.DeleteProjectYear("2024/001", 2024);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<bool>>(badRequest.Value);
        Assert.False(apiResponse.Success);
        Assert.Contains("\n", apiResponse.Errors![0].Message);
    }

    [Fact]
    public async Task DeleteProjectYear_ReturnsOk_WhenYearNotFoundButNoErrors()
    {
        _service.DeleteProjectYearAsync("2024/001", 9999)
            .Returns((false, (IReadOnlyList<string>)Array.Empty<string>()));

        var result = await _controller.DeleteProjectYear("2024/001", 9999);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<bool>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.False(apiResponse.Data);
    }

    #endregion

    #region DeleteStaffRequirement - false result

    [Fact]
    public async Task DeleteStaffRequirement_ReturnsOk_WithFalse_WhenNotFound()
    {
        _service.DeleteStaffRequirementAsync(99).Returns(false);

        var result = await _controller.DeleteStaffRequirement("2024/001", 2024, 99);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<bool>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.False(apiResponse.Data);
    }

    #endregion

    #region DeleteTestRequirement - false result

    [Fact]
    public async Task DeleteTestRequirement_ReturnsOk_WithFalse_WhenNotFound()
    {
        _service.DeleteTestRequirementAsync("2024/001", 2024, "TC001").Returns(false);

        var result = await _controller.DeleteTestRequirement("2024/001", 2024, "TC001");

        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<bool>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.False(apiResponse.Data);
    }

    #endregion

    #region DeleteAnimalRequirement - false result

    [Fact]
    public async Task DeleteAnimalRequirement_ReturnsOk_WithFalse_WhenNotFound()
    {
        _service.DeleteAnimalRequirementAsync(99).Returns(false);

        var result = await _controller.DeleteAnimalRequirement("2024/001", 2024, 99);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<bool>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.False(apiResponse.Data);
    }

    #endregion

    #region DeleteAdditionalCost - false result

    [Fact]
    public async Task DeleteAdditionalCost_ReturnsOk_WithFalse_WhenNotFound()
    {
        _service.DeleteAdditionalCostAsync(55).Returns(false);

        var result = await _controller.DeleteAdditionalCost("2024/001", 2024, 55);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<bool>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.False(apiResponse.Data);
    }

    #endregion

    #region Lookups - isDefra variants and service delegation

    [Fact]
    public async Task GetPayRates_Defra_DelegatesToService_WithIsDefraTrue()
    {
        var dtos = new List<PayRateDto>();
        _service.GetPayRatesAsync(true).Returns(dtos);
        _mapper.Map<List<PayRateRes>>(dtos).Returns(new List<PayRateRes>());

        await _controller.GetPayRates(true);

        await _service.Received(1).GetPayRatesAsync(true);
    }

    [Fact]
    public async Task GetPayRates_DefaultsToNonDefra()
    {
        var dtos = new List<PayRateDto>();
        _service.GetPayRatesAsync(false).Returns(dtos);
        _mapper.Map<List<PayRateRes>>(dtos).Returns(new List<PayRateRes>());

        var result = await _controller.GetPayRates();

        Assert.IsType<OkObjectResult>(result);
        await _service.Received(1).GetPayRatesAsync(false);
    }

    [Fact]
    public async Task GetAnimalRates_DefaultsToNonDefra()
    {
        var dtos = new List<AnimalRateDto>();
        _service.GetAnimalRatesAsync(false).Returns(dtos);
        _mapper.Map<List<AnimalRateRes>>(dtos).Returns(new List<AnimalRateRes>());

        var result = await _controller.GetAnimalRates();

        Assert.IsType<OkObjectResult>(result);
        await _service.Received(1).GetAnimalRatesAsync(false);
    }

    [Fact]
    public async Task GetAnimalRates_NonDefra_DelegatesToService()
    {
        var dtos = new List<AnimalRateDto> { new() { AnimalType = "DOG" } };
        _service.GetAnimalRatesAsync(false).Returns(dtos);
        _mapper.Map<List<AnimalRateRes>>(dtos).Returns(new List<AnimalRateRes> { new() { AnimalType = "DOG" } });

        var result = await _controller.GetAnimalRates(false);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<List<AnimalRateRes>>>(okResult.Value);
        Assert.True(apiResponse.Success);
    }

    [Fact]
    public async Task GetTestCodeLookups_DefaultsToNonDefra()
    {
        var dtos = new List<TestCodeLookupDto>();
        _service.GetTestCodeLookupsAsync(false).Returns(dtos);
        _mapper.Map<List<TestCodeLookupRes>>(dtos).Returns(new List<TestCodeLookupRes>());

        var result = await _controller.GetTestCodeLookups();

        Assert.IsType<OkObjectResult>(result);
        await _service.Received(1).GetTestCodeLookupsAsync(false);
    }

    [Fact]
    public async Task GetTestCodeLookups_Defra_DelegatesToService_WithIsDefraTrue()
    {
        var dtos = new List<TestCodeLookupDto>();
        _service.GetTestCodeLookupsAsync(true).Returns(dtos);
        _mapper.Map<List<TestCodeLookupRes>>(dtos).Returns(new List<TestCodeLookupRes>());

        await _controller.GetTestCodeLookups(true);

        await _service.Received(1).GetTestCodeLookupsAsync(true);
    }

    [Fact]
    public async Task GetAccountCategories_ReturnsOk_AndApiResponseSuccessTrue()
    {
        var dtos = new List<AccountCategoryDto>();
        _service.GetAccountCategoriesAsync().Returns(dtos);
        _mapper.Map<List<AccountCategoryRes>>(dtos).Returns(new List<AccountCategoryRes>());

        var result = await _controller.GetAccountCategories();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<List<AccountCategoryRes>>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Empty(apiResponse.Errors!);
    }

    [Fact]
    public async Task GetAllAnimals_ReturnsOk_AndApiResponseSuccessTrue()
    {
        var dtos = new List<AnimalLookupDto>();
        _service.GetAllAnimalsAsync().Returns(dtos);
        _mapper.Map<List<AnimalLookupRes>>(dtos).Returns(new List<AnimalLookupRes>());

        var result = await _controller.GetAllAnimals();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<List<AnimalLookupRes>>>(okResult.Value);
        Assert.True(apiResponse.Success);
    }

    #endregion

    #region ApiResponse structure

    [Fact]
    public async Task GetProjectYears_ReturnsOk_WithSuccessTrueAndEmptyErrors()
    {
        var dtos = new List<ProjectYearDto>();
        _service.GetProjectYearsAsync("P1").Returns(dtos);
        _mapper.Map<List<ProjectYearRes>>(dtos).Returns(new List<ProjectYearRes>());

        var result = await _controller.GetProjectYears("P1");

        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<List<ProjectYearRes>>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Empty(apiResponse.Errors!);
        Assert.NotNull(apiResponse.Meta);
    }

    [Fact]
    public async Task DeleteProjectYear_BadRequest_HasNonNullMeta()
    {
        var errors = new List<string> { "Some error." };
        _service.DeleteProjectYearAsync("P1", 1)
            .Returns((false, (IReadOnlyList<string>)errors));

        var result = await _controller.DeleteProjectYear("P1", 1);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<bool>>(badRequest.Value);
        Assert.NotNull(apiResponse.Meta);
    }

    #endregion
}
