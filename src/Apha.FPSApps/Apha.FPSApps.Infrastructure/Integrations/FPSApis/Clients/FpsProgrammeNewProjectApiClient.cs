using Apha.Common.Constants;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsProgrammeNewProjectApiClient : IFpsProgrammeNewProjectApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string InternalCodeError = "INTERNAL_ERROR";

        public FpsProgrammeNewProjectApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<ProgrammeNewProjectDto>> GetProjectByIdAsync(string parentProject)
        {
            try
            {
                var response = await _http.GetAsync<ProgrammeNewProjectRes>(
                    string.Format(FpsApiEndpoints.GetProgrammeNewProjectById, Uri.EscapeDataString(parentProject)));
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ProgrammeNewProjectDto>>(response);

                var dto = _mapper.Map<ApiResponseDto<ProgrammeNewProjectDto>>(response);
                return ApiResponseDto<ProgrammeNewProjectDto>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ProgrammeNewProjectDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve project", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<ProgrammeNewProjectDto>> CreateProjectAsync(ProgrammeNewProjectDto project)
        {
            try
            {
                var req = _mapper.Map<ProgrammeNewProjectReq>(project);
                var response = await _http.PostAsync<ProgrammeNewProjectReq, ProgrammeNewProjectRes>(
                    FpsApiEndpoints.CreateProgrammeNewProject, req);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ProgrammeNewProjectDto>>(response);

                var dto = _mapper.Map<ApiResponseDto<ProgrammeNewProjectDto>>(response);
                return ApiResponseDto<ProgrammeNewProjectDto>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ProgrammeNewProjectDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to create project", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<ProgrammeNewProjectDto>> UpdateProjectAsync(string parentProject, ProgrammeNewProjectDto project)
        {
            try
            {
                var req = _mapper.Map<ProgrammeNewProjectReq>(project);
                var response = await _http.PutAsync<ProgrammeNewProjectReq, ProgrammeNewProjectRes>(
                    string.Format(FpsApiEndpoints.UpdateProgrammeNewProject, Uri.EscapeDataString(parentProject)), req);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ProgrammeNewProjectDto>>(response);

                var dto = _mapper.Map<ApiResponseDto<ProgrammeNewProjectDto>>(response);
                return ApiResponseDto<ProgrammeNewProjectDto>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ProgrammeNewProjectDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to update project", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteProjectAndChildrenAsync(string parentProject)
        {
            try
            {
                var response = await _http.DeleteAsync<bool?>(
                    string.Format(FpsApiEndpoints.DeleteProgrammeNewProjectAndChildren, Uri.EscapeDataString(parentProject)));
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var dto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to delete project", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<bool>> ChangeProjectCodeAsync(string oldCode, string newCode)
        {
            try
            {
                var req = new { OldCode = oldCode, NewCode = newCode };
                var response = await _http.PostAsync<object, bool?>(FpsApiEndpoints.ChangeProjectCode, req);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var dto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to change project code", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<bool>> CheckProjectExistsAsync(string code)
        {
            try
            {
                var response = await _http.GetAsync<bool>(
                    string.Format(FpsApiEndpoints.CheckProjectExists, Uri.EscapeDataString(code)));
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var dto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to check project existence", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<ManagerDto>>> GetManagersAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<ManagerRes>>(FpsApiEndpoints.GetProgrammeNewProjectManagers);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<ManagerDto>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<ManagerDto>>>(response);
                return ApiResponseDto<List<ManagerDto>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<ManagerDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve managers", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<CostCentreWorkgroupDto>>> GetCostCentresAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<CostCentreWorkgroupRes>>(FpsApiEndpoints.GetProgrammeNewProjectCostCentres);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<CostCentreWorkgroupDto>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<CostCentreWorkgroupDto>>>(response);
                return ApiResponseDto<List<CostCentreWorkgroupDto>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<CostCentreWorkgroupDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve cost centres", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<ProjectGroupDto>>> GetProjectGroupsAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<ProjectGroupRes>>(FpsApiEndpoints.GetProgrammeNewProjectProjectGroups);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<ProjectGroupDto>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<ProjectGroupDto>>>(response);
                return ApiResponseDto<List<ProjectGroupDto>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<ProjectGroupDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve project groups", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<AccountCodeDto>>> GetAccountCodesAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<AccountCodeRes>>(FpsApiEndpoints.GetProgrammeNewProjectAccountCodes);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<AccountCodeDto>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<AccountCodeDto>>>(response);
                return ApiResponseDto<List<AccountCodeDto>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<AccountCodeDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve account codes", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<SubAccountDto>>> GetSubAccountsAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<SubAccountRes>>(FpsApiEndpoints.GetProgrammeNewProjectSubAccounts);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<SubAccountDto>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<SubAccountDto>>>(response);
                return ApiResponseDto<List<SubAccountDto>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<SubAccountDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve sub accounts", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}
