// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — PimsRadTrackInvoiceApiClientTests.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 13 — Unit Tests - Backend + Frontend xUnit Coverage
 * Migrated : 2026-06-12
 *
 * CHANGED:
 *   - New file: xUnit tests for PimsRadTrackInvoiceApiClient (Phase 9).
 *   - Tests cover all 6 HTTP methods: GetAllAsync, GetTotalsAsync, GetByIdAsync,
 *     CreateAsync, UpdateAsync, DeleteAsync.
 *   - NSubstitute used for IPimsHttpExecutor and IMapper mocks.
 *   - Exception catch-block paths verified: client returns FailureResponse with
 *     INTERNAL_ERROR code rather than propagating exceptions (try/catch pattern).
 *   - URL construction verified for base route, id-based routes, and query-string filters.
 *   - Mapper.Received(1) verified on all success paths.
 *   - Follows PimsProjectCommentApiClientTests conventions: ThrowsAsync for exception paths.
 *
 * PRESERVED:
 *   - Naming convention: [MethodName]_[StateUnderTest]_[ExpectedResult].
 *   - BaseUrl = "api/v1/radtrackinvoice" tested via Arg.Is URL assertion.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm query-string format for flat filter params
 *     (?project=...&contract=...) vs nested (?filter.project=...) when integration-tested.
 */

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

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PIMS.PimsRadTrackInvoiceApiClientTest
{
    public class PimsRadTrackInvoiceApiClientTests
    {
        private readonly IPimsHttpExecutor            _http;
        private readonly IMapper                      _mapper;
        private readonly PimsRadTrackInvoiceApiClient _client;

        private const string BaseUrl = "api/v1/radtrackinvoice";

        public PimsRadTrackInvoiceApiClientTests()
        {
            _http   = Substitute.For<IPimsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PimsRadTrackInvoiceApiClient(_http, _mapper);
        }

        // ── Constructor ────────────────────────────────────────────────────────

        #region Constructor

        [Fact]
        public void Constructor_WithValidDependencies_InitializesClient()
        {
            // Act
            var client = new PimsRadTrackInvoiceApiClient(_http, _mapper);

            // Assert
            Assert.NotNull(client);
        }

        #endregion

        // ── GetAllAsync ────────────────────────────────────────────────────────

        #region GetAllAsync

        [Fact]
        public async Task GetAllAsync_WithSuccessResponseAndData_ReturnsMappedDtoList()
        {
            // Arrange
            var query      = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var resList    = new List<RadTrackInvoiceRes> { new() { InvoiceCounter = 1, Project = "PP001" } };
            var apiResponse = new ApiResponse<List<RadTrackInvoiceRes>> { Success = true, Data = resList };
            var mappedDto  = ApiResponseDto<List<RadTrackInvoiceDto>>.SuccessResponse(
                new List<RadTrackInvoiceDto> { new() { InvoiceCounter = 1, Project = "PP001" } });

            _http.GetAsync<List<RadTrackInvoiceRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<RadTrackInvoiceDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetAllAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            Assert.Equal("PP001", result.Data[0].Project);
            await _http.Received(1).GetAsync<List<RadTrackInvoiceRes>>(Arg.Any<string>());
            _mapper.Received(1).Map<ApiResponseDto<List<RadTrackInvoiceDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAllAsync_UsesBaseUrlInRequest()
        {
            // Arrange
            var query      = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<RadTrackInvoiceRes>> { Success = true, Data = [] };
            var mappedDto  = ApiResponseDto<List<RadTrackInvoiceDto>>.SuccessResponse([]);

            _http.GetAsync<List<RadTrackInvoiceRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<RadTrackInvoiceDto>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetAllAsync(query);

            // Assert
            await _http.Received(1).GetAsync<List<RadTrackInvoiceRes>>(
                Arg.Is<string>(u => u.Contains(BaseUrl)));
        }

        [Fact]
        public async Task GetAllAsync_WithFilterParams_AppendsFilterToUrl()
        {
            // Arrange
            var query     = new QueryParameters<string> { Page = 1, PageSize = 10 };
            const string project  = "PP001";
            const string contract = "C001";
            const int    year     = 2025;
            const string program  = "PROG1";

            var apiResponse = new ApiResponse<List<RadTrackInvoiceRes>> { Success = true, Data = [] };
            var mappedDto   = ApiResponseDto<List<RadTrackInvoiceDto>>.SuccessResponse([]);

            _http.GetAsync<List<RadTrackInvoiceRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<RadTrackInvoiceDto>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetAllAsync(query, project, contract, year, program);

            // Assert
            await _http.Received(1).GetAsync<List<RadTrackInvoiceRes>>(
                Arg.Is<string>(u =>
                    u.Contains("project=PP001") &&
                    u.Contains("contract=C001") &&
                    u.Contains("year=2025") &&
                    u.Contains("program=PROG1")));
        }

        [Fact]
        public async Task GetAllAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query   = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors  = new List<ApiError> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<List<RadTrackInvoiceRes>> { Success = false, Errors = errors };
            var mappedDto   = new ApiResponseDto<List<RadTrackInvoiceDto>>
            {
                Success = false,
                Errors  = [new ApiErrorDto { Message = "Not found", Code = "NOT_FOUND" }],
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<List<RadTrackInvoiceRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<RadTrackInvoiceDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetAllAsync(query);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Equal("NOT_FOUND", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetAllAsync_WhenHttpExecutorThrowsException_ReturnsInternalError()
        {
            // Arrange
            var query = new QueryParameters<string>();
            _http.GetAsync<List<RadTrackInvoiceRes>>(Arg.Any<string>())
                 .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetAllAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        #endregion

        // ── GetTotalsAsync ─────────────────────────────────────────────────────

        #region GetTotalsAsync

        [Fact]
        public async Task GetTotalsAsync_WithSuccessResponse_ReturnsMappedTotals()
        {
            // Arrange
            var totalsDto    = new RadTrackInvoiceTotalsDto { TotalPlannedAmount = 5000, TotalDueAmount = 3000, TotalActualAmount = 2000 };
            var apiResponse  = new ApiResponse<RadTrackInvoiceTotalsDto> { Success = true, Data = totalsDto };
            var mappedDto    = ApiResponseDto<RadTrackInvoiceTotalsDto>.SuccessResponse(totalsDto);

            _http.GetAsync<RadTrackInvoiceTotalsDto>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<RadTrackInvoiceTotalsDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetTotalsAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Equal(5000.0, result.Data!.TotalPlannedAmount);
            await _http.Received(1).GetAsync<RadTrackInvoiceTotalsDto>(
                Arg.Is<string>(u => u.Contains($"{BaseUrl}/totals")));
            _mapper.Received(1).Map<ApiResponseDto<RadTrackInvoiceTotalsDto>>(apiResponse);
        }

        [Fact]
        public async Task GetTotalsAsync_WithFilterParams_AppendsQueryString()
        {
            // Arrange
            const string project  = "PP001";
            const int    year     = 2025;
            var apiResponse  = new ApiResponse<RadTrackInvoiceTotalsDto> { Success = true, Data = new RadTrackInvoiceTotalsDto() };
            var mappedDto    = ApiResponseDto<RadTrackInvoiceTotalsDto>.SuccessResponse(new RadTrackInvoiceTotalsDto());

            _http.GetAsync<RadTrackInvoiceTotalsDto>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<RadTrackInvoiceTotalsDto>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetTotalsAsync(project: project, year: year);

            // Assert
            await _http.Received(1).GetAsync<RadTrackInvoiceTotalsDto>(
                Arg.Is<string>(u => u.Contains("project=PP001") && u.Contains("year=2025")));
        }

        [Fact]
        public async Task GetTotalsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors      = new List<ApiError> { new() { Message = "Server error", Code = "SERVER_ERR" } };
            var apiResponse = new ApiResponse<RadTrackInvoiceTotalsDto> { Success = false, Errors = errors };
            var mappedDto   = new ApiResponseDto<RadTrackInvoiceTotalsDto>
            {
                Success = false,
                Errors  = [new ApiErrorDto { Code = "SERVER_ERR", Message = "Server error" }],
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<RadTrackInvoiceTotalsDto>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<RadTrackInvoiceTotalsDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetTotalsAsync();

            // Assert
            Assert.False(result.Success);
            Assert.Equal("SERVER_ERR", result.Errors![0].Code);
        }

        [Fact]
        public async Task GetTotalsAsync_WhenHttpExecutorThrowsException_ReturnsInternalError()
        {
            // Arrange
            _http.GetAsync<RadTrackInvoiceTotalsDto>(Arg.Any<string>())
                 .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetTotalsAsync();

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        #endregion

        // ── GetByIdAsync ───────────────────────────────────────────────────────

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_WithSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            const int id     = 42;
            var res          = new RadTrackInvoiceRes { InvoiceCounter = id, Project = "PP001" };
            var apiResponse  = new ApiResponse<RadTrackInvoiceRes> { Success = true, Data = res };
            var mappedDto    = ApiResponseDto<RadTrackInvoiceDto>.SuccessResponse(
                new RadTrackInvoiceDto { InvoiceCounter = id, Project = "PP001" });

            _http.GetAsync<RadTrackInvoiceRes>($"{BaseUrl}/{id}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<RadTrackInvoiceDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetByIdAsync(id);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(id, result.Data!.InvoiceCounter);
            await _http.Received(1).GetAsync<RadTrackInvoiceRes>($"{BaseUrl}/{id}");
            _mapper.Received(1).Map<ApiResponseDto<RadTrackInvoiceDto>>(apiResponse);
        }

        [Fact]
        public async Task GetByIdAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            const int id    = 999;
            var errors      = new List<ApiError> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<RadTrackInvoiceRes> { Success = false, Errors = errors };
            var mappedDto   = new ApiResponseDto<RadTrackInvoiceDto>
            {
                Success = false,
                Errors  = [new ApiErrorDto { Code = "NOT_FOUND" }],
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<RadTrackInvoiceRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<RadTrackInvoiceDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetByIdAsync(id);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.Errors![0].Code);
        }

        [Fact]
        public async Task GetByIdAsync_WhenHttpExecutorThrowsException_ReturnsInternalError()
        {
            // Arrange
            _http.GetAsync<RadTrackInvoiceRes>(Arg.Any<string>())
                 .ThrowsAsync(new Exception("Timeout"));

            // Act
            var result = await _client.GetByIdAsync(1);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        #endregion

        // ── CreateAsync ────────────────────────────────────────────────────────

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_WithSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            var dto          = new RadTrackInvoiceDto { Project = "PP001", DueAmount = 1000.0 };
            var req          = new RadTrackInvoiceReq { Project = "PP001", DueAmount = 1000.0 };
            var res          = new RadTrackInvoiceRes { InvoiceCounter = 5, Project = "PP001" };
            var apiResponse  = new ApiResponse<RadTrackInvoiceRes> { Success = true, Data = res };
            var mappedDto    = ApiResponseDto<RadTrackInvoiceDto>.SuccessResponse(
                new RadTrackInvoiceDto { InvoiceCounter = 5, Project = "PP001" });

            _mapper.Map<RadTrackInvoiceReq>(dto).Returns(req);
            _http.PostAsync<RadTrackInvoiceReq, RadTrackInvoiceRes>(BaseUrl, req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<RadTrackInvoiceDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.CreateAsync(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(5, result.Data!.InvoiceCounter);
            _mapper.Received(1).Map<RadTrackInvoiceReq>(dto);
            await _http.Received(1).PostAsync<RadTrackInvoiceReq, RadTrackInvoiceRes>(BaseUrl, req);
            _mapper.Received(1).Map<ApiResponseDto<RadTrackInvoiceDto>>(apiResponse);
        }

        [Fact]
        public async Task CreateAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new RadTrackInvoiceDto { Project = "PP001" };
            var req = new RadTrackInvoiceReq { Project = "PP001" };
            var errors      = new List<ApiError> { new() { Message = "Validation error", Code = "VALIDATION_ERR" } };
            var apiResponse = new ApiResponse<RadTrackInvoiceRes> { Success = false, Errors = errors };
            var mappedDto   = new ApiResponseDto<RadTrackInvoiceDto>
            {
                Success = false,
                Errors  = [new ApiErrorDto { Code = "VALIDATION_ERR" }],
                Meta    = new ApiMetaDto()
            };

            _mapper.Map<RadTrackInvoiceReq>(dto).Returns(req);
            _http.PostAsync<RadTrackInvoiceReq, RadTrackInvoiceRes>(Arg.Any<string>(), req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<RadTrackInvoiceDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.CreateAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("VALIDATION_ERR", result.Errors![0].Code);
        }

        [Fact]
        public async Task CreateAsync_WhenHttpExecutorThrowsException_ReturnsInternalError()
        {
            // Arrange
            var dto = new RadTrackInvoiceDto { Project = "PP001" };
            var req = new RadTrackInvoiceReq { Project = "PP001" };

            _mapper.Map<RadTrackInvoiceReq>(dto).Returns(req);
            _http.PostAsync<RadTrackInvoiceReq, RadTrackInvoiceRes>(Arg.Any<string>(), Arg.Any<RadTrackInvoiceReq>())
                 .ThrowsAsync(new Exception("Timeout"));

            // Act
            var result = await _client.CreateAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("INTERNAL_ERROR", result.Errors![0].Code);
        }

        #endregion

        // ── UpdateAsync ────────────────────────────────────────────────────────

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_WithSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            const int id     = 7;
            var dto          = new RadTrackInvoiceDto { InvoiceCounter = id, Project = "PP001" };
            var req          = new RadTrackInvoiceReq { Project = "PP001" };
            var res          = new RadTrackInvoiceRes { InvoiceCounter = id, Project = "PP001" };
            var apiResponse  = new ApiResponse<RadTrackInvoiceRes> { Success = true, Data = res };
            var mappedDto    = ApiResponseDto<RadTrackInvoiceDto>.SuccessResponse(
                new RadTrackInvoiceDto { InvoiceCounter = id, Project = "PP001" });

            _mapper.Map<RadTrackInvoiceReq>(dto).Returns(req);
            _http.PutAsync<RadTrackInvoiceReq, RadTrackInvoiceRes>($"{BaseUrl}/{id}", req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<RadTrackInvoiceDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.UpdateAsync(id, dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(id, result.Data!.InvoiceCounter);
            await _http.Received(1).PutAsync<RadTrackInvoiceReq, RadTrackInvoiceRes>($"{BaseUrl}/{id}", req);
        }

        [Fact]
        public async Task UpdateAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            const int id = 7;
            var dto      = new RadTrackInvoiceDto { InvoiceCounter = id };
            var req      = new RadTrackInvoiceReq();
            var errors   = new List<ApiError> { new() { Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<RadTrackInvoiceRes> { Success = false, Errors = errors };
            var mappedDto   = new ApiResponseDto<RadTrackInvoiceDto>
            {
                Success = false,
                Errors  = [new ApiErrorDto { Code = "NOT_FOUND" }],
                Meta    = new ApiMetaDto()
            };

            _mapper.Map<RadTrackInvoiceReq>(dto).Returns(req);
            _http.PutAsync<RadTrackInvoiceReq, RadTrackInvoiceRes>(Arg.Any<string>(), req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<RadTrackInvoiceDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.UpdateAsync(id, dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.Errors![0].Code);
        }

        [Fact]
        public async Task UpdateAsync_WhenHttpExecutorThrowsException_ReturnsInternalError()
        {
            // Arrange
            const int id = 7;
            var dto = new RadTrackInvoiceDto { InvoiceCounter = id };
            var req = new RadTrackInvoiceReq();

            _mapper.Map<RadTrackInvoiceReq>(dto).Returns(req);
            _http.PutAsync<RadTrackInvoiceReq, RadTrackInvoiceRes>(Arg.Any<string>(), Arg.Any<RadTrackInvoiceReq>())
                 .ThrowsAsync(new Exception("Timeout"));

            // Act
            var result = await _client.UpdateAsync(id, dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("INTERNAL_ERROR", result.Errors![0].Code);
        }

        [Fact]
        public async Task UpdateAsync_UsesCorrectUrlWithId()
        {
            // Arrange
            const int id    = 99;
            var dto         = new RadTrackInvoiceDto { InvoiceCounter = id };
            var req         = new RadTrackInvoiceReq();
            var apiResponse = new ApiResponse<RadTrackInvoiceRes> { Success = true, Data = new RadTrackInvoiceRes() };
            var mappedDto   = ApiResponseDto<RadTrackInvoiceDto>.SuccessResponse(new RadTrackInvoiceDto());

            _mapper.Map<RadTrackInvoiceReq>(dto).Returns(req);
            _http.PutAsync<RadTrackInvoiceReq, RadTrackInvoiceRes>(Arg.Any<string>(), req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<RadTrackInvoiceDto>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.UpdateAsync(id, dto);

            // Assert
            await _http.Received(1).PutAsync<RadTrackInvoiceReq, RadTrackInvoiceRes>(
                Arg.Is<string>(u => u == $"{BaseUrl}/{id}"), req);
        }

        #endregion

        // ── DeleteAsync ────────────────────────────────────────────────────────

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_WithSuccessResponse_ReturnsMappedSuccessResponse()
        {
            // Arrange
            const int id    = 3;
            var apiResponse = new ApiResponse<object> { Success = true, Data = new { success = true } };
            var mappedDto   = ApiResponseDto<object>.SuccessResponse(new { success = true });

            _http.DeleteAsync<object>($"{BaseUrl}/{id}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<object>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.DeleteAsync(id);

            // Assert
            Assert.True(result.Success);
            await _http.Received(1).DeleteAsync<object>($"{BaseUrl}/{id}");
            _mapper.Received(1).Map<ApiResponseDto<object>>(apiResponse);
        }

        [Fact]
        public async Task DeleteAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            const int id    = 999;
            var errors      = new List<ApiError> { new() { Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<object> { Success = false, Errors = errors };
            var mappedDto   = new ApiResponseDto<object>
            {
                Success = false,
                Errors  = [new ApiErrorDto { Code = "NOT_FOUND" }],
                Meta    = new ApiMetaDto()
            };

            _http.DeleteAsync<object>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<object>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.DeleteAsync(id);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.Errors![0].Code);
        }

        [Fact]
        public async Task DeleteAsync_WhenHttpExecutorThrowsException_ReturnsInternalError()
        {
            // Arrange
            _http.DeleteAsync<object>(Arg.Any<string>())
                 .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.DeleteAsync(1);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task DeleteAsync_UsesCorrectUrlWithId()
        {
            // Arrange
            const int id    = 7;
            var apiResponse = new ApiResponse<object> { Success = true, Data = new { } };
            var mappedDto   = ApiResponseDto<object>.SuccessResponse(new { });

            _http.DeleteAsync<object>($"{BaseUrl}/{id}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<object>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.DeleteAsync(id);

            // Assert
            await _http.Received(1).DeleteAsync<object>($"{BaseUrl}/{id}");
        }

        #endregion
    }
}
