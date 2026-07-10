using Apha.Common.Contracts.PACT;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PACT.Application.Dtos
{
    public class BatchJobEventTriggerDto
    {
        public BatchJobQueueDto Jobqueue { get; set; }
        public string EventId { get; set; }
    }
}
