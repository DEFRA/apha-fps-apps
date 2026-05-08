using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Services;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PACT.Application.UnitTests.Services.ProjectProfileServiceTest
{
    public class ProjectProfileServiceTests
    {
        private readonly IProjectProfileRepository _mockRepository;
        private readonly ProjectProfileService _sut;

        public ProjectProfileServiceTests()
        {
            _mockRepository = Substitute.For<IProjectProfileRepository>();
            _sut = new ProjectProfileService(_mockRepository);
        }

        #region GetProfileGraphDataAsync

        [Fact]
        public async Task GetProfileGraphDataAsync_WithData_ReturnsMappedDtoList()
        {
            var entities = new List<ProjectProfile>
            {
                new() { MonthNo = 1.0, Profile = 100m, Cost = 200m },
                new() { MonthNo = 2.0, Profile = 150m, Cost = 300m }
            };

            _mockRepository.GetProfileGraphDataAsync("PRJ1").Returns(entities);

            var result = await _sut.GetProfileGraphDataAsync("PRJ1");

            result.Should().HaveCount(2);
            result[0].MonthNo.Should().Be(1);
            result[0].Profile.Should().Be(100m);
            result[0].TotalCost.Should().Be(200m);
            result[1].MonthNo.Should().Be(2);
            result[1].Profile.Should().Be(150m);
            result[1].TotalCost.Should().Be(300m);
            await _mockRepository.Received(1).GetProfileGraphDataAsync("PRJ1");
        }

        [Fact]
        public async Task GetProfileGraphDataAsync_EmptyResult_ReturnsEmptyList()
        {
            _mockRepository.GetProfileGraphDataAsync("PRJ_NONE").Returns(new List<ProjectProfile>());

            var result = await _sut.GetProfileGraphDataAsync("PRJ_NONE");

            result.Should().BeEmpty();
            await _mockRepository.Received(1).GetProfileGraphDataAsync("PRJ_NONE");
        }

        [Fact]
        public async Task GetProfileGraphDataAsync_MapsMonthNoFromDoubleToInt()
        {
            var entities = new List<ProjectProfile>
            {
                new() { MonthNo = 3.0, Profile = 50m, Cost = 75m }
            };

            _mockRepository.GetProfileGraphDataAsync("PRJ1").Returns(entities);

            var result = await _sut.GetProfileGraphDataAsync("PRJ1");

            result.Should().ContainSingle();
            result[0].MonthNo.Should().Be(3);
            result[0].Should().BeOfType<ProjectProfileGraphDto>();
        }

        [Fact]
        public async Task GetProfileGraphDataAsync_NullProfileAndCost_MapsNullValues()
        {
            var entities = new List<ProjectProfile>
            {
                new() { MonthNo = 1.0, Profile = null, Cost = null }
            };

            _mockRepository.GetProfileGraphDataAsync("PRJ1").Returns(entities);

            var result = await _sut.GetProfileGraphDataAsync("PRJ1");

            result.Should().ContainSingle();
            result[0].Profile.Should().BeNull();
            result[0].TotalCost.Should().BeNull();
        }

        [Fact]
        public async Task GetProfileGraphDataAsync_RepositoryThrows_PropagatesException()
        {
            _mockRepository.GetProfileGraphDataAsync("PRJ1").ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.GetProfileGraphDataAsync("PRJ1"));
        }

        #endregion

        #region GetCumulativeGraphDataAsync

        [Fact]
        public async Task GetCumulativeGraphDataAsync_WithData_ReturnsMappedDtoList()
        {
            var entities = new List<ProjectProfile>
            {
                new() { MonthNo = 1.0, Profile = 100m, Cost = 200m },
                new() { MonthNo = 2.0, Profile = 250m, Cost = 500m }
            };

            _mockRepository.GetCumulativeGraphDataAsync("PRJ1").Returns(entities);

            var result = await _sut.GetCumulativeGraphDataAsync("PRJ1");

            result.Should().HaveCount(2);
            result[0].MonthNo.Should().Be(1);
            result[0].CumulativeProfile.Should().Be(100m);
            result[0].CumulativeCost.Should().Be(200m);
            result[1].MonthNo.Should().Be(2);
            result[1].CumulativeProfile.Should().Be(250m);
            result[1].CumulativeCost.Should().Be(500m);
            await _mockRepository.Received(1).GetCumulativeGraphDataAsync("PRJ1");
        }

        [Fact]
        public async Task GetCumulativeGraphDataAsync_EmptyResult_ReturnsEmptyList()
        {
            _mockRepository.GetCumulativeGraphDataAsync("PRJ_NONE").Returns(new List<ProjectProfile>());

            var result = await _sut.GetCumulativeGraphDataAsync("PRJ_NONE");

            result.Should().BeEmpty();
            await _mockRepository.Received(1).GetCumulativeGraphDataAsync("PRJ_NONE");
        }

        [Fact]
        public async Task GetCumulativeGraphDataAsync_MapsMonthNoFromDoubleToInt()
        {
            var entities = new List<ProjectProfile>
            {
                new() { MonthNo = 5.0, Profile = 400m, Cost = 800m }
            };

            _mockRepository.GetCumulativeGraphDataAsync("PRJ1").Returns(entities);

            var result = await _sut.GetCumulativeGraphDataAsync("PRJ1");

            result.Should().ContainSingle();
            result[0].MonthNo.Should().Be(5);
            result[0].Should().BeOfType<ProjectProfileCumulativeGraphDto>();
        }

        [Fact]
        public async Task GetCumulativeGraphDataAsync_NullProfileAndCost_MapsNullValues()
        {
            var entities = new List<ProjectProfile>
            {
                new() { MonthNo = 2.0, Profile = null, Cost = null }
            };

            _mockRepository.GetCumulativeGraphDataAsync("PRJ1").Returns(entities);

            var result = await _sut.GetCumulativeGraphDataAsync("PRJ1");

            result.Should().ContainSingle();
            result[0].CumulativeProfile.Should().BeNull();
            result[0].CumulativeCost.Should().BeNull();
        }

        [Fact]
        public async Task GetCumulativeGraphDataAsync_RepositoryThrows_PropagatesException()
        {
            _mockRepository.GetCumulativeGraphDataAsync("PRJ1").ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.GetCumulativeGraphDataAsync("PRJ1"));
        }

        #endregion
    }
}
