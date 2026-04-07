using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apha.Common.Contracts.Costbook
{
    public class ProjectEditRes
    {
        public ProjectRes Project { get; set; } = new();
        public List<string> AvailablePrograms { get; set; } = new();
        public List<string> AvailableCustomers { get; set; } = new();
        public List<string> AvailableDiseases { get; set; } = new();
        public List<string> AvailableStaff { get; set; } = new();
        public List<string> AvailableContracts { get; set; } = new();
    }
}
