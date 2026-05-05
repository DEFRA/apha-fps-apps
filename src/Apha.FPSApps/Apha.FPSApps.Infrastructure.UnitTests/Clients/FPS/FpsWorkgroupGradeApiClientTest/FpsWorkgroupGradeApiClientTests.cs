using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsWorkgroupGradeApiClientTest
{
    public class FpsWorkgroupGradeApiClientTests
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly FpsWorkgroupGradeApiClient _client;

        public FpsWorkgroupGradeApiClientTests()
        {
            _http = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsWorkgroupGradeApiClient(_http, _mapper);
        }

        #region Constructor

        [Fact]
        public void Constructor_NullHttp_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new FpsWorkgroupGradeApiClient(null!, _mapper));
        }

        [Fact]
        public void Constructor_NullMapper_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new FpsWorkgroupGradeApiClient(_http, null!));
        }

        #endregion

        #region GetAllWorkgroupGradesPagedAsync

        [Fact]
        public async Task GetAllWorkgroupGradesPagedAsync_WithSuccessResponse_ReturnsMappedList()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var data = new List<WorkgroupGradeRes>
            {
                new() { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" }
            };
            var apiResponse = new ApiResponse<List<WorkgroupGradeRes>> { Success = true, Data = data };
            var expectedDto = ApiResponseDto<List<WorkgroupGradeDto>>.SuccessResponse(
                new List<WorkgroupGradeDto> { new() { WgGrade = "WG01" } });

            _http.GetAsync<List<WorkgroupGradeRes>>(Arg.Is<string>(url => url.Contains("api/v1/workgroupgrade/paged"))).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<WorkgroupGradeDto>>>(apiResponse).Returns(expectedDto);

            var result = await _client.GetAllWorkgroupGradesPagedAsync(query);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
        }

        [Fact]
        public async Task GetAllWorkgroupGradesPagedAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<WorkgroupGradeRes>>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Error", Code = "ERROR" } }
            };
            var mappedResponse = new ApiResponseDto<List<WorkgroupGradeDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<WorkgroupGradeRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<WorkgroupGradeDto>>>(apiResponse).Returns(mappedResponse);

            var result = await _client.GetAllWorkgroupGradesPagedAsync(query);

            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetAllWorkgroupGradesPagedAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _http.GetAsync<List<WorkgroupGradeRes>>(Arg.Any<string>()).Throws(new Exception("Network error"));

            var result = await _client.GetAllWorkgroupGradesPagedAsync(query);

            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        #endregion

        #region GetByWgGradeAsync

        [Fact]
        public async Task GetByWgGradeAsync_WithSuccessResponse_ReturnsMappedRecord()
        {
            var apiResponse = new ApiResponse<WorkgroupGradeRes>
            {
                Success = true,
                Data = new WorkgroupGradeRes { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" }
            };
            var expectedDto = ApiResponseDto<WorkgroupGradeDto>.SuccessResponse(new WorkgroupGradeDto { WgGrade = "WG01" });

            _http.GetAsync<WorkgroupGradeRes>("api/v1/workgroupgrade/WG01").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<WorkgroupGradeDto>>(apiResponse).Returns(expectedDto);

            var result = await _client.GetByWgGradeAsync("WG01");

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("WG01", result.Data?.WgGrade);
            await _http.Received(1).GetAsync<WorkgroupGradeRes>("api/v1/workgroupgrade/WG01");
        }

        [Fact]
        public async Task GetByWgGradeAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var apiResponse = new ApiResponse<WorkgroupGradeRes>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Not found", Code = "NOT_FOUND" } }
            };
            var mappedResponse = new ApiResponseDto<WorkgroupGradeDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<WorkgroupGradeRes>("api/v1/workgroupgrade/INVALID").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<WorkgroupGradeDto>>(apiResponse).Returns(mappedResponse);

            var result = await _client.GetByWgGradeAsync("INVALID");

            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetByWgGradeAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            _http.GetAsync<WorkgroupGradeRes>(Arg.Any<string>()).Throws(new Exception("Network error"));

            var result = await _client.GetByWgGradeAsync("WG01");

            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        [Theory]
        [InlineData("WG01")]
        [InlineData("WG02")]
        [InlineData("TEST")]
        public async Task GetByWgGradeAsync_WithVariousCodes_CallsCorrectUrl(string wgGrade)
        {
            var apiResponse = new ApiResponse<WorkgroupGradeRes>
            {
                Success = true,
                Data = new WorkgroupGradeRes { WgGrade = wgGrade }
            };
            var expectedDto = ApiResponseDto<WorkgroupGradeDto>.SuccessResponse(new WorkgroupGradeDto { WgGrade = wgGrade });

            _http.GetAsync<WorkgroupGradeRes>($"api/v1/workgroupgrade/{wgGrade}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<WorkgroupGradeDto>>(apiResponse).Returns(expectedDto);

            await _client.GetByWgGradeAsync(wgGrade);

            await _http.Received(1).GetAsync<WorkgroupGradeRes>($"api/v1/workgroupgrade/{wgGrade}");
        }

        #endregion

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_WithSuccessResponse_ReturnsMappedRecord()
        {
            var dto = new WorkgroupGradeDto { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" };
            var req = new WorkgroupGradeReq { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" };
            var apiResponse = new ApiResponse<WorkgroupGradeRes>
            {
                Success = true,
                Data = new WorkgroupGradeRes { WgGrade = "WG01" }
            };
            var expectedDto = ApiResponseDto<WorkgroupGradeDto>.SuccessResponse(dto);

            _mapper.Map<WorkgroupGradeReq>(dto).Returns(req);
            _http.PostAsync<WorkgroupGradeReq, WorkgroupGradeRes>("api/v1/workgroupgrade", req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<WorkgroupGradeDto>>(apiResponse).Returns(expectedDto);

            var result = await _client.CreateAsync(dto);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("WG01", result.Data?.WgGrade);
            await _http.Received(1).PostAsync<WorkgroupGradeReq, WorkgroupGradeRes>("api/v1/workgroupgrade", req);
        }

        [Fact]
        public async Task CreateAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var dto = new WorkgroupGradeDto { WgGrade = "WG01" };
            var req = new WorkgroupGradeReq { WgGrade = "WG01" };
            var apiResponse = new ApiResponse<WorkgroupGradeRes>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Duplicate", Code = "DUPLICATE" } }
            };
            var mappedResponse = new ApiResponseDto<WorkgroupGradeDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Duplicate", Code = "DUPLICATE" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<WorkgroupGradeReq>(dto).Returns(req);
            _http.PostAsync<WorkgroupGradeReq, WorkgroupGradeRes>("api/v1/workgroupgrade", req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<WorkgroupGradeDto>>(apiResponse).Returns(mappedResponse);

            var result = await _client.CreateAsync(dto);

            Assert.False(result.Success);
        }

        [Fact]
        public async Task CreateAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            var dto = new WorkgroupGradeDto { WgGrade = "WG01" };
            _mapper.Map<WorkgroupGradeReq>(dto).Throws(new Exception("Mapping error"));

            var result = await _client.CreateAsync(dto);

            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_WithSuccessResponse_ReturnsMappedRecord()
        {
            var dto = new WorkgroupGradeDto { WgGrade = "WG01", ProfitCentreGrade = "PC02", GradeCode = "G02", Workgroup = "HR" };
            var req = new WorkgroupGradeReq { WgGrade = "WG01", ProfitCentreGrade = "PC02", GradeCode = "G02", Workgroup = "HR" };
            var apiResponse = new ApiResponse<WorkgroupGradeRes>
            {
                Success = true,
                Data = new WorkgroupGradeRes { WgGrade = "WG01", ProfitCentreGrade = "PC02" }
            };
            var expectedDto = ApiResponseDto<WorkgroupGradeDto>.SuccessResponse(dto);

            _mapper.Map<WorkgroupGradeReq>(dto).Returns(req);
            _http.PutAsync<WorkgroupGradeReq, WorkgroupGradeRes>("api/v1/workgroupgrade/WG01", req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<WorkgroupGradeDto>>(apiResponse).Returns(expectedDto);

            var result = await _client.UpdateAsync("WG01", dto);

            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).PutAsync<WorkgroupGradeReq, WorkgroupGradeRes>("api/v1/workgroupgrade/WG01", req);
        }

        [Fact]
        public async Task UpdateAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var dto = new WorkgroupGradeDto { WgGrade = "WG01" };
            var req = new WorkgroupGradeReq { WgGrade = "WG01" };
            var apiResponse = new ApiResponse<WorkgroupGradeRes>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Not found", Code = "NOT_FOUND" } }
            };
            var mappedResponse = new ApiResponseDto<WorkgroupGradeDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<WorkgroupGradeReq>(dto).Returns(req);
            _http.PutAsync<WorkgroupGradeReq, WorkgroupGradeRes>("api/v1/workgroupgrade/WG01", req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<WorkgroupGradeDto>>(apiResponse).Returns(mappedResponse);

            var result = await _client.UpdateAsync("WG01", dto);

            Assert.False(result.Success);
        }

        [Fact]
        public async Task UpdateAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            var dto = new WorkgroupGradeDto { WgGrade = "WG01" };
            _mapper.Map<WorkgroupGradeReq>(dto).Throws(new Exception("Mapping error"));

            var result = await _client.UpdateAsync("WG01", dto);

            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_WithSuccessResponse_ReturnsSuccess()
        {
            var apiResponse = new ApiResponse<bool> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool>("api/v1/workgroupgrade/WG01").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            var result = await _client.DeleteAsync("WG01");

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _http.Received(1).DeleteAsync<bool>("api/v1/workgroupgrade/WG01");
        }

        [Fact]
        public async Task DeleteAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var apiResponse = new ApiResponse<bool>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Not found", Code = "NOT_FOUND" } }
            };
            var mappedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.DeleteAsync<bool>("api/v1/workgroupgrade/INVALID").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            var result = await _client.DeleteAsync("INVALID");

            Assert.False(result.Success);
        }

        [Fact]
        public async Task DeleteAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            _http.DeleteAsync<bool>(Arg.Any<string>()).Throws(new Exception("Network error"));

            var result = await _client.DeleteAsync("WG01");

            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        [Theory]
        [InlineData("WG01")]
        [InlineData("WG02")]
        [InlineData("TEST")]
        public async Task DeleteAsync_WithVariousCodes_CallsCorrectUrl(string wgGrade)
        {
            var apiResponse = new ApiResponse<bool> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool>($"api/v1/workgroupgrade/{wgGrade}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            await _client.DeleteAsync(wgGrade);

            await _http.Received(1).DeleteAsync<bool>($"api/v1/workgroupgrade/{wgGrade}");
        }

        #endregion

        #region GetAllPcGradesAsync

        [Fact]
        public async Task GetAllPcGradesAsync_WithSuccessResponse_ReturnsMappedList()
        {
            var apiResponse = new ApiResponse<List<string>> { Success = true, Data = new List<string> { "PC01", "PC02" } };
            var expectedDto = ApiResponseDto<List<string>>.SuccessResponse(new List<string> { "PC01", "PC02" });

            _http.GetAsync<List<string>>("api/v1/workgroupgrade/pcgrades").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<string>>>(apiResponse).Returns(expectedDto);

            var result = await _client.GetAllPcGradesAsync();

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<string>>("api/v1/workgroupgrade/pcgrades");
        }

        [Fact]
        public async Task GetAllPcGradesAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var apiResponse = new ApiResponse<List<string>>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Error", Code = "ERROR" } }
            };
            var mappedResponse = new ApiResponseDto<List<string>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<string>>("api/v1/workgroupgrade/pcgrades").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<string>>>(apiResponse).Returns(mappedResponse);

            var result = await _client.GetAllPcGradesAsync();

            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetAllPcGradesAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            _http.GetAsync<List<string>>("api/v1/workgroupgrade/pcgrades").Throws(new Exception("Network error"));

            var result = await _client.GetAllPcGradesAsync();

            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        #endregion

        #region GetAllGradeCodesAsync

        [Fact]
        public async Task GetAllGradeCodesAsync_WithSuccessResponse_ReturnsMappedList()
        {
            var apiResponse = new ApiResponse<List<string>> { Success = true, Data = new List<string> { "G01", "G02" } };
            var expectedDto = ApiResponseDto<List<string>>.SuccessResponse(new List<string> { "G01", "G02" });

            _http.GetAsync<List<string>>("api/v1/workgroupgrade/gradecodes").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<string>>>(apiResponse).Returns(expectedDto);

            var result = await _client.GetAllGradeCodesAsync();

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<string>>("api/v1/workgroupgrade/gradecodes");
        }

        [Fact]
        public async Task GetAllGradeCodesAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var apiResponse = new ApiResponse<List<string>>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Error", Code = "ERROR" } }
            };
            var mappedResponse = new ApiResponseDto<List<string>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<string>>("api/v1/workgroupgrade/gradecodes").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<string>>>(apiResponse).Returns(mappedResponse);

            var result = await _client.GetAllGradeCodesAsync();

            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetAllGradeCodesAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            _http.GetAsync<List<string>>("api/v1/workgroupgrade/gradecodes").Throws(new Exception("Network error"));

            var result = await _client.GetAllGradeCodesAsync();

            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        #endregion

        #region GetAllWorkgroupNamesAsync

        [Fact]
        public async Task GetAllWorkgroupNamesAsync_WithSuccessResponse_ReturnsMappedList()
        {
            var apiResponse = new ApiResponse<List<string>> { Success = true, Data = new List<string> { "IT", "HR" } };
            var expectedDto = ApiResponseDto<List<string>>.SuccessResponse(new List<string> { "IT", "HR" });

            _http.GetAsync<List<string>>("api/v1/workgroupgrade/workgroups").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<string>>>(apiResponse).Returns(expectedDto);

            var result = await _client.GetAllWorkgroupNamesAsync();

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<string>>("api/v1/workgroupgrade/workgroups");
        }

        [Fact]
        public async Task GetAllWorkgroupNamesAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var apiResponse = new ApiResponse<List<string>>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Error", Code = "ERROR" } }
            };
            var mappedResponse = new ApiResponseDto<List<string>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<string>>("api/v1/workgroupgrade/workgroups").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<string>>>(apiResponse).Returns(mappedResponse);

            var result = await _client.GetAllWorkgroupNamesAsync();

            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetAllWorkgroupNamesAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            _http.GetAsync<List<string>>("api/v1/workgroupgrade/workgroups").Throws(new Exception("Network error"));

            var result = await _client.GetAllWorkgroupNamesAsync();

            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        #endregion
    }
}
