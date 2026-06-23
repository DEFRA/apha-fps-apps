using Apha.Common.Constants;
using Apha.Common.Contracts.PACT;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using Microsoft.AspNetCore.WebUtilities;

namespace Apha.FPSApps.Infrastructure.Integrations.PACTApis.Clients
{
    public class PactWorkGroupApiClient : IPactWorkGroupApiClient
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;

        public PactWorkGroupApiClient(IPactHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<string>>> GetAllWorkGroupNamesAsync()
        {
            var response = await _http.GetAsync<List<string>>(PactApiEndpoints.GetAllWorkGroupNames);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<List<string>>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<List<string>>>(response);
                return ApiResponseDto<List<string>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<List<WorkGroupViewDto>>> GetWorkGroupsByProfitCentreForBudgetAsync(string profitCentre)
        {
            var url = QueryHelpers.AddQueryString(
                PactApiEndpoints.GetWorkGroupsByProfitCentreForBudget,
                "profitCentre", profitCentre);
            var response = await _http.GetAsync<List<WorkGroupViewRes>>(url);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<List<WorkGroupViewDto>>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<List<WorkGroupViewDto>>>(response);
                return ApiResponseDto<List<WorkGroupViewDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<List<WorkGroupViewDto>>> GetWorkGroupsByProfitCentreForBudgetPagedAsync(
            QueryParameters<string> query, string profitCentre)
        {
            var url = QueryStringHelper.AddQueryString(PactApiEndpoints.GetWorkGroupsByProfitCentreForBudgetPaged, query);
            url = QueryHelpers.AddQueryString(url, "profitCentre", profitCentre);

            var response = await _http.GetAsync<List<WorkGroupViewRes>>(url);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<List<WorkGroupViewDto>>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<List<WorkGroupViewDto>>>(response);
                return ApiResponseDto<List<WorkGroupViewDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<List<WorkGroupDto>>> GetAllWorkGroupsAsync()
        {
            var response = await _http.GetAsync<List<WorkGroupRes>>(PactApiEndpoints.GetAllWorkGroups);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<List<WorkGroupDto>>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<List<WorkGroupDto>>>(response);
                return ApiResponseDto<List<WorkGroupDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<List<WorkGroupTimeCodeDto>>> GetPagedWorkGroupTimeCodesAsync(
            QueryParameters<string> query, string? workGroup, int? monthNumber)
        {
            var url = QueryStringHelper.AddQueryString(PactApiEndpoints.GetPagedWorkGroupTimeCodes, query);
            if (!string.IsNullOrWhiteSpace(workGroup))
                url += $"&workGroup={Uri.EscapeDataString(workGroup)}";
            if (monthNumber.HasValue)
                url += $"&monthNumber={monthNumber.Value}";

            var response = await _http.GetAsync<List<WorkGroupTimeCodeRes>>(url);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<List<WorkGroupTimeCodeDto>>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<List<WorkGroupTimeCodeDto>>>(response);
                return ApiResponseDto<List<WorkGroupTimeCodeDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<List<WorkGroupValidTimeCodeDto>>> GetPagedWorkGroupValidTimeCodesAsync(
            QueryParameters<string> query, string workGroup)
        {
            var url = QueryStringHelper.AddQueryString(PactApiEndpoints.GetPagedWorkGroupValidTimeCodes, query);
            if (!string.IsNullOrWhiteSpace(workGroup))
                url += $"&workGroup={Uri.EscapeDataString(workGroup)}";

            var response = await _http.GetAsync<List<WorkGroupValidTimeCodeRes>>(url);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<List<WorkGroupValidTimeCodeDto>>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<List<WorkGroupValidTimeCodeDto>>>(response);
                return ApiResponseDto<List<WorkGroupValidTimeCodeDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<WgSummarisedStaffTimeUsageDto>> GetWgSummarisedStaffTimeUsageAsync(
            QueryParameters<string> query, string staffName)
        {
            var url = QueryStringHelper.AddQueryString(PactApiEndpoints.GetWgSummarisedStaffTimeUsage, query);
            url += $"&staffName={Uri.EscapeDataString(staffName)}";

            var response = await _http.GetAsync<WgSummarisedStaffTimeUsageRes>(url);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<WgSummarisedStaffTimeUsageDto>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<WgSummarisedStaffTimeUsageDto>>(response);
                return ApiResponseDto<WgSummarisedStaffTimeUsageDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<List<WorkGroupDto>>> GetWorkGroupsByProfitCentreAsync(
            QueryParameters<string> query, string profitCentre)
        {
            var url = QueryStringHelper.AddQueryString(PactApiEndpoints.GetPagedWorkGroupsByProfitCentre, query);
            url = QueryHelpers.AddQueryString(url, "profitCentre", profitCentre);

            var response = await _http.GetAsync<List<WorkGroupRes>>(url);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<List<WorkGroupDto>>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<List<WorkGroupDto>>>(response);
                return ApiResponseDto<List<WorkGroupDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<bool>> SetSendEmailForProfitCentreWorkGroupsAsync(string profitCentre, short flag)
        {
            var request = new UpdateSendEmailFlagReq { ProfitCentre = profitCentre, SendEmail = flag };
            var response = await _http.PutAsync<UpdateSendEmailFlagReq, bool?>(
                PactApiEndpoints.SetSendEmailForProfitCentreWorkGroups, request);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<bool>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<bool>> SetSendEmailForAllWorkGroupsAsync(short flag)
        {
            var request = new UpdateSendEmailFlagReq { SendEmail = flag };
            var response = await _http.PutAsync<UpdateSendEmailFlagReq, bool?>(
                PactApiEndpoints.SetSendEmailForAllWorkGroups, request);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<bool>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<bool>> UpdateWorkGroupEmailAsync(
            string workGroupName, short sendEmail, string? emailRecipient)
        {
            var url = string.Format(PactApiEndpoints.UpdateWorkGroupEmail, Uri.EscapeDataString(workGroupName));
            var request = new UpdateWorkGroupEmailReq
            {
                WorkGroupName = workGroupName,
                SendEmail = sendEmail,
                EmailRecipient = emailRecipient
            };
            var response = await _http.PutAsync<UpdateWorkGroupEmailReq, bool?>(url, request);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<bool>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }
    }
}
