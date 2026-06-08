using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsPurchasesApiClientTest
{
    public class FpsPurchasesApiClientTests
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly FpsPurchasesApiClient _client;

        public FpsPurchasesApiClientTests()
        {
            _http   = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsPurchasesApiClient(_http, _mapper);
        }

        #region GetPurchasesAsync Tests

        [Fact]
        public async Task GetPurchasesAsync_WithSuccessResponse_ReturnsPurchases()
        {
            // Arrange
            var res = new List<PurchaseRes> { new() { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m } };
            var apiResponse = new ApiResponse<List<PurchaseRes>> { Success = true, Data = res };
            var expectedDto = ApiResponseDto<List<PurchaseDto>>.SuccessResponse(new List<PurchaseDto>
            {
                new() { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m }
            });

            _http.GetAsync<List<PurchaseRes>>(Arg.Is<string>(url => url.Contains("purchases")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<PurchaseDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPurchasesAsync("WG01", "ACC1");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
        }

        [Fact]
        public async Task GetPurchasesAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<PurchaseRes>> { Success = true, Data = new List<PurchaseRes>() };
            var expectedDto = ApiResponseDto<List<PurchaseDto>>.SuccessResponse(new List<PurchaseDto>());

            _http.GetAsync<List<PurchaseRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<PurchaseDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPurchasesAsync("WG01", "ACC1");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetPurchasesAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<PurchaseRes>>
            {
                Success = false,
                Errors  = new List<ApiError> { new() { Message = "Error", Code = "ERR" } }
            };
            var mappedResponse = new ApiResponseDto<List<PurchaseDto>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERR" } },
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<List<PurchaseRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<PurchaseDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetPurchasesAsync("WG01", "ACC1");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        #endregion

        #region GetPurchaseByIdAsync Tests

        [Fact]
        public async Task GetPurchaseByIdAsync_WithSuccessResponse_ReturnsPurchase()
        {
            // Arrange
            var res = new PurchaseRes { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m };
            var apiResponse = new ApiResponse<PurchaseRes> { Success = true, Data = res };
            var expectedDto = ApiResponseDto<PurchaseDto>.SuccessResponse(
                new PurchaseDto { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m });

            _http.GetAsync<PurchaseRes>(Arg.Is<string>(url => url.Contains("WG01") && url.Contains("ACC1") && url.Contains("Item")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<PurchaseDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPurchaseByIdAsync("WG01", "ACC1", "Item A");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("Item A", result.Data?.ItemDescription);
        }

        [Fact]
        public async Task GetPurchaseByIdAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<PurchaseRes>
            {
                Success = false,
                Errors  = new List<ApiError> { new() { Message = "Not found", Code = "NOT_FOUND" } }
            };
            var mappedResponse = new ApiResponseDto<PurchaseDto>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } },
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<PurchaseRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<PurchaseDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetPurchaseByIdAsync("WG01", "ACC1", "NOTEXIST");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region CreatePurchaseAsync Tests

        [Fact]
        public async Task CreatePurchaseAsync_WithSuccessResponse_ReturnsPurchase()
        {
            // Arrange
            var dto = new PurchaseDto { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m };
            var req = new PurchaseReq { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m };
            var res = new PurchaseRes { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m };
            var apiResponse = new ApiResponse<PurchaseRes> { Success = true, Data = res };
            var expectedDto = ApiResponseDto<PurchaseDto>.SuccessResponse(dto);

            _mapper.Map<PurchaseReq>(dto).Returns(req);
            _http.PostAsync<PurchaseReq, PurchaseRes>(Arg.Any<string>(), req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<PurchaseDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.CreatePurchaseAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
        }

        [Fact]
        public async Task CreatePurchaseAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new PurchaseDto { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m };
            var req = new PurchaseReq { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m };
            var apiResponse = new ApiResponse<PurchaseRes>
            {
                Success = false,
                Errors  = new List<ApiError> { new() { Message = "Already exists", Code = "CONFLICT" } }
            };
            var mappedResponse = new ApiResponseDto<PurchaseDto>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Already exists", Code = "CONFLICT" } },
                Meta    = new ApiMetaDto()
            };

            _mapper.Map<PurchaseReq>(dto).Returns(req);
            _http.PostAsync<PurchaseReq, PurchaseRes>(Arg.Any<string>(), req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<PurchaseDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.CreatePurchaseAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region UpdatePurchaseAsync Tests

        [Fact]
        public async Task UpdatePurchaseAsync_WithSuccessResponse_ReturnsPurchase()
        {
            // Arrange
            var dto = new PurchaseDto { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item B", Amount = 200m, OldItemDescription = "Item A" };
            var req = new PurchaseReq { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item B", Amount = 200m, OldItemDescription = "Item A" };
            var res = new PurchaseRes { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item B", Amount = 200m };
            var apiResponse = new ApiResponse<PurchaseRes> { Success = true, Data = res };
            var expectedDto = ApiResponseDto<PurchaseDto>.SuccessResponse(dto);

            _mapper.Map<PurchaseReq>(dto).Returns(req);
            _http.PutAsync<PurchaseReq, PurchaseRes>(Arg.Any<string>(), req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<PurchaseDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.UpdatePurchaseAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
        }

        [Fact]
        public async Task UpdatePurchaseAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new PurchaseDto { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item B", Amount = 200m };
            var req = new PurchaseReq { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item B", Amount = 200m };
            var apiResponse = new ApiResponse<PurchaseRes>
            {
                Success = false,
                Errors  = new List<ApiError> { new() { Message = "Not found", Code = "NOT_FOUND" } }
            };
            var mappedResponse = new ApiResponseDto<PurchaseDto>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } },
                Meta    = new ApiMetaDto()
            };

            _mapper.Map<PurchaseReq>(dto).Returns(req);
            _http.PutAsync<PurchaseReq, PurchaseRes>(Arg.Any<string>(), req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<PurchaseDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.UpdatePurchaseAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region DeletePurchaseAsync Tests

        [Fact]
        public async Task DeletePurchaseAsync_WithSuccessResponse_ReturnsTrue()
        {
            // Arrange
            var dto = new PurchaseDto { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item A" };
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool?>(Arg.Is<string>(url => url.Contains("WG01") && url.Contains("ACC1")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.DeletePurchaseAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
        }

        [Fact]
        public async Task DeletePurchaseAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new PurchaseDto { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item A" };
            var apiResponse = new ApiResponse<bool?>
            {
                Success = false,
                Errors  = new List<ApiError> { new() { Message = "Not found", Code = "NOT_FOUND" } }
            };
            var mappedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } },
                Meta    = new ApiMetaDto()
            };

            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.DeletePurchaseAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion
    }
}
