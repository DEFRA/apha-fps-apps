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

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PIMS.PimsProjectListApiClientTest
{
    public class PimsProjectListApiClientTests
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PimsProjectListApiClient _client;

        public PimsProjectListApiClientTests()
        {
            _http = Substitute.For<IPimsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PimsProjectListApiClient(_http, _mapper);
        }

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

        #region GetAllProjectsAsync Tests

        [Fact]
        public async Task GetAllProjectsAsync_WhenSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var httpResponse = new ApiResponse<List<ProjectListRes>>
            {
                Success = true,
                Data = [new ProjectListRes { Parentproject = "PP001", Program = "PROG1", Customer = "CUST1", OnFps = "Yes" }]
            };
            var expectedDto = ApiResponseDto<List<ProjectListViewDto>>.SuccessResponse(
                [new ProjectListViewDto { Parentproject = "PP001", Program = "PROG1", Customer = "CUST1", OnFps = "Yes" }]);

            _http.GetAsync<List<ProjectListRes>>(Arg.Any<string>()).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<ProjectListViewDto>>>(httpResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllProjectsAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            Assert.Equal("PP001", result.Data![0].Parentproject);
            await _http.Received(1).GetAsync<List<ProjectListRes>>(Arg.Is<string>(u => u.Contains(PimsApiEndpoints.GetAllProjects)));
        }

        [Fact]
        public async Task GetAllProjectsAsync_AppendsShowWhichProjectsToUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var httpResponse = new ApiResponse<List<ProjectListRes>> { Success = true, Data = [] };
            var expectedDto = ApiResponseDto<List<ProjectListViewDto>>.SuccessResponse([]);

            _http.GetAsync<List<ProjectListRes>>(Arg.Any<string>()).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<ProjectListViewDto>>>(httpResponse).Returns(expectedDto);

            // Act
            await _client.GetAllProjectsAsync(query, filterOption: 1);

            // Assert
            await _http.Received(1).GetAsync<List<ProjectListRes>>(Arg.Is<string>(u => u.Contains("showWhichProjects=1")));
        }

        [Fact]
        public async Task GetAllProjectsAsync_WhenResponseNotSuccess_ReturnsFailure()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var httpResponse = new ApiResponse<List<ProjectListRes>> { Success = false, Data = null };
            var failDto = ApiResponseDto<List<ProjectListViewDto>>.FailureResponse(
                [new ApiErrorDto { Message = "Error", Code = "ERR" }], new ApiMetaDto());

            _http.GetAsync<List<ProjectListRes>>(Arg.Any<string>()).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<ProjectListViewDto>>>(httpResponse).Returns(failDto);

            // Act
            var result = await _client.GetAllProjectsAsync(query);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetAllProjectsListAsync Tests

        [Fact]
        public async Task GetAllProjectsListAsync_WhenSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            var httpResponse = new ApiResponse<List<ProjectListRes>>
            {
                Success = true,
                Data = [new ProjectListRes { Parentproject = "PP001", OnFps = "Yes" }]
            };
            var expectedDto = ApiResponseDto<List<ProjectListViewDto>>.SuccessResponse(
                [new ProjectListViewDto { Parentproject = "PP001", OnFps = "Yes" }]);

            _http.GetAsync<List<ProjectListRes>>(PimsApiEndpoints.GetAllProjectsList).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<ProjectListViewDto>>>(httpResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllProjectsListAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _http.Received(1).GetAsync<List<ProjectListRes>>(PimsApiEndpoints.GetAllProjectsList);
        }

        [Fact]
        public async Task GetAllProjectsListAsync_WhenResponseNotSuccess_ReturnsFailure()
        {
            // Arrange
            var httpResponse = new ApiResponse<List<ProjectListRes>> { Success = false, Data = null };
            var failDto = ApiResponseDto<List<ProjectListViewDto>>.FailureResponse(
                [new ApiErrorDto { Message = "Error", Code = "ERR" }], new ApiMetaDto());

            _http.GetAsync<List<ProjectListRes>>(PimsApiEndpoints.GetAllProjectsList).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<ProjectListViewDto>>>(httpResponse).Returns(failDto);

            // Act
            var result = await _client.GetAllProjectsListAsync();

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            await _http.Received(1).GetAsync<List<ProjectListRes>>(PimsApiEndpoints.GetAllProjectsList);
        }

        [Fact]
        public async Task GetAllProjectsListAsync_CallsCorrectEndpoint()
        {
            // Arrange
            var httpResponse = new ApiResponse<List<ProjectListRes>> { Success = true, Data = [] };
            var expectedDto = ApiResponseDto<List<ProjectListViewDto>>.SuccessResponse([]);

            _http.GetAsync<List<ProjectListRes>>(Arg.Any<string>()).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<ProjectListViewDto>>>(httpResponse).Returns(expectedDto);

            // Act
            await _client.GetAllProjectsListAsync();

            // Assert
            await _http.Received(1).GetAsync<List<ProjectListRes>>(PimsApiEndpoints.GetAllProjectsList);
        }

        #endregion

        #region GetAllProjectsForMilestoneAsync Tests

        [Fact]
        public async Task GetAllProjectsForMilestoneAsync_WhenSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            var httpResponse = new ApiResponse<List<ProjectListMilestoneRes>>
            {
                Success = true,
                Data = [new ProjectListMilestoneRes { Parentproject = "PP001", Program = "PROG1", Customer = "CUST1", ProjectGroup = "GRP1" }]
            };
            var expectedDto = ApiResponseDto<List<ProjectListMilestoneDto>>.SuccessResponse(
                [new ProjectListMilestoneDto { Parentproject = "PP001", Program = "PROG1", Customer = "CUST1", ProjectGroup = "GRP1" }]);

            _http.GetAsync<List<ProjectListMilestoneRes>>(PimsApiEndpoints.GetAllProjectsMilestone).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<ProjectListMilestoneDto>>>(httpResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllProjectsForMilestoneAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            Assert.Equal("GRP1", result.Data![0].ProjectGroup);
            await _http.Received(1).GetAsync<List<ProjectListMilestoneRes>>(PimsApiEndpoints.GetAllProjectsMilestone);
        }

        [Fact]
        public async Task GetAllProjectsForMilestoneAsync_WhenResponseNotSuccess_ReturnsFailure()
        {
            // Arrange
            var httpResponse = new ApiResponse<List<ProjectListMilestoneRes>> { Success = false, Data = null };
            var failDto = ApiResponseDto<List<ProjectListMilestoneDto>>.FailureResponse(
                [new ApiErrorDto { Message = "Error", Code = "ERR" }], new ApiMetaDto());

            _http.GetAsync<List<ProjectListMilestoneRes>>(PimsApiEndpoints.GetAllProjectsMilestone).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<ProjectListMilestoneDto>>>(httpResponse).Returns(failDto);

            // Act
            var result = await _client.GetAllProjectsForMilestoneAsync();

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            await _http.Received(1).GetAsync<List<ProjectListMilestoneRes>>(PimsApiEndpoints.GetAllProjectsMilestone);
        }

        [Fact]
        public async Task GetAllProjectsForMilestoneAsync_CallsCorrectEndpoint()
        {
            // Arrange
            var httpResponse = new ApiResponse<List<ProjectListMilestoneRes>> { Success = true, Data = [] };
            var expectedDto = ApiResponseDto<List<ProjectListMilestoneDto>>.SuccessResponse([]);

            _http.GetAsync<List<ProjectListMilestoneRes>>(Arg.Any<string>()).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<ProjectListMilestoneDto>>>(httpResponse).Returns(expectedDto);

            // Act
            await _client.GetAllProjectsForMilestoneAsync();

            // Assert
            await _http.Received(1).GetAsync<List<ProjectListMilestoneRes>>(PimsApiEndpoints.GetAllProjectsMilestone);
        }

        #endregion

        #region GetFpsProjectByIdAsync Tests

        [Fact]
        public async Task GetFpsProjectByIdAsync_WhenSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            var parentproject = "PP001";
            var url = string.Format(PimsApiEndpoints.GetFpsProjectById, parentproject);
            var httpResponse = new ApiResponse<ProjectRes>
            {
                Success = true,
                Data = new ProjectRes { Parentproject = parentproject }
            };
            var expectedDto = ApiResponseDto<ProjectDto>.SuccessResponse(new ProjectDto { Parentproject = parentproject });

            _http.GetAsync<ProjectRes>(url).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<ProjectDto>>(httpResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetFpsProjectByIdAsync(parentproject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(parentproject, result.Data!.Parentproject);
            await _http.Received(1).GetAsync<ProjectRes>(url);
        }

        [Fact]
        public async Task GetFpsProjectByIdAsync_WhenResponseNotSuccess_ReturnsFailure()
        {
            // Arrange
            var parentproject = "PP001";
            var url = string.Format(PimsApiEndpoints.GetFpsProjectById, parentproject);
            var httpResponse = new ApiResponse<ProjectRes> { Success = false };
            var failDto = ApiResponseDto<ProjectDto>.FailureResponse(
                [new ApiErrorDto { Message = "Not found", Code = "NOT_FOUND" }], new ApiMetaDto());

            _http.GetAsync<ProjectRes>(url).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<ProjectDto>>(httpResponse).Returns(failDto);

            // Act
            var result = await _client.GetFpsProjectByIdAsync(parentproject);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task GetFpsProjectByIdAsync_WhenHttpThrows_ReturnsInternalErrorResponse()
        {
            // Arrange
            var parentproject = "PP001";
            _http.GetAsync<ProjectRes>(Arg.Any<string>()).Throws(new Exception("Network failure"));

            // Act
            var result = await _client.GetFpsProjectByIdAsync(parentproject);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Contains(result.Errors, e => e.Code == "INTERNAL_ERROR");
        }

        #endregion

        #region GetProposedProjectByIdAsync Tests

        [Fact]
        public async Task GetProposedProjectByIdAsync_WhenSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            var parentproject = "PP001";
            var url = string.Format(PimsApiEndpoints.GetProposedProjectById, parentproject);
            var httpResponse = new ApiResponse<ProposedProjectRes>
            {
                Success = true,
                Data = new ProposedProjectRes { Parentproject = parentproject }
            };
            var expectedDto = ApiResponseDto<ProposedProjectDto>.SuccessResponse(new ProposedProjectDto { Parentproject = parentproject });

            _http.GetAsync<ProposedProjectRes>(url).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<ProposedProjectDto>>(httpResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetProposedProjectByIdAsync(parentproject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(parentproject, result.Data!.Parentproject);
            await _http.Received(1).GetAsync<ProposedProjectRes>(url);
        }

        [Fact]
        public async Task GetProposedProjectByIdAsync_WhenResponseNotSuccess_ReturnsFailure()
        {
            // Arrange
            var parentproject = "PP001";
            var url = string.Format(PimsApiEndpoints.GetProposedProjectById, parentproject);
            var httpResponse = new ApiResponse<ProposedProjectRes> { Success = false };
            var failDto = ApiResponseDto<ProposedProjectDto>.FailureResponse(
                [new ApiErrorDto { Message = "Not found", Code = "NOT_FOUND" }], new ApiMetaDto());

            _http.GetAsync<ProposedProjectRes>(url).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<ProposedProjectDto>>(httpResponse).Returns(failDto);

            // Act
            var result = await _client.GetProposedProjectByIdAsync(parentproject);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task GetProposedProjectByIdAsync_WhenHttpThrows_ReturnsInternalErrorResponse()
        {
            // Arrange
            var parentproject = "PP001";
            _http.GetAsync<ProposedProjectRes>(Arg.Any<string>()).Throws(new Exception("Network failure"));

            // Act
            var result = await _client.GetProposedProjectByIdAsync(parentproject);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Contains(result.Errors, e => e.Code == "INTERNAL_ERROR");
        }

        #endregion

        #region GetYearlyDetailsByProjectAsync Tests

        [Fact]
        public async Task GetYearlyDetailsByProjectAsync_WhenSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            var parentproject = "PP001";
            var url = string.Format(PimsApiEndpoints.GetYearlyDetailsByProject, parentproject);
            var httpResponse = new ApiResponse<List<ProjectsRes>>
            {
                Success = true,
                Data = [new ProjectsRes { Year = 2024, Parentproject = parentproject }]
            };
            var expectedDto = ApiResponseDto<List<ProjectsDto>>.SuccessResponse(
                [new ProjectsDto { Year = 2024, Parentproject = parentproject }]);

            _http.GetAsync<List<ProjectsRes>>(url).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<ProjectsDto>>>(httpResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetYearlyDetailsByProjectAsync(parentproject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            Assert.Equal(2024, result.Data![0].Year);
            await _http.Received(1).GetAsync<List<ProjectsRes>>(url);
        }

        [Fact]
        public async Task GetYearlyDetailsByProjectAsync_WhenResponseNotSuccess_ReturnsFailure()
        {
            // Arrange
            var parentproject = "PP001";
            var url = string.Format(PimsApiEndpoints.GetYearlyDetailsByProject, parentproject);
            var httpResponse = new ApiResponse<List<ProjectsRes>> { Success = false };
            var failDto = ApiResponseDto<List<ProjectsDto>>.FailureResponse(
                [new ApiErrorDto { Message = "Not found", Code = "NOT_FOUND" }], new ApiMetaDto());

            _http.GetAsync<List<ProjectsRes>>(url).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<ProjectsDto>>>(httpResponse).Returns(failDto);

            // Act
            var result = await _client.GetYearlyDetailsByProjectAsync(parentproject);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task GetYearlyDetailsByProjectAsync_WhenHttpThrows_ReturnsInternalErrorResponse()
        {
            // Arrange
            var parentproject = "PP001";
            _http.GetAsync<List<ProjectsRes>>(Arg.Any<string>()).Throws(new Exception("Network failure"));

            // Act
            var result = await _client.GetYearlyDetailsByProjectAsync(parentproject);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Contains(result.Errors, e => e.Code == "INTERNAL_ERROR");
        }

        #endregion
    }
}
