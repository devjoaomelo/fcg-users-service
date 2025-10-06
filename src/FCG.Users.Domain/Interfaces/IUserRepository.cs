using FCG.Users.Domain.Entities;
using FCG.Users.Domain.ValueObjects;

namespace FCG.Users.Domain.Interfaces;

public interface IUserRepository
{
    // Queries
    Task<User?> GetByEmailAsync(Email email, CancellationToken ct = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(Email email, CancellationToken ct = default);
    Task<int> CountAsync(CancellationToken ct = default);

    // Commands
    Task AddAsync(User user, CancellationToken ct = default);
    Task UpdateAsync(User user, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
