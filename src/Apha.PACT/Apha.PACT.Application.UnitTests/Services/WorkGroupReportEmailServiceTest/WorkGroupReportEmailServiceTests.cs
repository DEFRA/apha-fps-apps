using Apha.Common.Contracts.Email;
using Apha.Common.Utilities.Email;
using Apha.PACT.Application.Services;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PACT.Application.UnitTests.Services.WorkGroupReportEmailServiceTest
{
    public class WorkGroupReportEmailServiceTests
    {
        private readonly IWorkGroupRepository _mockRepository;
        private readonly IGraphEmailService _mockEmailService;
        private readonly WorkGroupReportEmailService _sut;

        private const short Month4 = 4;
        private const string ProfitCentre = "PC001";

        // PactProfitCentreView with Timesheet=-1 (Access True) and Outputsheet=0 (False)
        private static PactProfitCentreView TimesheetOnlySettings => new()
        {
            ProfitCentre    = ProfitCentre,
            Timesheet       = -1,
            Outputsheet     = 0,
            TimesheetLayout = 1
        };

        // PactProfitCentreView with both sheets enabled
        private static PactProfitCentreView BothSheetsSettings => new()
        {
            ProfitCentre    = ProfitCentre,
            Timesheet       = -1,
            Outputsheet     = -1,
            TimesheetLayout = 1
        };

        // PactProfitCentreView with only output sheet enabled
        private static PactProfitCentreView OutputSheetOnlySettings => new()
        {
            ProfitCentre    = ProfitCentre,
            Timesheet       = 0,
            Outputsheet     = -1,
            TimesheetLayout = 1
        };

        private static WorkGroup WorkGroupWithRecipient => new()
        {
            WorkGroupName  = "WG1",
            ProfitCentre   = ProfitCentre,
            EmailRecipient = "wg@test.com"
        };

        private static WorkGroup WorkGroupWithoutRecipient => new()
        {
            WorkGroupName  = "WG2",
            ProfitCentre   = ProfitCentre,
            EmailRecipient = null
        };

        public WorkGroupReportEmailServiceTests()
        {
            _mockRepository   = Substitute.For<IWorkGroupRepository>();
            _mockEmailService = Substitute.For<IGraphEmailService>();
            _sut = new WorkGroupReportEmailService(_mockRepository, _mockEmailService);
        }

        #region SendEmailsAsync — empty work groups

        [Fact]
        public async Task SendEmailsAsync_NoWorkGroups_ReturnsEmptyResults()
        {
            // Arrange
            _mockRepository.GetProfitCentreAsync(ProfitCentre).Returns(TimesheetOnlySettings);
            _mockRepository.GetWorkGroupsForEmailAsync(ProfitCentre).Returns(new List<WorkGroup>());

            // Act
            var results = await _sut.SendEmailsAsync(ProfitCentre, Month4);

            // Assert
            results.Should().BeEmpty();
            await _mockEmailService.DidNotReceive().SendEmailAsync(Arg.Any<EmailMessageModel>(), Arg.Any<CancellationToken>());
        }

        #endregion

        #region SendEmailsAsync — skipped (no email recipient)

        [Fact]
        public async Task SendEmailsAsync_WorkGroupWithNoRecipient_ReturnsSkippedResult()
        {
            // Arrange
            _mockRepository.GetProfitCentreAsync(ProfitCentre).Returns(TimesheetOnlySettings);
            _mockRepository.GetWorkGroupsForEmailAsync(ProfitCentre)
                .Returns(new List<WorkGroup> { WorkGroupWithoutRecipient });

            // Act
            var results = (await _sut.SendEmailsAsync(ProfitCentre, Month4)).ToList();

            // Assert
            results.Should().HaveCount(1);
            results[0].WorkGroupName.Should().Be("WG2");
            results[0].Status.Should().Be("Skipped");
            results[0].Reason.Should().NotBeNullOrWhiteSpace();
            await _mockEmailService.DidNotReceive().SendEmailAsync(Arg.Any<EmailMessageModel>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task SendEmailsAsync_WorkGroupWithWhitespaceRecipient_ReturnsSkippedResult()
        {
            // Arrange
            var wg = new WorkGroup { WorkGroupName = "WG3", ProfitCentre = ProfitCentre, EmailRecipient = "   " };

            _mockRepository.GetProfitCentreAsync(ProfitCentre).Returns(TimesheetOnlySettings);
            _mockRepository.GetWorkGroupsForEmailAsync(ProfitCentre).Returns(new List<WorkGroup> { wg });

            // Act
            var results = (await _sut.SendEmailsAsync(ProfitCentre, Month4)).ToList();

            // Assert
            results[0].Status.Should().Be("Skipped");
        }

        #endregion

        #region SendEmailsAsync — sent (timesheet only)

        [Fact]
        public async Task SendEmailsAsync_TimesheetEnabled_SendsTimesheetEmailAndReturnsSent()
        {
            // Arrange
            var templateRows = new List<TimeSheetTemplateRow>
            {
                new() { StaffName = "Alice", TimeCode = "TC01", ParentProject = "PP01", Month = Month4 }
            };

            _mockRepository.GetProfitCentreAsync(ProfitCentre).Returns(TimesheetOnlySettings);
            _mockRepository.GetWorkGroupsForEmailAsync(ProfitCentre)
                .Returns(new List<WorkGroup> { WorkGroupWithRecipient });
            _mockRepository.GetTimeSheetTemplateAsync("WG1", Month4, 1).Returns(templateRows);

            // Act
            var results = (await _sut.SendEmailsAsync(ProfitCentre, Month4)).ToList();

            // Assert
            results.Should().HaveCount(1);
            results[0].Status.Should().Be("Sent");
            results[0].WorkGroupName.Should().Be("WG1");
            results[0].EmailRecipient.Should().Be("wg@test.com");

            await _mockEmailService.Received(1).SendEmailAsync(
                Arg.Is<EmailMessageModel>(m =>
                    m.Subject != null && m.Subject.Contains("Time Sheets") &&
                    m.To != null && m.To.Contains("wg@test.com")),
                Arg.Any<CancellationToken>());
        }

        #endregion

        #region SendEmailsAsync — sent (output sheet only)

        [Fact]
        public async Task SendEmailsAsync_OutputSheetEnabled_SendsOutputSheetEmailAndReturnsSent()
        {
            // Arrange
            var outputRows = new List<OutputSheetTemplateRow>
            {
                new() { TestCode = "TC01", ItemDescription = "Test", Buyer = "B1", Month = Month4 }
            };

            _mockRepository.GetProfitCentreAsync(ProfitCentre).Returns(OutputSheetOnlySettings);
            _mockRepository.GetWorkGroupsForEmailAsync(ProfitCentre)
                .Returns(new List<WorkGroup> { WorkGroupWithRecipient });
            _mockRepository.GetOutputSheetTemplateAsync("WG1", Month4).Returns(outputRows);

            // Act
            var results = (await _sut.SendEmailsAsync(ProfitCentre, Month4)).ToList();

            // Assert
            results[0].Status.Should().Be("Sent");

            await _mockEmailService.Received(1).SendEmailAsync(
                Arg.Is<EmailMessageModel>(m =>
                    m.Subject != null && m.Subject.Contains("Output Sheets")),
                Arg.Any<CancellationToken>());
        }

        #endregion

        #region SendEmailsAsync — sent (both sheets)

        [Fact]
        public async Task SendEmailsAsync_BothSheetsEnabled_SendsTwoEmailsAndReturnsSent()
        {
            // Arrange
            var timesheetRows = new List<TimeSheetTemplateRow>
            {
                new() { StaffName = "Alice", TimeCode = "TC01", ParentProject = "PP01", Month = Month4 }
            };
            var outputRows = new List<OutputSheetTemplateRow>
            {
                new() { TestCode = "TC01", ItemDescription = "Test", Buyer = "B1", Month = Month4 }
            };

            _mockRepository.GetProfitCentreAsync(ProfitCentre).Returns(BothSheetsSettings);
            _mockRepository.GetWorkGroupsForEmailAsync(ProfitCentre)
                .Returns(new List<WorkGroup> { WorkGroupWithRecipient });
            _mockRepository.GetTimeSheetTemplateAsync("WG1", Month4, 1).Returns(timesheetRows);
            _mockRepository.GetOutputSheetTemplateAsync("WG1", Month4).Returns(outputRows);

            // Act
            var results = (await _sut.SendEmailsAsync(ProfitCentre, Month4)).ToList();

            // Assert
            results[0].Status.Should().Be("Sent");
            await _mockEmailService.Received(2).SendEmailAsync(Arg.Any<EmailMessageModel>(), Arg.Any<CancellationToken>());
        }

        #endregion

        #region SendEmailsAsync — failed (email throws)

        [Fact]
        public async Task SendEmailsAsync_EmailServiceThrows_ReturnsFailedResultWithReason()
        {
            // Arrange
            var templateRows = new List<TimeSheetTemplateRow>
            {
                new() { StaffName = "Alice", TimeCode = "TC01", ParentProject = "PP01", Month = Month4 }
            };

            _mockRepository.GetProfitCentreAsync(ProfitCentre).Returns(TimesheetOnlySettings);
            _mockRepository.GetWorkGroupsForEmailAsync(ProfitCentre)
                .Returns(new List<WorkGroup> { WorkGroupWithRecipient });
            _mockRepository.GetTimeSheetTemplateAsync("WG1", Month4, 1).Returns(templateRows);

            _mockEmailService.SendEmailAsync(Arg.Any<EmailMessageModel>(), Arg.Any<CancellationToken>())
                .ThrowsAsync(new Exception("SMTP failure"));

            // Act
            var results = (await _sut.SendEmailsAsync(ProfitCentre, Month4)).ToList();

            // Assert
            results.Should().HaveCount(1);
            results[0].Status.Should().Be("Failed");
            results[0].Reason.Should().Be("SMTP failure");
        }

        #endregion

        #region SendEmailsAsync — mixed results (multiple work groups)

        [Fact]
        public async Task SendEmailsAsync_MixedWorkGroups_ReturnsOneSkippedOneSent()
        {
            // Arrange
            var templateRows = new List<TimeSheetTemplateRow>
            {
                new() { StaffName = "Alice", TimeCode = "TC01", ParentProject = "PP01", Month = Month4 }
            };

            _mockRepository.GetProfitCentreAsync(ProfitCentre).Returns(TimesheetOnlySettings);
            _mockRepository.GetWorkGroupsForEmailAsync(ProfitCentre)
                .Returns(new List<WorkGroup> { WorkGroupWithoutRecipient, WorkGroupWithRecipient });
            _mockRepository.GetTimeSheetTemplateAsync("WG1", Month4, 1).Returns(templateRows);

            // Act
            var results = (await _sut.SendEmailsAsync(ProfitCentre, Month4)).ToList();

            // Assert
            results.Should().HaveCount(2);
            results.Should().ContainSingle(r => r.Status == "Skipped");
            results.Should().ContainSingle(r => r.Status == "Sent");
        }

        #endregion

        #region SendEmailsAsync — null profit centre view (defaults)

        [Fact]
        public async Task SendEmailsAsync_NullProfitCentreView_NoEmailsSentForWorkGroups()
        {
            // Arrange — null view means sendTimeSheet=false, sendOutputSheet=false
            _mockRepository.GetProfitCentreAsync(ProfitCentre).Returns((PactProfitCentreView?)null);
            _mockRepository.GetWorkGroupsForEmailAsync(ProfitCentre)
                .Returns(new List<WorkGroup> { WorkGroupWithRecipient });

            // Act
            var results = (await _sut.SendEmailsAsync(ProfitCentre, Month4)).ToList();

            // Assert — no sheets to send, still returns Sent (no email sent, no attachment error)
            results.Should().HaveCount(1);
            results[0].Status.Should().Be("Sent");
            await _mockEmailService.DidNotReceive().SendEmailAsync(Arg.Any<EmailMessageModel>(), Arg.Any<CancellationToken>());
        }

        #endregion

        #region SendEmailsAsync — cancellation token forwarded

        [Fact]
        public async Task SendEmailsAsync_CancellationTokenForwarded_ServiceReceivesToken()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            var templateRows = new List<TimeSheetTemplateRow>
            {
                new() { StaffName = "Alice", TimeCode = "TC01", ParentProject = "PP01", Month = Month4 }
            };

            _mockRepository.GetProfitCentreAsync(ProfitCentre).Returns(TimesheetOnlySettings);
            _mockRepository.GetWorkGroupsForEmailAsync(ProfitCentre)
                .Returns(new List<WorkGroup> { WorkGroupWithRecipient });
            _mockRepository.GetTimeSheetTemplateAsync("WG1", Month4, 1).Returns(templateRows);

            // Act
            await _sut.SendEmailsAsync(ProfitCentre, Month4, cts.Token);

            // Assert
            await _mockEmailService.Received(1).SendEmailAsync(Arg.Any<EmailMessageModel>(), cts.Token);
        }

        #endregion
    }
}
