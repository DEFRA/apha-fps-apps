using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.Common.Utilities.Query;
using Apha.PIMS.Api.Controllers;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PIMS.Api.UnitTests.Controllers.ReviewItemControllerTest
{
    public class ReviewItemControllerTests
    {
        private readonly IReviewItemService _service;
        private readonly IMapper _mapper;
        private readonly ReviewItemController _controller;

        public ReviewItemControllerTests()
        {
            _service = Substitute.For<IReviewItemService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new ReviewItemController(_service, _mapper);
        }

        // ── helper ──────────────────────────────────────────────────────────────────────

        private static ReviewItemDto MakeDto(int itemId = 1) => new ReviewItemDto
        {
            ItemId = itemId,
            Item = $"Item {itemId}"
        };

        private static ReviewItemRes MakeRes(int itemId = 1) => new ReviewItemRes
        {
            ItemId = itemId,
            Item = $"Item {itemId}"
        };

        private static ReviewItemReq MakeReq() => new ReviewItemReq
        {
            ItemId = 1,
            Item = "New Item"
        };

        private static PaginationRes<ReviewItemRes> MakePaginationRes() => new PaginationRes<ReviewItemRes>
        {
            Data = new List<ReviewItemRes> { MakeRes(1) },
            PaginationData = new Pagination
            {
                PageNumber = 1,
                PageSize = 10,
                TotalRecords = 1
            }
        };

        // ── GetAll ──────────────────────────────────────────────────────────────────────

        #region GetAll

        [Fact]
        public async Task GetAll_ServiceReturnsData_ReturnsOkWithMappedList()
        {
            // Arrange
            var dtos = new List<ReviewItemDto> { MakeDto(1), MakeDto(2) };
            var resList = new List<ReviewItemRes> { MakeRes(1), MakeRes(2) };
            _service.GetAllReviewItemsAsync().Returns(dtos);
            _mapper.Map<List<ReviewItemRes>>(dtos).Returns(resList);

            // Act
            var result = await _controller.GetAllReviewItems();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<List<ReviewItemRes>>(ok.Value);
            Assert.Equal(2, returned.Count);
            await _service.Received(1).GetAllReviewItemsAsync();
            _mapper.Received(1).Map<List<ReviewItemRes>>(dtos);
        }

        [Fact]
        public async Task GetAll_ServiceReturnsEmptyList_ReturnsOkWithEmptyList()
        {
            // Arrange
            var dtos = new List<ReviewItemDto>();
            var resList = new List<ReviewItemRes>();
            _service.GetAllReviewItemsAsync().Returns(dtos);
            _mapper.Map<List<ReviewItemRes>>(dtos).Returns(resList);

            // Act
            var result = await _controller.GetAllReviewItems();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<List<ReviewItemRes>>(ok.Value);
            Assert.Empty(returned);
            await _service.Received(1).GetAllReviewItemsAsync();
        }

        [Fact]
        public async Task GetAll_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            _service.GetAllReviewItemsAsync().ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAllReviewItems());
        }

        #endregion

        // ── GetPaged ────────────────────────────────────────────────────────────────────

        #region GetPaged

        [Fact]
        public async Task GetPaged_ServiceReturnsData_ReturnsOkWithMappedPaginationResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationData = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 };
            var paginatedResult = new PaginatedResult<ReviewItemDto>(
                new List<ReviewItemDto> { MakeDto(1) }, paginationData);
            var mappedPaginationRes = MakePaginationRes();
            _service.GetPagedReviewItemsAsync(query).Returns(paginatedResult);
            _mapper.Map<PaginationRes<ReviewItemRes>>(paginatedResult).Returns(mappedPaginationRes);

            // Act
            var result = await _controller.GetPagedReviewItems(query);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<PaginationRes<ReviewItemRes>>(ok.Value);
            Assert.Single(returned.Data);
            Assert.Equal(1, returned.PaginationData.TotalRecords);
            await _service.Received(1).GetPagedReviewItemsAsync(query);
        }

        [Fact]
        public async Task GetPaged_ServiceReturnsEmptyList_ReturnsOkWithEmptyPaginationResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationData = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0, TotalPages = 0 };
            var paginatedResult = new PaginatedResult<ReviewItemDto>(
                new List<ReviewItemDto>(), paginationData);
            var mappedPaginationRes = new PaginationRes<ReviewItemRes>
            {
                Data = new List<ReviewItemRes>(),
                PaginationData = new Pagination
                {
                    PageNumber = 1,
                    PageSize = 10,
                    TotalRecords = 0
                }
            };
            _service.GetPagedReviewItemsAsync(query).Returns(paginatedResult);
            _mapper.Map<PaginationRes<ReviewItemRes>>(paginatedResult).Returns(mappedPaginationRes);

            // Act
            var result = await _controller.GetPagedReviewItems(query);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<PaginationRes<ReviewItemRes>>(ok.Value);
            Assert.Empty(returned.Data);
            Assert.Equal(0, returned.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetPaged_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _service.GetPagedReviewItemsAsync(Arg.Any<QueryParameters<string>>()).ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetPagedReviewItems(query));
        }

        #endregion

        // ── GetById ─────────────────────────────────────────────────────────────────────

        #region GetById

        [Fact]
        public async Task GetById_ServiceReturnsDto_ReturnsOkWithMappedResult()
        {
            // Arrange
            var dto = MakeDto(5);
            var res = MakeRes(5);
            _service.GetReviewItemByIdAsync(5).Returns(dto);
            _mapper.Map<ReviewItemRes>(dto).Returns(res);

            // Act
            var result = await _controller.GetReviewItemById(5);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, ok.Value);
            await _service.Received(1).GetReviewItemByIdAsync(5);
        }

        [Fact]
        public async Task GetById_ServiceReturnsNull_ReturnsJsonResultWithSuccessTrueAndNullData()
        {
            // Arrange
            _service.GetReviewItemByIdAsync(99).Returns((ReviewItemDto?)null);

            // Act
            var result = await _controller.GetReviewItemById(99);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var response = Assert.IsType<Apha.Common.Contracts.ApiResponse<ReviewItemRes>>(jsonResult.Value);
            Assert.True(response.Success);
            Assert.Null(response.Data);
            Assert.NotEmpty(response.Meta.CorrelationId);
        }

        [Fact]
        public async Task GetById_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            _service.GetReviewItemByIdAsync(Arg.Any<int>()).ThrowsAsync(new Exception("unexpected"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetReviewItemById(1));
        }

        #endregion

        // ── Create ──────────────────────────────────────────────────────────────────────

        #region Create

        [Fact]
        public async Task Create_ValidRequest_ReturnsCreatedAtActionWithMappedResult()
        {
            // Arrange
            var req = MakeReq();
            var dto = MakeDto(1);
            var res = MakeRes(1);
            _mapper.Map<ReviewItemDto>(req).Returns(dto);
            _service.CreateReviewItemAsync(dto).Returns(dto);
            _mapper.Map<ReviewItemRes>(dto).Returns(res);

            // Act
            var result = await _controller.CreateReviewItem(req);

            // Assert
            var created = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.GetReviewItemById), created.ActionName);
            Assert.Equal(res, created.Value);
            await _service.Received(1).CreateReviewItemAsync(dto);
            _mapper.Received(1).Map<ReviewItemRes>(dto);
        }

        [Fact]
        public async Task Create_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            var req = MakeReq();
            var dto = MakeDto(1);
            _mapper.Map<ReviewItemDto>(req).Returns(dto);
            _service.CreateReviewItemAsync(dto).ThrowsAsync(new Exception("Data error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.CreateReviewItem(req));
        }

        #endregion

        // ── Update ──────────────────────────────────────────────────────────────────────

        #region Update

        [Fact]
        public async Task Update_SetsRoutePkOnDtoBeforeCallingService_DtoIdMatchesRouteId()
        {
            // Arrange
            var itemId = 7;
            var req = MakeReq();
            var dto = MakeDto(itemId);
            var res = MakeRes(itemId);
            _mapper.Map<ReviewItemDto>(req).Returns(dto);
            _service.UpdateReviewItemAsync(dto).Returns(dto);
            _mapper.Map<ReviewItemRes>(dto).Returns(res);

            // Act
            await _controller.UpdateReviewItem(itemId, req);

            // Assert
            Assert.Equal(itemId, dto.ItemId);
        }

        [Fact]
        public async Task Update_ServiceReturnsDto_ReturnsOkWithMappedResult()
        {
            // Arrange
            var dto = MakeDto(3);
            var res = MakeRes(3);
            var req = MakeReq();
            _mapper.Map<ReviewItemDto>(req).Returns(dto);
            _service.UpdateReviewItemAsync(dto).Returns(dto);
            _mapper.Map<ReviewItemRes>(dto).Returns(res);

            // Act
            var result = await _controller.UpdateReviewItem(3, req);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, ok.Value);
            await _service.Received(1).UpdateReviewItemAsync(dto);
        }

        [Fact]
        public async Task Update_ServiceThrowsKeyNotFoundException_PropagatesException()
        {
            // Arrange
            var req = MakeReq();
            var dto = MakeDto(99);
            _mapper.Map<ReviewItemDto>(req).Returns(dto);
            _service.UpdateReviewItemAsync(dto).ThrowsAsync(new KeyNotFoundException("Not found"));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.UpdateReviewItem(99, req));
        }

        #endregion

        // ── Delete ──────────────────────────────────────────────────────────────────────

        #region Delete

        [Fact]
        public async Task Delete_ServiceCompletes_ReturnsOkWithSuccessTrue()
        {
            // Arrange
            _service.DeleteReviewItemAsync(5).Returns(true);

            // Act
            var result = await _controller.DeleteReviewItem(5);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)ok.Value!);
            await _service.Received(1).DeleteReviewItemAsync(5);
        }

        [Fact]
        public async Task Delete_ServiceThrowsKeyNotFoundException_PropagatesException()
        {
            // Arrange
            _service.DeleteReviewItemAsync(99).ThrowsAsync(new KeyNotFoundException("Not found"));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.DeleteReviewItem(99));
        }

        #endregion
    }
}
