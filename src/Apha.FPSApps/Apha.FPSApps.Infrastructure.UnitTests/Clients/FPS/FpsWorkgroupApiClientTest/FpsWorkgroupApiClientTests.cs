/*
 * TRANSFORMENGINE MIGRATION — FpsWorkgroupApiClientTests.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 13 — Unit Tests - Backend + Frontend xUnit Coverage
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - NEW FILE: xUnit tests for FpsWorkgroupApiClient (frmMaintWorkGroup2 infrastructure HTTP client)
 *   - Tests cover all 8 public methods:
 *       GetPagedAsync, GetByWorkGroupNameAsync, CreateAsync, UpdateAsync, DeleteAsync,
 *       GetProfitCentresAsync, GetOwnersAsync, GetCostCentresAsync
 *   - Uses NSubstitute for IFpsHttpExecutor and IMapper mocks
 *   - Scenarios per method: HTTP success + mapper call, HTTP failure + mapped error, exception + FailureResponse
 *   - URL construction verified via Arg.Is<string>(s => s.Contains(...)) assertions
 *
 * PRESERVED:
 *   - Pattern matches PimsProjectApiClientTests — NSubstitute HTTP executor + mapper approach
 *   - Naming convention [MethodName]_[StateUnderTest]_[ExpectedResult]
 *   - #region grouping per method
 *   - InternalCodeError = "INTERNAL_ERROR" verified on catch paths
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: GetCostCentresAsync URL construction includes Uri.EscapeDataString(profitCentre);
 *     verify that encoded URL comparison works correctly with special characters in profitCentre values
 */

using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsWorkgroupApiClientTest
{
    public class FpsWorkgroupApiClientTests
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper          _mapper;
        private readonly FpsWorkgroupApiClient _client;

        private const string BaseUrl = "api/v1/workgroup";

        public FpsWorkgroupApiClientTests()
        {
            _http   = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsWorkgroupApiClient(_http, _mapper);
        }

        // TRANSFORMENGINE: static helpers — minimal valid objects for test setup
        private static WorkgroupMaintenanceDto BuildDto(string name = "WG001") =>
            new() { WorkGroupName = name, ProfitCentre = "PC01" };

        private static WorkgroupMaintenanceRes BuildRes(string name = "WG001") =>
            new() { WorkGroupName = name, ProfitCentre = "PC01" };

        private static WorkgroupMaintenanceReq BuildReq(string name = "WG001") =>
            new() { WorkGroupName = name, ProfitCentre = "PC01" };

        private static ApiResponse<T> SuccessApiResponse<T>(T data) =>
            new() { Success = true, Data = data };

        private static ApiResponse<T> FailureApiResponse<T>() =>
            new() { Success = false, Data = default, Errors = new List<ApiError> { new() { Code = "ERR", Message = "Error" } } };

        private static ApiResponseDto<T> MappedSuccessDto<T>(T data) =>
            new() { Success = true, Data = data };

        private static ApiResponseDto<T> MappedFailureDto<T>() =>
            new()
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Code = "ERR", Message = "Error" } },
                Meta    = new ApiMetaDto()
            };

        #region Constructor Tests

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenHttpExecutorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new FpsWorkgroupApiClient(null!, _mapper));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenMapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new FpsWorkgroupApiClient(_http, null!));
        }

        #endregion

        #region GetPagedAsync Tests

        [Fact]
        public async Task GetPagedAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var query       = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var httpResponse = SuccessApiResponse(new List<WorkgroupMaintenanceRes> { BuildRes() });
            var expectedDto  = MappedSuccessDto(new List<WorkgroupMaintenanceDto> { BuildDto() });

            _http.GetAsync<List<WorkgroupMaintenanceRes>>(Arg.Any<string>()).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<WorkgroupMaintenanceDto>>>(httpResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedAsync(query);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            _mapper.Received(1).Map<ApiResponseDto<List<WorkgroupMaintenanceDto>>>(httpResponse);
        }

        [Fact]
        public async Task GetPagedAsync_HttpReturnsFailure_ReturnsMappedFailureResponse()
        {
            // Arrange
            var query        = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var httpResponse = FailureApiResponse<List<WorkgroupMaintenanceRes>>();
            var mappedDto    = MappedFailureDto<List<WorkgroupMaintenanceDto>>();

            _http.GetAsync<List<WorkgroupMaintenanceRes>>(Arg.Any<string>()).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<WorkgroupMaintenanceDto>>>(httpResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetPagedAsync(query);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetPagedAsync_HttpThrowsException_ReturnsFailureResponseWithInternalError()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _http.GetAsync<List<WorkgroupMaintenanceRes>>(Arg.Any<string>())
                 .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetPagedAsync(query);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Contains(result.Errors, e => e.Code == "INTERNAL_ERROR");
        }

        [Fact]
        public async Task GetPagedAsync_UrlContainsPagedEndpoint()
        {
            // Arrange
            var query        = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var httpResponse = SuccessApiResponse(new List<WorkgroupMaintenanceRes>());
            var mappedDto    = MappedSuccessDto(new List<WorkgroupMaintenanceDto>());

            _http.GetAsync<List<WorkgroupMaintenanceRes>>(Arg.Any<string>()).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<WorkgroupMaintenanceDto>>>(httpResponse).Returns(mappedDto);

            // Act
            await _client.GetPagedAsync(query);

            // Assert — URL must include paged segment and base path
            await _http.Received(1).GetAsync<List<WorkgroupMaintenanceRes>>(
                Arg.Is<string>(s => s.Contains($"{BaseUrl}/paged")));
        }

        #endregion

        #region GetByWorkGroupNameAsync Tests

        [Fact]
        public async Task GetByWorkGroupNameAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var httpResponse = SuccessApiResponse(BuildRes("WG001"));
            var expectedDto  = MappedSuccessDto(BuildDto("WG001"));

            _http.GetAsync<WorkgroupMaintenanceRes>(Arg.Any<string>()).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<WorkgroupMaintenanceDto>>(httpResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetByWorkGroupNameAsync("WG001");

            // Assert
            Assert.True(result.Success);
            _mapper.Received(1).Map<ApiResponseDto<WorkgroupMaintenanceDto>>(httpResponse);
        }

        [Fact]
        public async Task GetByWorkGroupNameAsync_HttpReturnsFailure_ReturnsMappedFailure()
        {
            // Arrange
            var httpResponse = FailureApiResponse<WorkgroupMaintenanceRes>();
            var mappedDto    = MappedFailureDto<WorkgroupMaintenanceDto>();

            _http.GetAsync<WorkgroupMaintenanceRes>(Arg.Any<string>()).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<WorkgroupMaintenanceDto>>(httpResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetByWorkGroupNameAsync("NOTEXIST");

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetByWorkGroupNameAsync_HttpThrowsException_ReturnsFailureResponseWithInternalError()
        {
            // Arrange
            _http.GetAsync<WorkgroupMaintenanceRes>(Arg.Any<string>())
                 .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetByWorkGroupNameAsync("WG001");

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Contains(result.Errors, e => e.Code == "INTERNAL_ERROR");
        }

        [Fact]
        public async Task GetByWorkGroupNameAsync_UrlContainsWorkGroupName()
        {
            // Arrange
            var httpResponse = SuccessApiResponse(BuildRes("WG001"));
            var mappedDto    = MappedSuccessDto(BuildDto("WG001"));

            _http.GetAsync<WorkgroupMaintenanceRes>(Arg.Any<string>()).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<WorkgroupMaintenanceDto>>(httpResponse).Returns(mappedDto);

            // Act
            await _client.GetByWorkGroupNameAsync("WG001");

            // Assert
            await _http.Received(1).GetAsync<WorkgroupMaintenanceRes>(
                Arg.Is<string>(s => s.Contains("WG001")));
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var dto          = BuildDto();
            var req          = BuildReq();
            var httpResponse = SuccessApiResponse(BuildRes());
            var expectedDto  = MappedSuccessDto(BuildDto());

            _mapper.Map<WorkgroupMaintenanceReq>(dto).Returns(req);
            _http.PostAsync<WorkgroupMaintenanceReq, WorkgroupMaintenanceRes>(Arg.Any<string>(), req).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<WorkgroupMaintenanceDto>>(httpResponse).Returns(expectedDto);

            // Act
            var result = await _client.CreateAsync(dto);

            // Assert
            Assert.True(result.Success);
            _mapper.Received(1).Map<ApiResponseDto<WorkgroupMaintenanceDto>>(httpResponse);
        }

        [Fact]
        public async Task CreateAsync_HttpReturnsFailure_ReturnsMappedFailure()
        {
            // Arrange
            var dto          = BuildDto();
            var req          = BuildReq();
            var httpResponse = FailureApiResponse<WorkgroupMaintenanceRes>();
            var mappedDto    = MappedFailureDto<WorkgroupMaintenanceDto>();

            _mapper.Map<WorkgroupMaintenanceReq>(dto).Returns(req);
            _http.PostAsync<WorkgroupMaintenanceReq, WorkgroupMaintenanceRes>(Arg.Any<string>(), req).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<WorkgroupMaintenanceDto>>(httpResponse).Returns(mappedDto);

            // Act
            var result = await _client.CreateAsync(dto);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task CreateAsync_HttpThrowsException_ReturnsFailureResponseWithInternalError()
        {
            // Arrange
            var dto = BuildDto();
            var req = BuildReq();

            _mapper.Map<WorkgroupMaintenanceReq>(dto).Returns(req);
            _http.PostAsync<WorkgroupMaintenanceReq, WorkgroupMaintenanceRes>(Arg.Any<string>(), req)
                 .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.CreateAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Contains(result.Errors, e => e.Code == "INTERNAL_ERROR");
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var dto          = BuildDto("WG001");
            var req          = BuildReq("WG001");
            var httpResponse = SuccessApiResponse(BuildRes("WG001"));
            var expectedDto  = MappedSuccessDto(BuildDto("WG001"));

            _mapper.Map<WorkgroupMaintenanceReq>(dto).Returns(req);
            _http.PutAsync<WorkgroupMaintenanceReq, WorkgroupMaintenanceRes>(Arg.Any<string>(), req).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<WorkgroupMaintenanceDto>>(httpResponse).Returns(expectedDto);

            // Act
            var result = await _client.UpdateAsync("WG001", dto);

            // Assert
            Assert.True(result.Success);
            _mapper.Received(1).Map<ApiResponseDto<WorkgroupMaintenanceDto>>(httpResponse);
        }

        [Fact]
        public async Task UpdateAsync_HttpReturnsFailure_ReturnsMappedFailure()
        {
            // Arrange
            var dto          = BuildDto();
            var req          = BuildReq();
            var httpResponse = FailureApiResponse<WorkgroupMaintenanceRes>();
            var mappedDto    = MappedFailureDto<WorkgroupMaintenanceDto>();

            _mapper.Map<WorkgroupMaintenanceReq>(dto).Returns(req);
            _http.PutAsync<WorkgroupMaintenanceReq, WorkgroupMaintenanceRes>(Arg.Any<string>(), req).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<WorkgroupMaintenanceDto>>(httpResponse).Returns(mappedDto);

            // Act
            var result = await _client.UpdateAsync("WG001", dto);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task UpdateAsync_HttpThrowsException_ReturnsFailureResponseWithInternalError()
        {
            // Arrange
            var dto = BuildDto();
            var req = BuildReq();

            _mapper.Map<WorkgroupMaintenanceReq>(dto).Returns(req);
            _http.PutAsync<WorkgroupMaintenanceReq, WorkgroupMaintenanceRes>(Arg.Any<string>(), req)
                 .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.UpdateAsync("WG001", dto);

            // Assert
            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        [Fact]
        public async Task UpdateAsync_UrlContainsOriginalWorkGroupName()
        {
            // Arrange
            var dto          = BuildDto("WG_RENAMED");
            var req          = BuildReq("WG_RENAMED");
            var httpResponse = SuccessApiResponse(BuildRes("WG_RENAMED"));
            var mappedDto    = MappedSuccessDto(BuildDto("WG_RENAMED"));

            _mapper.Map<WorkgroupMaintenanceReq>(dto).Returns(req);
            _http.PutAsync<WorkgroupMaintenanceReq, WorkgroupMaintenanceRes>(Arg.Any<string>(), req).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<WorkgroupMaintenanceDto>>(httpResponse).Returns(mappedDto);

            // Act
            await _client.UpdateAsync("WG_ORIGINAL", dto);

            // Assert — URL must include the original key, not the renamed value
            await _http.Received(1).PutAsync<WorkgroupMaintenanceReq, WorkgroupMaintenanceRes>(
                Arg.Is<string>(s => s.Contains("WG_ORIGINAL")), req);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var httpResponse = SuccessApiResponse<bool?>(true);
            var expectedDto  = new ApiResponseDto<bool> { Success = true };

            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<bool>>(httpResponse).Returns(expectedDto);

            // Act
            var result = await _client.DeleteAsync("WG001");

            // Assert
            Assert.True(result.Success);
            _mapper.Received(1).Map<ApiResponseDto<bool>>(httpResponse);
        }

        [Fact]
        public async Task DeleteAsync_HttpReturnsFailure_ReturnsMappedFailure()
        {
            // Arrange
            var httpResponse = FailureApiResponse<bool?>();
            var mappedDto    = new ApiResponseDto<bool>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Code = "ERR", Message = "Error" } },
                Meta    = new ApiMetaDto()
            };

            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<bool>>(httpResponse).Returns(mappedDto);

            // Act
            var result = await _client.DeleteAsync("WG001");

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task DeleteAsync_HttpThrowsException_ReturnsFailureResponseWithInternalError()
        {
            // Arrange
            _http.DeleteAsync<bool?>(Arg.Any<string>())
                 .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.DeleteAsync("WG001");

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Contains(result.Errors, e => e.Code == "INTERNAL_ERROR");
        }

        [Fact]
        public async Task DeleteAsync_UrlContainsWorkGroupName()
        {
            // Arrange
            var httpResponse = SuccessApiResponse<bool?>(true);
            var mappedDto    = new ApiResponseDto<bool> { Success = true };

            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<bool>>(httpResponse).Returns(mappedDto);

            // Act
            await _client.DeleteAsync("WG001");

            // Assert
            await _http.Received(1).DeleteAsync<bool?>(
                Arg.Is<string>(s => s.Contains("WG001")));
        }

        #endregion

        #region GetProfitCentresAsync Tests

        [Fact]
        public async Task GetProfitCentresAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var httpResponse = SuccessApiResponse(new List<string> { "PC01", "PC02" });
            var expectedDto  = MappedSuccessDto(new List<string> { "PC01", "PC02" });

            _http.GetAsync<List<string>>(Arg.Any<string>()).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<string>>>(httpResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetProfitCentresAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            _mapper.Received(1).Map<ApiResponseDto<List<string>>>(httpResponse);
        }

        [Fact]
        public async Task GetProfitCentresAsync_HttpThrowsException_ReturnsFailureResponseWithInternalError()
        {
            // Arrange
            _http.GetAsync<List<string>>(Arg.Any<string>())
                 .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetProfitCentresAsync();

            // Assert
            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        [Fact]
        public async Task GetProfitCentresAsync_UrlContainsProfitCentresEndpoint()
        {
            // Arrange
            var httpResponse = SuccessApiResponse(new List<string>());
            var mappedDto    = MappedSuccessDto(new List<string>());

            _http.GetAsync<List<string>>(Arg.Any<string>()).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<string>>>(httpResponse).Returns(mappedDto);

            // Act
            await _client.GetProfitCentresAsync();

            // Assert
            await _http.Received(1).GetAsync<List<string>>(
                Arg.Is<string>(s => s.Contains("profitcentres")));
        }

        #endregion

        #region GetOwnersAsync Tests

        [Fact]
        public async Task GetOwnersAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var httpResponse = SuccessApiResponse(new List<ManagerRes> { new() { Name = "Alice Smith" } });
            var expectedDto  = MappedSuccessDto(new List<ManagerDto> { new() { Name = "Alice Smith" } });

            _http.GetAsync<List<ManagerRes>>(Arg.Any<string>()).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<ManagerDto>>>(httpResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetOwnersAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            _mapper.Received(1).Map<ApiResponseDto<List<ManagerDto>>>(httpResponse);
        }

        [Fact]
        public async Task GetOwnersAsync_HttpThrowsException_ReturnsFailureResponseWithInternalError()
        {
            // Arrange
            _http.GetAsync<List<ManagerRes>>(Arg.Any<string>())
                 .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetOwnersAsync();

            // Assert
            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        [Fact]
        public async Task GetOwnersAsync_UrlContainsOwnersEndpoint()
        {
            // Arrange
            var httpResponse = SuccessApiResponse(new List<ManagerRes>());
            var mappedDto    = MappedSuccessDto(new List<ManagerDto>());

            _http.GetAsync<List<ManagerRes>>(Arg.Any<string>()).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<ManagerDto>>>(httpResponse).Returns(mappedDto);

            // Act
            await _client.GetOwnersAsync();

            // Assert
            await _http.Received(1).GetAsync<List<ManagerRes>>(
                Arg.Is<string>(s => s.Contains("owners")));
        }

        #endregion

        #region GetCostCentresAsync Tests

        [Fact]
        public async Task GetCostCentresAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var httpResponse = SuccessApiResponse(new List<double?> { 100.0, 200.0 });
            var expectedDto  = MappedSuccessDto(new List<double?> { 100.0, 200.0 });

            _http.GetAsync<List<double?>>(Arg.Any<string>()).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<double?>>>(httpResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetCostCentresAsync("PC01");

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            _mapper.Received(1).Map<ApiResponseDto<List<double?>>>(httpResponse);
        }

        [Fact]
        public async Task GetCostCentresAsync_HttpReturnsFailure_ReturnsMappedFailure()
        {
            // Arrange
            var httpResponse = FailureApiResponse<List<double?>>();
            var mappedDto    = MappedFailureDto<List<double?>>();

            _http.GetAsync<List<double?>>(Arg.Any<string>()).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<double?>>>(httpResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetCostCentresAsync("PC01");

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetCostCentresAsync_HttpThrowsException_ReturnsFailureResponseWithInternalError()
        {
            // Arrange
            _http.GetAsync<List<double?>>(Arg.Any<string>())
                 .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetCostCentresAsync("PC01");

            // Assert
            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        [Fact]
        public async Task GetCostCentresAsync_UrlContainsCostCentresEndpointAndProfitCentreParam()
        {
            // Arrange
            var httpResponse = SuccessApiResponse(new List<double?>());
            var mappedDto    = MappedSuccessDto(new List<double?>());

            _http.GetAsync<List<double?>>(Arg.Any<string>()).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<double?>>>(httpResponse).Returns(mappedDto);

            // Act
            await _client.GetCostCentresAsync("PC01");

            // Assert
            await _http.Received(1).GetAsync<List<double?>>(
                Arg.Is<string>(s => s.Contains("costcentres") && s.Contains("profitCentre")));
        }

        #endregion
    }
}
