using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.PACT;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.PACT.ProjectInvoiceServiceTest
{
    public class ProjectInvoiceServiceTests
    {
        private readonly IPactApiClient _pactClient;
        private readonly IPactProjectInvoiceApiClient _pactProjectInvoiceApiClient;
        private readonly ProjectInvoiceService _service;

        public ProjectInvoiceServiceTests()
        {
            _pactClient = Substitute.For<IPactApiClient>();
            _pactProjectInvoiceApiClient = Substitute.For<IPactProjectInvoiceApiClient>();
            _pactClient.PactProjectInvoice.Returns(_pactProjectInvoiceApiClient);
            _service = new ProjectInvoiceService(_pactClient);
        }

        #region GetPagedProjectInvoicesAsync Tests

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_WithValidQuery_ReturnsPaginatedInvoices()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var parentProject = "PP001";
            var invoices = new List<ProjectInvoiceDto>
            {
                new ProjectInvoiceDto { InvoiceCounter = 1, ProjectParent = parentProject, Amount = 100.00m },
                new ProjectInvoiceDto { InvoiceCounter = 2, ProjectParent = parentProject, Amount = 200.00m }
            };
            var expectedResponse = ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse(
                invoices,
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            );
            _pactProjectInvoiceApiClient.GetPagedProjectInvoicesAsync(query, parentProject).Returns(expectedResponse);

            // Act
            var result = await _service.GetPagedProjectInvoicesAsync(query, parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _pactProjectInvoiceApiClient.Received(1).GetPagedProjectInvoicesAsync(query, parentProject);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_WithNullProject_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse = ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse(new List<ProjectInvoiceDto>());
            _pactProjectInvoiceApiClient.GetPagedProjectInvoicesAsync(query, null).Returns(expectedResponse);

            // Act
            var result = await _service.GetPagedProjectInvoicesAsync(query, null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<ProjectInvoiceDto>>.FailureResponse(errors, new ApiMetaDto());
            _pactProjectInvoiceApiClient.GetPagedProjectInvoicesAsync(query, null).Returns(expectedResponse);

            // Act
            var result = await _service.GetPagedProjectInvoicesAsync(query, null);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        #endregion

        #region GetTotalAmountAsync Tests

        [Fact]
        public async Task GetTotalAmountAsync_WithValidProject_ReturnsTotalAmount()
        {
            // Arrange
            var parentProject = "PP001";
            var expectedResponse = ApiResponseDto<decimal>.SuccessResponse(1500.00m);
            _pactProjectInvoiceApiClient.GetTotalAmountAsync(parentProject).Returns(expectedResponse);

            // Act
            var result = await _service.GetTotalAmountAsync(parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(1500.00m, result.Data);
            await _pactProjectInvoiceApiClient.Received(1).GetTotalAmountAsync(parentProject);
        }

        [Fact]
        public async Task GetTotalAmountAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<decimal>.FailureResponse(errors, new ApiMetaDto());
            _pactProjectInvoiceApiClient.GetTotalAmountAsync(null).Returns(expectedResponse);

            // Act
            var result = await _service.GetTotalAmountAsync(null);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsInvoice()
        {
            // Arrange
            var invoiceCounter = 1;
            var invoice = new ProjectInvoiceDto { InvoiceCounter = invoiceCounter, ProjectParent = "PP001", Amount = 500.00m };
            var expectedResponse = ApiResponseDto<ProjectInvoiceDto>.SuccessResponse(invoice);
            _pactProjectInvoiceApiClient.GetByIdAsync(invoiceCounter).Returns(expectedResponse);

            // Act
            var result = await _service.GetByIdAsync(invoiceCounter);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(invoiceCounter, result.Data?.InvoiceCounter);
            await _pactProjectInvoiceApiClient.Received(1).GetByIdAsync(invoiceCounter);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistentId_ReturnsFailureResponse()
        {
            // Arrange
            var invoiceCounter = 9999;
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Invoice not found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<ProjectInvoiceDto>.FailureResponse(errors, new ApiMetaDto());
            _pactProjectInvoiceApiClient.GetByIdAsync(invoiceCounter).Returns(expectedResponse);

            // Act
            var result = await _service.GetByIdAsync(invoiceCounter);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_WithValidInvoice_ReturnsSuccessResponse()
        {
            // Arrange
            var newInvoice = new ProjectInvoiceDto
            {
                InvoiceCounter = 1,
                ProjectParent = "PP001",
                Amount = 750.00m,
                Type = "Standard"
            };
            var expectedResponse = ApiResponseDto<ProjectInvoiceDto>.SuccessResponse(newInvoice);
            _pactProjectInvoiceApiClient.CreateAsync(newInvoice).Returns(expectedResponse);

            // Act
            var result = await _service.CreateAsync(newInvoice);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(newInvoice.InvoiceCounter, result.Data?.InvoiceCounter);
            await _pactProjectInvoiceApiClient.Received(1).CreateAsync(newInvoice);
        }

        [Fact]
        public async Task CreateAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var newInvoice = new ProjectInvoiceDto { ProjectParent = "PP001" };
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Duplicate invoice", Code = "DUPLICATE" } };
            var expectedResponse = ApiResponseDto<ProjectInvoiceDto>.FailureResponse(errors, new ApiMetaDto());
            _pactProjectInvoiceApiClient.CreateAsync(newInvoice).Returns(expectedResponse);

            // Act
            var result = await _service.CreateAsync(newInvoice);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WithValidInvoice_ReturnsSuccessResponse()
        {
            // Arrange
            var invoiceCounter = 1;
            var updatedInvoice = new ProjectInvoiceDto
            {
                InvoiceCounter = invoiceCounter,
                ProjectParent = "PP001",
                Amount = 900.00m,
                Type = "Revised"
            };
            var expectedResponse = ApiResponseDto<ProjectInvoiceDto>.SuccessResponse(updatedInvoice);
            _pactProjectInvoiceApiClient.UpdateAsync(invoiceCounter, updatedInvoice).Returns(expectedResponse);

            // Act
            var result = await _service.UpdateAsync(invoiceCounter, updatedInvoice);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(900.00m, result.Data?.Amount);
            await _pactProjectInvoiceApiClient.Received(1).UpdateAsync(invoiceCounter, updatedInvoice);
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistentId_ReturnsFailureResponse()
        {
            // Arrange
            var invoiceCounter = 9999;
            var invoice = new ProjectInvoiceDto { InvoiceCounter = invoiceCounter, ProjectParent = "PP001" };
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Invoice not found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<ProjectInvoiceDto>.FailureResponse(errors, new ApiMetaDto());
            _pactProjectInvoiceApiClient.UpdateAsync(invoiceCounter, invoice).Returns(expectedResponse);

            // Act
            var result = await _service.UpdateAsync(invoiceCounter, invoice);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WithValidId_ReturnsSuccessResponse()
        {
            // Arrange
            var invoiceCounter = 1;
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _pactProjectInvoiceApiClient.DeleteAsync(invoiceCounter).Returns(expectedResponse);

            // Act
            var result = await _service.DeleteAsync(invoiceCounter);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _pactProjectInvoiceApiClient.Received(1).DeleteAsync(invoiceCounter);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistentId_ReturnsFailureResponse()
        {
            // Arrange
            var invoiceCounter = 9999;
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Invoice not found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _pactProjectInvoiceApiClient.DeleteAsync(invoiceCounter).Returns(expectedResponse);

            // Act
            var result = await _service.DeleteAsync(invoiceCounter);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion
    }
}
