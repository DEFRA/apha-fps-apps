using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Enums;
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
            switch (ParseTableName(tableName))
            {
                case MasterLookupTable.Directorate:
                    return await _dbContext.Directorates
                        .AsNoTracking()
                        .OrderBy(d => d.DirectorateName)
                        .Select(d => d.DirectorateName)
                    .ToListAsync();

                case MasterLookupTable.Disease:
                    return await _dbContext.Diseases
                        .AsNoTracking()
                        .OrderBy(d => d.DiseaseName)
                        .Select(d => d.DiseaseName)
                        .ToListAsync();

                case MasterLookupTable.Customer:
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
            switch (ParseTableName(tableName))
            {
                case MasterLookupTable.Directorate:
                    _dbContext.Directorates.Add(new Directorate { DirectorateName = value });
                    break;
                case MasterLookupTable.Disease:
                    _dbContext.Diseases.Add(new Disease { DiseaseName = value });
                    break;
                case MasterLookupTable.Customer:
                    _dbContext.Customers.Add(new Customer { CustomerName = value });
                    break;
                default:
                    throw UnknownTable(tableName);
            }

            return await _dbContext.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateLookupItemAsync(string tableName, string originalValue, string newValue)
        {
            var table = ParseTableName(tableName);
            int affectedRows;

            switch (table)
            {
                case MasterLookupTable.Directorate:
                    affectedRows = await _dbContext.Directorates
                        .Where(d => d.DirectorateName == originalValue)
                        .ExecuteUpdateAsync(s => s.SetProperty(d => d.DirectorateName, newValue));
                    break;

                case MasterLookupTable.Disease:
                    affectedRows = await _dbContext.Diseases
                        .Where(d => d.DiseaseName == originalValue)
                        .ExecuteUpdateAsync(s => s.SetProperty(d => d.DiseaseName, newValue));
                    break;

                case MasterLookupTable.Customer:
                    affectedRows = await _dbContext.Customers
                        .Where(c => c.CustomerName == originalValue)
                        .ExecuteUpdateAsync(s => s.SetProperty(c => c.CustomerName, newValue));
                    break;

                default:
                    throw UnknownTable(tableName);
            }

            return affectedRows > 0;
        }

        public async Task<bool> DeleteLookupItemAsync(string tableName, string value)
        {
            var table = ParseTableName(tableName);
            int affectedRows;

            switch (table)
            {
                case MasterLookupTable.Directorate:
                    affectedRows = await _dbContext.Directorates
                        .Where(d => d.DirectorateName == value)
                        .ExecuteDeleteAsync();
                    break;

                case MasterLookupTable.Disease:
                    affectedRows = await _dbContext.Diseases
                        .Where(d => d.DiseaseName == value)
                        .ExecuteDeleteAsync();
                    break;

                case MasterLookupTable.Customer:
                    affectedRows = await _dbContext.Customers
                        .Where(c => c.CustomerName == value)
                        .ExecuteDeleteAsync();
                    break;

                default:
                    throw UnknownTable(tableName);
            }

            return affectedRows > 0;
        }

        private IQueryable<string> GetLookupQuery(string tableName)
        {
            return ParseTableName(tableName) switch
            {
                MasterLookupTable.Directorate => _dbContext.Directorates.AsNoTracking().Select(d => d.DirectorateName),
                MasterLookupTable.Disease => _dbContext.Diseases.AsNoTracking().Select(d => d.DiseaseName),
                MasterLookupTable.Customer => _dbContext.Customers.AsNoTracking().Select(c => c.CustomerName),
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

        private static MasterLookupTable ParseTableName(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentException("Table name is required.", nameof(tableName));
            }

            if (!Enum.TryParse<MasterLookupTable>(tableName.Trim(), ignoreCase: true, out var table)
                || !Enum.IsDefined(table))
            {
                throw UnknownTable(tableName);
            }

            return table;
        }

        private static ArgumentException UnknownTable(string tableName) =>
            new($"'{tableName}' is not a supported master lookup table.", nameof(tableName));
    }
}