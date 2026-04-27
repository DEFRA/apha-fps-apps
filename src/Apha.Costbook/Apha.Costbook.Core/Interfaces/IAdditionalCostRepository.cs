using Apha.Costbook.Core.Entities;
using Apha.Costbook.DataAccess;

namespace Apha.Costbook.Core.Interfaces;

public interface IAdditionalCostRepository
{
    Task<IEnumerable<AdditionalCostDetailView>> GetAdditionalCostsByProjectYearAsync(string project, int year);
    Task<AdditionalCost> AddAdditionalCostAsync(AdditionalCost additionalCost);
    Task<AdditionalCost> UpdateAdditionalCostAsync(AdditionalCost additionalCost);
    Task<bool> DeleteAdditionalCostAsync(int acIdentity);
    Task<IEnumerable<AccountCategoryLookup>> GetProjectSpecificAccountCategoriesAsync();
}

public record AccountCategoryLookup(string AccShortName, bool UseInflation);
