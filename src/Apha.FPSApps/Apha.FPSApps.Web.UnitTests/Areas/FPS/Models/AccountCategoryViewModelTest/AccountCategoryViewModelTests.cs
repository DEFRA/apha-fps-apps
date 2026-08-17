using System.ComponentModel.DataAnnotations;
using Apha.FPSApps.Web.Areas.FPS.Models;

namespace Apha.FPSApps.Web.UnitTests.Areas.FPS.Models.AccountCategoryViewModelTest
{
    public class AccountCategoryViewModelTests
    {
        private static IList<ValidationResult> ValidateModel(AccountCategoryViewModel model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model);
            Validator.TryValidateObject(model, context, results, validateAllProperties: true);
            return results;
        }

        private static AccountCategoryViewModel CreateValidModel(string accountType)
        {
            return new AccountCategoryViewModel
            {
                AccShortName = "TEST001",
                AccountType = accountType
            };
        }

        [Theory]
        [InlineData("PAY")]
        [InlineData("NPRC")]
        public void AccountType_WithValidUpperCaseValue_PassesValidation(string accountType)
        {
            var model = CreateValidModel(accountType);

            var results = ValidateModel(model);

            Assert.DoesNotContain(results, r => r.MemberNames.Contains(nameof(AccountCategoryViewModel.AccountType)));
        }

        [Theory]
        [InlineData("pay")]
        [InlineData("Pay")]
        [InlineData("PaY")]
        [InlineData("nprc")]
        [InlineData("Nprc")]
        [InlineData("NHRC")]
        [InlineData("Income")]
        [InlineData("")]
        public void AccountType_WithInvalidValue_FailsValidation(string accountType)
        {
            var model = CreateValidModel(accountType);

            var results = ValidateModel(model);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(AccountCategoryViewModel.AccountType)));
        }

        [Fact]
        public void AccountType_ExceedingMaxLength_FailsValidation()
        {
            var model = CreateValidModel("PAYPAYPAYPAY");

            var results = ValidateModel(model);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(AccountCategoryViewModel.AccountType)));
        }
    }
}
