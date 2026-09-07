using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    public class MasterLookupService : IMasterLookupService
    {
        private readonly IMasterLookupRepository _masterLookupRepository;
        private readonly IMapper _mapper;

        public MasterLookupService(IMasterLookupRepository masterLookupRepository, IMapper mapper)
        {
            _masterLookupRepository = masterLookupRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<string>> GetAllMasterLookupsAsync()
        {
            var masterLookups = await _masterLookupRepository.GetAllMasterLookupsAsync();
            return masterLookups.Select(m => m.MasterTableName);
        }

        public Task<IEnumerable<string>> GetLookupItemsAsync(string tableName)
        {
            return _masterLookupRepository.GetLookupItemsAsync(tableName);
        }

        public async Task<PaginatedResult<string>> GetLookupItemsPagedAsync(string tableName, QueryParameters<string> query)
        {
            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var result = await _masterLookupRepository.GetLookupItemsPagedAsync(tableName, filter);
            return _mapper.Map<PaginatedResult<string>>(result);
        }

        public Task<bool> CreateLookupItemAsync(string tableName, string value)
        {
            return _masterLookupRepository.CreateLookupItemAsync(tableName, value);
        }

        public Task<bool> UpdateLookupItemAsync(string tableName, string originalValue, string newValue)
        {
            return _masterLookupRepository.UpdateLookupItemAsync(tableName, originalValue, newValue);
        }

        public Task<bool> DeleteLookupItemAsync(string tableName, string value)
        {
            return _masterLookupRepository.DeleteLookupItemAsync(tableName, value);
        }
    }
}
