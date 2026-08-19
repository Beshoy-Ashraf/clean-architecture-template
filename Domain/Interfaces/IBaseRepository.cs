using System.Linq.Expressions;

namespace Domain.Interfaces;

public interface IBaseRepository<T> where T : class
{

      Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> criteria, CancellationToken cancellationToken);
      Task<T> GetByIdAsync(Guid id, CancellationToken cancellationToken);
      Task<T> AddAsync(T entity, CancellationToken cancellationToken);
      Task<T> UpdateAsync(T entity);
      void DeleteAsync(T entity, CancellationToken cancellationToken);
      Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> criteria, CancellationToken cancellationToken, string[]? includes = null);
      Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> criteria, int skip, int take, CancellationToken cancellationToken, string[]? includes = null);

      Task<T> FindAsync(Expression<Func<T, bool>> criteria, CancellationToken cancellationToken, string[]? includes = null);

}
