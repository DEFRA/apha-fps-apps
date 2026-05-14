using Apha.Common.Contracts.FPS;
using Apha.FPS.Api.Controllers;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPS.Api.UnitTests.Controller.AccountCodeControllerTest
{
    public class AccountCodeControllerTests
    {
        private readonly IAccountCodeService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly AccountCodeController _controller;

        public AccountCodeControllerTests()
        {
            _serviceMock = Substitute.For<IAccountCodeService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new AccountCodeController(_serviceMock, _mapperMock);
        }

        [Fact]
        public async Task GetAllAccountCodesAsync_HappyPath_ReturnsOk()
        {
            var serviceResult = new List<AccountCodeDto> { new() { Code = "AC1", Description = "Account 1" } };
            var mappedResult = new List<AccountCodeRes> { new() { Code = "AC1", Description = "Account 1" } };

            _serviceMock.GetAllAccountCodeAsync().Returns(serviceResult);
            _mapperMock.Map<IEnumerable<AccountCodeRes>>(serviceResult).Returns(mappedResult);

            var result = await _controller.GetAllAccountCodesAsync();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(mappedResult, okResult.Value);
        }

        [Fact]
        public async Task GetAllAccountCodesAsync_EmptyList_ReturnsOk()
        {
            var serviceResult = new List<AccountCodeDto>();
            var mappedResult = new List<AccountCodeRes>();

            _serviceMock.GetAllAccountCodeAsync().Returns(serviceResult);
            _mapperMock.Map<IEnumerable<AccountCodeRes>>(serviceResult).Returns(mappedResult);

            var result = await _controller.GetAllAccountCodesAsync();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsAssignableFrom<IEnumerable<AccountCodeRes>>(okResult.Value);
            Assert.Empty(value);
        }

        [Fact]
        public async Task GetAllAccountCodesAsync_ServiceThrows_PropagatesException()
        {
            _serviceMock.GetAllAccountCodeAsync().Throws(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetAllAccountCodesAsync());
        }

        [Fact]
        public async Task GetAllAccountCodesAsync_MapperThrows_PropagatesException()
        {
            var serviceResult = new List<AccountCodeDto> { new() { Code = "AC1" } };
            _serviceMock.GetAllAccountCodeAsync().Returns(serviceResult);
            _mapperMock.Map<IEnumerable<AccountCodeRes>>(serviceResult).Throws(new AutoMapperMappingException("Mapping error"));

            await Assert.ThrowsAsync<AutoMapperMappingException>(() => _controller.GetAllAccountCodesAsync());
        }
    }
}
