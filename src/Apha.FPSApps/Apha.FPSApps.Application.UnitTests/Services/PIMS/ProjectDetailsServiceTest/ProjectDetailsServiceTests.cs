using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Application.Services.PIMS;
using NSubstitute;

namespace Apha.FPSApps.Application.UnitTests.Services.PIMS.ProjectDetailsServiceTest
{
    public class ProjectDetailsServiceTests
    {
        private readonly IPimsApiClient _pimsApiClient;
        private readonly IPimsProjectDetailsApiClient _pimsProjectDetailsApiClient;
        private readonly ProjectDetailsService _projectDetailsService;

        public ProjectDetailsServiceTests()
        {
            _pimsApiClient = Substitute.For<IPimsApiClient>();
            _pimsProjectDetailsApiClient = Substitute.For<IPimsProjectDetailsApiClient>();
            _pimsApiClient.PimsProjectDetails.Returns(_pimsProjectDetailsApiClient);
            _projectDetailsService = new ProjectDetailsService(_pimsApiClient);
        }

        #region GetPimsDetailAsync Tests

        [Fact]
        public async Task GetPimsDetailAsync_WithSuccessResponse_ReturnsProjectDetail()
        {
            // Arrange
            var parentproject = "PP001";
            var projectDetail = new ProjectDetailDto
            {
                Parentproject = parentproject,
                Version = "1.0",
                FileRef = "FR001",
                Riskid = 1
            };
            var expectedResponse = ApiResponseDto<ProjectDetailDto>.SuccessResponse(projectDetail);

            _pimsProjectDetailsApiClient.GetPimsDetailAsync(parentproject).Returns(expectedResponse);

            // Act
            var result = await _projectDetailsService.GetPimsDetailAsync(parentproject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(parentproject, result.Data.Parentproject);
            await _pimsProjectDetailsApiClient.Received(1).GetPimsDetailAsync(parentproject);
        }

        [Fact]
        public async Task GetPimsDetailAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var parentproject = "INVALID";
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Project detail not found", Code = "NOT_FOUND" }
            };
            var expectedResponse = ApiResponseDto<ProjectDetailDto>.FailureResponse(errors, new ApiMetaDto());

            _pimsProjectDetailsApiClient.GetPimsDetailAsync(parentproject).Returns(expectedResponse);

            // Act
            var result = await _projectDetailsService.GetPimsDetailAsync(parentproject);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task GetPimsDetailAsync_PassesCorrectParentProject()
        {
            // Arrange
            var parentproject = "PP123";
            var expectedResponse = ApiResponseDto<ProjectDetailDto>.SuccessResponse(new ProjectDetailDto { Parentproject = parentproject });

            _pimsProjectDetailsApiClient.GetPimsDetailAsync(parentproject).Returns(expectedResponse);

            // Act
            await _projectDetailsService.GetPimsDetailAsync(parentproject);

            // Assert
            await _pimsProjectDetailsApiClient.Received(1).GetPimsDetailAsync(parentproject);
        }

        #endregion

        #region SavePimsDetailAsync Tests

        [Fact]
        public async Task SavePimsDetailAsync_WithValidData_ReturnsUpdatedProjectDetail()
        {
            // Arrange
            var parentproject = "PP001";
            var dto = new ProjectDetailDto
            {
                Parentproject = parentproject,
                Version = "2.0",
                FileRef = "FR002",
                Riskid = 2
            };
            var expectedResponse = ApiResponseDto<ProjectDetailDto>.SuccessResponse(dto);

            _pimsProjectDetailsApiClient.SavePimsDetailAsync(parentproject, dto).Returns(expectedResponse);

            // Act
            var result = await _projectDetailsService.SavePimsDetailAsync(parentproject, dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(parentproject, result.Data.Parentproject);
            await _pimsProjectDetailsApiClient.Received(1).SavePimsDetailAsync(parentproject, dto);
        }

        [Fact]
        public async Task SavePimsDetailAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var parentproject = "INVALID";
            var dto = new ProjectDetailDto { Parentproject = parentproject };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Save failed", Code = "SAVE_ERROR" }
            };
            var expectedResponse = ApiResponseDto<ProjectDetailDto>.FailureResponse(errors, new ApiMetaDto());

            _pimsProjectDetailsApiClient.SavePimsDetailAsync(parentproject, dto).Returns(expectedResponse);

            // Act
            var result = await _projectDetailsService.SavePimsDetailAsync(parentproject, dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task SavePimsDetailAsync_PassesCorrectParameters()
        {
            // Arrange
            var parentproject = "PP123";
            var dto = new ProjectDetailDto
            {
                Parentproject = parentproject,
                Version = "1.0",
                Riskid = 3
            };
            var expectedResponse = ApiResponseDto<ProjectDetailDto>.SuccessResponse(dto);

            _pimsProjectDetailsApiClient.SavePimsDetailAsync(parentproject, dto).Returns(expectedResponse);

            // Act
            await _projectDetailsService.SavePimsDetailAsync(parentproject, dto);

            // Assert
            await _pimsProjectDetailsApiClient.Received(1).SavePimsDetailAsync(
                Arg.Is<string>(p => p == parentproject),
                Arg.Is<ProjectDetailDto>(d => d.Parentproject == parentproject && d.Version == "1.0" && d.Riskid == 3)
            );
        }

        #endregion

        #region GetProposedProjectAsync Tests

        [Fact]
        public async Task GetProposedProjectAsync_WithSuccessResponse_ReturnsProposedProject()
        {
            // Arrange
            var parentproject = "PP001";
            var proposedProject = new ProposedProjectDto
            {
                Id = 1,
                Parentproject = parentproject,
                Projecttitle = "Test Proposed Project",
                Projectstatus = "Proposed"
            };
            var expectedResponse = ApiResponseDto<ProposedProjectDto>.SuccessResponse(proposedProject);

            _pimsProjectDetailsApiClient.GetProposedProjectAsync(parentproject).Returns(expectedResponse);

            // Act
            var result = await _projectDetailsService.GetProposedProjectAsync(parentproject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(parentproject, result.Data.Parentproject);
            await _pimsProjectDetailsApiClient.Received(1).GetProposedProjectAsync(parentproject);
        }

        [Fact]
        public async Task GetProposedProjectAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var parentproject = "INVALID";
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Proposed project not found", Code = "NOT_FOUND" }
            };
            var expectedResponse = ApiResponseDto<ProposedProjectDto>.FailureResponse(errors, new ApiMetaDto());

            _pimsProjectDetailsApiClient.GetProposedProjectAsync(parentproject).Returns(expectedResponse);

            // Act
            var result = await _projectDetailsService.GetProposedProjectAsync(parentproject);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task GetProposedProjectAsync_PassesCorrectParentProject()
        {
            // Arrange
            var parentproject = "PP123";
            var expectedResponse = ApiResponseDto<ProposedProjectDto>.SuccessResponse(new ProposedProjectDto { Parentproject = parentproject });

            _pimsProjectDetailsApiClient.GetProposedProjectAsync(parentproject).Returns(expectedResponse);

            // Act
            await _projectDetailsService.GetProposedProjectAsync(parentproject);

            // Assert
            await _pimsProjectDetailsApiClient.Received(1).GetProposedProjectAsync(parentproject);
        }

        #endregion

        #region UpdateProposedProjectAsync Tests

        [Fact]
        public async Task UpdateProposedProjectAsync_WithValidData_ReturnsUpdatedProposedProject()
        {
            // Arrange
            var parentproject = "PP001";
            var dto = new ProposedProjectDto
            {
                Id = 1,
                Parentproject = parentproject,
                Projecttitle = "Updated Proposed Project",
                Projectstatus = "Active"
            };
            var expectedResponse = ApiResponseDto<ProposedProjectDto>.SuccessResponse(dto);

            _pimsProjectDetailsApiClient.UpdateProposedProjectAsync(parentproject, dto).Returns(expectedResponse);

            // Act
            var result = await _projectDetailsService.UpdateProposedProjectAsync(parentproject, dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(parentproject, result.Data.Parentproject);
            await _pimsProjectDetailsApiClient.Received(1).UpdateProposedProjectAsync(parentproject, dto);
        }

        [Fact]
        public async Task UpdateProposedProjectAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var parentproject = "INVALID";
            var dto = new ProposedProjectDto { Parentproject = parentproject };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Update failed", Code = "UPDATE_ERROR" }
            };
            var expectedResponse = ApiResponseDto<ProposedProjectDto>.FailureResponse(errors, new ApiMetaDto());

            _pimsProjectDetailsApiClient.UpdateProposedProjectAsync(parentproject, dto).Returns(expectedResponse);

            // Act
            var result = await _projectDetailsService.UpdateProposedProjectAsync(parentproject, dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task UpdateProposedProjectAsync_PassesCorrectParameters()
        {
            // Arrange
            var parentproject = "PP123";
            var dto = new ProposedProjectDto
            {
                Id = 5,
                Parentproject = parentproject,
                Projecttitle = "My Project",
                Projectstatus = "Active"
            };
            var expectedResponse = ApiResponseDto<ProposedProjectDto>.SuccessResponse(dto);

            _pimsProjectDetailsApiClient.UpdateProposedProjectAsync(parentproject, dto).Returns(expectedResponse);

            // Act
            await _projectDetailsService.UpdateProposedProjectAsync(parentproject, dto);

            // Assert
            await _pimsProjectDetailsApiClient.Received(1).UpdateProposedProjectAsync(
                Arg.Is<string>(p => p == parentproject),
                Arg.Is<ProposedProjectDto>(d => d.Id == 5 && d.Parentproject == parentproject && d.Projectstatus == "Active")
            );
        }

        #endregion

        #region GetAllRiskAsync Tests

        [Fact]
        public async Task GetAllRiskAsync_WithSuccessResponse_ReturnsRiskList()
        {
            // Arrange
            var risks = new List<RiskDto>
            {
                new RiskDto { Riskid = 1, Riskrating = "Low" },
                new RiskDto { Riskid = 2, Riskrating = "Medium" },
                new RiskDto { Riskid = 3, Riskrating = "High" }
            };
            var expectedResponse = ApiResponseDto<List<RiskDto>>.SuccessResponse(risks);

            _pimsProjectDetailsApiClient.GetAllRiskAsync().Returns(expectedResponse);

            // Act
            var result = await _projectDetailsService.GetAllRiskAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(3, result.Data?.Count);
            await _pimsProjectDetailsApiClient.Received(1).GetAllRiskAsync();
        }

        [Fact]
        public async Task GetAllRiskAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<List<RiskDto>>.SuccessResponse(new List<RiskDto>());

            _pimsProjectDetailsApiClient.GetAllRiskAsync().Returns(expectedResponse);

            // Act
            var result = await _projectDetailsService.GetAllRiskAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetAllRiskAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Failed to retrieve risks", Code = "API_ERROR" }
            };
            var expectedResponse = ApiResponseDto<List<RiskDto>>.FailureResponse(errors, new ApiMetaDto());

            _pimsProjectDetailsApiClient.GetAllRiskAsync().Returns(expectedResponse);

            // Act
            var result = await _projectDetailsService.GetAllRiskAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidClient_InitializesService()
        {
            // Arrange & Act
            var service = new ProjectDetailsService(_pimsApiClient);

            // Assert
            Assert.NotNull(service);
        }

        #endregion
    }
}
