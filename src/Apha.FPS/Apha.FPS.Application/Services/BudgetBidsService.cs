using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    public class BudgetBidsService : IBudgetBidsService
    {
        private readonly IBudgetBidsRepository _repository;
        private readonly IMapper _mapper;
        private readonly IFpsRequestContext _requestContext;

        public BudgetBidsService(IBudgetBidsRepository repository, IMapper mapper, IFpsRequestContext requestContext)
        {
            _repository = repository;
            _mapper = mapper;
            _requestContext = requestContext ?? throw new ArgumentNullException(nameof(requestContext));
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

            var isAuthorized = await _repository.IsAuthorizedAsync(bid.WorkgroupName);
            if (!isAuthorized)
                throw new UnauthorizedAccessException(
                    $"User does not have access to workgroup '{bid.WorkgroupName}'.");

            var existing = await _repository.GetBidByIdAsync(bid.WorkgroupName, bid.Account);
            if (existing != null)
                throw new InvalidOperationException("Account already exists.");

            var entity = _mapper.Map<Bid>(bid);
            var result = await _repository.AddBidAsync(entity);
            return _mapper.Map<BidDto>(result);
        }

        public async Task<BidDto> UpdateBidAsync(BidDto bid)
        {
            ArgumentNullException.ThrowIfNull(bid);
            ArgumentOutOfRangeException.ThrowIfNegative(bid.GenBid);

            var isAuthorized = await _repository.IsAuthorizedAsync(bid.WorkgroupName);
            if (!isAuthorized)
                throw new UnauthorizedAccessException(
                    $"User does not have access to workgroup '{bid.WorkgroupName}'.");

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

            var isAuthorized = await _repository.IsAuthorizedAsync(workgroupName);
            if (!isAuthorized)
                throw new UnauthorizedAccessException(
                    $"User does not have access to workgroup '{workgroupName}'.");

            return await _repository.DeleteBidAsync(workgroupName, account);
        }

        public async Task<List<AccountCategoryDto>> GetAccountCategoriesAsync()
        {
            var categories = await _repository.GetAccountCategoriesAsync();
            return _mapper.Map<List<AccountCategoryDto>>(categories);
        }
    }
}
