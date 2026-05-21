using Apha.Common.Constants;
using Apha.Common.Contracts.PACT;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

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

        public async Task<ApiResponseDto<List<WorkGroupDto>>> GetAllWorkGroupsAsync()
        {
            var response = await _http.GetAsync<List<WorkGroupRes>>(PactApiEndpoints.GetAllWorkGroups);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<WorkGroupDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<WorkGroupDto>>>(response);
            return ApiResponseDto<List<WorkGroupDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<WorkGroupDto>>> GetWorkGroupsByProfitCentreAsync(
            QueryParameters<string> query, string profitCentre)
        {
            var url = QueryStringHelper.AddQueryString(PactApiEndpoints.GetPagedWorkGroupsByProfitCentre, query);
            url += $"&profitCentre={Uri.EscapeDataString(profitCentre)}";

            var response = await _http.GetAsync<List<WorkGroupRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<WorkGroupDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<WorkGroupDto>>>(response);
            return ApiResponseDto<List<WorkGroupDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<bool>> SetSendEmailForProfitCentreWorkGroupsAsync(string profitCentre, short flag)
        {
            var request = new UpdateSendEmailFlagReq { ProfitCentre = profitCentre, SendEmail = flag };
            var response = await _http.PutAsync<UpdateSendEmailFlagReq, bool?>(
                PactApiEndpoints.SetSendEmailForProfitCentreWorkGroups, request);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<bool>>(response);

            var failureDto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(failureDto.Errors, failureDto.Meta);
        }

        public async Task<ApiResponseDto<bool>> SetSendEmailForAllWorkGroupsAsync(short flag)
        {
            var request = new UpdateSendEmailFlagReq { SendEmail = flag };
            var response = await _http.PutAsync<UpdateSendEmailFlagReq, bool?>(
                PactApiEndpoints.SetSendEmailForAllWorkGroups, request);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<bool>>(response);

            var failureDto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(failureDto.Errors, failureDto.Meta);
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
                return _mapper.Map<ApiResponseDto<bool>>(response);

            var failureDto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(failureDto.Errors, failureDto.Meta);
        }
    }
}
