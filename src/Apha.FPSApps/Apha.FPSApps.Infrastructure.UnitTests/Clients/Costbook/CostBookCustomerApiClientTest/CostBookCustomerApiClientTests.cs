using Apha.Common.Contracts;
using Apha.Common.Contracts.Costbook;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Infrastructure.Integrations.CostBookApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.Costbook.CostBookCustomerApiClientTest
{
    public class CostBookCustomerApiClientTests
    {
        private readonly ICostBookHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly CostBookCustomerApiClient _client;

        public CostBookCustomerApiClientTests()
        {
            _http = Substitute.For<ICostBookHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new CostBookCustomerApiClient(_http, _mapper);
        }

        #region GetAllCustomersAsync Tests

        [Fact]
        public async Task GetAllCustomersAsync_WithSuccessResponse_ReturnsMappedCustomerList()
        {
            // Arrange
            var customerResList = new List<CustomerRes> { new CustomerRes(), new CustomerRes() };
            var apiResponse = new ApiResponse<List<CustomerRes>> { Success = true, Data = customerResList };
            var mappedCustomerDto = new List<CustomerDto> { new CustomerDto(), new CustomerDto() };
            var expectedDto = ApiResponseDto<List<CustomerDto>>.SuccessResponse(mappedCustomerDto);

            _http.GetAsync<List<CustomerRes>>("api/v1/projects/customers").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<CustomerDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllCustomersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            await _http.Received(1).GetAsync<List<CustomerRes>>("api/v1/projects/customers");
            _mapper.Received(1).Map<ApiResponseDto<List<CustomerDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAllCustomersAsync_WithSuccessResponseButNullData_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<CustomerRes>> { Success = true, Data = null };
            var mappedResponse = new ApiResponseDto<List<CustomerDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "No data", Code = "NO_DATA" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<CustomerRes>>("api/v1/projects/customers").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<CustomerDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetAllCustomersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.NotNull(result.Meta);
            await _http.Received(1).GetAsync<List<CustomerRes>>("api/v1/projects/customers");
            _mapper.Received(1).Map<ApiResponseDto<List<CustomerDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAllCustomersAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<CustomerRes>>
            {
                Success = false,
                Data = null,
                Errors = new List<ApiError> { new ApiError { Message = "API Error", Code = "ERROR_CODE" } }
            };
            var mappedResponse = new ApiResponseDto<List<CustomerDto>>
            {
                Success = false,
                Data = null,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "ERROR_CODE" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<CustomerRes>>("api/v1/projects/customers").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<CustomerDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetAllCustomersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("API Error", result.Errors[0].Message);
            Assert.Equal("ERROR_CODE", result.Errors[0].Code);
            await _http.Received(1).GetAsync<List<CustomerRes>>("api/v1/projects/customers");
            _mapper.Received(1).Map<ApiResponseDto<List<CustomerDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAllCustomersAsync_WhenHttpExecutorThrowsException_ReturnsInternalError()
        {
            // Arrange
            var exceptionMessage = "Network connection failed";
            _http.GetAsync<List<CustomerRes>>("api/v1/projects/customers")
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _client.GetAllCustomersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve customers", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
            Assert.Equal(exceptionMessage, result.Errors[0].Details);
            Assert.NotNull(result.Meta);
            await _http.Received(1).GetAsync<List<CustomerRes>>("api/v1/projects/customers");
        }

        [Fact]
        public async Task GetAllCustomersAsync_WhenMapperThrowsException_ReturnsInternalError()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<CustomerRes>> { Success = true, Data = new List<CustomerRes> { new CustomerRes() } };
            _http.GetAsync<List<CustomerRes>>("api/v1/projects/customers").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<CustomerDto>>>(apiResponse).Throws(new AutoMapperMappingException("Mapping failed"));

            // Act
            var result = await _client.GetAllCustomersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve customers", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
            await _http.Received(1).GetAsync<List<CustomerRes>>("api/v1/projects/customers");
        }

        [Fact]
        public async Task GetAllCustomersAsync_WithEmptyDataList_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<CustomerRes>> { Success = true, Data = new List<CustomerRes>() };
            var mappedDto = ApiResponseDto<List<CustomerDto>>.SuccessResponse(new List<CustomerDto>());

            _http.GetAsync<List<CustomerRes>>("api/v1/projects/customers").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<CustomerDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetAllCustomersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
            await _http.Received(1).GetAsync<List<CustomerRes>>("api/v1/projects/customers");
        }

        [Fact]
        public async Task GetAllCustomersAsync_EnsuresCorrectApiEndpoint_CallsWithCorrectUrl()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<CustomerRes>> { Success = true, Data = new List<CustomerRes>() };
            _http.GetAsync<List<CustomerRes>>("api/v1/projects/customers").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<CustomerDto>>>(apiResponse)
                .Returns(ApiResponseDto<List<CustomerDto>>.SuccessResponse(new List<CustomerDto>()));

            // Act
            await _client.GetAllCustomersAsync();

            // Assert
            await _http.Received(1).GetAsync<List<CustomerRes>>(Arg.Is<string>(s => s == "api/v1/projects/customers"));
        }

        #endregion
    }
}
