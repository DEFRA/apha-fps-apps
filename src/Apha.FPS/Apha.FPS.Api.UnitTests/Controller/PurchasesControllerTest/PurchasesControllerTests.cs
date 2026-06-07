using Apha.Common.Contracts.FPS;
using Apha.FPS.Api.Controllers;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPS.Api.UnitTests.Controller.PurchasesControllerTest
{
    public class PurchasesControllerTests
    {
        private readonly IPurchasesService _service;
        private readonly IMapper _mapper;
        private readonly PurchasesController _controller;

        public PurchasesControllerTests()
        {
            _service    = Substitute.For<IPurchasesService>();
            _mapper     = Substitute.For<IMapper>();
            _controller = new PurchasesController(_service, _mapper);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PurchasesController(null!, _mapper));
        }

        [Fact]
        public void Constructor_WithNullMapper_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PurchasesController(_service, null!));
        }

        #endregion

        #region GetPurchasesAsync Tests

        [Fact]
        public async Task GetPurchasesAsync_WithData_ReturnsOkWithMappedResults()
        {
            // Arrange
            var dtos = new List<PurchaseDto> { new() { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m } };
            var res  = new List<PurchaseRes> { new() { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m } };
            _service.GetPurchasesAsync("WG01", "ACC1").Returns(dtos);
            _mapper.Map<List<PurchaseRes>>(dtos).Returns(res);

            // Act
            var result = await _controller.GetPurchasesAsync("WG01", "ACC1");

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<List<PurchaseRes>>(ok.Value);
            Assert.Single(data);
        }

        [Fact]
        public async Task GetPurchasesAsync_WithEmptyData_ReturnsOkWithEmptyList()
        {
            // Arrange
            _service.GetPurchasesAsync("WG01", "ACC1").Returns(new List<PurchaseDto>());
            _mapper.Map<List<PurchaseRes>>(Arg.Any<List<PurchaseDto>>()).Returns(new List<PurchaseRes>());

            // Act
            var result = await _controller.GetPurchasesAsync("WG01", "ACC1");

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<List<PurchaseRes>>(ok.Value);
        }

        #endregion

        #region GetPurchaseByIdAsync Tests

        [Fact]
        public async Task GetPurchaseByIdAsync_WithExistingPurchase_ReturnsOk()
        {
            // Arrange
            var dto = new PurchaseDto { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m };
            var res = new PurchaseRes { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m };
            _service.GetPurchaseByIdAsync("WG01", "ACC1", "Item A").Returns(dto);
            _mapper.Map<PurchaseRes>(dto).Returns(res);

            // Act
            var result = await _controller.GetPurchaseByIdAsync("WG01", "ACC1", "Item A");

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<PurchaseRes>(ok.Value);
        }

        [Fact]
        public async Task GetPurchaseByIdAsync_WithNonExistingPurchase_ThrowsKeyNotFoundException()
        {
            // Arrange
            _service.GetPurchaseByIdAsync("WG01", "ACC1", "NOTEXIST").Returns((PurchaseDto?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _controller.GetPurchaseByIdAsync("WG01", "ACC1", "NOTEXIST"));
        }

        #endregion

        #region AddPurchaseAsync Tests

        [Fact]
        public async Task AddPurchaseAsync_WithValidRequest_ReturnsOk()
        {
            // Arrange
            var req       = new PurchaseReq { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m };
            var dto       = new PurchaseDto { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m };
            var resultDto = new PurchaseDto { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m, FpsYear = 2024 };
            var res       = new PurchaseRes { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m };

            _mapper.Map<PurchaseDto>(req).Returns(dto);
            _service.AddPurchaseAsync(dto).Returns(resultDto);
            _mapper.Map<PurchaseRes>(resultDto).Returns(res);

            // Act
            var result = await _controller.AddPurchaseAsync(req);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<PurchaseRes>(ok.Value);
        }

        [Fact]
        public async Task AddPurchaseAsync_WhenItemAlreadyExists_PropagatesException()
        {
            // Arrange
            var req = new PurchaseReq { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m };
            var dto = new PurchaseDto { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m };
            _mapper.Map<PurchaseDto>(req).Returns(dto);
            _service.AddPurchaseAsync(dto).ThrowsAsync(new InvalidOperationException("Item already exists."));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.AddPurchaseAsync(req));
        }

        [Fact]
        public async Task AddPurchaseAsync_WhenUnauthorized_PropagatesException()
        {
            // Arrange
            var req = new PurchaseReq { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m };
            var dto = new PurchaseDto { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m };
            _mapper.Map<PurchaseDto>(req).Returns(dto);
            _service.AddPurchaseAsync(dto).ThrowsAsync(new UnauthorizedAccessException("Not authorized."));

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.AddPurchaseAsync(req));
        }

        #endregion

        #region UpdatePurchaseAsync Tests

        [Fact]
        public async Task UpdatePurchaseAsync_WithValidRequest_ReturnsOk()
        {
            // Arrange
            var req       = new PurchaseReq { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item B", Amount = 200m, OldItemDescription = "Item A" };
            var dto       = new PurchaseDto { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item B", Amount = 200m, OldItemDescription = "Item A" };
            var resultDto = new PurchaseDto { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item B", Amount = 200m };
            var res       = new PurchaseRes { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item B", Amount = 200m };

            _mapper.Map<PurchaseDto>(req).Returns(dto);
            _service.UpdatePurchaseAsync(dto).Returns(resultDto);
            _mapper.Map<PurchaseRes>(resultDto).Returns(res);

            // Act
            var result = await _controller.UpdatePurchaseAsync(req);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<PurchaseRes>(ok.Value);
        }

        [Fact]
        public async Task UpdatePurchaseAsync_WhenItemNotFound_PropagatesException()
        {
            // Arrange
            var req = new PurchaseReq { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item B", Amount = 200m };
            var dto = new PurchaseDto { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item B", Amount = 200m };
            _mapper.Map<PurchaseDto>(req).Returns(dto);
            _service.UpdatePurchaseAsync(dto).ThrowsAsync(new InvalidOperationException("Item not found."));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.UpdatePurchaseAsync(req));
        }

        #endregion

        #region DeletePurchaseAsync Tests

        [Fact]
        public async Task DeletePurchaseAsync_WithExistingPurchase_ReturnsOk()
        {
            // Arrange
            _service.DeletePurchaseAsync("WG01", "ACC1", "Item A").Returns(true);

            // Act
            var result = await _controller.DeletePurchaseAsync("WG01", "ACC1", "Item A");

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(true, ok.Value);
        }

        [Fact]
        public async Task DeletePurchaseAsync_WhenPurchaseNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _service.DeletePurchaseAsync("WG01", "ACC1", "NOTEXIST").Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _controller.DeletePurchaseAsync("WG01", "ACC1", "NOTEXIST"));
        }

        [Fact]
        public async Task DeletePurchaseAsync_WhenUnauthorized_PropagatesException()
        {
            // Arrange
            _service.DeletePurchaseAsync("WG01", "ACC1", "Item A")
                .ThrowsAsync(new UnauthorizedAccessException("Not authorized."));

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _controller.DeletePurchaseAsync("WG01", "ACC1", "Item A"));
        }

        #endregion
    }
}
