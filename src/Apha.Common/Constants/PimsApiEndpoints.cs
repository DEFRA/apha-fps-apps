namespace Apha.Common.Constants
{
    public static class PimsApiEndpoints
    {
        // Project List
        public const string GetAllProjects = "api/v1/projectlist";
        public const string GetAllProjectsList = "api/v1/projectlist/AllProjectsList";
        public const string GetFpsProjectById = "api/v1/projectlist/{0}/fps";
        public const string GetProposedProjectById = "api/v1/projectlist/{0}/proposed";
        public const string GetYearlyDetailsByProject = "api/v1/projectlist/{0}/yearly";

        // Proposed Project
        public const string CreateProject = "api/v1/proposedproject";
        public const string GetProjectStatuses = "api/v1/proposedproject/statuses";
        public const string GetProjectPrograms = "api/v1/proposedproject/programs";
        public const string GetProjectCustomers = "api/v1/proposedproject/customers";

        // Project Details
        public const string GetAllRisks = "api/v1/projectdetails/risks";
        public const string GetAllYears = "api/v1/projectdetails/years";
        public const string GetPimsDetail = "api/v1/projectdetails/{0}/pims";
        public const string SavePimsDetail = "api/v1/projectdetails/{0}/pims";
        public const string GetProposedProject = "api/v1/projectdetails/{0}/proposed";
        public const string UpdateProposedProject = "api/v1/projectdetails/{0}/proposed";

        // Project Comment
        public const string GetCommentsByProject = "api/v1/projectcomment";
        public const string GetCommentById = "api/v1/projectcomment/{0}";
        public const string CreateComment = "api/v1/projectcomment";
        public const string UpdateComment = "api/v1/projectcomment/{0}";
        public const string DeleteComment = "api/v1/projectcomment/{0}";
        public const string GetCommentTopics = "api/v1/projectcomment/commenttopics";

        // Project Year Costs
        public const string GetAdditionalActuals = "api/v1/projectyearcosts/{0}/{1}/additionalactuals";
        public const string GetAdditionalPlans = "api/v1/projectyearcosts/{0}/{1}/additionalplans";
        public const string GetAnimalActuals = "api/v1/projectyearcosts/{0}/{1}/animalactuals";
        public const string GetAnimalPlans = "api/v1/projectyearcosts/{0}/{1}/animalplans";
        public const string GetTestActuals = "api/v1/projectyearcosts/{0}/{1}/testactuals";
        public const string GetTestPlans = "api/v1/projectyearcosts/{0}/{1}/testplans";
        public const string GetStaffPlans = "api/v1/projectyearcosts/{0}/{1}/staffplans";
        public const string GetStaffActuals = "api/v1/projectyearcosts/{0}/{1}/staffactuals";
        public const string GetProjectYearDetails = "api/v1/projectyearcosts/{0}/{1}/projectyeardetails";
        public const string GetPactPay = "api/v1/projectyearcosts/{0}/{1}/pactpay";
        public const string GetMonthlyPactData = "api/v1/projectyearcosts/{0}/{1}/monthlypactdata";
        public const string GetFpsYearTotals = "api/v1/projectyearcosts/{0}/{1}/fpsyeartotals";
        public const string ExportProjectYearCostsToExcel = "api/v1/projectyearcosts/{0}/{1}/export-excel";
    }
}