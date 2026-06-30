using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.Common.Utilities.EventPublisher
{
    public class BatchTriggerEventDetail
    {
        public string JobExecutionId { get; set; }
        public string JobName { get; set; }
        public string RunMode { get; set; }
        public string RequestedBy { get; set; } = string.Empty;     

        public DateTime RequestedAtUtc { get; set; }
        public string? ParametersJson { get; set; }
    }
}
