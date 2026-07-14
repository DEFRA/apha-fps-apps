/*
 * TRANSFORMENGINE MIGRATION — FpsDepartmentIncomeApiClientTests.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 13 — Unit Tests - Backend + Frontend xUnit Coverage
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - New xUnit test class for FpsDepartmentIncomeApiClient (Infrastructure layer)
 *   - Covers all 6 public methods: GetTimeIncomeAsync, GetTestIncomeAsync, GetAnimalIncomeAsync,
 *     GetAdditionalIncomeAsync, GetTotalsAsync, GetPeriodsAsync
 *   - NSubstitute mocks for IFpsHttpExecutor and IMapper
 *   - Tests: HTTP success path (mapper called), HTTP failure path, exception catch path (INTERNAL_ERROR),
 *     URL construction (BaseUrl + segment + optional query params)
 *
 * PRESERVED:
 *   - BaseUrl = "api/v1/department-income" (matches backend DepartmentIncomeController route)
 *   - All 6 interface methods; no CRUD methods (resource is read-only)
 *   - Optional params (project, monthFrom, monthTo) appended only when non-null
 *
 * DEFERRED: none — fully automated.
 */

using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.DepartmentIncome;
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
        public async Task GetTimeIncomeAsync_HttpThrowsException_ReturnsFailureWithInternalError()
        {
            // Arrange
            _http.GetAsync<List<DepartmentIncomeTimeRes>>(Arg.Any<string>())
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetTimeIncomeAsync(TestProject, 1, 6);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Contains(result.Errors, e => e.Code == "INTERNAL_ERROR");
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
        public async Task GetTestIncomeAsync_HttpThrowsException_ReturnsFailureWithInternalError()
        {
            // Arrange
            _http.GetAsync<List<DepartmentIncomeTestRes>>(Arg.Any<string>())
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetTestIncomeAsync(TestProject, 1, 6);

            // Assert
            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
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
        public async Task GetAnimalIncomeAsync_HttpThrowsException_ReturnsFailureWithInternalError()
        {
            // Arrange
            _http.GetAsync<List<DepartmentIncomeAnimalRes>>(Arg.Any<string>())
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetAnimalIncomeAsync(TestProject, 1, 12);

            // Assert
            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
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
        public async Task GetAdditionalIncomeAsync_HttpThrowsException_ReturnsFailureWithInternalError()
        {
            // Arrange
            _http.GetAsync<List<DepartmentIncomeAdditionalRes>>(Arg.Any<string>())
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetAdditionalIncomeAsync(TestProject, 1, 12);

            // Assert
            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
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
        public async Task GetTotalsAsync_HttpThrowsException_ReturnsFailureWithInternalError()
        {
            // Arrange
            _http.GetAsync<List<DepartmentIncomeTotalsRes>>(Arg.Any<string>())
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetTotalsAsync(TestProject, 1, 12);

            // Assert
            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
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

            // TRANSFORMENGINE: GetPeriodsAsync calls exact base URL with no query params
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
        public async Task GetPeriodsAsync_HttpThrowsException_ReturnsFailureWithInternalError()
        {
            // Arrange
            _http.GetAsync<List<PeriodLookupRes>>(Arg.Any<string>())
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetPeriodsAsync();

            // Assert
            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
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
    }
}
