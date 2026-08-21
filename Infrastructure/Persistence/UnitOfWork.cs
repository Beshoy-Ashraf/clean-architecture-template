using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence.Repositories;

namespace Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
      private readonly AppDbContext _dbContext;

      public IBaseRepository<User> Users { get; private set; } = null!;
      public IUserRepository UserRepository { get; private set; }

      public UnitOfWork(AppDbContext dBContext)
      {
            _dbContext = dBContext;
            Users = new BaseRepository<User>(_dbContext);
            UserRepository = new UserRepository(_dbContext);

      }
      public async Task<int> Complete(CancellationToken cancellationToken)
      {
            return await _dbContext.SaveChangesAsync(cancellationToken);
      }

      public void Dispose()
      {
            _dbContext.Dispose();
      }
}
