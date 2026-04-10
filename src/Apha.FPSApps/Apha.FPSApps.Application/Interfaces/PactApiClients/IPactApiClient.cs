namespace Apha.FPSApps.Application.Interfaces.PactApiClients
{
    public interface IPactApiClient
    {
        IPactJobCodeApiClient PactJobCode { get; }
        IPactTimeCodeValidApiClient PactTimeCodeValid { get; }
        IPactWorkGroupApiClient PactWorkGroup { get; }
        IPactProjectInvoiceApiClient PactProjectInvoice { get; }
        IPactProjectSubContractApiClient PactProjectSubContract { get; }
        IPactWorkGroupTestCapabilityApiClient PactWorkGroupTestCapability { get; }
        IPactTestRequirementApiClient PactTestRequirement { get; }
    }
}
