/*
 * TRANSFORMENGINE MIGRATION — CostBookMaintenanceApiClientTests.cs (FPSApps Infrastructure UnitTests)
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 13 — Unit Tests - Backend + Frontend xUnit Coverage
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New xUnit test class for CostBookMaintenanceApiClient (Infrastructure HTTP client)
 *   - Covers GetSettingsAsync, UpdateSettingsAsync, GetAccountCategoriesAsync, UpdateAccountCategoryAsync
 *   - Tests: HTTP success path, null data path, HTTP failure path, exception/try-catch path, mapper call verification
 *   - URL construction verification: SettingsEndpoint and AccountCategoriesEndpoint with UrlEncode
 *
 * PRESERVED:
 *   - Test naming convention: [MethodName]_[StateUnderTest]_[ExpectedResult]
 *   - Constructor pattern mirrors CostBookCustomerApiClientTests (existing convention)
 *   - Uses NSubstitute + NSubstitute.ExceptionExtensions for all mocks
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: none — fully automated.
 */

using Apha.Common.Contracts;
using Apha.Common.Contracts.Costbook;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Infrastructure.Integrations.CostBookApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.CostBook.CostBookMaintenanceApiClientTest
{
    public class CostBookMaintenanceApiClientTests
    {
        private readonly ICostBookHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly CostBookMaintenanceApiClient _client;

        // TRANSFORMENGINE: Endpoint constants must match the production client exactly
        private const string SettingsEndpoint = "api/v1/maintenance/settings";
        private const string AccountCategoriesEndpoint = "api/v1/maintenance/account-categories";

        public CostBookMaintenanceApiClientTests()
        {
            _http   = Substitute.For<ICostBookHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new CostBookMaintenanceApiClient(_http, _mapper);
        }

        // ── GetSettingsAsync ──────────────────────────────────────────────────

        #region GetSettingsAsync Tests

        [Fact]
        public async Task GetSettingsAsync_HttpReturnsSuccessWithData_ReturnsMappedResponse()
        {
            // Arrange
            var resData = new MaintenanceSettingsRes();
            var apiResponse = new ApiResponse<MaintenanceSettingsRes> { Success = true, Data = resData };
            var expectedDto = ApiResponseDto<MaintenanceSettingsDto>.SuccessResponse(new MaintenanceSettingsDto { InflationAnimals = 2.5m });

            _http.GetAsync<MaintenanceSettingsRes>(SettingsEndpoint).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MaintenanceSettingsDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetSettingsAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2.5m, result.Data?.InflationAnimals);
            await _http.Received(1).GetAsync<MaintenanceSettingsRes>(SettingsEndpoint);
            _mapper.Received(1).Map<ApiResponseDto<MaintenanceSettingsDto>>(apiResponse);
        }

        [Fact]
        public async Task GetSettingsAsync_HttpReturnsSuccessButNullData_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<MaintenanceSettingsRes> { Success = true, Data = null };
            var mappedFailure = new ApiResponseDto<MaintenanceSettingsDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Code = "NO_DATA", Message = "No data returned" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<MaintenanceSettingsRes>(SettingsEndpoint).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MaintenanceSettingsDto>>(apiResponse).Returns(mappedFailure);

            // Act
            var result = await _client.GetSettingsAsync();

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            await _http.Received(1).GetAsync<MaintenanceSettingsRes>(SettingsEndpoint);
        }

        [Fact]
        public async Task GetSettingsAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<MaintenanceSettingsRes>
            {
                Success = false,
                Data = null,
                Errors = new List<ApiError> { new ApiError { Code = "SERVER_ERROR", Message = "Internal server error" } }
            };
            var mappedFailure = new ApiResponseDto<MaintenanceSettingsDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Code = "SERVER_ERROR", Message = "Internal server error" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<MaintenanceSettingsRes>(SettingsEndpoint).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MaintenanceSettingsDto>>(apiResponse).Returns(mappedFailure);

            // Act
            var result = await _client.GetSettingsAsync();

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Equal("SERVER_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetSettingsAsync_HttpExecutorThrowsException_ReturnsInternalErrorFailureResponse()
        {
            // Arrange
            _http.GetAsync<MaintenanceSettingsRes>(SettingsEndpoint)
                 .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetSettingsAsync();

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
            Assert.Equal("Failed to retrieve maintenance settings", result.Errors[0].Message);
        }

        #endregion

        // ── UpdateSettingsAsync ───────────────────────────────────────────────

        #region UpdateSettingsAsync Tests

        [Fact]
        public async Task UpdateSettingsAsync_HttpReturnsSuccessWithData_ReturnsMappedResponse()
        {
            // Arrange
            var dto = new MaintenanceSettingsDto { InflationAnimals = 3.0m, ProfitAnimals = 20m };
            var req = new MaintenanceSettingsReq();
            var resData = new MaintenanceSettingsRes();
            var apiResponse = new ApiResponse<MaintenanceSettingsRes> { Success = true, Data = resData };
            var expectedDto = ApiResponseDto<MaintenanceSettingsDto>.SuccessResponse(dto);

            _mapper.Map<MaintenanceSettingsReq>(dto).Returns(req);
            _http.PutAsync<MaintenanceSettingsReq, MaintenanceSettingsRes>(SettingsEndpoint, req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MaintenanceSettingsDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.UpdateSettingsAsync(dto);

            // Assert
            Assert.True(result.Success);
            await _http.Received(1).PutAsync<MaintenanceSettingsReq, MaintenanceSettingsRes>(SettingsEndpoint, req);
            _mapper.Received(1).Map<ApiResponseDto<MaintenanceSettingsDto>>(apiResponse);
        }

        [Fact]
        public async Task UpdateSettingsAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new MaintenanceSettingsDto { InflationAnimals = 3.0m };
            var req = new MaintenanceSettingsReq();
            var apiResponse = new ApiResponse<MaintenanceSettingsRes>
            {
                Success = false,
                Data = null,
                Errors = new List<ApiError> { new ApiError { Code = "VALIDATION", Message = "Invalid value" } }
            };
            var mappedFailure = new ApiResponseDto<MaintenanceSettingsDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Code = "VALIDATION", Message = "Invalid value" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<MaintenanceSettingsReq>(dto).Returns(req);
            _http.PutAsync<MaintenanceSettingsReq, MaintenanceSettingsRes>(SettingsEndpoint, req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MaintenanceSettingsDto>>(apiResponse).Returns(mappedFailure);

            // Act
            var result = await _client.UpdateSettingsAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task UpdateSettingsAsync_HttpExecutorThrowsException_ReturnsInternalErrorFailureResponse()
        {
            // Arrange
            var dto = new MaintenanceSettingsDto { InflationAnimals = 3.0m };
            var req = new MaintenanceSettingsReq();

            _mapper.Map<MaintenanceSettingsReq>(dto).Returns(req);
            _http.PutAsync<MaintenanceSettingsReq, MaintenanceSettingsRes>(SettingsEndpoint, req)
                 .ThrowsAsync(new Exception("Timeout"));

            // Act
            var result = await _client.UpdateSettingsAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
            Assert.Equal("Failed to update maintenance settings", result.Errors[0].Message);
        }

        [Fact]
        public async Task UpdateSettingsAsync_MapperThrowsExceptionOnDtoToReq_ReturnsInternalErrorFailureResponse()
        {
            // Arrange
            var dto = new MaintenanceSettingsDto { InflationAnimals = 3.0m };
            _mapper.Map<MaintenanceSettingsReq>(dto).Throws(new AutoMapperMappingException("Mapping failed"));

            // Act
            var result = await _client.UpdateSettingsAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        #endregion

        // ── GetAccountCategoriesAsync ─────────────────────────────────────────

        #region GetAccountCategoriesAsync Tests

        [Fact]
        public async Task GetAccountCategoriesAsync_HttpReturnsSuccessWithData_ReturnsMappedResponse()
        {
            // Arrange
            var resList = new List<AccountCategoryMaintenanceRes>
            {
                new AccountCategoryMaintenanceRes { AccShortName = "ACC01" },
                new AccountCategoryMaintenanceRes { AccShortName = "ACC02" }
            };
            var apiResponse = new ApiResponse<List<AccountCategoryMaintenanceRes>> { Success = true, Data = resList };
            var categories = new List<AccountCategoryMaintenanceDto>
            {
                new AccountCategoryMaintenanceDto { AccShortName = "ACC01" },
                new AccountCategoryMaintenanceDto { AccShortName = "ACC02" }
            };
            var expectedDto = ApiResponseDto<List<AccountCategoryMaintenanceDto>>.SuccessResponse(categories);

            _http.GetAsync<List<AccountCategoryMaintenanceRes>>(AccountCategoriesEndpoint).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<AccountCategoryMaintenanceDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAccountCategoriesAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<AccountCategoryMaintenanceRes>>(AccountCategoriesEndpoint);
            _mapper.Received(1).Map<ApiResponseDto<List<AccountCategoryMaintenanceDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAccountCategoriesAsync_HttpReturnsSuccessWithEmptyData_ReturnsMappedEmptyResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<AccountCategoryMaintenanceRes>> { Success = true, Data = new List<AccountCategoryMaintenanceRes>() };
            var expectedDto = ApiResponseDto<List<AccountCategoryMaintenanceDto>>.SuccessResponse(new List<AccountCategoryMaintenanceDto>());

            _http.GetAsync<List<AccountCategoryMaintenanceRes>>(AccountCategoriesEndpoint).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<AccountCategoryMaintenanceDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAccountCategoriesAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetAccountCategoriesAsync_HttpExecutorThrowsException_ReturnsInternalErrorFailureResponse()
        {
            // Arrange
            _http.GetAsync<List<AccountCategoryMaintenanceRes>>(AccountCategoriesEndpoint)
                 .ThrowsAsync(new Exception("Network failure"));

            // Act
            var result = await _client.GetAccountCategoriesAsync();

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
            Assert.Equal("Failed to retrieve account categories", result.Errors[0].Message);
        }

        [Fact]
        public async Task GetAccountCategoriesAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<AccountCategoryMaintenanceRes>>
            {
                Success = false,
                Data = null,
                Errors = new List<ApiError> { new ApiError { Code = "SERVER_ERROR", Message = "Server unavailable" } }
            };
            var mappedFailure = new ApiResponseDto<List<AccountCategoryMaintenanceDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Code = "SERVER_ERROR", Message = "Server unavailable" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<AccountCategoryMaintenanceRes>>(AccountCategoriesEndpoint).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<AccountCategoryMaintenanceDto>>>(apiResponse).Returns(mappedFailure);

            // Act
            var result = await _client.GetAccountCategoriesAsync();

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Equal("SERVER_ERROR", result.Errors[0].Code);
        }

        #endregion

        // ── UpdateAccountCategoryAsync ────────────────────────────────────────

        #region UpdateAccountCategoryAsync Tests

        [Fact]
        public async Task UpdateAccountCategoryAsync_HttpReturnsSuccessWithData_ReturnsMappedResponse()
        {
            // Arrange
            var accShortName = "ACC01";
            var dto = new AccountCategoryMaintenanceDto { AccShortName = accShortName, Csg7Group = "CSG003" };
            var req = new AccountCategoryMaintenanceReq();
            var resData = new AccountCategoryMaintenanceRes { AccShortName = accShortName };
            var expectedUrl = $"{AccountCategoriesEndpoint}/{accShortName}";   // ACC01 has no special chars; UrlEncode leaves it unchanged
            var apiResponse = new ApiResponse<AccountCategoryMaintenanceRes> { Success = true, Data = resData };
            var expectedDto = ApiResponseDto<AccountCategoryMaintenanceDto>.SuccessResponse(dto);

            _mapper.Map<AccountCategoryMaintenanceReq>(dto).Returns(req);
            _http.PutAsync<AccountCategoryMaintenanceReq, AccountCategoryMaintenanceRes>(expectedUrl, req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<AccountCategoryMaintenanceDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.UpdateAccountCategoryAsync(accShortName, dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("CSG003", result.Data?.Csg7Group);
            await _http.Received(1).PutAsync<AccountCategoryMaintenanceReq, AccountCategoryMaintenanceRes>(expectedUrl, req);
            _mapper.Received(1).Map<ApiResponseDto<AccountCategoryMaintenanceDto>>(apiResponse);
        }

        [Fact]
        public async Task UpdateAccountCategoryAsync_AccShortNameWithSpecialChars_UrlEncodesCorrectly()
        {
            // Arrange — accShortName with a space should be URL-encoded to %20
            var accShortName = "ACC 01";
            var dto = new AccountCategoryMaintenanceDto { AccShortName = accShortName, Csg7Group = "CSG001" };
            var req = new AccountCategoryMaintenanceReq();
            var expectedUrl = $"{AccountCategoriesEndpoint}/ACC+01";   // HttpUtility.UrlEncode encodes space as '+'
            var apiResponse = new ApiResponse<AccountCategoryMaintenanceRes> { Success = true, Data = new AccountCategoryMaintenanceRes() };
            var expectedDto = ApiResponseDto<AccountCategoryMaintenanceDto>.SuccessResponse(dto);

            _mapper.Map<AccountCategoryMaintenanceReq>(dto).Returns(req);
            _http.PutAsync<AccountCategoryMaintenanceReq, AccountCategoryMaintenanceRes>(expectedUrl, req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<AccountCategoryMaintenanceDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.UpdateAccountCategoryAsync(accShortName, dto);

            // Assert
            Assert.True(result.Success);
            await _http.Received(1).PutAsync<AccountCategoryMaintenanceReq, AccountCategoryMaintenanceRes>(expectedUrl, req);
        }

        [Fact]
        public async Task UpdateAccountCategoryAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var accShortName = "NOTEXIST";
            var dto = new AccountCategoryMaintenanceDto { AccShortName = accShortName, Csg7Group = "CSG001" };
            var req = new AccountCategoryMaintenanceReq();
            var expectedUrl = $"{AccountCategoriesEndpoint}/{accShortName}";
            var apiResponse = new ApiResponse<AccountCategoryMaintenanceRes>
            {
                Success = false,
                Data = null,
                Errors = new List<ApiError> { new ApiError { Code = "NOT_FOUND", Message = "Not found" } }
            };
            var mappedFailure = new ApiResponseDto<AccountCategoryMaintenanceDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Code = "NOT_FOUND", Message = "Not found" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<AccountCategoryMaintenanceReq>(dto).Returns(req);
            _http.PutAsync<AccountCategoryMaintenanceReq, AccountCategoryMaintenanceRes>(expectedUrl, req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<AccountCategoryMaintenanceDto>>(apiResponse).Returns(mappedFailure);

            // Act
            var result = await _client.UpdateAccountCategoryAsync(accShortName, dto);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Equal("NOT_FOUND", result.Errors[0].Code);
        }

        [Fact]
        public async Task UpdateAccountCategoryAsync_HttpExecutorThrowsException_ReturnsInternalErrorFailureResponse()
        {
            // Arrange
            var accShortName = "ACC01";
            var dto = new AccountCategoryMaintenanceDto { AccShortName = accShortName, Csg7Group = "CSG001" };
            var req = new AccountCategoryMaintenanceReq();
            var expectedUrl = $"{AccountCategoriesEndpoint}/{accShortName}";

            _mapper.Map<AccountCategoryMaintenanceReq>(dto).Returns(req);
            _http.PutAsync<AccountCategoryMaintenanceReq, AccountCategoryMaintenanceRes>(expectedUrl, req)
                 .ThrowsAsync(new Exception("Connection refused"));

            // Act
            var result = await _client.UpdateAccountCategoryAsync(accShortName, dto);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
            Assert.Equal("Failed to update account category", result.Errors[0].Message);
        }

        [Fact]
        public async Task UpdateAccountCategoryAsync_MapperThrowsExceptionOnDtoToReq_ReturnsInternalErrorFailureResponse()
        {
            // Arrange
            var accShortName = "ACC01";
            var dto = new AccountCategoryMaintenanceDto { AccShortName = accShortName, Csg7Group = "CSG001" };
            _mapper.Map<AccountCategoryMaintenanceReq>(dto).Throws(new AutoMapperMappingException("Mapping error"));

            // Act
            var result = await _client.UpdateAccountCategoryAsync(accShortName, dto);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        #endregion
    }
}
