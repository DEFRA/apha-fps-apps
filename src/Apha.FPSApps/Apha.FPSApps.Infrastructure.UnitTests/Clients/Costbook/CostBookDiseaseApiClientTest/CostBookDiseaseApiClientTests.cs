using Apha.Common.Contracts;
using Apha.Common.Contracts.Costbook;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Infrastructure.Integrations.CostBookApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.Costbook.CostBookDiseaseApiClientTest
{
    public class CostBookDiseaseApiClientTests
    {
        private readonly ICostBookHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly CostBookDiseaseApiClient _client;

        public CostBookDiseaseApiClientTests()
        {
            _http = Substitute.For<ICostBookHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new CostBookDiseaseApiClient(_http, _mapper);
        }

        #region GetAllDiseasesAsync Tests

        [Fact]
        public async Task GetAllDiseasesAsync_WithSuccessResponse_ReturnsMappedDiseaseList()
        {
            // Arrange
            var diseaseResList = new List<DiseaseRes> { new DiseaseRes(), new DiseaseRes() };
            var apiResponse = new ApiResponse<List<DiseaseRes>> { Success = true, Data = diseaseResList };
            var mappedDiseaseDto = new List<DiseaseDto> { new DiseaseDto(), new DiseaseDto() };
            var expectedDto = ApiResponseDto<List<DiseaseDto>>.SuccessResponse(mappedDiseaseDto);

            _http.GetAsync<List<DiseaseRes>>("api/v1/projects/diseases").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<DiseaseDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllDiseasesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            await _http.Received(1).GetAsync<List<DiseaseRes>>("api/v1/projects/diseases");
            _mapper.Received(1).Map<ApiResponseDto<List<DiseaseDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAllDiseasesAsync_WithSuccessResponseButNullData_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<DiseaseRes>> { Success = true, Data = null };
            var mappedResponse = new ApiResponseDto<List<DiseaseDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "No data", Code = "NO_DATA" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<DiseaseRes>>("api/v1/projects/diseases").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<DiseaseDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetAllDiseasesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.NotNull(result.Meta);
            await _http.Received(1).GetAsync<List<DiseaseRes>>("api/v1/projects/diseases");
            _mapper.Received(1).Map<ApiResponseDto<List<DiseaseDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAllDiseasesAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<DiseaseRes>>
            {
                Success = false,
                Data = null,
                Errors = new List<ApiError> { new ApiError { Message = "API Error", Code = "ERROR_CODE" } }
            };
            var mappedResponse = new ApiResponseDto<List<DiseaseDto>>
            {
                Success = false,
                Data = null,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "ERROR_CODE" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<DiseaseRes>>("api/v1/projects/diseases").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<DiseaseDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetAllDiseasesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("API Error", result.Errors[0].Message);
            Assert.Equal("ERROR_CODE", result.Errors[0].Code);
            await _http.Received(1).GetAsync<List<DiseaseRes>>("api/v1/projects/diseases");
            _mapper.Received(1).Map<ApiResponseDto<List<DiseaseDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAllDiseasesAsync_WhenHttpExecutorThrowsException_ReturnsInternalError()
        {
            // Arrange
            var exceptionMessage = "Network connection failed";
            _http.GetAsync<List<DiseaseRes>>("api/v1/projects/diseases")
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _client.GetAllDiseasesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve diseases", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
            Assert.Equal(exceptionMessage, result.Errors[0].Details);
            Assert.NotNull(result.Meta);
            await _http.Received(1).GetAsync<List<DiseaseRes>>("api/v1/projects/diseases");
        }

        [Fact]
        public async Task GetAllDiseasesAsync_WhenMapperThrowsException_ReturnsInternalError()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<DiseaseRes>> { Success = true, Data = new List<DiseaseRes> { new DiseaseRes() } };
            _http.GetAsync<List<DiseaseRes>>("api/v1/projects/diseases").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<DiseaseDto>>>(apiResponse).Throws(new AutoMapperMappingException("Mapping failed"));

            // Act
            var result = await _client.GetAllDiseasesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve diseases", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
            await _http.Received(1).GetAsync<List<DiseaseRes>>("api/v1/projects/diseases");
        }

        [Fact]
        public async Task GetAllDiseasesAsync_WithEmptyDataList_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<DiseaseRes>> { Success = true, Data = new List<DiseaseRes>() };
            var mappedDto = ApiResponseDto<List<DiseaseDto>>.SuccessResponse(new List<DiseaseDto>());

            _http.GetAsync<List<DiseaseRes>>("api/v1/projects/diseases").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<DiseaseDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetAllDiseasesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
            await _http.Received(1).GetAsync<List<DiseaseRes>>("api/v1/projects/diseases");
        }

        [Fact]
        public async Task GetAllDiseasesAsync_EnsuresCorrectApiEndpoint_CallsWithCorrectUrl()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<DiseaseRes>> { Success = true, Data = new List<DiseaseRes>() };
            _http.GetAsync<List<DiseaseRes>>("api/v1/projects/diseases").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<DiseaseDto>>>(apiResponse)
                .Returns(ApiResponseDto<List<DiseaseDto>>.SuccessResponse(new List<DiseaseDto>()));

            // Act
            await _client.GetAllDiseasesAsync();

            // Assert
            await _http.Received(1).GetAsync<List<DiseaseRes>>(Arg.Is<string>(s => s == "api/v1/projects/diseases"));
        }

        #endregion
    }
}
