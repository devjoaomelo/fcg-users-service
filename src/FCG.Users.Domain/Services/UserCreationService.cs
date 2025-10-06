using FCG.Users.Domain.Entities;
using FCG.Users.Domain.Interfaces;
using FCG.Users.Domain.ValueObjects;

namespace FCG.Users.Domain.Services;

public sealed class UserCreationService : IUserCreationService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _hasher;

    public UserCreationService(IUserRepository userRepository, IPasswordHasher hasher) 
    { 
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
    }

    public async Task<User> CreateUserAsync(string name, string email, string password, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        // ValueObjects
        var emailVo = Email.Create(email);
        var plainPasswordVo = Password.Create(password);

        // Gera o HASH da senha
        var hashed = _hasher.Hash(plainPasswordVo.Value);

        // passwordVo agora é o HASH
        var passwordVo = Password.Create(hashed);

        if (await _userRepository.ExistsByEmailAsync(emailVo, ct))
            throw new InvalidOperationException($"User with email '{emailVo.Value}' already exists");

        var isFirstUser = (await _userRepository.CountAsync(ct)) == 0;

        var user = new User(name, emailVo, passwordVo);
        if (isFirstUser)
            user.PromoteToAdmin();

        // Não persiste, apenas retorna a entidade pronta
        return user;
    }
}
