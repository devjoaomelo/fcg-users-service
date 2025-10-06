using FCG.Users.Domain.ValueObjects;

namespace FCG.Users.Domain.Interfaces;

public interface IUserValidationService
{
    void ValidateName(string name);
    Email ValidateEmail(string email);
    Password ValidatePassword(string password);
    Task ValidateEmailNotRegisteredAsync(string email, CancellationToken ct = default);
    void ValidateUpdate(string? name = null, string? password = null);
}

