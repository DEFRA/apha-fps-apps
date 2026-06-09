using Apha.Common.Contracts.FPS;
using Apha.FPS.Api.Controllers;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPS.Api.UnitTests.Controller.JobCodeControllerTest
{
    public class JobCodeControllerTest
    {
        private readonly IJobCodeService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly JobCodeController _controller;

        public JobCodeControllerTest()
        {
            _serviceMock = Substitute.For<IJobCodeService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new JobCodeController(_serviceMock, _mapperMock);
        }

        #region GetZtCodesAsync

        [Fact]
        public async Task GetZtCodesAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var serviceResult = new List<ZtJobCodeDto>
            {
                new() { JobCode = "ZT001", Description = "ZT Project 1" },
                new() { JobCode = "ZT002", Description = "ZT Project 2" }
            };
            var mappedResult = new List<ZtJobCodeRes>
            {
                new() { JobCode = "ZT001", Description = "ZT Project 1" },
                new() { JobCode = "ZT002", Description = "ZT Project 2" }
            };

            _serviceMock.GetZtCodeLookupAsync().Returns(serviceResult);
            _mapperMock.Map<IEnumerable<ZtJobCodeRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetZtCodesAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);
        }

        [Fact]
        public async Task GetZtCodesAsync_EmptyResult_ReturnsOkWithEmptyList()
        {
            // Arrange
            var serviceResult = new List<ZtJobCodeDto>();
            var mappedResult = new List<ZtJobCodeRes>();

            _serviceMock.GetZtCodeLookupAsync().Returns(serviceResult);
            _mapperMock.Map<IEnumerable<ZtJobCodeRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetZtCodesAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);
        }

        [Fact]
        public async Task GetZtCodesAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.GetZtCodeLookupAsync().Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetZtCodesAsync());
        }

        [Fact]
        public async Task GetZtCodesAsync_CallsServiceOnce()
        {
            // Arrange
            _serviceMock.GetZtCodeLookupAsync().Returns(new List<ZtJobCodeDto>());
            _mapperMock.Map<IEnumerable<ZtJobCodeRes>>(Arg.Any<IEnumerable<ZtJobCodeDto>>()).Returns(new List<ZtJobCodeRes>());

            // Act
            await _controller.GetZtCodesAsync();

            // Assert
            await _serviceMock.Received(1).GetZtCodeLookupAsync();
        }

        #endregion

        #region GetAllJobCodesAsync

        [Fact]
        public async Task GetAllJobCodesAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var serviceResult = new List<JobCodeDto>
            {
                new() { JobCodeId = "JC001", Jobcodename = "Field Operations" },
                new() { JobCodeId = "JC002", Jobcodename = "Lab Analysis" }
            };
            var mappedResult = new List<JobCodeRes>
            {
                new() { JobCodeId = "JC001", JobCodeName = "Field Operations" },
                new() { JobCodeId = "JC002", JobCodeName = "Lab Analysis" }
            };

            _serviceMock.GetJobCodeListAsync().Returns(serviceResult);
            _mapperMock.Map<IEnumerable<JobCodeRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetAllJobCodesAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);
        }

        [Fact]
        public async Task GetAllJobCodesAsync_EmptyResult_ReturnsOkWithEmptyList()
        {
            // Arrange
            var serviceResult = new List<JobCodeDto>();
            var mappedResult = new List<JobCodeRes>();

            _serviceMock.GetJobCodeListAsync().Returns(serviceResult);
            _mapperMock.Map<IEnumerable<JobCodeRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetAllJobCodesAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);
        }

        [Fact]
        public async Task GetAllJobCodesAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.GetJobCodeListAsync().Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAllJobCodesAsync());
        }

        [Fact]
        public async Task GetAllJobCodesAsync_CallsServiceOnce()
        {
            // Arrange
            _serviceMock.GetJobCodeListAsync().Returns(new List<JobCodeDto>());
            _mapperMock.Map<IEnumerable<JobCodeRes>>(Arg.Any<IEnumerable<JobCodeDto>>()).Returns(new List<JobCodeRes>());

            // Act
            await _controller.GetAllJobCodesAsync();

            // Assert
            await _serviceMock.Received(1).GetJobCodeListAsync();
        }

        #endregion
    }
}
