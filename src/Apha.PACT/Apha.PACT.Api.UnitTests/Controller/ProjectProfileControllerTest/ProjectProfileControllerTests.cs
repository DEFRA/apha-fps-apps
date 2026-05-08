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

        #region GetProfileGraph

        [Fact]
        public async Task GetProfileGraph_WithData_ReturnsOkWithMappedList()
        {
            var dtos = new List<ProjectProfileGraphDto>
            {
                new() { MonthNo = 1, Profile = 100m, TotalCost = 200m },
                new() { MonthNo = 2, Profile = 150m, TotalCost = 300m }
            };
            var response = new List<ProjectProfileGraphRes>
            {
                new() { MonthNo = 1, Profile = 100m, TotalCost = 200m },
                new() { MonthNo = 2, Profile = 150m, TotalCost = 300m }
            };

            _serviceMock.GetProfileGraphDataAsync("PRJ1").Returns(dtos);
            _mapperMock.Map<IList<ProjectProfileGraphRes>>(dtos).Returns(response);

            var result = await _controller.GetProfileGraph("PRJ1");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, okResult.Value);
            await _serviceMock.Received(1).GetProfileGraphDataAsync("PRJ1");
            _mapperMock.Received(1).Map<IList<ProjectProfileGraphRes>>(dtos);
        }

        [Fact]
        public async Task GetProfileGraph_EmptyResult_ReturnsOkWithEmptyList()
        {
            var dtos = new List<ProjectProfileGraphDto>();
            var response = new List<ProjectProfileGraphRes>();

            _serviceMock.GetProfileGraphDataAsync("PRJ_NONE").Returns(dtos);
            _mapperMock.Map<IList<ProjectProfileGraphRes>>(dtos).Returns(response);

            var result = await _controller.GetProfileGraph("PRJ_NONE");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, okResult.Value);
        }

        [Fact]
        public async Task GetProfileGraph_ServiceThrows_PropagatesException()
        {
            _serviceMock.GetProfileGraphDataAsync("PRJ1").ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetProfileGraph("PRJ1"));
        }

        #endregion

        #region GetCumulativeGraph

        [Fact]
        public async Task GetCumulativeGraph_WithData_ReturnsOkWithMappedList()
        {
            var dtos = new List<ProjectProfileCumulativeGraphDto>
            {
                new() { MonthNo = 1, CumulativeProfile = 100m, CumulativeCost = 200m },
                new() { MonthNo = 2, CumulativeProfile = 250m, CumulativeCost = 500m }
            };
            var response = new List<ProjectProfileCumulativeGraphRes>
            {
                new() { MonthNo = 1, CumulativeProfile = 100m, CumulativeCost = 200m },
                new() { MonthNo = 2, CumulativeProfile = 250m, CumulativeCost = 500m }
            };

            _serviceMock.GetCumulativeGraphDataAsync("PRJ1").Returns(dtos);
            _mapperMock.Map<IList<ProjectProfileCumulativeGraphRes>>(dtos).Returns(response);

            var result = await _controller.GetCumulativeGraph("PRJ1");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, okResult.Value);
            await _serviceMock.Received(1).GetCumulativeGraphDataAsync("PRJ1");
            _mapperMock.Received(1).Map<IList<ProjectProfileCumulativeGraphRes>>(dtos);
        }

        [Fact]
        public async Task GetCumulativeGraph_EmptyResult_ReturnsOkWithEmptyList()
        {
            var dtos = new List<ProjectProfileCumulativeGraphDto>();
            var response = new List<ProjectProfileCumulativeGraphRes>();

            _serviceMock.GetCumulativeGraphDataAsync("PRJ_NONE").Returns(dtos);
            _mapperMock.Map<IList<ProjectProfileCumulativeGraphRes>>(dtos).Returns(response);

            var result = await _controller.GetCumulativeGraph("PRJ_NONE");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, okResult.Value);
        }

        [Fact]
        public async Task GetCumulativeGraph_ServiceThrows_PropagatesException()
        {
            _serviceMock.GetCumulativeGraphDataAsync("PRJ1").ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetCumulativeGraph("PRJ1"));
        }

        #endregion
    }
}
