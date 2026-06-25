using Apha.FPSApps.Web.Areas.FPS.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Xunit;

namespace Apha.FPSApps.Web.UnitTests.Models.FPS.ProjectProfitabilityVla
{
    public class ProjectProfitabilityVlaItemTests
    {
        [Fact]
        public void ProjectProfitabilityVlaItem_AllProperties_GetAndSetCorrectly()
        {
            // Arrange & Act
            var item = new ProjectProfitabilityVlaItem
            {
                Id              = 42,
                JobCode         = "PP001",
                Program         = "PROG01",
                Customer        = "ACME Ltd",
                StaffCosts      = 1000m,
                TestCost        = 200m,
                AnimalCosts     = 150m,
                AdditionalCosts = 50m,
                TotalCosts      = 1400m,
                Budget          = 5000m,
                Profit          = 3600m,
                TargetProfit    = 3000m,
                OffTarget       = 600m,
                Manager         = "John Smith",
                Status          = "Approved"
            };

            // Assert
            Assert.Equal(42,           item.Id);
            Assert.Equal("PP001",      item.JobCode);
            Assert.Equal("PROG01",     item.Program);
            Assert.Equal("ACME Ltd",   item.Customer);
            Assert.Equal(1000m,        item.StaffCosts);
            Assert.Equal(200m,         item.TestCost);
            Assert.Equal(150m,         item.AnimalCosts);
            Assert.Equal(50m,          item.AdditionalCosts);
            Assert.Equal(1400m,        item.TotalCosts);
            Assert.Equal(5000m,        item.Budget);
            Assert.Equal(3600m,        item.Profit);
            Assert.Equal(3000m,        item.TargetProfit);
            Assert.Equal(600m,         item.OffTarget);
            Assert.Equal("John Smith", item.Manager);
            Assert.Equal("Approved",   item.Status);
        }

        [Fact]
        public void ProjectProfitabilityVlaItem_DefaultValues_AreCorrect()
        {
            // Arrange & Act
            var item = new ProjectProfitabilityVlaItem();

            // Assert — nullable properties default to null; value types default to 0
            Assert.Null(item.Id);
            Assert.Null(item.Program);
            Assert.Null(item.Customer);
            Assert.Null(item.Manager);
            Assert.Null(item.Status);
            Assert.Null(item.Budget);
            Assert.Equal(0m, item.StaffCosts);
            Assert.Equal(0m, item.TestCost);
            Assert.Equal(0m, item.AnimalCosts);
            Assert.Equal(0m, item.AdditionalCosts);
            Assert.Equal(0m, item.TotalCosts);
            Assert.Equal(0m, item.Profit);
            Assert.Equal(0m, item.TargetProfit);
            Assert.Equal(0m, item.OffTarget);
        }

        [Fact]
        public void ProjectProfitabilityVlaItem_OffTarget_AcceptsNegativeValue()
        {
            // Arrange & Act
            var item = new ProjectProfitabilityVlaItem { OffTarget = -500m };

            // Assert — negative off-target is a valid business value (triggers red highlight)
            Assert.Equal(-500m, item.OffTarget);
        }

        [Fact]
        public void ProjectProfitabilityVlaItem_Budget_AcceptsNull()
        {
            // Arrange & Act
            var item = new ProjectProfitabilityVlaItem { Budget = null };

            // Assert — budget is nullable (projects may have no budget set)
            Assert.Null(item.Budget);
        }
    }

    public class ProjectProfitabilityVlaViewModelTests
    {
        [Fact]
        public void ProjectProfitabilityVlaViewModel_DefaultValues_AreCorrect()
        {
            // Arrange & Act
            var model = new ProjectProfitabilityVlaViewModel();

            // Assert — string filters default to empty; lists default to empty collections
            Assert.Equal(string.Empty, model.SelectedStatus);
            Assert.Equal(string.Empty, model.SelectedProgram);
            Assert.Equal(string.Empty, model.SelectedManager);
            Assert.Equal(string.Empty, model.SelectedCustomer);
            Assert.NotNull(model.StatusList);
            Assert.Empty(model.StatusList);
            Assert.NotNull(model.ProgramList);
            Assert.Empty(model.ProgramList);
            Assert.NotNull(model.ManagerList);
            Assert.Empty(model.ManagerList);
            Assert.NotNull(model.CustomerList);
            Assert.Empty(model.CustomerList);
            Assert.NotNull(model.ProfitabilityVlaGrid);
            Assert.Null(model.TotalStaffCosts);
            Assert.Null(model.TotalTestCost);
            Assert.Null(model.TotalAnimalCosts);
            Assert.Null(model.TotalAdditionalCosts);
            Assert.Null(model.TotalTotalCosts);
            Assert.Null(model.TotalBudget);
            Assert.Null(model.TotalProfit);
            Assert.Null(model.TotalTargetProfit);
            Assert.Null(model.TotalOffTarget);
        }

        [Fact]
        public void ProjectProfitabilityVlaViewModel_AllFilterProperties_GetAndSetCorrectly()
        {
            // Arrange & Act
            var model = new ProjectProfitabilityVlaViewModel
            {
                SelectedStatus   = "Approved",
                SelectedProgram  = "P001",
                SelectedManager  = "John Smith",
                SelectedCustomer = "ACME Ltd"
            };

            // Assert
            Assert.Equal("Approved",   model.SelectedStatus);
            Assert.Equal("P001",       model.SelectedProgram);
            Assert.Equal("John Smith", model.SelectedManager);
            Assert.Equal("ACME Ltd",   model.SelectedCustomer);
        }

        [Fact]
        public void ProjectProfitabilityVlaViewModel_AllSummaryTotals_GetAndSetCorrectly()
        {
            // Arrange & Act
            var model = new ProjectProfitabilityVlaViewModel
            {
                TotalStaffCosts      = 3000m,
                TotalTestCost        = 500m,
                TotalAnimalCosts     = 300m,
                TotalAdditionalCosts = 150m,
                TotalTotalCosts      = 3950m,
                TotalBudget          = 11000m,
                TotalProfit          = 7050m,
                TotalTargetProfit    = 6500m,
                TotalOffTarget       = 550m
            };

            // Assert
            Assert.Equal(3000m,  model.TotalStaffCosts);
            Assert.Equal(500m,   model.TotalTestCost);
            Assert.Equal(300m,   model.TotalAnimalCosts);
            Assert.Equal(150m,   model.TotalAdditionalCosts);
            Assert.Equal(3950m,  model.TotalTotalCosts);
            Assert.Equal(11000m, model.TotalBudget);
            Assert.Equal(7050m,  model.TotalProfit);
            Assert.Equal(6500m,  model.TotalTargetProfit);
            Assert.Equal(550m,   model.TotalOffTarget);
        }

        [Fact]
        public void ProjectProfitabilityVlaViewModel_DropdownLists_AcceptItems()
        {
            // Arrange
            var model = new ProjectProfitabilityVlaViewModel();
            var statusItem   = new SelectListItem { Value = "Approved",  Text = "Approved" };
            var programItem  = new SelectListItem { Value = "P001",      Text = "P001 — Program One" };
            var managerItem  = new SelectListItem { Value = "John",      Text = "John" };
            var customerItem = new SelectListItem { Value = "ACME Ltd",  Text = "ACME Ltd" };

            // Act
            model.StatusList.Add(statusItem);
            model.ProgramList.Add(programItem);
            model.ManagerList.Add(managerItem);
            model.CustomerList.Add(customerItem);

            // Assert
            Assert.Single(model.StatusList);
            Assert.Single(model.ProgramList);
            Assert.Single(model.ManagerList);
            Assert.Single(model.CustomerList);
            Assert.Equal("Approved",         model.StatusList[0].Value);
            Assert.Equal("P001 — Program One", model.ProgramList[0].Text);
        }

        [Fact]
        public void ProjectProfitabilityVlaViewModel_TotalOffTarget_AcceptsNegativeValue()
        {
            // Arrange & Act
            var model = new ProjectProfitabilityVlaViewModel { TotalOffTarget = -1250m };

            // Assert — negative off-target triggers the fps-profit-offtarget CSS class on the summary input
            Assert.Equal(-1250m, model.TotalOffTarget);
        }
    }
}
