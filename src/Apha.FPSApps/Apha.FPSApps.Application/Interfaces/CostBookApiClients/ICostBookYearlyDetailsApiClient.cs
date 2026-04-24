using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.CostBookApiClients;

public interface ICostBookYearlyDetailsApiClient
{
    Task<ApiResponseDto<ProjectHeaderDto>> GetProjectHeaderAsync(string projectId);
    Task<ApiResponseDto<List<ProjectYearDto>>> GetProjectYearsAsync(string projectId);
    Task<ApiResponseDto<ProjectYearDto>> AddProjectYearAsync(string projectId, int year);
    Task<ApiResponseDto<ProjectYearDto>> UpdateProjectYearAsync(string projectId, int year, ProjectYearDto dto);

    // ── Staff — now paginated ─────────────────────────────────────────────────
    Task<ApiResponseDto<PaginatedResult<StaffRequirementDto>>> GetStaffRequirementsAsync(string projectId, int year, QueryParameters<string> query);
    Task<ApiResponseDto<StaffRequirementDto>> AddStaffRequirementAsync(string projectId, int year, StaffRequirementDto dto);
    Task<ApiResponseDto<StaffRequirementDto>> UpdateStaffRequirementAsync(string projectId, int year, int srIdentity, StaffRequirementDto dto);
    Task<ApiResponseDto<bool>> DeleteStaffRequirementAsync(string projectId, int year, int srIdentity);

    Task<ApiResponseDto<List<TestRequirementDto>>> GetTestRequirementsAsync(string projectId, int year);
    Task<ApiResponseDto<TestRequirementDto>> AddTestRequirementAsync(string projectId, int year, TestRequirementDto dto);
    Task<ApiResponseDto<TestRequirementDto>> UpdateTestRequirementAsync(string projectId, int year, string testCode, TestRequirementDto dto);
    Task<ApiResponseDto<bool>> DeleteTestRequirementAsync(string projectId, int year, string testCode);

    Task<ApiResponseDto<List<AnimalRequirementDto>>> GetAnimalRequirementsAsync(string projectId, int year);
    Task<ApiResponseDto<AnimalRequirementDto>> AddAnimalRequirementAsync(string projectId, int year, AnimalRequirementDto dto);
    Task<ApiResponseDto<AnimalRequirementDto>> UpdateAnimalRequirementAsync(string projectId, int year, int arIdentity, AnimalRequirementDto dto);
    Task<ApiResponseDto<bool>> DeleteAnimalRequirementAsync(string projectId, int year, int arIdentity);

    Task<ApiResponseDto<List<AdditionalCostDto>>> GetAdditionalCostsAsync(string projectId, int year);
    Task<ApiResponseDto<AdditionalCostDto>> AddAdditionalCostAsync(string projectId, int year, AdditionalCostDto dto);
    Task<ApiResponseDto<AdditionalCostDto>> UpdateAdditionalCostAsync(string projectId, int year, int acIdentity, AdditionalCostDto dto);
    Task<ApiResponseDto<bool>> DeleteAdditionalCostAsync(string projectId, int year, int acIdentity);

    Task<ApiResponseDto<List<PayRateDto>>> GetPayRatesAsync(bool isDefra);
    Task<ApiResponseDto<List<AnimalRateDto>>> GetAnimalRatesAsync(bool isDefra);
    Task<ApiResponseDto<List<AccountCategoryDto>>> GetAccountCategoriesAsync();
}
