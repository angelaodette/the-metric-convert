using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace TheMetricConvert.Api;

/// <summary>
/// Service for handling authentication logic.
/// </summary>
public interface IAuthService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshAccessTokenAsync(string refreshToken);
}

/// <summary>
/// Implementation of authentication service using bcrypt and JWT.
/// </summary>
public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthService> _logger;

    public AuthService(AppDbContext context, IConfiguration config, ILogger<AuthService> logger)
    {
        _context = context;
        _config = config;
        _logger = logger;
    }

    /// <summary>
    /// Hashes a password using bcrypt.
    /// </summary>
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    /// <summary>
    /// Verifies a password against its hash.
    /// </summary>
    public bool VerifyPassword(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Generates a JWT access token for the user.
    /// </summary>
    public string GenerateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Secret"] ?? "your-secret-key-change-me-in-production"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<System.Security.Claims.Claim>
        {
            new System.Security.Claims.Claim("sub", user.Id.ToString()),
            new System.Security.Claims.Claim("email", user.Email),
            new System.Security.Claims.Claim("email_verified", user.EmailVerified.ToString().ToLower())
        };

        // Add roles
        foreach (var role in user.Roles)
        {
            claims.Add(new System.Security.Claims.Claim("role", role.Role));
        }

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"] ?? "TheMetricConvert",
            audience: _config["Jwt:Audience"] ?? "TheMetricConvertClients",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15), // 15 minute access token
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Generates a long-lived refresh token.
    /// </summary>
    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
        }
        return Convert.ToBase64String(randomNumber);
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Email and password are required.");
        }

        // Check if user already exists
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email already exists.");
        }

        // Create new user
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            DisplayName = request.DisplayName ?? request.Email.Split('@')[0],
            IsActive = true,
            EmailVerified = false
        };

        // Hash password and create credential
        var passwordHash = HashPassword(request.Password);
        var credential = new UserCredential
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            PasswordHash = passwordHash
        };

        // Assign default "user" role
        var userRole = new UserRole
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Role = "user"
        };

        // Save to database
        _context.Users.Add(user);
        _context.UserCredentials.Add(credential);
        _context.UserRoles.Add(userRole);
        await _context.SaveChangesAsync();

        // Generate tokens
        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        // Save refresh token
        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7) // 7 day refresh token
        };
        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync();

        var userDto = new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            AvatarUrl = user.AvatarUrl,
            EmailVerified = user.EmailVerified,
            Roles = new List<string> { "user" }
        };

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = userDto,
            ExpiresIn = 900 // 15 minutes in seconds
        };
    }

    /// <summary>
    /// Logs in a user with email and password.
    /// </summary>
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        // Find user by email
        var user = await _context.Users
            .Include(u => u.Credentials)
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        // Get user's password credential
        var credential = user.Credentials.FirstOrDefault();
        if (credential == null || !VerifyPassword(request.Password, credential.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        // Generate tokens
        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        // Save refresh token
        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync();

        var userDto = new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            AvatarUrl = user.AvatarUrl,
            EmailVerified = user.EmailVerified,
            Roles = user.Roles.Select(r => r.Role).ToList()
        };

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = userDto,
            ExpiresIn = 900
        };
    }

    /// <summary>
    /// Refreshes an access token using a refresh token.
    /// </summary>
    public async Task<AuthResponse> RefreshAccessTokenAsync(string refreshToken)
    {
        var refreshTokenEntity = await _context.RefreshTokens
            .Include(rt => rt.User)
            .ThenInclude(u => u!.Roles)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (refreshTokenEntity == null || refreshTokenEntity.ExpiresAt < DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");
        }

        var user = refreshTokenEntity.User;
        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("User not found or inactive.");
        }

        // Generate new access token
        var newAccessToken = GenerateAccessToken(user);
        var newRefreshToken = GenerateRefreshToken();

        // Mark old refresh token as verified and create new one
        refreshTokenEntity.VerifiedAt = DateTime.UtcNow;
        var newRefreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _context.RefreshTokens.Update(refreshTokenEntity);
        _context.RefreshTokens.Add(newRefreshTokenEntity);
        await _context.SaveChangesAsync();

        var userDto = new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            AvatarUrl = user.AvatarUrl,
            EmailVerified = user.EmailVerified,
            Roles = user.Roles.Select(r => r.Role).ToList()
        };

        return new AuthResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            User = userDto,
            ExpiresIn = 900
        };
    }
}
