using FCG.Users.Domain.Interfaces;                       
using FCG.Users.Domain.Services;                         
using FCG.Users.Domain.ValueObjects;                     

namespace FCG.Users.Application.UseCases.Users.UpdateUser;

public sealed record UpdateUserRequest(Guid UserId, string? Name, string? NewPassword);
public sealed record UpdateUserResponse(Guid Id, string Name, string Email, string Profile);


public sealed class UpdateUserHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IUserValidationService _userValidator;
    private readonly IPasswordHasher _hasher;


    public UpdateUserHandler(IUserRepository userRepository, IUserValidationService userValidator, IPasswordHasher hasher)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _userValidator = userValidator ?? throw new ArgumentNullException(nameof(userValidator));
        _hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
    }

    public async Task<UpdateUserResponse> Handle(UpdateUserRequest request, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, ct) ?? throw new KeyNotFoundException("User not found");

        _userValidator.ValidateUpdate(request.Name, request.NewPassword);

        var nameToApply = request.Name ?? user.Name;

        // Se veio nova senha atualiza com a nova senha
        if (!string.IsNullOrWhiteSpace(request.NewPassword))
        {
            var plainVo = _userValidator.ValidatePassword(request.NewPassword);
            var hashed = _hasher.Hash(plainVo.Value);
            var passwordVo = Password.Create(hashed);

            user.Update(nameToApply, passwordVo);
        } 
        // Se não veio nova senha, mas o nome é diferente, atualiza só o nome
        else if (!string.Equals(nameToApply, user.Name, StringComparison.Ordinal))
        {
            user.Update(nameToApply, user.Password);
        }

        await _userRepository.UpdateAsync(user, ct);

        return new UpdateUserResponse(
            user.Id,
            user.Name,
            user.Email.Value,
            user.Profile.Value
        );
    }
}
