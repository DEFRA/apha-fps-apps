using Apha.Common.Contracts.FPS;
using Apha.FPS.Api.Controllers;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPS.Api.UnitTests.Controller.SubAccountControllerTest
{
    public class SubAccountControllerTests
    {
        private readonly ISubAccountService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly SubAccountController _controller;

        public SubAccountControllerTests()
        {
            _serviceMock = Substitute.For<ISubAccountService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new SubAccountController(_serviceMock, _mapperMock);
        }

        [Fact]
        public async Task GetAllSubAccountsAsync_HappyPath_ReturnsOk()
        {
            var serviceResult = new List<SubAccountDto> { new() { SubAccountCode = "SA1", SubAccountName = "Sub 1" } };
            var mappedResult = new List<SubAccountRes> { new() { SubAccountCode = "SA1", SubAccount = "Sub 1" } };

            _serviceMock.GetAllSubAccountsAsync().Returns(serviceResult);
            _mapperMock.Map<IEnumerable<SubAccountRes>>(serviceResult).Returns(mappedResult);

            var result = await _controller.GetAllSubAccountsAsync();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(mappedResult, okResult.Value);
        }

        [Fact]
        public async Task GetAllSubAccountsAsync_EmptyList_ReturnsOk()
        {
            var serviceResult = new List<SubAccountDto>();
            var mappedResult = new List<SubAccountRes>();

            _serviceMock.GetAllSubAccountsAsync().Returns(serviceResult);
            _mapperMock.Map<IEnumerable<SubAccountRes>>(serviceResult).Returns(mappedResult);

            var result = await _controller.GetAllSubAccountsAsync();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsAssignableFrom<IEnumerable<SubAccountRes>>(okResult.Value);
            Assert.Empty(value);
        }

        [Fact]
        public async Task GetAllSubAccountsAsync_ServiceThrows_PropagatesException()
        {
            _serviceMock.GetAllSubAccountsAsync().Throws(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetAllSubAccountsAsync());
        }

        [Fact]
        public async Task GetAllSubAccountsAsync_MapperThrows_PropagatesException()
        {
            var serviceResult = new List<SubAccountDto> { new() { SubAccountCode = "SA1" } };
            _serviceMock.GetAllSubAccountsAsync().Returns(serviceResult);
            _mapperMock.Map<IEnumerable<SubAccountRes>>(serviceResult).Throws(new AutoMapperMappingException("Mapping error"));

            await Assert.ThrowsAsync<AutoMapperMappingException>(() => _controller.GetAllSubAccountsAsync());
        }
    }
}
