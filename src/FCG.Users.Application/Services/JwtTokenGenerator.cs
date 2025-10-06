using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using FCG.Users.Application.Interfaces;

namespace FCG.Users.Application.Services;

public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    public string Generate(
        Guid userId,
        string name,
        string email,
        string role,
        string issuer,
        string audience,
        string signingKey,
        out DateTime expiresAtUtc,
        TimeSpan? ttl = null)
    {
        var now = DateTime.UtcNow;
        var lifetime = ttl ?? TimeSpan.FromHours(2);
        expiresAtUtc = now.Add(lifetime);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, name),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: now,
            expires: expiresAtUtc,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
