using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Apha.Common.Contracts.PACT
{
    public class TestRequirementReq : IValidatableObject
    {
        public string TestCode { get; set; } = null!;
        public string Buyer { get; set; } = null!;
        public decimal? UnitPrice { get; set; }
        public double? NoRequired { get; set; }
        public string? ProjectBuyerCode { get; set; }
        public string? TestBuyerCode { get; set; }
        public short? Active { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(ProjectBuyerCode) && string.IsNullOrWhiteSpace(TestBuyerCode))
                yield return new ValidationResult(
                    "You must fill in project buyer or test buyer.",
                    new[] { nameof(ProjectBuyerCode), nameof(TestBuyerCode) });
        }
    }
}
