using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.CostBookApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.Costbook;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Costbook.CostBookYearlyDetailsServiceTest;

public class CostBookYearlyDetailsServiceTests
{
    private readonly ICostBookYearlyDetailsApiClient _client;
    private readonly CostBookYearlyDetailsService _sut;

    public CostBookYearlyDetailsServiceTests()
    {
        _client = Substitute.For<ICostBookYearlyDetailsApiClient>();
        _sut = new CostBookYearlyDetailsService(_client);
    }

    #region GetProjectHeaderAsync

    [Fact]
    public async Task GetProjectHeaderAsync_WithSuccessResponse_ReturnsHeader()
    {
        var expected = ApiResponseDto<ProjectHeaderDto>.SuccessResponse(
            new ProjectHeaderDto { ProjectId = "2024/001", ProjectTitle = "Test" });
        _client.GetProjectHeaderAsync("2024/001").Returns(expected);

        var result = await _sut.GetProjectHeaderAsync("2024/001");

        Assert.True(result.Success);
        Assert.Equal("2024/001", result.Data!.ProjectId);
        await _client.Received(1).GetProjectHeaderAsync("2024/001");
    }

    [Fact]
    public async Task GetProjectHeaderAsync_WhenApiFails_ReturnsFailureResponse()
    {
        var expected = ApiResponseDto<ProjectHeaderDto>.FailureResponse(
            new List<ApiErrorDto> { new() { Code = "NOT_FOUND", Message = "Not found" } }, new ApiMetaDto());
        _client.GetProjectHeaderAsync("INVALID").Returns(expected);

        var result = await _sut.GetProjectHeaderAsync("INVALID");

        Assert.False(result.Success);
        Assert.Single(result.Errors!);
    }

    #endregion

    #region GetProjectYearsAsync

    [Fact]
    public async Task GetProjectYearsAsync_WithSuccessResponse_ReturnsYears()
    {
        var years = new List<ProjectYearDto> { new() { YearValue = 1 }, new() { YearValue = 2 } };
        _client.GetProjectYearsAsync("2024/001").Returns(ApiResponseDto<List<ProjectYearDto>>.SuccessResponse(years));

        var result = await _sut.GetProjectYearsAsync("2024/001");

        Assert.True(result.Success);
        Assert.Equal(2, result.Data!.Count);
        await _client.Received(1).GetProjectYearsAsync("2024/001");
    }

    [Fact]
    public async Task GetProjectYearsAsync_WithEmptyResult_ReturnsEmptyList()
    {
        _client.GetProjectYearsAsync("2024/001")
            .Returns(ApiResponseDto<List<ProjectYearDto>>.SuccessResponse(new List<ProjectYearDto>()));

        var result = await _sut.GetProjectYearsAsync("2024/001");

        Assert.True(result.Success);
        Assert.Empty(result.Data!);
    }

    #endregion

    #region AddProjectYearAsync

    [Fact]
    public async Task AddProjectYearAsync_DelegatesToClient()
    {
        var dto = new ProjectYearDto { Project = "2024/001", YearValue = 2 };
        var expected = ApiResponseDto<ProjectYearDto>.SuccessResponse(dto);
        _client.AddProjectYearAsync("2024/001", 2, dto).Returns(expected);

        var result = await _sut.AddProjectYearAsync("2024/001", 2, dto);

        Assert.True(result.Success);
        Assert.Equal(2, result.Data!.YearValue);
        await _client.Received(1).AddProjectYearAsync("2024/001", 2, dto);
    }

    #endregion

    #region UpdateProjectYearAsync

    [Fact]
    public async Task UpdateProjectYearAsync_DelegatesToClient()
    {
        var dto = new ProjectYearDto { Project = "2024/001", YearValue = 1 };
        _client.UpdateProjectYearAsync("2024/001", 1, dto)
            .Returns(ApiResponseDto<ProjectYearDto>.SuccessResponse(dto));

        var result = await _sut.UpdateProjectYearAsync("2024/001", 1, dto);

        Assert.True(result.Success);
        await _client.Received(1).UpdateProjectYearAsync("2024/001", 1, dto);
    }

    #endregion

    #region Staff Requirements

    [Fact]
    public async Task GetStaffRequirementsAsync_DelegatesToClient()
    {
        var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
        var pagedResult = new PaginatedResult<StaffRequirementDto>(
            new List<StaffRequirementDto> { new() { SrIdentity = 1 } }, 1);
        _client.GetStaffRequirementsAsync("2024/001", 2024, query)
            .Returns(ApiResponseDto<PaginatedResult<StaffRequirementDto>>.SuccessResponse(pagedResult));

        var result = await _sut.GetStaffRequirementsAsync("2024/001", 2024, query);

        Assert.True(result.Success);
        Assert.Single(result.Data!.data);
        await _client.Received(1).GetStaffRequirementsAsync("2024/001", 2024, query);
    }

    [Fact]
    public async Task AddStaffRequirementAsync_DelegatesToClient()
    {
        var dto = new StaffRequirementDto { WgGrade = "HEO" };
        _client.AddStaffRequirementAsync("2024/001", 2024, dto)
            .Returns(ApiResponseDto<StaffRequirementDto>.SuccessResponse(dto));

        var result = await _sut.AddStaffRequirementAsync("2024/001", 2024, dto);

        Assert.True(result.Success);
        await _client.Received(1).AddStaffRequirementAsync("2024/001", 2024, dto);
    }

    [Fact]
    public async Task UpdateStaffRequirementAsync_DelegatesToClient()
    {
        var dto = new StaffRequirementDto { SrIdentity = 1 };
        _client.UpdateStaffRequirementAsync("2024/001", 2024, 1, dto)
            .Returns(ApiResponseDto<StaffRequirementDto>.SuccessResponse(dto));

        var result = await _sut.UpdateStaffRequirementAsync("2024/001", 2024, 1, dto);

        Assert.True(result.Success);
        await _client.Received(1).UpdateStaffRequirementAsync("2024/001", 2024, 1, dto);
    }

    [Fact]
    public async Task DeleteStaffRequirementAsync_DelegatesToClient()
    {
        _client.DeleteStaffRequirementAsync("2024/001", 2024, 1)
            .Returns(ApiResponseDto<bool>.SuccessResponse(true));

        var result = await _sut.DeleteStaffRequirementAsync("2024/001", 2024, 1);

        Assert.True(result.Success);
        Assert.True(result.Data);
        await _client.Received(1).DeleteStaffRequirementAsync("2024/001", 2024, 1);
    }

    [Fact]
    public async Task DeleteStaffRequirementAsync_WhenApiFails_ReturnsFailure()
    {
        _client.DeleteStaffRequirementAsync("2024/001", 2024, 999)
            .Returns(ApiResponseDto<bool>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "ERR", Message = "Failed" } }, new ApiMetaDto()));

        var result = await _sut.DeleteStaffRequirementAsync("2024/001", 2024, 999);

        Assert.False(result.Success);
    }

    #endregion

    #region Test Requirements

    [Fact]
    public async Task GetTestRequirementsAsync_DelegatesToClient()
    {
        var tests = new List<TestRequirementDto> { new() { TestCode = "TC001" } };
        _client.GetTestRequirementsAsync("2024/001", 2024)
            .Returns(ApiResponseDto<List<TestRequirementDto>>.SuccessResponse(tests));

        var result = await _sut.GetTestRequirementsAsync("2024/001", 2024);

        Assert.True(result.Success);
        Assert.Single(result.Data!);
        await _client.Received(1).GetTestRequirementsAsync("2024/001", 2024);
    }

    [Fact]
    public async Task AddTestRequirementAsync_DelegatesToClient()
    {
        var dto = new TestRequirementDto { TestCode = "TC001" };
        _client.AddTestRequirementAsync("2024/001", 2024, dto)
            .Returns(ApiResponseDto<TestRequirementDto>.SuccessResponse(dto));

        var result = await _sut.AddTestRequirementAsync("2024/001", 2024, dto);

        Assert.True(result.Success);
        await _client.Received(1).AddTestRequirementAsync("2024/001", 2024, dto);
    }

    [Fact]
    public async Task UpdateTestRequirementAsync_DelegatesToClient()
    {
        var dto = new TestRequirementDto { TestCode = "TC001" };
        _client.UpdateTestRequirementAsync("2024/001", 2024, "TC001", dto)
            .Returns(ApiResponseDto<TestRequirementDto>.SuccessResponse(dto));

        var result = await _sut.UpdateTestRequirementAsync("2024/001", 2024, "TC001", dto);

        Assert.True(result.Success);
        await _client.Received(1).UpdateTestRequirementAsync("2024/001", 2024, "TC001", dto);
    }

    [Fact]
    public async Task DeleteTestRequirementAsync_DelegatesToClient()
    {
        _client.DeleteTestRequirementAsync("2024/001", 2024, "TC001")
            .Returns(ApiResponseDto<bool>.SuccessResponse(true));

        var result = await _sut.DeleteTestRequirementAsync("2024/001", 2024, "TC001");

        Assert.True(result.Success);
        Assert.True(result.Data);
        await _client.Received(1).DeleteTestRequirementAsync("2024/001", 2024, "TC001");
    }

    #endregion

    #region Animal Requirements

    [Fact]
    public async Task GetAnimalRequirementsAsync_DelegatesToClient()
    {
        var animals = new List<AnimalRequirementDto> { new() { ArIdentity = 1, AnimalType = "CAT" } };
        _client.GetAnimalRequirementsAsync("2024/001", 2024)
            .Returns(ApiResponseDto<List<AnimalRequirementDto>>.SuccessResponse(animals));

        var result = await _sut.GetAnimalRequirementsAsync("2024/001", 2024);

        Assert.True(result.Success);
        Assert.Single(result.Data!);
        await _client.Received(1).GetAnimalRequirementsAsync("2024/001", 2024);
    }

    [Fact]
    public async Task AddAnimalRequirementAsync_DelegatesToClient()
    {
        var dto = new AnimalRequirementDto { AnimalType = "CAT" };
        _client.AddAnimalRequirementAsync("2024/001", 2024, dto)
            .Returns(ApiResponseDto<AnimalRequirementDto>.SuccessResponse(dto));

        var result = await _sut.AddAnimalRequirementAsync("2024/001", 2024, dto);

        Assert.True(result.Success);
        await _client.Received(1).AddAnimalRequirementAsync("2024/001", 2024, dto);
    }

    [Fact]
    public async Task UpdateAnimalRequirementAsync_DelegatesToClient()
    {
        var dto = new AnimalRequirementDto { ArIdentity = 1 };
        _client.UpdateAnimalRequirementAsync("2024/001", 2024, 1, dto)
            .Returns(ApiResponseDto<AnimalRequirementDto>.SuccessResponse(dto));

        var result = await _sut.UpdateAnimalRequirementAsync("2024/001", 2024, 1, dto);

        Assert.True(result.Success);
        await _client.Received(1).UpdateAnimalRequirementAsync("2024/001", 2024, 1, dto);
    }

    [Fact]
    public async Task DeleteAnimalRequirementAsync_DelegatesToClient()
    {
        _client.DeleteAnimalRequirementAsync("2024/001", 2024, 1)
            .Returns(ApiResponseDto<bool>.SuccessResponse(true));

        var result = await _sut.DeleteAnimalRequirementAsync("2024/001", 2024, 1);

        Assert.True(result.Success);
        Assert.True(result.Data);
        await _client.Received(1).DeleteAnimalRequirementAsync("2024/001", 2024, 1);
    }

    #endregion

    #region Additional Costs

    [Fact]
    public async Task GetAdditionalCostsAsync_DelegatesToClient()
    {
        var costs = new List<AdditionalCostDto> { new() { AcIdentity = 1, Description = "Travel" } };
        _client.GetAdditionalCostsAsync("2024/001", 2024)
            .Returns(ApiResponseDto<List<AdditionalCostDto>>.SuccessResponse(costs));

        var result = await _sut.GetAdditionalCostsAsync("2024/001", 2024);

        Assert.True(result.Success);
        Assert.Single(result.Data!);
        await _client.Received(1).GetAdditionalCostsAsync("2024/001", 2024);
    }

    [Fact]
    public async Task AddAdditionalCostAsync_DelegatesToClient()
    {
        var dto = new AdditionalCostDto { Description = "Travel" };
        _client.AddAdditionalCostAsync("2024/001", 2024, dto)
            .Returns(ApiResponseDto<AdditionalCostDto>.SuccessResponse(dto));

        var result = await _sut.AddAdditionalCostAsync("2024/001", 2024, dto);

        Assert.True(result.Success);
        await _client.Received(1).AddAdditionalCostAsync("2024/001", 2024, dto);
    }

    [Fact]
    public async Task UpdateAdditionalCostAsync_DelegatesToClient()
    {
        var dto = new AdditionalCostDto { AcIdentity = 1 };
        _client.UpdateAdditionalCostAsync("2024/001", 2024, 1, dto)
            .Returns(ApiResponseDto<AdditionalCostDto>.SuccessResponse(dto));

        var result = await _sut.UpdateAdditionalCostAsync("2024/001", 2024, 1, dto);

        Assert.True(result.Success);
        await _client.Received(1).UpdateAdditionalCostAsync("2024/001", 2024, 1, dto);
    }

    [Fact]
    public async Task DeleteAdditionalCostAsync_DelegatesToClient()
    {
        _client.DeleteAdditionalCostAsync("2024/001", 2024, 1)
            .Returns(ApiResponseDto<bool>.SuccessResponse(true));

        var result = await _sut.DeleteAdditionalCostAsync("2024/001", 2024, 1);

        Assert.True(result.Success);
        Assert.True(result.Data);
        await _client.Received(1).DeleteAdditionalCostAsync("2024/001", 2024, 1);
    }

    #endregion

    #region Lookups

    [Fact]
    public async Task GetPayRatesAsync_DelegatesToClient()
    {
        var rates = new List<PayRateDto> { new() { WgGrade = "HEO" } };
        _client.GetPayRatesAsync(false).Returns(ApiResponseDto<List<PayRateDto>>.SuccessResponse(rates));

        var result = await _sut.GetPayRatesAsync(false);

        Assert.True(result.Success);
        Assert.Single(result.Data!);
        await _client.Received(1).GetPayRatesAsync(false);
    }

    [Fact]
    public async Task GetAnimalRatesAsync_DelegatesToClient()
    {
        var rates = new List<AnimalRateDto> { new() { AnimalType = "CAT" } };
        _client.GetAnimalRatesAsync(true).Returns(ApiResponseDto<List<AnimalRateDto>>.SuccessResponse(rates));

        var result = await _sut.GetAnimalRatesAsync(true);

        Assert.True(result.Success);
        Assert.Single(result.Data!);
        await _client.Received(1).GetAnimalRatesAsync(true);
    }

    [Fact]
    public async Task GetAccountCategoriesAsync_DelegatesToClient()
    {
        var cats = new List<AccountCategoryDto> { new() { AccShortName = "TRAVEL" } };
        _client.GetAccountCategoriesAsync().Returns(ApiResponseDto<List<AccountCategoryDto>>.SuccessResponse(cats));

        var result = await _sut.GetAccountCategoriesAsync();

        Assert.True(result.Success);
        Assert.Single(result.Data!);
        await _client.Received(1).GetAccountCategoriesAsync();
    }

    [Fact]
    public async Task GetTestCodeLookupsAsync_DelegatesToClient()
    {
        var lookups = new List<TestCodeLookupDto> { new() { ItemCode = "TC001" } };
        _client.GetTestCodeLookupsAsync(false).Returns(ApiResponseDto<List<TestCodeLookupDto>>.SuccessResponse(lookups));

        var result = await _sut.GetTestCodeLookupsAsync(false);

        Assert.True(result.Success);
        Assert.Single(result.Data!);
        await _client.Received(1).GetTestCodeLookupsAsync(false);
    }

    [Fact]
    public async Task GetAllAnimalsAsync_DelegatesToClient()
    {
        var animals = new List<AnimalLookupDto> { new() { AnimalType = "CAT" } };
        _client.GetAllAnimalsAsync().Returns(ApiResponseDto<List<AnimalLookupDto>>.SuccessResponse(animals));

        var result = await _sut.GetAllAnimalsAsync();

        Assert.True(result.Success);
        Assert.Single(result.Data!);
        await _client.Received(1).GetAllAnimalsAsync();
    }

    #endregion
}
