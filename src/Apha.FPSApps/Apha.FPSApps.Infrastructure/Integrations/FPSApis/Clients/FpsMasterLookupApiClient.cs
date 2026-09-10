using Apha.Common.Constants;
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
    /// <summary>
    /// HTTP client implementation for Master Lookup API operations.
    /// </summary>
    public class FpsMasterLookupApiClient : IFpsMasterLookupApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string internalCodeError = "INTERNAL_ERROR";

        public FpsMasterLookupApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ApiResponseDto<IEnumerable<MasterLookupDto>>> GetAllMasterLookupsAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<MasterLookupRes>>(FpsApiEndpoints.GetAllMasterLookups);

                if (response.Success)
                {
                    return _mapper.Map<ApiResponseDto<IEnumerable<MasterLookupDto>>>(response);
                }
                else
                {
                    var responseDto = _mapper.Map<ApiResponseDto<IEnumerable<MasterLookupDto>>>(response);
                    return ApiResponseDto<IEnumerable<MasterLookupDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
                }
            }
            catch (Exception)
            {
                var apiErrorsDto = new List<ApiErrorDto>
                {
                    new ApiErrorDto
                    {
                        Message = "Failed to retrieve master lookup data",
                        Code = internalCodeError,
                        Details = null
                    }
                };
                return ApiResponseDto<IEnumerable<MasterLookupDto>>.FailureResponse(apiErrorsDto, new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<IEnumerable<LookupItemDto>>> GetLookupItemsAsync(string tableName)
        {
            try
            {
                var response = await _http.GetAsync<List<LookupItemRes>>(
                    string.Format(FpsApiEndpoints.GetLookupItems, tableName));

                if (response.Success)
                {
                    return _mapper.Map<ApiResponseDto<IEnumerable<LookupItemDto>>>(response);
                }

                var responseDto = _mapper.Map<ApiResponseDto<IEnumerable<LookupItemDto>>>(response);
                return ApiResponseDto<IEnumerable<LookupItemDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<IEnumerable<LookupItemDto>>.FailureResponse(
                    BuildInternalError($"Failed to retrieve items for '{tableName}'"), new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<PaginatedResult<LookupItemDto>>> GetLookupItemsPagedAsync(
            string tableName, QueryParameters<string> query)
        {
            try
            {
                var url = QueryStringHelper.AddQueryString(
                    string.Format(FpsApiEndpoints.GetLookupItemsPaged, tableName), query);
                var response = await _http.GetAsync<List<LookupItemRes>>(url);

                if (response.Success)
                {
                    var items = _mapper.Map<List<LookupItemDto>>(response.Data ?? []);
                    var pageNumber = response.Pagination?.PageNumber ?? query.Page;
                    var pageSize = response.Pagination?.PageSize ?? query.PageSize;
                    var totalRecords = response.Pagination?.TotalRecords ?? items.Count;
                    var paged = new PaginatedResult<LookupItemDto>(items, totalRecords, pageNumber, pageSize);
                    return ApiResponseDto<PaginatedResult<LookupItemDto>>.SuccessResponse(paged);
                }

                return ApiResponseDto<PaginatedResult<LookupItemDto>>.FailureResponse(
                    _mapper.Map<List<ApiErrorDto>>(response.Errors ?? []),
                    _mapper.Map<ApiMetaDto>(response.Meta));
            }
            catch (Exception)
            {
                return ApiResponseDto<PaginatedResult<LookupItemDto>>.FailureResponse(
                    BuildInternalError($"Failed to retrieve paged items for '{tableName}'"), new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<bool>> CreateLookupItemAsync(string tableName, string value)
        {
            try
            {
                var request = new LookupItemReq { Value = value };
                var response = await _http.PostAsync<LookupItemReq, bool?>(
                    string.Format(FpsApiEndpoints.CreateLookupItem, tableName), request);

                if (response.Success)
                {
                    return _mapper.Map<ApiResponseDto<bool>>(response);
                }

                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    BuildInternalError($"Failed to create item in '{tableName}'"), new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<bool>> UpdateLookupItemAsync(string tableName, string originalValue, string newValue)
        {
            try
            {
                var request = new LookupItemReq { Value = newValue, OriginalValue = originalValue };
                var response = await _http.PutAsync<LookupItemReq, bool?>(
                    string.Format(FpsApiEndpoints.UpdateLookupItem, tableName), request);

                if (response.Success)
                {
                    return _mapper.Map<ApiResponseDto<bool>>(response);
                }

                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    BuildInternalError($"Failed to update item in '{tableName}'"), new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteLookupItemAsync(string tableName, string value)
        {
            try
            {
                var response = await _http.DeleteAsync<bool?>(
                    string.Format(FpsApiEndpoints.DeleteLookupItem, tableName, Uri.EscapeDataString(value)));

                if (response.Success)
                {
                    return _mapper.Map<ApiResponseDto<bool>>(response);
                }

                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    BuildInternalError($"Failed to delete item from '{tableName}'"), new ApiMetaDto());
            }
        }

        private static List<ApiErrorDto> BuildInternalError(string message) =>
            new List<ApiErrorDto>
            {
                new ApiErrorDto
                {
                    Message = message,
                    Code = internalCodeError,
                    Details = null
                }
            };
    }
}
