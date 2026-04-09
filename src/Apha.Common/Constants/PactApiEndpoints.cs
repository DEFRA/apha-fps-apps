namespace Apha.Common.Constants
{
    public static class PactApiEndpoints
    {
        // Job Code
        public const string GetJobCodesByProject = "api/v1/jobcode/project/{0}";
        public const string GetPagedJobCodes = "api/v1/jobcode/paged";
        public const string GetPagedJobCodesByProject = "api/v1/jobcode/paged?parentProject={0}";
        public const string GetJobCodeById = "api/v1/jobcode/{0}";
        public const string GetJobCodeTypes = "api/v1/jobcode/types";
        public const string CreateJobCode = "api/v1/jobcode";
        public const string UpdateJobCode = "api/v1/jobcode";
        public const string DeleteJobCode = "api/v1/jobcode/{0}";

        // Time Code Valid
        public const string GetTimeCodesByJobCode = "api/v1/timecodevalid/jobcode/{0}/project/{1}";
        public const string GetPagedTimeCodes = "api/v1/timecodevalid/paged";
        public const string GetPagedTimeCodesByJobCode = "api/v1/timecodevalid/paged?jobCode={0}";
        public const string GetPagedTimeCodesByProject = "api/v1/timecodevalid/paged?parentProject={0}";
        public const string GetPagedTimeCodesByJobCodeAndProject = "api/v1/timecodevalid/paged?jobCode={0}&parentProject={1}";
        public const string CreateTimeCodeValid = "api/v1/timecodevalid";
        public const string UpdateTimeCodeValid = "api/v1/timecodevalid";
        public const string DeleteTimeCodeValid = "api/v1/timecodevalid/{0}/{1}/{2}";
        public const string DeleteTimeCodesByJobCode = "api/v1/timecodevalid/jobcode/{0}/project/{1}";
        public const string CopyWorkGroup = "api/v1/timecodevalid/copy?sourceJobCode={0}&targetJobCode={1}&parentProject={2}";
        public const string DeleteBulkTimeCodes = "api/v1/timecodevalid/deletebulk";
        public const string CopySelectedWorkGroups = "api/v1/timecodevalid/copybulkworkgroups";

        // Work Group
        public const string GetAllWorkGroups = "api/v1/workgroup";

        // WorkGroup Test Capability
        public const string GetPagedTestCapabilityByWorkGroup = "api/v1/workgrouptestcapability/paged/workgroup";
        public const string GetPagedTestCapabilityByTestCode = "api/v1/workgrouptestcapability/paged/testcode";
        public const string GetTestCapabilityById = "api/v1/workgrouptestcapability/testcapability/{0}/{1}";
        public const string CreateTestCapability = "api/v1/workgrouptestcapability/testcapability";
        public const string UpdateTestCapability = "api/v1/workgrouptestcapability/testcapability";
        public const string DeleteTestCapability = "api/v1/workgrouptestcapability/testcapability/{0}/{1}";

        // Test Reqmt
        public const string GetPagedTestReqmt = "api/v1/workgrouptestcapability/testreqmt/paged/{0}";
        public const string GetAllTestReqmtForExport = "api/v1/workgrouptestcapability/testreqmt/all/{0}";
        public const string GetTestReqmtById = "api/v1/workgrouptestcapability/testreqmt/{0}/{1}";
        public const string CreateTestReqmt = "api/v1/workgrouptestcapability/testreqmt";
        public const string UpdateTestReqmt = "api/v1/workgrouptestcapability/testreqmt";
        public const string DeleteTestReqmt = "api/v1/workgrouptestcapability/testreqmt/{0}/{1}";

        // Lookups
        public const string GetAllTestorProducts = "api/v1/workgrouptestcapability/testorproducts";
        public const string GetTestReqmtPricing = "api/v1/workgrouptestcapability/testreqmt/pricing";
    }
}
