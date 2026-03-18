using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.ProgramServiceTest
{
    public class ProgramServiceTests
    {
        private readonly IFpsApiClient _fpsClient;
        private readonly IFpsProgramApiClient _fpsProgramApiClient;
        private readonly ProgramService _programService;

        public ProgramServiceTests()
        {
            _fpsClient = Substitute.For<IFpsApiClient>();
            _fpsProgramApiClient = Substitute.For<IFpsProgramApiClient>();
            _fpsClient.FpsProgram.Returns(_fpsProgramApiClient);
            _programService = new ProgramService(_fpsClient);
        }

        #region GetAllProgramsAsync Tests

        [Fact]
        public async Task GetAllProgramsAsync_WithSuccessResponse_ReturnsProgramList()
        {
            // Arrange
            var programs = new List<ProgramDto>
            {
                new ProgramDto { ProgramNo = "P001", ProgramName = "Program One", Directorate = "IT" },
                new ProgramDto { ProgramNo = "P002", ProgramName = "Program Two", Directorate = "Finance" }
            };
            var expectedResponse = ApiResponseDto<IEnumerable<ProgramDto>>.SuccessResponse(programs);

            _fpsProgramApiClient.GetAllProgramsAsync().Returns(expectedResponse);

            // Act
            var result = await _programService.GetAllProgramsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count());
            await _fpsProgramApiClient.Received(1).GetAllProgramsAsync();
        }

        [Fact]
        public async Task GetAllProgramsAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<IEnumerable<ProgramDto>>.SuccessResponse(new List<ProgramDto>());

            _fpsProgramApiClient.GetAllProgramsAsync().Returns(expectedResponse);

            // Act
            var result = await _programService.GetAllProgramsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetAllProgramsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "API Error", Code = "API_ERROR" }
            };
            var expectedResponse = ApiResponseDto<IEnumerable<ProgramDto>>.FailureResponse(errors, new ApiMetaDto());

            _fpsProgramApiClient.GetAllProgramsAsync().Returns(expectedResponse);

            // Act
            var result = await _programService.GetAllProgramsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        #endregion

        #region GetAllProgramsAsync (with QueryParameters) Tests

        [Fact]
        public async Task GetAllProgramsAsync_WithQuery_ReturnsPaginatedProgramList()
        {
            // Arrange
            var queryParameters = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Search = "Test"
            };
            var programs = new List<ProgramDto>
            {
                new ProgramDto { ProgramNo = "P001", ProgramName = "Test Program" }
            };
            var expectedResponse = ApiResponseDto<List<ProgramDto>>.SuccessResponse(
                programs,
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            );

            _fpsProgramApiClient.GetAllProgramsAsync(queryParameters).Returns(expectedResponse);

            // Act
            var result = await _programService.GetAllProgramsAsync(queryParameters);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _fpsProgramApiClient.Received(1).GetAllProgramsAsync(queryParameters);
        }

        [Fact]
        public async Task GetAllProgramsAsync_WithQuery_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "API Error", Code = "API_ERROR" }
            };
            var expectedResponse = ApiResponseDto<List<ProgramDto>>.FailureResponse(errors, new ApiMetaDto());

            _fpsProgramApiClient.GetAllProgramsAsync(queryParameters).Returns(expectedResponse);

            // Act
            var result = await _programService.GetAllProgramsAsync(queryParameters);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task GetAllProgramsAsync_WithQuery_PassesCorrectQueryParameters()
        {
            // Arrange
            var queryParameters = new QueryParameters<string>
            {
                Page = 2,
                PageSize = 20,
                Search = "Finance",
                SortBy = "ProgramName",
                Descending = true
            };
            var expectedResponse = ApiResponseDto<List<ProgramDto>>.SuccessResponse(
                new List<ProgramDto>(),
                new PaginationDto()
            );

            _fpsProgramApiClient.GetAllProgramsAsync(queryParameters).Returns(expectedResponse);

            // Act
            await _programService.GetAllProgramsAsync(queryParameters);

            // Assert
            await _fpsProgramApiClient.Received(1).GetAllProgramsAsync(Arg.Is<QueryParameters<string>>(q =>
                q.Page == queryParameters.Page &&
                q.PageSize == queryParameters.PageSize &&
                q.Search == queryParameters.Search &&
                q.SortBy == queryParameters.SortBy &&
                q.Descending == queryParameters.Descending
            ));
        }

        #endregion

        #region GetProgramByIdAsync Tests

        [Fact]
        public async Task GetProgramByIdAsync_WithValidProgramNo_ReturnsProgram()
        {
            // Arrange
            var programNo = "P001";
            var program = new ProgramDto
            {
                ProgramNo = programNo,
                ProgramName = "Test Program",
                Directorate = "IT",
                Manager = "John Doe"
            };
            var expectedResponse = ApiResponseDto<ProgramDto?>.SuccessResponse(program);

            _fpsProgramApiClient.GetProgramByIdAsync(programNo).Returns(expectedResponse);

            // Act
            var result = await _programService.GetProgramByIdAsync(programNo);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(programNo, result.Data.ProgramNo);
            await _fpsProgramApiClient.Received(1).GetProgramByIdAsync(programNo);
        }

        [Fact]
        public async Task GetProgramByIdAsync_WithNonExistentProgramNo_ReturnsFailureResponse()
        {
            // Arrange
            var programNo = "NONEXISTENT";
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Program not found", Code = "NOT_FOUND" }
            };
            var expectedResponse = ApiResponseDto<ProgramDto?>.FailureResponse(errors, new ApiMetaDto());

            _fpsProgramApiClient.GetProgramByIdAsync(programNo).Returns(expectedResponse);

            // Act
            var result = await _programService.GetProgramByIdAsync(programNo);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Theory]
        [InlineData("P001")]
        [InlineData("PROG123")]
        [InlineData("TEST")]
        public async Task GetProgramByIdAsync_WithVariousProgramNos_CallsApiClient(string programNo)
        {
            // Arrange
            var expectedResponse = ApiResponseDto<ProgramDto?>.SuccessResponse(new ProgramDto { ProgramNo = programNo });
            _fpsProgramApiClient.GetProgramByIdAsync(programNo).Returns(expectedResponse);

            // Act
            await _programService.GetProgramByIdAsync(programNo);

            // Assert
            await _fpsProgramApiClient.Received(1).GetProgramByIdAsync(programNo);
        }

        #endregion

        #region AddProgramAsync Tests

        [Fact]
        public async Task AddProgramAsync_WithValidProgram_ReturnsSuccessResponse()
        {
            // Arrange
            var newProgram = new ProgramDto
            {
                ProgramNo = "P001",
                ProgramName = "New Program",
                Directorate = "IT",
                Manager = "John Manager"
            };
            var expectedResponse = ApiResponseDto<ProgramDto>.SuccessResponse(newProgram);

            _fpsProgramApiClient.AddProgramAsync(newProgram).Returns(expectedResponse);

            // Act
            var result = await _programService.AddProgramAsync(newProgram);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(newProgram.ProgramNo, result.Data.ProgramNo);
            await _fpsProgramApiClient.Received(1).AddProgramAsync(newProgram);
        }

        [Fact]
        public async Task AddProgramAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var newProgram = new ProgramDto
            {
                ProgramNo = "P001",
                ProgramName = "Duplicate Program"
            };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Duplicate program", Code = "DUPLICATE" }
            };
            var expectedResponse = ApiResponseDto<ProgramDto>.FailureResponse(errors, new ApiMetaDto());

            _fpsProgramApiClient.AddProgramAsync(newProgram).Returns(expectedResponse);

            // Act
            var result = await _programService.AddProgramAsync(newProgram);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task AddProgramAsync_WithMinimalData_CallsApiClient()
        {
            // Arrange
            var newProgram = new ProgramDto { ProgramNo = "P001" };
            var expectedResponse = ApiResponseDto<ProgramDto>.SuccessResponse(newProgram);

            _fpsProgramApiClient.AddProgramAsync(newProgram).Returns(expectedResponse);

            // Act
            await _programService.AddProgramAsync(newProgram);

            // Assert
            await _fpsProgramApiClient.Received(1).AddProgramAsync(newProgram);
        }

        #endregion

        #region UpdateProgramAsync Tests

        [Fact]
        public async Task UpdateProgramAsync_WithValidProgram_ReturnsSuccessResponse()
        {
            // Arrange
            var updatedProgram = new ProgramDto
            {
                ProgramNo = "P001",
                ProgramName = "Updated Program",
                Directorate = "Finance",
                Manager = "Jane Manager"
            };
            var expectedResponse = ApiResponseDto<ProgramDto>.SuccessResponse(updatedProgram);

            _fpsProgramApiClient.UpdateProgramAsync(updatedProgram).Returns(expectedResponse);

            // Act
            var result = await _programService.UpdateProgramAsync(updatedProgram);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("Updated Program", result.Data.ProgramName);
            await _fpsProgramApiClient.Received(1).UpdateProgramAsync(updatedProgram);
        }

        [Fact]
        public async Task UpdateProgramAsync_WithNonExistentProgram_ReturnsFailureResponse()
        {
            // Arrange
            var program = new ProgramDto { ProgramNo = "NONEXISTENT" };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Program not found", Code = "NOT_FOUND" }
            };
            var expectedResponse = ApiResponseDto<ProgramDto>.FailureResponse(errors, new ApiMetaDto());

            _fpsProgramApiClient.UpdateProgramAsync(program).Returns(expectedResponse);

            // Act
            var result = await _programService.UpdateProgramAsync(program);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task UpdateProgramAsync_WhenApiReturnsError_ReturnsFailureResponse()
        {
            // Arrange
            var program = new ProgramDto { ProgramNo = "P001" };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Update failed", Code = "UPDATE_ERROR" }
            };
            var expectedResponse = ApiResponseDto<ProgramDto>.FailureResponse(errors, new ApiMetaDto());

            _fpsProgramApiClient.UpdateProgramAsync(program).Returns(expectedResponse);

            // Act
            var result = await _programService.UpdateProgramAsync(program);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region DeleteProgramAsync Tests

        [Fact]
        public async Task DeleteProgramAsync_WithValidProgramNo_ReturnsSuccessResponse()
        {
            // Arrange
            var programNo = "P001";
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);

            _fpsProgramApiClient.DeleteProgramAsync(programNo).Returns(expectedResponse);

            // Act
            var result = await _programService.DeleteProgramAsync(programNo);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _fpsProgramApiClient.Received(1).DeleteProgramAsync(programNo);
        }

        [Fact]
        public async Task DeleteProgramAsync_WithNonExistentProgramNo_ReturnsFailureResponse()
        {
            // Arrange
            var programNo = "NONEXISTENT";
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Program not found", Code = "NOT_FOUND" }
            };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());

            _fpsProgramApiClient.DeleteProgramAsync(programNo).Returns(expectedResponse);

            // Act
            var result = await _programService.DeleteProgramAsync(programNo);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Theory]
        [InlineData("P001")]
        [InlineData("PROG123")]
        [InlineData("TEST")]
        public async Task DeleteProgramAsync_WithVariousProgramNos_CallsApiClient(string programNo)
        {
            // Arrange
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _fpsProgramApiClient.DeleteProgramAsync(programNo).Returns(expectedResponse);

            // Act
            await _programService.DeleteProgramAsync(programNo);

            // Assert
            await _fpsProgramApiClient.Received(1).DeleteProgramAsync(programNo);
        }

        #endregion

        #region GetAllDirectoratesAsync Tests

        [Fact]
        public async Task GetAllDirectoratesAsync_ReturnsListOfDirectorates()
        {
            // Arrange
            var directorates = new List<string?> { "IT", "Finance", "HR", "Operations" };
            var expectedResponse = ApiResponseDto<List<string?>>.SuccessResponse(directorates);

            _fpsProgramApiClient.GetAllDirectoratesAsync().Returns(expectedResponse);

            // Act
            var result = await _programService.GetAllDirectoratesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(4, result.Data.Count);
            await _fpsProgramApiClient.Received(1).GetAllDirectoratesAsync();
        }

        [Fact]
        public async Task GetAllDirectoratesAsync_WithEmptyResult_ReturnsEmptyList()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<List<string?>>.SuccessResponse(new List<string?>());

            _fpsProgramApiClient.GetAllDirectoratesAsync().Returns(expectedResponse);

            // Act
            var result = await _programService.GetAllDirectoratesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetAllDirectoratesAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "API Error", Code = "API_ERROR" }
            };
            var expectedResponse = ApiResponseDto<List<string?>>.FailureResponse(errors, new ApiMetaDto());

            _fpsProgramApiClient.GetAllDirectoratesAsync().Returns(expectedResponse);

            // Act
            var result = await _programService.GetAllDirectoratesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region Edge Cases and Integration Tests

        [Fact]
        public async Task GetAllProgramsAsync_CallsApiClientOnce()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<IEnumerable<ProgramDto>>.SuccessResponse(new List<ProgramDto>());
            _fpsProgramApiClient.GetAllProgramsAsync().Returns(expectedResponse);

            // Act
            await _programService.GetAllProgramsAsync();

            // Assert
            await _fpsProgramApiClient.Received(1).GetAllProgramsAsync();
        }

        [Fact]
        public async Task AddProgramAsync_PassesExactProgramObject()
        {
            // Arrange
            var program = new ProgramDto
            {
                ProgramNo = "P001",
                ProgramName = "Test Program",
                Directorate = "IT",
                Manager = "Test Manager"
            };
            var expectedResponse = ApiResponseDto<ProgramDto>.SuccessResponse(program);

            _fpsProgramApiClient.AddProgramAsync(program).Returns(expectedResponse);

            // Act
            await _programService.AddProgramAsync(program);

            // Assert
            await _fpsProgramApiClient.Received(1).AddProgramAsync(Arg.Is<ProgramDto>(p =>
                p.ProgramNo == program.ProgramNo &&
                p.ProgramName == program.ProgramName &&
                p.Directorate == program.Directorate &&
                p.Manager == program.Manager
            ));
        }

        [Fact]
        public async Task UpdateProgramAsync_PassesExactProgramObject()
        {
            // Arrange
            var program = new ProgramDto
            {
                ProgramNo = "P001",
                ProgramName = "Updated Program",
                Directorate = "Finance"
            };
            var expectedResponse = ApiResponseDto<ProgramDto>.SuccessResponse(program);

            _fpsProgramApiClient.UpdateProgramAsync(program).Returns(expectedResponse);

            // Act
            await _programService.UpdateProgramAsync(program);

            // Assert
            await _fpsProgramApiClient.Received(1).UpdateProgramAsync(Arg.Is<ProgramDto>(p =>
                p.ProgramNo == program.ProgramNo &&
                p.ProgramName == program.ProgramName &&
                p.Directorate == program.Directorate
            ));
        }

        [Fact]
        public async Task GetAllProgramsAsync_WithQuery_CallsApiClientOnce()
        {
            // Arrange
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse = ApiResponseDto<List<ProgramDto>>.SuccessResponse(
                new List<ProgramDto>(),
                new PaginationDto()
            );

            _fpsProgramApiClient.GetAllProgramsAsync(queryParameters).Returns(expectedResponse);

            // Act
            await _programService.GetAllProgramsAsync(queryParameters);

            // Assert
            await _fpsProgramApiClient.Received(1).GetAllProgramsAsync(Arg.Any<QueryParameters<string>>());
        }

        #endregion
    }
}