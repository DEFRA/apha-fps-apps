using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.Common.Contracts.PIMS
{
    public class ProposedProjectRes
    {
        public int Id { get; set; }
        public string? Parentproject { get; set; }
        public string? TransferTo { get; set; }
        public string? Projecttitle { get; set; }
        public string? Costbookno { get; set; }
        public string? Disease { get; set; }
        public string? Program { get; set; }
        public string? Customer { get; set; }
        public string? Manager { get; set; }
        public string? Projectstatus { get; set; }
        public string? Reason { get; set; }
    }
}
