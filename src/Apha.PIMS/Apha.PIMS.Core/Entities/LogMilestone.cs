using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PIMS.Core.Entities
{
    public class LogMilestone
    {
        public string? Project { get; set; }

        public string? Number { get; set; }

        public string? Description { get; set; }

        public DateTime? DateDue { get; set; }

        public DateTime? DateCompleted { get; set; }

        public DateTime? DateFormReceived { get; set; } 
        public short? UnderSdReview { get; set; }

        public short? OnTarget { get; set; }

        public string? ProjectLeaderComment { get; set; }
        public string? CapsComment { get; set; }

        public char? IdType { get; set; }

        public DateTime? DateChanged { get; set; }
        public string? ChangedBy { get; set; }

        public char? UpdateType { get; set; }

        public int Id { get; set; }
    }

}
