using Insurance_Backend.Enums;
using Insurance_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Insurance_Backend.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IHost host)
        {
            using var scope = host.Services.CreateScope();

            var services = scope.ServiceProvider;

            var context =
                services.GetRequiredService<AppDbContext>();

            var configuration =
                services.GetRequiredService<IConfiguration>();

            // Read initial admin details from configuration.
            var adminName =
                configuration["SeedAdmin:FullName"];

            var adminEmail =
                configuration["SeedAdmin:Email"];

            var adminPassword =
                configuration["SeedAdmin:Password"];

            var adminMobileNumber =
                configuration["SeedAdmin:MobileNumber"];

            if (string.IsNullOrWhiteSpace(adminName) ||
                string.IsNullOrWhiteSpace(adminEmail) ||
                string.IsNullOrWhiteSpace(adminPassword) ||
                string.IsNullOrWhiteSpace(adminMobileNumber))
            {
                throw new InvalidOperationException(
                    "Initial admin configuration is incomplete.");
            }

            // Do not create another admin if one already exists.
            var adminExists = await context.User
                .AnyAsync(u => u.Role == Role.Admin);

            if (adminExists)
            {
                return;
            }

            // Prevent a duplicate email if the account already exists
            // with a different role.
            var emailExists = await context.User
                .AnyAsync(u => u.Email == adminEmail);

            if (emailExists)
            {
                throw new InvalidOperationException(
                    "The configured initial admin email already belongs " +
                    "to an existing user. Verify the account before seeding.");
            }

            var adminUser = new User
            {
                FullName = adminName,
                Email = adminEmail,

                Password = BCrypt.Net.BCrypt.HashPassword(adminPassword),

                MobileNumber = adminMobileNumber,

                Role = Role.Admin,
                ActiveStatus = ActiveStatus.Active,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            await context.User.AddAsync(adminUser);
            await context.SaveChangesAsync();
        }
    }
}
