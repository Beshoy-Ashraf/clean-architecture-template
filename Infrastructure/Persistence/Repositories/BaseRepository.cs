using System.Linq.Expressions;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;


public class BaseRepository<T>(AppDbContext appDbContext) : IBaseRepository<T> where T : class
{
      protected AppDbContext _dbContext = appDbContext;
      public async Task<T> AddAsync(T entity, CancellationToken cancellationToken)
      {
            var result = await _dbContext.Set<T>().AddAsync(entity, cancellationToken);
            return result.Entity;
      }

      public void DeleteAsync(T entity, CancellationToken cancellationToken)
      {
            _dbContext.Set<T>().Remove(entity);
      }

      public async Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> criteria, CancellationToken cancellationToken, string[]? includes = null)
      {
            IQueryable<T> query = _dbContext.Set<T>();
            if (includes != null)
                  foreach (var include in includes)
                        query = query.Include(include);

            return await query.Where(criteria).ToListAsync(cancellationToken);
      }
      public async Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> criteria, int skip, int take, CancellationToken cancellationToken, string[]? includes = null)
      {
            IQueryable<T> query = _dbContext.Set<T>();
            if (includes != null)
                  foreach (var include in includes)
                        query = query.Include(include);

            return await query.Where(criteria).Skip(skip).Take(take).ToListAsync(cancellationToken);
      }

      public async Task<T> FindAsync(Expression<Func<T, bool>> criteria, CancellationToken cancellationToken, string[]? includes = null)
      {
            IQueryable<T> query = _dbContext.Set<T>() ?? throw new KeyNotFoundException("No data found in the database");

            if (includes != null)
                  foreach (var include in includes)
                        query = query.Include(include);
            var result = await query.FirstOrDefaultAsync(criteria, cancellationToken) ?? throw new KeyNotFoundException("No matching entity found in the database");
            return result;

      }

      public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> criteria, CancellationToken cancellationToken)
      {
            return await _dbContext.Set<T>().Where(criteria).ToListAsync(cancellationToken);
      }

      public async Task<T> GetByIdAsync(Guid id, CancellationToken cancellationToken)
      {
            return await _dbContext.Set<T>().FindAsync(id, cancellationToken) ?? throw new KeyNotFoundException("No matching entity found in the database");
      }

      public Task<T> UpdateAsync(T entity)
      {
            var entry = _dbContext.Set<T>().Update(entity);
            return Task.FromResult(entry.Entity);
      }
}
