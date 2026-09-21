using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Api.Controllers;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PIMS.Api.UnitTests.Controllers.ReportGroupLinkControllerTest
{
    public class ReportGroupLinkControllerTests
    {
        private readonly IReportGroupLinkService _service;
        private readonly IMapper _mapper;
        private readonly ReportGroupLinkController _controller;

        public ReportGroupLinkControllerTests()
        {
            _service = Substitute.For<IReportGroupLinkService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new ReportGroupLinkController(_service, _mapper);
        }

        // ── helper ──────────────────────────────────────────────────────────────────────

        private static ReportGroupLinkDto MakeDto(int reportId = 1, int groupId = 1) => new ReportGroupLinkDto
        {
            ReportId = reportId,
            GroupId = groupId
        };

        private static ReportGroupLinkRes MakeRes(int reportId = 1, int groupId = 1) => new ReportGroupLinkRes
        {
            ReportId = reportId,
            GroupId = groupId
        };

        private static ReportGroupLinkReq MakeReq(int reportId = 1, int groupId = 1) => new ReportGroupLinkReq
        {
            ReportId = reportId,
            GroupId = groupId
        };

        // ── GetAll ──────────────────────────────────────────────────────────────────────

        #region GetAll

        [Fact]
        public async Task GetAll_ServiceReturnsData_ReturnsOkWithMappedList()
        {
            // Arrange
            var dtos = new List<ReportGroupLinkDto> { MakeDto(1, 1), MakeDto(1, 2) };
            var resList = new List<ReportGroupLinkRes> { MakeRes(1, 1), MakeRes(1, 2) };
            _service.GetAllReportGroupLinksAsync().Returns(dtos);
            _mapper.Map<List<ReportGroupLinkRes>>(dtos).Returns(resList);

            // Act
            var result = await _controller.GetAllReportGroupLinks();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<List<ReportGroupLinkRes>>(ok.Value);
            Assert.Equal(2, returned.Count);
            await _service.Received(1).GetAllReportGroupLinksAsync();
            _mapper.Received(1).Map<List<ReportGroupLinkRes>>(dtos);
        }

        [Fact]
        public async Task GetAll_ServiceReturnsEmptyList_ReturnsOkWithEmptyList()
        {
            // Arrange
            var dtos = new List<ReportGroupLinkDto>();
            var resList = new List<ReportGroupLinkRes>();
            _service.GetAllReportGroupLinksAsync().Returns(dtos);
            _mapper.Map<List<ReportGroupLinkRes>>(dtos).Returns(resList);

            // Act
            var result = await _controller.GetAllReportGroupLinks();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<List<ReportGroupLinkRes>>(ok.Value);
            Assert.Empty(returned);
            await _service.Received(1).GetAllReportGroupLinksAsync();
        }

        [Fact]
        public async Task GetAll_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            _service.GetAllReportGroupLinksAsync().ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAllReportGroupLinks());
        }

        #endregion

        // ── GetByReportId ──────────────────────────────────────────────────────────────

        #region GetByReportId

        [Fact]
        public async Task GetByReportId_ServiceReturnsData_ReturnsOkWithMappedList()
        {
            // Arrange
            var dtos = new List<ReportGroupLinkDto> { MakeDto(1, 1) };
            var resList = new List<ReportGroupLinkRes> { MakeRes(1, 1) };
            _service.GetReportGroupLinksByReportIdAsync(1).Returns(dtos);
            _mapper.Map<List<ReportGroupLinkRes>>(dtos).Returns(resList);

            // Act
            var result = await _controller.GetReportGroupLinksByReportId(1);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<List<ReportGroupLinkRes>>(ok.Value);
            Assert.Single(returned);
            await _service.Received(1).GetReportGroupLinksByReportIdAsync(1);
        }

        [Fact]
        public async Task GetByReportId_ServiceReturnsEmptyList_ReturnsOkWithEmptyList()
        {
            // Arrange
            var dtos = new List<ReportGroupLinkDto>();
            var resList = new List<ReportGroupLinkRes>();
            _service.GetReportGroupLinksByReportIdAsync(99).Returns(dtos);
            _mapper.Map<List<ReportGroupLinkRes>>(dtos).Returns(resList);

            // Act
            var result = await _controller.GetReportGroupLinksByReportId(99);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<List<ReportGroupLinkRes>>(ok.Value);
            Assert.Empty(returned);
        }

        [Fact]
        public async Task GetByReportId_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            _service.GetReportGroupLinksByReportIdAsync(Arg.Any<int>()).ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetReportGroupLinksByReportId(1));
        }

        #endregion

        // ── GetById ─────────────────────────────────────────────────────────────────────

        #region GetById

        [Fact]
        public async Task GetById_ServiceReturnsDto_ReturnsOkWithMappedResult()
        {
            // Arrange
            var dto = MakeDto(5, 3);
            var res = MakeRes(5, 3);
            _service.GetReportGroupLinkByIdAsync(5, 3).Returns(dto);
            _mapper.Map<ReportGroupLinkRes>(dto).Returns(res);

            // Act
            var result = await _controller.GetReportGroupLinkById(5, 3);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, ok.Value);
            await _service.Received(1).GetReportGroupLinkByIdAsync(5, 3);
        }

        [Fact]
        public async Task GetById_ServiceReturnsNull_ReturnsJsonResultWithSuccessTrueAndNullData()
        {
            // Arrange
            _service.GetReportGroupLinkByIdAsync(99, 99).Returns((ReportGroupLinkDto?)null);

            // Act
            var result = await _controller.GetReportGroupLinkById(99, 99);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var response = Assert.IsType<Apha.Common.Contracts.ApiResponse<ReportGroupLinkRes>>(jsonResult.Value);
            Assert.True(response.Success);
            Assert.Null(response.Data);
            Assert.NotEmpty(response.Meta.CorrelationId);
        }

        [Fact]
        public async Task GetById_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            _service.GetReportGroupLinkByIdAsync(Arg.Any<int>(), Arg.Any<int>()).ThrowsAsync(new Exception("unexpected"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetReportGroupLinkById(1, 1));
        }

        #endregion

        // ── Create ──────────────────────────────────────────────────────────────────────

        #region Create

        [Fact]
        public async Task Create_ValidRequest_ReturnsCreatedAtActionWithMappedResult()
        {
            // Arrange
            var req = MakeReq(1, 1);
            var dto = MakeDto(1, 1);
            var res = MakeRes(1, 1);
            _mapper.Map<ReportGroupLinkDto>(req).Returns(dto);
            _service.CreateReportGroupLinkAsync(dto).Returns(dto);
            _mapper.Map<ReportGroupLinkRes>(dto).Returns(res);

            // Act
            var result = await _controller.CreateReportGroupLink(req);

            // Assert
            var created = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.GetReportGroupLinkById), created.ActionName);
            Assert.Equal(res, created.Value);
            await _service.Received(1).CreateReportGroupLinkAsync(dto);
            _mapper.Received(1).Map<ReportGroupLinkRes>(dto);
        }

        [Fact]
        public async Task Create_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            var req = MakeReq(1, 1);
            var dto = MakeDto(1, 1);
            _mapper.Map<ReportGroupLinkDto>(req).Returns(dto);
            _service.CreateReportGroupLinkAsync(dto).ThrowsAsync(new Exception("Data error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.CreateReportGroupLink(req));
        }

        #endregion

        // ── Delete ──────────────────────────────────────────────────────────────────────

        #region Delete

        [Fact]
        public async Task Delete_ServiceCompletes_ReturnsOkWithSuccessTrue()
        {
            // Arrange
            _service.DeleteReportGroupLinkAsync(5, 3).Returns(true);

            // Act
            var result = await _controller.DeleteReportGroupLink(5, 3);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)ok.Value!);
            await _service.Received(1).DeleteReportGroupLinkAsync(5, 3);
        }

        [Fact]
        public async Task Delete_ServiceThrowsKeyNotFoundException_PropagatesException()
        {
            // Arrange
            _service.DeleteReportGroupLinkAsync(99, 99).ThrowsAsync(new KeyNotFoundException("Not found"));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.DeleteReportGroupLink(99, 99));
        }

        #endregion
    }
}
