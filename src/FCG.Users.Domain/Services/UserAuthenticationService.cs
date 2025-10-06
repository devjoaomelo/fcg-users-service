using FCG.Users.Domain.Entities;
using FCG.Users.Domain.Interfaces;
using FCG.Users.Domain.ValueObjects;

namespace FCG.Users.Domain.Services;

public sealed class UserAuthenticationService : IUserAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _hasher;

    public UserAuthenticationService(IUserRepository userRepository, IPasswordHasher hasher)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
    }

    public async Task<User> AuthenticateAsync(string email, string password, CancellationToken ct = default)
    {
        // ValueObjects
        var emailVo = Email.Create(email);
        var passwordVo = Password.Create(password);

        var user = await _userRepository.GetByEmailAsync(emailVo, ct);
        if (user is null)
            throw new InvalidOperationException("Invalid credentials");

        // IMPORTANTE: Password armazenado em user deve ser HASH
        var isValid = _hasher.Verify(passwordVo.Value, user.Password.Value);
        if (!isValid)
            throw new InvalidOperationException("Invalid credentials");

        return user;
    }
}
