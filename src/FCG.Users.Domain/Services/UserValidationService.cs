using FCG.Users.Domain.Interfaces;
using FCG.Users.Domain.ValueObjects;

namespace FCG.Users.Domain.Services;

public sealed class UserValidationService : IUserValidationService
{
    private readonly IUserRepository _userRepository;

    public UserValidationService(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));
    }

    public Email ValidateEmail(string email)
    {
        return Email.Create(email);
    }

    public Password ValidatePassword(string password)
    {
        return Password.Create(password);
    }

    public async Task ValidateEmailNotRegisteredAsync(string email, CancellationToken ct = default)
    {
        var emailVo = Email.Create(email);
        var exists = await _userRepository.ExistsByEmailAsync(emailVo, ct);
        if (exists)
            throw new InvalidOperationException($"User with email '{emailVo.Value}' already exists");
    }

    public void ValidateUpdate(string? name = null, string? password = null)
    {
        if (!string.IsNullOrWhiteSpace(name))
            ValidateName(name);

        if (!string.IsNullOrWhiteSpace(password))
            ValidatePassword(password);
    }
}
