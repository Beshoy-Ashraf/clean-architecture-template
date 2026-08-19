using Domain.Entities;

namespace Domain.Interfaces;

public interface IUserRepository : IBaseRepository<User>
{
      Task<User> GetUserByEmail(string email, CancellationToken cancellationToken);

}
