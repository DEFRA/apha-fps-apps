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

        #region GetWorkGroupsByProfitCentre

        [Fact]
        public async Task GetWorkGroupsByProfitCentre_HappyPath_ReturnsOkWithMappedPage()
        {
            // Arrange
            var query = new QueryParameters<string>();
            const string profitCentre = "PC001";
            var pagedResult = new PaginatedResult<WorkGroupDto>
            {
                Data = new List<WorkGroupDto> { new() { WorkGroupName = "WG1", ProfitCentre = profitCentre } }
            };
            var mapped = new PaginationRes<WorkGroupRes>
            {
                Data = new List<WorkGroupRes> { new() { WorkGroupName = "WG1", ProfitCentre = profitCentre } }
            };

            _serviceMock.GetWorkGroupsByProfitCentreAsync(query, profitCentre).Returns(pagedResult);
            _mapperMock.Map<PaginationRes<WorkGroupRes>>(pagedResult).Returns(mapped);

            // Act
            var result = await _controller.GetWorkGroupsByProfitCentre(query, profitCentre);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
            await _serviceMock.Received(1).GetWorkGroupsByProfitCentreAsync(query, profitCentre);
        }

        [Fact]
        public async Task GetWorkGroupsByProfitCentre_EmptyPage_ReturnsOkWithEmptyData()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var pagedResult = new PaginatedResult<WorkGroupDto>();
            var mapped = new PaginationRes<WorkGroupRes>();

            _serviceMock.GetWorkGroupsByProfitCentreAsync(query, Arg.Any<string>()).Returns(pagedResult);
            _mapperMock.Map<PaginationRes<WorkGroupRes>>(pagedResult).Returns(mapped);

            // Act
            var result = await _controller.GetWorkGroupsByProfitCentre(query, "PC001");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task GetWorkGroupsByProfitCentre_ServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.GetWorkGroupsByProfitCentreAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _controller.GetWorkGroupsByProfitCentre(new QueryParameters<string>(), "PC001"));
        }

        #endregion

        #region SetSendEmailForProfitCentreWorkGroupsAsync

        [Fact]
        public async Task SetSendEmailForProfitCentreWorkGroupsAsync_WithValidRequest_ReturnsOkWithTrue()
        {
            // Arrange
            var request = new UpdateSendEmailFlagReq { ProfitCentre = "PC001", SendEmail = 1 };
            _serviceMock.SetSendEmailForProfitCentreWorkGroupsAsync("PC001", 1).Returns(true);

            // Act
            var result = await _controller.SetSendEmailForProfitCentreWorkGroupsAsync(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
            await _serviceMock.Received(1).SetSendEmailForProfitCentreWorkGroupsAsync("PC001", 1);
        }

        [Fact]
        public async Task SetSendEmailForProfitCentreWorkGroupsAsync_WithNullProfitCentre_ReturnsBadRequest()
        {
            // Arrange
            var request = new UpdateSendEmailFlagReq { ProfitCentre = null, SendEmail = 1 };

            // Act
            var result = await _controller.SetSendEmailForProfitCentreWorkGroupsAsync(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            await _serviceMock.DidNotReceive()
                .SetSendEmailForProfitCentreWorkGroupsAsync(Arg.Any<string>(), Arg.Any<short>());
        }

        [Fact]
        public async Task SetSendEmailForProfitCentreWorkGroupsAsync_WithWhitespaceProfitCentre_ReturnsBadRequest()
        {
            // Arrange
            var request = new UpdateSendEmailFlagReq { ProfitCentre = "   ", SendEmail = 0 };

            // Act
            var result = await _controller.SetSendEmailForProfitCentreWorkGroupsAsync(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task SetSendEmailForProfitCentreWorkGroupsAsync_ServiceReturnsFalse_ReturnsOkWithFalse()
        {
            // Arrange
            var request = new UpdateSendEmailFlagReq { ProfitCentre = "PC001", SendEmail = 0 };
            _serviceMock.SetSendEmailForProfitCentreWorkGroupsAsync("PC001", 0).Returns(false);

            // Act
            var result = await _controller.SetSendEmailForProfitCentreWorkGroupsAsync(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.False((bool)okResult.Value!);
        }

        #endregion

        #region SetSendEmailForAllWorkGroupsAsync

        [Fact]
        public async Task SetSendEmailForAllWorkGroupsAsync_WithFlagOne_ReturnsOkWithTrue()
        {
            // Arrange
            var request = new UpdateSendEmailFlagReq { SendEmail = 1 };
            _serviceMock.SetSendEmailForAllWorkGroupsAsync(1).Returns(true);

            // Act
            var result = await _controller.SetSendEmailForAllWorkGroupsAsync(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
            await _serviceMock.Received(1).SetSendEmailForAllWorkGroupsAsync(1);
        }

        [Fact]
        public async Task SetSendEmailForAllWorkGroupsAsync_WithFlagZero_ReturnsOkWithTrue()
        {
            // Arrange
            var request = new UpdateSendEmailFlagReq { SendEmail = 0 };
            _serviceMock.SetSendEmailForAllWorkGroupsAsync(0).Returns(true);

            // Act
            var result = await _controller.SetSendEmailForAllWorkGroupsAsync(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
            await _serviceMock.Received(1).SetSendEmailForAllWorkGroupsAsync(0);
        }

        [Fact]
        public async Task SetSendEmailForAllWorkGroupsAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.SetSendEmailForAllWorkGroupsAsync(Arg.Any<short>())
                .ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _controller.SetSendEmailForAllWorkGroupsAsync(new UpdateSendEmailFlagReq { SendEmail = 0 }));
        }

        #endregion

        #region UpdateWorkGroupEmail

        [Fact]
        public async Task UpdateWorkGroupEmail_WithValidRequest_ReturnsOkWithTrue()
        {
            // Arrange
            const string workGroupName = "WG1";
            var request = new UpdateWorkGroupEmailReq
            {
                WorkGroupName = workGroupName,
                SendEmail = 1,
                EmailRecipient = "test@test.com"
            };
            _serviceMock.UpdateWorkGroupEmailAsync(workGroupName, 1, "test@test.com").Returns(true);

            // Act
            var result = await _controller.UpdateWorkGroupEmail(workGroupName, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
            await _serviceMock.Received(1).UpdateWorkGroupEmailAsync(workGroupName, 1, "test@test.com");
        }

        [Fact]
        public async Task UpdateWorkGroupEmail_WithNullEmailRecipient_ReturnsOkWithTrue()
        {
            // Arrange
            const string workGroupName = "WG1";
            var request = new UpdateWorkGroupEmailReq { WorkGroupName = workGroupName, SendEmail = 0, EmailRecipient = null };
            _serviceMock.UpdateWorkGroupEmailAsync(workGroupName, 0, null).Returns(true);

            // Act
            var result = await _controller.UpdateWorkGroupEmail(workGroupName, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
        }

        [Fact]
        public async Task UpdateWorkGroupEmail_WithEmptyWorkGroupName_ReturnsBadRequest()
        {
            // Arrange
            var request = new UpdateWorkGroupEmailReq { SendEmail = 1 };

            // Act
            var result = await _controller.UpdateWorkGroupEmail("", request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            await _serviceMock.DidNotReceive()
                .UpdateWorkGroupEmailAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<string>());
        }

        [Fact]
        public async Task UpdateWorkGroupEmail_ServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.UpdateWorkGroupEmailAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<string?>())
                .ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _controller.UpdateWorkGroupEmail("WG1", new UpdateWorkGroupEmailReq { WorkGroupName = "WG1", SendEmail = 1 }));
        }

        [Fact]
        public async Task UpdateWorkGroupEmail_BodyWorkGroupNameMismatch_ReturnsBadRequest()
        {
            // Arrange
            const string routeName = "WG1";
            var request = new UpdateWorkGroupEmailReq { WorkGroupName = "WG_OTHER", SendEmail = 1 };

            // Act
            var result = await _controller.UpdateWorkGroupEmail(routeName, request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            await _serviceMock.DidNotReceive()
                .UpdateWorkGroupEmailAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<string>());
        }

        #endregion
    }
}
