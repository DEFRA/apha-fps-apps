namespace Apha.FPS.Application.Interfaces
{
    public interface ICustomerService
    {
        Task<IEnumerable<string>> GetAllCustomersAsync();
    }
}
