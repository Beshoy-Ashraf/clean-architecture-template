using Domain.Entities;

namespace Domain.Interfaces;

public interface IUserRepository : IBaseRepository<User>
{
      Task<User?> GetUserByEmail(string email, CancellationToken cancellationToken);
      Task<User?> GetUserByUsername(string username, CancellationToken cancellationToken);
      Task<User?> GetUserByUserId(Guid id, CancellationToken cancellationToken);
}