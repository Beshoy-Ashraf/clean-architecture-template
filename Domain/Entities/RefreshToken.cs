namespace Domain.Entities;

public class RefreshToken
{
      public Guid Id { get; private set; }
      public string? Token { get; set; }
      public DateTime? RefreshTokenExpiryTime { get; set; }
      public Guid UserId { get; set; }
      public User User { get; set; } = null!;
}
