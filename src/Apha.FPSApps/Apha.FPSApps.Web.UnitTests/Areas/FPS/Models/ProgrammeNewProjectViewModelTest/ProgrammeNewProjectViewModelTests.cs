using System.ComponentModel.DataAnnotations;
using Apha.FPSApps.Web.Areas.FPS.Models;

namespace Apha.FPSApps.Web.UnitTests.Areas.FPS.Models.ProgrammeNewProjectViewModelTest
{
    public class ProgrammeNewProjectViewModelTests
    {
        private static IList<ValidationResult> ValidateModel(ProgrammeNewProjectViewModel model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model);
            Validator.TryValidateObject(model, context, results, validateAllProperties: true);
            return results;
        }

        private static ProgrammeNewProjectViewModel CreateValidModel()
        {
            return new ProgrammeNewProjectViewModel
            {
                ParentProject = "PP001",
                ProjectTitle = "Test Project",
                ShortTitle = "Test",
                Customer = "DEFRA",
                Program = "P001",
                Manager = "Alice",
                Disease = "FMD",
                ProjectStatus = "Active",
                Contract = "C001",
                IsDefraProject = -1,
                IncomeAccountCode = "INC001",
                SubAccountCode = "SUB001",
                BudgetCvl = 0m,
                CostCentre = 1
            };
        }

        [Fact]
        public void ParentProject_AtMaxDatabaseLength_PassesValidation()
        {
            var model = CreateValidModel();
            model.ParentProject = new string('A', 20);

            var results = ValidateModel(model);

            Assert.DoesNotContain(results, r => r.MemberNames.Contains(nameof(ProgrammeNewProjectViewModel.ParentProject)));
        }

        [Fact]
        public void ParentProject_ExceedingDatabaseLength_FailsValidation()
        {
            var model = CreateValidModel();
            model.ParentProject = new string('A', 21);

            var results = ValidateModel(model);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(ProgrammeNewProjectViewModel.ParentProject)));
        }

        [Fact]
        public void ProjectTitle_AtMaxDatabaseLength_PassesValidation()
        {
            var model = CreateValidModel();
            model.ProjectTitle = new string('A', 200);

            var results = ValidateModel(model);

            Assert.DoesNotContain(results, r => r.MemberNames.Contains(nameof(ProgrammeNewProjectViewModel.ProjectTitle)));
        }

        [Fact]
        public void ProjectTitle_ExceedingDatabaseLength_FailsValidation()
        {
            var model = CreateValidModel();
            model.ProjectTitle = new string('A', 201);

            var results = ValidateModel(model);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(ProgrammeNewProjectViewModel.ProjectTitle)));
        }

        [Fact]
        public void ShortTitle_AtMaxDatabaseLength_PassesValidation()
        {
            var model = CreateValidModel();
            model.ShortTitle = new string('A', 30);

            var results = ValidateModel(model);

            Assert.DoesNotContain(results, r => r.MemberNames.Contains(nameof(ProgrammeNewProjectViewModel.ShortTitle)));
        }

        [Fact]
        public void ShortTitle_ExceedingDatabaseLength_FailsValidation()
        {
            var model = CreateValidModel();
            model.ShortTitle = new string('A', 31);

            var results = ValidateModel(model);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(ProgrammeNewProjectViewModel.ShortTitle)));
        }

        [Fact]
        public void CostBookNo_AtMaxDatabaseLength_PassesValidation()
        {
            var model = CreateValidModel();
            model.CostBookNo = new string('A', 50);

            var results = ValidateModel(model);

            Assert.DoesNotContain(results, r => r.MemberNames.Contains(nameof(ProgrammeNewProjectViewModel.CostBookNo)));
        }

        [Fact]
        public void CostBookNo_ExceedingDatabaseLength_FailsValidation()
        {
            var model = CreateValidModel();
            model.CostBookNo = new string('A', 51);

            var results = ValidateModel(model);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(ProgrammeNewProjectViewModel.CostBookNo)));
        }

        [Fact]
        public void Comments_WithLongText_PassesValidation()
        {
            var model = CreateValidModel();
            model.Comments = new string('A', 5000);

            var results = ValidateModel(model);

            Assert.DoesNotContain(results, r => r.MemberNames.Contains(nameof(ProgrammeNewProjectViewModel.Comments)));
        }
    }
}
