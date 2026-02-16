namespace Apha.FPSApps.Infrastructure.Integrations.HttpExecutor
{
    public class FpsHttpExecutor : HttpExecutor, IFpsHttpExecutor
    {
        public FpsHttpExecutor(HttpClient http)
            : base(http)
        {
        }
    }
}
