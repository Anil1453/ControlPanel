using ControlPanel.Models;
using Microsoft.AspNetCore.Identity;

namespace ControlPanel.Data
{
    public static class SeedData
    {
        // Program baslayinca bu metod calisir
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            // Rol yoneticisi roli suzdava
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Potrebitel yoneticisi potrebiteli suzdava
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 2 rola: Admin i User
            string[] roleNames = { "Admin", "Мениджър", "Служител", "Потребител" };

            // Za vsqka rolq:
            foreach (var roleName in roleNames)
            {
                // Ako ne sastostva
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    // Suzdai q
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Admin email i parola
            string adminEmail = "admin@admin.com";
            string adminPassword = "Admin123!";

            // Ako admin ne sastostva
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                // Suzdai nov admin potrebitel
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Administrator",
                    Department = "IT",
                    EmailConfirmed = true,
                    IsActive = true,
                    RegistrationDate = DateTime.Now
                };

                // Zapazi go v bazata
                var result = await userManager.CreateAsync(adminUser, adminPassword);

                // Ako uspexa
                if (result.Succeeded)
                {
                    // Dobavi Admin rolqta
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
            // Всички потребители без роля да получат роля Потребител
            foreach (var user in userManager.Users.ToList())
            {
                var roles = await userManager.GetRolesAsync(user);
                if (roles.Count == 0)
                {
                    await userManager.AddToRoleAsync(user, "Потребител");
                }
            }
        }
    }
}