using Apha.Common.Contracts.PACT;
using Apha.PACT.Api.Controllers;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PACT.Api.UnitTests.Controller.WorkGroupControllerTest
{
    public class WorkGroupControllerTests
    {
        private readonly IWorkGroupService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly WorkGroupController _controller;

        public WorkGroupControllerTests()
        {
            _serviceMock = Substitute.For<IWorkGroupService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new WorkGroupController(_serviceMock, _mapperMock);
        }

        #region GetAll

        [Fact]
        public async Task GetAll_HappyPath_ReturnsOk()
        {
            var dtos = new List<WorkGroupDto> { new WorkGroupDto { WorkGroupName = "WG1" } };
            var mapped = new List<WorkGroupRes> { new WorkGroupRes { WorkGroupName = "WG1" } };

            _serviceMock.GetAllWorkGroupsAsync().Returns(dtos);
            _mapperMock.Map<IEnumerable<WorkGroupRes>>(dtos).Returns(mapped);

            var result = await _controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task GetAll_EmptyList_ReturnsOkWithEmptyCollection()
        {
            var dtos = new List<WorkGroupDto>();
            var mapped = new List<WorkGroupRes>();

            _serviceMock.GetAllWorkGroupsAsync().Returns(dtos);
            _mapperMock.Map<IEnumerable<WorkGroupRes>>(dtos).Returns(mapped);

            var result = await _controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task GetAll_ServiceThrows_PropagatesException()
        {
            _serviceMock.GetAllWorkGroupsAsync().Throws(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetAll());
        }

        #endregion
    }
}
