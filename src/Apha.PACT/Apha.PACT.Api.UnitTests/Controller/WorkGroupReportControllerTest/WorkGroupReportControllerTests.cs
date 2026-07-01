using Apha.Common.Contracts.PACT;
using Apha.PACT.Api.Controllers;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PACT.Api.UnitTests.Controller.WorkGroupReportControllerTest
{
    public class WorkGroupReportControllerTests
    {
        private readonly IWorkGroupReportService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly WorkGroupReportController _controller;

        public WorkGroupReportControllerTests()
        {
            _serviceMock = Substitute.For<IWorkGroupReportService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new WorkGroupReportController(_serviceMock, _mapperMock);
        }

        #region SendEmails

        [Fact]
        public async Task SendEmails_HappyPath_ReturnsOkWithMappedResults()
        {
            // Arrange
            var request = new WorkGroupReportEmailReq { ProfitCentre = "PC001", MonthNumber = 4 };
            var dtos = new List<WorkGroupReportEmailResultDto>
            {
                new() { WorkGroupName = "WG1", EmailRecipient = "a@b.com", Status = "Sent",   Reason = null },
                new() { WorkGroupName = "WG2", EmailRecipient = "c@d.com", Status = "Failed", Reason = "Invalid address" }
            };
            var mapped = new List<WorkGroupReportEmailResultRes>
            {
                new() { WorkGroupName = "WG1", EmailRecipient = "a@b.com", Status = "Sent",   Reason = null },
                new() { WorkGroupName = "WG2", EmailRecipient = "c@d.com", Status = "Failed", Reason = "Invalid address" }
            };

            _serviceMock.SendEmailsAsync("PC001", 4, Arg.Any<CancellationToken>()).Returns(dtos);
            _mapperMock.Map<IEnumerable<WorkGroupReportEmailResultRes>>(dtos).Returns(mapped);

            // Act
            var result = await _controller.SendEmails(request, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
            await _serviceMock.Received(1).SendEmailsAsync("PC001", 4, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task SendEmails_EmptyResults_ReturnsOkWithEmptyCollection()
        {
            // Arrange
            var request = new WorkGroupReportEmailReq { ProfitCentre = "PC001", MonthNumber = 4 };
            var dtos = new List<WorkGroupReportEmailResultDto>();
            var mapped = new List<WorkGroupReportEmailResultRes>();

            _serviceMock.SendEmailsAsync("PC001", 4, Arg.Any<CancellationToken>()).Returns(dtos);
            _mapperMock.Map<IEnumerable<WorkGroupReportEmailResultRes>>(dtos).Returns(mapped);

            // Act
            var result = await _controller.SendEmails(request, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value as IEnumerable<WorkGroupReportEmailResultRes>;
            Assert.NotNull(value);
            Assert.Empty(value);
        }

        [Fact]
        public async Task SendEmails_CancellationTokenForwarded_ServiceReceivesToken()
        {
            // Arrange
            var request = new WorkGroupReportEmailReq { ProfitCentre = "PC001", MonthNumber = 4 };
            using var cts = new CancellationTokenSource();

            _serviceMock.SendEmailsAsync(Arg.Any<string>(), Arg.Any<short>(), cts.Token)
                .Returns(new List<WorkGroupReportEmailResultDto>());
            _mapperMock.Map<IEnumerable<WorkGroupReportEmailResultRes>>(Arg.Any<object>())
                .Returns(new List<WorkGroupReportEmailResultRes>());

            // Act
            await _controller.SendEmails(request, cts.Token);

            // Assert
            await _serviceMock.Received(1).SendEmailsAsync("PC001", 4, cts.Token);
        }

        [Fact]
        public async Task SendEmails_ServiceThrows_PropagatesException()
        {
            // Arrange
            var request = new WorkGroupReportEmailReq { ProfitCentre = "PC001", MonthNumber = 4 };
            _serviceMock.SendEmailsAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<CancellationToken>())
                .ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.SendEmails(request, CancellationToken.None));
        }

        #endregion

        #region ExportCos90s

        [Fact]
        public async Task ExportCos90s_WithValidRequest_ReturnsOkWithMappedResult()
        {
            // Arrange
            var request = new WorkGroupCos90SExportReq
            {
                ProfitCentre = "PC001",
                MonthNumber = 3,
                Year = 2025,
                PactId = "S001"
            };
            var serviceResult = new WorkGroupCos90SExportResultDto
            {
                Rows = [new WorkGroupCos90SExportRowDto { WorkGroupName = "WG1", StaffName = "John Smith" }]
            };
            var mappedResult = new WorkGroupCos90SExportRes
            {
                Rows = [new WorkGroupCos90SExportRowRes { WorkGroupName = "WG1", StaffName = "John Smith" }]
            };

            _serviceMock.ExportCos90sAsync("PC001", 3, 2025, "S001", Arg.Any<CancellationToken>()).Returns(serviceResult);
            _mapperMock.Map<WorkGroupCos90SExportRes>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.ExportCos90s(request, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);
            await _serviceMock.Received(1).ExportCos90sAsync("PC001", 3, 2025, "S001", Arg.Any<CancellationToken>());
            _mapperMock.Received(1).Map<WorkGroupCos90SExportRes>(serviceResult);
        }

        [Fact]
        public async Task ExportCos90s_WithNullPactId_ReturnsOkWithMappedResult()
        {
            // Arrange
            var request = new WorkGroupCos90SExportReq
            {
                ProfitCentre = "PC001",
                MonthNumber = 3,
                Year = 2025,
                PactId = null
            };
            var serviceResult = new WorkGroupCos90SExportResultDto { Rows = [] };
            var mappedResult = new WorkGroupCos90SExportRes { Rows = [] };

            _serviceMock.ExportCos90sAsync("PC001", 3, 2025, null, Arg.Any<CancellationToken>()).Returns(serviceResult);
            _mapperMock.Map<WorkGroupCos90SExportRes>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.ExportCos90s(request, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<WorkGroupCos90SExportRes>(okResult.Value);
            Assert.Empty(value.Rows);
        }

        [Fact]
        public async Task ExportCos90s_ServiceThrows_PropagatesException()
        {
            // Arrange
            var request = new WorkGroupCos90SExportReq
            {
                ProfitCentre = "PC001",
                MonthNumber = 3,
                Year = 2025,
                PactId = "S001"
            };

            _serviceMock.ExportCos90sAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<short>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
                .ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.ExportCos90s(request, CancellationToken.None));
        }

        #endregion
    }
}
