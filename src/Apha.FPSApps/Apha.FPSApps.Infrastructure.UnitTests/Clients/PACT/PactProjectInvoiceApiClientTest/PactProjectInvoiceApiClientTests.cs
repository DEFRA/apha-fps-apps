using Apha.Common.Constants;
using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using Apha.FPSApps.Infrastructure.Integrations.PACTApis.Clients;
using AutoMapper;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PACT.PactProjectInvoiceApiClientTest
{
    public class PactProjectInvoiceApiClientTests
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PactProjectInvoiceApiClient _client;

        public PactProjectInvoiceApiClientTests()
        {
            _http = Substitute.For<IPactHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PactProjectInvoiceApiClient(_http, _mapper);
        }

        #region GetPagedProjectInvoicesAsync Tests

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_WithParentProject_IncludesProjectInUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var parentProject = "PP001";
            var invoiceList = new List<ProjectInvoiceRes>
            {
                new() { InvoiceCounter = 1, ProjectParent = parentProject, Amount = 100.00m },
                new() { InvoiceCounter = 2, ProjectParent = parentProject, Amount = 200.00m }
            };
            var apiResponse = new ApiResponse<List<ProjectInvoiceRes>>
            {
                Success = true,
                Data = invoiceList,
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            };
            var expectedDto = ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse(
                new List<ProjectInvoiceDto>
                {
                    new() { InvoiceCounter = 1, ProjectParent = parentProject },
                    new() { InvoiceCounter = 2, ProjectParent = parentProject }
                },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            );

            _http.GetAsync<List<ProjectInvoiceRes>>(Arg.Is<string>(url =>
                url.Contains("api/v1/projectinvoice") && url.Contains($"parentProject={Uri.EscapeDataString(parentProject)}")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectInvoiceDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedProjectInvoicesAsync(query, parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<ProjectInvoiceRes>>(
                Arg.Is<string>(url => url.Contains("api/v1/projectinvoice") && url.Contains($"parentProject={Uri.EscapeDataString(parentProject)}")));
        }

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_WithNullParentProject_UsesBaseUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<ProjectInvoiceRes>> { Success = true, Data = new List<ProjectInvoiceRes>() };
            var expectedDto = ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse(new List<ProjectInvoiceDto>(), new PaginationDto());

            _http.GetAsync<List<ProjectInvoiceRes>>(Arg.Is<string>(url => url.Contains("api/v1/projectinvoice"))).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectInvoiceDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedProjectInvoicesAsync(query, null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<ProjectInvoiceRes>>(Arg.Is<string>(url => url.Contains("api/v1/projectinvoice")));
        }

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiError> { new() { Message = "API Error", Code = "API_ERROR" } };
            var apiResponse = new ApiResponse<List<ProjectInvoiceRes>> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<List<ProjectInvoiceDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ProjectInvoiceRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectInvoiceDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetPagedProjectInvoicesAsync(query, null);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetTotalAmountAsync Tests

        [Fact]
        public async Task GetTotalAmountAsync_WithParentProject_IncludesProjectInUrl()
        {
            // Arrange
            var parentProject = "PP001";
            var apiResponse = new ApiResponse<decimal?> { Success = true, Data = 1500.00m };
            var expectedDto = ApiResponseDto<decimal>.SuccessResponse(1500.00m);

            _http.GetAsync<decimal?>(Arg.Is<string>(url =>
                url.Contains("api/v1/projectinvoice/total") && url.Contains("parentProject=PP001")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<decimal>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetTotalAmountAsync(parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(1500.00m, result.Data);
            await _http.Received(1).GetAsync<decimal?>(
                Arg.Is<string>(url => url.Contains("api/v1/projectinvoice/total") && url.Contains("parentProject=PP001")));
        }

        [Fact]
        public async Task GetTotalAmountAsync_WithNullParentProject_ReturnsZeroWithoutHttpCall()
        {
            // Act
            var result = await _client.GetTotalAmountAsync(null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(0m, result.Data);
            await _http.DidNotReceive().GetAsync<decimal?>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetTotalAmountAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<decimal?>
            {
                Success = false,
                Errors = [new ApiError { Message = "Server error" }]
            };
            var mappedResponse = new ApiResponseDto<decimal>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Server error" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<decimal?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<decimal>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetTotalAmountAsync("PROJ-001");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsMappedInvoice()
        {
            // Arrange
            var invoiceCounter = 1;
            var invoiceRes = new ProjectInvoiceRes { InvoiceCounter = invoiceCounter, ProjectParent = "PP001", Amount = 500.00m };
            var apiResponse = new ApiResponse<ProjectInvoiceRes> { Success = true, Data = invoiceRes };
            var expectedDto = ApiResponseDto<ProjectInvoiceDto>.SuccessResponse(
                new ProjectInvoiceDto { InvoiceCounter = invoiceCounter, ProjectParent = "PP001" }
            );

            _http.GetAsync<ProjectInvoiceRes>($"api/v1/projectinvoice/invoice/id?id={invoiceCounter}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectInvoiceDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetByIdAsync(invoiceCounter);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(invoiceCounter, result.Data?.InvoiceCounter);
            await _http.Received(1).GetAsync<ProjectInvoiceRes>($"api/v1/projectinvoice/invoice/id?id={invoiceCounter}");
        }

        [Fact]
        public async Task GetByIdAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<ProjectInvoiceRes> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<ProjectInvoiceDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<ProjectInvoiceRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectInvoiceDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetByIdAsync(9999);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_WithValidInvoice_ReturnsMappedCreatedInvoice()
        {
            // Arrange
            var invoiceDto = new ProjectInvoiceDto { InvoiceCounter = 1, ProjectParent = "PP001", Amount = 750.00m };
            var invoiceReq = new ProjectInvoiceReq { ProjectParent = "PP001", Amount = 750.00m };
            var invoiceRes = new ProjectInvoiceRes { InvoiceCounter = 1, ProjectParent = "PP001", Amount = 750.00m };
            var apiResponse = new ApiResponse<ProjectInvoiceRes> { Success = true, Data = invoiceRes };
            var expectedDto = ApiResponseDto<ProjectInvoiceDto>.SuccessResponse(invoiceDto);

            _mapper.Map<ProjectInvoiceReq>(invoiceDto).Returns(invoiceReq);
            _http.PostAsync<ProjectInvoiceReq, ProjectInvoiceRes>("api/v1/projectinvoice", invoiceReq).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectInvoiceDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.CreateAsync(invoiceDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(1, result.Data?.InvoiceCounter);
            await _http.Received(1).PostAsync<ProjectInvoiceReq, ProjectInvoiceRes>("api/v1/projectinvoice", invoiceReq);
        }

        [Fact]
        public async Task CreateAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var invoiceDto = new ProjectInvoiceDto { ProjectParent = "PP001" };
            var invoiceReq = new ProjectInvoiceReq { ProjectParent = "PP001" };
            var errors = new List<ApiError> { new() { Message = "Duplicate", Code = "DUPLICATE" } };
            var apiResponse = new ApiResponse<ProjectInvoiceRes> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<ProjectInvoiceDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Duplicate", Code = "DUPLICATE" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<ProjectInvoiceReq>(invoiceDto).Returns(invoiceReq);
            _http.PostAsync<ProjectInvoiceReq, ProjectInvoiceRes>(Arg.Any<string>(), Arg.Any<ProjectInvoiceReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectInvoiceDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.CreateAsync(invoiceDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WithValidInvoice_ReturnsMappedUpdatedInvoice()
        {
            // Arrange
            var invoiceCounter = 1;
            var invoiceDto = new ProjectInvoiceDto { InvoiceCounter = invoiceCounter, ProjectParent = "PP001", Amount = 900.00m };
            var invoiceReq = new ProjectInvoiceReq { ProjectParent = "PP001", Amount = 900.00m };
            var invoiceRes = new ProjectInvoiceRes { InvoiceCounter = invoiceCounter, ProjectParent = "PP001", Amount = 900.00m };
            var apiResponse = new ApiResponse<ProjectInvoiceRes> { Success = true, Data = invoiceRes };
            var expectedDto = ApiResponseDto<ProjectInvoiceDto>.SuccessResponse(invoiceDto);

            _mapper.Map<ProjectInvoiceReq>(Arg.Any<ProjectInvoiceDto>()).Returns(invoiceReq);
            _http.PutAsync<ProjectInvoiceReq, ProjectInvoiceRes>($"api/v1/projectinvoice/invoice/id?id={invoiceCounter}", Arg.Any<ProjectInvoiceReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectInvoiceDto>>(Arg.Any<ApiResponse<ProjectInvoiceRes>>()).Returns(expectedDto);

            // Act
            var result = await _client.UpdateAsync(invoiceCounter, invoiceDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(900.00m, result.Data?.Amount);
            await _http.Received(1).PutAsync<ProjectInvoiceReq, ProjectInvoiceRes>($"api/v1/projectinvoice/invoice/id?id={invoiceCounter}", Arg.Any<ProjectInvoiceReq>());
        }

        [Fact]
        public async Task UpdateAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var invoiceCounter = 9999;
            var invoiceDto = new ProjectInvoiceDto { InvoiceCounter = invoiceCounter, ProjectParent = "PP001" };
            var invoiceReq = new ProjectInvoiceReq { ProjectParent = "PP001" };
            var errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<ProjectInvoiceRes> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<ProjectInvoiceDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<ProjectInvoiceReq>(invoiceDto).Returns(invoiceReq);
            _http.PutAsync<ProjectInvoiceReq, ProjectInvoiceRes>(Arg.Any<string>(), Arg.Any<ProjectInvoiceReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectInvoiceDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.UpdateAsync(invoiceCounter, invoiceDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WithValidId_ReturnsSuccess()
        {
            // Arrange
            var invoiceCounter = 1;
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool?>($"api/v1/projectinvoice/invoice/id?id={invoiceCounter}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.DeleteAsync(invoiceCounter);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _http.Received(1).DeleteAsync<bool?>($"api/v1/projectinvoice/invoice/id?id={invoiceCounter}");
        }

        [Fact]
        public async Task DeleteAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<bool?> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.DeleteAsync(9999);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetMonthlyInvoicesSummaryAsync Tests

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_SuccessResponse_ReturnsMappedPivotDto()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var pivotRes = new MonthlyInvoicesPivotRes
            {
                Months = [1, 2],
                Rows = [],
                Pagination = new Pagination()
            };
            var apiResponse = new ApiResponse<MonthlyInvoicesPivotRes> { Success = true, Data = pivotRes };
            var expectedDto = new ApiResponseDto<MonthlyInvoicesPivotDto>
            {
                Success = true,
                Data = new MonthlyInvoicesPivotDto { Months = [1, 2] }
            };

            _http.GetAsync<MonthlyInvoicesPivotRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MonthlyInvoicesPivotDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetMonthlyInvoicesSummaryAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(expectedDto.Data, result.Data);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_FailureResponse_ReturnsFailure()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var apiResponse = new ApiResponse<MonthlyInvoicesPivotRes>
            {
                Success = false,
                Errors = [new ApiError { Message = "Server error" }]
            };
            var mappedResponse = new ApiResponseDto<MonthlyInvoicesPivotDto>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Server error" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<MonthlyInvoicesPivotRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MonthlyInvoicesPivotDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetMonthlyInvoicesSummaryAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_UsesCorrectEndpoint()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var apiResponse = new ApiResponse<MonthlyInvoicesPivotRes> { Success = true, Data = new MonthlyInvoicesPivotRes() };
            _http.GetAsync<MonthlyInvoicesPivotRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MonthlyInvoicesPivotDto>>(apiResponse)
                .Returns(new ApiResponseDto<MonthlyInvoicesPivotDto> { Success = true });

            // Act
            await _client.GetMonthlyInvoicesSummaryAsync(query);

            // Assert
            await _http.Received(1).GetAsync<MonthlyInvoicesPivotRes>(
                Arg.Is<string>(url => url.StartsWith(PactApiEndpoints.GetMonthlyInvoicesSummary)));
        }

        #endregion

        #region GetPagedProjectInvoiceManualAsync Tests

        [Fact]
        public async Task GetPagedProjectInvoiceManualAsync_WithParentProject_IncludesProjectInUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var parentProject = "PP001";
            var apiResponse = new ApiResponse<List<ProjectInvoiceRes>> { Success = true, Data = new List<ProjectInvoiceRes> { new() { InvoiceCounter = 1 } } };
            var expectedDto = ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse(
                new List<ProjectInvoiceDto> { new() { InvoiceCounter = 1 } }, new PaginationDto());

            _http.GetAsync<List<ProjectInvoiceRes>>(Arg.Is<string>(url =>
                url.Contains(PactApiEndpoints.GetPagedProjectInvoiceManual) && url.Contains($"parentProject={Uri.EscapeDataString(parentProject)}")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectInvoiceDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedProjectInvoiceManualAsync(query, parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
        }

        [Fact]
        public async Task GetPagedProjectInvoiceManualAsync_WithNullParentProject_DoesNotIncludeProjectInUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<ProjectInvoiceRes>> { Success = true, Data = new List<ProjectInvoiceRes>() };
            var expectedDto = ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse(new List<ProjectInvoiceDto>(), new PaginationDto());

            _http.GetAsync<List<ProjectInvoiceRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectInvoiceDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedProjectInvoiceManualAsync(query, null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<ProjectInvoiceRes>>(
                Arg.Is<string>(url => !url.Contains("parentProject")));
        }

        [Fact]
        public async Task GetPagedProjectInvoiceManualAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var apiResponse = new ApiResponse<List<ProjectInvoiceRes>>
            {
                Success = false,
                Errors = [new ApiError { Message = "Server error" }]
            };
            var mappedResponse = new ApiResponseDto<List<ProjectInvoiceDto>>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Server error" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ProjectInvoiceRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectInvoiceDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetPagedProjectInvoiceManualAsync(query, null);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetFailedInvoiceImportAsync Tests

        [Fact]
        public async Task GetFailedInvoiceImportAsync_SuccessResponse_ReturnsMappedList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<InvoiceImportRowRes>>
            {
                Success = true,
                Data = new List<InvoiceImportRowRes> { new() { Id = 1, ProjectParent = "PP001" } }
            };
            var expectedDto = ApiResponseDto<List<InvoiceImportRowDto>>.SuccessResponse(
                new List<InvoiceImportRowDto> { new() { Id = 1, ProjectParent = "PP001" } });

            _http.GetAsync<List<InvoiceImportRowRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<InvoiceImportRowDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetFailedInvoiceImportAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
        }

        [Fact]
        public async Task GetFailedInvoiceImportAsync_FailureResponse_ReturnsFailure()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var apiResponse = new ApiResponse<List<InvoiceImportRowRes>>
            {
                Success = false,
                Errors = [new ApiError { Message = "Server error" }]
            };
            var mappedResponse = new ApiResponseDto<List<InvoiceImportRowDto>>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Server error" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<InvoiceImportRowRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<InvoiceImportRowDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetFailedInvoiceImportAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetFailedInvoiceImportAsync_FailureWithNullMeta_UsesNewApiMetaDto()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var apiResponse = new ApiResponse<List<InvoiceImportRowRes>>
            {
                Success = false,
                Errors = [new ApiError { Message = "Error" }]
            };
            var mappedResponse = new ApiResponseDto<List<InvoiceImportRowDto>>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Error" }],
                Meta = null!
            };

            _http.GetAsync<List<InvoiceImportRowRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<InvoiceImportRowDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetFailedInvoiceImportAsync(query);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Meta);
        }

        #endregion

        #region GetFailedInvoiceImportByIdAsync Tests

        [Fact]
        public async Task GetFailedInvoiceImportByIdAsync_SuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            var apiResponse = new ApiResponse<InvoiceImportRowRes>
            {
                Success = true,
                Data = new InvoiceImportRowRes { Id = 5, ProjectParent = "PP001" }
            };
            var expectedDto = ApiResponseDto<InvoiceImportRowDto>.SuccessResponse(
                new InvoiceImportRowDto { Id = 5, ProjectParent = "PP001" });

            _http.GetAsync<InvoiceImportRowRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<InvoiceImportRowDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetFailedInvoiceImportByIdAsync(5);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(5, result.Data!.Id);
        }

        [Fact]
        public async Task GetFailedInvoiceImportByIdAsync_FailureResponse_ReturnsFailure()
        {
            // Arrange
            var apiResponse = new ApiResponse<InvoiceImportRowRes>
            {
                Success = false,
                Errors = [new ApiError { Message = "Not Found" }]
            };
            var mappedResponse = new ApiResponseDto<InvoiceImportRowDto>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Not Found" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<InvoiceImportRowRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<InvoiceImportRowDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetFailedInvoiceImportByIdAsync(99);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetFailedInvoiceImportByIdAsync_FailureWithNullMeta_UsesNewApiMetaDto()
        {
            // Arrange
            var apiResponse = new ApiResponse<InvoiceImportRowRes>
            {
                Success = false,
                Errors = [new ApiError { Message = "Error" }]
            };
            var mappedResponse = new ApiResponseDto<InvoiceImportRowDto>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Error" }],
                Meta = null!
            };

            _http.GetAsync<InvoiceImportRowRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<InvoiceImportRowDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetFailedInvoiceImportByIdAsync(1);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Meta);
        }

        [Fact]
        public async Task GetFailedInvoiceImportByIdAsync_UsesCorrectUrlWithId()
        {
            // Arrange
            var apiResponse = new ApiResponse<InvoiceImportRowRes> { Success = true, Data = new InvoiceImportRowRes { Id = 42 } };
            _http.GetAsync<InvoiceImportRowRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<InvoiceImportRowDto>>(apiResponse)
                .Returns(new ApiResponseDto<InvoiceImportRowDto> { Success = true });

            // Act
            await _client.GetFailedInvoiceImportByIdAsync(42);

            // Assert
            await _http.Received(1).GetAsync<InvoiceImportRowRes>(
                Arg.Is<string>(url => url.Contains("42")));
        }

        #endregion

        #region SaveFailedInvoiceImportAsync Tests

        [Fact]
        public async Task SaveFailedInvoiceImportAsync_SuccessResponse_ReturnsSuccess()
        {
            // Arrange
            var dto = new InvoiceImportRowDto { Id = 1, ProjectParent = "PP001" };
            var req = new InvoiceImportRowReq { Id = 1, ProjectParent = "PP001" };
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _mapper.Map<InvoiceImportRowReq>(dto).Returns(req);
            _http.PutAsync<InvoiceImportRowReq, bool?>(Arg.Any<string>(), req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.SaveFailedInvoiceImportAsync(1, dto);

            // Assert
            Assert.True(result.Success);
            Assert.True(result.Data);
        }

        [Fact]
        public async Task SaveFailedInvoiceImportAsync_FailureResponse_ReturnsFailure()
        {
            // Arrange
            var dto = new InvoiceImportRowDto { Id = 1 };
            var req = new InvoiceImportRowReq { Id = 1 };
            var apiResponse = new ApiResponse<bool?> { Success = false, Errors = [new ApiError { Message = "Validation error" }] };
            var mappedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Validation error" }],
                Meta = new ApiMetaDto()
            };

            _mapper.Map<InvoiceImportRowReq>(dto).Returns(req);
            _http.PutAsync<InvoiceImportRowReq, bool?>(Arg.Any<string>(), Arg.Any<InvoiceImportRowReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.SaveFailedInvoiceImportAsync(1, dto);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task SaveFailedInvoiceImportAsync_FailureWithNullMeta_UsesNewApiMetaDto()
        {
            // Arrange
            var dto = new InvoiceImportRowDto { Id = 1 };
            var req = new InvoiceImportRowReq { Id = 1 };
            var apiResponse = new ApiResponse<bool?> { Success = false, Errors = [new ApiError { Message = "Error" }] };
            var mappedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Error" }],
                Meta = null!
            };

            _mapper.Map<InvoiceImportRowReq>(dto).Returns(req);
            _http.PutAsync<InvoiceImportRowReq, bool?>(Arg.Any<string>(), Arg.Any<InvoiceImportRowReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.SaveFailedInvoiceImportAsync(1, dto);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Meta);
        }

        [Fact]
        public async Task SaveFailedInvoiceImportAsync_UsesCorrectUrlWithId()
        {
            // Arrange
            var dto = new InvoiceImportRowDto { Id = 7 };
            var req = new InvoiceImportRowReq { Id = 7 };
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };

            _mapper.Map<InvoiceImportRowReq>(dto).Returns(req);
            _http.PutAsync<InvoiceImportRowReq, bool?>(Arg.Any<string>(), Arg.Any<InvoiceImportRowReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            await _client.SaveFailedInvoiceImportAsync(7, dto);

            // Assert
            await _http.Received(1).PutAsync<InvoiceImportRowReq, bool?>(
                Arg.Is<string>(url => url.Contains('7')), Arg.Any<InvoiceImportRowReq>());
        }

        #endregion

        #region DeleteFailedInvoiceImportByIdAsync Tests

        [Fact]
        public async Task DeleteFailedInvoiceImportByIdAsync_SuccessResponse_ReturnsSuccess()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.DeleteFailedInvoiceImportByIdAsync(1);

            // Assert
            Assert.True(result.Success);
            Assert.True(result.Data);
        }

        [Fact]
        public async Task DeleteFailedInvoiceImportByIdAsync_FailureResponse_ReturnsFailure()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool?> { Success = false, Errors = [new ApiError { Message = "Not Found" }] };
            var mappedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Not Found" }],
                Meta = new ApiMetaDto()
            };

            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.DeleteFailedInvoiceImportByIdAsync(99);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task DeleteFailedInvoiceImportByIdAsync_FailureWithNullMeta_UsesNewApiMetaDto()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool?> { Success = false, Errors = [new ApiError { Message = "Error" }] };
            var mappedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Error" }],
                Meta = null!
            };

            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.DeleteFailedInvoiceImportByIdAsync(1);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Meta);
        }

        [Fact]
        public async Task DeleteFailedInvoiceImportByIdAsync_UsesCorrectUrlWithId()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            await _client.DeleteFailedInvoiceImportByIdAsync(42);

            // Assert
            await _http.Received(1).DeleteAsync<bool?>(Arg.Is<string>(url => url.Contains("42")));
        }

        #endregion

        #region DeleteFailedInvoiceImportByUserAsync Tests

        [Fact]
        public async Task DeleteFailedInvoiceImportByUserAsync_SuccessWithDataTrue_ReturnsSuccessWithoutErrors()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var mappedResponse = new ApiResponseDto<bool> { Success = true, Data = true };

            _http.DeleteAsync<bool?>(PactApiEndpoints.DeleteFailedInvoiceImportByUser).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.DeleteFailedInvoiceImportByUserAsync();

            // Assert
            Assert.True(result.Success);
            Assert.True(result.Data);
        }

        [Fact]
        public async Task DeleteFailedInvoiceImportByUserAsync_SuccessWithDataFalse_AddsNoRecordsMessage()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = false };
            var mappedResponse = new ApiResponseDto<bool> { Success = true, Data = false };

            _http.DeleteAsync<bool?>(PactApiEndpoints.DeleteFailedInvoiceImportByUser).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.DeleteFailedInvoiceImportByUserAsync();

            // Assert
            Assert.True(result.Success);
            Assert.False(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("No failed imported records found to delete.", result.Errors[0].Message);
        }

        [Fact]
        public async Task DeleteFailedInvoiceImportByUserAsync_FailureResponse_ReturnsFailure()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool?> { Success = false, Errors = [new ApiError { Message = "Server error" }] };
            var mappedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Server error" }],
                Meta = new ApiMetaDto()
            };

            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.DeleteFailedInvoiceImportByUserAsync();

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task DeleteFailedInvoiceImportByUserAsync_FailureWithNullMeta_UsesNewApiMetaDto()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool?> { Success = false, Errors = [new ApiError { Message = "Error" }] };
            var mappedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Error" }],
                Meta = null!
            };

            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.DeleteFailedInvoiceImportByUserAsync();

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Meta);
        }

        #endregion

        #region ImportInvoiceAsync Tests

        [Fact]
        public async Task ImportInvoiceAsync_SuccessResponse_ReturnsMappedResult()
        {
            // Arrange
            var requestDto = new InvoiceImportReqDto
            {
                FileName = "test.xlsx",
                Rows = new List<InvoiceImportRowDto> { new() { ProjectParent = "PP001" } }
            };
            var req = new InvoiceImportReq { FileName = "test.xlsx" };
            var resData = new InvoiceImportRes { PassedCount = 1, FailedCount = 0, Message = "OK" };
            var apiResponse = new ApiResponse<InvoiceImportRes> { Success = true, Data = resData };
            var mappedResult = new InvoiceImportResultDto { PassedCount = 1, FailedCount = 0, Message = "OK" };

            _mapper.Map<InvoiceImportReq>(requestDto).Returns(req);
            _http.PostAsync<InvoiceImportReq, InvoiceImportRes>(PactApiEndpoints.ImportInvoice, req).Returns(apiResponse);
            _mapper.Map<InvoiceImportResultDto>(resData).Returns(mappedResult);

            // Act
            var result = await _client.ImportInvoiceAsync(requestDto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(1, result.Data!.PassedCount);
            Assert.Equal(0, result.Data.FailedCount);
        }

        [Fact]
        public async Task ImportInvoiceAsync_FailureResponse_ReturnsFailure()
        {
            // Arrange
            var requestDto = new InvoiceImportReqDto { FileName = "test.xlsx", Rows = new List<InvoiceImportRowDto>() };
            var req = new InvoiceImportReq { FileName = "test.xlsx" };
            var apiResponse = new ApiResponse<InvoiceImportRes>
            {
                Success = false,
                Errors = [new ApiError { Message = "Import failed" }]
            };
            var mappedResponse = new ApiResponseDto<InvoiceImportResultDto>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Import failed" }],
                Meta = new ApiMetaDto()
            };

            _mapper.Map<InvoiceImportReq>(requestDto).Returns(req);
            _http.PostAsync<InvoiceImportReq, InvoiceImportRes>(Arg.Any<string>(), Arg.Any<InvoiceImportReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<InvoiceImportResultDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.ImportInvoiceAsync(requestDto);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task ImportInvoiceAsync_FailureWithNullMeta_UsesNewApiMetaDto()
        {
            // Arrange
            var requestDto = new InvoiceImportReqDto { FileName = "test.xlsx", Rows = new List<InvoiceImportRowDto>() };
            var req = new InvoiceImportReq { FileName = "test.xlsx" };
            var apiResponse = new ApiResponse<InvoiceImportRes>
            {
                Success = false,
                Errors = [new ApiError { Message = "Error" }]
            };
            var mappedResponse = new ApiResponseDto<InvoiceImportResultDto>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Error" }],
                Meta = null!
            };

            _mapper.Map<InvoiceImportReq>(requestDto).Returns(req);
            _http.PostAsync<InvoiceImportReq, InvoiceImportRes>(Arg.Any<string>(), Arg.Any<InvoiceImportReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<InvoiceImportResultDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.ImportInvoiceAsync(requestDto);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Meta);
        }

        [Fact]
        public async Task ImportInvoiceAsync_UsesCorrectEndpoint()
        {
            // Arrange
            var requestDto = new InvoiceImportReqDto { FileName = "test.xlsx", Rows = new List<InvoiceImportRowDto>() };
            var req = new InvoiceImportReq();
            var resData = new InvoiceImportRes { PassedCount = 0, FailedCount = 0, Message = "" };
            var apiResponse = new ApiResponse<InvoiceImportRes> { Success = true, Data = resData };

            _mapper.Map<InvoiceImportReq>(requestDto).Returns(req);
            _http.PostAsync<InvoiceImportReq, InvoiceImportRes>(Arg.Any<string>(), Arg.Any<InvoiceImportReq>()).Returns(apiResponse);
            _mapper.Map<InvoiceImportResultDto>(resData).Returns(new InvoiceImportResultDto());

            // Act
            await _client.ImportInvoiceAsync(requestDto);

            // Assert
            await _http.Received(1).PostAsync<InvoiceImportReq, InvoiceImportRes>(
                PactApiEndpoints.ImportInvoice, Arg.Any<InvoiceImportReq>());
        }

        #endregion
    }
}
