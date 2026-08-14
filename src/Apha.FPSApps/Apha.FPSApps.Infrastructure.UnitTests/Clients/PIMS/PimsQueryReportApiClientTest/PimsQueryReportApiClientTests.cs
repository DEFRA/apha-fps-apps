using Apha.Common.Constants;
using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PIMS.PimsQueryReportApiClientTest
{
    public class PimsQueryReportApiClientTests
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PimsQueryReportApiClient _client;

        public PimsQueryReportApiClientTests()
        {
            _http = Substitute.For<IPimsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PimsQueryReportApiClient(_http, _mapper);
        }

        private static List<ApiError> OneApiError(string message = "API error", string code = "ERR")
            => [new ApiError { Message = message, Code = code }];

        private static List<ApiErrorDto> OneApiErrorDto(string message = "API error", string code = "ERR")
            => [new ApiErrorDto { Message = message, Code = code }];

        [Fact]
        public async Task GetMonitoringReportDataAsync_WithSuccessResponse_ReturnsMappedData()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var resData = new List<MonitoringReportDataRes>
            {
                new() { ParentProject = "PP001", Program = "TB", Contract = "R&D" }
            };
            var apiResponse = new ApiResponse<List<MonitoringReportDataRes>> { Success = true, Data = resData };
            var mappedDto = ApiResponseDto<List<MonitoringReportDataDto>>.SuccessResponse(
                [new MonitoringReportDataDto { ParentProject = "PP001", Program = "TB", Contract = "R&D" }]);

            _http.GetAsync<List<MonitoringReportDataRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<MonitoringReportDataDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetMonitoringReportDataAsync(query, 2025, 8, "R&D", ["PROG1", "PROG2"]);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data!);
            await _http.Received(1).GetAsync<List<MonitoringReportDataRes>>(
                Arg.Is<string>(u =>
                    u.Contains(PimsApiEndpoints.GetMonitoringQueryReportData) &&
                    u.Contains("reportYear=2025") &&
                    u.Contains("reportMonth=8") &&
                    u.Contains("contractFilter=R%26D") &&
                    u.Contains("programFilter=PROG1") &&
                    u.Contains("programFilter=PROG2")));
            _mapper.Received(1).Map<ApiResponseDto<List<MonitoringReportDataDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetMonitoringReportDataAsync_WithFailureResponse_ReturnsFailure()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<MonitoringReportDataRes>>
            {
                Success = false,
                Errors = OneApiError("Not found", "NOT_FOUND")
            };
            var mappedDto = new ApiResponseDto<List<MonitoringReportDataDto>>
            {
                Success = false,
                Errors = OneApiErrorDto("Not found", "NOT_FOUND"),
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<MonitoringReportDataRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<MonitoringReportDataDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetMonitoringReportDataAsync(query, 2025, 8);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors!);
            Assert.Equal("NOT_FOUND", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetMonitoringReportDataAsync_WhenHttpThrows_ReturnsInternalError()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _http.GetAsync<List<MonitoringReportDataRes>>(Arg.Any<string>())
                .ThrowsAsync(new Exception("network"));

            // Act
            var result = await _client.GetMonitoringReportDataAsync(query, 2025, 8);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
            Assert.Equal("Failed to retrieve monitoring query report data", result.Errors[0].Message);
        }

        [Fact]
        public async Task GetMonitoringReportDataAsync_WhenMapperThrows_ReturnsInternalError()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<MonitoringReportDataRes>> { Success = true, Data = [] };

            _http.GetAsync<List<MonitoringReportDataRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<MonitoringReportDataDto>>>(apiResponse)
                .Throws(new AutoMapperMappingException("map failed"));

            // Act
            var result = await _client.GetMonitoringReportDataAsync(query, 2025, 8);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("INTERNAL_ERROR", result.Errors![0].Code);
            Assert.Equal("Failed to retrieve monitoring query report data", result.Errors[0].Message);
        }

        [Fact]
        public async Task GetProgramCustomerMonitoringReportDataAsync_WithSuccessResponse_ReturnsMappedData_AndSkipsContractFilter()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 2, PageSize = 20 };
            var resData = new List<ProgramCustomerMonitoringReportDataRes>
            {
                new() { ParentProject = "PP100", Program = "AMR", PlannedCosts = 10m }
            };
            var apiResponse = new ApiResponse<List<ProgramCustomerMonitoringReportDataRes>> { Success = true, Data = resData };
            var mappedDto = ApiResponseDto<List<ProgramCustomerMonitoringReportDataDto>>.SuccessResponse(
                [new ProgramCustomerMonitoringReportDataDto { ParentProject = "PP100", Program = "AMR", PlannedCosts = 10m }]);

            _http.GetAsync<List<ProgramCustomerMonitoringReportDataRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProgramCustomerMonitoringReportDataDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetProgramCustomerMonitoringReportDataAsync(query, 2026, 2, ["AMR"]);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data!);
            await _http.Received(1).GetAsync<List<ProgramCustomerMonitoringReportDataRes>>(
                Arg.Is<string>(u =>
                    u.Contains(PimsApiEndpoints.GetProgramCustomerMonitoringQueryReportData) &&
                    u.Contains("reportYear=2026") &&
                    u.Contains("reportMonth=2") &&
                    u.Contains("programFilter=AMR") &&
                    !u.Contains("contractFilter=")));
        }

        [Fact]
        public async Task GetProgramCustomerMonitoringReportDataAsync_WithFailureResponse_ReturnsFailure()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<ProgramCustomerMonitoringReportDataRes>>
            {
                Success = false,
                Errors = OneApiError("Bad request", "BAD_REQUEST")
            };
            var mappedDto = new ApiResponseDto<List<ProgramCustomerMonitoringReportDataDto>>
            {
                Success = false,
                Errors = OneApiErrorDto("Bad request", "BAD_REQUEST"),
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ProgramCustomerMonitoringReportDataRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProgramCustomerMonitoringReportDataDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetProgramCustomerMonitoringReportDataAsync(query, 2026, 2);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors!);
            Assert.Equal("BAD_REQUEST", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetProgramCustomerMonitoringReportDataAsync_WhenHttpThrows_ReturnsInternalError()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _http.GetAsync<List<ProgramCustomerMonitoringReportDataRes>>(Arg.Any<string>())
                .ThrowsAsync(new Exception("network"));

            // Act
            var result = await _client.GetProgramCustomerMonitoringReportDataAsync(query, 2026, 2);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
            Assert.Equal("Failed to retrieve program and customer monitoring report data", result.Errors[0].Message);
        }

        [Fact]
        public async Task GetProgramCustomerMonitoringReportDataAsync_WhenMapperThrows_ReturnsInternalError()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<ProgramCustomerMonitoringReportDataRes>> { Success = true, Data = [] };

            _http.GetAsync<List<ProgramCustomerMonitoringReportDataRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProgramCustomerMonitoringReportDataDto>>>(apiResponse)
                .Throws(new AutoMapperMappingException("map failed"));

            // Act
            var result = await _client.GetProgramCustomerMonitoringReportDataAsync(query, 2026, 2);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("INTERNAL_ERROR", result.Errors![0].Code);
            Assert.Equal("Failed to retrieve program and customer monitoring report data", result.Errors[0].Message);
        }
    }
}
