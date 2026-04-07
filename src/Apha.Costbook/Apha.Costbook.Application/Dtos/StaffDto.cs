using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apha.Costbook.Application.Dtos
{
    public class StaffDto
    {
        public string Mnumber { get; set; } = null!;

        public string Name { get; set; } = null!;

        public string? Dt2number { get; set; }
    }
}
