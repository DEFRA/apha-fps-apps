using Apha.Common.Contracts.FPS;
using Apha.FPS.Api.Controllers;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPS.Api.UnitTests.Controller.CostCentreControllerTest
{
    public class CostCentreControllerTests
    {
        private readonly IStoredProcRepository _repositoryMock;
        private readonly IMapper _mapperMock;
        private readonly CostCentreController _controller;

        public CostCentreControllerTests()
        {
            _repositoryMock = Substitute.For<IStoredProcRepository>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new CostCentreController(_repositoryMock, _mapperMock);
        }

        [Fact]
        public async Task GetAllCostCentresAsync_HappyPath_ReturnsOk()
        {
            var serviceResult = new List<CostCentreWorkgroup> { new() { CostCentre = 100, ProfitCentre = "PC1" } };
            var mappedResult = new List<CostCentreWorkgroupRes> { new() { CostCentre = 100, ProfitCentre = "PC1" } };

            _repositoryMock.GetAllCostCentreWorkgroupAsync().Returns(serviceResult);
            _mapperMock.Map<IEnumerable<CostCentreWorkgroupRes>>(serviceResult).Returns(mappedResult);

            var result = await _controller.GetAllCostCentresAsync();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(mappedResult, okResult.Value);
        }

        [Fact]
        public async Task GetAllCostCentresAsync_EmptyList_ReturnsOk()
        {
            var serviceResult = new List<CostCentreWorkgroup>();
            var mappedResult = new List<CostCentreWorkgroupRes>();

            _repositoryMock.GetAllCostCentreWorkgroupAsync().Returns(serviceResult);
            _mapperMock.Map<IEnumerable<CostCentreWorkgroupRes>>(serviceResult).Returns(mappedResult);

            var result = await _controller.GetAllCostCentresAsync();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsAssignableFrom<IEnumerable<CostCentreWorkgroupRes>>(okResult.Value);
            Assert.Empty(value);
        }

        [Fact]
        public async Task GetAllCostCentresAsync_RepositoryThrows_PropagatesException()
        {
            _repositoryMock.GetAllCostCentreWorkgroupAsync().Throws(new Exception("Repository error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetAllCostCentresAsync());
        }

        [Fact]
        public async Task GetAllCostCentresAsync_MapperThrows_PropagatesException()
        {
            var serviceResult = new List<CostCentreWorkgroup> { new() { CostCentre = 100 } };
            _repositoryMock.GetAllCostCentreWorkgroupAsync().Returns(serviceResult);
            _mapperMock.Map<IEnumerable<CostCentreWorkgroupRes>>(serviceResult).Throws(new AutoMapperMappingException("Mapping error"));

            await Assert.ThrowsAsync<AutoMapperMappingException>(() => _controller.GetAllCostCentresAsync());
        }
    }
}
