// Контролер за потребителите - управление на роли и активност
using ControlPanel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ControlPanel.Controllers
{
    // Само Admin и Мениджър могат да управляват потребители
    [Authorize(Roles = "Admin,Мениджър")]
    public class UserController : Controller
    {
        // Потребителският мениджър на Identity
        private readonly UserManager<ApplicationUser> _потребителМениджър;

        // Конструктор
        public UserController(UserManager<ApplicationUser> userManager)
        {
            _потребителМениджър = userManager;
        }

        // ── Списък с всички потребители ───────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            // Вземаме всички потребители
            var потребители = _потребителМениджър.Users.ToList();

            // За всеки потребител намираме ролята му
            // Използваме речник: потребителID → роля
            var роли = new Dictionary<string, string>();

            foreach (var потребител in потребители)
            {
                var ролиНаПотребителя = await _потребителМениджър.GetRolesAsync(потребител);

                // Вземаме първата роля, ако няма - показваме "Няма роля"
                роли[потребител.Id] = ролиНаПотребителя.FirstOrDefault() ?? "Няма роля";
            }

            ViewBag.UserRoles = роли;
            return View(потребители);
        }

        // ── Смяна на роля (POST) ──────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(string userId, string newRole)
        {
            var потребител = await _потребителМениджър.FindByIdAsync(userId);
            var текущиРоли = await _потребителМениджър.GetRolesAsync(потребител);

            // Ролята на Admin не може да се сменя от никой
            bool еАдмин = текущиРоли.Contains("Admin");
            if (еАдмин)
            {
                TempData["Error"] = "Не може да се промени ролята на Администратор!";
                return RedirectToAction("Index");
            }

            // Мениджърът може да задава само Служител или Потребител
            bool еМениджър = User.IsInRole("Мениджър") && !User.IsInRole("Admin");
            if (еМениджър)
            {
                bool невалидноИзбор = newRole != "Служител" && newRole != "Потребител";
                if (невалидноИзбор)
                {
                    TempData["Error"] = "Мениджърът може да задава само роля Служител или Потребител!";
                    return RedirectToAction("Index");
                }

                // Мениджърът не може да сменя роля на друг Мениджър
                if (текущиРоли.Contains("Мениджър"))
                {
                    TempData["Error"] = "Не може да се промени ролята на друг Мениджър!";
                    return RedirectToAction("Index");
                }
            }

            // Премахваме старите роли и добавяме новата
            await _потребителМениджър.RemoveFromRolesAsync(потребител, текущиРоли);
            await _потребителМениджър.AddToRoleAsync(потребител, newRole);

            TempData["Success"] = "Ролята е променена успешно!";
            return RedirectToAction("Index");
        }

        // ── Активиране/Деактивиране на потребител (POST) ─────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // Само Admin може да деактивира
        public async Task<IActionResult> ToggleActive(string userId)
        {
            var потребител = await _потребителМениджър.FindByIdAsync(userId);

            if (потребител == null)
            {
                TempData["Error"] = "Потребителят не е намерен!";
                return RedirectToAction("Index");
            }

            // Admin не може да се деактивира
            var роли = await _потребителМениджър.GetRolesAsync(потребител);
            bool еАдмин = роли.Contains("Admin");
            if (еАдмин)
            {
                TempData["Error"] = "Администраторът не може да бъде деактивиран!";
                return RedirectToAction("Index");
            }

            // Обръщаме стойността: true → false, false → true
            потребител.IsActive = !потребител.IsActive;
            await _потребителМениджър.UpdateAsync(потребител);

            // Показваме различно съобщение според резултата
            TempData["Success"] = потребител.IsActive
                ? "Потребителят е активиран успешно!"
                : "Потребителят е деактивиран успешно!";

            return RedirectToAction("Index");
        }
    }
}