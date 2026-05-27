namespace Apha.Common.Constants
{
    public static class FpsApiEndpoints
    {
        // Animal
        public const string GetAnimalCosts = "api/v1/animal?jobCode={0}";
        public const string GetAnimalLookup = "api/v1/animal/lookup";
        public const string GetAnimalRate = "api/v1/animal/rate?animalType={0}&jobCode={1}";
        public const string CreateAnimalCost = "api/v1/animal";
        public const string UpdateAnimalCost = "api/v1/animal";
        public const string DeleteAnimalCost = "api/v1/animal?indCounter={0}";
        public const string GetTotalAnimalCost = "api/v1/animal/totalanimalcost?jobCode={0}";
        public const string GetAnimalCostViewById = "api/v1/animal/view?indCounter={0}&jobCode={1}";

        // Employee
        public const string GetFilteredEmployees = "api/v1/employee/paginated?filterOption={0}";
        public const string GetEmployeeById = "api/v1/employee/{0}";
        public const string CreateEmployee = "api/v1/employee";
        public const string UpdateEmployee = "api/v1/employee";
        public const string DeleteEmployee = "api/v1/employee/{0}";
        public const string GetAllManagers = "api/v1/employee/managers";
        public const string GetAllPactManagers = "api/v1/employee/pactmanagers";
        public const string GetAllPerson = "api/v1/employee/persons";
        public const string GetWorkGroupStaffPaginated = "api/v1/employee/WorkGroupStaff/paginated";

        // Lookup
        public const string GetAllStatuses = "api/v1/status";
        public const string GetAllDiseases = "api/v1/disease";
        public const string GetAllCustomers = "api/v1/customer";
        public const string GetAllContracts = "api/v1/contract";
        public const string GetContractsByUser = "api/v1/contract/by-user";

        // Division
        public const string GetAllDivisions = "api/v1/division";
        public const string GetPagedDivisions = "api/v1/division/paged";
        public const string GetDivisionByName = "api/v1/division/{0}";
        public const string CreateDivision = "api/v1/division";
        public const string UpdateDivision = "api/v1/division/{0}";
        public const string DeleteDivision = "api/v1/division/{0}";

        // Division Grade
        public const string GetPagedDivisionGrades = "api/v1/DivisionGrade/paged";
        public const string GetDivisionGradeById = "api/v1/DivisionGrade/{0}";
        public const string CreateDivisionGrade = "api/v1/DivisionGrade";
        public const string UpdateDivisionGrade = "api/v1/DivisionGrade/{0}";
        public const string DeleteDivisionGrade = "api/v1/DivisionGrade/{0}";
        public const string GetAllDivisionGrades = "api/v1/DivisionGrade/grades";

        // Program
        public const string GetAllPrograms = "api/v1/program";
        public const string GetPagedPrograms = "api/v1/program/paged";
        public const string GetProgramById = "api/v1/program/{0}";
        public const string CreateProgram = "api/v1/program";
        public const string UpdateProgram = "api/v1/program";
        public const string DeleteProgram = "api/v1/program/{0}";

        // Project
        public const string GetAllProjects = "api/v1/project";
        public const string GetPagedProjects = "api/v1/project/paged";
        public const string GetAllPactProjects = "api/v1/project/pactview/all";
        public const string GetPagedPactProjects = "api/v1/project/pactview";
        public const string GetProjectById = "api/v1/project/{0}";
        public const string CreateProject = "api/v1/project";
        public const string UpdateProject = "api/v1/project";
        public const string UpdatePactProject = "api/v1/project/external/pact";
        public const string UpdatePactPortfolio = "api/v1/project/external/portfolio";
        public const string DeleteProject = "api/v1/project/{0}";
        public const string GetProjectsByProgram = "api/v1/project/paged?programNo={0}";

        // Project Group
        public const string GetAllProjectGroups = "api/v1/projectgroup";
        public const string GetProjectGroupsByUser = "api/v1/projectgroup/by-user";

        // ProgrammeNewProject (merged into project route)
        public const string GetProgrammeNewProjectById = "api/v1/project/{0}";
        public const string CreateProgrammeNewProject = "api/v1/project";
        public const string UpdateProgrammeNewProject = "api/v1/project/{0}";
        public const string DeleteProgrammeNewProjectAndChildren = "api/v1/project/{0}/delete-with-children";
        public const string ChangeProjectCode = "api/v1/project/change-code";
        public const string CheckProjectExists = "api/v1/project/check-exists/{0}";
        public const string GetProgrammeNewProjectManagers = "api/v1/employee/managers";
        public const string GetProgrammeNewProjectCostCentres = "api/v1/costcentre";
        public const string GetProgrammeNewProjectProjectGroups = "api/v1/projectgroup";
        public const string GetProgrammeNewProjectAccountCodes = "api/v1/accountcode";
        public const string GetProgrammeNewProjectSubAccounts = "api/v1/subaccount";

        // Setting
        public const string GetHoursPerDay = "api/v1/setting/hoursperday";

        // Staff Job
        public const string GetAllStaffJobs = "api/v1/staffjob?jobCode={0}";
        public const string GetStaffWorkgroupLookup = "api/v1/staffjob/workgrouplookup";
        public const string GetStaffChargeRate = "api/v1/staffjob/chargerate?staffId={0}&jobcode={1}";
        public const string GetTotalStaffCost = "api/v1/staffjob/totalstaffcost?jobCode={0}";
        public const string GetStaffJobById = "api/v1/staffjob/{0}/{1}";
        public const string CreateStaffJob = "api/v1/staffjob";
        public const string UpdateStaffJob = "api/v1/staffjob";
        public const string DeleteStaffJob = "api/v1/staffjob?staffId={0}&jobcode={1}";
        public const string GetStaffJobViewById = "api/v1/staffjob/view?staffId={0}&jobcode={1}";

        // View Project Plan vs Actual Staff
        public const string GetTimeCostCalcsByProject = "api/v1/timecostcalcs?projectCode={0}";
        public const string GetTimeCostCalcsTotalsByProject = "api/v1/timecostcalcs/totals?projectCode={0}";
        public const string DeleteTimeCostCalcs = "api/v1/timecostcalcs";

        // Additional Cost
        public const string GetAdditionalCosts = "api/v1/additionalcost?jobCode={0}";
        public const string GetTotalItemCost = "api/v1/additionalcost/totalitemcost?jobCode={0}";
        public const string GetAccountCategories = "api/v1/additionalcost/accountcategories";
        public const string GetAdditionalCostById = "api/v1/additionalcost/{0}/{1}/{2}";
        public const string CreateAdditionalCost = "api/v1/additionalcost";
        public const string UpdateAdditionalCost = "api/v1/additionalcost";
        public const string DeleteAdditionalCost = "api/v1/additionalcost?jobCode={0}&account={1}&description={2}";

        // View Project Plan vs Actual Tests
        public const string GetMonthlyOutputByProject = "api/v1/MonthlyOutput?projectCode={0}";
        public const string GetMonthlyOutputTotalsByProject = "api/v1/MonthlyOutput/totals?projectCode={0}";
        public const string DeleteMonthlyOutput = "api/v1/MonthlyOutput";

        // Resource Set-Up – Profit Centres
        public const string GetProfitCentres = "api/v1/profitcentres";
        public const string GetPagedProfitCentres = "api/v1/profitcentres/paged";
        public const string GetProfitCentreById = "api/v1/profitcentres/{0}";
        public const string CreateProfitCentre = "api/v1/profitcentres";
        public const string UpdateProfitCentre = "api/v1/profitcentres/{0}";
        public const string DeleteProfitCentre = "api/v1/profitcentres/{0}";

        // Resource Set-Up — PC Grades
        public const string GetPcGrades = "api/v1/pcgrades?profitCentre={0}";

        // Resource Set-Up — WG Grades
        public const string GetWgGrades = "api/v1/wggrades?pcGrade={0}";

        // Resource Set-Up — WG Staff
        public const string GetWgStaff = "api/v1/wgstaff?wgGrade={0}";
        public const string GetWgEmployeeById = "api/v1/wgstaff/{0}";
        public const string UpdateWgEmployee = "api/v1/wgstaff";
        public const string DeleteWgEmployee = "api/v1/wgstaff/{0}";
        public const string DeleteWgGrade = "api/v1/wggrades/{0}";
    }
}
