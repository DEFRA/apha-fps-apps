using System;

namespace Apha.PIMS.Application.Dtos
{
    public class MilestoneFormDatesDto
    {
        public short Year { get; set; }
        public string ParentProject { get; set; } = null!;
        public DateTime? Jan { get; set; }
        public DateTime? Feb { get; set; }
        public DateTime? Mar { get; set; }
        public DateTime? Apr { get; set; }
        public DateTime? May { get; set; }
        public DateTime? Jun { get; set; }
        public DateTime? Jul { get; set; }
        public DateTime? Aug { get; set; }
        public DateTime? Sep { get; set; }
        public DateTime? Oct { get; set; }
        public DateTime? Nov { get; set; }
        public DateTime? Dec { get; set; }
    }
}
