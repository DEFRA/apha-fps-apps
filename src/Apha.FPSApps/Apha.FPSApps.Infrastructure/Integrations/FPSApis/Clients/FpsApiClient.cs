using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsApiClient : IFpsApiClient
    {
        public IFpsStaffJobApiClient FpsStaffJob { get; }
        public IFpsEmployeeApiClient FpsEmployee { get; }
        public IFpsProgramApiClient FpsProgram { get; }
        public IFpsProjectApiClient FpsProject { get; }
        public IFpsLookupApiClient FpsLookup { get; }
        public IFpsAnimalPlanApiClient FpsAnimalPlan { get; }
        public IFpsSettingApiClient FpsSetting { get; }

        public IFpsYearMasterApiClient FpsYearMaster { get; }

        public IFpsTestorProductApiClient FpsTestorProduct { get; }
        public IFpsProjectStaffPlanActualApiClient FpsProjectStaffPlanActual { get; }

        public IFpsDivisionApiClient FpsDivision { get; }

        public IFpsAgencyApiClient FpsAgency { get; }

        public IFpsProfitCentreApiClient FpsProfitCentre { get; }
        public IFpsResourceCentreGradeApiClient FpsResourceCentreGrade { get; }
        public IFpsWorkGroupGradeApiClient FpsWorkGroupGrade { get; }
        public IFpsWorkGroupEmployeeApiClient FpsWorkGroupEmployee { get; }

        public FpsApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            FpsStaffJob = new FpsStaffJobApiClient(http, mapper);
            FpsEmployee = new FpsEmployeeApiClient(http, mapper);
            FpsProgram = new FpsProgramApiClient(http, mapper);
            FpsProject = new FpsProjectApiClient(http, mapper);
            FpsLookup = new FpsLookupApiClient(http, mapper);
            FpsAnimalPlan = new FpsAnimalPlanApiClient(http, mapper);
            FpsSetting = new FpsSettingApiClient(http, mapper);
            FpsYearMaster = new FpsYearMasterApiClient(http, mapper);
            FpsTestorProduct = new FpsTestorProductApiClient(http, mapper);
            FpsProjectStaffPlanActual = new FpsProjectStaffPlanActualApiClient(http, mapper);
            FpsDivision = new FpsDivisionApiClient(http, mapper);
            FpsAgency = new FpsAgencyApiClient(http, mapper);
            FpsProfitCentre = new FpsProfitCentreApiClient(http, mapper);
            FpsResourceCentreGrade = new FpsResourceCentreGradeApiClient(http, mapper);
            FpsWorkGroupGrade = new FpsWorkGroupGradeApiClient(http, mapper);
            FpsWorkGroupEmployee = new FpsWorkGroupEmployeeApiClient(http, mapper);
        }
    }
}
