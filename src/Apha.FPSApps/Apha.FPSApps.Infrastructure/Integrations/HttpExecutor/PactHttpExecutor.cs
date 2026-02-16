namespace Apha.FPSApps.Infrastructure.Integrations.HttpExecutor
{
    public class PactHttpExecutor : HttpExecutor, IPactHttpExecutor
    {
        public PactHttpExecutor(HttpClient http)
            : base(http)
        {
        }
    }
}
