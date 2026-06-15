using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    public class PurchasesService : IPurchasesService
    {
        private readonly IPurchasesRepository _repository;
        private readonly IMapper _mapper;

        public PurchasesService(IPurchasesRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper     = mapper     ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<List<PurchaseDto>> GetPurchasesAsync(string WorkGroupName, string account)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(WorkGroupName);
            var entities = await _repository.GetPurchasesAsync(WorkGroupName, account);
            return _mapper.Map<List<PurchaseDto>>(entities);
        }

        public async Task<PurchaseDto?> GetPurchaseByIdAsync(string WorkGroupName, string account, string itemDescription)
        {
            var entity = await _repository.GetPurchaseByIdAsync(WorkGroupName, account, itemDescription);
            return _mapper.Map<PurchaseDto>(entity);
        }

        public Task<PurchaseDto> AddPurchaseAsync(PurchaseDto purchase)
        {
            ArgumentNullException.ThrowIfNull(purchase);
            ArgumentOutOfRangeException.ThrowIfNegative(purchase.Amount);
            return AddPurchaseAsyncCore(purchase);
        }

        private async Task<PurchaseDto> AddPurchaseAsyncCore(PurchaseDto purchase)
        {
            var existing = await _repository.GetPurchaseByIdAsync(purchase.WorkGroupName, purchase.Account, purchase.ItemDescription);
            if (existing != null)
                throw new InvalidOperationException(
                    $"A purchase with Workgroup '{purchase.WorkGroupName}', Account '{purchase.Account}' and Item Description '{purchase.ItemDescription}' already exists.");

            var entity = _mapper.Map<Purchase>(purchase);
            var result = await _repository.AddPurchaseAsync(entity);
            return _mapper.Map<PurchaseDto>(result);
        }

        public Task<PurchaseDto> UpdatePurchaseAsync(PurchaseDto purchase)
        {
            ArgumentNullException.ThrowIfNull(purchase);
            ArgumentOutOfRangeException.ThrowIfNegative(purchase.Amount);
            return UpdatePurchaseAsyncCore(purchase);
        }

        private async Task<PurchaseDto> UpdatePurchaseAsyncCore(PurchaseDto purchase)
        {
            var existing = await _repository.GetPurchaseByIdAsync(
                purchase.WorkGroupName, purchase.Account, purchase.OldItemDescription ?? purchase.ItemDescription);

            if (existing == null)
                throw new InvalidOperationException(
                    $"Purchase with Workgroup '{purchase.WorkGroupName}', Account '{purchase.Account}' and Item Description '{purchase.OldItemDescription ?? purchase.ItemDescription}' was not found.");

            var result = await _repository.UpdatePurchaseAsync(
                purchase.WorkGroupName, purchase.Account,
                purchase.OldItemDescription ?? purchase.ItemDescription,
                purchase.ItemDescription, purchase.Amount);
            return _mapper.Map<PurchaseDto>(result);
        }

        public Task<bool> DeletePurchaseAsync(string WorkGroupName, string account, string itemDescription)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(WorkGroupName);
            return DeletePurchaseAsyncCore(WorkGroupName, account, itemDescription);
        }

        private async Task<bool> DeletePurchaseAsyncCore(string WorkGroupName, string account, string itemDescription)
        {
            return await _repository.DeletePurchaseAsync(WorkGroupName, account, itemDescription);
        }
    }
}
