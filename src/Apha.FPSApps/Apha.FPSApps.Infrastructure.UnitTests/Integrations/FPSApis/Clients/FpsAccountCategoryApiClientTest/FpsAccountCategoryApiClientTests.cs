using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;

namespace Apha.FPSApps.Infrastructure.UnitTests.Integrations.FPSApis.Clients.FpsAccountCategoryApiClientTest
{
    public class FpsAccountCategoryApiClientTests
    {
        private const string TestAccShortName = "TEST001";
        private const string TestAccountDescription = "Test Description";
        private const string TestFilterType = "all";

        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly FpsAccountCategoryApiClient _client;

        public FpsAccountCategoryApiClientTests()
        {
            _http = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsAccountCategoryApiClient(_http, _mapper);
        }

        #region GetFilteredAccountCategoriesAsync

        [Fact]
        public async Task GetFilteredAccountCategoriesAsync_Success_ReturnsMappedResponse()
        {
            // Arrange
            var criteria = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<AccountCategoryRes>>
            {
                Success = true,
                Data = new List<AccountCategoryRes>
                {
                    new AccountCategoryRes { AccShortName = TestAccShortName, AccountDescription = TestAccountDescription }
                }
            };

            var expectedMappedResponse = new ApiResponseDto<List<AccountCategoryDto>>
            {
                Success = true,
                Data = new List<AccountCategoryDto>
                {
                    new AccountCategoryDto { AccShortName = TestAccShortName, AccountDescription = TestAccountDescription }
                }
            };

            _http.GetAsync<List<AccountCategoryRes>>(Arg.Any<string>())
                .Returns(apiResponse);

            _mapper.Map<ApiResponseDto<List<AccountCategoryDto>>>(apiResponse)
                .Returns(expectedMappedResponse);

            // Act
            var result = await _client.GetFilteredAccountCategoriesAsync(criteria, TestFilterType);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _http.Received(1).GetAsync<List<AccountCategoryRes>>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetFilteredAccountCategoriesAsync_NullFilterType_UsesDefaultAll()
        {
            // Arrange
            var criteria = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<AccountCategoryRes>>
            {
                Success = true,
                Data = new List<AccountCategoryRes>()
            };

            var expectedMappedResponse = new ApiResponseDto<List<AccountCategoryDto>>
            {
                Success = true,
                Data = new List<AccountCategoryDto>()
            };

            _http.GetAsync<List<AccountCategoryRes>>(Arg.Is<string>(url => url.Contains("all")))
                .Returns(apiResponse);

            _mapper.Map<ApiResponseDto<List<AccountCategoryDto>>>(apiResponse)
                .Returns(expectedMappedResponse);

            // Act
            var result = await _client.GetFilteredAccountCategoriesAsync(criteria, null);

            // Assert
            Assert.NotNull(result);
            await _http.Received(1).GetAsync<List<AccountCategoryRes>>(Arg.Is<string>(url => url.Contains("all")));
        }

        [Fact]
        public async Task GetFilteredAccountCategoriesAsync_ApiFailure_ReturnsFailureResponse()
        {
            // Arrange
            var criteria = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<AccountCategoryRes>>
            {
                Success = false,
                Errors = new List<ApiError> { new ApiError { Message = "API Error" } }
            };

            var mappedFailureResponse = new ApiResponseDto<List<AccountCategoryDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error" } }
            };

            _http.GetAsync<List<AccountCategoryRes>>(Arg.Any<string>())
                .Returns(apiResponse);

            _mapper.Map<ApiResponseDto<List<AccountCategoryDto>>>(apiResponse)
                .Returns(mappedFailureResponse);

            // Act
            var result = await _client.GetFilteredAccountCategoriesAsync(criteria, TestFilterType);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors!);
        }

        #endregion

        #region GetAccountCategoryByIdAsync

        [Fact]
        public async Task GetAccountCategoryByIdAsync_Success_ReturnsMappedResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<AccountCategoryRes>
            {
                Success = true,
                Data = new AccountCategoryRes { AccShortName = TestAccShortName, AccountDescription = TestAccountDescription }
            };

            var expectedMappedResponse = new ApiResponseDto<AccountCategoryDto>
            {
                Success = true,
                Data = new AccountCategoryDto { AccShortName = TestAccShortName, AccountDescription = TestAccountDescription }
            };

            _http.GetAsync<AccountCategoryRes>(Arg.Any<string>())
                .Returns(apiResponse);

            _mapper.Map<ApiResponseDto<AccountCategoryDto>>(apiResponse)
                .Returns(expectedMappedResponse);

            // Act
            var result = await _client.GetAccountCategoryByIdAsync(TestAccShortName);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(TestAccShortName, result.Data!.AccShortName);
            await _http.Received(1).GetAsync<AccountCategoryRes>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetAccountCategoryByIdAsync_ApiFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<AccountCategoryRes>
            {
                Success = false,
                Errors = new List<ApiError> { new ApiError { Message = "Not found" } }
            };

            var mappedFailureResponse = new ApiResponseDto<AccountCategoryDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Not found" } }
            };

            _http.GetAsync<AccountCategoryRes>(Arg.Any<string>())
                .Returns(apiResponse);

            _mapper.Map<ApiResponseDto<AccountCategoryDto>>(apiResponse)
                .Returns(mappedFailureResponse);

            // Act
            var result = await _client.GetAccountCategoryByIdAsync(TestAccShortName);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region CreateAccountCategoryAsync

        [Fact]
        public async Task CreateAccountCategoryAsync_Success_ReturnsMappedResponse()
        {
            // Arrange
            var dto = new AccountCategoryDto { AccShortName = TestAccShortName, AccountDescription = TestAccountDescription };
            var req = new AccountCategoryReq { AccShortName = TestAccShortName, AccountDescription = TestAccountDescription };
            var apiResponse = new ApiResponse<AccountCategoryRes>
            {
                Success = true,
                Data = new AccountCategoryRes { AccShortName = TestAccShortName, AccountDescription = TestAccountDescription }
            };

            var expectedMappedResponse = new ApiResponseDto<AccountCategoryDto>
            {
                Success = true,
                Data = dto
            };

            _mapper.Map<AccountCategoryReq>(dto).Returns(req);
            _http.PostAsync<AccountCategoryReq, AccountCategoryRes>(Arg.Any<string>(), req)
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<AccountCategoryDto>>(apiResponse)
                .Returns(expectedMappedResponse);

            // Act
            var result = await _client.CreateAccountCategoryAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(TestAccShortName, result.Data!.AccShortName);
            await _http.Received(1).PostAsync<AccountCategoryReq, AccountCategoryRes>(Arg.Any<string>(), req);
        }

        [Fact]
        public async Task CreateAccountCategoryAsync_ApiFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new AccountCategoryDto { AccShortName = TestAccShortName };
            var req = new AccountCategoryReq { AccShortName = TestAccShortName };
            var apiResponse = new ApiResponse<AccountCategoryRes>
            {
                Success = false,
                Errors = new List<ApiError> { new ApiError { Message = "Validation error" } }
            };

            var mappedFailureResponse = new ApiResponseDto<AccountCategoryDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Validation error" } }
            };

            _mapper.Map<AccountCategoryReq>(dto).Returns(req);
            _http.PostAsync<AccountCategoryReq, AccountCategoryRes>(Arg.Any<string>(), req)
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<AccountCategoryDto>>(apiResponse)
                .Returns(mappedFailureResponse);

            // Act
            var result = await _client.CreateAccountCategoryAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors!);
        }

        #endregion

        #region UpdateAccountCategoryAsync

        [Fact]
        public async Task UpdateAccountCategoryAsync_Success_ReturnsMappedResponse()
        {
            // Arrange
            var dto = new AccountCategoryDto { AccShortName = TestAccShortName, AccountDescription = "Updated" };
            var req = new AccountCategoryReq { AccShortName = TestAccShortName, AccountDescription = "Updated" };
            var apiResponse = new ApiResponse<AccountCategoryRes>
            {
                Success = true,
                Data = new AccountCategoryRes { AccShortName = TestAccShortName, AccountDescription = "Updated" }
            };

            var expectedMappedResponse = new ApiResponseDto<AccountCategoryDto>
            {
                Success = true,
                Data = dto
            };

            _mapper.Map<AccountCategoryReq>(dto).Returns(req);
            _http.PutAsync<AccountCategoryReq, AccountCategoryRes>(Arg.Any<string>(), req)
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<AccountCategoryDto>>(apiResponse)
                .Returns(expectedMappedResponse);

            // Act
            var result = await _client.UpdateAccountCategoryAsync(TestAccShortName, dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("Updated", result.Data!.AccountDescription);
            await _http.Received(1).PutAsync<AccountCategoryReq, AccountCategoryRes>(Arg.Any<string>(), req);
        }

        [Fact]
        public async Task UpdateAccountCategoryAsync_UsesOriginalAccShortNameInUrl()
        {
            // Arrange
            var originalId = "ORIGINAL";
            var dto = new AccountCategoryDto { AccShortName = "CHANGED", AccountDescription = "Updated" };
            var req = new AccountCategoryReq { AccShortName = "CHANGED", AccountDescription = "Updated" };
            var apiResponse = new ApiResponse<AccountCategoryRes> { Success = true };
            var mappedResponse = new ApiResponseDto<AccountCategoryDto> { Success = true };

            _mapper.Map<AccountCategoryReq>(dto).Returns(req);
            _http.PutAsync<AccountCategoryReq, AccountCategoryRes>(Arg.Is<string>(url => url.Contains(originalId)), req)
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<AccountCategoryDto>>(apiResponse)
                .Returns(mappedResponse);

            // Act
            await _client.UpdateAccountCategoryAsync(originalId, dto);

            // Assert
            await _http.Received(1).PutAsync<AccountCategoryReq, AccountCategoryRes>(
                Arg.Is<string>(url => url.Contains(originalId)), 
                req);
        }

        [Fact]
        public async Task UpdateAccountCategoryAsync_ApiFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new AccountCategoryDto { AccShortName = TestAccShortName };
            var req = new AccountCategoryReq { AccShortName = TestAccShortName };
            var apiResponse = new ApiResponse<AccountCategoryRes>
            {
                Success = false,
                Errors = new List<ApiError> { new ApiError { Message = "Update failed" } }
            };

            var mappedFailureResponse = new ApiResponseDto<AccountCategoryDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Update failed" } }
            };

            _mapper.Map<AccountCategoryReq>(dto).Returns(req);
            _http.PutAsync<AccountCategoryReq, AccountCategoryRes>(Arg.Any<string>(), req)
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<AccountCategoryDto>>(apiResponse)
                .Returns(mappedFailureResponse);

            // Act
            var result = await _client.UpdateAccountCategoryAsync(TestAccShortName, dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region DeleteAccountCategoryAsync

        [Fact]
        public async Task DeleteAccountCategoryAsync_Success_ReturnsMappedResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool?>
            {
                Success = true,
                Data = true
            };

            var expectedMappedResponse = new ApiResponseDto<bool>
            {
                Success = true,
                Data = true
            };

            _http.DeleteAsync<bool?>(Arg.Any<string>())
                .Returns(apiResponse);

            _mapper.Map<ApiResponseDto<bool>>(apiResponse)
                .Returns(expectedMappedResponse);

            // Act
            var result = await _client.DeleteAccountCategoryAsync(TestAccShortName);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _http.Received(1).DeleteAsync<bool?>(Arg.Any<string>());
        }

        [Fact]
        public async Task DeleteAccountCategoryAsync_ApiFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool?>
            {
                Success = false,
                Errors = new List<ApiError> { new ApiError { Message = "Delete failed" } }
            };

            var mappedFailureResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Delete failed" } }
            };

            _http.DeleteAsync<bool?>(Arg.Any<string>())
                .Returns(apiResponse);

            _mapper.Map<ApiResponseDto<bool>>(apiResponse)
                .Returns(mappedFailureResponse);

            // Act
            var result = await _client.DeleteAccountCategoryAsync(TestAccShortName);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion
    }
}
