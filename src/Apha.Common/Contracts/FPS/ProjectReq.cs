using System.ComponentModel.DataAnnotations;

namespace Apha.Common.Contracts.FPS
{
    public class ProjectReq
    {
        [Required]
        [MaxLength(20)]
        public string ParentProject { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string ProjectTitle { get; set; } = null!;

        [MaxLength(10)]
        public string? Program { get; set; }

        [MaxLength(50)]
        public string? Customer { get; set; }

        [MaxLength(50)]
        public string? Manager { get; set; }

        public decimal TransferIncome { get; set; }
        public decimal? BudgetCvl { get; set; }
        public decimal? BudgetExt { get; set; }
        public decimal? PvsIncome { get; set; }
        public decimal? WipEoy { get; set; }
        public decimal? WipLimit { get; set; }
        public decimal? WipCurrent { get; set; }
        public decimal? FecCost { get; set; }

        [MaxLength(50)]
        public string? ProjectStatus { get; set; }

        [MaxLength(50)]
        public string? Disease { get; set; }

        [MaxLength(10)]
        public string? Contract { get; set; }

        [MaxLength(50)]
        public string? ProjectParent { get; set; }

        public short? Finished { get; set; }
        public string? Comments { get; set; }

        [Range(0, 1)]
        public short IsDefraProject { get; set; }

        [MaxLength(50)]
        public string? OracleProjectCode { get; set; }

        [MaxLength(50)]
        public string? SubAccountCode { get; set; }

        [MaxLength(50)]
        public string? ProjectGroup { get; set; }

        public decimal? PlanCaseWorkDebit { get; set; }

        [MaxLength(50)]
        public string? IncomeAccountCode { get; set; }
    }
}
