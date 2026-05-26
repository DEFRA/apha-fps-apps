using Apha.Common.Contracts.Email;
using Apha.Common.Utilities.Email;
using Apha.Common.Utilities.ExcelExport;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Apha.PACT.Application.Services
{
    public class WorkGroupReportEmailService : IWorkGroupReportEmailService
    {
        private const string StatusSent    = "Sent";
        private const string StatusSkipped = "Skipped";
        private const string StatusFailed  = "Failed";
        
        private const int AccessTrue = -1;

        private readonly IWorkGroupRepository _workGroupReportRepository;
        private readonly IGraphEmailService _emailService;
        private readonly IExcelExportService _excelService;
        private readonly ILogger<WorkGroupReportEmailService> _logger;
        private readonly string _emailBody;

        public WorkGroupReportEmailService(
            IWorkGroupRepository workGroupReportRepository,
            IGraphEmailService emailService,
            IExcelExportService excelService,
            IOptions<WorkGroupReportEmailSettings> emailSettings,
            ILogger<WorkGroupReportEmailService> logger)
        {
            _workGroupReportRepository = workGroupReportRepository;
            _emailService = emailService;
            _excelService = excelService;
            _logger = logger;

            var settings = emailSettings.Value;
            _emailBody = string.Format(settings.EmailBodyTemplate, settings.GatekeeperMailbox);
        }

        public async Task<IEnumerable<WorkGroupReportEmailResultDto>> SendEmailsAsync(
            string profitCentre,
            short monthNumber,
            CancellationToken cancellationToken = default)
        {
            var results = new List<WorkGroupReportEmailResultDto>();

            var profitCentreView = await _workGroupReportRepository.GetProfitCentreAsync(profitCentre);

            // Access Yes/No fields are stored as -1 (True) / 0 (False)
            var sendTimeSheet   = profitCentreView?.Timesheet   == AccessTrue;
            var sendOutputSheet = profitCentreView?.Outputsheet == AccessTrue;
            // TimesheetLayout: 1 = Flat-file, 2 = Cross-tab; default to flat if not set
            var timesheetLayout = profitCentreView?.TimesheetLayout ?? 1;

            var workGroups = await _workGroupReportRepository.GetWorkGroupsForEmailAsync(profitCentre);

            foreach (var workGroup in workGroups)
            {
                var result = await ProcessWorkGroupAsync(
                    workGroup, monthNumber, sendTimeSheet, sendOutputSheet, timesheetLayout, cancellationToken);
                results.Add(result);
            }

            return results;
        }

        private async Task<WorkGroupReportEmailResultDto> ProcessWorkGroupAsync(
            WorkGroup workGroup,
            short monthNumber,
            bool sendTimeSheet,
            bool sendOutputSheet,
            short timesheetLayout,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(workGroup.EmailRecipient))
            {
                return new WorkGroupReportEmailResultDto
                {
                    WorkGroupName  = workGroup.WorkGroupName,
                    EmailRecipient = null,
                    Status         = StatusSkipped,
                    Reason         = "No email recipient configured"
                };
            }

            var month = monthNumber.ToString("D2");

            try
            {
                if (sendTimeSheet)
                    await SendTimeSheetEmailAsync(workGroup, monthNumber, month, timesheetLayout, cancellationToken);

                if (sendOutputSheet)
                    await SendOutputSheetEmailAsync(workGroup, monthNumber, month, cancellationToken);

                return new WorkGroupReportEmailResultDto
                {
                    WorkGroupName  = workGroup.WorkGroupName,
                    EmailRecipient = workGroup.EmailRecipient,
                    Status         = StatusSent
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send email for work group {WorkGroupName} (recipient: {EmailRecipient}, month: {MonthNumber})",
                    workGroup.WorkGroupName, workGroup.EmailRecipient, monthNumber);

                return new WorkGroupReportEmailResultDto
                {
                    WorkGroupName  = workGroup.WorkGroupName,
                    EmailRecipient = workGroup.EmailRecipient,
                    Status         = StatusFailed,
                    Reason         = "An error occurred while sending the email. Please contact support if the problem persists."
                };
            }
        }

        private async Task SendTimeSheetEmailAsync(
            WorkGroup workGroup,
            short monthNumber,
            string month,
            short timesheetLayout,
            CancellationToken cancellationToken)
        {
            var templateRows = await _workGroupReportRepository
                .GetTimeSheetTemplateAsync(workGroup.WorkGroupName, monthNumber, timesheetLayout);

            var fileName = $"{workGroup.WorkGroupName}{month}TS.xlsx";
            var bytes    = _excelService.BuildTimeSheetExcel(
                workGroup.WorkGroupName,
                monthNumber,
                templateRows.Select(r => new WorkGroupTimeSheetRow
                {
                    StaffName     = r.StaffName,
                    TimeCode      = r.TimeCode,
                    Description   = r.Description,
                    ParentProject = r.ParentProject,
                    Month         = r.Month
                }),
                timesheetLayout);

            await _emailService.SendEmailAsync(new EmailMessageModel
            {
                To          = [workGroup.EmailRecipient!],
                Subject     = $"MARS Time Sheets - {fileName}",
                Body        = _emailBody,
                IsBodyHtml  = false,
                Attachments =
                [
                    new EmailAttachmentModel
                    {
                        FileName     = fileName,
                        ContentBytes = bytes,
                        ContentType  = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                    }
                ]
            }, cancellationToken);
        }

        private async Task SendOutputSheetEmailAsync(
            WorkGroup workGroup,
            short monthNumber,
            string month,
            CancellationToken cancellationToken)
        {
            var templateRows = await _workGroupReportRepository
                .GetOutputSheetTemplateAsync(workGroup.WorkGroupName, monthNumber);

            var fileName = $"{workGroup.WorkGroupName}{month}OP.xlsx";
            var bytes    = _excelService.BuildOutputSheetExcel(
                workGroup.WorkGroupName,
                monthNumber,
                templateRows.Select(r => new WorkGroupOutputSheetRow
                {
                    TestCode        = r.TestCode,
                    ItemDescription = r.ItemDescription,
                    Buyer           = r.Buyer,
                    Month           = r.Month
                }));

            await _emailService.SendEmailAsync(new EmailMessageModel
            {
                To          = [workGroup.EmailRecipient!],
                Subject     = $"MARS Output Sheets - {fileName}",
                Body        = _emailBody,
                IsBodyHtml  = false,
                Attachments =
                [
                    new EmailAttachmentModel
                    {
                        FileName     = fileName,
                        ContentBytes = bytes,
                        ContentType  = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                    }
                ]
            }, cancellationToken);
        }       
    }
}
