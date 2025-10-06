using FCG.Users.Domain.Interfaces;

namespace FCG.Users.Application.UseCases.Users.GetUserById;

public sealed record GetUserByIdRequest(Guid UserId);
public sealed record GetUserByIdResponse(Guid Id, string Name, string Email, string Profile);

public sealed class GetUserByIdHandler
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task<GetUserByIdResponse> Handle(GetUserByIdRequest request, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, ct) ?? throw new KeyNotFoundException("User not found");

        return new GetUserByIdResponse(user.Id, user.Name, user.Email.Value, user.Profile.Value);
    }
}