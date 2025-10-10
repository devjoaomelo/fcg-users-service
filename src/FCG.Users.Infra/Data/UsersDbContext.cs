using FCG.Users.Domain.Entities;
using FCG.Users.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FCG.Users.Infra.Data;

public sealed class UsersDbContext : DbContext
{
    public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<StoredEvent> EventStore => Set<StoredEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // --------------------------------------------
        // Converters (VO <-> string)
        // --------------------------------------------
        var emailConv = new ValueConverter<Email, string>(v => v.Value, v => Email.Create(v));
        var passwordConv = new ValueConverter<Password, string>(v => v.Value, v => Password.Create(v));
        var profileConv = new ValueConverter<Profile, string>(v => v.Value, v => Profile.Parse(v)); // ajuste se o método tiver outro nome

        // --------------------------------------------
        // Comparers (como o EF compara/rastreia VO)
        // --------------------------------------------
        var emailComp = new ValueComparer<Email>(
            (a, b) => a.Value == b.Value,
            v => v.Value.GetHashCode(),
            v => Email.Create(v.Value));

        var passwordComp = new ValueComparer<Password>(
            (a, b) => a.Value == b.Value,
            v => v.Value.GetHashCode(),
            v => Password.Create(v.Value));

        var profileComp = new ValueComparer<Profile>(
            (a, b) => a.Value == b.Value,
            v => v.Value.GetHashCode(),
            v => Profile.Parse(v.Value)); // ou Profile.From(...)

        // --------------------------------------------
        // Mapeamento da entidade
        // --------------------------------------------
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

            // EMAIL
            var emailProp = e.Property(x => x.Email)
                .HasColumnName("email")
                .HasConversion(emailConv)
                .IsRequired()
                .HasMaxLength(320);
            emailProp.Metadata.SetValueComparer(emailComp);

            // PASSWORD (hash)
            var passProp = e.Property(x => x.Password)
                .HasColumnName("password_hash")
                .HasConversion(passwordConv)
                .IsRequired()
                .HasMaxLength(255);
            passProp.Metadata.SetValueComparer(passwordComp);

            // PROFILE
            var profileProp = e.Property(x => x.Profile)
                .HasColumnName("profile")
                .HasConversion(profileConv)
                .IsRequired()
                .HasMaxLength(20);
            profileProp.Metadata.SetValueComparer(profileComp);

            e.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<StoredEvent>(e =>
        {
            e.ToTable("event_store");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.AggregateId).HasColumnName("aggregate_id");
            e.Property(x => x.Type).HasColumnName("type").HasMaxLength(200);
            e.Property(x => x.Data).HasColumnName("data");
            e.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
            e.HasIndex(x => x.AggregateId);
            e.HasIndex(x => x.CreatedAtUtc);
        });

        
    }
}
