using FCG.Users.Domain.Interfaces;

namespace FCG.Users.Application.UseCases.Users.DeleteUser;

public sealed record DeleteUserRequest(Guid UserId);
public sealed record DeleteUserResponse(bool Deleted);

public sealed class DeleteUserHandler
{
    private readonly IUserRepository _userRepository;

    public DeleteUserHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task<DeleteUserResponse> Handle(DeleteUserRequest request, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, ct)
            ?? throw new KeyNotFoundException("User not found");

        await _userRepository.DeleteAsync(user.Id, ct);

        return new DeleteUserResponse(true);
    }
}
