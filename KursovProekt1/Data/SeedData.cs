// Начални данни - изпълнява се веднъж при стартиране на приложението
// Създава ролите и администраторския акаунт ако не съществуват
using ControlPanel.Models;
using Microsoft.AspNetCore.Identity;

namespace ControlPanel.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            // Вземаме услугите за роли и потребители
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Списък с всички роли в системата
            string[] роли = { "Admin", "Мениджър", "Служител", "Потребител" };

            // Създаваме всяка роля ако не съществува
            foreach (var роля in роли)
            {
                bool ролятаСъществува = await roleManager.RoleExistsAsync(роля);

                if (!ролятаСъществува)
                {
                    await roleManager.CreateAsync(new IdentityRole(роля));
                }
            }

            // Проверяваме дали администраторът вече съществува
            string adminEmail = "admin@admin.com";
            string adminПарола = "Admin123!";

            bool adminСъществува = await userManager.FindByEmailAsync(adminEmail) != null;

            if (!adminСъществува)
            {
                // Създаваме администраторския акаунт
                var adminПотребител = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Administrator",
                    Department = "IT",
                    EmailConfirmed = true,
                    IsActive = true,
                    RegistrationDate = DateTime.Now
                };

                // Запазваме го в базата с паролата
                var резултат = await userManager.CreateAsync(adminПотребител, adminПарола);

                // Ако е създаден успешно, даваме му Admin роля
                if (резултат.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminПотребител, "Admin");
                }
            }

            // Всеки потребител без роля получава роля "Потребител"
            foreach (var потребител in userManager.Users.ToList())
            {
                var ролиНаПотребителя = await userManager.GetRolesAsync(потребител);

                bool няmaРоля = ролиНаПотребителя.Count == 0;

                if (няmaРоля)
                {
                    await userManager.AddToRoleAsync(потребител, "Потребител");
                }
            }
        }
    }
}