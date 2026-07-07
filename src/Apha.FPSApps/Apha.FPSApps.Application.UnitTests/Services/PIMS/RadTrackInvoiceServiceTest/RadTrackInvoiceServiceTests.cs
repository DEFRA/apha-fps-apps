using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.PIMS;
using NSubstitute;

namespace Apha.FPSApps.Application.UnitTests.Services.PIMS.RadTrackInvoiceServiceTest
{
    public class RadTrackInvoiceServiceTests
    {
        private readonly IPimsApiClient _pimsApiClient;
        private readonly IPimsRadTrackInvoiceApiClient _pimsRadTrackInvoiceApiClient;
        private readonly RadTrackInvoiceService _sut;

        public RadTrackInvoiceServiceTests()
        {
            _pimsApiClient                = Substitute.For<IPimsApiClient>();
            _pimsRadTrackInvoiceApiClient = Substitute.For<IPimsRadTrackInvoiceApiClient>();
            _pimsApiClient.PimsRadTrackInvoice.Returns(_pimsRadTrackInvoiceApiClient);
            _sut = new RadTrackInvoiceService(_pimsApiClient);
        }

        private static List<ApiErrorDto> OneError(string message = "API error", string code = "ERR")
            => [new ApiErrorDto { Message = message, Code = code }];

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidClient_InitializesService()
        {
            var service = new RadTrackInvoiceService(_pimsApiClient);
            Assert.NotNull(service);
        }

        #endregion

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_WithData_ReturnsListOfInvoices()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var data = new List<RadTrackInvoiceDto>
            {
                new() { InvoiceCounter = 1, Project = "PP001", Contract = "C001" },
                new() { InvoiceCounter = 2, Project = "PP001", Contract = "C002" }
            };
            var expected = new ApiResponseDto<List<RadTrackInvoiceDto>> { Success = true, Data = data };
            _pimsRadTrackInvoiceApiClient.GetAllAsync(query).Returns(expected);

            // Act
            var result = await _sut.GetAllAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _pimsRadTrackInvoiceApiClient.Received(1).GetAllAsync(query);
        }

        [Fact]
        public async Task GetAllAsync_WithFilters_PassesFiltersToApiClient()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            const string project  = "PP001";
            const string contract = "C001";
            const int    year     = 2024;
            const string program  = "PROG1";
            var expected = new ApiResponseDto<List<RadTrackInvoiceDto>> { Success = true, Data = [] };
            _pimsRadTrackInvoiceApiClient.GetAllAsync(query, project, contract, year, program).Returns(expected);

            // Act
            await _sut.GetAllAsync(query, project, contract, year, program);

            // Assert
            await _pimsRadTrackInvoiceApiClient.Received(1).GetAllAsync(query, project, contract, year, program);
        }

        [Fact]
        public async Task GetAllAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query    = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expected = new ApiResponseDto<List<RadTrackInvoiceDto>>
            {
                Success = false,
                Errors  = OneError("Retrieval failed", "RETRIEVAL_ERROR")
            };
            _pimsRadTrackInvoiceApiClient.GetAllAsync(query).Returns(expected);

            // Act
            var result = await _sut.GetAllAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            await _pimsRadTrackInvoiceApiClient.Received(1).GetAllAsync(query);
        }

        [Fact]
        public async Task GetAllAsync_WhenApiClientThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _pimsRadTrackInvoiceApiClient.GetAllAsync(query)
                .Returns(Task.FromException<ApiResponseDto<List<RadTrackInvoiceDto>>>(new Exception("API error")));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sut.GetAllAsync(query));
        }

        #endregion

        #region GetTotalsAsync Tests

        [Fact]
        public async Task GetTotalsAsync_WithData_ReturnsTotals()
        {
            // Arrange
            var data = new RadTrackInvoiceTotalsDto
            {
                TotalPlannedAmount = 10000.00,
                TotalDueAmount     = 8000.00,
                TotalActualAmount  = 7500.00
            };
            var expected = new ApiResponseDto<RadTrackInvoiceTotalsDto> { Success = true, Data = data };
            _pimsRadTrackInvoiceApiClient.GetTotalsAsync().Returns(expected);

            // Act
            var result = await _sut.GetTotalsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(10000.00, result.Data?.TotalPlannedAmount);
            Assert.Equal(8000.00,  result.Data?.TotalDueAmount);
            Assert.Equal(7500.00,  result.Data?.TotalActualAmount);
            await _pimsRadTrackInvoiceApiClient.Received(1).GetTotalsAsync();
        }

        [Fact]
        public async Task GetTotalsAsync_WithFilters_PassesFiltersToApiClient()
        {
            // Arrange
            const string project  = "PP001";
            const string contract = "C001";
            const int    year     = 2024;
            const string program  = "PROG1";
            var expected = new ApiResponseDto<RadTrackInvoiceTotalsDto> { Success = true, Data = new RadTrackInvoiceTotalsDto() };
            _pimsRadTrackInvoiceApiClient.GetTotalsAsync(project, contract, year, program).Returns(expected);

            // Act
            await _sut.GetTotalsAsync(project, contract, year, program);

            // Assert
            await _pimsRadTrackInvoiceApiClient.Received(1).GetTotalsAsync(project, contract, year, program);
        }

        [Fact]
        public async Task GetTotalsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var expected = new ApiResponseDto<RadTrackInvoiceTotalsDto>
            {
                Success = false,
                Errors  = OneError()
            };
            _pimsRadTrackInvoiceApiClient.GetTotalsAsync().Returns(expected);

            // Act
            var result = await _sut.GetTotalsAsync();

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            await _pimsRadTrackInvoiceApiClient.Received(1).GetTotalsAsync();
        }

        [Fact]
        public async Task GetTotalsAsync_DelegatesToApiClient()
        {
            // Arrange
            _pimsRadTrackInvoiceApiClient.GetTotalsAsync()
                .Returns(new ApiResponseDto<RadTrackInvoiceTotalsDto> { Success = true, Data = new RadTrackInvoiceTotalsDto() });

            // Act
            await _sut.GetTotalsAsync();

            // Assert
            await _pimsRadTrackInvoiceApiClient.Received(1).GetTotalsAsync();
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsInvoice()
        {
            // Arrange
            const int id = 1;
            var data     = new RadTrackInvoiceDto { InvoiceCounter = id, Project = "PP001", InvoiceRef = "INV-001" };
            var expected = new ApiResponseDto<RadTrackInvoiceDto> { Success = true, Data = data };
            _pimsRadTrackInvoiceApiClient.GetByIdAsync(id).Returns(expected);

            // Act
            var result = await _sut.GetByIdAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(id,       result.Data?.InvoiceCounter);
            Assert.Equal("PP001",  result.Data?.Project);
            Assert.Equal("INV-001", result.Data?.InvoiceRef);
            await _pimsRadTrackInvoiceApiClient.Received(1).GetByIdAsync(id);
        }

        [Fact]
        public async Task GetByIdAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            const int id = 99;
            var expected = new ApiResponseDto<RadTrackInvoiceDto>
            {
                Success = false,
                Errors  = OneError("Invoice not found", "NOT_FOUND")
            };
            _pimsRadTrackInvoiceApiClient.GetByIdAsync(id).Returns(expected);

            // Act
            var result = await _sut.GetByIdAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("NOT_FOUND", result.Errors[0].Code);
            await _pimsRadTrackInvoiceApiClient.Received(1).GetByIdAsync(id);
        }

        [Fact]
        public async Task GetByIdAsync_WhenApiClientThrows_PropagatesException()
        {
            // Arrange
            const int id = 1;
            _pimsRadTrackInvoiceApiClient.GetByIdAsync(id)
                .Returns(Task.FromException<ApiResponseDto<RadTrackInvoiceDto>>(new Exception("API error")));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sut.GetByIdAsync(id));
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_WithValidDto_ReturnsSuccessResponse()
        {
            // Arrange
            var dto = new RadTrackInvoiceDto
            {
                Project       = "PP001",
                Contract      = "C001",
                InvoiceRef    = "INV-001",
                PlannedAmount = 5000.00,
                InvoicePaid   = 0
            };
            var expected = new ApiResponseDto<RadTrackInvoiceDto> { Success = true, Data = dto };
            _pimsRadTrackInvoiceApiClient.CreateAsync(dto).Returns(expected);

            // Act
            var result = await _sut.CreateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("PP001",   result.Data?.Project);
            Assert.Equal("INV-001", result.Data?.InvoiceRef);
            await _pimsRadTrackInvoiceApiClient.Received(1).CreateAsync(dto);
        }

        [Fact]
        public async Task CreateAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var dto      = new RadTrackInvoiceDto { Project = "PP001", InvoiceRef = "INV-001" };
            var expected = new ApiResponseDto<RadTrackInvoiceDto>
            {
                Success = false,
                Errors  = OneError("Invoice already exists", "DUPLICATE")
            };
            _pimsRadTrackInvoiceApiClient.CreateAsync(dto).Returns(expected);

            // Act
            var result = await _sut.CreateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            await _pimsRadTrackInvoiceApiClient.Received(1).CreateAsync(dto);
        }

        [Fact]
        public async Task CreateAsync_PassesExactDtoToApiClient()
        {
            // Arrange
            var dto = new RadTrackInvoiceDto { Project = "PP001", InvoiceRef = "INV-002" };
            _pimsRadTrackInvoiceApiClient.CreateAsync(dto)
                .Returns(new ApiResponseDto<RadTrackInvoiceDto> { Success = true, Data = dto });

            // Act
            await _sut.CreateAsync(dto);

            // Assert
            await _pimsRadTrackInvoiceApiClient.Received(1).CreateAsync(dto);
        }

        [Fact]
        public async Task CreateAsync_WhenApiClientThrows_PropagatesException()
        {
            // Arrange
            var dto = new RadTrackInvoiceDto { Project = "PP001" };
            _pimsRadTrackInvoiceApiClient.CreateAsync(dto)
                .Returns(Task.FromException<ApiResponseDto<RadTrackInvoiceDto>>(new Exception("API error")));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sut.CreateAsync(dto));
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WithValidIdAndDto_ReturnsSuccessResponse()
        {
            // Arrange
            const int id = 1;
            var dto      = new RadTrackInvoiceDto { InvoiceCounter = id, Project = "PP001", InvoiceRef = "INV-001-UPDATED" };
            var expected = new ApiResponseDto<RadTrackInvoiceDto> { Success = true, Data = dto };
            _pimsRadTrackInvoiceApiClient.UpdateAsync(id, dto).Returns(expected);

            // Act
            var result = await _sut.UpdateAsync(id, dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(id,                result.Data?.InvoiceCounter);
            Assert.Equal("INV-001-UPDATED", result.Data?.InvoiceRef);
            await _pimsRadTrackInvoiceApiClient.Received(1).UpdateAsync(id, dto);
        }

        [Fact]
        public async Task UpdateAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            const int id = 99;
            var dto      = new RadTrackInvoiceDto { InvoiceCounter = id };
            var expected = new ApiResponseDto<RadTrackInvoiceDto>
            {
                Success = false,
                Errors  = OneError("Invoice not found", "NOT_FOUND")
            };
            _pimsRadTrackInvoiceApiClient.UpdateAsync(id, dto).Returns(expected);

            // Act
            var result = await _sut.UpdateAsync(id, dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("NOT_FOUND", result.Errors[0].Code);
            await _pimsRadTrackInvoiceApiClient.Received(1).UpdateAsync(id, dto);
        }

        [Fact]
        public async Task UpdateAsync_PassesCorrectIdAndDtoToApiClient()
        {
            // Arrange
            const int id = 5;
            var dto      = new RadTrackInvoiceDto { InvoiceCounter = id, Project = "PP002" };
            _pimsRadTrackInvoiceApiClient.UpdateAsync(id, dto)
                .Returns(new ApiResponseDto<RadTrackInvoiceDto> { Success = true, Data = dto });

            // Act
            await _sut.UpdateAsync(id, dto);

            // Assert
            await _pimsRadTrackInvoiceApiClient.Received(1).UpdateAsync(id, dto);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WithValidId_ReturnsSuccessResponse()
        {
            // Arrange
            const int id = 1;
            var expected = new ApiResponseDto<object> { Success = true };
            _pimsRadTrackInvoiceApiClient.DeleteAsync(id).Returns(expected);

            // Act
            var result = await _sut.DeleteAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _pimsRadTrackInvoiceApiClient.Received(1).DeleteAsync(id);
        }

        [Fact]
        public async Task DeleteAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            const int id = 99;
            var expected = new ApiResponseDto<object>
            {
                Success = false,
                Errors  = OneError("Invoice not found", "NOT_FOUND")
            };
            _pimsRadTrackInvoiceApiClient.DeleteAsync(id).Returns(expected);

            // Act
            var result = await _sut.DeleteAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("NOT_FOUND", result.Errors[0].Code);
            await _pimsRadTrackInvoiceApiClient.Received(1).DeleteAsync(id);
        }

        [Fact]
        public async Task DeleteAsync_WhenApiClientThrows_PropagatesException()
        {
            // Arrange
            const int id = 1;
            _pimsRadTrackInvoiceApiClient.DeleteAsync(id)
                .Returns(Task.FromException<ApiResponseDto<object>>(new Exception("API error")));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sut.DeleteAsync(id));
        }

        #endregion

        #region GetProjectsAsync Tests

        [Fact]
        public async Task GetProjectsAsync_WithData_ReturnsListOfProjects()
        {
            // Arrange
            var projects = new List<string> { "PP001", "PP002", "PP003" };
            _pimsRadTrackInvoiceApiClient.GetProjectsAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = projects });

            // Act
            var result = await _sut.GetProjectsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(3, result.Data?.Count);
            await _pimsRadTrackInvoiceApiClient.Received(1).GetProjectsAsync();
        }

        [Fact]
        public async Task GetProjectsAsync_WithEmptyList_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            _pimsRadTrackInvoiceApiClient.GetProjectsAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = new List<string>() });

            // Act
            var result = await _sut.GetProjectsAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetProjectsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            _pimsRadTrackInvoiceApiClient.GetProjectsAsync()
                .Returns(new ApiResponseDto<List<string>>
                {
                    Success = false,
                    Errors  = OneError()
                });

            // Act
            var result = await _sut.GetProjectsAsync();

            // Assert
            Assert.False(result.Success);
            await _pimsRadTrackInvoiceApiClient.Received(1).GetProjectsAsync();
        }

        [Fact]
        public async Task GetProjectsAsync_DelegatesToApiClient()
        {
            // Arrange
            _pimsRadTrackInvoiceApiClient.GetProjectsAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = [] });

            // Act
            await _sut.GetProjectsAsync();

            // Assert
            await _pimsRadTrackInvoiceApiClient.Received(1).GetProjectsAsync();
        }

        #endregion

        #region GetYearsAsync Tests

        [Fact]
        public async Task GetYearsAsync_WithData_ReturnsListOfYears()
        {
            // Arrange
            var years = new List<int> { 2022, 2023, 2024 };
            _pimsRadTrackInvoiceApiClient.GetYearsAsync()
                .Returns(new ApiResponseDto<List<int>> { Success = true, Data = years });

            // Act
            var result = await _sut.GetYearsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(3, result.Data?.Count);
            await _pimsRadTrackInvoiceApiClient.Received(1).GetYearsAsync();
        }

        [Fact]
        public async Task GetYearsAsync_WithEmptyList_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            _pimsRadTrackInvoiceApiClient.GetYearsAsync()
                .Returns(new ApiResponseDto<List<int>> { Success = true, Data = new List<int>() });

            // Act
            var result = await _sut.GetYearsAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetYearsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            _pimsRadTrackInvoiceApiClient.GetYearsAsync()
                .Returns(new ApiResponseDto<List<int>>
                {
                    Success = false,
                    Errors  = OneError()
                });

            // Act
            var result = await _sut.GetYearsAsync();

            // Assert
            Assert.False(result.Success);
            await _pimsRadTrackInvoiceApiClient.Received(1).GetYearsAsync();
        }

        [Fact]
        public async Task GetYearsAsync_DelegatesToApiClient()
        {
            // Arrange
            _pimsRadTrackInvoiceApiClient.GetYearsAsync()
                .Returns(new ApiResponseDto<List<int>> { Success = true, Data = [] });

            // Act
            await _sut.GetYearsAsync();

            // Assert
            await _pimsRadTrackInvoiceApiClient.Received(1).GetYearsAsync();
        }

        #endregion

        #region GetContractsAsync Tests

        [Fact]
        public async Task GetContractsAsync_WithData_ReturnsListOfContracts()
        {
            // Arrange
            var contracts = new List<string> { "C001", "C002", "C003" };
            _pimsRadTrackInvoiceApiClient.GetContractsAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = contracts });

            // Act
            var result = await _sut.GetContractsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(3, result.Data?.Count);
            await _pimsRadTrackInvoiceApiClient.Received(1).GetContractsAsync();
        }

        [Fact]
        public async Task GetContractsAsync_WithEmptyList_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            _pimsRadTrackInvoiceApiClient.GetContractsAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = new List<string>() });

            // Act
            var result = await _sut.GetContractsAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetContractsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            _pimsRadTrackInvoiceApiClient.GetContractsAsync()
                .Returns(new ApiResponseDto<List<string>>
                {
                    Success = false,
                    Errors  = OneError()
                });

            // Act
            var result = await _sut.GetContractsAsync();

            // Assert
            Assert.False(result.Success);
            await _pimsRadTrackInvoiceApiClient.Received(1).GetContractsAsync();
        }

        [Fact]
        public async Task GetContractsAsync_DelegatesToApiClient()
        {
            // Arrange
            _pimsRadTrackInvoiceApiClient.GetContractsAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = [] });

            // Act
            await _sut.GetContractsAsync();

            // Assert
            await _pimsRadTrackInvoiceApiClient.Received(1).GetContractsAsync();
        }

        #endregion

        #region GetProgramsAsync Tests

        [Fact]
        public async Task GetProgramsAsync_WithData_ReturnsListOfPrograms()
        {
            // Arrange
            var programs = new List<string> { "PROG1", "PROG2", "PROG3" };
            _pimsRadTrackInvoiceApiClient.GetProgramsAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = programs });

            // Act
            var result = await _sut.GetProgramsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(3, result.Data?.Count);
            await _pimsRadTrackInvoiceApiClient.Received(1).GetProgramsAsync();
        }

        [Fact]
        public async Task GetProgramsAsync_WithEmptyList_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            _pimsRadTrackInvoiceApiClient.GetProgramsAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = new List<string>() });

            // Act
            var result = await _sut.GetProgramsAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetProgramsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            _pimsRadTrackInvoiceApiClient.GetProgramsAsync()
                .Returns(new ApiResponseDto<List<string>>
                {
                    Success = false,
                    Errors  = OneError()
                });

            // Act
            var result = await _sut.GetProgramsAsync();

            // Assert
            Assert.False(result.Success);
            await _pimsRadTrackInvoiceApiClient.Received(1).GetProgramsAsync();
        }

        [Fact]
        public async Task GetProgramsAsync_DelegatesToApiClient()
        {
            // Arrange
            _pimsRadTrackInvoiceApiClient.GetProgramsAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = [] });

            // Act
            await _sut.GetProgramsAsync();

            // Assert
            await _pimsRadTrackInvoiceApiClient.Received(1).GetProgramsAsync();
        }

        #endregion
    }
}
