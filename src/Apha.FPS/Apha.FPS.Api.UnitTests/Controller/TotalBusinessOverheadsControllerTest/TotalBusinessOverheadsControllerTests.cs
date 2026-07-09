using Apha.Common.Contracts.FPS;
using Apha.FPS.Api.Controllers;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPS.Api.UnitTests.Controller.TotalBusinessOverheadsControllerTest
{
    public class TotalBusinessOverheadsControllerTests
    {
        private readonly ITotalBusinessOverheadsService _service;
        private readonly IMapper _mapper;
        private readonly TotalBusinessOverheadsController _controller;

        public TotalBusinessOverheadsControllerTests()
        {
            _service = Substitute.For<ITotalBusinessOverheadsService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new TotalBusinessOverheadsController(_service, _mapper);
        }

        private static TotalBusinessOverheadsDto BuildDto(decimal? overheads = 1000000m, int fpsYear = 2025) =>
            new() { TotalBusinessOverheads = overheads, FpsYear = fpsYear };

        private static TotalBusinessOverheadsRes BuildRes(decimal? overheads = 1000000m, int fpsYear = 2025) =>
            new() { TotalBusinessOverheads = overheads, FpsYear = fpsYear };

        private static TotalBusinessOverheadsReq BuildReq(decimal? overheads = 1000000m) =>
            new() { TotalBusinessOverheads = overheads };

        #region GetAsync Tests

        [Fact]
        public async Task GetAsync_WithExistingData_ReturnsOkWithMappedResponse()
        {
            // Arrange
            var dto = BuildDto();
            var response = BuildRes();

            _service.GetAsync().Returns(dto);
            _mapper.Map<TotalBusinessOverheadsRes>(dto).Returns(response);

            // Act
            var result = await _controller.GetAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<TotalBusinessOverheadsRes>(okResult.Value);
            Assert.Equal(1000000m, data.TotalBusinessOverheads);
            Assert.Equal(2025, data.FpsYear);
        }

        [Fact]
        public async Task GetAsync_WhenServiceReturnsNull_ThrowsKeyNotFoundException()
        {
            // Arrange
            _service.GetAsync().Returns((TotalBusinessOverheadsDto?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.GetAsync());
            Assert.Equal("Total Business Overheads not found for the current year.", exception.Message);
        }

        [Fact]
        public async Task GetAsync_CallsServiceGetAsync()
        {
            // Arrange
            var dto = BuildDto();
            _service.GetAsync().Returns(dto);
            _mapper.Map<TotalBusinessOverheadsRes>(dto).Returns(BuildRes());

            // Act
            await _controller.GetAsync();

            // Assert
            await _service.Received(1).GetAsync();
        }

        [Fact]
        public async Task GetAsync_CallsMapperToConvertDtoToResponse()
        {
            // Arrange
            var dto = BuildDto();
            _service.GetAsync().Returns(dto);
            _mapper.Map<TotalBusinessOverheadsRes>(dto).Returns(BuildRes());

            // Act
            await _controller.GetAsync();

            // Assert
            _mapper.Received(1).Map<TotalBusinessOverheadsRes>(dto);
        }

        [Fact]
        public async Task GetAsync_WithNullOverheads_ReturnsOk()
        {
            // Arrange
            var dto = BuildDto(null);
            var response = BuildRes(null);

            _service.GetAsync().Returns(dto);
            _mapper.Map<TotalBusinessOverheadsRes>(dto).Returns(response);

            // Act
            var result = await _controller.GetAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<TotalBusinessOverheadsRes>(okResult.Value);
            Assert.Null(data.TotalBusinessOverheads);
        }

        [Fact]
        public async Task GetAsync_WithZeroOverheads_ReturnsOk()
        {
            // Arrange
            var dto = BuildDto(0m);
            var response = BuildRes(0m);

            _service.GetAsync().Returns(dto);
            _mapper.Map<TotalBusinessOverheadsRes>(dto).Returns(response);

            // Act
            var result = await _controller.GetAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<TotalBusinessOverheadsRes>(okResult.Value);
            Assert.Equal(0m, data.TotalBusinessOverheads);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WithValidRequest_ReturnsOkWithUpdatedData()
        {
            // Arrange
            var request = BuildReq(1500000m);
            var dto = BuildDto(1500000m);
            var updatedDto = BuildDto(1500000m);
            var response = BuildRes(1500000m);

            _mapper.Map<TotalBusinessOverheadsDto>(request).Returns(dto);
            _service.UpdateAsync(dto).Returns(updatedDto);
            _mapper.Map<TotalBusinessOverheadsRes>(updatedDto).Returns(response);

            // Act
            var result = await _controller.UpdateAsync(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<TotalBusinessOverheadsRes>(okResult.Value);
            Assert.Equal(1500000m, data.TotalBusinessOverheads);
        }

        [Fact]
        public async Task UpdateAsync_CallsMapperToConvertRequestToDto()
        {
            // Arrange
            var request = BuildReq();
            var dto = BuildDto();
            var updatedDto = BuildDto();

            _mapper.Map<TotalBusinessOverheadsDto>(request).Returns(dto);
            _service.UpdateAsync(dto).Returns(updatedDto);
            _mapper.Map<TotalBusinessOverheadsRes>(updatedDto).Returns(BuildRes());

            // Act
            await _controller.UpdateAsync(request);

            // Assert
            _mapper.Received(1).Map<TotalBusinessOverheadsDto>(request);
        }

        [Fact]
        public async Task UpdateAsync_CallsServiceUpdateAsync()
        {
            // Arrange
            var request = BuildReq();
            var dto = BuildDto();
            var updatedDto = BuildDto();

            _mapper.Map<TotalBusinessOverheadsDto>(request).Returns(dto);
            _service.UpdateAsync(dto).Returns(updatedDto);
            _mapper.Map<TotalBusinessOverheadsRes>(updatedDto).Returns(BuildRes());

            // Act
            await _controller.UpdateAsync(request);

            // Assert
            await _service.Received(1).UpdateAsync(dto);
        }

        [Fact]
        public async Task UpdateAsync_CallsMapperToConvertDtoToResponse()
        {
            // Arrange
            var request = BuildReq();
            var dto = BuildDto();
            var updatedDto = BuildDto();

            _mapper.Map<TotalBusinessOverheadsDto>(request).Returns(dto);
            _service.UpdateAsync(dto).Returns(updatedDto);
            _mapper.Map<TotalBusinessOverheadsRes>(updatedDto).Returns(BuildRes());

            // Act
            await _controller.UpdateAsync(request);

            // Assert
            _mapper.Received(1).Map<TotalBusinessOverheadsRes>(updatedDto);
        }

        [Fact]
        public async Task UpdateAsync_WithNullOverheads_ProcessesSuccessfully()
        {
            // Arrange
            var request = BuildReq(null);
            var dto = BuildDto(null);
            var updatedDto = BuildDto(null);
            var response = BuildRes(null);

            _mapper.Map<TotalBusinessOverheadsDto>(request).Returns(dto);
            _service.UpdateAsync(dto).Returns(updatedDto);
            _mapper.Map<TotalBusinessOverheadsRes>(updatedDto).Returns(response);

            // Act
            var result = await _controller.UpdateAsync(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<TotalBusinessOverheadsRes>(okResult.Value);
            Assert.Null(data.TotalBusinessOverheads);
        }

        [Fact]
        public async Task UpdateAsync_WithZeroOverheads_ProcessesSuccessfully()
        {
            // Arrange
            var request = BuildReq(0m);
            var dto = BuildDto(0m);
            var updatedDto = BuildDto(0m);
            var response = BuildRes(0m);

            _mapper.Map<TotalBusinessOverheadsDto>(request).Returns(dto);
            _service.UpdateAsync(dto).Returns(updatedDto);
            _mapper.Map<TotalBusinessOverheadsRes>(updatedDto).Returns(response);

            // Act
            var result = await _controller.UpdateAsync(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<TotalBusinessOverheadsRes>(okResult.Value);
            Assert.Equal(0m, data.TotalBusinessOverheads);
        }

        [Fact]
        public async Task UpdateAsync_WithLargeValue_ProcessesSuccessfully()
        {
            // Arrange
            var request = BuildReq(999999999.99m);
            var dto = BuildDto(999999999.99m);
            var updatedDto = BuildDto(999999999.99m);
            var response = BuildRes(999999999.99m);

            _mapper.Map<TotalBusinessOverheadsDto>(request).Returns(dto);
            _service.UpdateAsync(dto).Returns(updatedDto);
            _mapper.Map<TotalBusinessOverheadsRes>(updatedDto).Returns(response);

            // Act
            var result = await _controller.UpdateAsync(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<TotalBusinessOverheadsRes>(okResult.Value);
            Assert.Equal(999999999.99m, data.TotalBusinessOverheads);
        }

        [Fact]
        public async Task UpdateAsync_WhenServiceThrowsInvalidOperationException_ExceptionPropagates()
        {
            // Arrange
            var request = BuildReq();
            var dto = BuildDto();

            _mapper.Map<TotalBusinessOverheadsDto>(request).Returns(dto);
            _service.UpdateAsync(dto).ThrowsAsync(new InvalidOperationException("Record not found"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.UpdateAsync(request));
        }

        #endregion
    }
}
