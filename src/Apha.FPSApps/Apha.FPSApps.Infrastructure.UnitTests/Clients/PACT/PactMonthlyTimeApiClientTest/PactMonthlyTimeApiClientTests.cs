using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using Apha.FPSApps.Infrastructure.Integrations.PACTApis.Clients;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PACT.PactMonthlyTimeApiClientTest
{
    public class PactMonthlyTimeApiClientTests
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PactMonthlyTimeApiClient _client;

        public PactMonthlyTimeApiClientTests()
        {
            _http = Substitute.For<IPactHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            SetupMapper();
            _client = new PactMonthlyTimeApiClient(_http, _mapper);
        }

        private void SetupMapper()
        {
            _mapper.Map<ApiResponseDto<List<MonthlyTimeLogDto>>>(Arg.Any<ApiResponse<List<MonthlyTimeLogRes>>>())
                .Returns(callInfo =>
                {
                    var response = callInfo.ArgAt<ApiResponse<List<MonthlyTimeLogRes>>>(0);
                    if (response == null || !response.Success || response.Data == null)
                        return ApiResponseDto<List<MonthlyTimeLogDto>>.FailureResponse(
                            response?.Errors?.Select(e => new ApiErrorDto { Message = e.Message, Code = e.Code }).ToList() ?? [],
                            new ApiMetaDto());

                    var dtoList = response.Data.Select(res => new MonthlyTimeLogDto
                    {
                        SequenceNo = res.SequenceNo,
                        TimeCode = res.TimeCode,
                        ParentProject = res.ParentProject,
                        Month = res.Month,
                        PactStaffId = res.PactStaffId,
                        WorkGroup = res.WorkGroup,
                        Hours = res.Hours,
                        DateTime = res.DateTime,
                        UserId = res.UserId,
                        InsertDelete = res.InsertDelete,
                        FpsYear = res.FpsYear
                    }).ToList();

                    return ApiResponseDto<List<MonthlyTimeLogDto>>.SuccessResponse(dtoList);
                });
        }

        #region SearchAsync Tests

        [Fact]
        public async Task SearchAsync_WithSuccessResponse_ReturnsMappedDtoList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var filter = new MonthlyTimeLogFilterDto { WorkGroup = "WG1", TimeCode = "TC1" };
            var resList = new List<MonthlyTimeLogRes>
            {
                new() { SequenceNo = 1, TimeCode = "TC1", PactStaffId = "S001", WorkGroup = "WG1" },
                new() { SequenceNo = 2, TimeCode = "TC1", PactStaffId = "S002", WorkGroup = "WG1" }
            };
            var httpResponse = new ApiResponse<List<MonthlyTimeLogRes>> { Success = true, Data = resList };

            _http.GetAsync<List<MonthlyTimeLogRes>>(Arg.Any<string>()).Returns(httpResponse);

            // Act
            var result = await _client.SearchAsync(query, filter);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<MonthlyTimeLogRes>>(Arg.Any<string>());
        }

        [Fact]
        public async Task SearchAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var filter = new MonthlyTimeLogFilterDto();
            var httpResponse = new ApiResponse<List<MonthlyTimeLogRes>> { Success = true, Data = new List<MonthlyTimeLogRes>() };

            _http.GetAsync<List<MonthlyTimeLogRes>>(Arg.Any<string>()).Returns(httpResponse);

            // Act
            var result = await _client.SearchAsync(query, filter);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task SearchAsync_WhenHttpFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var filter = new MonthlyTimeLogFilterDto { WorkGroup = "WG1" };
            var httpResponse = new ApiResponse<List<MonthlyTimeLogRes>>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "HTTP Error", Code = "HTTP_ERROR" } }
            };

            _http.GetAsync<List<MonthlyTimeLogRes>>(Arg.Any<string>()).Returns(httpResponse);

            // Act
            var result = await _client.SearchAsync(query, filter);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task SearchAsync_WithAllFilters_AppendsFilterParamsToUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var filter = new MonthlyTimeLogFilterDto
            {
                WorkGroup = "WG1",
                TimeCode = "TC1",
                PactStaffId = "S001",
                ParentProject = "PP1",
                DateImported = new DateTime(2024, 6, 1),
                Month = 6.0,
                UserId = "USER1",
                InsertDelete = "I"
            };
            var httpResponse = new ApiResponse<List<MonthlyTimeLogRes>> { Success = true, Data = new List<MonthlyTimeLogRes>() };

            _http.GetAsync<List<MonthlyTimeLogRes>>(Arg.Any<string>()).Returns(httpResponse);

            // Act
            var result = await _client.SearchAsync(query, filter);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<MonthlyTimeLogRes>>(
                Arg.Is<string>(url =>
                    url.Contains("workGroup=WG1") &&
                    url.Contains("timeCode=TC1") &&
                    url.Contains("pactStaffId=S001") &&
                    url.Contains("parentProject=PP1") &&
                    url.Contains("dateImported=2024-06-01") &&
                    url.Contains("month=6") &&
                    url.Contains("userId=USER1") &&
                    url.Contains("insertDelete=I")));
        }

        [Fact]
        public async Task SearchAsync_WithNullFilters_DoesNotAppendFilterParamsToUrl()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var filter = new MonthlyTimeLogFilterDto();
            var httpResponse = new ApiResponse<List<MonthlyTimeLogRes>> { Success = true, Data = new List<MonthlyTimeLogRes>() };

            _http.GetAsync<List<MonthlyTimeLogRes>>(Arg.Any<string>()).Returns(httpResponse);

            // Act
            var result = await _client.SearchAsync(query, filter);

            // Assert
            Assert.NotNull(result);
            await _http.Received(1).GetAsync<List<MonthlyTimeLogRes>>(
                Arg.Is<string>(url =>
                    !url.Contains("workGroup=") &&
                    !url.Contains("timeCode=") &&
                    !url.Contains("pactStaffId=") &&
                    !url.Contains("parentProject=") &&
                    !url.Contains("dateImported=") &&
                    !url.Contains("month=") &&
                    !url.Contains("userId=") &&
                    !url.Contains("insertDelete=")));
        }

        [Fact]
        public async Task SearchAsync_UrlContainsBaseEndpoint()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var filter = new MonthlyTimeLogFilterDto();
            var httpResponse = new ApiResponse<List<MonthlyTimeLogRes>> { Success = true, Data = new List<MonthlyTimeLogRes>() };

            _http.GetAsync<List<MonthlyTimeLogRes>>(Arg.Any<string>()).Returns(httpResponse);

            // Act
            await _client.SearchAsync(query, filter);

            // Assert
            await _http.Received(1).GetAsync<List<MonthlyTimeLogRes>>(
                Arg.Is<string>(url => url.Contains("api/v1/monthlytime/log/search")));
        }

        [Fact]
        public async Task SearchAsync_WorkGroupOnly_AppendsOnlyWorkGroupToUrl()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var filter = new MonthlyTimeLogFilterDto { WorkGroup = "WG1" };
            var httpResponse = new ApiResponse<List<MonthlyTimeLogRes>> { Success = true, Data = new List<MonthlyTimeLogRes>() };

            _http.GetAsync<List<MonthlyTimeLogRes>>(Arg.Any<string>()).Returns(httpResponse);

            // Act
            await _client.SearchAsync(query, filter);

            // Assert
            await _http.Received(1).GetAsync<List<MonthlyTimeLogRes>>(
                Arg.Is<string>(url =>
                    url.Contains("workGroup=WG1") &&
                    !url.Contains("timeCode=") &&
                    !url.Contains("pactStaffId=") &&
                    !url.Contains("parentProject=") &&
                    !url.Contains("dateImported=") &&
                    !url.Contains("month=") &&
                    !url.Contains("userId=") &&
                    !url.Contains("insertDelete=")));
        }

        [Fact]
        public async Task SearchAsync_DateImportedOnly_AppendsFormattedDateToUrl()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var filter = new MonthlyTimeLogFilterDto { DateImported = new DateTime(2024, 3, 5) };
            var httpResponse = new ApiResponse<List<MonthlyTimeLogRes>> { Success = true, Data = new List<MonthlyTimeLogRes>() };

            _http.GetAsync<List<MonthlyTimeLogRes>>(Arg.Any<string>()).Returns(httpResponse);

            // Act
            await _client.SearchAsync(query, filter);

            // Assert
            await _http.Received(1).GetAsync<List<MonthlyTimeLogRes>>(
                Arg.Is<string>(url => url.Contains("dateImported=2024-03-05")));
        }

        [Fact]
        public async Task SearchAsync_MonthOnly_AppendsMonthToUrl()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var filter = new MonthlyTimeLogFilterDto { Month = 9.0 };
            var httpResponse = new ApiResponse<List<MonthlyTimeLogRes>> { Success = true, Data = new List<MonthlyTimeLogRes>() };

            _http.GetAsync<List<MonthlyTimeLogRes>>(Arg.Any<string>()).Returns(httpResponse);

            // Act
            await _client.SearchAsync(query, filter);

            // Assert
            await _http.Received(1).GetAsync<List<MonthlyTimeLogRes>>(
                Arg.Is<string>(url => url.Contains("month=9")));
        }

        [Fact]
        public async Task SearchAsync_HttpExecutorThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var filter = new MonthlyTimeLogFilterDto();

            _http.GetAsync<List<MonthlyTimeLogRes>>(Arg.Any<string>())
                .ThrowsAsync(new Exception("HTTP executor error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _client.SearchAsync(query, filter));
        }

        #endregion

        #region Live Methods Tests

        [Fact]
        public async Task GetLiveAsync_WithSuccessResponse_ReturnsMappedData()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<MonthlyTimeRes>>
            {
                Success = true,
                Data = [new MonthlyTimeRes { PactStaffId = "S1", TimeCode = "TC1", ParentProject = "PP1", Month = 6, WorkGroup = "WG1", Hours = 7 }]
            };
            var mapped = ApiResponseDto<List<MonthlyTimeDto>>.SuccessResponse(
            [
                new MonthlyTimeDto { PactStaffId = "S1", TimeCode = "TC1", ParentProject = "PP1", Month = 6, WorkGroup = "WG1", Hours = 7 }
            ]);

            _http.GetAsync<List<MonthlyTimeRes>>(Arg.Is<string>(url =>
                url.Contains("monthlytime/live") &&
                url.Contains("workGroup=WG1") &&
                url.Contains("timeCode=TC1") &&
                url.Contains("pactStaffId=S1") &&
                url.Contains("parentProject=PP1") &&
                url.Contains("month=6"))).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<MonthlyTimeDto>>>(apiResponse).Returns(mapped);

            var result = await _client.GetLiveAsync(query, "WG1", "TC1", "S1", "PP1", 6);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
        }

        [Fact]
        public async Task GetLiveByKeyAsync_WhenApiFails_ReturnsFailureResponse()
        {
            var apiResponse = new ApiResponse<MonthlyTimeRes>
            {
                Success = false,
                Errors = [new ApiError { Message = "failed", Code = "ERR" }]
            };
            var mapped = new ApiResponseDto<MonthlyTimeDto>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "failed", Code = "ERR" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<MonthlyTimeRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MonthlyTimeDto>>(apiResponse).Returns(mapped);

            var result = await _client.GetLiveByKeyAsync("S1", "TC1", 6, "PP1");

            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        [Fact]
        public async Task UpdateLiveAsync_WithSuccessResponse_ReturnsMappedData()
        {
            var dto = new MonthlyTimeDto { PactStaffId = "S1", TimeCode = "TC1", ParentProject = "PP1", Month = 6, WorkGroup = "WG1", Hours = 7 };
            var request = new MonthlyTimeReq { PactStaffId = "S1", TimeCode = "TC1", ParentProject = "PP1", Month = 6, WorkGroup = "WG1", Hours = 7 };
            var apiResponse = new ApiResponse<MonthlyTimeRes>
            {
                Success = true,
                Data = new MonthlyTimeRes { PactStaffId = "S1", TimeCode = "TC1", ParentProject = "PP1", Month = 6, WorkGroup = "WG1", Hours = 7 }
            };
            var mapped = ApiResponseDto<MonthlyTimeDto>.SuccessResponse(dto);

            _mapper.Map<MonthlyTimeReq>(dto).Returns(request);
            _http.PutAsync<MonthlyTimeReq, MonthlyTimeRes>(Arg.Any<string>(), request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MonthlyTimeDto>>(apiResponse).Returns(mapped);

            var result = await _client.UpdateLiveAsync(dto);

            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).PutAsync<MonthlyTimeReq, MonthlyTimeRes>(Arg.Any<string>(), request);
        }

        [Fact]
        public async Task GetLiveAsync_WhenApiFails_ReturnsFailureResponse()
        {
            var query = new QueryParameters<string>();
            var apiResponse = new ApiResponse<List<MonthlyTimeRes>>
            {
                Success = false,
                Errors = [new ApiError { Message = "fail", Code = "ERR" }]
            };
            var mapped = new ApiResponseDto<List<MonthlyTimeDto>>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "fail", Code = "ERR" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<MonthlyTimeRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<MonthlyTimeDto>>>(apiResponse).Returns(mapped);

            var result = await _client.GetLiveAsync(query, null, null, null, null, null);

            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task GetLiveAsync_WithNullFilters_DoesNotAppendFilterParams()
        {
            var query = new QueryParameters<string>();
            var apiResponse = new ApiResponse<List<MonthlyTimeRes>> { Success = true, Data = [] };
            var mapped = ApiResponseDto<List<MonthlyTimeDto>>.SuccessResponse([]);

            _http.GetAsync<List<MonthlyTimeRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<MonthlyTimeDto>>>(apiResponse).Returns(mapped);

            await _client.GetLiveAsync(query, null, null, null, null, null);

            await _http.Received(1).GetAsync<List<MonthlyTimeRes>>(
                Arg.Is<string>(url =>
                    !url.Contains("workGroup=") &&
                    !url.Contains("timeCode=") &&
                    !url.Contains("pactStaffId=") &&
                    !url.Contains("parentProject=") &&
                    !url.Contains("month=")));
        }

        [Fact]
        public async Task GetLiveByKeyAsync_WithSuccessResponse_ReturnsMappedData()
        {
            var apiResponse = new ApiResponse<MonthlyTimeRes>
            {
                Success = true,
                Data = new MonthlyTimeRes { PactStaffId = "S1", TimeCode = "TC1", ParentProject = "PP1", Month = 6 }
            };
            var mapped = ApiResponseDto<MonthlyTimeDto>.SuccessResponse(
                new MonthlyTimeDto { PactStaffId = "S1", TimeCode = "TC1", ParentProject = "PP1", Month = 6 });

            _http.GetAsync<MonthlyTimeRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MonthlyTimeDto>>(apiResponse).Returns(mapped);

            var result = await _client.GetLiveByKeyAsync("S1", "TC1", 6, "PP1");

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
        }

        [Fact]
        public async Task UpdateLiveAsync_WhenApiFails_ReturnsFailureResponse()
        {
            var dto = new MonthlyTimeDto { PactStaffId = "S1" };
            var request = new MonthlyTimeReq { PactStaffId = "S1" };
            var apiResponse = new ApiResponse<MonthlyTimeRes>
            {
                Success = false,
                Errors = [new ApiError { Message = "fail", Code = "ERR" }]
            };
            var mapped = new ApiResponseDto<MonthlyTimeDto>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "fail", Code = "ERR" }],
                Meta = null!
            };

            _mapper.Map<MonthlyTimeReq>(dto).Returns(request);
            _http.PutAsync<MonthlyTimeReq, MonthlyTimeRes>(Arg.Any<string>(), request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MonthlyTimeDto>>(apiResponse).Returns(mapped);

            var result = await _client.UpdateLiveAsync(dto);

            Assert.False(result.Success);
            Assert.NotNull(result.Meta);
        }

        #endregion

        #region Staging Methods Tests

        [Fact]
        public async Task GetStagingAsync_WithPassedFilter_AppendsPassedQuery()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<StagingMonthlyTimeRes>> { Success = true, Data = [] };
            var mapped = ApiResponseDto<List<StagingMonthlyTimeDto>>.SuccessResponse([]);

            _http.GetAsync<List<StagingMonthlyTimeRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<StagingMonthlyTimeDto>>>(apiResponse).Returns(mapped);

            var result = await _client.GetStagingAsync(query, false);

            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<StagingMonthlyTimeRes>>(Arg.Is<string>(url => url.Contains("passed=false")));
        }

        [Fact]
        public async Task GetStagingAsync_WithNullPassed_DoesNotAppendPassedQuery()
        {
            var query = new QueryParameters<string>();
            var apiResponse = new ApiResponse<List<StagingMonthlyTimeRes>> { Success = true, Data = [] };
            var mapped = ApiResponseDto<List<StagingMonthlyTimeDto>>.SuccessResponse([]);

            _http.GetAsync<List<StagingMonthlyTimeRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<StagingMonthlyTimeDto>>>(apiResponse).Returns(mapped);

            await _client.GetStagingAsync(query, null);

            await _http.Received(1).GetAsync<List<StagingMonthlyTimeRes>>(Arg.Is<string>(url => !url.Contains("passed=")));
        }

        [Fact]
        public async Task GetStagingAsync_WhenApiFails_ReturnsFailureResponse()
        {
            var query = new QueryParameters<string>();
            var apiResponse = new ApiResponse<List<StagingMonthlyTimeRes>>
            {
                Success = false,
                Errors = [new ApiError { Message = "fail" }]
            };
            var mapped = new ApiResponseDto<List<StagingMonthlyTimeDto>>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "fail" }],
                Meta = null!
            };

            _http.GetAsync<List<StagingMonthlyTimeRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<StagingMonthlyTimeDto>>>(apiResponse).Returns(mapped);

            var result = await _client.GetStagingAsync(query, null);

            Assert.False(result.Success);
            Assert.NotNull(result.Meta);
        }

        [Fact]
        public async Task GetStagingByIdAsync_WithSuccessResponse_ReturnsMappedData()
        {
            var apiResponse = new ApiResponse<StagingMonthlyTimeRes>
            {
                Success = true,
                Data = new StagingMonthlyTimeRes { PactStaffId = "S1" }
            };
            var mapped = ApiResponseDto<StagingMonthlyTimeDto>.SuccessResponse(new StagingMonthlyTimeDto { PactStaffId = "S1" });

            _http.GetAsync<StagingMonthlyTimeRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<StagingMonthlyTimeDto>>(apiResponse).Returns(mapped);

            var result = await _client.GetStagingByIdAsync(1);

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
        }

        [Fact]
        public async Task GetStagingByIdAsync_WhenApiFails_ReturnsFailureResponse()
        {
            var apiResponse = new ApiResponse<StagingMonthlyTimeRes>
            {
                Success = false,
                Errors = [new ApiError { Message = "not found" }]
            };
            var mapped = new ApiResponseDto<StagingMonthlyTimeDto>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "not found" }],
                Meta = null!
            };

            _http.GetAsync<StagingMonthlyTimeRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<StagingMonthlyTimeDto>>(apiResponse).Returns(mapped);

            var result = await _client.GetStagingByIdAsync(999);

            Assert.False(result.Success);
            Assert.NotNull(result.Meta);
        }

        [Fact]
        public async Task CreateStagingAsync_WithSuccessResponse_ReturnsMappedData()
        {
            var dto = new StagingMonthlyTimeDto { PactStaffId = "S1" };
            var request = new StagingMonthlyTimeReq { PactStaffId = "S1" };
            var apiResponse = new ApiResponse<StagingMonthlyTimeRes>
            {
                Success = true,
                Data = new StagingMonthlyTimeRes { PactStaffId = "S1" }
            };
            var mapped = ApiResponseDto<StagingMonthlyTimeDto>.SuccessResponse(dto);

            _mapper.Map<StagingMonthlyTimeReq>(dto).Returns(request);
            _http.PostAsync<StagingMonthlyTimeReq, StagingMonthlyTimeRes>(Arg.Any<string>(), request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<StagingMonthlyTimeDto>>(apiResponse).Returns(mapped);

            var result = await _client.CreateStagingAsync(dto);

            Assert.True(result.Success);
            await _http.Received(1).PostAsync<StagingMonthlyTimeReq, StagingMonthlyTimeRes>(Arg.Any<string>(), request);
        }

        [Fact]
        public async Task CreateStagingAsync_WhenApiFails_ReturnsFailureResponse()
        {
            var dto = new StagingMonthlyTimeDto { PactStaffId = "S1" };
            var request = new StagingMonthlyTimeReq { PactStaffId = "S1" };
            var apiResponse = new ApiResponse<StagingMonthlyTimeRes>
            {
                Success = false,
                Errors = [new ApiError { Message = "fail" }]
            };
            var mapped = new ApiResponseDto<StagingMonthlyTimeDto>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "fail" }],
                Meta = null!
            };

            _mapper.Map<StagingMonthlyTimeReq>(dto).Returns(request);
            _http.PostAsync<StagingMonthlyTimeReq, StagingMonthlyTimeRes>(Arg.Any<string>(), request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<StagingMonthlyTimeDto>>(apiResponse).Returns(mapped);

            var result = await _client.CreateStagingAsync(dto);

            Assert.False(result.Success);
            Assert.NotNull(result.Meta);
        }

        [Fact]
        public async Task UpdateStagingAsync_WithSuccessResponse_ReturnsMappedData()
        {
            var dto = new StagingMonthlyTimeDto { PactStaffId = "S1" };
            var request = new StagingMonthlyTimeReq { PactStaffId = "S1" };
            var apiResponse = new ApiResponse<StagingMonthlyTimeRes>
            {
                Success = true,
                Data = new StagingMonthlyTimeRes { PactStaffId = "S1" }
            };
            var mapped = ApiResponseDto<StagingMonthlyTimeDto>.SuccessResponse(dto);

            _mapper.Map<StagingMonthlyTimeReq>(dto).Returns(request);
            _http.PutAsync<StagingMonthlyTimeReq, StagingMonthlyTimeRes>(Arg.Any<string>(), request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<StagingMonthlyTimeDto>>(apiResponse).Returns(mapped);

            var result = await _client.UpdateStagingAsync(1, dto);

            Assert.True(result.Success);
            await _http.Received(1).PutAsync<StagingMonthlyTimeReq, StagingMonthlyTimeRes>(Arg.Any<string>(), request);
        }

        [Fact]
        public async Task UpdateStagingAsync_WhenApiFails_ReturnsFailureResponse()
        {
            var dto = new StagingMonthlyTimeDto { PactStaffId = "S1" };
            var request = new StagingMonthlyTimeReq { PactStaffId = "S1" };
            var apiResponse = new ApiResponse<StagingMonthlyTimeRes>
            {
                Success = false,
                Errors = [new ApiError { Message = "fail" }]
            };
            var mapped = new ApiResponseDto<StagingMonthlyTimeDto>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "fail" }],
                Meta = null!
            };

            _mapper.Map<StagingMonthlyTimeReq>(dto).Returns(request);
            _http.PutAsync<StagingMonthlyTimeReq, StagingMonthlyTimeRes>(Arg.Any<string>(), request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<StagingMonthlyTimeDto>>(apiResponse).Returns(mapped);

            var result = await _client.UpdateStagingAsync(1, dto);

            Assert.False(result.Success);
            Assert.NotNull(result.Meta);
        }

        [Fact]
        public async Task BulkUpdateStagingNamesAsync_WithSuccessResponse_ReturnsMappedData()
        {
            var dto = new BulkUpdateStagingMonthlyTimeNamesDto();
            var request = new BulkUpdateStagingMonthlyTimeNamesReq();
            var apiResponse = new ApiResponse<BulkUpdateStagingMonthlyTimeNamesRes> { Success = true, Data = new BulkUpdateStagingMonthlyTimeNamesRes() };
            var mapped = ApiResponseDto<BulkUpdateStagingMonthlyTimeNamesResultDto>.SuccessResponse(new BulkUpdateStagingMonthlyTimeNamesResultDto());

            _mapper.Map<BulkUpdateStagingMonthlyTimeNamesReq>(dto).Returns(request);
            _http.PostAsync<BulkUpdateStagingMonthlyTimeNamesReq, BulkUpdateStagingMonthlyTimeNamesRes>(Arg.Any<string>(), request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<BulkUpdateStagingMonthlyTimeNamesResultDto>>(apiResponse).Returns(mapped);

            var result = await _client.BulkUpdateStagingNamesAsync(dto);

            Assert.True(result.Success);
        }

        [Fact]
        public async Task BulkUpdateStagingNamesAsync_WhenApiFails_ReturnsFailureResponse()
        {
            var dto = new BulkUpdateStagingMonthlyTimeNamesDto();
            var request = new BulkUpdateStagingMonthlyTimeNamesReq();
            var apiResponse = new ApiResponse<BulkUpdateStagingMonthlyTimeNamesRes>
            {
                Success = false,
                Errors = [new ApiError { Message = "fail" }]
            };
            var mapped = new ApiResponseDto<BulkUpdateStagingMonthlyTimeNamesResultDto>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "fail" }],
                Meta = null!
            };

            _mapper.Map<BulkUpdateStagingMonthlyTimeNamesReq>(dto).Returns(request);
            _http.PostAsync<BulkUpdateStagingMonthlyTimeNamesReq, BulkUpdateStagingMonthlyTimeNamesRes>(Arg.Any<string>(), request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<BulkUpdateStagingMonthlyTimeNamesResultDto>>(apiResponse).Returns(mapped);

            var result = await _client.BulkUpdateStagingNamesAsync(dto);

            Assert.False(result.Success);
            Assert.NotNull(result.Meta);
        }

        [Fact]
        public async Task DeleteStagingAsync_WithSuccessResponse_ReturnsMappedData()
        {
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var mapped = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mapped);

            var result = await _client.DeleteStagingAsync(1);

            Assert.True(result.Success);
            Assert.True(result.Data);
        }

        [Fact]
        public async Task DeleteStagingAsync_WhenApiFails_ReturnsFailureResponse()
        {
            var apiResponse = new ApiResponse<bool?> { Success = false, Errors = [new ApiError { Message = "fail" }] };
            var mapped = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "fail" }],
                Meta = null!
            };

            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mapped);

            var result = await _client.DeleteStagingAsync(1);

            Assert.False(result.Success);
            Assert.NotNull(result.Meta);
        }

        [Fact]
        public async Task DeleteAllStagingByUserAsync_WithSuccessAndDataTrue_ReturnsSuccessWithNoErrors()
        {
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var mapped = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mapped);

            var result = await _client.DeleteAllStagingByUserAsync();

            Assert.True(result.Success);
            Assert.True(result.Data);
        }

        [Fact]
        public async Task DeleteAllStagingByUserAsync_WithSuccessAndDataFalse_ReturnsNoRecordsMessage()
        {
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = false };
            var mapped = ApiResponseDto<bool>.SuccessResponse(false);

            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mapped);

            var result = await _client.DeleteAllStagingByUserAsync();

            Assert.True(result.Success);
            Assert.False(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Contains(result.Errors!, e => e.Message!.Contains("No staging records found"));
        }

        [Fact]
        public async Task DeleteAllStagingByUserAsync_WhenApiFails_ReturnsFailureResponse()
        {
            var apiResponse = new ApiResponse<bool?> { Success = false, Errors = [new ApiError { Message = "fail" }] };
            var mapped = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "fail" }],
                Meta = null!
            };

            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mapped);

            var result = await _client.DeleteAllStagingByUserAsync();

            Assert.False(result.Success);
            Assert.NotNull(result.Meta);
        }

        [Fact]
        public async Task DeleteFailedStagingByUserAsync_WithSuccessAndDataTrue_ReturnsSuccessWithNoErrors()
        {
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var mapped = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mapped);

            var result = await _client.DeleteFailedStagingByUserAsync();

            Assert.True(result.Success);
            Assert.True(result.Data);
        }

        [Fact]
        public async Task DeleteFailedStagingByUserAsync_WithSuccessAndDataFalse_ReturnsNoRecordsMessage()
        {
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = false };
            var mapped = ApiResponseDto<bool>.SuccessResponse(false);

            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mapped);

            var result = await _client.DeleteFailedStagingByUserAsync();

            Assert.True(result.Success);
            Assert.False(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Contains(result.Errors!, e => e.Message!.Contains("No failed imported records"));
        }

        [Fact]
        public async Task DeleteFailedStagingByUserAsync_WhenApiFails_ReturnsFailureResponse()
        {
            var apiResponse = new ApiResponse<bool?> { Success = false, Errors = [new ApiError { Message = "fail" }] };
            var mapped = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "fail" }],
                Meta = null!
            };

            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mapped);

            var result = await _client.DeleteFailedStagingByUserAsync();

            Assert.False(result.Success);
            Assert.NotNull(result.Meta);
        }

        [Fact]
        public async Task ImportStagingAsync_WithSuccessResponse_ReturnsMappedImportResult()
        {
            var reqDto = new MonthlyTimeImportReqDto();
            var request = new MonthlyTimeImportReq();
            var importRes = new MonthlyTimeImportRes();
            var apiResponse = new ApiResponse<MonthlyTimeImportRes> { Success = true, Data = importRes };
            var mappedResult = new MonthlyTimeImportResultDto();

            _mapper.Map<MonthlyTimeImportReq>(reqDto).Returns(request);
            _http.PostAsync<MonthlyTimeImportReq, MonthlyTimeImportRes>(Arg.Any<string>(), request).Returns(apiResponse);
            _mapper.Map<MonthlyTimeImportResultDto>(importRes).Returns(mappedResult);

            var result = await _client.ImportStagingAsync(reqDto);

            Assert.True(result.Success);
            Assert.Same(mappedResult, result.Data);
        }

        [Fact]
        public async Task ImportStagingAsync_WhenApiFails_ReturnsFailureResponse()
        {
            var reqDto = new MonthlyTimeImportReqDto();
            var request = new MonthlyTimeImportReq();
            var apiResponse = new ApiResponse<MonthlyTimeImportRes>
            {
                Success = false,
                Errors = [new ApiError { Message = "fail" }]
            };
            var mapped = new ApiResponseDto<MonthlyTimeImportResultDto>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "fail" }],
                Meta = null!
            };

            _mapper.Map<MonthlyTimeImportReq>(reqDto).Returns(request);
            _http.PostAsync<MonthlyTimeImportReq, MonthlyTimeImportRes>(Arg.Any<string>(), request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MonthlyTimeImportResultDto>>(apiResponse).Returns(mapped);

            var result = await _client.ImportStagingAsync(reqDto);

            Assert.False(result.Success);
            Assert.NotNull(result.Meta);
        }

        [Fact]
        public async Task ValidateStagingAsync_WithSuccessResponse_ReturnsMappedResult()
        {
            var validateRes = new MonthlyTimeValidateRes();
            var apiResponse = new ApiResponse<MonthlyTimeValidateRes> { Success = true, Data = validateRes };
            var mappedResult = new MonthlyTimeValidateResultDto();

            _http.PostAsync<object, MonthlyTimeValidateRes>(Arg.Any<string>(), Arg.Any<object>()).Returns(apiResponse);
            _mapper.Map<MonthlyTimeValidateResultDto>(validateRes).Returns(mappedResult);

            var result = await _client.ValidateStagingAsync();

            Assert.True(result.Success);
            Assert.Same(mappedResult, result.Data);
        }

        [Fact]
        public async Task ValidateStagingAsync_WhenApiFails_ReturnsFailureResponse()
        {
            var apiResponse = new ApiResponse<MonthlyTimeValidateRes>
            {
                Success = false,
                Errors = [new ApiError { Message = "fail" }]
            };
            var mapped = new ApiResponseDto<MonthlyTimeValidateResultDto>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "fail" }],
                Meta = null!
            };

            _http.PostAsync<object, MonthlyTimeValidateRes>(Arg.Any<string>(), Arg.Any<object>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MonthlyTimeValidateResultDto>>(apiResponse).Returns(mapped);

            var result = await _client.ValidateStagingAsync();

            Assert.False(result.Success);
            Assert.NotNull(result.Meta);
        }

        [Fact]
        public async Task MakeLiveAsync_WithSuccessAndNoFailures_ReturnsSuccessWithNoErrors()
        {
            var makeLiveRes = new MonthlyTimeMakeLiveRes();
            var apiResponse = new ApiResponse<MonthlyTimeMakeLiveRes> { Success = true, Data = makeLiveRes };
            var mappedResult = new MonthlyTimeMakeLiveResultDto { FailedCount = 0 };

            _http.PostAsync<object, MonthlyTimeMakeLiveRes>(Arg.Any<string>(), Arg.Any<object>()).Returns(apiResponse);
            _mapper.Map<MonthlyTimeMakeLiveResultDto>(makeLiveRes).Returns(mappedResult);

            var result = await _client.MakeLiveAsync();

            Assert.True(result.Success);
        }

        [Fact]
        public async Task MakeLiveAsync_WithSuccessAndFailures_ReturnsSuccessWithErrorMessage()
        {
            var makeLiveRes = new MonthlyTimeMakeLiveRes();
            var apiResponse = new ApiResponse<MonthlyTimeMakeLiveRes> { Success = true, Data = makeLiveRes };
            var mappedResult = new MonthlyTimeMakeLiveResultDto { FailedCount = 3, Message = "3 records failed" };

            _http.PostAsync<object, MonthlyTimeMakeLiveRes>(Arg.Any<string>(), Arg.Any<object>()).Returns(apiResponse);
            _mapper.Map<MonthlyTimeMakeLiveResultDto>(makeLiveRes).Returns(mappedResult);

            var result = await _client.MakeLiveAsync();

            Assert.True(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Contains(result.Errors!, e => e.Message == "3 records failed");
        }

        [Fact]
        public async Task MakeLiveAsync_WhenApiFails_ReturnsFailureResponse()
        {
            var apiResponse = new ApiResponse<MonthlyTimeMakeLiveRes>
            {
                Success = false,
                Errors = [new ApiError { Message = "fail" }]
            };
            var mapped = new ApiResponseDto<MonthlyTimeMakeLiveResultDto>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "fail" }],
                Meta = null!
            };

            _http.PostAsync<object, MonthlyTimeMakeLiveRes>(Arg.Any<string>(), Arg.Any<object>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MonthlyTimeMakeLiveResultDto>>(apiResponse).Returns(mapped);

            var result = await _client.MakeLiveAsync();

            Assert.False(result.Success);
            Assert.NotNull(result.Meta);
        }

        #endregion
    }
}
