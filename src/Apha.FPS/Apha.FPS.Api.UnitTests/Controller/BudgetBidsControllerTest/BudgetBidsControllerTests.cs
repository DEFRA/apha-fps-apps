using Apha.Common.Contracts.FPS;
using Apha.FPS.Api.Controllers;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPS.Api.UnitTests.Controller.BudgetBidsControllerTest
{
    public class BudgetBidsControllerTests
    {
        private readonly IBudgetBidsService _service;
        private readonly IMapper _mapper;
        private readonly BudgetBidsController _controller;

        public BudgetBidsControllerTests()
        {
            _service    = Substitute.For<IBudgetBidsService>();
            _mapper     = Substitute.For<IMapper>();
            _controller = new BudgetBidsController(_service, _mapper);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new BudgetBidsController(null!, _mapper));
        }

        [Fact]
        public void Constructor_WithNullMapper_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new BudgetBidsController(_service, null!));
        }

        #endregion

        #region GetBidViewAsync Tests

        [Fact]
        public async Task GetBidViewAsync_WithData_ReturnsOkWithMappedResults()
        {
            // Arrange
            var dtos = new List<BidViewDto> { new() { WorkgroupName = "WG01", Account = "ACC1", GenBid = 100m } };
            var res  = new List<BidViewRes> { new() { WorkgroupName = "WG01", Account = "ACC1", GenBid = 100m } };
            _service.GetBidViewAsync("WG01").Returns(dtos);
            _mapper.Map<List<BidViewRes>>(dtos).Returns(res);

            // Act
            var result = await _controller.GetBidViewAsync("WG01");

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<List<BidViewRes>>(ok.Value);
            Assert.Single(data);
        }

        [Fact]
        public async Task GetBidViewAsync_WithEmptyData_ReturnsOkWithEmptyList()
        {
            // Arrange
            _service.GetBidViewAsync("WG01").Returns(new List<BidViewDto>());
            _mapper.Map<List<BidViewRes>>(Arg.Any<List<BidViewDto>>()).Returns(new List<BidViewRes>());

            // Act
            var result = await _controller.GetBidViewAsync("WG01");

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<List<BidViewRes>>(ok.Value);
        }

        #endregion

        #region GetBidByIdAsync Tests

        [Fact]
        public async Task GetBidByIdAsync_WithExistingBid_ReturnsOk()
        {
            // Arrange
            var dto = new BidDto { WorkgroupName = "WG01", Account = "ACC1", GenBid = 100m };
            var res = new BidRes { WorkgroupName = "WG01", Account = "ACC1", GenBid = 100m };
            _service.GetBidByIdAsync("WG01", "ACC1").Returns(dto);
            _mapper.Map<BidRes>(dto).Returns(res);

            // Act
            var result = await _controller.GetBidByIdAsync("WG01", "ACC1");

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<BidRes>(ok.Value);
        }

        [Fact]
        public async Task GetBidByIdAsync_WithNonExistingBid_ThrowsKeyNotFoundException()
        {
            // Arrange
            _service.GetBidByIdAsync("WG01", "NOTEXIST").Returns((BidDto?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _controller.GetBidByIdAsync("WG01", "NOTEXIST"));
        }

        #endregion

        #region AddBidAsync Tests

        [Fact]
        public async Task AddBidAsync_WithValidRequest_ReturnsOk()
        {
            // Arrange
            var req = new BidReq { WorkgroupName = "WG01", Account = "ACC1", GenBid = 100m };
            var dto = new BidDto { WorkgroupName = "WG01", Account = "ACC1", GenBid = 100m };
            var resultDto = new BidDto { WorkgroupName = "WG01", Account = "ACC1", GenBid = 100m, FpsYear = 2024 };
            var res       = new BidRes { WorkgroupName = "WG01", Account = "ACC1", GenBid = 100m, FpsYear = 2024 };

            _mapper.Map<BidDto>(req).Returns(dto);
            _service.AddBidAsync(dto).Returns(resultDto);
            _mapper.Map<BidRes>(resultDto).Returns(res);

            // Act
            var result = await _controller.AddBidAsync(req);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<BidRes>(ok.Value);
        }

        [Fact]
        public async Task AddBidAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            var req = new BidReq { WorkgroupName = "WG01", Account = "ACC1", GenBid = 100m };
            var dto = new BidDto { WorkgroupName = "WG01", Account = "ACC1", GenBid = 100m };
            _mapper.Map<BidDto>(req).Returns(dto);
            _service.AddBidAsync(dto).ThrowsAsync(new InvalidOperationException("Account already exists."));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.AddBidAsync(req));
        }

        [Fact]
        public async Task AddBidAsync_WhenUnauthorized_PropagatesException()
        {
            // Arrange
            var req = new BidReq { WorkgroupName = "WG01", Account = "ACC1", GenBid = 100m };
            var dto = new BidDto { WorkgroupName = "WG01", Account = "ACC1", GenBid = 100m };
            _mapper.Map<BidDto>(req).Returns(dto);
            _service.AddBidAsync(dto).ThrowsAsync(new UnauthorizedAccessException("Not authorized."));

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.AddBidAsync(req));
        }

        #endregion

        #region UpdateBidAsync Tests

        [Fact]
        public async Task UpdateBidAsync_WithValidRequest_ReturnsOk()
        {
            // Arrange
            var req = new BidReq { WorkgroupName = "WG01", Account = "ACC1", GenBid = 200m };
            var dto = new BidDto { WorkgroupName = "WG01", Account = "ACC1", GenBid = 200m };
            var resultDto = new BidDto { WorkgroupName = "WG01", Account = "ACC1", GenBid = 200m, FpsYear = 2024 };
            var res       = new BidRes { WorkgroupName = "WG01", Account = "ACC1", GenBid = 200m, FpsYear = 2024 };

            _mapper.Map<BidDto>(req).Returns(dto);
            _service.UpdateBidAsync(dto).Returns(resultDto);
            _mapper.Map<BidRes>(resultDto).Returns(res);

            // Act
            var result = await _controller.UpdateBidAsync(req);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<BidRes>(ok.Value);
        }

        [Fact]
        public async Task UpdateBidAsync_WhenBidNotFound_PropagatesException()
        {
            // Arrange
            var req = new BidReq { WorkgroupName = "WG01", Account = "ACC1", GenBid = 200m };
            var dto = new BidDto { WorkgroupName = "WG01", Account = "ACC1", GenBid = 200m };
            _mapper.Map<BidDto>(req).Returns(dto);
            _service.UpdateBidAsync(dto).ThrowsAsync(new InvalidOperationException("Bid not found."));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.UpdateBidAsync(req));
        }

        #endregion

        #region DeleteBidAsync Tests

        [Fact]
        public async Task DeleteBidAsync_WithExistingBid_ReturnsOk()
        {
            // Arrange
            _service.DeleteBidAsync("WG01", "ACC1").Returns(true);

            // Act
            var result = await _controller.DeleteBidAsync("WG01", "ACC1");

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(true, ok.Value);
        }

        [Fact]
        public async Task DeleteBidAsync_WhenBidNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _service.DeleteBidAsync("WG01", "NOTEXIST").Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _controller.DeleteBidAsync("WG01", "NOTEXIST"));
        }

        [Fact]
        public async Task DeleteBidAsync_WhenUnauthorized_PropagatesException()
        {
            // Arrange
            _service.DeleteBidAsync("WG01", "ACC1").ThrowsAsync(new UnauthorizedAccessException("Not authorized."));

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.DeleteBidAsync("WG01", "ACC1"));
        }

        #endregion

        #region GetAccountCategoriesAsync Tests

        [Fact]
        public async Task GetAccountCategoriesAsync_WithData_ReturnsOkWithMappedResults()
        {
            // Arrange
            var dtos = new List<AccountCategoryDto> { new() { AccShortName = "ACC1" } };
            var res  = new List<AccountCategoryRes> { new() { AccShortName = "ACC1" } };
            _service.GetAccountCategoriesAsync().Returns(dtos);
            _mapper.Map<List<AccountCategoryRes>>(dtos).Returns(res);

            // Act
            var result = await _controller.GetAccountCategoriesAsync();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<List<AccountCategoryRes>>(ok.Value);
            Assert.Single(data);
        }

        [Fact]
        public async Task GetAccountCategoriesAsync_WithEmptyData_ReturnsOkWithEmptyList()
        {
            // Arrange
            _service.GetAccountCategoriesAsync().Returns(new List<AccountCategoryDto>());
            _mapper.Map<List<AccountCategoryRes>>(Arg.Any<List<AccountCategoryDto>>()).Returns(new List<AccountCategoryRes>());

            // Act
            var result = await _controller.GetAccountCategoriesAsync();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<List<AccountCategoryRes>>(ok.Value);
        }

        #endregion
    }
}
