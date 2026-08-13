using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS.BulkRates;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsBulkRatesApiClientTest
{
    public class FpsBulkRatesApiClientTests
    {
        private static readonly Guid JobExecutionId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly FpsBulkRatesApiClient _client;

        public FpsBulkRatesApiClientTests()
        {
            _http = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsBulkRatesApiClient(_http, _mapper);
        }

        private static BulkRatesRequestDetailRes RequestDetailRes(string status = "Initiated") => new()
        {
            Entry = new BulkRatesQueueEntryRes { JobQueueId = Guid.NewGuid(), JobExecutionId = JobExecutionId, Status = status },
        };

        private static BulkRatesRequestDetailDto RequestDetailDto(string status = "Initiated") => new()
        {
            Entry = new BulkRatesQueueEntryDto { JobQueueId = Guid.NewGuid(), JobExecutionId = JobExecutionId, Status = status },
        };

        #region CreateRequestAsync

        [Fact]
        public async Task CreateRequestAsync_WhenApiReturnsSuccess_ReturnsMappedRequestDetail()
        {
            var apiResponse = new ApiResponse<BulkRatesRequestDetailRes> { Success = true, Data = RequestDetailRes() };
            var mappedDto = ApiResponseDto<BulkRatesRequestDetailDto>.SuccessResponse(RequestDetailDto());

            _http.PostAsync<CreateBulkRatesRequestReq, BulkRatesRequestDetailRes>(Arg.Any<string>(), Arg.Any<CreateBulkRatesRequestReq>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<BulkRatesRequestDetailDto>>(apiResponse).Returns(mappedDto);

            var result = await _client.CreateRequestAsync("BulkTestRatesUpdate", 2027);

            Assert.True(result.Success);
            Assert.Equal("Initiated", result.Data!.Entry.Status);
            await _http.Received(1).PostAsync<CreateBulkRatesRequestReq, BulkRatesRequestDetailRes>(
                Apha.Common.Constants.FpsApiEndpoints.CreateBulkRatesRequest,
                Arg.Is<CreateBulkRatesRequestReq>(r => r.JobName == "BulkTestRatesUpdate" && r.FpsYear == 2027));
        }

        [Fact]
        public async Task CreateRequestAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var errors = new List<ApiError> { new() { Message = "FpsYear is not Planned", Code = "INVALID_STATE" } };
            var apiResponse = new ApiResponse<BulkRatesRequestDetailRes> { Success = false, Errors = errors, Meta = new ApiMeta() };
            var mappedFailure = new ApiResponseDto<BulkRatesRequestDetailDto>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "FpsYear is not Planned", Code = "INVALID_STATE" }],
                Meta = new ApiMetaDto(),
            };

            _http.PostAsync<CreateBulkRatesRequestReq, BulkRatesRequestDetailRes>(Arg.Any<string>(), Arg.Any<CreateBulkRatesRequestReq>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<BulkRatesRequestDetailDto>>(apiResponse).Returns(mappedFailure);

            var result = await _client.CreateRequestAsync("BulkTestRatesUpdate", 2027);

            Assert.False(result.Success);
            Assert.Single(result.Errors!);
            Assert.Equal("INVALID_STATE", result.Errors![0].Code);
        }

        #endregion

        #region UploadFileAsync

        [Fact]
        public async Task UploadFileAsync_WhenApiReturnsSuccess_ReturnsMappedUploadResult()
        {
            var apiResult = new BulkRatesUploadResultRes { JobQueueId = Guid.NewGuid(), Status = "Initiated", UploadVersion = 1 };
            var apiResponse = new ApiResponse<BulkRatesUploadResultRes> { Success = true, Data = apiResult };
            var mappedDto = ApiResponseDto<BulkRatesUploadResultDto>.SuccessResponse(
                new BulkRatesUploadResultDto { UploadVersion = 1 });

            _http.PostMultipartAsync<BulkRatesUploadResultRes>(Arg.Any<string>(), Arg.Any<MultipartFormDataContent>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<BulkRatesUploadResultDto>>(apiResponse).Returns(mappedDto);

            var result = await _client.UploadFileAsync(JobExecutionId, [1, 2, 3], "rates.xlsx");

            Assert.True(result.Success);
            Assert.Equal(1, result.Data!.UploadVersion);
            await _http.Received(1).PostMultipartAsync<BulkRatesUploadResultRes>(
                Arg.Is<string>(u => u.Contains(JobExecutionId.ToString())), Arg.Any<MultipartFormDataContent>());
        }

        #endregion

        #region GetValidationResultsAsync

        [Fact]
        public async Task GetValidationResultsAsync_WhenApiReturnsSuccess_ReturnsMappedValidationResult()
        {
            var apiResult = new BulkRatesUploadResultRes
            {
                JobQueueId = Guid.NewGuid(),
                Status = "Initiated",
                ValidationErrors = [new BulkRatesValidationErrorRes { Severity = "Error", ValidationMessage = "bad row" }],
            };
            var apiResponse = new ApiResponse<BulkRatesUploadResultRes> { Success = true, Data = apiResult };
            var mappedDto = ApiResponseDto<BulkRatesUploadResultDto>.SuccessResponse(
                new BulkRatesUploadResultDto { ValidationErrors = [new BulkRatesValidationErrorDto { ValidationMessage = "bad row" }] });

            _http.GetAsync<BulkRatesUploadResultRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<BulkRatesUploadResultDto>>(apiResponse).Returns(mappedDto);

            var result = await _client.GetValidationResultsAsync(JobExecutionId);

            Assert.True(result.Success);
            Assert.Single(result.Data!.ValidationErrors);
            Assert.Equal("bad row", result.Data.ValidationErrors[0].ValidationMessage);
        }

        #endregion

        #region ReleaseForApprovalAsync / ApproveAsync

        [Fact]
        public async Task ReleaseForApprovalAsync_WhenApiReturnsSuccess_ReturnsMappedRequestDetail()
        {
            var apiResponse = new ApiResponse<BulkRatesRequestDetailRes> { Success = true, Data = RequestDetailRes("ReleasedForApproval") };
            var mappedDto = ApiResponseDto<BulkRatesRequestDetailDto>.SuccessResponse(RequestDetailDto("ReleasedForApproval"));

            _http.PostAsync<object, BulkRatesRequestDetailRes>(Arg.Any<string>(), Arg.Any<object>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<BulkRatesRequestDetailDto>>(apiResponse).Returns(mappedDto);

            var result = await _client.ReleaseForApprovalAsync(JobExecutionId);

            Assert.True(result.Success);
            Assert.Equal("ReleasedForApproval", result.Data!.Entry.Status);
            await _http.Received(1).PostAsync<object, BulkRatesRequestDetailRes>(
                Arg.Is<string>(u => u.Contains(JobExecutionId.ToString())), Arg.Any<object>());
        }

        [Fact]
        public async Task ApproveAsync_WhenApiReturnsSuccess_ReturnsMappedRequestDetail()
        {
            var apiResponse = new ApiResponse<BulkRatesRequestDetailRes> { Success = true, Data = RequestDetailRes("Approved") };
            var mappedDto = ApiResponseDto<BulkRatesRequestDetailDto>.SuccessResponse(RequestDetailDto("Approved"));

            _http.PostAsync<object, BulkRatesRequestDetailRes>(Arg.Any<string>(), Arg.Any<object>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<BulkRatesRequestDetailDto>>(apiResponse).Returns(mappedDto);

            var result = await _client.ApproveAsync(JobExecutionId);

            Assert.True(result.Success);
            Assert.Equal("Approved", result.Data!.Entry.Status);
        }

        #endregion

        #region RejectAsync / CancelAsync

        [Fact]
        public async Task RejectAsync_SendsReasonInTypedRequestBody()
        {
            var apiResponse = new ApiResponse<BulkRatesRequestDetailRes> { Success = true, Data = RequestDetailRes("Rejected") };
            var mappedDto = ApiResponseDto<BulkRatesRequestDetailDto>.SuccessResponse(RequestDetailDto("Rejected"));

            _http.PostAsync<RejectBulkRatesRequestReq, BulkRatesRequestDetailRes>(Arg.Any<string>(), Arg.Any<RejectBulkRatesRequestReq>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<BulkRatesRequestDetailDto>>(apiResponse).Returns(mappedDto);

            var result = await _client.RejectAsync(JobExecutionId, "not enough evidence");

            Assert.True(result.Success);
            Assert.Equal("Rejected", result.Data!.Entry.Status);
            await _http.Received(1).PostAsync<RejectBulkRatesRequestReq, BulkRatesRequestDetailRes>(
                Arg.Any<string>(), Arg.Is<RejectBulkRatesRequestReq>(r => r.Reason == "not enough evidence"));
        }

        [Fact]
        public async Task CancelAsync_WithNullReason_SendsNullReasonInTypedRequestBody()
        {
            var apiResponse = new ApiResponse<BulkRatesRequestDetailRes> { Success = true, Data = RequestDetailRes("Cancelled") };
            var mappedDto = ApiResponseDto<BulkRatesRequestDetailDto>.SuccessResponse(RequestDetailDto("Cancelled"));

            _http.PostAsync<CancelBulkRatesRequestReq, BulkRatesRequestDetailRes>(Arg.Any<string>(), Arg.Any<CancelBulkRatesRequestReq>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<BulkRatesRequestDetailDto>>(apiResponse).Returns(mappedDto);

            var result = await _client.CancelAsync(JobExecutionId, null);

            Assert.True(result.Success);
            await _http.Received(1).PostAsync<CancelBulkRatesRequestReq, BulkRatesRequestDetailRes>(
                Arg.Any<string>(), Arg.Is<CancelBulkRatesRequestReq>(r => r.Reason == null));
        }

        #endregion

        #region GetRequestAsync

        [Fact]
        public async Task GetRequestAsync_WhenApiReturnsSuccess_ReturnsMappedRequestDetail()
        {
            var apiResponse = new ApiResponse<BulkRatesRequestDetailRes?> { Success = true, Data = RequestDetailRes() };
            var mappedDto = ApiResponseDto<BulkRatesRequestDetailDto?>.SuccessResponse(RequestDetailDto());

            _http.GetAsync<BulkRatesRequestDetailRes?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<BulkRatesRequestDetailDto?>>(apiResponse).Returns(mappedDto);

            var result = await _client.GetRequestAsync(JobExecutionId);

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            await _http.Received(1).GetAsync<BulkRatesRequestDetailRes?>(
                Arg.Is<string>(u => u.Contains(JobExecutionId.ToString())));
        }

        [Fact]
        public async Task GetRequestAsync_WhenApiReturnsNotFound_ReturnsNullData()
        {
            var apiResponse = new ApiResponse<BulkRatesRequestDetailRes?> { Success = true, Data = null };
            var mappedDto = ApiResponseDto<BulkRatesRequestDetailDto?>.SuccessResponse(null);

            _http.GetAsync<BulkRatesRequestDetailRes?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<BulkRatesRequestDetailDto?>>(apiResponse).Returns(mappedDto);

            var result = await _client.GetRequestAsync(JobExecutionId);

            Assert.True(result.Success);
            Assert.Null(result.Data);
        }

        #endregion

        #region GetRequestsAsync (query-string construction)

        [Fact]
        public async Task GetRequestsAsync_BuildsUrlWithJobNameFpsYearAndStatusFilters()
        {
            var apiResponse = new ApiResponse<List<BulkRatesQueueEntryRes>> { Success = true, Data = [] };
            var mappedDto = ApiResponseDto<List<BulkRatesQueueEntryDto>>.SuccessResponse([]);
            string? capturedUrl = null;

            _http.GetAsync<List<BulkRatesQueueEntryRes>>(Arg.Do<string>(u => capturedUrl = u)).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<BulkRatesQueueEntryDto>>>(apiResponse).Returns(mappedDto);

            var query = new QueryParameters<string> { Page = 2, PageSize = 25, Descending = true };
            await _client.GetRequestsAsync(query, jobName: "BulkTestRatesUpdate", fpsYear: 2027, status: "Initiated");

            Assert.NotNull(capturedUrl);
            Assert.Contains("Page=2", capturedUrl);
            Assert.Contains("PageSize=25", capturedUrl);
            Assert.Contains("Descending=True", capturedUrl);
            Assert.Contains("jobName=BulkTestRatesUpdate", capturedUrl);
            Assert.Contains("fpsYear=2027", capturedUrl);
            Assert.Contains("status=Initiated", capturedUrl);
        }

        [Fact]
        public async Task GetRequestsAsync_WithNoOptionalFilters_OmitsThemFromUrl()
        {
            var apiResponse = new ApiResponse<List<BulkRatesQueueEntryRes>> { Success = true, Data = [] };
            var mappedDto = ApiResponseDto<List<BulkRatesQueueEntryDto>>.SuccessResponse([]);
            string? capturedUrl = null;

            _http.GetAsync<List<BulkRatesQueueEntryRes>>(Arg.Do<string>(u => capturedUrl = u)).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<BulkRatesQueueEntryDto>>>(apiResponse).Returns(mappedDto);

            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            await _client.GetRequestsAsync(query);

            Assert.NotNull(capturedUrl);
            Assert.DoesNotContain("jobName=", capturedUrl);
            Assert.DoesNotContain("fpsYear=", capturedUrl);
            Assert.DoesNotContain("status=", capturedUrl);
        }

        #endregion

        #region GetActiveRequestAsync / GetStagingDataAsync

        [Fact]
        public async Task GetActiveRequestAsync_WhenApiReturnsSuccess_ReturnsMappedEntry()
        {
            var entryRes = new BulkRatesQueueEntryRes { JobQueueId = Guid.NewGuid(), Status = "Initiated" };
            var apiResponse = new ApiResponse<BulkRatesQueueEntryRes?> { Success = true, Data = entryRes };
            var mappedDto = ApiResponseDto<BulkRatesQueueEntryDto?>.SuccessResponse(
                new BulkRatesQueueEntryDto { Status = "Initiated" });

            _http.GetAsync<BulkRatesQueueEntryRes?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<BulkRatesQueueEntryDto?>>(apiResponse).Returns(mappedDto);

            var result = await _client.GetActiveRequestAsync("BulkTestRatesUpdate");

            Assert.True(result.Success);
            Assert.Equal("Initiated", result.Data!.Status);
        }

        [Fact]
        public async Task GetStagingDataAsync_WhenApiReturnsSuccess_ReturnsMappedStagingData()
        {
            var stagingRes = new BulkRatesStagingDataRes { FecRows = [new BulkRatesFecStagingRowRes { TestCode = "T1" }] };
            var apiResponse = new ApiResponse<BulkRatesStagingDataRes> { Success = true, Data = stagingRes };
            var mappedDto = ApiResponseDto<BulkRatesStagingDataDto>.SuccessResponse(
                new BulkRatesStagingDataDto { FecRows = [new BulkRatesFecStagingRowDto { TestCode = "T1" }] });

            _http.GetAsync<BulkRatesStagingDataRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<BulkRatesStagingDataDto>>(apiResponse).Returns(mappedDto);

            var result = await _client.GetStagingDataAsync(JobExecutionId);

            Assert.True(result.Success);
            Assert.Single(result.Data!.FecRows);
            Assert.Equal("T1", result.Data.FecRows[0].TestCode);
        }

        #endregion

        #region File downloads (representative — delegation pattern is identical across all 7)

        [Fact]
        public async Task DownloadFecTestDataForRequestAsync_DelegatesToGetFileAsyncWithCorrectUrl()
        {
            byte[] expectedBytes = [1, 2, 3, 4];
            _http.GetFileAsync(Arg.Is<string>(u => u.Contains(JobExecutionId.ToString()))).Returns(expectedBytes);

            var result = await _client.DownloadFecTestDataForRequestAsync(JobExecutionId);

            Assert.Same(expectedBytes, result);
            await _http.Received(1).GetFileAsync(Arg.Is<string>(u => u.Contains(JobExecutionId.ToString())));
        }

        [Fact]
        public async Task DownloadFecTestDataAsync_DelegatesToGetFileAsyncWithFpsYearInUrl()
        {
            byte[] expectedBytes = [5, 6, 7];
            _http.GetFileAsync(Arg.Is<string>(u => u.Contains("2027"))).Returns(expectedBytes);

            var result = await _client.DownloadFecTestDataAsync(2027);

            Assert.Same(expectedBytes, result);
            await _http.Received(1).GetFileAsync(Arg.Is<string>(u => u.Contains("2027")));
        }

        #endregion
    }
}
