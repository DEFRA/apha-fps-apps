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

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PACT.PactTimeCodeValidApiClientTest
{
    public class PactTimeCodeValidApiClientTests
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PactTimeCodeValidApiClient _client;

        public PactTimeCodeValidApiClientTests()
        {
            _http = Substitute.For<IPactHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PactTimeCodeValidApiClient(_http, _mapper);
        }

        #region GetByJobCodeAsync Tests

        [Fact]
        public async Task GetByJobCodeAsync_WithValidParams_ReturnsMappedTimeCodeList()
        {
            // Arrange
            var jobCode = "JC001";
            var parentProject = "PP001";
            var timeCodeList = new List<TimeCodeValidRes>
            {
                new() { TimeCode = "TC001", WorkGroup = "WG001", ParentProject = parentProject, JobCode = jobCode },
                new() { TimeCode = "TC002", WorkGroup = "WG001", ParentProject = parentProject, JobCode = jobCode }
            };
            var apiResponse = new ApiResponse<List<TimeCodeValidRes>> { Success = true, Data = timeCodeList };
            var expectedDto = ApiResponseDto<List<TimeCodeValidDto>>.SuccessResponse(
                new List<TimeCodeValidDto>
                {
                    new() { TimeCode = "TC001", WorkGroup = "WG001", ParentProject = parentProject },
                    new() { TimeCode = "TC002", WorkGroup = "WG001", ParentProject = parentProject }
                }
            );

            _http.GetAsync<List<TimeCodeValidRes>>(Arg.Is<string>(url =>
                url.Contains($"api/timecodevalid/jobcode/{Uri.EscapeDataString(jobCode)}/project/{Uri.EscapeDataString(parentProject)}")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TimeCodeValidDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetByJobCodeAsync(jobCode, parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<TimeCodeValidRes>>(
                Arg.Is<string>(url => url.Contains($"api/timecodevalid/jobcode/{Uri.EscapeDataString(jobCode)}/project/{Uri.EscapeDataString(parentProject)}")));
        }

        [Fact]
        public async Task GetByJobCodeAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<List<TimeCodeValidRes>> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<List<TimeCodeValidDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<TimeCodeValidRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TimeCodeValidDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetByJobCodeAsync("JC001", "PP001");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task GetByJobCodeAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            // Arrange
            _http.GetAsync<List<TimeCodeValidRes>>(Arg.Any<string>()).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetByJobCodeAsync("JC001", "PP001");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal("Failed to retrieve time codes", error.Message);
        }

        #endregion

        #region GetPagedTimeCodesAsync Tests

        [Fact]
        public async Task GetPagedTimeCodesAsync_WithJobCodeAndProject_IncludesBothInUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var jobCode = "JC001";
            var parentProject = "PP001";
            var timeCodeList = new List<TimeCodeValidRes>
            {
                new() { TimeCode = "TC001", WorkGroup = "WG001", ParentProject = parentProject }
            };
            var apiResponse = new ApiResponse<List<TimeCodeValidRes>>
            {
                Success = true,
                Data = timeCodeList,
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };
            var expectedDto = ApiResponseDto<List<TimeCodeValidDto>>.SuccessResponse(
                new List<TimeCodeValidDto> { new() { TimeCode = "TC001", WorkGroup = "WG001", ParentProject = parentProject } },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            );

            _http.GetAsync<List<TimeCodeValidRes>>(Arg.Is<string>(url =>
                url.Contains("api/timecodevalid/paged") &&
                url.Contains($"jobCode={Uri.EscapeDataString(jobCode)}") &&
                url.Contains($"parentProject={Uri.EscapeDataString(parentProject)}")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TimeCodeValidDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedTimeCodesAsync(query, jobCode, parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
        }

        [Fact]
        public async Task GetPagedTimeCodesAsync_WithNullParams_OmitsOptionalParamsFromUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<TimeCodeValidRes>> { Success = true, Data = new List<TimeCodeValidRes>() };
            var expectedDto = ApiResponseDto<List<TimeCodeValidDto>>.SuccessResponse(new List<TimeCodeValidDto>(), new PaginationDto());

            _http.GetAsync<List<TimeCodeValidRes>>(Arg.Is<string>(url => url.Contains("api/timecodevalid/paged"))).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TimeCodeValidDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedTimeCodesAsync(query, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
        }

        [Fact]
        public async Task GetPagedTimeCodesAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _http.GetAsync<List<TimeCodeValidRes>>(Arg.Any<string>()).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetPagedTimeCodesAsync(query, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal("Failed to retrieve paged time codes", error.Message);
        }

        #endregion

        #region CreateTimeCodeValidAsync Tests

        [Fact]
        public async Task CreateTimeCodeValidAsync_WithValidItem_ReturnsMappedCreatedTimeCode()
        {
            // Arrange
            var itemDto = new TimeCodeValidDto { TimeCode = "TC001", WorkGroup = "WG001", ParentProject = "PP001", Active = true };
            var itemReq = new TimeCodeValidReq { TimeCode = "TC001", WorkGroup = "WG001", ParentProject = "PP001" };
            var itemRes = new TimeCodeValidRes { TimeCode = "TC001", WorkGroup = "WG001", ParentProject = "PP001" };
            var apiResponse = new ApiResponse<TimeCodeValidRes> { Success = true, Data = itemRes };
            var expectedDto = ApiResponseDto<TimeCodeValidDto>.SuccessResponse(itemDto);

            _mapper.Map<TimeCodeValidReq>(itemDto).Returns(itemReq);
            _http.PostAsync<TimeCodeValidReq, TimeCodeValidRes>("api/timecodevalid", itemReq).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TimeCodeValidDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.CreateTimeCodeValidAsync(itemDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("TC001", result.Data?.TimeCode);
            await _http.Received(1).PostAsync<TimeCodeValidReq, TimeCodeValidRes>("api/timecodevalid", itemReq);
        }

        [Fact]
        public async Task CreateTimeCodeValidAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var itemDto = new TimeCodeValidDto { TimeCode = "TC001", WorkGroup = "WG001", ParentProject = "PP001" };
            var errors = new List<ApiError> { new() { Message = "Duplicate", Code = "DUPLICATE" } };
            var apiResponse = new ApiResponse<TimeCodeValidRes> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<TimeCodeValidDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Duplicate", Code = "DUPLICATE" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<TimeCodeValidReq>(itemDto).Returns(new TimeCodeValidReq());
            _http.PostAsync<TimeCodeValidReq, TimeCodeValidRes>(Arg.Any<string>(), Arg.Any<TimeCodeValidReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TimeCodeValidDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.CreateTimeCodeValidAsync(itemDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task CreateTimeCodeValidAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            // Arrange
            var itemDto = new TimeCodeValidDto { TimeCode = "TC001", WorkGroup = "WG001", ParentProject = "PP001" };
            _mapper.Map<TimeCodeValidReq>(itemDto).Returns(new TimeCodeValidReq());
            _http.PostAsync<TimeCodeValidReq, TimeCodeValidRes>(Arg.Any<string>(), Arg.Any<TimeCodeValidReq>())
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.CreateTimeCodeValidAsync(itemDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal("Failed to create time code", error.Message);
        }

        #endregion

        #region UpdateTimeCodeValidAsync Tests

        [Fact]
        public async Task UpdateTimeCodeValidAsync_WithValidItem_ReturnsMappedUpdatedTimeCode()
        {
            // Arrange
            var itemDto = new TimeCodeValidDto { TimeCode = "TC001", WorkGroup = "WG001", ParentProject = "PP001", Active = false };
            var itemReq = new TimeCodeValidReq { TimeCode = "TC001", WorkGroup = "WG001", ParentProject = "PP001" };
            var itemRes = new TimeCodeValidRes { TimeCode = "TC001", WorkGroup = "WG001", ParentProject = "PP001" };
            var apiResponse = new ApiResponse<TimeCodeValidRes> { Success = true, Data = itemRes };
            var expectedDto = ApiResponseDto<TimeCodeValidDto>.SuccessResponse(itemDto);

            _mapper.Map<TimeCodeValidReq>(itemDto).Returns(itemReq);
            _http.PutAsync<TimeCodeValidReq, TimeCodeValidRes>("api/timecodevalid", itemReq).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TimeCodeValidDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.UpdateTimeCodeValidAsync(itemDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("TC001", result.Data?.TimeCode);
            await _http.Received(1).PutAsync<TimeCodeValidReq, TimeCodeValidRes>("api/timecodevalid", itemReq);
        }

        [Fact]
        public async Task UpdateTimeCodeValidAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var itemDto = new TimeCodeValidDto { TimeCode = "NONEXISTENT", WorkGroup = "WG001", ParentProject = "PP001" };
            var errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<TimeCodeValidRes> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<TimeCodeValidDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<TimeCodeValidReq>(itemDto).Returns(new TimeCodeValidReq());
            _http.PutAsync<TimeCodeValidReq, TimeCodeValidRes>(Arg.Any<string>(), Arg.Any<TimeCodeValidReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TimeCodeValidDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.UpdateTimeCodeValidAsync(itemDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task UpdateTimeCodeValidAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            // Arrange
            var itemDto = new TimeCodeValidDto { TimeCode = "TC001", WorkGroup = "WG001", ParentProject = "PP001" };
            _mapper.Map<TimeCodeValidReq>(itemDto).Returns(new TimeCodeValidReq());
            _http.PutAsync<TimeCodeValidReq, TimeCodeValidRes>(Arg.Any<string>(), Arg.Any<TimeCodeValidReq>())
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.UpdateTimeCodeValidAsync(itemDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal("Failed to update time code", error.Message);
        }

        #endregion

        #region DeleteTimeCodeValidAsync Tests

        [Fact]
        public async Task DeleteTimeCodeValidAsync_WithValidParams_ReturnsSuccess()
        {
            // Arrange
            var workGroup = "WG001";
            var timeCode = "TC001";
            var parentProject = "PP001";
            var expectedUrl = $"api/timecodevalid/{Uri.EscapeDataString(workGroup)}/{Uri.EscapeDataString(timeCode)}/{Uri.EscapeDataString(parentProject)}";
            var apiResponse = new ApiResponse<bool> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.DeleteTimeCodeValidAsync(workGroup, timeCode, parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _http.Received(1).DeleteAsync<bool>(expectedUrl);
        }

        [Fact]
        public async Task DeleteTimeCodeValidAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
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
            var result = await _client.DeleteTimeCodeValidAsync("WG001", "NONE", "PP001");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task DeleteTimeCodeValidAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            // Arrange
            _http.DeleteAsync<bool>(Arg.Any<string>()).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.DeleteTimeCodeValidAsync("WG001", "TC001", "PP001");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal("Failed to delete time code", error.Message);
        }

        #endregion

        #region DeleteAllByJobCodeAsync Tests

        [Fact]
        public async Task DeleteAllByJobCodeAsync_WithValidParams_ReturnsSuccess()
        {
            // Arrange
            var jobCode = "JC001";
            var parentProject = "PP001";
            var expectedUrl = $"api/timecodevalid/jobcode/{Uri.EscapeDataString(jobCode)}/project/{Uri.EscapeDataString(parentProject)}";
            var apiResponse = new ApiResponse<bool> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.DeleteAllByJobCodeAsync(jobCode, parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _http.Received(1).DeleteAsync<bool>(expectedUrl);
        }

        [Fact]
        public async Task DeleteAllByJobCodeAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new() { Message = "API Error", Code = "API_ERROR" } };
            var apiResponse = new ApiResponse<bool> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.DeleteAsync<bool>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.DeleteAllByJobCodeAsync("JC001", "PP001");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task DeleteAllByJobCodeAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            // Arrange
            _http.DeleteAsync<bool>(Arg.Any<string>()).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.DeleteAllByJobCodeAsync("JC001", "PP001");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal("Failed to delete time codes for job code", error.Message);
        }

        #endregion

        #region CopyWorkGroupAsync Tests

        [Fact]
        public async Task CopyWorkGroupAsync_WithValidParams_ReturnsCopiedTimeCodes()
        {
            // Arrange
            var sourceJobCode = "JC001";
            var targetJobCode = "JC002";
            var parentProject = "PP001";
            var expectedUrl = $"api/timecodevalid/copy?sourceJobCode={Uri.EscapeDataString(sourceJobCode)}&targetJobCode={Uri.EscapeDataString(targetJobCode)}&parentProject={Uri.EscapeDataString(parentProject)}";
            var timeCodeList = new List<TimeCodeValidRes>
            {
                new() { TimeCode = "TC001", WorkGroup = "WG001", ParentProject = parentProject, JobCode = targetJobCode },
                new() { TimeCode = "TC002", WorkGroup = "WG001", ParentProject = parentProject, JobCode = targetJobCode }
            };
            var apiResponse = new ApiResponse<List<TimeCodeValidRes>> { Success = true, Data = timeCodeList };
            var expectedDto = ApiResponseDto<List<TimeCodeValidDto>>.SuccessResponse(
                new List<TimeCodeValidDto>
                {
                    new() { TimeCode = "TC001", WorkGroup = "WG001", ParentProject = parentProject },
                    new() { TimeCode = "TC002", WorkGroup = "WG001", ParentProject = parentProject }
                }
            );

            _http.PostAsync<object, List<TimeCodeValidRes>>(expectedUrl, Arg.Any<object>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TimeCodeValidDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.CopyWorkGroupAsync(sourceJobCode, targetJobCode, parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).PostAsync<object, List<TimeCodeValidRes>>(expectedUrl, Arg.Any<object>());
        }

        [Fact]
        public async Task CopyWorkGroupAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new() { Message = "Copy failed", Code = "COPY_ERROR" } };
            var apiResponse = new ApiResponse<List<TimeCodeValidRes>> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<List<TimeCodeValidDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Copy failed", Code = "COPY_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.PostAsync<object, List<TimeCodeValidRes>>(Arg.Any<string>(), Arg.Any<object>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TimeCodeValidDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.CopyWorkGroupAsync("JC001", "JC002", "PP001");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task CopyWorkGroupAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            // Arrange
            _http.PostAsync<object, List<TimeCodeValidRes>>(Arg.Any<string>(), Arg.Any<object>())
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.CopyWorkGroupAsync("JC001", "JC002", "PP001");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal("Failed to copy work group time codes", error.Message);
        }

        #endregion
    }
}
