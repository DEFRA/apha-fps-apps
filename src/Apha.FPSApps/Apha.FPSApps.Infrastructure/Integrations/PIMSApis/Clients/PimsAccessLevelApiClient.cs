
using Apha.Common.Constants;
using Apha.Common.Contracts.PIMS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients
{
    public class PimsAccessLevelApiClient : IPimsAccessLevelApiClient
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;

        public PimsAccessLevelApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<AccessLevelDto>>> GetAllAsync()
        {
            var response = await _http.GetAsync<List<AccessLevelRes>>(PimsApiEndpoints.GetAllAccessLevels);
            if (response.Success && response.Data != null)
                return _mapper.Map<ApiResponseDto<List<AccessLevelDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<AccessLevelDto>>>(response);
            return ApiResponseDto<List<AccessLevelDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<List<AccessLevelDto>>> GetBySystemIdAsync(int systemid)
        {
            var url = string.Format(PimsApiEndpoints.GetAccessLevelsBySystemId, systemid);
            var response = await _http.GetAsync<List<AccessLevelRes>>(url);
            if (response.Success && response.Data != null)
                return _mapper.Map<ApiResponseDto<List<AccessLevelDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<AccessLevelDto>>>(response);
            return ApiResponseDto<List<AccessLevelDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<AccessLevelDto>> GetByIdAsync(int systemid, int accesslevelid)
        {
            var url = string.Format(PimsApiEndpoints.GetAccessLevelById, systemid, accesslevelid);
            var response = await _http.GetAsync<AccessLevelRes>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<AccessLevelDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<AccessLevelDto>>(response);
            return ApiResponseDto<AccessLevelDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<AccessLevelDto>> CreateAsync(AccessLevelDto dto)
        {
            var request = _mapper.Map<AccessLevelRes>(dto);
            var response = await _http.PostAsync<AccessLevelRes, AccessLevelRes>(PimsApiEndpoints.CreateAccessLevel, request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<AccessLevelDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<AccessLevelDto>>(response);
            return ApiResponseDto<AccessLevelDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<AccessLevelDto>> UpdateAsync(int systemid, int accesslevelid, AccessLevelDto dto)
        {
            var request = _mapper.Map<AccessLevelRes>(dto);
            var url = string.Format(PimsApiEndpoints.UpdateAccessLevel, systemid, accesslevelid);
            var response = await _http.PutAsync<AccessLevelRes, AccessLevelRes>(url, request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<AccessLevelDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<AccessLevelDto>>(response);
            return ApiResponseDto<AccessLevelDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<bool>> DeleteAsync(int systemid, int accesslevelid)
        {
            var url = string.Format(PimsApiEndpoints.DeleteAccessLevel, systemid, accesslevelid);
            var response = await _http.DeleteAsync<bool>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<bool>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }
    }
}
