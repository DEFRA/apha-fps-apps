using Apha.Common.Constants;
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

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PIMS.PimsProjectYearCostsApiClientTest
{
    public class PimsProjectYearCostsApiClientTests
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PimsProjectYearCostsApiClient _client;
        private const string Project = "PP001";
        private const short Year = 2024;

        public PimsProjectYearCostsApiClientTests()
        {
            _http = Substitute.For<IPimsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PimsProjectYearCostsApiClient(_http, _mapper);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_InitializesClient()
        {
            // Arrange & Act
            var client = new PimsProjectYearCostsApiClient(_http, _mapper);

            // Assert
            Assert.NotNull(client);
        }

        #endregion

        #region GetAdditionalActualsAsync Tests

        [Fact]
        public async Task GetAdditionalActualsAsync_WithSuccessResponseAndData_ReturnsMappedList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var resList = new List<AdditionalCostRes>
            {
                new AdditionalCostRes { Year = Year, Project = Project, Description = "Subcontract A" }
            };
            var apiResponse = new ApiResponse<List<AdditionalCostRes>> { Success = true, Data = resList };
            var mappedDto = ApiResponseDto<List<AdditionalCostDto>>.SuccessResponse(new List<AdditionalCostDto>
            {
                new AdditionalCostDto { Year = Year, Project = Project, Description = "Subcontract A" }
            });

            _http.GetAsync<List<AdditionalCostRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<AdditionalCostDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetAdditionalActualsAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            await _http.Received(1).GetAsync<List<AdditionalCostRes>>(Arg.Any<string>());
            _mapper.Received(1).Map<ApiResponseDto<List<AdditionalCostDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAdditionalActualsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiError> { new ApiError { Message = "API Error", Code = "ERROR_CODE" } };
            var apiResponse = new ApiResponse<List<AdditionalCostRes>> { Success = false, Data = null, Errors = errors };
            var mappedDto = new ApiResponseDto<List<AdditionalCostDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "ERROR_CODE" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<AdditionalCostRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<AdditionalCostDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetAdditionalActualsAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("API Error", result.Errors[0].Message);
            Assert.Equal("ERROR_CODE", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetAdditionalActualsAsync_WhenHttpExecutorThrowsException_ReturnsInternalError()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _http.GetAsync<List<AdditionalCostRes>>(Arg.Any<string>()).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetAdditionalActualsAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve additional actuals", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetAdditionalActualsAsync_WhenMapperThrowsException_ReturnsInternalError()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<AdditionalCostRes>> { Success = true, Data = new List<AdditionalCostRes>() };

            _http.GetAsync<List<AdditionalCostRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<AdditionalCostDto>>>(apiResponse).Throws(new AutoMapperMappingException("Mapping failed"));

            // Act
            var result = await _client.GetAdditionalActualsAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve additional actuals", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetAdditionalActualsAsync_EnsuresCorrectApiEndpoint_CallsWithCorrectUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedUrlBase = string.Format(PimsApiEndpoints.GetAdditionalActuals, Project, Year);
            var apiResponse = new ApiResponse<List<AdditionalCostRes>> { Success = true, Data = new List<AdditionalCostRes>() };
            var mappedDto = ApiResponseDto<List<AdditionalCostDto>>.SuccessResponse(new List<AdditionalCostDto>());

            _http.GetAsync<List<AdditionalCostRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<AdditionalCostDto>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetAdditionalActualsAsync(Project, Year, query);

            // Assert
            await _http.Received(1).GetAsync<List<AdditionalCostRes>>(
                Arg.Is<string>(s => s.StartsWith(expectedUrlBase)));
        }

        #endregion

        #region GetAdditionalPlansAsync Tests

        [Fact]
        public async Task GetAdditionalPlansAsync_WithSuccessResponseAndData_ReturnsMappedList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var resList = new List<AdditionalCostRes>
            {
                new AdditionalCostRes { Year = Year, JobCode = Project, Account = "ACC001" }
            };
            var apiResponse = new ApiResponse<List<AdditionalCostRes>> { Success = true, Data = resList };
            var mappedDto = ApiResponseDto<List<AdditionalCostDto>>.SuccessResponse(new List<AdditionalCostDto>
            {
                new AdditionalCostDto { Year = Year, JobCode = Project }
            });

            _http.GetAsync<List<AdditionalCostRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<AdditionalCostDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetAdditionalPlansAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            await _http.Received(1).GetAsync<List<AdditionalCostRes>>(Arg.Any<string>());
            _mapper.Received(1).Map<ApiResponseDto<List<AdditionalCostDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAdditionalPlansAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiError> { new ApiError { Message = "API Error", Code = "ERROR_CODE" } };
            var apiResponse = new ApiResponse<List<AdditionalCostRes>> { Success = false, Data = null, Errors = errors };
            var mappedDto = new ApiResponseDto<List<AdditionalCostDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "ERROR_CODE" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<AdditionalCostRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<AdditionalCostDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetAdditionalPlansAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("API Error", result.Errors[0].Message);
            Assert.Equal("ERROR_CODE", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetAdditionalPlansAsync_WhenHttpExecutorThrowsException_ReturnsInternalError()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _http.GetAsync<List<AdditionalCostRes>>(Arg.Any<string>()).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetAdditionalPlansAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve additional plans", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetAdditionalPlansAsync_WhenMapperThrowsException_ReturnsInternalError()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<AdditionalCostRes>> { Success = true, Data = new List<AdditionalCostRes>() };

            _http.GetAsync<List<AdditionalCostRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<AdditionalCostDto>>>(apiResponse).Throws(new AutoMapperMappingException("Mapping failed"));

            // Act
            var result = await _client.GetAdditionalPlansAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve additional plans", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetAdditionalPlansAsync_EnsuresCorrectApiEndpoint_CallsWithCorrectUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedUrlBase = string.Format(PimsApiEndpoints.GetAdditionalPlans, Project, Year);
            var apiResponse = new ApiResponse<List<AdditionalCostRes>> { Success = true, Data = new List<AdditionalCostRes>() };
            var mappedDto = ApiResponseDto<List<AdditionalCostDto>>.SuccessResponse(new List<AdditionalCostDto>());

            _http.GetAsync<List<AdditionalCostRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<AdditionalCostDto>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetAdditionalPlansAsync(Project, Year, query);

            // Assert
            await _http.Received(1).GetAsync<List<AdditionalCostRes>>(
                Arg.Is<string>(s => s.StartsWith(expectedUrlBase)));
        }

        #endregion

        #region GetAnimalActualsAsync Tests

        [Fact]
        public async Task GetAnimalActualsAsync_WithSuccessResponseAndData_ReturnsMappedList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var resList = new List<AnimalCostRes>
            {
                new AnimalCostRes { Year = Year, Project = Project, AcctCode = "LargeAnimals" }
            };
            var apiResponse = new ApiResponse<List<AnimalCostRes>> { Success = true, Data = resList };
            var mappedDto = ApiResponseDto<List<AnimalCostDto>>.SuccessResponse(new List<AnimalCostDto>
            {
                new AnimalCostDto { Year = Year, Project = Project }
            });

            _http.GetAsync<List<AnimalCostRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<AnimalCostDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetAnimalActualsAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            await _http.Received(1).GetAsync<List<AnimalCostRes>>(Arg.Any<string>());
            _mapper.Received(1).Map<ApiResponseDto<List<AnimalCostDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAnimalActualsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiError> { new ApiError { Message = "API Error", Code = "ERROR_CODE" } };
            var apiResponse = new ApiResponse<List<AnimalCostRes>> { Success = false, Data = null, Errors = errors };
            var mappedDto = new ApiResponseDto<List<AnimalCostDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "ERROR_CODE" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<AnimalCostRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<AnimalCostDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetAnimalActualsAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("API Error", result.Errors[0].Message);
            Assert.Equal("ERROR_CODE", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetAnimalActualsAsync_WhenHttpExecutorThrowsException_ReturnsInternalError()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _http.GetAsync<List<AnimalCostRes>>(Arg.Any<string>()).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetAnimalActualsAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve animal actuals", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetAnimalActualsAsync_WhenMapperThrowsException_ReturnsInternalError()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<AnimalCostRes>> { Success = true, Data = new List<AnimalCostRes>() };

            _http.GetAsync<List<AnimalCostRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<AnimalCostDto>>>(apiResponse).Throws(new AutoMapperMappingException("Mapping failed"));

            // Act
            var result = await _client.GetAnimalActualsAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve animal actuals", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetAnimalActualsAsync_EnsuresCorrectApiEndpoint_CallsWithCorrectUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedUrlBase = string.Format(PimsApiEndpoints.GetAnimalActuals, Project, Year);
            var apiResponse = new ApiResponse<List<AnimalCostRes>> { Success = true, Data = new List<AnimalCostRes>() };
            var mappedDto = ApiResponseDto<List<AnimalCostDto>>.SuccessResponse(new List<AnimalCostDto>());

            _http.GetAsync<List<AnimalCostRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<AnimalCostDto>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetAnimalActualsAsync(Project, Year, query);

            // Assert
            await _http.Received(1).GetAsync<List<AnimalCostRes>>(
                Arg.Is<string>(s => s.StartsWith(expectedUrlBase)));
        }

        #endregion

        #region GetAnimalPlansAsync Tests

        [Fact]
        public async Task GetAnimalPlansAsync_WithSuccessResponseAndData_ReturnsMappedList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var resList = new List<AnimalCostRes>
            {
                new AnimalCostRes { Year = Year, ParentProject = Project, AnimalType = "Cattle" }
            };
            var apiResponse = new ApiResponse<List<AnimalCostRes>> { Success = true, Data = resList };
            var mappedDto = ApiResponseDto<List<AnimalCostDto>>.SuccessResponse(new List<AnimalCostDto>
            {
                new AnimalCostDto { Year = Year, ParentProject = Project }
            });

            _http.GetAsync<List<AnimalCostRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<AnimalCostDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetAnimalPlansAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            await _http.Received(1).GetAsync<List<AnimalCostRes>>(Arg.Any<string>());
            _mapper.Received(1).Map<ApiResponseDto<List<AnimalCostDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAnimalPlansAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiError> { new ApiError { Message = "API Error", Code = "ERROR_CODE" } };
            var apiResponse = new ApiResponse<List<AnimalCostRes>> { Success = false, Data = null, Errors = errors };
            var mappedDto = new ApiResponseDto<List<AnimalCostDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "ERROR_CODE" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<AnimalCostRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<AnimalCostDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetAnimalPlansAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("API Error", result.Errors[0].Message);
            Assert.Equal("ERROR_CODE", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetAnimalPlansAsync_WhenHttpExecutorThrowsException_ReturnsInternalError()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _http.GetAsync<List<AnimalCostRes>>(Arg.Any<string>()).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetAnimalPlansAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve animal plans", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetAnimalPlansAsync_WhenMapperThrowsException_ReturnsInternalError()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<AnimalCostRes>> { Success = true, Data = new List<AnimalCostRes>() };

            _http.GetAsync<List<AnimalCostRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<AnimalCostDto>>>(apiResponse).Throws(new AutoMapperMappingException("Mapping failed"));

            // Act
            var result = await _client.GetAnimalPlansAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve animal plans", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetAnimalPlansAsync_EnsuresCorrectApiEndpoint_CallsWithCorrectUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedUrlBase = string.Format(PimsApiEndpoints.GetAnimalPlans, Project, Year);
            var apiResponse = new ApiResponse<List<AnimalCostRes>> { Success = true, Data = new List<AnimalCostRes>() };
            var mappedDto = ApiResponseDto<List<AnimalCostDto>>.SuccessResponse(new List<AnimalCostDto>());

            _http.GetAsync<List<AnimalCostRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<AnimalCostDto>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetAnimalPlansAsync(Project, Year, query);

            // Assert
            await _http.Received(1).GetAsync<List<AnimalCostRes>>(
                Arg.Is<string>(s => s.StartsWith(expectedUrlBase)));
        }

        #endregion

        #region GetTestPlansAsync Tests

        [Fact]
        public async Task GetTestPlansAsync_WithSuccessResponseAndData_ReturnsMappedList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var resList = new List<TestCostRes>
            {
                new TestCostRes { Year = Year, Buyer = Project, TestCode = "TC001" }
            };
            var apiResponse = new ApiResponse<List<TestCostRes>> { Success = true, Data = resList };
            var mappedDto = ApiResponseDto<List<TestCostDto>>.SuccessResponse(new List<TestCostDto>
            {
                new TestCostDto { Year = Year, Buyer = Project }
            });

            _http.GetAsync<List<TestCostRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestCostDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetTestPlansAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            await _http.Received(1).GetAsync<List<TestCostRes>>(Arg.Any<string>());
            _mapper.Received(1).Map<ApiResponseDto<List<TestCostDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetTestPlansAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiError> { new ApiError { Message = "API Error", Code = "ERROR_CODE" } };
            var apiResponse = new ApiResponse<List<TestCostRes>> { Success = false, Data = null, Errors = errors };
            var mappedDto = new ApiResponseDto<List<TestCostDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "ERROR_CODE" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<TestCostRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestCostDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetTestPlansAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("API Error", result.Errors[0].Message);
            Assert.Equal("ERROR_CODE", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetTestPlansAsync_WhenHttpExecutorThrowsException_ReturnsInternalError()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _http.GetAsync<List<TestCostRes>>(Arg.Any<string>()).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetTestPlansAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve test plans", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetTestPlansAsync_WhenMapperThrowsException_ReturnsInternalError()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<TestCostRes>> { Success = true, Data = new List<TestCostRes>() };

            _http.GetAsync<List<TestCostRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestCostDto>>>(apiResponse).Throws(new AutoMapperMappingException("Mapping failed"));

            // Act
            var result = await _client.GetTestPlansAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve test plans", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetTestPlansAsync_EnsuresCorrectApiEndpoint_CallsWithCorrectUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedUrlBase = string.Format(PimsApiEndpoints.GetTestPlans, Project, Year);
            var apiResponse = new ApiResponse<List<TestCostRes>> { Success = true, Data = new List<TestCostRes>() };
            var mappedDto = ApiResponseDto<List<TestCostDto>>.SuccessResponse(new List<TestCostDto>());

            _http.GetAsync<List<TestCostRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestCostDto>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetTestPlansAsync(Project, Year, query);

            // Assert
            await _http.Received(1).GetAsync<List<TestCostRes>>(
                Arg.Is<string>(s => s.StartsWith(expectedUrlBase)));
        }

        #endregion

        #region GetTestActualsAsync Tests

        [Fact]
        public async Task GetTestActualsAsync_WithSuccessResponseAndData_ReturnsMappedList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var resList = new List<TestCostRes>
            {
                new TestCostRes { Year = Year, WorkGroup = "WorkGroup1", Volume = 5.0 }
            };
            var apiResponse = new ApiResponse<List<TestCostRes>> { Success = true, Data = resList };
            var mappedDto = ApiResponseDto<List<TestCostDto>>.SuccessResponse(new List<TestCostDto>
            {
                new TestCostDto { Year = Year, WorkGroup = "WorkGroup1" }
            });

            _http.GetAsync<List<TestCostRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestCostDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetTestActualsAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            await _http.Received(1).GetAsync<List<TestCostRes>>(Arg.Any<string>());
            _mapper.Received(1).Map<ApiResponseDto<List<TestCostDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetTestActualsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiError> { new ApiError { Message = "API Error", Code = "ERROR_CODE" } };
            var apiResponse = new ApiResponse<List<TestCostRes>> { Success = false, Data = null, Errors = errors };
            var mappedDto = new ApiResponseDto<List<TestCostDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "ERROR_CODE" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<TestCostRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestCostDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetTestActualsAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("API Error", result.Errors[0].Message);
            Assert.Equal("ERROR_CODE", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetTestActualsAsync_WhenHttpExecutorThrowsException_ReturnsInternalError()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _http.GetAsync<List<TestCostRes>>(Arg.Any<string>()).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetTestActualsAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve test actuals", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetTestActualsAsync_WhenMapperThrowsException_ReturnsInternalError()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<TestCostRes>> { Success = true, Data = new List<TestCostRes>() };

            _http.GetAsync<List<TestCostRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestCostDto>>>(apiResponse).Throws(new AutoMapperMappingException("Mapping failed"));

            // Act
            var result = await _client.GetTestActualsAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve test actuals", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetTestActualsAsync_EnsuresCorrectApiEndpoint_CallsWithCorrectUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedUrlBase = string.Format(PimsApiEndpoints.GetTestActuals, Project, Year);
            var apiResponse = new ApiResponse<List<TestCostRes>> { Success = true, Data = new List<TestCostRes>() };
            var mappedDto = ApiResponseDto<List<TestCostDto>>.SuccessResponse(new List<TestCostDto>());

            _http.GetAsync<List<TestCostRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestCostDto>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetTestActualsAsync(Project, Year, query);

            // Assert
            await _http.Received(1).GetAsync<List<TestCostRes>>(
                Arg.Is<string>(s => s.StartsWith(expectedUrlBase)));
        }

        #endregion

        #region GetStaffPlansAsync Tests

        [Fact]
        public async Task GetStaffPlansAsync_WithSuccessResponseAndData_ReturnsMappedList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var resList = new List<StaffCostRes>
            {
                new StaffCostRes { Year = Year, ParentProject = Project, Name = "Staff A" }
            };
            var apiResponse = new ApiResponse<List<StaffCostRes>> { Success = true, Data = resList };
            var mappedDto = ApiResponseDto<List<StaffCostDto>>.SuccessResponse(new List<StaffCostDto>
            {
                new StaffCostDto { Year = Year, ParentProject = Project }
            });

            _http.GetAsync<List<StaffCostRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<StaffCostDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetStaffPlansAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            await _http.Received(1).GetAsync<List<StaffCostRes>>(Arg.Any<string>());
            _mapper.Received(1).Map<ApiResponseDto<List<StaffCostDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetStaffPlansAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiError> { new ApiError { Message = "API Error", Code = "ERROR_CODE" } };
            var apiResponse = new ApiResponse<List<StaffCostRes>> { Success = false, Data = null, Errors = errors };
            var mappedDto = new ApiResponseDto<List<StaffCostDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "ERROR_CODE" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<StaffCostRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<StaffCostDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetStaffPlansAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("API Error", result.Errors[0].Message);
            Assert.Equal("ERROR_CODE", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetStaffPlansAsync_WhenHttpExecutorThrowsException_ReturnsInternalError()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _http.GetAsync<List<StaffCostRes>>(Arg.Any<string>()).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetStaffPlansAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve staff plans", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetStaffPlansAsync_WhenMapperThrowsException_ReturnsInternalError()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<StaffCostRes>> { Success = true, Data = new List<StaffCostRes>() };

            _http.GetAsync<List<StaffCostRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<StaffCostDto>>>(apiResponse).Throws(new AutoMapperMappingException("Mapping failed"));

            // Act
            var result = await _client.GetStaffPlansAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve staff plans", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetStaffPlansAsync_EnsuresCorrectApiEndpoint_CallsWithCorrectUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedUrlBase = string.Format(PimsApiEndpoints.GetStaffPlans, Project, Year);
            var apiResponse = new ApiResponse<List<StaffCostRes>> { Success = true, Data = new List<StaffCostRes>() };
            var mappedDto = ApiResponseDto<List<StaffCostDto>>.SuccessResponse(new List<StaffCostDto>());

            _http.GetAsync<List<StaffCostRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<StaffCostDto>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetStaffPlansAsync(Project, Year, query);

            // Assert
            await _http.Received(1).GetAsync<List<StaffCostRes>>(
                Arg.Is<string>(s => s.StartsWith(expectedUrlBase)));
        }

        #endregion

        #region GetStaffActualsAsync Tests

        [Fact]
        public async Task GetStaffActualsAsync_WithSuccessResponseAndData_ReturnsMappedList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var resList = new List<StaffCostRes>
            {
                new StaffCostRes { Year = Year, JobCode = Project, WorkGroup = "WG01" }
            };
            var apiResponse = new ApiResponse<List<StaffCostRes>> { Success = true, Data = resList };
            var mappedDto = ApiResponseDto<List<StaffCostDto>>.SuccessResponse(new List<StaffCostDto>
            {
                new StaffCostDto { Year = Year, JobCode = Project }
            });

            _http.GetAsync<List<StaffCostRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<StaffCostDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetStaffActualsAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            await _http.Received(1).GetAsync<List<StaffCostRes>>(Arg.Any<string>());
            _mapper.Received(1).Map<ApiResponseDto<List<StaffCostDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetStaffActualsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiError> { new ApiError { Message = "API Error", Code = "ERROR_CODE" } };
            var apiResponse = new ApiResponse<List<StaffCostRes>> { Success = false, Data = null, Errors = errors };
            var mappedDto = new ApiResponseDto<List<StaffCostDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "ERROR_CODE" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<StaffCostRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<StaffCostDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetStaffActualsAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("API Error", result.Errors[0].Message);
            Assert.Equal("ERROR_CODE", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetStaffActualsAsync_WhenHttpExecutorThrowsException_ReturnsInternalError()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _http.GetAsync<List<StaffCostRes>>(Arg.Any<string>()).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetStaffActualsAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve staff actuals", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetStaffActualsAsync_WhenMapperThrowsException_ReturnsInternalError()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<StaffCostRes>> { Success = true, Data = new List<StaffCostRes>() };

            _http.GetAsync<List<StaffCostRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<StaffCostDto>>>(apiResponse).Throws(new AutoMapperMappingException("Mapping failed"));

            // Act
            var result = await _client.GetStaffActualsAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve staff actuals", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetStaffActualsAsync_EnsuresCorrectApiEndpoint_CallsWithCorrectUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedUrlBase = string.Format(PimsApiEndpoints.GetStaffActuals, Project, Year);
            var apiResponse = new ApiResponse<List<StaffCostRes>> { Success = true, Data = new List<StaffCostRes>() };
            var mappedDto = ApiResponseDto<List<StaffCostDto>>.SuccessResponse(new List<StaffCostDto>());

            _http.GetAsync<List<StaffCostRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<StaffCostDto>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetStaffActualsAsync(Project, Year, query);

            // Assert
            await _http.Received(1).GetAsync<List<StaffCostRes>>(
                Arg.Is<string>(s => s.StartsWith(expectedUrlBase)));
        }

        #endregion

        #region GetProjectYearDetailsAsync Tests

        [Fact]
        public async Task GetProjectYearDetailsAsync_WithSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            var url = string.Format(PimsApiEndpoints.GetProjectYearDetails, Project, Year);
            var res = new ProjectYearDetailsRes { Year = Year, Parentproject = Project, Manager = "Manager A" };
            var apiResponse = new ApiResponse<ProjectYearDetailsRes> { Success = true, Data = res };
            var mappedDto = ApiResponseDto<ProjectYearDetailsDto>.SuccessResponse(
                new ProjectYearDetailsDto { Year = Year, Parentproject = Project, Manager = "Manager A" }
            );

            _http.GetAsync<ProjectYearDetailsRes>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectYearDetailsDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetProjectYearDetailsAsync(Project, Year);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(Project, result.Data.Parentproject);
            Assert.Equal(Year, result.Data.Year);
            await _http.Received(1).GetAsync<ProjectYearDetailsRes>(url);
            _mapper.Received(1).Map<ApiResponseDto<ProjectYearDetailsDto>>(apiResponse);
        }

        [Fact]
        public async Task GetProjectYearDetailsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var url = string.Format(PimsApiEndpoints.GetProjectYearDetails, Project, Year);
            var errors = new List<ApiError> { new ApiError { Message = "Not found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<ProjectYearDetailsRes> { Success = false, Data = null, Errors = errors };
            var mappedDto = new ApiResponseDto<ProjectYearDetailsDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Not found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<ProjectYearDetailsRes>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectYearDetailsDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetProjectYearDetailsAsync(Project, Year);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Not found", result.Errors[0].Message);
            Assert.Equal("NOT_FOUND", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetProjectYearDetailsAsync_WhenHttpExecutorThrowsException_ReturnsInternalError()
        {
            // Arrange
            var url = string.Format(PimsApiEndpoints.GetProjectYearDetails, Project, Year);
            _http.GetAsync<ProjectYearDetailsRes>(url).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetProjectYearDetailsAsync(Project, Year);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve project year details", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetProjectYearDetailsAsync_WhenMapperThrowsException_ReturnsInternalError()
        {
            // Arrange
            var url = string.Format(PimsApiEndpoints.GetProjectYearDetails, Project, Year);
            var apiResponse = new ApiResponse<ProjectYearDetailsRes> { Success = true, Data = new ProjectYearDetailsRes { Parentproject = Project } };

            _http.GetAsync<ProjectYearDetailsRes>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectYearDetailsDto>>(apiResponse).Throws(new AutoMapperMappingException("Mapping failed"));

            // Act
            var result = await _client.GetProjectYearDetailsAsync(Project, Year);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve project year details", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetProjectYearDetailsAsync_EnsuresCorrectApiEndpoint_CallsWithCorrectUrl()
        {
            // Arrange
            var expectedUrl = string.Format(PimsApiEndpoints.GetProjectYearDetails, Project, Year);
            var apiResponse = new ApiResponse<ProjectYearDetailsRes> { Success = true, Data = new ProjectYearDetailsRes() };
            var mappedDto = ApiResponseDto<ProjectYearDetailsDto>.SuccessResponse(new ProjectYearDetailsDto());

            _http.GetAsync<ProjectYearDetailsRes>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectYearDetailsDto>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetProjectYearDetailsAsync(Project, Year);

            // Assert
            await _http.Received(1).GetAsync<ProjectYearDetailsRes>(Arg.Is<string>(s => s == expectedUrl));
        }

        #endregion

        #region GetPactPayAsync Tests

        [Fact]
        public async Task GetPactPayAsync_WithSuccessResponseAndData_ReturnsMappedList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var resList = new List<PactPayRes>
            {
                new PactPayRes { Year = Year, Project = Project, Pay = 1000m, NonPay = 200m }
            };
            var apiResponse = new ApiResponse<List<PactPayRes>> { Success = true, Data = resList };
            var mappedDto = ApiResponseDto<List<PactPayDto>>.SuccessResponse(new List<PactPayDto>
            {
                new PactPayDto { Year = Year, Project = Project, Pay = 1000m }
            });

            _http.GetAsync<List<PactPayRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<PactPayDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetPactPayAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            Assert.Equal(1000m, result.Data[0].Pay);
            await _http.Received(1).GetAsync<List<PactPayRes>>(Arg.Any<string>());
            _mapper.Received(1).Map<ApiResponseDto<List<PactPayDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetPactPayAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiError> { new ApiError { Message = "API Error", Code = "ERROR_CODE" } };
            var apiResponse = new ApiResponse<List<PactPayRes>> { Success = false, Data = null, Errors = errors };
            var mappedDto = new ApiResponseDto<List<PactPayDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "ERROR_CODE" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<PactPayRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<PactPayDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetPactPayAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("API Error", result.Errors[0].Message);
            Assert.Equal("ERROR_CODE", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetPactPayAsync_WhenHttpExecutorThrowsException_ReturnsInternalError()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _http.GetAsync<List<PactPayRes>>(Arg.Any<string>()).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetPactPayAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve pact pay data", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetPactPayAsync_WhenMapperThrowsException_ReturnsInternalError()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<PactPayRes>> { Success = true, Data = new List<PactPayRes>() };

            _http.GetAsync<List<PactPayRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<PactPayDto>>>(apiResponse).Throws(new AutoMapperMappingException("Mapping failed"));

            // Act
            var result = await _client.GetPactPayAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve pact pay data", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetPactPayAsync_EnsuresCorrectApiEndpoint_CallsWithCorrectUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedUrlBase = string.Format(PimsApiEndpoints.GetPactPay, Project, Year);
            var apiResponse = new ApiResponse<List<PactPayRes>> { Success = true, Data = new List<PactPayRes>() };
            var mappedDto = ApiResponseDto<List<PactPayDto>>.SuccessResponse(new List<PactPayDto>());

            _http.GetAsync<List<PactPayRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<PactPayDto>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetPactPayAsync(Project, Year, query);

            // Assert
            await _http.Received(1).GetAsync<List<PactPayRes>>(
                Arg.Is<string>(s => s.StartsWith(expectedUrlBase)));
        }

        #endregion

        #region GetMonthlyPactDataAsync Tests

        [Fact]
        public async Task GetMonthlyPactDataAsync_WithSuccessResponseAndData_ReturnsMappedList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var resList = new List<MonthlyPactRes>
            {
                new MonthlyPactRes { Year = Year, Project = Project, Monthno = 1, Periodname = "April" }
            };
            var apiResponse = new ApiResponse<List<MonthlyPactRes>> { Success = true, Data = resList };
            var mappedDto = ApiResponseDto<List<MonthlyPactDto>>.SuccessResponse(new List<MonthlyPactDto>
            {
                new MonthlyPactDto { Year = Year, Project = Project, Monthno = 1 }
            });

            _http.GetAsync<List<MonthlyPactRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<MonthlyPactDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetMonthlyPactDataAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            Assert.Equal(1, result.Data[0].Monthno);
            await _http.Received(1).GetAsync<List<MonthlyPactRes>>(Arg.Any<string>());
            _mapper.Received(1).Map<ApiResponseDto<List<MonthlyPactDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetMonthlyPactDataAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiError> { new ApiError { Message = "API Error", Code = "ERROR_CODE" } };
            var apiResponse = new ApiResponse<List<MonthlyPactRes>> { Success = false, Data = null, Errors = errors };
            var mappedDto = new ApiResponseDto<List<MonthlyPactDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "ERROR_CODE" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<MonthlyPactRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<MonthlyPactDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetMonthlyPactDataAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("API Error", result.Errors[0].Message);
            Assert.Equal("ERROR_CODE", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetMonthlyPactDataAsync_WhenHttpExecutorThrowsException_ReturnsInternalError()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _http.GetAsync<List<MonthlyPactRes>>(Arg.Any<string>()).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetMonthlyPactDataAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve monthly pact data", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetMonthlyPactDataAsync_WhenMapperThrowsException_ReturnsInternalError()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<MonthlyPactRes>> { Success = true, Data = new List<MonthlyPactRes>() };

            _http.GetAsync<List<MonthlyPactRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<MonthlyPactDto>>>(apiResponse).Throws(new AutoMapperMappingException("Mapping failed"));

            // Act
            var result = await _client.GetMonthlyPactDataAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve monthly pact data", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetMonthlyPactDataAsync_EnsuresCorrectApiEndpoint_CallsWithCorrectUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedUrlBase = string.Format(PimsApiEndpoints.GetMonthlyPactData, Project, Year);
            var apiResponse = new ApiResponse<List<MonthlyPactRes>> { Success = true, Data = new List<MonthlyPactRes>() };
            var mappedDto = ApiResponseDto<List<MonthlyPactDto>>.SuccessResponse(new List<MonthlyPactDto>());

            _http.GetAsync<List<MonthlyPactRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<MonthlyPactDto>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetMonthlyPactDataAsync(Project, Year, query);

            // Assert
            await _http.Received(1).GetAsync<List<MonthlyPactRes>>(
                Arg.Is<string>(s => s.StartsWith(expectedUrlBase)));
        }

        #endregion

        #region GetFpsYearTotalsAsync Tests

        [Fact]
        public async Task GetFpsYearTotalsAsync_WithSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            var url = string.Format(PimsApiEndpoints.GetFpsYearTotals, Project, Year);
            var res = new FpsYearTotalsRes
            {
                Year = Year,
                Parentproject = Project,
                Totalcosts = 5000.0,
                Custincome = 6000m,
                Transferincome = 500m,
                Totalincome = 6500m
            };
            var apiResponse = new ApiResponse<FpsYearTotalsRes> { Success = true, Data = res };
            var mappedDto = ApiResponseDto<FpsYearTotalsDto>.SuccessResponse(new FpsYearTotalsDto
            {
                Year = Year,
                Parentproject = Project,
                Totalcosts = 5000.0,
                Custincome = 6000m
            });

            _http.GetAsync<FpsYearTotalsRes>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<FpsYearTotalsDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetFpsYearTotalsAsync(Project, Year);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(Project, result.Data.Parentproject);
            Assert.Equal(Year, result.Data.Year);
            await _http.Received(1).GetAsync<FpsYearTotalsRes>(url);
            _mapper.Received(1).Map<ApiResponseDto<FpsYearTotalsDto>>(apiResponse);
        }

        [Fact]
        public async Task GetFpsYearTotalsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var url = string.Format(PimsApiEndpoints.GetFpsYearTotals, Project, Year);
            var errors = new List<ApiError> { new ApiError { Message = "Not found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<FpsYearTotalsRes> { Success = false, Data = null, Errors = errors };
            var mappedDto = new ApiResponseDto<FpsYearTotalsDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Not found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<FpsYearTotalsRes>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<FpsYearTotalsDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetFpsYearTotalsAsync(Project, Year);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Not found", result.Errors[0].Message);
            Assert.Equal("NOT_FOUND", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetFpsYearTotalsAsync_WhenHttpExecutorThrowsException_ReturnsInternalError()
        {
            // Arrange
            var url = string.Format(PimsApiEndpoints.GetFpsYearTotals, Project, Year);
            _http.GetAsync<FpsYearTotalsRes>(url).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetFpsYearTotalsAsync(Project, Year);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve FPS year totals", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetFpsYearTotalsAsync_WhenMapperThrowsException_ReturnsInternalError()
        {
            // Arrange
            var url = string.Format(PimsApiEndpoints.GetFpsYearTotals, Project, Year);
            var apiResponse = new ApiResponse<FpsYearTotalsRes> { Success = true, Data = new FpsYearTotalsRes { Parentproject = Project } };

            _http.GetAsync<FpsYearTotalsRes>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<FpsYearTotalsDto>>(apiResponse).Throws(new AutoMapperMappingException("Mapping failed"));

            // Act
            var result = await _client.GetFpsYearTotalsAsync(Project, Year);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve FPS year totals", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetFpsYearTotalsAsync_EnsuresCorrectApiEndpoint_CallsWithCorrectUrl()
        {
            // Arrange
            var expectedUrl = string.Format(PimsApiEndpoints.GetFpsYearTotals, Project, Year);
            var apiResponse = new ApiResponse<FpsYearTotalsRes> { Success = true, Data = new FpsYearTotalsRes() };
            var mappedDto = ApiResponseDto<FpsYearTotalsDto>.SuccessResponse(new FpsYearTotalsDto());

            _http.GetAsync<FpsYearTotalsRes>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<FpsYearTotalsDto>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetFpsYearTotalsAsync(Project, Year);

            // Assert
            await _http.Received(1).GetAsync<FpsYearTotalsRes>(Arg.Is<string>(s => s == expectedUrl));
        }

        #endregion

        #region ExportProjectYearCostsToExcelAsync Tests

        [Fact]
        public async Task ExportProjectYearCostsToExcelAsync_WithValidParameters_ReturnsByteArray()
        {
            // Arrange
            var expectedBytes = new byte[] { 1, 2, 3, 4, 5 };
            var url = string.Format(PimsApiEndpoints.ExportProjectYearCostsToExcel, Project, Year);

            _http.GetFileAsync(url).Returns(expectedBytes);

            // Act
            var result = await _client.ExportProjectYearCostsToExcelAsync(Project, Year);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedBytes, result);
            await _http.Received(1).GetFileAsync(url);
        }

        [Fact]
        public async Task ExportProjectYearCostsToExcelAsync_EnsuresCorrectApiEndpoint_CallsWithCorrectUrl()
        {
            // Arrange
            var expectedUrl = string.Format(PimsApiEndpoints.ExportProjectYearCostsToExcel, Project, Year);
            _http.GetFileAsync(expectedUrl).Returns(Array.Empty<byte>());

            // Act
            await _client.ExportProjectYearCostsToExcelAsync(Project, Year);

            // Assert
            await _http.Received(1).GetFileAsync(Arg.Is<string>(s => s == expectedUrl));
        }

        [Fact]
        public async Task ExportProjectYearCostsToExcelAsync_WhenHttpExecutorThrowsException_PropagatesException()
        {
            // Arrange
            var url = string.Format(PimsApiEndpoints.ExportProjectYearCostsToExcel, Project, Year);
            _http.GetFileAsync(url).ThrowsAsync(new Exception("Network error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _client.ExportProjectYearCostsToExcelAsync(Project, Year));
        }

        #endregion
    }
}