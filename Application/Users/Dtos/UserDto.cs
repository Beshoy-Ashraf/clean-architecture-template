namespace Application.Users.Dtos;

public record UserDto(Guid Id, string Username, string Email, string DisplayName, string Role);