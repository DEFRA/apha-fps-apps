using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using Apha.FPSApps.Infrastructure.Integrations.PACTApis.Clients;
using AutoMapper;
using NSubstitute;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PACT.PactRecreateSummariesLogApiClientTest
{
    public class PactRecreateSummaryLogApiClientTests
    {
        private readonly IPactHttpExecutor _mockHttp;
        private readonly IMapper _mockMapper;
        private readonly PactRecreateSummaryApiClient _client;

        private const string TestUserId = "TestUser1";
        private const string TestUserName = "Test User";
        private const short TestPeriod = 1;
        private const int TestPageNumber = 1;
        private const int TestPageSize = 20;
        private const int TestTotalRecords = 50;

        public PactRecreateSummaryLogApiClientTests()
        {
            _mockHttp = Substitute.For<IPactHttpExecutor>();
            _mockMapper = Substitute.For<IMapper>();
            _client = new PactRecreateSummaryApiClient(_mockHttp, _mockMapper);
        }

        #region GetAllRecreateSummariesLogsAsync

        [Fact]
        public async Task GetAllRecreateSummariesLogsAsync_WithSuccessfulResponse_ReturnsPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = TestPageNumber,
                PageSize = TestPageSize,
                Filter = "{}"
            };

            var apiResponse = new ApiResponse<List<RecreateSummaryLogRes>>
            {
                Success = true,
                Data = new List<RecreateSummaryLogRes>
                {
                    new() { Id = 1, UserId = TestUserId, Comments = TestUserName, Period = TestPeriod, DateDone = DateTime.UtcNow }
                },
                Pagination = new Pagination
                {
                    TotalRecords = TestTotalRecords,
                    PageNumber = TestPageNumber,
                    PageSize = TestPageSize,
                    TotalPages = 3
                }
            };

            var mappedDtos = new List<RecreateSummaryLogDto>
            {
                new() { Id = 1, UserId = TestUserId, Comments = TestUserName, Period = TestPeriod, DateDone = DateTime.UtcNow }
            };

            _mockHttp.GetAsync<List<RecreateSummaryLogRes>>(Arg.Any<string>()).Returns(Task.FromResult(apiResponse));
            _mockMapper.Map<ApiResponseDto<List<RecreateSummaryLogDto>>>(Arg.Any<ApiResponse<List<RecreateSummaryLogRes>>>())
                .Returns(new ApiResponseDto<List<RecreateSummaryLogDto>> { Success = true, Data = mappedDtos });

            // Act
            var result = await _client.GetRecreateSummaryLogAsync(query);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data.data);
            Assert.Equal(TestTotalRecords, result.Data.TotalCount);
            Assert.Equal(TestPageNumber, result.Data.PageNumber);
            Assert.Equal(TestPageSize, result.Data.PageSize);
            await _mockHttp.Received(1).GetAsync<List<RecreateSummaryLogRes>>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetAllRecreateSummariesLogsAsync_WithFailedResponse_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = TestPageNumber,
                PageSize = TestPageSize,
                Filter = "{}"
            };

            var apiResponse = new ApiResponse<List<RecreateSummaryLogRes>>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "API Error", Code = "ERR001" } }
            };

            _mockHttp.GetAsync<List<RecreateSummaryLogRes>>(Arg.Any<string>()).Returns(Task.FromResult(apiResponse));
            _mockMapper.Map<ApiResponseDto<List<RecreateSummaryLogDto>>>(Arg.Any<ApiResponse<List<RecreateSummaryLogRes>>>())
                .Returns(new ApiResponseDto<List<RecreateSummaryLogDto>> 
                { 
                    Success = false, 
                    Errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "ERR001" } }
                });

            // Act
            var result = await _client.GetRecreateSummaryLogAsync(query);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("API Error", result.Errors.First().Message);
        }

        [Fact]
        public async Task GetAllRecreateSummariesLogsAsync_WithNullData_ReturnsEmptyPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = TestPageNumber,
                PageSize = TestPageSize,
                Filter = "{}"
            };

            var apiResponse = new ApiResponse<List<RecreateSummaryLogRes>>
            {
                Success = true,
                Data = null,
                Pagination = new Pagination
                {
                    TotalRecords = 0,
                    PageNumber = TestPageNumber,
                    PageSize = TestPageSize,
                    TotalPages = 0
                }
            };

            _mockHttp.GetAsync<List<RecreateSummaryLogRes>>(Arg.Any<string>()).Returns(Task.FromResult(apiResponse));
            _mockMapper.Map<ApiResponseDto<List<RecreateSummaryLogDto>>>(Arg.Any<ApiResponse<List<RecreateSummaryLogRes>>>())
                .Returns(new ApiResponseDto<List<RecreateSummaryLogDto>> { Success = true, Data = null });

            // Act
            var result = await _client.GetRecreateSummaryLogAsync(query);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data.data);
            Assert.Equal(0, result.Data.TotalCount);
        }

        [Fact]
        public async Task GetRecreateSummaryLogAsync_WithNullPagination_UsesFallbackValues()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = TestPageNumber,
                PageSize = TestPageSize,
                Filter = "{}"
            };

            var apiResponse = new ApiResponse<List<RecreateSummaryLogRes>>
            {
                Success = true,
                Data = new List<RecreateSummaryLogRes>(),
                Pagination = null
            };

            _mockHttp.GetAsync<List<RecreateSummaryLogRes>>(Arg.Any<string>()).Returns(Task.FromResult(apiResponse));
            _mockMapper.Map<ApiResponseDto<List<RecreateSummaryLogDto>>>(Arg.Any<ApiResponse<List<RecreateSummaryLogRes>>>())
                .Returns(new ApiResponseDto<List<RecreateSummaryLogDto>> { Success = true, Data = new List<RecreateSummaryLogDto>() });

            // Act
            var result = await _client.GetRecreateSummaryLogAsync(query);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(0, result.Data!.TotalCount);
            Assert.Equal(TestPageNumber, result.Data.PageNumber);
            Assert.Equal(TestPageSize, result.Data.PageSize);
        }

        #endregion
    }
}
