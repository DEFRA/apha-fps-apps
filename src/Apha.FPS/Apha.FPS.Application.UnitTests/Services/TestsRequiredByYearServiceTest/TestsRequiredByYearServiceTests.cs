using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Services;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using MapsterMapper;
using NSubstitute;
using Xunit;

namespace Apha.FPS.Application.UnitTests.Services.TestsRequiredByYearServiceTest
{
    public class TestsRequiredByYearServiceTests
    {
        private readonly IMapper _mapper = Substitute.For<IMapper>();

        [Fact]
        public async Task GetTestsRequiredByWgByYearAsync_CallsRepositoryWithYearAndMapsResult()
        {
            var repository = Substitute.For<ITestsRequiredByWgRepository>();
            var entities = new List<TestsRequiredByWgView> { new() { TestCode = "TC1", WorkGroup = "WG1" } };
            var dtos = new List<TestsRequiredByWgDto> { new() { TestCode = "TC1", WorkGroup = "WG1" } };
            repository.GetTestsRequiredByWgByYearAsync("ASU", 2025).Returns(entities);
            _mapper.Map<List<TestsRequiredByWgDto>>(entities).Returns(dtos);
            var sut = new TestsRequiredByWgService(repository, _mapper);

            var result = await sut.GetTestsRequiredByWgByYearAsync("ASU", 2025);

            Assert.Same(dtos, result);
            await repository.Received(1).GetTestsRequiredByWgByYearAsync("ASU", 2025);
        }

        [Fact]
        public async Task GetTestsRequiredByWgByYearAsync_WhenRepositoryReturnsEmpty_ReturnsEmpty()
        {
            var repository = Substitute.For<ITestsRequiredByWgRepository>();
            var entities = new List<TestsRequiredByWgView>();
            repository.GetTestsRequiredByWgByYearAsync(null, 2024).Returns(entities);
            _mapper.Map<List<TestsRequiredByWgDto>>(entities).Returns(new List<TestsRequiredByWgDto>());
            var sut = new TestsRequiredByWgService(repository, _mapper);

            var result = await sut.GetTestsRequiredByWgByYearAsync(null, 2024);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetTestsRequiredByRcByYearAsync_CallsRepositoryWithYearAndMapsResult()
        {
            var repository = Substitute.For<ITestsRequiredByRcRepository>();
            var entities = new List<TestsRequiredByRcView> { new() { TestCode = "TC1", ProfitCentre = "ASU" } };
            var dtos = new List<TestsRequiredByRcDto> { new() { TestCode = "TC1", ProfitCentre = "ASU" } };
            repository.GetTestsRequiredByRcByYearAsync("ASU", 2025).Returns(entities);
            _mapper.Map<List<TestsRequiredByRcDto>>(entities).Returns(dtos);
            var sut = new TestsRequiredByRcService(repository, _mapper);

            var result = await sut.GetTestsRequiredByRcByYearAsync("ASU", 2025);

            Assert.Same(dtos, result);
            await repository.Received(1).GetTestsRequiredByRcByYearAsync("ASU", 2025);
        }

        [Fact]
        public async Task GetTestsRequiredByRcByYearAsync_WhenRepositoryReturnsEmpty_ReturnsEmpty()
        {
            var repository = Substitute.For<ITestsRequiredByRcRepository>();
            var entities = new List<TestsRequiredByRcView>();
            repository.GetTestsRequiredByRcByYearAsync(null, 2024).Returns(entities);
            _mapper.Map<List<TestsRequiredByRcDto>>(entities).Returns(new List<TestsRequiredByRcDto>());
            var sut = new TestsRequiredByRcService(repository, _mapper);

            var result = await sut.GetTestsRequiredByRcByYearAsync(null, 2024);

            Assert.Empty(result);
        }
    }
}
