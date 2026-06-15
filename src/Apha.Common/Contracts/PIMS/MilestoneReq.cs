using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.Common.Contracts.PIMS
{
    public class MilestoneReq
    {
        public string Project { get; set; } = null!;
        public string Number { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime DateDue { get; set; }
        public DateTime? DateCompleted { get; set; }
        public DateTime? DateFormReceived { get; set; }
        public short UnderSdReview { get; set; }
        public short OnTarget { get; set; }
        public string? ProjectLeaderComment { get; set; }
        public string? CapsComment { get; set; }
        public string? IdType { get; set; }
        public bool IsLate { get; set; }
    }
}
