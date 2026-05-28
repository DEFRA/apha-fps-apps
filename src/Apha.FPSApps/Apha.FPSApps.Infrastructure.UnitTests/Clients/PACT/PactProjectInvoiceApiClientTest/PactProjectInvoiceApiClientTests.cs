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

        #region GetPagedProjectInvoiceManualAsync Tests

        [Fact]
        public async Task GetPagedProjectInvoiceManualAsync_WithParentProject_IncludesProjectInUrl()
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
            var result = await _client.GetPagedProjectInvoiceManualAsync(query, parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<ProjectInvoiceRes>>(
                Arg.Is<string>(url => url.Contains("api/v1/projectinvoice") && url.Contains($"parentProject={Uri.EscapeDataString(parentProject)}")));
        }

        [Fact]
        public async Task GetPagedProjectInvoiceManualAsync_WithNullParentProject_UsesBaseUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<ProjectInvoiceRes>> { Success = true, Data = new List<ProjectInvoiceRes>() };
            var expectedDto = ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse(new List<ProjectInvoiceDto>(), new PaginationDto());

            _http.GetAsync<List<ProjectInvoiceRes>>(Arg.Is<string>(url => url.Contains("api/v1/projectinvoice"))).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectInvoiceDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedProjectInvoiceManualAsync(query, null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<ProjectInvoiceRes>>(Arg.Is<string>(url => url.Contains("api/v1/projectinvoice") && !url.Contains("parentProject=")));
        }

        [Fact]
        public async Task GetPagedProjectInvoiceManualAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
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
            var result = await _client.GetPagedProjectInvoiceManualAsync(query, null);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task GetPagedProjectInvoiceManualAsync_WithEmptyParentProject_DoesNotIncludeInUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<ProjectInvoiceRes>> { Success = true, Data = new List<ProjectInvoiceRes>() };
            var expectedDto = ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse(new List<ProjectInvoiceDto>(), new PaginationDto());

            _http.GetAsync<List<ProjectInvoiceRes>>(Arg.Is<string>(url => url.Contains("api/v1/projectinvoice") && !url.Contains("parentProject="))).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectInvoiceDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedProjectInvoiceManualAsync(query, "");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<ProjectInvoiceRes>>(Arg.Is<string>(url => !url.Contains("parentProject=")));
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

        [Fact]
        public async Task GetTotalAmountAsync_LargeAmounts_HandlesCorrectly()
        {
            // Arrange
            var parentProject = "PP001";
            var largeAmount = 999999999.99m;
            var apiResponse = new ApiResponse<decimal?> { Success = true, Data = largeAmount };
            var expectedDto = ApiResponseDto<decimal>.SuccessResponse(largeAmount);

            _http.GetAsync<decimal?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<decimal>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetTotalAmountAsync(parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(largeAmount, result.Data);
        }

        [Fact]
        public async Task GetTotalAmountAsync_EmptyParentProject_ReturnsZeroWithoutHttpCall()
        {
            // Act
            var result = await _client.GetTotalAmountAsync("");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(0m, result.Data);
            await _http.DidNotReceive().GetAsync<decimal?>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetTotalAmountAsync_WhitespaceParentProject_ReturnsZeroWithoutHttpCall()
        {
            // Act
            var result = await _client.GetTotalAmountAsync("   ");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(0m, result.Data);
            await _http.DidNotReceive().GetAsync<decimal?>(Arg.Any<string>());
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

            _http.GetAsync<ProjectInvoiceRes>($"api/v1/projectinvoice/{invoiceCounter}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectInvoiceDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetByIdAsync(invoiceCounter);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(invoiceCounter, result.Data?.InvoiceCounter);
            await _http.Received(1).GetAsync<ProjectInvoiceRes>($"api/v1/projectinvoice/{invoiceCounter}");
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

            _mapper.Map<ProjectInvoiceReq>(invoiceDto).Returns(invoiceReq);
            _http.PutAsync<ProjectInvoiceReq, ProjectInvoiceRes>($"api/v1/projectinvoice/{invoiceCounter}", invoiceReq).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectInvoiceDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.UpdateAsync(invoiceCounter, invoiceDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(900.00m, result.Data?.Amount);
            await _http.Received(1).PutAsync<ProjectInvoiceReq, ProjectInvoiceRes>($"api/v1/projectinvoice/{invoiceCounter}", invoiceReq);
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

            _http.DeleteAsync<bool?>($"api/v1/projectinvoice/{invoiceCounter}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.DeleteAsync(invoiceCounter);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _http.Received(1).DeleteAsync<bool?>($"api/v1/projectinvoice/{invoiceCounter}");
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

        #region GetPagedProjectInvoicesByMonthAsync Tests

        [Fact]
        public async Task GetPagedProjectInvoicesByMonthAsync_WithValidMonth_IncludesMonthInUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            const int month = 6;
            var invoiceList = new List<ProjectInvoiceRes>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PP001", Month = month, Amount = 100.00m },
                new() { InvoiceCounter = 2, ProjectParent = "PP002", Month = month, Amount = 200.00m }
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
                    new() { InvoiceCounter = 1, ProjectParent = "PP001", Month = month },
                    new() { InvoiceCounter = 2, ProjectParent = "PP002", Month = month }
                },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            );

            _http.GetAsync<List<ProjectInvoiceRes>>(Arg.Is<string>(url =>
                url.Contains("by-month") && url.Contains($"month={month}")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectInvoiceDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedProjectInvoicesByMonthAsync(query, month);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            Assert.All(result.Data!, invoice => Assert.Equal(month, invoice.Month));
            await _http.Received(1).GetAsync<List<ProjectInvoiceRes>>(
                Arg.Is<string>(url => url.Contains("by-month") && url.Contains($"month={month}")));
        }

        [Fact]
        public async Task GetPagedProjectInvoicesByMonthAsync_WithNullMonth_UsesBaseUrlWithoutMonthParameter()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<ProjectInvoiceRes>> 
            { 
                Success = true, 
                Data = new List<ProjectInvoiceRes>(),
                Pagination = new Pagination()
            };
            var expectedDto = ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse(
                new List<ProjectInvoiceDto>(), 
                new PaginationDto());

            _http.GetAsync<List<ProjectInvoiceRes>>(Arg.Is<string>(url => 
                url.Contains("by-month") && !url.Contains("month=")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectInvoiceDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedProjectInvoicesByMonthAsync(query, null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<ProjectInvoiceRes>>(
                Arg.Is<string>(url => url.Contains("by-month") && !url.Contains("month=")));
        }

        [Fact]
        public async Task GetPagedProjectInvoicesByMonthAsync_UsesCorrectEndpoint()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            const int month = 3;
            var apiResponse = new ApiResponse<List<ProjectInvoiceRes>> 
            { 
                Success = true, 
                Data = new List<ProjectInvoiceRes>() 
            };
            _http.GetAsync<List<ProjectInvoiceRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectInvoiceDto>>>(apiResponse)
                .Returns(ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse(new List<ProjectInvoiceDto>(), new PaginationDto()));

            // Act
            await _client.GetPagedProjectInvoicesByMonthAsync(query, month);

            // Assert
            await _http.Received(1).GetAsync<List<ProjectInvoiceRes>>(
                Arg.Is<string>(url => url.StartsWith(PactApiEndpoints.GetPagedProjectInvoicesByMonth)));
        }

        [Fact]
        public async Task GetPagedProjectInvoicesByMonthAsync_WithQueryParameters_AppendsCorrectly()
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
            var apiResponse = new ApiResponse<List<ProjectInvoiceRes>> 
            { 
                Success = true, 
                Data = new List<ProjectInvoiceRes>() 
            };
            var expectedDto = ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse(
                new List<ProjectInvoiceDto>(), 
                new PaginationDto());

            _http.GetAsync<List<ProjectInvoiceRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectInvoiceDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedProjectInvoicesByMonthAsync(query, month);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<ProjectInvoiceRes>>(
                Arg.Is<string>(url => 
                    url.Contains("month=7") && 
                    url.Contains("Page=2") && 
                    url.Contains("PageSize=25")));
        }

        [Fact]
        public async Task GetPagedProjectInvoicesByMonthAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
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
            var result = await _client.GetPagedProjectInvoicesByMonthAsync(query, 6);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(6)]
        [InlineData(12)]
        public async Task GetPagedProjectInvoicesByMonthAsync_WithBoundaryMonths_Works(int month)
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<ProjectInvoiceRes>> 
            { 
                Success = true, 
                Data = new List<ProjectInvoiceRes>() 
            };
            var expectedDto = ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse(
                new List<ProjectInvoiceDto>(), 
                new PaginationDto());

            _http.GetAsync<List<ProjectInvoiceRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectInvoiceDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedProjectInvoicesByMonthAsync(query, month);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<ProjectInvoiceRes>>(
                Arg.Is<string>(url => url.Contains($"month={month}")));
        }

        [Fact]
        public async Task GetPagedProjectInvoicesByMonthAsync_WithEmptyResult_ReturnsEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<ProjectInvoiceRes>> 
            { 
                Success = true, 
                Data = new List<ProjectInvoiceRes>(),
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 0 }
            };
            var expectedDto = ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse(
                new List<ProjectInvoiceDto>(), 
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 });

            _http.GetAsync<List<ProjectInvoiceRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectInvoiceDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedProjectInvoicesByMonthAsync(query, 5);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
            Assert.Equal(0, result.Pagination?.TotalRecords);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesByMonthAsync_WithLargePageSize_ProcessesCorrectly()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 1000 };
            var invoiceList = Enumerable.Range(1, 100)
                .Select(i => new ProjectInvoiceRes 
                { 
                    InvoiceCounter = i, 
                    ProjectParent = $"PP{i:000}", 
                    Month = 8,
                    Amount = i * 100m 
                })
                .ToList();
            var apiResponse = new ApiResponse<List<ProjectInvoiceRes>> 
            { 
                Success = true, 
                Data = invoiceList,
                Pagination = new Pagination { PageNumber = 1, PageSize = 1000, TotalRecords = 100 }
            };
            var mappedDtos = invoiceList.Select(r => new ProjectInvoiceDto 
            { 
                InvoiceCounter = r.InvoiceCounter, 
                ProjectParent = r.ProjectParent,
                Month = r.Month 
            }).ToList();
            var expectedDto = ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse(
                mappedDtos,
                new PaginationDto { PageNumber = 1, PageSize = 1000, TotalRecords = 100 });

            _http.GetAsync<List<ProjectInvoiceRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectInvoiceDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedProjectInvoicesByMonthAsync(query, 8);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(100, result.Data?.Count);
        }

        #endregion

        #region CopyInvoicesAsync Tests

        [Fact]
        public async Task CopyInvoicesAsync_WithNullInvoiceIds_ReturnsSuccessTrue()
        {
            // Arrange
            var copyDto = new CopyInvoicesDto
            {
                SourceMonth = 5,
                TargetMonth = 6,
                InvoiceIds = null
            };
            var copyRes = new CopyInvoicesRes { Success = true };
            var apiResponse = new ApiResponse<CopyInvoicesRes> { Success = true, Data = copyRes };

            _http.PostAsync<CopyInvoicesReq, CopyInvoicesRes>(
                Arg.Is<string>(url => url == PactApiEndpoints.CopyProjectInvoices),
                Arg.Is<CopyInvoicesReq>(req =>
                    req.InvoiceIds == null &&
                    req.SourceMonth == 5 &&
                    req.TargetMonth == 6))
                .Returns(apiResponse);

            // Act
            var result = await _client.CopyInvoicesAsync(copyDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
        }

        [Fact]
        public async Task CopyInvoicesAsync_WithInvoiceIds_SendsIdsCorrectly()
        {
            // Arrange
            var copyDto = new CopyInvoicesDto
            {
                SourceMonth = 3,
                TargetMonth = 4,
                InvoiceIds = [1, 2, 3]
            };
            var copyRes = new CopyInvoicesRes { Success = true };
            var apiResponse = new ApiResponse<CopyInvoicesRes> { Success = true, Data = copyRes };

            _http.PostAsync<CopyInvoicesReq, CopyInvoicesRes>(Arg.Any<string>(), Arg.Any<CopyInvoicesReq>())
                .Returns(apiResponse);

            // Act
            var result = await _client.CopyInvoicesAsync(copyDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _http.Received(1).PostAsync<CopyInvoicesReq, CopyInvoicesRes>(
                Arg.Any<string>(),
                Arg.Is<CopyInvoicesReq>(req =>
                    req.InvoiceIds != null &&
                    req.InvoiceIds.Count == 3));
        }

        [Fact]
        public async Task CopyInvoicesAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var copyDto = new CopyInvoicesDto { SourceMonth = 5, TargetMonth = 6, InvoiceIds = [1, 2] };
            var errors = new List<ApiError> { new() { Message = "Copy failed", Code = "COPY_ERROR" } };
            var apiResponse = new ApiResponse<CopyInvoicesRes> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Copy failed", Code = "COPY_ERROR" }],
                Meta = new ApiMetaDto()
            };

            _http.PostAsync<CopyInvoicesReq, CopyInvoicesRes>(Arg.Any<string>(), Arg.Any<CopyInvoicesReq>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.CopyInvoicesAsync(copyDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task CopyInvoicesAsync_WhenResponseDataIsNull_ReturnsFalse()
        {
            // Arrange
            var copyDto = new CopyInvoicesDto { SourceMonth = 5, TargetMonth = 6 };
            var apiResponse = new ApiResponse<CopyInvoicesRes> { Success = true, Data = null };

            _http.PostAsync<CopyInvoicesReq, CopyInvoicesRes>(Arg.Any<string>(), Arg.Any<CopyInvoicesReq>())
                .Returns(apiResponse);

            // Act
            var result = await _client.CopyInvoicesAsync(copyDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data);
        }

        [Fact]
        public async Task CopyInvoicesAsync_UsesCorrectEndpoint()
        {
            // Arrange
            var copyDto = new CopyInvoicesDto { SourceMonth = 7, TargetMonth = 8 };
            var apiResponse = new ApiResponse<CopyInvoicesRes> { Success = true, Data = new CopyInvoicesRes { Success = true } };
            _http.PostAsync<CopyInvoicesReq, CopyInvoicesRes>(Arg.Any<string>(), Arg.Any<CopyInvoicesReq>())
                .Returns(apiResponse);

            // Act
            await _client.CopyInvoicesAsync(copyDto);

            // Assert
            await _http.Received(1).PostAsync<CopyInvoicesReq, CopyInvoicesRes>(
                Arg.Is<string>(url => url == PactApiEndpoints.CopyProjectInvoices),
                Arg.Any<CopyInvoicesReq>());
        }

        [Theory]
        [InlineData(1, 2)]
        [InlineData(5, 6)]
        [InlineData(11, 12)]
        public async Task CopyInvoicesAsync_WithDifferentMonths_SendsCorrectMonths(int source, int target)
        {
            // Arrange
            var copyDto = new CopyInvoicesDto { SourceMonth = source, TargetMonth = target };
            var apiResponse = new ApiResponse<CopyInvoicesRes> { Success = true, Data = new CopyInvoicesRes { Success = true } };
            _http.PostAsync<CopyInvoicesReq, CopyInvoicesRes>(Arg.Any<string>(), Arg.Any<CopyInvoicesReq>())
                .Returns(apiResponse);

            // Act
            await _client.CopyInvoicesAsync(copyDto);

            // Assert
            await _http.Received(1).PostAsync<CopyInvoicesReq, CopyInvoicesRes>(
                Arg.Any<string>(),
                Arg.Is<CopyInvoicesReq>(req =>
                    req.SourceMonth == source &&
                    req.TargetMonth == target));
        }

        [Fact]
        public async Task CopyInvoicesAsync_WhenApiResponseSuccessFalse_MapsToBoolFalse()
        {
            // Arrange
            var copyDto = new CopyInvoicesDto { SourceMonth = 5, TargetMonth = 6 };
            var copyRes = new CopyInvoicesRes { Success = false };
            var apiResponse = new ApiResponse<CopyInvoicesRes> { Success = true, Data = copyRes };

            _http.PostAsync<CopyInvoicesReq, CopyInvoicesRes>(Arg.Any<string>(), Arg.Any<CopyInvoicesReq>())
                .Returns(apiResponse);

            // Act
            var result = await _client.CopyInvoicesAsync(copyDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data);
        }

        [Fact]
        public async Task CopyInvoicesAsync_WithEmptyInvoiceIds_SendsEmptyList()
        {
            // Arrange
            var copyDto = new CopyInvoicesDto
            {
                SourceMonth = 3,
                TargetMonth = 4,
                InvoiceIds = []
            };
            var apiResponse = new ApiResponse<CopyInvoicesRes> { Success = true, Data = new CopyInvoicesRes { Success = true } };
            _http.PostAsync<CopyInvoicesReq, CopyInvoicesRes>(Arg.Any<string>(), Arg.Any<CopyInvoicesReq>())
                .Returns(apiResponse);

            // Act
            await _client.CopyInvoicesAsync(copyDto);

            // Assert
            await _http.Received(1).PostAsync<CopyInvoicesReq, CopyInvoicesRes>(
                Arg.Any<string>(),
                Arg.Is<CopyInvoicesReq>(req =>
                    req.InvoiceIds != null &&
                    req.InvoiceIds.Count == 0));
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

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_WithComplexData_ReturnsMappedData()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 50 };
            var pivotRes = new MonthlyInvoicesPivotRes
            {
                Months = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12],
                Rows = [],
                Pagination = new Pagination { PageNumber = 1, PageSize = 50, TotalRecords = 100 }
            };
            var apiResponse = new ApiResponse<MonthlyInvoicesPivotRes> { Success = true, Data = pivotRes };
            var expectedDto = new ApiResponseDto<MonthlyInvoicesPivotDto>
            {
                Success = true,
                Data = new MonthlyInvoicesPivotDto
                {
                    Months = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12]
                },
                Pagination = new PaginationDto { PageNumber = 1, PageSize = 50, TotalRecords = 100 }
            };

            _http.GetAsync<MonthlyInvoicesPivotRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MonthlyInvoicesPivotDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetMonthlyInvoicesSummaryAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(12, result.Data?.Months.Count);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_WithNullData_HandlesGracefully()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var apiResponse = new ApiResponse<MonthlyInvoicesPivotRes> { Success = true, Data = null };
            var expectedDto = new ApiResponseDto<MonthlyInvoicesPivotDto>
            {
                Success = true,
                Data = null
            };

            _http.GetAsync<MonthlyInvoicesPivotRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MonthlyInvoicesPivotDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetMonthlyInvoicesSummaryAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_WithEmptyMonthsList_ReturnsEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var pivotRes = new MonthlyInvoicesPivotRes
            {
                Months = [],
                Rows = [],
                Pagination = new Pagination()
            };
            var apiResponse = new ApiResponse<MonthlyInvoicesPivotRes> { Success = true, Data = pivotRes };
            var expectedDto = new ApiResponseDto<MonthlyInvoicesPivotDto>
            {
                Success = true,
                Data = new MonthlyInvoicesPivotDto { Months = [] }
            };

            _http.GetAsync<MonthlyInvoicesPivotRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MonthlyInvoicesPivotDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetMonthlyInvoicesSummaryAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!.Months);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_WithQueryParameters_AppendsCorrectly()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 2,
                PageSize = 25,
                SortBy = "ProjectParent",
                Descending = true
            };
            var apiResponse = new ApiResponse<MonthlyInvoicesPivotRes>
            {
                Success = true,
                Data = new MonthlyInvoicesPivotRes()
            };
            _http.GetAsync<MonthlyInvoicesPivotRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MonthlyInvoicesPivotDto>>(apiResponse)
                .Returns(new ApiResponseDto<MonthlyInvoicesPivotDto> { Success = true });

            // Act
            await _client.GetMonthlyInvoicesSummaryAsync(query);

            // Assert
            await _http.Received(1).GetAsync<MonthlyInvoicesPivotRes>(
                Arg.Is<string>(url =>
                    url.Contains("Page=2") &&
                    url.Contains("PageSize=25")));
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_WithMultipleErrors_ReturnsAllErrors()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var errors = new List<ApiError>
            {
                new() { Message = "Error 1", Code = "ERR_001" },
                new() { Message = "Error 2", Code = "ERR_002" }
            };
            var apiResponse = new ApiResponse<MonthlyInvoicesPivotRes>
            {
                Success = false,
                Errors = errors
            };
            var mappedResponse = new ApiResponseDto<MonthlyInvoicesPivotDto>
            {
                Success = false,
                Errors = errors.Select(e => new ApiErrorDto { Message = e.Message, Code = e.Code }).ToList(),
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<MonthlyInvoicesPivotRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MonthlyInvoicesPivotDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetMonthlyInvoicesSummaryAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(2, result.Errors?.Count);
        }

        #endregion
    }
}
