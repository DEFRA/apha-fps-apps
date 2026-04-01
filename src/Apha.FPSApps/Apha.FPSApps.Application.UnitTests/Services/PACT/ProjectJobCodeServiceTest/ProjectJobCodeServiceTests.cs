using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.PACT.ProjectJobCodeServiceTest
{
    public class ProjectJobCodeServiceTests
    {
        private readonly IPactApiClient _pactClient;
        private readonly IPactJobCodeApiClient _pactJobCodeApiClient;
        private readonly IPactWorkGroupApiClient _pactWorkGroupApiClient;
        private readonly ProjectJobCodeService _service;

        public ProjectJobCodeServiceTests()
        {
            _pactClient = Substitute.For<IPactApiClient>();
            _pactJobCodeApiClient = Substitute.For<IPactJobCodeApiClient>();
            _pactWorkGroupApiClient = Substitute.For<IPactWorkGroupApiClient>();
            _pactClient.PactJobCode.Returns(_pactJobCodeApiClient);
            _pactClient.PactWorkGroup.Returns(_pactWorkGroupApiClient);
            _service = new ProjectJobCodeService(_pactClient);
        }

        #region GetJobCodesByProjectAsync Tests

        [Fact]
        public async Task GetJobCodesByProjectAsync_WithValidProject_ReturnsJobCodeList()
        {
            // Arrange
            var parentProject = "PP001";
            var jobCodes = new List<JobCodeDto>
            {
                new JobCodeDto { JobCodeId = "JC001", ParentProject = parentProject, JobCodeName = "Job Code One" },
                new JobCodeDto { JobCodeId = "JC002", ParentProject = parentProject, JobCodeName = "Job Code Two" }
            };
            var expectedResponse = ApiResponseDto<List<JobCodeDto>>.SuccessResponse(jobCodes);
            _pactJobCodeApiClient.GetJobCodesByProjectAsync(parentProject).Returns(expectedResponse);

            // Act
            var result = await _service.GetJobCodesByProjectAsync(parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _pactJobCodeApiClient.Received(1).GetJobCodesByProjectAsync(parentProject);
        }

        [Fact]
        public async Task GetJobCodesByProjectAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var parentProject = "PP001";
            var expectedResponse = ApiResponseDto<List<JobCodeDto>>.SuccessResponse(new List<JobCodeDto>());
            _pactJobCodeApiClient.GetJobCodesByProjectAsync(parentProject).Returns(expectedResponse);

            // Act
            var result = await _service.GetJobCodesByProjectAsync(parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetJobCodesByProjectAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var parentProject = "PP001";
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<JobCodeDto>>.FailureResponse(errors, new ApiMetaDto());
            _pactJobCodeApiClient.GetJobCodesByProjectAsync(parentProject).Returns(expectedResponse);

            // Act
            var result = await _service.GetJobCodesByProjectAsync(parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        #endregion

        #region GetPagedJobCodesAsync Tests

        [Fact]
        public async Task GetPagedJobCodesAsync_WithValidQuery_ReturnsPaginatedJobCodes()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, Search = "JC" };
            var parentProject = "PP001";
            var jobCodes = new List<JobCodeDto>
            {
                new JobCodeDto { JobCodeId = "JC001", ParentProject = parentProject }
            };
            var expectedResponse = ApiResponseDto<List<JobCodeDto>>.SuccessResponse(
                jobCodes,
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            );
            _pactJobCodeApiClient.GetPagedJobCodesAsync(query, parentProject).Returns(expectedResponse);

            // Act
            var result = await _service.GetPagedJobCodesAsync(query, parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _pactJobCodeApiClient.Received(1).GetPagedJobCodesAsync(query, parentProject);
        }

        [Fact]
        public async Task GetPagedJobCodesAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<JobCodeDto>>.FailureResponse(errors, new ApiMetaDto());
            _pactJobCodeApiClient.GetPagedJobCodesAsync(query, null).Returns(expectedResponse);

            // Act
            var result = await _service.GetPagedJobCodesAsync(query, null);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetJobCodeByIdAsync Tests

        [Fact]
        public async Task GetJobCodeByIdAsync_WithValidId_ReturnsJobCode()
        {
            // Arrange
            var jobCodeId = "JC001";
            var jobCode = new JobCodeDto { JobCodeId = jobCodeId, JobCodeName = "Test Job Code", ParentProject = "PP001" };
            var expectedResponse = ApiResponseDto<JobCodeDto>.SuccessResponse(jobCode);
            _pactJobCodeApiClient.GetJobCodeByIdAsync(jobCodeId).Returns(expectedResponse);

            // Act
            var result = await _service.GetJobCodeByIdAsync(jobCodeId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(jobCodeId, result.Data?.JobCodeId);
            await _pactJobCodeApiClient.Received(1).GetJobCodeByIdAsync(jobCodeId);
        }

        [Fact]
        public async Task GetJobCodeByIdAsync_WithNonExistentId_ReturnsFailureResponse()
        {
            // Arrange
            var jobCodeId = "NONEXISTENT";
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Job code not found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<JobCodeDto>.FailureResponse(errors, new ApiMetaDto());
            _pactJobCodeApiClient.GetJobCodeByIdAsync(jobCodeId).Returns(expectedResponse);

            // Act
            var result = await _service.GetJobCodeByIdAsync(jobCodeId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetTypesAsync Tests

        [Fact]
        public async Task GetTypesAsync_WithSuccessResponse_ReturnsTypeList()
        {
            // Arrange
            var types = new List<string> { "TypeA", "TypeB", "TypeC" };
            var expectedResponse = ApiResponseDto<List<string>>.SuccessResponse(types);
            _pactJobCodeApiClient.GetTypesAsync().Returns(expectedResponse);

            // Act
            var result = await _service.GetTypesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(3, result.Data?.Count);
            await _pactJobCodeApiClient.Received(1).GetTypesAsync();
        }

        [Fact]
        public async Task GetTypesAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<List<string>>.SuccessResponse(new List<string>());
            _pactJobCodeApiClient.GetTypesAsync().Returns(expectedResponse);

            // Act
            var result = await _service.GetTypesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetTypesAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<string>>.FailureResponse(errors, new ApiMetaDto());
            _pactJobCodeApiClient.GetTypesAsync().Returns(expectedResponse);

            // Act
            var result = await _service.GetTypesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region CreateJobCodeAsync Tests

        [Fact]
        public async Task CreateJobCodeAsync_WithValidJobCode_ReturnsSuccessResponse()
        {
            // Arrange
            var newJobCode = new JobCodeDto
            {
                JobCodeId = "JC001",
                ParentProject = "PP001",
                JobCodeName = "New Job Code",
                Type = "TypeA"
            };
            var expectedResponse = ApiResponseDto<JobCodeDto>.SuccessResponse(newJobCode);
            _pactJobCodeApiClient.CreateJobCodeAsync(newJobCode).Returns(expectedResponse);

            // Act
            var result = await _service.CreateJobCodeAsync(newJobCode);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(newJobCode.JobCodeId, result.Data?.JobCodeId);
            await _pactJobCodeApiClient.Received(1).CreateJobCodeAsync(newJobCode);
        }

        [Fact]
        public async Task CreateJobCodeAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var newJobCode = new JobCodeDto { JobCodeId = "JC001" };
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Duplicate job code", Code = "DUPLICATE" } };
            var expectedResponse = ApiResponseDto<JobCodeDto>.FailureResponse(errors, new ApiMetaDto());
            _pactJobCodeApiClient.CreateJobCodeAsync(newJobCode).Returns(expectedResponse);

            // Act
            var result = await _service.CreateJobCodeAsync(newJobCode);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region UpdateJobCodeAsync Tests

        [Fact]
        public async Task UpdateJobCodeAsync_WithValidJobCode_ReturnsSuccessResponse()
        {
            // Arrange
            var updatedJobCode = new JobCodeDto
            {
                JobCodeId = "JC001",
                ParentProject = "PP001",
                JobCodeName = "Updated Job Code",
                Type = "TypeB"
            };
            var expectedResponse = ApiResponseDto<JobCodeDto>.SuccessResponse(updatedJobCode);
            _pactJobCodeApiClient.UpdateJobCodeAsync(updatedJobCode).Returns(expectedResponse);

            // Act
            var result = await _service.UpdateJobCodeAsync(updatedJobCode);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("Updated Job Code", result.Data?.JobCodeName);
            await _pactJobCodeApiClient.Received(1).UpdateJobCodeAsync(updatedJobCode);
        }

        [Fact]
        public async Task UpdateJobCodeAsync_WithNonExistentJobCode_ReturnsFailureResponse()
        {
            // Arrange
            var jobCode = new JobCodeDto { JobCodeId = "NONEXISTENT" };
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Job code not found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<JobCodeDto>.FailureResponse(errors, new ApiMetaDto());
            _pactJobCodeApiClient.UpdateJobCodeAsync(jobCode).Returns(expectedResponse);

            // Act
            var result = await _service.UpdateJobCodeAsync(jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region DeleteJobCodeAsync Tests

        [Fact]
        public async Task DeleteJobCodeAsync_WithValidId_ReturnsSuccessResponse()
        {
            // Arrange
            var jobCodeId = "JC001";
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _pactJobCodeApiClient.DeleteJobCodeAsync(jobCodeId).Returns(expectedResponse);

            // Act
            var result = await _service.DeleteJobCodeAsync(jobCodeId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _pactJobCodeApiClient.Received(1).DeleteJobCodeAsync(jobCodeId);
        }

        [Fact]
        public async Task DeleteJobCodeAsync_WithNonExistentId_ReturnsFailureResponse()
        {
            // Arrange
            var jobCodeId = "NONEXISTENT";
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Job code not found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _pactJobCodeApiClient.DeleteJobCodeAsync(jobCodeId).Returns(expectedResponse);

            // Act
            var result = await _service.DeleteJobCodeAsync(jobCodeId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetAllWorkGroupsAsync Tests

        [Fact]
        public async Task GetAllWorkGroupsAsync_WithSuccessResponse_ReturnsWorkGroupList()
        {
            // Arrange
            var workGroups = new List<WorkGroupDto>
            {
                new WorkGroupDto { WorkGroupName = "WG001", ProfitCentre = "PC001" },
                new WorkGroupDto { WorkGroupName = "WG002", ProfitCentre = "PC002" }
            };
            var expectedResponse = ApiResponseDto<List<WorkGroupDto>>.SuccessResponse(workGroups);
            _pactWorkGroupApiClient.GetAllWorkGroupsAsync().Returns(expectedResponse);

            // Act
            var result = await _service.GetAllWorkGroupsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _pactWorkGroupApiClient.Received(1).GetAllWorkGroupsAsync();
        }

        [Fact]
        public async Task GetAllWorkGroupsAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<List<WorkGroupDto>>.SuccessResponse(new List<WorkGroupDto>());
            _pactWorkGroupApiClient.GetAllWorkGroupsAsync().Returns(expectedResponse);

            // Act
            var result = await _service.GetAllWorkGroupsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetAllWorkGroupsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<WorkGroupDto>>.FailureResponse(errors, new ApiMetaDto());
            _pactWorkGroupApiClient.GetAllWorkGroupsAsync().Returns(expectedResponse);

            // Act
            var result = await _service.GetAllWorkGroupsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion
    }
}
