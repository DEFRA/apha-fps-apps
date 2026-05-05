using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.WorkgroupGradeServiceTest
{
    public class WorkgroupGradeServiceTests
    {
        private readonly IFpsApiClient _fpsClient;
        private readonly IFpsWorkgroupGradeApiClient _fpsWgGradeApiClient;
        private readonly WorkgroupGradeService _service;

        public WorkgroupGradeServiceTests()
        {
            _fpsClient = Substitute.For<IFpsApiClient>();
            _fpsWgGradeApiClient = Substitute.For<IFpsWorkgroupGradeApiClient>();
            _fpsClient.FpsWorkgroupGrade.Returns(_fpsWgGradeApiClient);
            _service = new WorkgroupGradeService(_fpsClient);
        }

        #region Constructor

        [Fact]
        public void Constructor_NullClient_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new WorkgroupGradeService(null!));
        }

        #endregion

        #region GetAllWorkgroupGradesPagedAsync

        [Fact]
        public async Task GetAllWorkgroupGradesPagedAsync_WithSuccessResponse_ReturnsList()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var data = new List<WorkgroupGradeDto>
            {
                new() { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" },
                new() { WgGrade = "WG02", ProfitCentreGrade = "PC02", GradeCode = "G02", Workgroup = "HR" }
            };
            var expected = ApiResponseDto<List<WorkgroupGradeDto>>.SuccessResponse(data);
            _fpsWgGradeApiClient.GetAllWorkgroupGradesPagedAsync(query).Returns(expected);

            var result = await _service.GetAllWorkgroupGradesPagedAsync(query);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _fpsWgGradeApiClient.Received(1).GetAllWorkgroupGradesPagedAsync(query);
        }

        [Fact]
        public async Task GetAllWorkgroupGradesPagedAsync_WhenApiFails_ReturnsFailureResponse()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expected = ApiResponseDto<List<WorkgroupGradeDto>>.FailureResponse(errors, new ApiMetaDto());
            _fpsWgGradeApiClient.GetAllWorkgroupGradesPagedAsync(query).Returns(expected);

            var result = await _service.GetAllWorkgroupGradesPagedAsync(query);

            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task GetAllWorkgroupGradesPagedAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expected = ApiResponseDto<List<WorkgroupGradeDto>>.SuccessResponse(new List<WorkgroupGradeDto>());
            _fpsWgGradeApiClient.GetAllWorkgroupGradesPagedAsync(query).Returns(expected);

            var result = await _service.GetAllWorkgroupGradesPagedAsync(query);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        #endregion

        #region GetByWgGradeAsync

        [Fact]
        public async Task GetByWgGradeAsync_WithValidCode_ReturnsRecord()
        {
            var dto = new WorkgroupGradeDto { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" };
            var expected = ApiResponseDto<WorkgroupGradeDto>.SuccessResponse(dto);
            _fpsWgGradeApiClient.GetByWgGradeAsync("WG01").Returns(expected);

            var result = await _service.GetByWgGradeAsync("WG01");

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("WG01", result.Data?.WgGrade);
            await _fpsWgGradeApiClient.Received(1).GetByWgGradeAsync("WG01");
        }

        [Fact]
        public async Task GetByWgGradeAsync_WhenNotFound_ReturnsFailureResponse()
        {
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var expected = ApiResponseDto<WorkgroupGradeDto>.FailureResponse(errors, new ApiMetaDto());
            _fpsWgGradeApiClient.GetByWgGradeAsync("INVALID").Returns(expected);

            var result = await _service.GetByWgGradeAsync("INVALID");

            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        [Theory]
        [InlineData("WG01")]
        [InlineData("WG02")]
        [InlineData("TEST")]
        public async Task GetByWgGradeAsync_WithVariousCodes_CallsApiClient(string wgGrade)
        {
            var expected = ApiResponseDto<WorkgroupGradeDto>.SuccessResponse(new WorkgroupGradeDto { WgGrade = wgGrade });
            _fpsWgGradeApiClient.GetByWgGradeAsync(wgGrade).Returns(expected);

            await _service.GetByWgGradeAsync(wgGrade);

            await _fpsWgGradeApiClient.Received(1).GetByWgGradeAsync(wgGrade);
        }

        #endregion

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_WithValidDto_ReturnsSuccessResponse()
        {
            var dto = new WorkgroupGradeDto { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" };
            var expected = ApiResponseDto<WorkgroupGradeDto>.SuccessResponse(dto);
            _fpsWgGradeApiClient.CreateAsync(dto).Returns(expected);

            var result = await _service.CreateAsync(dto);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("WG01", result.Data?.WgGrade);
            await _fpsWgGradeApiClient.Received(1).CreateAsync(dto);
        }

        [Fact]
        public async Task CreateAsync_WhenApiFails_ReturnsFailureResponse()
        {
            var dto = new WorkgroupGradeDto { WgGrade = "WG01" };
            var errors = new List<ApiErrorDto> { new() { Message = "Duplicate", Code = "DUPLICATE" } };
            var expected = ApiResponseDto<WorkgroupGradeDto>.FailureResponse(errors, new ApiMetaDto());
            _fpsWgGradeApiClient.CreateAsync(dto).Returns(expected);

            var result = await _service.CreateAsync(dto);

            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task CreateAsync_PassesExactDtoObject()
        {
            var dto = new WorkgroupGradeDto { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" };
            var expected = ApiResponseDto<WorkgroupGradeDto>.SuccessResponse(dto);
            _fpsWgGradeApiClient.CreateAsync(dto).Returns(expected);

            await _service.CreateAsync(dto);

            await _fpsWgGradeApiClient.Received(1).CreateAsync(Arg.Is<WorkgroupGradeDto>(d =>
                d.WgGrade == "WG01" && d.ProfitCentreGrade == "PC01" && d.GradeCode == "G01" && d.Workgroup == "IT"));
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_WithValidDto_ReturnsSuccessResponse()
        {
            var dto = new WorkgroupGradeDto { WgGrade = "WG01", ProfitCentreGrade = "PC02", GradeCode = "G02", Workgroup = "HR" };
            var expected = ApiResponseDto<WorkgroupGradeDto>.SuccessResponse(dto);
            _fpsWgGradeApiClient.UpdateAsync("WG01", dto).Returns(expected);

            var result = await _service.UpdateAsync("WG01", dto);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("PC02", result.Data?.ProfitCentreGrade);
            await _fpsWgGradeApiClient.Received(1).UpdateAsync("WG01", dto);
        }

        [Fact]
        public async Task UpdateAsync_WhenNotFound_ReturnsFailureResponse()
        {
            var dto = new WorkgroupGradeDto { WgGrade = "INVALID" };
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var expected = ApiResponseDto<WorkgroupGradeDto>.FailureResponse(errors, new ApiMetaDto());
            _fpsWgGradeApiClient.UpdateAsync("INVALID", dto).Returns(expected);

            var result = await _service.UpdateAsync("INVALID", dto);

            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        [Fact]
        public async Task UpdateAsync_PassesWgGradeAndDto()
        {
            var dto = new WorkgroupGradeDto { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" };
            var expected = ApiResponseDto<WorkgroupGradeDto>.SuccessResponse(dto);
            _fpsWgGradeApiClient.UpdateAsync("WG01", dto).Returns(expected);

            await _service.UpdateAsync("WG01", dto);

            await _fpsWgGradeApiClient.Received(1).UpdateAsync("WG01", Arg.Is<WorkgroupGradeDto>(d => d.WgGrade == "WG01"));
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_WithValidCode_ReturnsSuccessResponse()
        {
            var expected = ApiResponseDto<bool>.SuccessResponse(true);
            _fpsWgGradeApiClient.DeleteAsync("WG01").Returns(expected);

            var result = await _service.DeleteAsync("WG01");

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _fpsWgGradeApiClient.Received(1).DeleteAsync("WG01");
        }

        [Fact]
        public async Task DeleteAsync_WhenNotFound_ReturnsFailureResponse()
        {
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var expected = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _fpsWgGradeApiClient.DeleteAsync("INVALID").Returns(expected);

            var result = await _service.DeleteAsync("INVALID");

            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Theory]
        [InlineData("WG01")]
        [InlineData("WG02")]
        [InlineData("TEST")]
        public async Task DeleteAsync_WithVariousCodes_CallsApiClient(string wgGrade)
        {
            var expected = ApiResponseDto<bool>.SuccessResponse(true);
            _fpsWgGradeApiClient.DeleteAsync(wgGrade).Returns(expected);

            await _service.DeleteAsync(wgGrade);

            await _fpsWgGradeApiClient.Received(1).DeleteAsync(wgGrade);
        }

        #endregion

        #region GetAllPcGradesAsync

        [Fact]
        public async Task GetAllPcGradesAsync_ReturnsSuccessResponse()
        {
            var grades = new List<string> { "PC01", "PC02" };
            var expected = ApiResponseDto<List<string>>.SuccessResponse(grades);
            _fpsWgGradeApiClient.GetAllPcGradesAsync().Returns(expected);

            var result = await _service.GetAllPcGradesAsync();

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _fpsWgGradeApiClient.Received(1).GetAllPcGradesAsync();
        }

        [Fact]
        public async Task GetAllPcGradesAsync_WhenApiFails_ReturnsFailureResponse()
        {
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } };
            var expected = ApiResponseDto<List<string>>.FailureResponse(errors, new ApiMetaDto());
            _fpsWgGradeApiClient.GetAllPcGradesAsync().Returns(expected);

            var result = await _service.GetAllPcGradesAsync();

            Assert.False(result.Success);
        }

        #endregion

        #region GetAllGradeCodesAsync

        [Fact]
        public async Task GetAllGradeCodesAsync_ReturnsSuccessResponse()
        {
            var codes = new List<string> { "G01", "G02" };
            var expected = ApiResponseDto<List<string>>.SuccessResponse(codes);
            _fpsWgGradeApiClient.GetAllGradeCodesAsync().Returns(expected);

            var result = await _service.GetAllGradeCodesAsync();

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _fpsWgGradeApiClient.Received(1).GetAllGradeCodesAsync();
        }

        [Fact]
        public async Task GetAllGradeCodesAsync_WhenApiFails_ReturnsFailureResponse()
        {
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } };
            var expected = ApiResponseDto<List<string>>.FailureResponse(errors, new ApiMetaDto());
            _fpsWgGradeApiClient.GetAllGradeCodesAsync().Returns(expected);

            var result = await _service.GetAllGradeCodesAsync();

            Assert.False(result.Success);
        }

        #endregion

        #region GetAllWorkgroupNamesAsync

        [Fact]
        public async Task GetAllWorkgroupNamesAsync_ReturnsSuccessResponse()
        {
            var names = new List<string> { "IT", "HR" };
            var expected = ApiResponseDto<List<string>>.SuccessResponse(names);
            _fpsWgGradeApiClient.GetAllWorkgroupNamesAsync().Returns(expected);

            var result = await _service.GetAllWorkgroupNamesAsync();

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _fpsWgGradeApiClient.Received(1).GetAllWorkgroupNamesAsync();
        }

        [Fact]
        public async Task GetAllWorkgroupNamesAsync_WhenApiFails_ReturnsFailureResponse()
        {
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } };
            var expected = ApiResponseDto<List<string>>.FailureResponse(errors, new ApiMetaDto());
            _fpsWgGradeApiClient.GetAllWorkgroupNamesAsync().Returns(expected);

            var result = await _service.GetAllWorkgroupNamesAsync();

            Assert.False(result.Success);
        }

        #endregion
    }
}
