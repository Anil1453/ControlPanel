using ControlPanel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ControlPanel.Controllers
{
    [Authorize(Roles = "Admin,Мениджър")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();
            var userRoles = new Dictionary<string, string>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = roles.FirstOrDefault() ?? "Няма роля";
            }

            ViewBag.UserRoles = userRoles;
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var roles = await _userManager.GetRolesAsync(user);

            // Admin rolü değiştirilemez
            if (roles.Contains("Admin"))
            {
                TempData["Error"] = "Не може да се промени ролята на Администратор!";
                return RedirectToAction("Index");
            }

            // Мениджър sadece Потребител → Служител yapabilir, daha fazlasını sadece Admin
            if (User.IsInRole("Мениджър") && !User.IsInRole("Admin"))
            {
                if (newRole != "Служител" && newRole != "Потребител")
                {
                    TempData["Error"] = "Мениджърът може да задава само роля Служител или Потребител!";
                    return RedirectToAction("Index");
                }
                // Мениджър kendi rolünü değiştiremez
                if (roles.Contains("Мениджър"))
                {
                    TempData["Error"] = "Не може да се промени ролята на друг Мениджър!";
                    return RedirectToAction("Index");
                }
            }

            await _userManager.RemoveFromRolesAsync(user, roles);
            await _userManager.AddToRoleAsync(user, newRole);

            TempData["Success"] = "Ролята е променена успешно!";
            return RedirectToAction("Index");
        }
    }
}
