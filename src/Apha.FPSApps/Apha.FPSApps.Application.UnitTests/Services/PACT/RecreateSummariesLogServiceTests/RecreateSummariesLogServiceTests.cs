using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.PACT;
using NSubstitute;

namespace Apha.FPSApps.Application.UnitTests.Services.PACT.RecreateSummariesLogServiceTests
{
    public class RecreateSummariesLogServiceTests
    {
        private readonly IPactApiClient _mockPactClient;
        private readonly IPactRecreateSummariesLogApiClient _mockLogApiClient;
        private readonly RecreateSummariesLogService _service;

        private const string TestUserId = "TestUser1";
        private const string TestUserName = "Test User";
        private const short TestPeriod = 1;
        private const int TestPageNumber = 1;
        private const int TestPageSize = 20;
        private const int TestTotalRecords = 50;

        public RecreateSummariesLogServiceTests()
        {
            _mockPactClient = Substitute.For<IPactApiClient>();
            _mockLogApiClient = Substitute.For<IPactRecreateSummariesLogApiClient>();
            _mockPactClient.PactRecreateSummariesLog.Returns(_mockLogApiClient);
            _service = new RecreateSummariesLogService(_mockPactClient);
        }

        #region GetAllRecreateSummariesLogsAsync

        [Fact]
        public async Task GetAllRecreateSummariesLogsAsync_WithValidQuery_ReturnsSuccessResponse()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = TestPageNumber,
                PageSize = TestPageSize,
                Filter = "{}"
            };

            var expectedResponse = new ApiResponseDto<PaginatedResult<RecreateSummariesLogDto>>
            {
                Success = true,
                Data = new PaginatedResult<RecreateSummariesLogDto>(
                    new List<RecreateSummariesLogDto>
                    {
                        new() { Id = 1, UserId = TestUserId, UserName = TestUserName, Period = TestPeriod, DateDone = DateTime.UtcNow }
                    },
                    TestTotalRecords,
                    TestPageNumber,
                    TestPageSize)
            };

            _mockLogApiClient.GetAllRecreateSummariesLogsAsync(query).Returns(expectedResponse);

            // Act
            var result = await _service.GetAllRecreateSummariesLogsAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data.data);
            Assert.Equal(TestTotalRecords, result.Data.TotalCount);
            await _mockLogApiClient.Received(1).GetAllRecreateSummariesLogsAsync(query);
        }

        [Fact]
        public async Task GetAllRecreateSummariesLogsAsync_WithFailedApiResponse_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = TestPageNumber,
                PageSize = TestPageSize,
                Filter = "{}"
            };

            var expectedResponse = new ApiResponseDto<PaginatedResult<RecreateSummariesLogDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "ERR001" } }
            };

            _mockLogApiClient.GetAllRecreateSummariesLogsAsync(query).Returns(expectedResponse);

            // Act
            var result = await _service.GetAllRecreateSummariesLogsAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task GetAllRecreateSummariesLogsAsync_WithEmptyResult_ReturnsEmptyPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = TestPageNumber,
                PageSize = TestPageSize,
                Filter = "{}"
            };

            var expectedResponse = new ApiResponseDto<PaginatedResult<RecreateSummariesLogDto>>
            {
                Success = true,
                Data = new PaginatedResult<RecreateSummariesLogDto>(
                    new List<RecreateSummariesLogDto>(),
                    0,
                    TestPageNumber,
                    TestPageSize)
            };

            _mockLogApiClient.GetAllRecreateSummariesLogsAsync(query).Returns(expectedResponse);

            // Act
            var result = await _service.GetAllRecreateSummariesLogsAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data.data);
            Assert.Equal(0, result.Data.TotalCount);
        }

        [Fact]
        public async Task GetAllRecreateSummariesLogsAsync_ApiClientThrowsException_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = TestPageNumber,
                PageSize = TestPageSize,
                Filter = "{}"
            };

            _mockLogApiClient.GetAllRecreateSummariesLogsAsync(query)
                .Returns(Task.FromException<ApiResponseDto<PaginatedResult<RecreateSummariesLogDto>>>(
                    new InvalidOperationException("API Client error")));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetAllRecreateSummariesLogsAsync(query));
        }

        #endregion
    }
}
