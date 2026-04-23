using Apha.Common.Constants;
using Apha.Common.Contracts.Costbook;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.CostBookApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using System.Web;

namespace Apha.FPSApps.Infrastructure.Integrations.CostBookApis.Clients;

public class CostBookYearlyDetailsApiClient : ICostBookYearlyDetailsApiClient
{
    private readonly ICostBookHttpExecutor _http;
    private readonly IMapper _mapper;

    public CostBookYearlyDetailsApiClient(ICostBookHttpExecutor http, IMapper mapper)
    {
        _http = http;
        _mapper = mapper;
    }

    public async Task<ApiResponseDto<ProjectHeaderDto>> GetProjectHeaderAsync(string projectId)
    {
        var response = await _http.GetAsync<ProjectHeaderRes>(
            string.Format(CostBookApiEndpoints.GetProjectHeader, HttpUtility.UrlEncode(projectId)));
        if (response.Success && response.Data != null)
            return ApiResponseDto<ProjectHeaderDto>.SuccessResponse(_mapper.Map<ProjectHeaderDto>(response.Data));
        var err = _mapper.Map<ApiResponseDto<ProjectHeaderDto>>(response);
        return ApiResponseDto<ProjectHeaderDto>.FailureResponse(err.Errors, err.Meta);
    }

    public async Task<ApiResponseDto<List<ProjectYearDto>>> GetProjectYearsAsync(string projectId)
    {
        var response = await _http.GetAsync<List<ProjectYearRes>>(
            string.Format(CostBookApiEndpoints.GetProjectYears, HttpUtility.UrlEncode(projectId)));
        if (response.Success && response.Data != null)
            return ApiResponseDto<List<ProjectYearDto>>.SuccessResponse(_mapper.Map<List<ProjectYearDto>>(response.Data));
        var err = _mapper.Map<ApiResponseDto<List<ProjectYearDto>>>(response);
        return ApiResponseDto<List<ProjectYearDto>>.FailureResponse(err.Errors, err.Meta);
    }

    public async Task<ApiResponseDto<ProjectYearDto>> AddProjectYearAsync(string projectId, int year)
    {
        var req = new AddProjectYearReq { Project = projectId, Year = year };
        var response = await _http.PostAsync<AddProjectYearReq, ProjectYearRes>(
            string.Format(CostBookApiEndpoints.AddProjectYear, HttpUtility.UrlEncode(projectId)), req);
        if (response.Success && response.Data != null)
            return ApiResponseDto<ProjectYearDto>.SuccessResponse(_mapper.Map<ProjectYearDto>(response.Data));
        var err = _mapper.Map<ApiResponseDto<ProjectYearDto>>(response);
        return ApiResponseDto<ProjectYearDto>.FailureResponse(err.Errors, err.Meta);
    }

    public async Task<ApiResponseDto<ProjectYearDto>> UpdateProjectYearAsync(string projectId, int year, ProjectYearDto dto)
    {
        var req = _mapper.Map<ProjectYearReq>(dto);
        var response = await _http.PutAsync<ProjectYearReq, ProjectYearRes>(
            string.Format(CostBookApiEndpoints.UpdateProjectYear, HttpUtility.UrlEncode(projectId), year), req);
        if (response.Success && response.Data != null)
            return ApiResponseDto<ProjectYearDto>.SuccessResponse(_mapper.Map<ProjectYearDto>(response.Data));
        var err = _mapper.Map<ApiResponseDto<ProjectYearDto>>(response);
        return ApiResponseDto<ProjectYearDto>.FailureResponse(err.Errors, err.Meta);
    }

    // ── Staff ─────────────────────────────────────────────────────────────────

    public async Task<ApiResponseDto<List<StaffRequirementDto>>> GetStaffRequirementsAsync(string projectId, int year)
    {
        var response = await _http.GetAsync<List<StaffRequirementRes>>(
            string.Format(CostBookApiEndpoints.GetStaffRequirements, HttpUtility.UrlEncode(projectId), year));
        if (response.Success && response.Data != null)
            return ApiResponseDto<List<StaffRequirementDto>>.SuccessResponse(_mapper.Map<List<StaffRequirementDto>>(response.Data));
        var err = _mapper.Map<ApiResponseDto<List<StaffRequirementDto>>>(response);
        return ApiResponseDto<List<StaffRequirementDto>>.FailureResponse(err.Errors, err.Meta);
    }

    public async Task<ApiResponseDto<StaffRequirementDto>> AddStaffRequirementAsync(string projectId, int year, StaffRequirementDto dto)
    {
        var req = _mapper.Map<StaffRequirementReq>(dto);
        var response = await _http.PostAsync<StaffRequirementReq, StaffRequirementRes>(
            string.Format(CostBookApiEndpoints.AddStaffRequirement, HttpUtility.UrlEncode(projectId), year), req);
        if (response.Success && response.Data != null)
            return ApiResponseDto<StaffRequirementDto>.SuccessResponse(_mapper.Map<StaffRequirementDto>(response.Data));
        var err = _mapper.Map<ApiResponseDto<StaffRequirementDto>>(response);
        return ApiResponseDto<StaffRequirementDto>.FailureResponse(err.Errors, err.Meta);
    }

    public async Task<ApiResponseDto<StaffRequirementDto>> UpdateStaffRequirementAsync(string projectId, int year, int srIdentity, StaffRequirementDto dto)
    {
        var req = _mapper.Map<StaffRequirementReq>(dto);
        var response = await _http.PutAsync<StaffRequirementReq, StaffRequirementRes>(
            string.Format(CostBookApiEndpoints.UpdateStaffRequirement, HttpUtility.UrlEncode(projectId), year, srIdentity), req);
        if (response.Success && response.Data != null)
            return ApiResponseDto<StaffRequirementDto>.SuccessResponse(_mapper.Map<StaffRequirementDto>(response.Data));
        var err = _mapper.Map<ApiResponseDto<StaffRequirementDto>>(response);
        return ApiResponseDto<StaffRequirementDto>.FailureResponse(err.Errors, err.Meta);
    }

    public async Task<ApiResponseDto<bool>> DeleteStaffRequirementAsync(string projectId, int year, int srIdentity)
    {
        var response = await _http.DeleteAsync<bool>(
            string.Format(CostBookApiEndpoints.DeleteStaffRequirement, HttpUtility.UrlEncode(projectId), year, srIdentity));
        if (response.Success)
            return ApiResponseDto<bool>.SuccessResponse(response.Data);
        var err = _mapper.Map<ApiResponseDto<bool>>(response);
        return ApiResponseDto<bool>.FailureResponse(err.Errors, err.Meta);
    }

    // ── Tests ─────────────────────────────────────────────────────────────────

    public async Task<ApiResponseDto<List<TestRequirementDto>>> GetTestRequirementsAsync(string projectId, int year)
    {
        var response = await _http.GetAsync<List<TestRequirementRes>>(
            string.Format(CostBookApiEndpoints.GetTestRequirements, HttpUtility.UrlEncode(projectId), year));
        if (response.Success && response.Data != null)
            return ApiResponseDto<List<TestRequirementDto>>.SuccessResponse(_mapper.Map<List<TestRequirementDto>>(response.Data));
        var err = _mapper.Map<ApiResponseDto<List<TestRequirementDto>>>(response);
        return ApiResponseDto<List<TestRequirementDto>>.FailureResponse(err.Errors, err.Meta);
    }

    public async Task<ApiResponseDto<TestRequirementDto>> AddTestRequirementAsync(string projectId, int year, TestRequirementDto dto)
    {
        var req = _mapper.Map<TestRequirementReq>(dto);
        var response = await _http.PostAsync<TestRequirementReq, TestRequirementRes>(
            string.Format(CostBookApiEndpoints.AddTestRequirement, HttpUtility.UrlEncode(projectId), year), req);
        if (response.Success && response.Data != null)
            return ApiResponseDto<TestRequirementDto>.SuccessResponse(_mapper.Map<TestRequirementDto>(response.Data));
        var err = _mapper.Map<ApiResponseDto<TestRequirementDto>>(response);
        return ApiResponseDto<TestRequirementDto>.FailureResponse(err.Errors, err.Meta);
    }

    public async Task<ApiResponseDto<TestRequirementDto>> UpdateTestRequirementAsync(string projectId, int year, string testCode, TestRequirementDto dto)
    {
        var req = _mapper.Map<TestRequirementReq>(dto);
        var response = await _http.PutAsync<TestRequirementReq, TestRequirementRes>(
            string.Format(CostBookApiEndpoints.UpdateTestRequirement, HttpUtility.UrlEncode(projectId), year, HttpUtility.UrlEncode(testCode)), req);
        if (response.Success && response.Data != null)
            return ApiResponseDto<TestRequirementDto>.SuccessResponse(_mapper.Map<TestRequirementDto>(response.Data));
        var err = _mapper.Map<ApiResponseDto<TestRequirementDto>>(response);
        return ApiResponseDto<TestRequirementDto>.FailureResponse(err.Errors, err.Meta);
    }

    public async Task<ApiResponseDto<bool>> DeleteTestRequirementAsync(string projectId, int year, string testCode)
    {
        var response = await _http.DeleteAsync<bool>(
            string.Format(CostBookApiEndpoints.DeleteTestRequirement, HttpUtility.UrlEncode(projectId), year, HttpUtility.UrlEncode(testCode)));
        if (response.Success)
            return ApiResponseDto<bool>.SuccessResponse(response.Data);
        var err = _mapper.Map<ApiResponseDto<bool>>(response);
        return ApiResponseDto<bool>.FailureResponse(err.Errors, err.Meta);
    }

    // ── Animals ───────────────────────────────────────────────────────────────

    public async Task<ApiResponseDto<List<AnimalRequirementDto>>> GetAnimalRequirementsAsync(string projectId, int year)
    {
        var response = await _http.GetAsync<List<AnimalRequirementRes>>(
            string.Format(CostBookApiEndpoints.GetAnimalRequirements, HttpUtility.UrlEncode(projectId), year));
        if (response.Success && response.Data != null)
            return ApiResponseDto<List<AnimalRequirementDto>>.SuccessResponse(_mapper.Map<List<AnimalRequirementDto>>(response.Data));
        var err = _mapper.Map<ApiResponseDto<List<AnimalRequirementDto>>>(response);
        return ApiResponseDto<List<AnimalRequirementDto>>.FailureResponse(err.Errors, err.Meta);
    }

    public async Task<ApiResponseDto<AnimalRequirementDto>> AddAnimalRequirementAsync(string projectId, int year, AnimalRequirementDto dto)
    {
        var req = _mapper.Map<AnimalRequirementReq>(dto);
        var response = await _http.PostAsync<AnimalRequirementReq, AnimalRequirementRes>(
            string.Format(CostBookApiEndpoints.AddAnimalRequirement, HttpUtility.UrlEncode(projectId), year), req);
        if (response.Success && response.Data != null)
            return ApiResponseDto<AnimalRequirementDto>.SuccessResponse(_mapper.Map<AnimalRequirementDto>(response.Data));
        var err = _mapper.Map<ApiResponseDto<AnimalRequirementDto>>(response);
        return ApiResponseDto<AnimalRequirementDto>.FailureResponse(err.Errors, err.Meta);
    }

    public async Task<ApiResponseDto<AnimalRequirementDto>> UpdateAnimalRequirementAsync(string projectId, int year, int arIdentity, AnimalRequirementDto dto)
    {
        var req = _mapper.Map<AnimalRequirementReq>(dto);
        var response = await _http.PutAsync<AnimalRequirementReq, AnimalRequirementRes>(
            string.Format(CostBookApiEndpoints.UpdateAnimalRequirement, HttpUtility.UrlEncode(projectId), year, arIdentity), req);
        if (response.Success && response.Data != null)
            return ApiResponseDto<AnimalRequirementDto>.SuccessResponse(_mapper.Map<AnimalRequirementDto>(response.Data));
        var err = _mapper.Map<ApiResponseDto<AnimalRequirementDto>>(response);
        return ApiResponseDto<AnimalRequirementDto>.FailureResponse(err.Errors, err.Meta);
    }

    public async Task<ApiResponseDto<bool>> DeleteAnimalRequirementAsync(string projectId, int year, int arIdentity)
    {
        var response = await _http.DeleteAsync<bool>(
            string.Format(CostBookApiEndpoints.DeleteAnimalRequirement, HttpUtility.UrlEncode(projectId), year, arIdentity));
        if (response.Success)
            return ApiResponseDto<bool>.SuccessResponse(response.Data);
        var err = _mapper.Map<ApiResponseDto<bool>>(response);
        return ApiResponseDto<bool>.FailureResponse(err.Errors, err.Meta);
    }

    // ── Additional Costs ──────────────────────────────────────────────────────

    public async Task<ApiResponseDto<List<AdditionalCostDto>>> GetAdditionalCostsAsync(string projectId, int year)
    {
        var response = await _http.GetAsync<List<AdditionalCostRes>>(
            string.Format(CostBookApiEndpoints.GetAdditionalCosts, HttpUtility.UrlEncode(projectId), year));
        if (response.Success && response.Data != null)
            return ApiResponseDto<List<AdditionalCostDto>>.SuccessResponse(_mapper.Map<List<AdditionalCostDto>>(response.Data));
        var err = _mapper.Map<ApiResponseDto<List<AdditionalCostDto>>>(response);
        return ApiResponseDto<List<AdditionalCostDto>>.FailureResponse(err.Errors, err.Meta);
    }

    public async Task<ApiResponseDto<AdditionalCostDto>> AddAdditionalCostAsync(string projectId, int year, AdditionalCostDto dto)
    {
        var req = _mapper.Map<AdditionalCostReq>(dto);
        var response = await _http.PostAsync<AdditionalCostReq, AdditionalCostRes>(
            string.Format(CostBookApiEndpoints.AddAdditionalCost, HttpUtility.UrlEncode(projectId), year), req);
        if (response.Success && response.Data != null)
            return ApiResponseDto<AdditionalCostDto>.SuccessResponse(_mapper.Map<AdditionalCostDto>(response.Data));
        var err = _mapper.Map<ApiResponseDto<AdditionalCostDto>>(response);
        return ApiResponseDto<AdditionalCostDto>.FailureResponse(err.Errors, err.Meta);
    }

    public async Task<ApiResponseDto<AdditionalCostDto>> UpdateAdditionalCostAsync(string projectId, int year, int acIdentity, AdditionalCostDto dto)
    {
        var req = _mapper.Map<AdditionalCostReq>(dto);
        var response = await _http.PutAsync<AdditionalCostReq, AdditionalCostRes>(
            string.Format(CostBookApiEndpoints.UpdateAdditionalCost, HttpUtility.UrlEncode(projectId), year, acIdentity), req);
        if (response.Success && response.Data != null)
            return ApiResponseDto<AdditionalCostDto>.SuccessResponse(_mapper.Map<AdditionalCostDto>(response.Data));
        var err = _mapper.Map<ApiResponseDto<AdditionalCostDto>>(response);
        return ApiResponseDto<AdditionalCostDto>.FailureResponse(err.Errors, err.Meta);
    }

    public async Task<ApiResponseDto<bool>> DeleteAdditionalCostAsync(string projectId, int year, int acIdentity)
    {
        var response = await _http.DeleteAsync<bool>(
            string.Format(CostBookApiEndpoints.DeleteAdditionalCost, HttpUtility.UrlEncode(projectId), year, acIdentity));
        if (response.Success)
            return ApiResponseDto<bool>.SuccessResponse(response.Data);
        var err = _mapper.Map<ApiResponseDto<bool>>(response);
        return ApiResponseDto<bool>.FailureResponse(err.Errors, err.Meta);
    }

    // ── Lookups ───────────────────────────────────────────────────────────────

    public async Task<ApiResponseDto<List<PayRateDto>>> GetPayRatesAsync(bool isDefra)
    {
        var response = await _http.GetAsync<List<PayRateRes>>($"{CostBookApiEndpoints.GetPayRates}?isDefra={isDefra}");
        if (response.Success && response.Data != null)
            return ApiResponseDto<List<PayRateDto>>.SuccessResponse(_mapper.Map<List<PayRateDto>>(response.Data));
        var err = _mapper.Map<ApiResponseDto<List<PayRateDto>>>(response);
        return ApiResponseDto<List<PayRateDto>>.FailureResponse(err.Errors, err.Meta);
    }

    public async Task<ApiResponseDto<List<AnimalRateDto>>> GetAnimalRatesAsync(bool isDefra)
    {
        var response = await _http.GetAsync<List<AnimalRateRes>>($"{CostBookApiEndpoints.GetAnimalRates}?isDefra={isDefra}");
        if (response.Success && response.Data != null)
            return ApiResponseDto<List<AnimalRateDto>>.SuccessResponse(_mapper.Map<List<AnimalRateDto>>(response.Data));
        var err = _mapper.Map<ApiResponseDto<List<AnimalRateDto>>>(response);
        return ApiResponseDto<List<AnimalRateDto>>.FailureResponse(err.Errors, err.Meta);
    }

    public async Task<ApiResponseDto<List<AccountCategoryDto>>> GetAccountCategoriesAsync()
    {
        var response = await _http.GetAsync<List<AccountCategoryRes>>(CostBookApiEndpoints.GetAccountCategories);
        if (response.Success && response.Data != null)
            return ApiResponseDto<List<AccountCategoryDto>>.SuccessResponse(_mapper.Map<List<AccountCategoryDto>>(response.Data));
        var err = _mapper.Map<ApiResponseDto<List<AccountCategoryDto>>>(response);
        return ApiResponseDto<List<AccountCategoryDto>>.FailureResponse(err.Errors, err.Meta);
    }
}
