namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsApiClient
    {
        IFpsStaffJobApiClient FpsStaffJob { get; }
        IFpsEmployeeApiClient FpsEmployee { get; }
        IFpsProgramApiClient FpsProgram { get; }
        IFpsProjectApiClient FpsProject { get; }
        IFpsLookupApiClient FpsLookup { get; }
        IFpsAnimalPlanApiClient FpsAnimalPlan { get; }
        IFpsSettingApiClient FpsSetting { get; }
        IFpsYearMasterApiClient FpsYearMaster { get; }
        IFpsProjectStaffPlanActualApiClient FpsProjectStaffPlanActual { get; }
        IFpsDivisionApiClient FpsDivision { get; }
        IFpsAgencyApiClient FpsAgency { get; }
        IFpsAdditionalCostApiClient FpsAdditionalCost { get; }
        IFpsWorkgroupGradeApiClient FpsWorkgroupGrade { get; }
    }
}
