using Apha.Common.Constants;
using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients;
using MapsterMapper;
using NSubstitute;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PIMS.PimsReportApiClientTest
{
    public class PimsReportApiClientTests
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PimsReportApiClient _client;

        public PimsReportApiClientTests()
        {
            _http = Substitute.For<IPimsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PimsReportApiClient(_http, _mapper);
        }

        #region GetAllReportsAsync Tests

        [Fact]
        public async Task GetAllReportsAsync_WhenSuccessResponse_ReturnsMappedList()
        {
            // Arrange
            var reportList = new List<ReportRes>
            {
                new ReportRes { Id = 1, ReportName = "Report1", Type = "R" },
                new ReportRes { Id = 2, ReportName = "Report2", Type = "R" }
            };
            var apiResponse = new ApiResponse<List<ReportRes>> { Success = true, Data = reportList };
            var mappedDto = ApiResponseDto<List<ReportDto>>.SuccessResponse(new List<ReportDto>
            {
                new ReportDto { Id = 1, ReportName = "Report1", Type = "R" },
                new ReportDto { Id = 2, ReportName = "Report2", Type = "R" }
            });

            _http.GetAsync<List<ReportRes>>(PimsApiEndpoints.GetAllReports).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ReportDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetAllReportsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            await _http.Received(1).GetAsync<List<ReportRes>>(PimsApiEndpoints.GetAllReports);
            _mapper.Received(1).Map<ApiResponseDto<List<ReportDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAllReportsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new ApiError { Message = "Error", Code = "ERR" } };
            var apiResponse = new ApiResponse<List<ReportRes>> { Success = false, Data = null, Errors = errors };
            var failDto = new ApiResponseDto<List<ReportDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Error", Code = "ERR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ReportRes>>(PimsApiEndpoints.GetAllReports).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ReportDto>>>(apiResponse).Returns(failDto);

            // Act
            var result = await _client.GetAllReportsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            await _http.Received(1).GetAsync<List<ReportRes>>(PimsApiEndpoints.GetAllReports);
        }

        #endregion

        #region GetPagedReportsAsync Tests

        [Fact]
        public async Task GetPagedReportsAsync_WhenSuccessResponse_ReturnsPagedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var reportList = new List<ReportRes> { new ReportRes { Id = 1, ReportName = "Report1", Type = "R" } };
            var apiResponse = new ApiResponse<List<ReportRes>>
            {
                Success = true,
                Data = reportList,
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };
            var mappedItems = new List<ReportDto> { new ReportDto { Id = 1, ReportName = "Report1", Type = "R" } };

            _http.GetAsync<List<ReportRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<List<ReportDto>>(reportList).Returns(mappedItems);

            // Act
            var result = await _client.GetPagedReportsAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data.data);
            await _http.Received(1).GetAsync<List<ReportRes>>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetPagedReportsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiError> { new ApiError { Message = "Error", Code = "ERR" } };
            var apiResponse = new ApiResponse<List<ReportRes>> { Success = false, Errors = errors };

            _http.GetAsync<List<ReportRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<List<ApiErrorDto>>(errors).Returns(new List<ApiErrorDto> { new ApiErrorDto { Message = "Error", Code = "ERR" } });
            _mapper.Map<ApiMetaDto>(apiResponse.Meta).Returns(new ApiMetaDto());

            // Act
            var result = await _client.GetPagedReportsAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            await _http.Received(1).GetAsync<List<ReportRes>>(Arg.Any<string>());
        }

        #endregion

        #region GetReportByIdAsync Tests

        [Fact]
        public async Task GetReportByIdAsync_WhenSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            var id = 1;
            var url = string.Format(PimsApiEndpoints.GetReportById, id);
            var reportRes = new ReportRes { Id = id, ReportName = "Report1", Type = "R" };
            var apiResponse = new ApiResponse<ReportRes> { Success = true, Data = reportRes };
            var mappedDto = ApiResponseDto<ReportDto>.SuccessResponse(new ReportDto { Id = id, ReportName = "Report1", Type = "R" });

            _http.GetAsync<ReportRes>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ReportDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetReportByIdAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(id, result.Data.Id);
            await _http.Received(1).GetAsync<ReportRes>(url);
        }

        [Fact]
        public async Task GetReportByIdAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var id = 999;
            var url = string.Format(PimsApiEndpoints.GetReportById, id);
            var errors = new List<ApiError> { new ApiError { Message = "Not found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<ReportRes> { Success = false, Errors = errors };
            var failDto = new ApiResponseDto<ReportDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Not found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<ReportRes>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ReportDto>>(apiResponse).Returns(failDto);

            // Act
            var result = await _client.GetReportByIdAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            await _http.Received(1).GetAsync<ReportRes>(url);
        }

        #endregion

        #region CreateReportAsync Tests

        [Fact]
        public async Task CreateReportAsync_WhenSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            var dto = new ReportDto { Id = 1, ReportName = "NewReport", Type = "R" };
            var request = new ReportReq { ReportName = "NewReport", Type = "R" };
            var response = new ReportRes { Id = 1, ReportName = "NewReport", Type = "R" };
            var apiResponse = new ApiResponse<ReportRes> { Success = true, Data = response };
            var mappedDto = ApiResponseDto<ReportDto>.SuccessResponse(dto);

            _mapper.Map<ReportReq>(dto).Returns(request);
            _http.PostAsync<ReportReq, ReportRes>(PimsApiEndpoints.CreateReport, request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ReportDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.CreateReportAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            await _http.Received(1).PostAsync<ReportReq, ReportRes>(PimsApiEndpoints.CreateReport, request);
        }

        [Fact]
        public async Task CreateReportAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new ReportDto { Id = 1, ReportName = "NewReport", Type = "R" };
            var request = new ReportReq { ReportName = "NewReport", Type = "R" };
            var errors = new List<ApiError> { new ApiError { Message = "Duplicate", Code = "DUPLICATE" } };
            var apiResponse = new ApiResponse<ReportRes> { Success = false, Errors = errors };
            var failDto = new ApiResponseDto<ReportDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Duplicate", Code = "DUPLICATE" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<ReportReq>(dto).Returns(request);
            _http.PostAsync<ReportReq, ReportRes>(PimsApiEndpoints.CreateReport, request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ReportDto>>(apiResponse).Returns(failDto);

            // Act
            var result = await _client.CreateReportAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            await _http.Received(1).PostAsync<ReportReq, ReportRes>(PimsApiEndpoints.CreateReport, request);
        }

        #endregion

        #region UpdateReportAsync Tests

        [Fact]
        public async Task UpdateReportAsync_WhenSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            var id = 1;
            var url = string.Format(PimsApiEndpoints.UpdateReport, id);
            var dto = new ReportDto { Id = id, ReportName = "UpdatedReport", Type = "R" };
            var request = new ReportReq { ReportName = "UpdatedReport", Type = "R" };
            var response = new ReportRes { Id = id, ReportName = "UpdatedReport", Type = "R" };
            var apiResponse = new ApiResponse<ReportRes> { Success = true, Data = response };
            var mappedDto = ApiResponseDto<ReportDto>.SuccessResponse(dto);

            _mapper.Map<ReportReq>(dto).Returns(request);
            _http.PutAsync<ReportReq, ReportRes>(url, request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ReportDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.UpdateReportAsync(id, dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            await _http.Received(1).PutAsync<ReportReq, ReportRes>(url, request);
        }

        [Fact]
        public async Task UpdateReportAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var id = 999;
            var url = string.Format(PimsApiEndpoints.UpdateReport, id);
            var dto = new ReportDto { Id = id };
            var request = new ReportReq();
            var errors = new List<ApiError> { new ApiError { Message = "Not found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<ReportRes> { Success = false, Errors = errors };
            var failDto = new ApiResponseDto<ReportDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Not found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<ReportReq>(dto).Returns(request);
            _http.PutAsync<ReportReq, ReportRes>(url, request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ReportDto>>(apiResponse).Returns(failDto);

            // Act
            var result = await _client.UpdateReportAsync(id, dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            await _http.Received(1).PutAsync<ReportReq, ReportRes>(url, request);
        }

        #endregion

        #region DeleteReportAsync Tests

        [Fact]
        public async Task DeleteReportAsync_WhenSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            var id = 1;
            var url = string.Format(PimsApiEndpoints.DeleteReport, id);
            var apiResponse = new ApiResponse<bool> { Success = true, Data = true };
            var mappedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.DeleteReportAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _http.Received(1).DeleteAsync<bool>(url);
        }

        [Fact]
        public async Task DeleteReportAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var id = 999;
            var url = string.Format(PimsApiEndpoints.DeleteReport, id);
            var errors = new List<ApiError> { new ApiError { Message = "Not found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<bool> { Success = false, Errors = errors };
            var failDto = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Not found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.DeleteAsync<bool>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(failDto);

            // Act
            var result = await _client.DeleteReportAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            await _http.Received(1).DeleteAsync<bool>(url);
        }

        #endregion
    }
}
