using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
{
    public interface IPimsProjectYearCostsApiClient
    {
        Task<ApiResponseDto<List<AdditionalCostDto>>> GetAdditionalActualsAsync(string project, short year, QueryParameters<string> query);
        Task<ApiResponseDto<List<AdditionalCostDto>>> GetAdditionalPlansAsync(string project, short year, QueryParameters<string> query);
        Task<ApiResponseDto<List<AnimalCostDto>>> GetAnimalActualsAsync(string project, short year, QueryParameters<string> query);
        Task<ApiResponseDto<List<AnimalCostDto>>> GetAnimalPlansAsync(string project, short year, QueryParameters<string> query);
        Task<ApiResponseDto<List<TestCostDto>>> GetTestPlansAsync(string project, short year, QueryParameters<string> query);
        Task<ApiResponseDto<List<TestCostDto>>> GetTestActualsAsync(string project, short year, QueryParameters<string> query);
        Task<ApiResponseDto<List<StaffCostDto>>> GetStaffPlansAsync(string project, short year, QueryParameters<string> query);
        Task<ApiResponseDto<List<StaffCostDto>>> GetStaffActualsAsync(string project, short year, QueryParameters<string> query);
        Task<ApiResponseDto<ProjectYearDetailsDto>> GetProjectYearDetailsAsync(string project, short year);
    }
}
