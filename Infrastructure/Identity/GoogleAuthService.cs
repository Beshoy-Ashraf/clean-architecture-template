using Application.Common.Interfaces;
using Domain.Exceptions;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;

namespace Infrastructure.Identity;

public class GoogleAuthService(IOptions<GoogleSettings> settings) : IGoogleAuthService
{
      private readonly GoogleSettings _settings = settings.Value;

      public async Task<GoogleUserPayload> ValidateAsync(string idToken, CancellationToken cancellationToken)
      {
            GoogleJsonWebSignature.Payload payload;

            try
            {
                  var validationSettings = new GoogleJsonWebSignature.ValidationSettings
                  {
                        Audience = [_settings.ClientId]
                  };

                  payload = await GoogleJsonWebSignature.ValidateAsync(idToken, validationSettings);
            }
            catch (InvalidJwtException)
            {
                  throw new UnauthorizedException("Invalid Google token");
            }

            return new GoogleUserPayload(
                payload.Subject,
                payload.Email,
                payload.EmailVerified,
                payload.Name,
                payload.Picture);
      }
}
