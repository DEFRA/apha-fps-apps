namespace Apha.Common.Contracts.PACT
{
    public class UpdateProfitCentreSettingsReq
    {
        public string ProfitCentre { get; set; } = string.Empty;
        public int Timesheet { get; set; }
        public int Outputsheet { get; set; }
        public short TimesheetLayout { get; set; }
    }
}
