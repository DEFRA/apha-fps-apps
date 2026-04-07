using Apha.Costbook.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apha.Costbook.DataAccess.Repositories
{
    public class RepositoryBase<TEntity> where TEntity : class
    {
        protected readonly CostbookDbContext _context;

        public RepositoryBase(CostbookDbContext context)
        {
            _context = context;
        }

        protected virtual IQueryable<TEntity> GetQueryableResult(string sql, params object[] parameters)
        {
            return _context.Set<TEntity>().FromSqlRaw(sql, parameters);
        }

        // allows querying for any arbitrary type (like Project, Staff, etc.)
        protected virtual IQueryable<T> GetQueryableResultFor<T>(string sql, params object[] parameters) where T : class
        {
            return _context.Set<T>().FromSqlRaw(sql, parameters);
        }

        protected virtual IQueryable<T> GetDbSetFor<T>() where T : class
        {
            return _context.Set<T>();
        }
        protected virtual DbSet<T> GetDbSet<T>() where T : class
        {
            return _context.Set<T>();
        }

        protected virtual Task<int> ExecuteSqlAsync(string sql, params object[] parameters)
        {
            return _context.Database.ExecuteSqlRawAsync(sql, parameters);
        }
    }
}
