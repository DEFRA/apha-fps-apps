using Apha.FPSApps.Application.Dtos.PACT;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.FPSApps.Application.Dtos.PACT
{
    public class BatchJobEventTriggerDto
    {
        public BatchJobQueueDto Jobqueue { get; set; }
        public string EventId { get; set; }
    }
}
