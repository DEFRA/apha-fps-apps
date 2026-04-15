using Apha.Common.Contracts;
using Apha.Common.Contracts.Costbook;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.CostBookApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apha.FPSApps.Infrastructure.Integrations.CostBookApis.Clients
{
    public class CostBookProgramApiClient : ICostBookProgramApiClient
    {
        private readonly ICostBookHttpExecutor _http;
        private readonly IMapper _mapper;

        public CostBookProgramApiClient(ICostBookHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<ProgramDto>>> GetAllProgramsAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<ProgramRes>>("api/projects/programs");

                if (response.Success && response.Data != null)
                {
                    return _mapper.Map<ApiResponseDto<List<ProgramDto>>>(response);
                }
                else
                {
                    var responseDto = _mapper.Map<ApiResponseDto<List<ProgramDto>>>(response);
                    return ApiResponseDto<List<ProgramDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
                }
            }
            catch (Exception ex)
            {
                var apiErrorsDto = new List<ApiErrorDto> {
                   new ApiErrorDto {
                        Message = "Failed to retrieve programs",
                        Code = "INTERNAL_ERROR",
                        Details = ex.Message
                    }
                };
                return ApiResponseDto<List<ProgramDto>>.FailureResponse(apiErrorsDto, new ApiMetaDto());
            }
        }
        //public async Task<ApiResponseDto<List<ProgramDto>>> GetAllProgramsAsync()
        //{
        //    try
        //    {
        //        // Deserialize into ApiResponse<List<ProgramRes>> instead of List<ProgramRes>
        //        var response = await _http.GetAsync<ApiResponse<List<ProgramRes>>>("api/projects/programs");

        //        if (response.Success && response.Data != null)
        //        {
        //            // Map only the Data from API response
        //            var mappedResponse = new ApiResponseDto<List<ProgramDto>>
        //            {
        //                Success = true,
        //                Data = _mapper.Map<List<ProgramDto>>(response.Data),
        //                Meta = _mapper.Map<ApiMetaDto>(response.Meta),
        //                Errors = null
        //            };
        //            return mappedResponse;
        //        }
        //        else
        //        {
        //            var failure = new ApiResponseDto<List<ProgramDto>>
        //            {
        //                Success = false,
        //                Data = null,
        //                Errors = response.Errors?.Select(e => _mapper.Map<ApiErrorDto>(e)).ToList(),
        //                Meta = _mapper.Map<ApiMetaDto>(response.Meta)
        //            };
        //            return failure;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        var apiErrorsDto = new List<ApiErrorDto>
        //{
        //    new ApiErrorDto
        //    {
        //        Message = "Failed to retrieve programs",
        //        Code = "INTERNAL_ERROR",
        //        Details = ex.Message
        //    }
        //};
        //        return ApiResponseDto<List<ProgramDto>>.FailureResponse(apiErrorsDto, new ApiMetaDto());
        //    }
        //}
    }
}
