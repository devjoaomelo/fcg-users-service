using FCG.Users.Application.Interfaces;
using FCG.Users.Domain.Interfaces;

namespace FCG.Users.Application.UseCases.Users.CreateUser;

public sealed record CreateUserRequest(string Name, string Email, string Password);
public sealed record CreateUserResponse(Guid Id, string Name, string Email, string Profile);

public sealed class CreateUserHandler
{
    private readonly IUserCreationService _userCreationService;
    private readonly IUserRepository _userRepository;
    private readonly IEventStore _eventStore;

    public CreateUserHandler(IUserCreationService userCreationService, IUserRepository userRepository, IEventStore eventStore)
    {
        _userCreationService = userCreationService ?? throw new ArgumentNullException(nameof(userCreationService));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
    }

    public async Task<CreateUserResponse> Handle(CreateUserRequest request, CancellationToken ct = default)
    {
        var user = await _userCreationService.CreateUserAsync(request.Name, request.Email, request.Password, ct);

        await _userRepository.AddAsync(user, ct);

        await _eventStore.AppendAsync(
            user.Id,
            "UserCreated",
            new
            {
                user.Id,
                user.Name,
                user.Email,
                user.Profile
            },
            ct
        );

        return new CreateUserResponse(
            user.Id,
            user.Name,
            user.Email.Value,
            user.Profile.Value
        );
    }
}