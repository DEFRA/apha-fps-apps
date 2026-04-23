using Apha.Costbook.Core.Entities;
using Apha.Costbook.DataAccess;

namespace Apha.Costbook.Core.Interfaces;

public interface IAdditionalCostRepository
{
    Task<IEnumerable<AdditionalCost>> GetByProjectYearAsync(string project, int year);
    Task<AdditionalCost> AddAsync(AdditionalCost additionalCost);
    Task<AdditionalCost> UpdateAsync(AdditionalCost additionalCost);
    Task<bool> DeleteAsync(int acIdentity);
    Task<IEnumerable<AccountCategoryLookup>> GetProjectSpecificAccountCategoriesAsync();
}

public record AccountCategoryLookup(string AccShortName, bool UseInflation);
