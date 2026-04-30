namespace Apha.Common.Constants
{
    public static class CostBookApiEndpoints
    {
        // Projects
        public const string GetFilteredProjects = "api/v1/projects/paginated";
        public const string GetProjectById = "api/v1/projects/{0}";
        public const string AddProject = "api/v1/projects";
        public const string UpdateProject = "api/v1/projects/{0}";
        public const string DeleteProject = "api/v1/projects/{0}/delete";
        public const string CopyProject = "api/v1/projects/{0}/copy";
        public const string RecostProject = "api/v1/projects/{0}/recost";
        public const string GetNextProjectNumber = "api/v1/projects/number";
        public const string GetAllCustomers = "api/v1/projects/customers";
        public const string GetAllDiseases = "api/v1/projects/diseases";
        public const string GetAllPrograms = "api/v1/projects/programs";
        public const string GetAllStaff = "api/v1/projects/staff";
        public const string GetAllContracts = "api/v1/projects/contracts";

        // Yearly Details – Project Header & Years
        public const string GetProjectHeader = "api/v1/yearlydetails/{0}/header";
        public const string GetProjectYears = "api/v1/yearlydetails/{0}/years";
        public const string AddProjectYear = "api/v1/yearlydetails/{0}/years";
        public const string UpdateProjectYear = "api/v1/yearlydetails/{0}/years/{1}";
        public const string DeleteProjectYear = "api/v1/yearlydetails/{0}/years/{1}";

        // Yearly Details – Staff
        public const string GetStaffRequirements = "api/v1/yearlydetails/{0}/years/{1}/staff";
        public const string AddStaffRequirement = "api/v1/yearlydetails/{0}/years/{1}/staff";
        public const string UpdateStaffRequirement = "api/v1/yearlydetails/{0}/years/{1}/staff/{2}";
        public const string DeleteStaffRequirement = "api/v1/yearlydetails/{0}/years/{1}/staff/{2}";

        // Yearly Details – Tests
        public const string GetTestRequirements = "api/v1/yearlydetails/{0}/years/{1}/tests";
        public const string AddTestRequirement = "api/v1/yearlydetails/{0}/years/{1}/tests";
        public const string UpdateTestRequirement = "api/v1/yearlydetails/{0}/years/{1}/tests/{2}";
        public const string DeleteTestRequirement = "api/v1/yearlydetails/{0}/years/{1}/tests/{2}";

        // Yearly Details – Animals
        public const string GetAnimalRequirements = "api/v1/yearlydetails/{0}/years/{1}/animals";
        public const string AddAnimalRequirement = "api/v1/yearlydetails/{0}/years/{1}/animals";
        public const string UpdateAnimalRequirement = "api/v1/yearlydetails/{0}/years/{1}/animals/{2}";
        public const string DeleteAnimalRequirement = "api/v1/yearlydetails/{0}/years/{1}/animals/{2}";

        // Yearly Details – Additional Costs
        public const string GetAdditionalCosts = "api/v1/yearlydetails/{0}/years/{1}/additionalcosts";
        public const string AddAdditionalCost = "api/v1/yearlydetails/{0}/years/{1}/additionalcosts";
        public const string UpdateAdditionalCost = "api/v1/yearlydetails/{0}/years/{1}/additionalcosts/{2}";
        public const string DeleteAdditionalCost = "api/v1/yearlydetails/{0}/years/{1}/additionalcosts/{2}";

        // Yearly Details – Lookups
        public const string GetPayRates = "api/v1/yearlydetails/lookups/payrates";
        public const string GetAnimalRates = "api/v1/yearlydetails/lookups/animalrates";
        public const string GetAccountCategories = "api/v1/yearlydetails/lookups/accountcategories";
        public const string GetTestCodeLookups = "api/v1/yearlydetails/lookups/testcodes";
        public const string GetAllAnimals = "api/v1/yearlydetails/lookups/animals";
    }
}