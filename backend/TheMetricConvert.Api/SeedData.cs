using Microsoft.EntityFrameworkCore;

namespace TheMetricConvert.Api;

/// <summary>
/// Service for seeding development data into the database.
/// </summary>
public static class SeedData
{
    /// <summary>
    /// Seeds initial admin user and other development data.
    /// Only runs in development mode and is idempotent.
    /// </summary>
    public static async Task InitializeAsync(IServiceProvider serviceProvider, IConfiguration config)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            // Check if admin user already exists
            var adminEmail = config["Seed:AdminEmail"] ?? "admin@metric-convert.dev";
            var adminPassword = config["Seed:AdminPassword"] ?? "AdminPassword123!";
            var adminName = config["Seed:AdminName"] ?? "Admin";

            var existingAdmin = await context.Users.FirstOrDefaultAsync<User>(u => u.Email == adminEmail);
            if (existingAdmin != null)
            {
                logger.LogInformation("Admin user already exists, skipping seed.");
                return;
            }

            try
            {
                logger.LogInformation("Creating development admin user: {Email}", adminEmail);

                // Create admin user
                var adminUser = new User
                {
                    Id = Guid.NewGuid(),
                    Email = adminEmail,
                    DisplayName = adminName,
                    IsActive = true,
                    EmailVerified = true,
                };

                // Hash password and create credential
                var passwordHash = authService.HashPassword(adminPassword);
                var credential = new UserCredential
                {
                    Id = Guid.NewGuid(),
                    UserId = adminUser.Id,
                    PasswordHash = passwordHash,
                };

                // Assign admin role
                var adminRole = new UserRole
                {
                    Id = Guid.NewGuid(),
                    UserId = adminUser.Id,
                    Role = "admin",
                };

                // Save to database
                context.Users.Add(adminUser);
                context.UserCredentials.Add(credential);
                context.UserRoles.Add(adminRole);
                await context.SaveChangesAsync();

                logger.LogInformation("✅ Development admin user created successfully");
                logger.LogInformation("📧 Email: {Email}", adminEmail);
                logger.LogInformation("🔑 Password: {Password}", adminPassword);
                logger.LogInformation("⚠️  Change these credentials before deploying to production!");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to seed admin user");
            }
        }
    }
}
