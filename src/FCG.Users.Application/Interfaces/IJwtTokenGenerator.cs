namespace FCG.Users.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string Generate(
        Guid userId,
        string name,
        string email,
        string role,
        string issuer,
        string audience,
        string signingKey,
        out DateTime expiresAtUtc,
        TimeSpan? ttl = null);
}