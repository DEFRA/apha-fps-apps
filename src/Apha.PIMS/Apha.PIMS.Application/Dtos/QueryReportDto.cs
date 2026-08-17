namespace Apha.PIMS.Application.Dtos
{
    public class QueryReportDto
    {
        public string ReportName { get; set; } = null!;

        public string? ReportDescription { get; set; }

        public bool Emailable { get; set; }

        public bool AllowPickProgramme { get; set; }

        public bool AllowPickProject { get; set; }

        public bool AllowPickManager { get; set; }

        public bool AllowPickContract { get; set; }

        public bool AllowPickCustomer { get; set; }

        public string? ReportHelp { get; set; }

        public string? Filter { get; set; }
    }
}
