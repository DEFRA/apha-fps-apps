using Apha.Common.Contracts.PACT;
using Apha.PACT.Api.Controllers;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PACT.Api.UnitTests.Controller.ProjectProfileControllerTest
{
    public class ProjectProfileControllerTests
    {
        private readonly IProjectProfileService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly ProjectProfileController _controller;

        public ProjectProfileControllerTests()
        {
            _serviceMock = Substitute.For<IProjectProfileService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new ProjectProfileController(_serviceMock, _mapperMock);
        }

        #region GetProfile

        [Fact]
        public async Task GetProfile_WithData_ReturnsOkWithMappedList()
        {
            var dtos = new List<ProjectProfileDto>
            {
                new() { MonthNo = 1, Profile = 100m, TotalCost = 200m },
                new() { MonthNo = 2, Profile = 150m, TotalCost = 300m }
            };
            var response = new List<ProjectProfileRes>
            {
                new() { MonthNo = 1, Profile = 100m, TotalCost = 200m },
                new() { MonthNo = 2, Profile = 150m, TotalCost = 300m }
            };

            _serviceMock.GetProfileDataAsync("PRJ1").Returns(dtos);
            _mapperMock.Map<IList<ProjectProfileRes>>(dtos).Returns(response);

            var result = await _controller.GetProfile("PRJ1");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, okResult.Value);
            await _serviceMock.Received(1).GetProfileDataAsync("PRJ1");
            _mapperMock.Received(1).Map<IList<ProjectProfileRes>>(dtos);
        }

        [Fact]
        public async Task GetProfile_EmptyResult_ReturnsOkWithEmptyList()
        {
            var dtos = new List<ProjectProfileDto>();
            var response = new List<ProjectProfileRes>();

            _serviceMock.GetProfileDataAsync("PRJ_NONE").Returns(dtos);
            _mapperMock.Map<IList<ProjectProfileRes>>(dtos).Returns(response);

            var result = await _controller.GetProfile("PRJ_NONE");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, okResult.Value);
        }

        [Fact]
        public async Task GetProfile_ServiceThrows_PropagatesException()
        {
            _serviceMock.GetProfileDataAsync("PRJ1").ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetProfile("PRJ1"));
        }

        #endregion

        #region GetCumulative

        [Fact]
        public async Task GetCumulative_WithData_ReturnsOkWithMappedList()
        {
            var dtos = new List<ProjectProfileCumulativeDto>
            {
                new() { MonthNo = 1, CumulativeProfile = 100m, CumulativeCost = 200m },
                new() { MonthNo = 2, CumulativeProfile = 250m, CumulativeCost = 500m }
            };
            var response = new List<ProjectProfileCumulativeRes>
            {
                new() { MonthNo = 1, CumulativeProfile = 100m, CumulativeCost = 200m },
                new() { MonthNo = 2, CumulativeProfile = 250m, CumulativeCost = 500m }
            };

            _serviceMock.GetCumulativeDataAsync("PRJ1").Returns(dtos);
            _mapperMock.Map<IList<ProjectProfileCumulativeRes>>(dtos).Returns(response);

            var result = await _controller.GetCumulative("PRJ1");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, okResult.Value);
            await _serviceMock.Received(1).GetCumulativeDataAsync("PRJ1");
            _mapperMock.Received(1).Map<IList<ProjectProfileCumulativeRes>>(dtos);
        }

        [Fact]
        public async Task GetCumulative_EmptyResult_ReturnsOkWithEmptyList()
        {
            var dtos = new List<ProjectProfileCumulativeDto>();
            var response = new List<ProjectProfileCumulativeRes>();

            _serviceMock.GetCumulativeDataAsync("PRJ_NONE").Returns(dtos);
            _mapperMock.Map<IList<ProjectProfileCumulativeRes>>(dtos).Returns(response);

            var result = await _controller.GetCumulative("PRJ_NONE");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, okResult.Value);
        }

        [Fact]
        public async Task GetCumulative_ServiceThrows_PropagatesException()
        {
            _serviceMock.GetCumulativeDataAsync("PRJ1").ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetCumulative("PRJ1"));
        }

        #endregion
    }
}
