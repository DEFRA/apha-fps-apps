using Apha.Common.Contracts.Email;
using Apha.Common.Utilities.Email;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using ClosedXML.Excel;

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

        public WorkGroupReportEmailService(
            IWorkGroupRepository workGroupReportRepository,
            IGraphEmailService emailService)
        {
            _workGroupReportRepository = workGroupReportRepository;
            _emailService = emailService;
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

        private static readonly string EmailBody =
            "Please complete and return to APHA Gatekeeper - OTL Mailbox. " +
            "[Mailto:CAPSMailbox@vla.defra.gsi.gov.uk]. Thank you.";

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

            var month = monthNumber < 10 ? $"0{monthNumber}" : $"{monthNumber}";

            try
            {
                if (sendTimeSheet)
                {
                    var templateRows = await _workGroupReportRepository
                        .GetTimeSheetTemplateAsync(workGroup.WorkGroupName, monthNumber, timesheetLayout);

                    var fileName = $"{workGroup.WorkGroupName}{month}TS.xlsx";
                    var bytes    = BuildTimeSheetExcel(workGroup.WorkGroupName, monthNumber, templateRows, timesheetLayout);

                    await _emailService.SendEmailAsync(new EmailMessageModel
                    {
                        To          = [workGroup.EmailRecipient],
                        Subject     = $"MARS Time Sheets - {fileName}",
                        Body        = EmailBody,
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

                if (sendOutputSheet)
                {
                    var templateRows = await _workGroupReportRepository
                        .GetOutputSheetTemplateAsync(workGroup.WorkGroupName, monthNumber);

                    var fileName = $"{workGroup.WorkGroupName}{month}OP.xlsx";
                    var bytes    = BuildOutputSheetExcel(workGroup.WorkGroupName, monthNumber, templateRows);

                    await _emailService.SendEmailAsync(new EmailMessageModel
                    {
                        To          = [workGroup.EmailRecipient],
                        Subject     = $"MARS Output Sheets - {fileName}",
                        Body        = EmailBody,
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

                return new WorkGroupReportEmailResultDto
                {
                    WorkGroupName  = workGroup.WorkGroupName,
                    EmailRecipient = workGroup.EmailRecipient,
                    Status         = StatusSent
                };
            }
            catch (Exception ex)
            {
                return new WorkGroupReportEmailResultDto
                {
                    WorkGroupName  = workGroup.WorkGroupName,
                    EmailRecipient = workGroup.EmailRecipient,
                    Status         = StatusFailed,
                    Reason         = ex.Message
                };
            }
        }
       
        private static byte[] BuildTimeSheetExcel(
            string workGroupName,
            short monthNumber,
            IEnumerable<TimeSheetTemplateRow> rows,
            short layout)
        {
            using var workbook = new XLWorkbook();
            var ws   = workbook.Worksheets.Add("TimeSheet");
            var data = rows.ToList();

            if (layout == 2)
            {
                // Cross-tab: fixed cols + one column per staff name (PIVOT tblStaff.Name)
                // StaffName on each row is the comma-separated list of staff for that
                // (TimeCode, ParentProject) group — extract the full distinct staff set
                // across all rows to build the pivot column headers.
                var staffNames = data
                    .SelectMany(r => r.StaffName.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    .Distinct()
                    .OrderBy(n => n)
                    .ToList();

                // Header row
                ws.Cell(1, 1).Value = "Time Code";
                ws.Cell(1, 2).Value = "Description";
                ws.Cell(1, 3).Value = "Parent Project";
                ws.Cell(1, 4).Value = "Month";
                for (int c = 0; c < staffNames.Count; c++)
                    ws.Cell(1, 5 + c).Value = staffNames[c];

                int row = 2;
                foreach (var item in data)
                {
                    ws.Cell(row, 1).Value = item.TimeCode;
                    ws.Cell(row, 2).Value = item.Description ?? string.Empty;
                    ws.Cell(row, 3).Value = item.ParentProject;
                    ws.Cell(row, 4).Value = item.Month;
                    // Staff pivot columns are blank — recipient fills in Hours per staff member
                    row++;
                }
            }
            else
            {
                // Flat-file: WorkGroup | Name | TimeCode | ParentProject | Month | Hours
                ws.Cell(1, 1).Value = "Work Group";
                ws.Cell(1, 2).Value = "Name";
                ws.Cell(1, 3).Value = "Time Code";
                ws.Cell(1, 4).Value = "Parent Project";
                ws.Cell(1, 5).Value = "Month";
                ws.Cell(1, 6).Value = "Hours";

                int row = 2;
                foreach (var item in data)
                {
                    ws.Cell(row, 1).Value = workGroupName;
                    ws.Cell(row, 2).Value = item.StaffName;
                    ws.Cell(row, 3).Value = item.TimeCode;
                    ws.Cell(row, 4).Value = item.ParentProject;
                    ws.Cell(row, 5).Value = item.Month;
                    ws.Cell(row, 6).Value = string.Empty; // blank — recipient fills in
                    row++;
                }
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
        /// <summary>
        /// Builds the output-sheet Excel from template rows (blank Volume — recipient fills in).       
        ///   Columns: WorkGroup | TestCode | ItemDescription | Buyer | Month | Volume
        /// </summary>
        private static byte[] BuildOutputSheetExcel(
            string workGroupName,
            short monthNumber,
            IEnumerable<OutputSheetTemplateRow> rows)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("OutputSheet");

            ws.Cell(1, 1).Value = "Work Group";
            ws.Cell(1, 2).Value = "Test Code";
            ws.Cell(1, 3).Value = "Item Description";
            ws.Cell(1, 4).Value = "Buyer";
            ws.Cell(1, 5).Value = "Month";
            ws.Cell(1, 6).Value = "Volume";

            int row = 2;
            foreach (var item in rows)
            {
                ws.Cell(row, 1).Value = workGroupName;
                ws.Cell(row, 2).Value = item.TestCode;
                ws.Cell(row, 3).Value = item.ItemDescription ?? string.Empty;
                ws.Cell(row, 4).Value = item.Buyer;
                ws.Cell(row, 5).Value = item.Month;
                ws.Cell(row, 6).Value = string.Empty;   // blank — recipient fills in
                row++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }


        #if DEBUG
        public async Task<IEnumerable<string>> SaveExcelFilesAsync(
            string profitCentre,
            string workGroupName,
            short monthNumber,
            string outputFolder,
            CancellationToken cancellationToken = default)
        {
            var profitCentreView = await _workGroupReportRepository.GetProfitCentreAsync(profitCentre);
            var sendTimeSheet   = profitCentreView?.Timesheet   == AccessTrue;
            var sendOutputSheet = profitCentreView?.Outputsheet == AccessTrue;
            var timesheetLayout = profitCentreView?.TimesheetLayout ?? (short)1;

            Directory.CreateDirectory(outputFolder);
            var saved = new List<string>();
            var month = monthNumber < 10 ? $"0{monthNumber}" : $"{monthNumber}";

            if (sendTimeSheet)
            {
                var rows  = await _workGroupReportRepository.GetTimeSheetTemplateAsync(workGroupName, monthNumber, timesheetLayout);
                var bytes = BuildTimeSheetExcel(workGroupName, monthNumber, rows, timesheetLayout);
                var path  = Path.Combine(outputFolder, $"{workGroupName}{month}TS.xlsx");
                await File.WriteAllBytesAsync(path, bytes, cancellationToken);
                saved.Add(path);
            }

            if (sendOutputSheet)
            {
                var rows  = await _workGroupReportRepository.GetOutputSheetTemplateAsync(workGroupName, monthNumber);
                var bytes = BuildOutputSheetExcel(workGroupName, monthNumber, rows);
                var path  = Path.Combine(outputFolder, $"{workGroupName}{month}OP.xlsx");
                await File.WriteAllBytesAsync(path, bytes, cancellationToken);
                saved.Add(path);
            }

            return saved;
        }
#endif
    }
}
