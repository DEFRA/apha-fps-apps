using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.DivisionGradeServiceTest
{
    public class DivisionGradeServiceTests
    {
        private readonly IFpsApiClient _mockFpsClient;
        private readonly IFpsDivisionGradeApiClient _mockApiClient;
        private readonly DivisionGradeService _sut;

        public DivisionGradeServiceTests()
        {
            _mockFpsClient = Substitute.For<IFpsApiClient>();
            _mockApiClient = Substitute.For<IFpsDivisionGradeApiClient>();
            _mockFpsClient.FpsMaintDG.Returns(_mockApiClient);
            _sut = new DivisionGradeService(_mockFpsClient);
        }

        private static DivisionGradeDto BuildDto(string code = "A-VSD") =>
            new() { DivisionGradeCode = code, GradeCode = "A", Division = "VSD", ChargeRate = 100m };

        #region GetAllPagedAsync Tests

        [Fact]
        public async Task GetAllPagedAsync_ReturnsApiResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos = new List<DivisionGradeDto> { BuildDto() };
            var pagination = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 };
            var apiResponse = ApiResponseDto<List<DivisionGradeDto>>.SuccessResponse(dtos, pagination);

            _mockApiClient.GetAllPagedAsync(query).Returns(apiResponse);

            // Act
            var result = await _sut.GetAllPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            Assert.NotNull(result.Pagination);
            await _mockApiClient.Received(1).GetAllPagedAsync(query);
        }

        [Fact]
        public async Task GetAllPagedAsync_PropagatesApiErrors()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } };
            var apiResponse = ApiResponseDto<List<DivisionGradeDto>>.FailureResponse(errors, new ApiMetaDto());

            _mockApiClient.GetAllPagedAsync(query).Returns(apiResponse);

            // Act
            var result = await _sut.GetAllPagedAsync(query);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task GetAllPagedAsync_PassesFilterAndSortParameters()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 2, PageSize = 5, SortBy = "DivisionGradeCode", Descending = true,
                Filter = "{\"DivisionGradeCode\":\"A-VSD\"}"
            };
            var apiResponse = ApiResponseDto<List<DivisionGradeDto>>.SuccessResponse([]);

            _mockApiClient.GetAllPagedAsync(query).Returns(apiResponse);

            // Act
            await _sut.GetAllPagedAsync(query);

            // Assert
            await _mockApiClient.Received(1).GetAllPagedAsync(
                Arg.Is<QueryParameters<string>>(q =>
                    q.Page == 2 && q.PageSize == 5 &&
                    q.SortBy == "DivisionGradeCode" && q.Descending == true));
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_ReturnsApiResponse()
        {
            // Arrange
            var dto = BuildDto("A-VSD");
            var apiResponse = ApiResponseDto<DivisionGradeDto>.SuccessResponse(dto);

            _mockApiClient.GetByIdAsync("A-VSD").Returns(apiResponse);

            // Act
            var result = await _sut.GetByIdAsync("A-VSD");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("A-VSD", result.Data!.DivisionGradeCode);
            await _mockApiClient.Received(1).GetByIdAsync("A-VSD");
        }

        [Fact]
        public async Task GetByIdAsync_PropagatesNotFoundError()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var apiResponse = ApiResponseDto<DivisionGradeDto>.FailureResponse(errors, new ApiMetaDto());

            _mockApiClient.GetByIdAsync("NOTEXIST").Returns(apiResponse);

            // Act
            var result = await _sut.GetByIdAsync("NOTEXIST");

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_ReturnsSuccessResponse()
        {
            // Arrange
            var dto = BuildDto("A-VSD");
            var apiResponse = ApiResponseDto<DivisionGradeDto>.SuccessResponse(dto);

            _mockApiClient.CreateAsync(dto).Returns(apiResponse);

            // Act
            var result = await _sut.CreateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("A-VSD", result.Data!.DivisionGradeCode);
            await _mockApiClient.Received(1).CreateAsync(dto);
        }

        [Fact]
        public async Task CreateAsync_PropagatesApiErrors()
        {
            // Arrange
            var dto = BuildDto();
            var errors = new List<ApiErrorDto> { new() { Message = "Already exists", Code = "CONFLICT" } };
            var apiResponse = ApiResponseDto<DivisionGradeDto>.FailureResponse(errors, new ApiMetaDto());

            _mockApiClient.CreateAsync(dto).Returns(apiResponse);

            // Act
            var result = await _sut.CreateAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("CONFLICT", result.Errors!.First().Code);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ReturnsSuccessResponse()
        {
            // Arrange
            var dto = BuildDto("A-VSD");
            var apiResponse = ApiResponseDto<DivisionGradeDto>.SuccessResponse(dto);

            _mockApiClient.UpdateAsync("A-VSD", dto).Returns(apiResponse);

            // Act
            var result = await _sut.UpdateAsync("A-VSD", dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _mockApiClient.Received(1).UpdateAsync("A-VSD", dto);
        }

        [Fact]
        public async Task UpdateAsync_PropagatesApiErrors()
        {
            // Arrange
            var dto = BuildDto();
            var errors = new List<ApiErrorDto> { new() { Message = "Update failed", Code = "UPDATE_ERROR" } };
            var apiResponse = ApiResponseDto<DivisionGradeDto>.FailureResponse(errors, new ApiMetaDto());

            _mockApiClient.UpdateAsync("A-VSD", dto).Returns(apiResponse);

            // Act
            var result = await _sut.UpdateAsync("A-VSD", dto);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_ReturnsSuccessResponse()
        {
            // Arrange
            var apiResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _mockApiClient.DeleteAsync("A-VSD").Returns(apiResponse);

            // Act
            var result = await _sut.DeleteAsync("A-VSD");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _mockApiClient.Received(1).DeleteAsync("A-VSD");
        }

        [Fact]
        public async Task DeleteAsync_PropagatesApiErrors()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Delete failed", Code = "DELETE_ERROR" } };
            var apiResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());

            _mockApiClient.DeleteAsync("A-VSD").Returns(apiResponse);

            // Act
            var result = await _sut.DeleteAsync("A-VSD");

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region GetAllGradeCodesAsync Tests

        [Fact]
        public async Task GetAllGradeCodesAsync_ReturnsGradeCodes()
        {
            // Arrange
            var gradeCodes = new List<string> { "A", "B", "C" };
            var apiResponse = ApiResponseDto<List<string>>.SuccessResponse(gradeCodes);

            _mockApiClient.GetAllGradeCodesAsync().Returns(apiResponse);

            // Act
            var result = await _sut.GetAllGradeCodesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(3, result.Data!.Count);
            await _mockApiClient.Received(1).GetAllGradeCodesAsync();
        }

        [Fact]
        public async Task GetAllGradeCodesAsync_PropagatesApiErrors()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } };
            var apiResponse = ApiResponseDto<List<string>>.FailureResponse(errors, new ApiMetaDto());

            _mockApiClient.GetAllGradeCodesAsync().Returns(apiResponse);

            // Act
            var result = await _sut.GetAllGradeCodesAsync();

            // Assert
            Assert.False(result.Success);
        }

        #endregion
    }
}
