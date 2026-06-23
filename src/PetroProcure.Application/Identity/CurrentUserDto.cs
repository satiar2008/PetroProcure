namespace PetroProcure.Application.Identity;

public sealed record CurrentUserDto(Guid Id, string UserName, string? Email);
