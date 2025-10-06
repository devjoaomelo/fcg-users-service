using FCG.Users.Domain.Entities;

namespace FCG.Users.Domain.Interfaces
{
    public interface IUserCreationService
    {
        Task<User> CreateUserAsync(string name, string email, string password, CancellationToken ct = default);
    }
}
