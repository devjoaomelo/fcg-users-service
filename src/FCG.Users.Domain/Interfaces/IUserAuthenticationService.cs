using FCG.Users.Domain.Entities;

public interface IUserAuthenticationService
{
    Task<User> AuthenticateAsync(string email, string password, CancellationToken ct = default);
}