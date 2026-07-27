using Apha.Common.Contracts.FPS;
using Apha.FPS.Api.Controllers;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPS.Api.UnitTests.Controller.DiseaseControllerTest
{
    public class DiseaseControllerTests
    {
        private readonly IDiseaseService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly DiseaseController _controller;

        public DiseaseControllerTests()
        {
            _serviceMock = Substitute.For<IDiseaseService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new DiseaseController(_serviceMock, _mapperMock);
        }

        private static DiseaseDto BuildDto(string name = "Foot and Mouth Disease") =>
            new() { DiseaseName = name };

        private static DiseaseReq BuildReq(string name = "Foot and Mouth Disease") =>
            new() { DiseaseName = name };

        private static DiseaseRes BuildRes(string name = "Foot and Mouth Disease") =>
            new() { Disease = name };

        #region Constructor Tests

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenServiceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new DiseaseController(null!, _mapperMock));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenMapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new DiseaseController(_serviceMock, null!));
        }

        #endregion

        #region GetAllDiseasesAsync Tests

        [Fact]
        public async Task GetAllDiseasesAsync_ReturnsOk_WithMappedList()
        {
            // Arrange
            var dtos = new List<DiseaseDto> { BuildDto("Anthrax"), BuildDto("Rabies") };
            var expected = new List<DiseaseRes> { BuildRes("Anthrax"), BuildRes("Rabies") };

            _serviceMock.GetAllDiseasesAsync().Returns(dtos);
            _mapperMock.Map<List<DiseaseRes>>(dtos).Returns(expected);

            // Act
            var result = await _controller.GetAllDiseasesAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expected, okResult.Value);
            await _serviceMock.Received(1).GetAllDiseasesAsync();
        }

        [Fact]
        public async Task GetAllDiseasesAsync_ReturnsOk_WithEmptyList()
        {
            // Arrange
            var dtos = new List<DiseaseDto>();
            var expected = new List<DiseaseRes>();

            _serviceMock.GetAllDiseasesAsync().Returns(dtos);
            _mapperMock.Map<List<DiseaseRes>>(dtos).Returns(expected);

            // Act
            var result = await _controller.GetAllDiseasesAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<List<DiseaseRes>>(okResult.Value);
            Assert.Empty(response);
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_ValidReq_ReturnsCreatedAtAction_WithMappedRes()
        {
            // Arrange
            var req = BuildReq();
            var dto = BuildDto();
            var created = BuildDto();
            var res = BuildRes();

            _mapperMock.Map<DiseaseDto>(req).Returns(dto);
            _serviceMock.CreateDiseaseAsync(dto).Returns(created);
            _mapperMock.Map<DiseaseRes>(created).Returns(res);

            // Act
            var result = await _controller.CreateAsync(req);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(res, createdResult.Value);
            Assert.Equal(nameof(DiseaseController.GetAllDiseasesAsync), createdResult.ActionName);
            await _serviceMock.Received(1).CreateDiseaseAsync(dto);
        }

        [Fact]
        public async Task CreateAsync_ServiceThrowsArgumentException_PropagatesException()
        {
            // Arrange
            var req = BuildReq("");
            var dto = BuildDto("");

            _mapperMock.Map<DiseaseDto>(req).Returns(dto);
            _serviceMock.CreateDiseaseAsync(dto).ThrowsAsync(new ArgumentException("Disease name is required"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.CreateAsync(req));
        }

        [Fact]
        public async Task CreateAsync_ServiceThrowsInvalidOperationException_PropagatesException()
        {
            // Arrange
            var req = BuildReq("Anthrax");
            var dto = BuildDto("Anthrax");

            _mapperMock.Map<DiseaseDto>(req).Returns(dto);
            _serviceMock.CreateDiseaseAsync(dto).ThrowsAsync(new InvalidOperationException("Disease already exists"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.CreateAsync(req));
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_ValidExistingName_ReturnsOkTrue()
        {
            // Arrange
            _serviceMock.DeleteDiseaseAsync("Anthrax").Returns(true);

            // Act
            var result = await _controller.DeleteAsync("Anthrax");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
            await _serviceMock.Received(1).DeleteDiseaseAsync("Anthrax");
        }

        [Fact]
        public async Task DeleteAsync_ServiceReturnsFalse_ThrowsKeyNotFoundException()
        {
            // Arrange
            _serviceMock.DeleteDiseaseAsync("NotExist").Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.DeleteAsync("NotExist"));
        }

        [Fact]
        public async Task DeleteAsync_EmptyName_ServicePropagatesArgumentException()
        {
            // Arrange
            _serviceMock.DeleteDiseaseAsync("").ThrowsAsync(new ArgumentException("Disease name is required"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.DeleteAsync(""));
        }

        #endregion
    }
}
