using Apha.Common.Constants;
using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using Apha.FPSApps.Infrastructure.Integrations.PACTApis.Clients;
using AutoMapper;
using NSubstitute;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PACT.PactReleaseSummaryApiClientTest
{
    public class PactReleaseSummaryApiClientTests
    {
        private readonly IPactHttpExecutor _mockHttp;
        private readonly IMapper _mockMapper;
        private readonly PactReleaseSummaryApiClient _client;

        private const string TestPeriodName      = "TestPeriod";
        private const short  TestFinalSummariesRun = 1;

        public PactReleaseSummaryApiClientTests()
        {
            _mockHttp   = Substitute.For<IPactHttpExecutor>();
            _mockMapper = Substitute.For<IMapper>();
            _client     = new PactReleaseSummaryApiClient(_mockHttp, _mockMapper);
        }

        #region GetReleaseSummariesAsync

        [Fact]
        public async Task GetReleaseSummariesAsync_WithSuccessfulResponse_ReturnsMappedDtoList()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<ReleasePeriodRes>>
            {
                Success = true,
                Data = new List<ReleasePeriodRes>
                {
                    new() { PeriodName = "Period1", PeriodType = "Month", StartPeriod = 0.5, EndPeriod = 1.0, FinalSummariesRun = 0, PeriodLocked = 0 },
                    new() { PeriodName = "Period2", PeriodType = "Month", StartPeriod = 1.5, EndPeriod = 2.0, FinalSummariesRun = 1, PeriodLocked = 0 }
                }
            };

            var mappedDtos = new List<ReleasePeriodDto>
            {
                new() { PeriodName = "Period1", PeriodType = "Month", StartPeriod = 0.5, EndPeriod = 1.0, FinalSummariesRun = 0, PeriodLocked = 0 },
                new() { PeriodName = "Period2", PeriodType = "Month", StartPeriod = 1.5, EndPeriod = 2.0, FinalSummariesRun = 1, PeriodLocked = 0 }
            };

            _mockHttp.GetAsync<List<ReleasePeriodRes>>(PactApiEndpoints.GetReleaseSummaries)
                .Returns(apiResponse);
            _mockMapper.Map<IReadOnlyList<ReleasePeriodDto>>(apiResponse.Data)
                .Returns(mappedDtos.AsReadOnly());

            // Act
            var result = await _client.GetReleaseSummariesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            Assert.Equal("Period1", result.Data[0].PeriodName);
            Assert.Equal("Period2", result.Data[1].PeriodName);

            await _mockHttp.Received(1).GetAsync<List<ReleasePeriodRes>>(PactApiEndpoints.GetReleaseSummaries);
            _mockMapper.Received(1).Map<IReadOnlyList<ReleasePeriodDto>>(apiResponse.Data);
        }

        [Fact]
        public async Task GetReleaseSummariesAsync_WithNullData_MapsEmptyListAndReturnsSuccess()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<ReleasePeriodRes>>
            {
                Success = true,
                Data    = null
            };

            var emptyDtos = new List<ReleasePeriodDto>().AsReadOnly();

            _mockHttp.GetAsync<List<ReleasePeriodRes>>(PactApiEndpoints.GetReleaseSummaries)
                .Returns(apiResponse);
            // When Data is null the client falls back to new List<ReleasePeriodRes>()
            _mockMapper.Map<IReadOnlyList<ReleasePeriodDto>>(Arg.Any<List<ReleasePeriodRes>>())
                .Returns(emptyDtos);

            // Act
            var result = await _client.GetReleaseSummariesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);

            await _mockHttp.Received(1).GetAsync<List<ReleasePeriodRes>>(PactApiEndpoints.GetReleaseSummaries);
        }

        [Fact]
        public async Task GetReleaseSummariesAsync_WithEmptyDataList_ReturnsMappedEmptyDtoList()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<ReleasePeriodRes>>
            {
                Success = true,
                Data    = new List<ReleasePeriodRes>()
            };

            var emptyDtos = new List<ReleasePeriodDto>().AsReadOnly();

            _mockHttp.GetAsync<List<ReleasePeriodRes>>(PactApiEndpoints.GetReleaseSummaries)
                .Returns(apiResponse);
            _mockMapper.Map<IReadOnlyList<ReleasePeriodDto>>(apiResponse.Data)
                .Returns(emptyDtos);

            // Act
            var result = await _client.GetReleaseSummariesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);

            await _mockHttp.Received(1).GetAsync<List<ReleasePeriodRes>>(PactApiEndpoints.GetReleaseSummaries);
        }

        [Fact]
        public async Task GetReleaseSummariesAsync_WithFailedResponse_ReturnsFailureResponseWithErrors()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<ReleasePeriodRes>>
            {
                Success = false,
                Errors  = new List<ApiError> { new() { Code = "ERR001", Message = "API Error" } }
            };

            var mappedFailure = new ApiResponseDto<IReadOnlyList<ReleasePeriodDto>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Code = "ERR001", Message = "API Error" } },
                Meta    = new ApiMetaDto()
            };

            _mockHttp.GetAsync<List<ReleasePeriodRes>>(PactApiEndpoints.GetReleaseSummaries)
                .Returns(apiResponse);
            _mockMapper.Map<ApiResponseDto<IReadOnlyList<ReleasePeriodDto>>>(apiResponse)
                .Returns(mappedFailure);

            // Act
            var result = await _client.GetReleaseSummariesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("ERR001",   result.Errors[0].Code);
            Assert.Equal("API Error", result.Errors[0].Message);

            await _mockHttp.Received(1).GetAsync<List<ReleasePeriodRes>>(PactApiEndpoints.GetReleaseSummaries);
            _mockMapper.Received(1).Map<ApiResponseDto<IReadOnlyList<ReleasePeriodDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetReleaseSummariesAsync_WithFailedResponse_DoesNotCallMapperForDtoList()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<ReleasePeriodRes>>
            {
                Success = false,
                Errors  = new List<ApiError> { new() { Code = "ERR001", Message = "API Error" } }
            };

            var mappedFailure = new ApiResponseDto<IReadOnlyList<ReleasePeriodDto>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Code = "ERR001", Message = "API Error" } },
                Meta    = new ApiMetaDto()
            };

            _mockHttp.GetAsync<List<ReleasePeriodRes>>(PactApiEndpoints.GetReleaseSummaries)
                .Returns(apiResponse);
            _mockMapper.Map<ApiResponseDto<IReadOnlyList<ReleasePeriodDto>>>(apiResponse)
                .Returns(mappedFailure);

            // Act
            await _client.GetReleaseSummariesAsync();

            // Assert — the DTO list mapper must NOT be invoked on the failure path
            _mockMapper.DidNotReceive().Map<IReadOnlyList<ReleasePeriodDto>>(Arg.Any<List<ReleasePeriodRes>>());
        }

        [Fact]
        public async Task GetReleaseSummariesAsync_UsesCorrectEndpoint()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<ReleasePeriodRes>> { Success = true, Data = new List<ReleasePeriodRes>() };
            _mockHttp.GetAsync<List<ReleasePeriodRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mockMapper.Map<IReadOnlyList<ReleasePeriodDto>>(Arg.Any<List<ReleasePeriodRes>>())
                .Returns(new List<ReleasePeriodDto>().AsReadOnly());

            // Act
            await _client.GetReleaseSummariesAsync();

            // Assert
            await _mockHttp.Received(1).GetAsync<List<ReleasePeriodRes>>(
                Arg.Is<string>(url => url == PactApiEndpoints.GetReleaseSummaries));
        }

        #endregion

        #region SetFinalSummaryRunAsync

        [Fact]
        public async Task SetFinalSummaryRunAsync_WithSuccessfulResponseAndNonNullData_ReturnsMappedDto()
        {
            // Arrange
            var apiResponse = new ApiResponse<ReleasePeriodRes>
            {
                Success = true,
                Data    = new ReleasePeriodRes
                {
                    PeriodName        = TestPeriodName,
                    FinalSummariesRun = TestFinalSummariesRun,
                    EndPeriod         = 1.0
                }
            };

            var mappedDto = new ReleasePeriodDto
            {
                PeriodName        = TestPeriodName,
                FinalSummariesRun = TestFinalSummariesRun,
                EndPeriod         = 1.0
            };

            _mockHttp.PutAsync<ReleasePeriodReq, ReleasePeriodRes>(
                    PactApiEndpoints.SetFinalSummaryRun,
                    Arg.Is<ReleasePeriodReq>(r =>
                        r.PeriodName        == TestPeriodName &&
                        r.FinalSummariesRun == TestFinalSummariesRun))
                .Returns(apiResponse);
            _mockMapper.Map<ReleasePeriodDto>(apiResponse.Data).Returns(mappedDto);

            // Act
            var result = await _client.SetFinalSummaryRunAsync(TestPeriodName, TestFinalSummariesRun);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(TestPeriodName,        result.Data.PeriodName);
            Assert.Equal(TestFinalSummariesRun, result.Data.FinalSummariesRun);

            await _mockHttp.Received(1).PutAsync<ReleasePeriodReq, ReleasePeriodRes>(
                PactApiEndpoints.SetFinalSummaryRun,
                Arg.Is<ReleasePeriodReq>(r =>
                    r.PeriodName        == TestPeriodName &&
                    r.FinalSummariesRun == TestFinalSummariesRun));
            _mockMapper.Received(1).Map<ReleasePeriodDto>(apiResponse.Data);
        }

        [Fact]
        public async Task SetFinalSummaryRunAsync_WithSuccessButNullData_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<ReleasePeriodRes>
            {
                Success = true,
                Data    = null          // success=true but no body — falls through to the failure path
            };

            var mappedFailure = new ApiResponseDto<ReleasePeriodDto>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Code = "NO_DATA", Message = "No data returned" } },
                Meta    = new ApiMetaDto()
            };

            _mockHttp.PutAsync<ReleasePeriodReq, ReleasePeriodRes>(
                    PactApiEndpoints.SetFinalSummaryRun, Arg.Any<ReleasePeriodReq>())
                .Returns(apiResponse);
            _mockMapper.Map<ApiResponseDto<ReleasePeriodDto>>(apiResponse).Returns(mappedFailure);

            // Act
            var result = await _client.SetFinalSummaryRunAsync(TestPeriodName, TestFinalSummariesRun);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);

            _mockMapper.DidNotReceive().Map<ReleasePeriodDto>(Arg.Any<ReleasePeriodRes>());
        }

        [Fact]
        public async Task SetFinalSummaryRunAsync_WithFailedResponse_ReturnsFailureResponseWithErrors()
        {
            // Arrange
            var apiResponse = new ApiResponse<ReleasePeriodRes>
            {
                Success = false,
                Errors  = new List<ApiError> { new() { Code = "ERR002", Message = "Period not found" } }
            };

            var mappedFailure = new ApiResponseDto<ReleasePeriodDto>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Code = "ERR002", Message = "Period not found" } },
                Meta    = new ApiMetaDto()
            };

            _mockHttp.PutAsync<ReleasePeriodReq, ReleasePeriodRes>(
                    PactApiEndpoints.SetFinalSummaryRun, Arg.Any<ReleasePeriodReq>())
                .Returns(apiResponse);
            _mockMapper.Map<ApiResponseDto<ReleasePeriodDto>>(apiResponse).Returns(mappedFailure);

            // Act
            var result = await _client.SetFinalSummaryRunAsync(TestPeriodName, TestFinalSummariesRun);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("ERR002",           result.Errors[0].Code);
            Assert.Equal("Period not found", result.Errors[0].Message);

            await _mockHttp.Received(1).PutAsync<ReleasePeriodReq, ReleasePeriodRes>(
                PactApiEndpoints.SetFinalSummaryRun, Arg.Any<ReleasePeriodReq>());
            _mockMapper.Received(1).Map<ApiResponseDto<ReleasePeriodDto>>(apiResponse);
        }

        [Fact]
        public async Task SetFinalSummaryRunAsync_WithFailedResponse_DoesNotCallMapperForDto()
        {
            // Arrange
            var apiResponse = new ApiResponse<ReleasePeriodRes>
            {
                Success = false,
                Errors  = new List<ApiError> { new() { Code = "ERR002", Message = "Period not found" } }
            };

            var mappedFailure = new ApiResponseDto<ReleasePeriodDto>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Code = "ERR002", Message = "Period not found" } },
                Meta    = new ApiMetaDto()
            };

            _mockHttp.PutAsync<ReleasePeriodReq, ReleasePeriodRes>(
                    PactApiEndpoints.SetFinalSummaryRun, Arg.Any<ReleasePeriodReq>())
                .Returns(apiResponse);
            _mockMapper.Map<ApiResponseDto<ReleasePeriodDto>>(apiResponse).Returns(mappedFailure);

            // Act
            await _client.SetFinalSummaryRunAsync(TestPeriodName, TestFinalSummariesRun);

            // Assert — DTO mapper must NOT be invoked on the failure path
            _mockMapper.DidNotReceive().Map<ReleasePeriodDto>(Arg.Any<ReleasePeriodRes>());
        }

        [Fact]
        public async Task SetFinalSummaryRunAsync_BuildsRequestWithCorrectPeriodNameAndFinalSummariesRun()
        {
            // Arrange
            const string periodName        = "ArgCheckPeriod";
            const short  finalSummariesRun = 3;

            var apiResponse = new ApiResponse<ReleasePeriodRes>
            {
                Success = true,
                Data    = new ReleasePeriodRes { PeriodName = periodName, FinalSummariesRun = finalSummariesRun }
            };

            _mockHttp.PutAsync<ReleasePeriodReq, ReleasePeriodRes>(
                    Arg.Any<string>(), Arg.Any<ReleasePeriodReq>())
                .Returns(apiResponse);
            _mockMapper.Map<ReleasePeriodDto>(apiResponse.Data)
                .Returns(new ReleasePeriodDto { PeriodName = periodName, FinalSummariesRun = finalSummariesRun });

            // Act
            await _client.SetFinalSummaryRunAsync(periodName, finalSummariesRun);

            // Assert — exact request payload sent to the HTTP executor
            await _mockHttp.Received(1).PutAsync<ReleasePeriodReq, ReleasePeriodRes>(
                Arg.Any<string>(),
                Arg.Is<ReleasePeriodReq>(r =>
                    r.PeriodName        == periodName &&
                    r.FinalSummariesRun == finalSummariesRun));
        }

        [Fact]
        public async Task SetFinalSummaryRunAsync_UsesCorrectEndpoint()
        {
            // Arrange
            var apiResponse = new ApiResponse<ReleasePeriodRes>
            {
                Success = true,
                Data    = new ReleasePeriodRes { PeriodName = TestPeriodName, FinalSummariesRun = TestFinalSummariesRun }
            };

            _mockHttp.PutAsync<ReleasePeriodReq, ReleasePeriodRes>(
                    Arg.Any<string>(), Arg.Any<ReleasePeriodReq>())
                .Returns(apiResponse);
            _mockMapper.Map<ReleasePeriodDto>(apiResponse.Data)
                .Returns(new ReleasePeriodDto { PeriodName = TestPeriodName });

            // Act
            await _client.SetFinalSummaryRunAsync(TestPeriodName, TestFinalSummariesRun);

            // Assert
            await _mockHttp.Received(1).PutAsync<ReleasePeriodReq, ReleasePeriodRes>(
                Arg.Is<string>(url => url == PactApiEndpoints.SetFinalSummaryRun),
                Arg.Any<ReleasePeriodReq>());
        }

        [Fact]
        public async Task SetFinalSummaryRunAsync_MapsAllFieldsFromApiResponseToDto()
        {
            // Arrange
            var responseData = new ReleasePeriodRes
            {
                PeriodName        = TestPeriodName,
                PeriodType        = "Month",
                StartPeriod       = 1.5,
                EndPeriod         = 2.5,
                FinalSummariesRun = TestFinalSummariesRun,
                PeriodLocked      = 0
            };

            var apiResponse = new ApiResponse<ReleasePeriodRes> { Success = true, Data = responseData };

            var mappedDto = new ReleasePeriodDto
            {
                PeriodName        = TestPeriodName,
                PeriodType        = "Month",
                StartPeriod       = 1.5,
                EndPeriod         = 2.5,
                FinalSummariesRun = TestFinalSummariesRun,
                PeriodLocked      = 0
            };

            _mockHttp.PutAsync<ReleasePeriodReq, ReleasePeriodRes>(
                    Arg.Any<string>(), Arg.Any<ReleasePeriodReq>())
                .Returns(apiResponse);
            _mockMapper.Map<ReleasePeriodDto>(responseData).Returns(mappedDto);

            // Act
            var result = await _client.SetFinalSummaryRunAsync(TestPeriodName, TestFinalSummariesRun);

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

        #endregion
    }
}
