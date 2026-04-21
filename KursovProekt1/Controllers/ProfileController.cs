using ControlPanel.Data;
using ControlPanel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ControlPanel.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ProfileController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "Няма роля";
            ViewBag.Role = role;
            ViewBag.TotalRequests = _context.AccessRequests.Count(r => r.UserId == user.Id);
            ViewBag.ApprovedRequests = _context.AccessRequests.Count(r => r.UserId == user.Id && r.Status == "Approved");
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(string fullName, string department, IFormFile? profilePicture)
        {
            var user = await _userManager.GetUserAsync(User);

            user.FullName = fullName;
            user.Department = department;

            // качи ли снимка?
            if (profilePicture != null && profilePicture.Length > 0)
            {
                // приеми само снимки
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
                if (!allowedTypes.Contains(profilePicture.ContentType))
                {
                    TempData["Error"] = "Само изображения са разрешени (jpg, png, gif, webp)!";
                    return RedirectToAction("Index");
                }

                // Max 2MB
                if (profilePicture.Length > 2 * 1024 * 1024)
                {
                    TempData["Error"] = "Файлът е твърде голям. Максимум 2MB!";
                    return RedirectToAction("Index");
                }

                // създай Uploads папка
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Име на файла като: profil_USERID.jpg
                var extension = Path.GetExtension(profilePicture.FileName);
                var fileName = $"profil_{user.Id}{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profilePicture.CopyToAsync(stream);
                }

                user.ProfilePicture = fileName;
            }

            await _userManager.UpdateAsync(user);
            TempData["Success"] = "Профилът е обновен успешно!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemovePicture()
        {
            var user = await _userManager.GetUserAsync(User);

            if (!string.IsNullOrEmpty(user.ProfilePicture))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", user.ProfilePicture);
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);

                user.ProfilePicture = null;
                await _userManager.UpdateAsync(user);
            }

            TempData["Success"] = "Снимката е премахната.";
            return RedirectToAction("Index");
        }
    }
}
