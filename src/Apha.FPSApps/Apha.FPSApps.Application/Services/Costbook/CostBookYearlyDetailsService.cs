using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.CostBookApiClients;
using Apha.FPSApps.Application.Interfaces.Costbook;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.Costbook;

public class CostBookYearlyDetailsService : ICostBookYearlyDetailsService
{
    private readonly ICostBookYearlyDetailsApiClient _client;

    public CostBookYearlyDetailsService(ICostBookYearlyDetailsApiClient client) => _client = client;

    public Task<ApiResponseDto<ProjectHeaderDto>> GetProjectHeaderAsync(string projectId)
        => _client.GetProjectHeaderAsync(projectId);

    public Task<ApiResponseDto<List<ProjectYearDto>>> GetProjectYearsAsync(string projectId)
        => _client.GetProjectYearsAsync(projectId);

    public Task<ApiResponseDto<ProjectYearDto>> AddProjectYearAsync(string projectId, int year, ProjectYearDto dto)
        => _client.AddProjectYearAsync(projectId, year, dto);

    public Task<ApiResponseDto<ProjectYearDto>> UpdateProjectYearAsync(string projectId, int year, ProjectYearDto dto)
        => _client.UpdateProjectYearAsync(projectId, year, dto);

    public Task<ApiResponseDto<PaginatedResult<StaffRequirementDto>>> GetStaffRequirementsAsync(
        string projectId, int year, QueryParameters<string> query)
        => _client.GetStaffRequirementsAsync(projectId, year, query);

    public Task<ApiResponseDto<StaffRequirementDto>> AddStaffRequirementAsync(string projectId, int year, StaffRequirementDto dto)
        => _client.AddStaffRequirementAsync(projectId, year, dto);

    public Task<ApiResponseDto<StaffRequirementDto>> UpdateStaffRequirementAsync(string projectId, int year, int srIdentity, StaffRequirementDto dto)
        => _client.UpdateStaffRequirementAsync(projectId, year, srIdentity, dto);

    public Task<ApiResponseDto<bool>> DeleteStaffRequirementAsync(string projectId, int year, int srIdentity)
        => _client.DeleteStaffRequirementAsync(projectId, year, srIdentity);

    public Task<ApiResponseDto<List<TestRequirementDto>>> GetTestRequirementsAsync(string projectId, int year)
        => _client.GetTestRequirementsAsync(projectId, year);

    public Task<ApiResponseDto<TestRequirementDto>> AddTestRequirementAsync(string projectId, int year, TestRequirementDto dto)
        => _client.AddTestRequirementAsync(projectId, year, dto);

    public Task<ApiResponseDto<TestRequirementDto>> UpdateTestRequirementAsync(string projectId, int year, string testCode, TestRequirementDto dto)
        => _client.UpdateTestRequirementAsync(projectId, year, testCode, dto);

    public Task<ApiResponseDto<bool>> DeleteTestRequirementAsync(string projectId, int year, string testCode)
        => _client.DeleteTestRequirementAsync(projectId, year, testCode);

    public Task<ApiResponseDto<List<AnimalRequirementDto>>> GetAnimalRequirementsAsync(string projectId, int year)
        => _client.GetAnimalRequirementsAsync(projectId, year);

    public Task<ApiResponseDto<AnimalRequirementDto>> AddAnimalRequirementAsync(string projectId, int year, AnimalRequirementDto dto)
        => _client.AddAnimalRequirementAsync(projectId, year, dto);

    public Task<ApiResponseDto<AnimalRequirementDto>> UpdateAnimalRequirementAsync(string projectId, int year, int arIdentity, AnimalRequirementDto dto)
        => _client.UpdateAnimalRequirementAsync(projectId, year, arIdentity, dto);

    public Task<ApiResponseDto<bool>> DeleteAnimalRequirementAsync(string projectId, int year, int arIdentity)
        => _client.DeleteAnimalRequirementAsync(projectId, year, arIdentity);

    public Task<ApiResponseDto<List<AdditionalCostDto>>> GetAdditionalCostsAsync(string projectId, int year)
        => _client.GetAdditionalCostsAsync(projectId, year);

    public Task<ApiResponseDto<AdditionalCostDto>> AddAdditionalCostAsync(string projectId, int year, AdditionalCostDto dto)
        => _client.AddAdditionalCostAsync(projectId, year, dto);

    public Task<ApiResponseDto<AdditionalCostDto>> UpdateAdditionalCostAsync(string projectId, int year, int acIdentity, AdditionalCostDto dto)
        => _client.UpdateAdditionalCostAsync(projectId, year, acIdentity, dto);

    public Task<ApiResponseDto<bool>> DeleteAdditionalCostAsync(string projectId, int year, int acIdentity)
        => _client.DeleteAdditionalCostAsync(projectId, year, acIdentity);

    public Task<ApiResponseDto<List<PayRateDto>>> GetPayRatesAsync(bool isDefra)
        => _client.GetPayRatesAsync(isDefra);

    public Task<ApiResponseDto<List<AnimalRateDto>>> GetAnimalRatesAsync(bool isDefra)
        => _client.GetAnimalRatesAsync(isDefra);

    public Task<ApiResponseDto<List<AccountCategoryDto>>> GetAccountCategoriesAsync()
        => _client.GetAccountCategoriesAsync();

    public Task<ApiResponseDto<List<TestCodeLookupDto>>> GetTestCodeLookupsAsync(bool isDefra)
        => _client.GetTestCodeLookupsAsync(isDefra);

    public Task<ApiResponseDto<List<AnimalLookupDto>>> GetAllAnimalsAsync()
        => _client.GetAllAnimalsAsync();
}
