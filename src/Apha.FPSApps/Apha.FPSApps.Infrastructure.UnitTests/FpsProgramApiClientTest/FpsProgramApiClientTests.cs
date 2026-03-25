using Apha.Common.Contracts;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.FpsProgramApiClientTest
{
    public class FpsProgramApiClientTests
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly FpsProgramApiClient _client;

        public FpsProgramApiClientTests()
        {
            _http = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsProgramApiClient(_http, _mapper);
        }

        #region GetAllProgramsAsync Tests

        [Fact]
        public async Task GetAllProgramsAsync_WithSuccessResponse_ReturnsMappedProgramList()
        {
            // Arrange
            var programList = new List<ProgramDto>
            {
                new ProgramDto { ProgramNo = "P001", ProgramName = "Program One", Directorate = "IT" },
                new ProgramDto { ProgramNo = "P002", ProgramName = "Program Two", Directorate = "HR" }
            };
            var apiResponse = new ApiResponse<IEnumerable<ProgramDto>>
            {
                Success = true,
                Data = programList
            };
            var expectedDto = ApiResponseDto<IEnumerable<ProgramDto>>.SuccessResponse(programList);

            _http.GetAsync<IEnumerable<ProgramDto>>("api/program").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<IEnumerable<ProgramDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllProgramsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count());
            await _http.Received(1).GetAsync<IEnumerable<ProgramDto>>("api/program");
            _mapper.Received(1).Map<ApiResponseDto<IEnumerable<ProgramDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAllProgramsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new ApiError { Message = "API Error", Code = "ERROR" } };
            var apiResponse = new ApiResponse<IEnumerable<ProgramDto>>
            {
                Success = false,
                Errors = errors
            };
            var mappedResponse = new ApiResponseDto<IEnumerable<ProgramDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<IEnumerable<ProgramDto>>("api/program").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<IEnumerable<ProgramDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetAllProgramsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task GetAllProgramsAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            // Arrange
            _http.GetAsync<IEnumerable<ProgramDto>>("api/program").ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetAllProgramsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            var error = Assert.Single(result.Errors);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal("Failed to retrieve programs", error.Message);
        }

        #endregion

        #region GetAllProgramsAsync (with QueryParameters) Tests

        [Fact]
        public async Task GetAllProgramsAsync_WithQuery_ReturnsMappedProgramList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, Search = "Test" };
            var programList = new List<ProgramDto>
            {
                new ProgramDto { ProgramNo = "P001", ProgramName = "Test Program" }
            };
            var apiResponse = new ApiResponse<List<ProgramDto>>
            {
                Success = true,
                Data = programList,
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };
            var expectedDto = ApiResponseDto<List<ProgramDto>>.SuccessResponse(
                programList,
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            );

            _http.GetAsync<List<ProgramDto>>(Arg.Is<string>(url => url.Contains("api/program/paged"))).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProgramDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllProgramsAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _http.Received(1).GetAsync<List<ProgramDto>>(Arg.Is<string>(url => url.Contains("api/program/paged")));
        }

        [Fact]
        public async Task GetAllProgramsAsync_WithQuery_WhenExceptionThrown_ReturnsInternalError()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _http.GetAsync<List<ProgramDto>>(Arg.Any<string>()).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetAllProgramsAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            var error = Assert.Single(result.Errors);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal("Failed to retrieve paginated programs", error.Message);
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
                Directorate = "IT"
            };
            var apiResponse = new ApiResponse<ProgramDto>
            {
                Success = true,
                Data = program
            };
            var expectedDto = ApiResponseDto<ProgramDto?>.SuccessResponse(program);

            _http.GetAsync<ProgramDto>($"api/program/{programNo}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProgramDto?>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetProgramByIdAsync(programNo);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(programNo, result.Data.ProgramNo);
            await _http.Received(1).GetAsync<ProgramDto>($"api/program/{programNo}");
        }

        [Theory]
        [InlineData("P001")]
        [InlineData("PROG123")]
        [InlineData("TEST")]
        public async Task GetProgramByIdAsync_WithVariousProgramNos_CallsCorrectUrl(string programNo)
        {
            // Arrange
            var apiResponse = new ApiResponse<ProgramDto>
            {
                Success = true,
                Data = new ProgramDto { ProgramNo = programNo }
            };
            var expectedDto = ApiResponseDto<ProgramDto?>.SuccessResponse(apiResponse.Data);

            _http.GetAsync<ProgramDto>($"api/program/{programNo}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProgramDto?>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetProgramByIdAsync(programNo);

            // Assert
            await _http.Received(1).GetAsync<ProgramDto>($"api/program/{programNo}");
        }

        [Fact]
        public async Task GetProgramByIdAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            // Arrange
            var programNo = "P001";
            _http.GetAsync<ProgramDto>(Arg.Any<string>()).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetProgramByIdAsync(programNo);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            var error = Assert.Single(result.Errors);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal("Failed to retrieve program", error.Message);
        }

        #endregion

        #region AddProgramAsync Tests

        [Fact]
        public async Task AddProgramAsync_WithValidProgram_ReturnsCreatedProgram()
        {
            // Arrange
            var programDto = new ProgramDto
            {
                ProgramNo = "P001",
                ProgramName = "New Program",
                Directorate = "IT"
            };
            var apiResponse = new ApiResponse<ProgramDto>
            {
                Success = true,
                Data = programDto
            };
            var expectedDto = ApiResponseDto<ProgramDto>.SuccessResponse(programDto);

            _http.PostAsync<ProgramDto, ProgramDto>("api/program", programDto).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProgramDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.AddProgramAsync(programDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("P001", result.Data.ProgramNo);
            await _http.Received(1).PostAsync<ProgramDto, ProgramDto>("api/program", programDto);
        }

        [Fact]
        public async Task AddProgramAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var programDto = new ProgramDto { ProgramNo = "P001" };
            var errors = new List<ApiError> { new ApiError { Message = "Duplicate", Code = "DUPLICATE" } };
            var apiResponse = new ApiResponse<ProgramDto>
            {
                Success = false,
                Errors = errors
            };
            var mappedResponse = new ApiResponseDto<ProgramDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Duplicate", Code = "DUPLICATE" } },
                Meta = new ApiMetaDto()
            };

            _http.PostAsync<ProgramDto, ProgramDto>("api/program", programDto).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProgramDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.AddProgramAsync(programDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task AddProgramAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            // Arrange
            var programDto = new ProgramDto { ProgramNo = "P001" };
            _http.PostAsync<ProgramDto, ProgramDto>("api/program", programDto).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.AddProgramAsync(programDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            var error = Assert.Single(result.Errors);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal("Failed to add program", error.Message);
        }

        #endregion

        #region UpdateProgramAsync Tests

        [Fact]
        public async Task UpdateProgramAsync_WithValidProgram_ReturnsUpdatedProgram()
        {
            // Arrange
            var programDto = new ProgramDto
            {
                ProgramNo = "P001",
                ProgramName = "Updated Program",
                Directorate = "Finance"
            };
            var apiResponse = new ApiResponse<ProgramDto>
            {
                Success = true,
                Data = programDto
            };
            var expectedDto = ApiResponseDto<ProgramDto>.SuccessResponse(programDto);

            _http.PutAsync<ProgramDto, ProgramDto>("api/program", programDto).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProgramDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.UpdateProgramAsync(programDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("Updated Program", result.Data.ProgramName);
            await _http.Received(1).PutAsync<ProgramDto, ProgramDto>("api/program", programDto);
        }

        [Fact]
        public async Task UpdateProgramAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            // Arrange
            var programDto = new ProgramDto { ProgramNo = "P001" };
            _http.PutAsync<ProgramDto, ProgramDto>("api/program", programDto).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.UpdateProgramAsync(programDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            var error = Assert.Single(result.Errors);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal("Failed to update program", error.Message);
        }

        #endregion

        #region DeleteProgramAsync Tests

        [Fact]
        public async Task DeleteProgramAsync_WithValidProgramNo_ReturnsSuccess()
        {
            // Arrange
            var programNo = "P001";
            var apiResponse = new ApiResponse<bool>
            {
                Success = true,
                Data = true
            };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool>($"api/program/{programNo}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.DeleteProgramAsync(programNo);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _http.Received(1).DeleteAsync<bool>($"api/program/{programNo}");
        }

        [Fact]
        public async Task DeleteProgramAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            // Arrange
            var programNo = "P001";
            _http.DeleteAsync<bool>(Arg.Any<string>()).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.DeleteProgramAsync(programNo);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            var error = Assert.Single(result.Errors);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal("Failed to delete program", error.Message);
        }

        [Theory]
        [InlineData("P001")]
        [InlineData("PROG123")]
        [InlineData("TEST")]
        public async Task DeleteProgramAsync_WithVariousProgramNos_CallsCorrectUrl(string programNo)
        {
            // Arrange
            var apiResponse = new ApiResponse<bool> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool>($"api/program/{programNo}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.DeleteProgramAsync(programNo);

            // Assert
            await _http.Received(1).DeleteAsync<bool>($"api/program/{programNo}");
        }

        #endregion

        #region Edge Cases and Integration Tests

        [Fact]
        public async Task GetAllProgramsAsync_WithQuery_ConstructsUrlWithQueryParameters()
        {
            // Arrange
            var queryParameters = new QueryParameters<string>
            {
                Page = 2,
                PageSize = 20,
                Search = "Test Program",
                SortBy = "ProgramName",
                Descending = true
            };
            var apiResponse = new ApiResponse<List<ProgramDto>> { Success = true, Data = new List<ProgramDto>() };
            var expectedDto = ApiResponseDto<List<ProgramDto>>.SuccessResponse(new List<ProgramDto>(), new PaginationDto());

            _http.GetAsync<List<ProgramDto>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProgramDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetAllProgramsAsync(queryParameters);

            // Assert
            await _http.Received(1).GetAsync<List<ProgramDto>>(Arg.Is<string>(url =>
                url.Contains("api/program/paged")
            ));
        }

        #endregion
    }
}