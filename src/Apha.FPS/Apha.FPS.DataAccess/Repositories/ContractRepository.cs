using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.DataAccess.Repositories
{
    public class ContractRepository : IContractRepository
    {
        private readonly FpsDbContext _dbContext;
        private readonly IFpsRequestContext _requestContext;

        public ContractRepository(FpsDbContext dbContext, IFpsRequestContext requestContext)
        {
            _dbContext = dbContext;
            _requestContext = requestContext;
        }

        public async Task<IEnumerable<Contract>> GetAllContractsAsync()
        {
            return await (from contract in _dbContext.Contracts
                        join userCategory in _dbContext.UserCategories
                            on contract.Category equals userCategory.Category
                        join user in _dbContext.Users
                            on userCategory.UserId equals user.UserId
                          where user.UserEmail != null && user.UserEmail.ToLower() == _requestContext.UserEmailId
                          select contract).AsNoTracking()
                          .ToListAsync(); 
        }

        public async Task<IEnumerable<Contract>> GetAllContractsByUserAsync()
        {
            return await _dbContext.ContractViews
                .Where(c => c.UserEmail != null && c.UserEmail.ToLower() == _requestContext.UserEmailId)
                .Select(c => new Contract
                {
                    ContractNo = c.ContractNo,
                    Category = c.Category,
                    Manager = c.Manager,
                    Customer = c.Customer,
                    Title = c.Title,
                    RegisteredDate = c.RegisteredDate.HasValue ? c.RegisteredDate.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                    StartDate = c.StartDate.HasValue ? c.StartDate.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                    EndDate = c.EndDate.HasValue ? c.EndDate.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                    ContractDoc = c.ContractDoc,
                    Duration = c.Duration,
                    FpsYear = c.FpsYear
                })
                .Distinct()
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
