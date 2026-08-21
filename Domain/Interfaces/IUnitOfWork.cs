using Domain.Entities;

namespace Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
      IBaseRepository<User> Users { get; }
      IUserRepository UserRepository { get; }
      Task<int> Complete(CancellationToken cancellationToken);
}
