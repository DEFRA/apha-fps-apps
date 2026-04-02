using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using Apha.FPSApps.Infrastructure.Integrations.PACTApis.Clients;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PACT.PactJobCodeApiClientTest
{
    public class PactJobCodeApiClientTests
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PactJobCodeApiClient _client;

        public PactJobCodeApiClientTests()
        {
            _http = Substitute.For<IPactHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PactJobCodeApiClient(_http, _mapper);
        }

        #region GetJobCodesByProjectAsync Tests

        [Fact]
        public async Task GetJobCodesByProjectAsync_WithValidProject_ReturnsMappedJobCodeList()
        {
            // Arrange
            var parentProject = "PP001";
            var jobCodeList = new List<JobCodeRes>
            {
                new() { JobCodeId = "JC001", ParentProject = parentProject, JobCodeName = "Job Code One" },
                new() { JobCodeId = "JC002", ParentProject = parentProject, JobCodeName = "Job Code Two" }
            };
            var apiResponse = new ApiResponse<List<JobCodeRes>> { Success = true, Data = jobCodeList };
            var expectedDto = ApiResponseDto<List<JobCodeDto>>.SuccessResponse(
                new List<JobCodeDto>
                {
                    new() { JobCodeId = "JC001", ParentProject = parentProject },
                    new() { JobCodeId = "JC002", ParentProject = parentProject }
                }
            );

            _http.GetAsync<List<JobCodeRes>>(Arg.Is<string>(url => url.Contains($"api/jobcode/project/{Uri.EscapeDataString(parentProject)}")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<JobCodeDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetJobCodesByProjectAsync(parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<JobCodeRes>>(
                Arg.Is<string>(url => url.Contains($"api/jobcode/project/{Uri.EscapeDataString(parentProject)}")));
        }

        [Fact]
        public async Task GetJobCodesByProjectAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<List<JobCodeRes>> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<List<JobCodeDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<JobCodeRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<JobCodeDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetJobCodesByProjectAsync("PP001");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task GetJobCodesByProjectAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            // Arrange
            _http.GetAsync<List<JobCodeRes>>(Arg.Any<string>()).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetJobCodesByProjectAsync("PP001");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal("Failed to retrieve job codes", error.Message);
        }

        #endregion

        #region GetPagedJobCodesAsync Tests

        [Fact]
        public async Task GetPagedJobCodesAsync_WithParentProject_IncludesProjectInUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var parentProject = "PP001";
            var jobCodeList = new List<JobCodeRes> { new() { JobCodeId = "JC001" } };
            var apiResponse = new ApiResponse<List<JobCodeRes>>
            {
                Success = true,
                Data = jobCodeList,
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };
            var expectedDto = ApiResponseDto<List<JobCodeDto>>.SuccessResponse(
                new List<JobCodeDto> { new() { JobCodeId = "JC001" } },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            );

            _http.GetAsync<List<JobCodeRes>>(Arg.Is<string>(url =>
                url.Contains("api/jobcode/paged") && url.Contains($"parentProject={Uri.EscapeDataString(parentProject)}")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<JobCodeDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedJobCodesAsync(query, parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _http.Received(1).GetAsync<List<JobCodeRes>>(
                Arg.Is<string>(url => url.Contains("api/jobcode/paged") && url.Contains($"parentProject={Uri.EscapeDataString(parentProject)}")));
        }

        [Fact]
        public async Task GetPagedJobCodesAsync_WithNullParentProject_OmitsProjectFromUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<JobCodeRes>> { Success = true, Data = new List<JobCodeRes>() };
            var expectedDto = ApiResponseDto<List<JobCodeDto>>.SuccessResponse(new List<JobCodeDto>(), new PaginationDto());

            _http.GetAsync<List<JobCodeRes>>(Arg.Is<string>(url => url.Contains("api/jobcode/paged"))).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<JobCodeDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedJobCodesAsync(query, null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<JobCodeRes>>(Arg.Is<string>(url => url.Contains("api/jobcode/paged")));
        }

        [Fact]
        public async Task GetPagedJobCodesAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _http.GetAsync<List<JobCodeRes>>(Arg.Any<string>()).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetPagedJobCodesAsync(query, null);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal("Failed to retrieve paged job codes", error.Message);
        }

        #endregion

        #region GetJobCodeByIdAsync Tests

        [Fact]
        public async Task GetJobCodeByIdAsync_WithValidId_ReturnsMappedJobCode()
        {
            // Arrange
            var jobCodeId = "JC001";
            var jobCodeRes = new JobCodeRes { JobCodeId = jobCodeId, JobCodeName = "Test Job Code" };
            var apiResponse = new ApiResponse<JobCodeRes> { Success = true, Data = jobCodeRes };
            var expectedDto = ApiResponseDto<JobCodeDto>.SuccessResponse(
                new JobCodeDto { JobCodeId = jobCodeId, JobCodeName = "Test Job Code" }
            );

            _http.GetAsync<JobCodeRes>($"api/jobcode/{Uri.EscapeDataString(jobCodeId)}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<JobCodeDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetJobCodeByIdAsync(jobCodeId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(jobCodeId, result.Data?.JobCodeId);
            await _http.Received(1).GetAsync<JobCodeRes>($"api/jobcode/{Uri.EscapeDataString(jobCodeId)}");
        }

        [Fact]
        public async Task GetJobCodeByIdAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<JobCodeRes> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<JobCodeDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<JobCodeRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<JobCodeDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetJobCodeByIdAsync("NONEXISTENT");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task GetJobCodeByIdAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            // Arrange
            _http.GetAsync<JobCodeRes>(Arg.Any<string>()).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetJobCodeByIdAsync("JC001");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal("Failed to retrieve job code", error.Message);
        }

        #endregion

        #region GetTypesAsync Tests

        [Fact]
        public async Task GetTypesAsync_WithSuccessResponse_ReturnsMappedTypeList()
        {
            // Arrange
            var types = new List<string> { "TypeA", "TypeB", "TypeC" };
            var apiResponse = new ApiResponse<List<string>> { Success = true, Data = types };
            var expectedDto = ApiResponseDto<List<string>>.SuccessResponse(types);

            _http.GetAsync<List<string>>("api/jobcode/types").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<string>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetTypesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(3, result.Data?.Count);
            await _http.Received(1).GetAsync<List<string>>("api/jobcode/types");
        }

        [Fact]
        public async Task GetTypesAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new() { Message = "API Error", Code = "API_ERROR" } };
            var apiResponse = new ApiResponse<List<string>> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<List<string>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<string>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<string>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetTypesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task GetTypesAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            // Arrange
            _http.GetAsync<List<string>>(Arg.Any<string>()).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetTypesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal("Failed to retrieve job code types", error.Message);
        }

        #endregion

        #region CreateJobCodeAsync Tests

        [Fact]
        public async Task CreateJobCodeAsync_WithValidJobCode_ReturnsMappedCreatedJobCode()
        {
            // Arrange
            var jobCodeDto = new JobCodeDto { JobCodeId = "JC001", ParentProject = "PP001", JobCodeName = "New Job Code" };
            var jobCodeReq = new JobCodeReq { JobCodeId = "JC001", ParentProject = "PP001" };
            var jobCodeRes = new JobCodeRes { JobCodeId = "JC001", ParentProject = "PP001" };
            var apiResponse = new ApiResponse<JobCodeRes> { Success = true, Data = jobCodeRes };
            var expectedDto = ApiResponseDto<JobCodeDto>.SuccessResponse(jobCodeDto);

            _mapper.Map<JobCodeReq>(jobCodeDto).Returns(jobCodeReq);
            _http.PostAsync<JobCodeReq, JobCodeRes>("api/jobcode", jobCodeReq).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<JobCodeDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.CreateJobCodeAsync(jobCodeDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("JC001", result.Data?.JobCodeId);
            await _http.Received(1).PostAsync<JobCodeReq, JobCodeRes>("api/jobcode", jobCodeReq);
        }

        [Fact]
        public async Task CreateJobCodeAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var jobCodeDto = new JobCodeDto { JobCodeId = "JC001" };
            var jobCodeReq = new JobCodeReq { JobCodeId = "JC001" };
            var errors = new List<ApiError> { new() { Message = "Duplicate", Code = "DUPLICATE" } };
            var apiResponse = new ApiResponse<JobCodeRes> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<JobCodeDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Duplicate", Code = "DUPLICATE" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<JobCodeReq>(jobCodeDto).Returns(jobCodeReq);
            _http.PostAsync<JobCodeReq, JobCodeRes>(Arg.Any<string>(), Arg.Any<JobCodeReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<JobCodeDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.CreateJobCodeAsync(jobCodeDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task CreateJobCodeAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            // Arrange
            var jobCodeDto = new JobCodeDto { JobCodeId = "JC001" };
            _mapper.Map<JobCodeReq>(jobCodeDto).Returns(new JobCodeReq());
            _http.PostAsync<JobCodeReq, JobCodeRes>(Arg.Any<string>(), Arg.Any<JobCodeReq>())
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.CreateJobCodeAsync(jobCodeDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal("Failed to create job code", error.Message);
        }

        #endregion

        #region UpdateJobCodeAsync Tests

        [Fact]
        public async Task UpdateJobCodeAsync_WithValidJobCode_ReturnsMappedUpdatedJobCode()
        {
            // Arrange
            var jobCodeDto = new JobCodeDto { JobCodeId = "JC001", JobCodeName = "Updated Job Code" };
            var jobCodeReq = new JobCodeReq { JobCodeId = "JC001" };
            var jobCodeRes = new JobCodeRes { JobCodeId = "JC001", JobCodeName = "Updated Job Code" };
            var apiResponse = new ApiResponse<JobCodeRes> { Success = true, Data = jobCodeRes };
            var expectedDto = ApiResponseDto<JobCodeDto>.SuccessResponse(jobCodeDto);

            _mapper.Map<JobCodeReq>(jobCodeDto).Returns(jobCodeReq);
            _http.PutAsync<JobCodeReq, JobCodeRes>("api/jobcode", jobCodeReq).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<JobCodeDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.UpdateJobCodeAsync(jobCodeDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("Updated Job Code", result.Data?.JobCodeName);
            await _http.Received(1).PutAsync<JobCodeReq, JobCodeRes>("api/jobcode", jobCodeReq);
        }

        [Fact]
        public async Task UpdateJobCodeAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var jobCodeDto = new JobCodeDto { JobCodeId = "NONEXISTENT" };
            var jobCodeReq = new JobCodeReq { JobCodeId = "NONEXISTENT" };
            var errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<JobCodeRes> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<JobCodeDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<JobCodeReq>(jobCodeDto).Returns(jobCodeReq);
            _http.PutAsync<JobCodeReq, JobCodeRes>(Arg.Any<string>(), Arg.Any<JobCodeReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<JobCodeDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.UpdateJobCodeAsync(jobCodeDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task UpdateJobCodeAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            // Arrange
            var jobCodeDto = new JobCodeDto { JobCodeId = "JC001" };
            _mapper.Map<JobCodeReq>(jobCodeDto).Returns(new JobCodeReq());
            _http.PutAsync<JobCodeReq, JobCodeRes>(Arg.Any<string>(), Arg.Any<JobCodeReq>())
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.UpdateJobCodeAsync(jobCodeDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal("Failed to update job code", error.Message);
        }

        #endregion

        #region DeleteJobCodeAsync Tests

        [Fact]
        public async Task DeleteJobCodeAsync_WithValidId_ReturnsSuccess()
        {
            // Arrange
            var jobCodeId = "JC001";
            var apiResponse = new ApiResponse<bool> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool>($"api/jobcode/{Uri.EscapeDataString(jobCodeId)}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.DeleteJobCodeAsync(jobCodeId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _http.Received(1).DeleteAsync<bool>($"api/jobcode/{Uri.EscapeDataString(jobCodeId)}");
        }

        [Fact]
        public async Task DeleteJobCodeAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<bool> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.DeleteAsync<bool>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.DeleteJobCodeAsync("NONEXISTENT");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task DeleteJobCodeAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            // Arrange
            _http.DeleteAsync<bool>(Arg.Any<string>()).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.DeleteJobCodeAsync("JC001");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal("Failed to delete job code", error.Message);
        }

        #endregion
    }
}
