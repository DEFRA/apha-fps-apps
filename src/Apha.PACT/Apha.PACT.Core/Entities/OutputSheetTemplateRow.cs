namespace Apha.PACT.Core.Entities
{    
    public class OutputSheetTemplateRow
    {
        public string TestCode { get; set; } = string.Empty;
        public string? ItemDescription { get; set; }
        public string Buyer { get; set; } = string.Empty;
        public short Month { get; set; }
        public double? Volume { get; set; }
    }
}
