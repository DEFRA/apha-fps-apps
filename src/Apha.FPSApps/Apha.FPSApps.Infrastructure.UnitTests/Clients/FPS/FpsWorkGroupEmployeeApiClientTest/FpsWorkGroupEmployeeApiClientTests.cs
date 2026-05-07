using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsWorkGroupEmployeeApiClientTest
{
    public class FpsWorkGroupEmployeeApiClientTests
    {
        private const string DefaultWgGrade = "WG01";
        private const string DefaultPactId  = "PACT001";

        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly FpsWorkGroupEmployeeApiClient _client;

        public FpsWorkGroupEmployeeApiClientTests()
        {
            _http   = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsWorkGroupEmployeeApiClient(_http, _mapper);
        }

        #region GetWorkGroupEmployeeAsync Tests

        [Fact]
        public async Task GetWorkGroupEmployeeAsync_WithSuccessResponse_ReturnsMappedEmployeeList()
        {
            // Arrange
            var query   = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var resList = new List<WorkGroupEmployeeViewRes>
            {
                new() { PactId = DefaultPactId, SpNumber = "SP001", WorkGroupGrade = DefaultWgGrade }
            };
            var apiResponse = new ApiResponse<List<WorkGroupEmployeeViewRes>> { Success = true, Data = resList };
            var dtoList     = new List<WorkGroupEmployeeViewDto>
            {
                new() { PactId = DefaultPactId, SpNumber = "SP001", WorkGroupGrade = DefaultWgGrade }
            };
            var expectedDto = ApiResponseDto<List<WorkGroupEmployeeViewDto>>.SuccessResponse(dtoList);

            _http.GetAsync<List<WorkGroupEmployeeViewRes>>(
                    Arg.Is<string>(url => url.Contains("wgstaff") && url.Contains(DefaultWgGrade)))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<WorkGroupEmployeeViewDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetWorkGroupEmployeeAsync(query, DefaultWgGrade);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _http.Received(1).GetAsync<List<WorkGroupEmployeeViewRes>>(
                Arg.Is<string>(url => url.Contains("wgstaff") && url.Contains(DefaultWgGrade)));
        }

        [Fact]
        public async Task GetWorkGroupEmployeeAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query       = new QueryParameters<string>();
            var apiResponse = new ApiResponse<List<WorkGroupEmployeeViewRes>>
            {
                Success = false,
                Errors  = new List<ApiError> { new() { Message = "API Error", Code = "ERROR" } }
            };
            var mappedResponse = new ApiResponseDto<List<WorkGroupEmployeeViewDto>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "API Error", Code = "ERROR" } },
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<List<WorkGroupEmployeeViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<WorkGroupEmployeeViewDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetWorkGroupEmployeeAsync(query, DefaultWgGrade);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region GetWorkGroupEmployeeByIdAsync Tests

        [Fact]
        public async Task GetWorkGroupEmployeeByIdAsync_WithSuccessResponse_ReturnsMappedEmployee()
        {
            // Arrange
            var res         = new WorkGroupEmployeeRes { PactId = DefaultPactId, SpNumber = "SP001" };
            var apiResponse = new ApiResponse<WorkGroupEmployeeRes> { Success = true, Data = res };
            var dto         = new WorkGroupEmployeeDto { PactId = DefaultPactId };
            var expectedDto = ApiResponseDto<WorkGroupEmployeeDto>.SuccessResponse(dto);

            _http.GetAsync<WorkGroupEmployeeRes>(
                    Arg.Is<string>(url => url.Contains(DefaultPactId)))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<WorkGroupEmployeeDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetWorkGroupEmployeeByIdAsync(DefaultPactId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(DefaultPactId, result.Data?.PactId);
        }

        [Fact]
        public async Task GetWorkGroupEmployeeByIdAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<WorkGroupEmployeeRes>
            {
                Success = false,
                Errors  = new List<ApiError> { new() { Message = "Not found", Code = "NOT_FOUND" } }
            };
            var mappedResponse = new ApiResponseDto<WorkGroupEmployeeDto>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } },
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<WorkGroupEmployeeRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<WorkGroupEmployeeDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetWorkGroupEmployeeByIdAsync(DefaultPactId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region UpdateWorkGroupEmployeeAsync Tests

        [Fact]
        public async Task UpdateWorkGroupEmployeeAsync_WithSuccessResponse_ReturnsMappedEmployee()
        {
            // Arrange
            var dto = new WorkGroupEmployeeDto { PactId = DefaultPactId, HrsPaid = 40.0 };
            var req = new WorkGroupEmployeeReq { PactId = DefaultPactId, HrsPaid = 40.0 };
            var res = new WorkGroupEmployeeRes { PactId = DefaultPactId };
            var apiResponse = new ApiResponse<WorkGroupEmployeeRes> { Success = true, Data = res };
            var updatedDto  = new WorkGroupEmployeeDto { PactId = DefaultPactId };
            var expectedDto = ApiResponseDto<WorkGroupEmployeeDto>.SuccessResponse(updatedDto);

            _mapper.Map<WorkGroupEmployeeReq>(dto).Returns(req);
            _http.PutAsync<WorkGroupEmployeeReq, WorkGroupEmployeeRes>(Arg.Any<string>(), req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<WorkGroupEmployeeDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.UpdateWorkGroupEmployeeAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).PutAsync<WorkGroupEmployeeReq, WorkGroupEmployeeRes>(Arg.Any<string>(), req);
        }

        [Fact]
        public async Task UpdateWorkGroupEmployeeAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new WorkGroupEmployeeDto { PactId = DefaultPactId };
            var req = new WorkGroupEmployeeReq { PactId = DefaultPactId };
            var apiResponse = new ApiResponse<WorkGroupEmployeeRes>
            {
                Success = false,
                Errors  = new List<ApiError> { new() { Message = "Update failed", Code = "ERR" } }
            };
            var mappedResponse = new ApiResponseDto<WorkGroupEmployeeDto>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Update failed", Code = "ERR" } },
                Meta    = new ApiMetaDto()
            };

            _mapper.Map<WorkGroupEmployeeReq>(dto).Returns(req);
            _http.PutAsync<WorkGroupEmployeeReq, WorkGroupEmployeeRes>(Arg.Any<string>(), req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<WorkGroupEmployeeDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.UpdateWorkGroupEmployeeAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region DeleteWorkGroupEmployeeAsync Tests

        [Fact]
        public async Task DeleteWorkGroupEmployeeAsync_WithSuccessResponse_ReturnsSuccess()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool>(Arg.Is<string>(url => url.Contains(DefaultPactId))).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.DeleteWorkGroupEmployeeAsync(DefaultPactId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).DeleteAsync<bool>(Arg.Is<string>(url => url.Contains(DefaultPactId)));
        }

        [Fact]
        public async Task DeleteWorkGroupEmployeeAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool>
            {
                Success = false,
                Errors  = new List<ApiError> { new() { Message = "Delete failed", Code = "ERR" } }
            };
            var mappedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Delete failed", Code = "ERR" } },
                Meta    = new ApiMetaDto()
            };

            _http.DeleteAsync<bool>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.DeleteWorkGroupEmployeeAsync(DefaultPactId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion
    }
}
