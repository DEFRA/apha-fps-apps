namespace Apha.FPSApps.Application.Interfaces.PactApiClients
{
    public interface IPactApiClient
    {
        IPactJobCodeApiClient PactJobCode { get; }
        IPactTimeCodeValidApiClient PactTimeCodeValid { get; }
        IPactWorkGroupApiClient PactWorkGroup { get; }
        IPactWorkGroupTestCapabilityApiClient PactWorkGroupTestCapability { get; }
    }
}
