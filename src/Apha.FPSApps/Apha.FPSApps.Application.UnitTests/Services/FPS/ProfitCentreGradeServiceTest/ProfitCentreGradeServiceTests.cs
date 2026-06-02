using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.ProfitCentreGradeServiceTest
{
    public class ProfitCentreGradeServiceTests
    {
        private const string DefaultProfitCentre = "PC01";

        private readonly IFpsApiClient _fpsClient;
        private readonly IFpsProfitCentreGradeApiClient _fpsRcGradeApiClient;
        private readonly ProfitCentreGradeService _sut;

        public ProfitCentreGradeServiceTests()
        {
            _fpsClient           = Substitute.For<IFpsApiClient>();
            _fpsRcGradeApiClient = Substitute.For<IFpsProfitCentreGradeApiClient>();
            _fpsClient.FpsProfitCentreGrade.Returns(_fpsRcGradeApiClient);
            _sut = new ProfitCentreGradeService(_fpsClient);
        }

        #region GetProfitCentreGradesAsync Tests

        [Fact]
        public async Task GetProfitCentreGradesAsync_WithSuccessResponse_ReturnsGradeList()
        {
            // Arrange
            var grades = new List<ProfitCentreGradeDto>
            {
                new() { PcGrade = "G001", ProfitCentre = DefaultProfitCentre, ChargeRate = 100m },
                new() { PcGrade = "G002", ProfitCentre = DefaultProfitCentre, ChargeRate = 200m }
            };
            var expectedResponse = ApiResponseDto<List<ProfitCentreGradeDto>>.SuccessResponse(grades);

            _fpsRcGradeApiClient.GetProfitCentreGradesAsync(Arg.Any<QueryParameters<string>>(), DefaultProfitCentre)
                .Returns(expectedResponse);

            // Act
            var result = await _sut.GetProfitCentreGradesAsync(DefaultProfitCentre);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _fpsRcGradeApiClient.Received(1)
                .GetProfitCentreGradesAsync(Arg.Any<QueryParameters<string>>(), DefaultProfitCentre);
        }

        [Fact]
        public async Task GetProfitCentreGradesAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<List<ProfitCentreGradeDto>>.SuccessResponse(new List<ProfitCentreGradeDto>());

            _fpsRcGradeApiClient.GetProfitCentreGradesAsync(Arg.Any<QueryParameters<string>>(), DefaultProfitCentre)
                .Returns(expectedResponse);

            // Act
            var result = await _sut.GetProfitCentreGradesAsync(DefaultProfitCentre);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetProfitCentreGradesAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto>
            {
                new() { Message = "API Error", Code = "API_ERROR" }
            };
            var expectedResponse = ApiResponseDto<List<ProfitCentreGradeDto>>.FailureResponse(errors, new ApiMetaDto());

            _fpsRcGradeApiClient.GetProfitCentreGradesAsync(Arg.Any<QueryParameters<string>>(), DefaultProfitCentre)
                .Returns(expectedResponse);

            // Act
            var result = await _sut.GetProfitCentreGradesAsync(DefaultProfitCentre);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenFpsClientIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ProfitCentreGradeService(null!));
        }

        #endregion

        #region GetAllPagedAsync Tests

        [Fact]
        public async Task GetAllPagedAsync_WithSuccessResponse_ReturnsPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos = new List<ProfitCentreGradeDto>
            {
                new() { PcGrade = "G001", ProfitCentre = DefaultProfitCentre }
            };
            var pagination    = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 };
            var apiResponse   = ApiResponseDto<List<ProfitCentreGradeDto>>.SuccessResponse(dtos, pagination);

            _fpsRcGradeApiClient.GetAllPagedAsync(query).Returns(apiResponse);

            // Act
            var result = await _sut.GetAllPagedAsync(query);

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _fpsRcGradeApiClient.Received(1).GetAllPagedAsync(query);
        }

        [Fact]
        public async Task GetAllPagedAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query  = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } };
            var apiResponse = ApiResponseDto<List<ProfitCentreGradeDto>>.FailureResponse(errors, new ApiMetaDto());

            _fpsRcGradeApiClient.GetAllPagedAsync(query).Returns(apiResponse);

            // Act
            var result = await _sut.GetAllPagedAsync(query);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WithSuccessResponse_ReturnsDto()
        {
            // Arrange
            var dto         = new ProfitCentreGradeDto { PcGrade = "G001", ProfitCentre = DefaultProfitCentre };
            var apiResponse = ApiResponseDto<ProfitCentreGradeDto>.SuccessResponse(dto);

            _fpsRcGradeApiClient.GetByIdAsync("G001").Returns(apiResponse);

            // Act
            var result = await _sut.GetByIdAsync("G001");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("G001", result.Data!.PcGrade);
            await _fpsRcGradeApiClient.Received(1).GetByIdAsync("G001");
        }

        [Fact]
        public async Task GetByIdAsync_WhenNotFound_ReturnsFailureResponse()
        {
            // Arrange
            var errors      = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var apiResponse = ApiResponseDto<ProfitCentreGradeDto>.FailureResponse(errors, new ApiMetaDto());

            _fpsRcGradeApiClient.GetByIdAsync("NOTEXIST").Returns(apiResponse);

            // Act
            var result = await _sut.GetByIdAsync("NOTEXIST");

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_WithSuccessResponse_ReturnsCreatedDto()
        {
            // Arrange
            var dto         = new ProfitCentreGradeDto { PcGrade = "G001", ProfitCentre = DefaultProfitCentre };
            var apiResponse = ApiResponseDto<ProfitCentreGradeDto>.SuccessResponse(dto);

            _fpsRcGradeApiClient.CreateAsync(dto).Returns(apiResponse);

            // Act
            var result = await _sut.CreateAsync(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("G001", result.Data!.PcGrade);
            await _fpsRcGradeApiClient.Received(1).CreateAsync(dto);
        }

        [Fact]
        public async Task CreateAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var dto    = new ProfitCentreGradeDto { PcGrade = "G001", ProfitCentre = "INVALID" };
            var errors = new List<ApiErrorDto> { new() { Message = "ProfitCentre does not exist.", Code = "INVALID_PC" } };
            var apiResponse = ApiResponseDto<ProfitCentreGradeDto>.FailureResponse(errors, new ApiMetaDto());

            _fpsRcGradeApiClient.CreateAsync(dto).Returns(apiResponse);

            // Act
            var result = await _sut.CreateAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("INVALID_PC", result.Errors!.First().Code);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WithSuccessResponse_ReturnsUpdatedDto()
        {
            // Arrange
            var dto         = new ProfitCentreGradeDto { PcGrade = "G001", ProfitCentre = DefaultProfitCentre };
            var apiResponse = ApiResponseDto<ProfitCentreGradeDto>.SuccessResponse(dto);

            _fpsRcGradeApiClient.UpdateAsync("G001", dto).Returns(apiResponse);

            // Act
            var result = await _sut.UpdateAsync("G001", dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("G001", result.Data!.PcGrade);
            await _fpsRcGradeApiClient.Received(1).UpdateAsync("G001", dto);
        }

        [Fact]
        public async Task UpdateAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var dto    = new ProfitCentreGradeDto { PcGrade = "G001", ProfitCentre = "INVALID" };
            var errors = new List<ApiErrorDto> { new() { Message = "Update failed", Code = "UPDATE_ERROR" } };
            var apiResponse = ApiResponseDto<ProfitCentreGradeDto>.FailureResponse(errors, new ApiMetaDto());

            _fpsRcGradeApiClient.UpdateAsync("G001", dto).Returns(apiResponse);

            // Act
            var result = await _sut.UpdateAsync("G001", dto);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WithSuccessResponse_ReturnsTrue()
        {
            // Arrange
            var apiResponse = ApiResponseDto<bool>.SuccessResponse(true);

            _fpsRcGradeApiClient.DeleteAsync("G001").Returns(apiResponse);

            // Act
            var result = await _sut.DeleteAsync("G001");

            // Assert
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _fpsRcGradeApiClient.Received(1).DeleteAsync("G001");
        }

        [Fact]
        public async Task DeleteAsync_WhenNotFound_ReturnsFailureResponse()
        {
            // Arrange
            var errors      = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var apiResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());

            _fpsRcGradeApiClient.DeleteAsync("NOTEXIST").Returns(apiResponse);

            // Act
            var result = await _sut.DeleteAsync("NOTEXIST");

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region GetAllProfitCentreCodesAsync Tests

        [Fact]
        public async Task GetAllProfitCentreCodesAsync_WithSuccessResponse_ReturnsCodes()
        {
            // Arrange
            var codes       = new List<string> { "PC01", "PC02", "PC03" };
            var apiResponse = ApiResponseDto<List<string>>.SuccessResponse(codes);

            _fpsRcGradeApiClient.GetAllProfitCentreCodesAsync().Returns(apiResponse);

            // Act
            var result = await _sut.GetAllProfitCentreCodesAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Equal(3, result.Data!.Count);
            await _fpsRcGradeApiClient.Received(1).GetAllProfitCentreCodesAsync();
        }

        [Fact]
        public async Task GetAllProfitCentreCodesAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors      = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } };
            var apiResponse = ApiResponseDto<List<string>>.FailureResponse(errors, new ApiMetaDto());

            _fpsRcGradeApiClient.GetAllProfitCentreCodesAsync().Returns(apiResponse);

            // Act
            var result = await _sut.GetAllProfitCentreCodesAsync();

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetAllProfitCentreCodesAsync_ReturnsEmpty_WhenNoCodes()
        {
            // Arrange
            var apiResponse = ApiResponseDto<List<string>>.SuccessResponse([]);

            _fpsRcGradeApiClient.GetAllProfitCentreCodesAsync().Returns(apiResponse);

            // Act
            var result = await _sut.GetAllProfitCentreCodesAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        #endregion
    }
}
