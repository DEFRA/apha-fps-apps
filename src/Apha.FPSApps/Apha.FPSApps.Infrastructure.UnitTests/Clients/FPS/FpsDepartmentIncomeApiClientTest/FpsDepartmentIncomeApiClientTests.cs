using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsDepartmentIncomeApiClientTest
{
    public class FpsDepartmentIncomeApiClientTests
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly FpsDepartmentIncomeApiClient _client;

        private const string BaseUrl   = "api/v1/department-income";
        private const string TestProject = "AH0033";

        public FpsDepartmentIncomeApiClientTests()
        {
            _http   = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsDepartmentIncomeApiClient(_http, _mapper);
        }

        // ── Helper builders ─────────────────────────────────────────────────────

        private static ApiResponse<List<DepartmentIncomeTimeRes>> TimeHttpSuccess() =>
            new() { Success = true, Data = new List<DepartmentIncomeTimeRes>
            {
                new() { Project = "PROJ1", Month = 1, TotalCost = 100m },
                new() { Project = "PROJ2", Month = 2, TotalCost = 200m },
            }};

        private static ApiResponse<List<DepartmentIncomeTestRes>> TestHttpSuccess() =>
            new() { Success = true, Data = new List<DepartmentIncomeTestRes>
            {
                new() { Project = "PROJ1", Month = 1, TotalCost = 50m },
            }};

        private static ApiResponse<List<DepartmentIncomeAnimalRes>> AnimalHttpSuccess() =>
            new() { Success = true, Data = new List<DepartmentIncomeAnimalRes>
            {
                new() { Project = "PROJ1", Month = 1, TotalCost = 75m },
            }};

        private static ApiResponse<List<DepartmentIncomeAdditionalRes>> AdditionalHttpSuccess() =>
            new() { Success = true, Data = new List<DepartmentIncomeAdditionalRes>
            {
                new() { Project = "PROJ1", Month = 1, TotalCost = 25m },
            }};

        private static ApiResponse<List<DepartmentIncomeTotalsRes>> TotalsHttpSuccess() =>
            new() { Success = true, Data = new List<DepartmentIncomeTotalsRes>
            {
                new() { Project = "PROJ1", TotalCosts = 250m },
            }};

        private static ApiResponse<List<PeriodLookupRes>> PeriodsHttpSuccess() =>
            new() { Success = true, Data = new List<PeriodLookupRes>
            {
                new() { AccntsPeriod = 1, MonthName = "April", MonthNumber = 4 },
                new() { AccntsPeriod = 2, MonthName = "May",   MonthNumber = 5 },
            }};

        private static ApiResponse<List<DepartmentIncomeTimeRes>> HttpFailure<T>() =>
            new ApiResponse<List<DepartmentIncomeTimeRes>>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } }
            };

        private static ApiResponseDto<List<DepartmentIncomeTimeDto>> TimeDtoSuccess() =>
            ApiResponseDto<List<DepartmentIncomeTimeDto>>.SuccessResponse(
                new List<DepartmentIncomeTimeDto>
                {
                    new() { Project = "PROJ1", Month = 1, TotalCost = 100m }
                });

        private static ApiResponseDto<List<DepartmentIncomeTimeDto>> TimeDtoFailure() =>
            new ApiResponseDto<List<DepartmentIncomeTimeDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

        // ── Constructor Guard ────────────────────────────────────────────────────

        #region Constructor

        [Fact]
        public void Constructor_NullHttpExecutor_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new FpsDepartmentIncomeApiClient(null!, _mapper));
        }

        [Fact]
        public void Constructor_NullMapper_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new FpsDepartmentIncomeApiClient(_http, null!));
        }

        #endregion

        // ── GetTimeIncomeAsync ──────────────────────────────────────────────────

        #region GetTimeIncomeAsync

        [Fact]
        public async Task GetTimeIncomeAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var httpResponse = TimeHttpSuccess();
            var expectedDto  = TimeDtoSuccess();

            _http.GetAsync<List<DepartmentIncomeTimeRes>>(
                    Arg.Is<string>(url => url.Contains($"{BaseUrl}/time")))
                .Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<DepartmentIncomeTimeDto>>>(httpResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetTimeIncomeAsync(TestProject, 1, 6);

            // Assert
            Assert.True(result.Success);
            _mapper.Received(1).Map<ApiResponseDto<List<DepartmentIncomeTimeDto>>>(httpResponse);
        }

        [Fact]
        public async Task GetTimeIncomeAsync_WithProject_IncludesProjectInUrl()
        {
            // Arrange
            var httpResponse = TimeHttpSuccess();
            var dto = TimeDtoSuccess();

            _http.GetAsync<List<DepartmentIncomeTimeRes>>(
                    Arg.Is<string>(url => url.Contains($"{BaseUrl}/time") && url.Contains($"project={TestProject}")))
                .Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<DepartmentIncomeTimeDto>>>(httpResponse).Returns(dto);

            // Act
            var result = await _client.GetTimeIncomeAsync(TestProject, null, null);

            // Assert
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<DepartmentIncomeTimeRes>>(
                Arg.Is<string>(url => url.Contains($"project={TestProject}")));
        }

        [Fact]
        public async Task GetTimeIncomeAsync_WithMonthFromAndMonthTo_IncludesMonthParamsInUrl()
        {
            // Arrange
            var httpResponse = TimeHttpSuccess();
            var dto = TimeDtoSuccess();

            _http.GetAsync<List<DepartmentIncomeTimeRes>>(
                    Arg.Is<string>(url => url.Contains("monthFrom=1") && url.Contains("monthTo=6")))
                .Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<DepartmentIncomeTimeDto>>>(httpResponse).Returns(dto);

            // Act
            await _client.GetTimeIncomeAsync(null, 1, 6);

            // Assert
            await _http.Received(1).GetAsync<List<DepartmentIncomeTimeRes>>(
                Arg.Is<string>(url => url.Contains("monthFrom=1") && url.Contains("monthTo=6")));
        }

        [Fact]
        public async Task GetTimeIncomeAsync_NullParams_CallsBaseUrlWithNoQueryString()
        {
            // Arrange
            var httpResponse = TimeHttpSuccess();
            var dto = TimeDtoSuccess();

            _http.GetAsync<List<DepartmentIncomeTimeRes>>($"{BaseUrl}/time").Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<DepartmentIncomeTimeDto>>>(httpResponse).Returns(dto);

            // Act
            await _client.GetTimeIncomeAsync(null, null, null);

            // Assert
            await _http.Received(1).GetAsync<List<DepartmentIncomeTimeRes>>($"{BaseUrl}/time");
        }

        [Fact]
        public async Task GetTimeIncomeAsync_HttpReturnsFailure_ReturnsFailureMappedResponse()
        {
            // Arrange
            var httpResponse = new ApiResponse<List<DepartmentIncomeTimeRes>>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "API Error", Code = "ERROR" } }
            };
            var mappedFailure = TimeDtoFailure();

            _http.GetAsync<List<DepartmentIncomeTimeRes>>(Arg.Any<string>()).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<DepartmentIncomeTimeDto>>>(httpResponse).Returns(mappedFailure);

            // Act
            var result = await _client.GetTimeIncomeAsync(TestProject, 1, 6);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetTimeIncomeAsync_HttpThrowsException_PropagatesException()
        {
            // Arrange
            _http.GetAsync<List<DepartmentIncomeTimeRes>>(Arg.Any<string>())
                .ThrowsAsync(new Exception("Network error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _client.GetTimeIncomeAsync(TestProject, 1, 6));
        }

        #endregion

        // ── GetTestIncomeAsync ──────────────────────────────────────────────────

        #region GetTestIncomeAsync

        [Fact]
        public async Task GetTestIncomeAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var httpResponse = TestHttpSuccess();
            var dto = ApiResponseDto<List<DepartmentIncomeTestDto>>.SuccessResponse(
                new List<DepartmentIncomeTestDto> { new() { Project = "PROJ1" } });

            _http.GetAsync<List<DepartmentIncomeTestRes>>(
                    Arg.Is<string>(url => url.Contains($"{BaseUrl}/tests")))
                .Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<DepartmentIncomeTestDto>>>(httpResponse).Returns(dto);

            // Act
            var result = await _client.GetTestIncomeAsync(TestProject, 1, 6);

            // Assert
            Assert.True(result.Success);
            _mapper.Received(1).Map<ApiResponseDto<List<DepartmentIncomeTestDto>>>(httpResponse);
        }

        [Fact]
        public async Task GetTestIncomeAsync_HttpThrowsException_PropagatesException()
        {
            // Arrange
            _http.GetAsync<List<DepartmentIncomeTestRes>>(Arg.Any<string>())
                .ThrowsAsync(new Exception("Network error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _client.GetTestIncomeAsync(TestProject, 1, 6));
        }

        [Fact]
        public async Task GetTestIncomeAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var httpResponse = new ApiResponse<List<DepartmentIncomeTestRes>>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Error", Code = "ERROR" } }
            };
            var mappedFailure = new ApiResponseDto<List<DepartmentIncomeTestDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<DepartmentIncomeTestRes>>(Arg.Any<string>()).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<DepartmentIncomeTestDto>>>(httpResponse).Returns(mappedFailure);

            // Act
            var result = await _client.GetTestIncomeAsync(TestProject, 1, 6);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        // ── GetTestSnapshotIncomeAsync ─────────────────────────────────────────

        #region GetTestSnapshotIncomeAsync

        [Fact]
        public async Task GetTestSnapshotIncomeAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var httpResponse = TestHttpSuccess();
            var dto = ApiResponseDto<List<DepartmentIncomeTestDto>>.SuccessResponse(
                new List<DepartmentIncomeTestDto> { new() { Project = "PROJ1" } });

            _http.GetAsync<List<DepartmentIncomeTestRes>>(
                    Arg.Is<string>(url => url.Contains($"{BaseUrl}/snapshot/tests")))
                .Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<DepartmentIncomeTestDto>>>(httpResponse).Returns(dto);

            // Act
            var result = await _client.GetTestSnapshotIncomeAsync(TestProject, 1, 6);

            // Assert
            Assert.True(result.Success);
            _mapper.Received(1).Map<ApiResponseDto<List<DepartmentIncomeTestDto>>>(httpResponse);
        }

        [Fact]
        public async Task GetTestSnapshotIncomeAsync_HttpThrowsException_PropagatesException()
        {
            // Arrange
            _http.GetAsync<List<DepartmentIncomeTestRes>>(Arg.Any<string>())
                .ThrowsAsync(new Exception("Network error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _client.GetTestSnapshotIncomeAsync(TestProject, 1, 6));
        }

        [Fact]
        public async Task GetTestSnapshotIncomeAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var httpResponse = new ApiResponse<List<DepartmentIncomeTestRes>>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Error", Code = "ERROR" } }
            };
            var mappedFailure = new ApiResponseDto<List<DepartmentIncomeTestDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<DepartmentIncomeTestRes>>(Arg.Any<string>()).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<DepartmentIncomeTestDto>>>(httpResponse).Returns(mappedFailure);

            // Act
            var result = await _client.GetTestSnapshotIncomeAsync(TestProject, 1, 6);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        // ── GetAnimalIncomeAsync ────────────────────────────────────────────────

        #region GetAnimalIncomeAsync

        [Fact]
        public async Task GetAnimalIncomeAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var httpResponse = AnimalHttpSuccess();
            var dto = ApiResponseDto<List<DepartmentIncomeAnimalDto>>.SuccessResponse(
                new List<DepartmentIncomeAnimalDto> { new() { Project = "PROJ1" } });

            _http.GetAsync<List<DepartmentIncomeAnimalRes>>(
                    Arg.Is<string>(url => url.Contains($"{BaseUrl}/animals")))
                .Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<DepartmentIncomeAnimalDto>>>(httpResponse).Returns(dto);

            // Act
            var result = await _client.GetAnimalIncomeAsync(TestProject, 1, 12);

            // Assert
            Assert.True(result.Success);
            _mapper.Received(1).Map<ApiResponseDto<List<DepartmentIncomeAnimalDto>>>(httpResponse);
        }

        [Fact]
        public async Task GetAnimalIncomeAsync_HttpThrowsException_PropagatesException()
        {
            // Arrange
            _http.GetAsync<List<DepartmentIncomeAnimalRes>>(Arg.Any<string>())
                .ThrowsAsync(new Exception("Network error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _client.GetAnimalIncomeAsync(TestProject, 1, 12));
        }

        #endregion

        // ── GetAdditionalIncomeAsync ────────────────────────────────────────────

        #region GetAdditionalIncomeAsync

        [Fact]
        public async Task GetAdditionalIncomeAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var httpResponse = AdditionalHttpSuccess();
            var dto = ApiResponseDto<List<DepartmentIncomeAdditionalDto>>.SuccessResponse(
                new List<DepartmentIncomeAdditionalDto> { new() { Project = "PROJ1" } });

            _http.GetAsync<List<DepartmentIncomeAdditionalRes>>(
                    Arg.Is<string>(url => url.Contains($"{BaseUrl}/additional")))
                .Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<DepartmentIncomeAdditionalDto>>>(httpResponse).Returns(dto);

            // Act
            var result = await _client.GetAdditionalIncomeAsync(TestProject, 1, 12);

            // Assert
            Assert.True(result.Success);
            _mapper.Received(1).Map<ApiResponseDto<List<DepartmentIncomeAdditionalDto>>>(httpResponse);
        }

        [Fact]
        public async Task GetAdditionalIncomeAsync_HttpThrowsException_PropagatesException()
        {
            // Arrange
            _http.GetAsync<List<DepartmentIncomeAdditionalRes>>(Arg.Any<string>())
                .ThrowsAsync(new Exception("Network error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _client.GetAdditionalIncomeAsync(TestProject, 1, 12));
        }

        [Fact]
        public async Task GetAdditionalIncomeAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var httpResponse = new ApiResponse<List<DepartmentIncomeAdditionalRes>>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Error", Code = "ERROR" } }
            };
            var mappedFailure = new ApiResponseDto<List<DepartmentIncomeAdditionalDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<DepartmentIncomeAdditionalRes>>(Arg.Any<string>()).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<DepartmentIncomeAdditionalDto>>>(httpResponse).Returns(mappedFailure);

            // Act
            var result = await _client.GetAdditionalIncomeAsync(TestProject, 1, 12);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        // ── GetTotalsAsync ──────────────────────────────────────────────────────

        #region GetTotalsAsync

        [Fact]
        public async Task GetTotalsAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var httpResponse = TotalsHttpSuccess();
            var dto = ApiResponseDto<List<DepartmentIncomeTotalsDto>>.SuccessResponse(
                new List<DepartmentIncomeTotalsDto> { new() { Project = "PROJ1", TotalCosts = 250m } });

            _http.GetAsync<List<DepartmentIncomeTotalsRes>>(
                    Arg.Is<string>(url => url.Contains($"{BaseUrl}/totals")))
                .Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<DepartmentIncomeTotalsDto>>>(httpResponse).Returns(dto);

            // Act
            var result = await _client.GetTotalsAsync(TestProject, 1, 12);

            // Assert
            Assert.True(result.Success);
            _mapper.Received(1).Map<ApiResponseDto<List<DepartmentIncomeTotalsDto>>>(httpResponse);
        }

        [Fact]
        public async Task GetTotalsAsync_HttpThrowsException_PropagatesException()
        {
            // Arrange
            _http.GetAsync<List<DepartmentIncomeTotalsRes>>(Arg.Any<string>())
                .ThrowsAsync(new Exception("Network error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _client.GetTotalsAsync(TestProject, 1, 12));
        }

        [Fact]
        public async Task GetTotalsAsync_WithOptionalParams_IncludesParamsInUrl()
        {
            // Arrange
            var httpResponse = TotalsHttpSuccess();
            var dto = ApiResponseDto<List<DepartmentIncomeTotalsDto>>.SuccessResponse(new List<DepartmentIncomeTotalsDto>());

            _http.GetAsync<List<DepartmentIncomeTotalsRes>>(
                    Arg.Is<string>(url => url.Contains($"{BaseUrl}/totals")
                                       && url.Contains($"project={TestProject}")
                                       && url.Contains("monthFrom=1")
                                       && url.Contains("monthTo=12")))
                .Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<DepartmentIncomeTotalsDto>>>(httpResponse).Returns(dto);

            // Act
            await _client.GetTotalsAsync(TestProject, 1, 12);

            // Assert
            await _http.Received(1).GetAsync<List<DepartmentIncomeTotalsRes>>(
                Arg.Is<string>(url => url.Contains($"project={TestProject}")
                                   && url.Contains("monthFrom=1")
                                   && url.Contains("monthTo=12")));
        }

        #endregion

        // ── GetPeriodsAsync ─────────────────────────────────────────────────────

        #region GetPeriodsAsync

        [Fact]
        public async Task GetPeriodsAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var httpResponse = PeriodsHttpSuccess();
            var dto = ApiResponseDto<List<PeriodLookupDto>>.SuccessResponse(
                new List<PeriodLookupDto>
                {
                    new() { AccntsPeriod = 1, MonthName = "April", MonthNumber = 4 },
                    new() { AccntsPeriod = 2, MonthName = "May",   MonthNumber = 5 },
                });

            _http.GetAsync<List<PeriodLookupRes>>($"{BaseUrl}/periods").Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<PeriodLookupDto>>>(httpResponse).Returns(dto);

            // Act
            var result = await _client.GetPeriodsAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.Count);
            await _http.Received(1).GetAsync<List<PeriodLookupRes>>($"{BaseUrl}/periods");
            _mapper.Received(1).Map<ApiResponseDto<List<PeriodLookupDto>>>(httpResponse);
        }

        [Fact]
        public async Task GetPeriodsAsync_HttpThrowsException_PropagatesException()
        {
            // Arrange
            _http.GetAsync<List<PeriodLookupRes>>(Arg.Any<string>())
                .ThrowsAsync(new Exception("Network error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _client.GetPeriodsAsync());
        }

        [Fact]
        public async Task GetPeriodsAsync_HttpReturnsFailure_ReturnsFailureMappedResponse()
        {
            // Arrange
            var httpResponse = new ApiResponse<List<PeriodLookupRes>>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Error", Code = "ERROR" } }
            };
            var mappedFailure = new ApiResponseDto<List<PeriodLookupDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<PeriodLookupRes>>(Arg.Any<string>()).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<PeriodLookupDto>>>(httpResponse).Returns(mappedFailure);

            // Act
            var result = await _client.GetPeriodsAsync();

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        // ── GetSnapshotPeriodsAsync ─────────────────────────────────────────────

        #region GetSnapshotPeriodsAsync

        [Fact]
        public async Task GetSnapshotPeriodsAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var httpResponse = new ApiResponse<List<PeriodSnapshotRes>>
            {
                Success = true,
                Data = new List<PeriodSnapshotRes>
                {
                    new() { PeriodName = "April 2025 Only",  EndPeriod = 4, PeriodLocked = false },
                    new() { PeriodName = "April - May 2025", EndPeriod = 5, PeriodLocked = false },
                }
            };
            var expectedDto = ApiResponseDto<List<PeriodSnapshotDto>>.SuccessResponse(
                new List<PeriodSnapshotDto>
                {
                    new() { PeriodName = "April 2025 Only",  EndPeriod = 4, PeriodLocked = false },
                    new() { PeriodName = "April - May 2025", EndPeriod = 5, PeriodLocked = false },
                });

            _http.GetAsync<List<PeriodSnapshotRes>>(
                    Arg.Is<string>(url => url.Contains("snapshot-periods")))
                .Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<PeriodSnapshotDto>>>(httpResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetSnapshotPeriodsAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.Count);
            _mapper.Received(1).Map<ApiResponseDto<List<PeriodSnapshotDto>>>(httpResponse);
        }

        [Fact]
        public async Task GetSnapshotPeriodsAsync_HttpReturnsEmpty_ReturnsSuccessWithEmpty()
        {
            // Arrange
            var httpResponse = new ApiResponse<List<PeriodSnapshotRes>> { Success = true, Data = new List<PeriodSnapshotRes>() };
            var expectedDto  = ApiResponseDto<List<PeriodSnapshotDto>>.SuccessResponse(new List<PeriodSnapshotDto>());

            _http.GetAsync<List<PeriodSnapshotRes>>(Arg.Any<string>()).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<PeriodSnapshotDto>>>(httpResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetSnapshotPeriodsAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetSnapshotPeriodsAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var httpResponse = new ApiResponse<List<PeriodSnapshotRes>>
            {
                Success = false,
                Errors  = new List<ApiError> { new() { Message = "Error", Code = "ERROR" } }
            };
            var mappedFailure = new ApiResponseDto<List<PeriodSnapshotDto>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } },
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<List<PeriodSnapshotRes>>(Arg.Any<string>()).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<PeriodSnapshotDto>>>(httpResponse).Returns(mappedFailure);

            // Act
            var result = await _client.GetSnapshotPeriodsAsync();

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetSnapshotPeriodsAsync_HttpThrowsException_PropagatesException()
        {
            // Arrange
            _http.GetAsync<List<PeriodSnapshotRes>>(Arg.Any<string>())
                .ThrowsAsync(new Exception("Network error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _client.GetSnapshotPeriodsAsync());
        }

        #endregion

        // ── UpdatePeriodLockedAsync ─────────────────────────────────────────────

        #region UpdatePeriodLockedAsync

        [Fact]
        public async Task UpdatePeriodLockedAsync_HttpReturnsSuccess_ReturnsSuccessTrue()
        {
            // Arrange
            var httpResponse = new ApiResponse<bool> { Success = true, Data = true };

            _http.PutAsync<bool, bool>(
                    Arg.Is<string>(url => url.Contains("snapshot-periods/lock") && url.Contains("periodName=")),
                    true)
                .Returns(httpResponse);

            // Act
            var result = await _client.UpdatePeriodLockedAsync("April 2025 Only", true);

            // Assert
            Assert.True(result.Success);
            Assert.True(result.Data);
        }

        [Fact]
        public async Task UpdatePeriodLockedAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var httpResponse = new ApiResponse<bool>
            {
                Success = false,
                Errors  = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } }
            };
            var mappedFailure = new ApiResponseDto<bool>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta    = new ApiMetaDto()
            };

            _http.PutAsync<bool, bool>(Arg.Any<string>(), Arg.Any<bool>()).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<bool>>(httpResponse).Returns(mappedFailure);

            // Act
            var result = await _client.UpdatePeriodLockedAsync("NonExistent", true);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task UpdatePeriodLockedAsync_PeriodNameEncodedInQueryString()
        {
            // Arrange — period names with slashes must be passed as query-string, not route segment
            const string slashPeriodName = "April - August 2025/25";
            var httpResponse = new ApiResponse<bool> { Success = true, Data = true };

            _http.PutAsync<bool, bool>(
                    Arg.Is<string>(url => url.Contains("snapshot-periods/lock") && url.Contains("periodName=")),
                    true)
                .Returns(httpResponse);

            // Act
            var result = await _client.UpdatePeriodLockedAsync(slashPeriodName, true);

            // Assert
            Assert.True(result.Success);
            await _http.Received(1).PutAsync<bool, bool>(
                Arg.Is<string>(url => url.Contains("snapshot-periods/lock") && url.Contains("periodName=")),
                true);
        }

        [Fact]
        public async Task UpdatePeriodLockedAsync_HttpThrowsException_PropagatesException()
        {
            // Arrange
            _http.PutAsync<bool, bool>(Arg.Any<string>(), Arg.Any<bool>())
                .ThrowsAsync(new Exception("Network error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _client.UpdatePeriodLockedAsync("April 2025 Only", true));
        }

        #endregion

        // ── GetTimeIncomeCurrentAsync ───────────────────────────────────────────

        #region GetTimeIncomeCurrentAsync

        [Fact]
        public async Task GetTimeIncomeCurrentAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var httpResponse = TimeHttpSuccess();
            var expectedDto  = TimeDtoSuccess();

            _http.GetAsync<List<DepartmentIncomeTimeRes>>(
                    Arg.Is<string>(url => url.Contains("current/time")))
                .Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<DepartmentIncomeTimeDto>>>(httpResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetTimeIncomeCurrentAsync(TestProject, 1, 6);

            // Assert
            Assert.True(result.Success);
            _mapper.Received(1).Map<ApiResponseDto<List<DepartmentIncomeTimeDto>>>(httpResponse);
        }

        [Fact]
        public async Task GetTimeIncomeCurrentAsync_HttpThrowsException_PropagatesException()
        {
            // Arrange
            _http.GetAsync<List<DepartmentIncomeTimeRes>>(
                    Arg.Is<string>(url => url.Contains("current/time")))
                .ThrowsAsync(new Exception("Network error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _client.GetTimeIncomeCurrentAsync(TestProject, 1, 6));
        }

        [Fact]
        public async Task GetTimeIncomeCurrentAsync_HttpReturnsFailure_ReturnsFailureMappedResponse()
        {
            // Arrange
            var httpResponse = new ApiResponse<List<DepartmentIncomeTimeRes>>
            {
                Success = false,
                Errors  = new List<ApiError> { new() { Message = "Error", Code = "ERR" } }
            };
            var mappedFailure = TimeDtoFailure();

            _http.GetAsync<List<DepartmentIncomeTimeRes>>(Arg.Any<string>()).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<DepartmentIncomeTimeDto>>>(httpResponse).Returns(mappedFailure);

            // Act
            var result = await _client.GetTimeIncomeCurrentAsync(TestProject, 1, 6);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        // ── GetTestIncomeCurrentAsync ───────────────────────────────────────────

        #region GetTestIncomeCurrentAsync

        [Fact]
        public async Task GetTestIncomeCurrentAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var httpResponse = TestHttpSuccess();
            var expectedDto  = ApiResponseDto<List<DepartmentIncomeTestDto>>.SuccessResponse(
                new List<DepartmentIncomeTestDto> { new() { Project = "PROJ1", Month = 1, TotalCost = 50m } });

            _http.GetAsync<List<DepartmentIncomeTestRes>>(
                    Arg.Is<string>(url => url.Contains("current/tests")))
                .Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<DepartmentIncomeTestDto>>>(httpResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetTestIncomeCurrentAsync(TestProject, 1, 6);

            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public async Task GetTestIncomeCurrentAsync_HttpThrowsException_PropagatesException()
        {
            // Arrange
            _http.GetAsync<List<DepartmentIncomeTestRes>>(
                    Arg.Is<string>(url => url.Contains("current/tests")))
                .ThrowsAsync(new Exception("Network error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _client.GetTestIncomeCurrentAsync(TestProject, 1, 6));
        }

        #endregion

        // ── GetAnimalIncomeCurrentAsync ─────────────────────────────────────────

        #region GetAnimalIncomeCurrentAsync

        [Fact]
        public async Task GetAnimalIncomeCurrentAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var httpResponse = AnimalHttpSuccess();
            var expectedDto  = ApiResponseDto<List<DepartmentIncomeAnimalDto>>.SuccessResponse(
                new List<DepartmentIncomeAnimalDto> { new() { Project = "PROJ1", Month = 1, TotalCost = 75m } });

            _http.GetAsync<List<DepartmentIncomeAnimalRes>>(
                    Arg.Is<string>(url => url.Contains("current/animals")))
                .Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<DepartmentIncomeAnimalDto>>>(httpResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAnimalIncomeCurrentAsync(TestProject, 1, 6);

            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public async Task GetAnimalIncomeCurrentAsync_HttpThrowsException_PropagatesException()
        {
            // Arrange
            _http.GetAsync<List<DepartmentIncomeAnimalRes>>(
                    Arg.Is<string>(url => url.Contains("current/animals")))
                .ThrowsAsync(new Exception("Network error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _client.GetAnimalIncomeCurrentAsync(TestProject, 1, 6));
        }

        #endregion

        // ── GetAdditionalIncomeCurrentAsync ────────────────────────────────────

        #region GetAdditionalIncomeCurrentAsync

        [Fact]
        public async Task GetAdditionalIncomeCurrentAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var httpResponse = AdditionalHttpSuccess();
            var expectedDto  = ApiResponseDto<List<DepartmentIncomeAdditionalDto>>.SuccessResponse(
                new List<DepartmentIncomeAdditionalDto> { new() { Project = "PROJ1", Month = 1, TotalCost = 25m } });

            _http.GetAsync<List<DepartmentIncomeAdditionalRes>>(
                    Arg.Is<string>(url => url.Contains("current/additional")))
                .Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<DepartmentIncomeAdditionalDto>>>(httpResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAdditionalIncomeCurrentAsync(TestProject, 1, 6);

            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public async Task GetAdditionalIncomeCurrentAsync_HttpThrowsException_PropagatesException()
        {
            // Arrange
            _http.GetAsync<List<DepartmentIncomeAdditionalRes>>(
                    Arg.Is<string>(url => url.Contains("current/additional")))
                .ThrowsAsync(new Exception("Network error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _client.GetAdditionalIncomeCurrentAsync(TestProject, 1, 6));
        }

        #endregion

        // ── GetTotalsCurrentAsync ───────────────────────────────────────────────

        #region GetTotalsCurrentAsync

        [Fact]
        public async Task GetTotalsCurrentAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var httpResponse = TotalsHttpSuccess();
            var expectedDto  = ApiResponseDto<List<DepartmentIncomeTotalsDto>>.SuccessResponse(
                new List<DepartmentIncomeTotalsDto> { new() { Project = "PROJ1", TotalCosts = 250m } });

            _http.GetAsync<List<DepartmentIncomeTotalsRes>>(
                    Arg.Is<string>(url => url.Contains("current/totals")))
                .Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<DepartmentIncomeTotalsDto>>>(httpResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetTotalsCurrentAsync(TestProject, 1, 6);

            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public async Task GetTotalsCurrentAsync_HttpThrowsException_PropagatesException()
        {
            // Arrange
            _http.GetAsync<List<DepartmentIncomeTotalsRes>>(
                    Arg.Is<string>(url => url.Contains("current/totals")))
                .ThrowsAsync(new Exception("Network error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _client.GetTotalsCurrentAsync(TestProject, 1, 6));
        }

        #endregion
    }
}
