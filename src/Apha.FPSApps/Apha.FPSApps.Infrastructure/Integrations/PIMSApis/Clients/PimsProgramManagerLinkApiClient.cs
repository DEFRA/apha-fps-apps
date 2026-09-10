using Apha.Common.Constants;
using Apha.Common.Contracts.PIMS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients
{
    public class PimsProgramManagerLinkApiClient : IPimsProgramManagerLinkApiClient
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;

        public PimsProgramManagerLinkApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }


        public async Task<ApiResponseDto<List<ProgramManagerLinkDto>>> GetAllProgramManagerLinksAsync()
        {
            var response = await _http.GetAsync<List<ProgramManagerLinkRes>>(PimsApiEndpoints.GetAllProgramManagerLinks);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<ProgramManagerLinkDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<ProgramManagerLinkDto>>>(response);
            return ApiResponseDto<List<ProgramManagerLinkDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }


        public async Task<ApiResponseDto<PaginatedResult<ProgramManagerLinkDto>>> GetPagedByManagerAsync(
            QueryParameters<string> query,
            string manager)
        {
            var url = $"{PimsApiEndpoints.GetPagedProgramManagerLinks}?" +
                $"search={Uri.EscapeDataString(query.Search ?? string.Empty)}" +
                $"&sortBy={Uri.EscapeDataString(query.SortBy ?? string.Empty)}" +
                $"&descending={query.Descending}" +
                $"&page={query.Page}" +
                $"&pageSize={query.PageSize}" +
                $"&manager={Uri.EscapeDataString(manager)}";

            if (!string.IsNullOrWhiteSpace(query.Filter))
            {
                url += $"&filter={Uri.EscapeDataString(query.Filter)}";
            }

            var response = await _http.GetAsync<List<ProgramManagerLinkRes>>(url);
            if (response.Success)
            {
                var items = _mapper.Map<List<ProgramManagerLinkDto>>(response.Data ?? []);
                var pageNumber = response.Pagination?.PageNumber ?? query.Page;
                var pageSize = response.Pagination?.PageSize ?? query.PageSize;
                var totalRecords = response.Pagination?.TotalRecords ?? items.Count;
                var paged = new PaginatedResult<ProgramManagerLinkDto>(items, totalRecords, pageNumber, pageSize);
                return ApiResponseDto<PaginatedResult<ProgramManagerLinkDto>>.SuccessResponse(paged);
            }

            return ApiResponseDto<PaginatedResult<ProgramManagerLinkDto>>.FailureResponse(
                _mapper.Map<List<ApiErrorDto>>(response.Errors ?? []),
                _mapper.Map<ApiMetaDto>(response.Meta));
        }


        public async Task<ApiResponseDto<List<ProgramManagerLinkDto>>> GetByProgramAsync(string program)
        {
            var url = string.Format(PimsApiEndpoints.GetProgramManagerLinksByProgram, Uri.EscapeDataString(program));
            var response = await _http.GetAsync<List<ProgramManagerLinkRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<ProgramManagerLinkDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<ProgramManagerLinkDto>>>(response);
            return ApiResponseDto<List<ProgramManagerLinkDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<List<ProgramManagerLinkDto>>> GetByManagerAsync(string manager)
        {
            var url = string.Format(PimsApiEndpoints.GetProgramManagerLinksByManager, Uri.EscapeDataString(manager));
            var response = await _http.GetAsync<List<ProgramManagerLinkRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<ProgramManagerLinkDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<ProgramManagerLinkDto>>>(response);
            return ApiResponseDto<List<ProgramManagerLinkDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }


        public async Task<ApiResponseDto<ProgramManagerLinkDto>> GetProgramManagerLinkByIdAsync(string program, string manager)
        {
            var url = string.Format(PimsApiEndpoints.GetProgramManagerLinkById, Uri.EscapeDataString(program), Uri.EscapeDataString(manager));
            var response = await _http.GetAsync<ProgramManagerLinkRes>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<ProgramManagerLinkDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<ProgramManagerLinkDto>>(response);
            return ApiResponseDto<ProgramManagerLinkDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }


        public async Task<ApiResponseDto<ProgramManagerLinkDto>> CreateProgramManagerLinkAsync(ProgramManagerLinkDto dto)
        {
            var request = _mapper.Map<ProgramManagerLinkReq>(dto);
            var response = await _http.PostAsync<ProgramManagerLinkReq, ProgramManagerLinkRes>(PimsApiEndpoints.CreateProgramManagerLink, request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<ProgramManagerLinkDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<ProgramManagerLinkDto>>(response);
            return ApiResponseDto<ProgramManagerLinkDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<List<ProgramLookupDto>>> GetProgramsAsync()
        {
            var response = await _http.GetAsync<List<ProgramLookupRes>>(PimsApiEndpoints.GetPrograms);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<ProgramLookupDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<ProgramLookupDto>>>(response);
            return ApiResponseDto<List<ProgramLookupDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }


        public async Task<ApiResponseDto<bool>> DeleteProgramManagerLinkAsync(string program, string manager)
        {
            var url = string.Format(PimsApiEndpoints.DeleteProgramManagerLink, Uri.EscapeDataString(program), Uri.EscapeDataString(manager));
            var response = await _http.DeleteAsync<bool>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<bool>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }
    }
}
