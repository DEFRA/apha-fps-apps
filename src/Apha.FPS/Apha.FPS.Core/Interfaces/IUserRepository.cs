using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<PagedData<User>> GetAllUsersPagedAsync(PaginationParameters<string> query);
        Task<PagedData<User>> GetNonSuperUsersPagedAsync(PaginationParameters<string> query);
        Task<User?> GetUserByIdAsync(int userId);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User> AddUserAsync(User entity);
        Task<User> UpdateUserAsync(User entity);
        Task<bool> DeleteUserAsync(int userId);

        Task<List<string>> GetUserProfitCentresAsync(int userId);
        Task<List<string>> GetUserProgramsAsync(int userId);
        Task<List<string>> GetUserCategoriesAsync(int userId);
        Task<List<string>> GetUserTestOwnersAsync(int userId);
        Task<List<string>> GetUserProjectGroupsAsync(int userId);

        Task SaveUserPermissionsAsync(int userId, List<string> profitCentres, List<string> programs,
            List<string> categories, List<string> testOwners, List<string> projectGroups);

        Task<List<string>> GetAllProfitCentreOptionsAsync();
        Task<List<string>> GetAllProgramOptionsAsync();
        Task<List<string>> GetAllCategoryOptionsAsync();
        Task<List<string>> GetAllTestOwnerOptionsAsync();
        Task<List<string>> GetAllProjectGroupOptionsAsync();
    }
}
