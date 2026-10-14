using Apha.Common.Contracts.FPS;
using Apha.FPS.Api.Controllers;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace Apha.FPS.Api.UnitTests.Controller.TestsRequiredByYearControllerTest
{
    public class TestsRequiredByYearControllerTests
    {
        private readonly ITestsRequiredByWgService _wgService = Substitute.For<ITestsRequiredByWgService>();
        private readonly ITestsRequiredByRcService _rcService = Substitute.For<ITestsRequiredByRcService>();
        private readonly IMapper _mapper = Substitute.For<IMapper>();

        [Fact]
        public async Task GetTestsRequiredByWgByYearAsync_ReturnsOkWithMappedRows()
        {
            var dtos = new List<TestsRequiredByWgDto> { new() { TestCode = "TC1" } };
            var res = new List<TestsRequiredByWgRes> { new() { TestCode = "TC1" } };
            _wgService.GetTestsRequiredByWgByYearAsync("ASU", 2025).Returns(dtos);
            _mapper.Map<List<TestsRequiredByWgRes>>(dtos).Returns(res);
            var controller = new TestsRequiredByWgController(_wgService, _mapper);

            var result = await controller.GetTestsRequiredByWgByYearAsync("ASU", 2025);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Same(res, ok.Value);
            await _wgService.Received(1).GetTestsRequiredByWgByYearAsync("ASU", 2025);
        }

        [Fact]
        public async Task GetTestsRequiredByWgByYearAsync_WithNullProfitCentre_PassesNullToService()
        {
            _wgService.GetTestsRequiredByWgByYearAsync(null, 2024).Returns(new List<TestsRequiredByWgDto>());
            _mapper.Map<List<TestsRequiredByWgRes>>(Arg.Any<List<TestsRequiredByWgDto>>()).Returns(new List<TestsRequiredByWgRes>());
            var controller = new TestsRequiredByWgController(_wgService, _mapper);

            var result = await controller.GetTestsRequiredByWgByYearAsync(null, 2024);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Empty(Assert.IsType<List<TestsRequiredByWgRes>>(ok.Value));
            await _wgService.Received(1).GetTestsRequiredByWgByYearAsync(null, 2024);
        }

        [Fact]
        public async Task GetTestsRequiredByRcByYearAsync_ReturnsOkWithMappedRows()
        {
            var dtos = new List<TestsRequiredByRcDto> { new() { TestCode = "TC1" } };
            var res = new List<TestsRequiredByRcRes> { new() { TestCode = "TC1" } };
            _rcService.GetTestsRequiredByRcByYearAsync("ASU", 2025).Returns(dtos);
            _mapper.Map<List<TestsRequiredByRcRes>>(dtos).Returns(res);
            var controller = new TestsRequiredByRcController(_rcService, _mapper);

            var result = await controller.GetTestsRequiredByRcByYearAsync("ASU", 2025);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Same(res, ok.Value);
            await _rcService.Received(1).GetTestsRequiredByRcByYearAsync("ASU", 2025);
        }

        [Fact]
        public async Task GetTestsRequiredByRcByYearAsync_WithNullProfitCentre_PassesNullToService()
        {
            _rcService.GetTestsRequiredByRcByYearAsync(null, 2024).Returns(new List<TestsRequiredByRcDto>());
            _mapper.Map<List<TestsRequiredByRcRes>>(Arg.Any<List<TestsRequiredByRcDto>>()).Returns(new List<TestsRequiredByRcRes>());
            var controller = new TestsRequiredByRcController(_rcService, _mapper);

            var result = await controller.GetTestsRequiredByRcByYearAsync(null, 2024);

            Assert.IsType<OkObjectResult>(result);
            await _rcService.Received(1).GetTestsRequiredByRcByYearAsync(null, 2024);
        }
    }
}
