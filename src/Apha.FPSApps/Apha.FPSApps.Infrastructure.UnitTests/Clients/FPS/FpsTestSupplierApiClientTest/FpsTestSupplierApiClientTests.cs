using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsTestSupplierApiClientTest
{
    public class FpsTestSupplierApiClientTests
    {
        private const string DefaultTestCode = "TST001";
        private const int DefaultPageNumber = 1;
        private const int DefaultPageSize = 10;

        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly FpsTestSupplierApiClient _client;

        public FpsTestSupplierApiClientTests()
        {
            _http = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsTestSupplierApiClient(_http, _mapper);
        }

        private static List<TestSupplierViewRes> BuildResList(int count = 2) =>
            Enumerable.Range(1, count).Select(i => new TestSupplierViewRes
            {
                TestCode = DefaultTestCode,
                Buyer = $"B{i:D3}",
                UnitPrice = 10m * i,
                NoRequired = i
            }).ToList();

        private static ApiResponseDto<List<TestSupplierViewDto>> BuildSuccessDto(int count = 2) =>
            ApiResponseDto<List<TestSupplierViewDto>>.SuccessResponse(
                Enumerable.Range(1, count).Select(i => new TestSupplierViewDto
                {
                    TestCode = DefaultTestCode,
                    Buyer = $"B{i:D3}"
                }).ToList(),
                new PaginationDto { PageNumber = 1, PageSize = DefaultPageSize, TotalRecords = count });

        #region GetPagedTestSupplierAsync Tests

        [Fact]
        public async Task GetPagedTestSupplierAsync_WithSuccessResponse_ReturnsMappedList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = DefaultPageNumber, PageSize = DefaultPageSize };
            var resList = BuildResList();
            var apiResponse = new ApiResponse<List<TestSupplierViewRes>>
            {
                Success = true,
                Data = resList,
                Pagination = new Pagination { PageNumber = 1, PageSize = DefaultPageSize, TotalRecords = 2 }
            };
            var expectedDto = BuildSuccessDto();

            _http.GetAsync<List<TestSupplierViewRes>>(
                    Arg.Is<string>(url => url.Contains("api/v1/testsupplier")
                        && url.Contains($"testCode={DefaultTestCode}")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestSupplierViewDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedTestSupplierAsync(query, DefaultTestCode, showRejected: false);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<TestSupplierViewRes>>(
                Arg.Is<string>(url => url.Contains("api/v1/testsupplier")));
            _mapper.Received(1).Map<ApiResponseDto<List<TestSupplierViewDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetPagedTestSupplierAsync_ShowRejectedTrue_IncludesFlagInUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<TestSupplierViewRes>>
            {
                Success = true,
                Data = BuildResList(1),
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };
            var expectedDto = BuildSuccessDto(1);

            _http.GetAsync<List<TestSupplierViewRes>>(
                    Arg.Is<string>(url => url.Contains("showRejected=True")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestSupplierViewDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedTestSupplierAsync(query, DefaultTestCode, showRejected: true);

            // Assert
            Assert.NotNull(result);
            await _http.Received(1).GetAsync<List<TestSupplierViewRes>>(
                Arg.Is<string>(url => url.Contains("showRejected=True")));
        }

        [Fact]
        public async Task GetPagedTestSupplierAsync_ApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<TestSupplierViewRes>>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Not found", Code = "NOT_FOUND" } }
            };
            var failureDto = new ApiResponseDto<List<TestSupplierViewDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<TestSupplierViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestSupplierViewDto>>>(apiResponse).Returns(failureDto);

            // Act
            var result = await _client.GetPagedTestSupplierAsync(query, DefaultTestCode, showRejected: false);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetPagedTestSupplierAsync_EmptyResultList_ReturnsSuccessWithEmptyData()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<TestSupplierViewRes>>
            {
                Success = true,
                Data = new List<TestSupplierViewRes>(),
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 0 }
            };
            var emptyDto = ApiResponseDto<List<TestSupplierViewDto>>.SuccessResponse(
                new List<TestSupplierViewDto>(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 });

            _http.GetAsync<List<TestSupplierViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestSupplierViewDto>>>(apiResponse).Returns(emptyDto);

            // Act
            var result = await _client.GetPagedTestSupplierAsync(query, DefaultTestCode, showRejected: false);

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetPagedTestSupplierAsync_HttpThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _http.GetAsync<List<TestSupplierViewRes>>(Arg.Any<string>())
                .Throws(new HttpRequestException("Connection refused"));

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(
                () => _client.GetPagedTestSupplierAsync(query, DefaultTestCode, false));
        }

        #endregion
    }
}
