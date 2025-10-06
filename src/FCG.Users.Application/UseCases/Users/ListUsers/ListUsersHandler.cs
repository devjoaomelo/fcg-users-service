using FCG.Users.Domain.Interfaces;

namespace FCG.Users.Application.UseCases.Users.ListUsers;

public sealed record ListUsersRequest();
public sealed record ListUsersItem(Guid Id, string Name, string Email, string Profile);
public sealed record ListUsersResponse(IReadOnlyList<ListUsersItem> Users);

public sealed class ListUsersHandler
{
    private readonly IUserRepository _userRepository;

    public ListUsersHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task<ListUsersResponse> Handle(ListUsersRequest request, CancellationToken ct = default)
    {
        var users = await _userRepository.GetAllAsync(ct);
        var items = users
            .Select(u => new ListUsersItem(u.Id, u.Name, u.Email.Value, u.Profile.Value))
            .ToList()
            .AsReadOnly();

        return new ListUsersResponse(items);
    }
}
