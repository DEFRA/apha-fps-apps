using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.PIMS
{
    public class ProjectYearCostsService : IProjectYearCostsService
    {
        private readonly IPimsApiClient _client;

        public ProjectYearCostsService(IPimsApiClient client)
        {
            _client = client;
        }

        public async Task<ApiResponseDto<List<AdditionalCostDto>>> GetAdditionalActualsAsync(string project, short year, QueryParameters<string> query)
            => await _client.PimsProjectYearCosts.GetAdditionalActualsAsync(project, year, query);

        public async Task<ApiResponseDto<List<AdditionalCostDto>>> GetAdditionalPlansAsync(string project, short year, QueryParameters<string> query)
            => await _client.PimsProjectYearCosts.GetAdditionalPlansAsync(project, year, query);

        public async Task<ApiResponseDto<List<AnimalCostDto>>> GetAnimalActualsAsync(string project, short year, QueryParameters<string> query)
            => await _client.PimsProjectYearCosts.GetAnimalActualsAsync(project, year, query);

        public async Task<ApiResponseDto<List<AnimalCostDto>>> GetAnimalPlansAsync(string project, short year, QueryParameters<string> query)
            => await _client.PimsProjectYearCosts.GetAnimalPlansAsync(project, year, query);

        public async Task<ApiResponseDto<List<TestCostDto>>> GetTestPlansAsync(string project, short year, QueryParameters<string> query)
            => await _client.PimsProjectYearCosts.GetTestPlansAsync(project, year, query);

        public async Task<ApiResponseDto<List<TestCostDto>>> GetTestActualsAsync(string project, short year, QueryParameters<string> query)
            => await _client.PimsProjectYearCosts.GetTestActualsAsync(project, year, query);

        public async Task<ApiResponseDto<List<StaffCostDto>>> GetStaffPlansAsync(string project, short year, QueryParameters<string> query)
            => await _client.PimsProjectYearCosts.GetStaffPlansAsync(project, year, query);

        public async Task<ApiResponseDto<List<StaffCostDto>>> GetStaffActualsAsync(string project, short year, QueryParameters<string> query)
            => await _client.PimsProjectYearCosts.GetStaffActualsAsync(project, year, query);

        public async Task<ApiResponseDto<ProjectYearDetailsDto>> GetProjectYearDetailsAsync(string project, short year)
            => await _client.PimsProjectYearCosts.GetProjectYearDetailsAsync(project, year);
    }
}
