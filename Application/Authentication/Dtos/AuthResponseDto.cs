using Application.Users.Dtos;

namespace Application.Authentication.Dtos;

public record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    UserDto User
);