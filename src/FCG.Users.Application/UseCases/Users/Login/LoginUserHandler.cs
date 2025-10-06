using FCG.Users.Application.Interfaces;

namespace FCG.Users.Application.UseCases.Users.Login;

public sealed record LoginUserRequest(string Email, string Password);
public sealed record LoginUserResponse(string AccessToken, DateTime ExpiresAtUtc);

public sealed class LoginUserHandler
{
    private readonly IUserAuthenticationService _authService;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public LoginUserHandler(IUserAuthenticationService authService, IJwtTokenGenerator tokenGenerator)
    {
        _authService = authService;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<LoginUserResponse> Handle(LoginUserRequest request, string issuer, string audience, string signingKey, TimeSpan? ttl = null, CancellationToken ct = default)
    {
        var user = await _authService.AuthenticateAsync(request.Email, request.Password, ct);

        var token = _tokenGenerator.Generate(
            user.Id,
            user.Name,
            user.Email.Value,
            user.Profile.Value,
            issuer,
            audience,
            signingKey,
            out var expiresAtUtc,
            ttl
        );

        return new LoginUserResponse(token, expiresAtUtc);
    }
}
