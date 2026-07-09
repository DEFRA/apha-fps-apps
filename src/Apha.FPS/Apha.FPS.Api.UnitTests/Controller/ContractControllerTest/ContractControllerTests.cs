using Apha.Common.Contracts.FPS;
using Apha.FPS.Api.Controllers;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPS.Api.UnitTests.Controller.ContractControllerTest
{
    public class ContractControllerTests
    {
        private readonly IContractService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly ContractController _controller;

        public ContractControllerTests()
        {
            _serviceMock = Substitute.For<IContractService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new ContractController(_serviceMock, _mapperMock);
        }

        #region GetAllContractsAsync

        [Fact]
        public async Task GetAllContractsAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var serviceResult = new List<ContractDto>();
            var mappedResult = new List<ContractRes>();

            _serviceMock.GetAllContractsAsync().Returns(serviceResult);
            _mapperMock.Map<List<ContractRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetAllContractsAsync();

            // Assert            
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(mappedResult, okResult.Value);
        }

        [Fact]
        public async Task GetAllContractsAsync_EdgeCase_EmptyList()
        {
            // Arrange
            var serviceResult = new List<ContractDto>();
            var mappedResult = new List<ContractRes>();

            _serviceMock.GetAllContractsAsync().Returns(serviceResult);
            _mapperMock.Map<List<ContractRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetAllContractsAsync();

            // Assert
            Assert.Null(result.Value);
        }

        [Fact]
        public async Task GetAllContractsAsync_Error_ServiceThrows()
        {
            // Arrange
            _serviceMock.GetAllContractsAsync().Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAllContractsAsync());
        }

        [Fact]
        public async Task GetAllContractsAsync_Error_MapperThrows()
        {
            // Arrange
            var serviceResult = new List<ContractDto>();
            _serviceMock.GetAllContractsAsync().Returns(serviceResult);
            _mapperMock.Map<List<ContractRes>>(serviceResult).Throws(new Exception("Mapping error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAllContractsAsync());
        }

        #endregion

        #region GetContractsByUserAsync

        [Fact]
        public async Task GetContractsByUserAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var serviceResult = new List<ContractDto>();
            var mappedResult = new List<ContractRes>();

            _serviceMock.GetAllContractsByUserAsync().Returns(serviceResult);
            _mapperMock.Map<List<ContractRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetContractsByUserAsync();

            // Assert            
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(mappedResult, okResult.Value);
        }

        [Fact]
        public async Task GetContractsByUserAsync_EdgeCase_EmptyList()
        {
            // Arrange
            var serviceResult = new List<ContractDto>();
            var mappedResult = new List<ContractRes>();

            _serviceMock.GetAllContractsByUserAsync().Returns(serviceResult);
            _mapperMock.Map<List<ContractRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetContractsByUserAsync();

            // Assert
            Assert.Null(result.Value);
        }

        [Fact]
        public async Task GetContractsByUserAsync_Error_ServiceThrows()
        {
            // Arrange
            _serviceMock.GetAllContractsByUserAsync().Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetContractsByUserAsync());
        }

        [Fact]
        public async Task GetContractsByUserAsync_Error_MapperThrows()
        {
            // Arrange
            var serviceResult = new List<ContractDto>();
            _serviceMock.GetAllContractsByUserAsync().Returns(serviceResult);
            _mapperMock.Map<List<ContractRes>>(serviceResult).Throws(new Exception("Mapping error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetContractsByUserAsync());
        }

        #endregion

        #region GetAllPactContractsAsync

        [Fact]
        public async Task GetAllPactContractsAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var serviceResult = new List<ContractDto>();
            var mappedResult = new List<ContractRes>();

            _serviceMock.GetAllPactContractsAsync().Returns(serviceResult);
            _mapperMock.Map<List<ContractRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetAllPactContractsAsync();

            // Assert            
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(mappedResult, okResult.Value);
        }

        [Fact]
        public async Task GetAllPactContractsAsync_EdgeCase_EmptyList()
        {
            // Arrange
            var serviceResult = new List<ContractDto>();
            var mappedResult = new List<ContractRes>();

            _serviceMock.GetAllPactContractsAsync().Returns(serviceResult);
            _mapperMock.Map<List<ContractRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetAllPactContractsAsync();

            // Assert
            Assert.Null(result.Value);
        }

        [Fact]
        public async Task GetAllPactContractsAsync_Error_ServiceThrows()
        {
            // Arrange
            _serviceMock.GetAllPactContractsAsync().Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAllPactContractsAsync());
        }

        [Fact]
        public async Task GetAllPactContractsAsync_Error_MapperThrows()
        {
            // Arrange
            var serviceResult = new List<ContractDto>();
            _serviceMock.GetAllPactContractsAsync().Returns(serviceResult);
            _mapperMock.Map<List<ContractRes>>(serviceResult).Throws(new Exception("Mapping error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAllPactContractsAsync());
        }

        #endregion
    }
}
