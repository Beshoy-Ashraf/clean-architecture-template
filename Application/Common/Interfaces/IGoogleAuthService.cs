namespace Application.Common.Interfaces;

public record GoogleUserPayload(string Subject, string Email, bool EmailVerified, string Name, string PictureUrl);

public interface IGoogleAuthService
{
      Task<GoogleUserPayload> ValidateAsync(string idToken, CancellationToken cancellationToken);
}
