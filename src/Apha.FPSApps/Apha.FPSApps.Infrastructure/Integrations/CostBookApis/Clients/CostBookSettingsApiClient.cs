using Apha.Common.Constants;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Interfaces.CostBookApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Apha.FPSApps.Infrastructure.Integrations.CostBookApis.Clients
{
    public class CostBookSettingsApiClient : ICostBookSettingsApiClient
    {
        private readonly ICostBookHttpExecutor _http;
        private readonly IMapper _mapper;

        public CostBookSettingsApiClient(ICostBookHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<string>> GetSettingValueByIdAsync(string? id)
        {
            var query = !string.IsNullOrEmpty(id)
                ? $"{CostBookApiEndpoints.GetSettingValueById}?id={HttpUtility.UrlEncode(id)}"
                : CostBookApiEndpoints.GetSettingValueById;
            var response = await _http.GetAsync<string>(query);

            if (response.Success && response.Data != null)
                return ApiResponseDto<string>.SuccessResponse(response.Data);

            var responseDto = _mapper.Map<ApiResponseDto<string>>(response);
            return ApiResponseDto<string>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }
    }
}
