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

namespace Apha.FPSApps.Infrastructure.UnitTests.Costbook.CostBookProgramApiClientTest
{
    public class CostBookProgramApiClientTests
    {
        private readonly ICostBookHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly CostBookProgramApiClient _client;

        public CostBookProgramApiClientTests()
        {
            _http = Substitute.For<ICostBookHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new CostBookProgramApiClient(_http, _mapper);
        }

        #region GetAllProgramsAsync Tests

        [Fact]
        public async Task GetAllProgramsAsync_WithSuccessResponse_ReturnsMappedProgramList()
        {
            // Arrange
            var programResList = new List<ProgramRes>
            {
                new ProgramRes { /* populate with test data */ },
                new ProgramRes { /* populate with test data */ }
            };
            var apiResponse = new ApiResponse<List<ProgramRes>>
            {
                Success = true,
                Data = programResList
            };
            var mappedProgramDto = new List<ProgramDto>
            {
                new ProgramDto { /* populate with test data */ },
                new ProgramDto { /* populate with test data */ }
            };
            var expectedDto = ApiResponseDto<List<ProgramDto>>.SuccessResponse(mappedProgramDto);

            _http.GetAsync<List<ProgramRes>>("api/projects/programs").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProgramDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllProgramsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            await _http.Received(1).GetAsync<List<ProgramRes>>("api/projects/programs");
            _mapper.Received(1).Map<ApiResponseDto<List<ProgramDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAllProgramsAsync_WithSuccessResponseButNullData_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<ProgramRes>>
            {
                Success = true,
                Data = null
            };
            var mappedResponse = new ApiResponseDto<List<ProgramDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "No data", Code = "NO_DATA" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ProgramRes>>("api/projects/programs").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProgramDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetAllProgramsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.NotNull(result.Meta);
            await _http.Received(1).GetAsync<List<ProgramRes>>("api/projects/programs");
            _mapper.Received(1).Map<ApiResponseDto<List<ProgramDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAllProgramsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> 
            { 
                new ApiError { Message = "API Error", Code = "ERROR_CODE" } 
            };
            var apiResponse = new ApiResponse<List<ProgramRes>>
            {
                Success = false,
                Data = null,
                Errors = errors
            };
            var mappedErrors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "API Error", Code = "ERROR_CODE" }
            };
            var mappedResponse = new ApiResponseDto<List<ProgramDto>>
            {
                Success = false,
                Data = null,
                Errors = mappedErrors,
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ProgramRes>>("api/projects/programs").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProgramDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetAllProgramsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("API Error", result.Errors[0].Message);
            Assert.Equal("ERROR_CODE", result.Errors[0].Code);
            Assert.NotNull(result.Meta);
            await _http.Received(1).GetAsync<List<ProgramRes>>("api/projects/programs");
            _mapper.Received(1).Map<ApiResponseDto<List<ProgramDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAllProgramsAsync_WhenHttpExecutorThrowsException_ReturnsInternalError()
        {
            // Arrange
            var exceptionMessage = "Network connection failed";
            _http.GetAsync<List<ProgramRes>>("api/projects/programs")
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _client.GetAllProgramsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve programs", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
            Assert.Equal(exceptionMessage, result.Errors[0].Details);
            Assert.NotNull(result.Meta);
            await _http.Received(1).GetAsync<List<ProgramRes>>("api/projects/programs");
        }

        [Fact]
        public async Task GetAllProgramsAsync_WhenMapperThrowsException_ReturnsInternalError()
        {
            // Arrange
            var programResList = new List<ProgramRes>
            {
                new ProgramRes { /* populate with test data */ }
            };
            var apiResponse = new ApiResponse<List<ProgramRes>>
            {
                Success = true,
                Data = programResList
            };
            var mappingException = new AutoMapperMappingException("Mapping failed");

            _http.GetAsync<List<ProgramRes>>("api/projects/programs").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProgramDto>>>(apiResponse).Throws(mappingException);

            // Act
            var result = await _client.GetAllProgramsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve programs", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
            await _http.Received(1).GetAsync<List<ProgramRes>>("api/projects/programs");
        }

        [Fact]
        public async Task GetAllProgramsAsync_WithEmptyDataList_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var emptyProgramResList = new List<ProgramRes>();
            var apiResponse = new ApiResponse<List<ProgramRes>>
            {
                Success = true,
                Data = emptyProgramResList
            };
            var mappedEmptyDto = ApiResponseDto<List<ProgramDto>>.SuccessResponse(new List<ProgramDto>());

            _http.GetAsync<List<ProgramRes>>("api/projects/programs").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProgramDto>>>(apiResponse).Returns(mappedEmptyDto);

            // Act
            var result = await _client.GetAllProgramsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
            await _http.Received(1).GetAsync<List<ProgramRes>>("api/projects/programs");
        }

        [Fact]
        public async Task GetAllProgramsAsync_EnsuresCorrectApiEndpoint_CallsWithCorrectUrl()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<ProgramRes>>
            {
                Success = true,
                Data = new List<ProgramRes>()
            };
            var mappedDto = ApiResponseDto<List<ProgramDto>>.SuccessResponse(new List<ProgramDto>());

            _http.GetAsync<List<ProgramRes>>("api/projects/programs").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProgramDto>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetAllProgramsAsync();

            // Assert
            await _http.Received(1).GetAsync<List<ProgramRes>>(Arg.Is<string>(s => s == "api/projects/programs"));
        }

        #endregion
    }
}