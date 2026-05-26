using Apha.Common.Constants;
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
    public async Task GetTestRequirements_ReturnsOk_WithPaginatedResult()
    {
        var queryReq = new PaginationReq<string> { Page = 1 };
        var filter = new QueryParameters<string> { Page = 1 };
        var serviceResult = new PaginatedResult<TestRequirementDto>();
        var mappedResult = new PaginationRes<TestRequirementRes>();

        _mapper.Map<QueryParameters<string>>(queryReq).Returns(filter);
        _service.GetTestRequirementsAsync("2024/001", 2024, filter).Returns(serviceResult);
        _mapper.Map<PaginationRes<TestRequirementRes>>(serviceResult).Returns(mappedResult);

        var result = await _controller.GetTestRequirements("2024/001", 2024, queryReq);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        await _service.Received(1).GetTestRequirementsAsync("2024/001", 2024, filter);
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
    public async Task GetAnimalRequirements_ReturnsOk_WithPaginatedResult()
    {
        var queryReq = new PaginationReq<string> { Page = 1 };
        var filter = new QueryParameters<string> { Page = 1 };
        var serviceResult = new PaginatedResult<AnimalRequirementDto>();
        var mappedResult = new PaginationRes<AnimalRequirementRes>();

        _mapper.Map<QueryParameters<string>>(queryReq).Returns(filter);
        _service.GetAnimalRequirementsAsync("2024/001", 2024, filter).Returns(serviceResult);
        _mapper.Map<PaginationRes<AnimalRequirementRes>>(serviceResult).Returns(mappedResult);

        var result = await _controller.GetAnimalRequirements("2024/001", 2024, queryReq);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        await _service.Received(1).GetAnimalRequirementsAsync("2024/001", 2024, filter);
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
    public async Task GetAdditionalCosts_ReturnsOk_WithPaginatedResult()
    {
        var queryReq = new PaginationReq<string> { Page = 1 };
        var filter = new QueryParameters<string> { Page = 1 };
        var serviceResult = new PaginatedResult<AdditionalCostDto>();
        var mappedResult = new PaginationRes<AdditionalCostRes>();

        _mapper.Map<QueryParameters<string>>(queryReq).Returns(filter);
        _service.GetAdditionalCostsAsync("2024/001", 2024, filter).Returns(serviceResult);
        _mapper.Map<PaginationRes<AdditionalCostRes>>(serviceResult).Returns(mappedResult);

        var result = await _controller.GetAdditionalCosts("2024/001", 2024, queryReq);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        await _service.Received(1).GetAdditionalCostsAsync("2024/001", 2024, filter);
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

    #region AnimalRequirementReq - property coverage

    [Fact]
    public void AnimalRequirementReq_DefaultValues_AreCorrect()
    {
        var req = new AnimalRequirementReq();

        // null! suppresses the compiler warning but the runtime value is still null
        Assert.Null(req.ArIdentity);
        Assert.Null(req.Project);
        Assert.Equal(0, req.Year);
        Assert.Null(req.AnimalType);
        Assert.Null(req.NumberOfDays);
        Assert.Null(req.NumberOfAnimals);
        Assert.Null(req.DailyRate);
        Assert.Null(req.AnimalCost);
    }

    [Fact]
    public void AnimalRequirementReq_ArIdentity_CanBeSetNullAndNegative()
    {
        Assert.Equal(42,  new AnimalRequirementReq { ArIdentity = 42  }.ArIdentity);
        Assert.Null(      new AnimalRequirementReq { ArIdentity = null }.ArIdentity);
        Assert.Equal(0,   new AnimalRequirementReq { ArIdentity = 0   }.ArIdentity);
        Assert.Equal(-1,  new AnimalRequirementReq { ArIdentity = -1  }.ArIdentity);
    }

    [Fact]
    public void AnimalRequirementReq_Project_CanBeSetAndUpdated()
    {
        var req = new AnimalRequirementReq { Project = "2024/001" };
        Assert.Equal("2024/001", req.Project);

        req.Project = "2025/002";
        Assert.Equal("2025/002", req.Project);

        req.Project = "2024%2F001";
        Assert.Equal("2024%2F001", req.Project);
    }

    [Fact]
    public void AnimalRequirementReq_Year_CanBePositiveZeroAndNegative()
    {
        Assert.Equal(2024, new AnimalRequirementReq { Year = 2024 }.Year);
        Assert.Equal(0,    new AnimalRequirementReq { Year = 0    }.Year);
        Assert.Equal(-1,   new AnimalRequirementReq { Year = -1   }.Year);
    }

    [Fact]
    public void AnimalRequirementReq_AnimalType_CanBeSetAndUpdated()
    {
        var req = new AnimalRequirementReq { AnimalType = "CAT" };
        Assert.Equal("CAT", req.AnimalType);

        req.AnimalType = "DOG";
        Assert.Equal("DOG", req.AnimalType);

        req.AnimalType = "BOVINE CATTLE (DAIRY)";
        Assert.Equal("BOVINE CATTLE (DAIRY)", req.AnimalType);
    }

    [Fact]
    public void AnimalRequirementReq_NumberOfDays_CanBeSetNullZeroAndNegative()
    {
        Assert.Equal(10.5, new AnimalRequirementReq { NumberOfDays = 10.5 }.NumberOfDays);
        Assert.Null(       new AnimalRequirementReq { NumberOfDays = null }.NumberOfDays);
        Assert.Equal(0,    new AnimalRequirementReq { NumberOfDays = 0    }.NumberOfDays);
        Assert.Equal(-5.0, new AnimalRequirementReq { NumberOfDays = -5.0 }.NumberOfDays);
    }

    [Fact]
    public void AnimalRequirementReq_NumberOfAnimals_CanBeSetNullZeroAndFractional()
    {
        Assert.Equal(3.0,  new AnimalRequirementReq { NumberOfAnimals = 3.0  }.NumberOfAnimals);
        Assert.Null(       new AnimalRequirementReq { NumberOfAnimals = null }.NumberOfAnimals);
        Assert.Equal(0,    new AnimalRequirementReq { NumberOfAnimals = 0    }.NumberOfAnimals);
        Assert.Equal(1.5,  new AnimalRequirementReq { NumberOfAnimals = 1.5  }.NumberOfAnimals);
    }

    [Fact]
    public void AnimalRequirementReq_DailyRate_CanBeSetNullZeroAndNegative()
    {
        Assert.Equal(25.75, new AnimalRequirementReq { DailyRate = 25.75 }.DailyRate);
        Assert.Null(        new AnimalRequirementReq { DailyRate = null  }.DailyRate);
        Assert.Equal(0,     new AnimalRequirementReq { DailyRate = 0     }.DailyRate);
        Assert.Equal(-10.0, new AnimalRequirementReq { DailyRate = -10.0 }.DailyRate);
    }

    [Fact]
    public void AnimalRequirementReq_AnimalCost_CanBeSetNullAndZero()
    {
        Assert.Equal(525.0, new AnimalRequirementReq { AnimalCost = 525.0 }.AnimalCost);
        Assert.Null(        new AnimalRequirementReq { AnimalCost = null  }.AnimalCost);
        Assert.Equal(0,     new AnimalRequirementReq { AnimalCost = 0     }.AnimalCost);
    }

    [Fact]
    public void AnimalRequirementReq_AllPropertiesSet_AreCorrect()
    {
        var req = new AnimalRequirementReq
        {
            ArIdentity      = 7,
            Project         = "2024/001",
            Year            = 2024,
            AnimalType      = "CAT",
            NumberOfDays    = 5.0,
            NumberOfAnimals = 3.0,
            DailyRate       = 10.50,
            AnimalCost      = 157.50
        };

        Assert.Equal(7,          req.ArIdentity);
        Assert.Equal("2024/001", req.Project);
        Assert.Equal(2024,       req.Year);
        Assert.Equal("CAT",      req.AnimalType);
        Assert.Equal(5.0,        req.NumberOfDays);
        Assert.Equal(3.0,        req.NumberOfAnimals);
        Assert.Equal(10.50,      req.DailyRate);
        Assert.Equal(157.50,     req.AnimalCost);
    }

    [Fact]
    public void AnimalRequirementReq_AllNullablePropertiesNull_AreCorrect()
    {
        var req = new AnimalRequirementReq
        {
            ArIdentity      = null,
            Project         = "2024/001",
            Year            = 2024,
            AnimalType      = "CAT",
            NumberOfDays    = null,
            NumberOfAnimals = null,
            DailyRate       = null,
            AnimalCost      = null
        };

        Assert.Null(req.ArIdentity);
        Assert.Null(req.NumberOfDays);
        Assert.Null(req.NumberOfAnimals);
        Assert.Null(req.DailyRate);
        Assert.Null(req.AnimalCost);
    }

    #endregion

    #region AdditionalCostReq - property coverage

    [Fact]
    public void AdditionalCostReq_DefaultValues_AreCorrect()
    {
        var req = new AdditionalCostReq();

        Assert.Null(req.AcIdentity);
        Assert.Null(req.Project);
        Assert.Equal(0, req.Year);
        Assert.Null(req.AccountCat);
        Assert.Null(req.Description);
        Assert.Equal(0, req.CostEntered);
        Assert.Null(req.ItemCost);
        Assert.Null(req.Freq);
    }

    [Fact]
    public void AdditionalCostReq_AcIdentity_CanBeSetNullZeroAndNegative()
    {
        Assert.Equal(10,  new AdditionalCostReq { AcIdentity = 10  }.AcIdentity);
        Assert.Null(      new AdditionalCostReq { AcIdentity = null }.AcIdentity);
        Assert.Equal(0,   new AdditionalCostReq { AcIdentity = 0   }.AcIdentity);
        Assert.Equal(-1,  new AdditionalCostReq { AcIdentity = -1  }.AcIdentity);
    }

    [Fact]
    public void AdditionalCostReq_Project_CanBeSetAndUpdated()
    {
        var req = new AdditionalCostReq { Project = "2024/001" };
        Assert.Equal("2024/001", req.Project);

        req.Project = "2025/002";
        Assert.Equal("2025/002", req.Project);
    }

    [Fact]
    public void AdditionalCostReq_Year_CanBePositiveZeroAndNegative()
    {
        Assert.Equal(2024, new AdditionalCostReq { Year = 2024 }.Year);
        Assert.Equal(0,    new AdditionalCostReq { Year = 0    }.Year);
        Assert.Equal(-1,   new AdditionalCostReq { Year = -1   }.Year);
    }

    [Fact]
    public void AdditionalCostReq_AccountCatAndDescription_CanBeSet()
    {
        var req = new AdditionalCostReq { AccountCat = "TRAVEL", Description = "Travel expenses" };
        Assert.Equal("TRAVEL",           req.AccountCat);
        Assert.Equal("Travel expenses",  req.Description);

        req.AccountCat  = "EQUIP";
        req.Description = "Equipment";
        Assert.Equal("EQUIP",     req.AccountCat);
        Assert.Equal("Equipment", req.Description);
    }

    [Fact]
    public void AdditionalCostReq_CostEntered_CanBePositiveZeroAndNegative()
    {
        Assert.Equal(150.0,  new AdditionalCostReq { CostEntered = 150.0  }.CostEntered);
        Assert.Equal(0,      new AdditionalCostReq { CostEntered = 0      }.CostEntered);
        Assert.Equal(-50.0,  new AdditionalCostReq { CostEntered = -50.0  }.CostEntered);
    }

    [Fact]
    public void AdditionalCostReq_ItemCost_CanBeSetNullAndZero()
    {
        Assert.Equal(75.5, new AdditionalCostReq { ItemCost = 75.5  }.ItemCost);
        Assert.Null(       new AdditionalCostReq { ItemCost = null  }.ItemCost);
        Assert.Equal(0,    new AdditionalCostReq { ItemCost = 0     }.ItemCost);
    }

    [Fact]
    public void AdditionalCostReq_Freq_CanBeSetAndNull()
    {
        Assert.Equal("MONTHLY", new AdditionalCostReq { Freq = "MONTHLY" }.Freq);
        Assert.Null(            new AdditionalCostReq { Freq = null      }.Freq);
    }

    [Fact]
    public void AdditionalCostReq_AllPropertiesSet_AreCorrect()
    {
        var req = new AdditionalCostReq
        {
            AcIdentity   = 5,
            Project      = "2024/001",
            Year         = 2024,
            AccountCat   = "TRAVEL",
            Description  = "Travel expenses",
            CostEntered  = 200.0,
            ItemCost     = 100.0,
            Freq         = "MONTHLY"
        };

        Assert.Equal(5,                req.AcIdentity);
        Assert.Equal("2024/001",       req.Project);
        Assert.Equal(2024,             req.Year);
        Assert.Equal("TRAVEL",         req.AccountCat);
        Assert.Equal("Travel expenses",req.Description);
        Assert.Equal(200.0,            req.CostEntered);
        Assert.Equal(100.0,            req.ItemCost);
        Assert.Equal("MONTHLY",        req.Freq);
    }

    #endregion

    #region AddProjectYearReq - property coverage

    [Fact]
    public void AddProjectYearReq_DefaultValues_AreCorrect()
    {
        var req = new AddProjectYearReq();

        Assert.Null(req.Project);
        Assert.Equal(0, req.Year);
        Assert.Equal(0, req.YearValue);
        Assert.Null(req.MarkupTime);
        Assert.Null(req.MarkupTests);
        Assert.Null(req.MarkupAnimals);
        Assert.Null(req.MarkupAdditional);
        Assert.Null(req.ProfitTime);
        Assert.Null(req.ProfitTests);
        Assert.Null(req.ProfitAnimals);
        Assert.Null(req.ProfitAdditional);
    }

    [Fact]
    public void AddProjectYearReq_Project_CanBeSetAndUpdated()
    {
        var req = new AddProjectYearReq { Project = "2024/001" };
        Assert.Equal("2024/001", req.Project);

        req.Project = "2025/002";
        Assert.Equal("2025/002", req.Project);
    }

    [Fact]
    public void AddProjectYearReq_YearAndYearValue_CanBePositiveZeroAndNegative()
    {
        Assert.Equal(2024, new AddProjectYearReq { Year = 2024 }.Year);
        Assert.Equal(0,    new AddProjectYearReq { Year = 0    }.Year);
        Assert.Equal(1,    new AddProjectYearReq { YearValue = 1 }.YearValue);
        Assert.Equal(0,    new AddProjectYearReq { YearValue = 0 }.YearValue);
        Assert.Equal(-1,   new AddProjectYearReq { YearValue = -1 }.YearValue);
    }

    [Fact]
    public void AddProjectYearReq_MarkupProperties_CanBeSetNullAndZero()
    {
        Assert.Equal(10.0, new AddProjectYearReq { MarkupTime       = 10.0 }.MarkupTime);
        Assert.Equal(5.0,  new AddProjectYearReq { MarkupTests      = 5.0  }.MarkupTests);
        Assert.Equal(3.5,  new AddProjectYearReq { MarkupAnimals    = 3.5  }.MarkupAnimals);
        Assert.Equal(2.0,  new AddProjectYearReq { MarkupAdditional = 2.0  }.MarkupAdditional);
        Assert.Null(       new AddProjectYearReq { MarkupTime       = null }.MarkupTime);
        Assert.Equal(0,    new AddProjectYearReq { MarkupTests      = 0    }.MarkupTests);
    }

    [Fact]
    public void AddProjectYearReq_ProfitProperties_CanBeSetNullAndNegative()
    {
        Assert.Equal(15.0,  new AddProjectYearReq { ProfitTime       = 15.0  }.ProfitTime);
        Assert.Equal(8.0,   new AddProjectYearReq { ProfitTests      = 8.0   }.ProfitTests);
        Assert.Equal(4.5,   new AddProjectYearReq { ProfitAnimals    = 4.5   }.ProfitAnimals);
        Assert.Equal(3.0,   new AddProjectYearReq { ProfitAdditional = 3.0   }.ProfitAdditional);
        Assert.Null(        new AddProjectYearReq { ProfitTime       = null  }.ProfitTime);
        Assert.Equal(-1.0,  new AddProjectYearReq { ProfitTests      = -1.0  }.ProfitTests);
    }

    [Fact]
    public void AddProjectYearReq_AllPropertiesSet_AreCorrect()
    {
        var req = new AddProjectYearReq
        {
            Project          = "2024/001",
            Year             = 2024,
            YearValue        = 1,
            MarkupTime       = 10.0,
            MarkupTests      = 5.0,
            MarkupAnimals    = 3.5,
            MarkupAdditional = 2.0,
            ProfitTime       = 15.0,
            ProfitTests      = 8.0,
            ProfitAnimals    = 4.5,
            ProfitAdditional = 3.0
        };

        Assert.Equal("2024/001", req.Project);
        Assert.Equal(2024,       req.Year);
        Assert.Equal(1,          req.YearValue);
        Assert.Equal(10.0,       req.MarkupTime);
        Assert.Equal(5.0,        req.MarkupTests);
        Assert.Equal(3.5,        req.MarkupAnimals);
        Assert.Equal(2.0,        req.MarkupAdditional);
        Assert.Equal(15.0,       req.ProfitTime);
        Assert.Equal(8.0,        req.ProfitTests);
        Assert.Equal(4.5,        req.ProfitAnimals);
        Assert.Equal(3.0,        req.ProfitAdditional);
    }

    #endregion

    #region StaffRequirementReq - property coverage

    [Fact]
    public void StaffRequirementReq_DefaultValues_AreCorrect()
    {
        var req = new StaffRequirementReq();

        Assert.Null(req.SrIdentity);
        Assert.Null(req.Project);
        Assert.Equal(0, req.Year);
        Assert.Null(req.WgGrade);
        Assert.Null(req.Name);
        Assert.Null(req.Nohours);
        Assert.Null(req.Nodays);
        Assert.Null(req.Chargerate);
        Assert.Null(req.Payrate);
        Assert.Null(req.StaffCost);
        Assert.Null(req.Npr);
        Assert.Null(req.Ohr);
    }

    [Fact]
    public void StaffRequirementReq_SrIdentity_CanBeSetNullAndNegative()
    {
        Assert.Equal(99,  new StaffRequirementReq { SrIdentity = 99   }.SrIdentity);
        Assert.Null(      new StaffRequirementReq { SrIdentity = null }.SrIdentity);
        Assert.Equal(0,   new StaffRequirementReq { SrIdentity = 0    }.SrIdentity);
        Assert.Equal(-1,  new StaffRequirementReq { SrIdentity = -1   }.SrIdentity);
    }

    [Fact]
    public void StaffRequirementReq_Project_CanBeSetAndUpdated()
    {
        var req = new StaffRequirementReq { Project = "2024/001" };
        Assert.Equal("2024/001", req.Project);

        req.Project = "2025/002";
        Assert.Equal("2025/002", req.Project);
    }

    [Fact]
    public void StaffRequirementReq_Year_CanBePositiveZeroAndNegative()
    {
        Assert.Equal(2024, new StaffRequirementReq { Year = 2024 }.Year);
        Assert.Equal(0,    new StaffRequirementReq { Year = 0    }.Year);
        Assert.Equal(-1,   new StaffRequirementReq { Year = -1   }.Year);
    }

    [Fact]
    public void StaffRequirementReq_WgGradeAndName_CanBeSetAndNull()
    {
        var req = new StaffRequirementReq { WgGrade = "B1", Name = "John" };
        Assert.Equal("B1",   req.WgGrade);
        Assert.Equal("John", req.Name);

        req.Name = null;
        Assert.Null(req.Name);

        req.WgGrade = "C2";
        Assert.Equal("C2", req.WgGrade);
    }

    [Fact]
    public void StaffRequirementReq_NumericNullableProperties_CanBeSetNullAndZero()
    {
        Assert.Equal(7.5,  new StaffRequirementReq { Nohours    = 7.5  }.Nohours);
        Assert.Equal(3.0,  new StaffRequirementReq { Nodays     = 3.0  }.Nodays);
        Assert.Equal(25.0, new StaffRequirementReq { Chargerate = 25.0 }.Chargerate);
        Assert.Equal(20.0, new StaffRequirementReq { Payrate    = 20.0 }.Payrate);
        Assert.Equal(60.0, new StaffRequirementReq { StaffCost  = 60.0 }.StaffCost);
        Assert.Equal(1.2,  new StaffRequirementReq { Npr        = 1.2  }.Npr);
        Assert.Equal(0.5,  new StaffRequirementReq { Ohr        = 0.5  }.Ohr);
        Assert.Null(       new StaffRequirementReq { Nohours    = null }.Nohours);
        Assert.Equal(0,    new StaffRequirementReq { Nodays     = 0    }.Nodays);
    }

    [Fact]
    public void StaffRequirementReq_AllPropertiesSet_AreCorrect()
    {
        var req = new StaffRequirementReq
        {
            SrIdentity = 3,
            Project    = "2024/001",
            Year       = 2024,
            WgGrade    = "B1",
            Name       = "John",
            Nohours    = 7.5,
            Nodays     = 3.0,
            Chargerate = 25.0,
            Payrate    = 20.0,
            StaffCost  = 60.0,
            Npr        = 1.2,
            Ohr        = 0.5
        };

        Assert.Equal(3,          req.SrIdentity);
        Assert.Equal("2024/001", req.Project);
        Assert.Equal(2024,       req.Year);
        Assert.Equal("B1",       req.WgGrade);
        Assert.Equal("John",     req.Name);
        Assert.Equal(7.5,        req.Nohours);
        Assert.Equal(3.0,        req.Nodays);
        Assert.Equal(25.0,       req.Chargerate);
        Assert.Equal(20.0,       req.Payrate);
        Assert.Equal(60.0,       req.StaffCost);
        Assert.Equal(1.2,        req.Npr);
        Assert.Equal(0.5,        req.Ohr);
    }

    [Fact]
    public void StaffRequirementReq_AllNullablePropertiesNull_AreCorrect()
    {
        var req = new StaffRequirementReq
        {
            SrIdentity = null,
            Project    = "2024/001",
            Year       = 2024,
            WgGrade    = "B1",
            Name       = null,
            Nohours    = null,
            Nodays     = null,
            Chargerate = null,
            Payrate    = null,
            StaffCost  = null,
            Npr        = null,
            Ohr        = null
        };

        Assert.Null(req.SrIdentity);
        Assert.Null(req.Name);
        Assert.Null(req.Nohours);
        Assert.Null(req.Nodays);
        Assert.Null(req.Chargerate);
        Assert.Null(req.Payrate);
        Assert.Null(req.StaffCost);
        Assert.Null(req.Npr);
        Assert.Null(req.Ohr);
    }

    #endregion

    #region TestRequirementReq - property coverage

    [Fact]
    public void TestRequirementReq_DefaultValues_AreCorrect()
    {
        var req = new TestRequirementReq();

        Assert.Null(req.Project);
        Assert.Equal(0, req.Year);
        Assert.Null(req.TestCode);
        Assert.Null(req.NumberOfTests);
        Assert.Null(req.UnitPrice);
        Assert.Null(req.TestCost);
    }

    [Fact]
    public void TestRequirementReq_Project_CanBeSetAndUpdated()
    {
        var req = new TestRequirementReq { Project = "2024/001" };
        Assert.Equal("2024/001", req.Project);

        req.Project = "2025/002";
        Assert.Equal("2025/002", req.Project);
    }

    [Fact]
    public void TestRequirementReq_Year_CanBePositiveZeroAndNegative()
    {
        Assert.Equal(2024, new TestRequirementReq { Year = 2024 }.Year);
        Assert.Equal(0,    new TestRequirementReq { Year = 0    }.Year);
        Assert.Equal(-1,   new TestRequirementReq { Year = -1   }.Year);
    }

    [Fact]
    public void TestRequirementReq_TestCode_CanBeSetAndUpdated()
    {
        var req = new TestRequirementReq { TestCode = "TB-001" };
        Assert.Equal("TB-001", req.TestCode);

        req.TestCode = "TC-002";
        Assert.Equal("TC-002", req.TestCode);
    }

    [Fact]
    public void TestRequirementReq_NumericNullableProperties_CanBeSetNullAndZero()
    {
        Assert.Equal(5.0,  new TestRequirementReq { NumberOfTests = 5.0  }.NumberOfTests);
        Assert.Equal(12.5, new TestRequirementReq { UnitPrice     = 12.5 }.UnitPrice);
        Assert.Equal(62.5, new TestRequirementReq { TestCost      = 62.5 }.TestCost);
        Assert.Null(       new TestRequirementReq { NumberOfTests = null }.NumberOfTests);
        Assert.Equal(0,    new TestRequirementReq { UnitPrice     = 0    }.UnitPrice);
        Assert.Equal(-1.0, new TestRequirementReq { TestCost      = -1.0 }.TestCost);
    }

    [Fact]
    public void TestRequirementReq_AllPropertiesSet_AreCorrect()
    {
        var req = new TestRequirementReq
        {
            Project       = "2024/001",
            Year          = 2024,
            TestCode      = "TB-001",
            NumberOfTests = 5.0,
            UnitPrice     = 12.5,
            TestCost      = 62.5
        };

        Assert.Equal("2024/001", req.Project);
        Assert.Equal(2024,       req.Year);
        Assert.Equal("TB-001",   req.TestCode);
        Assert.Equal(5.0,        req.NumberOfTests);
        Assert.Equal(12.5,       req.UnitPrice);
        Assert.Equal(62.5,       req.TestCost);
    }

    [Fact]
    public void TestRequirementReq_AllNullablePropertiesNull_AreCorrect()
    {
        var req = new TestRequirementReq
        {
            Project       = "2024/001",
            Year          = 2024,
            TestCode      = "TB-001",
            NumberOfTests = null,
            UnitPrice     = null,
            TestCost      = null
        };

        Assert.Null(req.NumberOfTests);
        Assert.Null(req.UnitPrice);
        Assert.Null(req.TestCost);
    }

    #endregion

    #region CostBookApiEndpoints – YearlyDetails constants

    // Project Header & Years
    [Fact]
    public void CostBookApiEndpoints_GetProjectHeader_HasCorrectValue()
        => Assert.Equal("api/v1/yearlydetails/{0}/header", CostBookApiEndpoints.GetProjectHeader);

    [Fact]
    public void CostBookApiEndpoints_GetProjectYears_HasCorrectValue()
        => Assert.Equal("api/v1/yearlydetails/{0}/years", CostBookApiEndpoints.GetProjectYears);

    [Fact]
    public void CostBookApiEndpoints_AddProjectYear_HasCorrectValue()
        => Assert.Equal("api/v1/yearlydetails/{0}/years", CostBookApiEndpoints.AddProjectYear);

    [Fact]
    public void CostBookApiEndpoints_UpdateProjectYear_HasCorrectValue()
        => Assert.Equal("api/v1/yearlydetails/{0}/years/{1}", CostBookApiEndpoints.UpdateProjectYear);

    [Fact]
    public void CostBookApiEndpoints_DeleteProjectYear_HasCorrectValue()
        => Assert.Equal("api/v1/yearlydetails/{0}/years/{1}", CostBookApiEndpoints.DeleteProjectYear);

    // Staff
    [Fact]
    public void CostBookApiEndpoints_GetStaffRequirements_HasCorrectValue()
        => Assert.Equal("api/v1/yearlydetails/{0}/years/{1}/staff", CostBookApiEndpoints.GetStaffRequirements);

    [Fact]
    public void CostBookApiEndpoints_AddStaffRequirement_HasCorrectValue()
        => Assert.Equal("api/v1/yearlydetails/{0}/years/{1}/staff", CostBookApiEndpoints.AddStaffRequirement);

    [Fact]
    public void CostBookApiEndpoints_UpdateStaffRequirement_HasCorrectValue()
        => Assert.Equal("api/v1/yearlydetails/{0}/years/{1}/staff/{2}", CostBookApiEndpoints.UpdateStaffRequirement);

    [Fact]
    public void CostBookApiEndpoints_DeleteStaffRequirement_HasCorrectValue()
        => Assert.Equal("api/v1/yearlydetails/{0}/years/{1}/staff/{2}", CostBookApiEndpoints.DeleteStaffRequirement);

    // Tests
    [Fact]
    public void CostBookApiEndpoints_GetTestRequirements_HasCorrectValue()
        => Assert.Equal("api/v1/yearlydetails/{0}/years/{1}/tests", CostBookApiEndpoints.GetTestRequirements);

    [Fact]
    public void CostBookApiEndpoints_AddTestRequirement_HasCorrectValue()
        => Assert.Equal("api/v1/yearlydetails/{0}/years/{1}/tests", CostBookApiEndpoints.AddTestRequirement);

    [Fact]
    public void CostBookApiEndpoints_UpdateTestRequirement_HasCorrectValue()
        => Assert.Equal("api/v1/yearlydetails/{0}/years/{1}/tests/{2}", CostBookApiEndpoints.UpdateTestRequirement);

    [Fact]
    public void CostBookApiEndpoints_DeleteTestRequirement_HasCorrectValue()
        => Assert.Equal("api/v1/yearlydetails/{0}/years/{1}/tests/{2}", CostBookApiEndpoints.DeleteTestRequirement);

    // Animals
    [Fact]
    public void CostBookApiEndpoints_GetAnimalRequirements_HasCorrectValue()
        => Assert.Equal("api/v1/yearlydetails/{0}/years/{1}/animals", CostBookApiEndpoints.GetAnimalRequirements);

    [Fact]
    public void CostBookApiEndpoints_AddAnimalRequirement_HasCorrectValue()
        => Assert.Equal("api/v1/yearlydetails/{0}/years/{1}/animals", CostBookApiEndpoints.AddAnimalRequirement);

    [Fact]
    public void CostBookApiEndpoints_UpdateAnimalRequirement_HasCorrectValue()
        => Assert.Equal("api/v1/yearlydetails/{0}/years/{1}/animals/{2}", CostBookApiEndpoints.UpdateAnimalRequirement);

    [Fact]
    public void CostBookApiEndpoints_DeleteAnimalRequirement_HasCorrectValue()
        => Assert.Equal("api/v1/yearlydetails/{0}/years/{1}/animals/{2}", CostBookApiEndpoints.DeleteAnimalRequirement);

    // Additional Costs
    [Fact]
    public void CostBookApiEndpoints_GetAdditionalCosts_HasCorrectValue()
        => Assert.Equal("api/v1/yearlydetails/{0}/years/{1}/additionalcosts", CostBookApiEndpoints.GetAdditionalCosts);

    [Fact]
    public void CostBookApiEndpoints_AddAdditionalCost_HasCorrectValue()
        => Assert.Equal("api/v1/yearlydetails/{0}/years/{1}/additionalcosts", CostBookApiEndpoints.AddAdditionalCost);

    [Fact]
    public void CostBookApiEndpoints_UpdateAdditionalCost_HasCorrectValue()
        => Assert.Equal("api/v1/yearlydetails/{0}/years/{1}/additionalcosts/{2}", CostBookApiEndpoints.UpdateAdditionalCost);

    [Fact]
    public void CostBookApiEndpoints_DeleteAdditionalCost_HasCorrectValue()
        => Assert.Equal("api/v1/yearlydetails/{0}/years/{1}/additionalcosts/{2}", CostBookApiEndpoints.DeleteAdditionalCost);

    // Lookups
    [Fact]
    public void CostBookApiEndpoints_GetPayRates_HasCorrectValue()
        => Assert.Equal("api/v1/yearlydetails/lookups/payrates", CostBookApiEndpoints.GetPayRates);

    [Fact]
    public void CostBookApiEndpoints_GetAnimalRates_HasCorrectValue()
        => Assert.Equal("api/v1/yearlydetails/lookups/animalrates", CostBookApiEndpoints.GetAnimalRates);

    [Fact]
    public void CostBookApiEndpoints_GetAccountCategories_HasCorrectValue()
        => Assert.Equal("api/v1/yearlydetails/lookups/accountcategories", CostBookApiEndpoints.GetAccountCategories);

    [Fact]
    public void CostBookApiEndpoints_GetTestCodeLookups_HasCorrectValue()
        => Assert.Equal("api/v1/yearlydetails/lookups/testcodes", CostBookApiEndpoints.GetTestCodeLookups);

    [Fact]
    public void CostBookApiEndpoints_GetAllAnimals_HasCorrectValue()
        => Assert.Equal("api/v1/yearlydetails/lookups/animals", CostBookApiEndpoints.GetAllAnimals);

    // Formatted URL spot-checks
    [Fact]
    public void CostBookApiEndpoints_YearlyDetails_FormattedEndpoints_ProduceCorrectUrls()
    {
        Assert.Equal("api/v1/yearlydetails/P001/header",                          string.Format(CostBookApiEndpoints.GetProjectHeader,        "P001"));
        Assert.Equal("api/v1/yearlydetails/P001/years",                           string.Format(CostBookApiEndpoints.GetProjectYears,         "P001"));
        Assert.Equal("api/v1/yearlydetails/P001/years/2024",                      string.Format(CostBookApiEndpoints.UpdateProjectYear,       "P001", 2024));
        Assert.Equal("api/v1/yearlydetails/P001/years/2024/staff",                string.Format(CostBookApiEndpoints.GetStaffRequirements,    "P001", 2024));
        Assert.Equal("api/v1/yearlydetails/P001/years/2024/staff/5",              string.Format(CostBookApiEndpoints.UpdateStaffRequirement,  "P001", 2024, 5));
        Assert.Equal("api/v1/yearlydetails/P001/years/2024/tests",                string.Format(CostBookApiEndpoints.GetTestRequirements,     "P001", 2024));
        Assert.Equal("api/v1/yearlydetails/P001/years/2024/tests/TB-001",         string.Format(CostBookApiEndpoints.UpdateTestRequirement,   "P001", 2024, "TB-001"));
        Assert.Equal("api/v1/yearlydetails/P001/years/2024/animals",              string.Format(CostBookApiEndpoints.GetAnimalRequirements,   "P001", 2024));
        Assert.Equal("api/v1/yearlydetails/P001/years/2024/animals/3",            string.Format(CostBookApiEndpoints.UpdateAnimalRequirement, "P001", 2024, 3));
        Assert.Equal("api/v1/yearlydetails/P001/years/2024/additionalcosts",      string.Format(CostBookApiEndpoints.GetAdditionalCosts,      "P001", 2024));
        Assert.Equal("api/v1/yearlydetails/P001/years/2024/additionalcosts/7",    string.Format(CostBookApiEndpoints.UpdateAdditionalCost,    "P001", 2024, 7));
    }

    #endregion
}
