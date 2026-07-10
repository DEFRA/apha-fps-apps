using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.Common.Contracts.PACT
{
    public class BatchJobEventTriggerRes
    {
        public BatchJobQueueRes Jobqueue { get; set; }
        public string EventId { get; set; }
    }
}
