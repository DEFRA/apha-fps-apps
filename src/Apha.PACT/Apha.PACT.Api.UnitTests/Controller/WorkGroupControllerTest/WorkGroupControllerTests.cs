using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.PACT.Api.Controllers;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
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
        public async Task GetAll_HappyPath_ReturnsOkWithMappedResult()
        {
            // Arrange
            var dtos = new List<WorkGroupDto> { new() { WorkGroupName = "WG1", ProfitCentre = "PC1" } };
            var mapped = new List<WorkGroupRes> { new() { WorkGroupName = "WG1", ProfitCentre = "PC1" } };

            _serviceMock.GetAllWorkGroupsAsync().Returns(dtos);
            _mapperMock.Map<IEnumerable<WorkGroupRes>>(dtos).Returns(mapped);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
            await _serviceMock.Received(1).GetAllWorkGroupsAsync();
            _mapperMock.Received(1).Map<IEnumerable<WorkGroupRes>>(dtos);
        }

        [Fact]
        public async Task GetAll_EmptyList_ReturnsOkWithEmptyCollection()
        {
            // Arrange
            var dtos = new List<WorkGroupDto>();
            var mapped = new List<WorkGroupRes>();

            _serviceMock.GetAllWorkGroupsAsync().Returns(dtos);
            _mapperMock.Map<IEnumerable<WorkGroupRes>>(dtos).Returns(mapped);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<WorkGroupRes>>(okResult.Value);
            Assert.Empty(returnValue);
        }

        [Fact]
        public async Task GetAll_ServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.GetAllWorkGroupsAsync().ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAll());
        }

        #endregion

        #region GetPagedWorkGroupTimeCodes

        [Fact]
        public async Task GetPagedWorkGroupTimeCodes_WithData_ReturnsOkWithMappedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<WorkGroupTimeCodeDto>
            {
                Data = [new() { PACTStaffID = "S1", TimeCode = "TC1" }]
            };
            var mapped = new PaginationRes<WorkGroupTimeCodeRes>
            {
                Data = [new() { PACTStaffID = "S1", TimeCode = "TC1" }]
            };

            _serviceMock.GetWorkGroupTimeCodeAsync(query, "WG1", 3).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<WorkGroupTimeCodeRes>>(serviceResult).Returns(mapped);

            // Act
            var result = await _controller.GetPagedWorkGroupTimeCodes(query, "WG1", 3);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
            await _serviceMock.Received(1).GetWorkGroupTimeCodeAsync(query, "WG1", 3);
            _mapperMock.Received(1).Map<PaginationRes<WorkGroupTimeCodeRes>>(serviceResult);
        }

        [Fact]
        public async Task GetPagedWorkGroupTimeCodes_NullWorkGroupAndMonth_ReturnsOk()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<WorkGroupTimeCodeDto> { Data = [] };
            var mapped = new PaginationRes<WorkGroupTimeCodeRes> { Data = [] };

            _serviceMock.GetWorkGroupTimeCodeAsync(query, "WG2", 1).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<WorkGroupTimeCodeRes>>(serviceResult).Returns(mapped);

            // Act
            var result = await _controller.GetPagedWorkGroupTimeCodes(query, "WG2", 1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
            await _serviceMock.Received(1).GetWorkGroupTimeCodeAsync(query, "WG2", 1);
        }

        [Fact]
        public async Task GetPagedWorkGroupTimeCodes_EmptyResult_ReturnsOkWithEmptyData()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<WorkGroupTimeCodeDto> { Data = [] };
            var mapped = new PaginationRes<WorkGroupTimeCodeRes> { Data = [] };

            _serviceMock.GetWorkGroupTimeCodeAsync(query, "WG2", 1).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<WorkGroupTimeCodeRes>>(serviceResult).Returns(mapped);

            // Act
            var result = await _controller.GetPagedWorkGroupTimeCodes(query, "WG2", 1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PaginationRes<WorkGroupTimeCodeRes>>(okResult.Value);
            Assert.Empty(returnValue.Data);
        }

        [Fact]
        public async Task GetPagedWorkGroupTimeCodes_ServiceThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            _serviceMock.GetWorkGroupTimeCodeAsync(query, Arg.Any<string>(), Arg.Any<int>())
                        .ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetPagedWorkGroupTimeCodes(query, "WG1", 1));
        }

        #endregion

        #region GetPagedWorkGroupValidTimeCodes

        [Fact]
        public async Task GetPagedWorkGroupValidTimeCodes_WithData_ReturnsOkWithMappedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<WorkGroupValidTimeCodeDto>
            {
                Data = [new() { TimeCode = "TC1", ParentProject = "P001", WorkGroup = "WG1" }]
            };
            var mapped = new PaginationRes<WorkGroupValidTimeCodeRes>
            {
                Data = [new() { TimeCode = "TC1", ParentProject = "P001", WorkGroup = "WG1" }]
            };

            _serviceMock.GetWorkGroupValidTimeCodeAsync(query, "WG1").Returns(serviceResult);
            _mapperMock.Map<PaginationRes<WorkGroupValidTimeCodeRes>>(serviceResult).Returns(mapped);

            // Act
            var result = await _controller.GetPagedWorkGroupValidTimeCodes(query, "WG1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
            await _serviceMock.Received(1).GetWorkGroupValidTimeCodeAsync(query, "WG1");
            _mapperMock.Received(1).Map<PaginationRes<WorkGroupValidTimeCodeRes>>(serviceResult);
        }

        [Fact]
        public async Task GetPagedWorkGroupValidTimeCodes_EmptyResult_ReturnsOkWithEmptyData()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<WorkGroupValidTimeCodeDto> { Data = [] };
            var mapped = new PaginationRes<WorkGroupValidTimeCodeRes> { Data = [] };

            _serviceMock.GetWorkGroupValidTimeCodeAsync(query, "WG1").Returns(serviceResult);
            _mapperMock.Map<PaginationRes<WorkGroupValidTimeCodeRes>>(serviceResult).Returns(mapped);

            // Act
            var result = await _controller.GetPagedWorkGroupValidTimeCodes(query, "WG1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PaginationRes<WorkGroupValidTimeCodeRes>>(okResult.Value);
            Assert.Empty(returnValue.Data);
        }

        [Fact]
        public async Task GetPagedWorkGroupValidTimeCodes_ServiceThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            _serviceMock.GetWorkGroupValidTimeCodeAsync(query, Arg.Any<string>())
                        .ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetPagedWorkGroupValidTimeCodes(query, "WG1"));
        }

        [Fact]
        public async Task GetPagedWorkGroupValidTimeCodes_MapperThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<WorkGroupValidTimeCodeDto> { Data = [] };

            _serviceMock.GetWorkGroupValidTimeCodeAsync(query, "WG1").Returns(serviceResult);
            _mapperMock.Map<PaginationRes<WorkGroupValidTimeCodeRes>>(serviceResult)
                       .Throws(new AutoMapperMappingException("Mapping error"));

            // Act & Assert
            await Assert.ThrowsAsync<AutoMapperMappingException>(() => _controller.GetPagedWorkGroupValidTimeCodes(query, "WG1"));
        }

        #endregion
    }
}
