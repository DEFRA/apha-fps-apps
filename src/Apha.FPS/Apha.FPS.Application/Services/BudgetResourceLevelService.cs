using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    public class BudgetResourceLevelService : IBudgetResourceLevelService
    {
        private readonly IBudgetResourceLevelRepository _repository;
        private readonly IMapper _mapper;

        public BudgetResourceLevelService(IBudgetResourceLevelRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<WorkGroupViewDto>> GetWorkGroupsAsync(string profitCentre)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(profitCentre);
            var entities = await _repository.GetWorkGroupsAsync(profitCentre);
            return _mapper.Map<List<WorkGroupViewDto>>(entities);
        }

        public async Task<List<BidViewDto>> GetBidViewAsync(string workgroup)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(workgroup);
            var entities = await _repository.GetBidViewAsync(workgroup);
            return _mapper.Map<List<BidViewDto>>(entities);
        }

        public async Task<BidDto?> GetBidByIdAsync(string workgroupName, string account)
        {
            var entity = await _repository.GetBidByIdAsync(workgroupName, account);
            return _mapper.Map<BidDto>(entity);
        }

        public async Task<BidDto> AddBidAsync(BidDto bid)
        {
            ArgumentNullException.ThrowIfNull(bid);
            ArgumentOutOfRangeException.ThrowIfNegative(bid.GenBid);

            var existing = await _repository.GetBidByIdAsync(bid.WorkgroupName, bid.Account);
            if (existing != null)
                throw new InvalidOperationException(
                    $"A bid with Workgroup '{bid.WorkgroupName}' and Account '{bid.Account}' already exists.");

            var entity = _mapper.Map<Bid>(bid);
            var result = await _repository.AddBidAsync(entity);
            return _mapper.Map<BidDto>(result);
        }

        public async Task<BidDto> UpdateBidAsync(BidDto bid)
        {
            ArgumentNullException.ThrowIfNull(bid);
            ArgumentOutOfRangeException.ThrowIfNegative(bid.GenBid);

            var existing = await _repository.GetBidByIdAsync(bid.WorkgroupName, bid.Account);
            if (existing == null)
                throw new InvalidOperationException(
                    $"Bid with Workgroup '{bid.WorkgroupName}' and Account '{bid.Account}' was not found.");

            var entity = _mapper.Map<Bid>(bid);
            var result = await _repository.UpdateBidAsync(entity);
            return _mapper.Map<BidDto>(result);
        }

        public async Task<bool> DeleteBidAsync(string workgroupName, string account)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(workgroupName);
            return await _repository.DeleteBidAsync(workgroupName, account);
        }

        public async Task<List<PurchaseDto>> GetPurchasesAsync(string workgroupName, string account)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(workgroupName);
            var entities = await _repository.GetPurchasesAsync(workgroupName, account);
            return _mapper.Map<List<PurchaseDto>>(entities);
        }

        public async Task<PurchaseDto?> GetPurchaseByIdAsync(string workgroupName, string account, string itemDescription)
        {
            var entity = await _repository.GetPurchaseByIdAsync(workgroupName, account, itemDescription);
            return _mapper.Map<PurchaseDto>(entity);
        }

        public async Task<PurchaseDto> AddPurchaseAsync(PurchaseDto purchase)
        {
            ArgumentNullException.ThrowIfNull(purchase);
            ArgumentOutOfRangeException.ThrowIfNegative(purchase.Amount);

            var existing = await _repository.GetPurchaseByIdAsync(purchase.WorkgroupName, purchase.Account, purchase.ItemDescription);
            if (existing != null)
                throw new InvalidOperationException(
                    $"A purchase with Workgroup '{purchase.WorkgroupName}', Account '{purchase.Account}' and Item Description '{purchase.ItemDescription}' already exists.");

            var entity = _mapper.Map<Purchase>(purchase);
            var result = await _repository.AddPurchaseAsync(entity);
            return _mapper.Map<PurchaseDto>(result);
        }

        public async Task<PurchaseDto> UpdatePurchaseAsync(PurchaseDto purchase)
        {
            ArgumentNullException.ThrowIfNull(purchase);
            ArgumentOutOfRangeException.ThrowIfNegative(purchase.Amount);

            var existing = await _repository.GetPurchaseByIdAsync(
                purchase.WorkgroupName, purchase.Account, purchase.OldItemDescription ?? purchase.ItemDescription);

            if (existing == null)
                throw new InvalidOperationException(
                    $"Purchase with Workgroup '{purchase.WorkgroupName}', Account '{purchase.Account}' and Item Description '{purchase.OldItemDescription ?? purchase.ItemDescription}' was not found.");

            var result = await _repository.UpdatePurchaseAsync(
                purchase.WorkgroupName, purchase.Account,
                purchase.OldItemDescription ?? purchase.ItemDescription,
                purchase.ItemDescription, purchase.Amount);
            return _mapper.Map<PurchaseDto>(result);
        }

        public async Task<bool> DeletePurchaseAsync(string workgroupName, string account, string itemDescription)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(workgroupName);
            return await _repository.DeletePurchaseAsync(workgroupName, account, itemDescription);
        }

        public async Task<List<AccountCategoryDto>> GetAccountCategoriesAsync()
        {
            var categories = await _repository.GetAccountCategoriesAsync();
            return _mapper.Map<List<AccountCategoryDto>>(categories);
        }

        public async Task<List<ProfitCentreDto>> GetProfitCentresAsync()
        {
            var entities = await _repository.GetProfitCentresAsync();
            return _mapper.Map<List<ProfitCentreDto>>(entities);
        }
    }
}
