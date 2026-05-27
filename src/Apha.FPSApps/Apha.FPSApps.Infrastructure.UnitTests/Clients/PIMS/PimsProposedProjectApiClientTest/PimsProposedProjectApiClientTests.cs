using Apha.Common.Constants;
using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PIMS.PimsProposedProjectApiClientTest
{
    public class PimsProposedProjectApiClientTests
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PimsProposedProjectApiClient _client;

        public PimsProposedProjectApiClientTests()
        {
            _http = Substitute.For<IPimsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PimsProposedProjectApiClient(_http, _mapper);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_InitializesClient()
        {
            var client = new PimsProposedProjectApiClient(_http, _mapper);
            Assert.NotNull(client);
        }

        #endregion

        #region CreateProposedProjectAsync Tests

        [Fact]
        public async Task CreateProjectAsync_WithSuccessResponse_ReturnsMappedProposedProject()
        {
            // Arrange
            var dto = new ProposedProjectDto { Parentproject = "PP001", Projecttitle = "New Project" };
            var request = new ProposedProjectReq { Parentproject = "PP001", Projecttitle = "New Project" };
            var proposedProjectRes = new ProposedProjectRes { Id = 1, Parentproject = "PP001", Projecttitle = "New Project" };
            var apiResponse = new ApiResponse<ProposedProjectRes> { Success = true, Data = proposedProjectRes };
            var mappedDto = ApiResponseDto<ProposedProjectDto>.SuccessResponse(
                new ProposedProjectDto { Id = 1, Parentproject = "PP001", Projecttitle = "New Project" }
            );

            _mapper.Map<ProposedProjectReq>(dto).Returns(request);
            _http.PostAsync<ProposedProjectReq, ProposedProjectRes>(PimsApiEndpoints.CreateProject, request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProposedProjectDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.CreateProposedProjectAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("PP001", result.Data.Parentproject);
            Assert.Equal("New Project", result.Data.Projecttitle);
            _mapper.Received(1).Map<ProposedProjectReq>(dto);
            await _http.Received(1).PostAsync<ProposedProjectReq, ProposedProjectRes>(PimsApiEndpoints.CreateProject, request);
            _mapper.Received(1).Map<ApiResponseDto<ProposedProjectDto>>(apiResponse);
        }

        [Fact]
        public async Task CreateProjectAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new ProposedProjectDto { Parentproject = "PP001", Projecttitle = "New Project" };
            var request = new ProposedProjectReq { Parentproject = "PP001", Projecttitle = "New Project" };
            var errors = new List<ApiError>
            {
                new ApiError { Message = "Validation error", Code = "VALIDATION_ERROR" }
            };
            var apiResponse = new ApiResponse<ProposedProjectRes> { Success = false, Data = null, Errors = errors };
            var mappedDto = new ApiResponseDto<ProposedProjectDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Validation error", Code = "VALIDATION_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<ProposedProjectReq>(dto).Returns(request);
            _http.PostAsync<ProposedProjectReq, ProposedProjectRes>(PimsApiEndpoints.CreateProject, request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProposedProjectDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.CreateProposedProjectAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Validation error", result.Errors[0].Message);
            await _http.Received(1).PostAsync<ProposedProjectReq, ProposedProjectRes>(PimsApiEndpoints.CreateProject, request);
        }

        [Fact]
        public async Task CreateProjectAsync_WhenHttpExecutorThrowsException_ReturnsInternalError()
        {
            // Arrange
            var dto = new ProposedProjectDto { Parentproject = "PP001" };
            var request = new ProposedProjectReq { Parentproject = "PP001" };

            _mapper.Map<ProposedProjectReq>(dto).Returns(request);
            _http.PostAsync<ProposedProjectReq, ProposedProjectRes>(PimsApiEndpoints.CreateProject, request)
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.CreateProposedProjectAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to create project", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task CreateProjectAsync_WhenMapperThrowsExceptionOnRequestMapping_ReturnsInternalError()
        {
            // Arrange
            var dto = new ProposedProjectDto { Parentproject = "PP001" };
            _mapper.Map<ProposedProjectReq>(dto).Throws(new AutoMapperMappingException("Mapping failed"));

            // Act
            var result = await _client.CreateProposedProjectAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to create project", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task CreateProjectAsync_EnsuresCorrectApiEndpoint_CallsPostWithCorrectUrl()
        {
            // Arrange
            var dto = new ProposedProjectDto { Parentproject = "PP001" };
            var request = new ProposedProjectReq { Parentproject = "PP001" };
            var apiResponse = new ApiResponse<ProposedProjectRes> { Success = true, Data = new ProposedProjectRes { Parentproject = "PP001" } };
            var mappedDto = ApiResponseDto<ProposedProjectDto>.SuccessResponse(new ProposedProjectDto { Parentproject = "PP001" });

            _mapper.Map<ProposedProjectReq>(dto).Returns(request);
            _http.PostAsync<ProposedProjectReq, ProposedProjectRes>(PimsApiEndpoints.CreateProject, request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProposedProjectDto>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.CreateProposedProjectAsync(dto);

            // Assert
            await _http.Received(1).PostAsync<ProposedProjectReq, ProposedProjectRes>(
                Arg.Is<string>(s => s == PimsApiEndpoints.CreateProject),
                Arg.Any<ProposedProjectReq>()
            );
        }

        #endregion

        #region GetProjectProgramsAsync Tests

        [Fact]
        public async Task GetProjectProgramsAsync_WithSuccessResponseAndData_ReturnsMappedProgramList()
        {
            // Arrange
            var programList = new List<string> { "Program A", "Program B" };
            var apiResponse = new ApiResponse<List<string>> { Success = true, Data = programList };
            var mappedDto = ApiResponseDto<List<string>>.SuccessResponse(new List<string> { "Program A", "Program B" });

            _http.GetAsync<List<string>>(PimsApiEndpoints.GetProjectPrograms).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<string>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetProjectProgramsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            Assert.Contains("Program A", result.Data);
            await _http.Received(1).GetAsync<List<string>>(PimsApiEndpoints.GetProjectPrograms);
            _mapper.Received(1).Map<ApiResponseDto<List<string>>>(apiResponse);
        }

        [Fact]
        public async Task GetProjectProgramsAsync_WithSuccessResponseButNullData_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<string>> { Success = true, Data = null };
            var mappedDto = new ApiResponseDto<List<string>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "No data", Code = "NO_DATA" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<string>>(PimsApiEndpoints.GetProjectPrograms).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<string>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetProjectProgramsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            await _http.Received(1).GetAsync<List<string>>(PimsApiEndpoints.GetProjectPrograms);
            _mapper.Received(1).Map<ApiResponseDto<List<string>>>(apiResponse);
        }

        [Fact]
        public async Task GetProjectProgramsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<string>>
            {
                Success = false,
                Data = null,
                Errors = new List<ApiError> { new ApiError { Message = "Programs not found", Code = "NOT_FOUND" } }
            };
            var mappedDto = new ApiResponseDto<List<string>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Programs not found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<string>>(PimsApiEndpoints.GetProjectPrograms).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<string>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetProjectProgramsAsync();

            // Assert
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
            Assert.Equal("Programs not found", result.Errors![0].Message);
        }

        [Fact]
        public async Task GetProjectProgramsAsync_WhenHttpExecutorThrowsException_ReturnsInternalError()
        {
            // Arrange
            _http.GetAsync<List<string>>(PimsApiEndpoints.GetProjectPrograms).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetProjectProgramsAsync();

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Failed to retrieve programs", result.Errors![0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetProjectProgramsAsync_WhenMapperThrowsException_ReturnsInternalError()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<string>> { Success = true, Data = new List<string> { "Program A" } };

            _http.GetAsync<List<string>>(PimsApiEndpoints.GetProjectPrograms).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<string>>>(apiResponse).Throws(new AutoMapperMappingException("Mapping failed"));

            // Act
            var result = await _client.GetProjectProgramsAsync();

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Failed to retrieve programs", result.Errors![0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetProjectProgramsAsync_EnsuresCorrectApiEndpoint_CallsWithCorrectUrl()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<string>> { Success = true, Data = new List<string>() };
            var mappedDto = ApiResponseDto<List<string>>.SuccessResponse(new List<string>());

            _http.GetAsync<List<string>>(PimsApiEndpoints.GetProjectPrograms).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<string>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetProjectProgramsAsync();

            // Assert
            await _http.Received(1).GetAsync<List<string>>(Arg.Is<string>(s => s == PimsApiEndpoints.GetProjectPrograms));
        }

        #endregion

        #region GetProjectCustomersAsync Tests

        [Fact]
        public async Task GetProjectCustomersAsync_WithSuccessResponseAndData_ReturnsMappedCustomerList()
        {
            // Arrange
            var customerList = new List<string> { "Customer A", "Customer B" };
            var apiResponse = new ApiResponse<List<string>> { Success = true, Data = customerList };
            var mappedDto = ApiResponseDto<List<string>>.SuccessResponse(new List<string> { "Customer A", "Customer B" });

            _http.GetAsync<List<string>>(PimsApiEndpoints.GetProjectCustomers).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<string>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetProjectCustomersAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.Count);
            Assert.Contains("Customer A", result.Data);
            await _http.Received(1).GetAsync<List<string>>(PimsApiEndpoints.GetProjectCustomers);
            _mapper.Received(1).Map<ApiResponseDto<List<string>>>(apiResponse);
        }

        [Fact]
        public async Task GetProjectCustomersAsync_WithSuccessResponseButNullData_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<string>> { Success = true, Data = null };
            var mappedDto = new ApiResponseDto<List<string>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "No data", Code = "NO_DATA" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<string>>(PimsApiEndpoints.GetProjectCustomers).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<string>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetProjectCustomersAsync();

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task GetProjectCustomersAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<string>>
            {
                Success = false,
                Data = null,
                Errors = new List<ApiError> { new ApiError { Message = "Customers not found", Code = "NOT_FOUND" } }
            };
            var mappedDto = new ApiResponseDto<List<string>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Customers not found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<string>>(PimsApiEndpoints.GetProjectCustomers).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<string>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetProjectCustomersAsync();

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Customers not found", result.Errors![0].Message);
        }

        [Fact]
        public async Task GetProjectCustomersAsync_WhenHttpExecutorThrowsException_ReturnsInternalError()
        {
            // Arrange
            _http.GetAsync<List<string>>(PimsApiEndpoints.GetProjectCustomers).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetProjectCustomersAsync();

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Failed to retrieve customers", result.Errors![0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetProjectCustomersAsync_WhenMapperThrowsException_ReturnsInternalError()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<string>> { Success = true, Data = new List<string> { "Customer A" } };

            _http.GetAsync<List<string>>(PimsApiEndpoints.GetProjectCustomers).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<string>>>(apiResponse).Throws(new AutoMapperMappingException("Mapping failed"));

            // Act
            var result = await _client.GetProjectCustomersAsync();

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Failed to retrieve customers", result.Errors![0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetProjectCustomersAsync_EnsuresCorrectApiEndpoint_CallsWithCorrectUrl()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<string>> { Success = true, Data = new List<string>() };
            var mappedDto = ApiResponseDto<List<string>>.SuccessResponse(new List<string>());

            _http.GetAsync<List<string>>(PimsApiEndpoints.GetProjectCustomers).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<string>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetProjectCustomersAsync();

            // Assert
            await _http.Received(1).GetAsync<List<string>>(Arg.Is<string>(s => s == PimsApiEndpoints.GetProjectCustomers));
        }

        #endregion

        #region GetProjectStatusesAsync Tests

        [Fact]
        public async Task GetProjectStatusesAsync_WithSuccessResponseAndData_ReturnsMappedStatusList()
        {
            // Arrange
            var statusList = new List<string> { "Active", "Inactive", "Pending" };
            var apiResponse = new ApiResponse<List<string>> { Success = true, Data = statusList };
            var mappedDto = ApiResponseDto<List<string>>.SuccessResponse(new List<string> { "Active", "Inactive", "Pending" });

            _http.GetAsync<List<string>>(PimsApiEndpoints.GetProjectStatuses).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<string>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetProjectStatusesAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Equal(3, result.Data!.Count);
            Assert.Contains("Active", result.Data);
            await _http.Received(1).GetAsync<List<string>>(PimsApiEndpoints.GetProjectStatuses);
            _mapper.Received(1).Map<ApiResponseDto<List<string>>>(apiResponse);
        }

        [Fact]
        public async Task GetProjectStatusesAsync_WithSuccessResponseButNullData_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<string>> { Success = true, Data = null };
            var mappedDto = new ApiResponseDto<List<string>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "No data", Code = "NO_DATA" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<string>>(PimsApiEndpoints.GetProjectStatuses).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<string>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetProjectStatusesAsync();

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task GetProjectStatusesAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<string>>
            {
                Success = false,
                Data = null,
                Errors = new List<ApiError> { new ApiError { Message = "Statuses not found", Code = "NOT_FOUND" } }
            };
            var mappedDto = new ApiResponseDto<List<string>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Statuses not found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<string>>(PimsApiEndpoints.GetProjectStatuses).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<string>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetProjectStatusesAsync();

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Statuses not found", result.Errors![0].Message);
        }

        [Fact]
        public async Task GetProjectStatusesAsync_WhenHttpExecutorThrowsException_ReturnsInternalError()
        {
            // Arrange
            _http.GetAsync<List<string>>(PimsApiEndpoints.GetProjectStatuses).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetProjectStatusesAsync();

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Failed to retrieve statuses", result.Errors![0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetProjectStatusesAsync_WhenMapperThrowsException_ReturnsInternalError()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<string>> { Success = true, Data = new List<string> { "Active" } };

            _http.GetAsync<List<string>>(PimsApiEndpoints.GetProjectStatuses).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<string>>>(apiResponse).Throws(new AutoMapperMappingException("Mapping failed"));

            // Act
            var result = await _client.GetProjectStatusesAsync();

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Failed to retrieve statuses", result.Errors![0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetProjectStatusesAsync_EnsuresCorrectApiEndpoint_CallsWithCorrectUrl()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<string>> { Success = true, Data = new List<string>() };
            var mappedDto = ApiResponseDto<List<string>>.SuccessResponse(new List<string>());

            _http.GetAsync<List<string>>(PimsApiEndpoints.GetProjectStatuses).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<string>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetProjectStatusesAsync();

            // Assert
            await _http.Received(1).GetAsync<List<string>>(Arg.Is<string>(s => s == PimsApiEndpoints.GetProjectStatuses));
        }

        #endregion
    }
}
