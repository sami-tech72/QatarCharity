using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Persistence;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var user = modelBuilder.Entity<UserAccount>();

        user.ToTable("Users");
        user.HasKey(x => x.Id);

        user.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        user.Property(x => x.Username)
            .IsRequired()
            .HasMaxLength(128);

        user.Property(x => x.Password)
            .IsRequired()
            .HasMaxLength(256);

        user.Property(x => x.DisplayName)
            .IsRequired()
            .HasMaxLength(256);

        user.Property(x => x.Role)
            .IsRequired()
            .HasMaxLength(64);

        user.HasIndex(x => x.Username)
            .IsUnique();
    }
}
