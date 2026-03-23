using Microsoft.EntityFrameworkCore;

namespace TheMetricConvert.Api;

/// <summary>
/// EF Core database context for the Metric Convert app.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<UserCredential> UserCredentials { get; set; } = null!;
    public DbSet<UserRole> UserRoles { get; set; } = null!;
    public DbSet<SocialAuth> SocialAuths { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<AuthSession> AuthSessions { get; set; } = null!;
    public DbSet<DeviceToken> DeviceTokens { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Users: email should be unique
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique()
            .HasName("idx_users_email_unique")
            .HasFilter("deleted_at IS NULL");

        // UserCredentials: one-to-one with User
        modelBuilder.Entity<UserCredential>()
            .HasOne(uc => uc.User)
            .WithMany(u => u.Credentials)
            .HasForeignKey(uc => uc.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // UserRoles: one-to-many with User
        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.Roles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // SocialAuths: one-to-many with User
        modelBuilder.Entity<SocialAuth>()
            .HasOne(sa => sa.User)
            .WithMany(u => u.SocialAuths)
            .HasForeignKey(sa => sa.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SocialAuth>()
            .HasIndex(sa => new { sa.Provider, sa.ProviderUserId })
            .IsUnique()
            .HasName("idx_social_auths_provider_user_unique");

        // RefreshTokens: one-to-many with User
        modelBuilder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RefreshToken>()
            .HasIndex(rt => rt.Token)
            .IsUnique()
            .HasName("idx_refresh_tokens_token_unique");

        // AuthSessions: one-to-many with User
        modelBuilder.Entity<AuthSession>()
            .HasOne(@as => @as.User)
            .WithMany(u => u.Sessions)
            .HasForeignKey(@as => @as.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AuthSession>()
            .HasIndex(@as => @as.Token)
            .IsUnique()
            .HasName("idx_auth_sessions_token_unique");

        // DeviceTokens: one-to-many with User
        modelBuilder.Entity<DeviceToken>()
            .HasOne(dt => dt.User)
            .WithMany(u => u.DeviceTokens)
            .HasForeignKey(dt => dt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DeviceToken>()
            .HasIndex(dt => dt.TokenHash)
            .IsUnique()
            .HasName("idx_device_tokens_hash_unique");
    }
}
