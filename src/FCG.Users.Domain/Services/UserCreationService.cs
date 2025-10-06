using FCG.Users.Domain.Entities;
using FCG.Users.Domain.Interfaces;
using FCG.Users.Domain.ValueObjects;

namespace FCG.Users.Domain.Services;

public sealed class UserCreationService : IUserCreationService
{
    private readonly IUserRepository _userRepository;

    public UserCreationService(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task<User> CreateUserAsync(string name, string email, string password, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        // ValueObjects
        var emailVo = Email.Create(email);
        var passwordVo = Password.Create(password);

        if (await _userRepository.ExistsByEmailAsync(emailVo, ct))
            throw new InvalidOperationException($"User with email '{emailVo.Value}' already exists");

        var isFirstUser = (await _userRepository.CountAsync(ct)) == 0;

        var user = new User(name, emailVo, passwordVo);
        if (isFirstUser)
            user.PromoteToAdmin();

        // Não persiste, apenas cria a entidade
        return user;
    }
}
