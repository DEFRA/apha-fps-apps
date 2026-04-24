namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
{
    public interface IPimsApiClient
    {

        IPimsProjectListApiClient PimsProjectList { get; }
        IPimsProjectDetailsApiClient PimsProjectDetails { get; }
        IPimsProjectCommentApiClient PimsProjectComment { get; }
    }
}
