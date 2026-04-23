namespace Apha.PACT.Core.Interfaces
{
    public interface ITestorProductRepository
    {
        Task<Dictionary<string, string?>> GetDescriptionsByCodesAsync(IEnumerable<string> itemCodes);
    }
}
