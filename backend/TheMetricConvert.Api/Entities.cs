#pragma warning disable CS1591 // Entity properties are documented via Column attributes and class summaries
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheMetricConvert.Api;

/// <summary>
/// Core user entity.
/// </summary>
[Table("users")]
public class User
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("email")]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Column("display_name")]
    [MaxLength(255)]
    public string? DisplayName { get; set; }

    [Column("avatar_url")]
    public string? AvatarUrl { get; set; }

    [Column("email_verified")]
    public bool EmailVerified { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<UserCredential> Credentials { get; set; } = new List<UserCredential>();

    public ICollection<UserRole> Roles { get; set; } = new List<UserRole>();

    public ICollection<SocialAuth> SocialAuths { get; set; } = new List<SocialAuth>();

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public ICollection<AuthSession> Sessions { get; set; } = new List<AuthSession>();

    public ICollection<DeviceToken> DeviceTokens { get; set; } = new List<DeviceToken>();
}

/// <summary>
/// User password credentials.
/// </summary>
[Table("user_credentials")]
public class UserCredential
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("password_hash")]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    [Column("last_changed_at")]
    public DateTime LastChangedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
}

/// <summary>
/// User role assignment.
/// </summary>
[Table("user_roles")]
public class UserRole
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("role")]
    [MaxLength(100)]
    public string Role { get; set; } = "user"; // e.g., "user", "admin", "moderator"

    [Column("assigned_at")]
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
}

/// <summary>
/// OAuth/Social login credentials.
/// </summary>
[Table("social_auths")]
public class SocialAuth
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("provider")]
    [MaxLength(50)]
    public string Provider { get; set; } = string.Empty; // e.g., "google", "github"

    [Column("provider_user_id")]
    [MaxLength(255)]
    public string ProviderUserId { get; set; } = string.Empty;

    [Column("email")]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Column("linked_at")]
    public DateTime LinkedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
}

/// <summary>
/// JWT refresh tokens.
/// </summary>
[Table("refresh_tokens")]
public class RefreshToken
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("token")]
    [MaxLength(500)]
    public string Token { get; set; } = string.Empty;

    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }

    [Column("verified_at")]
    public DateTime? VerifiedAt { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
}

/// <summary>
/// Active user sessions (JWT tokens).
/// </summary>
[Table("auth_sessions")]
public class AuthSession
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("token")]
    [MaxLength(2000)]
    public string Token { get; set; } = string.Empty;

    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }

    [Column("verified_at")]
    public DateTime? VerifiedAt { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
}

/// <summary>
/// Device tokens for push notifications or device tracking.
/// </summary>
[Table("device_tokens")]
public class DeviceToken
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("token_hash")]
    [MaxLength(255)]
    public string TokenHash { get; set; } = string.Empty;

    [Column("device_hint")]
    [MaxLength(255)]
    public string? DeviceHint { get; set; }

    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("revoked_at")]
    public DateTime? RevokedAt { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
}
