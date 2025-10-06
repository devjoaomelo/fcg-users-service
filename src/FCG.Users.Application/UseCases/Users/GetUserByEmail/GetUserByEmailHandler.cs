using FCG.Users.Domain.Interfaces;
using FCG.Users.Domain.ValueObjects;

namespace FCG.Users.Application.UseCases.Users.GetUserByEmail;

public sealed record GetUserByEmailRequest(string Email);
public sealed record GetUserByEmailResponse(Guid Id, string Name, string Email, string Profile);

public sealed class GetUserByEmailHandler
{
    private readonly IUserRepository _userRepository;

    public GetUserByEmailHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task<GetUserByEmailResponse> Handle(GetUserByEmailRequest request, CancellationToken ct = default)
    {
        var emailVo = Email.Create(request.Email);
        var user = await _userRepository.GetByEmailAsync(emailVo, ct) ?? throw new KeyNotFoundException("User not found");

        return new GetUserByEmailResponse(user.Id, user.Name, user.Email.Value, user.Profile.Value);
    }
}