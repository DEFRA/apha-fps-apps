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
    public class FpsStaffJobApiClient : IFpsStaffJobApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        public FpsStaffJobApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<IEnumerable<StaffWorkgroupLookupDto>>> GetStaffWorkgroupLookupAsync()
        {
            try
            {
                var response = await _http.GetAsync<IEnumerable<StaffWorkgroupLookupRes>>("api/staffjob/workgrouplookup");

                if (response.Success)
                {
                    return _mapper.Map<ApiResponseDto<IEnumerable<StaffWorkgroupLookupDto>>>(response);
                }
                else
                {
                    var responseDto = _mapper.Map<ApiResponseDto<IEnumerable<StaffWorkgroupLookupDto>>>(response);
                    return ApiResponseDto<IEnumerable<StaffWorkgroupLookupDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
                }
            }
            catch (Exception ex)
            {
                var apiErrosDto = new List<ApiErrorDto> {
                    new ApiErrorDto {
                         Message = "Failed to retrieve workgroup lookup data",
                         Code = "INTERNAL_ERROR",
                         Details = null
                     }
                 };
                return ApiResponseDto<IEnumerable<StaffWorkgroupLookupDto>>.FailureResponse(apiErrosDto, new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<StaffJobViewDto>>> GetAllStaffJobAsync(QueryParameters<string> staffJob)
        {
            try
            {
                var url = QueryStringHelper.AddQueryString($"api/staffjob?jobCode=FZ2000", staffJob);
                var response = await _http.GetAsync<List<StaffJobViewRes>>(url);

                if (response.Success)
                {
                    return _mapper.Map<ApiResponseDto<List<StaffJobViewDto>>>(response);
                }
                else
                {
                    var responseDto = _mapper.Map<ApiResponseDto<List<StaffJobViewDto>>>(response);
                    return ApiResponseDto<List<StaffJobViewDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
                }
            }
            catch (Exception ex)
            {
                var apiErrosDto = new List<ApiErrorDto> {
                   new ApiErrorDto {
                        Message = "Failed to retrieve user",
                        Code = "INTERNAL_ERROR",
                        Details = null
                    }
                };
                return ApiResponseDto<List<StaffJobViewDto>>.FailureResponse(apiErrosDto,
                   new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<StaffJobDto>> GetStaffJobByIdAsync(string staffId)
        {
            try
            {
                var response = await _http.GetAsync<StaffJobRes>($"api/staffjob/{staffId}");

                if (response.Success)
                {
                    return _mapper.Map<ApiResponseDto<StaffJobDto>>(response);
                }
                else
                {
                    var responseDto = _mapper.Map<ApiResponseDto<StaffJobDto>>(response);
                    return ApiResponseDto<StaffJobDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
                }
            }
            catch (Exception ex)
            {
                var apiErrosDto = new List<ApiErrorDto> {
                   new ApiErrorDto {
                        Message = "Failed to retrieve staff job",
                        Code = "INTERNAL_ERROR",
                        Details = null
                    }
                };
                return ApiResponseDto<StaffJobDto>.FailureResponse(apiErrosDto, new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<StaffJobDto>> CreateStaffJobAsync(StaffJobDto staffJob)
        {
            try
            {
                var staffJobReq = _mapper.Map<StaffJobReq>(staffJob);
                var response = await _http.PostAsync<StaffJobReq, StaffJobRes>("api/staffjob", staffJobReq);

                if (response.Success)
                {
                    return _mapper.Map<ApiResponseDto<StaffJobDto>>(response);
                }
                else
                {
                    var responseDto = _mapper.Map<ApiResponseDto<StaffJobDto>>(response);
                    return ApiResponseDto<StaffJobDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
                }
            }
            catch (Exception ex)
            {
                var apiErrosDto = new List<ApiErrorDto> {
                   new ApiErrorDto {
                        Message = "Failed to create staff job",
                        Code = "INTERNAL_ERROR",
                        Details = null
                    }
                };
                return ApiResponseDto<StaffJobDto>.FailureResponse(apiErrosDto, new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<StaffJobDto>> UpdateStaffJobAsync(StaffJobDto staffJob)
        {
            try
            {
                var staffJobReq = _mapper.Map<StaffJobReq>(staffJob);
                var response = await _http.PutAsync<StaffJobReq, StaffJobRes>($"api/staffjob", staffJobReq);

                if (response.Success)
                {
                    return _mapper.Map<ApiResponseDto<StaffJobDto>>(response);
                }
                else
                {
                    var responseDto = _mapper.Map<ApiResponseDto<StaffJobDto>>(response);
                    return ApiResponseDto<StaffJobDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
                }
            }
            catch (Exception ex)
            {
                var apiErrosDto = new List<ApiErrorDto> {
                   new ApiErrorDto {
                        Message = "Failed to update staff job",
                        Code = "INTERNAL_ERROR",
                        Details = null
                    }
                };
                return ApiResponseDto<StaffJobDto>.FailureResponse(apiErrosDto, new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteStaffJobAsync(string staffId, string jobCode)
        {
            try
            {
                var response = await _http.DeleteAsync<bool>($"api/staffjob/{staffId}");

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
            catch (Exception ex)
            {
                var apiErrosDto = new List<ApiErrorDto> {
                   new ApiErrorDto {
                        Message = "Failed to delete staff job",
                        Code = "INTERNAL_ERROR",
                        Details = null
                    }
                };
                return ApiResponseDto<bool>.FailureResponse(apiErrosDto, new ApiMetaDto());
            }
        }
    }
}
