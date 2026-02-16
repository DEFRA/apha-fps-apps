namespace Apha.FPSApps.Infrastructure.Integrations.HttpExecutor
{
    public class PimsHttpExecutor : HttpExecutor, IPimsHttpExecutor
    {
        public PimsHttpExecutor(HttpClient http)
            : base(http)
        {
        }
    }
}
