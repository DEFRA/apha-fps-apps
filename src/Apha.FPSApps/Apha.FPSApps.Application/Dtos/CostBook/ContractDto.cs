using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apha.FPSApps.Application.Dtos.CostBook
{
    public class ContractDto
    {
        [Required]
        public string ContractNumber { get; set; } = string.Empty;

        public string? ContractName { get; set; }

        public string? Customer { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public decimal? ContractValue { get; set; }

        public string Status { get; set; } = "Active";
    }
}
