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

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PIMS.PimsProjectApiClientTest
{
    public class PimsProjectApiClientTests
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PimsProjectListApiClient _client;

        public PimsProjectApiClientTests()
        {
            _http = Substitute.For<IPimsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PimsProjectListApiClient(_http, _mapper);
        }

        #region GetAllProjectsAsync Tests

        [Fact]
        public async Task GetAllProjectsAsync_WithSuccessResponseAndData_ReturnsMappedProjectList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var filterOption = 2;
            var projectListResList = new List<ProjectListRes>
            {
                new ProjectListRes { Parentproject = "PP001", Program = "Program A", Customer = "Customer A", OnFps = "yes" },
                new ProjectListRes { Parentproject = "PP002", Program = "Program B", Customer = "Customer B", OnFps = "no" }
            };
            var apiResponse = new ApiResponse<List<ProjectListRes>> { Success = true, Data = projectListResList };
            var mappedDto = ApiResponseDto<List<ProjectListViewDto>>.SuccessResponse(new List<ProjectListViewDto>
            {
                new ProjectListViewDto { Parentproject = "PP001", Program = "Program A" },
                new ProjectListViewDto { Parentproject = "PP002", Program = "Program B" }
            });

            _http.GetAsync<List<ProjectListRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectListViewDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetAllProjectsAsync(query, filterOption);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            await _http.Received(1).GetAsync<List<ProjectListRes>>(Arg.Is<string>(s => s.Contains($"showWhichProjects={filterOption}")));
            _mapper.Received(1).Map<ApiResponseDto<List<ProjectListViewDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAllProjectsAsync_WithSuccessResponseButNullData_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var filterOption = 2;
            var apiResponse = new ApiResponse<List<ProjectListRes>> { Success = true, Data = null };
            var mappedDto = new ApiResponseDto<List<ProjectListViewDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "No data", Code = "NO_DATA" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ProjectListRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectListViewDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetAllProjectsAsync(query, filterOption);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.NotNull(result.Meta);
            await _http.Received(1).GetAsync<List<ProjectListRes>>(Arg.Any<string>());
            _mapper.Received(1).Map<ApiResponseDto<List<ProjectListViewDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAllProjectsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var filterOption = 2;
            var errors = new List<ApiError>
            {
                new ApiError { Message = "API Error", Code = "ERROR_CODE" }
            };
            var apiResponse = new ApiResponse<List<ProjectListRes>> { Success = false, Data = null, Errors = errors };
            var mappedDto = new ApiResponseDto<List<ProjectListViewDto>>
            {
                Success = false,
                Data = null,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "ERROR_CODE" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ProjectListRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectListViewDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetAllProjectsAsync(query, filterOption);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("API Error", result.Errors[0].Message);
            Assert.Equal("ERROR_CODE", result.Errors[0].Code);
            await _http.Received(1).GetAsync<List<ProjectListRes>>(Arg.Any<string>());
            _mapper.Received(1).Map<ApiResponseDto<List<ProjectListViewDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAllProjectsAsync_WithCustomFilterOption_AppendsCorrectQueryParameter()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var filterOption = 1;
            var apiResponse = new ApiResponse<List<ProjectListRes>> { Success = true, Data = new List<ProjectListRes>() };
            var mappedDto = ApiResponseDto<List<ProjectListViewDto>>.SuccessResponse(new List<ProjectListViewDto>());

            _http.GetAsync<List<ProjectListRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectListViewDto>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetAllProjectsAsync(query, filterOption);

            // Assert
            await _http.Received(1).GetAsync<List<ProjectListRes>>(
                Arg.Is<string>(s => s.Contains($"showWhichProjects={filterOption}"))
            );
        }

        [Fact]
        public async Task GetAllProjectsAsync_EnsuresCorrectApiEndpoint_CallsWithCorrectUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var filterOption = 2;
            var apiResponse = new ApiResponse<List<ProjectListRes>> { Success = true, Data = new List<ProjectListRes>() };
            var mappedDto = ApiResponseDto<List<ProjectListViewDto>>.SuccessResponse(new List<ProjectListViewDto>());

            _http.GetAsync<List<ProjectListRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectListViewDto>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetAllProjectsAsync(query, filterOption);

            // Assert
            await _http.Received(1).GetAsync<List<ProjectListRes>>(
                Arg.Is<string>(s => s.StartsWith(PimsApiEndpoints.GetAllProjects))
            );
        }

        #endregion

        #region GetAllProjectsListAsync Tests

        [Fact]
        public async Task GetAllProjectsListAsync_WithSuccessResponseAndData_ReturnsMappedProjectList()
        {
            // Arrange
            var projectListResList = new List<ProjectListRes>
            {
                new ProjectListRes { Parentproject = "PP001", Program = "Program A", Customer = "Customer A", OnFps = "yes" },
                new ProjectListRes { Parentproject = "PP002", Program = "Program B", Customer = "Customer B", OnFps = "no" },
                new ProjectListRes { Parentproject = "PP003", Program = "Program C", Customer = "Customer C", OnFps = "yes" }
            };
            var apiResponse = new ApiResponse<List<ProjectListRes>> { Success = true, Data = projectListResList };
            var mappedDto = ApiResponseDto<List<ProjectListViewDto>>.SuccessResponse(new List<ProjectListViewDto>
            {
                new ProjectListViewDto { Parentproject = "PP001", Program = "Program A" },
                new ProjectListViewDto { Parentproject = "PP002", Program = "Program B" },
                new ProjectListViewDto { Parentproject = "PP003", Program = "Program C" }
            });

            _http.GetAsync<List<ProjectListRes>>(PimsApiEndpoints.GetAllProjectsList).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectListViewDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetAllProjectsListAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(3, result.Data.Count);
            await _http.Received(1).GetAsync<List<ProjectListRes>>(PimsApiEndpoints.GetAllProjectsList);
            _mapper.Received(1).Map<ApiResponseDto<List<ProjectListViewDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAllProjectsListAsync_WithSuccessResponseButNullData_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<ProjectListRes>> { Success = true, Data = null };
            var mappedDto = new ApiResponseDto<List<ProjectListViewDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "No data", Code = "NO_DATA" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ProjectListRes>>(PimsApiEndpoints.GetAllProjectsList).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectListViewDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetAllProjectsListAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.NotNull(result.Meta);
            await _http.Received(1).GetAsync<List<ProjectListRes>>(PimsApiEndpoints.GetAllProjectsList);
            _mapper.Received(1).Map<ApiResponseDto<List<ProjectListViewDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAllProjectsListAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError>
            {
                new ApiError { Message = "API Error", Code = "ERROR_CODE" }
            };
            var apiResponse = new ApiResponse<List<ProjectListRes>> { Success = false, Data = null, Errors = errors };
            var mappedDto = new ApiResponseDto<List<ProjectListViewDto>>
            {
                Success = false,
                Data = null,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "ERROR_CODE" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ProjectListRes>>(PimsApiEndpoints.GetAllProjectsList).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectListViewDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetAllProjectsListAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("API Error", result.Errors[0].Message);
            Assert.Equal("ERROR_CODE", result.Errors[0].Code);
            await _http.Received(1).GetAsync<List<ProjectListRes>>(PimsApiEndpoints.GetAllProjectsList);
            _mapper.Received(1).Map<ApiResponseDto<List<ProjectListViewDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAllProjectsListAsync_WithEmptyList_ReturnsSuccessWithEmptyData()
        {
            // Arrange
            var emptyList = new List<ProjectListRes>();
            var apiResponse = new ApiResponse<List<ProjectListRes>> { Success = true, Data = emptyList };
            var mappedDto = ApiResponseDto<List<ProjectListViewDto>>.SuccessResponse(new List<ProjectListViewDto>());

            _http.GetAsync<List<ProjectListRes>>(PimsApiEndpoints.GetAllProjectsList).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectListViewDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetAllProjectsListAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
            await _http.Received(1).GetAsync<List<ProjectListRes>>(PimsApiEndpoints.GetAllProjectsList);
            _mapper.Received(1).Map<ApiResponseDto<List<ProjectListViewDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAllProjectsListAsync_EnsuresCorrectApiEndpoint_CallsWithCorrectUrl()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<ProjectListRes>> { Success = true, Data = new List<ProjectListRes>() };
            var mappedDto = ApiResponseDto<List<ProjectListViewDto>>.SuccessResponse(new List<ProjectListViewDto>());

            _http.GetAsync<List<ProjectListRes>>(PimsApiEndpoints.GetAllProjectsList).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectListViewDto>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetAllProjectsListAsync();

            // Assert
            await _http.Received(1).GetAsync<List<ProjectListRes>>(
                Arg.Is<string>(s => s == PimsApiEndpoints.GetAllProjectsList)
            );
        }

        #endregion

        #region GetFpsProjectByIdAsync Tests

        [Fact]
        public async Task GetFpsProjectByIdAsync_WithSuccessResponse_ReturnsMappedProject()
        {
            // Arrange
            var parentproject = "PP001";
            var url = string.Format(PimsApiEndpoints.GetFpsProjectById, parentproject);
            var projectRes = new ProjectRes { Parentproject = parentproject, Projecttitle = "Test FPS Project", Projectstatus = "Active" };
            var apiResponse = new ApiResponse<ProjectRes> { Success = true, Data = projectRes };
            var mappedDto = ApiResponseDto<ProjectDto>.SuccessResponse(
                new ProjectDto { Parentproject = parentproject, Projecttitle = "Test FPS Project" }
            );

            _http.GetAsync<ProjectRes>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetFpsProjectByIdAsync(parentproject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(parentproject, result.Data.Parentproject);
            await _http.Received(1).GetAsync<ProjectRes>(url);
            _mapper.Received(1).Map<ApiResponseDto<ProjectDto>>(apiResponse);
        }

        [Fact]
        public async Task GetFpsProjectByIdAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var parentproject = "INVALID";
            var url = string.Format(PimsApiEndpoints.GetFpsProjectById, parentproject);
            var errors = new List<ApiError>
            {
                new ApiError { Message = "Project not found", Code = "NOT_FOUND" }
            };
            var apiResponse = new ApiResponse<ProjectRes> { Success = false, Data = null, Errors = errors };
            var mappedDto = new ApiResponseDto<ProjectDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Project not found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<ProjectRes>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetFpsProjectByIdAsync(parentproject);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Project not found", result.Errors[0].Message);
            await _http.Received(1).GetAsync<ProjectRes>(url);
            _mapper.Received(1).Map<ApiResponseDto<ProjectDto>>(apiResponse);
        }

        [Fact]
        public async Task GetFpsProjectByIdAsync_WhenHttpExecutorThrowsException_ReturnsInternalError()
        {
            // Arrange
            var parentproject = "PP001";
            var url = string.Format(PimsApiEndpoints.GetFpsProjectById, parentproject);
            _http.GetAsync<ProjectRes>(url).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetFpsProjectByIdAsync(parentproject);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve FPS project", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetFpsProjectByIdAsync_WhenMapperThrowsException_ReturnsInternalError()
        {
            // Arrange
            var parentproject = "PP001";
            var url = string.Format(PimsApiEndpoints.GetFpsProjectById, parentproject);
            var apiResponse = new ApiResponse<ProjectRes> { Success = true, Data = new ProjectRes { Parentproject = parentproject } };

            _http.GetAsync<ProjectRes>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectDto>>(apiResponse).Throws(new AutoMapperMappingException("Mapping failed"));

            // Act
            var result = await _client.GetFpsProjectByIdAsync(parentproject);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve FPS project", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetFpsProjectByIdAsync_EnsuresCorrectApiEndpoint_CallsWithCorrectUrl()
        {
            // Arrange
            var parentproject = "PP123";
            var expectedUrl = string.Format(PimsApiEndpoints.GetFpsProjectById, parentproject);
            var apiResponse = new ApiResponse<ProjectRes> { Success = true, Data = new ProjectRes { Parentproject = parentproject } };
            var mappedDto = ApiResponseDto<ProjectDto>.SuccessResponse(new ProjectDto { Parentproject = parentproject });

            _http.GetAsync<ProjectRes>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectDto>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetFpsProjectByIdAsync(parentproject);

            // Assert
            await _http.Received(1).GetAsync<ProjectRes>(Arg.Is<string>(s => s == expectedUrl));
        }

        #endregion

        #region GetProposedProjectByIdAsync Tests

        [Fact]
        public async Task GetProposedProjectByIdAsync_WithSuccessResponse_ReturnsMappedProposedProject()
        {
            // Arrange
            var parentproject = "PP001";
            var url = string.Format(PimsApiEndpoints.GetProposedProjectById, parentproject);
            var proposedProjectRes = new ProposedProjectRes { Id = 1, Parentproject = parentproject, Projecttitle = "Test Proposed Project" };
            var apiResponse = new ApiResponse<ProposedProjectRes> { Success = true, Data = proposedProjectRes };
            var mappedDto = ApiResponseDto<ProposedProjectDto>.SuccessResponse(
                new ProposedProjectDto { Id = 1, Parentproject = parentproject, Projecttitle = "Test Proposed Project" }
            );

            _http.GetAsync<ProposedProjectRes>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProposedProjectDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetProposedProjectByIdAsync(parentproject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(parentproject, result.Data.Parentproject);
            await _http.Received(1).GetAsync<ProposedProjectRes>(url);
            _mapper.Received(1).Map<ApiResponseDto<ProposedProjectDto>>(apiResponse);
        }

        [Fact]
        public async Task GetProposedProjectByIdAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var parentproject = "INVALID";
            var url = string.Format(PimsApiEndpoints.GetProposedProjectById, parentproject);
            var errors = new List<ApiError>
            {
                new ApiError { Message = "Proposed project not found", Code = "NOT_FOUND" }
            };
            var apiResponse = new ApiResponse<ProposedProjectRes> { Success = false, Data = null, Errors = errors };
            var mappedDto = new ApiResponseDto<ProposedProjectDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Proposed project not found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<ProposedProjectRes>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProposedProjectDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetProposedProjectByIdAsync(parentproject);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Proposed project not found", result.Errors[0].Message);
            await _http.Received(1).GetAsync<ProposedProjectRes>(url);
            _mapper.Received(1).Map<ApiResponseDto<ProposedProjectDto>>(apiResponse);
        }

        [Fact]
        public async Task GetProposedProjectByIdAsync_WhenHttpExecutorThrowsException_ReturnsInternalError()
        {
            // Arrange
            var parentproject = "PP001";
            var url = string.Format(PimsApiEndpoints.GetProposedProjectById, parentproject);
            _http.GetAsync<ProposedProjectRes>(url).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetProposedProjectByIdAsync(parentproject);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve proposed project", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetProposedProjectByIdAsync_WhenMapperThrowsException_ReturnsInternalError()
        {
            // Arrange
            var parentproject = "PP001";
            var url = string.Format(PimsApiEndpoints.GetProposedProjectById, parentproject);
            var apiResponse = new ApiResponse<ProposedProjectRes> { Success = true, Data = new ProposedProjectRes { Parentproject = parentproject } };

            _http.GetAsync<ProposedProjectRes>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProposedProjectDto>>(apiResponse).Throws(new AutoMapperMappingException("Mapping failed"));

            // Act
            var result = await _client.GetProposedProjectByIdAsync(parentproject);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve proposed project", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetProposedProjectByIdAsync_EnsuresCorrectApiEndpoint_CallsWithCorrectUrl()
        {
            // Arrange
            var parentproject = "PP123";
            var expectedUrl = string.Format(PimsApiEndpoints.GetProposedProjectById, parentproject);
            var apiResponse = new ApiResponse<ProposedProjectRes> { Success = true, Data = new ProposedProjectRes { Parentproject = parentproject } };
            var mappedDto = ApiResponseDto<ProposedProjectDto>.SuccessResponse(new ProposedProjectDto { Parentproject = parentproject });

            _http.GetAsync<ProposedProjectRes>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProposedProjectDto>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetProposedProjectByIdAsync(parentproject);

            // Assert
            await _http.Received(1).GetAsync<ProposedProjectRes>(Arg.Is<string>(s => s == expectedUrl));
        }

        #endregion

        #region GetYearlyDetailsByProjectAsync Tests

        [Fact]
        public async Task GetYearlyDetailsByProjectAsync_WithSuccessResponse_ReturnsMappedYearlyDetails()
        {
            // Arrange
            var parentproject = "PP001";
            var url = string.Format(PimsApiEndpoints.GetYearlyDetailsByProject, parentproject);
            var projectsResList = new List<ProjectsRes>
            {
                new ProjectsRes { Year = 2023, Parentproject = parentproject, Program = "Program A", Customer = "Customer A", Manager = "Manager A" },
                new ProjectsRes { Year = 2024, Parentproject = parentproject, Program = "Program B", Customer = "Customer B", Manager = "Manager B" }
            };
            var apiResponse = new ApiResponse<List<ProjectsRes>> { Success = true, Data = projectsResList };
            var mappedDto = ApiResponseDto<List<ProjectsDto>>.SuccessResponse(new List<ProjectsDto>
            {
                new ProjectsDto { Year = 2023, Parentproject = parentproject },
                new ProjectsDto { Year = 2024, Parentproject = parentproject }
            });

            _http.GetAsync<List<ProjectsRes>>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectsDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetYearlyDetailsByProjectAsync(parentproject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            await _http.Received(1).GetAsync<List<ProjectsRes>>(url);
            _mapper.Received(1).Map<ApiResponseDto<List<ProjectsDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetYearlyDetailsByProjectAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var parentproject = "INVALID";
            var url = string.Format(PimsApiEndpoints.GetYearlyDetailsByProject, parentproject);
            var errors = new List<ApiError>
            {
                new ApiError { Message = "Yearly details not found", Code = "NOT_FOUND" }
            };
            var apiResponse = new ApiResponse<List<ProjectsRes>> { Success = false, Data = null, Errors = errors };
            var mappedDto = new ApiResponseDto<List<ProjectsDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Yearly details not found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ProjectsRes>>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectsDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetYearlyDetailsByProjectAsync(parentproject);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Yearly details not found", result.Errors[0].Message);
            await _http.Received(1).GetAsync<List<ProjectsRes>>(url);
            _mapper.Received(1).Map<ApiResponseDto<List<ProjectsDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetYearlyDetailsByProjectAsync_WhenHttpExecutorThrowsException_ReturnsInternalError()
        {
            // Arrange
            var parentproject = "PP001";
            var url = string.Format(PimsApiEndpoints.GetYearlyDetailsByProject, parentproject);
            _http.GetAsync<List<ProjectsRes>>(url).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetYearlyDetailsByProjectAsync(parentproject);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve yearly details", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetYearlyDetailsByProjectAsync_WhenMapperThrowsException_ReturnsInternalError()
        {
            // Arrange
            var parentproject = "PP001";
            var url = string.Format(PimsApiEndpoints.GetYearlyDetailsByProject, parentproject);
            var apiResponse = new ApiResponse<List<ProjectsRes>>
            {
                Success = true,
                Data = new List<ProjectsRes> { new ProjectsRes { Parentproject = parentproject } }
            };

            _http.GetAsync<List<ProjectsRes>>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectsDto>>>(apiResponse).Throws(new AutoMapperMappingException("Mapping failed"));

            // Act
            var result = await _client.GetYearlyDetailsByProjectAsync(parentproject);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve yearly details", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetYearlyDetailsByProjectAsync_EnsuresCorrectApiEndpoint_CallsWithCorrectUrl()
        {
            // Arrange
            var parentproject = "PP123";
            var expectedUrl = string.Format(PimsApiEndpoints.GetYearlyDetailsByProject, parentproject);
            var apiResponse = new ApiResponse<List<ProjectsRes>> { Success = true, Data = new List<ProjectsRes>() };
            var mappedDto = ApiResponseDto<List<ProjectsDto>>.SuccessResponse(new List<ProjectsDto>());

            _http.GetAsync<List<ProjectsRes>>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectsDto>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetYearlyDetailsByProjectAsync(parentproject);

            // Assert
            await _http.Received(1).GetAsync<List<ProjectsRes>>(Arg.Is<string>(s => s == expectedUrl));
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_InitializesClient()
        {
            // Arrange & Act
            var client = new PimsProjectListApiClient(_http, _mapper);

            // Assert
            Assert.NotNull(client);
        }

        #endregion
    }
}
