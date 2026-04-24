using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Pagination;

namespace Apha.Costbook.Application.Interfaces;

public interface IYearlyDetailsService
{
    Task<ProjectHeaderDto?> GetProjectHeaderAsync(string projectId);
    Task<IEnumerable<ProjectYearDto>> GetProjectYearsAsync(string projectId);
    Task<ProjectYearDto> AddProjectYearAsync(string projectId, int year);
    Task<ProjectYearDto> UpdateProjectYearAsync(ProjectYearDto dto);

    // ?? Staff — now paginated ?????????????????????????????????????????????????
    Task<PaginatedResult<StaffRequirementDto>> GetStaffRequirementsAsync(string projectId, int year, QueryParameters<string> query);
    Task<StaffRequirementDto> AddStaffRequirementAsync(StaffRequirementDto dto);
    Task<StaffRequirementDto> UpdateStaffRequirementAsync(StaffRequirementDto dto);
    Task<bool> DeleteStaffRequirementAsync(int srIdentity);

    Task<IEnumerable<TestRequirementDto>> GetTestRequirementsAsync(string projectId, int year);
    Task<TestRequirementDto> AddTestRequirementAsync(TestRequirementDto dto);
    Task<TestRequirementDto> UpdateTestRequirementAsync(TestRequirementDto dto);
    Task<bool> DeleteTestRequirementAsync(string projectId, int year, string testCode);

    Task<IEnumerable<AnimalRequirementDto>> GetAnimalRequirementsAsync(string projectId, int year);
    Task<AnimalRequirementDto> AddAnimalRequirementAsync(AnimalRequirementDto dto);
    Task<AnimalRequirementDto> UpdateAnimalRequirementAsync(AnimalRequirementDto dto);
    Task<bool> DeleteAnimalRequirementAsync(int arIdentity);

    Task<IEnumerable<AdditionalCostDto>> GetAdditionalCostsAsync(string projectId, int year);
    Task<AdditionalCostDto> AddAdditionalCostAsync(AdditionalCostDto dto);
    Task<AdditionalCostDto> UpdateAdditionalCostAsync(AdditionalCostDto dto);
    Task<bool> DeleteAdditionalCostAsync(int acIdentity);

    Task<IEnumerable<PayRateDto>> GetPayRatesAsync(bool isDefra);
    Task<IEnumerable<AnimalRateDto>> GetAnimalRatesAsync(bool isDefra);
    Task<IEnumerable<AccountCategoryDto>> GetAccountCategoriesAsync();
}
