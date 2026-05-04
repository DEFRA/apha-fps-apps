using Apha.Common.Constants;
using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using Apha.FPSApps.Infrastructure.Integrations.PACTApis.Clients;
using AutoMapper;
using NSubstitute;

namespace Apha.FPSApps.Infrastructure.UnitTests.Integrations.PACTApis
{
    public class PactProjectInvoiceApiClientTests
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PactProjectInvoiceApiClient _client;

        public PactProjectInvoiceApiClientTests()
        {
            _http = Substitute.For<IPactHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PactProjectInvoiceApiClient(_http, _mapper);
        }

        #region GetMonthlyInvoicesSummaryAsync

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_SuccessResponse_ReturnsMappedPivotDto()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var pivotRes = new MonthlyInvoicesPivotRes
            {
                Months = [1, 2],
                Rows = [],
                Pagination = new Pagination()
            };
            var apiResponse = new ApiResponse<MonthlyInvoicesPivotRes> { Success = true, Data = pivotRes };
            var expectedDto = new MonthlyInvoicesPivotDto { Months = [1, 2] };

            _http.GetAsync<MonthlyInvoicesPivotRes>(Arg.Any<string>())
                .Returns(apiResponse);
            _mapper.Map<MonthlyInvoicesPivotDto>(pivotRes)
                .Returns(expectedDto);

            // Act
            var result = await _client.GetMonthlyInvoicesSummaryAsync(query);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(expectedDto, result.Data);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_FailureResponse_ReturnsFailure()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var apiResponse = new ApiResponse<MonthlyInvoicesPivotRes>
            {
                Success = false,
                Errors = [new ApiError { Message = "Server error" }]
            };
            var errDto = new ApiResponseDto<MonthlyInvoicesPivotDto>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Server error" }],
                Meta = new ApiMetaDto()
            };
            _http.GetAsync<MonthlyInvoicesPivotRes>(Arg.Any<string>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MonthlyInvoicesPivotDto>>(apiResponse)
                .Returns(errDto);

            // Act
            var result = await _client.GetMonthlyInvoicesSummaryAsync(query);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_UsesCorrectEndpoint()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var apiResponse = new ApiResponse<MonthlyInvoicesPivotRes> { Success = true, Data = new MonthlyInvoicesPivotRes() };
            _http.GetAsync<MonthlyInvoicesPivotRes>(Arg.Any<string>())
                .Returns(apiResponse);
            _mapper.Map<MonthlyInvoicesPivotDto>(Arg.Any<MonthlyInvoicesPivotRes>())
                .Returns(new MonthlyInvoicesPivotDto());

            // Act
            await _client.GetMonthlyInvoicesSummaryAsync(query);

            // Assert: URL must start with the correct endpoint constant
            await _http.Received(1).GetAsync<MonthlyInvoicesPivotRes>(
                Arg.Is<string>(url => url.StartsWith(PactApiEndpoints.GetMonthlyInvoicesSummary)));
        }

        #endregion

        #region GetPagedProjectInvoicesAsync

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_SuccessWithParentProject_ReturnsMappedList()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var apiResponse = new ApiResponse<List<ProjectInvoiceRes>>
            {
                Success = true,
                Data = [new ProjectInvoiceRes()]
            };
            var mapped = new ApiResponseDto<List<ProjectInvoiceDto>>
            {
                Success = true,
                Data = [new ProjectInvoiceDto()]
            };
            _http.GetAsync<List<ProjectInvoiceRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectInvoiceDto>>>(apiResponse).Returns(mapped);

            // Act
            var result = await _client.GetPagedProjectInvoicesAsync(query, "PRJ001");

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_FailureResponse_ReturnsFailure()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var apiResponse = new ApiResponse<List<ProjectInvoiceRes>>
            {
                Success = false,
                Errors = [new ApiError { Message = "error" }]
            };
            var errDto = new ApiResponseDto<List<ProjectInvoiceDto>>
            {
                Success = false,
                Errors = [],
                Meta = new ApiMetaDto()
            };
            _http.GetAsync<List<ProjectInvoiceRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectInvoiceDto>>>(apiResponse).Returns(errDto);

            // Act
            var result = await _client.GetPagedProjectInvoicesAsync(query, null);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region GetTotalAmountAsync

        [Fact]
        public async Task GetTotalAmountAsync_SuccessResponse_ReturnsMappedTotal()
        {
            // Arrange
            var apiResponse = new ApiResponse<decimal?> { Success = true, Data = 2500m };
            var mapped = new ApiResponseDto<decimal> { Success = true, Data = 2500m };
            _http.GetAsync<decimal?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<decimal>>(apiResponse).Returns(mapped);

            // Act
            var result = await _client.GetTotalAmountAsync("PRJ001");

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2500m, result.Data);
        }

        [Fact]
        public async Task GetTotalAmountAsync_FailureResponse_ReturnsFailure()
        {
            // Arrange
            var apiResponse = new ApiResponse<decimal?>
            {
                Success = false,
                Errors = [new ApiError { Message = "error" }]
            };
            var errDto = new ApiResponseDto<decimal> { Success = false, Errors = [], Meta = new ApiMetaDto() };
            _http.GetAsync<decimal?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<decimal>>(apiResponse).Returns(errDto);

            // Act
            var result = await _client.GetTotalAmountAsync(null);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_SuccessResponse_ReturnsTrue()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var mapped = new ApiResponseDto<bool> { Success = true, Data = true };
            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mapped);

            // Act
            var result = await _client.DeleteAsync(1);

            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public async Task DeleteAsync_FailureResponse_ReturnsFailure()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool?> { Success = false, Errors = [new ApiError { Message = "Not found" }] };
            var errDto = new ApiResponseDto<bool> { Success = false, Errors = [], Meta = new ApiMetaDto() };
            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(errDto);

            // Act
            var result = await _client.DeleteAsync(99);

            // Assert
            Assert.False(result.Success);
        }

        #endregion
    }
}
