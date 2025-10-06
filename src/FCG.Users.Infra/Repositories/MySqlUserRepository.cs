using FCG.Users.Domain.Entities;
using FCG.Users.Domain.Interfaces;
using FCG.Users.Domain.ValueObjects;
using FCG.Users.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace FCG.Users.Infra.Repositories;

public sealed class MySqlUserRepository : IUserRepository
{
    private readonly UsersDbContext _db;
    public MySqlUserRepository(UsersDbContext db) => _db = db;

    // 👇 compare VO == VO (EF traduz via ValueComparer)
    public Task<User?> GetByEmailAsync(Email email, CancellationToken ct = default)
        => _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email, ct);

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default)
        => await _db.Users.AsNoTracking().OrderBy(u => u.Name).ToListAsync(ct);

    // 👇 nada de .Value aqui
    public Task<bool> ExistsByEmailAsync(Email email, CancellationToken ct = default)
        => _db.Users.AnyAsync(u => u.Email == email, ct);

    public Task<int> CountAsync(CancellationToken ct = default)
        => _db.Users.CountAsync(ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        await _db.Users.AddAsync(user, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        _db.Users.Update(user);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.Users.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return;
        _db.Users.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }
}
