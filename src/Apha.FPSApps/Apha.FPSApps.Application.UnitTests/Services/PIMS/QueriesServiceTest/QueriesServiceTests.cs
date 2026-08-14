using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.PIMS;
using NSubstitute;

namespace Apha.FPSApps.Application.UnitTests.Services.PIMS.QueriesServiceTest
{
    public class QueriesServiceTests
    {
        private readonly IPimsApiClient _pimsApiClient;
        private readonly IPimsQueryReportApiClient _pimsQueryReportApiClient;
        private readonly QueriesService _sut;

        public QueriesServiceTests()
        {
            _pimsApiClient = Substitute.For<IPimsApiClient>();
            _pimsQueryReportApiClient = Substitute.For<IPimsQueryReportApiClient>();
            _pimsApiClient.PimsQueryReport.Returns(_pimsQueryReportApiClient);
            _sut = new QueriesService(_pimsApiClient);
        }

        [Fact]
        public async Task GetMonitoringReportDataAsync_DelegatesToApiClient_ReturnsResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 2, PageSize = 10 };
            const short reportYear = 2025;
            const short reportMonth = 8;
            const string contractFilter = "R&DGen";
            var programFilter = new List<string> { "TB", "FMD" };

            var expectedData = new List<MonitoringReportDataDto>
            {
                new() { ParentProject = "PP001", Program = "TB", Contract = "R&DGen" }
            };

            var expected = ApiResponseDto<List<MonitoringReportDataDto>>.SuccessResponse(expectedData);

            _pimsQueryReportApiClient
                .GetMonitoringReportDataAsync(query, reportYear, reportMonth, contractFilter, programFilter)
                .Returns(expected);

            // Act
            var result = await _sut.GetMonitoringReportDataAsync(query, reportYear, reportMonth, contractFilter, programFilter);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data!);
            await _pimsQueryReportApiClient.Received(1)
                .GetMonitoringReportDataAsync(query, reportYear, reportMonth, contractFilter, programFilter);
        }

        [Fact]
        public async Task GetMonitoringReportDataAsync_WhenApiReturnsFailure_ReturnsFailure()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto>
            {
                new() { Code = "ERR", Message = "Failed" }
            };

            var expected = ApiResponseDto<List<MonitoringReportDataDto>>.FailureResponse(errors, new ApiMetaDto());

            _pimsQueryReportApiClient
                .GetMonitoringReportDataAsync(query, 2025, 1, "*", null)
                .Returns(expected);

            // Act
            var result = await _sut.GetMonitoringReportDataAsync(query, 2025, 1);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors!);
            await _pimsQueryReportApiClient.Received(1)
                .GetMonitoringReportDataAsync(query, 2025, 1, "*", null);
        }

        [Fact]
        public async Task GetProgramCustomerMonitoringReportDataAsync_DelegatesToDedicatedApiClientMethod()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 3, PageSize = 25, SortBy = "Project" };
            const short reportYear = 2024;
            const short reportMonth = 12;
            var programFilter = new List<string> { "AMR" };

            var expectedData = new List<ProgramCustomerMonitoringReportDataDto>
            {
                new() { ParentProject = "PP009", Program = "AMR", PlannedCosts = 1234.56m }
            };

            var expected = ApiResponseDto<List<ProgramCustomerMonitoringReportDataDto>>.SuccessResponse(expectedData);

            _pimsQueryReportApiClient
                .GetProgramCustomerMonitoringReportDataAsync(query, reportYear, reportMonth, programFilter)
                .Returns(expected);

            // Act
            var result = await _sut.GetProgramCustomerMonitoringReportDataAsync(query, reportYear, reportMonth, programFilter);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data!);
            await _pimsQueryReportApiClient.Received(1)
                .GetProgramCustomerMonitoringReportDataAsync(query, reportYear, reportMonth, programFilter);

            await _pimsQueryReportApiClient.DidNotReceiveWithAnyArgs()
                .GetMonitoringReportDataAsync(default!, default, default, default!, default);
        }

        [Fact]
        public async Task GetProgramCustomerMonitoringReportDataAsync_WhenApiReturnsFailure_ReturnsFailure()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto>
            {
                new() { Code = "ERR", Message = "Program monitoring failed" }
            };

            var expected = ApiResponseDto<List<ProgramCustomerMonitoringReportDataDto>>.FailureResponse(errors, new ApiMetaDto());

            _pimsQueryReportApiClient
                .GetProgramCustomerMonitoringReportDataAsync(query, 2026, 2, null)
                .Returns(expected);

            // Act
            var result = await _sut.GetProgramCustomerMonitoringReportDataAsync(query, 2026, 2);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors!);
            await _pimsQueryReportApiClient.Received(1)
                .GetProgramCustomerMonitoringReportDataAsync(query, 2026, 2, null);
        }
    }
}
