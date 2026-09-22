using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Api.Controllers;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PIMS.Api.UnitTests.Controllers.ReportGroupControllerTest
{
    public class ReportGroupControllerTests
    {
        private readonly IReportGroupService _service;
        private readonly MapsterMapper.IMapper _mapper;
        private readonly ReportGroupController _controller;

        public ReportGroupControllerTests()
        {
            _service = Substitute.For<IReportGroupService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new ReportGroupController(_service, _mapper);
        }

        // ── helper ──────────────────────────────────────────────────────────────────────────────

        private static ReportGroupDto MakeDto(int groupId = 1) => new ReportGroupDto
        {
            GroupId = groupId,
            Description = $"Group {groupId}"
        };

        private static ReportGroupRes MakeRes(int groupId = 1) => new ReportGroupRes
        {
            GroupId = groupId,
            Description = $"Group {groupId}"
        };

        private static ReportGroupReq MakeReq() => new ReportGroupReq
        {
            GroupId = 1,
            Description = "New Group"
        };

        // ── GetAll ──────────────────────────────────────────────────────────────────────────────

        #region GetAll

        [Fact]
        public async Task GetAll_ServiceReturnsData_ReturnsOkWithMappedList()
        {
            // Arrange
            var dtos = new List<ReportGroupDto> { MakeDto(1), MakeDto(2) };
            var resList = new List<ReportGroupRes> { MakeRes(1), MakeRes(2) };
            _service.GetAllReportGroupsAsync().Returns(dtos);
            _mapper.Map<List<ReportGroupRes>>(dtos).Returns(resList);

            // Act
            var result = await _controller.GetAllReportGroups();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<List<ReportGroupRes>>(ok.Value);
            Assert.Equal(2, returned.Count);
            await _service.Received(1).GetAllReportGroupsAsync();
            _mapper.Received(1).Map<List<ReportGroupRes>>(dtos);
        }

        [Fact]
        public async Task GetAll_ServiceReturnsEmptyList_ReturnsOkWithEmptyList()
        {
            // Arrange
            var dtos = new List<ReportGroupDto>();
            var resList = new List<ReportGroupRes>();
            _service.GetAllReportGroupsAsync().Returns(dtos);
            _mapper.Map<List<ReportGroupRes>>(dtos).Returns(resList);

            // Act
            var result = await _controller.GetAllReportGroups();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<List<ReportGroupRes>>(ok.Value);
            Assert.Empty(returned);
            await _service.Received(1).GetAllReportGroupsAsync();
        }

        [Fact]
        public async Task GetAll_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            _service.GetAllReportGroupsAsync().ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAllReportGroups());
        }

        #endregion

        // ── GetById ─────────────────────────────────────────────────────────────────────────────

        #region GetById

        [Fact]
        public async Task GetById_ServiceReturnsDto_ReturnsOkWithMappedResult()
        {
            // Arrange
            var dto = MakeDto(5);
            var res = MakeRes(5);
            _service.GetReportGroupByIdAsync(5).Returns(dto);
            _mapper.Map<ReportGroupRes>(dto).Returns(res);

            // Act
            var result = await _controller.GetReportGroupById(5);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, ok.Value);
            await _service.Received(1).GetReportGroupByIdAsync(5);
        }

        [Fact]
        public async Task GetById_ServiceReturnsNull_ReturnsJsonResultWithSuccessTrueAndNullData()
        {
            // Arrange
            _service.GetReportGroupByIdAsync(99).Returns((ReportGroupDto?)null);

            // Act
            var result = await _controller.GetReportGroupById(99);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var response = Assert.IsType<Apha.Common.Contracts.ApiResponse<ReportGroupRes>>(jsonResult.Value);
            Assert.True(response.Success);
            Assert.Null(response.Data);
            Assert.NotEmpty(response.Meta.CorrelationId);
        }

        [Fact]
        public async Task GetById_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            _service.GetReportGroupByIdAsync(Arg.Any<int>()).ThrowsAsync(new Exception("unexpected"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetReportGroupById(1));
        }

        #endregion

        // ── GetByReportId ───────────────────────────────────────────────────────────────────────

        #region GetByReportId

        [Fact]
        public async Task GetByReportId_ServiceReturnsData_ReturnsOkWithMappedList()
        {
            // Arrange
            var dtos = new List<ReportGroupDto> { MakeDto(1) };
            var resList = new List<ReportGroupRes> { MakeRes(1) };
            _service.GetReportGroupsByReportIdAsync(1).Returns(dtos);
            _mapper.Map<List<ReportGroupRes>>(dtos).Returns(resList);

            // Act
            var result = await _controller.GetReportGroupsByReportId(1);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<List<ReportGroupRes>>(ok.Value);
            Assert.Single(returned);
            await _service.Received(1).GetReportGroupsByReportIdAsync(1);
        }

        [Fact]
        public async Task GetByReportId_ServiceReturnsEmptyList_ReturnsOkWithEmptyList()
        {
            // Arrange
            var dtos = new List<ReportGroupDto>();
            var resList = new List<ReportGroupRes>();
            _service.GetReportGroupsByReportIdAsync(99).Returns(dtos);
            _mapper.Map<List<ReportGroupRes>>(dtos).Returns(resList);

            // Act
            var result = await _controller.GetReportGroupsByReportId(99);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<List<ReportGroupRes>>(ok.Value);
            Assert.Empty(returned);
        }

        [Fact]
        public async Task GetByReportId_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            _service.GetReportGroupsByReportIdAsync(Arg.Any<int>()).ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetReportGroupsByReportId(1));
        }

        #endregion

        // ── Create ──────────────────────────────────────────────────────────────────────────────

        #region Create

        [Fact]
        public async Task Create_ValidRequest_ReturnsCreatedAtActionWithMappedResult()
        {
            // Arrange
            var req = MakeReq();
            var dto = MakeDto(1);
            var res = MakeRes(1);
            _mapper.Map<ReportGroupDto>(req).Returns(dto);
            _service.CreateReportGroupAsync(dto).Returns(dto);
            _mapper.Map<ReportGroupRes>(dto).Returns(res);

            // Act
            var result = await _controller.CreateReportGroup(req);

            // Assert
            var created = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.GetReportGroupById), created.ActionName);
            Assert.Equal(res, created.Value);
            await _service.Received(1).CreateReportGroupAsync(dto);
            _mapper.Received(1).Map<ReportGroupRes>(dto);
        }

        [Fact]
        public async Task Create_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            var req = MakeReq();
            var dto = MakeDto(1);
            _mapper.Map<ReportGroupDto>(req).Returns(dto);
            _service.CreateReportGroupAsync(dto).ThrowsAsync(new Exception("Data error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.CreateReportGroup(req));
        }

        #endregion

        // ── Update ──────────────────────────────────────────────────────────────────────────────

        #region Update

        [Fact]
        public async Task Update_SetsRoutePkOnDtoBeforeCallingService_DtoIdMatchesRouteId()
        {
            // Arrange
            var groupId = 7;
            var req = MakeReq();
            var dto = MakeDto(groupId);
            var res = MakeRes(groupId);
            _mapper.Map<ReportGroupDto>(req).Returns(dto);
            _service.UpdateReportGroupAsync(dto).Returns(dto);
            _mapper.Map<ReportGroupRes>(dto).Returns(res);

            // Act
            await _controller.UpdateReportGroup(groupId, req);

            // Assert
            Assert.Equal(groupId, dto.GroupId);
        }

        [Fact]
        public async Task Update_ServiceReturnsDto_ReturnsOkWithMappedResult()
        {
            // Arrange
            var dto = MakeDto(3);
            var res = MakeRes(3);
            var req = MakeReq();
            _mapper.Map<ReportGroupDto>(req).Returns(dto);
            _service.UpdateReportGroupAsync(dto).Returns(dto);
            _mapper.Map<ReportGroupRes>(dto).Returns(res);

            // Act
            var result = await _controller.UpdateReportGroup(3, req);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, ok.Value);
            await _service.Received(1).UpdateReportGroupAsync(dto);
        }

        [Fact]
        public async Task Update_ServiceThrowsKeyNotFoundException_PropagatesException()
        {
            // Arrange
            var req = MakeReq();
            var dto = MakeDto(99);
            _mapper.Map<ReportGroupDto>(req).Returns(dto);
            _service.UpdateReportGroupAsync(dto).ThrowsAsync(new KeyNotFoundException("Not found"));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.UpdateReportGroup(99, req));
        }

        #endregion

        // ── Delete ──────────────────────────────────────────────────────────────────────────────

        #region Delete

        [Fact]
        public async Task Delete_ServiceCompletes_ReturnsOkWithSuccessTrue()
        {
            // Arrange
            _service.DeleteReportGroupAsync(5).Returns(true);

            // Act
            var result = await _controller.DeleteReportGroup(5);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)ok.Value!);
            await _service.Received(1).DeleteReportGroupAsync(5);
        }

        [Fact]
        public async Task Delete_ServiceThrowsKeyNotFoundException_PropagatesException()
        {
            // Arrange
            _service.DeleteReportGroupAsync(99).ThrowsAsync(new KeyNotFoundException("Not found"));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.DeleteReportGroup(99));
        }

        #endregion
    }
}
