using Apha.Common.Constants;
using Apha.Common.Contracts.FPS;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsProjectApiClient : IFpsProjectApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string InternalCodeError = "INTERNAL_ERROR";

        public FpsProjectApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<ProjectDto>>> GetAllProjectsAsync()
        {
            var response = await _http.GetAsync<List<ProjectRes>>(FpsApiEndpoints.GetAllProjects);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<ProjectDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<ProjectDto>>>(response);
            return ApiResponseDto<List<ProjectDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<ProjectDto>>> GetAllPactProjectsAsync()
        {
            var response = await _http.GetAsync<List<ProjectRes>>(FpsApiEndpoints.GetAllPactProjects);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<ProjectDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<ProjectDto>>>(response);
            return ApiResponseDto<List<ProjectDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<ProjectDto>>> GetPagedProjectsAsync(QueryParameters<string> query)
        {
            var url = QueryStringHelper.AddQueryString(FpsApiEndpoints.GetPagedProjects, query);
            var response = await _http.GetAsync<List<ProjectRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<ProjectDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<ProjectDto>>>(response);
            return ApiResponseDto<List<ProjectDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<ProjectDto>>> GetPagedPactProjectsAsync(QueryParameters<string> query)
        {
            var url = QueryStringHelper.AddQueryString(FpsApiEndpoints.GetPagedPactProjects, query);
            var response = await _http.GetAsync<List<ProjectRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<ProjectDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<ProjectDto>>>(response);
            return ApiResponseDto<List<ProjectDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<ProjectDto>> GetProjectByIdAsync(string parentProject)
        {
            var response = await _http.GetAsync<ProjectRes>(string.Format(FpsApiEndpoints.GetProjectById, Uri.EscapeDataString(parentProject)));
            if (response.Success)
                return _mapper.Map<ApiResponseDto<ProjectDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<ProjectDto>>(response);
            return ApiResponseDto<ProjectDto>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<ProjectDto>> CreateProjectAsync(ProjectDto project)
        {
            var request = _mapper.Map<ProjectReq>(project);
            var response = await _http.PostAsync<ProjectReq, ProjectRes>(FpsApiEndpoints.CreateProject, request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<ProjectDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<ProjectDto>>(response);
            return ApiResponseDto<ProjectDto>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<ProjectDto>> UpdateProjectAsync(ProjectDto project)
        {
            var request = _mapper.Map<ProjectReq>(project);
            var response = await _http.PutAsync<ProjectReq, ProjectRes>(FpsApiEndpoints.UpdateProject, request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<ProjectDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<ProjectDto>>(response);
            return ApiResponseDto<ProjectDto>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<ProjectDto>> UpdatePactProjectAsync(ProjectDto project)
        {
            var request = _mapper.Map<ProjectReq>(project);
            var response = await _http.PatchAsync<ProjectReq, ProjectRes>(FpsApiEndpoints.UpdatePactProject, request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<ProjectDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<ProjectDto>>(response);
            return ApiResponseDto<ProjectDto>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<ProjectDto>> UpdatePactPortfolioAsync(ProjectDto project)
        {
            var request = _mapper.Map<ProjectReq>(project);
            var response = await _http.PatchAsync<ProjectReq, ProjectRes>(FpsApiEndpoints.UpdatePactPortfolio, request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<ProjectDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<ProjectDto>>(response);
            return ApiResponseDto<ProjectDto>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<ProjectDto>> UpdateFpsPortfolioAsync(ProjectDto project)
        {
            var request = _mapper.Map<ProjectReq>(project);
            var response = await _http.PatchAsync<ProjectReq, ProjectRes>(FpsApiEndpoints.UpdateFpsPortfolio, request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<ProjectDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<ProjectDto>>(response);
            return ApiResponseDto<ProjectDto>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<bool>> DeleteProjectAsync(string parentProject)
        {
            var response = await _http.DeleteAsync<bool?>(string.Format(FpsApiEndpoints.DeleteProject, Uri.EscapeDataString(parentProject)));
            if (response.Success)
                return _mapper.Map<ApiResponseDto<bool>>(response);

            var dto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<ProjectDto>>> GetProjectsByProgramAsync(
            QueryParameters<string> query, string programNo)
        {
            var url = QueryStringHelper.AddQueryString(
                string.Format(FpsApiEndpoints.GetProjectsByProgram, Uri.EscapeDataString(programNo)), query);

            var response = await _http.GetAsync<List<ProjectRes>>(url);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<List<ProjectDto>>>(response);
            }

            var responseDto = _mapper.Map<ApiResponseDto<List<ProjectDto>>>(response);
            return ApiResponseDto<List<ProjectDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<ProjectDto>> UpdateProjectAsync(string parentProject, ProjectDto project)
        {
            var req = _mapper.Map<ProjectReq>(project);
            var response = await _http.PutAsync<ProjectReq, ProjectRes>(
                string.Format(FpsApiEndpoints.UpdateProgrammeNewProject, Uri.EscapeDataString(parentProject)), req);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<ProjectDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<ProjectDto>>(response);
            return ApiResponseDto<ProjectDto>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<bool>> DeleteProjectAndChildrenAsync(string parentProject)
        {
            var response = await _http.DeleteAsync<bool?>(
                string.Format(FpsApiEndpoints.DeleteProgrammeNewProjectAndChildren, Uri.EscapeDataString(parentProject)));
            if (response.Success)
                return _mapper.Map<ApiResponseDto<bool>>(response);

            var dto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<bool>> ChangeProjectCodeAsync(string oldCode, string newCode)
        {
            var req = new { OldCode = oldCode, NewCode = newCode };
            var response = await _http.PostAsync<object, bool?>(FpsApiEndpoints.ChangeProjectCode, req);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<bool>>(response);

            var dto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<bool>> CheckProjectExistsAsync(string code)
        {
            var response = await _http.GetAsync<bool>(
                string.Format(FpsApiEndpoints.CheckProjectExists, Uri.EscapeDataString(code)));
            if (response.Success)
                return _mapper.Map<ApiResponseDto<bool>>(response);

            var dto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<ManagerDto>>> GetManagersAsync()
        {
            var response = await _http.GetAsync<List<ManagerRes>>(FpsApiEndpoints.GetProgrammeNewProjectManagers);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<ManagerDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<ManagerDto>>>(response);
            return ApiResponseDto<List<ManagerDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<CostCentreWorkgroupDto>>> GetCostCentresAsync()
        {
            var response = await _http.GetAsync<List<CostCentreWorkgroupRes>>(FpsApiEndpoints.GetProgrammeNewProjectCostCentres);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<CostCentreWorkgroupDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<CostCentreWorkgroupDto>>>(response);
            return ApiResponseDto<List<CostCentreWorkgroupDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<ContractDto>>> GetContractsByUserAsync()
        {
            var response = await _http.GetAsync<List<ContractRes>>(FpsApiEndpoints.GetContractsByUser);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<ContractDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<ContractDto>>>(response);
            return ApiResponseDto<List<ContractDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<AccountCodeDto>>> GetAccountCodesAsync()
        {
            var response = await _http.GetAsync<List<AccountCodeRes>>(FpsApiEndpoints.GetProgrammeNewProjectAccountCodes);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<AccountCodeDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<AccountCodeDto>>>(response);
            return ApiResponseDto<List<AccountCodeDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<SubAccountDto>>> GetSubAccountsAsync()
        {
            var response = await _http.GetAsync<List<SubAccountRes>>(FpsApiEndpoints.GetProgrammeNewProjectSubAccounts);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<SubAccountDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<SubAccountDto>>>(response);
            return ApiResponseDto<List<SubAccountDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<ProjectProfitabilityDto>>> GetProjectProfitabilityAsync(
            QueryParameters<string> query, string programNo, string workTypeFilter)
        {
            var baseUrl = string.Format(FpsApiEndpoints.GetProjectProfitability, Uri.EscapeDataString(programNo));
            var url = QueryStringHelper.AddQueryString(baseUrl, query);
            url += (url.Contains('?') ? "&" : "?") + $"workTypeFilter={Uri.EscapeDataString(workTypeFilter)}";

            var response = await _http.GetAsync<List<ProjectProfitabilityRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<ProjectProfitabilityDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<ProjectProfitabilityDto>>>(response);
            return ApiResponseDto<List<ProjectProfitabilityDto>>.FailureResponse(dto.Errors, dto.Meta);
        }
    }
}

