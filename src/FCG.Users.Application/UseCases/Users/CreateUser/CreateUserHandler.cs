using FCG.Users.Domain.Interfaces;

namespace FCG.Users.Application.UseCases.Users.CreateUser;

public sealed record CreateUserRequest(string Name, string Email, string Password);
public sealed record CreateUserResponse(Guid Id, string Name, string Email, string Profile);

public sealed class CreateUserHandler
{
    private readonly IUserCreationService _userCreationService;
    private readonly IUserRepository _userRepository;              

    public CreateUserHandler(IUserCreationService userCreationService, IUserRepository userRepository)
    {
        _userCreationService = userCreationService ?? throw new ArgumentNullException(nameof(userCreationService));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task<CreateUserResponse> Handle(CreateUserRequest request, CancellationToken ct = default)
    {
        var user = await _userCreationService.CreateUserAsync(request.Name, request.Email, request.Password, ct);

        await _userRepository.AddAsync(user, ct);

        return new CreateUserResponse(
            user.Id,
            user.Name,
            user.Email.Value,
            user.Profile.Value
        );
    }
}