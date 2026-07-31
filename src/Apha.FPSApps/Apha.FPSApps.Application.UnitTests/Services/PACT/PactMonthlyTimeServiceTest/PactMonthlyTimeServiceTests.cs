using Apha.Common.Utilities.ExcelImport;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.PACT;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.PACT.PactMonthlyTimeServiceTest
{
    public class PactMonthlyTimeServiceTests
    {
        private readonly IPactApiClient _pactClient;
        private readonly IPactMonthlyTimeApiClient _pactMonthlyTimeApiClient;
        private readonly IExcelImportService _excelImportService;
        private readonly IWorkGroupService _workGroupService;
        private readonly IPactTimeCodeValidService _timeCodeValidService;
        private readonly IMonthService _monthService;
        private readonly PactMonthlyTimeService _service;

        public PactMonthlyTimeServiceTests()
        {
            _pactClient = Substitute.For<IPactApiClient>();
            _pactMonthlyTimeApiClient = Substitute.For<IPactMonthlyTimeApiClient>();
            _excelImportService = Substitute.For<IExcelImportService>();
            _workGroupService = Substitute.For<IWorkGroupService>();
            _timeCodeValidService = Substitute.For<IPactTimeCodeValidService>();
            _monthService = Substitute.For<IMonthService>();

            _pactClient.PactMonthlyTime.Returns(_pactMonthlyTimeApiClient);
            _service = new PactMonthlyTimeService(_pactClient, _excelImportService, _workGroupService, _timeCodeValidService, _monthService);
        }

        #region SearchAsync Tests

        [Fact]
        public async Task SearchAsync_WithValidQueryAndFilter_ReturnsSuccessResponse()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var filter = new MonthlyTimeLogFilterDto { WorkGroup = "WG1", TimeCode = "TC1" };
            var logs = new List<MonthlyTimeLogDto>
            {
                new() { SequenceNo = 1, TimeCode = "TC1", PactStaffId = "S001", WorkGroup = "WG1" },
                new() { SequenceNo = 2, TimeCode = "TC1", PactStaffId = "S002", WorkGroup = "WG1" }
            };
            var expectedResponse = ApiResponseDto<List<MonthlyTimeLogDto>>.SuccessResponse(logs);
            _pactMonthlyTimeApiClient.SearchAsync(query, filter).Returns(expectedResponse);

            // Act
            var result = await _service.SearchAsync(query, filter);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _pactMonthlyTimeApiClient.Received(1).SearchAsync(query, filter);
        }

        [Fact]
        public async Task SearchAsync_WithNoMatchingRecords_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var filter = new MonthlyTimeLogFilterDto { WorkGroup = "WG_NONE" };
            var expectedResponse = ApiResponseDto<List<MonthlyTimeLogDto>>.SuccessResponse(new List<MonthlyTimeLogDto>());
            _pactMonthlyTimeApiClient.SearchAsync(query, filter).Returns(expectedResponse);

            // Act
            var result = await _service.SearchAsync(query, filter);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task SearchAsync_WithAllFilters_PassesFilterToApiClient()
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
            var logs = new List<MonthlyTimeLogDto>
            {
                new() { SequenceNo = 1, TimeCode = "TC1", PactStaffId = "S001", WorkGroup = "WG1", Month = 6.0, UserId = "USER1", InsertDelete = "I" }
            };
            var expectedResponse = ApiResponseDto<List<MonthlyTimeLogDto>>.SuccessResponse(logs);
            _pactMonthlyTimeApiClient.SearchAsync(query, filter).Returns(expectedResponse);

            // Act
            var result = await _service.SearchAsync(query, filter);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _pactMonthlyTimeApiClient.Received(1).SearchAsync(query, filter);
        }

        [Fact]
        public async Task SearchAsync_WithEmptyFilter_DelegatesToApiClient()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var filter = new MonthlyTimeLogFilterDto();
            var expectedResponse = ApiResponseDto<List<MonthlyTimeLogDto>>.SuccessResponse(new List<MonthlyTimeLogDto>());
            _pactMonthlyTimeApiClient.SearchAsync(query, filter).Returns(expectedResponse);

            // Act
            var result = await _service.SearchAsync(query, filter);

            // Assert
            Assert.NotNull(result);
            await _pactMonthlyTimeApiClient.Received(1).SearchAsync(query, filter);
        }

        [Fact]
        public async Task SearchAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var filter = new MonthlyTimeLogFilterDto { WorkGroup = "WG1" };
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<MonthlyTimeLogDto>>.FailureResponse(errors, new ApiMetaDto());
            _pactMonthlyTimeApiClient.SearchAsync(query, filter).Returns(expectedResponse);

            // Act
            var result = await _service.SearchAsync(query, filter);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task SearchAsync_ApiClientThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var filter = new MonthlyTimeLogFilterDto();
            _pactMonthlyTimeApiClient
                .SearchAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<MonthlyTimeLogFilterDto>())
                .ThrowsAsync(new Exception("API client error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.SearchAsync(query, filter));
        }

        #endregion
    }
}
