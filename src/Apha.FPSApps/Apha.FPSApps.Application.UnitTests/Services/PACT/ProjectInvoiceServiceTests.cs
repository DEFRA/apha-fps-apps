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

        #region GetPagedProjectInvoiceManualAsync

        [Fact]
        public async Task GetPagedProjectInvoiceManualAsync_ValidQueryAndParentProject_DelegatesToApiClient()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var invoices = new List<ProjectInvoiceDto>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ001" },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ001" }
            };
            _invoiceApiClient.GetPagedProjectInvoiceManualAsync(query, "PRJ001")
                .Returns(ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse(invoices, new PaginationDto()));

            // Act
            var result = await _service.GetPagedProjectInvoiceManualAsync(query, "PRJ001");

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.Count);
            await _invoiceApiClient.Received(1).GetPagedProjectInvoiceManualAsync(query, "PRJ001");
        }

        [Fact]
        public async Task GetPagedProjectInvoiceManualAsync_NullParentProject_DelegatesToApiClientWithNull()
        {
            // Arrange
            var query = new QueryParameters<string>();
            _invoiceApiClient.GetPagedProjectInvoiceManualAsync(query, null)
                .Returns(ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse([], new PaginationDto()));

            // Act
            var result = await _service.GetPagedProjectInvoiceManualAsync(query, null);

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
            await _invoiceApiClient.Received(1).GetPagedProjectInvoiceManualAsync(query, null);
        }

        [Fact]
        public async Task GetPagedProjectInvoiceManualAsync_ApiClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var errors = new List<ApiErrorDto> { new() { Message = "API error", Code = "API_ERROR" } };
            _invoiceApiClient.GetPagedProjectInvoiceManualAsync(query, "PRJ001")
                .Returns(ApiResponseDto<List<ProjectInvoiceDto>>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _service.GetPagedProjectInvoiceManualAsync(query, "PRJ001");

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        #endregion

        #region GetPagedProjectInvoicesByMonthAsync

        [Fact]
        public async Task GetPagedProjectInvoicesByMonthAsync_ValidQueryAndMonth_DelegatesToApiClient()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 20 };
            var invoices = new List<ProjectInvoiceDto>
            {
                new() { InvoiceCounter = 1, Month = 5 },
                new() { InvoiceCounter = 2, Month = 5 },
                new() { InvoiceCounter = 3, Month = 5 }
            };
            _invoiceApiClient.GetPagedProjectInvoicesByMonthAsync(query, 5)
                .Returns(ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse(invoices, new PaginationDto { TotalRecords = 3 }));

            // Act
            var result = await _service.GetPagedProjectInvoicesByMonthAsync(query, 5);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(3, result.Data!.Count);
            Assert.All(result.Data, inv => Assert.Equal(5, inv.Month));
            await _invoiceApiClient.Received(1).GetPagedProjectInvoicesByMonthAsync(query, 5);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesByMonthAsync_NullMonth_DelegatesToApiClientWithNull()
        {
            // Arrange
            var query = new QueryParameters<string>();
            _invoiceApiClient.GetPagedProjectInvoicesByMonthAsync(query, null)
                .Returns(ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse([], new PaginationDto()));

            // Act
            var result = await _service.GetPagedProjectInvoicesByMonthAsync(query, null);

            // Assert
            Assert.True(result.Success);
            await _invoiceApiClient.Received(1).GetPagedProjectInvoicesByMonthAsync(query, null);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesByMonthAsync_ApiClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var errors = new List<ApiErrorDto> { new() { Message = "Invalid month" } };
            _invoiceApiClient.GetPagedProjectInvoicesByMonthAsync(query, 13)
                .Returns(ApiResponseDto<List<ProjectInvoiceDto>>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _service.GetPagedProjectInvoicesByMonthAsync(query, 13);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region CopyInvoicesAsync

        [Fact]
        public async Task CopyInvoicesAsync_ValidBulkCopyRequest_ReturnsSuccessTrue()
        {
            // Arrange
            var copyDto = new CopyInvoicesDto
            {
                SourceMonth = 5,
                TargetMonth = 6,
                InvoiceIds = null
            };
            _invoiceApiClient.CopyInvoicesAsync(copyDto)
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _service.CopyInvoicesAsync(copyDto);

            // Assert
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _invoiceApiClient.Received(1).CopyInvoicesAsync(copyDto);
        }

        [Fact]
        public async Task CopyInvoicesAsync_ValidSelectiveCopyRequest_ReturnsSuccessTrue()
        {
            // Arrange
            var copyDto = new CopyInvoicesDto
            {
                SourceMonth = 3,
                TargetMonth = 4,
                InvoiceIds = [1, 2, 3]
            };
            _invoiceApiClient.CopyInvoicesAsync(copyDto)
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _service.CopyInvoicesAsync(copyDto);

            // Assert
            Assert.True(result.Success);
            Assert.True(result.Data);
        }

        [Fact]
        public async Task CopyInvoicesAsync_ApiClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var copyDto = new CopyInvoicesDto { SourceMonth = 5, TargetMonth = 6 };
            var errors = new List<ApiErrorDto>
            {
                new() { Message = "Copy failed", Code = "COPY_ERROR" }
            };
            _invoiceApiClient.CopyInvoicesAsync(copyDto)
                .Returns(ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _service.CopyInvoicesAsync(copyDto);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task CopyInvoicesAsync_ApiClientReturnsSuccessFalse_ReturnsSuccessFalseData()
        {
            // Arrange
            var copyDto = new CopyInvoicesDto { SourceMonth = 5, TargetMonth = 6 };
            _invoiceApiClient.CopyInvoicesAsync(copyDto)
                .Returns(ApiResponseDto<bool>.SuccessResponse(false));

            // Act
            var result = await _service.CopyInvoicesAsync(copyDto);

            // Assert
            Assert.True(result.Success);
            Assert.False(result.Data);
        }

        [Fact]
        public async Task CopyInvoicesAsync_WithEmptyInvoiceIds_DelegatesToApiClient()
        {
            // Arrange
            var copyDto = new CopyInvoicesDto
            {
                SourceMonth = 1,
                TargetMonth = 2,
                InvoiceIds = []
            };
            _invoiceApiClient.CopyInvoicesAsync(copyDto)
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _service.CopyInvoicesAsync(copyDto);

            // Assert
            Assert.True(result.Success);
            await _invoiceApiClient.Received(1).CopyInvoicesAsync(Arg.Is<CopyInvoicesDto>(
                dto => dto.InvoiceIds != null && dto.InvoiceIds.Count == 0));
        }

        #endregion

        #region CreateAsync - Additional Tests

        [Fact]
        public async Task CreateAsync_ApiClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new ProjectInvoiceDto { ProjectParent = "PRJ001" };
            var errors = new List<ApiErrorDto> { new() { Message = "Validation error" } };
            _invoiceApiClient.CreateAsync(dto)
                .Returns(ApiResponseDto<ProjectInvoiceDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task CreateAsync_ValidDto_CallsApiClientOnce()
        {
            // Arrange
            var dto = new ProjectInvoiceDto { InvoiceCounter = 10 };
            _invoiceApiClient.CreateAsync(dto)
                .Returns(ApiResponseDto<ProjectInvoiceDto>.SuccessResponse(dto));

            // Act
            await _service.CreateAsync(dto);

            // Assert
            await _invoiceApiClient.Received(1).CreateAsync(dto);
        }

        #endregion

        #region UpdateAsync - Additional Tests

        [Fact]
        public async Task UpdateAsync_NotFound_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new ProjectInvoiceDto { InvoiceCounter = 999 };
            var errors = new List<ApiErrorDto> { new() { Message = "Invoice not found" } };
            _invoiceApiClient.UpdateAsync(999, dto)
                .Returns(ApiResponseDto<ProjectInvoiceDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _service.UpdateAsync(999, dto);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task UpdateAsync_ValidDto_CallsApiClientOnce()
        {
            // Arrange
            var dto = new ProjectInvoiceDto { InvoiceCounter = 5 };
            _invoiceApiClient.UpdateAsync(5, dto)
                .Returns(ApiResponseDto<ProjectInvoiceDto>.SuccessResponse(dto));

            // Act
            await _service.UpdateAsync(5, dto);

            // Assert
            await _invoiceApiClient.Received(1).UpdateAsync(5, dto);
        }

        #endregion

        #region GetPagedProjectInvoicesAsync - Additional Tests

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_ApiClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var errors = new List<ApiErrorDto> { new() { Message = "Server error" } };
            _invoiceApiClient.GetPagedProjectInvoicesAsync(query, "PRJ001")
                .Returns(ApiResponseDto<List<ProjectInvoiceDto>>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _service.GetPagedProjectInvoicesAsync(query, "PRJ001");

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_ValidQuery_CallsApiClientOnce()
        {
            // Arrange
            var query = new QueryParameters<string>();
            _invoiceApiClient.GetPagedProjectInvoicesAsync(query, null)
                .Returns(ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse([], new PaginationDto()));

            // Act
            await _service.GetPagedProjectInvoicesAsync(query, null);

            // Assert
            await _invoiceApiClient.Received(1).GetPagedProjectInvoicesAsync(query, null);
        }

        #endregion

        #region GetTotalAmountAsync - Additional Tests

        [Fact]
        public async Task GetTotalAmountAsync_NullParentProject_ReturnsTotal()
        {
            // Arrange
            _invoiceApiClient.GetTotalAmountAsync(null)
                .Returns(ApiResponseDto<decimal>.SuccessResponse(5000m));

            // Act
            var result = await _service.GetTotalAmountAsync(null);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(5000m, result.Data);
        }

        [Fact]
        public async Task GetTotalAmountAsync_ValidParentProject_CallsApiClientOnce()
        {
            // Arrange
            _invoiceApiClient.GetTotalAmountAsync("PRJ001")
                .Returns(ApiResponseDto<decimal>.SuccessResponse(0m));

            // Act
            await _service.GetTotalAmountAsync("PRJ001");

            // Assert
            await _invoiceApiClient.Received(1).GetTotalAmountAsync("PRJ001");
        }

        #endregion

        #region GetByIdAsync - Additional Tests

        [Fact]
        public async Task GetByIdAsync_ValidId_CallsApiClientOnce()
        {
            // Arrange
            var dto = new ProjectInvoiceDto { InvoiceCounter = 10 };
            _invoiceApiClient.GetByIdAsync(10)
                .Returns(ApiResponseDto<ProjectInvoiceDto>.SuccessResponse(dto));

            // Act
            await _service.GetByIdAsync(10);

            // Assert
            await _invoiceApiClient.Received(1).GetByIdAsync(10);
        }

        #endregion

        #region DeleteAsync - Additional Tests

        [Fact]
        public async Task DeleteAsync_ValidId_CallsApiClientOnce()
        {
            // Arrange
            _invoiceApiClient.DeleteAsync(5)
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            await _service.DeleteAsync(5);

            // Assert
            await _invoiceApiClient.Received(1).DeleteAsync(5);
        }

        #endregion

        #region GetMonthlyInvoicesSummaryAsync - Additional Tests

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_ValidQuery_CallsApiClientOnce()
        {
            // Arrange
            var query = new QueryParameters<string>();
            _invoiceApiClient.GetMonthlyInvoicesSummaryAsync(query)
                .Returns(ApiResponseDto<MonthlyInvoicesPivotDto>.SuccessResponse(new MonthlyInvoicesPivotDto()));

            // Act
            await _service.GetMonthlyInvoicesSummaryAsync(query);

            // Assert
            await _invoiceApiClient.Received(1).GetMonthlyInvoicesSummaryAsync(query);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_EmptyResult_ReturnsSuccessWithEmptyData()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var emptyDto = new MonthlyInvoicesPivotDto { Months = [], Rows = [] };
            _invoiceApiClient.GetMonthlyInvoicesSummaryAsync(query)
                .Returns(ApiResponseDto<MonthlyInvoicesPivotDto>.SuccessResponse(emptyDto));

            // Act
            var result = await _service.GetMonthlyInvoicesSummaryAsync(query);

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Data!.Months);
            Assert.Empty(result.Data.Rows);
        }

        #endregion
    }
}
