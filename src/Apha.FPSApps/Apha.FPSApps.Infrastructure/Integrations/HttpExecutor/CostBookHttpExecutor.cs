namespace Apha.FPSApps.Infrastructure.Integrations.HttpExecutor
{
    public class CostBookHttpExecutor : HttpExecutor, ICostBookHttpExecutor
    {
        public CostBookHttpExecutor(HttpClient http)
            : base(http)
        {
        }
    }
}
