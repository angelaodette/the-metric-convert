namespace TheMetricConvert.Api;

/// <summary>
/// Request to register a new user.
/// </summary>
public class RegisterRequest
{
    /// <summary>
    /// User's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's password (plaintext, will be hashed on server).
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Optional display name.
    /// </summary>
    public string? DisplayName { get; set; }
}

/// <summary>
/// Request to log in.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// User's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's password (plaintext).
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Optional device hint for tracking sessions.
    /// </summary>
    public string? DeviceHint { get; set; }
}

/// <summary>
/// Request to refresh JWT access token.
/// </summary>
public class RefreshTokenRequest
{
    /// <summary>
    /// The refresh token received during login.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>
/// Response after successful authentication.
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// JWT access token (short-lived).
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Refresh token (long-lived) for obtaining new access tokens.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// User data.
    /// </summary>
    public UserDto User { get; set; } = new();

    /// <summary>
    /// Access token expiry in seconds.
    /// </summary>
    public int ExpiresIn { get; set; }
}

/// <summary>
/// User data transfer object.
/// </summary>
public class UserDto
{
    /// <summary>
    /// User ID (UUID).
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User's email.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's display name.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// User's avatar URL.
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Whether email is verified.
    /// </summary>
    public bool EmailVerified { get; set; }

    /// <summary>
    /// User's roles.
    /// </summary>
    public List<string> Roles { get; set; } = new();
}

/// <summary>
/// Error response.
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Optional error code.
    /// </summary>
    public string? Code { get; set; }
}
