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
        private readonly IFpsRequestContext _requestContext;

        public PurchasesService(IPurchasesRepository repository, IMapper mapper, IFpsRequestContext requestContext)
        {
            _repository = repository;
            _mapper = mapper;
            _requestContext = requestContext ?? throw new ArgumentNullException(nameof(requestContext));
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

            var isAuthorized = await _repository.IsAuthorizedAsync(
                purchase.WorkgroupName, _requestContext.UserEmailId.ToLower());
            if (!isAuthorized)
                throw new UnauthorizedAccessException(
                    $"User does not have access to workgroup '{purchase.WorkgroupName}'.");

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

            var isAuthorized = await _repository.IsAuthorizedAsync(
                purchase.WorkgroupName, _requestContext.UserEmailId.ToLower());
            if (!isAuthorized)
                throw new UnauthorizedAccessException(
                    $"User does not have access to workgroup '{purchase.WorkgroupName}'.");

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

            var isAuthorized = await _repository.IsAuthorizedAsync(
                workgroupName, _requestContext.UserEmailId.ToLower());
            if (!isAuthorized)
                throw new UnauthorizedAccessException(
                    $"User does not have access to workgroup '{workgroupName}'.");

            return await _repository.DeletePurchaseAsync(workgroupName, account, itemDescription);
        }
    }
}
