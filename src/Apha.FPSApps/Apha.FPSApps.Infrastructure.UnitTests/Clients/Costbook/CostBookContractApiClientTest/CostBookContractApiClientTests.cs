using Apha.Common.Contracts;
using Apha.Common.Contracts.Costbook;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Infrastructure.Integrations.CostBookApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.Costbook.CostBookContractApiClientTest
{
    public class CostBookContractApiClientTests
    {
        private readonly ICostBookHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly CostBookContractApiClient _client;

        public CostBookContractApiClientTests()
        {
            _http = Substitute.For<ICostBookHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new CostBookContractApiClient(_http, _mapper);
        }

        #region GetAllContractNumbersAsync Tests

        [Fact]
        public async Task GetAllContractNumbersAsync_WithSuccessResponse_ReturnsMappedContractList()
        {
            // Arrange
            var contractResList = new List<ContractRes> { new ContractRes(), new ContractRes() };
            var apiResponse = new ApiResponse<List<ContractRes>> { Success = true, Data = contractResList };
            var mappedContractDto = new List<ContractDto> { new ContractDto(), new ContractDto() };
            var expectedDto = ApiResponseDto<List<ContractDto>>.SuccessResponse(mappedContractDto);

            _http.GetAsync<List<ContractRes>>("api/v1/projects/contracts").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ContractDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllContractNumbersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            await _http.Received(1).GetAsync<List<ContractRes>>("api/v1/projects/contracts");
            _mapper.Received(1).Map<ApiResponseDto<List<ContractDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAllContractNumbersAsync_WithSuccessResponseButNullData_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<ContractRes>> { Success = true, Data = null };
            var mappedResponse = new ApiResponseDto<List<ContractDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "No data", Code = "NO_DATA" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ContractRes>>("api/v1/projects/contracts").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ContractDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetAllContractNumbersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.NotNull(result.Meta);
            await _http.Received(1).GetAsync<List<ContractRes>>("api/v1/projects/contracts");
            _mapper.Received(1).Map<ApiResponseDto<List<ContractDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAllContractNumbersAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<ContractRes>>
            {
                Success = false,
                Data = null,
                Errors = new List<ApiError> { new ApiError { Message = "API Error", Code = "ERROR_CODE" } }
            };
            var mappedResponse = new ApiResponseDto<List<ContractDto>>
            {
                Success = false,
                Data = null,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "ERROR_CODE" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ContractRes>>("api/v1/projects/contracts").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ContractDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetAllContractNumbersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("API Error", result.Errors[0].Message);
            Assert.Equal("ERROR_CODE", result.Errors[0].Code);
            await _http.Received(1).GetAsync<List<ContractRes>>("api/v1/projects/contracts");
            _mapper.Received(1).Map<ApiResponseDto<List<ContractDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAllContractNumbersAsync_WhenHttpExecutorThrowsException_ReturnsInternalError()
        {
            // Arrange
            var exceptionMessage = "Network connection failed";
            _http.GetAsync<List<ContractRes>>("api/v1/projects/contracts")
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _client.GetAllContractNumbersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve contracts", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
            Assert.Equal(exceptionMessage, result.Errors[0].Details);
            Assert.NotNull(result.Meta);
            await _http.Received(1).GetAsync<List<ContractRes>>("api/v1/projects/contracts");
        }

        [Fact]
        public async Task GetAllContractNumbersAsync_WhenMapperThrowsException_ReturnsInternalError()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<ContractRes>> { Success = true, Data = new List<ContractRes> { new ContractRes() } };
            _http.GetAsync<List<ContractRes>>("api/v1/projects/contracts").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ContractDto>>>(apiResponse).Throws(new AutoMapperMappingException("Mapping failed"));

            // Act
            var result = await _client.GetAllContractNumbersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve contracts", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
            await _http.Received(1).GetAsync<List<ContractRes>>("api/v1/projects/contracts");
        }

        [Fact]
        public async Task GetAllContractNumbersAsync_WithEmptyDataList_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<ContractRes>> { Success = true, Data = new List<ContractRes>() };
            var mappedDto = ApiResponseDto<List<ContractDto>>.SuccessResponse(new List<ContractDto>());

            _http.GetAsync<List<ContractRes>>("api/v1/projects/contracts").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ContractDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetAllContractNumbersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
            await _http.Received(1).GetAsync<List<ContractRes>>("api/v1/projects/contracts");
        }

        [Fact]
        public async Task GetAllContractNumbersAsync_EnsuresCorrectApiEndpoint_CallsWithCorrectUrl()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<ContractRes>> { Success = true, Data = new List<ContractRes>() };
            _http.GetAsync<List<ContractRes>>("api/v1/projects/contracts").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ContractDto>>>(apiResponse)
                .Returns(ApiResponseDto<List<ContractDto>>.SuccessResponse(new List<ContractDto>()));

            // Act
            await _client.GetAllContractNumbersAsync();

            // Assert
            await _http.Received(1).GetAsync<List<ContractRes>>(Arg.Is<string>(s => s == "api/v1/projects/contracts"));
        }

        #endregion
    }
}
