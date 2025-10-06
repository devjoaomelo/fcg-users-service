using FCG.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using FCG.Users.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FCG.Users.Infra.Data;

public sealed class UsersDbContext : DbContext
{
    public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var emailConv = new ValueConverter<Email, string>(v => v.Value, v => Email.Create(v));
        var passwordConv = new ValueConverter<Password, string>(v => v.Value, v => Password.Create(v));
        var profileConv = new ValueConverter<Profile, string>(v => v.Value, v => Profile.Parse(v));

        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasKey(x => x.Id);

            e.Property(x => x.Id)
             .HasColumnName("id")
             .ValueGeneratedNever();

            e.Property(x => x.Name)
             .HasColumnName("name")
             .IsRequired()
             .HasMaxLength(150);

            e.Property(x => x.Email)
             .HasColumnName("email")
             .HasConversion(emailConv)
             .IsRequired()
             .HasMaxLength(320);

            e.Property(x => x.Password)
             .HasColumnName("password_hash")
             .HasConversion(passwordConv)
             .IsRequired()
             .HasMaxLength(255);

            e.Property(x => x.Profile)
             .HasColumnName("profile")
             .HasConversion(profileConv)
             .IsRequired()
             .HasMaxLength(20);

            e.HasIndex(x => x.Email).IsUnique();
        });
    }
}
