using Apha.Common.Constants;
using Apha.Common.Contracts.PIMS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using MapsterMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients
{
    public class PimsSettingApiClient : IPimsSettingApiClient
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;

        public PimsSettingApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<SettingDto>>> GetAllSettingsAsync()
        {
            var response = await _http.GetAsync<List<SettingRes>>(PimsApiEndpoints.GetAllSettings);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<SettingDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<SettingDto>>>(response);
            return ApiResponseDto<List<SettingDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        
        public async Task<ApiResponseDto<List<SettingDto>>> GetAllUserUpdateableSettingsAsync()
        {
            var url = PimsApiEndpoints.GetAllUserUpdateableSettings;
            var response = await _http.GetAsync<List<SettingRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<SettingDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<SettingDto>>>(response);
            return ApiResponseDto<List<SettingDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        
        public async Task<ApiResponseDto<SettingDto>> GetSettingByIdAsync(string id)
        {
            var url = string.Format(PimsApiEndpoints.GetSettingById, Uri.EscapeDataString(id));
            var response = await _http.GetAsync<SettingRes>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<SettingDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<SettingDto>>(response);
            return ApiResponseDto<SettingDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        
        public async Task<ApiResponseDto<SettingDto>> UpdateSettingAsync(string id, SettingDto dto)
        {
            var request = _mapper.Map<SettingReq>(dto);
            var url = string.Format(PimsApiEndpoints.UpdateSetting, Uri.EscapeDataString(id));
            var response = await _http.PutAsync<SettingReq, SettingRes>(url, request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<SettingDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<SettingDto>>(response);
            return ApiResponseDto<SettingDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }
    }
}
