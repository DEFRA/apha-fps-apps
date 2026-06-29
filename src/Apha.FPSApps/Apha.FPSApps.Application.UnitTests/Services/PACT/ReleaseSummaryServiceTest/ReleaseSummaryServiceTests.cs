using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Services.PACT;
using NSubstitute;

namespace Apha.FPSApps.Application.UnitTests.Services.PACT.ReleaseSummaryServiceTest
{
    public class ReleaseSummaryServiceTests
    {
        private readonly IPactApiClient _mockPactClient;
        private readonly IPactReleaseSummaryApiClient _mockReleaseSummaryApiClient;
        private readonly ReleaseSummaryService _service;

        private const string TestPeriodName = "TestPeriod";
        private const short TestFinalSummariesRun = 1;

        public ReleaseSummaryServiceTests()
        {
            _mockPactClient = Substitute.For<IPactApiClient>();
            _mockReleaseSummaryApiClient = Substitute.For<IPactReleaseSummaryApiClient>();
            _mockPactClient.PactReleaseSummary.Returns(_mockReleaseSummaryApiClient);
            _service = new ReleaseSummaryService(_mockPactClient);
        }

        #region GetReleaseSummariesAsync

        [Fact]
        public async Task GetReleaseSummariesAsync_WithExistingPeriods_ReturnsSuccessResponseWithData()
        {
            // Arrange
            var periods = new List<ReleasePeriodDto>
            {
                new() { PeriodName = "Period1", PeriodType = "Month", StartPeriod = 0.5, EndPeriod = 1.0, FinalSummariesRun = 0, PeriodLocked = 0 },
                new() { PeriodName = "Period2", PeriodType = "Month", StartPeriod = 1.5, EndPeriod = 2.0, FinalSummariesRun = 1, PeriodLocked = 0 }
            };

            var summaryDto = new ReleaseSummaryDto
            {
                ReleasePeriods = periods.AsReadOnly()
            };
            var expectedResponse = ApiResponseDto<ReleaseSummaryDto>.SuccessResponse(summaryDto);

            _mockReleaseSummaryApiClient.GetReleaseSummariesAsync().Returns(expectedResponse);

            // Act
            var result = await _service.GetReleaseSummariesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.ReleasePeriods.Count);
            Assert.Equal("Period1", result.Data.ReleasePeriods[0].PeriodName);
            Assert.Equal("Period2", result.Data.ReleasePeriods[1].PeriodName);

            await _mockReleaseSummaryApiClient.Received(1).GetReleaseSummariesAsync();
        }

        [Fact]
        public async Task GetReleaseSummariesAsync_WithNoPeriods_ReturnsSuccessResponseWithEmptyList()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<ReleaseSummaryDto>.SuccessResponse(
                new ReleaseSummaryDto { ReleasePeriods = new List<ReleasePeriodDto>().AsReadOnly() });

            _mockReleaseSummaryApiClient.GetReleaseSummariesAsync().Returns(expectedResponse);

            // Act
            var result = await _service.GetReleaseSummariesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data.ReleasePeriods);

            await _mockReleaseSummaryApiClient.Received(1).GetReleaseSummariesAsync();
        }

        [Fact]
        public async Task GetReleaseSummariesAsync_WithFailedApiResponse_ReturnsFailureResponseWithErrors()
        {
            // Arrange
            var expectedResponse = new ApiResponseDto<ReleaseSummaryDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto>
                {
                    new() { Code = "ERR001", Message = "API Error" }
                }
            };

            _mockReleaseSummaryApiClient.GetReleaseSummariesAsync().Returns(expectedResponse);

            // Act
            var result = await _service.GetReleaseSummariesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("ERR001", result.Errors[0].Code);
            Assert.Equal("API Error", result.Errors[0].Message);

            await _mockReleaseSummaryApiClient.Received(1).GetReleaseSummariesAsync();
        }

        [Fact]
        public async Task GetReleaseSummariesAsync_MapsAllPeriodFieldsCorrectly()
        {
            // Arrange
            var period = new ReleasePeriodDto
            {
                PeriodName  = "P1",
                PeriodType  = "Quarter",
                StartPeriod = 1.0,
                EndPeriod   = 3.0,
                FinalSummariesRun = 2,
                PeriodLocked = 1
            };

            var expectedResponse = ApiResponseDto<ReleaseSummaryDto>.SuccessResponse(
                new ReleaseSummaryDto { ReleasePeriods = new List<ReleasePeriodDto> { period }.AsReadOnly() });

            _mockReleaseSummaryApiClient.GetReleaseSummariesAsync().Returns(expectedResponse);

            // Act
            var result = await _service.GetReleaseSummariesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!.ReleasePeriods);
            var dto = result.Data!.ReleasePeriods[0];
            Assert.Equal("P1",      dto.PeriodName);
            Assert.Equal("Quarter", dto.PeriodType);
            Assert.Equal(1.0,       dto.StartPeriod);
            Assert.Equal(3.0,       dto.EndPeriod);
            Assert.Equal((short)2,  dto.FinalSummariesRun);
            Assert.Equal((short)1,  dto.PeriodLocked);
        }

        [Fact]
        public async Task GetReleaseSummariesAsync_DelegatesDirectlyToPactReleaseSummaryApiClient()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<ReleaseSummaryDto>.SuccessResponse(
                new ReleaseSummaryDto { ReleasePeriods = new List<ReleasePeriodDto>().AsReadOnly() });

            _mockReleaseSummaryApiClient.GetReleaseSummariesAsync().Returns(expectedResponse);

            // Act
            await _service.GetReleaseSummariesAsync();

            // Assert — the root pact client must never be called directly; only the sub-client
            await _mockReleaseSummaryApiClient.Received(1).GetReleaseSummariesAsync();
            _ = _mockPactClient.Received(1).PactReleaseSummary;
        }

        [Fact]
        public async Task GetReleaseSummariesAsync_ApiClientThrowsException_PropagatesException()
        {
            // Arrange
            _mockReleaseSummaryApiClient.GetReleaseSummariesAsync()
                .Returns(Task.FromException<ApiResponseDto<ReleaseSummaryDto>>(
                    new InvalidOperationException("API Client error")));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetReleaseSummariesAsync());

            await _mockReleaseSummaryApiClient.Received(1).GetReleaseSummariesAsync();
        }

        #endregion

        #region GetReleasePeriodsAsync

        [Fact]
        public async Task GetReleasePeriodsAsync_WithExistingPeriods_ReturnsSuccessResponseWithData()
        {
            // Arrange
            var periods = new List<ReleasePeriodDto>
            {
                new() { PeriodName = "Period1", PeriodType = "Month", StartPeriod = 0.5, EndPeriod = 1.0, FinalSummariesRun = 0, PeriodLocked = 0 },
                new() { PeriodName = "Period2", PeriodType = "Month", StartPeriod = 1.5, EndPeriod = 2.0, FinalSummariesRun = 1, PeriodLocked = 0 }
            };

            var expectedResponse = ApiResponseDto<IReadOnlyList<ReleasePeriodDto>>.SuccessResponse(periods.AsReadOnly());

            _mockReleaseSummaryApiClient.GetReleasePeriodsAsync().Returns(expectedResponse);

            // Act
            var result = await _service.GetReleasePeriodsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            Assert.Equal("Period1", result.Data[0].PeriodName);
            Assert.Equal("Period2", result.Data[1].PeriodName);

            await _mockReleaseSummaryApiClient.Received(1).GetReleasePeriodsAsync();
        }

        [Fact]
        public async Task GetReleasePeriodsAsync_WithNoPeriods_ReturnsSuccessResponseWithEmptyList()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<IReadOnlyList<ReleasePeriodDto>>.SuccessResponse(
                new List<ReleasePeriodDto>().AsReadOnly());

            _mockReleaseSummaryApiClient.GetReleasePeriodsAsync().Returns(expectedResponse);

            // Act
            var result = await _service.GetReleasePeriodsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);

            await _mockReleaseSummaryApiClient.Received(1).GetReleasePeriodsAsync();
        }

        [Fact]
        public async Task GetReleasePeriodsAsync_WithFailedApiResponse_ReturnsFailureResponseWithErrors()
        {
            // Arrange
            var expectedResponse = new ApiResponseDto<IReadOnlyList<ReleasePeriodDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto>
                {
                    new() { Code = "ERR003", Message = "Failed to retrieve periods" }
                }
            };

            _mockReleaseSummaryApiClient.GetReleasePeriodsAsync().Returns(expectedResponse);

            // Act
            var result = await _service.GetReleasePeriodsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("ERR003", result.Errors[0].Code);
            Assert.Equal("Failed to retrieve periods", result.Errors[0].Message);

            await _mockReleaseSummaryApiClient.Received(1).GetReleasePeriodsAsync();
        }

        [Fact]
        public async Task GetReleasePeriodsAsync_MapsAllPeriodFieldsCorrectly()
        {
            // Arrange
            var period = new ReleasePeriodDto
            {
                PeriodName  = "P1",
                PeriodType  = "Quarter",
                StartPeriod = 1.0,
                EndPeriod   = 3.0,
                FinalSummariesRun = 2,
                PeriodLocked = 1
            };

            var expectedResponse = ApiResponseDto<IReadOnlyList<ReleasePeriodDto>>.SuccessResponse(
                new List<ReleasePeriodDto> { period }.AsReadOnly());

            _mockReleaseSummaryApiClient.GetReleasePeriodsAsync().Returns(expectedResponse);

            // Act
            var result = await _service.GetReleasePeriodsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            var dto = result.Data![0];
            Assert.Equal("P1",      dto.PeriodName);
            Assert.Equal("Quarter", dto.PeriodType);
            Assert.Equal(1.0,       dto.StartPeriod);
            Assert.Equal(3.0,       dto.EndPeriod);
            Assert.Equal((short)2,  dto.FinalSummariesRun);
            Assert.Equal((short)1,  dto.PeriodLocked);
        }

        [Fact]
        public async Task GetReleasePeriodsAsync_DelegatesDirectlyToPactReleaseSummaryApiClient()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<IReadOnlyList<ReleasePeriodDto>>.SuccessResponse(
                new List<ReleasePeriodDto>().AsReadOnly());

            _mockReleaseSummaryApiClient.GetReleasePeriodsAsync().Returns(expectedResponse);

            // Act
            await _service.GetReleasePeriodsAsync();

            // Assert — the root pact client must never be called directly; only the sub-client
            await _mockReleaseSummaryApiClient.Received(1).GetReleasePeriodsAsync();
            _ = _mockPactClient.Received(1).PactReleaseSummary;
        }

        [Fact]
        public async Task GetReleasePeriodsAsync_ApiClientThrowsException_PropagatesException()
        {
            // Arrange
            _mockReleaseSummaryApiClient.GetReleasePeriodsAsync()
                .Returns(Task.FromException<ApiResponseDto<IReadOnlyList<ReleasePeriodDto>>>(
                    new InvalidOperationException("API Client error")));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetReleasePeriodsAsync());

            await _mockReleaseSummaryApiClient.Received(1).GetReleasePeriodsAsync();
        }

        #endregion

        #region SetFinalSummaryRunAsync

        [Fact]
        public async Task SetFinalSummaryRunAsync_WithExistingPeriod_ReturnsSuccessResponseWithUpdatedDto()
        {
            // Arrange
            var updatedDto = new ReleasePeriodDto
            {
                PeriodName        = TestPeriodName,
                FinalSummariesRun = TestFinalSummariesRun,
                EndPeriod         = 1.0
            };

            var expectedResponse = ApiResponseDto<ReleasePeriodDto>.SuccessResponse(updatedDto);

            _mockReleaseSummaryApiClient
                .SetFinalSummaryRunAsync(TestPeriodName, TestFinalSummariesRun, Arg.Any<string>())
                .Returns(expectedResponse);

            // Act
            var result = await _service.SetFinalSummaryRunAsync(TestPeriodName, TestFinalSummariesRun, "1");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(TestPeriodName,        result.Data.PeriodName);
            Assert.Equal(TestFinalSummariesRun, result.Data.FinalSummariesRun);

            await _mockReleaseSummaryApiClient.Received(1)
                .SetFinalSummaryRunAsync(TestPeriodName, TestFinalSummariesRun, Arg.Any<string>());
        }

        [Fact]
        public async Task SetFinalSummaryRunAsync_WithFailedApiResponse_ReturnsFailureResponseWithErrors()
        {
            // Arrange
            var expectedResponse = new ApiResponseDto<ReleasePeriodDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto>
                {
                    new() { Code = "ERR002", Message = "Period not found" }
                }
            };

            _mockReleaseSummaryApiClient
                .SetFinalSummaryRunAsync(TestPeriodName, TestFinalSummariesRun, Arg.Any<string>())
                .Returns(expectedResponse);

            // Act
            var result = await _service.SetFinalSummaryRunAsync(TestPeriodName, TestFinalSummariesRun, "0");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("ERR002",           result.Errors[0].Code);
            Assert.Equal("Period not found", result.Errors[0].Message);

            await _mockReleaseSummaryApiClient.Received(1)
                .SetFinalSummaryRunAsync(TestPeriodName, TestFinalSummariesRun, Arg.Any<string>());
        }

        [Fact]
        public async Task SetFinalSummaryRunAsync_PassesCorrectArgumentsToApiClient()
        {
            // Arrange
            const string periodName        = "ArgCheckPeriod";
            const short  finalSummariesRun = 3;

            var updatedDto = new ReleasePeriodDto { PeriodName = periodName, FinalSummariesRun = finalSummariesRun };
            var expectedResponse = ApiResponseDto<ReleasePeriodDto>.SuccessResponse(updatedDto);

            _mockReleaseSummaryApiClient
                .SetFinalSummaryRunAsync(periodName, finalSummariesRun, Arg.Any<string>())
                .Returns(expectedResponse);

            // Act
            await _service.SetFinalSummaryRunAsync(periodName, finalSummariesRun, "1");

            // Assert
            await _mockReleaseSummaryApiClient.Received(1).SetFinalSummaryRunAsync(
                Arg.Is<string>(p => p == periodName),
                Arg.Is<short>(f  => f == finalSummariesRun),
                Arg.Any<string>()
            );
        }

        [Fact]
        public async Task SetFinalSummaryRunAsync_MapsAllFieldsFromApiClientResponse()
        {
            // Arrange
            var updatedDto = new ReleasePeriodDto
            {
                PeriodName        = TestPeriodName,
                PeriodType        = "Month",
                StartPeriod       = 1.5,
                EndPeriod         = 2.5,
                FinalSummariesRun = TestFinalSummariesRun,
                PeriodLocked      = 0
            };

            var expectedResponse = ApiResponseDto<ReleasePeriodDto>.SuccessResponse(updatedDto);

            _mockReleaseSummaryApiClient
                .SetFinalSummaryRunAsync(TestPeriodName, TestFinalSummariesRun, Arg.Any<string>())
                .Returns(expectedResponse);

            // Act
            var result = await _service.SetFinalSummaryRunAsync(TestPeriodName, TestFinalSummariesRun, "1");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(TestPeriodName,        result.Data.PeriodName);
            Assert.Equal("Month",               result.Data.PeriodType);
            Assert.Equal(1.5,                   result.Data.StartPeriod);
            Assert.Equal(2.5,                   result.Data.EndPeriod);
            Assert.Equal(TestFinalSummariesRun, result.Data.FinalSummariesRun);
            Assert.Equal((short)0,              result.Data.PeriodLocked);
        }

        [Fact]
        public async Task SetFinalSummaryRunAsync_DelegatesDirectlyToPactReleaseSummaryApiClient()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<ReleasePeriodDto>.SuccessResponse(
                new ReleasePeriodDto { PeriodName = TestPeriodName, FinalSummariesRun = TestFinalSummariesRun });

            _mockReleaseSummaryApiClient
                .SetFinalSummaryRunAsync(TestPeriodName, TestFinalSummariesRun, Arg.Any<string>())
                .Returns(expectedResponse);

            // Act
            await _service.SetFinalSummaryRunAsync(TestPeriodName, TestFinalSummariesRun, "1");

            // Assert — the root pact client must never be called directly; only the sub-client
            await _mockReleaseSummaryApiClient.Received(1)
                .SetFinalSummaryRunAsync(TestPeriodName, TestFinalSummariesRun, Arg.Any<string>());
            _ = _mockPactClient.Received(1).PactReleaseSummary;
        }

        [Fact]
        public async Task SetFinalSummaryRunAsync_ApiClientThrowsException_PropagatesException()
        {
            // Arrange
            _mockReleaseSummaryApiClient
                .SetFinalSummaryRunAsync(TestPeriodName, TestFinalSummariesRun, Arg.Any<string>())
                .Returns(Task.FromException<ApiResponseDto<ReleasePeriodDto>>(
                    new InvalidOperationException("API Client error")));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.SetFinalSummaryRunAsync(TestPeriodName, TestFinalSummariesRun, "1"));

            await _mockReleaseSummaryApiClient.Received(1)
                .SetFinalSummaryRunAsync(TestPeriodName, TestFinalSummariesRun, Arg.Any<string>());
        }

        #endregion
    }
}
