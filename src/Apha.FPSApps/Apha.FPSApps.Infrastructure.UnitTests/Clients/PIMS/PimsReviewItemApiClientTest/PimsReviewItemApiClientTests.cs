using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients;
using AutoMapper;
using NSubstitute;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PIMS.PimsReviewItemApiClientTest
{
    public class PimsReviewItemApiClientTests
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PimsReviewItemApiClient _client;

        public PimsReviewItemApiClientTests()
        {
            _http = Substitute.For<IPimsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PimsReviewItemApiClient(_http, _mapper);
        }

        #region GetAllReviewItemsAsync Tests

        [Fact]
        public async Task GetAllReviewItemsAsync_WhenSuccessResponse_ReturnsMappedList()
        {
            // Arrange
            var reviewItemList = new List<ReviewItemRes>
            {
                new ReviewItemRes { ItemId = 1, Item = "Item 1" },
                new ReviewItemRes { ItemId = 2, Item = "Item 2" }
            };
            var apiResponse = new ApiResponse<List<ReviewItemRes>> { Success = true, Data = reviewItemList };
            var mappedDto = ApiResponseDto<List<ReviewItemDto>>.SuccessResponse(new List<ReviewItemDto>
            {
                new ReviewItemDto { Itemid = 1, Item = "Item 1" },
                new ReviewItemDto { Itemid = 2, Item = "Item 2" }
            });

            _http.GetAsync<List<ReviewItemRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ReviewItemDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetAllReviewItemsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            await _http.Received(1).GetAsync<List<ReviewItemRes>>(Arg.Any<string>());
            _mapper.Received(1).Map<ApiResponseDto<List<ReviewItemDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAllReviewItemsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new ApiError { Message = "Error", Code = "ERR" } };
            var apiResponse = new ApiResponse<List<ReviewItemRes>> { Success = false, Data = null, Errors = errors };
            var failDto = new ApiResponseDto<List<ReviewItemDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Error", Code = "ERR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ReviewItemRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ReviewItemDto>>>(apiResponse).Returns(failDto);

            // Act
            var result = await _client.GetAllReviewItemsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            await _http.Received(1).GetAsync<List<ReviewItemRes>>(Arg.Any<string>());
        }

        #endregion

        #region GetPagedReviewItemsAsync Tests

        [Fact]
        public async Task GetPagedReviewItemsAsync_WhenSuccessResponse_ReturnsPaginatedList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, Search = "test" };
            var reviewItemList = new List<ReviewItemRes>
            {
                new ReviewItemRes { ItemId = 1, Item = "Item 1" }
            };
            var paginationMeta = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1 };
            var apiResponse = new ApiResponse<List<ReviewItemRes>>
            {
                Success = true,
                Data = reviewItemList,
                Pagination = paginationMeta
            };
            var mappedDtos = new List<ReviewItemDto>
            {
                new ReviewItemDto { Itemid = 1, Item = "Item 1" }
            };
            var paginatedResult = new PaginatedResult<ReviewItemDto>(mappedDtos, 1, 1, 10);
            var successResponse = ApiResponseDto<PaginatedResult<ReviewItemDto>>.SuccessResponse(paginatedResult);

            _http.GetAsync<List<ReviewItemRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<List<ReviewItemDto>>(reviewItemList).Returns(mappedDtos);

            // Act
            var result = await _client.GetPagedReviewItemsAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data.data);
            Assert.Equal(1, result.Data.TotalCount);
            await _http.Received(1).GetAsync<List<ReviewItemRes>>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetPagedReviewItemsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiError> { new ApiError { Message = "Error", Code = "ERR" } };
            var apiResponse = new ApiResponse<List<ReviewItemRes>>
            {
                Success = false,
                Data = null,
                Errors = errors,
                Meta = new ApiMeta { CorrelationId = "123" }
            };
            var failDto = new ApiResponseDto<PaginatedResult<ReviewItemDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Error", Code = "ERR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ReviewItemRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<List<ApiErrorDto>>(Arg.Any<List<ApiError>>()).Returns(new List<ApiErrorDto>());
            _mapper.Map<ApiMetaDto>(Arg.Any<ApiMeta>()).Returns(new ApiMetaDto());

            // Act
            var result = await _client.GetPagedReviewItemsAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            await _http.Received(1).GetAsync<List<ReviewItemRes>>(Arg.Any<string>());
        }

        #endregion

        #region GetReviewItemByIdAsync Tests

        [Fact]
        public async Task GetReviewItemByIdAsync_WhenSuccessResponse_ReturnsMappedObject()
        {
            // Arrange
            var itemId = 1;
            var reviewItem = new ReviewItemRes { ItemId = 1, Item = "Item 1" };
            var apiResponse = new ApiResponse<ReviewItemRes> { Success = true, Data = reviewItem };
            var mappedDto = ApiResponseDto<ReviewItemDto>.SuccessResponse(new ReviewItemDto { Itemid = 1, Item = "Item 1" });

            _http.GetAsync<ReviewItemRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ReviewItemDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetReviewItemByIdAsync(itemId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.Itemid);
            await _http.Received(1).GetAsync<ReviewItemRes>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetReviewItemByIdAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var itemId = 1;
            var errors = new List<ApiError> { new ApiError { Message = "Error", Code = "ERR" } };
            var apiResponse = new ApiResponse<ReviewItemRes> { Success = false, Data = null, Errors = errors };
            var failDto = new ApiResponseDto<ReviewItemDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Error", Code = "ERR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<ReviewItemRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ReviewItemDto>>(apiResponse).Returns(failDto);

            // Act
            var result = await _client.GetReviewItemByIdAsync(itemId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            await _http.Received(1).GetAsync<ReviewItemRes>(Arg.Any<string>());
        }

        #endregion

        #region CreateReviewItemAsync Tests

        [Fact]
        public async Task CreateReviewItemAsync_WhenSuccessResponse_ReturnsMappedObject()
        {
            // Arrange
            var dto = new ReviewItemDto { Itemid = 1, Item = "Item 1" };
            var request = new ReviewItemReq { ItemId = 1, Item = "Item 1" };
            var reviewItem = new ReviewItemRes { ItemId = 1, Item = "Item 1" };
            var apiResponse = new ApiResponse<ReviewItemRes> { Success = true, Data = reviewItem };
            var mappedDto = ApiResponseDto<ReviewItemDto>.SuccessResponse(new ReviewItemDto { Itemid = 1, Item = "Item 1" });

            _mapper.Map<ReviewItemReq>(dto).Returns(request);
            _http.PostAsync<ReviewItemReq, ReviewItemRes>(Arg.Any<string>(), Arg.Any<ReviewItemReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ReviewItemDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.CreateReviewItemAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            await _http.Received(1).PostAsync<ReviewItemReq, ReviewItemRes>(Arg.Any<string>(), Arg.Any<ReviewItemReq>());
        }

        [Fact]
        public async Task CreateReviewItemAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new ReviewItemDto { Itemid = 1, Item = "Item 1" };
            var request = new ReviewItemReq { ItemId = 1, Item = "Item 1" };
            var errors = new List<ApiError> { new ApiError { Message = "Error", Code = "ERR" } };
            var apiResponse = new ApiResponse<ReviewItemRes> { Success = false, Data = null, Errors = errors };
            var failDto = new ApiResponseDto<ReviewItemDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Error", Code = "ERR" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<ReviewItemReq>(dto).Returns(request);
            _http.PostAsync<ReviewItemReq, ReviewItemRes>(Arg.Any<string>(), Arg.Any<ReviewItemReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ReviewItemDto>>(apiResponse).Returns(failDto);

            // Act
            var result = await _client.CreateReviewItemAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            await _http.Received(1).PostAsync<ReviewItemReq, ReviewItemRes>(Arg.Any<string>(), Arg.Any<ReviewItemReq>());
        }

        #endregion

        #region UpdateReviewItemAsync Tests

        [Fact]
        public async Task UpdateReviewItemAsync_WhenSuccessResponse_ReturnsMappedObject()
        {
            // Arrange
            var itemId = 1;
            var dto = new ReviewItemDto { Itemid = 1, Item = "Updated Item 1" };
            var request = new ReviewItemReq { ItemId = 1, Item = "Updated Item 1" };
            var reviewItem = new ReviewItemRes { ItemId = 1, Item = "Updated Item 1" };
            var apiResponse = new ApiResponse<ReviewItemRes> { Success = true, Data = reviewItem };
            var mappedDto = ApiResponseDto<ReviewItemDto>.SuccessResponse(new ReviewItemDto { Itemid = 1, Item = "Updated Item 1" });

            _mapper.Map<ReviewItemReq>(dto).Returns(request);
            _http.PutAsync<ReviewItemReq, ReviewItemRes>(Arg.Any<string>(), Arg.Any<ReviewItemReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ReviewItemDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.UpdateReviewItemAsync(itemId, dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            await _http.Received(1).PutAsync<ReviewItemReq, ReviewItemRes>(Arg.Any<string>(), Arg.Any<ReviewItemReq>());
        }

        [Fact]
        public async Task UpdateReviewItemAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var itemId = 1;
            var dto = new ReviewItemDto { Itemid = 1, Item = "Updated Item 1" };
            var request = new ReviewItemReq { ItemId = 1, Item = "Updated Item 1" };
            var errors = new List<ApiError> { new ApiError { Message = "Error", Code = "ERR" } };
            var apiResponse = new ApiResponse<ReviewItemRes> { Success = false, Data = null, Errors = errors };
            var failDto = new ApiResponseDto<ReviewItemDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Error", Code = "ERR" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<ReviewItemReq>(dto).Returns(request);
            _http.PutAsync<ReviewItemReq, ReviewItemRes>(Arg.Any<string>(), Arg.Any<ReviewItemReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ReviewItemDto>>(apiResponse).Returns(failDto);

            // Act
            var result = await _client.UpdateReviewItemAsync(itemId, dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            await _http.Received(1).PutAsync<ReviewItemReq, ReviewItemRes>(Arg.Any<string>(), Arg.Any<ReviewItemReq>());
        }

        #endregion

        #region DeleteReviewItemAsync Tests

        [Fact]
        public async Task DeleteReviewItemAsync_WhenSuccessResponse_ReturnsTrue()
        {
            // Arrange
            var itemId = 1;
            var apiResponse = new ApiResponse<bool> { Success = true, Data = true };
            var mappedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.DeleteReviewItemAsync(itemId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).DeleteAsync<bool>(Arg.Any<string>());
        }

        [Fact]
        public async Task DeleteReviewItemAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var itemId = 1;
            var errors = new List<ApiError> { new ApiError { Message = "Error", Code = "ERR" } };
            var apiResponse = new ApiResponse<bool> { Success = false, Data = false, Errors = errors };
            var failDto = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Error", Code = "ERR" } },
                Meta = new ApiMetaDto()
            };

            _http.DeleteAsync<bool>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(failDto);

            // Act
            var result = await _client.DeleteReviewItemAsync(itemId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            await _http.Received(1).DeleteAsync<bool>(Arg.Any<string>());
        }

        #endregion
    }
}
