using Apha.Common.Constants;
using Apha.Common.Contracts.Costbook;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.CostBookApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using System.Web;

namespace Apha.FPSApps.Infrastructure.Integrations.CostBookApis.Clients;

public class CostBookProjectSummaryApiClient : ICostBookProjectSummaryApiClient
{
    private readonly ICostBookHttpExecutor _http;
    private readonly IMapper _mapper;

    public CostBookProjectSummaryApiClient(ICostBookHttpExecutor http, IMapper mapper)
    {
        _http = http;
        _mapper = mapper;
    }
    public async Task<ApiResponseDto<double>> GetProfitIncludedTotalAsync(string projectId, int year)
    {
        var response = await _http.GetAsync<double>(
            string.Format(CostBookApiEndpoints.GetProfitIncludedTotal,
                          HttpUtility.UrlEncode(projectId), year));

        if (response.Success)
            return ApiResponseDto<double>.SuccessResponse(response.Data);

        var err = _mapper.Map<ApiResponseDto<double>>(response);
        return ApiResponseDto<double>.FailureResponse(err.Errors, err.Meta);
    }

    public async Task<ApiResponseDto<StaffYearsPivotDto>> GetStaffYearsPivotAsync(string projectId, QueryParameters<string>? query = null)
    {
        var baseUrl = string.Format(CostBookApiEndpoints.GetStaffYearsPivot, HttpUtility.UrlEncode(projectId));
        var url = query != null
            ? QueryStringHelper.AddQueryString(baseUrl, query)
            : baseUrl;

        var response = await _http.GetAsync<StaffYearsPivotRes>(url);

        if (response.Success && response.Data != null)
            return _mapper.Map<ApiResponseDto<StaffYearsPivotDto>>(response);

        var responseDto = _mapper.Map<ApiResponseDto<StaffYearsPivotDto>>(response);
        return ApiResponseDto<StaffYearsPivotDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
    }

    public async Task<ApiResponseDto<StaffEffortPivotDto>> GetStaffEffortAsync(string projectId, QueryParameters<string>? query = null)
    {
        var baseUrl = string.Format(CostBookApiEndpoints.GetStaffEffortPivot, HttpUtility.UrlEncode(projectId));
        var url = query != null
            ? QueryStringHelper.AddQueryString(baseUrl, query)
            : baseUrl;

        var response = await _http.GetAsync<StaffEffortPivotRes>(url);

        if (response.Success && response.Data != null)
            return _mapper.Map<ApiResponseDto<StaffEffortPivotDto>>(response);

        var responseDto = _mapper.Map<ApiResponseDto<StaffEffortPivotDto>>(response);
        return ApiResponseDto<StaffEffortPivotDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
    }

    public async Task<ApiResponseDto<ProjectCostsPivotDto>> GetProjectCostsPivotAsync(string projectId, QueryParameters<string>? query = null)
    {
        var baseUrl = string.Format(CostBookApiEndpoints.GetProjectCostsPivot, HttpUtility.UrlEncode(projectId));
        var url = query != null
            ? QueryStringHelper.AddQueryString(baseUrl, query)
            : baseUrl;

        var response = await _http.GetAsync<ProjectCostsPivotRes>(url);

        if (response.Success && response.Data != null)
            return _mapper.Map<ApiResponseDto<ProjectCostsPivotDto>>(response);

        var responseDto = _mapper.Map<ApiResponseDto<ProjectCostsPivotDto>>(response);
        return ApiResponseDto<ProjectCostsPivotDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
    }
}