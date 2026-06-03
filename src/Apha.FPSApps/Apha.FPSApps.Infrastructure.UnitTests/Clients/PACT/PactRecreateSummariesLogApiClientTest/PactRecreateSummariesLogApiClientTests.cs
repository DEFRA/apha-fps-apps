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
    public class PactRecreateSummariesLogApiClientTests
    {
        private readonly IPactHttpExecutor _mockHttp;
        private readonly IMapper _mockMapper;
        private readonly PactRecreateAndReleaseSummaryLogApiClient _client;

        private const string TestUserId = "TestUser1";
        private const string TestUserName = "Test User";
        private const short TestPeriod = 1;
        private const int TestPageNumber = 1;
        private const int TestPageSize = 20;
        private const int TestTotalRecords = 50;

        public PactRecreateSummariesLogApiClientTests()
        {
            _mockHttp = Substitute.For<IPactHttpExecutor>();
            _mockMapper = Substitute.For<IMapper>();
            _client = new PactRecreateAndReleaseSummaryLogApiClient(_mockHttp, _mockMapper);
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

            var apiResponse = new ApiResponse<List<RecreateSummariesLogRes>>
            {
                Success = true,
                Data = new List<RecreateSummariesLogRes>
                {
                    new() { Id = 1, UserId = TestUserId, UserName = TestUserName, Period = TestPeriod, DateDone = DateTime.UtcNow }
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
                new() { Id = 1, UserId = TestUserId, UserName = TestUserName, Period = TestPeriod, DateDone = DateTime.UtcNow }
            };

            _mockHttp.GetAsync<List<RecreateSummariesLogRes>>(Arg.Any<string>()).Returns(Task.FromResult(apiResponse));
            _mockMapper.Map<ApiResponseDto<List<RecreateSummaryLogDto>>>(Arg.Any<ApiResponse<List<RecreateSummariesLogRes>>>())
                .Returns(new ApiResponseDto<List<RecreateSummaryLogDto>> { Success = true, Data = mappedDtos });

            // Act
            var result = await _client.GetAllRecreateSummariesLogsAsync(query);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data.data);
            Assert.Equal(TestTotalRecords, result.Data.TotalCount);
            Assert.Equal(TestPageNumber, result.Data.PageNumber);
            Assert.Equal(TestPageSize, result.Data.PageSize);
            await _mockHttp.Received(1).GetAsync<List<RecreateSummariesLogRes>>(Arg.Any<string>());
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

            var apiResponse = new ApiResponse<List<RecreateSummariesLogRes>>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "API Error", Code = "ERR001" } }
            };

            _mockHttp.GetAsync<List<RecreateSummariesLogRes>>(Arg.Any<string>()).Returns(Task.FromResult(apiResponse));
            _mockMapper.Map<ApiResponseDto<List<RecreateSummaryLogDto>>>(Arg.Any<ApiResponse<List<RecreateSummariesLogRes>>>())
                .Returns(new ApiResponseDto<List<RecreateSummaryLogDto>> 
                { 
                    Success = false, 
                    Errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "ERR001" } }
                });

            // Act
            var result = await _client.GetAllRecreateSummariesLogsAsync(query);

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

            var apiResponse = new ApiResponse<List<RecreateSummariesLogRes>>
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

            _mockHttp.GetAsync<List<RecreateSummariesLogRes>>(Arg.Any<string>()).Returns(Task.FromResult(apiResponse));
            _mockMapper.Map<ApiResponseDto<List<RecreateSummaryLogDto>>>(Arg.Any<ApiResponse<List<RecreateSummariesLogRes>>>())
                .Returns(new ApiResponseDto<List<RecreateSummaryLogDto>> { Success = true, Data = null });

            // Act
            var result = await _client.GetAllRecreateSummariesLogsAsync(query);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data.data);
            Assert.Equal(0, result.Data.TotalCount);
        }

        [Fact]
        public async Task GetAllRecreateSummariesLogsAsync_WithNullPagination_UsesFallbackValues()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = TestPageNumber,
                PageSize = TestPageSize,
                Filter = "{}"
            };

            var apiResponse = new ApiResponse<List<RecreateSummariesLogRes>>
            {
                Success = true,
                Data = new List<RecreateSummariesLogRes>(),
                Pagination = null
            };

            _mockHttp.GetAsync<List<RecreateSummariesLogRes>>(Arg.Any<string>()).Returns(Task.FromResult(apiResponse));
            _mockMapper.Map<ApiResponseDto<List<RecreateSummaryLogDto>>>(Arg.Any<ApiResponse<List<RecreateSummariesLogRes>>>())
                .Returns(new ApiResponseDto<List<RecreateSummaryLogDto>> { Success = true, Data = new List<RecreateSummaryLogDto>() });

            // Act
            var result = await _client.GetAllRecreateSummariesLogsAsync(query);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(0, result.Data!.TotalCount);
            Assert.Equal(TestPageNumber, result.Data.PageNumber);
            Assert.Equal(TestPageSize, result.Data.PageSize);
        }

        #endregion
    }
}
