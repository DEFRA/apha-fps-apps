using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.PACT;
using NSubstitute;

namespace Apha.FPSApps.Application.UnitTests.Services.PACT
{
    public class ProjectInvoiceServiceTests
    {
        private readonly IPactApiClient _pactClient;
        private readonly IPactProjectInvoiceApiClient _invoiceApiClient;
        private readonly ProjectInvoiceService _service;

        public ProjectInvoiceServiceTests()
        {
            _pactClient = Substitute.For<IPactApiClient>();
            _invoiceApiClient = Substitute.For<IPactProjectInvoiceApiClient>();
            _pactClient.PactProjectInvoice.Returns(_invoiceApiClient);
            _service = new ProjectInvoiceService(_pactClient);
        }

        #region GetMonthlyInvoicesSummaryAsync

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_ValidQuery_DelegatesToApiClient()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var pivotDto = new MonthlyInvoicesPivotDto
            {
                Months = [1, 2],
                Rows = [],
                Pagination = new PaginationDto()
            };
            _invoiceApiClient.GetMonthlyInvoicesSummaryAsync(query)
                .Returns(ApiResponseDto<MonthlyInvoicesPivotDto>.SuccessResponse(pivotDto));

            // Act
            var result = await _service.GetMonthlyInvoicesSummaryAsync(query);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Months.Count);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_ApiClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var errors = new List<ApiErrorDto> { new() { Message = "Server error" } };
            _invoiceApiClient.GetMonthlyInvoicesSummaryAsync(query)
                .Returns(ApiResponseDto<MonthlyInvoicesPivotDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _service.GetMonthlyInvoicesSummaryAsync(query);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region GetPagedProjectInvoicesAsync

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_ValidQueryAndParentProject_DelegatesToApiClient()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var invoices = new List<ProjectInvoiceDto> { new() { InvoiceCounter = 1 } };
            _invoiceApiClient.GetPagedProjectInvoicesAsync(query, "PRJ001")
                .Returns(ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse(invoices, new PaginationDto()));

            // Act
            var result = await _service.GetPagedProjectInvoicesAsync(query, "PRJ001");

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_NullParentProject_DelegatesToApiClientWithNull()
        {
            // Arrange
            var query = new QueryParameters<string>();
            _invoiceApiClient.GetPagedProjectInvoicesAsync(query, null)
                .Returns(ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse([], new PaginationDto()));

            // Act
            var result = await _service.GetPagedProjectInvoicesAsync(query, null);

            // Assert
            Assert.True(result.Success);
        }

        #endregion

        #region GetTotalAmountAsync

        [Fact]
        public async Task GetTotalAmountAsync_ValidParentProject_ReturnsTotalFromApiClient()
        {
            // Arrange
            _invoiceApiClient.GetTotalAmountAsync("PRJ001")
                .Returns(ApiResponseDto<decimal>.SuccessResponse(1500m));

            // Act
            var result = await _service.GetTotalAmountAsync("PRJ001");

            // Assert
            Assert.True(result.Success);
            Assert.Equal(1500m, result.Data);
        }

        [Fact]
        public async Task GetTotalAmountAsync_ApiClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "error" } };
            _invoiceApiClient.GetTotalAmountAsync(null)
                .Returns(ApiResponseDto<decimal>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _service.GetTotalAmountAsync(null);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_ExistingId_ReturnsInvoiceDto()
        {
            // Arrange
            var dto = new ProjectInvoiceDto { InvoiceCounter = 5 };
            _invoiceApiClient.GetByIdAsync(5)
                .Returns(ApiResponseDto<ProjectInvoiceDto>.SuccessResponse(dto));

            // Act
            var result = await _service.GetByIdAsync(5);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(5, result.Data!.InvoiceCounter);
        }

        [Fact]
        public async Task GetByIdAsync_NotFound_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Not found" } };
            _invoiceApiClient.GetByIdAsync(99)
                .Returns(ApiResponseDto<ProjectInvoiceDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _service.GetByIdAsync(99);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_ValidDto_ReturnsCreatedInvoice()
        {
            // Arrange
            var dto = new ProjectInvoiceDto { InvoiceCounter = 1 };
            _invoiceApiClient.CreateAsync(dto)
                .Returns(ApiResponseDto<ProjectInvoiceDto>.SuccessResponse(dto));

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            Assert.True(result.Success);
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_ValidDto_ReturnsUpdatedInvoice()
        {
            // Arrange
            var dto = new ProjectInvoiceDto { InvoiceCounter = 3 };
            _invoiceApiClient.UpdateAsync(3, dto)
                .Returns(ApiResponseDto<ProjectInvoiceDto>.SuccessResponse(dto));

            // Act
            var result = await _service.UpdateAsync(3, dto);

            // Assert
            Assert.True(result.Success);
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_ExistingId_ReturnsSuccessTrue()
        {
            // Arrange
            _invoiceApiClient.DeleteAsync(7)
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _service.DeleteAsync(7);

            // Assert
            Assert.True(result.Success);
            Assert.True(result.Data);
        }

        [Fact]
        public async Task DeleteAsync_NotFound_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Not found" } };
            _invoiceApiClient.DeleteAsync(99)
                .Returns(ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _service.DeleteAsync(99);

            // Assert
            Assert.False(result.Success);
        }

        #endregion
    }
}
