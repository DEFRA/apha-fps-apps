using Apha.FPSApps.Application.Interfaces.CostBookApiClients;

namespace Apha.FPSApps.Application.Interfaces.CostBookApiClients
{
    public interface ICostBookApiClient
    {
        ICostBookProjectApiClient Projects { get; }
        ICostBookCustomerApiClient Customers { get; }
        ICostBookDiseaseApiClient Diseases { get; }
        ICostBookProgramApiClient Programs { get; }
        ICostBookStaffApiClient Staff { get; }
        ICostBookContractApiClient Contracts { get; }
        ICostBookYearlyDetailsApiClient YearlyDetails { get; }
        ICostBookProjectSummaryApiClient ProjectSummary { get; }
    }
}
