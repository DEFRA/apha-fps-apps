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

        #region GetPagedProjectInvoiceManualAsync Tests

        [Fact]
        public async Task GetPagedProjectInvoiceManualAsync_WithValidQuery_ReturnsPaginatedInvoices()
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
            _pactProjectInvoiceApiClient.GetPagedProjectInvoiceManualAsync(query, parentProject).Returns(expectedResponse);

            // Act
            var result = await _service.GetPagedProjectInvoiceManualAsync(query, parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _pactProjectInvoiceApiClient.Received(1).GetPagedProjectInvoiceManualAsync(query, parentProject);
        }

        [Fact]
        public async Task GetPagedProjectInvoiceManualAsync_WithNullProject_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse = ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse(new List<ProjectInvoiceDto>());
            _pactProjectInvoiceApiClient.GetPagedProjectInvoiceManualAsync(query, null).Returns(expectedResponse);

            // Act
            var result = await _service.GetPagedProjectInvoiceManualAsync(query, null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetPagedProjectInvoiceManualAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<ProjectInvoiceDto>>.FailureResponse(errors, new ApiMetaDto());
            _pactProjectInvoiceApiClient.GetPagedProjectInvoiceManualAsync(query, null).Returns(expectedResponse);

            // Act
            var result = await _service.GetPagedProjectInvoiceManualAsync(query, null);

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

        #region GetMonthlyInvoicesSummaryAsync Tests

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_WithValidQuery_ReturnsSummary()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var summary = new MonthlyInvoicesPivotDto();
            var expectedResponse = ApiResponseDto<MonthlyInvoicesPivotDto>.SuccessResponse(summary);
            _pactProjectInvoiceApiClient.GetMonthlyInvoicesSummaryAsync(query).Returns(expectedResponse);

            // Act
            var result = await _service.GetMonthlyInvoicesSummaryAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            await _pactProjectInvoiceApiClient.Received(1).GetMonthlyInvoicesSummaryAsync(query);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<MonthlyInvoicesPivotDto>.FailureResponse(errors, new ApiMetaDto());
            _pactProjectInvoiceApiClient.GetMonthlyInvoicesSummaryAsync(query).Returns(expectedResponse);

            // Act
            var result = await _service.GetMonthlyInvoicesSummaryAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetPagedProjectInvoicesByMonthAsync Tests

        [Fact]
        public async Task GetPagedProjectInvoicesByMonthAsync_WithValidMonth_ReturnsPaginatedInvoices()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            const int month = 6;
            var invoices = new List<ProjectInvoiceDto>
            {
                new ProjectInvoiceDto { InvoiceCounter = 1, ProjectParent = "PP001", Month = month, Amount = 100.00m },
                new ProjectInvoiceDto { InvoiceCounter = 2, ProjectParent = "PP002", Month = month, Amount = 200.00m }
            };
            var expectedResponse = ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse(
                invoices,
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            );
            _pactProjectInvoiceApiClient.GetPagedProjectInvoicesByMonthAsync(query, month).Returns(expectedResponse);

            // Act
            var result = await _service.GetPagedProjectInvoicesByMonthAsync(query, month);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            Assert.All(result.Data!, invoice => Assert.Equal(month, invoice.Month));
            await _pactProjectInvoiceApiClient.Received(1).GetPagedProjectInvoicesByMonthAsync(query, month);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesByMonthAsync_WithNullMonth_ReturnsAllInvoices()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var invoices = new List<ProjectInvoiceDto>
            {
                new ProjectInvoiceDto { InvoiceCounter = 1, Month = 1 },
                new ProjectInvoiceDto { InvoiceCounter = 2, Month = 2 },
                new ProjectInvoiceDto { InvoiceCounter = 3, Month = 3 }
            };
            var expectedResponse = ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse(invoices);
            _pactProjectInvoiceApiClient.GetPagedProjectInvoicesByMonthAsync(query, null).Returns(expectedResponse);

            // Act
            var result = await _service.GetPagedProjectInvoicesByMonthAsync(query, null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(3, result.Data?.Count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(6)]
        [InlineData(12)]
        public async Task GetPagedProjectInvoicesByMonthAsync_WithBoundaryMonths_ReturnsSuccess(int month)
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse = ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse(
                new List<ProjectInvoiceDto> { new() { Month = month } },
                new PaginationDto());
            _pactProjectInvoiceApiClient.GetPagedProjectInvoicesByMonthAsync(query, month).Returns(expectedResponse);

            // Act
            var result = await _service.GetPagedProjectInvoicesByMonthAsync(query, month);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesByMonthAsync_WithEmptyResult_ReturnsEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse = ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse(
                new List<ProjectInvoiceDto>(),
                new PaginationDto { TotalRecords = 0 });
            _pactProjectInvoiceApiClient.GetPagedProjectInvoicesByMonthAsync(query, 5).Returns(expectedResponse);

            // Act
            var result = await _service.GetPagedProjectInvoicesByMonthAsync(query, 5);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
            Assert.Equal(0, result.Pagination?.TotalRecords);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesByMonthAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<ProjectInvoiceDto>>.FailureResponse(errors, new ApiMetaDto());
            _pactProjectInvoiceApiClient.GetPagedProjectInvoicesByMonthAsync(query, 6).Returns(expectedResponse);

            // Act
            var result = await _service.GetPagedProjectInvoicesByMonthAsync(query, 6);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesByMonthAsync_WithPaginationParameters_PassesThroughCorrectly()
        {
            // Arrange
            var query = new QueryParameters<string> 
            { 
                Page = 2, 
                PageSize = 25,
                SortBy = "ProjectParent",
                Descending = true
            };
            const int month = 7;
            var expectedResponse = ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse(
                new List<ProjectInvoiceDto>(),
                new PaginationDto { PageNumber = 2, PageSize = 25 });
            _pactProjectInvoiceApiClient.GetPagedProjectInvoicesByMonthAsync(query, month).Returns(expectedResponse);

            // Act
            var result = await _service.GetPagedProjectInvoicesByMonthAsync(query, month);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _pactProjectInvoiceApiClient.Received(1).GetPagedProjectInvoicesByMonthAsync(
                Arg.Is<QueryParameters<string>>(q => 
                    q.Page == 2 && 
                    q.PageSize == 25 && 
                    q.SortBy == "ProjectParent" && 
                    q.Descending),
                month);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesByMonthAsync_WithLargeDataSet_HandlesCorrectly()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 100 };
            var invoices = Enumerable.Range(1, 100)
                .Select(i => new ProjectInvoiceDto 
                { 
                    InvoiceCounter = i, 
                    ProjectParent = $"PP{i:000}",
                    Month = 8,
                    Amount = i * 100m 
                })
                .ToList();
            var expectedResponse = ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse(
                invoices,
                new PaginationDto { PageNumber = 1, PageSize = 100, TotalRecords = 100 });
            _pactProjectInvoiceApiClient.GetPagedProjectInvoicesByMonthAsync(query, 8).Returns(expectedResponse);

            // Act
            var result = await _service.GetPagedProjectInvoicesByMonthAsync(query, 8);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(100, result.Data?.Count);
        }

        #endregion

        #region CopyInvoicesAsync Tests

        [Fact]
        public async Task CopyInvoicesAsync_BulkCopy_WithNullInvoiceRecords_ReturnsSuccessResult()
        {
            // Arrange
            var sourceMonth = 5;
            var destinationMonth = 6;
            var copyResult = new CopyInvoicesResultDto
            {
                Success = true,
                CopiedCount = 10,
                FailedCount = 0,
                Message = "Successfully copied 10 invoices",
                Errors = new List<string>()
            };
            var expectedResponse = ApiResponseDto<CopyInvoicesResultDto>.SuccessResponse(copyResult);
            _pactProjectInvoiceApiClient.CopyInvoicesAsync(sourceMonth, destinationMonth, null).Returns(expectedResponse);

            // Act
            var result = await _service.CopyInvoicesAsync(sourceMonth, destinationMonth, null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(10, result.Data?.CopiedCount);
            Assert.Equal(0, result.Data?.FailedCount);
            await _pactProjectInvoiceApiClient.Received(1).CopyInvoicesAsync(sourceMonth, destinationMonth, null);
        }

        [Fact]
        public async Task CopyInvoicesAsync_SelectiveCopy_WithInvoiceRecords_ReturnsSuccessResult()
        {
            // Arrange
            var sourceMonth = 3;
            var destinationMonth = 4;
            var invoiceRecords = new List<ProjectInvoiceDto>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PP001", Month = sourceMonth, Amount = 1000m },
                new() { InvoiceCounter = 2, ProjectParent = "PP002", Month = sourceMonth, Amount = 2000m }
            };
            var copyResult = new CopyInvoicesResultDto
            {
                Success = true,
                CopiedCount = 2,
                FailedCount = 0,
                Message = "Successfully copied 2 invoices"
            };
            var expectedResponse = ApiResponseDto<CopyInvoicesResultDto>.SuccessResponse(copyResult);
            _pactProjectInvoiceApiClient.CopyInvoicesAsync(sourceMonth, destinationMonth, invoiceRecords)
                .Returns(expectedResponse);

            // Act
            var result = await _service.CopyInvoicesAsync(sourceMonth, destinationMonth, invoiceRecords);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.CopiedCount);
            await _pactProjectInvoiceApiClient.Received(1).CopyInvoicesAsync(sourceMonth, destinationMonth, invoiceRecords);
        }

        [Fact]
        public async Task CopyInvoicesAsync_WithPartialFailure_ReturnsPartialSuccessResult()
        {
            // Arrange
            var sourceMonth = 5;
            var destinationMonth = 6;
            var copyResult = new CopyInvoicesResultDto
            {
                Success = false,
                CopiedCount = 8,
                FailedCount = 2,
                Message = "Copied 8 invoices with some failures",
                Errors = new List<string> { "Error copying invoice 1", "Error copying invoice 2" }
            };
            var expectedResponse = ApiResponseDto<CopyInvoicesResultDto>.SuccessResponse(copyResult);
            _pactProjectInvoiceApiClient.CopyInvoicesAsync(sourceMonth, destinationMonth, null).Returns(expectedResponse);

            // Act
            var result = await _service.CopyInvoicesAsync(sourceMonth, destinationMonth, null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success); // HTTP call succeeded
            Assert.False(result.Data?.Success); // But operation had failures
            Assert.Equal(8, result.Data?.CopiedCount);
            Assert.Equal(2, result.Data?.FailedCount);
            Assert.Equal(2, result.Data?.Errors.Count);
        }

        [Fact]
        public async Task CopyInvoicesAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var sourceMonth = 5;
            var destinationMonth = 6;
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<CopyInvoicesResultDto>.FailureResponse(errors, new ApiMetaDto());
            _pactProjectInvoiceApiClient.CopyInvoicesAsync(sourceMonth, destinationMonth, null).Returns(expectedResponse);

            // Act
            var result = await _service.CopyInvoicesAsync(sourceMonth, destinationMonth, null);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task CopyInvoicesAsync_WithEmptyInvoiceRecordsList_TreatsAsBulkCopy()
        {
            // Arrange
            var sourceMonth = 7;
            var destinationMonth = 8;
            var emptyList = new List<ProjectInvoiceDto>();
            var copyResult = new CopyInvoicesResultDto { Success = true, CopiedCount = 15 };
            var expectedResponse = ApiResponseDto<CopyInvoicesResultDto>.SuccessResponse(copyResult);
            _pactProjectInvoiceApiClient.CopyInvoicesAsync(sourceMonth, destinationMonth, emptyList)
                .Returns(expectedResponse);

            // Act
            var result = await _service.CopyInvoicesAsync(sourceMonth, destinationMonth, emptyList);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(15, result.Data?.CopiedCount);
        }

        [Theory]
        [InlineData(1, 2)]
        [InlineData(5, 6)]
        [InlineData(11, 12)]
        public async Task CopyInvoicesAsync_WithDifferentMonthPairs_CallsApiCorrectly(int source, int destination)
        {
            // Arrange
            var copyResult = new CopyInvoicesResultDto { Success = true, CopiedCount = 5 };
            var expectedResponse = ApiResponseDto<CopyInvoicesResultDto>.SuccessResponse(copyResult);
            _pactProjectInvoiceApiClient.CopyInvoicesAsync(source, destination, null).Returns(expectedResponse);

            // Act
            var result = await _service.CopyInvoicesAsync(source, destination, null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _pactProjectInvoiceApiClient.Received(1).CopyInvoicesAsync(source, destination, null);
        }

        [Fact]
        public async Task CopyInvoicesAsync_WithLargeInvoiceRecordsList_HandlesCorrectly()
        {
            // Arrange
            var sourceMonth = 3;
            var destinationMonth = 4;
            var largeList = Enumerable.Range(1, 100)
                .Select(i => new ProjectInvoiceDto 
                { 
                    InvoiceCounter = i, 
                    ProjectParent = $"PP{i:000}",
                    Month = sourceMonth 
                })
                .ToList();
            var copyResult = new CopyInvoicesResultDto { Success = true, CopiedCount = 100 };
            var expectedResponse = ApiResponseDto<CopyInvoicesResultDto>.SuccessResponse(copyResult);
            _pactProjectInvoiceApiClient.CopyInvoicesAsync(sourceMonth, destinationMonth, largeList)
                .Returns(expectedResponse);

            // Act
            var result = await _service.CopyInvoicesAsync(sourceMonth, destinationMonth, largeList);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(100, result.Data?.CopiedCount);
        }

        [Fact]
        public async Task CopyInvoicesAsync_WhenNoInvoicesFound_ReturnsErrorResult()
        {
            // Arrange
            var sourceMonth = 5;
            var destinationMonth = 6;
            var copyResult = new CopyInvoicesResultDto
            {
                Success = false,
                CopiedCount = 0,
                FailedCount = 0,
                Errors = new List<string> { "No invoices found for source month 5" },
                Message = "No invoices to copy"
            };
            var expectedResponse = ApiResponseDto<CopyInvoicesResultDto>.SuccessResponse(copyResult);
            _pactProjectInvoiceApiClient.CopyInvoicesAsync(sourceMonth, destinationMonth, null).Returns(expectedResponse);

            // Act
            var result = await _service.CopyInvoicesAsync(sourceMonth, destinationMonth, null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success); // HTTP succeeded
            Assert.False(result.Data?.Success); // Operation failed
            Assert.Equal(0, result.Data?.CopiedCount);
            Assert.Contains("No invoices found", result.Data?.Errors.First());
        }

        [Fact]
        public async Task CopyInvoicesAsync_AllInvoicesFail_ReturnsAllErrors()
        {
            // Arrange
            var sourceMonth = 3;
            var destinationMonth = 4;
            var invoiceRecords = new List<ProjectInvoiceDto>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PP001" },
                new() { InvoiceCounter = 2, ProjectParent = "PP002" }
            };
            var copyResult = new CopyInvoicesResultDto
            {
                Success = false,
                CopiedCount = 0,
                FailedCount = 2,
                Errors = new List<string> 
                { 
                    "Failed to copy invoice 1: Duplicate",
                    "Failed to copy invoice 2: Duplicate"
                },
                Message = "All copy operations failed"
            };
            var expectedResponse = ApiResponseDto<CopyInvoicesResultDto>.SuccessResponse(copyResult);
            _pactProjectInvoiceApiClient.CopyInvoicesAsync(sourceMonth, destinationMonth, invoiceRecords)
                .Returns(expectedResponse);

            // Act
            var result = await _service.CopyInvoicesAsync(sourceMonth, destinationMonth, invoiceRecords);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Data?.Success);
            Assert.Equal(0, result.Data?.CopiedCount);
            Assert.Equal(2, result.Data?.FailedCount);
            Assert.Equal(2, result.Data?.Errors.Count);
        }

        [Fact]
        public async Task CopyInvoicesAsync_PassesThroughApiClientResponse()
        {
            // Arrange
            var sourceMonth = 9;
            var destinationMonth = 10;
            var copyResult = new CopyInvoicesResultDto
            {
                Success = true,
                CopiedCount = 7,
                FailedCount = 0,
                Message = "Custom success message",
                Errors = new List<string>()
            };
            var expectedResponse = ApiResponseDto<CopyInvoicesResultDto>.SuccessResponse(copyResult);
            _pactProjectInvoiceApiClient.CopyInvoicesAsync(sourceMonth, destinationMonth, null).Returns(expectedResponse);

            // Act
            var result = await _service.CopyInvoicesAsync(sourceMonth, destinationMonth, null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(7, result.Data?.CopiedCount);
            Assert.Equal("Custom success message", result.Data?.Message);
            Assert.Empty(result.Data!.Errors);
        }

        #endregion
    }
}
