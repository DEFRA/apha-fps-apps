using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;

namespace Apha.FPSApps.Web.Areas.PACT.Dependencies
{
    public interface IMonthlyPactControllerDependencies
    {
        IWorkGroupService WorkGroupService { get; }
        IEmployeeService EmployeeService { get; }
        IPactTimeCodeValidService TimeCodeValidService { get; }
        IMonthService MonthService { get; }
        ITestCapabilityService TestCapabilityService { get; }
        ITestRequirementService TestRequirementService { get; }
    }

    public class MonthlyPactControllerDependencies : IMonthlyPactControllerDependencies
    {
        public MonthlyPactControllerDependencies(
            IWorkGroupService workGroupService,
            IEmployeeService employeeService,
            IPactTimeCodeValidService timeCodeValidService,
            IMonthService monthService,
            ITestCapabilityService testCapabilityService,
            ITestRequirementService testRequirementService)
        {
            WorkGroupService = workGroupService;
            EmployeeService = employeeService;
            TimeCodeValidService = timeCodeValidService;
            MonthService = monthService;
            TestCapabilityService = testCapabilityService;
            TestRequirementService = testRequirementService;
        }

        public IWorkGroupService WorkGroupService { get; }
        public IEmployeeService EmployeeService { get; }
        public IPactTimeCodeValidService TimeCodeValidService { get; }
        public IMonthService MonthService { get; }
        public ITestCapabilityService TestCapabilityService { get; }
        public ITestRequirementService TestRequirementService { get; }
    }
}
