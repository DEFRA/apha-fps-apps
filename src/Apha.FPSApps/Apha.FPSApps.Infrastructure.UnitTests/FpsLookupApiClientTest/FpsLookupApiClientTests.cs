using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.FpsLookupApiClientTest
{
    public class FpsLookupApiClientTests
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly FpsLookupApiClient _client;

        public FpsLookupApiClientTests()
        {
            _http = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsLookupApiClient(_http, _mapper);
        }

        #region GetAllStatusesAsync Tests

        [Fact]
        public async Task GetAllStatusesAsync_WithSuccessResponse_ReturnsMappedStatusList()
        {
            // Arrange
            var statusList = new List<StatusRes> { new() { Status = "Active" }, new() { Status = "Inactive" } };
            var apiResponse = new ApiResponse<List<StatusRes>> { Success = true, Data = statusList };
            var expectedDto = ApiResponseDto<List<StatusDto>>.SuccessResponse(
                new List<StatusDto> { new() { Status = "Active" }, new() { Status = "Inactive" } }
            );

            _http.GetAsync<List<StatusRes>>("api/status").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<StatusDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllStatusesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<StatusRes>>("api/status");
        }

        [Fact]
        public async Task GetAllStatusesAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new() { Message = "API Error", Code = "API_ERROR" } };
            var apiResponse = new ApiResponse<List<StatusRes>> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<List<StatusDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<StatusRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<StatusDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetAllStatusesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task GetAllStatusesAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            // Arrange
            _http.GetAsync<List<StatusRes>>(Arg.Any<string>()).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetAllStatusesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal("Failed to retrieve statuses", error.Message);
        }

        #endregion

        #region GetAllDiseasesAsync Tests

        [Fact]
        public async Task GetAllDiseasesAsync_WithSuccessResponse_ReturnsMappedDiseaseList()
        {
            // Arrange
            var diseaseList = new List<DiseaseRes> { new() { Disease = "Foot and Mouth" }, new() { Disease = "Avian Flu" } };
            var apiResponse = new ApiResponse<List<DiseaseRes>> { Success = true, Data = diseaseList };
            var expectedDto = ApiResponseDto<List<DiseaseDto>>.SuccessResponse(
                new List<DiseaseDto> { new() { Disease = "Foot and Mouth" }, new() { Disease = "Avian Flu" } }
            );

            _http.GetAsync<List<DiseaseRes>>("api/disease").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<DiseaseDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllDiseasesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<DiseaseRes>>("api/disease");
        }

        [Fact]
        public async Task GetAllDiseasesAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new() { Message = "API Error", Code = "API_ERROR" } };
            var apiResponse = new ApiResponse<List<DiseaseRes>> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<List<DiseaseDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<DiseaseRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<DiseaseDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetAllDiseasesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task GetAllDiseasesAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            // Arrange
            _http.GetAsync<List<DiseaseRes>>(Arg.Any<string>()).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetAllDiseasesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal("Failed to retrieve diseases", error.Message);
        }

        #endregion

        #region GetAllCustomersAsync Tests

        [Fact]
        public async Task GetAllCustomersAsync_WithSuccessResponse_ReturnsMappedCustomerList()
        {
            // Arrange
            var customerList = new List<CustomerRes> { new() { Customer = "DEFRA" }, new() { Customer = "APHA" } };
            var apiResponse = new ApiResponse<List<CustomerRes>> { Success = true, Data = customerList };
            var expectedDto = ApiResponseDto<List<CustomerDto>>.SuccessResponse(
                new List<CustomerDto> { new() { Customer = "DEFRA" }, new() { Customer = "APHA" } }
            );

            _http.GetAsync<List<CustomerRes>>("api/customer").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<CustomerDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllCustomersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<CustomerRes>>("api/customer");
        }

        [Fact]
        public async Task GetAllCustomersAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new() { Message = "API Error", Code = "API_ERROR" } };
            var apiResponse = new ApiResponse<List<CustomerRes>> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<List<CustomerDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<CustomerRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<CustomerDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetAllCustomersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task GetAllCustomersAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            // Arrange
            _http.GetAsync<List<CustomerRes>>(Arg.Any<string>()).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetAllCustomersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal("Failed to retrieve customers", error.Message);
        }

        #endregion

        #region GetAllContractsAsync Tests

        [Fact]
        public async Task GetAllContractsAsync_WithSuccessResponse_ReturnsMappedContractList()
        {
            // Arrange
            var contractList = new List<ContractRes> { new() { ContractNo = "C001" }, new() { ContractNo = "C002" } };
            var apiResponse = new ApiResponse<List<ContractRes>> { Success = true, Data = contractList };
            var expectedDto = ApiResponseDto<List<ContractDto>>.SuccessResponse(
                new List<ContractDto> { new() { ContractNo = "C001" }, new() { ContractNo = "C002" } }
            );

            _http.GetAsync<List<ContractRes>>("api/contract").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ContractDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllContractsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<ContractRes>>("api/contract");
        }

        [Fact]
        public async Task GetAllContractsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new() { Message = "API Error", Code = "API_ERROR" } };
            var apiResponse = new ApiResponse<List<ContractRes>> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<List<ContractDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ContractRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ContractDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetAllContractsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task GetAllContractsAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            // Arrange
            _http.GetAsync<List<ContractRes>>(Arg.Any<string>()).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetAllContractsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal("Failed to retrieve contracts", error.Message);
        }

        #endregion
    }
}
