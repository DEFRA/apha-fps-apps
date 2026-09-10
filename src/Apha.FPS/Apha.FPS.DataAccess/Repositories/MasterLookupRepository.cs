using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Dynamic;

namespace Apha.FPS.DataAccess.Repositories
{
    public class MasterLookupRepository : BaseRepository, IMasterLookupRepository
    {
        private readonly FpsDbContext _dbContext;
        public MasterLookupRepository(FpsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<MasterLookup>> GetAllMasterLookupsAsync()
        {
            return await _dbContext.MasterLookups
           .AsNoTracking()
           .OrderBy(m => m.MasterTableName)
           .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetLookupItemsAsync(string tableName)
        {
            switch (NormalizeTableName(tableName))
            {
                case DirectorateTable:
                    return await _dbContext.Directorates
                        .AsNoTracking()
                        .OrderBy(d => d.DirectorateName)
                        .Select(d => d.DirectorateName)
                    .ToListAsync();

                case DiseaseTable:
                    return await _dbContext.Diseases
                        .AsNoTracking()
                        .OrderBy(d => d.DiseaseName)
                        .Select(d => d.DiseaseName)
                        .ToListAsync();

                case CustomerTable:
                    return await _dbContext.Customers
                        .AsNoTracking()
                        .OrderBy(c => c.CustomerName)
                        .Select(c => c.CustomerName)
                        .ToListAsync();

                default:
                    throw UnknownTable(tableName);
            }
        }

        public async Task<PagedData<string>> GetLookupItemsPagedAsync(string tableName, PaginationParameters<string> query)
        {
            ArgumentNullException.ThrowIfNull(query);

            var baseQuery = GetLookupQuery(tableName);

            baseQuery = ApplyLookupFilter(baseQuery, query.Filter);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                baseQuery = baseQuery.Where(v => EF.Functions.ILike(v, $"%{query.Search}%"));
            }

            baseQuery = ApplyLookupSorting(baseQuery, query.SortBy, query.Descending);

            return await ApplyPagingAsync(baseQuery, query.Page, query.PageSize);
        }

        public async Task<bool> CreateLookupItemAsync(string tableName, string value)
        {
            switch (NormalizeTableName(tableName))
            {
                case DirectorateTable:
                    _dbContext.Directorates.Add(new Directorate { DirectorateName = value });
                    break;
                case DiseaseTable:
                    _dbContext.Diseases.Add(new Disease { DiseaseName = value });
                    break;
                case CustomerTable:
                    _dbContext.Customers.Add(new Customer { CustomerName = value });
                    break;
                default:
                    throw UnknownTable(tableName);
            }

            return await _dbContext.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateLookupItemAsync(string tableName, string originalValue, string newValue)
        {
            // The lookup value is the primary key, so update via ExecuteUpdate to avoid
            // key-change tracking issues that arise when mutating a tracked entity's key.
            return NormalizeTableName(tableName) switch
            {
                DirectorateTable => await _dbContext.Directorates
                                        .Where(d => d.DirectorateName == originalValue)
                                        .ExecuteUpdateAsync(s => s.SetProperty(d => d.DirectorateName, newValue)) > 0,
                DiseaseTable => await _dbContext.Diseases
                                        .Where(d => d.DiseaseName == originalValue)
                                        .ExecuteUpdateAsync(s => s.SetProperty(d => d.DiseaseName, newValue)) > 0,
                CustomerTable => await _dbContext.Customers
                                        .Where(c => c.CustomerName == originalValue)
                                        .ExecuteUpdateAsync(s => s.SetProperty(c => c.CustomerName, newValue)) > 0,
                _ => throw UnknownTable(tableName),
            };
        }

        public async Task<bool> DeleteLookupItemAsync(string tableName, string value)
        {
            return NormalizeTableName(tableName) switch
            {
                DirectorateTable => await _dbContext.Directorates
                    .Where(d => d.DirectorateName == value)
                    .ExecuteDeleteAsync() > 0,
                DiseaseTable => await _dbContext.Diseases
                    .Where(d => d.DiseaseName == value)
                    .ExecuteDeleteAsync() > 0,
                CustomerTable => await _dbContext.Customers
                    .Where(c => c.CustomerName == value)
                    .ExecuteDeleteAsync() > 0,
                _ => throw UnknownTable(tableName),
            };
        }

        private const string DirectorateTable = "directorate";
        private const string DiseaseTable = "disease";
        private const string CustomerTable = "customer";

        private IQueryable<string> GetLookupQuery(string tableName)
        {
            return NormalizeTableName(tableName) switch
            {
                DirectorateTable => _dbContext.Directorates.AsNoTracking().Select(d => d.DirectorateName),
                DiseaseTable => _dbContext.Diseases.AsNoTracking().Select(d => d.DiseaseName),
                CustomerTable => _dbContext.Customers.AsNoTracking().Select(c => c.CustomerName),
                _ => throw UnknownTable(tableName),
            };
        }

        private static IQueryable<string> ApplyLookupFilter(IQueryable<string> query, string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter) || filter.Trim() == "{}")
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("Value", out var value) && value != null)
                query = query.Where(v => EF.Functions.ILike(v, $"%{value}%"));

            return query;
        }

        private static IQueryable<string> ApplyLookupSorting(IQueryable<string> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                return query.OrderBy(v => v);

            return descending ? query.OrderByDescending(v => v) : query.OrderBy(v => v);
        }

        private static string NormalizeTableName(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentException("Table name is required.", nameof(tableName));
            }

            return tableName.Trim().ToLowerInvariant();
        }

        private static ArgumentException UnknownTable(string tableName) =>
            new($"'{tableName}' is not a supported master lookup table.", nameof(tableName));
    }
}