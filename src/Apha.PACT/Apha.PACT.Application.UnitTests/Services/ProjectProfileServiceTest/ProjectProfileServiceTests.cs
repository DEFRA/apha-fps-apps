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

        #region GetProfileDataAsync

        [Fact]
        public async Task GetProfileDataAsync_WithData_ReturnsMappedDtoList()
        {
            var entities = new List<ProjectProfile>
            {
                new() { MonthNo = 1.0, Profile = 100m, Cost = 200m },
                new() { MonthNo = 2.0, Profile = 150m, Cost = 300m }
            };

            _mockRepository.GetProfileDataAsync("PRJ1").Returns(entities);

            var result = await _sut.GetProfileDataAsync("PRJ1");

            result.Should().HaveCount(2);
            result[0].MonthNo.Should().Be(1);
            result[0].Profile.Should().Be(100m);
            result[0].TotalCost.Should().Be(200m);
            result[1].MonthNo.Should().Be(2);
            result[1].Profile.Should().Be(150m);
            result[1].TotalCost.Should().Be(300m);
            await _mockRepository.Received(1).GetProfileDataAsync("PRJ1");
        }

        [Fact]
        public async Task GetProfileDataAsync_EmptyResult_ReturnsEmptyList()
        {
            _mockRepository.GetProfileDataAsync("PRJ_NONE").Returns(new List<ProjectProfile>());

            var result = await _sut.GetProfileDataAsync("PRJ_NONE");

            result.Should().BeEmpty();
            await _mockRepository.Received(1).GetProfileDataAsync("PRJ_NONE");
        }

        [Fact]
        public async Task GetProfileDataAsync_MapsMonthNoFromDoubleToInt()
        {
            var entities = new List<ProjectProfile>
            {
                new() { MonthNo = 3.0, Profile = 50m, Cost = 75m }
            };

            _mockRepository.GetProfileDataAsync("PRJ1").Returns(entities);

            var result = await _sut.GetProfileDataAsync("PRJ1");

            result.Should().ContainSingle();
            result[0].MonthNo.Should().Be(3);
            result[0].Should().BeOfType<ProjectProfileDto>();
        }

        [Fact]
        public async Task GetProfileDataAsync_NullProfileAndCost_MapsNullValues()
        {
            var entities = new List<ProjectProfile>
            {
                new() { MonthNo = 1.0, Profile = null, Cost = null }
            };

            _mockRepository.GetProfileDataAsync("PRJ1").Returns(entities);

            var result = await _sut.GetProfileDataAsync("PRJ1");

            result.Should().ContainSingle();
            result[0].Profile.Should().BeNull();
            result[0].TotalCost.Should().BeNull();
        }

        [Fact]
        public async Task GetProfileDataAsync_RepositoryThrows_PropagatesException()
        {
            _mockRepository.GetProfileDataAsync("PRJ1").ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.GetProfileDataAsync("PRJ1"));
        }

        #endregion

        #region GetCumulativeDataAsync

        [Fact]
        public async Task GetCumulativeDataAsync_WithData_ReturnsMappedDtoList()
        {
            var entities = new List<ProjectProfile>
            {
                new() { MonthNo = 1.0, Profile = 100m, Cost = 200m },
                new() { MonthNo = 2.0, Profile = 250m, Cost = 500m }
            };

            _mockRepository.GetCumulativeDataAsync("PRJ1").Returns(entities);

            var result = await _sut.GetCumulativeDataAsync("PRJ1");

            result.Should().HaveCount(2);
            result[0].MonthNo.Should().Be(1);
            result[0].CumulativeProfile.Should().Be(100m);
            result[0].CumulativeCost.Should().Be(200m);
            result[1].MonthNo.Should().Be(2);
            result[1].CumulativeProfile.Should().Be(250m);
            result[1].CumulativeCost.Should().Be(500m);
            await _mockRepository.Received(1).GetCumulativeDataAsync("PRJ1");
        }

        [Fact]
        public async Task GetCumulativeDataAsync_EmptyResult_ReturnsEmptyList()
        {
            _mockRepository.GetCumulativeDataAsync("PRJ_NONE").Returns(new List<ProjectProfile>());

            var result = await _sut.GetCumulativeDataAsync("PRJ_NONE");

            result.Should().BeEmpty();
            await _mockRepository.Received(1).GetCumulativeDataAsync("PRJ_NONE");
        }

        [Fact]
        public async Task GetCumulativeDataAsync_MapsMonthNoFromDoubleToInt()
        {
            var entities = new List<ProjectProfile>
            {
                new() { MonthNo = 5.0, Profile = 400m, Cost = 800m }
            };

            _mockRepository.GetCumulativeDataAsync("PRJ1").Returns(entities);

            var result = await _sut.GetCumulativeDataAsync("PRJ1");

            result.Should().ContainSingle();
            result[0].MonthNo.Should().Be(5);
            result[0].Should().BeOfType<ProjectProfileCumulativeDto>();
        }

        [Fact]
        public async Task GetCumulativeDataAsync_NullProfileAndCost_MapsNullValues()
        {
            var entities = new List<ProjectProfile>
            {
                new() { MonthNo = 2.0, Profile = null, Cost = null }
            };

            _mockRepository.GetCumulativeDataAsync("PRJ1").Returns(entities);

            var result = await _sut.GetCumulativeDataAsync("PRJ1");

            result.Should().ContainSingle();
            result[0].CumulativeProfile.Should().BeNull();
            result[0].CumulativeCost.Should().BeNull();
        }

        [Fact]
        public async Task GetCumulativeDataAsync_RepositoryThrows_PropagatesException()
        {
            _mockRepository.GetCumulativeDataAsync("PRJ1").ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.GetCumulativeDataAsync("PRJ1"));
        }

        #endregion
    }
}
